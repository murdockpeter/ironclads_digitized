using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionDetailReview
    {
        static void Check(bool okay,string message){if(!okay)throw new Exception("Sailing detail review: "+message);Debug.Log("CA DETAIL PASS: "+message);}
        public static void Run()
        {
            const string output="Artifacts/CloseAction/Detail";Directory.CreateDirectory(output);
            var profiles=CloseActionPrototypes.All;
            Check(profiles.Count(p=>p.side==0)==4&&profiles.Count(p=>p.side==1)==6,"four British and six French prototypes");
            var counts=new StringBuilder("type,before_vertices,after_vertices,vertex_ratio,before_meshes,after_meshes,mesh_ratio,guns\n");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var scene=new GameObject("Sailing geometry review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartCloseAction();scene.showOverlay=false;scene.scenario.waveHeight=0;scene.UpdateWorld();
            for(int entry=0;entry<profiles.Length;entry++){
                int index=profiles[entry].representative;var spec=CloseActionScenario.Spec(scene.scenario,index);var root=scene.vessels[index];string name=Shipwright.CloseActionModelKey(spec).Split('/').Last();
                foreach(var vessel in scene.vessels)vessel.gameObject.SetActive(vessel==root);
                var filters=root.GetComponentsInChildren<MeshFilter>(true);long vertices=filters.Sum(f=>(long)f.sharedMesh.vertexCount);int oldVertices=spec.side==0?(spec.rate==50?216935:275561):spec.rate==120?388639:spec.rate==80?290623:275449,oldMeshes=spec.side==0?285:281;float vertexRatio=vertices/(float)oldVertices,meshRatio=filters.Length/(float)oldMeshes;
                counts.AppendLine($"{name},{oldVertices},{vertices},{vertexRatio:F3},{oldMeshes},{filters.Length},{meshRatio:F3},{root.GetComponentsInChildren<GunMount>(true).Length}");
                File.WriteAllText(output+"/comparison.csv",counts.ToString());
                Check(vertexRatio>=2&&vertexRatio<=2.8f,name+" doubled vertices / ratio "+vertexRatio);Check(meshRatio>=2&&meshRatio<=2.8f,name+" doubled mesh detail / ratio "+meshRatio);
                foreach(int end in new[]{-1,1})foreach(bool lower in new[]{true,false}){
                    var ray=new Ray(new Vector3(.037f,lower?-.5f:spec.Deck*.5f,end*spec.Length*.6f),Vector3.back*end);
                    Check(HullHit(root,ray,spec.Length*.22f,lower),name+(end<0?" stern":" bow")+(lower?" underbody":" topside")+" closed");
                }
                var log=scene.scenario.closeAction.logs[index];log.sail=3;root.GetComponent<CloseActionVisual>().Refresh(log,0,270,19);
                var canvas=root.GetComponentsInChildren<CloseActionCanvas>().First(c=>c.square);var properties=new MaterialPropertyBlock();
                canvas.SetWind(Vector3.forward,0);canvas.GetComponent<Renderer>().GetPropertyBlock(properties);Check(properties.GetVector("_CanvasWind").x==0,name+" calm canvas has no wind pressure");
                canvas.SetWind(Vector3.forward,3);canvas.GetComponent<Renderer>().GetPropertyBlock(properties);float light=properties.GetVector("_CanvasWind").x;
                canvas.SetWind(Vector3.forward,9);canvas.GetComponent<Renderer>().GetPropertyBlock(properties);float strong=properties.GetVector("_CanvasWind").x;Check(Mathf.Abs(strong)>Mathf.Abs(light),name+" wind strength increases fullness");
                canvas.SetWind(Vector3.back,9);canvas.GetComponent<Renderer>().GetPropertyBlock(properties);Check(strong*properties.GetVector("_CanvasWind").x<0,name+" reversed wind reverses canvas pressure");
                root.GetComponent<CloseActionVisual>().Refresh(log,scene.scenario.ships[index].heading,300,12);
                var text=root.Find("Individual stern name");Check(text.GetComponent<MeshRenderer>().sharedMaterial.shader.name=="Ironclads/Stern Lettering"&&Vector3.Dot(text.localRotation*Vector3.back,Vector3.back)>.7f,name+" outward depth-tested name");
                scene.target=scene.scenario.ships[index].Position+Vector3.up*15;scene.pitch=17;scene.distance=spec.Length*2.15f;
                foreach(int yaw in new[]{0,90,180,270}){scene.yaw=yaw;scene.SetCamera();Capture(scene.view,output+"/"+name+"-"+yaw+".png");}
                if(entry==0){
                    ReviewCanvasMotion(scene.view,canvas,output);
                    foreach(int knots in new[]{0,7,19}){root.GetComponent<CloseActionVisual>().Refresh(log,0,300,knots);scene.yaw=125;scene.SetCamera();Capture(scene.view,output+"/Canvas-"+knots+"kn.png");}
                    root.GetComponent<CloseActionVisual>().Refresh(log,0,120,19);Capture(scene.view,output+"/Canvas-reverse-wind.png");
                }
            }
            File.WriteAllText(output+"/verification.txt","PASS: all ten prototypes exceed 2x baseline vertices and meshes; bow/stern lower and upper closure rays; wind strength and direction response; calm pressure; outward depth-tested stern lettering; four-angle rendered review.\n");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void ReviewCanvasMotion(Camera camera,CloseActionCanvas canvas,string output)
        {
            var cloth=canvas.GetComponent<Renderer>();var original=new MaterialPropertyBlock();cloth.GetPropertyBlock(original);
            int layer=canvas.gameObject.layer,mask=camera.cullingMask;var position=camera.transform.position;var rotation=camera.transform.rotation;
            bool ortho=camera.orthographic;float size=camera.orthographicSize;var flags=camera.clearFlags;var background=camera.backgroundColor;
            canvas.SetWind(canvas.transform.TransformDirection(Vector3.Cross(canvas.surfaceU,canvas.surfaceV).normalized),9);
            var block=new MaterialPropertyBlock();cloth.GetPropertyBlock(block);canvas.gameObject.layer=31;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.2f,.27f);
            camera.orthographic=true;camera.orthographicSize=Mathf.Max(canvas.width,canvas.drop)*.65f;
            Vector3 center=cloth.bounds.center,normal=canvas.transform.TransformDirection(Vector3.Cross(canvas.surfaceU,canvas.surfaceV).normalized);
            camera.transform.position=center+normal*30;camera.transform.LookAt(center,canvas.transform.up);
            Color32[] previous=null;
            foreach(float time in new[]{0f,2.8f,6.3f}){
                block.SetFloat("_CanvasTime",time);cloth.SetPropertyBlock(block);string path=output+"/Canvas-motion-"+time.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+".png";Capture(camera,path);
                var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(path));var pixels=image.GetPixels32();UnityEngine.Object.DestroyImmediate(image);
                if(previous!=null){int changed=0;for(int i=0;i<pixels.Length;i++)if(Mathf.Abs(pixels[i].r-previous[i].r)+Mathf.Abs(pixels[i].g-previous[i].g)+Mathf.Abs(pixels[i].b-previous[i].b)>12)changed++;Check(changed>pixels.Length*.005f,"isolated canvas visibly animates between preview times");}
                previous=pixels;
            }
            cloth.SetPropertyBlock(original);canvas.gameObject.layer=layer;camera.cullingMask=mask;camera.orthographic=ortho;camera.orthographicSize=size;camera.clearFlags=flags;camera.backgroundColor=background;camera.transform.SetPositionAndRotation(position,rotation);
        }
        static bool HullHit(Transform root,Ray ray,float maximum,bool lower)
        {
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true)){
                var material=filter.GetComponent<MeshRenderer>().sharedMaterial;if(material==null)continue;
                if(lower?!material.name.Contains("copper"):!material.name.Contains("oak"))continue;
                var vertices=filter.sharedMesh.vertices;var indices=filter.sharedMesh.triangles;var matrix=root.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                for(int i=0;i<indices.Length;i+=3){
                    Vector3 a=matrix.MultiplyPoint3x4(vertices[indices[i]]),b=matrix.MultiplyPoint3x4(vertices[indices[i+1]]),c=matrix.MultiplyPoint3x4(vertices[indices[i+2]]);
                    Vector3 edge=b-a,other=c-a,p=Vector3.Cross(ray.direction,other);float det=Vector3.Dot(edge,p);if(Mathf.Abs(det)<.000001f)continue;
                    float inverse=1/det;Vector3 delta=ray.origin-a;float u=Vector3.Dot(delta,p)*inverse;if(u<-.0001f||u>1.0001f)continue;
                    Vector3 q=Vector3.Cross(delta,edge);float v=Vector3.Dot(ray.direction,q)*inverse;if(v<-.0001f||u+v>1.0001f)continue;
                    float distance=Vector3.Dot(other,q)*inverse;if(distance>=0&&distance<=maximum)return true;
                }
            }
            return false;
        }
        static void Capture(Camera camera,string path)
        {
            var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
