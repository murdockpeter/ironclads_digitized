using UnityEngine;

namespace Ironclads
{
    public class PatrickHenryControl : MonoBehaviour
    {
        public MaritimeScene scene;
        public bool Own=>scene.scenario.Hampton&&(scene.network==null||scene.network.CanControl(3));
        public bool Place(Vector3 point)
        {
            if(!Own||!CourseMovement.Inside(point))return false;
            if(!scene.scenario.Miniatures){var hex=RuleChart.At(point);if(!RuleChart.OnBoard(hex,scene.scenario.Hampton))return false;point=RuleChart.Center(hex);}
            GetComponent<RuleChart>().Remember();var ship=scene.scenario.patrickHenry;ship.east=point.x;ship.north=point.z;scene.network?.SubmitShip(3);return true;
        }
        public bool HexMove(int sign)
        {
            if(!Own||scene.scenario.Miniatures)return false;var ship=scene.scenario.patrickHenry;var hex=RuleChart.At(ship.Position);var d=RuleChart.Directions[RuleChart.Facing(ship.heading)];return Place(RuleChart.Center(new Hex(hex.q+sign*d.q,hex.r+sign*d.r)));
        }
        public bool Move(float inches,int turn)
        {
            if(!Own||!scene.scenario.Miniatures||!float.IsFinite(inches)||inches<=0||Mathf.Abs(turn)>1)return false;
            var ship=scene.scenario.patrickHenry;if(inches>CourseMovement.Remaining(ship)+.001f)return false;
            Vector3 end=CourseMovement.Endpoint(ship.Position,ship.heading,inches,turn,ship.turnClass,ship.astern,out float heading);if(!CourseMovement.Inside(end))return false;
            GetComponent<RuleChart>().Remember();ship.east=end.x;ship.north=end.z;ship.heading=heading;ship.movementUsed+=inches;scene.network?.SubmitShip(3);return true;
        }
        public void TurnHex(int sign)
        {
            if(!Own||scene.scenario.Miniatures)return;GetComponent<RuleChart>().Remember();var ship=scene.scenario.patrickHenry;ship.heading=Mathf.Repeat((RuleChart.Facing(ship.heading)+sign)*60,360);scene.network?.SubmitShip(3);
        }
    }
}
