using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    // Invoked only by the explicit developer -verify-runtime command-line switch.
    public class RuntimeVerification : MonoBehaviour
    {
        readonly List<string> failures=new List<string>();
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)failures.Add(message);}
        void Check(bool ok,string message){if(!ok)failures.Add(message);}
        IEnumerator Capture(string path)
        {
            yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();
            if(image==null){failures.Add("No screenshot image for "+path);yield break;}
            var pixels=image.GetPixels32();int min=765,max=0;
            for(int i=0;i<pixels.Length;i+=1973){var c=pixels[i];int value=c.r+c.g+c.b;min=Math.Min(min,value);max=Math.Max(max,value);}
            Check(max-min>40,"Screenshot is blank or uniform: "+Path.GetFileName(path));
            File.WriteAllBytes(path,image.EncodeToPNG());Destroy(image);yield return new WaitForSeconds(.2f);
        }
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-verify-runtime");
            if(index<0)yield break;
            string output=index+1<args.Length?args[index+1]:Application.persistentDataPath;
            Directory.CreateDirectory(output);yield return new WaitForSeconds(2);
            var scene=FindFirstObjectByType<MaritimeScene>();var chart=scene.GetComponent<RuleChart>();var panel=scene.GetComponent<UmpirePanel>();
            Check(scene.vessels[0].GetComponentsInChildren<MeshFilter>().Length==36,"Niagara does not have the pass-4 spatial mesh sections.");
            Check(scene.vessels[1].GetComponentsInChildren<MeshFilter>().Length==32,"Stonewall does not have the pass-4 spatial mesh sections.");
            string initial=JsonUtility.ToJson(scene.scenario);
            Vector3 a=scene.scenario.ships[0].Position,b=scene.scenario.ships[1].Position;DateTime utc=scene.scenario.Date;
            chart.NextTurn();Check(scene.scenario.ships[0].Position==a&&scene.scenario.ships[1].Position==b,"End Turn moved a ship in umpire mode.");
            Check((scene.scenario.Date-utc).TotalSeconds==180&&scene.scenario.turn==2,"End Turn did not advance three minutes.");
            chart.Undo();Check(scene.scenario.turn==1,"Undo failed to restore turn.");
            Hex old=RuleChart.At(scene.scenario.ships[0].Position);chart.Move(1);
            Check(RuleChart.Range(old,RuleChart.At(scene.scenario.ships[0].Position))==1,"Ahead one hex did not move one hex.");
            chart.Undo();Check(scene.scenario.ships[0].Position==a,"Undo failed to restore ship position.");
            chart.Turn(1);Check(Mathf.RoundToInt(scene.scenario.ships[0].heading)%60==0,"Turn control broke hexside facing.");chart.Undo();
            Check(GunneryTools.Within(359,0,10)&&GunneryTools.Within(1,359,10),"Gunnery bearing failed across north.");
            Check(GunneryTools.Within(60,0,120)&&!GunneryTools.Within(61,0,120),"Gunnery arc boundary failed.");
            Check(GunneryTools.Within(180,0,360),"Full-circle custom guide excluded astern.");
            Check(GunneryTools.RayCrossesHex(Vector3.zero,0,new Hex(0,2))&&!GunneryTools.RayCrossesHex(Vector3.zero,90,new Hex(0,2)),"Target-hex boundary crossing failed.");
            Check(!GunneryTools.RayCrossesHex(Vector3.zero,180,new Hex(0,2)),"Boundary ray incorrectly crossed a hex behind it.");
            scene.scenario=JsonUtility.FromJson<Scenario>(initial);scene.status="Runtime checks: manual placement, range and turn clock passed.";
            yield return Capture(Path.Combine(output,"Player-interface.png"));
            panel.ShowTab(7);yield return new WaitForSeconds(.2f);yield return Capture(Path.Combine(output,"Player-multiplayer.png"));
            chart.topDown=true;scene.distance=1100;panel.ShowTab(2);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-chart.png"));
            var guns=scene.GetComponent<GunneryTools>();scene.selected=1;guns.preset=0;guns.visible=true;chart.showRange=false;panel.ShowTab(4);
            yield return new WaitForSeconds(.2f);yield return Capture(Path.Combine(output,"Player-gunnery.png"));
            Check(guns.Range==RuleChart.Range(RuleChart.At(a),RuleChart.At(b)),"Gunnery ruler hex range disagrees with board.");
            string beforeGunnery=JsonUtility.ToJson(scene.scenario);guns.pickedTarget=true;guns.targetHex=new Hex(0,3);guns.preset=3;guns.customCenter=0;guns.customSweep=90;
            yield return new WaitForSeconds(.2f);Check(JsonUtility.ToJson(scene.scenario)==beforeGunnery,"Gunnery guide modified scenario state.");
            yield return Capture(Path.Combine(output,"Player-gunnery-custom.png"));
            guns.visible=false;guns.pickedTarget=false;chart.showRange=true;
            chart.topDown=false;scene.Focus(0);panel.ShowTab(0);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Niagara.png"));
            scene.Focus(1);chart.showArcs=true;yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Stonewall.png"));
            scene.showOverlay=false;chart.showArcs=false;
            scene.selected=0;scene.InspectDetail(true);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Niagara-bow.png"));
            scene.InspectDetail(false);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Niagara-gun.png"));
            scene.InspectStern();yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Niagara-stern.png"));
            scene.selected=1;scene.InspectDetail(true);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Stonewall-bow.png"));
            scene.InspectDetail(false);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Stonewall-gun.png"));
            var effects=scene.GetComponent<BatteryEffects>();effects.silentVerification=true;
            Check(effects.Battery(0,-1).Length==5&&effects.Battery(0,1).Length==5&&effects.Battery(0,2).Length==10,"Niagara battery selection disagrees with modeled guns.");
            Check(effects.Battery(1,0).Length==1&&effects.Battery(1,-1).Length==1&&effects.Battery(1,1).Length==1&&effects.Battery(1,2).Length==3,"Stonewall battery selection disagrees with modeled guns.");
            Check(Vector3.Distance(BatteryEffects.Wind(0,10),Vector3.back*5.14444f)<.001f&&Vector3.Distance(BatteryEffects.Wind(270,10),Vector3.right*5.14444f)<.001f,"Gun smoke wind direction or knot conversion incorrect.");
            Vector3 transported=new Vector3(-20,0,0);for(int step=0;step<600;step++)transported=BatteryEffects.TransportVelocity(transported,Vector3.right*5,step/60f,1/60f,0,0);
            Check(transported.x>3&&transported.y>0,"Smoke did not lose blast momentum and approach downwind buoyant flow.");
            Vector3 eddyA=BatteryEffects.TransportVelocity(Vector3.zero,Vector3.right*5,8,.5f,1,10,new Vector3(0,5,0));
            Vector3 eddyB=BatteryEffects.TransportVelocity(Vector3.zero,Vector3.right*5,8,.5f,91,10,new Vector3(25,15,20));
            Check(Vector3.Distance(eddyA,eddyB)>.2f,"Smoke parcels have uniform drift despite spatial/particle variation.");
            Check(BatteryEffects.ReportRolloff().Evaluate(160/6000f)>.85f,"Normal inspection distance still excessively attenuates cannon reports.");
            Check(SeaAudioOutput.Limit(.3f)==.3f&&SeaAudioOutput.Limit(3)<.981f&&SeaAudioOutput.Limit(-3)>-.981f,"Overlapping cannon peaks are not bounded or quiet audio was changed.");
            scene.showOverlay=true;scene.selected=0;scene.Focus(0);panel.ShowTab(5);scene.scenario.windFrom=270;scene.scenario.windKnots=10;
            string beforeSalvo=JsonUtility.ToJson(scene.scenario);int shots=effects.ShotsFired;effects.Fire(0,-1);Check(!effects.Fire(0,-1),"Busy battery accepted duplicate salvo.");
            yield return new WaitForSeconds(2);
            Check(effects.ShotsFired-shots==5&&effects.powder.particleCount>250,"Niagara salvo did not emit five muzzle bursts.");
            var powderParticles=new ParticleSystem.Particle[8000];int powderCount=effects.powder.GetParticles(powderParticles);uint trackedSeed=powderParticles[0].randomSeed;Vector3 earlier=powderParticles[0].position;
            int darkPuffs=0,lightPuffs=0;float smallest=100,largest=0;
            for(int particle=0;particle<powderCount;particle++){float tone=powderParticles[particle].startColor.r/255f;if(tone<.66f)darkPuffs++;if(tone>.75f)lightPuffs++;smallest=Mathf.Min(smallest,powderParticles[particle].startSize);largest=Mathf.Max(largest,powderParticles[particle].startSize);}
            Check(darkPuffs>10&&lightPuffs>10&&largest>smallest*2,"Powder cloud lacks dark pockets, pale billows or size variation.");
            yield return Capture(Path.Combine(output,"Player-Niagara-salvo.png"));yield return new WaitForSeconds(4);
            powderCount=effects.powder.GetParticles(powderParticles);bool drifted=false;
            for(int particle=0;particle<powderCount;particle++)if(powderParticles[particle].randomSeed==trackedSeed)drifted=powderParticles[particle].position.x>earlier.x+3&&powderParticles[particle].position.y>earlier.y;
            Check(drifted,"Live powder cloud did not rise and drift downwind.");yield return Capture(Path.Combine(output,"Player-smoke-drift.png"));
            Check(JsonUtility.ToJson(scene.scenario)==beforeSalvo,"Salvo changed scenario positions, time or rules state.");
            scene.scenario.windFrom=90;yield return new WaitForSeconds(2);
            powderCount=effects.powder.GetParticles(powderParticles);Check(powderParticles[0].velocity.x<0,"Existing smoke failed to turn with a wind change.");
            effects.Clear();Check(effects.powder.particleCount==0&&!effects.firing[0],"Clear smoke failed.");
            effects.powder.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);effects.flash.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            scene.Focus(1);shots=effects.ShotsFired;effects.Fire(1,2);yield return new WaitForSeconds(1.5f);
            Check(effects.ShotsFired-shots==3&&effects.powder.particleCount>100,"Stonewall all-guns display did not restart stopped effects and fire three guns.");yield return Capture(Path.Combine(output,"Player-Stonewall-salvo.png"));effects.Clear();
            for(int kind=0;kind<3;kind++) {
                var reportClip=BatteryEffects.MakeReport(kind);var waveform=new float[reportClip.samples];reportClip.GetData(waveform,0);float peak=0;double energy=0;
                foreach(float sample in waveform){peak=Mathf.Max(peak,Mathf.Abs(sample));energy+=sample*sample;}
                Check(peak>.4f&&peak<1&&energy/waveform.Length>.001,"Cannon report waveform silent, invalid or hard clipped.");Destroy(reportClip);
            }
            scene.scenario=JsonUtility.FromJson<Scenario>(initial);
            var exhaust=scene.GetComponent<EngineExhaust>();exhaust.Build();exhaust.Clear();
            Check(exhaust.funnels.Length==4&&exhaust.funnels[3]==null&&exhaust.funnels[0].transform.parent==scene.vessels[0]&&exhaust.funnels[1].transform.parent==scene.vessels[0]&&exhaust.funnels[2].transform.parent==scene.vessels[1],"Engine exhaust funnel/ship attachment incorrect.");
            Check(EngineExhaust.EmissionRate(1,true)>EngineExhaust.EmissionRate(0,true)*10&&EngineExhaust.EmissionRate(1,false)==0,"Engine order does not control exhaust output.");
            exhaust.SetOrder(0,1);exhaust.SetOrder(1,1);scene.scenario.windFrom=270;scene.scenario.windKnots=14;
            scene.Focus(0);panel.ShowTab(6);yield return new WaitForSeconds(3);
            Check(exhaust.funnels[0].particleCount>60&&exhaust.funnels[1].particleCount>60&&exhaust.funnels[2].particleCount>60,"Loaded engines did not emit from all three funnels.");
            var engineCloud=new ParticleSystem.Particle[2600];exhaust.funnels[0].GetParticles(engineCloud);Vector3 engineEarlier=engineCloud[0].position;uint engineSeed=engineCloud[0].randomSeed;
            Check(engineCloud[0].startColor.r/255f<.35f,"Fresh loaded funnel exhaust is not darker than powder smoke.");
            yield return Capture(Path.Combine(output,"Player-Niagara-engine-smoke.png"));scene.Focus(1);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Stonewall-engine-smoke.png"));yield return new WaitForSeconds(3);
            int engineCount=exhaust.funnels[0].GetParticles(engineCloud);bool engineDrift=false;
            for(int i=0;i<engineCount;i++)if(engineCloud[i].randomSeed==engineSeed)engineDrift=engineCloud[i].position.x>engineEarlier.x+3&&engineCloud[i].position.y>engineEarlier.y;
            Check(engineDrift,"Funnel smoke did not rise and drift with the weather wind.");
            Vector3 stationaryCloud=engineCloud[0].position;scene.scenario.ships[0].east+=RuleChart.HexMeters;scene.UpdateWorld();exhaust.funnels[0].GetParticles(engineCloud);
            Check(Vector3.Distance(stationaryCloud,engineCloud[0].position)<.001f,"Existing funnel plume teleported with its ship.");
            scene.scenario.ships[0].east-=RuleChart.HexMeters;
            var engineRoundTrip=JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(scene.scenario));Check(engineRoundTrip.ships[0].engineLoad==1&&engineRoundTrip.ships[1].boilersLit,"Engine settings lost in table serialization.");
            Check(scene.scenario.ships[0].Position==a&&scene.scenario.ships[1].Position==b&&scene.scenario.Date==utc,"Visual engine order moved ships or advanced time.");
            exhaust.SetOrder(0,0,false);exhaust.SetOrder(1,0,false);int emittedBefore=exhaust.emitted[0]+exhaust.emitted[1]+exhaust.emitted[2];yield return new WaitForSeconds(.5f);
            Check(exhaust.emitted[0]+exhaust.emitted[1]+exhaust.emitted[2]==emittedBefore&&exhaust.funnels[0].particleCount>0,"Cold boilers did not stop new exhaust while preserving the existing plume.");
            exhaust.Clear();Check(exhaust.funnels[0].particleCount==0&&exhaust.funnels[2].particleCount==0,"Funnel plume clear failed.");
            scene.scenario=JsonUtility.FromJson<Scenario>(initial);
            scene.showOverlay=true;scene.Focus(1);
            scene.scenario.cloud=.95f;scene.scenario.rain=.85f;scene.scenario.windKnots=28;scene.scenario.waveHeight=2.5f;scene.scenario.visibilityKm=3;
            panel.ShowTab(1);chart.showGrid=false;chart.showRange=false;chart.showArcs=false;yield return new WaitForSeconds(2);
            yield return Capture(Path.Combine(output,"Player-weather.png"));
            scene.scenario=JsonUtility.FromJson<Scenario>(initial);scene.ToggleGlobe();panel.ShowTab(2);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-globe.png"));
            scene.ToggleGlobe();panel.ShowTab(3);panel.ShowSheet();yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-shipsheet.png"));
            panel.ShowSheet(true);yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(output,"Player-Niagara-sheet.png"));
            string report=failures.Count==0?"PASS: player startup and graphics; placement/turn/undo; gunnery; battery selection and salvos; audio waveforms; powder drift and variation; three funnel attachments; engine load/cold controls; dark exhaust, live rise/drift and world-space trails; engine settings serialization; engines do not move ships or advance time; clear effects; ship, weather, globe and reference views; no runtime errors logged. Automated salvos were muted.":"FAIL:\n"+string.Join("\n",failures);
            File.WriteAllText(Path.Combine(output,"runtime-verification.txt"),report);Application.Quit(failures.Count==0?0:1);
        }
    }
}
