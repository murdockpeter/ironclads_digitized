"""Fetch public geographic inputs for the detailed Hampton terrain builder."""
import json, pathlib, urllib.parse, urllib.request, zipfile
root=pathlib.Path(__file__).resolve().parents[1]
cache=root/'Artifacts/Coastline';cache.mkdir(parents=True,exist_ok=True)
archive=cache/'gshhg-shp-2.3.7.zip'
if not archive.exists():
    urllib.request.urlretrieve('https://github.com/GenericMappingTools/gshhg-gmt/releases/download/2.3.7/gshhg-shp-2.3.7.zip',archive)
with zipfile.ZipFile(archive) as package:
    for name in package.namelist():
        if name.startswith('GSHHS_shp/f/GSHHS_f_L1.') or name in ['LICENSE.TXT','COPYING.LESSERv3','COPYINGv3','README.TXT']:
            package.extract(name,cache)
query=dict(f='image',bbox='-76.56,36.85,-76.17,37.13',bboxSR='4326',imageSR='4326',size='377,269',format='tiff',pixelType='F32',renderingRule=json.dumps(dict(rasterFunction='None')),adjustAspectRatio='false',interpolation='RSP_BilinearInterpolation')
url='https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer/exportImage?'+urllib.parse.urlencode(query)
if not (cache/'3dep.tif').exists():
    request=urllib.request.Request(url,headers={'User-Agent':'Mozilla/5.0','Accept':'image/tiff,*/*'})
    with urllib.request.urlopen(request,timeout=45) as response:data=response.read()
    if data[:4]!=b'II*\x00':raise RuntimeError('USGS did not return the expected elevation TIFF')
    (cache/'3dep.tif').write_bytes(data)
(cache/'source-requests.json').write_text(json.dumps(dict(shoreline='https://github.com/GenericMappingTools/gshhg-gmt/releases/tag/2.3.7',elevation=url),indent=2),encoding='utf-8')
print('Geographic input cache ready:',cache)
