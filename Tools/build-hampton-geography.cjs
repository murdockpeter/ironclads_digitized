// Reuses the Natural Earth + NOAA ETOPO pipeline used in the user's campaign projects.
const fs=require('node:fs'),path=require('node:path');
const bounds={west:-76.55,east:-76.18,south:36.86,north:37.12};
const source=process.argv[2]||path.resolve(__dirname,'../../gcbh_dynamic_campaign/mission-map/renderer/data/global-land.geojson');
function clip(ring,inside,intersect){const out=[];for(let i=0;i<ring.length;i++){const a=ring[(i+ring.length-1)%ring.length],b=ring[i],ai=inside(a),bi=inside(b);if(ai!==bi)out.push(intersect(a,b));if(bi)out.push(b);}return out;}
function crop(ring){let p=ring.slice(0,-1);for(const [axis,value,greater] of [[0,bounds.west,true],[0,bounds.east,false],[1,bounds.south,true],[1,bounds.north,false]])p=clip(p,a=>greater?a[axis]>=value:a[axis]<=value,(a,b)=>{const t=(value-a[axis])/(b[axis]-a[axis]);return[a[0]+(b[0]-a[0])*t,a[1]+(b[1]-a[1])*t];});return p.filter((a,i)=>!i||Math.hypot(a[0]-p[i-1][0],a[1]-p[i-1][1])>1e-9);}
async function main(){
 const polygonClip=require(path.resolve(__dirname,'../.tools/node_modules/polygon-clipping'));
 const {default:earcut}=await import(require('node:url').pathToFileURL(require.resolve('earcut',{paths:[path.resolve(__dirname,'../.tools')]})).href);
 const land=JSON.parse(fs.readFileSync(source,'utf8')),polygons=[];
 const box=[[[bounds.west,bounds.south],[bounds.east,bounds.south],[bounds.east,bounds.north],[bounds.west,bounds.north],[bounds.west,bounds.south]]];
 for(const f of land.features){const sets=f.geometry.type==='Polygon'?[f.geometry.coordinates]:f.geometry.type==='MultiPolygon'?f.geometry.coordinates:[];for(const rings of sets){
  const outer=rings[0];if(!outer.some(([x,y])=>x>=bounds.west-1&&x<=bounds.east+1&&y>=bounds.south-1&&y<=bounds.north+1))continue;
  for(const clipped of polygonClip.intersection(rings,box)){const points=[],ringStarts=[];for(const ring of clipped){ringStarts.push(points.length);for(const [longitude,latitude] of ring.slice(0,-1))points.push({longitude,latitude});}const flat=points.flatMap(p=>[p.longitude,p.latitude]),triangles=earcut(flat,ringStarts.slice(1),2);if(points.length>=3&&triangles.length)polygons.push({points,ringStarts,triangles});}
 }}
 // Natural Earth's regional coastline omits Old Point Comfort at this scale.
 // Add a documented approximate footprint so the period Fort Monroe sits on land.
 const oldPoint=[[[-76.319,37.016],[-76.298,37.012],[-76.297,37.006],[-76.300,36.997],[-76.309,36.996],[-76.317,37.004],[-76.319,37.016]]];
 const merged=polygonClip.union(...polygons.map(p=>[p.points.map(q=>[q.longitude,q.latitude]).concat([[p.points[0].longitude,p.points[0].latitude]])]),oldPoint);polygons.length=0;
 for(const rings of merged){const points=[],ringStarts=[];for(const r of rings){ringStarts.push(points.length);for(const [longitude,latitude] of r.slice(0,-1))points.push({longitude,latitude});}polygons.push({points,ringStarts,triangles:earcut(points.flatMap(p=>[p.longitude,p.latitude]),ringStarts.slice(1),2)});}
 const url='https://coastwatch.pfeg.noaa.gov/erddap/griddap/ETOPO_2022_v1_15s.csv0?z[(36.85):1:(37.13)][(-76.56):1:(-76.17)]';
 const response=await fetch(url,{signal:AbortSignal.timeout(120000)});if(!response.ok)throw Error('NOAA HTTP '+response.status);
 const rows=(await response.text()).trim().split(/\r?\n/).map(line=>line.split(',').map(Number));
 if(rows.some(r=>r.length!==3||r.some(n=>!Number.isFinite(n))))throw Error('Invalid elevation CSV');
 const lat=rows[0][0];let width=rows.findIndex(r=>r[0]!==lat);if(width<2||rows.length%width)throw Error('Elevation grid shape');
 const height=rows.length/width;
 const data={metadata:{source:'Natural Earth 1:10m land and minor islands; NOAA ETOPO 2022 15 arc-second relief',coastlineLicense:'Natural Earth public domain',elevationLicense:'US government work; not for navigation',elevationDoi:'10.25921/fd45-gt74',elevationUrl:url,accessedUtc:new Date().toISOString(),historicalNote:'Modern generalized geography, with an approximate Old Point Comfort footprint added for Fort Monroe where the regional coastline omits it. Foliage and surface color are procedural, not surveyed land cover.'},...bounds,polygons,elevation:{width,height,west:rows[0][1],east:rows[width-1][1],south:rows[0][0],north:rows[(height-1)*width][0],meters:rows.map(r=>r[2])}};
 const target=path.resolve(__dirname,'../Assets/Resources/Geography/HamptonRoads.json');fs.mkdirSync(path.dirname(target),{recursive:true});fs.writeFileSync(target,JSON.stringify(data));console.log(JSON.stringify({target,polygons:polygons.length,coastPoints:polygons.reduce((n,p)=>n+p.points.length,0),width,height,min:Math.min(...data.elevation.meters),max:Math.max(...data.elevation.meters)}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});
