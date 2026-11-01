using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        void MovementPanel()
        {
            if(!scene.scenario.Miniatures){Text("Choose Iron & Fire in Scenarios to use these movement tools.");return;}
            GUILayout.BeginHorizontal();if(Button(scene.scenario.ShortName(0)))scene.selected=0;if(Button(scene.scenario.ShortName(1)))scene.selected=1;GUILayout.EndHorizontal();
            Text(scene.scenario.ships[scene.selected].name);MiniaturesMovement();
            GUI.enabled=ManageTable;if(Button("End turn · apply speed orders / +3 minutes"))chart.NextTurn();if(Button("Undo last table action"))chart.Undo();GUI.enabled=true;SaveLoad();
        }
        void SelectRules(bool miniatures)
        {
            if(!ManageTable)return;chart.Remember();var s=scene.scenario;s.rulesetId=miniatures?"iron-fire":"ironclads";s.hexMode=!miniatures;scene.animateTime=false;
            foreach(var ship in s.ships){ship.orderedSpeed=ship.speed;ship.orderedAstern=ship.astern;ship.movementUsed=0;}
            if(!miniatures)chart.Snap();GetComponent<GunneryTools>().preset=3;
            scene.status=miniatures?"Iron & Fire: continuous courses, 1 inch = 100 yards, 1 inch per knot per turn. Umpire controls eligibility.":"Yaquinto Ironclads: 100-yard hexes and six facings.";
        }
        void MiniaturesMovement()
        {
            var ship=scene.scenario.ships[scene.selected];var movement=GetComponent<CourseMovement>();
            Heading("IRON & FIRE · COURSE & SPEED");GUI.enabled=OwnShip;
            // These are explicit umpire adjustments, not checks of damage/engine eligibility.
            float heading=Slider("Current course (setup / override)",ship.heading,0,359.9f,"° true");
            float speed=Slider("Current speed (setup / override)",ship.speed,0,20," kn");
            bool astern=GUILayout.Toggle(ship.astern,"Current movement astern",toggle);
            if(heading!=ship.heading||speed!=ship.speed||astern!=ship.astern){ship.heading=heading;ship.speed=speed;ship.astern=astern;scene.network?.SubmitShip(scene.selected);}
            Text($"Movement used {ship.movementUsed:0.00}″ / {ship.speed:0.00}″\nRemaining {CourseMovement.Remaining(ship):0.00}″ = {CourseMovement.Remaining(ship)*100:0} yd");
            Heading("PLAN A MOVEMENT SEGMENT");
            int turn=GUILayout.SelectionGrid(movement.turn+1,new[]{"Port","Straight","Starboard"},3,button);movement.turn=turn-1;
            int turnClass=GUILayout.SelectionGrid(ship.turnClass-1,new[]{"VS · 1″","S · 2″","M · 3″","L · 4″","VL · 5″"},3,button)+1;
            if(turnClass!=ship.turnClass){ship.turnClass=turnClass;scene.network?.SubmitShip(scene.selected);}
            Text("Choose the effective turning class after speed and damage modifiers (p. 7). Radius is measured in tabletop inches.");
            movement.segment=Slider("Segment distance",movement.segment,.05f,20,"″");
            if(Button("Move previewed segment"))movement.Move(movement.segment,movement.turn);
            if(Button("Move remaining allowance"))movement.Move(CourseMovement.Remaining(ship),movement.turn);
            GUILayout.BeginHorizontal();if(Button("Pivot −45°"))ManualPivot(-45);if(Button("Pivot +45°"))ManualPivot(45);GUILayout.EndHorizontal();
            GUILayout.Label("Stationary screw vessels may pivot up to 45° per turn with power available. Umpire approves and tracks eligibility.",small);
            Heading("SPEED ORDER FOR END OF TURN");
            float order=Slider("Ordered speed",ship.orderedSpeed,0,20," kn");bool orderedAstern=GUILayout.Toggle(ship.orderedAstern,"Ordered movement astern",toggle);
            if(order!=ship.orderedSpeed||orderedAstern!=ship.orderedAstern){ship.orderedSpeed=order;ship.orderedAstern=orderedAstern;scene.network?.SubmitShip(scene.selected);}
            Text("Normal speed change is up to 3 knots per turn; stop before reversing. Orders apply when the host ends the turn. Max speed, damage and exceptions remain your rulings.");
            Heading("MANUAL CURRENT / DRIFT");movement.driftBearing=Slider("Drift TOWARD bearing",movement.driftBearing,0,359.9f,"°");movement.driftInches=Slider("Drift distance",movement.driftInches,0,5,"″");
            if(Button("Apply drift to selected ship"))movement.Drift(movement.driftInches,movement.driftBearing);
            Text("Shift + click the sea places the ship at the exact point. Drift does not spend powered movement or change course. Ships remain rendered at their real dimensions.");
            if(Button("Reset movement ruler · umpire override")){chart.Remember();ship.movementUsed=0;scene.network?.SubmitShip(scene.selected);}
            GUI.enabled=true;
        }
        void ManualPivot(float angle){var ship=scene.scenario.ships[scene.selected];chart.Remember();ship.heading=Mathf.Repeat(ship.heading+angle,360);scene.network?.SubmitShip(scene.selected);}
    }
}
