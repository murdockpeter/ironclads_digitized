using System.Linq;
using UnityEngine;

namespace Ironclads
{
    public class SuppliedShipBinding : MonoBehaviour
    {
        public Vector3[] outlets;
    }
    public static partial class Shipwright
    {
        public static GameObject CreateSuppliedHybrid(bool monitor)
        {
            if(!SuppliedShips.Available(monitor))return monitor?CreateMonitor():CreateVirginia();
            var root=(monitor?CreateMonitor(false):CreateVirginia(false)).transform;
            foreach(var f in root.GetComponentsInChildren<MeshFilter>()){
                bool keep=monitor?f.name!="Armored overhanging raft"&&f.name!="Deck plate seam"&&f.name!="Flush deck rivet":f.GetComponentInParent<GunMount>()!=null||f.name=="Low wooden hull"||f.name=="Broken ram stump below waterline";
                if(!keep)Object.DestroyImmediate(f.gameObject);
                else if(!monitor&&f.name=="Low wooden hull")f.transform.localPosition+=Vector3.down*.9f;
            }
            var paint=SuppliedShips.Material(monitor,!monitor);var original=monitor?paint:SuppliedShips.Material(false,false);
            var data=SuppliedShips.Data(monitor);
            foreach(var part in data.parts){
                if(monitor&&part.role!="hull"||!monitor&&part.role=="barrel")continue;
                Part("Supplied "+part.role+" "+part.id,root,SuppliedShips.Mesh(part),part.role=="flag"?original:paint);
            }
            var binding=root.gameObject.AddComponent<SuppliedShipBinding>();
            if(monitor){
                var turret=root.GetComponentInChildren<TurretAssembly>();Combine(turret.transform,4);
                binding.outlets=new[]{new Vector3(-.95f,.86f,-13),new Vector3(.95f,.86f,-13)};
            }else{
                var barrelParts=data.parts.Where(p=>p.role=="barrel").Select(p=>SuppliedShips.Mesh(p).bounds).ToArray();
                foreach(int side in new[]{-1,1}){
                    var positions=barrelParts.Where(p=>Mathf.Sign(p.center.x)==side&&Mathf.Abs(p.center.x)>1).OrderByDescending(p=>p.center.z).ToArray();
                    var guns=root.GetComponentsInChildren<GunMount>().Where(g=>g.Side==side).OrderByDescending(g=>g.transform.localPosition.z).ToArray();
                    for(int i=0;i<guns.Length&&i<positions.Length;i++){
                        var b=positions[i];Vector3 muzzle=new Vector3(side*(Mathf.Max(Mathf.Abs(b.min.x),Mathf.Abs(b.max.x))+.45f),b.center.y,b.center.z);
                        guns[i].transform.localPosition=muzzle-Vector3.right*side*(BarrelLength(guns[i].type)*.45f);
                    }
                }
                var shell=data.parts.First(p=>p.role=="casemate");Mesh shellMesh=SuppliedShips.Mesh(shell);
                foreach(var gun in root.GetComponentsInChildren<GunMount>().Where(g=>g.Side==0||g.Side==3)){
                    Vector3 direction=gun.Side==0?Vector3.forward:Vector3.back;float y=barrelParts.Average(b=>b.center.y);
                    Vector3 origin=new Vector3(0,y,0);float distance=RayShell(shellMesh,origin,direction);
                    if(distance>0)gun.transform.localPosition=origin+direction*(distance+.65f-BarrelLength(gun.type)*.45f);
                }
                var funnel=SuppliedShips.Mesh(data.parts.First(p=>p.role=="funnel")).bounds;
                binding.outlets=new[]{new Vector3(funnel.center.x,funnel.max.y,funnel.center.z)};
            }
            Combine(root,4);return root.gameObject;
        }
        static float RayShell(Mesh mesh,Vector3 origin,Vector3 direction)
        {
            float nearest=float.PositiveInfinity;var v=mesh.vertices;var t=mesh.triangles;
            for(int i=0;i<t.Length;i+=3){Vector3 a=v[t[i]],e=v[t[i+1]]-a,f=v[t[i+2]]-a,p=Vector3.Cross(direction,f);float det=Vector3.Dot(e,p);if(Mathf.Abs(det)<.000001f)continue;
                Vector3 s=origin-a;float u=Vector3.Dot(s,p)/det;if(u<0||u>1)continue;Vector3 q=Vector3.Cross(s,e);float w=Vector3.Dot(direction,q)/det;if(w<0||u+w>1)continue;float d=Vector3.Dot(f,q)/det;if(d>0)nearest=Mathf.Min(nearest,d);
            }return float.IsPositiveInfinity(nearest)?0:nearest;
        }
    }
}
