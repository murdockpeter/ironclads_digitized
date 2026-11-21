using UnityEngine;
namespace Ironclads
{
    public sealed class CloseActionCanvas : MonoBehaviour
    {
        public int minimumSail=1;
        public float width=10,drop=8;
        public bool square=true;
        public Vector3 surfaceU=Vector3.right,surfaceV=Vector3.down;
        Renderer cloth;MaterialPropertyBlock properties;
        float gustPhase;
        public void SetWind(Vector3 direction,float metersPerSecond)
        {
            if(cloth==null){
                cloth=GetComponent<Renderer>();properties=new MaterialPropertyBlock();
                // Keep the phase stable while a ship moves; neighboring sails and ships breathe independently.
                Vector3 seed=transform.position;
                gustPhase=Mathf.Repeat(Mathf.Sin(Vector3.Dot(seed,new Vector3(12.9898f,78.233f,37.719f))+width*3.17f+drop)*43758.5453f,1)*100;
                // Vertex-shader billow extends outside the original flat mesh, including triangular sails.
                var filter=GetComponent<MeshFilter>();var bounds=filter.sharedMesh.bounds;bounds.Expand(10);
                cloth.localBounds=bounds;
            }
            Vector3 normal=Vector3.Cross(surfaceU,surfaceV).normalized;
            float incidence=Vector3.Dot(transform.TransformDirection(normal),direction);
            float pressure=Mathf.Sign(incidence)*Mathf.Sqrt(Mathf.Abs(incidence))*Mathf.Clamp01(metersPerSecond/10);
            float fullness=Mathf.Clamp(Mathf.Min(width,drop)*.29f,.3f,3.4f)*pressure;
            properties.SetVector("_CanvasWind",new Vector4(fullness,metersPerSecond,gustPhase,square?1:0));
            properties.SetVector("_CanvasU",surfaceU);properties.SetVector("_CanvasV",surfaceV);
            cloth.SetPropertyBlock(properties);
        }
    }
}
