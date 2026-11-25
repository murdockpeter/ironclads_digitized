"""Small geographic shoreline package for the Pantelleria tactical module.
Reuses the cached full-resolution GSHHG database. Relief is a flat display platform.
"""
import json,pathlib,sys
ROOT=pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.tools/terrain-python'))
import shapefile
from shapely.geometry import shape,box,mapping
from shapely.ops import unary_union
region=box(11.0,36.45,12.9,37.7)
reader=shapefile.Reader(str(ROOT/'Artifacts/Coastline/GSHHS_shp/f/GSHHS_f_L1.shp'))
land=unary_union([shape(s.__geo_interface__).intersection(region) for s in reader.iterShapes(bbox=region.bounds)])
out=ROOT/'Assets/StreamingAssets/GeographySource/Pantelleria-source.geojson'
out.write_text(json.dumps(dict(type='Feature',properties=dict(source='GSHHG 2.3.7 full resolution',relief='Flat display platform; no elevation or depth survey'),geometry=mapping(land)),separators=(',',':')),encoding='utf-8')
polys=[]
for p in land.geoms if hasattr(land,'geoms') else [land]:
    if p.geom_type!='Polygon':continue
    rings=[list(p.exterior.coords)[:-1]]+[list(r.coords)[:-1] for r in p.interiors]
    starts=[];points=[]
    for ring in rings:
        starts.append(len(points));points.extend(dict(longitude=x,latitude=y) for x,y in ring)
    polys.append(dict(points=points,ringStarts=starts))
cache=ROOT/'Artifacts/Pantelleria';cache.mkdir(parents=True,exist_ok=True)
(cache/'coast.json').write_text(json.dumps(dict(polygons=polys),separators=(',',':')),encoding='utf-8')
(out.parent/'Pantelleria-NOTICE.txt').write_text('Pantelleria module shoreline: GSHHG 2.3.7, Paul Wessel and Walter H. F. Smith, LGPL-3.0-or-later.\nSource: https://github.com/GenericMappingTools/gshhg-gmt/releases/tag/2.3.7\nReuses Artifacts/Coastline/GSHHS_shp/f/GSHHS_f_L1.*. Crop: 11.0â€“12.9 E, 36.45â€“37.7 N. Processing: Tools/build-pantelleria-coast.py and Tools/triangulate-pantelleria-coast.cjs.\nDerived clipped source is Pantelleria-source.geojson. Existing LGPL/GPL texts accompany this directory. Land is displayed as a flat platform, not reconstructed elevation, bathymetry or a 1901 shoreline.\n',encoding='utf-8')
print('Coast polygons:',len(polys),'points:',sum(len(p['points']) for p in polys))
