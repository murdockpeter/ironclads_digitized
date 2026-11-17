using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public class LobbyPlayer
    {
        public int id,side=-1;
        public string name;
        public bool connected,ready;
    }
    [Serializable] public class ControlRequest { public int id,player,ship; }
    [Serializable] public class TableLobby
    {
        public bool running;
        public int generation;
        public string scenarioId,rulesetId,message="Choose ships, mark ready, then the host starts the match.";
        public LobbyPlayer[] players=Array.Empty<LobbyPlayer>();
        public int[] owners=Array.Empty<int>(),epochs=Array.Empty<int>();
        public ControlRequest[] requests=Array.Empty<ControlRequest>();
    }
    [Serializable] public class TablePacket
    {
        public int protocol=7,ship,q,r,facing,sequence,ack,side,player,epoch,generation,request;
        public float load,turret,aftTurret,dobMoved,east,north,heading,speed,orderedSpeed,movementUsed;
        public int turnClass=3,turnNumber;
        public bool astern,orderedAstern,boilers,ready;
        public string type,code,state,scenarioId,rulesetId,name,token;
        public TableLobby lobby;
    }
    public class MultiplayerSession : MonoBehaviour
    {
        public const int Protocol=7,MaxPlayers=7;
        public MaritimeScene scene;
        public bool Active {get;private set;}
        public bool IsHost {get;private set;}
        public bool Connected {get;private set;}
        public int LocalPlayer {get;private set;}=-1;
        public int LocalShip {get {if(LocalPlayer<0)return -1;for(int i=0;i<Lobby.owners.Length;i++)if(Lobby.owners[i]==LocalPlayer)return i;return -1;}}
        public int Port=>transport?.Port??0;
        public string PlayerName="Captain";
        public string ConnectionStatus {get;private set;}="Offline - umpire controls the table.";
        public string SessionCode {get;private set;}="";
        public string HostAddresses {get;private set;}="";
        public TableLobby Lobby {get;private set;}=new TableLobby();
        public bool CanManage=>!Active||IsHost;
        public static int Owner(int ship)=>ship==2?0:ship==3?1:ship;
        public int OwnerFor(int ship)=>ship>=0&&ship<Lobby.owners.Length?Lobby.owners[ship]:-1;
        public bool CanControl(int ship)=>ship>=0&&ship<scene.scenario.ShipSlots&&(!Active||Lobby.running&&Lobby.scenarioId==scene.scenario.scenarioId&&Lobby.rulesetId==scene.scenario.rulesetId&&(IsHost||Connected)&&OwnerFor(ship)==LocalPlayer);
        public string PlayerLabel(int id)=>id<0?"Unassigned":(Lobby.players.FirstOrDefault(p=>p.id==id)?.name??"Unknown")+" #"+id;
        public int ShipSide(int ship)=>scene.scenario.James?(ship<3?0:1):scene.scenario.DoB?UlsanScenario.Ships[ship].side:Owner(ship);
        sealed class Client
        {
            public int peer,player=-1,ack,count;
            public float heard,since,rateStart,rejectAt,lastSend;
            public string lastState;
        }
        DirectTcpTransport transport;
        readonly Dictionary<int,Client> clients=new Dictionary<int,Client>();
        readonly Dictionary<int,string> tokens=new Dictionary<int,string>();
        readonly Dictionary<int,TablePacket> pending=new Dictionary<int,TablePacket>();
        readonly Dictionary<int,int> sentSequence=new Dictionary<int,int>();
        int serverPeer,sequence,nextPlayer=1,nextRequest=1;
        float heard,lastSend,lastShipSend=-1;
        bool applying;
        Scenario hostedTable;
        string reconnectKey="",reconnectToken="";
        readonly float[] nextFire=new float[16];
        public static string NewCode()=>Guid.NewGuid().ToString("N").Substring(0,12).ToUpperInvariant();
        static string CleanName(string name)=>new string((name??"Captain").Where(c=>!char.IsControl(c)&&c!='<'&&c!='>').Take(24).ToArray()).Trim();
        public bool Host(int port,int ship,string code,bool loopbackOnly=false)
        {
            Stop();
            if(ship< -1||ship>=scene.scenario.ShipSlots||port<0||port>65535||(port==0&&!loopbackOnly)||string.IsNullOrWhiteSpace(code)||code.Length<6||code.Length>64){ConnectionStatus="Choose a valid port and a session code of 6-64 characters.";return false;}
            try {
                transport=new DirectTcpTransport();transport.Listen(port,loopbackOnly);Active=true;IsHost=true;LocalPlayer=0;SessionCode=code;
                Lobby=new TableLobby{players=new[]{new LobbyPlayer{id=0,name=CleanName(PlayerName)+" (host)",connected=true,ready=true}}};
                ResetAssignments();if(ship>=0)Lobby.owners[ship]=0;
                scene.animateTime=false;if(!scene.scenario.Miniatures&&!scene.scenario.DoB){scene.scenario.hexMode=true;scene.GetComponent<RuleChart>().Snap();foreach(var vessel in scene.scenario.MovingShips)vessel.speed=0;}
                hostedTable=scene.scenario;HostAddresses=FindAddresses();ConnectionStatus="Lobby open on TCP "+Port+". Up to six guests can join.";return true;
            }catch{Stop();ConnectionStatus="Could not host on that TCP port. It may already be in use.";return false;}
        }
        public bool Join(string address,int port,string code)
        {
            Stop();
            if(!IPAddress.TryParse(address,out var ip)||ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork||port<1||port>65535||string.IsNullOrWhiteSpace(code)||code.Length<6||code.Length>64){ConnectionStatus="Enter an IPv4 address, TCP port, and 6-64 character session code.";return false;}
            string key=address+":"+port+":"+code;if(key!=reconnectKey){reconnectToken="";reconnectKey=key;}
            Active=true;SessionCode=code;heard=Time.realtimeSinceStartup;scene.animateTime=false;transport=new DirectTcpTransport();transport.Connect(address,port);ConnectionStatus="Connecting to "+address+":"+port+"...";return true;
        }
        public void Stop()
        {
            transport?.Dispose();transport=null;Active=Connected=IsHost=false;serverPeer=sequence=0;LocalPlayer=-1;
            clients.Clear();tokens.Clear();pending.Clear();sentSequence.Clear();nextPlayer=nextRequest=1;Lobby=new TableLobby();
            hostedTable=null;Array.Clear(nextFire,0,nextFire.Length);lastSend=0;lastShipSend=-1;ConnectionStatus="Offline - last table retained.";
        }
        void ResetAssignments()
        {
            Lobby.generation++;Lobby.scenarioId=scene.scenario.scenarioId;Lobby.rulesetId=scene.scenario.rulesetId;
            Lobby.owners=Enumerable.Repeat(-1,scene.scenario.ShipSlots).ToArray();Lobby.epochs=new int[scene.scenario.ShipSlots];Lobby.requests=Array.Empty<ControlRequest>();Lobby.running=false;
            foreach(var p in Lobby.players)p.ready=p.id==0;
            Array.Clear(nextFire,0,nextFire.Length);Lobby.message="Choose ships and mark ready.";
        }
        bool SendTo(int peer,TablePacket packet)=>transport!=null&&transport.Send(peer,JsonUtility.ToJson(packet));
        bool Send(TablePacket packet)=>SendTo(serverPeer,packet);
        void Broadcast(TablePacket packet){foreach(var c in clients.Values.Where(c=>c.player>=0&&c.rejectAt==0).ToArray())SendTo(c.peer,packet);}
        void SendSnapshot(Client c,string type="state",string token=null)
        {
            SendTo(c.peer,new TablePacket{type=type,state=JsonUtility.ToJson(scene.scenario),lobby=Lobby,player=c.player,ack=c.ack,token=token});
            c.lastSend=Time.realtimeSinceStartup;
        }
        void Publish(){foreach(var c in clients.Values.Where(c=>c.player>=0&&c.rejectAt==0).ToArray()){SendSnapshot(c);c.lastState=null;}}
        public void SetReady(bool ready)
        {
            if(!Active)return;if(!IsHost){Send(new TablePacket{type="ready",ready=ready});return;}
            Lobby.players[0].ready=ready;Publish();
        }
        public void SetSide(int side)
        {
            if(side< -1||side>1||!Active)return;
            if(IsHost){Lobby.players[0].side=side;Publish();}else Send(new TablePacket{type="side",side=side});
        }
        public bool StartMatch()
        {
            if(!IsHost||!Active||Lobby.running)return false;
            if(Lobby.players.Any(p=>p.connected&&!p.ready)||!Lobby.owners.Any(o=>o>=0)){Lobby.message="Assign at least one ship and have every connected player mark ready.";Publish();return false;}
            Lobby.running=true;Lobby.message="Match started. Control requests remain available during play.";Publish();return true;
        }
        public void ReturnToLobby(){if(!IsHost)return;Lobby.running=false;Lobby.generation++;foreach(var p in Lobby.players)p.ready=p.id==0;Lobby.message="Returned to lobby.";Publish();}
        public bool RequestControl(int ship)
        {
            if(!Active||ship<0||ship>=Lobby.owners.Length||OwnerFor(ship)==LocalPlayer)return false;
            if(IsHost)return AssignShip(ship,0);
            return Connected&&Send(new TablePacket{type="control",ship=ship,generation=Lobby.generation});
        }
        void AddRequest(int player,int ship)
        {
            if(ship<0||ship>=Lobby.owners.Length||OwnerFor(ship)==player||Lobby.requests.Any(r=>r.player==player&&r.ship==ship))return;
            Lobby.requests=Lobby.requests.Concat(new[]{new ControlRequest{id=nextRequest++,player=player,ship=ship}}).ToArray();
            Lobby.message=PlayerLabel(player)+" requests "+scene.scenario.ShortName(ship)+".";Publish();
        }
        public bool DecideRequest(int id,bool approve)
        {
            if(!IsHost)return false;var request=Lobby.requests.FirstOrDefault(r=>r.id==id);if(request==null)return false;
            Lobby.requests=Lobby.requests.Where(r=>r.id!=id).ToArray();
            if(approve)return AssignShip(request.ship,request.player);
            Lobby.message="Host declined "+PlayerLabel(request.player)+"'s request for "+scene.scenario.ShortName(request.ship)+".";Publish();return true;
        }
        public bool AssignShip(int ship,int player)
        {
            if(!IsHost||ship<0||ship>=Lobby.owners.Length||player< -1||player>=0&&!Lobby.players.Any(p=>p.id==player&&p.connected))return false;
            Lobby.owners[ship]=player;Lobby.epochs[ship]++;pending.Remove(ship);sentSequence.Remove(ship);
            Lobby.requests=Lobby.requests.Where(r=>r.ship!=ship).ToArray();
            if(!Lobby.running)foreach(var p in Lobby.players)p.ready=p.id==0;
            Lobby.message=scene.scenario.ShortName(ship)+": control assigned to "+PlayerLabel(player)+".";Publish();return true;
        }
        public void KickPlayer(int player)
        {
            if(!IsHost||player<=0)return;
            foreach(var c in clients.Values.Where(c=>c.player==player).ToArray()){c.player=-1;transport.Drop(c.peer);}
            for(int i=0;i<Lobby.owners.Length;i++)if(Lobby.owners[i]==player){Lobby.owners[i]=-1;Lobby.epochs[i]++;}
            Lobby.players=Lobby.players.Where(p=>p.id!=player).ToArray();Lobby.requests=Lobby.requests.Where(r=>r.player!=player).ToArray();tokens.Remove(player);Connected=Lobby.players.Any(p=>p.id>0&&p.connected);Publish();
        }
        public void SubmitShip(int ship)
        {
            if(!scene.scenario.Mobile(ship)||!Active||IsHost||applying||!CanControl(ship))return;
            var vessel=scene.scenario.State(ship);var hex=RuleChart.At(vessel.Position);
            pending[ship]=new TablePacket{type="ship",ship=ship,q=hex.q,r=hex.r,facing=RuleChart.Facing(vessel.heading),load=vessel.engineLoad,boilers=vessel.boilersLit,turret=vessel.turretHeading,aftTurret=vessel.aftTurretHeading,dobMoved=vessel.dobMovedSeconds,scenarioId=scene.scenario.scenarioId,rulesetId=scene.scenario.rulesetId,east=vessel.east,north=vessel.north,heading=vessel.heading,speed=vessel.speed,orderedSpeed=vessel.orderedSpeed,movementUsed=vessel.movementUsed,turnClass=vessel.turnClass,astern=vessel.astern,orderedAstern=vessel.orderedAstern,turnNumber=scene.scenario.turn,sequence=++sequence,epoch=Lobby.epochs[ship],generation=Lobby.generation};
            SendPending(false);
        }
        void SendPending(bool force)
        {
            if(pending.Count==0||(!force&&Time.realtimeSinceStartup-lastShipSend<.1f))return;
            foreach(var command in pending.Values.OrderBy(p=>p.sequence))if((!sentSequence.TryGetValue(command.ship,out int sent)||command.sequence>sent)&&Send(command))sentSequence[command.ship]=command.sequence;
            lastShipSend=Time.realtimeSinceStartup;
        }
        public bool RequestFire(int ship,int side)
        {
            if(!Active)return scene.GetComponent<BatteryEffects>().FireLocal(ship,side);
            if(!CanControl(ship)){scene.status="You can fire only ships assigned to you during a match.";return false;}
            if(IsHost)return BroadcastFire(ship,side);
            SendPending(true);return Send(new TablePacket{type="fire",ship=ship,side=side,scenarioId=scene.scenario.scenarioId,rulesetId=scene.scenario.rulesetId,turnNumber=scene.scenario.turn,epoch=Lobby.epochs[ship],generation=Lobby.generation});
        }
        bool BroadcastFire(int ship,int side)
        {
            if(ship<0||ship>=scene.scenario.ShipSlots||side< -1||side>3)return false;
            scene.UpdateWorld();var effects=scene.GetComponent<BatteryEffects>();int count=effects.Battery(ship,side).Length;
            if(count==0||Time.realtimeSinceStartup<nextFire[ship])return false;
            if(!scene.globeMode&&!effects.FireLocal(ship,side))return false;
            nextFire[ship]=Time.realtimeSinceStartup+count*effects.ripple*1.12f;
            Publish();Broadcast(new TablePacket{type="fire",ship=ship,side=side});return true;
        }
        internal bool VerificationSend(TablePacket packet)=>Send(packet);
        bool Authorized(Client c,TablePacket p)=>Lobby.running&&p.ship>=0&&p.ship<Lobby.owners.Length&&OwnerFor(p.ship)==c.player&&p.epoch==Lobby.epochs[p.ship]&&p.generation==Lobby.generation&&p.scenarioId==scene.scenario.scenarioId&&p.rulesetId==scene.scenario.rulesetId&&p.turnNumber==scene.scenario.turn;
        void Update()
        {
            if(!Active||transport==null)return;
            if(transport.Overflowed){Stop();ConnectionStatus="Network backlog exceeded the session limit. Reconnect to continue.";return;}
            if(IsHost&&(hostedTable!=scene.scenario||Lobby.scenarioId!=scene.scenario.scenarioId||Lobby.rulesetId!=scene.scenario.rulesetId)){
                if(Lobby.scenarioId!=scene.scenario.scenarioId||Lobby.rulesetId!=scene.scenario.rulesetId)ResetAssignments();else Lobby.generation++;
                hostedTable=scene.scenario;Publish();
            }
            float now=Time.realtimeSinceStartup;
            for(int i=0;i<128&&transport!=null&&transport.Poll(out var notice);i++) {
                if(notice.kind=="error"){string message=notice.text;Stop();ConnectionStatus=message;break;}
                if(notice.kind=="connected") {
                    if(IsHost)clients[notice.peer]=new Client{peer=notice.peer,since=now,heard=now,rateStart=now};
                    else {serverPeer=notice.peer;heard=now;Send(new TablePacket{type="hello",code=SessionCode,name=CleanName(PlayerName),token=reconnectToken});}
                    continue;
                }
                if(IsHost){
                    if(!clients.TryGetValue(notice.peer,out var c))continue;
                    if(notice.kind=="closed"){Disconnected(c);continue;}
                    if(notice.kind!="message"||c.rejectAt>0)continue;
                    if(now-c.rateStart>=1){c.rateStart=now;c.count=0;}if(++c.count>80){DropClient(c,"message rate exceeded");continue;}
                    c.heard=now;
                    try{ReceiveHost(c,JsonUtility.FromJson<TablePacket>(notice.text));}catch{Reject(c,"Invalid multiplayer message or protocol.");}
                }else if(notice.peer==serverPeer){
                    if(notice.kind=="closed"){Stop();ConnectionStatus="Host disconnected. Last table retained offline.";break;}
                    if(notice.kind!="message")continue;heard=now;
                    try{ReceiveGuest(JsonUtility.FromJson<TablePacket>(notice.text));}catch(Exception e){Debug.LogWarning("Rejected host state: "+e);Stop();ConnectionStatus="Invalid host state or incompatible protocol.";break;}
                }
            }
            if(!Active)return;
            if(IsHost){
                foreach(var c in clients.Values.ToArray()){
                    if(c.rejectAt>0&&now>=c.rejectAt)DropClient(c,"rejected handshake or message");
                    else if(c.player<0&&now-c.since>10)DropClient(c,"handshake timeout");
                    else if(now-c.heard>45)DropClient(c,"heartbeat timeout");
                }
                string state=null;
                foreach(var c in clients.Values.Where(c=>c.player>=0&&c.rejectAt==0))if(now-c.lastSend>=.1f){if(state==null)state=JsonUtility.ToJson(scene.scenario)+JsonUtility.ToJson(Lobby);if(state!=c.lastState||now-c.lastSend>=1){SendSnapshot(c);c.lastState=state;}}
            }else {
                if(now-heard>(Connected?45:10)){Stop();ConnectionStatus="Host connection timed out. Last table retained offline.";return;}
                if(Connected){SendPending(false);if(now-lastSend>=2){Send(new TablePacket{type="ping"});lastSend=now;}}
            }
        }
        void DropClient(Client c,string reason){Debug.Log("TCP player "+c.player+": "+reason);transport.Drop(c.peer);}
        void Disconnected(Client c)
        {
            clients.Remove(c.peer);var p=Lobby.players.FirstOrDefault(p=>p.id==c.player);
            if(p!=null){p.connected=p.ready=false;for(int i=0;i<Lobby.owners.Length;i++)if(Lobby.owners[i]==p.id)Lobby.epochs[i]++;Lobby.requests=Lobby.requests.Where(r=>r.player!=p.id).ToArray();Lobby.message=p.name+" disconnected. Ships remain reserved; host may reassign them.";}
            Connected=Lobby.players.Any(p=>p.id>0&&p.connected);Publish();
        }
        void Reject(Client c,string message){SendTo(c.peer,new TablePacket{type="reject",name=message});c.rejectAt=Time.realtimeSinceStartup+.15f;}
        void ReceiveHost(Client c,TablePacket packet)
        {
            if(packet==null||packet.protocol!=Protocol)throw new Exception();
            if(c.player<0){
                if(packet.type!="hello"||packet.code!=SessionCode){Reject(c,"Session code or protocol did not match.");return;}
                var returning=tokens.FirstOrDefault(t=>!string.IsNullOrEmpty(packet.token)&&t.Value==packet.token);
                LobbyPlayer p=null;
                if(returning.Value!=null){p=Lobby.players.FirstOrDefault(p=>p.id==returning.Key);if(p==null||p.connected){Reject(c,"That player is already connected.");return;}}
                else {
                    if(Lobby.players.Length>=MaxPlayers){Reject(c,"Lobby full. Host can remove a disconnected player to free a place.");return;}
                    string name=CleanName(packet.name);p=new LobbyPlayer{id=nextPlayer++,name=string.IsNullOrEmpty(name)?"Captain":name};
                    Lobby.players=Lobby.players.Concat(new[]{p}).ToArray();tokens[p.id]=Guid.NewGuid().ToString("N");
                }
                p.connected=true;p.ready=false;c.player=p.id;c.ack=0;Connected=true;
                SendSnapshot(c,"welcome",tokens[p.id]);Lobby.message=p.name+" joined the table.";Publish();return;
            }
            var player=Lobby.players.First(p=>p.id==c.player);
            switch(packet.type){
                case "ping":SendTo(c.peer,new TablePacket{type="pong"});return;
                case "ready":player.ready=packet.ready;Publish();return;
                case "side":if(packet.side< -1||packet.side>1)throw new Exception();player.side=packet.side;Publish();return;
                case "control":if(packet.generation==Lobby.generation)AddRequest(c.player,packet.ship);return;
                case "fire":if(Authorized(c,packet))BroadcastFire(packet.ship,packet.side);return;
                case "ship":
                    if(!Authorized(c,packet)||!scene.scenario.Mobile(packet.ship)||packet.sequence<=c.ack)return;
                    if(!ValidShip(packet))throw new Exception();
                    scene.GetComponent<RuleChart>().Remember();ApplyShip(scene.scenario.State(packet.ship),packet,scene.scenario.Miniatures||scene.scenario.DoB);c.ack=packet.sequence;c.lastState=null;return;
                default:throw new Exception();
            }
        }
        void ReceiveGuest(TablePacket packet)
        {
            if(packet==null||packet.protocol!=Protocol)throw new Exception();
            if(packet.type=="reject"){Stop();ConnectionStatus=packet.name??"Host rejected the session.";return;}
            if(!Connected){
                if(packet.type!="welcome"||packet.player<=0||string.IsNullOrEmpty(packet.token))throw new Exception();
                LocalPlayer=packet.player;reconnectToken=packet.token;ApplyState(packet);Connected=true;ConnectionStatus="Connected to host. Open the lobby for ship assignments.";return;
            }
            if(packet.type=="state")ApplyState(packet);
            else if(packet.type=="fire"){if(packet.ship<0||packet.ship>=scene.scenario.ShipSlots||packet.side< -1||packet.side>3)throw new Exception();scene.UpdateWorld();scene.GetComponent<BatteryEffects>().FireLocal(packet.ship,packet.side);}
            else if(packet.type!="pong")throw new Exception();
        }
        public static bool ValidShip(TablePacket packet)=>ValidCourse(packet)&&float.IsFinite(packet.dobMoved)&&packet.dobMoved>=0&&packet.dobMoved<=1800&&float.IsFinite(packet.aftTurret)&&packet.aftTurret>=0&&packet.aftTurret<360&&packet.ship>=0&&packet.ship<(packet.rulesetId=="dob"?7:packet.scenarioId=="james-1865"?6:packet.scenarioId=="hampton-roads"?4:2)&&(packet.rulesetId=="dob"||packet.scenarioId=="james-1865"||packet.ship!=2)&&(packet.rulesetId=="dob"||packet.rulesetId=="iron-fire"||RuleChart.OnBoard(new Hex(packet.q,packet.r),packet.scenarioId=="hampton-roads"))&&packet.facing>=0&&packet.facing<6&&float.IsFinite(packet.turret)&&packet.turret>=0&&packet.turret<360&&float.IsFinite(packet.load)&&packet.load>=0&&packet.load<=1;
        static bool ValidCourse(TablePacket p)
        {
            if(p.rulesetId!="iron-fire"&&p.rulesetId!="dob")return true;
            bool dob=p.rulesetId=="dob";float limit=dob?200000:20000,maxSpeed=dob?35:20;
            return float.IsFinite(p.east)&&float.IsFinite(p.north)&&Mathf.Abs(p.east)<=limit&&Mathf.Abs(p.north)<=limit&&float.IsFinite(p.heading)&&p.heading>=0&&p.heading<360&&float.IsFinite(p.speed)&&p.speed>=0&&p.speed<=maxSpeed&&float.IsFinite(p.orderedSpeed)&&p.orderedSpeed>=0&&p.orderedSpeed<=maxSpeed&&float.IsFinite(p.movementUsed)&&p.movementUsed>=0&&p.movementUsed<=100&&p.turnClass>=1&&p.turnClass<=5&&float.IsFinite(p.aftTurret)&&p.aftTurret>=0&&p.aftTurret<360&&float.IsFinite(p.dobMoved)&&p.dobMoved>=0&&p.dobMoved<=1800;
        }
        static string FindAddresses()
        {
            var addresses=new System.Collections.Generic.List<string>();
            try{foreach(var adapter in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
                if(adapter.OperationalStatus!=System.Net.NetworkInformation.OperationalStatus.Up)continue;
                foreach(var entry in adapter.GetIPProperties().UnicastAddresses)if(entry.Address.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork&&!IPAddress.IsLoopback(entry.Address)&&!addresses.Contains(entry.Address.ToString()))addresses.Add(entry.Address.ToString());
            }}catch{}
            return addresses.Count==0?"No active LAN IPv4 address found":string.Join(", ",addresses);
        }
        static void ApplyShip(ShipState ship,TablePacket packet,bool miniatures)
        {
            Vector3 point=RuleChart.Center(new Hex(packet.q,packet.r));ship.east=point.x;ship.north=point.z;ship.heading=packet.facing*60;ship.speed=0;ship.engineLoad=packet.load;ship.boilersLit=packet.boilers;ship.turretHeading=packet.turret;ship.aftTurretHeading=packet.aftTurret;ship.dobMovedSeconds=packet.dobMoved;
            if(miniatures){ship.east=packet.east;ship.north=packet.north;ship.heading=packet.heading;ship.speed=packet.speed;ship.orderedSpeed=packet.orderedSpeed;ship.movementUsed=packet.movementUsed;ship.turnClass=packet.turnClass;ship.astern=packet.astern;ship.orderedAstern=packet.orderedAstern;}
        }
        void ApplyState(TablePacket packet)
        {
            if(packet.state==null||packet.state.Length>32000)throw new Exception();
            var state=Scenario.Parse(packet.state);if((!state.hexMode&&!state.Miniatures&&!state.DoB)||state.umpireNotes.Length>4000||state.turn<1||state.turn>1000000)throw new Exception();
            foreach(var ship in state.MovingShips)if(!state.Miniatures&&!state.DoB&&!RuleChart.OnBoard(RuleChart.At(ship.Position),state.Hampton))throw new Exception();
            if(packet.lobby==null||packet.lobby.owners==null||packet.lobby.epochs==null||packet.lobby.players==null||packet.lobby.requests==null||packet.lobby.owners.Length!=state.ShipSlots||packet.lobby.epochs.Length!=state.ShipSlots||packet.lobby.players.Length>MaxPlayers)throw new Exception();
            if(state.scenarioId!=scene.scenario.scenarioId||state.rulesetId!=scene.scenario.rulesetId||packet.lobby.generation!=Lobby.generation)pending.Clear();
            Lobby=packet.lobby;
            foreach(var pair in pending.ToArray()){
                var command=pair.Value;
                if(command.sequence<=packet.ack||!state.Mobile(pair.Key)||!Lobby.running||OwnerFor(pair.Key)!=LocalPlayer||command.epoch!=Lobby.epochs[pair.Key]||command.turnNumber!=state.turn){pending.Remove(pair.Key);sentSequence.Remove(pair.Key);continue;}
                ApplyShip(state.State(pair.Key),command,state.Miniatures||state.DoB);
            }
            bool moduleChanged=scene.scenario.scenarioId!=state.scenarioId;
            applying=true;scene.scenario=state;scene.selected=Mathf.Clamp(scene.selected,0,state.ships.Length-1);scene.animateTime=false;try{if(moduleChanged)scene.UpdateWorld();}finally{applying=false;}
        }
        void OnDestroy()=>Stop();
        void OnApplicationQuit()=>Stop();
    }
}
