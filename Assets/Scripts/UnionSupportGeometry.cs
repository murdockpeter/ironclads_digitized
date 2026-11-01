using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        public static GameObject CreateMinnesota()
        {
            Materials();var root=new GameObject("USS Minnesota — grounded steam frigate").transform;
            const float length=80.6831f,beam=15.6464f,deck=4.3f;
            var stations=new float[14];for(int i=0;i<14;i++)stations[i]=-29.25f+i*4.5f;
            HullForm(root,length,beam,deck,7.112f,Hull,stations);
            // Supplied Minnesota card: 14 heavy and 7 light broadside guns per side.
            for(int side=-1;side<=1;side+=2){
                foreach(float z in stations){float w=HullHalfWidth(z,length,beam);Gun(root,new Vector3(side*(w-1),DeckHeight(z,length,deck)-.85f,z),Vector3.right*side,GunType.DahlgrenIX).GetComponent<GunMount>().batterySide=side;}
                for(int i=0;i<7;i++){float z=-25+i*8;float w=HullHalfWidth(z,length,beam);Gun(root,new Vector3(side*(w-1),DeckHeight(z,length,deck)+1.05f,z),Vector3.right*side,GunType.DahlgrenVIII).GetComponent<GunMount>().batterySide=side;}
                for(int j=5;j<123;j++){float a=(j/128f-.5f)*length,b=((j+1)/128f-.5f)*length;if(InPort(a,stations)||InPort(b,stations)||InPort((a+b)*.5f,stations))continue;
                    Vector3 p=new Vector3(side*(HullHalfWidth(a,length,beam)+.04f),DeckHeight(a,length,deck)-1,a),q=new Vector3(side*(HullHalfWidth(b,length,beam)+.04f),DeckHeight(b,length,deck)-1,b);
                    Quad(root,"White gun-deck band",p+Vector3.up*.3f,q+Vector3.up*.3f,q-Vector3.up*.3f,p-Vector3.up*.3f,White);
                }
            }
            Gun(root,new Vector3(0,deck+1.15f,32),Vector3.forward,GunType.DahlgrenX).GetComponent<GunMount>().batterySide=0;
            Mast(root,25,deck,37,11.3f,beam);Mast(root,0,deck,41,12.5f,beam);Mast(root,-26,deck,32,9.3f,beam);
            Rod(root,"Single steam funnel",new Vector3(0,deck,12),new Vector3(0,13.8f,12),.95f,Iron,sides:28);
            Rod(root,"Funnel cap",new Vector3(0,13.6f,12),new Vector3(0,14,12),1.05f,Black,sides:28);
            Rod(root,"Bowsprit",new Vector3(0,deck+1,34),new Vector3(0,deck+5.6f,53),.33f,Wood,.09f);
            for(int side=-1;side<=1;side+=2)Rod(root,"Bowsprit bobstay",new Vector3(side*1.5f,1,38),new Vector3(0,deck+5.6f,53),.045f,Rope);
            Fittings(root,deck,length,beam);Box(root,"Quarterdeck companion",new Vector3(0,deck+.7f,-31),new Vector3(3.8f,1.4f,4),Hull);
            // Ground tackle remains fixed with the grounded ship.
            for(int side=-1;side<=1;side+=2){Rod(root,"Anchor cable",new Vector3(side*2,2.6f,37),new Vector3(side*7,-3,49),.065f,Iron,sides:8);Rod(root,"Cathead",new Vector3(side*2,deck+1,32),new Vector3(side*6,deck+1.4f,35),.15f,Wood,sides:8);}
            Combine(root);return root.gameObject;
        }
        public static GameObject CreateCongressWreck()
        {
            Materials();var root=new GameObject("USS Congress — burned and exploded wreck").transform;
            var charred=MakeMaterial("Charred Congress timbers",new Color(.055f,.043f,.032f));
            const float length=54.9f,beam=12.8f;var v=new List<Vector3>();var t=new List<int>();
            // Torn open sides: no intact weather deck or complete sailing rig on March 9.
            for(int i=0;i<=64;i++){float f=i/64f,z=(f-.5f)*length,w=beam*.5f*Width(f),height=.18f+.45f*Mathf.Sin(i*2.7f)+.25f*Mathf.Cos(i*.8f);
                v.Add(new Vector3(-w,height,z));v.Add(new Vector3(-w*.85f,-2.6f,z));v.Add(new Vector3(w*.85f,-2.6f,z));v.Add(new Vector3(w,height+.2f*Mathf.Sin(i),z));
                if(i<64)for(int j=0;j<3;j++){int a=i*4+j;t.AddRange(new[]{a,a+1,a+4,a+1,a+5,a+4,a,a+4,a+1,a+1,a+4,a+5});}
                if(i%3==0&&i>3&&i<61){for(int side=-1;side<=1;side+=2)Rod(root,"Broken burned frame",new Vector3(side*w*.6f,-1.2f,z),new Vector3(side*w,height+.7f,z),.13f,charred,sides:6);}
            }
            // Split normals on double-sided damaged planking rather than cancelling faces.
            var mesh=Mesh("Ruptured burned shell",v,t);var normals=mesh.normals;for(int i=0;i<normals.Length;i++)if(normals[i].sqrMagnitude<.1f)normals[i]=Vector3.up;mesh.normals=normals;Part("Burned hull remains",root,mesh,charred);
            for(int i=0;i<22;i++){float z=-23+i*2.1f,x=Mathf.Sin(i*19)*3;var timber=Box(root,"Collapsed charred deck beam",new Vector3(x,.1f,z),new Vector3(5+(i%3),.16f,.24f),charred);timber.transform.localRotation=Quaternion.Euler(i%5*3,i*31,8);}
            for(int i=-1;i<=1;i++){float z=i*16;Rod(root,"Burned mast stump",new Vector3(0,-2,z),new Vector3(.3f*i,1.5f+(i+1)*.7f,z+1.5f),.32f,charred,.14f);}
            Rod(root,"Fallen yard",new Vector3(-8,.2f,-9),new Vector3(8,.45f,-1),.13f,charred,.07f);
            Combine(root);return root.gameObject;
        }
        public static GameObject CreateCumberlandWreck()
        {
            Materials();var root=new GameObject("USS Cumberland — sunken sloop with exposed masts").transform;
            // Submerged hull is intact in outline, unlike Congress's burned shell.
            var hull=new GameObject("Submerged razee hull").transform;hull.SetParent(root,false);hull.localPosition=new Vector3(0,-13,0);hull.localRotation=Quaternion.Euler(0,0,-35);
            Raft(hull,"Submerged hull",53.3f,13.7f,0,-5,Hull);
            var rig=new GameObject("Leaning surviving rig").transform;rig.SetParent(root,false);rig.localPosition=new Vector3(0,-13,0);rig.localRotation=Quaternion.Euler(0,0,-40);
            Mast(rig,16,0,35,9.5f,13.7f);Mast(rig,0,0,39,11,13.7f);Mast(rig,-17,0,30,8.5f,13.7f);
            Rod(root,"Exposed bow timber",new Vector3(0,-3,25),new Vector3(.8f,.8f,27.5f),.28f,Wood,.12f);
            Rod(root,"Loose spar at waterline",new Vector3(-8,.05f,-6),new Vector3(3,.2f,-9),.11f,Wood,.06f);
            Combine(root);return root.gameObject;
        }
    }
}
