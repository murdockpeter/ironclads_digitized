using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    [Serializable] public class SuppliedPart
    {
        public int id;public string role;public float[] positions,normals,uv;public int[] triangles;
    }
    [Serializable] public class SuppliedShipData
    {
        public string name,source,sha256;public float scale,hullLength,beam;public SuppliedPart[] parts;
    }
    public static class SuppliedShips
    {
        static readonly SuppliedShipData[] cache=new SuppliedShipData[2];
        public static SuppliedShipData Data(bool monitor)
        {
            int i=monitor?0:1;if(cache[i]!=null)return cache[i];var source=Resources.Load<TextAsset>("SuppliedShips/"+(monitor?"Monitor":"Virginia")+"Data");
            return source==null?null:(cache[i]=JsonUtility.FromJson<SuppliedShipData>(source.text));
        }
        public static bool Available(bool monitor)=>Data(monitor)!=null&&Resources.Load<Texture2D>("SuppliedShips/"+(monitor?"Monitor":"Virginia")+"Albedo")!=null;
        public static Mesh Mesh(SuppliedPart part)
        {
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();
            for(int i=0;i<part.positions.Length;i+=3){vertices.Add(new Vector3(part.positions[i],part.positions[i+1],part.positions[i+2]));normals.Add(new Vector3(part.normals[i],part.normals[i+1],part.normals[i+2]).normalized);}
            for(int i=0;i<part.uv.Length;i+=2)uv.Add(new Vector2(part.uv[i],part.uv[i+1]));
            var mesh=Shipwright.Mesh("Supplied "+part.role+" "+part.id,vertices,new List<int>(part.triangles),uv);mesh.SetNormals(normals);return mesh;
        }
        public static Material Material(bool monitor,bool neutralize)
        {
            var material=new Material(Shader.Find("Ironclads/Supplied Ship")){name=(monitor?"Monitor":"Virginia")+" supplied atlas"};material.mainTexture=Resources.Load<Texture2D>("SuppliedShips/"+(monitor?"Monitor":"Virginia")+"Albedo");material.SetFloat("_NeutralizeRed",neutralize?1:0);return material;
        }
        public static GameObject Raw(bool monitor,bool neutralize=false)
        {
            var root=new GameObject((monitor?"Monitor":"Virginia")+" supplied source review");var material=Material(monitor,neutralize);
            foreach(var part in Data(monitor).parts){var go=new GameObject(part.role+" "+part.id);go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=Mesh(part);go.AddComponent<MeshRenderer>().sharedMaterial=material;}
            return root;
        }
    }
}
