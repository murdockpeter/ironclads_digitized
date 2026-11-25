using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class CanopusAssetBuilder
    {
        const string Folder="Assets/Resources/PantelleriaReplacements";
        public static void EnsureAsset(){if(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Canopus.prefab")==null)Rebuild();}
        [MenuItem("Ironclads/Rebuild Canopus replacement asset")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            // Delete only this generator's three named outputs, preserving the definition and other replacements.
            foreach(string file in new[]{"Canopus.prefab","CanopusGeometry.asset","CanopusMaterials.asset"})if(AssetDatabase.LoadMainAssetAtPath(Folder+"/"+file)!=null)AssetDatabase.DeleteAsset(Folder+"/"+file);
            var go=Shipwright.AuthorCanopusReplacement();
            try {
                // The existing marker classes are runtime types in shared source files.
                // Persist typed transform references, then restore these bindings on load.
                var bindings=go.GetComponent<CanopusReplacement>();var turrets=go.GetComponentsInChildren<CruiserTurret>();var funnels=go.GetComponentsInChildren<CruiserFunnel>();
                bindings.turretRoots=turrets.Select(t=>t.transform).ToArray();bindings.turretGroups=turrets.Select(t=>t.group).ToArray();bindings.exhaustOutlets=funnels.Select(f=>f.transform).ToArray();
                foreach(var turret in turrets){UnityEngine.Object.DestroyImmediate(turret.GetComponent<TurretAssembly>());UnityEngine.Object.DestroyImmediate(turret);}foreach(var funnel in funnels)UnityEngine.Object.DestroyImmediate(funnel);
                var meshes=new Dictionary<Mesh,Mesh>();var materials=new Dictionary<Material,Material>();int meshIndex=0,materialIndex=0;
                foreach(var filter in go.GetComponentsInChildren<MeshFilter>()) {
                    var source=filter.sharedMesh;if(!meshes.TryGetValue(source,out var mesh)) {
                        mesh=UnityEngine.Object.Instantiate(source);mesh.name="Canopus "+(++meshIndex)+" "+source.name;
                        if(meshIndex==1)AssetDatabase.CreateAsset(mesh,Folder+"/CanopusGeometry.asset");else AssetDatabase.AddObjectToAsset(mesh,Folder+"/CanopusGeometry.asset");meshes[source]=mesh;
                    }filter.sharedMesh=mesh;
                }
                foreach(var renderer in go.GetComponentsInChildren<MeshRenderer>()) {
                    var source=renderer.sharedMaterial;if(!materials.TryGetValue(source,out var material)) {
                        material=new Material(source){name="Canopus "+(++materialIndex)+" "+source.name};
                        if(materialIndex==1)AssetDatabase.CreateAsset(material,Folder+"/CanopusMaterials.asset");else AssetDatabase.AddObjectToAsset(material,Folder+"/CanopusMaterials.asset");materials[source]=material;
                    }renderer.sharedMaterial=material;
                }
                PrefabUtility.SaveAsPrefabAsset(go,Folder+"/Canopus.prefab");AssetDatabase.SaveAssets();AssetDatabase.Refresh();
                Debug.Log("CANOPUS ASSET: saved prefab, "+meshIndex+" meshes, "+materialIndex+" materials.");
            } finally {UnityEngine.Object.DestroyImmediate(go);}
        }
        public static void Review()
        {
            int checks=0;void Check(bool ok,string description){if(!ok)throw new Exception("Canopus replacement: "+description);checks++;}
            bool preference=PantelleriaModels.CanopusEnabled;PantelleriaModels.SelectCanopus(true,false);
            GameObject prototype=null,previous=null,ocean=null;
            try {
                Check(PantelleriaModels.CanopusAvailable,"replacement prefab loads");prototype=Shipwright.CreatePantelleriaShip(0);previous=Shipwright.CreatePantelleriaTemplate(0);ocean=Shipwright.CreatePantelleriaShip(1);
                Check(prototype.GetComponent<CanopusReplacement>()!=null&&ocean.GetComponent<CanopusReplacement>()==null,"only Canopus is swapped");
                var d=CanopusReplacement.Definition;var hull=prototype.transform.Find("Canopus distinct lofted hull").GetComponent<MeshFilter>().sharedMesh;
                Check(Mathf.Abs(hull.bounds.size.z-d.loa)<.01f&&Mathf.Abs(hull.bounds.size.x-d.beam)<.01f,"replacement hull overall dimensions");
                Check(Mathf.Abs(hull.bounds.min.y+d.draft)<.001f,"replacement design draft");
                // Raycasts cull back faces. Sample the stem/counter from outside,
                // including both halves, to catch the formerly inverted end faces.
                var hullObject=prototype.transform.Find("Canopus distinct lofted hull").gameObject;
                var surface=hullObject.AddComponent<MeshCollider>();surface.sharedMesh=hull;
                try {
                    foreach(int end in new[]{-1,1})foreach(float x in new[]{-.25f,.25f})foreach(float y in new[]{1f,3f,5f}) {
                        var origin=new Vector3(x,y,end*(d.loa*.5f+10));
                        Check(surface.Raycast(new Ray(origin,Vector3.back*end),out var hit,25)&&hit.normal.z*end>0,"outward hull surface at end "+end+" x "+x+" y "+y);
                    }
                } finally {UnityEngine.Object.DestroyImmediate(surface);}
                Check(d.waterlineWidths.Length==21&&d.deckWidths.Length==21&&d.fractions.Length==21,"21 documented outline stations");
                Check(d.WaterlineWidth(d.waterlineAft-1)==0&&d.WaterlineWidth(d.waterlineFore+1)==0,"station width outside waterline ends");
                Check(Mathf.Abs(d.WaterlineWidth(0)-d.beam*.5f)<.05f,"full middle waterline beam");
                var pmeshes=prototype.GetComponentsInChildren<MeshFilter>(true);Check(pmeshes.All(m=>m.sharedMesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z))),"finite replacement and flag geometry");
                var detail=prototype.transform.Find("Canopus close fittings");
                Check(detail!=null&&detail.GetComponentsInChildren<MeshFilter>().Length==210,"additional close fittings are present");
                var coloursForCount=prototype.GetComponent<NavalColours>();
                var visibleBody=pmeshes.Where(m=>!coloursForCount.IsRigging(m.transform)).ToArray();
                Check(visibleBody.Length>=400&&visibleBody.Length<=440,"replacement mesh budget approximately doubles the previous 210");
                Debug.Log("CANOPUS MODEL: "+visibleBody.Sum(m=>m.sharedMesh.vertexCount)+" vertices, "+visibleBody.Length+" mesh instances including ensign and pennant, excluding decorative hoists.");
                Check(prototype.GetComponentsInChildren<GunMount>().Length==22&&prototype.GetComponentsInChildren<CruiserTurret>().Length==2,"existing gun slots and trainable turrets");
                Check(prototype.GetComponentsInChildren<Transform>(true).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"prefab has no missing scripts after runtime binding");
                Check(prototype.GetComponentsInChildren<CruiserFunnel>().Length==2,"two working exhaust anchors");
                foreach(var turret in prototype.GetComponentsInChildren<TurretAssembly>()) {
                    var root=turret.transform;float expected=d.DeckHeight(root.localPosition.z);
                    float bottom=root.GetComponentsInChildren<MeshFilter>().SelectMany(m=>m.sharedMesh.vertices.Select(v=>prototype.transform.InverseTransformPoint(m.transform.TransformPoint(v)).y)).Min();
                    Check(bottom<=expected+.01f&&bottom>=expected-.15f,"barbette intersects the sheer deck");
                }
                foreach(var gun in prototype.GetComponentsInChildren<GunMount>())Check(float.IsFinite(gun.Muzzle.x)&&gun.Direction.sqrMagnitude>.99f,"valid replacement muzzle "+gun.name);
                var colours=prototype.GetComponent<NavalColours>();Check(colours!=null&&colours.Nation==0&&Mathf.Abs(colours.Pennant.localPosition.z-d.foreMastZ)<.001f,"national colours use replacement mast anchors");
                Check(Mathf.Abs(colours.Ensign.localPosition.z-d.aftMastZ)<.001f,"ensign uses replacement after mast");
                var previousHull=previous.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.vertexCount);Check(pmeshes.Sum(m=>m.sharedMesh.vertexCount)!=previousHull,"replacement is distinct geometry");
                Check(pmeshes.Where(m=>m.GetComponent<MeshRenderer>().sharedMaterial.shader.name!="Ironclads/Naval Cloth"&&!colours.IsRigging(m.transform)).All(m=>EditorUtility.IsPersistent(m.sharedMesh)),"static replacement geometry comes from the prefab assets");
                Directory.CreateDirectory("Artifacts/Pantelleria/Canopus");
                File.WriteAllText("Artifacts/Pantelleria/Canopus/editor-verification.txt","PASS: "+checks+" replacement checks; prefab, independent hull, dimensions, draft, source stations, 22 gun slots, turrets, funnels, mast anchors and saved meshes. Underwater form remains inferred.");
                Debug.Log("CANOPUS PASS: "+checks+" replacement checks.");
            } finally {if(prototype!=null)UnityEngine.Object.DestroyImmediate(prototype);if(previous!=null)UnityEngine.Object.DestroyImmediate(previous);if(ocean!=null)UnityEngine.Object.DestroyImmediate(ocean);PantelleriaModels.SelectCanopus(preference,false);}
        }
    }
}
