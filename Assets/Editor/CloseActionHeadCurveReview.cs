using System;
using System.IO;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionHeadCurveReview
    {
        static void Check(bool okay,string message){if(!okay)throw new Exception("Head curve: "+message);Debug.Log("HEAD CURVE PASS: "+message);}
        public static void Run()
        {
            foreach(var profile in CloseActionPrototypes.All){
                var spec=CloseActionScenario.BaseFleet[profile.representative];var ship=Shipwright.CreateCloseActionShip(spec);var root=ship.transform;var carvings=root.Find("Attached bow assembly/Ornate bow carvings");
                var knee=carvings.Find("Curved knee of head");Check(knee!=null,profile.key+" solid curved stem extension");var v=knee.GetComponent<MeshFilter>().sharedMesh.vertices;int steps=v.Length/4-1;
                Vector3 Heel(int n)=>(v[n*4]+v[n*4+1])*.5f;
                Vector3 heel=Heel(0),crown=Heel(steps),lower=Heel(4)-heel,upper=crown-Heel(steps-4);
                Check(crown.y-heel.y>spec.Deck*.85f,profile.key+" stem rises from near waterline to figurehead");
                Check(lower.y/lower.z<1&&upper.y/upper.z>4,profile.key+" near-horizontal heel sweeps into steep crown");
                for(int n=1;n<=steps;n++)Check(Heel(n).z>=Heel(n-1).z&&Heel(n).y>Heel(n-1).y,profile.key+" continuous forward and upward sweep");
                var main=CloseActionSparReview.Straight(root.Find("Attached bow assembly/Rigid bowsprit assembly/Bowsprit").GetComponent<MeshFilter>(),root,.42f);Vector3 span=main.end-main.heel;
                float clearance=float.PositiveInfinity;
                foreach(var filter in carvings.GetComponentsInChildren<MeshFilter>())foreach(var p in filter.sharedMesh.vertices){
                    var local=root.InverseTransformPoint(filter.transform.TransformPoint(p));if(local.z<spec.Length*.55f||Mathf.Abs(local.x)>1)continue;
                    float sparY=main.heel.y+span.y*(local.z-main.heel.z)/span.z;clearance=Mathf.Min(clearance,sparY-local.y);
                }
                Check(float.IsFinite(clearance)&&clearance>.28f,profile.key+" raised carving clears straight bowsprit");
                UnityEngine.Object.DestroyImmediate(ship);
            }
            Directory.CreateDirectory("Artifacts/CloseAction/HeadCurves");File.WriteAllText("Artifacts/CloseAction/HeadCurves/verification.txt","PASS: all ten prototypes; solid curved knee rises from waterline; increasing forward/upward sweep; steep crown supports the figurehead; carving clears the straight bowsprit.\n");
        }
    }
}
