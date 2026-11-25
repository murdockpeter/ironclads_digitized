using UnityEngine;

namespace Ironclads
{
    public static class PantelleriaAppearance
    {
        static uint Identity(int ship)
        {
            unchecked {uint hash=2166136261;foreach(char c in PantelleriaScenario.Id+"/"+PantelleriaScenario.Ships[ship].name)hash=(hash^c)*16777619;return hash;}
        }
        static float Variation(uint hash,uint salt)
        {
            unchecked {hash^=salt*0x9e3779b9;hash^=hash>>16;hash*=0x85ebca6b;hash^=hash>>13;hash*=0xc2b2ae35;hash^=hash>>16;return (hash&0xffffff)/16777215f;}
        }
        public static float FinishSeed(int ship)=>(Identity(ship)&0xffff)*.03125f;
        public static Vector4 WeatherProfile(int ship)
        {
            uint hash=Identity(ship);return new Vector4(.25f+.50f*Variation(hash,1),.20f+.55f*Variation(hash,2),.20f+.55f*Variation(hash,3),.15f+.65f*Variation(hash,4));
        }
        public static float AgeDays(Scenario scenario)=>scenario.Pantelleria?Mathf.Clamp((float)(scenario.Date-new System.DateTime(1901,2,14,13,30,0,System.DateTimeKind.Utc)).TotalDays,0,90):0;
        public static void RefreshWeather(Scenario scenario)
        {
            Shader.SetGlobalFloat("_PantelleriaRain",scenario.Pantelleria?Mathf.Clamp01(scenario.rain):0);
            Shader.SetGlobalFloat("_PantelleriaAgeDays",AgeDays(scenario));
        }
        public static Material Paint(string name,Color colour,float deck,bool funnel=false)
        {
            var shader=Shader.Find("Ironclads/Pantelleria Surface");if(shader==null)throw new System.InvalidOperationException("Pantelleria surface shader missing");
            var material=new Material(shader){name=name,color=colour};material.SetFloat("_Metallic",.12f);material.SetFloat("_Glossiness",funnel?.21f:.28f);material.SetFloat("_FinishKind",funnel?1:0);material.SetFloat("_DeckHeight",deck);return material;
        }
        public static void Apply(Transform root,int ship)
        {
            var properties=new MaterialPropertyBlock();
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()) {
                string shader=renderer.sharedMaterial.shader.name;
                if(shader!="Ironclads/Pantelleria Surface"&&shader!="Ironclads/Pantelleria Teak")continue;
                if(shader=="Ironclads/Pantelleria Teak")renderer.GetComponent<MeshFilter>().sharedMesh.RecalculateTangents();
                properties.SetFloat("_HullPass",renderer.GetComponentInParent<TurretAssembly>()==null?1:0);
                properties.SetFloat("_FinishSeed",FinishSeed(ship));properties.SetVector("_WeatherProfile",WeatherProfile(ship));
                var model=PantelleriaScenario.Ships[ship].model;
                properties.SetFloat("_ShipLength",model.length);properties.SetFloat("_ShipBeam",model.beam);
                renderer.SetPropertyBlock(properties);
            }
        }
        public static float ExhaustPulse(int funnel,float time)=>.95f+.16f*Mathf.Sin(time*.67f+funnel*2.39996f)+.07f*Mathf.Sin(time*1.31f+funnel*.83f);
        public static float ExhaustRate(bool lit,float load,int funnel,float time)=>lit?(4+15*Mathf.Clamp01(load))*ExhaustPulse(funnel,time):0;
        public static float ExhaustShade(float load,float variation)=>Mathf.Clamp(Mathf.Lerp(.37f,.20f,Mathf.Clamp01(load))+variation*.045f,.13f,.43f);
    }
}
