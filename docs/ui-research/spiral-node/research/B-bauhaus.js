// Bauhaus / Constructivism / Suprematism / Swiss lens -- spiral node variants.
// Convention: all drawing happens in a "math" frame (y up, +angle = CCW on screen),
// clipped to the node circle. dir = -1 mirrors the whole design, so CW and CCW
// are exact mirror images. st.angle already carries the sign of dir (page does angle += dir*speed*dt).
(function () {
  var TAU = Math.PI * 2;

  function frame(ctx, cx, cy, R, st, fn) {
    var d = st.dir < 0 ? -1 : 1;
    ctx.save();
    ctx.beginPath();
    ctx.arc(cx, cy, R, 0, TAU);
    ctx.clip();
    ctx.translate(cx, cy);
    ctx.scale(1, -1);          // y up
    ctx.scale(d, 1);           // mirror for CW
    ctx.rotate(st.angle * d);  // mirror * rotate(angle*d) == rotate(angle) in the mirrored frame
    fn(ctx);
    ctx.restore();
  }

  function disc(ctx, x, y, r, col) {
    ctx.fillStyle = col;
    ctx.beginPath();
    ctx.arc(x, y, r, 0, TAU);
    ctx.fill();
  }

  // ---------------------------------------------------------------- B1
  // Uc kollu girdap: 3 equal arms, 50/50 coverage, constant angular width (60 deg).
  // Arms trail backwards (outer end lags the rotation) -> pattern reads as "flowing out".
  var B1_IND = '#362d70', B1_YEL = '#fcae04';
  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push({
    id: 'B1',
    name: 'Uc Kollu Girdap',
    movement: 'Bauhaus',
    colors: [B1_IND, B1_YEL],
    draw: function (ctx, cx, cy, R, st) {
      var h = st.held || 0;
      frame(ctx, cx, cy, R, st, function (c) {
        var N = 3, K = 2.1, W = Math.PI / N, STEPS = 40, RR = R * 1.06;
        disc(c, 0, 0, R * 1.01, B1_YEL);
        c.fillStyle = B1_IND;
        for (var j = 0; j < N; j++) {
          var a0 = TAU * j / N, i, u;
          c.beginPath();
          c.moveTo(0, 0);
          for (i = 1; i <= STEPS; i++) {
            u = i / STEPS;
            c.lineTo(RR * u * Math.cos(a0 - K * u), RR * u * Math.sin(a0 - K * u));
          }
          c.arc(0, 0, RR, a0 - K, a0 - K + W, false);
          for (i = STEPS; i >= 1; i--) {
            u = i / STEPS;
            c.lineTo(RR * u * Math.cos(a0 + W - K * u), RR * u * Math.sin(a0 + W - K * u));
          }
          c.closePath();
          c.fill();
        }
        disc(c, 0, 0, R * (0.15 + 0.03 * h), B1_IND);
        disc(c, 0, 0, R * (0.062 + 0.02 * h), B1_YEL);
      });
    }
  });

  // -------------------- B3 (Albers eccentric stack)
  // Albers: "Homage to the Circle". Six nested flat circles, two colors alternating,
  // each one nudged off its parent so the crescents curl into a spiral.
  // Centers are chained (each child stays inside its parent) and the 5 steps are spread
  // over a full turn so the innermost disc lands ~at the node center.
  var B2_PAP = '#f1e6c7', B2_TEA = '#04586c';
  var B2_RAD = [1, 0.80, 0.61, 0.44, 0.29, 0.15];
  window.SPIRAL_VARIANTS.push({
    id: 'B3',
    name: 'Albers Ic Ice Daireler',
    movement: 'Bauhaus',
    colors: [B2_PAP, B2_TEA],
    draw: function (ctx, cx, cy, R, st) {
      frame(ctx, cx, cy, R, st, function (c) {
        var x = 0, y = 0, i, g, th;
        var step = TAU / 5;
        disc(c, 0, 0, R * 1.01, B2_PAP);
        for (i = 1; i < B2_RAD.length; i++) {
          g = (B2_RAD[i - 1] - B2_RAD[i]) * 0.62 * R;
          th = -i * step;                    // negative: centers wind clockwise inward
          x += g * Math.cos(th);
          y += g * Math.sin(th);
          disc(c, x, y, B2_RAD[i] * R, (i % 2) ? B2_TEA : B2_PAP);
        }
      });
    }
  });

  // -------------------- B2 (Kandinsky single line)
  // Kandinsky, Point and Line to Plane: the line is the trace of a moving point.
  // One point in the middle, one Archimedean line of constant width, 3 turns. Nothing else.
  var B3_PAP = '#eeeac7', B3_ORA = '#ff5104';
  window.SPIRAL_VARIANTS.push({
    id: 'B2',
    name: 'Nokta ve Cizgi',
    movement: 'Bauhaus',
    colors: [B3_PAP, B3_ORA],
    draw: function (ctx, cx, cy, R, st) {
      var h = st.held || 0;
      frame(ctx, cx, cy, R, st, function (c) {
        var TURNS = 3, pitch = R / TURNS, lw = pitch * (0.5 + 0.06 * h);
        var maxPhi = TAU * TURNS * 1.08, STEPS = 220, i, p, r;
        disc(c, 0, 0, R * 1.01, B3_PAP);
        c.strokeStyle = B3_ORA;
        c.lineWidth = lw;
        c.lineCap = 'butt';
        c.lineJoin = 'round';
        c.beginPath();
        for (i = 0; i <= STEPS; i++) {
          p = maxPhi * i / STEPS;
          r = pitch * p / TAU;
          // trailing: outward travel goes clockwise (angle decreases)
          if (i === 0) c.moveTo(r * Math.cos(-p), r * Math.sin(-p));
          else c.lineTo(r * Math.cos(-p), r * Math.sin(-p));
        }
        c.stroke();
        disc(c, 0, 0, R * (0.12 + 0.02 * h), B3_ORA);
      });
    }
  });
})();
