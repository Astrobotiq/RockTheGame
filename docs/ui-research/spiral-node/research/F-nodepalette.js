/* F-nodepalette.js - Spiral node tasarimlari: SADECE normal node'un halka rampasindan.
 * Per-pixel analitik AA (kenarlarda 0.8 px yumusatma), onbellekli polar izgara.
 * Yon: st.angle + = ekranda saat yonunun tersi (CCW). dir=+1 -> node CCW doner.
 * Kol egimi "pervane" kuralina uyar: kollar donusun ARKASINDA kalir
 * (dir=+1 iken kol disa dogru saat yonune kivrilir). Ters istenirse TRAIL = -1.
 * Her iki yon birbirinin ayna goruntusudur.
 */
(function () {
  'use strict';

  var TAU = Math.PI * 2;
  var TRAIL = 1;          // -1 yaparsan tum tasarimlarda egim yonu tersine doner
  var MAX_RPX = 150;      // render yaricapi tavani (DPR yuksekse hizi korur)

  // Normal node rampasi (CircularRings, ic -> dis)
  var PAPER  = [0xf1, 0xe6, 0xc7];
  var ORANGE = [0xff, 0x51, 0x04];
  var YELLOW = [0xfc, 0xae, 0x04];
  var TEAL   = [0x28, 0x9a, 0xa4];
  var DTEAL  = [0x04, 0x58, 0x6c];

  // Normal node'un merkezden kenara solma kurali (OuterFadeStart / Power)
  function fadeAlpha(u, start, pw) {
    if (start >= 1) return 1;                 // solma yok: sadece disk kenari AA (build'de)
    if (u >= 1) return 0;
    if (u <= start) return 1;
    var t = (u - start) / (1 - start + 0.0001);
    return Math.pow(1 - t, pw);
  }

  function clamp01(x) { return x < 0 ? 0 : (x > 1 ? 1 : x); }

  // Kenar yumusatma: d = kenara px mesafe (kendi tarafinda >= 0). Komsu renk agirligi (0..0.5)
  function edgeW(d) {
    var w = 0.5 * (1 - d / 0.8);
    return w > 0 ? w : 0;
  }

  /* Ortak iskelet: shade(u, th, s, out) -> out[0..2]=rgb, out[3]=kapsama(0..1)
   * th = piksel acisi - st.angle (CCW +). s = yon isareti * TRAIL. */
  function makeRenderer(opts) {
    var fadeStart = opts.fadeStart, fadePow = opts.fadePow;
    var grid = null;      // {Rpx, D, u, ang, fa, canvas, img}
    var out = [0, 0, 0, 0];

    function build(Rpx) {
      var D = 2 * Math.ceil(Rpx) + 2, half = D / 2;
      var n = D * D;
      var u = new Float32Array(n), ang = new Float32Array(n), fa = new Float32Array(n);
      var ax = new Float32Array(n);
      for (var y = 0; y < D; y++) {
        for (var x = 0; x < D; x++) {
          var dx = x + 0.5 - half, dy = y + 0.5 - half;
          var i = y * D + x;
          var uu = Math.sqrt(dx * dx + dy * dy) / Rpx;
          u[i] = uu;
          ang[i] = Math.atan2(-dy, dx);
          var edge = clamp01(0.5 + (1 - uu) * Rpx);   // disk kenari kapsamasi
          fa[i] = fadeAlpha(uu, fadeStart, fadePow) * edge;
          ax[i] = opts.aux ? opts.aux(uu, Rpx) : 0;
        }
      }
      var canvas = document.createElement('canvas');
      canvas.width = D; canvas.height = D;
      var c2 = canvas.getContext('2d');
      grid = { Rpx: Rpx, D: D, u: u, ang: ang, fa: fa, ax: ax, canvas: canvas,
               c2: c2, img: c2.createImageData(D, D) };
    }

    return function draw(ctx, cx, cy, R, st) {
      var m = ctx.getTransform ? ctx.getTransform() : null;
      var sc = m ? (Math.hypot(m.a, m.b) || 1) : 1;
      var Rpx = Math.min(R * sc, MAX_RPX);
      Rpx = Math.max(8, Math.round(Rpx * 2) / 2);
      if (!grid || grid.Rpx !== Rpx) build(Rpx);

      var g = grid, D = g.D, n = D * D, data = g.img.data;
      var s = (st.dir < 0 ? -1 : 1) * TRAIL;
      var rot = st.angle || 0;
      var u = g.u, ang = g.ang, fa = g.fa, ax = g.ax;
      opts.begin && opts.begin(Rpx, s);
      for (var i = 0; i < n; i++) {
        var a = fa[i], o = i * 4;
        if (a <= 0) { data[o + 3] = 0; continue; }
        opts.shade(u[i], ang[i] - rot, s, out, ax[i]);
        data[o] = out[0]; data[o + 1] = out[1]; data[o + 2] = out[2];
        data[o + 3] = Math.round(255 * a * out[3]);
      }
      g.c2.putImageData(g.img, 0, 0);

      ctx.save();
      ctx.beginPath();
      ctx.arc(cx, cy, R, 0, TAU);
      ctx.clip();
      var side = (D / Rpx) * R; // D piksel = side birim
      ctx.imageSmoothingEnabled = true;
      ctx.drawImage(g.canvas, cx - side / 2, cy - side / 2, side, side);
      ctx.restore();
    };
  }

  function mix3(out, A, B, w) { // w = B agirligi
    out[0] = A[0] + (B[0] - A[0]) * w;
    out[1] = A[1] + (B[1] - A[1]) * w;
    out[2] = A[2] + (B[2] - A[2]) * w;
  }

  // Gercek node: kucuk sari cekirdek disk (cap 1 birim, collider r=0.5) + buyuk halka halesi.
  // Varsayim: spiral node gorseli 3 birimlik daire (R = 1.5 birim) -> cekirdek u = 1/3 (tutma hedefi).
  // Halka bandi cekirdekten kenara kalan 1 birimde (u 1/3..1); normal node'daki 1 birimlik halka
  // kalinligi burada ~0.25-0.33 birime sikistirilmistir (aksi halde tek halka sigar).
  var CORE = 1 / 3, CORE_COL = [0xf0, 0xd4, 0x20];
  var FS = 1, FP = 1;        // solma yok: normal node'un 4 birimlik solmasi bu 1.5 birimlik dairede gorunmez, kenar sert
  var VSPAN = 1 - CORE;
  function sq(x) { return x * x; }

  // Cekirdek disk (kenari AA). Dondurur: true -> piksel tamamen cekirdekte, shade bitti.
  function core(out, u, Rpx) {
    var dh = (u - CORE) * Rpx;               // + = cekirdek disinda
    if (dh <= -1) { out[0] = CORE_COL[0]; out[1] = CORE_COL[1]; out[2] = CORE_COL[2]; out[3] = 1; return true; }
    return false;
  }
  function coreEdge(out, u, Rpx) {
    var dh = (u - CORE) * Rpx;
    if (dh < 0.8) mix3(out, out, CORE_COL, clamp01(0.5 - dh / 1.6));
    out[3] = 1;
  }

  /* ---------- F1: Sarili seritli sarmal (a)+(d) ---------------------------------
   * Cekirdegin etrafinda normal node'un 3 halka rengi (turuncu, sari, kagit) halka yerine
   * 3 esit sarmal kol olarak sarilir. v = (u-CORE)/(1-CORE), f = NB*v + s*K*theta/2pi,
   * renk = rampa[floor(f) mod 3]. K=3 (renk sayisinin kati) -> dikis cizgisi yok. */
  var F1_NB = 3, F1_K = 3;
  var f1cols = [ORANGE, YELLOW, PAPER];
  var f1Rpx = 100;
  var F1 = makeRenderer({
    fadeStart: FS, fadePow: FP,
    begin: function (Rpx) { f1Rpx = Rpx; },
    aux: function (u, Rpx) {                     // f'nin piksel basina degisimi (1/gr)
      var uu = u < CORE ? CORE : u;
      return Rpx / Math.sqrt(sq(F1_NB / VSPAN) + sq(F1_K / (TAU * uu)));
    },
    shade: function (u, th, s, out, inv) {
      if (core(out, u, f1Rpx)) return;
      var f = F1_NB * (u - CORE) / VSPAN + s * F1_K * th / TAU;
      var fl = Math.floor(f), p = f - fl;
      var idx = ((fl % 3) + 3) % 3;
      var d = (p < 0.5 ? p : 1 - p) * inv;       // px
      var nb = p < 0.5 ? (idx + 2) % 3 : (idx + 1) % 3;
      mix3(out, f1cols[idx], f1cols[nb], edgeW(d));
      coreEdge(out, u, f1Rpx);
    }
  });

  /* ---------- F2: Kaymis diskler (c) --------------------------------------------
   * Normal node gibi ic ice halkalar, ama her halka bir oncekine gore biraz kaydirilmis
   * ic ice daireler (dis -> ic: turuncu, kagit, sari, kagit, turuncu). Merkezler bir sarmal
   * uzerinde doner -> ay-kesitleri sarmal bir merdiven olusturur. Cekirdek tam ortada sabit.
   * Vektor: sadece 5 dolu daire + cekirdek. */
  var F2_RHO = [1, 0.84, 0.70, 0.57, 0.45];     // yaricap (R cinsinden)
  var F2_OFF = [0, 0.14, 0.2, 0.18, 0.10];     // merkez kaymasi (R cinsinden)
  var F2_DEG = 70 * Math.PI / 180;              // halka basina aci adimi
  var F2_COL = ['#ff5104', '#f1e6c7', '#fcae04', '#f1e6c7', '#ff5104'];
  var CORE_HEX = '#f0d420';
  function F2(ctx, cx, cy, R, st) {
    var s = (st.dir < 0 ? -1 : 1) * TRAIL;
    var rot = st.angle || 0;
    ctx.save();
    ctx.beginPath(); ctx.arc(cx, cy, R, 0, TAU); ctx.clip();
    for (var k = 0; k < F2_RHO.length; k++) {
      // dis halka disa dogru: aci s ile azalir (kollar donusun arkasinda)
      var a = rot - s * k * F2_DEG;
      var x = cx + Math.cos(a) * F2_OFF[k] * R, y = cy - Math.sin(a) * F2_OFF[k] * R;
      ctx.fillStyle = F2_COL[k];
      ctx.beginPath(); ctx.arc(x, y, F2_RHO[k] * R, 0, TAU); ctx.fill();
    }
    ctx.fillStyle = CORE_HEX;
    ctx.beginPath(); ctx.arc(cx, cy, R * CORE, 0, TAU); ctx.fill();
    ctx.restore();
  }

  /* ---------- F3: Tek kol (e) ---------------------------------------------------
   * Kagit zemin, sari cekirdek ve uzerinde TEK kalin turuncu kol: 1 baslangicli Arsimet
   * sarmali, kol periyodun %46'si. 2 renk (turuncu + kagit) + cekirdek. */
  var F3_T = 1.25, F3_DUTY = 0.46;
  var f3Rpx = 100;
  var F3 = makeRenderer({
    fadeStart: FS, fadePow: FP,
    begin: function (Rpx) { f3Rpx = Rpx; },
    aux: function (u, Rpx) {
      var uu = u < CORE ? CORE : u;
      return Rpx / Math.sqrt(sq(F3_T / VSPAN) + sq(1 / (TAU * uu)));
    },
    shade: function (u, th, s, out, inv) {
      if (core(out, u, f3Rpx)) return;
      var f = F3_T * (u - CORE) / VSPAN + s * th / TAU;
      var p = f - Math.floor(f);
      var d = p, e = Math.abs(p - F3_DUTY), e2 = 1 - p;
      if (e < d) d = e; if (e2 < d) d = e2;
      var w = edgeW(d * inv);
      if (p < F3_DUTY) mix3(out, ORANGE, PAPER, w); else mix3(out, PAPER, ORANGE, w);
      coreEdge(out, u, f3Rpx);
    }
  });

  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push({
    id: 'F1', name: 'Sarili seritli sarmal', movement: 'Node paleti',
    colors: ['#ff5104', '#fcae04', '#f1e6c7'],
    draw: F1
  });
  window.SPIRAL_VARIANTS.push({
    id: 'F2', name: 'Kaymis diskler', movement: 'Node paleti',
    colors: ['#ff5104', '#fcae04', '#f1e6c7'],
    draw: F2
  });
  window.SPIRAL_VARIANTS.push({
    id: 'F3', name: 'Tek kalin kol', movement: 'Node paleti',
    colors: ['#ff5104', '#f1e6c7'],
    draw: F3
  });
})();
