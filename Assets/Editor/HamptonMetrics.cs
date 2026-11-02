using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class HamptonMetrics
    {
        [MenuItem("Ironclads/Report Hampton Roads geometry counts")]
        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var scene=new GameObject("Geometry metrics").AddComponent<MaritimeScene>();scene.Initialize();
            scene.scenario=Scenario.HamptonRoads();scene.UpdateWorld();
            var output=new StringBuilder("Object,vertices,mesh_instances,triangles\n");
            Write(output,"USS Monitor",scene.vessels[0]);Write(output,"CSS Virginia",scene.vessels[1]);
            Write(output,"USS Minnesota",scene.support.minnesota);Write(output,"USS Congress wreck",scene.support.congress);Write(output,"USS Cumberland wreck",scene.support.cumberland);
            Write(output,"Shorelines forts and scenery",scene.terrain.root);
            Write(output,"Local scene total including ocean",scene.seaRoot);
            scene.GetComponent<RuleChart>().Build();
            Write(output,"Local scene including hex grid",scene.seaRoot);
            scene.ToggleGlobe();Write(output,"Separate globe view",scene.globeRoot);
            Directory.CreateDirectory("Artifacts");File.WriteAllText("Artifacts/hampton-geometry-counts.csv",output.ToString());Debug.Log(output);
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void Write(StringBuilder output,string label,Transform root)
        {
            var filters=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.gameObject.activeInHierarchy&&f.sharedMesh!=null).ToArray();
            output.Append(label).Append(',').Append(filters.Sum(f=>(long)f.sharedMesh.vertexCount)).Append(',').Append(filters.Length).Append(',').Append(filters.Sum(f=>Triangles(f.sharedMesh))).AppendLine();
        }
        static long Triangles(Mesh mesh){long result=0;for(int i=0;i<mesh.subMeshCount;i++)if(mesh.GetTopology(i)==MeshTopology.Triangles)result+=mesh.GetIndexCount(i)/3;return result;}
    }
}
