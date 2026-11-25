using System;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public sealed class CanopusHullData
    {
        public string name,coverage;public float loa,lbp,waterlineAft,waterlineFore,beam,draft,foreMastZ,aftMastZ,mastTop;
        public float[] fractions,waterlineWidths,deckWidths,keelY,deckY;public string[] sources;
        public float At(float[] values,float f){float t=Mathf.Clamp01(f)*20;int i=Mathf.Min(19,Mathf.FloorToInt(t));return Mathf.Lerp(values[i],values[i+1],t-i);}
        public float WaterlineWidth(float z)=>z<waterlineAft||z>waterlineFore?0:At(waterlineWidths,Mathf.InverseLerp(waterlineAft,waterlineFore,z))*beam*.5f;
        public float DeckHeight(float z)=>At(deckY,Mathf.InverseLerp(-loa*.5f,loa*.5f-4.5f,z));
    }
    public sealed class CanopusReplacement : MonoBehaviour
    {
        public string modelId="canopus-1901-prototype",evidence="silhouette researched; underwater inferred";
        public Transform[] turretRoots,exhaustOutlets;public int[] turretGroups;
        public void BindRuntime()
        {
            for(int i=0;i<turretRoots.Length;i++){var root=turretRoots[i];if(root.GetComponent<TurretAssembly>()==null)root.gameObject.AddComponent<TurretAssembly>();var turret=root.GetComponent<CruiserTurret>();if(turret==null)turret=root.gameObject.AddComponent<CruiserTurret>();turret.group=turretGroups[i];}
            foreach(var outlet in exhaustOutlets)if(outlet.GetComponent<CruiserFunnel>()==null)outlet.gameObject.AddComponent<CruiserFunnel>();
        }
        static CanopusHullData data;
        static BattleshipClass motion;
        public static CanopusHullData Definition=>data??(data=JsonUtility.FromJson<CanopusHullData>(Resources.Load<TextAsset>("PantelleriaReplacements/CanopusDefinition").text));
        public BattleshipClass MotionModel=>motion??(motion=new BattleshipClass("Canopus visual waterline",Definition.waterlineFore-Definition.waterlineAft,Definition.beam,18,.305f,2,2,12));
        public float HalfWidth(float z)=>Definition.WaterlineWidth(z);
        public void PushWaterline()
        {
            var d=Definition;Shader.SetGlobalFloatArray("_CanopusWaterlineWidths",d.waterlineWidths);
            Shader.SetGlobalVector("_CanopusWaterlineBounds",new Vector4(d.waterlineAft,d.waterlineFore,0,0));
        }
    }
    public static class PantelleriaModels
    {
        const string PrefabPath="PantelleriaReplacements/Canopus";
        static bool? enabled;static GameObject prefab;
        public static bool CanopusEnabled=>enabled??(enabled=PlayerPrefs.GetInt("Pantelleria.CanopusReplacement",1)!=0).Value;
        public static bool CanopusAvailable=>Prefab!=null&&Resources.Load<TextAsset>("PantelleriaReplacements/CanopusDefinition")!=null;
        static GameObject Prefab=>prefab!=null?prefab:(prefab=Resources.Load<GameObject>(PrefabPath));
        public static void SelectCanopus(bool value,bool persist=true){enabled=value;if(persist){PlayerPrefs.SetInt("Pantelleria.CanopusReplacement",value?1:0);PlayerPrefs.Save();}}
        public static GameObject TryCreate(int index)
        {
            if(index!=0||!CanopusEnabled||!CanopusAvailable)return null;
            var go=UnityEngine.Object.Instantiate(Prefab);go.name=PantelleriaScenario.Ships[index].name;
            go.GetComponent<CanopusReplacement>().BindRuntime();
            PantelleriaAppearance.Apply(go.transform,index);var d=CanopusReplacement.Definition;
            go.AddComponent<NavalColours>().Build(0,d.loa,6.2f,index,new Vector3(0,d.mastTop,d.foreMastZ),new Vector3(0,d.mastTop-3,d.aftMastZ),new Vector2(d.deckY[20],d.deckY[0]));
            return go;
        }
    }
}
