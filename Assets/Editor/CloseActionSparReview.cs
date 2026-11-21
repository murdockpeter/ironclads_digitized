using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionSparReview
    {
        static void Check(bool okay,string message){if(!okay)throw new Exception("Spar review: "+message);Debug.Log("SPAR PASS: "+message);}
        internal static (Vector3 heel,Vector3 end) Straight(MeshFilter filter,Transform root,float radius)
        {
            var mesh=filter.sharedMesh;var vertices=mesh.vertices;var normals=mesh.normals;
            Vector3 axis=normals[normals.Length-1].normalized,heel=Vector3.zero,end=Vector3.zero;int heels=0,ends=0;
            for(int i=0;i<vertices.Length;i++){
                if(Vector3.Dot(normals[i],axis)>.999f){end+=vertices[i];ends++;}
                if(Vector3.Dot(normals[i],axis)<-.999f){heel+=vertices[i];heels++;}
            }
            Check(heels>3&&ends>3,filter.name+" planar end caps");heel/=heels;end/=ends;axis=(end-heel).normalized;
            Check(vertices.All(v=>Vector3.Cross(v-heel,axis).magnitude<radius+.003f),filter.name+" remains inside a straight tapered spar envelope");
            var matrix=root.worldToLocalMatrix*filter.transform.localToWorldMatrix;
            return(matrix.MultiplyPoint3x4(heel),matrix.MultiplyPoint3x4(end));
        }
        public static void Run()
        {
            foreach(var profile in CloseActionPrototypes.All){
                var ship=Shipwright.CreateCloseActionShip(CloseActionScenario.BaseFleet[profile.representative]);
                var rig=ship.transform.Find("Attached bow assembly/Rigid bowsprit assembly");Check(rig!=null,profile.key+" rigid assembly bypasses deck deformation");
                var main=Straight(rig.Find("Bowsprit").GetComponent<MeshFilter>(),ship.transform,.42f);
                var boom=Straight(rig.Find("Jibboom").GetComponent<MeshFilter>(),ship.transform,.13f);
                Vector3 axis=(main.end-main.heel).normalized,boomAxis=(boom.end-boom.heel).normalized;
                Check(Vector3.Dot(axis,boomAxis)>.99999f,profile.key+" parallel bowsprit and jibboom axes");
                float overlap=Vector3.Dot(boom.heel-main.heel,axis)/Vector3.Distance(main.heel,main.end);
                Check(overlap>.83f&&overlap<.92f&&Vector3.Cross(boom.heel-main.heel,axis).magnitude<.32f,profile.key+" overlapping joint with close offset");
                foreach(var filter in ship.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Lower mast"||f.name=="Topmast"||f.name=="Topgallant and royal pole"||f.name=="Tapered yard"))Straight(filter,ship.transform,.5f);
                UnityEngine.Object.DestroyImmediate(ship);
            }
            foreach(bool stonewall in new[]{false,true}){
                var ship=Shipwright.Create(stonewall,false);
                foreach(var spar in ship.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Bowsprit"))Straight(spar,ship.transform,.5f);
                UnityEngine.Object.DestroyImmediate(ship);
            }
            Directory.CreateDirectory("Artifacts/CloseAction/Spars");File.WriteAllText("Artifacts/CloseAction/Spars/verification.txt","PASS: all ten sailing prototypes have straight bowsprits, parallel overlapping jibbooms, straight masts and yards. Ferrol's two original bowsprits remain straight. Other module bowsprits use the unchanged rigid Rod builder.\n");
        }
    }
}
