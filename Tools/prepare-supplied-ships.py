"""Bake the user's private OBJ/texture pairs into optional Unity resources.
Uses only Python's standard library. Originals are preserved under contextonly.
The generated third-party assets stay ignored by Git pending their provenance.
"""
import collections, hashlib, json, pathlib, shutil
root=pathlib.Path(__file__).resolve().parents[1]
target=root/'Assets/Resources/SuppliedShips';target.mkdir(parents=True,exist_ok=True)
for monitor,filename,texture in [(True,'Monitor_1.obj','Monitor_hull.001_Monitor_turret.001.png'),(False,'CSS Virginia.obj','CSS_Virginia_texture.jpg')]:
    path=root/'contextonly'/filename;v=[];uv=[];norm=[];faces=[]
    for line in path.read_text(encoding='utf-8').splitlines():
        p=line.split()
        if not p:continue
        if p[0]=='v':v.append(tuple(map(float,p[1:4])))
        elif p[0]=='vt':uv.append(tuple(map(float,p[1:3])))
        elif p[0]=='vn':norm.append(tuple(map(float,p[1:4])))
        elif p[0]=='f':faces.append([tuple(int(k)-1 if k else -1 for k in x.split('/')) for x in p[1:]])
    parent=list(range(len(v)))
    def find(x):
        while parent[x]!=x:parent[x]=parent[parent[x]];x=parent[x]
        return x
    for face in faces:
        for point in face[1:]:parent[find(point[0])]=find(face[0][0])
    components=collections.defaultdict(list)
    for face in faces:components[find(face[0][0])].append(face)
    components=sorted(components.values(),key=lambda fs:-len(fs))
    bounds=[]
    for fs in components:
        points=[v[i[0]] for f in fs for i in f]
        bounds.append([(min(p[a] for p in points),max(p[a] for p in points)) for a in range(3)])
    hull=next(b for b in bounds if b[0][1]-b[0][0]>(3 if monitor else 4) and b[1][1]<.06)
    scale=(52.4256 if monitor else 80.16)/(hull[0][1]-hull[0][0]);center=(hull[0][0]+hull[0][1])/2
    deck=.4572 if monitor else .29;offset=deck-hull[1][1]*scale
    parts=[]
    for i,fs in enumerate(components):
        b=bounds[i];role='fitting'
        if b==hull:role='hull'
        if monitor:
            if b[0][1]-b[0][0]>.45 and b[0][1]-b[0][0]<.48 and abs(b[1][1]-.236683)<.00001:role='turret'
            if b[1][0]>=.23668 and abs((b[0][0]+b[0][1])/2)<.25:role='roof-rail'
            if .09<b[1][0]<.10 and .13<b[1][1]<.14:role='barrel'
        else:
            if b[0][1]-b[0][0]>3 and b[1][0]>.05:role='casemate'
            if b[1][1]>.85:role='funnel'
            if len(fs)==46 and .17<b[1][0]<.18:role='barrel'
            if len(fs)==2:role='flag'
        positions=[];normals=[];texcoords=[];triangles=[];ids={}
        for f in fs:
            mapped=[]
            for corner in f:
                if corner not in ids:
                    ids[corner]=len(positions)//3;x,y,z=v[corner[0]]
                    positions.extend([-z*scale,y*scale+offset,(x-center)*scale])
                    n=norm[corner[2]] if len(corner)>2 and corner[2]>=0 else (0,1,0);normals.extend([-n[2],n[1],n[0]])
                    texcoords.extend(uv[corner[1]] if len(corner)>1 and corner[1]>=0 else (0,0))
                mapped.append(ids[corner])
            for j in range(1,len(mapped)-1):triangles.extend([mapped[0],mapped[j],mapped[j+1]])
        parts.append(dict(id=i,role=role,positions=positions,normals=normals,uv=texcoords,triangles=triangles))
    name='Monitor' if monitor else 'Virginia'
    data=dict(name=name,source=filename,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),scale=scale,sourceHullLength=hull[0][1]-hull[0][0],hullLength=52.4256 if monitor else 80.16,beam=(hull[2][1]-hull[2][0])*scale,parts=parts)
    (target/(name+'Data.json')).write_text(json.dumps(data,separators=(',',':')),encoding='utf-8')
    shutil.copyfile(root/'contextonly'/texture,target/(name+'Albedo'+pathlib.Path(texture).suffix))
    print(name,len(v),'OBJ positions,',sum(len(f)-2 for f in faces),'triangles;',len(parts),'components; scale',round(scale,5),'beam',round(data['beam'],3))
