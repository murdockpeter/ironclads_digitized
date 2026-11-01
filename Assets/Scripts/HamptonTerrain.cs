using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    // Hand-built geographic scenery, not bathymetry or an exact 1862 shoreline survey.
    public class HamptonTerrain : MonoBehaviour
    {
        public const double Latitude=36.983,Longitude=-76.385;
        public Transform root;
        public readonly List<(string,Vector3)> landmarks=new List<(string,Vector3)>();
        Material earth,grass,sand,stone,roof,trunk,leaves;
        public static Vector3 Point(double lat,double lon,float y=0)=>new Vector3((float)((lon-Longitude)*Math.PI/180*Navigation.EarthRadius*Math.Cos(Latitude*Math.PI/180)),y,(float)((lat-Latitude)*Math.PI/180*Navigation.EarthRadius));
        public void Build(Transform parent)
        {
            root=new GameObject("Hampton Roads — reconstructed 1862 shores").transform;root.SetParent(parent,false);
            earth=Shipwright.MakeMaterial("Coastal soil",new Color(.22f,.20f,.13f));grass=Shipwright.MakeMaterial("Salt marsh and grass",new Color(.27f,.30f,.17f));sand=Shipwright.MakeMaterial("Tidal beach",new Color(.54f,.48f,.32f));
            grass.color=Color.white;var ground=new Texture2D(128,128,TextureFormat.RGB24,true);ground.name="Procedural marsh and meadow";ground.wrapMode=TextureWrapMode.Repeat;ground.filterMode=FilterMode.Trilinear;
            for(int y=0;y<128;y++)for(int x=0;x<128;x++){
                float n=Mathf.PerlinNoise(x*.055f,y*.055f)*.7f+Mathf.PerlinNoise(x*.23f+15,y*.23f)*.3f;
                ground.SetPixel(x,y,Color.Lerp(new Color(.22f,.26f,.13f),new Color(.44f,.43f,.25f),n));
            }ground.Apply();grass.mainTexture=ground;
            stone=Shipwright.MakeMaterial("Fort masonry",new Color(.40f,.38f,.31f));roof=Shipwright.MakeMaterial("Period roof",new Color(.23f,.19f,.16f));trunk=Shipwright.Wood;leaves=Shipwright.MakeMaterial("Distant coastal woodland",new Color(.13f,.21f,.13f));
            Shore("Hampton and Newport News",new[]{Point(37.060,-76.52),Point(37.060,-76.25),Point(37.020,-76.25),Point(37.010,-76.292),Point(37.000,-76.300),Point(36.997,-76.307),Point(37.000,-76.315),Point(37.015,-76.334),Point(37.020,-76.349),Point(37.014,-76.365),Point(37.006,-76.383),Point(36.998,-76.401),Point(36.987,-76.411),Point(36.975,-76.420),Point(36.960,-76.427),Point(36.953,-76.434),Point(36.957,-76.455),Point(36.984,-76.477),Point(37.015,-76.49)});
            Shore("Sewells Point and Norfolk shore",new[]{Point(36.900,-76.35),Point(36.900,-76.20),Point(36.960,-76.20),Point(36.967,-76.265),Point(36.970,-76.295),Point(36.968,-76.313),Point(36.961,-76.329),Point(36.953,-76.336),Point(36.943,-76.335),Point(36.934,-76.337),Point(36.925,-76.347)});
            Shore("Craney Island",new[]{Point(36.884,-76.395),Point(36.884,-76.358),Point(36.905,-76.358),Point(36.912,-76.365),Point(36.916,-76.375),Point(36.911,-76.385)});
            FortMonroe(Point(37.003,-76.308));
            RipRaps(Point(36.985,-76.305));
            Battery("Sewell's Point batteries",Point(36.960,-76.329),-25);
            Battery("Newport News batteries",Point(36.963,-76.425),100);
            landmarks.Add(("Hampton Roads duel",Vector3.zero));
            // Sparse low wharves and warehouses: no modern container terminal, reclamation or bridge.
            Waterfront(Point(36.970,-76.423),85);Waterfront(Point(37.013,-76.343),170);
            Shipwright.CombineScenery(root);
        }
        void Shore(string name,Vector3[] coast)
        {
            // Ear clipping supports the concave inlets rather than filling them with a fan.
            var vertices=new List<Vector3>(coast);for(int i=0;i<vertices.Count;i++)vertices[i]+=Vector3.up*2.4f;
            var indices=new List<int>();for(int i=0;i<vertices.Count;i++)indices.Add(i);
            float area=0;for(int i=0;i<coast.Length;i++){var a=coast[i];var b=coast[(i+1)%coast.Length];area+=a.x*b.z-b.x*a.z;}
            if(area<0)indices.Reverse();var triangles=new List<int>();int guard=0;
            while(indices.Count>2&&guard++<1000){bool cut=false;
                for(int j=0;j<indices.Count;j++){
                    int a=indices[(j+indices.Count-1)%indices.Count],b=indices[j],c=indices[(j+1)%indices.Count];
                    if(Cross(coast[a],coast[b],coast[c])<=0)continue;bool occupied=false;
                    foreach(int p in indices)if(p!=a&&p!=b&&p!=c&&Cross(coast[a],coast[b],coast[p])>=0&&Cross(coast[b],coast[c],coast[p])>=0&&Cross(coast[c],coast[a],coast[p])>=0){occupied=true;break;}
                    if(occupied)continue;triangles.AddRange(new[]{a,c,b});indices.RemoveAt(j);cut=true;break;
                }if(!cut)throw new InvalidOperationException("Invalid coastline polygon: "+name);
            }
            Surface(root,name,vertices,triangles,grass);
            for(int i=0;i<coast.Length;i++){
                Vector3 a=coast[i],b=coast[(i+1)%coast.Length],inward=Vector3.Cross((b-a).normalized,Vector3.up)*(area>0?1:-1);
                Surface(root,"Sloping beach",new List<Vector3>{a-inward*12-Vector3.up*.4f,b-inward*12-Vector3.up*.4f,b+Vector3.up*2.4f,a+Vector3.up*2.4f},new List<int>{0,1,2,0,2,3,2,1,0,3,2,0},sand);
                // Restrained woodland clusters inland; deterministic across network peers.
                int count=Mathf.Min(18,Mathf.FloorToInt(Vector3.Distance(a,b)/130));
                for(int j=1;j<count;j++){Vector3 p=Vector3.Lerp(a,b,j/(float)count)+inward*(55+25*(j%4));for(int k=0;k<4;k++)Tree(p+new Vector3(Mathf.Sin(j*17+k*7)*18,2.4f,Mathf.Cos(j*13+k*5)*18),6+(j+k)%5);}
            }
        }
        static float Cross(Vector3 a,Vector3 b,Vector3 c)=>(b.x-a.x)*(c.z-a.z)-(b.z-a.z)*(c.x-a.x);
        static void Surface(Transform parent,string name,List<Vector3> v,List<int> t,Material material)
        {
            // Keep each face once, facing skyward. Reversed duplicates sharing vertices
            // cancel their averaged normals and turn the beaches/moat black.
            var unique=new HashSet<(int,int,int)>();var faces=new List<int>();
            for(int i=0;i<t.Count;i+=3){int a=t[i],b=t[i+1],c=t[i+2];int min=Mathf.Min(a,Mathf.Min(b,c)),max=Mathf.Max(a,Mathf.Max(b,c));
                if(!unique.Add((min,a+b+c-min-max,max)))continue;
                if(Vector3.Cross(v[b]-v[a],v[c]-v[a]).y<0){int swap=b;b=c;c=swap;}faces.AddRange(new[]{a,b,c});
            }
            var go=new GameObject(name);go.transform.SetParent(parent,false);var uv=new List<Vector2>();foreach(var p in v)uv.Add(new Vector2(p.x/140,p.z/140));
            go.AddComponent<MeshFilter>().sharedMesh=Shipwright.Mesh(name,v,faces,uv);go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        void Tree(Vector3 p,float h){Shipwright.Rod(root,"Woodland trunk",p,p+Vector3.up*h,.3f,trunk,sides:4);for(int i=0;i<2;i++){var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.name="Coastal woodland crown";crown.transform.SetParent(root,false);crown.transform.localPosition=p+new Vector3((i-.5f)*h*.25f,h*.75f+i,0);crown.transform.localScale=new Vector3(h*.7f,h*.8f,h*.65f);crown.GetComponent<Renderer>().sharedMaterial=leaves;DestroyImmediate(crown.GetComponent<Collider>());}}
        Transform Group(string name,Vector3 p,float heading=0){var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localRotation=Quaternion.Euler(0,heading,0);landmarks.Add((name,p));return go.transform;}
        void FortMonroe(Vector3 p)
        {
            var fort=Group("Fort Monroe",p,20);
            // Six fronts with projecting bastion points, moat and earth-covered ramparts.
            var ring=new List<Vector3>();for(int i=0;i<6;i++){float a=i*Mathf.PI/3;ring.Add(new Vector3(Mathf.Sin(a)*235,0,Mathf.Cos(a)*235));ring.Add(new Vector3(Mathf.Sin(a+.19f)*185,0,Mathf.Cos(a+.19f)*185));ring.Add(new Vector3(Mathf.Sin(a+.86f)*185,0,Mathf.Cos(a+.86f)*185));}
            for(int i=0;i<ring.Count;i++){
                Vector3 a=ring[i],b=ring[(i+1)%ring.Count],mid=(a+b)*.5f;float length=Vector3.Distance(a,b);
                var wall=Shipwright.Box(fort,"Masonry rampart",mid+Vector3.up*5,new Vector3(length,9,12),stone);wall.transform.localRotation=Quaternion.LookRotation(Vector3.Cross(b-a,Vector3.up));
                var cap=Shipwright.Box(fort,"Earth parapet",mid+Vector3.up*9.8f,new Vector3(length,2.1f,17),grass);cap.transform.localRotation=wall.transform.localRotation;
                for(int j=1;j<5;j++){Vector3 gun=Vector3.Lerp(a,b,j/5f)+Vector3.up*10.8f;Vector3 outwards=mid.normalized;Shipwright.Rod(fort,"Fort rampart cannon",gun,gun+outwards*3,.24f,Shipwright.Iron,sides:8);}
                Vector3 outA=a.normalized*25,outB=b.normalized*25;
                Surface(fort,"Moat water",new List<Vector3>{a+outA+Vector3.up*2.46f,b+outB+Vector3.up*2.46f,b+outB*2+Vector3.up*2.46f,a+outA*2+Vector3.up*2.46f},new List<int>{0,2,1,0,3,2,0,1,2,0,2,3},Shipwright.MakeMaterial("Moat",new Color(.14f,.23f,.23f),0,.7f));
            }
            Shipwright.Box(fort,"Fort parade ground",new Vector3(0,2,0),new Vector3(290,1,290),grass);
            for(int i=-1;i<=1;i++)Building(fort,new Vector3(i*65,2,-60),new Vector3(40,12,24));
        }
        void RipRaps(Vector3 p){var f=Group("Rip Raps / Fort Calhoun (later Fort Wool)",p,12);Shipwright.Rod(f,"Riprap island",new Vector3(0,-2,0),new Vector3(0,2,0),75,stone,sides:32);
            for(int i=0;i<14;i++){float a=i*2*Mathf.PI/14;Vector3 pos=new Vector3(Mathf.Sin(a)*50,4,Mathf.Cos(a)*50);var wall=Shipwright.Box(f,"Unfinished fort wall",pos,new Vector3(23,4.5f,8),stone);wall.transform.localRotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);if(i%2==0)Shipwright.Rod(f,"Island battery gun",pos+Vector3.up*2.8f,pos+Vector3.up*2.8f+pos.normalized*4,.26f,Shipwright.Iron,sides:8);}}
        void Battery(string name,Vector3 p,float heading){var f=Group(name,p,heading);Shipwright.Box(f,"Earthwork",new Vector3(0,3,0),new Vector3(110,6,15),earth);
            for(int i=-3;i<=3;i++){Vector3 gun=new Vector3(i*14,5,0);Shipwright.Box(f,"Gun platform",gun-Vector3.up,new Vector3(5,.5f,6),Shipwright.Wood);Shipwright.Rod(f,"Shore cannon",gun,gun+Vector3.forward*4,.25f,Shipwright.Iron,sides:12);}}
        void Building(Transform f,Vector3 p,Vector3 size){Shipwright.Box(f,"Period masonry building",p+Vector3.up*size.y*.5f,size,stone);var r=Shipwright.Box(f,"Pitched roof",p+Vector3.up*size.y,new Vector3(size.x+2,1,size.z+2),roof);}
        void Waterfront(Vector3 p,float heading){var f=Group("Period waterfront",p,heading);for(int i=-2;i<=2;i++){Building(f,new Vector3(i*45,2,-35),new Vector3(25,8,16));Shipwright.Box(f,"Timber wharf",new Vector3(i*45,2,25),new Vector3(8,1.2f,70),Shipwright.Wood);}}
        public void Position(Scenario s){if(root==null)return;root.gameObject.SetActive(s.Hampton);if(!s.Hampton)return;root.localPosition=new Vector3((float)((Longitude-s.longitude)*Math.PI/180*Navigation.EarthRadius*Math.Cos(s.latitude*Math.PI/180)),0,(float)((Latitude-s.latitude)*Math.PI/180*Navigation.EarthRadius));}
    }
}
