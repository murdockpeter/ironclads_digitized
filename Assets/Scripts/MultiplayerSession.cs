using System;
using System.Net;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public class TablePacket
    {
        public int protocol=1,ship,q,r,facing,sequence,ack,side;
        public float load;
        public bool boilers;
        public string type,code,state;
    }
    public class MultiplayerSession : MonoBehaviour
    {
        public MaritimeScene scene;
        public bool Active {get;private set;}
        public bool IsHost {get;private set;}
        public bool Connected {get;private set;}
        public int LocalShip {get;private set;}
        public int Port=>transport?.Port??0;
        public string ConnectionStatus {get;private set;}="Offline — umpire controls both ships.";
        public string SessionCode {get;private set;}="";
        public string HostAddresses {get;private set;}="";
        public bool CanManage=>!Active||IsHost;
        public bool CanControl(int ship)=>!Active||((IsHost||Connected)&&ship==LocalShip);
        DirectTcpTransport transport;
        int peer,sequence,ack;
        float heard,lastSend,peerSince,rejectAt,rateStart;
        int rateCount;
        string lastState;
        TablePacket pending;
        int sentSequence;
        float lastShipSend=-1;
        bool applying;
        readonly float[] nextFire=new float[2];
        public static string NewCode()=>Guid.NewGuid().ToString("N").Substring(0,12).ToUpperInvariant();
        public bool Host(int port,int ship,string code,bool loopbackOnly=false)
        {
            Stop();
            if(ship<0||ship>1||port<0||port>65535||(port==0&&!loopbackOnly)||string.IsNullOrWhiteSpace(code)||code.Length<6||code.Length>64){ConnectionStatus="Choose a valid port and a session code of 6–64 characters.";return false;}
            try {
                transport=new DirectTcpTransport();transport.Listen(port,loopbackOnly);Active=true;IsHost=true;LocalShip=ship;SessionCode=code;
                scene.animateTime=false;scene.scenario.hexMode=true;scene.GetComponent<RuleChart>().Snap();foreach(var vessel in scene.scenario.ships)vessel.speed=0;
                scene.selected=LocalShip;
                HostAddresses=FindAddresses();
                ConnectionStatus="Hosting on TCP "+Port+" — waiting for one opponent.";return true;
            }catch{transport?.Dispose();transport=null;ConnectionStatus="Could not host on that TCP port. It may already be in use.";return false;}
        }
        public bool Join(string address,int port,string code)
        {
            Stop();
            if(!IPAddress.TryParse(address,out var ip)||ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork||port<1||port>65535||string.IsNullOrWhiteSpace(code)||code.Length<6||code.Length>64){ConnectionStatus="Enter an IPv4 address, TCP port, and 6–64 character session code.";return false;}
            Active=true;IsHost=false;LocalShip=-1;SessionCode=code;heard=Time.realtimeSinceStartup;scene.animateTime=false;transport=new DirectTcpTransport();transport.Connect(address,port);ConnectionStatus="Connecting to "+address+":"+port+"…";return true;
        }
        public void Stop()
        {
            transport?.Dispose();transport=null;Active=false;Connected=false;IsHost=false;peer=0;sequence=ack=0;pending=null;lastState=null;rejectAt=0;
            ConnectionStatus="Offline — last table retained.";
            nextFire[0]=nextFire[1]=0;
            sentSequence=0;lastShipSend=-1;
        }
        bool Send(TablePacket packet)=>transport!=null&&transport.Send(peer,JsonUtility.ToJson(packet));
        public void SubmitShip(int ship)
        {
            if(!Active||IsHost||applying||!CanControl(ship))return;
            var vessel=scene.scenario.ships[ship];var hex=RuleChart.At(vessel.Position);
            pending=new TablePacket{type="ship",ship=ship,q=hex.q,r=hex.r,facing=RuleChart.Facing(vessel.heading),load=vessel.engineLoad,boilers=vessel.boilersLit,sequence=++sequence};
            SendPending(false);
        }
        void SendPending(bool force)
        {
            if(pending==null||pending.sequence<=sentSequence||(!force&&Time.realtimeSinceStartup-lastShipSend<.1f))return;
            if(Send(pending)){sentSequence=pending.sequence;lastShipSend=Time.realtimeSinceStartup;}
        }
        public bool RequestFire(int ship,int side)
        {
            if(!Active)return scene.GetComponent<BatteryEffects>().FireLocal(ship,side);
            if(!CanControl(ship)||(!Connected&&!IsHost)){scene.status="You can fire only your assigned ship.";return false;}
            if(IsHost)return BroadcastFire(ship,side);
            SendPending(true);return Send(new TablePacket{type="fire",ship=ship,side=side});
        }
        bool BroadcastFire(int ship,int side)
        {
            if(ship<0||ship>1||side< -1||side>2)return false;
            var effects=scene.GetComponent<BatteryEffects>();int count=effects.Battery(ship,side).Length;
            if(count==0||Time.realtimeSinceStartup<nextFire[ship])return false;
            scene.UpdateWorld();
            if(!scene.globeMode&&!effects.FireLocal(ship,side))return false;
            nextFire[ship]=Time.realtimeSinceStartup+count*effects.ripple*1.12f;
            if(Connected){Send(new TablePacket{type="state",state=JsonUtility.ToJson(scene.scenario),ack=ack});Send(new TablePacket{type="fire",ship=ship,side=side});}return true;
        }
        internal bool VerificationSend(TablePacket packet)=>Send(packet);
        void Update()
        {
            if(!Active||transport==null)return;
            if(transport.Overflowed){Stop();ConnectionStatus="Network backlog exceeded the session limit. Reconnect to continue.";return;}
            float now=Time.realtimeSinceStartup;
            for(int i=0;i<32&&transport!=null&&transport.Poll(out var notice);i++) {
                if(notice.kind=="error"){string message=notice.text;Stop();ConnectionStatus=message;break;}
                if(notice.kind=="connected") {
                    peer=notice.peer;peerSince=heard=now;Connected=false;rejectAt=0;rateStart=now;rateCount=0;
                    if(!IsHost)Send(new TablePacket{type="hello",code=SessionCode});
                    continue;
                }
                if(notice.peer!=peer)continue;
                if(notice.kind=="closed") {
                    Connected=false;peer=0;pending=null;scene.GetComponent<BatteryEffects>().Clear();
                    if(IsHost){ConnectionStatus="Opponent disconnected — waiting for a new join.";ack=0;}else{Stop();ConnectionStatus=notice.text;}
                    continue;
                }
                if(notice.kind!="message"||rejectAt>0)continue;
                if(now-rateStart>1){rateStart=now;rateCount=0;}if(++rateCount>60){transport.Drop(peer);continue;}
                heard=now;
                try{Receive(JsonUtility.FromJson<TablePacket>(notice.text));}catch{Reject("Invalid multiplayer message.");}
            }
            if(!Active)return;
            if(!IsHost&&Connected)SendPending(false);
            if(rejectAt>0&&now>=rejectAt){transport.Drop(peer);rejectAt=0;}
            if(peer!=0&&!Connected&&now-peerSince>5)transport.Drop(peer);
            if(!IsHost&&!Connected&&now-heard>7){Stop();ConnectionStatus="Connection timed out before the host accepted the session.";return;}
            if(Connected&&now-heard>12){transport.Drop(peer);return;}
            if(IsHost&&Connected&&now-lastSend>=.1f) {
                string state=JsonUtility.ToJson(scene.scenario);if(state!=lastState||now-lastSend>=1){Send(new TablePacket{type="state",state=state,ack=ack});lastState=state;lastSend=now;}
            }else if(!IsHost&&Connected&&now-lastSend>=2){Send(new TablePacket{type="ping"});lastSend=now;}
        }
        void Reject(string message)
        {
            ConnectionStatus=message;if(IsHost){Send(new TablePacket{type="reject"});rejectAt=Time.realtimeSinceStartup+.2f;}else{Stop();ConnectionStatus=message;}
        }
        void Receive(TablePacket packet)
        {
            if(packet==null||packet.protocol!=1)throw new Exception();
            if(IsHost) {
                if(!Connected) {
                    if(packet.type!="hello"||packet.code!=SessionCode){Reject("Session code or protocol did not match.");return;}
                    Connected=true;ack=0;lastState=null;Send(new TablePacket{type="welcome",ship=1-LocalShip,state=JsonUtility.ToJson(scene.scenario)});ConnectionStatus="Connected — you command "+scene.scenario.ships[LocalShip].name+".";return;
                }
                if(packet.type=="ping"){Send(new TablePacket{type="pong"});return;}
                if(packet.type=="fire"){if(packet.ship==1-LocalShip)BroadcastFire(packet.ship,packet.side);return;}
                if(packet.type!="ship")throw new Exception();
                if(packet.ship!=1-LocalShip||packet.sequence<=ack)return;
                if(!ValidShip(packet))throw new Exception();
                GetComponent<RuleChart>().Remember();ApplyShip(scene.scenario.ships[packet.ship],packet);ack=packet.sequence;lastState=null;
            }else {
                if(packet.type=="reject"){Stop();ConnectionStatus="Host rejected the session code or protocol.";return;}
                if(!Connected) {
                    if(packet.type!="welcome"||packet.ship<0||packet.ship>1)throw new Exception();
                    LocalShip=packet.ship;ApplyState(packet);Connected=true;scene.selected=LocalShip;ConnectionStatus="Connected — you command "+scene.scenario.ships[LocalShip].name+".";return;
                }
                if(packet.type=="state")ApplyState(packet);
                else if(packet.type=="fire"){if(packet.ship<0||packet.ship>1||packet.side< -1||packet.side>2)throw new Exception();scene.UpdateWorld();scene.GetComponent<BatteryEffects>().FireLocal(packet.ship,packet.side);}
                else if(packet.type!="pong")throw new Exception();
            }
        }
        public static bool ValidShip(TablePacket packet)=>packet.ship>=0&&packet.ship<2&&RuleChart.OnBoard(new Hex(packet.q,packet.r))&&packet.facing>=0&&packet.facing<6&&float.IsFinite(packet.load)&&packet.load>=0&&packet.load<=1;
        static string FindAddresses()
        {
            var addresses=new System.Collections.Generic.List<string>();
            try{foreach(var adapter in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
                if(adapter.OperationalStatus!=System.Net.NetworkInformation.OperationalStatus.Up)continue;
                foreach(var entry in adapter.GetIPProperties().UnicastAddresses)if(entry.Address.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork&&!IPAddress.IsLoopback(entry.Address)&&!addresses.Contains(entry.Address.ToString()))addresses.Add(entry.Address.ToString());
            }}catch{}
            return addresses.Count==0?"No active LAN IPv4 address found":string.Join(", ",addresses);
        }
        static void ApplyShip(ShipState ship,TablePacket packet)
        {
            Vector3 point=RuleChart.Center(new Hex(packet.q,packet.r));ship.east=point.x;ship.north=point.z;ship.heading=packet.facing*60;ship.speed=0;ship.engineLoad=packet.load;ship.boilersLit=packet.boilers;
        }
        void ApplyState(TablePacket packet)
        {
            if(packet.state==null||packet.state.Length>32000)throw new Exception();
            var state=Scenario.Parse(packet.state);if(!state.hexMode||state.umpireNotes.Length>4000||state.turn<1||state.turn>1000000)throw new Exception();
            foreach(var ship in state.ships)if(!RuleChart.OnBoard(RuleChart.At(ship.Position)))throw new Exception();
            if(pending!=null&&pending.sequence>packet.ack)ApplyShip(state.ships[LocalShip],pending);else pending=null;
            applying=true;scene.scenario=state;scene.animateTime=false;applying=false;
        }
        void OnDestroy()=>Stop();
        void OnApplicationQuit()=>Stop();
    }
}
