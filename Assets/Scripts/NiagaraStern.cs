using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        // Qualitative round-stern reconstruction. The measured 1863 after-body drawing
        // has not been recovered; these offsets are deliberately not presented as a tracing.
        public static float NiagaraRakedZ(float station,float y)
        {
            float f=station/100.228f+.5f;
            float weight=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(f/.20f));
            float drop=Mathf.Max(0,DeckHeight(station,100.228f,4.3f)-y);
            float inset=4.2f*Mathf.SmoothStep(0,1,Mathf.Clamp01(drop/5.5f))+
                1.8f*Mathf.SmoothStep(0,1,Mathf.Clamp01((drop-5.5f)/6));
            return station+weight*inset;
        }

        static void NiagaraPaintBand(Transform root)
        {
            const float length=100.228f;
            var stations=new List<float>();
            for(int i=0;i<=384;i++)stations.Add(Mathf.Lerp(-length*.48f,length*.48f,i/384f));
            foreach(float z in NiagaraGunStations){stations.Add(z-.9f);stations.Add(z+.9f);}
            stations.Sort();
            for(int side=-1;side<=1;side+=2)for(int i=0;i<stations.Count-1;i++) {
                float z=stations[i],next=stations[i+1];
                if(InGunPort((z+next)*.5f))continue;
                Vector3 P(float s,float drop){float y=DeckHeight(s,length,4.3f)-drop;return new Vector3(side*(NiagaraSurfaceX(s,y)+.035f),y,s);}
                Quad(root,"Painted gun deck band",P(z,.68f),P(next,.68f),P(next,1.32f),P(z,1.32f),White);
            }
        }

        static Material niagaraHullPaint,niagaraStripePaint,niagaraIronPaint;
        static void ApplyNiagaraFinish(Transform root)
        {
            if(niagaraHullPaint==null) {
                niagaraHullPaint=MakeMaterial("Niagara black painted timber",new Color(.035f,.038f,.040f),0,.16f);
                niagaraStripePaint=MakeMaterial("Niagara off-white gun-deck paint",new Color(.84f,.84f,.81f),0,.13f);
                niagaraIronPaint=MakeMaterial("Niagara blackened iron",new Color(.065f,.070f,.073f),.35f,.20f);
            }
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()) {
                var material=renderer.sharedMaterial;
                bool sternTrim=renderer.name.Contains("stern window")||renderer.name.Contains("stern molding");
                if(material==Hull||sternTrim)renderer.sharedMaterial=niagaraHullPaint;
                else if(material==White)renderer.sharedMaterial=niagaraStripePaint;
                else if(material==Iron)renderer.sharedMaterial=niagaraIronPaint;
            }
        }
    }
}
