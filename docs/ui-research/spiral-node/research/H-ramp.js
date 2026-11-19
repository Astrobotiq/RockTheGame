// H: normal node'un 10 renkli rampasının TAMAMI sarmala sarılır. Normal node'un 10 halkası ayrı halka olmak yerine
// sarmal bant(lar) olur: her bant ramp'ten bir rengi alır, tek kuralla: renk = floor(10*u + dir*K*aci/2pi) mod 10.
// K = kol sayısı. K=1: tek şerit, her turda ramp'te bir adım (dikiş çizgisinde renk basamaklanır).
// Bant kalınlığı normal node halkasıyla aynı mantık: (R - r0)/10.
(function(){
  var TAU = Math.PI*2;
  var RAMP = ['#f1e6c7','#ff5104','#fcae04','#888c03','#289aa4','#eeeac7','#04586c','#fc1e00','#ee7284','#362d70'];
  var CORE = '#F0D420', INK = '#140C1C';

  function rampSpiral(ctx, cx, cy, R, st, K){
    var dir = st.dir >= 0 ? 1 : -1, held = st.held || 0;
    var r0 = R*0.17, span = R - r0, steps = 72;
    ctx.save();
    ctx.beginPath(); ctx.arc(cx, cy, R, 0, TAU); ctx.clip();
    // arka plan: en dış rengin altı boş kalmasın diye en içteki renk
    ctx.fillStyle = RAMP[0]; ctx.fillRect(cx-R, cy-R, R*2, R*2);
    for(var j=-K-1; j<=10+K; j++){
      ctx.fillStyle = RAMP[((j%10)+10)%10];
      ctx.beginPath();
      var i, th, u, r, p;
      for(i=0;i<=steps;i++){
        var rel = TAU*i/steps;                       // dikişten itibaren açı (dönen çerçevede)
        u = (j - dir*K*rel/TAU)/10;
        r = r0 + span*Math.max(0, Math.min(1.2, u));
        th = rel + st.angle;
        p = [cx + r*Math.cos(th), cy - r*Math.sin(th)];
        if(i===0) ctx.moveTo(p[0],p[1]); else ctx.lineTo(p[0],p[1]);
      }
      for(i=steps;i>=0;i--){
        var rel2 = TAU*i/steps;
        u = (j + 1 - dir*K*rel2/TAU)/10;
        r = r0 + span*Math.max(0, Math.min(1.2, u));
        th = rel2 + st.angle;
        p = [cx + r*Math.cos(th), cy - r*Math.sin(th)];
        ctx.lineTo(p[0],p[1]);
      }
      ctx.closePath(); ctx.fill();
    }
    ctx.restore();
    // çekirdek: normal node'un sarı diski
    var cr = r0*(1+0.12*held);
    ctx.beginPath(); ctx.arc(cx, cy, cr, 0, TAU); ctx.fillStyle = CORE; ctx.fill();
    ctx.lineWidth = R*0.025; ctx.strokeStyle = INK; ctx.stroke();
  }

  // Kalın iki sarmal kol (G1/A1 şekli); renk kolun BOYUNCA ramp'te 10 adımda ilerler. Kol 2, ramp'in diğer yarısından başlar.
  function rampArms(ctx, cx, cy, R, st, gap){
    var dir = st.dir >= 0 ? 1 : -1, held = st.held || 0;
    var N = 2, r0 = R*0.17, rmax = R*1.1, turns = 1.6, fill = 0.5;
    var turn = (rmax - r0)/turns, w = (TAU/N)*fill*(1+0.05*held), seg = 10, per = 6, steps = seg*per;
    ctx.save();
    ctx.beginPath(); ctx.arc(cx, cy, R, 0, TAU); ctx.clip();
    if(gap){ ctx.fillStyle = gap; ctx.fillRect(cx-R, cy-R, R*2, R*2); }
    function P(r, th){ return [cx + r*Math.cos(th), cy - r*Math.sin(th)]; }
    for(var j=0;j<N;j++){
      var base = st.angle + j*TAU/N;
      for(var s=0;s<seg;s++){
        var a = s*per, b = (s+1)*per, i, r, th, p;
        ctx.fillStyle = ctx.strokeStyle = RAMP[(s + j*5) % 10];
        ctx.lineWidth = 0.7; ctx.lineJoin = 'round';
        ctx.beginPath();
        for(i=a;i<=b;i++){
          r = r0*0.6 + (rmax - r0*0.6)*i/steps; th = base - dir*TAU*(r - r0)/turn;
          p = P(r, th); if(i===a) ctx.moveTo(p[0],p[1]); else ctx.lineTo(p[0],p[1]);
        }
        for(i=b;i>=a;i--){
          r = r0*0.6 + (rmax - r0*0.6)*i/steps; th = base + w - dir*TAU*(r - r0)/turn;
          p = P(r, th); ctx.lineTo(p[0],p[1]);
        }
        ctx.closePath(); ctx.fill(); ctx.stroke();
      }
    }
    ctx.restore();
    var cr = r0*(1+0.12*held);
    ctx.beginPath(); ctx.arc(cx, cy, cr, 0, TAU); ctx.fillStyle = CORE; ctx.fill();
    ctx.lineWidth = R*0.025; ctx.strokeStyle = INK; ctx.stroke();
  }

  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push({
    id:'H4', name:'Açık iki kol, boyuna 10 renk', movement:'Node 10 renk',
    colors:RAMP.slice(),
    draw:function(ctx,cx,cy,R,st){ rampArms(ctx,cx,cy,R,st,null); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'H1', name:'Tek şerit, 10 renk', movement:'Node 10 renk',
    colors:RAMP.slice(),
    draw:function(ctx,cx,cy,R,st){ rampSpiral(ctx,cx,cy,R,st,1); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'H2', name:'İki şerit, 10 renk', movement:'Node 10 renk',
    colors:RAMP.slice(),
    draw:function(ctx,cx,cy,R,st){ rampSpiral(ctx,cx,cy,R,st,2); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'H3', name:'Üç şerit, 10 renk', movement:'Node 10 renk',
    colors:RAMP.slice(),
    draw:function(ctx,cx,cy,R,st){ rampSpiral(ctx,cx,cy,R,st,3); }
  });
})();
