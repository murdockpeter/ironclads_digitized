using UnityEngine;

namespace Ironclads
{
    public class EngineExhaust : MonoBehaviour
    {
        public MaritimeScene scene;
        public readonly ParticleSystem[] funnels=new ParticleSystem[4];
        public readonly int[] emitted=new int[4];
        readonly float[] accumulation=new float[4];
        readonly ParticleSystem.Particle[] particles=new ParticleSystem.Particle[2600];
        static readonly int[] Ships={0,0,1,2};
        bool globeCleared;
        public static float EmissionRate(float load,bool lit)=>lit?3+37*Mathf.Clamp01(load)*Mathf.Clamp01(load):0;
        void Start()=>Build();
        public void ResetFunnels()
        {
            Clear();foreach(var ps in funnels)if(ps!=null){ps.gameObject.SetActive(false);if(Application.isPlaying)Destroy(ps.gameObject);else DestroyImmediate(ps.gameObject);}
            for(int i=0;i<4;i++)funnels[i]=null;
        }
        public void Build()
        {
            if(funnels[0]!=null)return;
            var material=new Material(Shader.Find("Ironclads/Powder Smoke"));material.mainTexture=BatteryEffects.SmokeTexture(true);
            Vector3[] outlets={new Vector3(0,14.5f,7),new Vector3(0,14.5f,-9),new Vector3(0,12.05f,0)};
            if(scene.scenario.Hampton)outlets=new[]{new Vector3(-.95f,.86f,-13),new Vector3(.95f,.86f,-13),new Vector3(0,11.9f,-1),new Vector3(0,14,12)};
            for(int i=0;i<4;i++) {
                if(i==3&&!scene.scenario.Hampton)continue;
                var go=new GameObject((i==3?"Minnesota":scene.scenario.ShortName(Ships[i]))+" coal exhaust "+i);go.transform.SetParent(scene.Vessel(Ships[i]),false);go.transform.localPosition=outlets[i];
                var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);funnels[i]=ps;
                var main=ps.main;main.loop=true;main.playOnAwake=false;main.maxParticles=2600;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startSpeed=0;main.startSize=1;main.startLifetime=45;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
                var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
                var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(.14f,2.6f),new Keyframe(.5f,5.5f),new Keyframe(1,7.5f)));
                var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.45f,0),new GradientAlphaKey(.78f,.04f),new GradientAlphaKey(.5f,.4f),new GradientAlphaKey(0,1)});color.color=gradient;
                var atlas=ps.textureSheetAnimation;atlas.enabled=true;atlas.numTilesX=2;atlas.numTilesY=2;atlas.animation=ParticleSystemAnimationType.WholeSheet;atlas.frameOverTime=new ParticleSystem.MinMaxCurve(0);atlas.startFrame=new ParticleSystem.MinMaxCurve(0,3.999f);
                var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.sortMode=ParticleSystemSortMode.Distance;renderer.maxParticleSize=.85f;ps.Play();
            }
        }
        public void Clear()
        {
            foreach(var ps in funnels)if(ps!=null)ps.Clear();for(int i=0;i<4;i++)accumulation[i]=0;
        }
        public void SetOrder(int ship,float load,bool lit=true)
        {
            if(scene.network!=null&&!scene.network.CanControl(ship)){scene.status="You can change engine orders only for your assigned ship.";return;}
            GetComponent<RuleChart>().Remember();var state=scene.scenario.State(ship);state.engineLoad=Mathf.Clamp01(load);state.boilersLit=lit;
            scene.status=state.name+": "+(!lit?"boilers cold":load<=0?"screws stopped / boilers banked":load<.35f?"slow engine effort":load<.8f?"half engine effort":"full engine effort")+" (visual only).";
            scene.network?.SubmitShip(ship);
        }
        void LateUpdate()
        {
            if(scene==null||!Application.isPlaying)return;Build();
            if(scene.globeMode){if(!globeCleared)Clear();globeCleared=true;return;}globeCleared=false;
            float dt=Mathf.Min(Time.deltaTime,.1f);Vector3 wind=BatteryEffects.Wind(scene.scenario.windFrom,scene.scenario.windKnots);
            for(int funnel=0;funnel<4;funnel++) {
                var ps=funnels[funnel];if(ps==null)continue;var state=scene.scenario.State(Ships[funnel]);float load=state.engineLoad;
                if(!ps.isPlaying)ps.Play();
                float stoke=1+.22f*Mathf.Sin(Time.time*.83f+funnel*2.3f)+.1f*Mathf.Sin(Time.time*2.1f+funnel);
                accumulation[funnel]+=EmissionRate(load,state.boilersLit)*stoke*dt;
                int count=Mathf.FloorToInt(accumulation[funnel]);accumulation[funnel]-=count;
                for(int i=0;i<count;i++) {
                    float shade=Mathf.Lerp(.42f,.13f,load)+Random.Range(-.045f,.11f),opacity=Mathf.Lerp(.35f,.72f,load)*Random.Range(.8f,1.15f);
                    uint seed=((uint)Random.Range(1,8388607)<<8)|(uint)Mathf.RoundToInt(Mathf.Clamp01(shade)*255);
                    Vector3 momentum=state.speed*.514444f*scene.Vessel(Ships[funnel]).forward;
                    ps.Emit(new ParticleSystem.EmitParams{position=ps.transform.position+Random.insideUnitSphere*.3f,velocity=Vector3.up*Random.Range(2.2f,4.8f)+momentum+Random.insideUnitSphere*.7f,startSize=Random.Range(1.1f,2.1f)*Mathf.Lerp(.8f,1.25f,load),startLifetime=Random.Range(36f,57f),startColor=new Color(shade,shade*.99f,shade*.96f,opacity),rotation=Random.Range(0,360),angularVelocity=Random.Range(-9f,9f),randomSeed=seed},1);emitted[funnel]++;
                }
                int live=ps.GetParticles(particles);
                for(int i=0;i<live;i++) {
                    float age=particles[i].startLifetime-particles[i].remainingLifetime;
                    particles[i].velocity=BatteryEffects.TransportVelocity(particles[i].velocity,wind,age,dt,(particles[i].randomSeed%1024)*.17f,Time.time,particles[i].position);
                    float original=(particles[i].randomSeed&255)/255f,alpha=particles[i].startColor.a/255f;
                    particles[i].startColor=Color.Lerp(new Color(original,original*.99f,original*.96f,alpha),new Color(.56f,.565f,.57f,alpha),1-Mathf.Exp(-age*.045f));
                }
                ps.SetParticles(particles,live);
            }
        }
    }
}
