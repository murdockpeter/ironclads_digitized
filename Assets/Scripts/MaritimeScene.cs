using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ironclads
{
    public class MaritimeScene : MonoBehaviour
    {
        public Scenario scenario = new Scenario();
        public Camera view;
        public Transform[] vessels = new Transform[2];
        public Transform seaRoot, globeRoot, globe, marker;
        public Material oceanMaterial, skyMaterial;
        public Light sun;
        public bool globeMode, animateTime, showOverlay = true, soundEnabled = true;
        public int selected;
        public Vector3 target = new Vector3(0,6,50);
        public float yaw = -40, pitch = 22, distance = 350;
        public float globeYaw = -8, globePitch = 43, globeDistance = 325;
        public string status = "Scenario ready. You have the conn.";
        public float SunAltitude {get; private set;}
        public ParticleSystem rain;
        AudioSource ambience;
        GameObject nearOcean;
        bool initialized;

        public void Initialize()
        {
            if(initialized)return;initialized=true;
            QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=1200;QualitySettings.shadows=ShadowQuality.All;
            QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.vSyncCount=1;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.fogMode=FogMode.ExponentialSquared;
            seaRoot=new GameObject("Local sea — meters east/north").transform;
            globeRoot=new GameObject("Global chart").transform;globeRoot.position=new Vector3(0,10000,0);
            oceanMaterial=new Material(Shader.Find("Ironclads/Ocean"));
            oceanMaterial.SetTexture("_NormalTex",WorldBuilder.WaterNormals());
            skyMaterial=new Material(Shader.Find("Ironclads/Sky"));RenderSettings.skybox=skyMaterial;
            nearOcean=WorldBuilder.Ocean(seaRoot,oceanMaterial,2400,256,0);
            WorldBuilder.Ocean(seaRoot,oceanMaterial,120000,128,-2.8f);
            for(int i=0;i<2;i++) {vessels[i]=Shipwright.Create(i==1).transform;vessels[i].SetParent(seaRoot,false);}
            rain=WorldBuilder.Particles(seaRoot,"Rain",true);
            globe=WorldBuilder.Globe(globeRoot).transform;
            marker=GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;marker.name="Scenario location";marker.SetParent(globeRoot,false);
            marker.localScale=Vector3.one*1.8f;DestroyImmediate(marker.GetComponent<Collider>());
            var markerMat=Shipwright.MakeMaterial("Position marker",new Color(.95f,.56f,.19f));
            markerMat.EnableKeyword("_EMISSION");markerMat.SetColor("_EmissionColor",new Color(1,.4f,.05f));marker.GetComponent<Renderer>().sharedMaterial=markerMat;
            sun=new GameObject("Geographic sunlight").AddComponent<Light>();sun.type=LightType.Directional;sun.shadows=LightShadows.Soft;sun.shadowBias=.035f;
            RenderSettings.sun=sun;
            view=new GameObject("Umpire camera").AddComponent<Camera>();view.tag="MainCamera";view.nearClipPlane=.3f;view.farClipPlane=70000;
            view.fieldOfView=48;view.allowHDR=true;view.gameObject.AddComponent<AudioListener>();
            view.gameObject.AddComponent<SeaAudioOutput>();
            view.depthTextureMode|=DepthTextureMode.Depth;
            ambience=gameObject.AddComponent<AudioSource>();ambience.loop=true;ambience.spatialBlend=0;
            if(Application.isPlaying) {ambience.clip=SeaAudio();ambience.Play();}
            gameObject.AddComponent<UmpirePanel>().scene=this;
            gameObject.AddComponent<RuleChart>().scene=this;
            gameObject.AddComponent<GunneryTools>().scene=this;
            gameObject.AddComponent<BatteryEffects>().scene=this;
            gameObject.AddComponent<EngineExhaust>().scene=this;
            if(Application.isPlaying&&Array.IndexOf(Environment.GetCommandLineArgs(),"-verify-runtime")>=0)gameObject.AddComponent<RuntimeVerification>();
            UpdateWorld();SetCamera();
        }
        void Awake() => Initialize();
        static AudioClip SeaAudio()
        {
            const int rate=22050,count=rate*12;var samples=new float[count];
            var random=new System.Random(817);float low=0;
            for(int i=0;i<count;i++) {
                low=Mathf.Lerp(low,(float)random.NextDouble()*2-1,.07f);
                float swell=.6f+.25f*Mathf.Sin(i/(float)rate*2*Mathf.PI/6)+.15f*Mathf.Sin(i/(float)rate*2*Mathf.PI/3);
                samples[i]=low*swell*.65f*Mathf.Min(1,Mathf.Min(i,count-1-i)/600f);
            }
            var clip=AudioClip.Create("Generated ocean ambience",count,1,rate,false);clip.SetData(samples,0);return clip;
        }
        public void UpdateWorld()
        {
            if(!initialized)return;
            Vector3 localSun=Navigation.SunDirection(scenario.Date,scenario.latitude,scenario.longitude);
            SunAltitude=Mathf.Asin(localSun.y)*Mathf.Rad2Deg;
            float daylight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.12f,.20f,localSun.y));
            Color haze=Color.Lerp(new Color(.012f,.018f,.035f),new Color(.49f,.64f,.70f),daylight);
            haze=Color.Lerp(haze,new Color(.32f,.38f,.42f)*(.15f+.85f*daylight),scenario.cloud*.68f);
            Color sunColor=Color.Lerp(new Color(1,.40f,.16f),new Color(1,.95f,.82f),Mathf.Clamp01(localSun.y*3));
            float strength=daylight*(1-scenario.cloud*.77f);
            Vector3 wind=Quaternion.Euler(0,scenario.windFrom+180,0)*Vector3.forward;
            Shader.SetGlobalVector("_SunDirection",localSun);Shader.SetGlobalColor("_SunColor",sunColor*strength);
            Shader.SetGlobalColor("_SkyTint",haze);Shader.SetGlobalFloat("_SeaTime",Application.isPlaying?Time.time:15);
            Shader.SetGlobalVector("_Wind",new Vector4(wind.x,wind.z,0,0));Shader.SetGlobalFloat("_WindSpeed",scenario.windKnots);
            oceanMaterial.SetFloat("_WaveHeight",scenario.waveHeight);skyMaterial.SetFloat("_Cloud",scenario.cloud);
            sun.color=sunColor;sun.intensity=strength*1.3f;
            Vector3 lightDir=globeMode?Navigation.GlobePoint(0,0):localSun;
            if(globeMode) {
                // At lat/lon 0, local up becomes globe +Z, north becomes +Y.
                Vector3 g=Navigation.SunDirection(scenario.Date,0,0);lightDir=new Vector3(-g.x,g.z,g.y);
                sun.intensity=1.1f;sun.color=Color.white;
            }
            sun.transform.rotation=Quaternion.LookRotation(-lightDir,Mathf.Abs(lightDir.y)>.99f?Vector3.forward:Vector3.up);
            RenderSettings.ambientSkyColor=globeMode?new Color(.03f,.03f,.035f):haze*(.47f+.15f*daylight);
            RenderSettings.ambientEquatorColor=globeMode?new Color(.015f,.015f,.02f):haze*.43f;
            RenderSettings.ambientGroundColor=globeMode?Color.black:new Color(.065f,.09f,.10f)*(.1f+.9f*daylight);
            RenderSettings.fog=!globeMode;RenderSettings.fogColor=haze;
            RenderSettings.fogDensity=1.52f/(scenario.visibilityKm*1000);
            seaRoot.gameObject.SetActive(!globeMode);globeRoot.gameObject.SetActive(globeMode);
            view.clearFlags=globeMode?CameraClearFlags.SolidColor:CameraClearFlags.Skybox;view.backgroundColor=new Color(.002f,.004f,.010f);
            marker.localPosition=Navigation.GlobePoint(scenario.latitude,scenario.longitude)*101.3f;
            for(int i=0;i<2;i++) {
                var s=scenario.ships[i];float time=Application.isPlaying?Time.time:15;
                vessels[i].localPosition=s.Position+Vector3.up*WaveAt(s.Position,time)*.65f;
                vessels[i].localRotation=Quaternion.Euler(Mathf.Sin(time*.6f+i)*scenario.waveHeight*.65f,s.heading,Mathf.Sin(time*.83f+i*2)*scenario.waveHeight*1.25f);
            }
            var emission=rain.emission;emission.rateOverTime=scenario.rain*2000;
            rain.transform.position=view.transform.position+Vector3.up*48;
            rain.transform.rotation=Quaternion.Euler(90,0,0);
            var velocity=rain.velocityOverLifetime;velocity.x=wind.x*scenario.windKnots*.25f;velocity.z=wind.z*scenario.windKnots*.25f;
            ambience.volume=soundEnabled&&!globeMode?.28f+scenario.windKnots*.007f:0;
        }
        public float WaveAt(Vector3 p,float t)
        {
            Vector3 d=Quaternion.Euler(0,scenario.windFrom+180,0)*Vector3.forward;
            return scenario.waveHeight*(.32f*Mathf.Sin((p.x*d.x+p.z*d.z)*.14f+t*1.2f)+.14f*Mathf.Sin((p.x*d.z-p.z*d.x)*.29f+t*1.7f)+.04f*Mathf.Sin(p.x*.73f+p.z*.6f+t*2.3f));
        }
        void Update()
        {
            if(!initialized)return;
            if(Input.GetKeyDown(KeyCode.Tab)) ToggleGlobe();
            if(Input.GetKeyDown(KeyCode.H))showOverlay=!showOverlay;
            if(Input.GetKeyDown(KeyCode.F))Focus(selected);
            if(Input.GetKeyDown(KeyCode.Alpha1))Focus(0);
            if(Input.GetKeyDown(KeyCode.Alpha2))Focus(1);
            if(Input.GetKeyDown(KeyCode.Space)&&!scenario.hexMode)animateTime=!animateTime;
            if(Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            if(Input.GetKeyDown(KeyCode.F12)) Screenshot();
            bool outsidePanel=!showOverlay || Input.mousePosition.x>330;
            if(outsidePanel) {
                if(Input.GetMouseButton(1)) {
                    float dx=Input.GetAxisRaw("Mouse X")*3,dy=Input.GetAxisRaw("Mouse Y")*3;
                    if(globeMode){globeYaw-=dx;globePitch=Mathf.Clamp(globePitch-dy,-85,85);}
                    else {yaw+=dx;pitch=Mathf.Clamp(pitch-dy,3,88);}
                }
                float scroll=Input.mouseScrollDelta.y;
                if(globeMode)globeDistance=Mathf.Clamp(globeDistance*Mathf.Exp(-scroll*.10f),115,600);
                else distance=Mathf.Clamp(distance*Mathf.Exp(-scroll*.12f),18,12000);
                if(!globeMode) {
                    Vector3 right=Quaternion.Euler(0,yaw,0)*Vector3.right,forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
                    if(Input.GetMouseButton(2))target-=right*Input.GetAxisRaw("Mouse X")*distance*.025f+forward*Input.GetAxisRaw("Mouse Y")*distance*.025f;
                    float x=0,z=0;if(Input.GetKey(KeyCode.A))x--;if(Input.GetKey(KeyCode.D))x++;if(Input.GetKey(KeyCode.W))z++;if(Input.GetKey(KeyCode.S))z--;
                    target+=(right*x+forward*z)*Time.deltaTime*distance*.45f;
                    target.x=Mathf.Clamp(target.x,-20000,20000);target.z=Mathf.Clamp(target.z,-20000,20000);
                } else if(Input.GetMouseButtonDown(0)) PickGlobe();
            }
            if(animateTime&&!scenario.hexMode) scenario.Advance(Time.deltaTime*60);
            SetCamera();UpdateWorld();
        }
        public void SetCamera()
        {
            if(globeMode) {
                Vector3 direction=Navigation.GlobePoint(globePitch,globeYaw);
                view.transform.position=globeRoot.position+direction*globeDistance;view.transform.LookAt(globeRoot.position);
                view.nearClipPlane=.1f;view.farClipPlane=1200;
            } else {
                view.transform.position=target+Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);
                view.transform.LookAt(target);view.nearClipPlane=.3f;view.farClipPlane=70000;
                nearOcean.transform.position=new Vector3(Mathf.Round(target.x/100)*100,0,Mathf.Round(target.z/100)*100);
            }
        }
        public void Focus(int i)
        {
            globeMode=false;selected=i;target=scenario.ships[i].Position+Vector3.up*(i==0?10:6);
            distance=i==0?160:100;pitch=15;yaw=scenario.ships[i].heading-65;
            UpdateWorld();SetCamera();
        }
        public void InspectDetail(bool bow)
        {
            globeMode=false;var ship=scenario.ships[selected];bool stonewall=selected==1;
            float length=stonewall?52.3756f:100.228f,deck=stonewall?1.85f:4.3f;
            Vector3 local=bow?new Vector3(0,deck,length*.47f):stonewall?new Vector3(0,deck+1.25f,22.0f):new Vector3(7.5f,deck-.85f,0);
            target=ship.Position+Quaternion.Euler(0,ship.heading,0)*local;
            yaw=ship.heading+(bow?-135:stonewall?-160:-90);pitch=bow?18:10;
            distance=bow?(stonewall?26:34):18;UpdateWorld();SetCamera();
        }
        public void InspectStern()
        {
            globeMode=false;var ship=scenario.ships[selected];float length=selected==0?100.228f:52.3756f,deck=selected==0?4.3f:1.85f;
            target=ship.Position+Quaternion.Euler(0,ship.heading,0)*new Vector3(0,deck-1,-length*.47f);
            yaw=ship.heading+32;pitch=18;distance=selected==0?29:24;UpdateWorld();SetCamera();
        }
        public void Overview() {globeMode=false;target=(scenario.ships[0].Position+scenario.ships[1].Position)/2+Vector3.up*5;distance=Mathf.Max(330,Vector3.Distance(scenario.ships[0].Position,scenario.ships[1].Position)*1.65f);pitch=32;UpdateWorld();SetCamera();}
        public void ToggleGlobe() {globeMode=!globeMode;if(globeMode){globeYaw=(float)scenario.longitude;globePitch=(float)scenario.latitude;}UpdateWorld();SetCamera();}
        void PickGlobe()
        {
            Ray ray=view.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray,out var hit,1000)) {
                Vector2 ll=Navigation.GlobeLatLon(hit.point-globeRoot.position);
                scenario.latitude=Math.Round(Mathf.Clamp(ll.x,-85,85),4);scenario.longitude=Math.Round(ll.y,4);
                status="Sea board relocated. Globe imagery is modern; local shorelines are not yet modeled.";
            }
        }
        public void Step(float seconds) {scenario.Advance(seconds);animateTime=false;status=$"Advanced {seconds:0} seconds. Movement is kinematic; adjudicate effects yourself.";}
        public void Screenshot()
        {
            string path=System.IO.Path.Combine(Application.persistentDataPath,"Ironclads-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png");
            ScreenCapture.CaptureScreenshot(path);status="Screenshot: "+path;
        }
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();
            int i=Array.IndexOf(args,"-capture");
            if(i>=0&&i+1<args.Length) {
                yield return new WaitForSeconds(2);yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(args[i+1]);yield return new WaitForSeconds(1);Application.Quit();
            }
        }
    }
}
