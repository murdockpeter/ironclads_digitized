using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Ironclads
{
    public class PantelleriaVerification : MonoBehaviour
    {
        readonly List<string> errors=new List<string>();string output;
        void OnEnable()=>Application.logMessageReceived+=Log;
        void OnDisable()=>Application.logMessageReceived-=Log;
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+trace);}
        void Check(bool ok,string message){if(!ok)errors.Add(message);}
        IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return null;}
        IEnumerator ReviewWeather(MaritimeScene scene)
        {
            float rain=scene.scenario.rain;string clock=scene.scenario.utc;
            Check(Enumerable.Range(0,42).Select(PantelleriaAppearance.FinishSeed).Distinct().Count()==42,"all ships retain unique finish seeds");
            foreach(int ship in new[]{0,1}) {
                scene.scenario.rain=0;scene.Focus(ship);
                scene.target=scene.scenario.ships[ship].Position+Quaternion.Euler(0,scene.scenario.ships[ship].heading,0)*new Vector3(0,6,PantelleriaScenario.Ships[ship].model.length*.39f);
                scene.pitch=35;scene.distance=30;scene.SetCamera();scene.UpdateWorld();yield return new WaitForSeconds(.7f);yield return Capture("weathered-deck-dry-"+ship);
                var deck=scene.vessels[ship].GetComponentsInChildren<MeshRenderer>().First(r=>r.sharedMaterial.shader.name=="Ironclads/Pantelleria Teak");var properties=new MaterialPropertyBlock();deck.GetPropertyBlock(properties);
                Check(properties.GetFloat("_FinishSeed")==PantelleriaAppearance.FinishSeed(ship),"runtime deck retains stable seed "+ship);
                if(ship==0){scene.scenario.rain=1;scene.UpdateWorld();Check(Shader.GetGlobalFloat("_PantelleriaRain")==1,"rain drives exposed finish wetness");yield return new WaitForSeconds(.3f);yield return Capture("weathered-deck-wet-0");}
            }
            scene.scenario.rain=rain;scene.UpdateWorld();Check(scene.scenario.utc==clock,"finish previews do not advance game clock");
        }
        IEnumerator ReviewSeas(MaritimeScene scene)
        {
            var s=scene.scenario;float height=s.waveHeight,wind=s.windFrom;var speeds=s.ships.Select(ship=>ship.speed).ToArray();
            string clock=s.utc;scene.Focus(0);scene.pitch=12;scene.distance=190;scene.SetCamera();s.waveHeight=3;s.windFrom=s.ships[0].heading;scene.UpdateWorld();
            int before=scene.pantelleriaWash.TotalSprayEmitted;yield return new WaitForSeconds(6);yield return Capture("hull-head-sea");
            Check(scene.pantelleriaWash.TotalSprayEmitted>before&&scene.pantelleriaWash.SprayParticleCount<=500,"rough head sea produces bounded crest-impact droplets");
            Check(scene.NearOceanSurface.sharedMesh.vertexCount==769*769,"fine near-water mesh active");
            s.windFrom=s.ships[0].heading+90;scene.UpdateWorld();yield return new WaitForSeconds(1);yield return Capture("hull-beam-sea");
            s.windFrom=s.ships[0].heading+180;scene.UpdateWorld();yield return new WaitForSeconds(1);yield return Capture("hull-following-sea");
            foreach(var ship in s.ships)ship.speed=0;scene.UpdateWorld();Check(scene.pantelleriaWash.ActiveCount==0&&scene.pantelleriaWash.HullContactCount>0,"stopped hulls retain wave contact without movement wake");yield return Capture("hull-stopped-rough");
            s.waveHeight=0;scene.UpdateWorld();Check(scene.pantelleriaWash.HullContactCount==0&&scene.pantelleriaWash.SprayParticleCount==0,"calm stopped ships have no contact foam or spray");yield return Capture("hull-stopped-calm");
            for(int ship=0;ship<42;ship++)s.ships[ship].speed=speeds[ship];s.waveHeight=height;s.windFrom=wind;scene.UpdateWorld();Check(s.utc==clock,"sea effects leave operational clock unchanged");
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-verify-pantelleria");output=args[index+1];Directory.CreateDirectory(output);yield return new WaitForSeconds(1);
            var scene=GetComponent<MaritimeScene>();scene.StartPantelleria();var naval=scene.dawn;var chart=GetComponent<RuleChart>();var fx=GetComponent<BatteryEffects>();fx.silentVerification=true;fx.ripple=.002f;
            Check(scene.vessels.Length==42&&scene.network.PlayerCapacity==32,"42 models and 32-player lobby capacity");
            Check(scene.vessels.All(v=>v.GetComponent<NavalColours>()!=null)&&scene.vessels.Sum(v=>v.GetComponent<NavalColours>().GetComponentsInChildren<Renderer>(true).Count(r=>r.sharedMaterial.shader.name=="Ironclads/Naval Cloth"&&!v.GetComponent<NavalColours>().IsRigging(r.transform)))==84,"84 national ensigns and commissioning pennants");
            int savedRigging=NavalColours.RiggingMode;
            yield return ReviewWeather(scene);
            yield return ReviewSeas(scene);
            Check(scene.vessels.All(v=>v.transform.Find(Shipwright.PantelleriaCloseDetailsName)!=null),"close-up fittings on all 42 ships");
            scene.Focus(0);scene.target=scene.scenario.ships[0].Position+Quaternion.Euler(0,scene.scenario.ships[0].heading,0)*new Vector3(0,8,PantelleriaScenario.Ships[0].model.length*.19f);scene.distance=28;scene.pitch=32;scene.SetCamera();yield return Capture("bridge-fittings");
            scene.target=scene.scenario.ships[0].Position+Quaternion.Euler(0,scene.scenario.ships[0].heading,0)*new Vector3(0,5,PantelleriaScenario.Ships[0].model.length*.39f);scene.distance=22;scene.pitch=42;scene.SetCamera();yield return Capture("foredeck-fittings");
            scene.target=scene.scenario.ships[0].Position+Quaternion.Euler(0,scene.scenario.ships[0].heading,0)*new Vector3(1.5f,5.1f,PantelleriaScenario.Ships[0].model.length*.425f);scene.distance=6;scene.pitch=48;scene.SetCamera();yield return Capture("anchor-chain-detail");
            scene.target=scene.scenario.ships[0].Position+Quaternion.Euler(0,scene.scenario.ships[0].heading,0)*new Vector3(PantelleriaScenario.Ships[0].model.beam*.28f,8.1f,8);scene.distance=12;scene.pitch=65;scene.SetCamera();yield return Capture("boat-floorboards");
            foreach(int mode in new[]{2,1,0}) {
                NavalColours.SelectRiggingMode(mode,false);scene.Focus(0);scene.pitch=22;scene.UpdateWorld();scene.SetCamera();
                int expectedFlags=84+42*(mode==2?NavalColours.DressFlagCount:mode==1?NavalColours.SignalFlagCount:0);
                Check(scene.vessels.Sum(v=>v.GetComponentsInChildren<Renderer>().Count(r=>r.sharedMaterial.shader.name=="Ironclads/Naval Cloth"))==expectedFlags,"visible rigging flags in mode "+mode);
                yield return new WaitForSeconds(.3f);yield return Capture("rigging-mode-"+mode);
                if(mode==2){scene.yaw+=120;scene.SetCamera();yield return Capture("rigging-full-dress-reverse");}
            }
            NavalColours.SelectRiggingMode(savedRigging,false);
            scene.Focus(0);scene.SetCamera();scene.UpdateWorld();var wash=scene.pantelleriaWash;int movingWash=wash.ActiveCount;Check(movingWash>0,"nearby moving fleet has surface wash");
            var speeds=scene.scenario.ships.Select(ship=>ship.speed).ToArray();foreach(var ship in scene.scenario.ships)ship.speed=0;scene.UpdateWorld();Check(wash.ActiveCount==0,"stopped fleet hides all wash");for(int ship=0;ship<speeds.Length;ship++)scene.scenario.ships[ship].speed=speeds[ship];
            scene.scenario.pantelleria.sunk[0]=true;scene.UpdateWorld();Check(wash.ActiveCount==movingWash-1,"sunk ship hides its wash");scene.scenario.pantelleria.sunk[0]=false;scene.UpdateWorld();
            scene.pitch=38;scene.SetCamera();yield return new WaitForSeconds(.3f);yield return Capture("bow-stern-wash");
            bool wasAstern=scene.scenario.ships[0].astern;scene.scenario.ships[0].astern=true;scene.UpdateWorld();yield return Capture("astern-wash");scene.scenario.ships[0].astern=wasAstern;scene.UpdateWorld();
            var flagClock=scene.scenario.Date;
            foreach(int ship in new[]{0,13,21,34}) {
                scene.Focus(ship);scene.UpdateWorld();var colours=scene.vessels[ship].GetComponent<NavalColours>();
                scene.target=colours.Ensign.position+colours.Ensign.right*2;scene.distance=9;scene.pitch=12;scene.yaw=Mathf.Atan2(colours.Ensign.forward.x,colours.Ensign.forward.z)*Mathf.Rad2Deg;scene.SetCamera();
                yield return Capture("ensign-"+colours.Nation+"-a");yield return new WaitForSeconds(.28f);yield return Capture("ensign-"+colours.Nation+"-b");
                if(ship==0){scene.yaw+=180;scene.SetCamera();yield return Capture("ensign-reverse");}
                scene.target=colours.Pennant.position+colours.Pennant.right*3.5f;scene.distance=11;scene.SetCamera();yield return Capture("pennant-"+colours.Nation);
            }
            Check(scene.scenario.Date==flagClock,"cloth animates while operational game clock stays paused");
            Check(naval.ExhaustParticlesEmitted>0,"lit fleet produces real exhaust particles");
            var lit=scene.scenario.ships.Select(ship=>ship.boilersLit).ToArray();foreach(var ship in scene.scenario.ships)ship.boilersLit=false;
            long stoppedEmission=naval.ExhaustParticlesEmitted;yield return new WaitForSeconds(.4f);Check(naval.ExhaustParticlesEmitted==stoppedEmission,"extinguished fleet emits no new exhaust");
            for(int ship=0;ship<lit.Length;ship++)scene.scenario.ships[ship].boilersLit=lit[ship];
            var original=scene.scenario.ships.Select(v=>v.Position).ToArray();
            for(int i=0;i<42;i++){Check(naval.Move(i,30),"move slot "+i);Check(Mathf.Abs(Vector3.Distance(scene.scenario.ships[i].Position,original[i])-UlsanScenario.Distance(15,30))<.02f,"metric movement "+i);}
            for(int division=0;division<14;division++)Check(naval.MoveDivision(division,30),"division movement "+division);
            var before=scene.scenario.ships.Select(v=>v.Position).ToArray();Check(naval.PlotDivision(13,330,12),"plot final division");Check(scene.scenario.ships[41].heading==0&&scene.scenario.ships[41].dobOrderedHeading==330,"plotted course does not immediately turn ship");Check(naval.ApplyDivision(13)&&scene.scenario.ships[41].heading==330&&scene.scenario.ships[41].speed==12,"apply final division orders");
            Check(!naval.MoveDivision(0,1800)&&scene.scenario.ships.Select(v=>v.Position).SequenceEqual(before),"invalid group action is atomic");
            int turn=scene.scenario.turn;naval.EndTurn();Check(scene.scenario.turn==turn+1&&scene.scenario.ships.All(v=>v.dobMovedSeconds==0)&&scene.scenario.ships.Select(v=>v.Position).SequenceEqual(before),"clock does not move fleet");
            scene.selected=41;float heading=scene.scenario.ships[41].heading;Check(naval.AdvanceTurn(41,45,false)&&scene.scenario.ships[41].heading!=heading,"bridge turn last ship");
            var p=scene.scenario.ships[0].Position;scene.scenario.pantelleria.damagePercent[41]=51;scene.scenario.pantelleria.divisionOrders[13]="Runtime final division order";var copy=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(copy.pantelleria.damagePercent[41]==51&&copy.pantelleria.divisionOrders[13].Length>0,"result and order persistence");
            scene.Focus(0);scene.pitch=14;scene.distance=240;scene.SetCamera();yield return new WaitForSeconds(6);yield return Capture("sustained-exhaust");
            int expected=0;foreach(int i in new[]{0,13,21,34,41}){expected+=fx.Battery(i,2).Length;Check(fx.FireLocal(i,2),"salvo "+i);}float salvoDeadline=Time.realtimeSinceStartup+15;while(fx.ShotsFired<expected&&Time.realtimeSinceStartup<salvoDeadline)yield return null;Check(fx.ShotsFired==expected,"all sampled national batteries discharge from real muzzles");
            Check(naval.ExhaustCount==PantelleriaScenario.Ships.Sum(v=>v.model.funnels),"all fleet funnels attached");fx.Clear();
            scene.scenario.pantelleria.sunk[41]=true;scene.UpdateWorld();Check(!scene.vessels[41].gameObject.activeSelf&&!naval.Move(41,1)&&fx.Battery(41,2).Length==0,"sunk status disables model, movement and fire");scene.scenario.pantelleria.sunk[41]=false;scene.UpdateWorld();
            scene.Overview();yield return Capture("fleet-overview");
            scene.pitch=12;scene.SetCamera();yield return Capture("fleet-low-angle");
            foreach(int i in new[]{0,10,16,19,24,30,33,34,38}){scene.Focus(i);scene.pitch=25;scene.distance*=.85f;scene.SetCamera();yield return Capture("ship-"+i);}
            scene.Focus(0);scene.target=scene.scenario.ships[0].Position+Quaternion.Euler(0,scene.scenario.ships[0].heading,0)*new Vector3(0,6.8f,PantelleriaScenario.Ships[0].model.length*.34f);scene.distance=24;scene.pitch=28;scene.SetCamera();yield return Capture("gunhouse-detail");
            int openShip=Array.FindIndex(PantelleriaScenario.Ships,ship=>ship.model.open);Check(openShip>=0,"open-barbette camera sample exists");scene.Focus(openShip);scene.target=scene.scenario.ships[openShip].Position+Quaternion.Euler(0,scene.scenario.ships[openShip].heading,0)*new Vector3(0,7.1f,PantelleriaScenario.Ships[openShip].model.length*.34f);scene.distance=16;scene.pitch=12;scene.SetCamera();yield return Capture("open-carriage-side");scene.yaw+=120;scene.SetCamera();yield return Capture("open-carriage-reverse");
            var panel=GetComponent<UmpirePanel>();panel.ShowPantelleriaTab(2);scene.Focus(24);yield return Capture("gunnery");panel.ShowPantelleriaTab(4);yield return Capture("result-ledger");
            File.WriteAllText(Path.Combine(output,"fleet-roundtrip.json"),JsonUtility.ToJson(scene.scenario,true));
            scene.StartUlsan();Check(scene.vessels.Length==7&&naval.Move(6,30),"Ulsan switches back and moves last cruiser");Check(scene.NearOceanSurface.sharedMesh.vertexCount==257*257&&Shader.GetGlobalFloat("_PantelleriaSea")==0,"legacy water mesh and sea path restore outside Pantelleria");scene.StartJames();Check(scene.vessels.Length==6,"Stonewall switches back");scene.StartScenario(false);Check(scene.vessels.Length==2,"Ferrol switches back");scene.StartPantelleria();Check(scene.vessels.Length==42,"Pantelleria restores after module switching");
            File.WriteAllText(Path.Combine(output,"runtime-verification.txt"),errors.Count==0?"PASS: all 42 ships and 14 divisions move; plot/apply orders; atomic group rejection; clock, bridge pivots, results and save/load; all fleet funnels; national salvos; sunk-state controls; existing scenario switching. No logged runtime errors.":"FAIL:\n"+string.Join("\n",errors));Application.Quit(errors.Count==0?0:1);
        }
    }
}
