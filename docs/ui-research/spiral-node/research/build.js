// spiral-yonler.src.html -> spiral-yonler.html (script ve gorselleri icine gomer). Calistir: node research/build.js
const fs=require('fs'),path=require('path');
const root=path.join(__dirname,'..');
let h=fs.readFileSync(path.join(root,'spiral-yonler.src.html'),'utf8');
h=h.replace(/<script src="(research\/[^"]+)"[^>]*><\/script>/g,(m,p)=>'<script>\n'+fs.readFileSync(path.join(root,p),'utf8').replace(/<\/script/g,'<\/script')+'\n</script>');
fs.writeFileSync(path.join(root,'spiral-yonler.html'),h);
console.log('ok',(h.length/1024).toFixed(0)+' KB');
