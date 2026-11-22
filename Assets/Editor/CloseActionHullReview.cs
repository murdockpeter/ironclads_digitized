using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionHullReview
    {
        static void Check(bool okay,string description){if(!okay)throw new Exception("Hull contour review: "+description);Debug.Log("CA HULL PASS: "+description);}
        public static void Run()
        {
            const string folder="Artifacts/CloseAction/Hull";Directory.CreateDirectory(folder);
            var data=new StringBuilder("prototype,station_fraction,height_m,half_breadth_m,sheer_m,keel_depth_m\n");
            foreach(var profile in CloseActionPrototypes.All){
                var lines=CloseActionHullLines.Get(profile.length,profile.beam);float mid=lines.HalfBreadth(0,0),upper=lines.HalfBreadth(0,lines.DeckHeight(0));
                Check(upper<mid*.91f&&upper>mid*.70f,profile.key+" rounded sections and tumblehome");
                Check(lines.HalfBreadth(profile.length*.425f,0)<mid*.5f,profile.key+" fine bow waterline");
                Check(lines.HalfBreadth(-profile.length*.40f,0)<mid*.65f,profile.key+" fine after run");
                float bow=profile.length*.5f,waterRake=lines.BowRake(bow,0),keelRake=lines.BowRake(bow,-lines.KeelDepth(bow));
                Check(waterRake>profile.length*.02f&&keelRake>waterRake*2.5f&&lines.BowRake(bow,lines.DeckHeight(bow))<.001f,profile.key+" swept stem and rounded forefoot");
                Check(lines.HalfBreadth(profile.length*.4875f,lines.DeckHeight(bow))>lines.HalfBreadth(profile.length*.475f,lines.DeckHeight(bow))*.6f,profile.key+" rounded upper-bow entrance");
                for(int station=0;station<=40;station++){
                    float f=station/40f,z=(f-.5f)*profile.length;
                    foreach(float y in new[]{-lines.KeelDepth(z),-lines.KeelDepth(z)*.65f,0,profile.deck*.5f,lines.DeckHeight(z)}){
                        float width=lines.HalfBreadth(z,y);Check(float.IsFinite(width)&&width>=0&&width<=profile.beam*.52f,profile.key+" finite bounded section");
                        data.AppendLine(FormattableString.Invariant($"{profile.key},{f:F3},{y:F3},{width:F3},{lines.Sheer(z):F3},{lines.KeelDepth(z):F3}"));
                    }
                }
            }
            File.WriteAllText(folder+"/station-offsets.csv",data.ToString());
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var scene=new GameObject("Hull station and weather review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartCloseAction();scene.showOverlay=false;scene.scenario.waveHeight=0;scene.UpdateWorld();
            foreach(var profile in CloseActionPrototypes.All){
                int ship=profile.representative;var spec=CloseActionScenario.Spec(scene.scenario,ship);var root=scene.vessels[ship];var guns=root.GetComponentsInChildren<GunMount>(true);int[] ports=spec.rate==120?new[]{16,17,17}:spec.rate==80?new[]{15,16}:spec.rate==50?new[]{11,11}:new[]{14,15};int index=0;
                for(int level=0;level<ports.Length;level++)for(int side=-1;side<=1;side+=2)for(int j=0;j<ports[level];j++){
                    float z=Mathf.Lerp(-spec.Length*.37f,spec.Length*.37f,j/(float)(ports[level]-1)),y=1.55f+level*2.05f;
                    Vector3 wall=Shipwright.CloseActionHullPoint(spec,z,y,side);var gun=guns[index++];Vector3 axis=root.InverseTransformDirection(gun.Direction);
                    Check(Vector3.Distance(gun.transform.localPosition,wall-axis*.85f)<.005f,profile.key+" cannon seated inside its curved gunport");
                    Vector3 muzzle=root.InverseTransformPoint(gun.Muzzle);Check(Vector3.Dot(muzzle-wall,axis)>.35f,profile.key+" muzzle clears planking");
                }
            }
            var state=scene.scenario.closeAction;var table=scene.closeAction;
            Check(table.SetWindNow(2,2)&&state.windDirection==2&&Mathf.Abs(scene.scenario.windKnots-7)<.01f,"current wind controls apply to scene and rules");
            state.phase=1;Check(table.Submit(new CloseActionOrder{ship=0,plot="0"}),"plot before wind change");Check(table.SetWindNow(3,3)&&state.submitted.Length==0&&table.drafts.Count==0,"wind change requires sealed plots to be resubmitted");
            state.phase=2;Check(!table.SetWindNow(1,1)&&state.windDirection==3&&state.wind==3,"revealed plots retain current wind");
            scene.scenario.cloud=.8f;scene.scenario.rain=.4f;scene.scenario.visibilityKm=6;scene.scenario.waveHeight=1.2f;state.pendingDirection=6;state.pendingWind=4;
            var saved=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(saved.closeAction.pendingDirection==6&&saved.closeAction.pendingWind==4&&Mathf.Abs(saved.rain-.4f)<.001f&&Mathf.Abs(saved.waveHeight-1.2f)<.001f,"environment and queued wind survive save serialization");
            File.WriteAllText(folder+"/verification.txt","PASS: all ten station profiles; tumblehome, bow entry and after run; bounded finite offsets; every battery gun aligned to its contoured opening and muzzle clear; immediate/queued weather and saved environment.\n");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
    }
}
