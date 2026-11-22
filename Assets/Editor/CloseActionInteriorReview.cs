using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionInteriorReview
    {
        static void Check(bool okay,string message){if(!okay)throw new Exception("Interior review: "+message);Debug.Log("INTERIOR PASS: "+message);}
        internal static bool FrontHit(Transform root,MeshFilter[] filters,Ray ray,float limit)
        {
            foreach(var filter in filters){
                var mesh=filter.sharedMesh;var v=mesh.vertices;var t=mesh.triangles;var matrix=root.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                for(int i=0;i<v.Length;i++)v[i]=matrix.MultiplyPoint3x4(v[i]);
                for(int i=0;i<t.Length;i+=3){
                    Vector3 a=v[t[i]],edge=v[t[i+1]]-a,other=v[t[i+2]]-a,p=Vector3.Cross(ray.direction,other);float det=Vector3.Dot(edge,p);
                    if(det<.000001f)continue;float inverse=1/det;Vector3 delta=ray.origin-a;float u=Vector3.Dot(delta,p)*inverse;if(u<0||u>1)continue;
                    Vector3 q=Vector3.Cross(delta,edge);float w=Vector3.Dot(ray.direction,q)*inverse;if(w<0||u+w>1)continue;
                    float distance=Vector3.Dot(other,q)*inverse;if(distance>=0&&distance<=limit)return true;
                }
            }
            return false;
        }
        public static void Run()
        {
            foreach(var profile in CloseActionPrototypes.All){
                var spec=CloseActionScenario.BaseFleet[profile.representative];var ship=Shipwright.CreateCloseActionShip(spec);var root=ship.transform;
                var inside=root.Find("Closed hull interior");Check(inside!=null,profile.key+" interior present");
                var floors=inside.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Solid gun deck")||f.name=="Weather-deck inner ceiling").ToArray();int decks=spec.rate==120?3:2;
                Check(floors.Length==decks+1,profile.key+" solid floors at every gun tier and weather-deck ceiling");
                for(int tier=0;tier<decks;tier++){
                    float y=1.55f+tier*2.05f;
                    Check(FrontHit(root,floors,new Ray(new Vector3(1.2f,y+.3f,0),Vector3.down),2),profile.key+" gun deck visible looking down from tier "+tier);
                    int ports=spec.rate==120?(tier==0?16:17):spec.rate==50?11:spec.rate==80?(tier==0?15:16):(tier==0?14:15);
                    float z=Mathf.Lerp(-spec.Length*.37f,spec.Length*.37f,(ports/2)/(float)(ports-1));Vector3 port=Shipwright.CloseActionHullPoint(spec,z,y,1);
                    Check(!FrontHit(root,floors,new Ray(port+Vector3.left*.4f+Vector3.up*.46f,Vector3.right),2),profile.key+" floor does not plug tier "+tier+" gunports");
                }
                Check(FrontHit(root,floors,new Ray(new Vector3(1.2f,1.55f+(decks-1)*2.05f+.4f,0),Vector3.up),4),profile.key+" ceiling visible from upper gun bay");
                var all=root.GetComponentsInChildren<MeshFilter>();
                foreach(float station in new[]{-.3f,0,.3f})Check(FrontHit(root,all,new Ray(new Vector3(.5f,-1,station*spec.Length),Vector3.down),12),profile.key+" inward-facing bottom closes hold / station "+station);
                var carvings=root.Find("Attached bow assembly/Ornate bow carvings");Check(carvings!=null&&carvings.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount)>1500,profile.key+" modeled bow carvings");
                Check(carvings.GetComponentsInChildren<MeshRenderer>().Any(r=>r.sharedMaterial.name.Contains("gold bow carving")),profile.key+" gilt carved details");
                UnityEngine.Object.DestroyImmediate(ship);
            }
            Directory.CreateDirectory("Artifacts/CloseAction/Interiors");File.WriteAllText("Artifacts/CloseAction/Interiors/verification.txt","PASS: all ten prototypes; front-face ray checks from each gun tier to its floor; upper ceiling; inward-facing hold bottom at three stations; gunports stay open; modeled bow carvings and gilt materials.\n");
        }
    }
}
