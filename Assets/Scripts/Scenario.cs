using System;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public class ShipState
    {
        public string name;
        public float east, north, heading, speed, turretHeading;
        public float aftTurretHeading=180,dobMovedSeconds;
        public float orderedSpeed,movementUsed;
        public int turnClass=3;
        public bool astern,orderedAstern;
        public float engineLoad=.55f;
        public bool boilersLit=true;
        public ShipState(string name, float east, float north, float heading)
        { this.name = name; this.east = east; this.north = north; this.heading = heading; }
        public Vector3 Position => new Vector3(east, 0, north);
        public void Advance(float seconds)
        {
            float meters = speed * .514444f * seconds;
            east += Mathf.Sin(heading * Mathf.Deg2Rad) * meters;
            north += Mathf.Cos(heading * Mathf.Deg2Rad) * meters;
        }
    }

    [Serializable] public class Scenario
    {
        public int version = 1;
        public string scenarioId = "ferrol";
        public string rulesetId="ironclads";
        public bool Miniatures => rulesetId=="iron-fire";
        public bool Hampton => scenarioId == "hampton-roads";
        public bool DoB=>scenarioId=="ulsan";
        public int dobTurnSeconds=180;
        public float dobVisibilityPercent=90;
        public float Limit=>DoB?200000:20000;
        public int supportVersion=1;
        public ShipState minnesota=NewMinnesota();
        public int patrickVersion=1;
        public ShipState patrickHenry=NewPatrickHenry();
        public static ShipState NewPatrickHenry()=>new ShipState("CSS Patrick Henry",430,520,240){turnClass=3};
        public int ShipSlots=>DoB?7:Hampton?4:2;
        public bool Mobile(int i)=>i>=0&&(i<ships.Length||Hampton&&i==3);
        public System.Collections.Generic.IEnumerable<ShipState> MovingShips {get{foreach(var ship in ships)yield return ship;if(Hampton)yield return patrickHenry;}}
        public int minnesotaPortGuns=11,minnesotaStarboardGuns=21,minnesotaBowGuns=1;
        public static ShipState NewMinnesota()=>new ShipState("USS Minnesota",-650,-450,90){engineLoad=.15f};
        public ShipState State(int i)=>Hampton&&i==2?minnesota:Hampton&&i==3?patrickHenry:ships[i];
        public string ShortName(int i) => DoB?UlsanScenario.Ships[i].name:Hampton&&i==3?"Patrick Henry":Hampton&&i==2?"Minnesota":Hampton ? (i==0?"Monitor":"Virginia") : (i==0?"Niagara":"Stonewall");
        public float Length(int i) => DoB?UlsanScenario.Ships[i].length:Hampton&&i==3?76.2f:Hampton&&i==2?80.6831f:Hampton?(i==0?52.4256f:80.16f):(i==0?100.228f:52.3756f);
        public float Deck(int i) => DoB?5.2f:Hampton&&i==3?2.6f:Hampton&&i==2?4.3f:Hampton?(i==0?.4572f:.29f):(i==0?4.3f:1.85f);
        public static Scenario HamptonRoads() => new Scenario {
            scenarioId="hampton-roads",latitude=36.983,longitude=-76.385,utc="1862-03-09T14:00:00Z",
            windKnots=7,windFrom=270,cloud=.22f,waveHeight=.18f,visibilityKm=22,
            ships=new[]{new ShipState("USS Monitor",-115,20,0),new ShipState("CSS Virginia (Merrimack)",120,150,240){turnClass=4}}
        };
        public double latitude = 43.43, longitude = -8.65;
        public string utc = "1865-03-24T14:00:00Z";
        public int turn = 1;
        public string umpireNotes = "";
        public bool hexMode = true;
        public float windKnots = 12, windFrom = 290, cloud = .3f, rain = 0, visibilityKm = 20, waveHeight = .8f;
        public ShipState[] ships = {
            FerrolShip("USS Niagara",new Hex(20,-2),0),
            FerrolShip("CSS Stonewall",new Hex(20,18),180)
        };
        static ShipState FerrolShip(string name,Hex hex,float heading)
        {var p=RuleChart.Center(hex);return new ShipState(name,p.x,p.z,heading);}
        public DateTime Date => DateTime.Parse(utc, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal);
        public void Advance(float seconds)
        { foreach (var ship in MovingShips) ship.Advance(seconds); utc = Date.AddSeconds(seconds).ToString("yyyy-MM-ddTHH:mm:ssZ"); }
        public static string SavePath => Path.Combine(Application.persistentDataPath, "ferrol-scenario.json");
        public void Save() => File.WriteAllText(SavePath, JsonUtility.ToJson(this, true));
        public static Scenario Load()
        {
            return Parse(File.ReadAllText(SavePath));
        }
        public static Scenario Parse(string json)
        {
            var s = JsonUtility.FromJson<Scenario>(json);
            if (s == null || s.version != 1 || s.ships == null || s.ships.Length != (s.DoB?7:2) ||
                !Finite(s.latitude) || !Finite(s.longitude) || Math.Abs(s.latitude) > 90 || Math.Abs(s.longitude) > 180)
                throw new InvalidDataException("Invalid scenario location or ship list.");
            var date = s.Date;
            if(string.IsNullOrEmpty(s.scenarioId))s.scenarioId="ferrol";
            if(s.scenarioId!="ferrol"&&s.scenarioId!="hampton-roads"&&!s.DoB)throw new InvalidDataException("Unknown scenario.");
            if(string.IsNullOrEmpty(s.rulesetId))s.rulesetId="ironclads";
            if(s.rulesetId!="ironclads"&&s.rulesetId!="iron-fire"&&s.rulesetId!="dob")throw new InvalidDataException("Unknown tabletop ruleset.");
            if(s.DoB!=(s.rulesetId=="dob"))throw new InvalidDataException("Ulsan uses the Dawn of the Battleship tabletop mode.");
            if(s.DoB){s.hexMode=false;if(s.dobTurnSeconds!=180&&s.dobTurnSeconds!=1800)throw new InvalidDataException("Invalid DoB turn interval.");if(!Finite(s.dobVisibilityPercent)||s.dobVisibilityPercent<0||s.dobVisibilityPercent>100)throw new InvalidDataException("Invalid visibility percentage.");}
            if(s.Miniatures)s.hexMode=false;
            if(s.minnesota==null||s.supportVersion==0){s.minnesota=NewMinnesota();s.minnesotaPortGuns=11;s.minnesotaStarboardGuns=21;s.minnesotaBowGuns=1;s.supportVersion=1;}
            var auxiliary=s.minnesota;
            if(!Finite(auxiliary.east)||!Finite(auxiliary.north)||!Finite(auxiliary.heading)||!Finite(auxiliary.engineLoad)||Math.Abs(auxiliary.east)>20000||Math.Abs(auxiliary.north)>20000||s.minnesotaPortGuns<0||s.minnesotaPortGuns>21||s.minnesotaStarboardGuns<0||s.minnesotaStarboardGuns>21||s.minnesotaBowGuns<0||s.minnesotaBowGuns>1)throw new InvalidDataException("Invalid Minnesota support state.");
            auxiliary.name="USS Minnesota";auxiliary.speed=auxiliary.orderedSpeed=0;auxiliary.engineLoad=Mathf.Clamp01(auxiliary.engineLoad);
            if(s.patrickHenry==null||s.patrickVersion==0){s.patrickHenry=NewPatrickHenry();s.patrickVersion=1;}
            var patrick=s.patrickHenry;
            if(!Finite(patrick.east)||!Finite(patrick.north)||Math.Abs(patrick.east)>20000||Math.Abs(patrick.north)>20000||!Finite(patrick.heading)||!Finite(patrick.speed)||patrick.speed<0||patrick.speed>20||!Finite(patrick.orderedSpeed)||patrick.orderedSpeed<0||patrick.orderedSpeed>20||!Finite(patrick.movementUsed)||patrick.movementUsed<0||patrick.movementUsed>100||!Finite(patrick.engineLoad)||patrick.turnClass<1||patrick.turnClass>5)throw new InvalidDataException("Invalid Patrick Henry state.");
            patrick.name="CSS Patrick Henry";patrick.heading=Mathf.Repeat(patrick.heading,360);patrick.engineLoad=Mathf.Clamp01(patrick.engineLoad);
            if(s.umpireNotes==null)s.umpireNotes="";
            if(s.umpireNotes.Length>4000||s.turn<1||s.turn>1000000)throw new InvalidDataException("Invalid notes or turn counter.");
            if (date.Year < 1600 || date.Year > 9998) throw new InvalidDataException("Year must be 1600–9998.");
            foreach (var ship in s.ships)
                if (ship == null || !Finite(ship.east) || !Finite(ship.north) || !Finite(ship.heading) || !Finite(ship.speed) ||
                    Math.Abs(ship.east) > s.Limit || Math.Abs(ship.north) > s.Limit || ship.speed < 0 || ship.speed > (s.DoB?35:20))
                    throw new InvalidDataException("Invalid ship position or speed (20 km local scene limit).");
            foreach(var ship in s.ships) {
                if(!Finite(ship.engineLoad)||!Finite(ship.turretHeading)||!Finite(ship.aftTurretHeading)||!Finite(ship.dobMovedSeconds)||ship.dobMovedSeconds<0||ship.dobMovedSeconds>1800)throw new InvalidDataException("Invalid engine, turret or movement setting.");
                ship.engineLoad=Mathf.Clamp01(ship.engineLoad);
                ship.turretHeading=Mathf.Repeat(ship.turretHeading,360);
                ship.aftTurretHeading=Mathf.Repeat(ship.aftTurretHeading,360);
                if(ship.turnClass==0)ship.turnClass=3; // field absent in pre-miniatures saves
                if(!Finite(ship.orderedSpeed)||!Finite(ship.movementUsed)||ship.orderedSpeed<0||ship.orderedSpeed>(s.DoB?35:20)||ship.movementUsed<0||ship.movementUsed>100||ship.turnClass<1||ship.turnClass>5)throw new InvalidDataException("Invalid miniatures movement order.");
            }
            if(s.DoB)for(int i=0;i<7;i++)s.ships[i].name=UlsanScenario.Ships[i].name;
            else {s.ships[0].name=s.Hampton?"USS Monitor":"USS Niagara";s.ships[1].name=s.Hampton?"CSS Virginia (Merrimack)":"CSS Stonewall";}
            if (!Finite(s.windKnots) || !Finite(s.windFrom) || !Finite(s.cloud) || !Finite(s.rain) || !Finite(s.visibilityKm) || !Finite(s.waveHeight))
                throw new InvalidDataException("Invalid weather values.");
            s.windKnots = Mathf.Clamp(s.windKnots, 0, 50); s.windFrom = Mathf.Repeat(s.windFrom, 360);
            s.cloud = Mathf.Clamp01(s.cloud); s.rain = Mathf.Clamp01(s.rain);
            s.visibilityKm = Mathf.Clamp(s.visibilityKm, .5f, 40); s.waveHeight = Mathf.Clamp(s.waveHeight, 0, 5);
            return s;
        }
        static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
    }

    public static class Navigation
    {
        // East / up / north. A spherical Earth is sufficient for the local umpire board.
        public const double EarthRadius = 6371000;
        public static Vector2 LatLon(Scenario s, ShipState ship) => new Vector2(
            (float)(s.latitude + ship.north / EarthRadius * 180 / Math.PI),
            (float)(s.longitude + ship.east / (EarthRadius * Math.Cos(s.latitude * Math.PI / 180)) * 180 / Math.PI));
        public static Vector3 GlobePoint(double lat, double lon)
        {
            double a = lat * Math.PI / 180, b = lon * Math.PI / 180;
            return new Vector3((float)(-Math.Cos(a) * Math.Sin(b)), (float)Math.Sin(a), (float)(Math.Cos(a) * Math.Cos(b)));
        }
        public static Vector2 GlobeLatLon(Vector3 p)
        { p.Normalize(); return new Vector2(Mathf.Asin(p.y) * Mathf.Rad2Deg, Mathf.Atan2(-p.x, p.z) * Mathf.Rad2Deg); }

        // NOAA fractional-year approximation, no atmospheric refraction. UTC, east-positive longitude.
        public static Vector3 SunDirection(DateTime utc, double latitude, double longitude)
        {
            double g = 2 * Math.PI / (DateTime.IsLeapYear(utc.Year) ? 366 : 365) * (utc.DayOfYear - 1 + (utc.TimeOfDay.TotalHours - 12) / 24);
            double eq = 229.18 * (.000075 + .001868 * Math.Cos(g) - .032077 * Math.Sin(g) - .014615 * Math.Cos(2*g) - .040849 * Math.Sin(2*g));
            double d = .006918 - .399912*Math.Cos(g) + .070257*Math.Sin(g) - .006758*Math.Cos(2*g) + .000907*Math.Sin(2*g) - .002697*Math.Cos(3*g) + .00148*Math.Sin(3*g);
            double h = (utc.TimeOfDay.TotalMinutes + eq + 4*longitude) / 4 * Math.PI / 180 - Math.PI;
            double a = latitude * Math.PI / 180;
            return new Vector3((float)(-Math.Cos(d)*Math.Sin(h)),
                (float)(Math.Sin(a)*Math.Sin(d) + Math.Cos(a)*Math.Cos(d)*Math.Cos(h)),
                (float)(Math.Cos(a)*Math.Sin(d) - Math.Sin(a)*Math.Cos(d)*Math.Cos(h))).normalized;
        }
    }
}
