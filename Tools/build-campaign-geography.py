"""Build an offline James River campaign basemap from the existing GSHHG cache,
USGS NHD river-area polygons and a USGS 3DEP elevation crop. No invented river widths.
Dependencies match Tools/build-hampton-coastline.py; Node earcut finishes triangulation.
"""
import concurrent.futures, hashlib, json, math, pathlib, subprocess, sys, time, urllib.parse, urllib.request
ROOT=pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.tools/terrain-python'))
import numpy as np
import shapefile, shapely, tifffile
from shapely.geometry import shape, box, mapping, Point, Polygon
from shapely.ops import unary_union
from shapely.affinity import affine_transform

CACHE=ROOT/'Artifacts/CampaignGeography';CACHE.mkdir(parents=True,exist_ok=True)
OUT=ROOT/'Assets/StreamingAssets/GeographySource';OUT.mkdir(parents=True,exist_ok=True)
WEST,SOUTH,EAST,NORTH=-77.60,36.75,-75.80,37.65
LAT,LON=37.20,-76.70
MX=6371000*math.pi/180*math.cos(math.radians(LAT));MZ=6371000*math.pi/180
REGION=box(WEST,SOUTH,EAST,NORTH)
requests=[]

def fetch(url,path):
    requests.append(dict(url=url,file=path.name))
    if path.exists():return path.read_bytes()
    for attempt in range(3):
        try:
            request=urllib.request.Request(url,headers={'User-Agent':'IroncladsCampaign/1.9 (geography build)','Accept':'*/*'})
            with urllib.request.urlopen(request,timeout=45) as response:data=response.read()
            path.write_bytes(data);return data
        except Exception:
            if attempt==2:raise
            time.sleep(1)

# NHDArea represents the measured areal extents of rivers, rather than a buffered centerline.
base='https://hydro.nationalmap.gov/arcgis/rest/services/nhd/MapServer/9/query'
features=[];offset=0
while True:
    query=dict(f='geojson',where='FTYPE IN (460,312,336)',geometry=f'{WEST},{SOUTH},{EAST},{NORTH}',geometryType='esriGeometryEnvelope',inSR=4326,spatialRel='esriSpatialRelIntersects',outSR=4326,outFields='OBJECTID,GNIS_NAME,FTYPE,PERMANENT_IDENTIFIER',returnGeometry='true',orderByFields='OBJECTID',resultOffset=offset,resultRecordCount=1000)
    data=json.loads(fetch(base+'?'+urllib.parse.urlencode(query),CACHE/f'nhd-area-{offset}.geojson'))
    if 'error' in data:raise RuntimeError(data['error'])
    page=data.get('features',[]);features+=page
    if not data.get('exceededTransferLimit',False) and len(page)<1000:break
    if not page:break
    offset+=len(page)
if not features:raise RuntimeError('NHD returned no river polygons')
water=unary_union([shapely.make_valid(shape(f['geometry'])).intersection(REGION) for f in features if f.get('geometry')])
print('NHD features',len(features),'water polygon coordinates',shapely.get_num_coordinates(water),flush=True)

shp=ROOT/'Artifacts/Coastline/GSHHS_shp/f/GSHHS_f_L1.shp'
if not shp.exists():raise RuntimeError('Run Tools/fetch-hampton-coastline.py to populate the existing GSHHG cache first.')
reader=shapefile.Reader(str(shp))
coast=unary_union([shapely.make_valid(shape(s.__geo_interface__)).intersection(REGION) for s in reader.iterShapes(bbox=REGION.bounds)])
land=shapely.make_valid(coast.difference(water))
# Preserve holes/islands; no hand-drawn river channel is added.
land=land.simplify(0.000015,preserve_topology=True) # about 1–2 m, well below campaign display resolution
source=dict(type='FeatureCollection',features=[dict(type='Feature',properties=dict(source='GSHHG 2.3.7 land minus USGS NHD river/bay/canal areas; modern geography'),geometry=mapping(land))])
(OUT/'JamesCampaign-land.geojson').write_text(json.dumps(source,separators=(',',':')),encoding='utf-8')
(OUT/'JamesCampaign-NHD.geojson').write_text(json.dumps(dict(type='FeatureCollection',features=features),separators=(',',':')),encoding='utf-8')

query=dict(f='image',bbox=f'{WEST},{SOUTH},{EAST},{NORTH}',bboxSR=4326,imageSR=4326,size='1025,641',format='tiff',pixelType='F32',renderingRule=json.dumps(dict(rasterFunction='None')),adjustAspectRatio='false',interpolation='RSP_BilinearInterpolation')
url='https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer/exportImage?'+urllib.parse.urlencode(query)
fetch(url,CACHE/'3dep-campaign.tif')
with tifffile.TiffFile(CACHE/'3dep-campaign.tif') as tif:
    raster=tif.asarray();tags=tif.pages[0].tags
    sx,sy,_=tags['ModelPixelScaleTag'].value;_,_,_,west,north,_=tags['ModelTiepointTag'].value
    height,width=raster.shape
    if width!=1025 or height!=641:raise RuntimeError('Unexpected elevation raster shape')
    # Ocean cells can be NoData: these are excluded by land clipping. Fill only unavailable heights.
    valid=np.isfinite(raster)&(raster>-1000)&(raster<10000)
    nodata=int((~valid).sum());raster=np.where(valid,raster,0)
    elevation=dict(width=width,height=height,west=west+sx/2,east=west+sx*(width-.5),south=north-sy*(height-.5),north=north-sy/2,meters=np.round(raster[::-1],2).ravel().tolist())

local=affine_transform(land,[MX,0,0,MZ,-LON*MX,-LAT*MZ]);shapely.prepare(local)
step=250
x0,z0,x1,z1=local.bounds
xs=np.arange(math.floor(x0/step)*step,x1,step);zs=np.arange(math.floor(z0/step)*step,z1,step)
xx,zz=np.meshgrid(xs,zs);xx=xx.ravel();zz=zz.ravel();cells=shapely.box(xx,zz,xx+step,zz+step)
full=shapely.covers(local,cells);touch=shapely.intersects(local,cells)&~full
points=[];triangles=[];ids={}
def vertex(x,z):
    key=round(float(x),3),round(float(z),3)
    if key not in ids:ids[key]=len(points)//2;points.extend(key)
    return ids[key]
for x,z in zip(xx[full],zz[full]):
    a,b,c,d=vertex(x,z),vertex(x,z+step),vertex(x+step,z),vertex(x+step,z+step);triangles.extend([a,b,c,c,b,d])
boundary=[]
def polygons(geometry):
    if geometry.geom_type=='Polygon':yield geometry
    elif hasattr(geometry,'geoms'):
        for piece in geometry.geoms:yield from polygons(piece)
for cell in cells[touch]:
    for p in polygons(local.intersection(cell)):
        if p.area>.01:boundary.append(mapping(p)['coordinates'])
print('Ground interior vertices',len(points)//2,'boundary cells',len(boundary),flush=True)
(CACHE/'ground-grid.json').write_text(json.dumps(dict(points=points,triangles=triangles,boundary=boundary),separators=(',',':')),encoding='utf-8')

# Anchors are operational viewing/placement presets, not historical obstructions or starting stations.
# Deep Bottom anchor is in the river adjacent to the DWR landing (37.40723874,-77.3053799).
anchors=[('Upper James',37.425,-77.421), ("Trent's Reach",37.383,-77.337),('Deep Bottom',37.405,-77.304),('City Point',37.318,-77.270),('Lower James',37.113,-76.660),('Hampton Roads',36.983,-76.385),('Virginia Capes',36.995,-76.015),('Open Atlantic',37.0,-75.87)]
# Check anchors against real polygons and snap to nearby water only if the supplied viewing point is on a bank.
openwater=REGION.difference(land)
from shapely.ops import nearest_points
stations=[]
for name,lat,lon in anchors:
    p=Point(lon,lat);distance=0
    if land.covers(p):
        safe=openwater.buffer(-0.00012)
        nearest=nearest_points(p,safe)[1];distance=math.hypot((nearest.x-lon)*MX,(nearest.y-lat)*MZ);lon,lat=nearest.x,nearest.y
        if distance>2500:raise RuntimeError(f'{name} anchor is {distance:.0f}m from mapped water; review coordinates')
    stations.append(dict(name=name,latitude=lat,longitude=lon,snappedMeters=round(distance,1)))
    print(name,round(lat,6),round(lon,6),'snap',round(distance,1),'m',flush=True)

metadata=dict(source='Cached GSHHG 2.3.7 full-resolution coastlines; USGS NHD high-resolution river-area polygons; USGS 3DEP bare-earth elevation',coastlineLicense='GSHHG LGPL-3.0-or-later',hydrographyLicense='USGS public domain',elevationLicense='USGS public domain',historicalNote='Modern geographic data, not an 1864 shoreline or navigational/bathymetric chart. Modern channel cuts and reclamation may be present. Station anchors are approximate umpire presets, not surveyed historical positions.',originLatitude=LAT,originLongitude=LON,terrainSpacingMeters=step,riverFeatureCount=len(features),shoreCoordinates=int(shapely.get_num_coordinates(land)),elevationNoDataCells=nodata)
result=dict(metadata=metadata,west=WEST,east=EAST,south=SOUTH,north=NORTH,stations=stations,elevation=elevation,polygons=[])
for p in polygons(land):
    rings=[list(p.exterior.coords)[:-1]]+[list(r.coords)[:-1] for r in p.interiors]
    result['polygons'].append(dict(points=[dict(longitude=x,latitude=y) for ring in rings for x,y in ring],ringStarts=list(np.cumsum([0]+[len(r) for r in rings[:-1]]).astype(int).tolist())))
(CACHE/'campaign-geography.json').write_text(json.dumps(result,separators=(',',':')),encoding='utf-8')
(CACHE/'source-requests.json').write_text(json.dumps(requests,indent=2),encoding='utf-8')
(OUT/'JamesCampaign-NOTICE.txt').write_text('James River campaign basemap\n'+json.dumps(metadata,indent=2)+'\n\nGSHHG notices and LGPL/GPL license texts accompany this directory. The cached full-resolution GSHHG input is clipped and differenced against USGS NHD polygons by Tools/build-campaign-geography.py; the derived source geometry and USGS query features are included alongside this notice. Rebuild with that script and Tools/triangulate-campaign-geography.cjs.\n\nSources:\nhttps://github.com/GenericMappingTools/gshhg-gmt/releases/tag/2.3.7\nhttps://hydro.nationalmap.gov/arcgis/rest/services/nhd/MapServer/9\nhttps://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer\nhttps://dwr.virginia.gov/place/deep-bottom/\n\nExact build-time requests:\n'+json.dumps(requests,indent=2),encoding='utf-8')
