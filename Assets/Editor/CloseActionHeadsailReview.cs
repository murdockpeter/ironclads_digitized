using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionHeadsailReview
    {
        static void Check(bool okay,string message){if(!okay)throw new Exception("Headsail review: "+message);Debug.Log("HEADSAIL PASS: "+message);}
        public static void Run()
        {
            var scenario=CloseActionScenario.Create();
            foreach(var prototype in CloseActionPrototypes.All){
                var spec=CloseActionScenario.BaseFleet[prototype.representative];var ship=Shipwright.CreateCloseActionShip(spec);var fore=ship.transform.Find("Foremast and rigging");
                var sails=fore.GetComponentsInChildren<CloseActionCanvas>(true).Where(c=>!c.square).ToArray();
                Check(sails.Length==3,prototype.key+" three distinct headsails");
                float previousTack=float.NegativeInfinity;
                foreach(string name in new[]{"Fore staysail","Fore-topmast staysail","Jib"}){
                    var sail=sails.Single(s=>s.name==name);var mesh=sail.GetComponent<MeshFilter>().sharedMesh;var v=mesh.vertices;var uv=mesh.uv;var triangles=mesh.triangles;float area=0;
                    for(int t=0;t<triangles.Length;t+=3)area+=Vector3.Cross(v[triangles[t+1]]-v[triangles[t]],v[triangles[t+2]]-v[triangles[t]]).magnitude*.5f;
                    Check(area>spec.Length*spec.Length*.005f&&area<spec.Length*spec.Length*.055f,prototype.key+" / "+name+" bounded individual sail area");
                    var stay=CloseActionSparReview.Straight(fore.Find(name+" stay").GetComponent<MeshFilter>(),fore,.026f);Vector3 axis=(stay.end-stay.heel).normalized;
                    Check(Enumerable.Range(0,v.Length).Where(i=>uv[i].y<.0001f).All(i=>Vector3.Cross(v[i]-stay.heel,axis).magnitude<.003f),name+" luff follows its own stay");
                    float tack=v[v.Length-1].z;Check(tack>previousTack+spec.Length*.1f,name+" tack separated from the inner sail");previousTack=tack;
                }
                var log=scenario.closeAction.logs[prototype.representative];var visual=ship.GetComponent<CloseActionVisual>();
                foreach(int setting in new[]{0,1,2,3}){
                    log.sail=setting;visual.Refresh(log,0,300,12);
                    Check(sails.Count(s=>s.gameObject.activeInHierarchy)==(setting==0?0:setting==1?1:3),prototype.key+" headsail visibility / sail setting "+setting);
                }
                UnityEngine.Object.DestroyImmediate(ship);
            }
            Directory.CreateDirectory("Artifacts/CloseAction/Headsails");File.WriteAllText("Artifacts/CloseAction/Headsails/verification.txt","PASS: all ten prototypes; three separate, size-bounded headsails; each luff on its own straight stay; separated tacks; furled / plain / medium / full sail visibility.\n");
        }
    }
}
