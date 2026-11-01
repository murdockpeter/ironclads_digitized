using UnityEngine;

namespace Ironclads
{
    // Iron & Fire 3rd ed. pp.3,7 and turning-circle sheet p.52.
    // Exact tabletop conversion: 1 inch = 100 yd, 1 inch per knot per turn.
    public class CourseMovement : MonoBehaviour
    {
        public MaritimeScene scene;
        public float segment=1,driftBearing,driftInches=.5f;
        public int turn; // -1 port, 0 straight, +1 starboard
        LineRenderer preview;
        public static Vector3 Endpoint(Vector3 origin,float heading,float inches,int turn,int turnClass,bool astern,out float finalHeading)
        {
            float length=inches*RuleChart.HexMeters*(astern?-1:1),angle=0;
            Vector3 local;
            if(turn==0){local=Vector3.forward*length;}
            else{
                float radius=Mathf.Clamp(turnClass,1,5)*RuleChart.HexMeters;
                angle=length/radius*turn;
                local=new Vector3(turn*radius*(1-Mathf.Cos(angle)),0,turn*radius*Mathf.Sin(angle));
            }
            finalHeading=Mathf.Repeat(heading+angle*Mathf.Rad2Deg,360);
            return origin+Quaternion.Euler(0,heading,0)*local;
        }
        bool Own(int ship)=>scene.network==null||scene.network.CanControl(ship);
        public void Move(float inches,int direction)
        {
            int i=scene.selected;if(!scene.scenario.Miniatures||!Own(i))return;var ship=scene.scenario.ships[i];
            if(!float.IsFinite(inches)||inches<=0||inches>Remaining(ship)+.001f){scene.status="Segment exceeds the remaining movement ruler. Adjust current speed if the umpire orders an override.";return;}
            Vector3 end=Endpoint(ship.Position,ship.heading,inches,direction,ship.turnClass,ship.astern,out float heading);
            if(!Inside(end)){scene.status="Movement would leave the 20 km local scene.";return;}
            GetComponent<RuleChart>().Remember();ship.east=end.x;ship.north=end.z;ship.heading=heading;ship.movementUsed+=inches;
            scene.network?.SubmitShip(i);scene.status=$"{ship.name}: moved {inches:0.00} tabletop inches / {inches*100:0} yards. Course {heading:0.0}° true.";
        }
        public static float Remaining(ShipState ship)=>Mathf.Max(0,ship.speed-ship.movementUsed);
        public static bool Inside(Vector3 point)=>float.IsFinite(point.x)&&float.IsFinite(point.z)&&Mathf.Abs(point.x)<=20000&&Mathf.Abs(point.z)<=20000;
        public void PlaceExact(Vector3 point)
        {
            if(!scene.scenario.Miniatures||!Own(scene.selected)||!Inside(point))return;
            GetComponent<RuleChart>().Remember();var ship=scene.scenario.ships[scene.selected];ship.east=point.x;ship.north=point.z;
            scene.network?.SubmitShip(scene.selected);scene.status="Exact placement set by umpire; movement ruler retained.";
        }
        public void Drift(float inches,float bearing)
        {
            if(!scene.scenario.Miniatures||!Own(scene.selected))return;var ship=scene.scenario.ships[scene.selected];
            Vector3 end=ship.Position+Quaternion.Euler(0,bearing,0)*Vector3.forward*(inches*RuleChart.HexMeters);
            if(!Inside(end))return;GetComponent<RuleChart>().Remember();ship.east=end.x;ship.north=end.z;scene.network?.SubmitShip(scene.selected);
            scene.status="Manual drift applied; course and powered movement ruler retained.";
        }
        void LateUpdate()
        {
            if(scene==null||scene.seaRoot==null)return;
            if(preview==null){var mat=new Material(Shader.Find("Ironclads/Chart"));mat.SetColor("_Color",new Color(.5f,1,.8f,.9f));preview=WorldBuilder.Line(scene.seaRoot,"Course movement preview",new Vector3[2],mat,.65f);}
            bool show=scene.scenario.Miniatures&&!scene.globeMode&&scene.showOverlay;preview.gameObject.SetActive(show);if(!show)return;
            var ship=scene.scenario.ships[scene.selected];float inches=Mathf.Min(segment,Remaining(ship));var points=new Vector3[65];
            for(int i=0;i<points.Length;i++)points[i]=Endpoint(ship.Position,ship.heading,inches*i/64,turn,ship.turnClass,ship.astern,out _)+Vector3.up*.4f;
            preview.positionCount=points.Length;preview.SetPositions(points);
            if(Input.GetKey(KeyCode.LeftShift)&&Input.GetMouseButtonDown(0)&&(!scene.showOverlay||Input.mousePosition.x>330)){
                var plane=new Plane(Vector3.up,Vector3.zero);var ray=scene.view.ScreenPointToRay(Input.mousePosition);if(plane.Raycast(ray,out float distance))PlaceExact(ray.GetPoint(distance));
            }
        }
    }
}
