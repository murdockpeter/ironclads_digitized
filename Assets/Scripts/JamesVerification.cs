using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Ironclads
{
    public class JamesVerification : MonoBehaviour
    {
        readonly List<string> errors=new List<string>();
        void OnEnable()=>Application.logMessageReceived+=Log;
        void OnDisable()=>Application.logMessageReceived-=Log;
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void Check(bool value,string message){if(!value)errors.Add(message);}
        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);var scene=GetComponent<MaritimeScene>();scene.StartJames();var chart=GetComponent<RuleChart>();var effects=GetComponent<BatteryEffects>();var exhaust=GetComponent<EngineExhaust>();var tools=GetComponent<GunneryTools>();effects.silentVerification=true;
            string initial=JsonUtility.ToJson(scene.scenario);var positions=scene.scenario.ships.Select(s=>s.Position).ToArray();
            Check(scene.vessels.Length==6&&scene.scenario.ShipSlots==6,"six moving ships available");
            for(int i=0;i<6;i++){scene.selected=i;chart.Move(1);Check(RuleChart.Range(RuleChart.At(positions[i]),RuleChart.At(scene.scenario.ships[i].Position))==1,"move ship "+i);chart.Undo();Check(scene.scenario.ships[i].Position==positions[i],"undo ship "+i);}
            chart.NextTurn();Check(scene.scenario.turn==2&&scene.scenario.ships.Select(s=>s.Position).SequenceEqual(positions),"end turn must not move ships");chart.Undo();
            scene.scenario.ships[0].turretHeading=90;scene.scenario.ships[0].aftTurretHeading=270;scene.UpdateWorld();
            foreach(var t in scene.vessels[0].GetComponentsInChildren<JamesTurret>())Check(Vector3.Dot(t.transform.forward,t.group==0?scene.vessels[0].right:-scene.vessels[0].right)>.99f,"independent Onondaga turrets");
            var roundtrip=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(roundtrip.ships[0].turretHeading==90&&roundtrip.ships[0].aftTurretHeading==270&&roundtrip.ships[5].name=="CSS Fredericksburg","six-ship turret save/load");
            scene.scenario=Scenario.Parse(initial);scene.UpdateWorld();exhaust.Build();
            for(int i=0;i<6;i++){Check(effects.Battery(i,2).Length==JamesScenario.Guns[i],"working battery "+i);Check(exhaust.funnels[i]!=null&&exhaust.funnels[i].transform.parent==scene.vessels[i],"funnel attachment "+i);Check(effects.FireLocal(i,2),"salvo accepted "+i);}
            yield return new WaitForSeconds(4);Check(effects.ShotsFired==19,"nineteen gun discharges");for(int i=0;i<6;i++)Check(exhaust.emitted[i]>0,"funnel emission "+i);
            scene.selected=5;tools.fleetTarget=0;tools.visible=true;tools.preset=3;Check(tools.Range==RuleChart.Range(RuleChart.At(scene.scenario.ships[5].Position),RuleChart.At(scene.scenario.ships[0].Position)),"last ship targeting first ship");
            scene.GetComponent<UmpirePanel>().ShowJamesTab(1);scene.Overview();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"James-gunnery.png"));yield return null;
            effects.Clear();tools.visible=false;scene.GetComponent<UmpirePanel>().ShowJamesTab(0);for(int i=0;i<6;i++){scene.Focus(i);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"James-ship-"+i+".png"));yield return null;}
            scene.StartScenario(false);yield return null;Check(scene.vessels.Length==2&&!scene.terrain.root.gameObject.activeSelf,"Ferrol restoration");Check(effects.Battery(0,2).Length==10,"Niagara working battery retained");
            scene.StartScenario(true);yield return null;Check(scene.vessels.Length==2&&scene.terrain.root.gameObject.activeSelf,"Hampton restoration");
            scene.StartJames();yield return null;Check(scene.vessels.Length==6&&effects.Battery(5,2).Length==4,"James restoration after module switching");scene.Overview();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"James-opening.png"));yield return null;
            Check(scene.network.Host(0,-1,"LOBBYPREVIEW",true),"James lobby opens");scene.GetComponent<UmpirePanel>().ShowJamesTab(5);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Output(),"James-multiplayer-lobby.png"));yield return null;scene.network.Stop();
            File.WriteAllText(Path.Combine(Output(),"runtime-verification.txt"),errors.Count==0?"PASS: all six ships move/undo, manual turn clock, independent Onondaga turret aiming, six-ship serialization, 19 muzzle discharges, six attached drifting exhaust systems, arbitrary target selection, Ferrol/Hampton/James module restoration, captured all six ships and table UI; no logged runtime errors.":"FAIL:\n"+string.Join("\n",errors));Application.Quit(errors.Count==0?0:1);
        }
        string Output(){var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-verify-james");string output=index>=0&&index+1<args.Length?args[index+1]:Application.persistentDataPath;Directory.CreateDirectory(output);return output;}
    }
}
