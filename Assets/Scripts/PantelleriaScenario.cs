using System;
using System.Linq;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    public enum BattleLayout { End, Diamond, Echelon, ForwardTwin, Triangle }
    public sealed class BattleshipClass
    {
        public string name;public float length,beam,speed,bore;public int fore,aft,secondary,funnels;public BattleLayout layout;public bool open,tumblehome;
        // Length/beam are first-pass visual estimates. Battery families and reference speeds follow local annex research.
        public BattleshipClass(string n,float l,float b,float v,float gun,int f,int a,int secondary,int funnels=2,BattleLayout layout=BattleLayout.End,bool open=false,bool tumble=false)
        {name=n;length=l;beam=b;speed=v;bore=gun;fore=f;aft=a;this.secondary=secondary;this.funnels=funnels;this.layout=layout;this.open=open;tumblehome=tumble;}
        public int MainGuns=>layout==BattleLayout.Diamond?4:layout==BattleLayout.Echelon?4:layout==BattleLayout.Triangle?6:fore+aft;
    }
    public sealed class BattleShipSpec
    {
        public string name;public int nation,division;public BattleshipClass model;
        public BattleShipSpec(string n,int nation,int division,BattleshipClass model){name=n;this.nation=nation;this.division=division;this.model=model;}
        public int Side=>nation<2?0:1;
    }
    [Serializable] public class PantelleriaState
    {
        public int version=1;
        public string[] divisionOrders=Enumerable.Repeat("",14).ToArray();
        public float[] damagePercent=new float[42];
        public bool[] sunk=new bool[42];
    }
    public static class PantelleriaScenario
    {
        public const string Id="pantelleria-1901";
        public static readonly string[] Nations={"Britain","Italy","France","Russia"};
        public static readonly string[] Divisions={"British 8th","British 9th","British 10th","British 11th","British 12th","Italian 1st","Italian 2nd","Italian 3rd","French 1st","French 3rd","French 5th","French 7th","Russian 3rd","Russian 4th"};
        public static readonly int[] Leaders={0,13,21,34};
        public static readonly float[] Courses={105,282,180,0},BearingsFromX={290,100,345,200};
        public static readonly BattleshipClass[] Classes={
            new BattleshipClass("Canopus",132,22.3f,18,.305f,2,2,12),
            new BattleshipClass("Majestic",128,22.9f,16,.305f,2,2,12),
            new BattleshipClass("Renown",125,22,18,.254f,2,2,10),
            new BattleshipClass("Royal Sovereign",125,22.9f,16,.343f,2,2,10,open:true),
            new BattleshipClass("Hood",125,22.9f,16,.343f,2,2,10),
            new BattleshipClass("Admiral",101,20.7f,17,.343f,2,2,6,open:true),
            new BattleshipClass("Re Umberto",127.6f,23.4f,18,.343f,2,2,8,3,open:true),
            new BattleshipClass("Ruggiero di Lauria",106,19.8f,17,.432f,0,0,6,2,BattleLayout.Echelon,true),
            new BattleshipClass("Italia",124.7f,22.5f,18,.431f,0,0,12,6,BattleLayout.Echelon,true),
            new BattleshipClass("Charlemagne",117.7f,20.3f,18,.305f,2,2,10,tumble:true),
            new BattleshipClass("Bouvet",122,21.4f,18,.305f,1,1,8,2,BattleLayout.Diamond,tumble:true),
            new BattleshipClass("Charles Martel",115.5f,21.6f,18,.305f,1,1,8,2,BattleLayout.Diamond,tumble:true),
            new BattleshipClass("Jauréguiberry",118.5f,23,18,.305f,1,1,8,2,BattleLayout.Diamond,tumble:true),
            new BattleshipClass("Brennus",114.5f,20.4f,17,.340f,2,1,10,tumble:true),
            new BattleshipClass("Bouvines",89,17.9f,16,.305f,1,1,8),
            new BattleshipClass("Jemmapes",89,17.5f,16,.340f,1,1,4),
            new BattleshipClass("Terrible",82.8f,17.9f,14,.340f,1,1,4,open:true),
            new BattleshipClass("Marceau",101.6f,20.2f,16,.340f,1,1,16,3,BattleLayout.Diamond,true,true),
            new BattleshipClass("Imperator Aleksandr II",101.8f,20.4f,15,.305f,2,0,12,2,BattleLayout.ForwardTwin,true),
            new BattleshipClass("Dvenadtsat' Apostolov",104.2f,18.3f,15,.305f,2,2,4,open:true),
            new BattleshipClass("Tri Sviatitelia",113,22.3f,17,.305f,2,2,8),
            new BattleshipClass("Rostislav",107.2f,20.7f,16,.254f,2,2,8,tumble:true),
            new BattleshipClass("Ekaterina II",103.5f,21,15,.305f,0,2,7,2,BattleLayout.Triangle,true),
            new BattleshipClass("Georgii Pobedonosets",104,21,16,.305f,0,2,7,2,BattleLayout.Triangle,true)
        };
        public static readonly BattleShipSpec[] Ships=MakeFleet();
        static BattleShipSpec[] MakeFleet()
        {
            var result=new System.Collections.Generic.List<BattleShipSpec>();
            void Add(int nation,int division,int model,params string[] names){foreach(string name in names)result.Add(new BattleShipSpec(name,nation,division,Classes[model]));}
            Add(0,0,0,"Canopus","Ocean");Add(0,1,1,"Caesar","Illustrious");Add(0,1,2,"Renown");Add(0,2,3,"Revenge","Royal Oak","Ramillies");Add(0,3,3,"Empress of India","Royal Sovereign");Add(0,4,4,"Hood");Add(0,4,5,"Camperdown","Anson");
            Add(1,5,6,"Re Umberto","Sicilia","Sardegna");Add(1,6,7,"Ruggiero di Lauria","Andrea Doria","Francesco Morosini");Add(1,7,8,"Italia","Lepanto");
            Add(2,8,9,"St. Louis","Gaulois","Charlemagne");Add(2,9,10,"Bouvet");Add(2,9,11,"Charles Martel");Add(2,9,12,"Jauréguiberry");Add(2,9,13,"Brennus");Add(2,10,14,"Amiral Tréhouart","Bouvines");Add(2,10,15,"Valmy","Jemmapes");Add(2,11,16,"Terrible");Add(2,11,17,"Marceau");
            Add(3,12,18,"Imperator Aleksandr II");Add(3,12,19,"Dvenadtsat' Apostolov");Add(3,12,20,"Tri Sviatitelia");Add(3,12,21,"Rostislav");Add(3,13,22,"Ekaterina II","Chesma","Sinop");Add(3,13,23,"Georgii Pobedonosets");return result.ToArray();
        }
        public static int[] Members(int division)=>Enumerable.Range(0,42).Where(i=>Ships[i].division==division).ToArray();
        public static Scenario Create()
        {
            // Approximate island reference; explicit printed setup places X 20 nm on bearing 320°.
            var toX=Quaternion.Euler(0,320,0)*Vector3.forward*(20*1852);
            var s=new Scenario{scenarioId=Id,rulesetId="dob",hexMode=false,latitude=36.78+toX.z/Navigation.EarthRadius*180/Math.PI,longitude=11.99+toX.x/(Navigation.EarthRadius*Math.Cos(36.78*Math.PI/180))*180/Math.PI,utc="1901-02-14T13:30:00Z",windKnots=20,windFrom=260,cloud=.45f,waveHeight=2.1f,visibilityKm=40,dobVisibilityPercent=60,ships=new ShipState[42],pantelleria=new PantelleriaState()};
            for(int i=0;i<42;i++) {
                var spec=Ships[i];int first=Array.FindIndex(Ships,v=>v.division==spec.division),nationalDivision=spec.division-Ships[Leaders[spec.nation]].division;
                var forward=Quaternion.Euler(0,Courses[spec.nation],0)*Vector3.forward;var right=Quaternion.Euler(0,Courses[spec.nation],0)*Vector3.right;
                var leader=Quaternion.Euler(0,BearingsFromX[spec.nation],0)*Vector3.forward*(32000*.9144f);
                var p=leader+right*(nationalDivision*800*.9144f)-forward*((i-first)*500*.9144f);
                s.ships[i]=new ShipState(spec.name,p.x,p.z,Courses[spec.nation]){speed=15,orderedSpeed=15,engineLoad=.6f};
            }
            s.umpireNotes="Fictional scenario, In Mahan's Wake pp.70–71. Initial 15 kn follows scenario; Terrible annex reference is 14 kn. Division columns (500 yd spacing, 800 yd lanes) are an editable implementation preset. Shipboard clock assumes UTC+1. Combat, damage, signaling and victory are umpire-run.";return s;
        }
        public static void Validate(Scenario s)
        {
            var p=s.pantelleria;if(p==null||p.version!=1||p.damagePercent==null||p.damagePercent.Length!=42||p.sunk==null||p.sunk.Length!=42||p.divisionOrders==null||p.divisionOrders.Length!=14)throw new InvalidDataException("Invalid Pantelleria orders / result ledger.");
            if(p.damagePercent.Any(v=>!float.IsFinite(v)||v<0||v>100)||p.divisionOrders.Any(v=>v==null||v.Length>1000))throw new InvalidDataException("Invalid Pantelleria damage or orders.");
        }
    }
    public static class DawnFleet
    {
        public static int Count(string id)=>id==PantelleriaScenario.Id?42:id=="ulsan"?7:0;
        public static float Length(Scenario s,int i)=>s.Pantelleria?PantelleriaScenario.Ships[i].model.length:UlsanScenario.Ships[i].length;
        public static float Speed(Scenario s,int i)=>s.Pantelleria?PantelleriaScenario.Ships[i].model.speed:UlsanScenario.Ships[i].maxSpeed;
        public static int Side(Scenario s,int i)=>s.Pantelleria?PantelleriaScenario.Ships[i].Side:UlsanScenario.Ships[i].side;
        public static string Name(Scenario s,int i)=>s.Pantelleria?PantelleriaScenario.Ships[i].name:UlsanScenario.Ships[i].name;
        public static bool HasEndTurrets(Scenario s,int i)=>s.Pantelleria?PantelleriaScenario.Ships[i].model.fore>0||PantelleriaScenario.Ships[i].model.aft>0:UlsanScenario.Ships[i].twinTurrets;
    }
}
