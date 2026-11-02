using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public class DawnNavalTable : MonoBehaviour
    {
        public MaritimeScene scene;
        public int target=3;
        public bool measure=true,arcVisible;
        public bool lowLightAid=true;
        public float arcCenter=90,arcWidth=120,arcKyd=12;
        LineRenderer ruler,arc;
        readonly List<ParticleSystem> exhaust=new List<ParticleSystem>();
        readonly List<int> exhaustShips=new List<int>();
        readonly List<float> emission=new List<float>();
        readonly ParticleSystem.Particle[] cloud=new ParticleSystem.Particle[1500];
        Material smokeMaterial;
        public int ExhaustCount=>exhaust.Count;
        public bool Own(int i)=>scene.network==null||scene.network.CanControl(i);
        public float Remaining(int i)=>Mathf.Max(0,scene.scenario.dobTurnSeconds-scene.scenario.ships[i].dobMovedSeconds);
        public void ResetEffects(){foreach(var p in exhaust)if(p!=null){p.gameObject.SetActive(false);if(Application.isPlaying)Destroy(p.gameObject);else DestroyImmediate(p.gameObject);}exhaust.Clear();exhaustShips.Clear();emission.Clear();}
        public bool Move(int ship,float seconds)
        {
            if(!scene.scenario.DoB||!Own(ship)||!float.IsFinite(seconds)||seconds<=0||seconds>Remaining(ship)+.01f)return false;
            var s=scene.scenario.ships[ship];var end=s.Position+Quaternion.Euler(0,s.heading,0)*Vector3.forward*UlsanScenario.Distance(s.speed*(s.astern?-1:1),seconds);
            if(!UlsanScenario.Inside(end))return false;GetComponent<RuleChart>().Remember();s.east=end.x;s.north=end.z;s.dobMovedSeconds+=seconds;scene.network?.SubmitShip(ship);
            scene.status=$"{s.name}: moved {seconds:0.0} seconds at {s.speed:0.0} knots. Clock advances separately.";return true;
        }
        public bool AdvanceTurn(int ship,float degrees,bool hard)
        {
            if(!scene.scenario.DoB||!Own(ship)||!float.IsFinite(degrees)||Mathf.Abs(degrees)>45||Mathf.Abs(degrees)<.01f)return false;
            var s=scene.scenario.ships[ship];if(s.speed<=0)return false;
            float advance=(hard?200:300)*.9144f,seconds=advance/(s.speed*1852/3600);
            if(seconds>Remaining(ship)+.01f)return false;
            Vector3 forward=Quaternion.Euler(0,s.heading,0)*Vector3.forward;float heading=Mathf.Repeat(s.heading+degrees,360),bridgeOffset=UlsanScenario.Ships[ship].length*.22f;
            Vector3 bridge=s.Position+forward*(advance*(s.astern?-1:1)+bridgeOffset),end=bridge-Quaternion.Euler(0,heading,0)*Vector3.forward*bridgeOffset;
            if(!UlsanScenario.Inside(end))return false;GetComponent<RuleChart>().Remember();s.east=end.x;s.north=end.z;s.heading=heading;s.dobMovedSeconds+=seconds;scene.network?.SubmitShip(ship);
            scene.status=$"{s.name}: {(hard?200:300)} yd advance, bridge pivot {degrees:+0;-0}°. Apply effective speed after turn loss and acceleration manually.";return true;
        }
        public void EndTurn()
        {
            if(scene.network!=null&&!scene.network.CanManage)return;GetComponent<RuleChart>().Remember();var s=scene.scenario;s.utc=s.Date.AddSeconds(s.dobTurnSeconds).ToString("yyyy-MM-ddTHH:mm:ssZ");s.turn++;
            foreach(var ship in s.ships)ship.dobMovedSeconds=0;scene.status="DoB turn advanced; movement rulers reset. Positions retained; adjudicate detection, fire and damage.";
        }
        public bool Place(int ship,Vector3 point)
        {
            if(!Own(ship)||!UlsanScenario.Inside(point))return false;GetComponent<RuleChart>().Remember();var s=scene.scenario.ships[ship];s.east=point.x;s.north=point.z;scene.network?.SubmitShip(ship);return true;
        }
        void BuildEffects()
        {
            if(exhaust.Count>0)return;
            if(smokeMaterial==null){smokeMaterial=new Material(Shader.Find("Ironclads/Powder Smoke"));smokeMaterial.mainTexture=BatteryEffects.SmokeTexture(true);}
            for(int ship=0;ship<scene.vessels.Length;ship++)foreach(var funnel in scene.vessels[ship].GetComponentsInChildren<CruiserFunnel>()){
                var go=new GameObject("Cruiser coal exhaust");go.transform.SetParent(funnel.transform,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=ps.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=1500;main.startSpeed=0;main.startLifetime=25;main.startSize=1;
                var e=ps.emission;e.enabled=false;var shape=ps.shape;shape.enabled=false;
                var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(.3f,3.5f),new Keyframe(1,6)));
                var color=ps.colorOverLifetime;color.enabled=true;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.7f,0),new GradientAlphaKey(.5f,.5f),new GradientAlphaKey(0,1)});color.color=g;
                var sheet=ps.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=sheet.numTilesY=2;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(0);sheet.startFrame=new ParticleSystem.MinMaxCurve(0,3.999f);
                var r=ps.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=smokeMaterial;r.sortMode=ParticleSystemSortMode.Distance;r.maxParticleSize=.8f;ps.Play();exhaust.Add(ps);exhaustShips.Add(ship);emission.Add(0);
            }
        }
        void LateUpdate()
        {
            if(scene==null||scene.seaRoot==null)return;
            if(!scene.scenario.DoB){if(ruler!=null){ruler.gameObject.SetActive(false);arc.gameObject.SetActive(false);}if(exhaust.Count>0)ResetEffects();return;}
            target=Mathf.Clamp(target,0,6);
            if(ruler==null){var material=new Material(Shader.Find("Ironclads/Chart"));material.SetColor("_Color",new Color(.45f,1,.85f,.85f));ruler=WorldBuilder.Line(scene.seaRoot,"DoB range and bearing",new Vector3[2],material,1.1f);arc=WorldBuilder.Line(scene.seaRoot,"DoB custom angle ruler",new Vector3[2],material,.8f);}
            bool show=!scene.globeMode&&scene.showOverlay;ruler.gameObject.SetActive(show&&measure);arc.gameObject.SetActive(show&&arcVisible);Shader.SetGlobalFloat("_ChartWaveHeight",scene.scenario.waveHeight);
            Vector3 origin=scene.scenario.ships[scene.selected].Position+Vector3.up*.5f;ruler.SetPositions(new[]{origin,scene.scenario.ships[target].Position+Vector3.up*.5f});
            if(arcVisible){var points=new Vector3[99];points[0]=points[98]=origin;for(int i=0;i<=96;i++)points[i+1]=origin+Quaternion.Euler(0,scene.scenario.ships[scene.selected].heading+arcCenter-arcWidth/2+arcWidth*i/96,0)*Vector3.forward*(arcKyd*914.4f);arc.positionCount=points.Length;arc.SetPositions(points);}
            if(show&&Input.GetKey(KeyCode.LeftShift)&&Input.GetMouseButtonDown(0)&&Input.mousePosition.x>330){var plane=new Plane(Vector3.up,Vector3.zero);var ray=scene.view.ScreenPointToRay(Input.mousePosition);if(plane.Raycast(ray,out float d))Place(scene.selected,ray.GetPoint(d));}
            if(!Application.isPlaying)return;BuildEffects();
            if(scene.globeMode){foreach(var ps in exhaust)ps.Clear();return;}
            float dt=Mathf.Min(.1f,Time.deltaTime);Vector3 wind=BatteryEffects.Wind(scene.scenario.windFrom,scene.scenario.windKnots);
            for(int f=0;f<exhaust.Count;f++){
                var ps=exhaust[f];var ship=scene.scenario.ships[exhaustShips[f]];emission[f]+=(ship.boilersLit?5+20*ship.engineLoad:0)*dt;
                int count=Mathf.FloorToInt(emission[f]);emission[f]-=count;
                for(int j=0;j<count;j++){float shade=Random.Range(.16f,.30f);ps.Emit(new ParticleSystem.EmitParams{position=ps.transform.position+Random.insideUnitSphere*.3f,velocity=Vector3.up*Random.Range(2,5),startLifetime=Random.Range(19,34),startSize=Random.Range(1.4f,2.6f),startColor=new Color(shade,shade,shade*.98f,.65f),rotation=Random.Range(0,360),randomSeed=(uint)Random.Range(1,1000000)},1);}
                int live=ps.GetParticles(cloud);for(int j=0;j<live;j++){float age=cloud[j].startLifetime-cloud[j].remainingLifetime;cloud[j].velocity=BatteryEffects.TransportVelocity(cloud[j].velocity,wind,age,dt,(cloud[j].randomSeed%1024)*.17f,Time.time,cloud[j].position);}ps.SetParticles(cloud,live);
            }
        }
    }
}
