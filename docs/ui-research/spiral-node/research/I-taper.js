// I: merkezden dışarı açılan, inceden kalına giden sarmal kol(lar). Renk = normal node'un 10 halkası:
// kol şekli bir "maske", içinde node'un eş merkezli halka renkleri (yarıçapa göre ramp) görünür.
// Arşimet sarmalı: kol merkezdeki çekirdekten çıkar, her turda dışa doğru aynı aralıkla açılır, kalınlık doğrusal artar.
(function(){
  var TAU = Math.PI*2;
  var RAMP = ['#f1e6c7','#ff5104','#fcae04','#888c03','#289aa4','#eeeac7','#04586c','#fc1e00','#ee7284','#362d70'];
  var CORE = '#F0D420', INK = '#140C1C';

  function taper(ctx, cx, cy, R, st, o){
    var dir = st.dir >= 0 ? 1 : -1, held = st.held || 0;
    var N = o.n, r0 = R*0.17, span = R - r0, steps = 90;
    var turn = span/o.turns;                       // bir arm'ın 1 tam turda dışa kat ettiği mesafe
    var f0 = o.fillIn, f1 = o.fillOut*(1+0.04*held);
    function P(r, th){ return [cx + r*Math.cos(th), cy - r*Math.sin(th)]; }
    ctx.save();
    ctx.beginPath(); ctx.arc(cx, cy, R, 0, TAU); ctx.clip();
    if(o.bg){ ctx.fillStyle = o.bg; ctx.fillRect(cx-R, cy-R, R*2, R*2); }
    for(var j=0;j<N;j++){
      var base = st.angle + j*TAU/N;
      ctx.save();
      ctx.beginPath();
      var i, u, r, th, p;
      for(i=0;i<=steps;i++){                       // arm'ın ön kenarı
        u = i/steps; r = r0*0.5 + (R*1.02 - r0*0.5)*u;
        th = base - dir*TAU*(r - r0)/turn;
        p = P(r, th); if(i===0) ctx.moveTo(p[0],p[1]); else ctx.lineTo(p[0],p[1]);
      }
      var rOut = R*1.02, thF = base - dir*TAU*(rOut - r0)/turn, wOut = (TAU/N)*f1;
      for(i=1;i<=24;i++){                          // dış uçta düz çizgi yerine yay (kalın kolda düz kesik oluşmasın)
        p = P(rOut, thF + wOut*i/24); ctx.lineTo(p[0],p[1]);
      }
      for(i=steps;i>=0;i--){                       // arka kenar: açısal genişlik merkezde ince, dışta kalın
        u = i/steps; r = r0*0.5 + (R*1.02 - r0*0.5)*u;
        var uu = Math.max(0, Math.min(1, (r - r0)/span));
        var w = (TAU/N)*(f0 + (f1 - f0)*uu);
        th = base + w - dir*TAU*(r - r0)/turn;
        p = P(r, th); ctx.lineTo(p[0],p[1]);
      }
      ctx.closePath(); ctx.clip();
      // maske içinde node'un halka renkleri (dıştan içe boya)
      for(var k=9;k>=0;k--){
        ctx.fillStyle = RAMP[k];
        ctx.beginPath(); ctx.arc(cx, cy, r0 + span*(k+1)/10 + (k===9?R:0), 0, TAU); ctx.fill();
      }
      ctx.restore();
    }
    ctx.restore();
    var cr = r0*(1+0.12*held);
    ctx.beginPath(); ctx.arc(cx, cy, cr, 0, TAU); ctx.fillStyle = CORE; ctx.fill();
    ctx.lineWidth = R*0.025; ctx.strokeStyle = INK; ctx.stroke();
  }

  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push({
    id:'I1', name:'Tek kol, 3 tur', movement:'İnceden kalına',
    colors:RAMP.slice(),
    draw:function(ctx,cx,cy,R,st){ taper(ctx,cx,cy,R,st,{n:1,turns:3,fillIn:0.05,fillOut:0.86}); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'I2', name:'İki kol, 2 tur', movement:'İnceden kalına',
    colors:RAMP.slice(),
    draw:function(ctx,cx,cy,R,st){ taper(ctx,cx,cy,R,st,{n:2,turns:2.2,fillIn:0.06,fillOut:0.84}); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'I3', name:'Üç kol, 1,5 tur', movement:'İnceden kalına',
    colors:RAMP.slice(),
    draw:function(ctx,cx,cy,R,st){ taper(ctx,cx,cy,R,st,{n:3,turns:1.5,fillIn:0.06,fillOut:0.84}); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'I4', name:'Tek kol, koyu zemin', movement:'İnceden kalına',
    colors:RAMP.slice().concat([INK]),
    draw:function(ctx,cx,cy,R,st){ taper(ctx,cx,cy,R,st,{n:1,turns:3,fillIn:0.05,fillOut:0.86,bg:INK}); }
  });
})();
