using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ironclads
{
    // Shared procedural patterns: no downloaded flag artwork is bundled.
    public static class NavalFlagDesign
    {
        static readonly Texture2D[,] textures=new Texture2D[4,2];
        static readonly Color white=new Color(.94f,.93f,.87f),red=new Color(.72f,.025f,.045f),blue=new Color(.025f,.10f,.34f),green=new Color(.015f,.36f,.17f),gold=new Color(.85f,.61f,.15f);
        static Color Saltire(float u,float v)=>Mathf.Abs(v-u)<.085f||Mathf.Abs(v-(1-u))<.085f?blue:white;
        public static Color Sample(int nation,bool pennant,float u,float v)
        {
            if(pennant) {
                if(nation==0)return u<.13f&&(Mathf.Abs(u-.055f)<.013f||Mathf.Abs(v-.5f)<.10f)?red:white;
                if(nation==3)return u<.12f?Saltire(u/.12f,v):white;
                if(nation==2)return u<.2f?blue:u<.4f?white:red;
                return u<.2f?green:u<.4f?white:red;
            }
            if(nation==3)return Saltire(u,v);
            if(nation==2)return u<.30f?blue:u<.63f?white:red;
            if(nation==1) {
                Color c=u<1f/3?green:u<2f/3?white:red;
                float x=(u-.5f)/.09f,y=(v-.45f)/.22f;
                if(y>-.95f&&y<1&&Mathf.Abs(x)<Mathf.Sqrt(Mathf.Clamp01(y+1.1f))) {
                    c=blue;if(Mathf.Abs(x)<.87f&&y>-.80f&&y<.87f)c=red;
                    if(Mathf.Abs(x)<.16f||Mathf.Abs(y-.1f)<.18f)c=white;
                }
                if(v>.69f&&v<.76f&&Mathf.Abs(u-.5f)<.075f)c=gold;
                if(v>.76f&&v<.83f&&(Mathf.Abs(u-.5f)<.012f||Mathf.Abs(Mathf.Abs(u-.5f)-.055f)<.012f))c=gold;
                return c;
            }
            Color field=Mathf.Abs(u-.5f)<.025f||Mathf.Abs(v-.5f)<.05f?red:white;
            if(u>=.5f||v<=.5f)return field;
            float a=u*2,b=(v-.5f)*2;Color union=blue;
            float diagonal=Mathf.Min(Mathf.Abs(b-a),Mathf.Abs(b-(1-a)));
            if(diagonal<.15f)union=white;
            if(Mathf.Min(Mathf.Abs(b-a-.035f),Mathf.Abs(b-(1-a)+.035f))<.045f)union=red;
            if(Mathf.Abs(a-.5f)<.15f||Mathf.Abs(b-.5f)<.24f)union=white;
            if(Mathf.Abs(a-.5f)<.085f||Mathf.Abs(b-.5f)<.14f)union=red;
            return union;
        }
        public static Texture2D Texture(int nation,bool pennant)
        {
            int type=pennant?1:0;if(textures[nation,type]!=null)return textures[nation,type];
            int width=pennant?1024:512,height=pennant?64:256;
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,true){name=PantelleriaScenario.Nations[nation]+(pennant?" commissioning pennant":" naval ensign"),wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=4};
            var pixels=new Color[width*height];for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=Sample(nation,pennant,(x+.5f)/width,(y+.5f)/height);
            texture.SetPixels(pixels);texture.Apply(true,false);textures[nation,type]=texture;return texture;
        }
    }
    public sealed partial class NavalColours : MonoBehaviour
    {
        public int Nation {get;private set;}
        public Transform Ensign {get;private set;}
        public Transform Pennant {get;private set;}
        public Vector3 ApparentWind {get;private set;}
        readonly List<Renderer> cloth=new List<Renderer>();
        readonly List<Vector3> hoistAxes=new List<Vector3>();
        readonly List<float> clothLengths=new List<float>();
        readonly List<Mesh> ownedMeshes=new List<Mesh>();
        readonly List<Material> ownedMaterials=new List<Material>();
        MaterialPropertyBlock properties;
        float phase;
        public void Build(int nation,float length,float deck,int index)
        {
            Nation=nation;phase=index*2.399963f;
            Ensign=AddCloth("National naval ensign",new Vector3(0,deck+20.5f,-length*.24f),nation==0?4.6f:4.2f,nation==0?2.3f:2.8f,false);
            Pennant=AddCloth("Commissioning masthead pennant",new Vector3(0,deck+23,length*.24f),8,.45f,true);
            BuildRigging(length,deck,index);
        }
        Transform AddCloth(string name,Vector3 position,float length,float height,bool pennant,Texture2D pattern=null,int shape=-1,Vector3 hoistAxis=default,Transform parent=null)
        {
            var go=new GameObject(name);go.transform.SetParent(parent==null?transform:parent,false);go.transform.localPosition=position;
            int columns=pattern==null?64:20,rows=pattern==null?8:4;var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int x=0;x<=columns;x++)for(int y=0;y<=rows;y++) {
                float u=x/(float)columns,v=y/(float)rows,taper=shape>=0?(shape==1?Mathf.Lerp(1,.025f,u):shape==2?Mathf.Lerp(1,.65f,u):1):pennant?Mathf.Lerp(1,Nation==2?.015f:.2f,u):1;
                float notch=shape==2?.24f*Mathf.Pow(u,8)*(1-Mathf.Abs(2*v-1)):shape<0&&pennant&&Nation==3?.08f*Mathf.Pow(u,10)*(1-Mathf.Abs(2*v-1)):0;
                vertices.Add(new Vector3((u-notch)*length,(v-.5f)*height*taper,0));uv.Add(new Vector2(u,v));
                if(x<columns&&y<rows){int a=x*(rows+1)+y,b=a+rows+1;triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            }
            var mesh=Shipwright.Mesh(name,vertices,triangles,uv);
            mesh.bounds=new Bounds(new Vector3(length*.5f,-length*.22f,0),new Vector3(length+2,height+length+2,length+2));ownedMeshes.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var shader=Shader.Find("Ironclads/Naval Cloth");if(shader==null)throw new System.InvalidOperationException("Naval cloth shader missing");
            var texture=pattern==null?NavalFlagDesign.Texture(Nation,pennant):pattern;
            var material=new Material(shader){name=texture.name,mainTexture=texture};ownedMaterials.Add(material);
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;cloth.Add(renderer);hoistAxes.Add(hoistAxis);clothLengths.Add(length);return go.transform;
        }
        public void Refresh(float heading,float shipKnots,float windFrom,float windKnots,float time)
        {
            properties??=new MaterialPropertyBlock();
            ApparentWind=Quaternion.Euler(0,windFrom+180,0)*Vector3.forward*Mathf.Max(0,windKnots)-Quaternion.Euler(0,heading,0)*Vector3.forward*Mathf.Max(0,shipKnots);
            Vector3 direction=ApparentWind.sqrMagnitude>.0001f?ApparentWind.normalized:Quaternion.Euler(0,heading+180,0)*Vector3.forward;
            SetRiggingMode(RiggingMode);
            for(int index=0;index<cloth.Count;index++) {
                var renderer=cloth[index];if(!renderer.gameObject.activeInHierarchy)continue;
                Vector3 up=hoistAxes[index]==Vector3.zero?Vector3.up:transform.TransformDirection(hoistAxes[index]).normalized;
                Vector3 flow=Vector3.ProjectOnPlane(direction,up);if(flow.sqrMagnitude<.0001f)flow=Vector3.ProjectOnPlane(transform.right,up);if(flow.sqrMagnitude<.0001f)flow=Vector3.ProjectOnPlane(transform.forward,up);
                renderer.transform.rotation=Quaternion.LookRotation(Vector3.Cross(flow.normalized,up),up);
                properties.SetFloat("_ClothTime",time);properties.SetFloat("_ClothWind",Vector3.ProjectOnPlane(ApparentWind,up).magnitude);properties.SetFloat("_Phase",phase+(renderer.transform==Pennant?1.7f:index*.71f));
                properties.SetFloat("_ClothScale",Mathf.Clamp(clothLengths[index]/4.6f,.2f,1.5f));properties.SetVector("_ClothGravity",renderer.transform.InverseTransformDirection(Vector3.down));renderer.SetPropertyBlock(properties);
            }
        }
        void OnDestroy()
        {
            foreach(var mesh in ownedMeshes)if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);
            foreach(var material in ownedMaterials)if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);
        }
    }
}
