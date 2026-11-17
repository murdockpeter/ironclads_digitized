using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Ironclads
{
    // Runs in independent standalone processes; uses real sockets and the same APIs as the lobby UI.
    public class LobbyVerification : MonoBehaviour
    {
        MaritimeScene scene;MultiplayerSession net;RuleChart chart;int role,count,port;string output,mode;
        readonly List<string> errors=new List<string>();
        void OnEnable()=>Application.logMessageReceived+=Log;
        void OnDisable()=>Application.logMessageReceived-=Log;
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void Check(bool ok,string message){if(!ok){errors.Add(message);Debug.LogWarning("Lobby check: "+message);}}
        IEnumerator Wait(Func<bool> test,string reason)
        {
            float end=Time.realtimeSinceStartup+50;while(!test()&&Time.realtimeSinceStartup<end)yield return null;
            if(!test()){errors.Add("Timeout: "+reason+" / "+net.ConnectionStatus+" / player "+net.LocalPlayer+" / "+JsonUtility.ToJson(net.Lobby));Finish();yield return null;}
        }
        string FileAt(string name)=>Path.Combine(output,name);
        void Finish(){File.WriteAllText(FileAt("lobby-"+role+".txt"),errors.Count==0?"PASS: "+mode+" / player "+role+"; real multi-client lobby, assignments, ready/start, orders, engines/turrets, salvos, transfers, stale command rejection, reconnect and offline recovery.":"FAIL:\n"+string.Join("\n",errors));Application.Quit(errors.Count==0?0:1);}
        int Player(int captain)=>net.Lobby.players.FirstOrDefault(p=>p.name=="Captain"+captain)?.id??-1;
        TablePacket Command(int ship,int epoch,int sequence,float load)=>new TablePacket{type="ship",ship=ship,epoch=epoch,sequence=sequence,load=load,boilers=true,q=role*2,r=2,facing=1,turret=72+role,aftTurret=190+role,east=role*300+12.25f,north=500+role,heading=61.25f,speed=4,orderedSpeed=5,movementUsed=.5f,scenarioId=scene.scenario.scenarioId,rulesetId=scene.scenario.rulesetId,generation=net.Lobby.generation,turnNumber=scene.scenario.turn};
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int a=Array.IndexOf(args,"-verify-lobby");role=int.Parse(args[a+1]);port=int.Parse(args[a+2]);output=args[a+3];count=int.Parse(args[a+4]);mode=args[a+5];Directory.CreateDirectory(output);
            yield return new WaitForSeconds(1);scene=GetComponent<MaritimeScene>();net=scene.network;chart=GetComponent<RuleChart>();scene.soundEnabled=false;var fx=GetComponent<BatteryEffects>();fx.silentVerification=true;fx.ripple=.02f;net.PlayerName="Captain"+role;
            try{NetworkVerification.Framing();}catch(Exception e){Check(false,"framing: "+e.Message);}
            if(role==0)yield return Host();else yield return Guest();Finish();
        }
        IEnumerator Host()
        {
            if(mode=="james")scene.StartJames();else if(mode=="ulsan")scene.StartUlsan();else {scene.StartScenario(mode=="hampton"||mode=="ironfire");if(mode=="ironfire"){scene.scenario.rulesetId="iron-fire";scene.scenario.hexMode=false;}}
            Check(net.Host(port,-1,"LOBBYTEST01",true),"host startup");Check(!net.CanControl(0),"umpire must assign a ship before controlling it");File.WriteAllText(FileAt("host-ready.txt"),"ready");
            yield return Wait(()=>net.Lobby.players.Count(p=>p.connected)==count+1,"all guests connected");
            Check(!net.StartMatch(),"start before readiness accepted");
            yield return Wait(()=>net.Lobby.requests.Length==count,"initial control requests");
            foreach(var request in net.Lobby.requests.ToArray())Check(net.DecideRequest(request.id,true),"initial request approval");
            Check(net.Lobby.owners.Take(count).Distinct().Count()==count,"one commander per ship");scene.scenario.umpireNotes="ready";
            yield return Wait(()=>net.Lobby.players.All(p=>p.ready),"players ready");Check(net.StartMatch(),"start match");scene.scenario.windKnots=17;scene.scenario.windFrom=35;chart.NextTurn();scene.scenario.umpireNotes="orders";
            yield return Wait(()=>Enumerable.Range(1,count).All(i=>Mathf.Abs(scene.scenario.State(i-1).engineLoad-i*.1f)<.001f&&scene.scenario.State(i-1).turretHeading==72+i),"simultaneous ship orders");
            for(int i=1;i<=count;i++){
                var s=scene.scenario.State(i-1);Check(s.turretHeading==72+i&&s.aftTurretHeading==190+i,"turret state "+i);
                if(scene.scenario.Miniatures||scene.scenario.DoB)Check(s.east==i*300+12.25f&&s.heading==61.25f&&s.speed==4&&s.orderedSpeed==5&&s.movementUsed==.5f,"exact movement "+i);
                else Check(RuleChart.At(s.Position).Equals(new Hex(i*2,2)),"hex position "+i);
            }
            int shots=Enumerable.Range(0,count).Sum(i=>GetComponent<BatteryEffects>().Battery(i,2).Length);
            yield return Wait(()=>GetComponent<BatteryEffects>().ShotsFired==shots,"all salvos on host");scene.scenario.umpireNotes="request-denied";
            yield return Wait(()=>net.Lobby.requests.Any(r=>r.player==Player(1)&&r.ship==1),"first takeover request");
            var first=net.Lobby.requests.First(r=>r.player==Player(1)&&r.ship==1);net.DecideRequest(first.id,false);Check(net.OwnerFor(1)==Player(2),"decline changed owner");scene.scenario.umpireNotes="denied";
            yield return Wait(()=>net.Lobby.requests.Any(r=>r.player==Player(1)&&r.ship==1),"second takeover request");
            net.DecideRequest(net.Lobby.requests.First(r=>r.player==Player(1)&&r.ship==1).id,true);Check(net.OwnerFor(0)==Player(1)&&net.OwnerFor(1)==Player(1),"multi-ship command");scene.scenario.umpireNotes="transferred";
            yield return Wait(()=>Mathf.Abs(scene.scenario.State(1).engineLoad-.93f)<.001f,"new owner command");yield return new WaitForSeconds(.7f);Check(scene.scenario.State(1).engineLoad==.93f,"stale former owner command applied");
            net.AssignShip(1,Player(2));scene.scenario.umpireNotes="reconnect";
            yield return Wait(()=>net.Lobby.players.Any(p=>p.id==Player(2)&&!p.connected),"guest disconnect");
            Check(scene.scenario.State(1).engineLoad==.93f,"old epoch accepted after original commander regained ship");
            yield return Wait(()=>net.Lobby.players.Any(p=>p.id==Player(2)&&p.connected)&&Mathf.Abs(scene.scenario.State(1).engineLoad-.87f)<.001f,"authenticated reconnect and command");
            Check(net.Lobby.players.Length==count+1,"reconnect created duplicate identity");
            int generation=net.Lobby.generation;scene.scenario=Scenario.Parse(JsonUtility.ToJson(scene.scenario));
            yield return Wait(()=>net.Lobby.generation>generation,"loaded table generation invalidates queued orders");
            if(scene.scenario.James)scene.StartScenario(false);else scene.StartJames();
            yield return Wait(()=>!net.Lobby.running&&net.Lobby.owners.All(o=>o<0),"scenario switch resets lobby");
            scene.scenario.umpireNotes="switched";
            yield return Wait(()=>Enumerable.Range(1,count).All(i=>File.Exists(FileAt("switched-"+i+".txt"))),"all clients rebuilt switched scenario");
            scene.scenario.umpireNotes="shutdown";yield return new WaitForSeconds(1);
            net.Stop();Check(net.CanManage&&net.CanControl(0),"host offline recovery");yield return new WaitForSeconds(1);
        }
        IEnumerator Probe(bool full)
        {
            using(var probe=new DirectTcpTransport()){
                probe.Connect("127.0.0.1",port);float end=Time.realtimeSinceStartup+5;bool rejected=false;
                while(Time.realtimeSinceStartup<end&&!rejected){while(probe.Poll(out var n)){
                    if(n.kind=="connected")probe.Send(n.peer,JsonUtility.ToJson(new TablePacket{type="hello",code=full?"LOBBYTEST01":"WRONGCODE01",name="Captain2"}));
                    if(n.kind=="message"){var p=JsonUtility.FromJson<TablePacket>(n.text);if(p.type=="reject")rejected=true;if(p.type=="welcome")Check(false,"probe unexpectedly admitted");}
                }yield return null;}
                Check(rejected,full?"full lobby accepted extra identity":"wrong session code accepted");
            }
        }
        IEnumerator Guest()
        {
            if(role==1)yield return Probe(false);
            Check(net.Join("127.0.0.1",port,"LOBBYTEST01"),"join startup");yield return Wait(()=>net.Connected,"welcome");
            int id=net.LocalPlayer;Check(!net.CanControl(role-1),"unassigned guest has control");net.SetSide(role<=3?0:1);
            Check(net.RequestControl(role-1),"initial request");
            yield return Wait(()=>net.OwnerFor(role-1)==id,"approved initial assignment");Check(!net.CanControl(role-1),"orders allowed before start");
            yield return Wait(()=>scene.scenario.umpireNotes=="ready","ready phase");net.SetReady(true);
            yield return Wait(()=>scene.scenario.umpireNotes=="orders"&&net.Lobby.running,"match start");Check(net.CanControl(role-1)&&!net.CanManage,"commander permissions");Check(scene.scenario.turn==2&&scene.scenario.windKnots==17,"weather and clock");
            scene.selected=(role-1+1)%scene.scenario.ships.Length;var before=scene.scenario.State(scene.selected).Position;chart.Place(new Hex(19,0));Check(scene.scenario.State(scene.selected).Position==before,"UI edited another captain's ship");
            var cmd=Command(role-1,net.Lobby.epochs[role-1],1,role*.1f);var s=scene.scenario.State(role-1);
            if(scene.scenario.Miniatures||scene.scenario.DoB){s.east=cmd.east;s.north=cmd.north;s.heading=cmd.heading;s.speed=4;s.orderedSpeed=5;s.movementUsed=.5f;}
            else {scene.selected=role-1;chart.Place(new Hex(role*2,2));s.heading=60;}
            s.turretHeading=72+role;s.aftTurretHeading=190+role;s.engineLoad=role*.1f;net.SubmitShip(role-1);Check(net.RequestFire(role-1,2),"assigned salvo");
            int oldEpoch=net.Lobby.epochs[1];
            if(role==1&&count==6)yield return Probe(true);
            yield return Wait(()=>scene.scenario.umpireNotes=="request-denied","takeover phase");
            if(role==1)net.RequestControl(1);
            yield return Wait(()=>scene.scenario.umpireNotes=="denied","decline replicated");Check(net.OwnerFor(1)==Player(2),"decline changed ownership on guest");
            if(role==1)net.RequestControl(1);
            yield return Wait(()=>scene.scenario.umpireNotes=="transferred","transfer replicated");
            if(role==1){Check(net.CanControl(0)&&net.CanControl(1),"multiple assigned ships not controllable");scene.scenario.State(1).engineLoad=.93f;net.SubmitShip(1);}
            if(role==2){Check(!net.CanControl(1)&&!net.RequestFire(1,2),"former commander retained control");net.VerificationSend(Command(1,oldEpoch,900,.01f));}
            yield return Wait(()=>scene.scenario.umpireNotes=="reconnect","reconnect phase");
            if(role==2){net.VerificationSend(Command(1,oldEpoch,901,.02f));yield return new WaitForSeconds(.25f);net.VerificationSend(new TablePacket{type="state",state=JsonUtility.ToJson(new Scenario())});yield return Wait(()=>!net.Active,"guest-authored world snapshot rejected");Check(net.CanManage,"rejected guest retains offline table");yield return new WaitForSeconds(.7f);net.Join("127.0.0.1",port,"LOBBYTEST01");yield return Wait(()=>net.Connected,"reconnect welcome");Check(net.LocalPlayer==id&&net.OwnerFor(1)==id&&net.CanControl(1),"reconnect failed to restore reserved identity");scene.scenario.State(1).engineLoad=.87f;net.SubmitShip(1);}
            yield return Wait(()=>scene.scenario.umpireNotes=="switched","scenario switched");
            scene.UpdateWorld();Check(!net.Lobby.running&&!net.CanControl(0)&&net.Lobby.owners.All(o=>o<0)&&scene.vessels.Length==scene.scenario.ships.Length,"scenario change resets ownership and rebuilds models");File.WriteAllText(FileAt("switched-"+role+".txt"),"ok");
            yield return Wait(()=>scene.scenario.umpireNotes=="shutdown","host shutdown phase");
            yield return Wait(()=>!net.Active,"host shutdown recovery");Check(net.CanManage&&net.CanControl(0)&&scene.scenario.turn==1,"retained offline table");
        }
    }
}
