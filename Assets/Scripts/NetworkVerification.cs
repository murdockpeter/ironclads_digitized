using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    // Two real standalone processes exercise the actual session/ownership/UI command paths.
    public class NetworkVerification : MonoBehaviour
    {
        readonly List<string> errors=new List<string>();
        MaritimeScene scene;MultiplayerSession net;RuleChart chart;string output;bool host;
        void OnEnable()=>Application.logMessageReceived+=Log;
        void OnDisable()=>Application.logMessageReceived-=Log;
        void Log(string text,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        void Check(bool ok,string text){if(!ok)errors.Add(text);}
        IEnumerator Until(Func<bool> test,string reason)
        {
            float end=Time.realtimeSinceStartup+18;while(!test()&&Time.realtimeSinceStartup<end)yield return null;
            Check(test(),"Timeout: "+reason);
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-verify-network");if(index<0)yield break;
            host=args[index+1]=="host";int port=int.Parse(args[index+2]);output=args[index+3];Directory.CreateDirectory(output);
            yield return new WaitForSeconds(1);
            scene=FindFirstObjectByType<MaritimeScene>();net=scene.network;chart=scene.GetComponent<RuleChart>();scene.GetComponent<BatteryEffects>().silentVerification=true;scene.soundEnabled=false;
            try{Framing();}catch(Exception e){errors.Add("TCP framing: "+e.Message);}
            if(host)yield return Host(port);else yield return Guest(port);
            net.Stop();
            File.WriteAllText(Path.Combine(output,host?"network-host.txt":"network-guest.txt"),errors.Count==0?"PASS: "+(host?"host":"guest")+"; real TCP connection, code rejection, ship assignment/ownership, shared weather/time, ship and engine replication, salvos, opponent-edit rejection, reconnect, third-player rejection and offline recovery.":"FAIL:\n"+string.Join("\n",errors));
            Application.Quit(errors.Count==0?0:1);
        }
        IEnumerator Host(int port)
        {
            Check(net.Host(port,0,"TESTSESSION01",true),"Host startup failed.");
            yield return Until(()=>net.Connected,"guest handshake");
            scene.selected=0;chart.Place(new Hex(-2,1));chart.Turn(1);chart.NextTurn();scene.scenario.windFrom=45;scene.scenario.windKnots=18;scene.scenario.umpireNotes="net:phase1";
            yield return Until(()=>RuleChart.At(scene.scenario.ships[1].Position).Equals(new Hex(3,-1))&&Mathf.Abs(scene.scenario.ships[1].engineLoad-.8f)<.001f,"guest ship and engines");
            Check(RuleChart.At(scene.scenario.ships[0].Position).Equals(new Hex(-2,1)),"Guest forged an edit to host's ship.");
            Check(RuleChart.Facing(scene.scenario.ships[1].heading)==5,"Guest facing not replicated.");
            yield return Until(()=>scene.GetComponent<BatteryEffects>().ShotsFired>=1,"guest salvo on host");
            Check(net.RequestFire(0,1),"Host broadside rejected.");yield return new WaitForSeconds(2);
            scene.scenario.umpireNotes="net:phase2";
            yield return Until(()=>!net.Connected,"guest disconnect");
            yield return Until(()=>net.Connected,"guest reconnect");
            yield return Until(()=>!scene.scenario.ships[1].boilersLit,"reconnected guest engine order");
            scene.scenario.umpireNotes="net:phase3";yield return new WaitForSeconds(2);
            Check(scene.GetComponent<BatteryEffects>().ShotsFired==6,"Salvos duplicated or lost on host.");
        }
        IEnumerator Guest(int port)
        {
            Check(net.Join("127.0.0.1",port,"WRONGCODE01"),"Wrong-code probe failed to start.");
            yield return Until(()=>!net.Active,"wrong-code rejection");Check(!net.Connected,"Wrong code was accepted.");yield return new WaitForSeconds(.3f);
            net.Join("127.0.0.1",port,"TESTSESSION01");yield return Until(()=>net.Connected,"guest welcome");Check(net.LocalShip==1,"Guest did not receive Stonewall.");
            yield return Until(()=>scene.scenario.umpireNotes=="net:phase1","host weather/turn snapshot");
            Check(RuleChart.At(scene.scenario.ships[0].Position).Equals(new Hex(-2,1))&&scene.scenario.ships[0].heading==60,"Host move/facing not replicated.");
            Check(scene.scenario.turn==2&&scene.scenario.windKnots==18&&scene.scenario.windFrom==45,"Host clock/weather not replicated.");
            scene.selected=0;chart.Place(new Hex(8,0));chart.NextTurn();scene.GetComponent<EngineExhaust>().SetOrder(0,0,false);
            Check(RuleChart.At(scene.scenario.ships[0].Position).Equals(new Hex(-2,1))&&scene.scenario.turn==2&&scene.scenario.ships[0].boilersLit,"Guest UI commands changed host-owned state.");
            net.VerificationSend(new TablePacket{type="ship",ship=0,q=10,r=0,facing=2,load=1,boilers=true,sequence=999});
            scene.selected=1;chart.Place(new Hex(3,-1));chart.Turn(1);scene.GetComponent<EngineExhaust>().SetOrder(1,.8f);net.RequestFire(1,0);
            using(var third=new DirectTcpTransport()) {
                third.Connect("127.0.0.1",port);yield return new WaitForSeconds(.8f);bool closed=false;
                while(third.Poll(out var notice)){if(notice.kind=="connected")third.Send(notice.peer,JsonUtility.ToJson(new TablePacket{type="hello",code="TESTSESSION01"}));if(notice.kind=="closed"||notice.kind=="error")closed=true;}
                Check(closed&&net.Connected,"Third connection displaced a player or remained open.");
            }
            yield return Until(()=>scene.scenario.umpireNotes=="net:phase2","host salvo complete");
            yield return Until(()=>scene.GetComponent<BatteryEffects>().ShotsFired==6,"six shared gun discharges");
            Check(RuleChart.At(scene.scenario.ships[1].Position).Equals(new Hex(3,-1))&&Mathf.Abs(scene.scenario.ships[1].engineLoad-.8f)<.001f,"Guest edits were overwritten before acknowledgement.");
            net.VerificationSend(new TablePacket{type="state",state=JsonUtility.ToJson(new Scenario())});
            yield return Until(()=>!net.Active,"host rejects a guest-authored world snapshot");
            Check(net.CanManage&&net.CanControl(0),"Offline control did not return after disconnect.");yield return new WaitForSeconds(1);
            net.Join("127.0.0.1",port,"TESTSESSION01");yield return Until(()=>net.Connected,"rejoin handshake");
            scene.GetComponent<EngineExhaust>().SetOrder(1,0,false);yield return Until(()=>scene.scenario.umpireNotes=="net:phase3","reconnected command acknowledgement");
        }
        static void Framing()
        {
            byte[] payload=System.Text.Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");using(var stream=new MemoryStream()) {
                DirectTcpTransport.WriteFrame(stream,payload);stream.Position=0;using(var fragmented=new Fragmented(stream))if(System.Text.Encoding.UTF8.GetString(DirectTcpTransport.ReadFrame(fragmented))!="{\"type\":\"ping\"}")throw new Exception("Fragmented frame changed.");
            }
            bool rejected=false;try{DirectTcpTransport.ReadFrame(new MemoryStream(new byte[]{0x7f,0xff,0xff,0xff}));}catch(InvalidDataException){rejected=true;}if(!rejected)throw new Exception("Oversized frame accepted.");
            if(MultiplayerSession.ValidShip(new TablePacket{ship=1,q=10000,r=0,load=.5f})||MultiplayerSession.ValidShip(new TablePacket{ship=1,load=float.NaN}))throw new Exception("Invalid ship coordinates/load accepted.");
        }
        sealed class Fragmented : Stream
        {
            readonly Stream source;public Fragmented(Stream source){this.source=source;}
            public override int Read(byte[] buffer,int offset,int count)=>source.Read(buffer,offset,Math.Min(2,count));
            public override bool CanRead=>true;public override bool CanSeek=>false;public override bool CanWrite=>false;public override long Length=>source.Length;
            public override long Position {get=>source.Position;set=>throw new NotSupportedException();}
            public override void Flush(){}public override long Seek(long offset,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long value)=>throw new NotSupportedException();public override void Write(byte[] buffer,int offset,int count)=>throw new NotSupportedException();
        }
    }
}
