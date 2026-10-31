using UnityEngine;

namespace Ironclads
{
    // Retained through mesh combining, so effects originate at each modeled muzzle.
    public class GunMount : MonoBehaviour
    {
        public Shipwright.GunType type;
        public Vector3 Muzzle=>transform.TransformPoint(Vector3.forward*(Shipwright.BarrelLength(type)*.45f+.08f));
        public Vector3 Direction=>transform.forward;
        public int Side=>type==Shipwright.GunType.Armstrong300?0:transform.localPosition.x<0?-1:1;
    }
}
