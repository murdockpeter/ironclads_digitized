using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        // Local to Pantelleria: other modules keep their existing tessellation.
        static void BattleRod(Transform parent,string name,Vector3 a,Vector3 b,float radius,Material material,float tip=-1,int sides=12)
            =>Rod(parent,name,a,b,radius,material,tip,Mathf.CeilToInt(sides*1.25f));

        static void BattleAnchorCable(Transform parent,Vector3 start,Vector3 end)
        {
            var v=new List<Vector3>();var triangles=new List<int>();
            var along=(end-start).normalized;var across=Vector3.Cross(Vector3.up,along).normalized;
            int links=Mathf.Max(2,Mathf.CeilToInt(Vector3.Distance(start,end)/.22f));
            const int around=24,tube=8;
            for(int link=0;link<links;link++) {
                var centre=Vector3.Lerp(start,end,(link+.5f)/links);
                var side=Quaternion.AngleAxis(link%2==0?35:-35,along)*across;
                var normal=Vector3.Cross(along,side).normalized;int offset=v.Count;
                for(int i=0;i<around;i++)for(int j=0;j<tube;j++) {
                    float a=i*Mathf.PI*2/around,b=j*Mathf.PI*2/tube;
                    var radial=(along*Mathf.Cos(a)+side*Mathf.Sin(a)).normalized;
                    v.Add(centre+along*(.16f*Mathf.Cos(a))+side*(.075f*Mathf.Sin(a))+.022f*(radial*Mathf.Cos(b)+normal*Mathf.Sin(b)));
                    int p=offset+i*tube+j,q=offset+((i+1)%around)*tube+j,r=offset+i*tube+(j+1)%tube,s=offset+((i+1)%around)*tube+(j+1)%tube;
                    triangles.AddRange(new[]{p,q,r,r,q,s});
                }
            }
            Part("Interlocking anchor chain",parent,Mesh("Oval anchor cable links",v,triangles),Iron);
        }
    }
}
