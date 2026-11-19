// Op Art / Kinetik Sanat lensi: Spiral Node tasarimlari (A1-A3)
// Konvansiyon: st.angle POZITIF = ekranda saat yonu tersi (CCW). Canvas y asagi oldugundan
// matematiksel aciyi (cx + r cos a, cy - r sin a) ile ciziyoruz.
// Kollar "iz birakir" (trailing): CCW donen node'da kol disa dogru saat yonunde kivrilir
// (pervane / galaksi gibi). Boylece donus -> disa dogru genisleme algisi uretir.
// dir=-1 ayna goruntusudur: a = angle - dir*(burulma) formulu kendiliginden aynalar.
(function () {
  'use strict';
  var TAU = Math.PI * 2;

  function disc(ctx, cx, cy, R, fill) {
    ctx.beginPath();
    ctx.arc(cx, cy, R, 0, TAU);
    ctx.fillStyle = fill;
    ctx.fill();
  }
  function pt(cx, cy, r, a) {
    return [cx + r * Math.cos(a), cy - r * Math.sin(a)];
  }
  function sgn(d) { return d < 0 ? -1 : 1; }
  function clamp01(v) { v = +v || 0; return v < 0 ? 0 : v > 1 ? 1 : v; }

  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];

  // ---------------------------------------------------------------------------
  // A1: Duchamp Diski. Iki kollu Arsimet sarmali, iki duz renk. Kollar ile aralari
  // esit genislikte: sarmal donerken halkalar merkezden disa dogru "akar".
  // ---------------------------------------------------------------------------
  var A1_BG = '#fcae04', A1_FG = '#362d70';
  window.SPIRAL_VARIANTS.push({
    id: 'A1',
    name: 'Duchamp Diski',
    movement: 'Op Art',
    colors: [A1_FG, A1_BG],
    draw: function (ctx, cx, cy, R, st) {
      var dir = sgn(st.dir), held = clamp01(st.held);
      var N = 2;
      var spacing = R / 3.1;               // iki komsu kol arasi radyal mesafe
      var pitch = spacing * N;             // bir tam turda radyal ilerleme
      var w = spacing * (0.5 + 0.06 * held);
      var uMax = (R + w) / pitch;          // tur sayisi (kol disari tasana dek)
      var steps = Math.ceil(uMax * 72);
      ctx.save();
      ctx.beginPath();
      ctx.arc(cx, cy, R, 0, TAU);
      ctx.clip();
      disc(ctx, cx, cy, R, A1_BG);
      ctx.strokeStyle = A1_FG;
      ctx.lineWidth = w;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';
      for (var k = 0; k < N; k++) {
        ctx.beginPath();
        for (var i = 0; i <= steps; i++) {
          var u = uMax * i / steps;
          var a = st.angle - dir * TAU * u + TAU * k / N;
          var p = pt(cx, cy, pitch * u, a);
          if (i === 0) ctx.moveTo(p[0], p[1]); else ctx.lineTo(p[0], p[1]);
        }
        ctx.stroke();
      }
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------------------
  // A2: Vasarely Kanatlari. 5 kalin kanat, hafif burulmus (tek tur degil, ~110 derece).
  // Kanatlar merkezde tek noktada bulusur; disa dogru acilir (kama).
  // ---------------------------------------------------------------------------
  var A2_BG = '#f1e6c7', A2_FG = '#04586c';
  window.SPIRAL_VARIANTS.push({
    id: 'A2',
    name: 'Vasarely Kanatlari',
    movement: 'Op Art',
    colors: [A2_FG, A2_BG],
    draw: function (ctx, cx, cy, R, st) {
      var dir = sgn(st.dir), held = clamp01(st.held);
      var N = 5;
      var tw = 1.95;                       // yaricap boyunca burulma (rad)
      var delta = (Math.PI / N) * (1 + 0.1 * held); // kanat acisal genisligi
      var steps = 32;
      var Rr = R * 1.03;
      ctx.save();
      ctx.beginPath();
      ctx.arc(cx, cy, R, 0, TAU);
      ctx.clip();
      disc(ctx, cx, cy, R, A2_BG);
      ctx.fillStyle = A2_FG;
      for (var k = 0; k < N; k++) {
        var base = st.angle + TAU * k / N;
        ctx.beginPath();
        var i, r, a, p;
        for (i = 0; i <= steps; i++) {
          r = Rr * i / steps;
          a = base - dir * tw * (r / R);
          p = pt(cx, cy, r, a);
          if (i === 0) ctx.moveTo(p[0], p[1]); else ctx.lineTo(p[0], p[1]);
        }
        for (i = steps; i >= 0; i--) {
          r = Rr * i / steps;
          a = base - dir * tw * (r / R) + delta;
          p = pt(cx, cy, r, a);
          ctx.lineTo(p[0], p[1]);
        }
        ctx.closePath();
        ctx.fill();
      }
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------------------
  // A3: Vasarely Boncuklari. En sade: iki kolda 4 buyuyen daire + merkez noktasi.
  // Boyut gradyani (Vasarely kabarmasi) + donus = disa kayan, buyuyen toplar.
  // ---------------------------------------------------------------------------
  var A3_BG = '#362d70', A3_FG = '#ee7284';
  window.SPIRAL_VARIANTS.push({
    id: 'A3',
    name: 'Vasarely Boncuklari',
    movement: 'Op Art',
    colors: [A3_FG, A3_BG],
    draw: function (ctx, cx, cy, R, st) {
      var dir = sgn(st.dir), held = clamp01(st.held);
      var arms = 2, dots = 4;
      var g = 1.42;                        // her topun yaricap carpani
      var alpha = 1.25;                    // ardisik toplar arasi aci (rad)
      var grow = 1 + 0.08 * held;
      ctx.save();
      ctx.beginPath();
      ctx.arc(cx, cy, R, 0, TAU);
      ctx.clip();
      disc(ctx, cx, cy, R, A3_BG);
      ctx.fillStyle = A3_FG;
      // merkez noktasi (hedef alani)
      ctx.beginPath();
      ctx.arc(cx, cy, R * 0.09 * grow, 0, TAU);
      ctx.fill();
      for (var k = 0; k < arms; k++) {
        for (var i = 0; i < dots; i++) {
          var r = R * 0.2 * Math.pow(g, i);
          var rho = R * (0.06 + 0.13 * (r / R)) * grow;
          var a = st.angle - dir * alpha * i + Math.PI * k;
          var p = pt(cx, cy, r, a);
          ctx.beginPath();
          ctx.arc(p[0], p[1], rho, 0, TAU);
          ctx.fill();
        }
      }
      ctx.restore();
    }
  });
})();
