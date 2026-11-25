"""Validate geographic source coverage, connected river water and terrain packaging."""
import json, pathlib, struct, sys, math
ROOT=pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.tools/terrain-python'))
import numpy as np
from shapely.geometry import shape,box,Point
land=shape(json.loads((ROOT/'Assets/StreamingAssets/GeographySource/JamesCampaign-land.geojson').read_text())['features'][0]['geometry'])
data=json.loads((ROOT/'Assets/Resources/Geography/JamesCampaign.json').read_text())
checks=[]
def check(value,name):
    if not value:raise AssertionError(name)
    checks.append(name)
check(land.is_valid,'valid derived shoreline geometry')
check(data['metadata']['riverFeatureCount']>=200,'high-resolution USGS river polygons acquired')
check(data['metadata']['shoreCoordinates']>100000,'detailed shoreline vertices retained')
water=box(data['west'],data['south'],data['east'],data['north']).difference(land)
components=list(water.geoms) if hasattr(water,'geoms') else [water]
ocean=next(p for p in components if p.covers(Point(-75.87,37.0)))
for station in data['stations']:
    p=Point(station['longitude'],station['latitude'])
    check(not land.covers(p),station['name']+' anchor on mapped water')
    check(ocean.covers(p),station['name']+' connected to Atlantic water polygon')
check(land.covers(Point(-76.308,37.003)),'Fort Monroe land preserved')
check(not land.covers(Point(-76.385,36.983)),'Hampton Roads duel water preserved')
g=data['elevation'];heights=np.array(g['meters'])
check(g['width']==1025 and g['height']==641 and len(heights)==1025*641,'full-theater elevation grid dimensions')
check(np.isfinite(heights).all() and heights.max()>50,'finite measured elevation and regional relief')
raw=(ROOT/'Assets/Resources/Geography/JamesCampaignSurface.bytes').read_bytes();vertices,indices=struct.unpack_from('<ii',raw)
check(len(raw)==8+vertices*8+indices*4,'binary terrain size matches header')
points=np.frombuffer(raw,dtype='<f4',count=vertices*2,offset=8).reshape(-1,2)
triangles=np.frombuffer(raw,dtype='<i4',count=indices,offset=8+vertices*8).reshape(-1,3)
check(np.isfinite(points).all() and triangles.min()>=0 and triangles.max()<vertices,'finite terrain coordinates and valid indices')
a,b,c=points[triangles[:,0]],points[triangles[:,1]],points[triangles[:,2]]
cross=(b[:,0]-a[:,0])*(c[:,1]-a[:,1])-(b[:,1]-a[:,1])*(c[:,0]-a[:,0])
# Allow tiny zero-area triangles caused by millimeter rounding, but no downward faces.
check((cross<=.1).all(),'terrain triangles face upward')
report='PASS: '+str(len(checks))+' geographic checks\n'+'\n'.join(checks)+'\n'
(ROOT/'Artifacts/CampaignGeography/geography-verification.txt').write_text(report)
print(report)
