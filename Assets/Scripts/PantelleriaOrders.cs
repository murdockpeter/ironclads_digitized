using System.Linq;
using UnityEngine;

namespace Ironclads
{
    public partial class DawnNavalTable
    {
        int[] CommandableDivision(int division)
        {
            if(!scene.scenario.Pantelleria||division<0||division>=14)return new int[0];
            var ships=PantelleriaScenario.Members(division).Where(i=>!scene.scenario.pantelleria.sunk[i]).ToArray();
            if(ships.Any(i=>!Own(i))){scene.status="You must command every afloat ship in this division to issue a group action.";return new int[0];}return ships;
        }
        public bool PlotDivision(int division,float course,float speed)
        {
            if(!float.IsFinite(course)||course<0||course>=360||!float.IsFinite(speed)||speed<0||speed>35)return false;
            var ships=CommandableDivision(division);if(ships.Length==0)return false;GetComponent<RuleChart>().Remember();
            foreach(int i in ships){var s=scene.scenario.ships[i];s.dobOrderedHeading=course;s.orderedSpeed=speed;scene.network?.SubmitShip(i);}scene.status="Division course and speed plotted. Apply when the umpire permits; positions unchanged.";return true;
        }
        public bool ApplyDivision(int division)
        {
            var ships=CommandableDivision(division);if(ships.Length==0)return false;GetComponent<RuleChart>().Remember();
            foreach(int i in ships){var s=scene.scenario.ships[i];s.heading=s.dobOrderedHeading;s.speed=s.orderedSpeed;scene.network?.SubmitShip(i);}scene.status="Division plotted course and speed applied as umpire overrides. Positions unchanged.";return true;
        }
        public bool MoveDivision(int division,float seconds)
        {
            if(!float.IsFinite(seconds)||seconds<=0)return false;var ships=CommandableDivision(division);if(ships.Length==0)return false;
            var ends=ships.Select(i=>scene.scenario.ships[i].Position+Quaternion.Euler(0,scene.scenario.ships[i].heading,0)*Vector3.forward*UlsanScenario.Distance(scene.scenario.ships[i].speed*(scene.scenario.ships[i].astern?-1:1),seconds)).ToArray();
            if(ships.Any(i=>seconds>Remaining(i)+.01f)||ends.Any(p=>!UlsanScenario.Inside(p))){scene.status="Division move rejected: every ship needs enough allowance and a destination inside the board.";return false;}
            GetComponent<RuleChart>().Remember();for(int k=0;k<ships.Length;k++){var s=scene.scenario.ships[ships[k]];s.east=ends[k].x;s.north=ends[k].z;s.dobMovedSeconds+=seconds;scene.network?.SubmitShip(ships[k]);}scene.status="Division moved "+seconds.ToString("0")+" seconds; clock unchanged.";return true;
        }
    }
}
