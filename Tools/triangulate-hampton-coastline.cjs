const fs=require('node:fs'),path=require('node:path');
(async()=>{
 const {default:earcut}=await import(require('node:url').pathToFileURL(require.resolve('earcut',{paths:[path.resolve(__dirname,'../.tools')]})).href);
 const data=JSON.parse(fs.readFileSync(path.resolve(__dirname,'../Artifacts/Coastline/refined-geography.json'),'utf8'));
 data.polygons=data.polygons.map(p=>{const points=[],ringStarts=[];for(const ring of p.coordinates){ringStarts.push(points.length);for(const [longitude,latitude] of ring.slice(0,-1))points.push({longitude,latitude});}return {points,ringStarts,triangles:earcut(points.flatMap(p=>[p.longitude,p.latitude]),ringStarts.slice(1),2)};});
 const target=path.resolve(__dirname,'../Assets/Resources/Geography/HamptonRoads.json');fs.writeFileSync(target,JSON.stringify(data));
 console.log({target,polygons:data.polygons.length,coastPoints:data.polygons.reduce((n,p)=>n+p.points.length,0),grid:[data.elevation.width,data.elevation.height]});
 const ground=JSON.parse(fs.readFileSync(path.resolve(__dirname,'../Artifacts/Coastline/ground-grid.json'),'utf8'));
 const ids=new Map();for(let i=0;i<ground.points.length;i+=2)ids.set(ground.points[i].toFixed(3)+','+ground.points[i+1].toFixed(3),i/2);
 for(const polygon of ground.boundary){const local=[],holes=[],mapping=[];for(const ring of polygon){if(local.length)holes.push(local.length/2);for(const [x,z] of ring.slice(0,-1)){local.push(x,z);const key=x.toFixed(3)+','+z.toFixed(3);if(!ids.has(key)){ids.set(key,ground.points.length/2);ground.points.push(Number(x.toFixed(3)),Number(z.toFixed(3)));}mapping.push(ids.get(key));}}for(const i of earcut(local,holes,2))ground.triangles.push(mapping[i]);}
 const bytes=Buffer.alloc(8+ground.points.length*4+ground.triangles.length*4);bytes.writeInt32LE(ground.points.length/2,0);bytes.writeInt32LE(ground.triangles.length,4);let offset=8;for(const p of ground.points){bytes.writeFloatLE(p,offset);offset+=4;}for(const i of ground.triangles){bytes.writeInt32LE(i,offset);offset+=4;}
 fs.writeFileSync(path.resolve(__dirname,'../Assets/Resources/Geography/HamptonSurface.bytes'),bytes);console.log({groundVertices:ground.points.length/2,groundTriangles:ground.triangles.length/3,bytes:bytes.length});
})().catch(e=>{console.error(e);process.exitCode=1;});
