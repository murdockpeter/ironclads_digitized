using System.Globalization;
using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        string patrickEast="430",patrickNorth="520";float patrickSegment=1;
        void PatrickHenryPanel()
        {
            if(!scene.scenario.Hampton){Text("Patrick Henry belongs to Hampton Roads.");return;}
            var ship=scene.scenario.patrickHenry;var control=GetComponent<PatrickHenryControl>();var effects=GetComponent<BatteryEffects>();
            Heading("CSS PATRICK HENRY · CONFEDERATE SUPPORT");
            Text("Side-wheel steamer · 250 ft hull × 34 ft beam, excluding wheel boxes. Two masts, one funnel and a walking-beam engine. Reference speed 12 knots. Placement is a staged March 9 supporting position.");
            if(Button("Inspect Patrick Henry"))scene.support.InspectPatrick();
            if(Button("Measure from Patrick Henry / gun guide")){var guns=GetComponent<GunneryTools>();guns.patrickOrigin=true;guns.minnesotaOrigin=false;guns.preset=3;guns.visible=true;guns.pickedTarget=false;ShowTab(4);}
            GUI.enabled=control.Own;string before=JsonUtility.ToJson(ship);
            Heading("POSITION & MOVEMENT");
            if(scene.scenario.Miniatures){
                ship.heading=Slider("Current course / umpire setup",ship.heading,0,359.9f,"°");
                ship.speed=Slider("Current speed",ship.speed,0,12," kn");ship.orderedSpeed=Slider("Next-turn speed order",ship.orderedSpeed,0,12," kn");
                ship.astern=GUILayout.Toggle(ship.astern,"Current movement astern",toggle);ship.orderedAstern=GUILayout.Toggle(ship.orderedAstern,"Next-turn astern order",toggle);
                ship.turnClass=Mathf.RoundToInt(Slider("Turning aid class / umpire",ship.turnClass,1,5));
                Text("Class 3 is a working default, not a verified Patrick Henry record. Set the class and speed permitted by your rules and adjudicated damage.");
                Text($"Movement used {ship.movementUsed:0.00} / {ship.speed:0.00} inches; remaining {CourseMovement.Remaining(ship):0.00}. One inch = 100 yards.");
                patrickSegment=Slider("Move segment",patrickSegment,.1f,6," in");
                GUILayout.BeginHorizontal();if(Button("Port arc"))control.Move(patrickSegment,-1);if(Button("Straight"))control.Move(patrickSegment,0);if(Button("Starboard arc"))control.Move(patrickSegment,1);GUILayout.EndHorizontal();
                if(Button("Reset movement ruler / umpire override")){chart.Remember();ship.movementUsed=0;}
            }else{
                Text("Local hex "+RuleChart.At(ship.Position)+$" · heading {ship.heading:0}°");
                GUILayout.BeginHorizontal();if(Button("Port 60°"))control.TurnHex(-1);if(Button("Starboard 60°"))control.TurnHex(1);GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();if(Button("Astern 1 hex"))control.HexMove(-1);if(Button("Ahead 1 hex"))control.HexMove(1);GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();patrickEast=GUILayout.TextField(patrickEast,field);patrickNorth=GUILayout.TextField(patrickNorth,field);GUILayout.EndHorizontal();
            if(Button("Place at east / north meters")){
                if(float.TryParse(patrickEast,NumberStyles.Float,CultureInfo.InvariantCulture,out float e)&&float.TryParse(patrickNorth,NumberStyles.Float,CultureInfo.InvariantCulture,out float n)&&control.Place(new Vector3(e,0,n)))scene.status="Patrick Henry placed. Hex mode snaps to the local board.";else scene.status="Use finite coordinates inside the local scene / hex board.";
            }
            Heading("PATRICK HENRY'S BATTERIES");
            GUI.enabled=control.Own&&!scene.globeMode&&!effects.firing[3];
            if(Button("Fire BOW · ten-inch shell gun"))effects.Fire(3,0);
            if(Button("Fire STERN · eight-inch solid-shot pivot"))effects.Fire(3,3);
            if(Button("Fire PORT · five 32-pounders"))effects.Fire(3,-1);
            if(Button("Fire STARBOARD · five 32-pounders"))effects.Fire(3,1);
            if(Button("All twelve guns · display"))effects.Fire(3,2);
            GUI.enabled=control.Own;
            Heading("PADDLES & FUNNEL SMOKE");
            ship.boilersLit=GUILayout.Toggle(ship.boilersLit,"Boilers lit",toggle);ship.engineLoad=Slider("Visual engine effort",ship.engineLoad,0,1);
            if(before!=JsonUtility.ToJson(ship))scene.network?.SubmitShip(3);
            GUI.enabled=true;
            Text("The Virginia player commands Patrick Henry in TCP play. Damage, loading, arcs, movement eligibility and combat remain umpire decisions. Engine effort animates paddles and smoke; it does not move the ship automatically.");
            Text("Working battery: the March battle roster lists 12 guns; Rochelle describes ten broadside 32-pounders and 10-/8-inch pivots. DANFS lists a different ten-gun outfit. Exact mounts and 1862 fittings remain reconstructed; later school-ship cabins are omitted.");
            if(Button("Navy historical images / Patrick Henry"))Application.OpenURL("https://www.ibiblio.org/hyperwar/OnlineLibrary/photos/sh-us-cs/csa-sh/csash-mr/pat-hnry.htm");
            SaveLoad();
        }
    }
}
