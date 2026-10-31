using UnityEngine;

namespace Ironclads
{
    // Measuring aids only. No loading, hit, damage, obstruction or fire eligibility decisions.
    public class GunneryTools : MonoBehaviour
    {
        public MaritimeScene scene;
        public bool visible, rangeRings=true, pickedTarget;
        public Hex targetHex;
        public int preset;
        public float reach=6, customSweep=120, customCenter=90;
        public static readonly string[] Presets={"Stonewall: bow 300-pdr / 180°","Stonewall: port 70-pdr / 120°","Stonewall: starboard 70-pdr / 120°","Custom angle guide"};
        LineRenderer sector, sight, axis, targetOutline;
        readonly LineRenderer[] bands=new LineRenderer[12];
        public float Sweep=>preset==0?180:preset<3?120:customSweep;
        public float Offset=>preset==0?0:preset==1?-90:preset==2?90:customCenter;
        public Vector3 Origin=>RuleChart.Center(RuleChart.At(scene.scenario.ships[scene.selected].Position));
        public Hex Target=>pickedTarget?targetHex:RuleChart.At(scene.scenario.ships[1-scene.selected].Position);
        public float AxisBearing=>Mathf.Repeat(scene.scenario.ships[scene.selected].heading+Offset,360);
        public static float Bearing(Vector3 delta)=>Mathf.Repeat(Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,360);
        public static bool Within(float bearing,float axis,float sweep)=>Mathf.Abs(Mathf.DeltaAngle(axis,bearing))<=sweep*.5f+.0001f;
        static Vector3 Direction(float bearing)=>Quaternion.Euler(0,bearing,0)*Vector3.forward;
        public bool SameHex=>RuleChart.At(Origin).Equals(Target);
        public float TargetBearing=>Bearing(RuleChart.Center(Target)-Origin);
        public int Range=>RuleChart.Range(RuleChart.At(Origin),Target);
        // A ray crossing the target polygon flags split-hex cases for the umpire.
        public static bool RayCrossesHex(Vector3 origin,float bearing,Hex hex)
        {
            Vector3 center=RuleChart.Center(hex),d=Direction(bearing);
            float min=float.PositiveInfinity,max=float.NegativeInfinity,forward=float.NegativeInfinity;
            for(int i=0;i<6;i++) {
                Vector3 v=center+new Vector3(Mathf.Cos(i*Mathf.PI/3)*RuleChart.Radius,0,Mathf.Sin(i*Mathf.PI/3)*RuleChart.Radius)-origin;
                float side=d.z*v.x-d.x*v.z;min=Mathf.Min(min,side);max=Mathf.Max(max,side);forward=Mathf.Max(forward,Vector3.Dot(v,d));
            }
            return min<=.001f&&max>=-.001f&&forward>0;
        }
        public bool SplitHex=>!SameHex&&Sweep<359.99f&&(RayCrossesHex(Origin,AxisBearing-Sweep/2,Target)||RayCrossesHex(Origin,AxisBearing+Sweep/2,Target));
        LineRenderer Line(string name,Color color,float width)
        {
            var material=new Material(Shader.Find("Ironclads/Chart"));material.SetColor("_Color",color);
            return WorldBuilder.Line(scene.seaRoot,name,new Vector3[2],material,width);
        }
        void Build()
        {
            sector=Line("Gunnery angle boundary",new Color(1,.66f,.24f,.95f),.9f);
            axis=Line("Gunnery angle center",new Color(1,.66f,.24f,.5f),.45f);
            sight=Line("Umpire sighting ruler",new Color(.5f,1,.85f,.95f),.9f);
            targetOutline=Line("Gunnery target hex",new Color(.5f,1,.85f,.95f),1);
            for(int i=0;i<bands.Length;i++)bands[i]=Line("Gunnery range band "+(i+1),new Color(.5f,.9f,.8f,.36f),.45f);
        }
        void Positions(LineRenderer line,Vector3[] points){line.positionCount=points.Length;line.SetPositions(points);}
        void LateUpdate()
        {
            if(scene==null||scene.seaRoot==null)return;if(sector==null)Build();
            if(scene.selected==0&&preset<3)preset=3;
            bool show=visible&&!scene.globeMode&&scene.showOverlay;
            foreach(var line in new[]{sector,axis,sight,targetOutline})line.gameObject.SetActive(show);
            for(int i=0;i<bands.Length;i++)bands[i].gameObject.SetActive(show&&rangeRings&&i<Mathf.RoundToInt(reach));
            if(!show)return;
            // Alt-click measures; Shift-click remains placement only.
            if(Input.GetKey(KeyCode.LeftAlt)&&!Input.GetKey(KeyCode.LeftShift)&&Input.GetMouseButtonDown(0)&&Input.mousePosition.x>330&&Input.mousePosition.y>55&&Input.mousePosition.y<Screen.height-100) {
                var plane=new Plane(Vector3.up,Vector3.zero);var ray=scene.view.ScreenPointToRay(Input.mousePosition);
                if(plane.Raycast(ray,out float distance)){var h=RuleChart.At(ray.GetPoint(distance));if(RuleChart.OnBoard(h)){targetHex=h;pickedTarget=true;}}
            }
            Vector3 origin=Origin+Vector3.up*.38f,target=RuleChart.Center(Target)+Vector3.up*.38f;
            var points=new Vector3[99];points[0]=origin;points[98]=origin;
            for(int i=0;i<=96;i++)points[i+1]=origin+Direction(AxisBearing-Sweep/2+Sweep*i/96)*reach*RuleChart.HexMeters;
            Positions(sector,points);Positions(axis,new[]{origin,origin+Direction(AxisBearing)*reach*RuleChart.HexMeters});Positions(sight,new[]{origin,target});
            var hex=new Vector3[7];for(int i=0;i<7;i++)hex[i]=target+new Vector3(Mathf.Cos(i*Mathf.PI/3)*RuleChart.Radius,0,Mathf.Sin(i*Mathf.PI/3)*RuleChart.Radius);Positions(targetOutline,hex);
            for(int i=0;i<bands.Length;i++)if(bands[i].gameObject.activeSelf) {
                var ring=new Vector3[97];for(int j=0;j<ring.Length;j++)ring[j]=origin+Direction(AxisBearing-Sweep/2+Sweep*j/96)*(i+1)*RuleChart.HexMeters;Positions(bands[i],ring);
            }
        }
    }
}
