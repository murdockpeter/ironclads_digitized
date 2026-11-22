using System.Collections.Generic;
using UnityEngine;
namespace Ironclads
{
    // Normalized visual station fits to the RMG body / half-breadth plans cited in
    // Documentation/Close-Action-Hull-Contours.md; not surveyed construction offsets.
    public sealed class CloseActionHullLines
    {
        static readonly float[] Stations={0,.025f,.05f,.10f,.175f,.25f,.35f,.45f,.55f,.65f,.75f,.85f,.925f,.975f,.9875f,.9975f,1};
        static readonly float[] BritishWater={.045f,.15f,.29f,.51f,.75f,.90f,.98f,1,.995f,.945f,.825f,.61f,.40f,.19f,.10f,.025f,0};
        static readonly float[] BritishUpper={.70f,.74f,.80f,.88f,.945f,.985f,1,1,.995f,.96f,.88f,.72f,.53f,.30f,.205f,.085f,0};
        static readonly float[] FrenchWater={.035f,.12f,.255f,.475f,.72f,.88f,.975f,1,.995f,.955f,.84f,.635f,.415f,.20f,.105f,.026f,0};
        static readonly float[] FrenchUpper={.68f,.73f,.79f,.88f,.955f,.99f,1,1,.995f,.965f,.89f,.735f,.55f,.315f,.215f,.09f,0};
        static readonly float[] SectionHeights={0,.10f,.22f,.40f,.60f,.80f,1};
        static readonly float[] SectionBreadths={0,.37f,.63f,.84f,.965f,1.01f,1};
        static readonly float[] RunDepth={.55f,.61f,.70f,.84f,.95f,.99f,1,1,1,1,.97f,.88f,.72f,.57f,.52f,.47f,.45f};
        static readonly Dictionary<Vector2,CloseActionHullLines> Cache=new Dictionary<Vector2,CloseActionHullLines>();
        readonly float[] water,upper;
        public readonly CloseActionPrototype prototype;
        public readonly float draft,tumblehome,foreSheer,aftSheer;
        CloseActionHullLines(CloseActionPrototype p)
        {
            prototype=p;water=(float[])(p.side==0?BritishWater:FrenchWater).Clone();upper=(float[])(p.side==0?BritishUpper:FrenchUpper).Clone();
            draft=(p.rate==120?29:p.rate==80?24:p.rate==50?20:22)*.3048f;
            tumblehome=p.rate==120?.79f:p.rate==50?.86f:p.side==0?.82f:.845f;
            foreSheer=p.rate==120?1.4f:p.rate==80?1.25f:p.rate==50?1.0f:1.1f;
            aftSheer=p.rate==120?1.75f:p.rate==80?1.55f:p.rate==50?1.25f:1.4f;
            if(p.key.Contains("Alfred")){tumblehome=.80f;upper[1]=.76f;upper[2]=.82f;}
            if(p.key.Contains("Culloden")){water[4]=.77f;water[5]=.915f;tumblehome=.83f;}
            if(p.key.Contains("Centaure")){water[3]=.50f;water[4]=.75f;foreSheer=1.2f;}
            if(p.key.Contains("Citoyen")){water[3]=.52f;water[4]=.77f;tumblehome=.82f;}
            if(p.key.Contains("Souverain")){water[3]=.54f;water[4]=.785f;tumblehome=.815f;aftSheer=1.5f;}
            if(p.rate==120){water[4]=.79f;water[5]=.93f;water[10]=.86f;}
        }
        public static CloseActionHullLines Get(float length,float beam)
        {
            var key=new Vector2(length,beam);if(Cache.TryGetValue(key,out var lines))return lines;
            foreach(var p in CloseActionPrototypes.All)if(Mathf.Abs(p.length-length)<.001f&&Mathf.Abs(p.beam-beam)<.001f){lines=new CloseActionHullLines(p);Cache[key]=lines;return lines;}
            throw new System.ArgumentException("Unknown sailing hull dimensions.");
        }
        public float Station(float z)=>Mathf.Clamp01(z/prototype.length+.5f);
        public float Sheer(float z)
        {
            float f=Station(z);return f<.48f?aftSheer*Mathf.Pow((.48f-f)/.48f,2.6f):foreSheer*Mathf.Pow((f-.48f)/.52f,2.6f);
        }
        public float KeelDepth(float z)=>draft*Sample(Stations,RunDepth,Station(z));
        public float HalfBreadth(float z,float y)
        {
            float f=Station(z),wl=Sample(Stations,water,f),value;
            if(y<0)value=wl*Sample(SectionHeights,SectionBreadths,Mathf.Clamp01(1+y/KeelDepth(z)));
            else{
                float v=Mathf.Clamp01(y/(prototype.deck+Sheer(z)+.3f));
                float flare=Mathf.SmoothStep(0,1,Mathf.Clamp01(v*2.8f));
                value=Mathf.Lerp(wl,Sample(Stations,upper,f),flare)*(1-(1-tumblehome)*Mathf.Pow(v,1.15f));
            }
            return Mathf.Max(0,value)*prototype.beam*.5f;
        }
        public float DeckHeight(float z)=>prototype.deck+Sheer(z);
        public float PlankHeight(float z,float height)=>height*(1+Sheer(z)/prototype.deck);
        public float BowRake(float z,float y)
        {
            float fore=Mathf.SmoothStep(0,1,Mathf.Clamp01((Station(z)-.78f)/.22f)),deck=DeckHeight(z),depth=KeelDepth(z);
            float setback;
            if(y>=0)setback=.028f*Mathf.Pow(Mathf.Clamp01(1-y/deck),1.2f);
            else{
                float below=Mathf.Clamp01(-y/depth),slope=.0336f*depth/deck;
                // The forefoot rolls aft below the waterline, with a continuous tangent at the waterline.
                setback=.028f+slope*below+(.05f-slope)*below*below;
            }
            return prototype.length*fore*setback;
        }
        // Monotone cubic interpolation preserves the plan offsets without ripples.
        static float Sample(float[] x,float[] y,float position)
        {
            int i=0;while(i<x.Length-2&&position>x[i+1])i++;
            float h=x[i+1]-x[i],d=(y[i+1]-y[i])/h;
            float before=i>0?(y[i]-y[i-1])/(x[i]-x[i-1]):d,after=i+2<x.Length?(y[i+2]-y[i+1])/(x[i+2]-x[i+1]):d;
            float m0=before*d>0?2*before*d/(before+d):0,m1=d*after>0?2*d*after/(d+after):0,t=Mathf.Clamp01((position-x[i])/h);
            return (2*t*t*t-3*t*t+1)*y[i]+(t*t*t-2*t*t+t)*h*m0+(-2*t*t*t+3*t*t)*y[i+1]+(t*t*t-t*t)*h*m1;
        }
    }
}
