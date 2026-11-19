// D-motion.js - Hareketli grafik / oyun gorsel dili lensi: 3 minimal sarmal dugum tasarimi.
// st.angle: +CCW (ekranda), st.dir: +1 CCW / -1 CW. Kollar dir'e gore aynalanir.
// Ortak kural: canvas acisi = dir*phi + rot, rot = -st.angle (canvas y asagi, + aci = saat yonu).
(function () {
  'use strict';
  var TAU = Math.PI * 2;
  var PAPER = '#f1e6c7', TEAL = '#04586c', YELLOW = '#fcae04', INDIGO = '#362d70';

  function sgn(d) { return d < 0 ? -1 : 1; }
  function clipDisc(ctx, cx, cy, R) {
    ctx.beginPath(); ctx.arc(cx, cy, R, 0, TAU); ctx.clip();
  }
  function disc(ctx, x, y, r, col) {
    ctx.beginPath(); ctx.arc(x, y, r, 0, TAU); ctx.fillStyle = col; ctx.fill();
  }

  // ---------------------------------------------------------------- D1
  // Bass/Whitney: tek sekil = iki kollu Arsimet sarmali, %50 dolu / %50 bos.
  // Koyu teal zemin, kagit renkli iki kol. Kollar merkezden disa dogru
  // donus yonunun tersine sarilir (arkada iz birakir) -> donerken disa akis hissi.
  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push({
    id: 'D1',
    name: 'Bass sarmali (iki kol)',
    movement: 'Hareketli grafik / oyun',
    colors: [TEAL, PAPER],
    draw: function (ctx, cx, cy, R, st) {
      var dir = sgn(st.dir), rot = -st.angle, held = st.held || 0;
      var T = 2;                       // her kolun tur sayisi
      var spacing = R / (2 * T);       // komsu bantlar arasi radyal aralik
      var w = spacing * (0.5 + 0.06 * held);
      var n = 120, phiMax = TAU * T * 0.93;   // kol kenara varmadan biter: koyu kenar siluet verir
      ctx.save();
      clipDisc(ctx, cx, cy, R);
      disc(ctx, cx, cy, R, TEAL);
      ctx.strokeStyle = PAPER; ctx.lineWidth = w; ctx.lineJoin = 'round'; ctx.lineCap = 'butt';
      for (var a = 0; a < 2; a++) {
        ctx.beginPath();
        for (var i = 0; i <= n; i++) {
          var phi = phiMax * i / n;
          var r = (R / T) * phi / TAU;
          var ang = dir * phi + rot + a * Math.PI;
          var x = cx + r * Math.cos(ang), y = cy + r * Math.sin(ang);
          if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
        }
        ctx.stroke();
      }
      disc(ctx, cx, cy, w * (0.85 + 0.1 * held), PAPER);  // net merkez noktasi
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------- D2
  // Duchamp Rotorelief: ic ice daireler, her biri bir oncekine ictenteget.
  // Teget noktasi halka basina 75 derece doner -> daire zincirinden sarmal cikar;
  // donerken ince-kalin hilal darbesi. Sadece 4 daire + merkez nokta.
  window.SPIRAL_VARIANTS.push({
    id: 'D2',
    name: 'Rotorelief (ice teget daireler)',
    movement: 'Hareketli grafik / oyun',
    colors: [INDIGO, YELLOW],
    draw: function (ctx, cx, cy, R, st) {
      var dir = sgn(st.dir), rot = -st.angle, held = st.held || 0;
      var rad = [1, 0.8, 0.6, 0.42, 0.26, 0.13 + 0.02 * held];   // son eleman = merkez nokta
      var cols = [INDIGO, YELLOW, INDIGO, YELLOW, INDIGO, YELLOW];
      var lam = 0.95, step = 60 * Math.PI / 180, N = 5, i;
      var px = [0], py = [0];
      for (i = 1; i <= N; i++) {
        var ang = dir * (i - 1) * step + rot, d = (rad[i - 1] - rad[i]) * lam;
        px.push(px[i - 1] + d * Math.cos(ang)); py.push(py[i - 1] + d * Math.sin(ang));
      }
      ctx.save();
      clipDisc(ctx, cx, cy, R);
      for (i = 0; i <= N; i++) {   // zincirin basi ve sonu tam merkezde olsun: dogrusal duzeltme
        var f = i / N;
        disc(ctx, cx + (px[i] - f * px[N]) * R, cy + (py[i] - f * py[N]) * R, rad[i] * R, cols[i]);
      }
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------- D3
  // Super Hexagon + Bass indirgemesi: uc kavisli kanat, bir renk + bir nokta.
  // Zemin yok (seffaf); kanatlar merkezde sivri, kenarda genis -> disa buyume.
  window.SPIRAL_VARIANTS.push({
    id: 'D3',
    name: 'Uc kanat (turbin)',
    movement: 'Hareketli grafik / oyun',
    colors: [INDIGO, PAPER],
    draw: function (ctx, cx, cy, R, st) {
      var dir = sgn(st.dir), rot = -st.angle, held = st.held || 0;
      var N = 3, sweep = 130 * Math.PI / 180, seg = TAU / N, n = 45, U = 1.5;   // U: uc kenarin disina tasar, clip keser
      ctx.save();
      clipDisc(ctx, cx, cy, R);
      ctx.fillStyle = INDIGO;
      for (var b = 0; b < N; b++) {
        var base = b * seg, i, u, ang;
        ctx.beginPath();
        for (i = 0; i <= n; i++) {            // on kenar, icten disa
          u = U * i / n;
          ang = rot + dir * (base + sweep * u);
          ctx.lineTo(cx + u * R * Math.cos(ang), cy + u * R * Math.sin(ang));
        }
        for (i = n; i >= 0; i--) {            // arka kenar, distan ice
          u = U * i / n;
          ang = rot + dir * (base + sweep * u + seg * (0.3 + 0.4 * Math.min(u, 1)));
          ctx.lineTo(cx + u * R * Math.cos(ang), cy + u * R * Math.sin(ang));
        }
        ctx.closePath();
        ctx.fill();
      }
      disc(ctx, cx, cy, R * (0.14 + 0.02 * held), PAPER);
      ctx.restore();
    }
  });
})();
