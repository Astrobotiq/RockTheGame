// G: benim önerilerim. Ajanların ortak bulgusundan türetildi: 2 kol, düz iki renk, kollar dönüşün arkasında,
// 50/50 kaplama. Üstüne tek ekleme: ortada normal node'un sarı diski ("bu bir node"). Arşimet (sabit aralık) sarmal:
// iki renk dönerken dışa akar, dış kenarda bile kolların okunur kalır.
(function(){
  var TAU = Math.PI*2, CORE = '#F0D420', INK = '#140C1C';
  function arms(ctx, cx, cy, R, st, o){
    var dir = st.dir >= 0 ? 1 : -1, held = st.held || 0;
    var N = o.n, r0 = R*0.17, steps = 48;
    var turn = (R*1.1 - r0) / o.turns;                 // 1 tam tur burulma için radyal mesafe
    var w = (TAU/N) * o.fill * (1 + 0.05*held);
    ctx.save();
    ctx.beginPath(); ctx.arc(cx, cy, R, 0, TAU); ctx.clip();
    if(o.disc){ ctx.fillStyle = o.disc; ctx.fillRect(cx-R, cy-R, R*2, R*2); }
    ctx.fillStyle = o.arm;
    function P(r, th){ return [cx + r*Math.cos(th), cy - r*Math.sin(th)]; } // ekranda saat tersi = +th
    for(var j=0;j<N;j++){
      var base = st.angle + j*TAU/N;
      ctx.beginPath();
      for(var i=0;i<=steps;i++){
        var r = r0*0.6 + (R*1.1 - r0*0.6)*i/steps;
        var th = base - dir*TAU*(r - r0)/turn;
        var p = P(r, th); if(i===0) ctx.moveTo(p[0],p[1]); else ctx.lineTo(p[0],p[1]);
      }
      for(var i2=steps;i2>=0;i2--){
        var r2 = r0*0.6 + (R*1.1 - r0*0.6)*i2/steps;
        var th2 = base + w - dir*TAU*(r2 - r0)/turn;
        var q = P(r2, th2); ctx.lineTo(q[0], q[1]);
      }
      ctx.closePath(); ctx.fill();
    }
    ctx.restore();
    // çekirdek: normal node'un sarı diski
    var cr = R*0.17*(1+0.12*held);
    ctx.beginPath(); ctx.arc(cx, cy, cr, 0, TAU); ctx.fillStyle = CORE; ctx.fill();
    ctx.lineWidth = R*0.025; ctx.strokeStyle = INK; ctx.stroke();
  }
  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push({
    id:'G1', name:'Düğüm + iki kol (dolu)', movement:'Benim önerim',
    colors:['#ff5104','#f1e6c7','#F0D420'],
    draw:function(ctx,cx,cy,R,st){ arms(ctx,cx,cy,R,st,{n:2,turns:1.6,fill:0.5,disc:'#f1e6c7',arm:'#ff5104'}); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'G2', name:'Düğüm + açık kol', movement:'Benim önerim',
    colors:['#362d70','#F0D420'],
    draw:function(ctx,cx,cy,R,st){ arms(ctx,cx,cy,R,st,{n:2,turns:1.6,fill:0.38,disc:null,arm:'#362d70'}); }
  });
  window.SPIRAL_VARIANTS.push({
    id:'G3', name:'Düğüm + üç kol (dolu)', movement:'Benim önerim',
    colors:['#362d70','#f1e6c7','#F0D420'],
    draw:function(ctx,cx,cy,R,st){ arms(ctx,cx,cy,R,st,{n:3,turns:1.1,fill:0.5,disc:'#f1e6c7',arm:'#362d70'}); }
  });
})();
