using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public class PatrickPaddle : MonoBehaviour { }
    public static partial class Shipwright
    {
        public static GameObject CreatePatrickHenry()
        {
            Materials();var root=new GameObject("CSS Patrick Henry").transform;
            const float length=76.2f,beam=10.3632f,deck=2.6f;
            var casing=MakeMaterial("Patrick Henry paddle-box finish",new Color(.40f,.38f,.32f));
            HullForm(root,length,beam,deck,3.9624f,Hull);
            // Battle roster: twelve guns. Rochelle's account supplies the working layout:
            // ten broadside 32-pounders plus a 10-inch bow and 8-inch stern pivot.
            foreach(int side in new[]{-1,1}){
                foreach(float z in new[]{-25f,-18f,8f,16f,24f}){
                    float x=HullHalfWidth(z,length,beam);
                    Gun(root,new Vector3(side*(x-.85f),DeckHeight(z,length,deck)+1.05f,z),Vector3.right*side,GunType.Smoothbore32).GetComponent<GunMount>().batterySide=side;
                }
                // Paddle wheels turn independently, with the upper semicircle behind its box.
                var wheel=new GameObject(side<0?"Port paddle wheel":"Starboard paddle wheel").transform;wheel.SetParent(root,false);wheel.localPosition=new Vector3(side*6.0f,1.4f,-3);
                wheel.gameObject.AddComponent<PatrickPaddle>();wheel.gameObject.AddComponent<TurretAssembly>();
                for(int ring=-1;ring<=1;ring+=2)CruiserLoop(wheel,"Paddle wheel rim",new Vector3(ring*.7f,0,0),Vector3.right,3.05f,.10f,Iron,48);
                Rod(wheel,"Paddle axle",new Vector3(-.9f,0,0),new Vector3(.9f,0,0),.23f,Iron,sides:16);
                for(int blade=0;blade<16;blade++){
                    float a=blade*Mathf.PI/8;Vector3 p=new Vector3(0,Mathf.Sin(a)*2.9f,Mathf.Cos(a)*2.9f);
                    var board=Box(wheel,"Timber paddle float",p,new Vector3(1.65f,.12f,.75f),Wood);board.transform.localRotation=Quaternion.Euler(-a*Mathf.Rad2Deg,0,0);
                    foreach(int ring in new[]{-1,1})Rod(wheel,"Paddle spoke",new Vector3(ring*.7f,0,0),new Vector3(ring*.7f,p.y,p.z),.06f,Iron,sides:6);
                }
                Combine(wheel,2);
                PatrickPaddleBox(root,side,casing);
                for(float z=-length*.45f;z<length*.45f;z+=1.5f){float w=HullHalfWidth(z,length,beam);float y=DeckHeight(z,length,deck);
                    if(z> -8&&z<2)continue;
                    Rod(root,"Gunwale rail post",new Vector3(side*w,y,z),new Vector3(side*w,y+.8f,z),.035f,Wood,sides:4);
                    float next=Mathf.Min(z+1.5f,length*.45f);Rod(root,"Gunwale handrail",new Vector3(side*w,y+.8f,z),new Vector3(side*HullHalfWidth(next,length,beam),DeckHeight(next,length,deck)+.8f,next),.045f,Wood,sides:5);
                }
            }
            Gun(root,new Vector3(0,3.9f,30),Vector3.forward,GunType.DahlgrenX).GetComponent<GunMount>().batterySide=0;
            var sternGun=Gun(root,new Vector3(0,3.9f,-30),Vector3.back,GunType.DahlgrenVIII);sternGun.name="Eight-inch solid-shot pivot (reconstructed)";sternGun.GetComponent<GunMount>().batterySide=3;
            Box(root,"Low deckhouse",new Vector3(0,3.4f,-3),new Vector3(5.6f,1.6f,19),casing);
            Box(root,"Walking beam engine well",new Vector3(0,4.35f,-3),new Vector3(3,.3f,8),Black);
            Rod(root,"Single funnel",new Vector3(0,3.9f,5),new Vector3(0,13.5f,5),.78f,Iron,sides:36);
            Rod(root,"Funnel cap",new Vector3(0,13.25f,5),new Vector3(0,13.8f,5),.85f,Black,sides:36);
            for(int ring=0;ring<5;ring++)CruiserLoop(root,"Funnel band",new Vector3(0,5.2f+ring*1.7f,5),Vector3.up,.79f,.025f,Iron,24);
            foreach(int side in new[]{-1,1}){
                Rod(root,"Engine beam A-frame",new Vector3(side*1.3f,4.4f,-6),new Vector3(side*.45f,7.8f,-3),.13f,Iron,sides:10);
                Rod(root,"Engine beam A-frame",new Vector3(side*.45f,7.8f,-3),new Vector3(side*1.3f,4.4f,0),.13f,Iron,sides:10);
                Rod(root,"Walking beam",new Vector3(side*.45f,7.5f,-6.5f),new Vector3(side*.45f,8.1f,.5f),.12f,Iron,sides:10);
                Rod(root,"Engine linkage",new Vector3(side*.45f,7.5f,-6.5f),new Vector3(side*.45f,4.4f,-6.5f),.07f,Iron,sides:8);
            }
            foreach(int end in new[]{-1,1}){
                float z=end*22,height=end>0?23:20;
                Rod(root,"Two-masted brigantine rig",new Vector3(0,deck,z),new Vector3(0,height,z),.22f,Wood,.07f,sides:20);
                for(int level=0;level<3;level++){float y=height-3-level*3;float span=(end>0?4.7f:3.6f)+level;
                    Rod(root,"Bare yard",new Vector3(-span,y,z),new Vector3(span,y,z),.065f,Wood,sides:10);
                }
                foreach(int side in new[]{-1,1})for(int stay=0;stay<3;stay++)Rod(root,"Standing rigging",new Vector3(0,height-4,z),new Vector3(side*3.6f,deck,z-end*(3+stay)),.018f,Rope,sides:4);
                Rod(root,"Fore and aft stay",new Vector3(0,height,z),new Vector3(0,3,end*35),.018f,Rope,sides:4);
            }
            Rod(root,"Bowsprit",new Vector3(0,3.5f,33),new Vector3(0,5.1f,45),.18f,Wood,.045f,sides:16);
            Rod(root,"Bowsprit bobstay",new Vector3(0,.5f,36),new Vector3(0,5.1f,45),.025f,Rope,sides:6);
            for(int side=-1;side<=1;side+=2){var boat=new GameObject("Ship's boat").transform;boat.SetParent(root,false);boat.localPosition=new Vector3(side*3,4.5f,-13);boat.localScale=new Vector3(.65f,.65f,.8f);HullFormSmall(boat);}
            foreach(float z in new[]{-14f,14f}){Box(root,"Companion hatch",new Vector3(0,3,z),new Vector3(2,.7f,2.5f),Wood);Rod(root,"Ventilator",new Vector3(1.8f,deck,z),new Vector3(1.8f,4.7f,z),.25f,Iron,sides:16);}
            PatrickEnsign(root);Combine(root,4);return root.gameObject;
        }
        static void PatrickPaddleBox(Transform root,int side,Material material)
        {
            var v=new List<Vector3>();var t=new List<int>();const int n=64;float inner=side*5.05f,outer=side*7.05f;
            for(int j=0;j<=n;j++){float a=j*Mathf.PI/n,y=1.4f+Mathf.Sin(a)*3.4f,z=-3+Mathf.Cos(a)*3.4f;v.Add(new Vector3(inner,y,z));v.Add(new Vector3(outer,y,z));if(j<n){int q=j*2;t.AddRange(side<0?new[]{q,q+2,q+1,q+1,q+2,q+3}:new[]{q,q+1,q+2,q+1,q+3,q+2});}}
            int center=v.Count;v.Add(new Vector3(outer,1.4f,-3));for(int j=0;j<n;j++)t.AddRange(side<0?new[]{center,j*2+1,(j+1)*2+1}:new[]{center,(j+1)*2+1,j*2+1});
            // The side panel and roof share the same material, avoiding a bright seam.
            Part("Paddle-box outer panel",root,Mesh("Semicircular wheel cover",v,t),material);
            Box(root,"Paddle-box sponson",new Vector3(side*6,1.5f,-3),new Vector3(2.2f,.35f,7.4f),material);
            for(int j=-3;j<=3;j++)Box(root,"Paddle-box slatted trim",new Vector3(side*7.07f,1.95f,-3+j*.78f),new Vector3(.08f,.55f,.44f),Black);
            CruiserLoop(root,"Paddle-box circular trim",new Vector3(side*7.08f,3.4f,-3),Vector3.right,.55f,.055f,material,32);
        }
        static void PatrickEnsign(Transform root)
        {
            Material red=MakeMaterial("Patrick Henry ensign red",new Color(.47f,.04f,.04f)),blue=MakeMaterial("Patrick Henry ensign blue",new Color(.04f,.08f,.20f));
            Rod(root,"Stern ensign staff",new Vector3(0,3,-35),new Vector3(0,7,-35),.035f,Wood,sides:8);
            for(int j=0;j<3;j++)Quad(root,"First National ensign stripe",new Vector3(0,5.4f+j*.3f,-35),new Vector3(1.7f,5.4f+j*.3f,-35),new Vector3(1.7f,5.7f+j*.3f,-35),new Vector3(0,5.7f+j*.3f,-35),j==1?White:red);
            Quad(root,"Ensign canton",new Vector3(0,5.7f,-35.01f),new Vector3(.7f,5.7f,-35.01f),new Vector3(.7f,6.3f,-35.01f),new Vector3(0,6.3f,-35.01f),blue);
        }
    }
}
