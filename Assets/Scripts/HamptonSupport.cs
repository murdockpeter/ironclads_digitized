using UnityEngine;

namespace Ironclads
{
    public class HamptonSupport : MonoBehaviour
    {
        public MaritimeScene scene;
        public Transform root,minnesota,congress,cumberland,patrickHenry;
        Transform[] paddleWheels;
        public static readonly Vector3 CongressPoint=HamptonTerrain.Point(36.963,-76.419);
        public static readonly Vector3 CumberlandPoint=HamptonTerrain.Point(36.955,-76.424);
        public void Build()
        {
            if(root!=null)return;
            root=new GameObject("Hampton Roads supporting ships").transform;root.SetParent(scene.seaRoot,false);
            minnesota=Shipwright.CreateMinnesota().transform;minnesota.SetParent(root,false);
            patrickHenry=Shipwright.CreatePatrickHenry().transform;patrickHenry.SetParent(root,false);
            var wheels=patrickHenry.GetComponentsInChildren<PatrickPaddle>();paddleWheels=new Transform[wheels.Length];for(int i=0;i<wheels.Length;i++)paddleWheels[i]=wheels[i].transform;
            congress=Shipwright.CreateCongressWreck().transform;congress.SetParent(root,false);congress.localPosition=CongressPoint;congress.localRotation=Quaternion.Euler(0,40,-5);
            cumberland=Shipwright.CreateCumberlandWreck().transform;cumberland.SetParent(root,false);cumberland.localPosition=CumberlandPoint;cumberland.localRotation=Quaternion.Euler(0,90,0);
        }
        public void Refresh()
        {
            if(root==null&&scene.scenario.Hampton)Build();if(root==null)return;
            root.gameObject.SetActive(scene.scenario.Hampton);if(!scene.scenario.Hampton)return;
            var s=scene.scenario.minnesota;minnesota.localPosition=s.Position+Vector3.up*.3f;minnesota.localRotation=Quaternion.Euler(.5f,s.heading,-1.5f);
            var p=scene.scenario.patrickHenry;float time=Application.isPlaying?Time.time:15;
            patrickHenry.localPosition=p.Position+Vector3.up*scene.WaveAt(p.Position,time)*.5f;patrickHenry.localRotation=Quaternion.Euler(Mathf.Sin(time*.6f)*scene.scenario.waveHeight*.5f,p.heading,Mathf.Sin(time*.8f)*scene.scenario.waveHeight);
            foreach(var wheel in paddleWheels)wheel.localRotation=Quaternion.Euler(p.boilersLit?time*p.engineLoad*60*(p.astern?-1:1):0,0,0);
            Vector3 coastOffset=scene.terrain!=null?scene.terrain.root.localPosition:Vector3.zero;
            congress.localPosition=CongressPoint+coastOffset;cumberland.localPosition=CumberlandPoint+coastOffset;
        }
        public void Inspect(int ship)
        {
            scene.globeMode=false;scene.GetComponent<RuleChart>().topDown=false;
            Transform target=ship==2?minnesota:ship==3?congress:cumberland;
            scene.target=target.position+Vector3.up*(ship==2?10:5);scene.distance=ship==2?150:ship==3?95:110;scene.pitch=20;scene.yaw=ship==2?scene.scenario.minnesota.heading-65:-35;
            scene.UpdateWorld();scene.SetCamera();
        }
        public void InspectPatrick()
        {
            scene.globeMode=false;scene.GetComponent<RuleChart>().topDown=false;scene.target=patrickHenry.position+Vector3.up*6;scene.distance=125;scene.pitch=18;scene.yaw=scene.scenario.patrickHenry.heading-65;scene.UpdateWorld();scene.SetCamera();
        }
        void OnGUI()
        {
            if(scene==null||!scene.scenario.Hampton||scene.globeMode||!scene.showOverlay||minnesota==null)return;
            var style=new GUIStyle(GUI.skin.label){fontSize=12,normal={textColor=new Color(.65f,.79f,.78f)}};
            Label(minnesota.position+Vector3.up*50,"USS Minnesota · aground",style);
            Label(patrickHenry.position+Vector3.up*25,"CSS Patrick Henry",style);
            Label(congress.position+Vector3.up*6,"USS Congress · burned wreck",style);
            Label(cumberland.position+Vector3.up*23,"USS Cumberland · sunken wreck",style);
        }
        void Label(Vector3 point,string text,GUIStyle style){Vector3 p=scene.view.WorldToScreenPoint(point);if(p.z>0&&p.x>350&&p.x<Screen.width-50&&p.y>50&&p.y<Screen.height-95)GUI.Label(new Rect(p.x-90,Screen.height-p.y,240,25),text,style);}
    }
}
