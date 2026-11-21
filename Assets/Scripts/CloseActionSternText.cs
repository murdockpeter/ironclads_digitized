using UnityEngine;
namespace Ironclads
{
    // The default TextMesh shader draws through the ship and shows mirrored backs.
    public sealed class CloseActionSternText : MonoBehaviour
    {
        static Material lettering;
        TextMesh label;
        public void Refresh()
        {
            if(label==null)label=GetComponent<TextMesh>();if(label.font==null)return;
            if(lettering==null)lettering=new Material(Shader.Find("Ironclads/Stern Lettering")){name="Occluded outward stern lettering"};
            lettering.mainTexture=label.font.material.mainTexture;label.GetComponent<MeshRenderer>().sharedMaterial=lettering;
        }
        void LateUpdate()=>Refresh();
    }
}
