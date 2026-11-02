"""Build the detailed Hampton crop from cached GSHHG and USGS 3DEP sources.
Dependencies: numpy, tifffile, pyshp, shapely (install into .tools/terrain-python).
Source acquisition and licenses: Documentation/Hampton-Coastline.md.
"""
import json, math, pathlib, sys
ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / '.tools/terrain-python'))
import numpy as np
import shapefile, tifffile
from shapely.geometry import shape, box, Polygon, mapping, Point
from shapely.geometry.polygon import orient
from shapely.ops import unary_union

cache = ROOT / 'Artifacts/Coastline'
region = box(-76.55, 36.86, -76.18, 37.12)
meters_x = 6371000 * math.pi / 180 * math.cos(math.radians(36.983))
meters_y = 6371000 * math.pi / 180

def refine_ring(ring):
    """Round corners within a 15 m neighborhood, then sample at <=25 m.
    No large headlands or inlets are invented by this bounded visual refinement.
    """
    p = [(x*meters_x, y*meters_y) for x,y in list(ring.coords)[:-1]]
    rounded = []
    for i, (x,y) in enumerate(p):
        a,b = p[i-1], p[(i+1)%len(p)]
        da,db = math.dist(a,(x,y)), math.dist(b,(x,y))
        radius = min(15, da*.2, db*.2)
        # Leave artificial crop corners fixed and straight.
        lon,lat=x/meters_x,y/meters_y
        if min(abs(lon-region.bounds[0]),abs(lon-region.bounds[2]),abs(lat-region.bounds[1]),abs(lat-region.bounds[3]))<1e-8:radius=0
        if radius<.01:rounded.append((x,y));continue
        start=(x+(a[0]-x)*radius/da, y+(a[1]-y)*radius/da)
        end=(x+(b[0]-x)*radius/db, y+(b[1]-y)*radius/db)
        for j in range(5):
            t=j/4;rounded.append(((1-t)**2*start[0]+2*t*(1-t)*x+t*t*end[0],(1-t)**2*start[1]+2*t*(1-t)*y+t*t*end[1]))
    out=[]
    for i,a in enumerate(rounded):
        b=rounded[(i+1)%len(rounded)];steps=max(1,math.ceil(math.dist(a,b)/25))
        for j in range(steps):
            t=j/steps;out.append(((a[0]+(b[0]-a[0])*t)/meters_x,(a[1]+(b[1]-a[1])*t)/meters_y))
    return out

reader=shapefile.Reader(str(cache/'GSHHS_shp/f/GSHHS_f_L1.shp'))
pieces=[];source_points=0
for item in reader.iterShapes(bbox=region.bounds):
    clipped=shape(item.__geo_interface__).intersection(region)
    if not clipped.is_empty:pieces.append(clipped)
land=unary_union(pieces)
polygons=[]
for polygon in (land.geoms if land.geom_type=='MultiPolygon' else [land]):
    polygon=orient(polygon,1)
    source_points+=len(polygon.exterior.coords)-1+sum(len(r.coords)-1 for r in polygon.interiors)
    refined=Polygon(refine_ring(polygon.exterior),[refine_ring(r) for r in polygon.interiors])
    if not refined.is_valid:raise RuntimeError('Rounded coastline self-intersects; review this crop rather than hiding invalid geometry')
    polygons.append(mapping(orient(refined,1)))

with tifffile.TiffFile(cache/'3dep.tif') as tif:
    pixels=tif.asarray();tags=tif.pages[0].tags
    sx,sy,_=tags['ModelPixelScaleTag'].value
    _,_,_,west,north,_=tags['ModelTiepointTag'].value
    height,width=pixels.shape
    if width!=377 or height!=269 or not np.isfinite(pixels).all() or pixels.min()< -1000:raise RuntimeError('Invalid 3DEP raster')
    elevation=dict(width=width,height=height,west=west+sx/2,east=west+sx*(width-.5),south=north-sy*(height-.5),north=north-sy/2,meters=np.round(pixels[::-1],3).ravel().tolist())
data=dict(metadata=dict(source='GSHHG 2.3.7 full-resolution shoreline; USGS 3DEP bare-earth DEM',coastlineLicense='GSHHG LGPL-3.0-or-later; source and processing retained',elevationLicense='USGS public domain',sourceCoastPoints=source_points,shoreRoundingMeters=15,elevationSampleRatio=4,historicalNote='Modern geographic data; bounded shoreline rounding, beach profiles, foliage and scattered rocks are visual reconstructions, not an 1862 survey.'),west=-76.55,east=-76.18,south=36.86,north=37.12,polygons=polygons,elevation=elevation)
(cache/'refined-geography.json').write_text(json.dumps(data,separators=(',',':')),encoding='utf-8')
print(json.dumps(dict(sourceCoastPoints=source_points,refinedCoastPoints=sum(len(p['coordinates'][0]) for p in polygons),width=width,height=height,fortOnLand=land.contains(Point(-76.308,37.003)),duelOnLand=land.contains(Point(-76.385,36.983)))))

# Clip a regular 55 m ground grid to the same refined shore. This avoids skinny
# fan triangles and non-conforming subdivision cracks at very detailed inlets.
import shapely
from shapely.affinity import affine_transform
local=unary_union([affine_transform(shape(p),[meters_x,0,0,meters_y,76.385*meters_x,-36.983*meters_y]) for p in polygons])
shapely.prepare(local)
x0,z0,x1,z1=local.bounds
xs=np.arange(math.floor(x0/55)*55,x1,55);zs=np.arange(math.floor(z0/55)*55,z1,55)
xx,zz=np.meshgrid(xs,zs);xx=xx.ravel();zz=zz.ravel()
cells=shapely.box(xx,zz,xx+55,zz+55)
full=shapely.covers(local,cells);touch=shapely.intersects(local,cells)&~full
points=[];triangles=[];ids={}
def vertex(x,z):
    key=(round(x,3),round(z,3))
    if key not in ids:ids[key]=len(points)//2;points.extend(key)
    return ids[key]
for x,z in zip(xx[full],zz[full]):
    a,b,c,d=vertex(x,z),vertex(x,z+55),vertex(x+55,z),vertex(x+55,z+55)
    triangles.extend([a,b,c,c,b,d])
boundary=[]
for cell in cells[touch]:
    clipped=local.intersection(cell)
    for p in clipped.geoms if hasattr(clipped,'geoms') else [clipped]:
        if p.geom_type=='Polygon' and p.area>.001:boundary.append(mapping(p)['coordinates'])
(cache/'ground-grid.json').write_text(json.dumps(dict(points=points,triangles=triangles,boundary=boundary),separators=(',',':')),encoding='utf-8')
print(json.dumps(dict(gridVertices=len(points)//2,interiorTriangles=len(triangles)//3,boundaryCells=len(boundary))))
