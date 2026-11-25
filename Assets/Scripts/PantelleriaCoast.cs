using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public class PantelleriaCoast : MonoBehaviour
    {
        [Serializable] class Data { public HamptonTerrain.GeoPolygon[] polygons; }
        Transform root;double originLat,originLon;
        public void Refresh(MaritimeScene scene)
        {
            if(root==null&&scene.scenario.Pantelleria)Build(scene.scenario);
            if(root==null)return;root.gameObject.SetActive(scene.scenario.Pantelleria);if(!scene.scenario.Pantelleria)return;
            root.localPosition=new Vector3((float)((originLon-scene.scenario.longitude)*Math.PI/180*Navigation.EarthRadius*Math.Cos(scene.scenario.latitude*Math.PI/180)),0,(float)((originLat-scene.scenario.latitude)*Math.PI/180*Navigation.EarthRadius));
        }
        void Build(Scenario s)
        {
            var source=Resources.Load<TextAsset>("Geography/PantelleriaCoast");if(source==null)throw new InvalidOperationException("Pantelleria shoreline resource missing.");var data=JsonUtility.FromJson<Data>(source.text);originLat=s.latitude;originLon=s.longitude;
            root=new GameObject("GSHHG Mediterranean shoreline / flat display relief").transform;root.SetParent(GetComponent<MaritimeScene>().seaRoot,false);var mat=Shipwright.MakeMaterial("Mediterranean shore",new Color(.46f,.43f,.28f));
            foreach(var polygon in data.polygons) {
                var vertices=new List<Vector3>();foreach(var p in polygon.points)vertices.Add(new Vector3((float)((p.longitude-originLon)*Math.PI/180*Navigation.EarthRadius*Math.Cos(originLat*Math.PI/180)),2,(float)((p.latitude-originLat)*Math.PI/180*Navigation.EarthRadius)));
                var go=new GameObject("Geographic island / coastal land");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=Shipwright.Mesh("GSHHG coastal polygon",vertices,new List<int>(polygon.triangles));go.AddComponent<MeshRenderer>().sharedMaterial=mat;
            }
        }
    }
}
