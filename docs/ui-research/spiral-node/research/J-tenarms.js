// J: dolu disk, normal node'un 10 rengi = 10 kol. Kollar merkezde tek noktada birleşir (ince), dışa doğru kıvrılarak açılır (kalın).
// Kıvrım miktarı = merkezden kenara kaç tur geriye döndüğü. Boşluk yok, siyah yok, yeni renk yok.
// Desen sabit bir görüntü: bir kez çizilir (per-pixel, 2x2 AA), sonra döndürülür; dir=-1 ayna.
(function(){
  var TAU = Math.PI*2;
  var RAMP = ['#f1e6c7','#ff5104','#fcae04','#888c03','#289aa4','#eeeac7','#04586c','#fc1e00','#ee7284','#362d70'];
  var RGB = RAMP.map(function(h){ return [parseInt(h.substr(1,2),16),parseInt(h.substr(3,2),16),parseInt(h.substr(5,2),16)]; });

  function prerender(twist, size){
    var c = document.createElement('canvas'); c.width = c.height = size;
    var x = c.getContext('2d'), im = x.createImageData(size,size), d = im.data, R = size/2;
    var ss = [[.25,.25],[.75,.25],[.25,.75],[.75,.75]];
    for(var py=0;py<size;py++) for(var px=0;px<size;px++){
      var r0=0,g0=0,b0=0,a0=0;
      for(var s=0;s<4;s++){
        var dx=(px+ss[s][0]-R)/R, dy=-(py+ss[s][1]-R)/R, r=Math.sqrt(dx*dx+dy*dy);
        if(r>1) continue;
        var p = Math.atan2(dy,dx)/TAU + twist*r; p -= Math.floor(p);   // dönüş yönünün arkasında kıvrılır
        var col = RGB[Math.min(9,Math.floor(p*10))]; r0+=col[0]; g0+=col[1]; b0+=col[2]; a0++;
      }
      var o=(py*size+px)*4;
      if(a0){ d[o]=r0/a0; d[o+1]=g0/a0; d[o+2]=b0/a0; d[o+3]=a0*63.75; }
    }
    x.putImageData(im,0,0); return c;
  }

  function make(id, name, twist){
    var img = null;
    return {
      id:id, name:name, movement:'10 kol', colors:RAMP.slice(),
      draw:function(ctx,cx,cy,R,st){
        if(!img) img = prerender(twist, 520);
        ctx.save(); ctx.translate(cx,cy); ctx.rotate(-st.angle); if(st.dir < 0) ctx.scale(-1,1);
        ctx.imageSmoothingEnabled = true; ctx.imageSmoothingQuality = 'high';
        ctx.drawImage(img,-R,-R,R*2,R*2); ctx.restore();
      }
    };
  }
  window.SPIRAL_VARIANTS = window.SPIRAL_VARIANTS || [];
  window.SPIRAL_VARIANTS.push(make('J1','10 kol, hafif kıvrım',0.6));
  window.SPIRAL_VARIANTS.push(make('J2','10 kol, orta kıvrım',1.0));
  window.SPIRAL_VARIANTS.push(make('J3','10 kol, güçlü kıvrım',1.7));
})();
