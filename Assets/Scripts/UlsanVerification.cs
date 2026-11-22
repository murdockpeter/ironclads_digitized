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
            string[] args=Environment.GetCommandLineArgs();int offline=Array.IndexOf(args,"-verify-ulsan");
            output=args[offline+1];Directory.CreateDirectory(output);yield return new WaitForSeconds(1);
            scene=GetComponent<MaritimeScene>();GetComponent<BatteryEffects>().silentVerification=true;scene.soundEnabled=false;
            yield return Offline();scene.network.Stop();
            File.WriteAllText(Path.Combine(output,"ulsan-runtime-verification.txt"),errors.Count==0?"PASS: Ulsan fleet models, movement, bridge pivot, turn intervals, turrets, 21 funnels, salvos, save/load and scenario switching.":"FAIL:\n"+string.Join("\n",errors));Application.Quit(errors.Count==0?0:1);
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
            Check(scene.network.Host(0,-1,"ULSANTEST01",true),"Ulsan lobby host");scene.network.AssignShip(3,0);scene.network.AssignShip(6,0);Check(scene.network.StartMatch()&&scene.network.CanControl(3)&&scene.network.CanControl(6)&&!scene.network.CanControl(0),"host controls only assigned cruisers");scene.network.Stop();
            scene.StartUlsan();scene.Focus(3);GetComponent<UmpirePanel>().ShowDawnTab(1);yield return Capture("Player-Ulsan-gunnery.png");
            scene.Focus(0);GetComponent<UmpirePanel>().ShowDawnTab(0);yield return Capture("Player-Ulsan-movement.png");scene.Overview();yield return Capture("Player-Ulsan-opening.png");
            scene.StartScenario(true);yield return null;Check(scene.vessels.Length==2&&scene.support.root.gameObject.activeSelf,"Hampton restoration");
            scene.scenario=copy;scene.UpdateWorld();yield return null;Check(scene.vessels.Length==7&&!scene.support.root.gameObject.activeSelf&&!scene.terrain.root.gameObject.activeSelf,"restore saved Ulsan hides Hampton content");
        }
    }
}
