using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Ironclads
{
    public sealed class CloseActionVerification : MonoBehaviour
    {
        string output;MaritimeScene host,guest;float deadline;
        void Check(bool okay,string message){if(!okay){File.AppendAllText(output,"FAIL: "+message+"\n");Debug.LogError("CA verification: "+message);Application.Quit(1);throw new Exception(message);}File.AppendAllText(output,"PASS: "+message+"\n");}
        IEnumerator Until(Func<bool> condition,string message)
        {deadline=Time.realtimeSinceStartup+20;while(!condition()&&Time.realtimeSinceStartup<deadline)yield return null;Check(condition(),message);}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-verify-close-action");output=at>=0&&at+1<args.Length?args[at+1]:Path.Combine(Application.persistentDataPath,"close-action-verification.txt");Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));File.WriteAllText(output,"");
            yield return new WaitForSeconds(2);host=GetComponent<MaritimeScene>();host.StartCloseAction();host.soundEnabled=false;host.GetComponent<BatteryEffects>().silentVerification=true;
            host.Focus(2);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.ChangeExtension(output,"png"));
            host.GetComponent<UmpirePanel>().ShowCloseActionTab(5);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output)),"CloseAction-weather.png"));yield return null;
            host.GetComponent<UmpirePanel>().ShowCloseActionTab(0);
            foreach(var prototype in CloseActionPrototypes.All){
                int index=prototype.representative;int oldSail=host.scenario.closeAction.logs[index].sail;host.scenario.closeAction.logs[index].sail=2;host.UpdateWorld();host.Focus(index);var spec=CloseActionScenario.Spec(host.scenario,index);
                Check(host.vessels[index].Find("Attached bow assembly/Rigid bowsprit assembly")!=null,"rigid bowsprit present / "+prototype.key);
                var headsails=host.vessels[index].Find("Foremast and rigging").GetComponentsInChildren<CloseActionCanvas>(true).Where(c=>!c.square).ToArray();Check(headsails.Length==3&&headsails.All(c=>c.gameObject.activeInHierarchy),"three separate headsails at medium sail / "+prototype.key);
                host.target=host.vessels[index].TransformPoint(new Vector3(0,spec.Deck+12,spec.Length*.48f));host.distance=spec.Length*1.1f;host.pitch=12;host.yaw=host.scenario.ships[index].heading+90;host.SetCamera();
                yield return new WaitForEndOfFrame();CaptureSmoke("CloseAction-headsails-"+prototype.key+".png");
                Check(host.vessels[index].Find("Closed hull interior")!=null&&host.vessels[index].Find("Attached bow assembly/Ornate bow carvings")!=null&&host.vessels[index].Find("Attached bow assembly/Attached beakhead framing")!=null,"interior and bow carvings present / "+prototype.key);
                CaptureGunport(host.vessels[index],spec,"CloseAction-gunport-"+prototype.key+".png");
                host.target=host.vessels[index].TransformPoint(new Vector3(0,spec.Deck+1.8f,spec.Length*.535f));host.distance=spec.Length*.48f;host.pitch=12;host.yaw=host.scenario.ships[index].heading+65;host.SetCamera();
                yield return new WaitForEndOfFrame();CaptureSmoke("CloseAction-carvings-"+prototype.key+".png");
                host.yaw=host.scenario.ships[index].heading+90;host.pitch=3;host.SetCamera();yield return new WaitForEndOfFrame();CaptureSmoke("CloseAction-bow-side-"+prototype.key+".png");
                host.pitch=65;host.SetCamera();yield return new WaitForEndOfFrame();CaptureSmoke("CloseAction-bow-top-"+prototype.key+".png");
                host.pitch=6;host.yaw=host.scenario.ships[index].heading+180;host.distance=spec.Length*.65f;host.SetCamera();yield return new WaitForEndOfFrame();CaptureSmoke("CloseAction-bow-front-"+prototype.key+".png");host.scenario.closeAction.logs[index].sail=oldSail;host.UpdateWorld();
            }
            host.Focus(2);host.distance=CloseActionScenario.Spec(host.scenario,2).Length*1.9f;host.pitch=17;host.yaw=125;host.SetCamera();
            int openingSail=host.scenario.closeAction.logs[2].sail;host.scenario.closeAction.logs[2].sail=3;float previousFullness=0;
            foreach(int strength in new[]{1,2,3,4}){
                host.closeAction.SetWindNow(3,strength);host.UpdateWorld();
                var canvas=host.vessels[2].GetComponentsInChildren<CloseActionCanvas>().First(c=>c.square);var block=new MaterialPropertyBlock();canvas.GetComponent<Renderer>().GetPropertyBlock(block);float fullness=Mathf.Abs(block.GetVector("_CanvasWind").x);
                Check(fullness>previousFullness,"sail fullness increases through wind strength "+strength);previousFullness=fullness;
                yield return new WaitForEndOfFrame();CaptureSmoke("CloseAction-canvas-WS"+strength+".png");
            }
            host.scenario.closeAction.logs[2].sail=openingSail;host.closeAction.SetWindNow(3,3);host.UpdateWorld();
            Check(host.closeAction.SetWindNow(2,2),"host applies weather before play");host.scenario.cloud=.8f;host.scenario.rain=.4f;host.scenario.visibilityKm=6;
            Check(host.vessels.Length==23&&host.vessels[16].GetComponentsInChildren<GunMount>(true).Length==120,"runtime fleet and Orient physical guns");
            Check(host.GetComponent<EngineExhaust>().funnels.All(p=>p==null),"no steam funnels in sailing module");
            Check(host.network.Host(0,0,"CATEST2026",true),"loopback host");Check(host.network.PlayerCapacity==32,"large fleet captain capacity");
            guest=new GameObject("Close Action verification guest").AddComponent<MaritimeScene>();guest.enabled=false;guest.soundEnabled=false;guest.GetComponent<UmpirePanel>().enabled=false;guest.GetComponent<RuleChart>().enabled=false;guest.GetComponent<GunneryTools>().enabled=false;guest.GetComponent<BroadsideOverlay>().enabled=false;guest.closeAction.enabled=false;guest.view.enabled=false;guest.view.GetComponent<AudioListener>().enabled=false;
            Check(guest.network.Join("127.0.0.1",host.network.Port,"CATEST2026"),"guest joins");yield return Until(()=>guest.network.Connected,"large fleet snapshot received");
            Check(guest.scenario.closeAction.windDirection==2&&guest.scenario.closeAction.wind==2&&Mathf.Abs(guest.scenario.rain-.4f)<.001f,"wind and environmental weather synchronize");
            Check(!guest.closeAction.SetWindNow(1,1),"guest cannot change shared weather");Check(host.closeAction.SetWindNow(3,3),"host restores opening wind");
            int player=guest.network.LocalPlayer;Check(host.network.AssignShip(10,player),"assign French captain");guest.network.SetReady(true);yield return Until(()=>host.network.Lobby.players.Any(p=>p.id==player&&p.ready),"guest ready");Check(host.network.StartMatch(),"match starts");yield return Until(()=>guest.network.Lobby.running,"guest match synchronization");
            host.scenario.closeAction.scatterComplete=true;Check(host.closeAction.Advance(),"plot phase");yield return Until(()=>guest.scenario.closeAction.phase==1,"guest plot phase");
            Check(guest.closeAction.Submit(new CloseActionOrder{ship=10,plot="3"}),"guest sends private plot");yield return Until(()=>host.closeAction.drafts.ContainsKey(10),"host receives private plot");
            Check(host.scenario.closeAction.revealed.Length==0&&guest.scenario.closeAction.revealed.Length==0,"sealed movement absent from public snapshots");
            Check(host.closeAction.SubmitSignal(0,"Fleet","CLOSE THE LINE"),"British signal sealed");Check(guest.closeAction.SubmitSignal(10,"Fleet","HOLD THE LINE"),"French signal sent");yield return new WaitForSeconds(.5f);
            host.closeAction.FillMissing();Check(host.closeAction.Advance(),"reveal complete fleet plots");yield return Until(()=>guest.scenario.closeAction.revealed.Length==23,"guest sees revealed orders");
            for(int i=0;i<3;i++)Check(host.closeAction.StepMovement(),"simultaneous segment "+i);Check(host.closeAction.Advance(),"combat phase");Check(host.closeAction.Advance(),"maintenance phase");host.scenario.closeAction.pendingDirection=6;host.scenario.closeAction.pendingWind=4;for(int i=0;i<7;i++)host.scenario.closeAction.maintenance[i]=true;Check(host.closeAction.Advance(),"finish turn");
            yield return Until(()=>guest.scenario.turn==2,"guest receives next turn");yield return Until(()=>guest.closeAction.inbox.Count>0,"friendly signal delivery");Check(guest.closeAction.inbox.All(s=>s.side==1&&s.text!="CLOSE THE LINE"),"enemy signal excluded from recipient packets");
            Check(guest.scenario.closeAction.windDirection==6&&guest.scenario.closeAction.wind==4,"queued wind applies and synchronizes next turn");
            host.network.Stop();guest.network.Stop();Destroy(guest.gameObject);host.StartCloseAction();host.GetComponent<UmpirePanel>().ShowCloseActionTab(2);host.Focus(16);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output)),"CloseAction-gunnery.png"));
            host.StartCloseAction();host.closeAction.SetWindNow(4,1);host.Focus(0);host.distance=230;host.pitch=24;host.yaw=70;host.SetCamera();
            var effects=host.GetComponent<BatteryEffects>();Check(effects.FireCloseActionLocal(0,1),"powder broadside starts");yield return new WaitForSeconds(5.5f);
            yield return Until(()=>!effects.firing[0],"all guns finish before smoke aging");
            var smoke=new ParticleSystem.Particle[effects.powder.main.maxParticles];int initial=effects.powder.GetParticles(smoke);
            Check(initial>500&&smoke.Take(initial).Max(p=>p.startLifetime)>150,"dense long-lived powder emitted");
            Check(smoke.Take(initial).Max(p=>p.startSize)<2,"sailing-gun puff diameter matches the smaller battery scale");
            Check(smoke.Take(initial).Max(p=>p.startColor.r)-smoke.Take(initial).Min(p=>p.startColor.r)>70,"smoke retains varied grey tones");
            yield return new WaitForEndOfFrame();CaptureSmoke("CloseAction-smoke-5s.png");
            effects.enabled=false;effects.powder.Pause();
            for(int step=0;step<720;step++){
                effects.powder.Simulate(.25f,false,false,true);effects.AdvectSmoke(.25f,5.5f+step*.25f);
                if(step==159||step==359){
                    int count=effects.powder.GetParticles(smoke);Check(count>initial*.5f,"smoke persists at "+(step==159?45:95)+" seconds");
                    Vector3 center=Vector3.zero;for(int p=0;p<count;p++)center+=smoke[p].position;host.target=center/count;host.SetCamera();
                    yield return new WaitForEndOfFrame();CaptureSmoke(step==159?"CloseAction-smoke-45s.png":"CloseAction-smoke-95s.png");
                }
            }
            int remaining=effects.powder.GetParticles(smoke);File.AppendAllText(output,"Smoke age diagnostics: remaining "+remaining+"; max lifetime left "+(remaining==0?0:smoke.Take(remaining).Max(p=>p.remainingLifetime))+"; system time "+effects.powder.time+"\n");
            Check(remaining==0,"old smoke eventually disperses");effects.enabled=true;
            foreach(int ship in new[]{4,16}){
                effects.Clear();host.Focus(ship);host.distance=CloseActionScenario.Spec(host.scenario,ship).Length*3.1f;host.pitch=22;host.yaw=70;host.SetCamera();
                Check(effects.FireCloseActionLocal(ship,1),"scale comparison battery starts / ship "+ship);
                yield return Until(()=>!effects.firing[ship],"scale comparison broadside complete / ship "+ship);
                yield return new WaitForEndOfFrame();CaptureSmoke(ship==4?"CloseAction-smoke-Leander.png":"CloseAction-smoke-Orient.png");
            }
            yield return new WaitForSeconds(.5f);File.AppendAllText(output,"ALL PASS\n");Application.Quit(0);
        }
        void CaptureSmoke(string name)
        {
            // Read this completed frame before accelerated simulation advances the cloud again.
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output)),name),image.EncodeToPNG());Destroy(image);
        }
        void CaptureGunport(Transform root,CloseActionSpec spec,string name)
        {
            var camera=host.view;Vector3 oldPosition=camera.transform.position;Quaternion oldRotation=camera.transform.rotation;float oldNear=camera.nearClipPlane,oldAspect=camera.aspect,oldFov=camera.fieldOfView;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            int ports=spec.rate==120?16:spec.rate==50?11:spec.rate==80?15:14;float z=Mathf.Lerp(-spec.Length*.37f,spec.Length*.37f,(ports/2)/(float)(ports-1));Vector3 wall=Shipwright.CloseActionHullPoint(spec,z,1.55f,1);
            camera.transform.position=root.TransformPoint(wall+new Vector3(1.3f,.25f,.12f));camera.transform.LookAt(root.TransformPoint(wall+new Vector3(-3,-.65f,0)),root.up);camera.nearClipPlane=.03f;camera.fieldOfView=62;
            var rt=new RenderTexture(1000,750,24);camera.targetTexture=rt;camera.aspect=4f/3;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1000,750,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1000,750),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output)),name),image.EncodeToPNG());
            camera.targetTexture=oldTarget;RenderTexture.active=oldActive;camera.nearClipPlane=oldNear;camera.aspect=oldAspect;camera.fieldOfView=oldFov;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);Destroy(image);Destroy(rt);
        }
    }
}
