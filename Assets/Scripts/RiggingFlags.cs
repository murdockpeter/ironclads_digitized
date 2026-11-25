using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public sealed partial class NavalColours
    {
        public const int DressFlagCount=26,SignalFlagCount=12;
        public Transform DressingRoot {get;private set;}
        public Transform SignalRoot {get;private set;}
        static int? riggingMode;
        public static int RiggingMode {
            get {if(!riggingMode.HasValue)riggingMode=Mathf.Clamp(PlayerPrefs.GetInt("Pantelleria.RiggingFlags",2),0,2);return riggingMode.Value;}
            set {SelectRiggingMode(value);}
        }
        public static void SelectRiggingMode(int mode,bool persist=true){riggingMode=Mathf.Clamp(mode,0,2);if(persist){PlayerPrefs.SetInt("Pantelleria.RiggingFlags",riggingMode.Value);PlayerPrefs.Save();}}
        public bool IsRigging(Transform part)=>part.IsChildOf(DressingRoot)||part.IsChildOf(SignalRoot);
        public void SetRiggingMode(int mode)
        {
            if(DressingRoot!=null&&DressingRoot.gameObject.activeSelf!=(mode==2))DressingRoot.gameObject.SetActive(mode==2);
            if(SignalRoot!=null&&SignalRoot.gameObject.activeSelf!=(mode==1))SignalRoot.gameObject.SetActive(mode==1);
        }
        static Vector3 Catenary(Vector3 a,Vector3 b,float t,float sag)=>Vector3.Lerp(a,b,t)-Vector3.up*(Mathf.Sin(t*Mathf.PI)*sag);
        static Vector3 LineTangent(Vector3 a,Vector3 b,float t,float sag)=>(b-a-Vector3.up*(Mathf.Cos(t*Mathf.PI)*sag*Mathf.PI)).normalized;
        Transform Group(string name){var group=new GameObject(name).transform;group.SetParent(transform,false);return group;}
        void BuildRigging(float length,float deck,int ship)
        {
            DressingRoot=Group("Full dress decorative rigging");SignalRoot=Group("Decorative yardarm hoists");
            var anchors=new[]{new Vector3(0,deck+2.2f,length*.46f),new Vector3(0,deck+22,length*.24f),new Vector3(0,deck+22,-length*.24f),new Vector3(0,deck+2.2f,-length*.46f)};
            for(int end=0;end<2;end++) {
                var top=anchors[end==0?0:3];MakeRope(DressingRoot,"Dressing staff",new[]{new Vector3(top.x,deck+.02f,top.z),top},.055f,Shipwright.Iron);
            }
            int sequence=0;
            for(int span=0;span<3;span++) {
                int count=span==1?10:8;float sag=span==1?1.8f:.8f;var a=anchors[span];var b=anchors[span+1];var points=new Vector3[25];
                for(int i=0;i<points.Length;i++)points[i]=Catenary(a,b,i/(float)(points.Length-1),sag);
                MakeRope(DressingRoot,"Dressing line "+span,points,.022f,Shipwright.Rope);
                for(int i=0;i<count;i++) {
                    float t=(i+.65f)/(count+.3f);int pattern=(ship*5+sequence*7+Nation*3)%16,shape=(sequence+ship)%4==0?1:(sequence+ship)%7==0?2:0;
                    float fly=shape==1?2.35f:1.8f,height=shape==1?.95f:1.15f;
                    AddCloth("Dressing flag "+(++sequence),Catenary(a,b,t,sag),fly,height,false,SignalPattern(pattern),shape,LineTangent(a,b,t,sag),DressingRoot);
                }
            }
            sequence=0;
            foreach(int end in new[]{-1,1})foreach(int side in new[]{-1,1}) {
                var bottom=new Vector3(side*3.2f,deck+.025f,end*length*.24f);var top=new Vector3(side*3.5f,deck+20,end*length*.24f);
                MakeRope(SignalRoot,"Yardarm flag halyard",new[]{bottom,top},.018f,Shipwright.Rope);
                for(int i=0;i<3;i++) {
                    int pattern=(ship*3+sequence*5+Nation)%16;
                    AddCloth("Decorative hoist flag "+(++sequence),Vector3.Lerp(bottom,top,.66f+i*.13f),i==1?2.2f:1.65f,i==1?.75f:1.2f,false,SignalPattern(pattern),i==1?1:0,(top-bottom).normalized,SignalRoot);
                }
            }
            SetRiggingMode(2);
        }
        void MakeRope(Transform parent,string name,Vector3[] points,float radius,Material material)
        {
            const int sides=6;var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<points.Length;i++) {
                Vector3 axis=(points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(0,i-1)]).normalized;
                var u=Vector3.Cross(axis,Mathf.Abs(Vector3.Dot(axis,Vector3.up))>.95f?Vector3.forward:Vector3.up).normalized;var v=Vector3.Cross(axis,u);
                for(int j=0;j<sides;j++){float angle=j*Mathf.PI*2/sides;vertices.Add(points[i]+(Mathf.Cos(angle)*u+Mathf.Sin(angle)*v)*radius);}
                if(i==0)continue;
                for(int j=0;j<sides;j++){int a=(i-1)*sides+j,b=(i-1)*sides+(j+1)%sides;triangles.AddRange(new[]{a,b,a+sides,b,b+sides,a+sides});}
            }
            var mesh=Shipwright.Mesh(name,vertices,triangles);ownedMeshes.Add(mesh);var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        static readonly Texture2D[] signalPatterns=new Texture2D[16];
        static Texture2D SignalPattern(int pattern)
        {
            if(signalPatterns[pattern]!=null)return signalPatterns[pattern];
            const int size=128;var pixels=new Color[size*size];Color white=new Color(.96f,.94f,.87f),red=new Color(.78f,.035f,.045f),blue=new Color(.025f,.13f,.43f),yellow=new Color(.96f,.73f,.08f),black=new Color(.025f,.03f,.04f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float u=(x+.5f)/size,v=(y+.5f)/size;Color c;
                switch(pattern) {
                    case 0:c=u<.5f?white:blue;break;
                    case 1:c=red;break;
                    case 2:c=v<.33f||v>.67f?blue:white;break;
                    case 3:c=Mathf.Abs(u-.5f)<.13f||Mathf.Abs(v-.5f)<.13f?blue:white;break;
                    case 4:c=u<.5f?yellow:blue;break;
                    case 5:c=(x/32+y/32)%2==0?white:blue;break;
                    case 6:c=Mathf.Abs(u-v)<.12f||Mathf.Abs(u+v-1)<.12f?red:white;break;
                    case 7:c=yellow;break;
                    case 8:c=(x/26)%2==0?red:yellow;break;
                    case 9:c=Mathf.Abs(u-.5f)+Mathf.Abs(v-.5f)<.34f?red:white;break;
                    case 10:c=(new Vector2(u-.5f,v-.5f)).sqrMagnitude<.085f?blue:yellow;break;
                    case 11:c=(x/43+y/43)%2==0?red:white;break;
                    case 12:c=v<.5f?blue:red;break;
                    case 13:c=Mathf.Abs(u-.5f)<.11f||Mathf.Abs(v-.5f)<.11f?yellow:blue;break;
                    case 14:c=u+v<1?white:red;break;
                    default:c=(x/32)%2==0?black:yellow;break;
                }
                pixels[y*size+x]=c;
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,true){name="Decorative maritime pattern "+pattern,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=4};texture.SetPixels(pixels);texture.Apply();signalPatterns[pattern]=texture;return texture;
        }
    }
}
