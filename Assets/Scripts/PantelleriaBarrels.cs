using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        // Separate surface rings preserve the hard muzzle lip and inward bore normals.
        public static Mesh PantelleriaBarrel(float caliber,float length)
        {
            const int sides=60;var v=new List<Vector3>();var triangles=new List<int>();
            float rear=Mathf.Max(.065f,caliber*.8f),front=Mathf.Max(.04f,caliber*.65f),bore=caliber*.5f,depth=Mathf.Min(.45f,caliber*1.3f);
            void Band(float z0,float r0,float z1,float r1)
            {
                int start=v.Count;
                for(int ring=0;ring<2;ring++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Cos(a)*(ring==0?r0:r1),Mathf.Sin(a)*(ring==0?r0:r1),ring==0?z0:z1));}
                for(int i=0;i<sides;i++){int a=start+i,n=start+(i+1)%sides,b=a+sides,q=n+sides;triangles.AddRange(new[]{a,n,b,n,q,b});}
            }
            Band(-.7f,rear,length,front);Band(length,front,length,bore);Band(length,bore,length-depth,bore);
            int centre=v.Count;v.Add(new Vector3(0,0,-.7f));for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Cos(a)*rear,Mathf.Sin(a)*rear,-.7f));}
            for(int i=0;i<sides;i++)triangles.AddRange(new[]{centre,centre+1+(i+1)%sides,centre+1+i});
            return Mesh("Stepped tube with recessed bore",v,triangles);
        }
    }
}
