using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    // Real-world geographic basemap; ship marker meshes remain deliberately generic and enlarged.
    public class CampaignBoard : MonoBehaviour
    {
        public static readonly string[] StationNames={"Upper James","Trent's Reach","Deep Bottom","City Point","Lower James","Hampton Roads","Virginia Capes","Open Atlantic"};
        public static Vector3[] Stations { get {var points=new Vector3[8];var source=CampaignGeography.Data.stations;for(int i=0;i<8;i++)points[i]=CampaignGeography.Point(source[i].latitude,source[i].longitude);return points;} }
        public BreakoutDesk desk;
        public Camera view;
        public readonly List<Transform> tokens=new List<Transform>();
        Transform world;
        Vector3 target=new Vector3(-10000,0,0);
        float yaw=0,pitch=70,distance=235000;
        bool overhead;
        Material union,confederate,hull,deck,iron,land,water,marker;
        LineRenderer selection;
        LineRenderer ruler;
        Vector3? rulerEnd;
        readonly List<Transform> stationMarkers=new List<Transform>();
        public float MarkerScale=>Mathf.Clamp(distance/6500,1,45);
        GUIStyle label,small;
        readonly List<Rect> labelRects=new List<Rect>();
        const int Layer=30;
        readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        readonly List<UnityEngine.Object> markerOwned=new List<UnityEngine.Object>();
        Material Material(string name,Color color){var m=Shipwright.MakeMaterial(name,color);owned.Add(m);return m;}
        void LayerAll(GameObject go){foreach(var t in go.GetComponentsInChildren<Transform>(true))t.gameObject.layer=Layer;}
        GameObject Shape(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;go.layer=Layer;return go;
        }
        public void Build()
        {
            world=new GameObject("James River geographic campaign board").transform;world.SetParent(transform,false);
            union=Material("Union blue",new Color(.20f,.50f,.88f));confederate=Material("Confederate red",new Color(.82f,.31f,.23f));
            hull=Material("Campaign hull",new Color(.10f,.13f,.15f));deck=Material("Campaign deck",new Color(.59f,.49f,.33f));iron=Material("Campaign armor",new Color(.35f,.39f,.40f));
            land=Material("USGS terrain",Color.white);land.shader=Shader.Find("Ironclads/Coastal Ground");land.SetFloat("_CampaignMap",1);land.SetTexture("_MainTex",Resources.Load<Texture2D>("Geography/JamesCampaignLandCover"));var mapSW=CampaignGeography.Point(CampaignGeography.South,CampaignGeography.West);var mapNE=CampaignGeography.Point(CampaignGeography.North,CampaignGeography.East);land.SetVector("_MapBounds",new Vector4(mapSW.x,mapSW.z,mapNE.x-mapSW.x,mapNE.z-mapSW.z));water=Material("Geographic water",new Color(.08f,.27f,.36f));marker=Material("Station brass",new Color(.94f,.73f,.35f));
            var southwest=CampaignGeography.Point(CampaignGeography.South,CampaignGeography.West);var northeast=CampaignGeography.Point(CampaignGeography.North,CampaignGeography.East);
            Shape(world,"Water under geographic land polygons",PrimitiveType.Cube,new Vector3(0,-30,0),new Vector3(northeast.x-southwest.x,60,northeast.z-southwest.z),water);
            var ground=new GameObject("Cached GSHHG coastline / USGS river polygons / 3DEP terrain");ground.transform.SetParent(world,false);var mesh=CampaignGeography.TerrainMesh();owned.Add(mesh);ground.AddComponent<MeshFilter>().sharedMesh=mesh;ground.AddComponent<MeshRenderer>().sharedMaterial=land;
            var stations=Stations;for(int i=0;i<stations.Length;i++)stationMarkers.Add(Shape(world,StationNames[i],PrimitiveType.Cylinder,stations[i]+Vector3.up*8,new Vector3(25,4,25),marker).transform);
            Shape(world,"Confederate unassigned staging — outside map",PrimitiveType.Cube,new Vector3(-100000,-30,22000),new Vector3(12000,60,18000),confederate);
            Shape(world,"Union unassigned staging — outside map",PrimitiveType.Cube,new Vector3(-100000,-30,-24000),new Vector3(12000,60,22000),union);
            RebuildTokens();
            var lineMat=new Material(Shader.Find("Sprites/Default"));lineMat.color=new Color(1,.8f,.25f);owned.Add(lineMat);
            selection=WorldBuilder.Line(world,"Selected vessel",new Vector3[49],lineMat,3);
            ruler=WorldBuilder.Line(world,"Projected straight-line distance",new Vector3[2],lineMat,3);ruler.gameObject.SetActive(false);
            var cameraObject=new GameObject("Campaign camera");cameraObject.transform.SetParent(transform,false);view=cameraObject.AddComponent<Camera>();view.cullingMask=1<<Layer;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.025f,.05f,.065f);view.farClipPlane=600000;view.nearClipPlane=5;view.fieldOfView=48;view.depth=5;
            var light=new GameObject("Campaign daylight");light.transform.SetParent(world,false);var lamp=light.AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=1.25f;lamp.cullingMask=1<<Layer;lamp.transform.rotation=Quaternion.Euler(55,-35,0);
            LayerAll(world.gameObject);Sync();
        }
        void RebuildTokens() {
            foreach(var token in tokens){token.gameObject.SetActive(false);Destroy(token.gameObject);}tokens.Clear();
            foreach(var mesh in markerOwned)Destroy(mesh);markerOwned.Clear();
            for(int i=0;i<desk.state.vessels.Count;i++) {
                var v=desk.state.vessels[i];var root=new GameObject(v.name+" / generic campaign marker").transform;root.SetParent(world,false);tokens.Add(root);
                CampaignMarker.Build(root,v.type,hull,deck,iron,v.side==CampaignSide.Union?union:confederate,true,markerOwned);CampaignMarker.ApplyIdentity(root,v);
                // Root collider provides a generous click target at fleet scale.
                var collider=root.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,24,0);collider.size=new Vector3(48,58,110);LayerAll(root.gameObject);
            }
        }
        bool TokenNamesMatch(){if(tokens.Count!=desk.state.vessels.Count)return false;for(int i=0;i<tokens.Count;i++)if(tokens[i].name!=desk.state.vessels[i].name+" / generic campaign marker")return false;return true;}
        public void Sync(){if(!TokenNamesMatch())RebuildTokens();if(tokens.Count==0)return;for(int i=0;i<tokens.Count;i++){var v=desk.state.vessels[i];float y=v.station>=0&&CampaignGeography.Land(v.x,v.z)?CampaignGeography.Height(v.x,v.z):1;tokens[i].localPosition=new Vector3(v.x,y,v.z);tokens[i].localRotation=Quaternion.Euler(0,v.heading,0);tokens[i].localScale=Vector3.one*MarkerScale;}foreach(var station in stationMarkers)station.localScale=new Vector3(25,4,25)*Mathf.Max(1,MarkerScale);}
        public void Overview(){target=new Vector3(-10000,0,0);distance=235000;pitch=70;yaw=0;}
        public void Focus(){var v=desk.state.vessels[desk.selected];target=new Vector3(v.x,12,v.z);distance=300;pitch=32;overhead=false;}
        public void FocusStation(int index){target=Stations[index];distance=index<4?8000:22000;pitch=65;yaw=0;}
        public void NorthUp(){yaw=0;overhead=true;}
        public void ToggleOverhead(){overhead=!overhead;}
        public Vector3 CameraTarget=>target;
        void ClampTarget(){target.x=Mathf.Clamp(target.x,-115000,85000);target.z=Mathf.Clamp(target.z,-55000,55000);}
        public void PanKeyboard(Vector2 axes,float seconds,bool fast,bool textInputFocused)
        {
            if(textInputFocused||axes.sqrMagnitude==0)return;
            var rotation=Quaternion.Euler(0,overhead?0:yaw,0);
            var direction=rotation*new Vector3(axes.x,0,axes.y).normalized;
            target+=direction*Mathf.Clamp(seconds,0,.1f)*distance*.35f*(fast?3:1);ClampTarget();
        }
        void Update()
        {
            if(desk==null||!desk.open||view==null)return;
            Sync();float dock=desk.DockWidth;view.rect=new Rect(dock/Screen.width,0,1-dock/Screen.width,1);
            bool outside=Input.mousePosition.x>dock&&(!desk.UIVisible||(Input.mousePosition.y<Screen.height-115&&Input.mousePosition.y>54));
            if(outside&&Input.GetMouseButtonDown(0))desk.FocusMap();
            if(Application.isFocused){var axes=new Vector2((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));PanKeyboard(axes,Time.unscaledDeltaTime,Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),desk.TextInputFocused);}
            if(outside) {
                if(Input.GetMouseButton(1)){yaw+=Input.GetAxisRaw("Mouse X")*3;pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*3,12,85);}
                distance=Mathf.Clamp(distance*Mathf.Exp(-Input.mouseScrollDelta.y*.12f),130,320000);
                if(Input.GetMouseButton(2)){var rotation=Quaternion.Euler(0,overhead?0:yaw,0);target-=rotation*Vector3.right*Input.GetAxisRaw("Mouse X")*distance*.018f+rotation*Vector3.forward*Input.GetAxisRaw("Mouse Y")*distance*.018f;ClampTarget();}
                if(desk.UIVisible&&Input.GetMouseButtonDown(0)) {
                    var ray=view.ScreenPointToRay(Input.mousePosition);
                    if(desk.placing||Input.GetKey(KeyCode.LeftAlt)||Input.GetKey(KeyCode.RightAlt)){var plane=new Plane(Vector3.up,Vector3.zero);if(plane.Raycast(ray,out float d)){var mapPoint=ray.GetPoint(d);if(desk.placing)desk.PlaceOnBoard(mapPoint.x,mapPoint.z);else rulerEnd=mapPoint;}}
                    else if(Physics.Raycast(ray,out RaycastHit hit,600000,1<<Layer))for(int i=0;i<tokens.Count;i++)if(hit.transform==tokens[i]||hit.transform.IsChildOf(tokens[i])){desk.SelectVessel(i);break;}
                }
            }
            view.nearClipPlane=Mathf.Max(.5f,distance*.00003f);view.orthographic=overhead;if(overhead){view.orthographicSize=distance*.45f;view.transform.position=target+Vector3.up*distance;view.transform.rotation=Quaternion.Euler(90,0,0);}else {view.transform.position=target+Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);view.transform.LookAt(target);}
            selection.enabled=desk.UIVisible;
            var p=tokens[desk.selected].position;var ring=new Vector3[49];for(int i=0;i<ring.Length;i++)ring[i]=p+new Vector3(Mathf.Sin(i*Mathf.PI/24)*49*MarkerScale,3,Mathf.Cos(i*Mathf.PI/24)*49*MarkerScale);selection.SetPositions(ring);selection.startWidth=selection.endWidth=Mathf.Max(2,distance*.0008f);
            ruler.gameObject.SetActive(desk.UIVisible&&rulerEnd.HasValue&&desk.state.vessels[desk.selected].station>=0);if(ruler.gameObject.activeSelf){ruler.SetPositions(new[]{p+Vector3.up*100,rulerEnd.Value+Vector3.up*100});ruler.startWidth=ruler.endWidth=Mathf.Max(2,distance*.0005f);}
        }
        void OnGUI()
        {
            if(desk==null||!desk.open||view==null||!desk.UIVisible)return;
            if(label==null){label=new GUIStyle(GUI.skin.label){fontSize=12,fontStyle=FontStyle.Bold,wordWrap=true,normal={textColor=new Color(1,.88f,.59f)}};small=new GUIStyle(label){fontSize=11,fontStyle=FontStyle.Normal,normal={textColor=Color.white}};}
            labelRects.Clear();
            for(int i=0;i<tokens.Count;i++)if(distance<12000||(i==desk.selected&&distance<50000))LabelAt(tokens[i].position+Vector3.up*66*MarkerScale,desk.state.vessels[i].name,small);
            var stations=Stations;for(int i=0;i<stations.Length;i++)if(distance<50000||i==0||i>=3)LabelAt(stations[i]+new Vector3(20*MarkerScale,12,0),StationNames[i],label);
            LabelAt(new Vector3(-107000,10,33000),"CONFEDERATE\nUnassigned / off map",label);LabelAt(new Vector3(-107000,10,-10000),"UNION\nUnassigned / off map",label);
            string extra="";if(rulerEnd.HasValue&&desk.state.vessels[desk.selected].station>=0){float meters=Vector3.Distance(new Vector3(tokens[desk.selected].position.x,0,tokens[desk.selected].position.z),rulerEnd.Value);extra=$" | Range {meters/1852:0.00} nm / {meters/.9144:0} yd (straight, projected)";}
            GUI.Label(new Rect(desk.DockWidth+20,Screen.height-80,Screen.width-desk.DockWidth-30,25),"Modern land cover (2021) | North-up available | 1 map unit = 1 meter | Marker size enlarged"+extra,small);
            // Dynamic scale uses the current camera projection, so zoom and perspective remain measurable.
            var a=view.WorldToScreenPoint(target);float[] steps={100,250,500,1000,2500,5000,10000,25000,50000};float chosen=100,pixels=0;
            foreach(float step in steps){float width=Vector2.Distance(a,view.WorldToScreenPoint(target+view.transform.right*step));if(width<180){chosen=step;pixels=width;}else break;}
            GUI.color=Color.white;GUI.DrawTexture(new Rect(desk.DockWidth+20,Screen.height-113,pixels,3),Texture2D.whiteTexture);GUI.Label(new Rect(desk.DockWidth+20,Screen.height-135,300,22),(chosen>=1000?(chosen/1000).ToString("0.##")+" km":chosen.ToString("0")+" m")+" at view center",small);
        }
        void LabelAt(Vector3 worldPoint,string text,GUIStyle style)
        {
            var p=view.WorldToScreenPoint(worldPoint);float y=Screen.height-p.y;if(p.z<=0||p.x<=desk.DockWidth+8||p.x>=Screen.width-10||y<=115||y>=Screen.height-140)return;
            var rect=new Rect(Mathf.Min(p.x,Screen.width-165),y,155,text.Contains("\n")?36:19);
            for(int attempt=0;attempt<8;attempt++){bool overlaps=false;foreach(var used in labelRects)if(used.Overlaps(rect)){overlaps=true;break;}if(!overlaps)break;rect.y+=20;}
            if(rect.yMax<Screen.height-140){labelRects.Add(rect);GUI.Label(rect,text,style);}
        }
        public void SetVisible(bool visible){if(world!=null)world.gameObject.SetActive(visible);if(view!=null)view.gameObject.SetActive(visible);}
        void OnDestroy(){foreach(var item in owned)if(item!=null)Destroy(item);foreach(var item in markerOwned)if(item!=null)Destroy(item);}
    }
}
