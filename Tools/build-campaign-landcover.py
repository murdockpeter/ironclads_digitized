"""Cache a nearest-neighbour NLCD 2021 crop and produce a naturalistic map palette.
Modern land cover, not reconstructed 1864 vegetation. No network access during play.
"""
import sys, pathlib, urllib.request, urllib.parse, json, hashlib
ROOT=pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.tools/terrain-python'))
import numpy as np, tifffile
from PIL import Image
cache=ROOT/'Artifacts/CampaignGeography';cache.mkdir(exist_ok=True,parents=True)
query=dict(service='WCS',version='1.0.0',request='GetCoverage',coverage='NLCD_2021_Land_Cover_L48',crs='EPSG:4326',bbox='-77.60,36.75,-75.80,37.65',width=4096,height=2560,format='GeoTIFF',interpolation='nearest neighbor')
url='https://www.mrlc.gov/geoserver/mrlc_display/wcs?'+urllib.parse.urlencode(query)
raw=cache/'nlcd-2021-4096.tif'
if not raw.exists():
 with urllib.request.urlopen(url,timeout=90) as response:raw.write_bytes(response.read())
with tifffile.TiffFile(raw) as source:
 a=source.asarray();transform=source.pages[0].tags['ModelTransformationTag'].value
 assert a.shape==(2560,4096) and a.dtype==np.uint8
 assert abs(transform[3]+77.60)<1e-6 and abs(transform[7]-37.65)<1e-6
 assert abs(transform[0]-1.8/4096)<1e-9 and abs(transform[5]+.9/2560)<1e-9
# Muted late-season cartographic colors; thematic classes, not satellite RGB.
palette={0:(133,125,95),255:(133,125,95),11:(119,133,106),12:(208,211,205),21:(151,150,119),22:(155,144,127),23:(141,135,128),24:(120,122,120),31:(177,163,132),41:(112,119,71),42:(65,99,69),43:(88,110,70),52:(139,137,91),71:(163,153,103),81:(167,153,97),82:(157,131,88),90:(91,119,99),95:(149,151,111)}
assert set(np.unique(a)).issubset(palette),np.unique(a)
rgb=np.empty((*a.shape,3),dtype=np.uint8)
for code,color in palette.items():rgb[a==code]=color
out=ROOT/'Assets/Resources/Geography/JamesCampaignLandCover.png'
Image.fromarray(rgb).save(out,optimize=True)
meta=dict(source='USGS/MRLC NLCD 2021 Land Cover L48',license='Public-domain USGS/MRLC land-cover data; original thematic palette',legend='https://www.mrlc.gov/data/legends/annual-nlcd-land-cover-legend',year=2021,width=4096,height=2560,bounds=[-77.60,36.75,-75.80,37.65],resampling='nearest neighbor',palette='muted late-season thematic interpretation; not satellite RGB',historicalNote='Modern land cover, not an 1864 reconstruction; modern developed areas remain visible.',url=url,sourceSHA256=hashlib.sha256(raw.read_bytes()).hexdigest(),classes={str(int(k)):int(v) for k,v in zip(*np.unique(a,return_counts=True))},paletteRGB=palette)
(ROOT/'Assets/StreamingAssets/GeographySource/JamesCampaign-landcover-NOTICE.json').write_text(json.dumps(meta,indent=2))
(cache/'landcover-build-report.json').write_text(json.dumps(meta,indent=2))
print(json.dumps({k:meta[k] for k in ['source','width','height','classes']},indent=2))
