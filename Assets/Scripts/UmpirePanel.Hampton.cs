using System.Globalization;
using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        void Scenarios()
        {
            Heading("CHOOSE YOUR SEA TABLE");
            Text("Starting a scenario resets the shared table. Save your current positions first if you want to return to them.");
            GUI.enabled=ManageTable;
            Heading("TABLETOP RULESET");
            if(Button((!scene.scenario.Miniatures?"● ":"")+"Yaquinto Ironclads · hex movement"))SelectRules(false);
            if(Button((scene.scenario.Miniatures?"● ":"")+"Iron & Fire · miniatures movement"))SelectRules(true);
            Text("Iron & Fire uses exact courses, speed orders and measured turning arcs. Both rulesets use three-minute turns. Switching to hex play snaps positions and headings.");
            if(Button("Hampton Roads · Monitor vs Virginia")){scene.StartScenario(true);SyncScenarioFields();}
            if(Button("Ferrol · Niagara vs Stonewall")){scene.StartScenario(false);SyncScenarioFields();}
            GUI.enabled=true;
            if(!ManageTable)Text("The host chooses the scenario for both players.");
            if(scene.scenario.Hampton){
                Heading("HAMPTON ROADS · 9 MARCH 1862");
                Text("USS Monitor versus CSS Virginia, the converted USS Merrimack. Grounded Minnesota and the wrecks of Congress and Cumberland are also included.");
                if(Button("Coastal overview / landmarks"))CoastalView();
                if(scene.terrain!=null)foreach(var landmark in scene.terrain.landmarks)if(Button("View "+landmark.Item1)){
                    scene.globeMode=false;chart.topDown=false;scene.target=scene.terrain.root.TransformPoint(landmark.Item2)+Vector3.up*5;
                    scene.distance=landmark.Item1=="Fort Monroe"?1050:650;scene.pitch=32;scene.yaw=-30;scene.SetCamera();
                }
                Text("Shorelines, woodland, wharves, Fort Monroe, the unfinished Rip Raps fort, and shore batteries are approximate historical scenery. No modern bridge or container terminal is included. Depths, grounding, line of sight and shore fire remain umpire rulings.");
                Text("The Rip Raps fort was still named Fort Calhoun on March 9; it was renamed Fort Wool later that month.");
            }
            SaveLoad();
        }
        void SyncScenarioFields(){date=scene.scenario.utc;lat=scene.scenario.latitude.ToString(CultureInfo.InvariantCulture);lon=scene.scenario.longitude.ToString(CultureInfo.InvariantCulture);}
        void CoastalView(){scene.globeMode=false;chart.topDown=false;scene.target=new Vector3(1800,0,0)+(scene.terrain!=null?scene.terrain.root.localPosition:Vector3.zero);scene.distance=9000;scene.pitch=52;scene.yaw=-20;scene.SetCamera();}
        void HamptonBatteries(BatteryEffects effects)
        {
            if(scene.selected==0){
                var ship=scene.scenario.ships[0];
                float bearing=Slider("Turret angle from bow",ship.turretHeading,0,359,"°");
                if(Mathf.Abs(bearing-ship.turretHeading)>.01f){ship.turretHeading=bearing;scene.network?.SubmitShip(0);}
                GUILayout.BeginHorizontal();
                foreach(int angle in new[]{0,90,180,270})if(Button(angle+"°")){chart.Remember();ship.turretHeading=angle;scene.network?.SubmitShip(0);}
                GUILayout.EndHorizontal();
                if(Button("Fire turret · two XI-inch Dahlgrens")){scene.UpdateWorld();effects.Fire(0,0);}
            }else{
                if(Button("Fire BOW · 7-inch Brooke"))effects.Fire(1,0);
                if(Button("Fire STERN · 7-inch Brooke"))effects.Fire(1,3);
                if(Button("Fire PORT · three IX-inch + one 6.4-inch"))effects.Fire(1,-1);
                if(Button("Fire STARBOARD · three IX-inch + one 6.4-inch"))effects.Fire(1,1);
                if(Button("All ten guns · visual display"))effects.Fire(1,2);
            }
        }
        void HamptonSources()
        {
            Heading("HAMPTON ROADS RECONSTRUCTION");
            Text("Monitor: 172 ft raft length, 41 ft 6 in beam, approximately 18 in freeboard; twin XI-inch Dahlgrens in an eight-inch-armored turret. Low exhaust boxes depict the March battle configuration, before later taller smoke and vent arrangements.");
            Text("Virginia: converted Merrimack hull, fixed sloping casemate, six IX-inch Dahlgrens, two 6.4-inch Brooke broadside rifles and two 7-inch Brooke pivot rifles. Hull-length and beam conventions differ between sources; this reconstruction uses an approximate 263 ft hull and 51 ft overall armored width.");
            Text("March 9 configuration includes a damaged ram stump. The nominal ten-gun battery is available for demonstration; you decide which weapons remain usable after March 8 damage. End guns are shown at their center ports; manual turret training is provided only for Monitor.");
            Text("Procedural hull lines, gun carriages, fort outlines, shorelines and fittings are approximations. The fair-weather preset and initial ship positions are staged, rather than a documented exact instant of the battle. Terrain has no automatic grounding or line-of-sight behavior.");
            Text("The supplied Virginia2.pdf depicts the later CSS Virginia II and was excluded. Custom gunnery guides remain measuring aids; no verified Virginia I firing-pattern card is bundled.");
            if(Button("NOAA · Monitor construction"))Application.OpenURL("https://monitor.noaa.gov/150th/construction.html");
            if(Button("NOAA · March configuration research"))Application.OpenURL("https://monitor.noaa.gov/science/technology-for-conservation/");
            if(Button("Mariners' Museum · Virginia guns"))Application.OpenURL("https://www.marinersmuseum.org/2024/08/guns-of-css-virginia/");
            if(Button("NHHC · Hampton Roads battle accounts"))Application.OpenURL("https://www.history.navy.mil/research/library/online-reading-room/title-list-alphabetically/u/uss-monitor-versus-css-virginia-and-the-battle-for-hampton-roads.html");
            if(Button("Open supplied Monitor ship sheet"))OpenReference("Monitor.pdf");
            SaveLoad();
        }
    }
}
