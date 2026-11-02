using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    public class UlsanVerification : MonoBehaviour
    {
        readonly List<string> errors=new List<string>();MaritimeScene scene;string output;
        void OnEnable()=>Application.logMessageReceived+=Log;
        void OnDisable()=>Application.logMessageReceived-=Log;
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        void Check(bool ok,string reason){if(!ok)errors.Add(reason);}
        IEnumerator Until(Func<bool> condition,string reason){float end=Time.realtimeSinceStartup+18;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),"Timeout: "+reason);}
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();int network=Array.IndexOf(args,"-verify-ulsan-network"),offline=Array.IndexOf(args,"-verify-ulsan");bool host=network>=0&&args[network+1]=="host";
            output=network>=0?args[network+3]:args[offline+1];Directory.CreateDirectory(output);yield return new WaitForSeconds(1);
            scene=GetComponent<MaritimeScene>();GetComponent<BatteryEffects>().silentVerification=true;scene.soundEnabled=false;
            if(network>=0){if(host)yield return Host(int.Parse(args[network+2]));else yield return Guest(int.Parse(args[network+2]));}
            else yield return Offline();
            scene.network.Stop();string name=network>=0?(host?"ulsan-network-host.txt":"ulsan-network-guest.txt"):"ulsan-runtime-verification.txt";
            File.WriteAllText(Path.Combine(output,name),errors.Count==0?"PASS: "+(network>=0?"Ulsan fleet TCP ownership, queued multi-ship edits, turrets, shared salvos, clock, snapshots and disconnect.":"Ulsan fleet models, movement, bridge pivot, turn intervals, turrets, 21 funnels, salvos, save/load and scenario switching."):"FAIL:\n"+string.Join("\n",errors));Application.Quit(errors.Count==0?0:1);
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();var tex=ScreenCapture.CaptureScreenshotAsTexture();Check(tex!=null,"Missing screenshot "+name);if(tex!=null){File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());Destroy(tex);}yield return null;
        }
        IEnumerator Offline()
        {
            scene.StartUlsan();yield return new WaitForSeconds(1);var table=scene.dawn;var effects=GetComponent<BatteryEffects>();var s=scene.scenario;
            Check(s.ships.Length==7&&scene.vessels.Length==7,"seven ship state/model count");Check(table.ExhaustCount==21,"all 21 modeled funnels have exhaust");
            Check(effects.Battery(0,2).Length==32&&effects.Battery(6,2).Length==30,"first and last ship batteries");
            var ship=s.ships[0];Vector3 start=ship.Position;Check(table.Move(0,60),"one-minute move");Check(Mathf.Abs(ship.east-start.x-UlsanScenario.Distance(10,60))<.01f,"minute movement distance");
            start=ship.Position;Check(!table.Move(0,180)&&ship.Position==start,"remaining movement budget");Check(table.AdvanceTurn(0,45,false)&&Mathf.Abs(ship.heading-135)<.001f,"advance and bridge pivot");
            s.ships[3].turretHeading=90;s.ships[3].aftTurretHeading=270;scene.UpdateWorld();Check(Vector3.Dot(effects.Battery(3,0)[0].Direction,scene.vessels[3].right)>.99f&&Vector3.Dot(effects.Battery(3,3)[0].Direction,scene.vessels[3].right)<-.99f,"both turret meshes and muzzle directions rotate");
            int before=effects.ShotsFired;Check(effects.FireLocal(3,0),"Japanese turret salvo");yield return new WaitForSeconds(1);Check(effects.ShotsFired==before+2,"two-gun main turret discharge");
            var copy=Scenario.Parse(JsonUtility.ToJson(s));Check(copy.DoB&&copy.ships[3].aftTurretHeading==270&&copy.ships[0].dobMovedSeconds>0,"fleet save/load");
            start=ship.Position;DateTime time=s.Date;table.EndTurn();Check((s.Date-time).TotalSeconds==180&&ship.Position==start&&ship.dobMovedSeconds==0,"tactical clock reset preserves position");
            s.dobTurnSeconds=1800;Check(table.Move(1,1800),"intermediate move accepted");time=s.Date;table.EndTurn();Check((s.Date-time).TotalSeconds==1800,"intermediate clock");
            Check(scene.network.Host(0,1,"ULSANTEST01",true)&&scene.network.CanControl(3)&&scene.network.CanControl(6)&&!scene.network.CanControl(0),"Japanese host owns entire fleet only");scene.network.Stop();
            scene.StartUlsan();scene.Focus(3);GetComponent<UmpirePanel>().ShowDawnTab(1);yield return Capture("Player-Ulsan-gunnery.png");
            scene.Focus(0);GetComponent<UmpirePanel>().ShowDawnTab(0);yield return Capture("Player-Ulsan-movement.png");scene.Overview();yield return Capture("Player-Ulsan-opening.png");
            scene.StartScenario(true);yield return null;Check(scene.vessels.Length==2&&scene.support.root.gameObject.activeSelf,"Hampton restoration");
            scene.scenario=copy;scene.UpdateWorld();yield return null;Check(scene.vessels.Length==7&&!scene.support.root.gameObject.activeSelf&&!scene.terrain.root.gameObject.activeSelf,"restore saved Ulsan hides Hampton content");
        }
        IEnumerator Host(int port)
        {
            scene.StartUlsan();var net=scene.network;var original=UlsanScenario.Create();Check(net.Host(port,0,"ULSANSESSION01",true),"host startup");File.WriteAllText(Path.Combine(output,"host-ready.txt"),port.ToString());
            yield return Until(()=>net.Connected,"guest welcome");scene.scenario.umpireNotes="ulsan:orders";
            yield return Until(()=>Mathf.Abs(scene.scenario.ships[6].heading-(125+6*2.3f))<.01f,"fleet edits arrive");
            for(int i=3;i<7;i++){var ship=scene.scenario.ships[i];Check(Mathf.Abs(ship.east-original.ships[i].east-i*12.345f)<.01f&&Mathf.Abs(ship.heading-(125+i*2.3f))<.01f&&ship.dobMovedSeconds==30+i,"queued ship edit "+i);}
            Check(scene.scenario.ships[0].Position==original.ships[0].Position,"forged enemy edit rejected");yield return Until(()=>GetComponent<BatteryEffects>().ShotsFired==2,"guest turret salvo");
            Check(net.RequestFire(0,0),"host bow salvo");scene.scenario.umpireNotes="ulsan:fired";
            yield return Until(()=>File.Exists(Path.Combine(output,"guest-seen.txt")),"guest receives all state and salvos");scene.dawn.EndTurn();
            yield return Until(()=>!net.Connected,"guest orderly disconnect");Check(GetComponent<BatteryEffects>().ShotsFired==3,"shared discharge count on host");
        }
        IEnumerator Guest(int port)
        {
            var net=scene.network;Check(net.Join("127.0.0.1",port,"ULSANSESSION01"),"guest startup");yield return Until(()=>net.Connected&&scene.scenario.DoB&&scene.vessels.Length==7,"Ulsan snapshot creates fleet");
            Check(net.LocalShip==1&&!net.CanControl(0)&&net.CanControl(3)&&net.CanControl(6),"Japanese fleet assignment");yield return Until(()=>scene.scenario.umpireNotes=="ulsan:orders","host phase");
            var original=UlsanScenario.Create();Check(!scene.dawn.Place(0,new Vector3(123,0,456)),"opponent placement blocked");
            net.VerificationSend(new TablePacket{type="ship",ship=0,scenarioId="ulsan",rulesetId="dob",turnNumber=scene.scenario.turn,sequence=999,heading=90,load=.5f});
            for(int i=3;i<7;i++){var ship=scene.scenario.ships[i];ship.east+=i*12.345f;ship.heading=125+i*2.3f;ship.dobMovedSeconds=30+i;ship.turretHeading=70+i*10;ship.aftTurretHeading=245-i*5;net.SubmitShip(i);}
            Check(net.RequestFire(6,0),"guest fires last ship turret");yield return Until(()=>scene.scenario.umpireNotes=="ulsan:fired"&&GetComponent<BatteryEffects>().ShotsFired==3,"shared salvos");yield return new WaitForSeconds(.5f);
            for(int i=3;i<7;i++)Check(Mathf.Abs(scene.scenario.ships[i].east-original.ships[i].east-i*12.345f)<.01f,"pending edits survived snapshots "+i);
            Check(scene.scenario.ships[6].turretHeading==130&&scene.scenario.ships[6].aftTurretHeading==215,"both turret orders replicated");File.WriteAllText(Path.Combine(output,"guest-seen.txt"),"ready");
            yield return Until(()=>scene.scenario.turn==2,"shared clock");Check(scene.scenario.ships[6].dobMovedSeconds==0,"shared movement reset");net.Stop();
        }
    }
}
