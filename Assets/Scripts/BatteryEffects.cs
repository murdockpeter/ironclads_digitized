using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public class BatteryEffects : MonoBehaviour
    {
        public MaritimeScene scene;
        public bool cannonSound=true;
        public float volume=.95f,smokeAmount=1,ripple=.32f;
        public int ShotsFired {get;private set;}
        public readonly bool[] firing=new bool[3];
        public ParticleSystem powder,flash;
        public bool silentVerification;
        AudioClip[] reports;
        readonly List<AudioSource> voices=new List<AudioSource>();
        readonly List<Light> flashes=new List<Light>();
        readonly ParticleSystem.Particle[] particles=new ParticleSystem.Particle[8000];
        int voiceIndex;
        public static Vector3 Wind(float from,float knots)=>Quaternion.Euler(0,from+180,0)*Vector3.forward*(knots*.514444f);
        public static Vector3 TransportVelocity(Vector3 velocity,Vector3 wind,float age,float dt,float seed,float time,Vector3 position=default)
        {
            // Spatially coherent eddies move nearby puffs together; smaller eddies tear
            // them apart. Individual drag and buoyancy prevent a uniform smoke ribbon.
            float identity=Mathf.Repeat(seed*.618034f,1),eddyX=position.x*.028f+time*.13f,eddyZ=position.z*.028f-time*.09f;
            float gust=1+.18f*Mathf.Sin(time*.43f+position.x*.012f+position.z*.009f);
            float shear=Mathf.Clamp(.78f+position.y*.016f,.78f,1.3f);
            float strength=.45f+wind.magnitude*.14f;
            Vector3 largeEddy=new Vector3(Mathf.PerlinNoise(eddyX,eddyZ)*2-1,(Mathf.PerlinNoise(eddyZ+43,eddyX)*2-1)*.5f,Mathf.PerlinNoise(eddyX+19,eddyZ+7)*2-1)*strength;
            Vector3 smallEddy=new Vector3(Mathf.Sin(time*1.13f+seed),.35f*Mathf.Sin(time*.91f+seed*2),Mathf.Cos(time*.83f+seed))*Mathf.Min(.75f,age*.20f);
            float lift=Mathf.Lerp(1.65f+identity*1.7f,.18f+identity*.45f,Mathf.Clamp01(age/28));
            Vector3 flow=wind*gust*shear+largeEddy+smallEddy+Vector3.up*lift;
            return Vector3.Lerp(velocity,flow,1-Mathf.Exp(-(1+identity*.8f)*dt));
        }
        public static AnimationCurve ReportRolloff()=>new AnimationCurve(new Keyframe(0,1),new Keyframe(.015f,.98f),new Keyframe(.05f,.85f),new Keyframe(.16f,.48f),new Keyframe(.5f,.12f),new Keyframe(1,0));
        void Start(){if(Application.isPlaying)EnsureEffects();}
        public GunMount[] Battery(int ship,int side)
        {
            var result=new List<GunMount>();
            if(ship<0||ship>2||(ship==2&&!scene.scenario.Hampton)||scene.Vessel(ship)==null)return result.ToArray();
            int port=0,starboard=0;
            var mounted=scene.Vessel(ship).GetComponentsInChildren<GunMount>();System.Array.Sort(mounted,(a,b)=>b.transform.localPosition.z.CompareTo(a.transform.localPosition.z));
            foreach(var mount in mounted){
                if(ship==2){if(mount.Side==-1&&port++>=scene.scenario.minnesotaPortGuns)continue;if(mount.Side==1&&starboard++>=scene.scenario.minnesotaStarboardGuns)continue;if(mount.Side==0&&scene.scenario.minnesotaBowGuns==0)continue;}
                if(side==2||mount.Side==side)result.Add(mount);
            }
            result.Sort((a,b)=>b.transform.localPosition.z.CompareTo(a.transform.localPosition.z));return result.ToArray();
        }
        public bool Fire(int ship,int side)
        {
            if(scene.network!=null&&scene.network.Active)return scene.network.RequestFire(ship,side);
            return FireLocal(ship,side);
        }
        internal bool FireLocal(int ship,int side)
        {
            if(ship<0||ship>2||scene.globeMode||firing[ship])return false;
            var mounts=Battery(ship,side);if(mounts.Length==0)return false;
            EnsureEffects();firing[ship]=true;StartCoroutine(Salvo(ship,mounts));return true;
        }
        IEnumerator Salvo(int ship,GunMount[] mounts)
        {
            scene.status=$"{scene.scenario.State(ship).name}: firing {mounts.Length} gun{(mounts.Length==1?"":"s")} — visual salvo only.";
            foreach(var mount in mounts) {
                if(scene.globeMode)break;
                Discharge(mount);yield return new WaitForSeconds(ripple*Random.Range(.88f,1.12f));
            }
            firing[ship]=false;
        }
        void EnsureEffects()
        {
            if(powder!=null)return;
            powder=CreateParticles("Black powder gun smoke",false);flash=CreateParticles("Cannon muzzle flame",true);
            reports=new AudioClip[9];for(int kind=0;kind<3;kind++)for(int variation=0;variation<3;variation++)reports[kind*3+variation]=MakeReport(kind,variation);
            for(int i=0;i<32;i++) {
                var go=new GameObject("Cannon report voice "+i);go.transform.SetParent(scene.seaRoot,false);var source=go.AddComponent<AudioSource>();
                source.playOnAwake=false;source.spatialBlend=1;source.minDistance=90;source.maxDistance=6000;source.rolloffMode=AudioRolloffMode.Custom;source.SetCustomCurve(AudioSourceCurveType.CustomRolloff,ReportRolloff());source.dopplerLevel=0;source.priority=30;
                go.AddComponent<AudioLowPassFilter>().cutoffFrequency=18000;voices.Add(source);
            }
        }
        public static AudioClip MakeReport(int kind,int variation=0)
        {
            const int rate=44100;const float duration=6.5f;var samples=new float[Mathf.RoundToInt(rate*duration)];var rng=new System.Random(8317+kind*729+variation*1337);float low=0,mid=0,sub=0,phase=0,upperPhase=0,peak=0;
            float weight=(kind==1?1.24f:kind==2?.79f:1)*(1+variation*.018f);
            for(int i=0;i<samples.Length;i++) {
                float t=i/(float)rate,n=(float)rng.NextDouble()*2-1;
                low+=.018f*(n-low);mid+=.28f*(n-mid);sub+=.003f*(n-sub);
                phase+=2*Mathf.PI*(34+63*Mathf.Exp(-t*10))/rate/weight;upperPhase+=2*Mathf.PI*(96+110*Mathf.Exp(-t*13))/rate/weight;
                float shock=(1-t/.012f)*Mathf.Exp(-t/.012f)*.9f;
                float crack=(n-mid)*Mathf.Exp(-t*55)*1.05f+shock;
                float body=(Mathf.Sin(phase)*.55f+Mathf.Sin(upperPhase)*.17f+low*4.6f+mid*.62f)*Mathf.Exp(-t*(kind==1?1.6f:2));
                float rolling=(low*3.5f+sub*3.2f+mid*.7f)*Mathf.Exp(-t*.66f)*(1-Mathf.Exp(-t*25));
                float slap=0;for(int echo=0;echo<3;echo++){float delay=(echo==0?.085f:echo==1?.19f:.36f)+variation*.009f;if(t>delay)slap+=(low*1.2f+mid*.38f)*Mathf.Exp(-(t-delay)*5);}
                float value=(float)System.Math.Tanh((crack+body+rolling+slap)*1.25f)*Mathf.Min(1,t*3000)*Mathf.Clamp01((duration-t)*2);
                samples[i]=value;peak=Mathf.Max(peak,Mathf.Abs(value));
            }
            for(int i=0;i<samples.Length;i++)samples[i]*=.94f/Mathf.Max(.001f,peak);
            var clip=AudioClip.Create(kind==1?"Armstrong 300 deep report":kind==2?"Armstrong 70 sharp report":"Parrott 150 cannon report",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        ParticleSystem CreateParticles(string name,bool flame)
        {
            var go=new GameObject(name);go.transform.SetParent(scene.seaRoot,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=false;main.playOnAwake=false;main.maxParticles=flame?256:8000;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startSpeed=0;main.startLifetime=flame?.10f:35;main.startSize=1;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,flame?.5f:.6f),new Keyframe(.12f,flame?1.5f:2.8f),new Keyframe(1,flame?.1f:9)));
            var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();
            gradient.SetKeys(new[]{new GradientColorKey(flame?new Color(1,.85f,.32f):new Color(.85f,.82f,.74f),0),new GradientColorKey(flame?new Color(1,.25f,.02f):new Color(.93f,.94f,.92f),.3f),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(flame?1:.72f,.015f),new GradientAlphaKey(flame?.6f:.42f,.35f),new GradientAlphaKey(0,1)});color.color=gradient;
            if(!flame){var atlas=ps.textureSheetAnimation;atlas.enabled=true;atlas.numTilesX=2;atlas.numTilesY=2;atlas.animation=ParticleSystemAnimationType.WholeSheet;atlas.frameOverTime=new ParticleSystem.MinMaxCurve(0);atlas.startFrame=new ParticleSystem.MinMaxCurve(0,3.999f);}
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=new Material(Shader.Find(flame?"Ironclads/Weather Sprite":"Ironclads/Powder Smoke"));renderer.sharedMaterial.mainTexture=SmokeTexture(!flame);renderer.sortMode=ParticleSystemSortMode.Distance;renderer.maxParticleSize=.8f;
            ps.Play();return ps;
        }
        internal static Texture2D SmokeTexture(bool atlas)
        {
            int size=atlas?256:128;var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);tex.wrapMode=TextureWrapMode.Clamp;var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                int variant=atlas?x/128+(y/128)*2:0;float u=(x%128)/127f,v=(y%128)/127f;
                float coarse=Mathf.PerlinNoise(u*3.1f+variant*17+11,v*3.1f+variant*7+31);
                float r=new Vector2((u*2-1)*(variant==1?1.17f:1),(v*2-1)*(variant==2?1.15f:1)).magnitude;
                float noise=.65f*coarse+.25f*Mathf.PerlinNoise(u*8.3f+variant*23,v*8.3f)+.10f*Mathf.PerlinNoise(u*17+9,v*17+variant*11);
                float alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01((1-r+(coarse-.5f)*.28f)*4)) * Mathf.Clamp01((noise-.15f)*1.7f);
                colors[y*size+x]=new Color(.7f+noise*.3f,.7f+noise*.3f,.7f+noise*.3f,alpha);
            }
            tex.SetPixels(colors);tex.Apply();return tex;
        }
        void Discharge(GunMount mount)
        {
            if(!powder.isPlaying)powder.Play();if(!flash.isPlaying)flash.Play();
            ShotsFired++;Vector3 muzzle=mount.Muzzle,forward=mount.Direction;float weight=mount.type==Shipwright.GunType.Armstrong300?1.3f:mount.type==Shipwright.GunType.Armstrong70?.75f:1;
            StartCoroutine(PowderJet(muzzle,forward,weight*Random.Range(.86f,1.18f)));
            flash.Emit(new ParticleSystem.EmitParams {position=muzzle+forward*.75f,velocity=forward*10,startSize=weight*2.5f,startLifetime=.09f,startColor=new Color(1,.8f,.25f,1)},3);
            var lightGo=new GameObject("Brief cannon flash");lightGo.transform.SetParent(scene.seaRoot,false);lightGo.transform.position=muzzle;var light=lightGo.AddComponent<Light>();light.color=new Color(1,.56f,.18f);light.range=16*weight;light.intensity=5;flashes.Add(light);StartCoroutine(FlashOut(light));
            if(cannonSound&&!silentVerification) {
                var source=voices[voiceIndex++%voices.Count];source.transform.position=muzzle;source.clip=reports[Shipwright.SoundKind(mount.type)*3+Random.Range(0,3)];source.pitch=Random.Range(.96f,1.035f);source.volume=volume;
                float range=Vector3.Distance(scene.view.transform.position,muzzle);source.GetComponent<AudioLowPassFilter>().cutoffFrequency=Mathf.Lerp(18000,1500,Mathf.Clamp01(range/4000));source.PlayDelayed(range/343f);
            }
        }
        IEnumerator PowderJet(Vector3 muzzle,Vector3 forward,float weight)
        {
            Vector3 right=Vector3.Cross(Vector3.up,forward).normalized,up=Vector3.Cross(forward,right).normalized;
            for(int pulse=0;pulse<3;pulse++) {
                int count=Mathf.RoundToInt((pulse==0?75:35)*smokeAmount*weight);
                for(int i=0;i<count;i++) {
                    float type=Random.value,angle=Random.value*Mathf.PI*2;Vector3 ring=(right*Mathf.Cos(angle)+up*Mathf.Sin(angle));
                    float size=type<.55f?Random.Range(.8f,1.5f):type<.87f?Random.Range(1.5f,2.3f):Random.Range(.35f,.75f);
                    float opacity=type<.55f?Random.Range(.65f,.92f):type<.87f?Random.Range(.38f,.65f):Random.Range(.18f,.38f);
                    bool denseDark=type<.55f&&Random.value<.45f;
                    float shade=denseDark?Random.Range(.28f,.52f):Random.Range(.76f,.98f),speed=Random.Range(10f,31f)*weight/(1+pulse*.25f);
                    uint seed=((uint)Random.Range(1,8388607)<<8)|(uint)Mathf.RoundToInt(shade*255);
                    var emit=new ParticleSystem.EmitParams {position=muzzle+forward*Random.Range(.05f,2f)+ring*Random.Range(.08f,.6f),velocity=forward*speed+ring*Random.Range(1.5f,6f)+Vector3.up*Random.Range(1f,3.5f),startSize=size*weight,startLifetime=type>.87f?Random.Range(20f,32f):Random.Range(29f,46f),startColor=new Color(shade,shade*.995f,shade*.965f,opacity),rotation=Random.Range(0,360),angularVelocity=Random.Range(-13f,13f),randomSeed=seed};
                    powder.Emit(emit,1);
                }
                if(pulse<2)yield return new WaitForSeconds(Random.Range(.045f,.085f));
            }
        }
        IEnumerator FlashOut(Light light){yield return new WaitForSeconds(.07f);flashes.Remove(light);if(light!=null)Destroy(light.gameObject);}
        public void Clear()
        {
            StopAllCoroutines();firing[0]=firing[1]=firing[2]=false;if(powder!=null){powder.Clear();flash.Clear();}
            foreach(var source in voices)source.Stop();foreach(var light in flashes)if(light!=null)Destroy(light.gameObject);flashes.Clear();
        }
        void LateUpdate()
        {
            if(powder==null)return;if(scene.globeMode){Clear();return;}
            foreach(var voice in voices){voice.mute=!cannonSound;voice.volume=volume;}
            int count=powder.GetParticles(particles);Vector3 wind=Wind(scene.scenario.windFrom,scene.scenario.windKnots);
            for(int i=0;i<count;i++){
                float age=particles[i].startLifetime-particles[i].remainingLifetime;
                particles[i].velocity=TransportVelocity(particles[i].velocity,wind,age,Time.deltaTime,(particles[i].randomSeed%1024)*.17f,Time.time,particles[i].position);
                float original=(particles[i].randomSeed&255)/255f;float alpha=particles[i].startColor.a/255f;
                Color shade=new Color(original,original*.995f,original*.965f,alpha),pale=new Color(.94f,.94f,.91f,alpha);
                particles[i].startColor=Color.Lerp(shade,pale,1-Mathf.Exp(-age*.13f));
            }
            powder.SetParticles(particles,count);
        }
        void OnDestroy(){foreach(var clip in reports??new AudioClip[0])if(clip!=null)Destroy(clip);}
    }
}
