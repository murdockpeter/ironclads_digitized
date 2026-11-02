using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public partial class HamptonTerrain
    {
        public float beachHeading;
        bool CropEdge(Vector3 a,Vector3 b)
        {
            var lo=Point(geography.south,geography.west);var hi=Point(geography.north,geography.east);
            return Mathf.Abs(a.x-lo.x)<.1f&&Mathf.Abs(b.x-lo.x)<.1f||Mathf.Abs(a.x-hi.x)<.1f&&Mathf.Abs(b.x-hi.x)<.1f||Mathf.Abs(a.z-lo.z)<.1f&&Mathf.Abs(b.z-lo.z)<.1f||Mathf.Abs(a.z-hi.z)<.1f&&Mathf.Abs(b.z-hi.z)<.1f;
        }
        void BuildDetailedBeaches()
        {
            var beach=new Material(grass){name="Wet sand / dry sand / meadow transition"};beach.SetFloat("_Beach",1);
            var rock=Shipwright.MakeMaterial("Weathered shoreline outcrops",new Color(.33f,.31f,.25f),0,.12f);
            float nearest=float.PositiveInfinity;Vector3 inspection=Vector3.zero;
            foreach(var ring in geographicCoasts){
                var v=new List<Vector3>();var t=new List<int>();var colors=new List<Color>();float walked=0;
                for(int i=0;i<ring.Length;i++){
                    var p=ring[i];var prev=ring[(i+ring.Length-1)%ring.Length];var next=ring[(i+1)%ring.Length];
                    Vector3 tangent=(next-prev).normalized,inward=Vector3.Cross(tangent,Vector3.up).normalized;
                    float variation=Mathf.PerlinNoise(p.x*.005f+37,p.z*.005f+91),width=Mathf.Lerp(12,28,variation),landScale=1,seaScale=1;
                    for(int guard=0;guard<6&&!Land(p+inward*width*landScale);guard++)landScale*=.5f;
                    for(int guard=0;guard<6&&Land(p-inward*16*seaScale);guard++)seaScale*=.5f;
                    float[] offsets={-16*seaScale,-5*seaScale,0,width*.4f*landScale,width*landScale};
                    for(int band=0;band<5;band++){
                        Vector3 q=p+inward*offsets[band];q.y=band==0?-1.1f:band==1?-.10f:band==2?.57f:HeightAt(q)+.035f;
                        v.Add(q);colors.Add(new Color(1,1,1,band==4?0:band==3?.7f:1));
                    }
                    if(CropEdge(p,next))continue;
                    for(int band=0;band<4;band++){int a=i*5+band,b=((i+1)%ring.Length)*5+band;t.AddRange(new[]{a,b+1,b,a,a+1,b+1});}
                    walked+=Vector3.Distance(p,next);
                    if(walked>650){walked=0;float seed=Hash(p.x*.031f+p.z*.07f);
                        if(seed<.34f&&!NearLandmark(p)){
                            for(int stone=0;stone<3;stone++){Vector3 q=p+inward*(2+stone*1.6f)+tangent*(stone-1)*3;q.y=.20f;ShoreRock(q,new Vector3(2.5f+seed*5,.5f+seed*2,2+stone*.6f),rock,seed+stone);outcropCount++;}
                            if(p.sqrMagnitude<nearest&&Land(p+inward*10)){nearest=p.sqrMagnitude;inspection=p+inward*8;beachHeading=GunneryTools.Bearing(inward);}
                        }
                    }
                }
                beachVertices+=v.Count;var mesh=Shipwright.Mesh("Continuous curved tidal beach",v,t);mesh.SetColors(colors);
                var go=new GameObject("Curved beach / shared corner vertices");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=beach;
            }
            // A few low stones around the man-made Rip Raps island are locally appropriate.
            Vector3 riprap=Point(36.985,-76.305);for(int i=0;i<18;i++){float a=i*Mathf.PI*2/18;ShoreRock(riprap+new Vector3(Mathf.Sin(a)*74,-.2f,Mathf.Cos(a)*74),new Vector3(3.5f,1.3f,3),rock,i);outcropCount++;}
            if(nearest<float.PositiveInfinity)landmarks.Add(("Curved beach / shoreline outcrops",inspection));
        }
        void ShoreRock(Vector3 center,Vector3 size,Material material,float seed)
        {
            const int sides=16,rings=9;var v=new List<Vector3>();var t=new List<int>();
            for(int j=0;j<=rings;j++){float b=j*Mathf.PI/rings;for(int i=0;i<=sides;i++){
                float a=i*Mathf.PI*2/sides,r=1+.10f*Mathf.Sin(a*3+b*4+seed*21)+.06f*Mathf.Cos(a*5-b*3+seed*9);
                v.Add(center+Vector3.Scale(new Vector3(Mathf.Sin(b)*Mathf.Cos(a)*r,Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a)*r),size));
                if(j<rings&&i<sides){int q=j*(sides+1)+i;t.AddRange(new[]{q,q+1,q+sides+1,q+1,q+sides+2,q+sides+1});}
            }}
            var mesh=Shipwright.Mesh("Low irregular shoreline stone",v,t);var normals=mesh.normals;for(int i=0;i<normals.Length;i++)if(normals[i].sqrMagnitude<.1f)normals[i]=Vector3.up;mesh.normals=normals;
            var go=new GameObject("Low coastal outcrop");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
}
