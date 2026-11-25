using UnityEngine;

namespace Ironclads
{
    public sealed class PantelleriaWash : MonoBehaviour
    {
        readonly Vector4[] positions=new Vector4[42],profiles=new Vector4[42],motions=new Vector4[42],hydro=new Vector4[42];
        readonly float[] sprayBudget=new float[42*6];
        readonly MaterialPropertyBlock nearProperties=new MaterialPropertyBlock();
        ParticleSystem spray;Material sprayMaterial;Texture2D sprayTexture;Mesh originalOcean,fineOcean;MeshFilter oceanFilter;
        public int ActiveCount {get;private set;}
        public int HullContactCount {get;private set;}
        public int TotalSprayEmitted {get;private set;}
        public int SprayParticleCount=>spray!=null?spray.particleCount:0;
        public static float Strength(float knots)=>Mathf.InverseLerp(.25f,18,Mathf.Max(0,knots));
        public void Refresh(MaritimeScene scene)
        {
            ActiveCount=HullContactCount=0;bool enabled=scene.scenario.Pantelleria&&!scene.globeMode;
            Shader.SetGlobalFloat("_PantelleriaSea",scene.scenario.Pantelleria?1:0);
            if(oceanFilter==null){oceanFilter=scene.NearOceanSurface;originalOcean=oceanFilter.sharedMesh;var renderer=oceanFilter.GetComponent<MeshRenderer>();renderer.GetPropertyBlock(nearProperties);nearProperties.SetFloat("_PantelleriaFinePatch",1);renderer.SetPropertyBlock(nearProperties);}
            if(scene.scenario.Pantelleria&&fineOcean==null)fineOcean=WorldBuilder.Surface(2400,768);
            oceanFilter.sharedMesh=scene.scenario.Pantelleria?fineOcean:originalOcean;
            float time=Application.isPlaying?Time.time:15;
            if(enabled)for(int ship=0;ship<42;ship++) {
                var state=scene.scenario.ships[ship];float distance=Vector3.Distance(scene.view.transform.position,state.Position),strength=Strength(state.speed);
                if(scene.scenario.pantelleria.sunk[ship]||distance>=1500||(strength==0&&scene.scenario.waveHeight==0))continue;
                var model=PantelleriaScenario.Ships[ship].model;float sign=state.astern?-1:1,fade=Mathf.InverseLerp(1500,600,distance);
                var forward=Quaternion.Euler(0,state.heading+(state.astern?180:0),0)*Vector3.forward;var vessel=scene.vessels[ship];
                int slot=HullContactCount++;
                positions[slot]=new Vector4(state.east,state.north,forward.x,forward.z);
                profiles[slot]=new Vector4(model.length,model.beam,strength*fade*(state.astern?.45f:1),ship*1.731f);
                motions[slot]=new Vector4(vessel.localPosition.y,vessel.right.y*sign,vessel.forward.y*sign,fade);
                hydro[slot]=new Vector4(PantelleriaHydrodynamics.WakeLength(state.speed),PantelleriaHydrodynamics.Froude(state.speed,model.length),sign,scene.scenario.waveHeight);
                if(strength>0)ActiveCount++;
                if(Application.isPlaying&&distance<450&&scene.scenario.waveHeight>0)EmitSpray(scene,ship,time,sign,fade);
            }
            if(!enabled||HullContactCount==0){if(spray!=null)spray.Clear();System.Array.Clear(sprayBudget,0,sprayBudget.Length);}
            Shader.SetGlobalVectorArray("_PantelleriaWashPositions",positions);Shader.SetGlobalVectorArray("_PantelleriaWashProfiles",profiles);
            Shader.SetGlobalVectorArray("_PantelleriaHullMotion",motions);Shader.SetGlobalVectorArray("_PantelleriaWashHydro",hydro);Shader.SetGlobalInteger("_PantelleriaWashCount",HullContactCount);
        }
        void MakeSpray(MaritimeScene scene)
        {
            var go=new GameObject("Wave-impact droplets");go.transform.SetParent(scene.seaRoot,false);spray=go.AddComponent<ParticleSystem>();spray.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=spray.main;main.loop=true;main.playOnAwake=false;main.maxParticles=500;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=1;
            spray.useAutoRandomSeed=false;spray.randomSeed=613;
            var emission=spray.emission;emission.enabled=false;var shape=spray.shape;shape.enabled=false;
            var fade=spray.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(0,1)});fade.color=gradient;
            sprayMaterial=new Material(Shader.Find("Ironclads/Weather Sprite"));sprayTexture=new Texture2D(32,32,TextureFormat.RGBA32,false);
            var pixels=new Color[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++){float r=new Vector2((x-15.5f)/16,(y-15.5f)/16).magnitude;pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2));}sprayTexture.SetPixels(pixels);sprayTexture.Apply();sprayMaterial.mainTexture=sprayTexture;
            var renderer=spray.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=sprayMaterial;renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=1.3f;renderer.velocityScale=.04f;spray.Play();
        }
        void EmitSpray(MaritimeScene scene,int ship,float time,float sign,float fade)
        {
            var state=scene.scenario.ships[ship];var model=PantelleriaScenario.Ships[ship].model;var vessel=scene.vessels[ship];
            var pose=PantelleriaHydrodynamics.Pose(state,model,time,scene.scenario.waveHeight,scene.scenario.windFrom);
            var wind=Quaternion.Euler(0,scene.scenario.windFrom+180,0)*Vector3.forward;
            for(int sample=0;sample<6;sample++) {
                int side=sample%2==0?-1:1;float z=sign*model.length*(.27f+(sample/2)*.075f),width=PantelleriaHydrodynamics.HalfWidth(model.length,model.beam,z);
                var point=vessel.TransformPoint(new Vector3(side*width,0,z));var wave=PantelleriaHydrodynamics.Sample(point,time,scene.scenario.waveHeight,scene.scenario.windFrom);
                var outward=vessel.right*side;float exposure=.3f+.7f*Mathf.Max(0,Vector3.Dot(outward,-wind));
                float impact=PantelleriaHydrodynamics.Impact(wave.x-point.y,wave.w-pose.w,scene.scenario.waveHeight)*exposure;
                int index=ship*6+sample;sprayBudget[index]+=Mathf.Min(Time.deltaTime,.05f)*impact*16*fade;
                int count=Mathf.Min(4,Mathf.FloorToInt(sprayBudget[index]));if(count==0)continue;sprayBudget[index]-=count;
                if(spray==null)MakeSpray(scene);
                for(int drop=0;drop<count;drop++) {
                    float phase=Mathf.Repeat((TotalSprayEmitted+ship*23+sample*7)*.618034f,1);
                    float light=Mathf.Clamp(RenderSettings.ambientSkyColor.maxColorComponent,.15f,.9f);
                    var particle=new ParticleSystem.EmitParams{position=new Vector3(point.x,wave.x+.06f,point.z)+outward*.13f,
                        velocity=outward*(.5f+phase)+Vector3.up*(2+impact*1.3f)+wind*scene.scenario.windKnots*.025f-vessel.forward*sign*state.speed*.035f,
                        startLifetime=.35f+phase*.3f,startSize=.09f+phase*.16f,startColor=new Color(.78f*light,.85f*light,.85f*light,.52f),randomSeed=(uint)(TotalSprayEmitted+1)};
                    spray.Emit(particle,1);TotalSprayEmitted++;
                }
            }
        }
        void OnDestroy()
        {
            Shader.SetGlobalInteger("_PantelleriaWashCount",0);Shader.SetGlobalFloat("_PantelleriaSea",0);
            if(oceanFilter!=null&&oceanFilter.sharedMesh==fineOcean)oceanFilter.sharedMesh=originalOcean;
            if(fineOcean!=null)Destroy(fineOcean);if(spray!=null)Destroy(spray.gameObject);if(sprayMaterial!=null)Destroy(sprayMaterial);if(sprayTexture!=null)Destroy(sprayTexture);
        }
    }
}
