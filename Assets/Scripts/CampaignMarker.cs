using System.Collections.Generic;
using UnityEngine;
namespace Ironclads {
    // Enlarged operational symbols, not researched individual ship models.
    public static class CampaignMarker {
        static GameObject Part(Transform root,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material material) {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }
        public static bool IsResource(string type)=>type=="Shore battery"||type=="Signal station"||type=="Torpedo field"||type=="Obstruction"||type=="Support facility"||type=="Stores"||type=="Small boats"||type=="Personnel"||type=="Transports"||type=="Tug";
        public static void Build(Transform root,string type,Material hull,Material deck,Material iron,Material side,bool detail,List<Object> owned) {
            if(IsResource(type)){BuildResource(root,type,hull,deck,iron,side);return;}
            BuildPrevious(root,type,hull,deck,iron,side,detail,owned);
            if(detail)Refine(root,type,hull,deck,iron,owned);
        }
        public static void BuildPrevious(Transform root,string type,Material hull,Material deck,Material iron,Material side,bool detail,List<Object> owned) {
            Part(root,"Hull",PrimitiveType.Cube,new Vector3(0,8,0),new Vector3(25,12,68),hull);
            Part(root,"Deck",PrimitiveType.Cube,new Vector3(0,15,0),new Vector3(23,3,63),deck);
            var bow=Part(root,"Rounded bow",PrimitiveType.Sphere,new Vector3(0,8,29),new Vector3(24,12,25),hull);
            bool monitor=type=="Double-turret monitor";
            if(type=="Ironclad")Part(root,"Casemate",PrimitiveType.Cube,new Vector3(0,22,0),new Vector3(20,14,38),iron);
            else if(monitor)foreach(float z in new[]{-19f,19f}) {
                Part(root,"Monitor turret",PrimitiveType.Cylinder,new Vector3(0,22,z),new Vector3(18,5,18),iron);
                Part(root,detail?"Turret roof sight":"Turret barrels",PrimitiveType.Cube,new Vector3(0,23+(detail?4:0),z+(detail?0:11)),detail?new Vector3(3,1,3):new Vector3(10,2,15),hull);
            }
            else Part(root,"Deckhouse",PrimitiveType.Cube,new Vector3(0,23,0),new Vector3(17,15,24),deck);
            Part(root,"Funnel",PrimitiveType.Cylinder,new Vector3(0,34,-3),new Vector3(7,12,7),hull);
            Part(root,"Side identification pennant",PrimitiveType.Cube,new Vector3(8,52,-21),new Vector3(15,9,2),side);
            Part(root,"Mast",PrimitiveType.Cylinder,new Vector3(0,37,-21),new Vector3(1.4f,20,1.4f),hull);
            if(!detail)return;
            var mesh=BowMesh();owned.Add(mesh);bow.GetComponent<MeshFilter>().sharedMesh=mesh;
            foreach(int end in new[]{-1,1})foreach(int gun in new[]{-1,1}) {
                float z=end*19;
                var barrel=Part(root,"Separate gun barrel",PrimitiveType.Cylinder,new Vector3(gun*3,monitor?24:22,z+end*10),new Vector3(1.8f,7,1.8f),hull);
                barrel.transform.localRotation=Quaternion.Euler(90,0,0);
            }
            foreach(int s in new[]{-1,1}) {
                foreach(float z in new[]{-28f,28f}) {
                    Part(root,"Raised deck hatch",PrimitiveType.Cube,new Vector3(s*6,17,z),new Vector3(4,1.5f,5),iron);
                    Part(root,"Mooring bitt",PrimitiveType.Cylinder,new Vector3(s*9,18,z),new Vector3(1.6f,1.6f,1.6f),hull);
                }
                Part(root,"Bridge glazing",PrimitiveType.Cube,new Vector3(s*(type=="Ironclad"?10.2f:8.6f),monitor?18:26,4),new Vector3(.4f,3,11),hull);
                Part(root,"Stowed boat",PrimitiveType.Sphere,new Vector3(s*10.8f,17.5f,-10),new Vector3(3,2,11),deck);
                Part(root,"Deck edge rail",PrimitiveType.Cube,new Vector3(s*11.5f,18,0),new Vector3(.35f,.6f,49),iron);
            }
            foreach(float y in new[]{27f,43f})Part(root,"Funnel rim band",PrimitiveType.Cylinder,new Vector3(0,y,-3),new Vector3(7.4f,.45f,7.4f),iron);
            Part(root,"Signal yard",PrimitiveType.Cube,new Vector3(0,49,-21),new Vector3(16,.6f,.6f),hull);
            if(monitor) {
                foreach(int s in new[]{-1,1})foreach(float z in new[]{-8f,8f}) {
                    Part(root,"Monitor deck ventilator",PrimitiveType.Cylinder,new Vector3(s*7,18,z),new Vector3(2,2,2),iron);
                    Part(root,"Turret access cover",PrimitiveType.Cube,new Vector3(s*4,28,z>0?19:-19),new Vector3(2,.7f,3),deck);
                }
                Part(root,"Pilot position",PrimitiveType.Sphere,new Vector3(0,19,33),new Vector3(5,4,5),iron);
            }
        }
        static GameObject Rod(Transform root,string name,Vector3 a,Vector3 b,float radius,Material material) {
            var go=Part(root,name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(radius*2,Vector3.Distance(a,b)*.5f,radius*2),material);go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return go;
        }
        static void Refine(Transform root,string type,Material hull,Material deck,Material iron,List<Object> owned) {
            bool monitor=type=="Double-turret monitor";float roof=monitor?27:type=="Ironclad"?29:30.5f;
            var bow=BowMesh(64,48);owned.Add(bow);root.Find("Rounded bow").GetComponent<MeshFilter>().sharedMesh=bow;
            var ring=RingMesh();owned.Add(ring);
            foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="Deck edge rail")t.localPosition+=Vector3.up*2.5f;
            foreach(int side in new[]{-1,1}) {
                for(int i=0;i<12;i++)Rod(root,"Individual rail stanchion",new Vector3(side*11.5f,16.5f,-24+i*48f/11),new Vector3(side*11.5f,20.5f,-24+i*48f/11),.17f,iron);
                for(int i=0;i<8;i++) {
                    float z=monitor?(i<4?-19:19)+(i%4-1.5f)*3.5f:-16+i*32f/7;
                    Part(root,"Raised armor seam",PrimitiveType.Cube,new Vector3(side*4,roof+.09f,z),new Vector3(.2f,.18f,2.5f),iron);
                    Rod(root,"Stowed boat thwart",new Vector3(side*10.8f-1.05f,18.15f,-14+i*8f/7),new Vector3(side*10.8f+1.05f,18.15f,-14+i*8f/7),.12f,deck);
                }
                foreach(float z in new[]{-28f,28f}) {
                    foreach(float dz in new[]{0f,-3f}) {var go=new GameObject("Mooring eye and fairlead");go.transform.SetParent(root,false);go.transform.localPosition=new Vector3(side*9,19.7f,z+dz);go.AddComponent<MeshFilter>().sharedMesh=ring;go.AddComponent<MeshRenderer>().sharedMaterial=iron;}
                    foreach(float x in new[]{side*6-2.1f,side*6+2.1f})Part(root,"Hatch coaming",PrimitiveType.Cube,new Vector3(x,17.2f,z),new Vector3(.25f,1.2f,5.3f),iron);
                }
                foreach(float z in new[]{-12f,-4f,4f,12f})Part(root,"Raised ventilator",PrimitiveType.Cylinder,new Vector3(side*6,roof+1,z),new Vector3(1.5f,1,1.5f),iron);
                foreach(float dz in new[]{-6f,6f})Rod(root,"Funnel guy stay",new Vector3(0,39,-3),new Vector3(side*8,16.6f,-3+dz),.10f,hull);
            }
            if(monitor)foreach(float z in new[]{-19f,19f}) {
                for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var p=new Vector3(Mathf.Sin(a)*8.7f,24,z+Mathf.Cos(a)*8.7f);Rod(root,"Turret armor fastener",p,p+new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*.35f,.3f,hull);}
                for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;var p=new Vector3(Mathf.Sin(a)*7,27,z+Mathf.Cos(a)*7);Rod(root,"Turret roof guard",p,p+Vector3.up*2,.16f,iron);Part(root,"Turret access latch",PrimitiveType.Cylinder,new Vector3((i%2==0?-1:1)*3,27.5f,z+(i<2?-3:3)),new Vector3(.8f,.3f,.8f),iron);}
                foreach(int side in new[]{-1,1})Part(root,"Gun sight housing",PrimitiveType.Cube,new Vector3(side*3,28,z+2),new Vector3(1,.7f,1.4f),hull);
            }
            if(type=="Torpedo vessel") {
                int n=0;foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="Separate gun barrel") {t.name=n==0?"Torpedo spar":"Deck handling spar";t.localPosition=n==0?new Vector3(0,16,39):new Vector3((n-2)*3,18,-24);t.localScale=new Vector3(.5f,n==0?12:2,.5f);n++;}
            }
        }
        public static void ApplyIdentity(Transform root,CampaignVessel vessel) {
            if(vessel.supportResource)return;
            int count;if(string.IsNullOrEmpty(vessel.guideArmament)||!int.TryParse(vessel.guideArmament.Split(' ')[0],out count))return;
            int i=0;foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="Separate gun barrel") {
                bool armed=count>=4||(count==2&&i%2==0)||(count==1&&i==2);
                if(!armed){t.name="Deck ventilator pipe";t.localPosition=new Vector3((i-1.5f)*3,18,-24);t.localScale=new Vector3(.8f,1.5f,.8f);t.localRotation=Quaternion.identity;}
                i++;
            }
        }
        static Mesh RingMesh() {
            const int around=32,tube=12;var v=new List<Vector3>();var t=new List<int>();
            for(int a=0;a<=around;a++)for(int b=0;b<=tube;b++){float x=a*Mathf.PI*2/around,y=b*Mathf.PI*2/tube,r=.7f+.16f*Mathf.Cos(y);v.Add(new Vector3(Mathf.Cos(x)*r,.16f*Mathf.Sin(y),Mathf.Sin(x)*r));}
            for(int a=0;a<around;a++)for(int b=0;b<tube;b++){int i=a*(tube+1)+b,j=i+tube+1;t.AddRange(new[]{i,i+1,j,i+1,j+1,j});}return Shipwright.Mesh("Campaign mooring rings",v,t);
        }
        static void BuildResource(Transform root,string type,Material hull,Material deck,Material iron,Material side) {
            if(type=="Small boats"||type=="Transports"||type=="Tug") {
                foreach(float x in new[]{-12f,0f,12f}) {Part(root,"Symbolic group hull",PrimitiveType.Sphere,new Vector3(x,5,0),new Vector3(9,5,32),hull);Part(root,"Working deck",PrimitiveType.Cube,new Vector3(x,7,0),new Vector3(7,2,26),deck);Part(root,"Small craft funnel",PrimitiveType.Cylinder,new Vector3(x,12,-3),new Vector3(2,4,2),iron);}
            } else if(type=="Torpedo field"||type=="Obstruction") {
                for(int i=0;i<6;i++){var p=new Vector3((i%3-1)*12,3,(i/3-.5f)*20);Part(root,type=="Torpedo field"?"Symbolic torpedo":"Obstruction crib",type=="Torpedo field"?PrimitiveType.Sphere:PrimitiveType.Cube,p,new Vector3(6,6,6),iron);Rod(root,"Field boundary stake",p,p+Vector3.up*10,.4f,deck);}
            } else if(type=="Shore battery") {
                Part(root,"Earthwork platform",PrimitiveType.Cube,new Vector3(0,3,0),new Vector3(40,6,38),deck);
                foreach(int s in new[]{-1,1})foreach(float z in new[]{-14f,14f}) {Part(root,"Earthwork parapet",PrimitiveType.Cube,new Vector3(s*16,9,z),new Vector3(9,9,12),deck);Rod(root,"Battery gun",new Vector3(s*8,10,z),new Vector3(s*8,10,z+14),1.2f,hull);foreach(float x in new[]{s*8-2,s*8+2}){var wheel=Part(root,"Gun carriage wheel",PrimitiveType.Cylinder,new Vector3(x,8,z+3),new Vector3(4,.6f,4),iron);wheel.transform.localRotation=Quaternion.Euler(0,0,90);}}
            } else {
                Part(root,type=="Signal station"?"Signal post base":"Resource shelter",PrimitiveType.Cube,new Vector3(0,6,0),new Vector3(26,12,22),deck);
                foreach(int s in new[]{-1,1}) {var roof=Part(root,"Shelter roof",PrimitiveType.Cube,new Vector3(s*6,14,0),new Vector3(15,1,26),hull);roof.transform.localRotation=Quaternion.Euler(0,0,-s*24);for(int i=0;i<4;i++)Part(root,"Symbolic stores or personnel",type=="Personnel"?PrimitiveType.Capsule:PrimitiveType.Cylinder,new Vector3(s*17,4,-12+i*8),new Vector3(3,4,3),iron);}
            }
            Rod(root,"Resource identification staff",new Vector3(-18,1,-20),new Vector3(-18,35,-20),.4f,hull);Part(root,"Resource side pennant",PrimitiveType.Cube,new Vector3(-12,31,-20),new Vector3(12,7,1),side);
        }
        static Mesh BowMesh(int around=32,int rows=24) {var v=new List<Vector3>();var t=new List<int>();
            for(int y=0;y<=rows;y++)for(int x=0;x<=around;x++) {
                float a=x*Mathf.PI*2/around,b=y*Mathf.PI/rows;float radius=y==0||y==rows?0:Mathf.Sin(b);v.Add(new Vector3(radius*Mathf.Cos(a),Mathf.Cos(b),radius*Mathf.Sin(a))*.5f);
            }
            for(int y=0;y<rows;y++)for(int x=0;x<around;x++){int a=y*(around+1)+x,b=a+around+1;if(y>0)t.AddRange(new[]{a,a+1,b});if(y<rows-1)t.AddRange(new[]{a+1,b+1,b});}
            return Shipwright.Mesh("Refined campaign marker bow",v,t);
        }
    }
}
