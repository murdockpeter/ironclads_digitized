using UnityEngine;

namespace Ironclads
{
    public static class JamesScenario
    {
        public static readonly string[] Names={"USS Onondaga","USS Mahopac","USS Sangamon","CSS Stonewall","CSS Virginia II","CSS Fredericksburg"};
        public static readonly float[] Lengths={68.8848f,71.628f,60.96f,52.3756f,60.0456f,57.3024f};
        public static readonly float[] Beams={15.0114f,13.3096f,14.0208f,9.9568f,14.478f,12.2682f};
        public static readonly float[] Decks={.65f,.55f,.55f,1.85f,.65f,.65f};
        public static readonly int[] Guns={4,2,2,3,4,4};
        public static readonly string[] Batteries={"Two turrets: each XV-inch Dahlgren + 150-pdr Parrott","Two XV-inch Dahlgrens in one turret","XV-inch + XI-inch Dahlgrens in one turret","300-pdr forward + two 70-pdr Armstrongs aft","7-inch Brooke forward, two 6.4-inch Brooke broadside, XI-inch aft (supplied card)","8-inch rifle forward, two 6.4-inch rifles broadside, XI-inch smoothbore aft (DANFS working outfit)"};
        public static readonly Vector3[] Funnels={new Vector3(0,10,-1),new Vector3(0,8,-10),new Vector3(0,8,-10),new Vector3(0,12.05f,0),new Vector3(0,10,-2),new Vector3(0,9.5f,-1)};
        public static Scenario Create()
        {
            var s=new Scenario{scenarioId="james-1865",rulesetId="ironclads",hexMode=true,latitude=36.983,longitude=-76.385,utc="1865-03-28T14:00:00Z",windKnots=7,windFrom=270,cloud=.22f,waveHeight=.25f,visibilityKm=12,ships=new ShipState[6],umpireNotes="Stonewall at the Gates: alternate history. Stonewall arrives early; Virginia II and Fredericksburg have passed the upper-James obstructions. Union holds the roadstead. See module briefing for suggested objectives. Combat and movement allowances are umpire-adjudicated."};
            // Two lines three hexes apart, twenty hexes between corresponding opponents.
            for(int i=0;i<6;i++){int lane=i%3;var p=RuleChart.Center(new Hex(8+lane*3,i<3?-18:2));s.ships[i]=new ShipState(Names[i],p.x,p.z,i<3?0:180){engineLoad=.4f};}
            return s;
        }
    }
    public class JamesTurret : MonoBehaviour { public int group; }
}
