using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    public class HamptonVerification : MonoBehaviour
    {
        readonly List<string> errors=new List<string>();
        void OnEnable()=>Application.logMessageReceived+=Log;
        void OnDisable()=>Application.logMessageReceived-=Log;
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        void Check(bool ok,string message){if(!ok)errors.Add(message);}
        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);var scene=GetComponent<MaritimeScene>();scene.soundEnabled=false;scene.StartScenario(true);
            var effects=GetComponent<BatteryEffects>();effects.silentVerification=true;effects.ripple=.12f;
            var exhaust=GetComponent<EngineExhaust>();exhaust.Build();Check(exhaust.funnels[0].transform.localPosition.y<1&&exhaust.funnels[2].transform.localPosition.y>11,"scenario exhaust locations");
            Check(effects.Battery(0,0).Length==2&&effects.Battery(1,-1).Length==4&&effects.Battery(1,3).Length==1,"battery grouping");
            scene.scenario.ships[0].turretHeading=90;scene.UpdateWorld();Check(Vector3.Dot(effects.Battery(0,0)[0].Direction,scene.vessels[0].right)>.99f,"turret and muzzle rotation");
            Check(effects.FireLocal(0,0)&&effects.FireLocal(1,2),"both salvos accepted");yield return new WaitForSeconds(3);
            Check(effects.ShotsFired==12,"two plus ten discharges");Check(exhaust.emitted[0]>0&&exhaust.emitted[2]>0,"coal smoke emitted for both ships");
            var saved=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(saved.Hampton&&saved.ships[0].turretHeading==90,"save load preserves scenario and turret");
            scene.Focus(0);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"Player-Hampton-Monitor.png"));yield return null;
            scene.Focus(1);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"Player-Hampton-Virginia.png"));yield return null;
            scene.StartScenario(false);yield return null;Check(effects.Battery(0,2).Length==10&&effects.Battery(1,2).Length==3&&!scene.terrain.root.gameObject.activeSelf,"Ferrol battery/terrain restoration");
            scene.StartScenario(true);yield return null;Check(effects.Battery(0,2).Length==2&&scene.terrain.root.gameObject.activeSelf,"Hampton restoration");
            Check(effects.Battery(2,-1).Length==11&&effects.Battery(2,1).Length==21&&effects.Battery(2,0).Length==1,"Minnesota available battery groups");
            exhaust.Build();Check(exhaust.funnels[3]!=null&&exhaust.funnels[3].transform.parent==scene.support.minnesota,"Minnesota funnel attachment");
            int before=effects.ShotsFired;Check(effects.FireLocal(2,-1),"Minnesota broadside accepted");yield return new WaitForSeconds(2);Check(effects.ShotsFired==before+11,"Minnesota broadside discharge count");
            scene.GetComponent<UmpirePanel>().ShowTab(10);scene.support.Inspect(2);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"Player-Hampton-Minnesota.png"));yield return null;
            scene.support.Inspect(3);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"Player-Hampton-Congress.png"));yield return null;
            scene.support.Inspect(4);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"Player-Hampton-Cumberland.png"));yield return null;
            saved=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(saved.minnesotaPortGuns==11&&saved.minnesotaStarboardGuns==21&&saved.minnesota.east==scene.scenario.minnesota.east,"support ship save/load");
            Vector3 grounded=scene.scenario.minnesota.Position;GetComponent<RuleChart>().NextTurn();scene.UpdateWorld();Check(scene.scenario.minnesota.Position==grounded,"Minnesota stays grounded at turn end");
            var guide=GetComponent<GunneryTools>();guide.minnesotaOrigin=true;Check(Vector3.Distance(guide.Origin,RuleChart.Center(RuleChart.At(grounded)))<.001f,"Minnesota hex gunnery origin");guide.minnesotaOrigin=false;
            scene.scenario.rulesetId="iron-fire";scene.scenario.hexMode=false;scene.selected=0;
            var ship=scene.scenario.ships[0];ship.speed=10;ship.orderedSpeed=7;ship.heading=0;ship.movementUsed=0;ship.turnClass=3;
            var motion=GetComponent<CourseMovement>();Vector3 start=ship.Position;motion.Move(1,0);
            Check(Vector3.Distance(ship.Position,start+Vector3.forward*91.44f)<.001f&&ship.movementUsed==1,"Iron & Fire exact inch-to-yard movement");
            start=ship.Position;motion.Move(1,1);float angle=1f/3;
            Check(Vector3.Distance(ship.Position,start+new Vector3(3*91.44f*(1-Mathf.Cos(angle)),0,3*91.44f*Mathf.Sin(angle)))<.001f&&Mathf.Abs(ship.heading-angle*Mathf.Rad2Deg)<.001f,"turn follows class-M radius and retains exact heading");
            start=ship.Position;GetComponent<RuleChart>().NextTurn();Check(ship.speed==7&&ship.movementUsed==0&&ship.Position==start,"end turn applies speed orders without moving ships");
            motion.PlaceExact(new Vector3(123.45f,0,-98.76f));Check(Vector3.Distance(ship.Position,new Vector3(123.45f,0,-98.76f))<.001f,"exact placement avoids hex snapping");
            motion.Drift(.5f,90);Check(Mathf.Abs(ship.east-(123.45f+45.72f))<.001f&&ship.movementUsed==0,"manual drift is independent of powered movement");
            saved=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(saved.Miniatures&&!saved.hexMode&&saved.ships[0].speed==7&&saved.ships[0].orderedSpeed==7,"miniatures save/load");
            scene.GetComponent<UmpirePanel>().ShowTab(9);scene.Focus(0);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"Player-Iron-Fire-Movement.png"));yield return null;
            File.WriteAllText(Path.Combine(Output(),"hampton-runtime-verification.txt"),errors.Count==0?"PASS: Hampton batteries, twelve shots, turret, smoke, scenario switching; Iron & Fire exact movement, turning radius, speed orders, drift and persistence.":"FAIL:\n"+string.Join("\n",errors));
            Application.Quit(errors.Count==0?0:1);
        }
        static string Output(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-verify-hampton");return i>=0&&i+1<args.Length?args[i+1]:Application.persistentDataPath;}
    }
}
