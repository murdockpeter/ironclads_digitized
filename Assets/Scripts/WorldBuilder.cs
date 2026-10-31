using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static class WorldBuilder
    {
        public static Mesh Surface(float size, int n)
        {
            var v=new List<Vector3>(); var t=new List<int>();
            for(int z=0;z<=n;z++) for(int x=0;x<=n;x++) {
                v.Add(new Vector3((x/(float)n-.5f)*size,0,(z/(float)n-.5f)*size));
                if(x<n&&z<n) {int i=z*(n+1)+x;t.AddRange(new[]{i,i+n+1,i+1,i+1,i+n+1,i+n+2});}
            }
            return Shipwright.Mesh("Ocean surface",v,t);
        }
        public static GameObject Ocean(Transform root, Material material, float size, int n, float y)
        {
            var go=new GameObject("Atlantic surface");go.transform.SetParent(root,false);go.transform.localPosition=Vector3.up*y;
            go.AddComponent<MeshFilter>().sharedMesh=Surface(size,n);go.AddComponent<MeshRenderer>().sharedMaterial=material;return go;
        }
        public static Mesh GlobeMesh()
        {
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
            const int nx=256,ny=128;
            for(int j=0;j<=ny;j++) for(int i=0;i<=nx;i++) {
                double lat=-90+180.0*j/ny,lon=-180+360.0*i/nx;
                v.Add(Navigation.GlobePoint(lat,lon)*100); uv.Add(new Vector2(i/(float)nx,j/(float)ny));
                if(i<nx&&j<ny) {int q=j*(nx+1)+i;t.AddRange(new[]{q,q+nx+1,q+1,q+1,q+nx+1,q+nx+2});}
            }
            return Shipwright.Mesh("Geographic Earth",v,t,uv);
        }
        public static GameObject Globe(Transform root)
        {
            var go=new GameObject("Earth — NASA Blue Marble March");go.transform.SetParent(root,false);
            go.AddComponent<MeshFilter>().sharedMesh=GlobeMesh();
            var mat=Shipwright.MakeMaterial("NASA Blue Marble",Color.white,0,.12f);mat.mainTexture=Resources.Load<Texture2D>("EarthMarch");
            go.AddComponent<MeshRenderer>().sharedMaterial=mat;
            go.AddComponent<SphereCollider>().radius=100;
            var atmosphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);atmosphere.name="Atmosphere";
            Object.DestroyImmediate(atmosphere.GetComponent<Collider>());atmosphere.transform.SetParent(go.transform,false);
            atmosphere.transform.localScale=Vector3.one*205;
            atmosphere.GetComponent<Renderer>().sharedMaterial=new Material(Shader.Find("Ironclads/Atmosphere"));
            // Thin geographic graticule, on top of the actual satellite texture.
            var line=Shipwright.MakeMaterial("Graticule",new Color(.15f,.28f,.31f));
            for(int lat=-60;lat<=60;lat+=30) {
                var pts=new List<Vector3>();for(int lon=-180;lon<=180;lon+=3)pts.Add(Navigation.GlobePoint(lat,lon)*100.15f);
                Line(go.transform,"Latitude",pts.ToArray(),line,.035f);
            }
            for(int lon=-180;lon<180;lon+=30) {
                var pts=new List<Vector3>();for(int lat=-90;lat<=90;lat+=3)pts.Add(Navigation.GlobePoint(lat,lon)*100.15f);
                Line(go.transform,"Longitude",pts.ToArray(),line,.035f);
            }
            return go;
        }
        public static Texture2D WaterNormals()
        {
            const int n=256;var tex=new Texture2D(n,n,TextureFormat.RGBA32,true,true){name="Generated ocean detail",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=8};
            var h=new float[n*n];var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++) {
                float u=x/(float)n,v=y/(float)n;
                float value=0,amp=1,scale=5;
                for(int k=0;k<4;k++) {
                    float a=Mathf.Lerp(Mathf.PerlinNoise(u*scale+17,v*scale+31),Mathf.PerlinNoise((u-1)*scale+17,v*scale+31),u);
                    float b=Mathf.Lerp(Mathf.PerlinNoise(u*scale+17,(v-1)*scale+31),Mathf.PerlinNoise((u-1)*scale+17,(v-1)*scale+31),u);
                    value+=Mathf.Lerp(a,b,v)*amp;scale*=2;amp*=.5f;
                }h[y*n+x]=value;
            }
            for(int y=0;y<n;y++)for(int x=0;x<n;x++) {
                float dx=(h[y*n+(x+1)%n]-h[y*n+(x+n-1)%n])*7;
                float dy=(h[((y+1)%n)*n+x]-h[((y+n-1)%n)*n+x])*7;
                Vector3 p=new Vector3(-dx,-dy,1).normalized;pixels[y*n+x]=new Color(p.x*.5f+.5f,p.y*.5f+.5f,p.z*.5f+.5f,1);
            }
            tex.SetPixels(pixels);tex.Apply();return tex;
        }
        public static LineRenderer Line(Transform root,string name,Vector3[] points,Material m,float width)
        {
            var go=new GameObject(name);go.transform.SetParent(root,false);var l=go.AddComponent<LineRenderer>();
            l.sharedMaterial=m;l.useWorldSpace=false;l.positionCount=points.Length;l.SetPositions(points);l.startWidth=l.endWidth=width;
            return l;
        }
        public static ParticleSystem Particles(Transform root,string name,bool rain)
        {
            var go=new GameObject(name);go.transform.SetParent(root,false);var ps=go.AddComponent<ParticleSystem>();
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=true;main.playOnAwake=false;main.maxParticles=rain?9000:450;
            main.startLifetime=rain?3.5f:9;main.startSpeed=rain?32:1.5f;main.startSize=rain?.10f:2.5f;
            main.startColor=rain?new Color(.7f,.8f,.85f,.4f):new Color(.1f,.11f,.12f,.20f);
            main.simulationSpace=ParticleSystemSimulationSpace.World;
            var shape=ps.shape;shape.shapeType=rain?ParticleSystemShapeType.Box:ParticleSystemShapeType.Cone;
            if(rain)shape.scale=new Vector3(140,1,140);else {shape.angle=9;shape.radius=.5f;}
            var emission=ps.emission;emission.rateOverTime=rain?0:8;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;
            velocity.x=2;velocity.z=1;velocity.y=rain?0:2;
            var size=ps.sizeOverLifetime;size.enabled=!rain;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,4));
            var color=ps.colorOverLifetime;color.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
                new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(rain?.4f:.28f,.12f),new GradientAlphaKey(0,1)});
            color.color=gradient;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();
            var mat=new Material(Shader.Find("Ironclads/Weather Sprite"));
            var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++) {
                float r=new Vector2((x-31.5f)/32,(y-31.5f)/32).magnitude;
                pixels[y*64+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2));
            }
            tex.SetPixels(pixels);tex.Apply();mat.mainTexture=tex;renderer.sharedMaterial=mat;
            if(rain) {renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=3;renderer.velocityScale=.035f;}
            go.transform.localRotation=Quaternion.Euler(rain?90:-90,0,0);ps.Play();return ps;
        }
    }
}
