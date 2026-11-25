using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public sealed class CloseActionArchive
    {
        public int version=1;
        public Scenario table;
        public CloseActionOrder[] drafts;
        public CloseActionFire[] fireDrafts;
        public CloseActionSignal[] signals,inbox;
        public int[] halted,tackShips;
        public bool[] tackSuccess;
        public string conflict="";
        public int conflictA=-1,conflictB=-1;
    }
    public sealed class CloseActionTable : MonoBehaviour
    {
        public MaritimeScene scene;
        public bool showGrid=true,showLabels=true,showArcs=true,showPaths=true,topDown;
        public int target=10,conflictA=-1,conflictB=-1;
        public string notice="",conflict="";
        public readonly Dictionary<int,CloseActionOrder> drafts=new Dictionary<int,CloseActionOrder>();
        readonly List<CloseActionFire> fireDrafts=new List<CloseActionFire>();
        readonly List<CloseActionHit> pendingHits=new List<CloseActionHit>();
        readonly Dictionary<int,CloseActionSignal> signals=new Dictionary<int,CloseActionSignal>();
        public readonly List<CloseActionSignal> inbox=new List<CloseActionSignal>();
        readonly HashSet<int> halted=new HashSet<int>();
        readonly Dictionary<int,bool> tackResults=new Dictionary<int,bool>();
        string bound="";int knownTurn;
        CloseActionState State=>scene.scenario.closeAction;
        public bool Manage=>scene.network==null||scene.network.CanManage;
        public static string SavePath=>Path.Combine(Application.persistentDataPath,"close-action-scenario.json");
        public void Bind(bool reset=false)
        {
            if(!scene.scenario.CloseAction)return;
            string identity=scene.scenario.scenarioId+":"+State.variant;
            if(reset||bound!=identity||knownTurn!=scene.scenario.turn){
                drafts.Clear();fireDrafts.Clear();signals.Clear();pendingHits.Clear();halted.Clear();tackResults.Clear();conflict="";conflictA=conflictB=-1;
                if(reset||bound!=identity)inbox.Clear();bound=identity;knownTurn=scene.scenario.turn;
            }
            target=Mathf.Clamp(target,0,scene.scenario.ships.Length-1);
        }
        public void Journal(string text)
        {
            State.journal+="T"+scene.scenario.turn+" / "+text+"\n";
            if(State.journal.Length>14000)State.journal=State.journal.Substring(State.journal.Length-14000);
            scene.status=notice=text;
        }
        public CloseActionOrder Draft(int ship)
        {
            if(drafts.TryGetValue(ship,out var order))return JsonUtility.FromJson<CloseActionOrder>(JsonUtility.ToJson(order));
            return new CloseActionOrder{ship=ship,turn=scene.scenario.turn,tasks=(string[])State.logs[ship].tasks.Clone()};
        }
        public bool Submit(CloseActionOrder order)
        {
            if(!scene.network.CanControl(order.ship)||State.phase!=1){scene.status="Your ship can submit orders only during Plot.";return false;}
            order.turn=scene.scenario.turn;order.epoch=scene.network.Active?scene.network.Lobby.epochs[order.ship]:0;order.generation=scene.network.Active?scene.network.Lobby.generation:0;
            if(!CloseActionRules.ValidateOrder(scene.scenario,order,out string error)){scene.status=error;return false;}
            if(scene.network.Active&&!scene.network.IsHost){
                if(!scene.network.SubmitCloseAction("ca-order",order.ship,JsonUtility.ToJson(order)))return false;
                drafts[order.ship]=order;scene.status="Order sent privately to the host.";return true;
            }
            return ReceiveOrder(order);
        }
        public bool ReceiveOrder(CloseActionOrder order)
        {
            if(State.phase!=1||!CloseActionRules.ValidateOrder(scene.scenario,order,out string error)){scene.status="Order rejected: wrong phase or invalid plot.";return false;}
            drafts[order.ship]=JsonUtility.FromJson<CloseActionOrder>(JsonUtility.ToJson(order));
            State.submitted=drafts.Keys.Where(Ready).OrderBy(i=>i).ToArray();scene.status="Order sealed for "+scene.scenario.ShortName(order.ship)+".";return true;
        }
        bool Ready(int ship)
        {
            if(!drafts.TryGetValue(ship,out var o)||o.turn!=scene.scenario.turn)return false;
            return !scene.network.Active||o.epoch==scene.network.Lobby.epochs[ship]&&o.generation==scene.network.Lobby.generation;
        }
        public void FillMissing()
        {
            if(!Manage||State.phase!=1)return;
            for(int i=0;i<State.logs.Length;i++){
                if(Ready(i))continue;var log=State.logs[i];int ma=CloseActionRules.Allowance(CloseActionScenario.Spec(scene.scenario,i),log,State,CloseActionRules.Facing(scene.scenario.ships[i]));
                int speed=log.Stopped?0:Math.Min(Math.Max(0,ma),log.lastMP);
                var order=new CloseActionOrder{ship=i,turn=scene.scenario.turn,plot=speed.ToString(),tasks=(string[])log.tasks.Clone(),epoch=scene.network.Active?scene.network.Lobby.epochs[i]:0,generation=scene.network.Active?scene.network.Lobby.generation:0};
                if(!ReceiveOrder(order)){scene.status="Coast order requires umpire correction for "+scene.scenario.ShortName(i)+".";return;}
            }
            Journal("Missing plots filled with straight movement at the closest legal previous speed (6.G.2).");
        }
        public bool Advance()
        {
            if(!Manage||State.finished)return false;
            if(State.phase==0){if(!State.scatterComplete){scene.status="Review scattering and opening sail / headings before starting Plot.";return false;}State.phase=1;Journal("Plot phase: orders and signals are private until reveal / delivery.");return true;}
            if(State.phase==1){
                for(int i=0;i<State.logs.Length;i++)if(!Ready(i)){scene.status="Awaiting a plot for "+scene.scenario.ShortName(i)+". Submit 0 deliberately or fill missing coast orders.";return false;}
                foreach(var order in drafts.Values)if(!CloseActionRules.ValidateOrder(scene.scenario,order,out string error)){scene.status=scene.scenario.ShortName(order.ship)+": "+error;return false;}
                GetComponent<RuleChart>().Remember();State.revealed=drafts.Values.OrderBy(o=>o.ship).Select(o=>JsonUtility.FromJson<CloseActionOrder>(JsonUtility.ToJson(o))).ToArray();
                foreach(var order in State.revealed){var l=State.logs[order.ship];l.tasks=(string[])order.tasks.Clone();l.executed="";l.firedMask=0;}
                State.phase=2;State.segment=0;halted.Clear();Journal("All plots revealed. Execute movement simultaneously, one MP per segment.");return true;
            }
            if(State.phase==2){
                if(conflict.Length>0){scene.status="Resolve the current movement conflict first.";return false;}
                if(State.segment<10&&State.revealed.Any(o=>CloseActionRules.TryPlot(o.plot,out var actions,out _)&&actions.Length>State.segment&&!halted.Contains(o.ship))){scene.status="Step remaining plotted movement before entering Combat.";return false;}
                State.phase=3;State.fireOrders=Array.Empty<CloseActionFire>();fireDrafts.Clear();pendingHits.Clear();Journal("Combat: seal fire declarations, reveal together, then adjudicate critical hits / boarding.");return true;
            }
            if(State.phase==3){
                if(fireDrafts.Count>0){scene.status="Reveal the sealed gunfire declarations first.";return false;}
                if(State.fireOrders.Any(f=>!f.resolved)){scene.status="Resolve every revealed shot or explicitly record no effect.";return false;}
                foreach(var fire in State.fireOrders)if(fire.hit!=null)CloseActionRules.Damage(scene.scenario,fire.hit);pendingHits.Clear();
                State.phase=4;Array.Clear(State.maintenance,0,State.maintenance.Length);Journal("Damage applied simultaneously. Maintenance requires the umpire's seven checks.");return true;
            }
            if(State.maintenance.Any(done=>!done)){scene.status="Complete and check all maintenance steps, including critical / morale adjudication.";return false;}
            CompleteMaintenance();return true;
        }
        public bool StepMovement(bool allowTemporaryOverlap=false)
        {
            if(!Manage||State.phase!=2||State.segment>=10)return false;
            if(conflict.Length>0&&!allowTemporaryOverlap){scene.status="Resolve the conflict before proceeding.";return false;}
            var bows=scene.scenario.ships.Select(CloseActionRules.Bow).ToArray();var facings=scene.scenario.ships.Select(CloseActionRules.Facing).ToArray();
            var moved=new char[bows.Length];
            foreach(var order in State.revealed){
                int i=order.ship;var log=State.logs[i];if(halted.Contains(i)||log.struck||log.captured||log.removed||log.fouled||log.grappled)continue;
                CloseActionRules.TryPlot(order.plot,out var actions,out _);if(State.segment>=actions.Length)continue;
                char action=actions[State.segment];
                int ma=CloseActionRules.Allowance(CloseActionScenario.Spec(scene.scenario,i),log,State,facings[i]);
                if(ma==0&&State.segment==0){
                    if(!tackResults.TryGetValue(i,out bool success)){
                        int crews=log.tasks.Count(t=>t=="T");int die=UnityEngine.Random.Range(0,10),modifier=CloseActionRules.Skill(log)+Math.Max(0,3-crews)*2+(4-log.RigSections)*2+(log.inIrons?1:0);
                        if(CloseActionRules.WindFacing(facings[i],State.windDirection)!=0)modifier-=2;
                        success=die==0||die+modifier<=CloseActionCharts.TackTarget(CloseActionScenario.Spec(scene.scenario,i).Maneuver,State.wind);tackResults[i]=success;
                        Journal(scene.scenario.ShortName(i)+" MA=0 turn: d10="+die+", modifier "+modifier+", "+(success?"succeeds":"fails")+".");
                    }
                    if(!success){halted.Add(i);continue;}
                }
                if(log.poorManeuver&&action!='F'){scene.status="Poor maneuvering requires umpire correction for "+scene.scenario.ShortName(i)+" before this segment.";conflict="Poor maneuvering: "+scene.scenario.ShortName(i);conflictA=i;conflictB=-1;return false;}
                moved[i]=action;
                if(action=='F')bows[i]=CloseActionRules.Add(bows[i],CloseActionRules.Directions[facings[i]]);
                else facings[i]=(facings[i]+(action=='S'?1:5))%6;
            }
            if(!allowTemporaryOverlap)for(int i=0;i<bows.Length;i++)for(int j=i+1;j<bows.Length;j++){
                if(State.logs[i].removed||State.logs[j].removed||moved[i]=='\0'&&moved[j]=='\0')continue;
                var a=new[]{bows[i],CloseActionRules.Stern(bows[i],facings[i])};var b=new[]{bows[j],CloseActionRules.Stern(bows[j],facings[j])};
                if(!a.Any(x=>b.Contains(x)))continue;
                conflictA=i;conflictB=j;conflict="Segment "+(State.segment+1)+": "+scene.scenario.ShortName(i)+" / "+scene.scenario.ShortName(j)+". Adjudicate contested hex, bearing off, temporary stern overlap or collision.";scene.status=conflict;return false;
            }
            for(int i=0;i<bows.Length;i++)if(moved[i]!='\0'){
                Vector3 p=CloseActionRules.ModelCenter(bows[i],facings[i]);var ship=scene.scenario.ships[i];var l=State.logs[i];ship.east=p.x;ship.north=p.z;ship.heading=facings[i]*60;
                l.executed+=moved[i]=='F'?"1":moved[i].ToString();if(moved[i]=='F')l.straight++;else{l.straight=0;l.lastTurn=moved[i]=='S'?1:-1;}
                if(CloseActionRules.WindFacing(facings[i],State.windDirection)==0)halted.Add(i);
            }
            State.segment++;conflict="";conflictA=conflictB=-1;
            if(State.segment==10||!State.revealed.Any(o=>CloseActionRules.TryPlot(o.plot,out var a,out _)&&a.Length>State.segment&&!halted.Contains(o.ship)))FinishMovement();
            scene.status="Movement segment "+State.segment+" completed.";return true;
        }
        void FinishMovement()
        {
            foreach(var l in State.logs){l.lastMP=l.executed.Contains("1")?l.executed.Length:0;if(l.executed.Length==0){l.straight=3;l.lastTurn=0;}}
            State.segment=10;
        }
        public void HoldConflict()
        {
            if(!Manage||conflictA<0)return;halted.Add(conflictA);if(conflictB>=0)halted.Add(conflictB);
            Journal("Umpire holds "+scene.scenario.ShortName(conflictA)+(conflictB>=0?" and "+scene.scenario.ShortName(conflictB):"")+" at their pre-conflict positions; assess collision / fouling separately.");conflict="";conflictA=conflictB=-1;
        }
        public void Place(int ship,Hex bow,int facing)
        {
            if(!Manage||ship<0||ship>=State.logs.Length)return;
            Vector3 p=CloseActionRules.ModelCenter(bow,facing);if(Math.Abs(p.x)>scene.scenario.Limit||Math.Abs(p.z)>scene.scenario.Limit)return;
            var state=scene.scenario.ships[ship];state.east=p.x;state.north=p.z;state.heading=facing*60;Journal("Umpire placed "+state.name+" bow "+CloseActionRules.Address(bow)+", facing "+(facing+1)+".");
        }
        public void SetProfile(CloseActionProfile profile)
        {
            if(!Manage)return;State.profiles=State.profiles.Where(p=>p.Key!=profile.Key).Concat(new[]{profile}).ToArray();Journal("Movement card override entered: "+profile.Key+".");
        }
        public bool DeclareFire(CloseActionFire fire)
        {
            if(!scene.network.CanControl(fire.ship)||State.phase!=3){scene.status="Declare gunfire during Combat for an assigned ship.";return false;}
            if(scene.network.Active&&!scene.network.IsHost)return scene.network.SubmitCloseAction("ca-fire",fire.ship,JsonUtility.ToJson(fire));
            return ReceiveFire(fire);
        }
        public bool ReceiveFire(CloseActionFire fire)
        {
            if(State.phase!=3||State.fireOrders.Length>0||fire==null||fire.ship<0||fire.ship>=State.logs.Length||fire.target<0||fire.target>=State.logs.Length||fire.mask<1||fire.mask>15){scene.status="Gunfire already revealed or declaration invalid.";return false;}
            int used=fireDrafts.Where(f=>f.ship==fire.ship).Aggregate(0,(mask,f)=>mask|f.mask);
            if((used&fire.mask)!=0){scene.status="A battery may be declared only once per turn.";return false;}
            if(fireDrafts.Where(f=>f.ship==fire.ship).Select(f=>f.target).Concat(new[]{fire.target}).Distinct().Count()>2){scene.status="At most two targets per ship.";return false;}
            var l=State.logs[fire.ship];if(!Enumerable.Range(0,4).Any(i=>l.crew[i]>0&&l.tasks[i]=="GUNS")){scene.status="No sailor section available to fire.";return false;}
            int range=CloseActionRules.Range(scene.scenario.ships[fire.ship],scene.scenario.ships[fire.target]);
            if(range<1||range>15||range>=10&&!fire.high||fire.dismantling&&range>7){scene.status="Check range, high aim at 10+ hexes, and dismantling-shot limit 7.";return false;}
            for(int b=0;b<4;b++)if((fire.mask&(1<<b))!=0&&!CloseActionRules.CanBattery(scene.scenario,fire.ship,fire.target,b,out string reason)){scene.status=reason;return false;}
            if(fire.bgn<0)fire.bgn=CloseActionCharts.BasicGunnery(CloseActionScenario.Spec(scene.scenario,fire.ship),range);
            if(fire.bgn<0||fire.bgn>80){scene.status="No legal BGN for this range, or invalid umpire override.";return false;}
            fireDrafts.Add(JsonUtility.FromJson<CloseActionFire>(JsonUtility.ToJson(fire)));scene.status="Fire declaration sealed.";return true;
        }
        public void RevealFire()
        {
            if(!Manage||State.phase!=3||State.fireOrders.Length>0)return;
            State.fireOrders=fireDrafts.OrderByDescending(f=>CloseActionRules.Rake(scene.scenario,f.ship,f.target)).ToArray();fireDrafts.Clear();
            foreach(var f in State.fireOrders){var l=State.logs[f.ship];f.initialPort=l.portInitial;f.initialStarboard=l.starboardInitial;f.die=UnityEngine.Random.Range(1,7);}
            foreach(var f in State.fireOrders){var l=State.logs[f.ship];if((f.mask&3)!=0)l.portInitial=false;if((f.mask&12)!=0)l.starboardInitial=false;l.firedMask|=f.mask;scene.network.CloseActionDisplay(f.ship,f.mask);}
            Journal(State.fireOrders.Length+" simultaneous fire declarations revealed. Damage is queued until Combat ends.");
        }
        public void ResolveFire(int index,bool automatic,CloseActionHit manual=null)
        {
            if(!Manage||index<0||index>=State.fireOrders.Length)return;var f=State.fireOrders[index];if(f.resolved)return;
            CloseActionHit hit;
            if(automatic)hit=CloseActionCharts.RollDamage(scene.scenario,f,()=>UnityEngine.Random.Range(1,7),out _);
            else hit=manual??new CloseActionHit{target=f.target,note="Umpire assessed no further effect."};
            hit.target=f.target;f.hit=hit;f.result=hit.note;f.resolved=true;Journal(scene.scenario.ShortName(f.ship)+" -> "+scene.scenario.ShortName(f.target)+": "+hit.note);
        }
        public bool SubmitSignal(int ship,string address,string text,int player=-1)
        {
            if(State.phase!=1||!scene.network.CanControl(ship)){scene.status="Signals are written during Plot by an assigned captain.";return false;}
            var signal=new CloseActionSignal{sender=ship,address=address,text=text,turn=scene.scenario.turn,side=CloseActionScenario.Spec(scene.scenario,ship).side};
            if(scene.network.Active&&!scene.network.IsHost)return scene.network.SubmitCloseAction("ca-signal",ship,JsonUtility.ToJson(signal));
            return ReceiveSignal(signal,scene.network.Active?scene.network.LocalPlayer:ship);
        }
        public bool ReceiveSignal(CloseActionSignal signal,int player)
        {
            if(State.phase!=1||signal==null||signal.sender<0||signal.sender>=State.logs.Length||signal.turn!=scene.scenario.turn||signal.text==null||signal.text.Length>150||signal.address==null||signal.address.Length>30)return false;
            var log=State.logs[signal.sender];if(log.struck||log.RigSections==0||log.signalCut){scene.status="This ship cannot signal.";return false;}
            var owned=Enumerable.Range(0,State.logs.Length).Where(i=>scene.network.Active?scene.network.OwnerFor(i)==player:i==signal.sender).ToArray();
            if(owned.Any(i=>State.logs[i].signalCut)){scene.status="A signal-halyard casualty on your command prevents signalling.";return false;}
            int words=owned.Any(i=>CloseActionScenario.Spec(scene.scenario,i).flagship)?5:3;
            string text=signal.text.ToUpperInvariant().Trim();
            foreach(var spec in CloseActionScenario.Fleet(State.variant).OrderByDescending(s=>s.name.Length))text=text.Replace(spec.name.ToUpperInvariant(),"SHIPNAME");
            if(!Regex.IsMatch(text,@"^[A-Z0-9 ]+$")||Regex.IsMatch(text,@"\b(HEX|HEXES|MP|MPS)\b|\b[AB]\d{4}\b|\b\d+[PS]\d*\b")||text.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries).Length>words){scene.status="Use at most "+words+" uppercase words; no plots, coordinates, punctuation or contractions.";return false;}
            string address=signal.address.Trim();bool group=new[]{"Fleet","Van","Center","Rear"}.Contains(address);
            int side=CloseActionScenario.Spec(scene.scenario,signal.sender).side;
            if(!group&&!CloseActionScenario.Fleet(State.variant).Any(s=>s.side==side&&s.name==address)){scene.status="Address Fleet, Van, Center, Rear, or one friendly ship.";return false;}
            signal.text=signal.text.ToUpperInvariant().Trim();signal.side=side;signal.player=player;
            signals[player]=signal;scene.status="Signal sealed for end-of-turn delivery (one per player).";return true;
        }
        public void ReceiveDeliveredSignal(CloseActionSignal signal)
        {inbox.Add(signal);if(inbox.Count>100)inbox.RemoveAt(0);scene.status="Signal delivered: "+signal.text;}
        public void RollWeather()
        {
            if(!Manage||State.phase!=0)return;
            int wd=UnityEngine.Random.Range(0,10)+UnityEngine.Random.Range(0,10),ws=UnityEngine.Random.Range(0,10)+UnityEngine.Random.Range(0,10);
            int direction=State.windDirection,speed=State.wind;
            if(wd<=1)direction=(direction-1+(UnityEngine.Random.Range(0,10)<=4?5:1))%6+1;
            else if(wd>=16){int d=((3-direction+6)%6);if(d!=0)direction=(direction-1+(d<=3?1:5))%6+1;}
            if(ws<=1)speed+=UnityEngine.Random.Range(0,10)<=4?-1:1;else if(ws>=16)speed+=Math.Sign(3-speed);
            State.pendingDirection=direction;State.pendingWind=Mathf.Clamp(speed,1,4);Journal("Oceanic / steady weather rolls "+wd+" and "+ws+"; next turn WD "+direction+", WS "+State.pendingWind+". Sea state remains 3.");
        }
        public bool SetWindNow(int direction,int strength)
        {
            if(!Manage||direction<1||direction>6||strength<1||strength>4||State.phase>1){scene.status="Current wind can be set by the host before movement orders are revealed. Queue later changes for next turn.";return false;}
            if(State.windDirection==direction&&State.wind==strength){scene.status="That wind is already in effect.";return true;}
            State.windDirection=direction;State.wind=strength;
            drafts.Clear();State.submitted=Array.Empty<int>();
            scene.scenario.windFrom=Mathf.Repeat((direction-1)*60+180,360);scene.scenario.windKnots=new[]{0,3,7,12,19}[strength];
            scene.UpdateWorld();Journal("Umpire sets current wind: WD "+direction+", WS "+strength+". Captains resubmit any sealed movement plots.");return true;
        }
        void CompleteMaintenance()
        {
            foreach(var l in State.logs){
                if(l.removed)continue;
                int target=l.tasks.Select(CloseActionRules.SailTarget).Where(n=>n>=0).DefaultIfEmpty(-1).First();
                if(target>=0&&!l.struck&&target<=l.MaxSail){
                    int mask=0;for(int i=0;i<5;i++)if(CloseActionRules.SailTarget(l.tasks[i])==target&&l.crew[i]>0)mask|=1<<i;
                    if(l.sailProgress>0&&(l.sailCrewMask!=mask||l.sailTarget!=target))l.sailProgress=0;
                    l.sailTarget=target;l.sailCrewMask=mask;l.sailProgress++;
                    int turns=target==3||l.sail==3?(l.quality>=4?3:2):(l.quality>=4?2:1);
                    if(l.sailProgress>=turns){l.sail=target;l.sailTarget=-1;l.sailProgress=0;}
                }else if(l.sailProgress>0){l.sailProgress=0;l.sailTarget=-1;}
                if(l.fires>0)l.notes+=" Fire damage / growth checked by umpire this turn.";
            }
            foreach(var signal in signals.Values){ReceiveDeliveredSignal(signal);scene.network.DeliverCloseActionSignal(signal);}
            Journal("Maintenance checked; sail changes take effect and signals are delivered.");
            scene.scenario.utc=scene.scenario.Date.AddSeconds(CloseActionRules.TurnSeconds).ToString("yyyy-MM-ddTHH:mm:ssZ");scene.scenario.turn++;
            State.phase=0;State.segment=0;State.submitted=Array.Empty<int>();State.revealed=Array.Empty<CloseActionOrder>();State.fireOrders=Array.Empty<CloseActionFire>();
            if(State.pendingDirection>=1)State.windDirection=State.pendingDirection;if(State.pendingWind>=1)State.wind=State.pendingWind;State.pendingDirection=State.pendingWind=-1;
            foreach(var l in State.logs)l.firedMask=0;Bind();
        }
        public void Morale(int ship,int die)
        {
            if(!Manage||die<0||die>9)return;var l=State.logs[ship];var spec=CloseActionScenario.Spec(scene.scenario,ship);
            if(die<spec.morale){if(l.quality==6)l.struck=true;else l.quality++;}l.pendingMorale=Math.Max(0,l.pendingMorale-1);Journal(spec.name+" morale d10="+die+"; CQ "+(char)('A'+l.quality)+(l.struck?" / STRUCK":"")+".");
        }
        public float Score(int side)=>Enumerable.Range(0,State.logs.Length).Where(i=>CloseActionScenario.Spec(scene.scenario,i).side!=side).Sum(i=>CloseActionRules.VictoryYield(CloseActionScenario.Spec(scene.scenario,i),State.logs[i]));
        public void Save()
        {
            var archive=new CloseActionArchive{table=scene.scenario,drafts=drafts.Values.ToArray(),fireDrafts=fireDrafts.ToArray(),signals=signals.Values.ToArray(),inbox=inbox.ToArray(),halted=halted.ToArray(),conflict=conflict,conflictA=conflictA,conflictB=conflictB,tackShips=tackResults.Keys.ToArray(),tackSuccess=tackResults.Values.ToArray()};
            File.WriteAllText(SavePath,JsonUtility.ToJson(archive,true));scene.status="Close Action table and local sealed orders saved: "+SavePath;
        }
        public void Load()
        {
            if(!Manage)return;var archive=JsonUtility.FromJson<CloseActionArchive>(File.ReadAllText(SavePath));
            if(archive==null||archive.version!=1||archive.table==null||!archive.table.CloseAction)throw new InvalidDataException("Not a Close Action save.");
            var loaded=Scenario.Parse(JsonUtility.ToJson(archive.table));scene.scenario=loaded;Bind(true);
            foreach(int ship in archive.halted??Array.Empty<int>())if(ship>=0&&ship<State.logs.Length)halted.Add(ship);
            conflict=archive.conflict??"";conflictA=archive.conflictA;conflictB=archive.conflictB;
            if(archive.tackShips!=null&&archive.tackSuccess!=null&&archive.tackShips.Length==archive.tackSuccess.Length)for(int i=0;i<archive.tackShips.Length;i++)tackResults[archive.tackShips[i]]=archive.tackSuccess[i];
            if(!scene.network.Active){
                foreach(var o in archive.drafts??Array.Empty<CloseActionOrder>())if(o.ship>=0&&o.ship<State.logs.Length&&o.turn==loaded.turn)drafts[o.ship]=o;
                fireDrafts.AddRange(archive.fireDrafts??Array.Empty<CloseActionFire>());
                foreach(var s in archive.signals??Array.Empty<CloseActionSignal>())signals[s.player]=s;
            }
            else State.submitted=Array.Empty<int>();
            inbox.AddRange(archive.inbox??Array.Empty<CloseActionSignal>());scene.Overview();scene.status="Close Action table loaded; online captains must resubmit sealed orders.";
        }
        public void Export()
        {
            string path=Path.Combine(Application.persistentDataPath,"close-action-log.csv");var b=new StringBuilder();
            if(!File.Exists(path))b.AppendLine("turn,phase,ship,bow,stern,facing,sail,executed,rigging,hull,crew,quality");
            for(int i=0;i<State.logs.Length;i++){var l=State.logs[i];var ship=scene.scenario.ships[i];var cells=CloseActionRules.Occupied(ship);b.AppendLine(string.Join(",",scene.scenario.turn,State.phase,ship.name,"\""+CloseActionRules.Address(cells[0])+"\"","\""+CloseActionRules.Address(cells[1])+"\"",CloseActionRules.Facing(ship)+1,l.sail,l.executed,string.Join("-",l.rig),string.Join("-",l.hull),string.Join("-",l.crew),(char)('A'+l.quality)));}
            File.AppendAllText(path,b.ToString());scene.status="Public Close Action log appended: "+path;
        }
        void LateUpdate()
        {
            if(BreakoutDesk.IsOpen(scene))return;
            if(scene==null||!scene.scenario.CloseAction){SetChartActive(false);return;}Bind();scene.view.orthographic=false;
            scene.scenario.windFrom=Mathf.Repeat((State.windDirection-1)*60+180,360);scene.scenario.windKnots=new[]{0,3,7,12,19}[State.wind];
            if(topDown&&!scene.globeMode){scene.view.orthographic=true;scene.view.orthographicSize=scene.distance*.45f;scene.view.transform.position=scene.target+Vector3.up*scene.distance;scene.view.transform.rotation=Quaternion.Euler(90,0,0);}
            DrawChart();
            if(Input.GetKey(KeyCode.LeftShift)&&Input.GetMouseButtonDown(0)&&Input.mousePosition.x>330&&Manage){var ray=scene.view.ScreenPointToRay(Input.mousePosition);var plane=new Plane(Vector3.up,Vector3.zero);if(plane.Raycast(ray,out float d))Place(scene.selected,CloseActionRules.At(ray.GetPoint(d)),CloseActionRules.Facing(scene.scenario.ships[scene.selected]));}
        }
        GameObject chartRoot,grid;Material gridMat,selectedMat,pathMat,arcMat;Hex gridCenter;int gridRadius;
        LineRenderer[] footprints,arcs;LineRenderer planned;
        void SetChartActive(bool value){if(chartRoot!=null)chartRoot.SetActive(value);}
        void EnsureChart()
        {
            if(chartRoot!=null)return;chartRoot=new GameObject("Close Action / 67.5-yard chart");chartRoot.transform.SetParent(scene.seaRoot,false);
            gridMat=new Material(Shader.Find("Ironclads/Chart"));gridMat.SetColor("_Color",new Color(.33f,.65f,.72f,.24f));
            selectedMat=new Material(gridMat);selectedMat.SetColor("_Color",new Color(1,.72f,.27f,.9f));
            pathMat=new Material(gridMat);pathMat.SetColor("_Color",new Color(.55f,.93f,.70f,.85f));arcMat=new Material(gridMat);arcMat.SetColor("_Color",new Color(.77f,.73f,.46f,.65f));
            grid=new GameObject("Close Action local sea room");grid.transform.SetParent(chartRoot.transform,false);grid.AddComponent<MeshFilter>();grid.AddComponent<MeshRenderer>().sharedMaterial=gridMat;
            footprints=new LineRenderer[2];for(int i=0;i<2;i++)footprints[i]=WorldBuilder.Line(chartRoot.transform,"Selected "+(i==0?"bow":"stern")+" occupied hex",new Vector3[7],selectedMat,.35f);
            arcs=new LineRenderer[4];for(int i=0;i<4;i++)arcs[i]=WorldBuilder.Line(chartRoot.transform,"60-degree battery "+i,new Vector3[20],arcMat,.25f);
            planned=WorldBuilder.Line(chartRoot.transform,"Own plotted bow path",new Vector3[2],pathMat,.65f);
        }
        void DrawChart()
        {
            EnsureChart();SetChartActive(scene.showOverlay&&!scene.globeMode);if(!chartRoot.activeSelf)return;
            Hex near=CloseActionRules.At(scene.target);int radius=Mathf.Clamp(Mathf.CeilToInt(scene.distance/CloseActionRules.HexMeters),12,55);
            if(gridRadius!=radius||RuleChart.Range(near,gridCenter)>6){
                gridCenter=near;gridRadius=radius;var v=new List<Vector3>();var indices=new List<int>();
                for(int q=near.q-radius;q<=near.q+radius;q++)for(int r=near.r-radius;r<=near.r+radius;r++){
                    Vector3 center=CloseActionRules.Center(new Hex(q,r));for(int i=0;i<6;i++){float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;int k=v.Count;v.Add(center+new Vector3(Mathf.Cos(a)*CloseActionRules.Radius,.14f,Mathf.Sin(a)*CloseActionRules.Radius));v.Add(center+new Vector3(Mathf.Cos(b)*CloseActionRules.Radius,.14f,Mathf.Sin(b)*CloseActionRules.Radius));indices.Add(k);indices.Add(k+1);}
                }
                var mesh=new Mesh{name="67.5-yard hexes",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetIndices(indices.ToArray(),MeshTopology.Lines,0);mesh.RecalculateBounds();var mf=grid.GetComponent<MeshFilter>();if(mf.sharedMesh!=null)Destroy(mf.sharedMesh);mf.sharedMesh=mesh;
            }
            grid.SetActive(showGrid);var ship=scene.scenario.ships[scene.selected];var occupied=CloseActionRules.Occupied(ship);
            for(int i=0;i<2;i++){footprints[i].gameObject.SetActive(showGrid);var pts=new Vector3[7];Vector3 c=CloseActionRules.Center(occupied[i]);for(int k=0;k<7;k++)pts[k]=c+new Vector3(Mathf.Cos(k*Mathf.PI/3)*CloseActionRules.Radius,.25f,Mathf.Sin(k*Mathf.PI/3)*CloseActionRules.Radius);footprints[i].SetPositions(pts);}
            for(int i=0;i<4;i++){
                arcs[i].gameObject.SetActive(showArcs);var origin=CloseActionRules.Center(CloseActionRules.BatteryHex(ship,i))+Vector3.up*.3f;var points=new Vector3[20];points[0]=points[19]=origin;
                for(int k=1;k<19;k++)points[k]=origin+Quaternion.Euler(0,ship.heading+CloseActionRules.BatterySide(i)*90-30+(k-1)*60/17f,0)*Vector3.forward*CloseActionRules.HexMeters*15;
                arcs[i].SetPositions(points);
            }
            string plot=null;if(State.phase==2)plot=State.revealed.FirstOrDefault(o=>o.ship==scene.selected)?.plot;else if(scene.network.CanControl(scene.selected)&&drafts.TryGetValue(scene.selected,out var own))plot=own.plot;
            planned.gameObject.SetActive(showPaths&&plot!=null);
            if(plot!=null&&CloseActionRules.TryPlot(plot,out var actions,out _)){
                var p=new List<Vector3>{CloseActionRules.Center(occupied[0])+Vector3.up*.4f};Hex bow=occupied[0];int face=CloseActionRules.Facing(ship);
                foreach(char a in actions){if(a=='F')bow=CloseActionRules.Add(bow,CloseActionRules.Directions[face]);else face=(face+(a=='S'?1:5))%6;p.Add(CloseActionRules.Center(bow)+Vector3.up*.4f);}if(p.Count==1)p.Add(p[0]);planned.positionCount=p.Count;planned.SetPositions(p.ToArray());
            }
        }
        GUIStyle labels;
        void OnGUI()
        {
            if(BreakoutDesk.IsOpen(scene))return;
            if(scene==null||!scene.scenario.CloseAction||scene.globeMode||!scene.showOverlay)return;
            if(labels==null)labels=new GUIStyle(GUI.skin.label){fontSize=10,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.80f,.88f,.87f,.8f)}};
            var placed=new List<Rect>();
            if(showLabels&&showGrid&&scene.distance>=180&&scene.distance<4200){Hex near=CloseActionRules.At(scene.target);int radius=Mathf.Clamp(Mathf.CeilToInt(scene.distance/CloseActionRules.HexMeters),6,30);for(int q=near.q-radius;q<=near.q+radius;q++)for(int r=near.r-radius;r<=near.r+radius;r++){
                var hex=new Hex(q,r);Vector3 point=CloseActionRules.Center(hex),p=scene.view.WorldToScreenPoint(point);
                if(p.z<=0||p.x<=350||p.x>=Screen.width-35||p.y<=65||p.y>=Screen.height-110)continue;
                float spacing=float.PositiveInfinity;for(int direction=0;direction<3;direction++){var n=scene.view.WorldToScreenPoint(CloseActionRules.Center(CloseActionRules.Add(hex,CloseActionRules.Directions[direction])));spacing=Mathf.Min(spacing,Vector2.Distance(p,n));}
                if(spacing<55)continue;
                var rect=new Rect(p.x-33,Screen.height-p.y-7,66,15);if(placed.Any(other=>other.Overlaps(rect)))continue;
                // Reserve the selected hull's screen area for its modeled details.
                var selected=scene.scenario.ships[scene.selected];var spec=CloseActionScenario.Spec(scene.scenario,scene.selected);
                Vector3 hull=scene.view.WorldToScreenPoint(selected.Position+Vector3.up*spec.Deck);
                float protect=Mathf.Clamp(spec.Length/Mathf.Max(1,hull.z)*Screen.height,12,180);
                if(Vector2.Distance(p,hull)<protect)continue;
                GUI.Label(rect,CloseActionRules.Address(hex),labels);placed.Add(rect);
            }}
            if(!showLabels||scene.distance<350)return;
            placed.Clear();
            foreach(int i in Enumerable.Range(0,scene.scenario.ships.Length).OrderBy(i=>(scene.scenario.ships[i].Position-scene.view.transform.position).sqrMagnitude)){
                if(State.logs[i].removed)continue;Vector3 p=scene.view.WorldToScreenPoint(scene.scenario.ships[i].Position+Vector3.up*(CloseActionScenario.Spec(scene.scenario,i).Deck+8));
                if(p.z<=0||p.x<=370||p.x>=Screen.width-70||p.y<=75||p.y>=Screen.height-110)continue;
                var rect=new Rect(p.x-65,Screen.height-p.y-20,130,18);if(placed.Any(other=>other.Overlaps(rect)))continue;
                GUI.Label(rect,scene.scenario.ShortName(i)+(State.logs[i].struck?" / STRUCK":""),labels);placed.Add(rect);
            }
        }
    }
}
