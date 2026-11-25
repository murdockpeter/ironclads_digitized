using UnityEngine;

namespace Ironclads
{
    public static class PantelleriaAppearance
    {
        public static Material Paint(string name,Color colour,float deck,bool funnel=false)
        {
            var shader=Shader.Find("Ironclads/Pantelleria Surface");if(shader==null)throw new System.InvalidOperationException("Pantelleria surface shader missing");
            var material=new Material(shader){name=name,color=colour};material.SetFloat("_Metallic",.12f);material.SetFloat("_Glossiness",funnel?.21f:.28f);material.SetFloat("_FinishKind",funnel?1:0);material.SetFloat("_DeckHeight",deck);return material;
        }
        public static void Apply(Transform root,int ship)
        {
            var properties=new MaterialPropertyBlock();
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()) {
                if(renderer.sharedMaterial.shader.name!="Ironclads/Pantelleria Surface")continue;
                properties.SetFloat("_HullPass",renderer.GetComponentInParent<TurretAssembly>()==null?1:0);
                properties.SetFloat("_FinishSeed",ship*1.731f);renderer.SetPropertyBlock(properties);
            }
        }
        public static float ExhaustPulse(int funnel,float time)=>.95f+.16f*Mathf.Sin(time*.67f+funnel*2.39996f)+.07f*Mathf.Sin(time*1.31f+funnel*.83f);
        public static float ExhaustRate(bool lit,float load,int funnel,float time)=>lit?(4+15*Mathf.Clamp01(load))*ExhaustPulse(funnel,time):0;
        public static float ExhaustShade(float load,float variation)=>Mathf.Clamp(Mathf.Lerp(.37f,.20f,Mathf.Clamp01(load))+variation*.045f,.13f,.43f);
    }
}
