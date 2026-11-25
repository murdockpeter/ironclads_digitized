using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    public class CampaignVerification : MonoBehaviour
    {
        readonly List<string> errors=new List<string>();
        void OnEnable()=>Application.logMessageReceived+=Log;
        void OnDisable()=>Application.logMessageReceived-=Log;
        void Log(string message,string trace,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)errors.Add(message+"\n"+trace);}
        void Check(bool value,string name){if(!value)errors.Add(name);}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-verify-campaign");string output=index>=0&&index+1<args.Length?args[index+1]:Application.persistentDataPath;Directory.CreateDirectory(output);
            yield return new WaitForSeconds(2);var scene=GetComponent<MaritimeScene>();BreakoutDesk.Show(scene);var desk=GetComponent<BreakoutDesk>();
            // Test on a fresh in-memory campaign; retain any actual saved campaign untouched.
            desk.suppressAutosave=true;var original=desk.state;desk.state=BreakoutCampaign.Create();desk.board.Sync();
            Check(desk.open&&desk.board.tokens.Count==19&&desk.board.view.enabled&&!scene.view.enabled,"campaign board has 19 selectable markers and its own active camera");
            desk.board.FocusStation(1);desk.board.NorthUp();var beforePan=desk.board.CameraTarget;
            desk.board.PanKeyboard(Vector2.up,.1f,false,false);var afterPan=desk.board.CameraTarget;
            Check(afterPan.z>beforePan.z&&afterPan.x==beforePan.x,"W moves north in north-up view");
            desk.board.PanKeyboard(Vector2.right,.1f,false,true);Check(desk.board.CameraTarget==afterPan,"typing in a text field blocks camera movement");
            desk.board.FocusStation(1);desk.board.PanKeyboard(Vector2.up,.1f,true,false);Check(Mathf.Abs((desk.board.CameraTarget-beforePan).magnitude/(afterPan-beforePan).magnitude-3)<.01f,"Shift increases keyboard pan speed threefold");
            string tactical=JsonUtility.ToJson(scene.scenario);desk.state.BeginLive();desk.state.Advance(15);var station=CampaignBoard.Stations[1];desk.state.Place(desk.state.vessels[0],1,station.x,station.z,180,"Verification deployment");desk.board.Sync();
            Check(desk.board.tokens[0].position==new Vector3(station.x,1,station.z),"campaign state updates 3D geographic marker position");
            desk.SelectVessel(0);desk.board.Overview();yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-overview.png"));yield return null;
            desk.board.Focus();yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-ship.png"));yield return null;
            desk.board.FocusStation(1);yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-upper-james.png"));yield return null;
            desk.board.FocusStation(5);yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-hampton-roads.png"));yield return null;
            desk.board.Overview();for(int tab=0;tab<8;tab++){desk.ShowTab(tab);yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-tab-"+tab+".png"));yield return null;}
            var rendered=ScreenCapture.CaptureScreenshotAsTexture();bool visible=false;for(int px=100;px<rendered.width;px+=100)for(int py=100;py<rendered.height;py+=100){var color=rendered.GetPixel(px,py);if(color.r+color.g+color.b>.1f)visible=true;}Check(visible,"campaign screenshot contains visible rendering (run the off-screen verifier, not a hidden player)");Destroy(rendered);
            var report=desk.state.QueueReport(CampaignSide.Union,"River picket","Division","No confirmed enemy contact.","Unconfirmed",15,0);desk.state.Release(report);
            File.WriteAllText(Path.Combine(output,"Union-example-dispatch.txt"),desk.state.Dispatch(CampaignSide.Union));File.WriteAllText(Path.Combine(output,"Confederate-example-dispatch.txt"),desk.state.Dispatch(CampaignSide.Confederate));
            Check(JsonUtility.ToJson(scene.scenario)==tactical,"campaign input and clock leave tactical state unchanged");
            string save=Path.Combine(output,"runtime-campaign.json");desk.state.Save(save);Check(BreakoutCampaign.Parse(File.ReadAllText(save)).minute==15,"runtime disk save round trip");
            desk.state=original;desk.ReturnToTable();yield return null;Check(scene.view.enabled&&!desk.board.view.gameObject.activeSelf,"return restores tactical camera");
            File.WriteAllText(Path.Combine(output,"runtime-verification.txt"),errors.Count==0?"PASS: WASD camera pan, typing guard, Shift speed, 19 3D campaign markers, live clock, marker placement, tactical isolation, save/load and camera restoration. No logged runtime errors.":"FAIL:\n"+string.Join("\n",errors));Application.Quit(errors.Count==0?0:1);
        }
    }
}
