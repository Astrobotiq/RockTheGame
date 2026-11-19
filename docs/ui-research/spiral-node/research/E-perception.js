// E-perception.js : Algi bilimi merceginden 3 sarmal dugum tasarimi.
// Ortak kural: kollar "arkada kalan" (trailing) kavisli; desen donerken kenarlar disa dogru akar
// (gorsel genisleme illuzyonu). FLOW = -1 yapilirsa kollar one kavislenir, akis ice (daralma) olur.
(function () {
  'use strict';
  var TAU = Math.PI * 2;
  var FLOW = 1;

  function pt(ctx, cx, cy, r, th) {
    return [cx + r * Math.cos(th), cy - r * Math.sin(th)]; // y asagi, th pozitif = saat yonunun tersi
  }

  // Log-sarmal kol kenari: theta(r) = angle + base - dir*FLOW*k*ln(r/r0) + dir*u
  function edgeTheta(angle, dir, k, r, r0, base, u) {
    return angle + base - dir * FLOW * k * Math.log(r / r0) + dir * u;
  }

  // Tek bir kolun [u0,u1] (radyan, tangensiyel) dilimini doldurur.
  function wedge(ctx, cx, cy, R, r0, angle, dir, k, base, u0, u1) {
    var S = 28, rMax = R * 1.04, ratio = rMax / r0, j, r, p;
    ctx.beginPath();
    for (j = 0; j <= S; j++) {
      r = r0 * Math.pow(ratio, j / S);
      p = pt(ctx, cx, cy, r, edgeTheta(angle, dir, k, r, r0, base, u0));
      if (j === 0) ctx.moveTo(p[0], p[1]); else ctx.lineTo(p[0], p[1]);
    }
    var a0 = edgeTheta(angle, dir, k, rMax, r0, base, u0), a1 = edgeTheta(angle, dir, k, rMax, r0, base, u1);
    for (j = 1; j < 12; j++) { // dis kenar: yay (kiris degil), yoksa genis kolda daire icine sarkar
      p = pt(ctx, cx, cy, rMax, a0 + (a1 - a0) * j / 12);
      ctx.lineTo(p[0], p[1]);
    }
    for (j = S; j >= 0; j--) {
      r = r0 * Math.pow(ratio, j / S);
      p = pt(ctx, cx, cy, r, edgeTheta(angle, dir, k, r, r0, base, u1));
      ctx.lineTo(p[0], p[1]);
    }
    ctx.closePath();
    ctx.fill();
  }

  function disc(ctx, cx, cy, r, col) {
    ctx.fillStyle = col;
    ctx.beginPath();
    ctx.arc(cx, cy, r, 0, TAU);
    ctx.fill();
  }

  function clipCircle(ctx, cx, cy, R) {
    ctx.beginPath();
    ctx.arc(cx, cy, R, 0, TAU);
    ctx.clip();
  }

  // ---------------------------------------------------------------- E1
  // Iki kanat: N=2, kol/bosluk 50/50, sabit egimli (log) kavis, 2 renk.
  var E1_N = 2, E1_K = 1.25, E1_R0 = 0.17;
  var E1_A = '#362d70', E1_B = '#f1e6c7';
  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push({
    id: 'E1',
    name: 'Iki kanat',
    movement: 'Algi bilimi',
    colors: [E1_A, E1_B],
    draw: function (ctx, cx, cy, R, st) {
      var i, r0 = R * E1_R0 * (1 + 0.12 * (st.held || 0));
      ctx.save();
      clipCircle(ctx, cx, cy, R);
      disc(ctx, cx, cy, R, E1_B);
      ctx.fillStyle = E1_A;
      for (i = 0; i < E1_N; i++) {
        wedge(ctx, cx, cy, R, R * E1_R0, st.angle, st.dir, E1_K, i * TAU / E1_N, 0, Math.PI / E1_N);
      }
      disc(ctx, cx, cy, r0, E1_A);
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------- E2
  // Tek cizgi sarmal: Arsimet, N=1, ince cizgi (~%30), ~2 tur. "5 yasindaki cizer".
  var E2_A = '#04586c', E2_B = '#fcae04';
  window.SPIRAL_VARIANTS.push({
    id: 'E2',
    name: 'Tek cizgi',
    movement: 'Algi bilimi',
    colors: [E2_A, E2_B],
    draw: function (ctx, cx, cy, R, st) {
      var pitch = R * 0.5, w = pitch * 0.34, hub = R * 0.17 * (1 + 0.12 * (st.held || 0));
      var tEnd = (R * 1.1 - hub * 0.4) / (pitch / TAU), S = Math.ceil(tEnd * 12), j, t, r, th, p;
      ctx.save();
      clipCircle(ctx, cx, cy, R);
      disc(ctx, cx, cy, R, E2_B);
      ctx.strokeStyle = E2_A;
      ctx.lineWidth = w;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';
      ctx.beginPath();
      for (j = 0; j <= S; j++) {
        t = tEnd * j / S;
        r = hub * 0.4 + t * pitch / TAU;
        th = st.angle - st.dir * FLOW * t;
        p = pt(ctx, cx, cy, r, th);
        if (j === 0) ctx.moveTo(p[0], p[1]); else ctx.lineTo(p[0], p[1]);
      }
      ctx.stroke();
      disc(ctx, cx, cy, hub, E2_A);
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------- E3
  // Parlaklik merdiveni: N=2, her kol koyu -> orta -> acik 3 duz basamak (asimetrik parlaklik,
  // Kitaoka/Pinna-Brelstaff'tan: statik karede bile yonlu "akis" ipucu). 3. renk bu yuzden.
  var E3_D = '#362d70', E3_M = '#ee7284', E3_L = '#f1e6c7';
  var E3_N = 2, E3_K = 1.0, E3_R0 = 0.16, E3_STAIR = 1; // E3_STAIR=-1: basamak sirasini ters cevir
  window.SPIRAL_VARIANTS.push({
    id: 'E3',
    name: 'Parlaklik merdiveni',
    movement: 'Algi bilimi',
    colors: [E3_D, E3_M, E3_L],
    draw: function (ctx, cx, cy, R, st) {
      var i, P = TAU / E3_N, s = E3_STAIR, r0 = R * E3_R0 * (1 + 0.12 * (st.held || 0));
      ctx.save();
      clipCircle(ctx, cx, cy, R);
      disc(ctx, cx, cy, R, E3_L);
      for (i = 0; i < E3_N; i++) {
        ctx.fillStyle = E3_M;
        wedge(ctx, cx, cy, R, R * E3_R0, st.angle, st.dir, E3_K, i * P, 0, s * P * 0.68);
        ctx.fillStyle = E3_D;
        wedge(ctx, cx, cy, R, R * E3_R0, st.angle, st.dir, E3_K, i * P, 0, s * P * 0.30);
      }
      disc(ctx, cx, cy, r0, E3_D);
      ctx.restore();
    }
  });
})();
