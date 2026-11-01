using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Ironclads
{
    public readonly struct Hex : IEquatable<Hex>
    {
        public readonly int q,r;
        public Hex(int q,int r){this.q=q;this.r=r;}
        public bool Equals(Hex other)=>q==other.q&&r==other.r;
        public override bool Equals(object o)=>o is Hex h&&Equals(h);
        public override int GetHashCode()=>q*397^r;
        public static Hex operator +(Hex a,Hex b)=>new Hex(a.q+b.q,a.r+b.r);
        public override string ToString() => (q+21).ToString("00")+(r+Mathf.FloorToInt(q/2f)+19).ToString("00");
    }
    public class RuleChart : MonoBehaviour
    {
        public const float HexMeters=91.44f, TurnSeconds=180;
        public static readonly float Radius=HexMeters/Mathf.Sqrt(3);
        public static readonly Hex[] Directions={new Hex(0,1),new Hex(1,0),new Hex(1,-1),new Hex(0,-1),new Hex(-1,0),new Hex(-1,1)};
        public static readonly string[] Facings={"N","NE","SE","S","SW","NW"};
        public MaritimeScene scene;
        public bool showGrid=true,showLabels=true,showRange=true,showArcs;
        public bool topDown;
        public static Vector3 Center(Hex h)=>new Vector3(1.5f*Radius*h.q,0,HexMeters*(h.r+h.q*.5f));
        public static Hex At(Vector3 p)
        {
            float q=p.x/(1.5f*Radius),r=p.z/HexMeters-q*.5f,s=-q-r;
            int qi=Mathf.RoundToInt(q),ri=Mathf.RoundToInt(r),si=Mathf.RoundToInt(s);
            float dq=Mathf.Abs(q-qi),dr=Mathf.Abs(r-ri),ds=Mathf.Abs(s-si);
            if(dq>dr&&dq>ds)qi=-ri-si;else if(dr>ds)ri=-qi-si;
            return new Hex(qi,ri);
        }
        public static int Range(Hex a,Hex b)=>Mathf.Max(Mathf.Abs(a.q-b.q),Mathf.Abs(a.r-b.r),Mathf.Abs((a.q+a.r)-(b.q+b.r)));
        public static int Facing(float heading)=>((Mathf.RoundToInt(heading/60)%6)+6)%6;
        public static bool OnBoard(Hex h){int row=h.r+Mathf.FloorToInt(h.q/2f);return h.q>=-20&&h.q<=20&&row>=-18&&row<=18;}
        GameObject grid;LineRenderer rangeLine;LineRenderer[] rings=new LineRenderer[2],headings=new LineRenderer[2];LineRenderer[] arcs;
        readonly Stack<string> undo=new Stack<string>();
        GUIStyle label;Material chartMat;
        public void Remember(){undo.Push(JsonUtility.ToJson(scene.scenario));if(undo.Count>100)undo.Clear();}
        public void Undo(){if(scene.network!=null&&!scene.network.CanManage)return;if(undo.Count>0){scene.scenario=JsonUtility.FromJson<Scenario>(undo.Pop());scene.status="Previous board position restored.";}}
        void Start(){Build();if(scene.scenario.hexMode)Snap();}
        public void Build()
        {
            if(grid!=null)return;
            chartMat=new Material(Shader.Find("Ironclads/Chart"));chartMat.SetColor("_Color",new Color(.35f,.74f,.75f,.22f));
            grid=new GameObject("100 yard hex chart — custom local coordinates");grid.transform.SetParent(scene.seaRoot,false);
            var verts=new List<Vector3>();var indices=new List<int>();
            for(int q=-20;q<=20;q++)for(int row=-18;row<=18;row++) {
                Hex h=new Hex(q,row-Mathf.FloorToInt(q/2f));Vector3 center=Center(h);
                for(int i=0;i<6;i++) {
                    float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;
                    int k=verts.Count;verts.Add(center+new Vector3(Mathf.Cos(a)*Radius,.13f,Mathf.Sin(a)*Radius));
                    verts.Add(center+new Vector3(Mathf.Cos(b)*Radius,.13f,Mathf.Sin(b)*Radius));indices.Add(k);indices.Add(k+1);
                }
            }
            var mesh=new Mesh {name="100 yard hexagons",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.SetVertices(verts);mesh.SetIndices(indices.ToArray(),MeshTopology.Lines,0);mesh.RecalculateBounds();
            grid.AddComponent<MeshFilter>().sharedMesh=mesh;grid.AddComponent<MeshRenderer>().sharedMaterial=chartMat;
            var highlight=new Material(chartMat);highlight.SetColor("_Color",new Color(.98f,.68f,.28f,.8f));
            rangeLine=WorldBuilder.Line(scene.seaRoot,"Range between occupied hex centers",new Vector3[2],highlight,.65f);
            for(int n=0;n<2;n++) {
                rings[n]=WorldBuilder.Line(scene.seaRoot,"Occupied hex",new Vector3[7],highlight,.6f);
                headings[n]=WorldBuilder.Line(scene.seaRoot,"Bow direction arrow",new Vector3[5],highlight,.65f);
            }
            var arcMat=new Material(chartMat);arcMat.SetColor("_Color",new Color(.65f,.83f,.76f,.6f));
            arcs=new LineRenderer[3];for(int i=0;i<3;i++)arcs[i]=WorldBuilder.Line(scene.seaRoot,"Stonewall sheet arc guide",new Vector3[33],arcMat,.55f);
        }
        void LateUpdate()=>Refresh();
        public void Refresh()
        {
            if(scene==null||scene.seaRoot==null)return;if(grid==null)Build();
            Shader.SetGlobalFloat("_ChartWaveHeight",scene.scenario.waveHeight);
            grid.SetActive(showGrid&&!scene.scenario.Miniatures&&!scene.globeMode&&scene.showOverlay);
            rangeLine.gameObject.SetActive(showRange&&!scene.globeMode&&scene.showOverlay);
            var s=scene.scenario;
            for(int i=0;i<2;i++) {
                Vector3 center=(s.Miniatures?s.ships[i].Position:Center(At(s.ships[i].Position)))+Vector3.up*.2f;
                rings[i].gameObject.SetActive(showGrid&&!s.Miniatures&&!scene.globeMode&&scene.showOverlay);
                headings[i].gameObject.SetActive((s.Miniatures?showRange:showGrid)&&!scene.globeMode&&scene.showOverlay);
                var pts=new Vector3[7];for(int k=0;k<7;k++)pts[k]=center+new Vector3(Mathf.Cos(k*Mathf.PI/3)*Radius,0,Mathf.Sin(k*Mathf.PI/3)*Radius);
                rings[i].SetPositions(pts);
                Quaternion rotation=Quaternion.Euler(0,s.ships[i].heading,0);Vector3 forward=rotation*Vector3.forward,right=rotation*Vector3.right,tip=center+forward*Radius*.8f;
                headings[i].SetPositions(new[]{center,tip,tip-forward*7+right*4,tip,tip-forward*7-right*4});
            }
            rangeLine.SetPosition(0,(s.Miniatures?s.ships[0].Position:Center(At(s.ships[0].Position)))+Vector3.up*.25f);
            rangeLine.SetPosition(1,(s.Miniatures?s.ships[1].Position:Center(At(s.ships[1].Position)))+Vector3.up*.25f);
            for(int i=0;i<3;i++) {
                arcs[i].gameObject.SetActive(showArcs&&!scene.globeMode&&scene.selected==1&&!scene.scenario.Hampton&&scene.showOverlay);
                Vector3 origin=s.ships[1].Position+Vector3.up*.2f;
                float start=s.ships[1].heading+(i==0?-90:i==1?-150:30),sweep=i==0?180:120;
                var pts=new Vector3[33];pts[0]=origin;pts[32]=origin;
                for(int k=1;k<32;k++)pts[k]=origin+Quaternion.Euler(0,start+sweep*(k-1)/30f,0)*Vector3.forward*(HexMeters*2);
                arcs[i].SetPositions(pts);
            }
            if(!scene.globeMode&&s.hexMode&&Input.GetKey(KeyCode.LeftShift)&&Input.GetMouseButtonDown(0)&&(!scene.showOverlay||Input.mousePosition.x>330)) {
                var plane=new Plane(Vector3.up,Vector3.zero);var ray=scene.view.ScreenPointToRay(Input.mousePosition);
                if(plane.Raycast(ray,out float d))Place(At(ray.GetPoint(d)));
            }
            if(topDown&&!scene.globeMode) {
                scene.view.orthographic=true;scene.view.orthographicSize=scene.distance*.45f;
                scene.view.transform.position=scene.target+Vector3.up*scene.distance;scene.view.transform.rotation=Quaternion.Euler(90,0,0);
            } else scene.view.orthographic=false;
        }
        public void Place(Hex h)
        {
            if(scene.network!=null&&!scene.network.CanControl(scene.selected)){scene.status="You can place only your assigned ship.";return;}
            if(!OnBoard(h)){scene.status="Outside the local 41 × 37 hex board.";return;}
            Remember();var s=scene.scenario.ships[scene.selected];Vector3 p=Center(h);s.east=p.x;s.north=p.z;
            scene.status=$"Placed {s.name} in local hex {h}.";
            scene.network?.SubmitShip(scene.selected);
        }
        public void Snap()
        {
            if(scene.scenario.Miniatures||(scene.network!=null&&!scene.network.CanManage))return;
            Remember();foreach(var s in scene.scenario.ships){var p=Center(At(s.Position));s.east=p.x;s.north=p.z;s.heading=Facing(s.heading)*60;}
        }
        public void Move(int sign) {var ship=scene.scenario.ships[scene.selected];Hex h=At(ship.Position),d=Directions[Facing(ship.heading)];Place(new Hex(h.q+sign*d.q,h.r+sign*d.r));}
        public void Turn(int sign){if(scene.network!=null&&!scene.network.CanControl(scene.selected))return;Remember();var s=scene.scenario.ships[scene.selected];s.heading=((Facing(s.heading)+sign+6)%6)*60;scene.network?.SubmitShip(scene.selected);}
        public void NextTurn(){if(scene.network!=null&&!scene.network.CanManage)return;Remember();scene.animateTime=false;scene.scenario.utc=scene.scenario.Date.AddSeconds(TurnSeconds).ToString("yyyy-MM-ddTHH:mm:ssZ");scene.scenario.turn++;if(scene.scenario.Miniatures)foreach(var ship in scene.scenario.ships){ship.speed=ship.orderedSpeed;ship.astern=ship.orderedAstern;ship.movementUsed=0;}scene.status="Turn advanced 3 minutes. Ship positions remain where the umpire placed them.";}
        public void Export()
        {
            bool mini=scene.scenario.Miniatures;
            string path=Path.Combine(Application.persistentDataPath,mini?"iron-fire-positions.csv":"umpire-positions.csv");
            bool header=!File.Exists(path);var sb=new StringBuilder();if(header)sb.AppendLine(mini?"turn,utc,ship,local_hex,east_m,north_m,heading_deg,speed_kn,ordered_speed_kn,astern,ordered_astern,movement_used_in,turn_class,notes":"turn,utc,ship,local_hex,east_m,north_m,heading_deg,notes");
            foreach(var s in scene.scenario.ships) {
                sb.Append(scene.scenario.turn).Append(',').Append(scene.scenario.utc).Append(',').Append(s.name).Append(',').Append(At(s.Position)).Append(',');
                sb.Append(s.east.ToString("F3",CultureInfo.InvariantCulture)).Append(',').Append(s.north.ToString("F3",CultureInfo.InvariantCulture)).Append(',').Append(s.heading.ToString("F1",CultureInfo.InvariantCulture)).Append(',');
                if(mini)sb.Append(s.speed.ToString("F3",CultureInfo.InvariantCulture)).Append(',').Append(s.orderedSpeed.ToString("F3",CultureInfo.InvariantCulture)).Append(',').Append(s.astern).Append(',').Append(s.orderedAstern).Append(',').Append(s.movementUsed.ToString("F3",CultureInfo.InvariantCulture)).Append(',').Append(s.turnClass).Append(',');
                sb.Append('"').Append(scene.scenario.umpireNotes.Replace("\"","\"\"")).AppendLine("\"");
            }
            File.AppendAllText(path,sb.ToString());scene.status="Position log appended: "+path;
        }
        void OnGUI()
        {
            if(scene==null||scene.scenario.Miniatures||scene.globeMode||!scene.showOverlay||!showGrid||!showLabels||scene.distance<220)return;
            if(label==null)label=new GUIStyle(GUI.skin.label){fontSize=10,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.67f,.85f,.83f,.65f)}};
            Hex near=At(scene.target);int radius=Mathf.Clamp(Mathf.CeilToInt(scene.distance/HexMeters),4,25);
            for(int q=near.q-radius;q<=near.q+radius;q++)for(int r=near.r-radius;r<=near.r+radius;r++) {
                Hex h=new Hex(q,r);if(!OnBoard(h))continue;
                Vector3 p=scene.view.WorldToScreenPoint(Center(h));
                if(p.z>0&&p.x>350&&p.x<Screen.width-20&&p.y>60&&p.y<Screen.height-100)
                    GUI.Label(new Rect(p.x-24,Screen.height-p.y-7,48,14),h.ToString(),label);
            }
        }
    }
}
