// C-folk.js - Halk / Dogu spiral gelenekleri: sadece geometri ilkeleri.
// C1 Mitsudomoe (3 virgul), C2 Tek cizgi (Ege/Newgrange tek sarmal), C3 Koru (tek sekil).
// Kural: angle pozitif = ekranda saat yonunun tersi (CCW), dir=+1 CCW donus.
// Kollar dir=+1 iken merkezden disari SAAT YONUNDE kivrilir (kuyruk geride kalir),
// dir=-1 iken ayna goruntusu. Canvas'ta aci artisi ekranda saat yonudur.
(function () {
  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  var TAU = Math.PI * 2;

  // Ortak kurulum: daireye kirp, deseni dondur, dir'e gore aynala.
  function begin(ctx, cx, cy, R, st) {
    ctx.save();
    ctx.translate(cx, cy);
    ctx.beginPath();
    ctx.arc(0, 0, R, 0, TAU);
    ctx.clip();
    ctx.rotate(-st.angle);
    ctx.scale(st.dir >= 0 ? 1 : -1, 1);
  }
  function disc(ctx, R, color) {
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.arc(0, 0, R + 0.5, 0, TAU);
    ctx.fill();
  }
  function smooth(x) { x = x < 0 ? 0 : x > 1 ? 1 : x; return x * x * (3 - 2 * x); }

  // ---------------------------------------------------------------- C1
  // Mitsudomoe: 3 virgul. Yuvarlak bas onde, sivri kuyruk kenar boyunca geride.
  var C1_SAMPLES = 40;
  function commaPath(ctx, R, grow) {
    var rh = 0.36 * R + grow;          // bas yaricapi
    var hc = 0.50 * R;                 // bas merkezi yaricapi
    var Ro0 = hc + rh;                 // basin dis kenari
    var Ro1 = 0.95 * R;                // kuyrugun dis kenari (kenar halkasi)
    var sweep = 105 * Math.PI / 180;
    // bas
    ctx.moveTo(hc + rh, 0);
    ctx.arc(hc, 0, rh, 0, TAU);
    ctx.closePath();
    // kuyruk seridi: dis kenar -> uc -> ic kenar
    var i, s, w, ro, a, outer = [], inner = [];
    for (i = 0; i <= C1_SAMPLES; i++) {
      s = i / C1_SAMPLES;
      w = rh * Math.pow(1 - s, 1.25);
      ro = Ro0 + (Ro1 - Ro0) * smooth(s / 0.4);
      a = sweep * s;
      outer.push([ro * Math.cos(a), ro * Math.sin(a)]);
      inner.push([(ro - 2 * w) * Math.cos(a), (ro - 2 * w) * Math.sin(a)]);
    }
    ctx.moveTo(outer[0][0], outer[0][1]);
    for (i = 1; i <= C1_SAMPLES; i++) ctx.lineTo(outer[i][0], outer[i][1]);
    for (i = C1_SAMPLES; i >= 0; i--) ctx.lineTo(inner[i][0], inner[i][1]);
    ctx.closePath();
  }
  window.SPIRAL_VARIANTS.push({
    id: 'C1',
    name: 'Mitsudomoe (uc virgul)',
    movement: 'Halk / Dogu',
    colors: ['#04586c', '#f1e6c7'],
    draw: function (ctx, cx, cy, R, st) {
      begin(ctx, cx, cy, R, st);
      disc(ctx, R, '#04586c');
      var grow = 0.02 * R * (st.held || 0);
      ctx.fillStyle = '#f1e6c7';
      for (var k = 0; k < 3; k++) {
        ctx.save();
        ctx.rotate(k * TAU / 3);
        ctx.beginPath();
        commaPath(ctx, R, grow);
        ctx.fill('nonzero');
        ctx.restore();
      }
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------- C2
  // Tek surekli cizgi: Arsimet sarmali, cizgi kalinligi = bosluk (%50 doluluk).
  window.SPIRAL_VARIANTS.push({
    id: 'C2',
    name: 'Tek cizgi sarmal',
    movement: 'Halk / Dogu',
    colors: ['#362d70', '#fcae04'],
    draw: function (ctx, cx, cy, R, st) {
      begin(ctx, cx, cy, R, st);
      disc(ctx, R, '#362d70');
      var pitch = 0.40 * R;
      var h = 0.10 * R + 0.012 * R * (st.held || 0);   // yarim cizgi kalinligi
      var phiMax = TAU * (R + h) / pitch;
      ctx.strokeStyle = '#fcae04';
      ctx.lineWidth = 2 * h;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';
      ctx.beginPath();
      ctx.moveTo(0, 0);
      var step = 0.06;
      for (var p = step; p <= phiMax; p += step) {
        var r = pitch * p / TAU;
        ctx.lineTo(r * Math.cos(p), r * Math.sin(p));
      }
      ctx.stroke();
      ctx.restore();
    }
  });

  // ---------------------------------------------------------------- C3
  // Koru: merkezde top, ondan disari tek kivrim, kuyruk kenarda sivrilir.
  var C3_SAMPLES = 90;
  window.SPIRAL_VARIANTS.push({
    id: 'C3',
    name: 'Koru (tek sekil)',
    movement: 'Halk / Dogu',
    colors: ['#f1e6c7', '#362d70'],
    draw: function (ctx, cx, cy, R, st) {
      begin(ctx, cx, cy, R, st);
      disc(ctx, R, '#f1e6c7');
      var held = st.held || 0;
      var r0 = 0.20 * R, r1 = 0.97 * R;
      var wMax = 0.22 * R + 0.015 * R * held;
      var Phi = TAU * 1.30;
      var i, s, a, r, w, outer = [], inner = [];
      for (i = 0; i <= C3_SAMPLES; i++) {
        s = i / C3_SAMPLES;
        a = Phi * s;
        r = r0 + (r1 - r0) * s;
        w = wMax * Math.pow(1 - s, 0.85);
        outer.push([(r + w) * Math.cos(a), (r + w) * Math.sin(a)]);
        inner.push([Math.max(r - w, 0) * Math.cos(a), Math.max(r - w, 0) * Math.sin(a)]);
      }
      ctx.fillStyle = '#362d70';
      ctx.beginPath();
      ctx.arc(0, 0, r0 + 0.04 * R + 0.015 * R * held, 0, TAU);   // top
      ctx.moveTo(outer[0][0], outer[0][1]);
      for (i = 1; i <= C3_SAMPLES; i++) ctx.lineTo(outer[i][0], outer[i][1]);
      for (i = C3_SAMPLES; i >= 0; i--) ctx.lineTo(inner[i][0], inner[i][1]);
      ctx.closePath();
      ctx.fill('nonzero');
      ctx.restore();
    }
  });
})();
