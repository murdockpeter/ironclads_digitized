using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class GeometryReview
    {
        static string Output="Artifacts/Pass2",Baseline="Artifacts/Pass1";
        [MenuItem("Ironclads/Review current model geometry (pass 3)")]
        public static void RunPass3(){Output="Artifacts/Pass3";Baseline="Artifacts/Pass2/Baseline";PassThreeReview.CheckCounts();Run();}
        public static void RunBowRevision(){Output="Artifacts/BowRevision";Baseline="Artifacts/Pass3/Baseline";Run();}
        public static void RunPass4(){Output="Artifacts/Pass4";Baseline="Artifacts/Pass4/Baseline";PassFourReview.CheckCounts();Run();}
        static void Require(bool ok,string message){if(!ok)throw new Exception("Geometry review: "+message);}
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            var report=new StringBuilder((Output.EndsWith("Pass4")?"MODEL PASS 4":Output.EndsWith("BowRevision")?"INTEGRATED FORECASTLE REVISION":Output.EndsWith("Pass3")?"MODEL PASS 3":"MODEL PASS 2")+" — GEOMETRY REVIEW\n\n");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            for(int i=0;i<2;i++) {
                bool stonewall=i==1;string name=stonewall?"CSS-Stonewall":"USS-Niagara";
                var ship=Shipwright.Create(stonewall,false);
                var filters=ship.GetComponentsInChildren<MeshFilter>();
                foreach(var f in filters)foreach(var p in f.sharedMesh.vertices)
                    Require(float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z),name+" invalid vertex in "+f.name);
                int triangles=filters.Sum(f=>f.sharedMesh.triangles.Length/3),vertices=filters.Sum(f=>f.sharedMesh.vertexCount);
                int oldTriangles=File.ReadLines(Baseline+"/"+name+".obj").Count(line=>line.StartsWith("f "));
                float ratio=triangles/(float)oldTriangles;
                report.AppendLine($"{name}: {oldTriangles:N0} → {triangles:N0} triangles ({ratio:0.00}×); {vertices:N0} vertices.");
                CheckHull(ship,stonewall,report);
                if(!stonewall)CheckRoundedStern(report);
                if(stonewall){CheckBatterySupport(report);CheckIntegratedForecastle(ship,report);}
                var guns=ship.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("pounder")).ToArray();
                Require(guns.Length==(stonewall?3:10),name+" battery count changed.");
                foreach(var gun in guns)CheckBore(gun);
                report.AppendLine($"PASS: all vertices finite; {guns.Length} gun bores physically recessed; battery count preserved.\n");
                UnityEngine.Object.DestroyImmediate(ship);
            }
            File.WriteAllText(Output+"/geometry-review.txt",report.ToString());
            RenderViews();
            Debug.Log(report.ToString());
        }
        static void CheckBatterySupport(StringBuilder report)
        {
            float margin=float.MaxValue;
            foreach(bool forward in new[]{false})for(int i=0;i<720;i++) {
                Vector3 p=Shipwright.StonewallBatteryPoint(forward,i*Mathf.PI*2/720,.25f);
                float m=Shipwright.StonewallHalfWidth(p.z)-Mathf.Abs(p.x);margin=Mathf.Min(margin,m);
                Require(m>=-.001f,"Stonewall battery base overhangs hull at "+p);
                Require(p.y<Shipwright.StonewallDeckY(p.z),"Stonewall battery root not embedded in hull.");
            }
            report.AppendLine($"PASS: 720 aft battery-base samples have hull support (minimum lateral margin {margin:0.000} m).");
        }
        static void CheckRoundedStern(StringBuilder report)
        {
            Require(Shipwright.Width(0)<.001f,"Niagara stern still has a broad flat transom.");
            float previous=0;
            for(int i=0;i<=200;i++) {
                float width=Shipwright.Width(i*.001f);Require(width>=previous-.00001f,"Niagara stern outline reverses curvature/width.");previous=width;
            }
            Require(Shipwright.Width(.02f)>.44f,"Niagara round stern tapers into an excessively fine second bow.");
            float left=(Shipwright.Width(.20f)-Shipwright.Width(.1999f))/.0001f,right=(Shipwright.Width(.2001f)-Shipwright.Width(.20f))/.0001f;
            Require(Mathf.Abs(left-right)<.03f,"Niagara quarter join has a tangent discontinuity.");
            for(int i=-10;i<=10;i++) {
                var p=Shipwright.NiagaraSternSurface(i*.25f,3.2f);Require(Mathf.Abs(Shipwright.NiagaraSurfaceX(p.z+.025f,p.y)-Mathf.Abs(p.x))<.036f,"Stern fitting detached from round hull.");
            }
            report.AppendLine("PASS: rounded stern outline, monotonic quarters, matched join tangent and curved gallery fitting positions. Shape remains an approximate reconstruction.");
        }
        static void CheckIntegratedForecastle(GameObject ship,StringBuilder report)
        {
            Require(!ship.GetComponentsInChildren<Transform>().Any(t=>t.name=="Forward integrated armored battery"),"Separate forward battery drum still present.");
            Require(ship.GetComponentsInChildren<Transform>().Count(t=>t.name=="Flat armored battery roof")==1,"Forward circular roof still present.");
            var hull=ship.GetComponentsInChildren<MeshFilter>().First(f=>f.name=="Carvel hull");
            Require(hull.sharedMesh.vertices.Count(p=>p.z>=12&&p.z<=21&&p.y>=3.5f)>30,"Raised forecastle not present in the hull mesh.");
            for(float z=12;z<=21;z+=.25f) {
                Require(Mathf.Abs(Shipwright.StonewallDeckY(z)-4.15f)<.001f,"Forecastle deck is not continuous.");
                Require(Shipwright.StonewallSurfaceX(z,4.15f)<=Shipwright.StonewallHalfWidth(z),"Forecastle roof overhangs hull.");
            }
            report.AppendLine("PASS: raised forecastle is in the hull mesh; no separate forward drum/roof; shared roof-to-hull edges; three intentional embrasures.");
        }
        static void CheckHull(GameObject ship,bool stonewall,StringBuilder report)
        {
            var ids=new Dictionary<Vector3Int,int>();var points=new List<Vector3>();var edges=new Dictionary<(int,int),int>();
            int Id(Vector3 p){var k=new Vector3Int(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));if(!ids.TryGetValue(k,out int n)){n=points.Count;ids[k]=n;points.Add(p);}return n;}
            void Edge(int a,int b){if(a==b)return;var key=a<b?(a,b):(b,a);edges.TryGetValue(key,out int count);edges[key]=count+1;}
            foreach(var f in ship.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Carvel hull"||f.name=="Planked weather deck")) {
                var v=f.sharedMesh.vertices;var t=f.sharedMesh.triangles;
                for(int j=0;j<t.Length;j+=3){int a=Id(v[t[j]]),b=Id(v[t[j+1]]),c=Id(v[t[j+2]]);if(a==b||b==c||a==c)continue;Edge(a,b);Edge(b,c);Edge(c,a);}
            }
            int intentional=0;
            foreach(var e in edges) {
                Require(e.Value<=2,ship.name+" has a nonmanifold hull edge.");if(e.Value==2)continue;
                Vector3 p=(points[e.Key.Item1]+points[e.Key.Item2])*.5f;
                float drop=Shipwright.DeckHeight(p.z,100.228f,4.3f)-p.y;
                bool niagaraPort=!stonewall&&Mathf.Abs(p.x)>4&&drop>=.349f&&drop<=1.651f&&new[]{-26f,-13,0,13,26}.Any(z=>Mathf.Abs(p.z-z)<=.901f);
                bool sidePort=stonewall&&Mathf.Abs(p.x)>3.8f&&p.z>=16.199f&&p.z<=17.801f&&p.y>=2.349f&&p.y<=3.551f;
                bool bowPort=stonewall&&Mathf.Abs(p.x)<=.851f&&p.z>=22.249f&&p.z<=23.351f&&Mathf.Abs(p.y-Shipwright.StonewallDeckY(p.z))<.03f;
                bool expected=niagaraPort||sidePort||bowPort;
                Require(expected,ship.name+" unexpected open hull edge at "+p.ToString("F4"));intentional++;
            }
            var hull=ship.GetComponentsInChildren<MeshFilter>().First(f=>f.name=="Carvel hull").sharedMesh;
            var ns=hull.normals;
            Require(ns[ns.Length-2].z<-.99f&&ns[ns.Length-1].z>.99f,ship.name+" hull end closures face inward.");
            report.AppendLine($"PASS: closed bow/stern/deck seams; no unexpected hull boundary edges ({intentional} intentionally open gun-port edges).");
        }
        static void CheckBore(Transform gun)
        {
            var f=gun.GetComponentsInChildren<MeshFilter>().First(m=>m.name=="Profiled barrel with open bore");
            var collider=f.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=f.sharedMesh;
            float length=gun.name.StartsWith("70-")?Shipwright.BarrelLength(Shipwright.GunType.Armstrong70):4.05f;
            Physics.SyncTransforms();
            Ray r=new Ray(f.transform.TransformPoint(new Vector3(0,0,length*.45f+.2f)),f.transform.TransformDirection(Vector3.back));
            Require(collider.Raycast(r,out var hit,3),gun.name+" bore has no closed internal bottom.");
            Require(hit.distance>.7f&&hit.distance<1.8f,gun.name+" muzzle incorrectly capped or bore too shallow: "+hit.distance);
            UnityEngine.Object.DestroyImmediate(collider);
        }
        static void Image(Camera camera,string path)
        {
            var rt=new RenderTexture(1800,1200,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var tex=new Texture2D(1800,1200,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1800,1200),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
        static void RenderViews()
        {
            var controller=new GameObject("Geometry review scene").AddComponent<MaritimeScene>();controller.Initialize();controller.showOverlay=false;
            controller.scenario.waveHeight=0;controller.scenario.cloud=.1f;
            controller.scenario.ships[0].east=-150;controller.scenario.ships[0].north=0;controller.scenario.ships[0].heading=0;
            controller.scenario.ships[1].east=150;controller.scenario.ships[1].north=0;controller.scenario.ships[1].heading=0;
            controller.UpdateWorld();var chart=controller.GetComponent<RuleChart>();chart.Build();chart.Refresh();
            foreach(var ring in controller.seaRoot.GetComponentsInChildren<LineRenderer>(true).Where(l=>l.name=="Occupied hex")) {
                Require(ring.positionCount==7&&Vector3.Distance(ring.GetPosition(0),ring.GetPosition(6))<.0001f,"Occupied hex outline contains a stray diagonal.");
            }
            var arrow=controller.seaRoot.GetComponentsInChildren<LineRenderer>(true).First(l=>l.name=="Bow direction arrow");
            Vector3 chartCenter=RuleChart.Center(RuleChart.At(controller.scenario.ships[0].Position))+Vector3.up*.2f;
            for(int heading=0;heading<360;heading+=60){controller.scenario.ships[0].heading=heading;chart.Refresh();Require(Vector3.Distance(arrow.GetPosition(0),chartCenter)<.001f,"Bow arrow does not start at occupied hex center.");Require(Vector3.Dot((arrow.GetPosition(1)-chartCenter).normalized,Quaternion.Euler(0,heading,0)*Vector3.forward)>.9999f,"Bow arrow facing incorrect.");}
            controller.scenario.ships[0].heading=0;controller.UpdateWorld();
            // A fill light is used only for inspection images so dark recesses can be judged.
            var fill=new GameObject("Inspection fill").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.6f;fill.transform.rotation=Quaternion.Euler(35,160,0);
            for(int i=0;i<2;i++) {
                controller.vessels[i].gameObject.SetActive(true);controller.vessels[1-i].gameObject.SetActive(false);
                string name=i==0?"Niagara":"Stonewall";Vector3 origin=controller.scenario.ships[i].Position;float length=i==0?100.228f:52.3756f,deck=i==0?4.3f:1.85f;
                var views=new[]{("Bow-port",new Vector3(-15,deck+8,length*.5f+19),new Vector3(0,deck-1,length*.47f)),
                    ("Bow-starboard",new Vector3(15,deck+5,length*.5f+17),new Vector3(0,deck-1,length*.47f)),
                    ("Stern",new Vector3(-14,deck+5,-length*.5f-18),new Vector3(0,deck-1,-length*.47f)),
                    ("Battery",new Vector3(i==0?22:14,deck+3,i==0?3:-16),new Vector3(i==0?7:3,deck-.2f,i==0?0:-16)),
                    ("Full-profile",new Vector3(length*1.05f,deck+5,0),new Vector3(0,deck+12,0)),
                    ("Overhead",new Vector3(0,length*1.4f,0),new Vector3(0,deck,0))};
                foreach(var v in views){
                    controller.view.orthographic=v.Item1=="Overhead";
                    controller.view.transform.position=origin+v.Item2;
                    if(controller.view.orthographic){controller.view.orthographicSize=length*.58f;controller.view.transform.rotation=Quaternion.Euler(90,0,0);}
                    else controller.view.transform.LookAt(origin+v.Item3);
                    Image(controller.view,Output+"/"+name+"-"+v.Item1+".png");
                }
            }
            // The three guns can also be examined outside the hull without changing their scale.
            controller.seaRoot.gameObject.SetActive(false);controller.globeRoot.gameObject.SetActive(false);RenderSettings.fog=false;
            controller.view.clearFlags=CameraClearFlags.SolidColor;controller.view.backgroundColor=new Color(.13f,.17f,.20f);
            controller.view.orthographic=false;
            foreach(Shipwright.GunType type in Enum.GetValues(typeof(Shipwright.GunType))) {
                var gun=Shipwright.Gun(null,Vector3.zero,Vector3.forward,type);
                controller.view.transform.position=new Vector3(3.5f,2.2f,4.5f);controller.view.transform.LookAt(new Vector3(0,-.1f,-.15f));
                Image(controller.view,Output+"/Gun-"+type+".png");UnityEngine.Object.DestroyImmediate(gun);
            }
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
    }
}
