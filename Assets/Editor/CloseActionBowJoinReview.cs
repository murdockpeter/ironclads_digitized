using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionBowJoinReview
    {
        static void Check(bool okay,string message){if(!okay)throw new Exception("Bow join: "+message);Debug.Log("BOW JOIN PASS: "+message);}
        public static void Run()
        {
            foreach(var profile in CloseActionPrototypes.All){
                var spec=CloseActionScenario.BaseFleet[profile.representative];var ship=Shipwright.CreateCloseActionShip(spec);var root=ship.transform;
                var knee=root.Find("Attached bow assembly/Ornate bow carvings/Curved knee of head").GetComponent<MeshFilter>().sharedMesh;var v=knee.vertices;int tested=0;
                var shell=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.transform.parent==root&&(f.GetComponent<MeshRenderer>().sharedMaterial.name.Contains("Tarred timber")||f.GetComponent<MeshRenderer>().sharedMaterial.name.Contains("ochre gun strakes"))).ToArray();
                var lines=CloseActionHullLines.Get(spec.Length,spec.Beam);float maxY=lines.DeckHeight(spec.Length*.5f)*.52f;
                for(int n=8;n<v.Length/4-1;n+=2){
                    Vector3 inside=(v[n*4+2]+v[n*4+3])*.5f;if(inside.y<1.5f||inside.y>maxY)continue;
                    float plankY=inside.y/(1+lines.Sheer(spec.Length*.5f)/spec.Deck);Vector3 stem=Shipwright.CloseActionHullPoint(spec,spec.Length*.5f,plankY,1);
                    Check(stem.z>=inside.z+.15f,profile.key+" shell overlaps back of knee / section "+n);
                    Check(CloseActionInteriorReview.FrontHit(root,shell,new Ray(new Vector3(spec.Beam,inside.y,inside.z-.05f),Vector3.left),spec.Beam*2),profile.key+" opaque hull fills former open gap / section "+n);tested++;
                }
                Check(tested>=3,profile.key+" multiple bow-to-knee junction heights checked");
                float deckY=lines.DeckHeight(spec.Length*.5f);
                Vector3 tip=Shipwright.CloseActionHullPoint(spec,spec.Length*.5f,spec.Deck,1);
                Check(tip.z<spec.Length*.505f,profile.key+" upper bow tucks aft of rising knee");
                Check(!CloseActionInteriorReview.FrontHit(root,shell,new Ray(new Vector3(.35f,deckY+.5f,spec.Length*.59f),Vector3.back),spec.Length*.06f),profile.key+" open beakhead above closed lower bow");
                Vector3 shoulder=Shipwright.CloseActionHullPoint(spec,spec.Length*.45f,spec.Deck,1);
                Check(shoulder.x<lines.HalfBreadth(spec.Length*.45f,shoulder.y)*.93f,profile.key+" slimmer upper forward sections");
                // The outer cheek must remain behind the narrow centerline stem join.
                Vector3 cheek=Shipwright.CloseActionHullPoint(spec,spec.Length*.45f,spec.Deck*.5f,1);
                Check(cheek.z<spec.Length*.48f,profile.key+" rounded cheek stays aft of stem without projecting shelf");
                float farthest=0;
                foreach(var point in v)farthest=Mathf.Max(farthest,point.z);
                Check(farthest<spec.Length*.60f,profile.key+" raised head remains within a tenth of hull length beyond stem");
                float previousProjection=spec.Length*.085f;
                Check(Mathf.Abs((farthest-spec.Length*.5f)/previousProjection-.85f)<.001f,profile.key+" head projection reduced by 15 percent from Bows 3");
                var frames=root.Find("Attached bow assembly/Attached beakhead framing");
                Check(frames!=null,profile.key+" separate attached headrail assembly");
                var cheekShell=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.transform.parent==root).ToArray();
                for(int rail=0;rail<3;rail++)for(int side=-1;side<=1;side+=2){
                    Vector3 seat=Shipwright.CloseActionHeadRailPoint(spec,side,rail,0);
                    Check(seat.z>spec.Length*.38f,profile.key+" aft rail tail trimmed toward bow shoulder");
                    Check(CloseActionInteriorReview.FrontHit(root,cheekShell,new Ray(seat,Vector3.right*side),1.25f),profile.key+" shortened rail cap seats inside planking / rail "+rail+" / side "+side);
                }
                Check(shell.SelectMany(f=>f.sharedMesh.vertices).Max(p=>p.z)<spec.Length*.507f,profile.key+" hull ends at its stem, behind projecting framework");
                var gold=frames.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<MeshRenderer>().sharedMaterial.name.Contains("gold bow carving"));
                var points=gold.SelectMany(f=>f.sharedMesh.vertices.Select(p=>root.InverseTransformPoint(f.transform.TransformPoint(p)))).Where(p=>p.x>0&&p.z>spec.Length*.425f&&p.z<spec.Length*.49f&&p.y>spec.Deck+.1f).OrderBy(p=>p.z).ToArray();
                Check(points.Length>10,profile.key+" exterior head moldings available for checks");
                for(int sample=0;sample<6;sample++){
                    Vector3 p=points[(points.Length-1)*sample/5];Check(!CloseActionInteriorReview.FrontHit(root,shell,new Ray(p+Vector3.right*.004f,Vector3.right),spec.Beam),profile.key+" decorative molding remains outside hull / sample "+sample);
                }
                UnityEngine.Object.DestroyImmediate(ship);
            }
            Directory.CreateDirectory("Artifacts/CloseAction/BowJoins");File.WriteAllText("Artifacts/CloseAction/BowJoins/verification.txt","PASS: all ten prototypes; hull remains within original stem envelope; separate bent headrails and open supports; knee seats against stem at multiple heights; interiors and gunports checked separately.\n");
        }
    }
}
