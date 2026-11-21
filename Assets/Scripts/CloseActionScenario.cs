using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public sealed class CloseActionSpec
    {
        public string name,start,quality,speedType,squadron;
        public int side,rate,shipClass,morale,draft,critical,longGuns,carronades,freeboard=4,lowerDeck,points;
        public bool flagship;
        public int[] rig,hull,crew;
        public CloseActionPrototype Prototype=>CloseActionPrototypes.For(this);
        public float Length=>Prototype.length;
        public float Beam=>Prototype.beam;
        public float Deck=>Prototype.deck;
        public char Maneuver=>shipClass==1||rate==80?'A':'B';
        public CloseActionSpec(string n,string at,int side,int rate,string cq,string st,int vp,int[] rig,int[] hull,int[] crew,int lg,int cr,int ld=-5,int ch=0,bool flag=false,string squad="")
        {name=n;start=at;this.side=side;this.rate=rate;quality=cq.Substring(0,1);morale=cq[1]-'0';speedType=st;points=vp;this.rig=rig;this.hull=hull;this.crew=crew;longGuns=lg;carronades=cr;lowerDeck=ld;critical=ch;shipClass=rate==120?1:2;draft=n=="Conquerant"?21:rate==120?29:rate==80?24:rate==50?20:22;flagship=flag;squadron=squad;}
    }
    public static class CloseActionScenario
    {
        public const string Id="close-action-6";
        public static readonly string[] Variants={"Book / 23 ships","Goliath replaces Leander / 23","Conquerant detached / 22","Goliath added / 24"};
        static int[] A(params int[] n)=>n;
        // Transcribed and visually checked against Scenario Book printed pp.15-16.
        public static readonly CloseActionSpec[] BaseFleet={
            new CloseActionSpec("Zealous","A1435",0,74,"B3","L-f",86,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),17,1,squad:"Van"),
            new CloseActionSpec("Audacious","A1439",0,74,"B3","L-f",86,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),17,1,squad:"Van"),
            new CloseActionSpec("Vanguard","A1443",0,74,"C2","L-f",85,A(5,7,7,7),A(8,8,9,9),A(6,6,6,6,5),16,4,flag:true,squad:"Van"),
            new CloseActionSpec("Defence","A1447",0,74,"B3","L-s",84,A(7,7,7,7),A(6,8,9,9),A(6,6,6,6,5),17,1,squad:"Van"),
            new CloseActionSpec("Leander","A1451",0,50,"A2","L-s",55,A(5,5,6,6),A(6,6,6,7),A(3,3,4,4,3),9,0,ch:-1,squad:"Van"),
            new CloseActionSpec("Minotaur","B1403",0,74,"B3","L-f",93,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),16,4,squad:"Van"),
            new CloseActionSpec("Theseus","B1409",0,74,"B3","L-f",86,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),17,1,squad:"Rear"),
            new CloseActionSpec("Culloden","B1413",0,74,"A3","L-f",94,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),17,1,flag:true,squad:"Rear"),
            new CloseActionSpec("Alexander","B1417",0,74,"A3","L-f",94,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),17,1,squad:"Rear"),
            new CloseActionSpec("Swiftsure","B1421",0,74,"B3","L-f",86,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),17,1,squad:"Rear"),
            new CloseActionSpec("Spartiate","A3329",1,74,"D5","L-f",67,A(7,7,7,8),A(9,9,9,9),A(6,6,7,7,8),19,1,squad:"Van"),
            new CloseActionSpec("Guerrier","A3333",1,74,"D3","L-s",71,A(7,7,7,7),A(4,8,9,9),A(6,6,7,7,8),19,1,squad:"Van"),
            new CloseActionSpec("Franklin","A3338",1,80,"D4","L-f",87,A(8,8,8,8),A(10,10,11,11),A(8,8,8,8,9),24,1,-4,flag:true,squad:"Van"),
            new CloseActionSpec("Aquilon","A3342",1,74,"D5","L-f",67,A(7,7,7,8),A(9,9,9,9),A(6,6,7,7,8),19,1,squad:"Van"),
            new CloseActionSpec("Tonnant","A3348",1,80,"D3","L-f",90,A(8,8,8,8),A(10,10,11,11),A(8,8,8,8,9),24,1,-4,squad:"Center"),
            new CloseActionSpec("Peuple Souverain","A3352",1,74,"D5","L-s",64,A(7,7,7,7),A(6,8,9,9),A(6,6,7,7,8),19,1,squad:"Center"),
            new CloseActionSpec("Orient","B3304",1,120,"D4","L-f",106,A(9,9,9,9),A(12,12,12,12),A(10,10,11,11,12),29,1,-4,1,true,"Center"),
            new CloseActionSpec("Conquerant","B3309",1,74,"E5","L-s",35,A(7,7,7,8),A(0,9,9,9),A(3,4,4,5,5),12,1,-4,squad:"Center"),
            new CloseActionSpec("Heureux","B3313",1,74,"D5","L-s",66,A(7,7,7,8),A(9,9,9,9),A(6,6,7,7,8),19,1,squad:"Center"),
            new CloseActionSpec("Mercure","B3319",1,74,"D5","L-f",67,A(7,7,7,8),A(9,9,9,9),A(6,6,7,7,8),19,1,squad:"Rear"),
            new CloseActionSpec("Guillaume Tell","B3323",1,80,"D5","L-f",84,A(8,8,8,8),A(10,10,11,11),A(8,8,8,8,9),24,1,-4,flag:true,squad:"Rear"),
            new CloseActionSpec("Timoleon","B3328",1,74,"E5","L-s",55,A(7,7,7,8),A(9,9,9,9),A(6,6,7,7,8),19,1,squad:"Rear"),
            new CloseActionSpec("Genereux","B3332",1,74,"D5","L-f",67,A(7,7,7,8),A(9,9,9,9),A(6,6,7,7,8),19,1,squad:"Rear")
        };
        public static readonly CloseActionSpec Goliath=new CloseActionSpec("Goliath","B1425",0,74,"B3","L-f",86,A(7,7,7,7),A(8,8,9,9),A(6,6,6,6,5),17,1,squad:"Rear");
        public static CloseActionSpec[] Fleet(int variant)
        {
            var result=new List<CloseActionSpec>(BaseFleet);
            if(variant==1)result[4]=Goliath;
            if(variant==2)result.RemoveAll(s=>s.name=="Conquerant");
            if(variant==3)result.Insert(10,Goliath);
            return result.ToArray();
        }
        static readonly CloseActionSpec[][] CachedFleets={Fleet(0),Fleet(1),Fleet(2),Fleet(3)};
        public static CloseActionSpec Spec(Scenario s,int i)=>CachedFleets[s.closeAction.variant][i];
        public static Scenario Create(int variant=0)
        {
            var fleet=Fleet(variant);
            var s=new Scenario{scenarioId=Id,rulesetId="close-action",latitude=36.6,longitude=15.2,utc="1798-06-23T12:00:00Z",hexMode=true,windKnots=12,windFrom=300,waveHeight=.45f,cloud=.18f,visibilityKm=20,ships=new ShipState[fleet.Length],closeAction=new CloseActionState{variant=variant,logs=new CloseActionLog[fleet.Length]}};
            for(int i=0;i<fleet.Length;i++){
                var spec=fleet[i];string start=variant==1&&i==4?"A1451":spec.start;
                Hex bow=CloseActionRules.ParseAddress(start);
                if(variant==2&&Array.IndexOf(BaseFleet,spec)>17)bow=CloseActionRules.Add(bow,CloseActionRules.Directions[0],4);
                Vector3 center=CloseActionRules.ModelCenter(bow,0);
                s.ships[i]=new ShipState(spec.name,center.x,center.z,0){engineLoad=0,boilersLit=false};
                s.closeAction.logs[i]=new CloseActionLog(spec){sail=spec.side==1?2:1};
            }
            s.umpireNotes="Close Action scenario 6: Nelson vs Brueys, 23 June 1798 (hypothetical). Base 10 British vs 13 French. Official errata: WD=3. Both fleets scatter before play; French start MS, British choose initial sail. Standard victory points, no automatic escape and no fixed turn limit. Movement and normal gunnery use published chart values; critical hits and boarding remain umpire-entered.";
            return s;
        }
    }
    [Serializable] public sealed class CloseActionLog
    {
        public int sail=1,quality,lastMP,lastTurn,straight=3,rigChecks,hullChecks,pendingMorale;
        public int[] rig,hull,crew;
        public string[] tasks={"GUNS","GUNS","GUNS","GUNS","FREE"};
        public string executed="",notes="";
        public int[] taskProgress=new int[5];
        public int sailTarget=-1,sailProgress,sailCrewMask;
        public int portDismounted,starboardDismounted,fires;
        public bool portInitial=true,starboardInitial=true,inIrons,struck,captured,removed,fouled,grappled,poorManeuver,signalCut;
        public int firedMask;
        public string profileKey="";
        public int sameTurn=-1,oppositeTurn=-1;
        public CloseActionLog(){}
        public CloseActionLog(CloseActionSpec s){quality=s.quality[0]-'A';rig=(int[])s.rig.Clone();hull=(int[])s.hull.Clone();crew=(int[])s.crew.Clone();for(int i=0;i<4;i++){if(rig[i]==0)rigChecks|=1<<i;if(hull[i]==0)hullChecks|=1<<i;}}
        public int RigSections=>rig.Count(n=>n>0);
        public int MaxSail=>RigSections==0?0:RigSections==4?3:RigSections==3?2:1;
        public int MovementSail=>Mathf.Min(sail,MaxSail);
        public bool Stopped=>lastMP==0||struck||captured||fouled||grappled;
    }
    [Serializable] public sealed class CloseActionProfile
    {
        public string speedType="L-f";
        public int wind=3,rig=4,sail=1,c=-1,b=-1,r=-1;
        public string Key=>speedType+":"+wind+":"+rig+":"+sail;
    }
    [Serializable] public sealed class CloseActionOrder
    {
        public int ship,turn,epoch,generation;
        public string plot="0";
        public string[] tasks={"GUNS","GUNS","GUNS","GUNS","FREE"};
    }
    [Serializable] public sealed class CloseActionSignal
    {
        public int sender,player,turn,side;
        public string address="Fleet",text="";
    }
    [Serializable] public sealed class CloseActionState
    {
        public int variant,phase,segment,wind=3,windDirection=3,seaState=3,pendingWind=-1,pendingDirection=-1;
        public bool scatterComplete,finished;
        public string outcome="",journal="";
        public CloseActionLog[] logs;
        public CloseActionProfile[] profiles={
            new CloseActionProfile{c=3,b=5,r=4,sail=1},
            new CloseActionProfile{c=4,b=7,r=6,sail=2},
            new CloseActionProfile{c=6,b=9,r=8,sail=3}
        };
        // Only revealed orders enter the broadcast state. Drafts stay in CloseActionTable.
        public CloseActionOrder[] revealed=Array.Empty<CloseActionOrder>();
        public CloseActionFire[] fireOrders=Array.Empty<CloseActionFire>();
        public int[] submitted=Array.Empty<int>();
        public bool[] maintenance=new bool[7];
        public string[] maintenanceLabels={"Drift / veering","Anchoring / towing (no anchors in S6)","Grappling","Ungrappling","Unfouling / fixing","Fires / explosions","Repairs / morale / reconciliation"};
    }
}
