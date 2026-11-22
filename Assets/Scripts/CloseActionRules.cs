using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public sealed class CloseActionFire
    {
        public int ship,target,mask,bgn=-1,die;
        public bool high,dismantling,initialPort,initialStarboard,resolved;
        public string result="";
        public CloseActionHit hit;
    }
    [Serializable] public sealed class CloseActionHit
    {
        public int target,rig,hull,sailors,marines;
        public string note="";
    }
    public static class CloseActionRules
    {
        public const float HexMeters=67.5f*.9144f,TurnSeconds=200;
        public static readonly float Radius=HexMeters/Mathf.Sqrt(3);
        public static readonly Hex[] Directions=RuleChart.Directions;
        public static readonly string[] SailNames={"Furled","FS / fighting","MS / medium","PS / plain"};
        public static readonly string[] PhaseNames={"Reinforcements & weather","Secret plots & crew","Simultaneous movement","Guns / marines / boarding","Maintenance & signals"};
        public static readonly string[] Tasks={"GUNS","FREE","T","FS","MS","PS","FURL","RR","RH","RS","FF","FB","BR","BA","BD","RO","PT"};
        public static Hex Add(Hex a,Hex b,int n=1)=>new Hex(a.q+b.q*n,a.r+b.r*n);
        public static Vector3 Center(Hex h)=>new Vector3(1.5f*Radius*h.q,0,HexMeters*(h.r+h.q*.5f));
        public static Hex At(Vector3 p)=>RuleChart.At(p*(RuleChart.HexMeters/HexMeters));
        public static Hex Stern(Hex bow,int facing)=>Add(bow,Directions[(facing+3)%6]);
        public static Vector3 ModelCenter(Hex bow,int facing)=>(Center(bow)+Center(Stern(bow,facing)))*.5f;
        public static int Facing(ShipState s)=>RuleChart.Facing(s.heading);
        public static Hex Bow(ShipState s)=>At(s.Position+Quaternion.Euler(0,s.heading,0)*Vector3.forward*HexMeters*.5f);
        public static Hex[] Occupied(ShipState s)=>new[]{Bow(s),Stern(Bow(s),Facing(s))};
        public static int Range(ShipState a,ShipState b)=>Occupied(a).Min(x=>Occupied(b).Min(y=>RuleChart.Range(x,y)));
        public static int WindFacing(int facing,int direction)
        {int diff=Math.Abs(((facing-(direction-1)+6)%6)-3);return diff; /* 0=L, 1=C, 2=B, 3=R */}
        public static string WindName(int wf)=>new[]{"L / luffing","C / close hauled","B / broad reaching","R / running"}[wf];
        public static Hex ParseAddress(string address)
        {
            address=(address??"").Trim().ToUpperInvariant();
            if(address.Length!=5||(address[0]!='A'&&address[0]!='B')||!int.TryParse(address.Substring(1,2),out int column)||!int.TryParse(address.Substring(3,2),out int row)||column<1||column>48||row<1||row>52)throw new ArgumentException("Use a nominal map address such as A1435 or B1403 (48 columns / 52-row join).");
            int q=column-20,total=row+(address[0]=='B'?52:0);
            return new Hex(q,52-total-Mathf.FloorToInt(q/2f));
        }
        public static string Address(Hex h)
        {
            int col=h.q+20,row=52-(h.r+Mathf.FloorToInt(h.q/2f));
            if(col>=1&&col<=48&&row>=1&&row<=104)return (row>52?"B":"A")+col.ToString("00")+(row>52?row-52:row).ToString("00");
            return "SEA "+h.q+","+h.r;
        }
        public static bool TryPlot(string text,out char[] actions,out string error)
        {
            var list=new List<char>();error="";actions=Array.Empty<char>();
            text=(text??"").Replace(" ","").ToUpperInvariant();
            if(text.Length==0){error="Submit 0 to stop; an empty field is not a plot.";return false;}
            if(text=="0")return true;
            for(int i=0;i<text.Length;i++){
                char c=text[i];
                if(c=='P'||c=='S')list.Add(c);
                else if(c>='1'&&c<='9'){
                    int n=c-'0';if(c=='1'&&i+1<text.Length&&text[i+1]=='0'){n=10;i++;}
                    for(int j=0;j<n;j++)list.Add('F');
                }else{error="Use ahead counts 1-10 and P/S turns, or 0 alone.";return false;}
                if(list.Count>10){error="A plot cannot exceed ten movement points.";return false;}
            }
            actions=list.ToArray();return true;
        }
        public static string ProfileKey(CloseActionSpec spec,CloseActionLog log,CloseActionState ca)=>spec.speedType+":"+ca.wind+":"+log.RigSections+":"+log.MovementSail;
        public static int Allowance(CloseActionSpec spec,CloseActionLog log,CloseActionState ca,int facing)
        {
            int wf=WindFacing(facing,ca.windDirection);
            if(ca.wind==0||wf==0||log.MovementSail==0||log.struck||log.captured||log.removed||log.fouled||log.grappled)return 0;
            string key=ProfileKey(spec,log,ca);var profile=ca.profiles.FirstOrDefault(p=>p.Key==key);
            return profile==null?CloseActionCharts.Allowance(spec.speedType,ca.wind,log.RigSections,log.MovementSail,wf):wf==1?profile.c:wf==2?profile.b:profile.r;
        }
        public static bool ValidateOrder(Scenario scene,CloseActionOrder order,out string error)
        {
            error="";var ca=scene.closeAction;
            if(order==null||order.ship<0||order.ship>=scene.ships.Length||order.turn!=scene.turn){error="Wrong ship or turn.";return false;}
            if(order.tasks==null||order.tasks.Length!=5||order.tasks.Any(t=>!Tasks.Contains(t))){error="Assign one recognized task per crew section.";return false;}
            if(!TryPlot(order.plot,out var actions,out error))return false;
            var ship=scene.ships[order.ship];var log=ca.logs[order.ship];var spec=CloseActionScenario.Spec(scene,order.ship);
            if(log.removed||log.struck||log.captured){if(actions.Length>0){error="Removed, struck and captured ships cannot sail.";return false;}return true;}
            if(log.tasks.Length!=5){error="Invalid crew log.";return false;}
            for(int i=0;i<5;i++){
                if(log.crew[i]==0&&order.tasks[i]!="FREE"&&order.tasks[i]!="GUNS"){error="A lost crew section cannot work.";return false;}
                if(log.sailProgress>0&&(log.sailCrewMask&(1<<i))!=0&&order.tasks[i]!=SailTask(log.sailTarget)){error="The same crew must continue the current sail alteration.";return false;}
            }
            int[] changed=order.tasks.Select(SailTarget).Where(n=>n>=0).Distinct().ToArray();
            if(changed.Length>1){error="Only one adjacent sail alteration can be plotted.";return false;}
            if(changed.Length==1){
                int target=changed[0],need=target==3||log.sail==3?2:1;
                if(Math.Abs(target-log.sail)!=1||target>log.MaxSail){error="Sail changes must be adjacent and within remaining rigging capacity.";return false;}
                if(order.tasks.Count(t=>SailTarget(t)==target)!=need){error="This sail change needs exactly "+need+" crew section(s).";return false;}
            }
            int rr=0,rh=0,rs=0;for(int i=0;i<5;i++){if(order.tasks[i]=="RR")rr++;if(order.tasks[i]=="RH")rh++;if(order.tasks[i]=="RS")rs++;}
            if(rr>1||rh>1||rs>1){error="Only one crew section may repair each damage type.";return false;}
            if(rr>0&&log.MovementSail>1){error="Rigging repairs require FS, furled or dismasted.";return false;}
            if(rh>0&&actions.Length>4){error="Hull repair requires at most four movement points.";return false;}
            int face=Facing(ship),ma=Allowance(spec,log,ca,face);
            if(ma<0){error="Enter the missing movement-card values for "+ProfileKey(spec,log,ca)+" from the red sheet.";return false;}
            if(ca.wind==0&&actions.Length>0){error="Calm prevents ahead movement and voluntary turns.";return false;}
            if(ma==0){
                if(actions.Length==0)return true;
                if(log.RigSections==0||log.fouled||log.grappled||actions.Length!=1||actions[0]=='F'){error="At MA=0 only a single umpire-adjudicated tack / turn can be plotted.";return false;}
                return true;
            }
            int acceleration=Math.Max(1,2-((log.RigSections<=2||ca.wind==1)?1:0));
            int deceleration=spec.Maneuver=='A'?2:3;
            int previous=log.Stopped?0:log.lastMP;
            int maximum=Math.Min(ma,previous+acceleration);
            if(scene.turn==1)maximum=ma; // Unspecified opening speed is chosen within the initial MA.
            if(actions.Length>maximum){error="Plot exceeds MA / acceleration: maximum "+maximum+" MPs.";return false;}
            if(log.Stopped&&scene.turn>1&&actions.Length>1&&actions[0]!='F'){error="A stopped ship beginning with a turn may do nothing further this turn.";return false;}
            int turnCount=0,straight=log.straight,lastTurn=log.lastTurn,ahead=0;
            bool forcedStop=false;
            foreach(char action in actions){
                if(forcedStop){error="The ship must stop after reaching its ahead limit or turning into luffing.";return false;}
                if(action=='F'){
                    int capacity=Allowance(spec,log,ca,face);
                    if(capacity<0){error="Enter the movement card for the new wind facing.";return false;}
                    ahead++;straight++;
                    if(ahead>capacity){error="Ahead movement exceeds the current wind-facing allowance.";return false;}
                    if(ahead==capacity)forcedStop=true;
                }else{
                    int sign=action=='S'?1:-1;
                    if(++turnCount>2){error="At most two 60-degree turns are allowed.";return false;}
                    int required=lastTurn==0?0:lastTurn==sign?log.sameTurn:log.oppositeTurn;
                    if(actions.Length<=1&&lastTurn!=0)required=1;
                    if(required<0&&lastTurn!=0)required=CloseActionCharts.TurnSpacing(spec.Maneuver,lastTurn==sign,actions.Length);
                    if(straight<required){error="Insufficient ahead movement since the previous turn.";return false;}
                    face=(face+sign+6)%6;straight=0;lastTurn=sign;
                    if(WindFacing(face,ca.windDirection)==0)forcedStop=true;
                }
            }
            int min=Math.Min(ma,Math.Max(0,previous-deceleration));
            if(actions.Length<min&&!forcedStop){error="Deceleration requires at least "+min+" MPs (unless a stated forced-stop exception applies).";return false;}
            return true;
        }
        public static int SailTarget(string task)=>task=="FURL"?0:task=="FS"?1:task=="MS"?2:task=="PS"?3:-1;
        public static string SailTask(int target)=>new[]{"FURL","FS","MS","PS"}[target];
        public static float VictoryYield(CloseActionSpec spec,CloseActionLog log)
        {
            if(log.struck||log.captured)return spec.points;
            float part=0;for(int i=0;i<4;i++){if(log.rig[i]==0)part+=i==3?.10f:.05f;if(i<3&&spec.hull[i]>0&&log.hull[i]==0)part+=.10f;}
            part+=Math.Min(4,log.crew.Count(n=>n==0))*.10f;return part*spec.points;
        }
        public static int Skill(CloseActionLog log)=>log.quality-3;
        public static int Bonus(int range)=>range<=2?4:range==3?3:range==4?2:range==5?1:0;
        public static bool InArc(Vector3 origin,float heading,int side,Vector3 target)
        {var d=target-origin;return d.sqrMagnitude>.001f&&Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,heading+side*90))<=30.0001f;}
        public static int Rake(Scenario state,int source,int target)
        {
            var a=state.ships[source];var b=state.ships[target];int range=Range(a,b);if(range>5||range<1)return 0;
            bool inArc=Occupied(b).Any(h=>Occupied(a).Any(t=>InArc(Center(h),b.heading,-1,Center(t))||InArc(Center(h),b.heading,1,Center(t))));
            if(inArc)return 0;return Vector3.Dot(a.Position-b.Position,Quaternion.Euler(0,b.heading,0)*Vector3.forward)<0?2:1;
        }
        public static int BatterySide(int battery)=>battery<2?-1:1;
        public static Hex BatteryHex(ShipState ship,int battery)=>(battery%2)==0?Bow(ship):Stern(Bow(ship),Facing(ship));
        public static bool CanBattery(Scenario state,int source,int target,int battery,out string reason)
        {
            reason="";if(source==target||source<0||target<0||source>=state.ships.Length||target>=state.ships.Length){reason="Select another ship.";return false;}
            if(CloseActionScenario.Spec(state,source).side==CloseActionScenario.Spec(state,target).side){reason="Friendly target.";return false;}
            if(state.closeAction.logs[source].struck||state.closeAction.logs[source].captured||state.closeAction.logs[source].removed||state.closeAction.logs[target].removed){reason="Ship cannot engage.";return false;}
            var a=state.ships[source];var b=state.ships[target];int range=Range(a,b);if(range>15){reason="Outside fifteen-hex gun range.";return false;}
            Hex origin=BatteryHex(a,battery);int side=BatterySide(battery);
            var endpoints=Occupied(b).Where(h=>InArc(Center(origin),a.heading,side,Center(h))).ToArray();
            if(endpoints.Length==0){reason="Neither target hex is in this 60-degree battery arc.";return false;}
            var blocked=new HashSet<Hex>();var physical=new HashSet<Hex>();
            for(int i=0;i<state.ships.Length;i++){
                if(i==source||i==target||state.closeAction.logs[i].removed)continue;
                var hexes=Occupied(state.ships[i]);foreach(var h in hexes){blocked.Add(h);physical.Add(h);}
                if(CloseActionScenario.Spec(state,i).side!=CloseActionScenario.Spec(state,source).side||!hexes.Any(h=>InArc(Center(origin),a.heading,side,Center(h))))continue;
                int friendRange=Range(a,state.ships[i]),radius=friendRange>=10?2:friendRange>=5?1:0;
                foreach(var h in hexes)for(int q=-radius;q<=radius;q++)for(int r=-radius;r<=radius;r++){
                    var nearby=Add(h,new Hex(q,r));if(RuleChart.Range(h,nearby)>radius)continue;
                    if(Occupied(a).Min(x=>RuleChart.Range(x,nearby))>=friendRange)blocked.Add(nearby);
                }
            }
            foreach(var self in Occupied(a))blocked.Remove(self);
            foreach(var endpoint in endpoints){
                var obstacles=new HashSet<Hex>(blocked);foreach(var h in Occupied(b))if(!physical.Contains(h))obstacles.Remove(h);
                if(BlockedLine(Center(origin),Center(endpoint),obstacles)){reason="Ship or friendly-fire clearance blocks line of sight.";return false;}
            }
            return true;
        }
        public static bool BlockedLine(Vector3 from,Vector3 to,HashSet<Hex> blocked)
        {
            // Convex clipping tests interiors; a spine blocks only when occupied on both sides.
            foreach(var h in blocked){
                Vector3 center=Center(h);float lo=0,hi=1;
                for(int k=0;k<6;k++){
                    float angle=(k*60+30)*Mathf.Deg2Rad;var n=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    float a=Vector3.Dot(from-center,n),b=Vector3.Dot(to-from,n),edge=HexMeters*.5f-.0002f;
                    if(Mathf.Abs(b)<.00001f){if(a>edge){lo=2;break;}}
                    else if(b>0)hi=Mathf.Min(hi,(edge-a)/b);else lo=Mathf.Max(lo,(edge-a)/b);
                }
                if(hi-lo>.00001f&&lo<.99999f&&hi>.00001f)return true;
                for(int k=0;k<6;k++){
                    var neighbor=Add(h,Directions[k]);if(!blocked.Contains(neighbor))continue;
                    Vector3 mid=(Center(neighbor)+center)*.5f,n=(Center(neighbor)-center).normalized;
                    if(Mathf.Abs(Vector3.Dot(from-mid,n))<.0005f&&Mathf.Abs(Vector3.Dot(to-mid,n))<.0005f){
                        Vector3 axis=Vector3.Cross(Vector3.up,n);float a=Vector3.Dot(from-mid,axis),b=Vector3.Dot(to-mid,axis);
                        if(Mathf.Min(Mathf.Max(a,b),Radius*.5f)-Mathf.Max(Mathf.Min(a,b),-Radius*.5f)>.001f)return true;
                    }
                }
            }
            return false;
        }
        public static int GunneryModifier(Scenario state,CloseActionFire fire)
        {
            var ca=state.closeAction;var l=ca.logs[fire.ship];int range=Range(state.ships[fire.ship],state.ships[fire.target]);
            int available=Enumerable.Range(0,4).Count(i=>l.crew[i]>0&&l.tasks[i]=="GUNS");
            int mod=6-l.quality*2-(4-available)*3;
            bool port=(fire.mask&3)!=0,star=(fire.mask&12)!=0;
            if(port)mod+=l.portDismounted;if(star)mod+=l.starboardDismounted;
            bool initial=port&&fire.initialPort||star&&fire.initialStarboard;if(initial)mod+=Bonus(range);if(Rake(state,fire.ship,fire.target)>0)mod+=Bonus(range);
            int targets=ca.fireOrders.Where(f=>f.ship==fire.ship).Select(f=>f.target).Distinct().Count();
            if(targets>1)mod-=12; // Official 1998 errata supersedes printed -10; no extra half penalty.
            else if(fire.mask!=3&&fire.mask!=12)mod-=8;
            if(l.sail==3&&ca.wind>=2)mod--;if(ca.wind==4)mod--;if(l.RigSections==0)mod-=ca.seaState;if(fire.dismantling)mod++;
            var spec=CloseActionScenario.Spec(state,fire.ship);int wf=WindFacing(Facing(state.ships[fire.ship]),ca.windDirection);
            Vector3 wind=Quaternion.Euler(0,(ca.windDirection-1)*60,0)*Vector3.forward;
            Vector3 right=Quaternion.Euler(0,state.ships[fire.ship].heading,0)*Vector3.right;
            int lee=Vector3.Dot(wind,right)>0?1:-1;
            if(ca.seaState>spec.freeboard||ca.seaState==spec.freeboard&&wf!=0&&wf!=3&&((port&&lee<0)||(star&&lee>0)))mod+=spec.lowerDeck;
            return mod;
        }
        public static void Damage(Scenario state,CloseActionHit hit)
        {
            var log=state.closeAction.logs[hit.target];
            Consume(log.rig,hit.rig);Consume(log.hull,hit.hull);Consume(log.crew,hit.sailors,4);
            log.crew[4]=Math.Max(0,log.crew[4]-hit.marines);
            for(int i=0;i<4;i++){
                if(log.rig[i]==0&&(log.rigChecks&(1<<i))==0){log.rigChecks|=1<<i;log.notes+=" Rigging check section "+(i+1)+" due.";}
                if(log.hull[i]==0&&(log.hullChecks&(1<<i))==0){log.hullChecks|=1<<i;int penalty=Math.Min(3,i+1);log.portDismounted-=penalty;log.starboardDismounted-=penalty;log.notes+=" Hull / morale checks section "+(i+1)+" due.";}
            }
            if(log.hull.All(n=>n==0)||log.crew.All(n=>n==0))log.struck=true;
            if(log.RigSections==0){log.notes+=" Dismasted: check morale.";log.sail=0;log.sailProgress=0;}
            log.notes=(log.notes+" "+hit.note).Trim();if(log.notes.Length>1500)log.notes=log.notes.Substring(log.notes.Length-1500);
        }
        static void Consume(int[] boxes,int count,int sections=int.MaxValue)
        {for(int i=0;i<boxes.Length&&i<sections&&count>0;i++){int n=Math.Min(count,boxes[i]);boxes[i]-=n;count-=n;}}
        public static void ValidateState(Scenario s)
        {
            var ca=s.closeAction;if(ca==null||ca.variant<0||ca.variant>3||ca.logs==null||ca.logs.Length!=s.ships.Length||s.ships.Length!=CloseActionScenario.Fleet(ca.variant).Length||ca.phase<0||ca.phase>4||ca.segment<0||ca.segment>10||ca.wind<1||ca.wind>4||ca.windDirection<1||ca.windDirection>6||ca.seaState!=3||ca.pendingWind< -1||ca.pendingWind>4||ca.pendingDirection< -1||ca.pendingDirection>6||ca.maintenance==null||ca.maintenance.Length!=7)throw new ArgumentException("Invalid Close Action scenario state.");
            for(int i=0;i<ca.logs.Length;i++){
                var l=ca.logs[i];if(l==null||l.rig==null||l.rig.Length!=4||l.hull==null||l.hull.Length!=4||l.crew==null||l.crew.Length!=5||l.tasks==null||l.tasks.Length!=5||l.quality<0||l.quality>6||l.sail<0||l.sail>3||l.tasks.Any(t=>!Tasks.Contains(t))||l.rig.Concat(l.hull).Concat(l.crew).Any(n=>n<0||n>30)||l.lastMP<0||l.lastMP>10||l.fires<0||l.fires>100||l.notes==null||l.notes.Length>2000)throw new ArgumentException("Invalid Close Action ship log.");
                if(l.taskProgress==null||l.taskProgress.Length!=5)throw new ArgumentException("Invalid crew progress.");
                s.ships[i].name=CloseActionScenario.Spec(s,i).name;
            }
            if(ca.profiles==null||ca.profiles.Length>300||ca.profiles.Any(p=>p==null||(p.speedType!="L-f"&&p.speedType!="L-s")||p.wind<1||p.wind>4||p.rig<1||p.rig>4||p.sail<0||p.sail>3||p.c< -1||p.c>10||p.b< -1||p.b>10||p.r< -1||p.r>10))throw new ArgumentException("Invalid movement cards.");
            if(ca.revealed==null||ca.revealed.Length>24||ca.fireOrders==null||ca.fireOrders.Length>48||ca.submitted==null||ca.submitted.Length>24||ca.journal==null||ca.journal.Length>16000)throw new ArgumentException("Invalid Close Action orders.");
        }
    }
}
