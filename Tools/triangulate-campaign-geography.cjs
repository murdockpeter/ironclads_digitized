// Triangulate the real shoreline-clipped 250 m terrain grid; retain all polygon holes.
const fs=require('node:fs'),path=require('node:path');
(async()=>{
 const root=path.resolve(__dirname,'..'),cache=path.join(root,'Artifacts/CampaignGeography');
 const {default:earcut}=await import(require('node:url').pathToFileURL(require.resolve('earcut',{paths:[path.join(root,'.tools')]})).href);
 const data=JSON.parse(fs.readFileSync(path.join(cache,'campaign-geography.json'),'utf8'));
 // Polygons support water/land queries; the rendered mesh is independently clipped at 250 m.
 for(const p of data.polygons)p.triangles=[];
 const grid=JSON.parse(fs.readFileSync(path.join(cache,'ground-grid.json'),'utf8'));
 const ids=new Map();for(let i=0;i<grid.points.length;i+=2)ids.set(grid.points[i].toFixed(3)+','+grid.points[i+1].toFixed(3),i/2);
 for(const polygon of grid.boundary){
  const flat=[],holes=[],mapping=[];
  for(const ring of polygon){if(flat.length)holes.push(flat.length/2);for(const [x,z] of ring.slice(0,-1)){
   flat.push(x,z);const key=x.toFixed(3)+','+z.toFixed(3);if(!ids.has(key)){ids.set(key,grid.points.length/2);grid.points.push(Number(x.toFixed(3)),Number(z.toFixed(3)));}mapping.push(ids.get(key));
  }}
  for(let j=0,t=earcut(flat,holes,2);j<t.length;j+=3){const a=mapping[t[j]],b=mapping[t[j+1]],c=mapping[t[j+2]];
   // Unity +Y faces require clockwise winding in east/north coordinates.
   const cross=(grid.points[b*2]-grid.points[a*2])*(grid.points[c*2+1]-grid.points[a*2+1])-(grid.points[b*2+1]-grid.points[a*2+1])*(grid.points[c*2]-grid.points[a*2]);
   grid.triangles.push(...(cross>0?[a,c,b]:[a,b,c]));
  }
 }
 // At theater-sized meter coordinates float32 quantization can reverse tiny boundary slivers.
 // Validate winding in the exact representation Unity will receive and drop collapsed slivers.
 grid.points=grid.points.map(Math.fround);const valid=[];
 for(let j=0;j<grid.triangles.length;j+=3){const [a,b,c]=grid.triangles.slice(j,j+3);const cross=(grid.points[b*2]-grid.points[a*2])*(grid.points[c*2+1]-grid.points[a*2+1])-(grid.points[b*2+1]-grid.points[a*2+1])*(grid.points[c*2]-grid.points[a*2]);if(Math.abs(cross)<.01)continue;valid.push(...(cross>0?[a,c,b]:[a,b,c]));}grid.triangles=valid;
 const bytes=Buffer.alloc(8+grid.points.length*4+grid.triangles.length*4);bytes.writeInt32LE(grid.points.length/2,0);bytes.writeInt32LE(grid.triangles.length,4);let offset=8;
 for(const p of grid.points){bytes.writeFloatLE(p,offset);offset+=4;}for(const i of grid.triangles){bytes.writeInt32LE(i,offset);offset+=4;}
 const out=path.join(root,'Assets/Resources/Geography');fs.writeFileSync(path.join(out,'JamesCampaign.json'),JSON.stringify(data));fs.writeFileSync(path.join(out,'JamesCampaignSurface.bytes'),bytes);
 const report={source:data.metadata,stations:data.stations,terrainVertices:grid.points.length/2,terrainTriangles:grid.triangles.length/3,meshBytes:bytes.length,grid:[data.elevation.width,data.elevation.height]};
 fs.writeFileSync(path.join(cache,'build-report.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
})().catch(e=>{console.error(e);process.exitCode=1;});
