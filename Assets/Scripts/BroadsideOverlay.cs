using UnityEngine;

namespace Ironclads
{
    // Offset-apex measuring template inspired by the user's IMG_1604 sketch.
    public class BroadsideOverlay : MonoBehaviour
    {
        public MaritimeScene scene;
        public bool visible;
        public int side=1; // 0 port, 1 starboard, 2 both
        public float innerWidth=16, outerWidth=40, offset=1, reach=6;
        public int template;
        readonly LineRenderer[] outlines=new LineRenderer[4];
        public static Vector3 Direction(float heading)=>Quaternion.Euler(0,heading,0)*Vector3.forward;
        public static Vector3 Apex(Vector3 center,float heading,int sign,float distance)=>center-Direction(heading+sign*90)*distance;
        public static bool Contains(Vector3 center,float heading,int sign,float distance,float sweep,float length,Vector3 target)
        {
            Vector3 axis=Direction(heading+sign*90),delta=target-Apex(center,heading,sign,distance);
            delta.y=0;
            return Vector3.Dot(target-center,axis)>.001f&&delta.magnitude<=length+.001f&&GunneryTools.Within(GunneryTools.Bearing(delta),heading+sign*90,sweep);
        }
        GunneryTools Guns=>GetComponent<GunneryTools>();
        public Vector3 Center=>scene.scenario.DoB?scene.scenario.ships[scene.selected].Position:Guns.Origin;
        public float Heading=>scene.scenario.DoB?scene.scenario.ships[scene.selected].heading:Guns.FiringState.heading;
        public Vector3 Target=>scene.scenario.DoB?scene.scenario.ships[scene.dawn.target].Position:Guns.TargetPoint;
        public void FrameView()
        {
            GetComponent<RuleChart>().topDown=true;scene.globeMode=false;scene.target=Center;
            scene.distance=Mathf.Max(1100,reach*RuleChart.HexMeters*3.2f);
        }
        public string Readout(int sign)
        {
            string label=sign<0?"Port":"Starboard";
            bool inner=Contains(Center,Heading,sign,offset*RuleChart.HexMeters,innerWidth,reach*RuleChart.HexMeters,Target);
            bool outer=Contains(Center,Heading,sign,offset*RuleChart.HexMeters,outerWidth,reach*RuleChart.HexMeters,Target);
            bool split=!scene.scenario.Miniatures&&!scene.scenario.DoB&&
                (Crosses(sign,innerWidth)||Crosses(sign,outerWidth));
            Vector3 delta=Target-Apex(Center,Heading,sign,offset*RuleChart.HexMeters);
            float angle=Mathf.Abs(Mathf.DeltaAngle(Heading+sign*90,GunneryTools.Bearing(delta)));
            string measurement=delta.sqrMagnitude<.000001f?"At pivot; angle undefined.":$"{angle:0.0} degrees off broadside from pivot.";
            return label+": "+measurement+" Target center "+(inner?"within inner arc":outer?"within outer arc only":"outside arcs")+"."+(split?" Angle edge crosses target hex.":"");
        }
        bool Crosses(int sign,float width)=>GunneryTools.RayCrossesHex(Apex(Center,Heading,sign,offset*RuleChart.HexMeters),Heading+sign*90-width/2,Guns.Target)||GunneryTools.RayCrossesHex(Apex(Center,Heading,sign,offset*RuleChart.HexMeters),Heading+sign*90+width/2,Guns.Target);
        void LateUpdate()
        {
            if(scene==null||scene.seaRoot==null)return;
            bool show=visible&&scene.showOverlay&&!scene.globeMode;
            for(int i=0;i<4;i++) {
                int bank=i/2;bool inner=i%2==0;float width=inner?innerWidth:outerWidth;
                if(outlines[i]==null&&show){
                    var material=new Material(Shader.Find("Ironclads/Chart"));material.SetColor("_Color",inner?new Color(1,.72f,.3f,.95f):new Color(.35f,.85f,1,.95f));
                    outlines[i]=WorldBuilder.Line(scene.seaRoot,(bank==0?"Port":"Starboard")+(inner?" inner":" outer")+" offset measuring arc",new Vector3[2],material,inner?1.3f:.9f);
                }
                if(outlines[i]==null)continue;
                outlines[i].gameObject.SetActive(show&&(side==2||side==bank));
                if(!outlines[i].gameObject.activeSelf)continue;
                int sign=bank==0?-1:1;
                Vector3 apex=Apex(Center,Heading,sign,offset*RuleChart.HexMeters)+Vector3.up*.65f;
                var points=new Vector3[51];points[0]=points[50]=apex;
                for(int j=0;j<=48;j++)points[j+1]=apex+Direction(Heading+sign*90-width/2+width*j/48)*reach*RuleChart.HexMeters;
                outlines[i].positionCount=points.Length;outlines[i].SetPositions(points);
            }
        }
    }

    public partial class UmpirePanel
    {
        void BroadsideMeasurement()
        {
            var tool=GetComponent<BroadsideOverlay>();
            Heading("OFFSET BROADSIDE MEASUREMENT");
            tool.visible=GUILayout.Toggle(tool.visible,"Show inner / outer measuring arcs",toggle);
            if(!tool.visible)return;
            tool.side=GUILayout.SelectionGrid(tool.side,new[]{"Port","Starboard","Both"},3,button);
            int chosen=GUILayout.SelectionGrid(tool.template,new[]{"Confederate 16 / 40","Wood / Union 20 / 50","Custom angles"},1,button);
            if(chosen!=tool.template){tool.template=chosen;if(chosen<2){tool.innerWidth=chosen==0?16:20;tool.outerWidth=chosen==0?40:50;}}
            Text($"Inner {tool.innerWidth:0.#} degrees / outer {tool.outerWidth:0.#} degrees (full widths)");
            if(tool.template==2){tool.innerWidth=Slider("Inner arc",tool.innerWidth,1,90," degrees");tool.outerWidth=Slider("Outer arc",tool.outerWidth,tool.innerWidth,180," degrees");}
            tool.offset=Slider("Pivot across ship",tool.offset,0,3," x 100 yd");
            tool.reach=Slider("Length from pivot",tool.reach,Mathf.Max(1,tool.offset+.1f),30," x 100 yd");
            Text($"Pivot offset: {tool.offset*100:0} yd / {tool.offset*RuleChart.HexMeters:0.0} m. Follows the ship's heading.");
            if(Button("Overhead measuring view"))tool.FrameView();
            if(tool.side!=1)Text(tool.Readout(-1));if(tool.side!=0)Text(tool.Readout(1));
            Text("Amber: inner arc. Blue: outer arc. Both share a pivot on the opposite side of the ship. Offset is adjustable; the sketch supplies no exact distance. Uses the current gunnery target. Measurement only.");
        }
    }
}
