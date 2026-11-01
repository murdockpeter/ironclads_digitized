using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ironclads.Editor
{
    public static class ProjectBuilder
    {
        const string ScenePath="Assets/Scenes/Ferrol.unity";
        public static void Deliver(){Prepare();BuildWindows();}
        public static void DeliverPass2(){Prepare();GeometryReview.Run();BuildWindows();}
        public static void DeliverPass3(){Prepare();GeometryReview.RunPass3();BuildWindows();}
        public static void DeliverBowRevision(){Prepare();GeometryReview.RunBowRevision();BuildWindowsTo("Builds/Windows-BowRevision");}
        public static void DeliverPass4(){Prepare();GeometryReview.RunPass4();BuildWindowsTo("Builds/Windows-Pass4");}
        public static void DeliverSalvos(){Prepare();GeometryReview.RunPass4();BuildWindowsTo("Builds/Windows-Salvos");}
        public static void DeliverSalvoRealism(){Prepare();BatteryRealismReview.Verify();GeometryReview.RunPass4();BuildWindowsTo("Builds/Windows-Salvos-Realism");}
        public static void DeliverEngineSmoke(){Prepare();BatteryRealismReview.Verify();GeometryReview.RunPass4();BuildWindowsTo("Builds/Windows-EngineSmoke");}
        public static void DeliverRelease(){Prepare();BuildWindowsTo("Builds/Windows-"+PlayerSettings.bundleVersion);}
        public static void DeliverMultiplayer(){Prepare();BuildWindowsTo("Builds/Windows-TCP");}
        public static void DeliverHamptonRoads(){Prepare();HamptonReview.Run();BuildWindowsTo("Builds/Windows-HamptonRoads");}
        public static void DeliverUnionShips(){Prepare();HamptonReview.Run();BuildWindowsTo("Builds/Windows-HamptonRoads-Union");}
        [MenuItem("Ironclads/Prepare project and ship assets")]
        public static void Prepare()
        {
            Directory.CreateDirectory("Assets/Generated");Directory.CreateDirectory("Artifacts");
            PlayerSettings.companyName="Ironclads";PlayerSettings.productName="Ironclads — Ferrol";
            PlayerSettings.bundleVersion="1.2.1-dev";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=1000;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
            PlayerSettings.resizableWindow=true;PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/EarthMarch.jpg");
            if(importer!=null){importer.maxTextureSize=8192;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.wrapModeU=TextureWrapMode.Repeat;importer.wrapModeV=TextureWrapMode.Clamp;importer.SaveAndReimport();}
            var si=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/StonewallSheet.png");
            if(si!=null){si.maxTextureSize=2048;si.textureCompression=TextureImporterCompression.Uncompressed;si.SaveAndReimport();}
            var ni=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/NiagaraSheet.jpg");
            if(ni!=null){ni.maxTextureSize=4096;ni.textureCompression=TextureImporterCompression.CompressedHQ;ni.SaveAndReimport();}
            IncludeShaders();
            Shipwright.Materials();
            var materials=new[]{Shipwright.Hull,Shipwright.Iron,Shipwright.Deck,Shipwright.Wood,Shipwright.Rope,Shipwright.Canvas,Shipwright.Black,Shipwright.Brass,Shipwright.White};
            foreach(var mat in materials) if(!AssetDatabase.Contains(mat))AssetDatabase.CreateAsset(mat,"Assets/Generated/"+mat.name+".mat");
            for(int i=0;i<2;i++) {
                var ship=Shipwright.Create(i==1);string name=i==0?"USS-Niagara":"CSS-Stonewall";
                foreach(var f in ship.GetComponentsInChildren<MeshFilter>()) {
                    if(!AssetDatabase.Contains(f.sharedMesh))AssetDatabase.CreateAsset(f.sharedMesh,"Assets/Generated/"+name+"-"+f.sharedMesh.name.Split(new[]{" - "},StringSplitOptions.None).Last()+".asset");
                }
                PrefabUtility.SaveAsPrefabAsset(ship,"Assets/Generated/"+name+".prefab");
                ExportObj(ship,"Artifacts/"+name+".obj");UnityEngine.Object.DestroyImmediate(ship);
            }
            var s=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Ironclads — scene controller").AddComponent<MaritimeScene>();
            EditorSceneManager.SaveScene(s,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Verify();RenderPreviews();
            Debug.Log("IRONCLADS: project preparation, verification and previews complete.");
        }
        static void IncludeShaders()
        {
            var settings=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").FirstOrDefault();
            if(settings==null)throw new Exception("Graphics settings asset missing.");
            var serialized=new SerializedObject(settings);var shaders=serialized.FindProperty("m_AlwaysIncludedShaders");
            foreach(string name in new[]{"Ironclads/Ocean","Ironclads/Sky","Ironclads/Atmosphere","Ironclads/Weather Sprite","Ironclads/Powder Smoke","Ironclads/Chart","Standard"}) {
                var shader=Shader.Find(name);if(shader==null)throw new Exception("Missing shader "+name);
                if(ShaderUtil.ShaderHasError(shader))throw new Exception("Shader compilation failed: "+name);
                bool found=false;for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;
                if(!found){shaders.InsertArrayElementAtIndex(shaders.arraySize);shaders.GetArrayElementAtIndex(shaders.arraySize-1).objectReferenceValue=shader;}
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        static void Check(bool test,string message){if(!test)throw new Exception("Verification failed: "+message);Debug.Log("PASS: "+message);}
        [MenuItem("Ironclads/Verify navigation and board")]
        public static void Verify()
        {
            var report=new List<string>();
            // Behavioral checks: metric scale, all six neighbors, shortest path, solar orientation, persistence.
            for(int i=0;i<6;i++) {
                Vector3 p=RuleChart.Center(RuleChart.Directions[i]);
                Check(Mathf.Abs(p.magnitude-91.44f)<.001f,"100 yards between adjacent hex centers, direction "+i);
                Check(RuleChart.At(p).Equals(RuleChart.Directions[i]),"Neighbor center resolves to its hex "+i);
                Check(RuleChart.Range(new Hex(0,0),RuleChart.Directions[i])==1,"Adjacent target range excludes firing hex "+i);
                Vector3 heading=Quaternion.Euler(0,i*60,0)*Vector3.forward;
                Check(Vector3.Dot(p.normalized,heading)>.9999f,"Facing points through hex side "+i);
            }
            Check(RuleChart.Range(new Hex(-3,4),new Hex(4,-2))==7,"Shortest hex route across mixed axes");
            Check(RuleChart.Range(new Hex(2,-2),new Hex(2,-2))==0,"Same hex range zero");
            Check(new Hex(0,0).ToString()=="2119","Local chart center label");
            var noon=Navigation.SunDirection(new DateTime(2024,3,20,12,0,0,DateTimeKind.Utc),0,0);
            Check(noon.y>.998f,"Equinox Greenwich noon sun near zenith");
            Check(Navigation.SunDirection(new DateTime(2024,3,20,6,0,0,DateTimeKind.Utc),0,0).x>.99f,"Morning sun rises in east");
            Check(Navigation.SunDirection(new DateTime(2024,3,20,18,0,0,DateTimeKind.Utc),0,0).x<-.99f,"Evening sun sets in west");
            Check(Navigation.SunDirection(new DateTime(2024,3,20,0,0,0,DateTimeKind.Utc),0,0).y<-.998f,"Midnight sun below horizon");
            foreach(var ll in new[]{new Vector2(43.43f,-8.65f),new Vector2(-33,151),new Vector2(0,179)}) {
                var actual=Navigation.GlobeLatLon(Navigation.GlobePoint(ll.x,ll.y));
                Check(Vector2.Distance(ll,actual)<.001f,"Globe round-trip geographic position "+ll);
            }
            Check(Navigation.GlobePoint(0,10).x<0,"East projects right in north-up front globe view");
            var scenario=new Scenario();var original=scenario.Date;var shipPos=scenario.ships[0].Position;
            scenario.ships[0].speed=0;scenario.Advance(180);
            Check((scenario.Date-original).TotalSeconds==180&&scenario.ships[0].Position==shipPos,"Three-minute turn with stationary ship");
            var ship=new ShipState("Check",0,0,90){speed=10};ship.Advance(180);
            Check(Mathf.Abs(ship.east-926)<.1f&&Mathf.Abs(ship.north)<.01f,"Knot-to-meter motion faces east correctly");
            var copy=JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(scenario));
            Check(copy.ships.Length==2&&copy.utc==scenario.utc&&copy.hexMode,"Scenario JSON preserves table state");
            Check(Resources.Load<Texture2D>("EarthMarch")!=null,"NASA globe texture included");
            Debug.Log("Optional private ship-sheet textures: Stonewall="+(Resources.Load<Texture2D>("StonewallSheet")!=null)+", Niagara="+(Resources.Load<Texture2D>("NiagaraSheet")!=null));
            foreach(string name in new[]{"USS-Niagara","CSS-Stonewall"}) {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Generated/"+name+".prefab");
                Check(prefab!=null&&prefab.GetComponentsInChildren<MeshFilter>().Length>=8,"Generated mesh prefab "+name);
                foreach(var f in prefab.GetComponentsInChildren<MeshFilter>())Check(f.sharedMesh.vertexCount>0,"Nonempty mesh "+f.sharedMesh.name);
            }
            File.WriteAllText("Artifacts/verification.txt","PASS: board metric scale, hex adjacency, side facings, range paths, solar orientation, globe coordinates, movement units, serialization, textures and ship prefab meshes.\nUnity "+Application.unityVersion+"\n"+DateTime.UtcNow.ToString("O"));
        }
        static void Render(Camera camera,string path)
        {
            var target=new RenderTexture(1600,1000,24,RenderTextureFormat.ARGB32);
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();
            File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(target);
        }
        [MenuItem("Ironclads/Render scene previews")]
        public static void RenderPreviews()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var controller=new GameObject("Preview controller").AddComponent<MaritimeScene>();controller.Initialize();
            controller.GetComponent<RuleChart>().Build();controller.GetComponent<RuleChart>().Snap();
            controller.UpdateWorld();controller.SetCamera();controller.GetComponent<RuleChart>().Refresh();Render(controller.view,"Artifacts/Ferrol-sea.png");
            controller.showOverlay=false;controller.Focus(0);controller.GetComponent<RuleChart>().Refresh();Render(controller.view,"Artifacts/USS-Niagara.png");
            controller.Focus(1);controller.GetComponent<RuleChart>().Refresh();Render(controller.view,"Artifacts/CSS-Stonewall.png");
            controller.ToggleGlobe();Render(controller.view,"Artifacts/World-globe.png");
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }
        [MenuItem("Ironclads/Build Windows player")]
        public static void BuildWindows()
        {
            BuildWindowsTo("Builds/Windows");
        }
        public static void BuildWindowsTo(string directory)
        {
            Directory.CreateDirectory(directory);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{ScenePath},locationPathName=directory+"/Ironclads.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Player build failed: "+report.summary.result);
            Debug.Log("IRONCLADS WINDOWS BUILD SUCCEEDED: "+report.summary.totalSize+" bytes");
        }
        static void ExportObj(GameObject ship,string path)
        {
            var b=new StringBuilder("# Ironclads procedural reconstruction. Meters, Y up, bow +Z. Approximate, see Documentation.\n");int offset=1;
            foreach(var f in ship.GetComponentsInChildren<MeshFilter>()) {
                b.Append("o ").AppendLine(f.name.Replace(' ','_'));var m=f.sharedMesh;
                foreach(var v in m.vertices){var p=f.transform.TransformPoint(v);b.AppendFormat(CultureInfo.InvariantCulture,"v {0} {1} {2}\n",p.x,p.y,p.z);}
                int[] t=m.triangles;for(int i=0;i<t.Length;i+=3)b.AppendFormat("f {0} {1} {2}\n",t[i]+offset,t[i+1]+offset,t[i+2]+offset);
                offset+=m.vertexCount;
            }
            File.WriteAllText(path,b.ToString());
        }
    }
}
