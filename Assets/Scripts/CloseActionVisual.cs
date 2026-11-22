using UnityEngine;
namespace Ironclads
{
    public sealed class CloseActionVisual : MonoBehaviour
    {
        CloseActionCanvas[] cloth;CloseActionYard[] yards;CloseActionMast[] masts;
        public void Refresh(CloseActionLog log,float heading,float windFrom,float windKnots=12)
        {
            if(cloth==null){
                cloth=GetComponentsInChildren<CloseActionCanvas>(true);yards=GetComponentsInChildren<CloseActionYard>(true);masts=GetComponentsInChildren<CloseActionMast>(true);
                // Hoisted yards stand forward of the mast, keeping canvas clear of its axis.
                foreach(var yard in yards)yard.transform.localPosition+=Vector3.forward*.65f;
            }
            foreach(var mast in masts)mast.gameObject.SetActive(log.RigSections>0);
            foreach(var sail in cloth)sail.gameObject.SetActive(log.MovementSail>=sail.minimumSail&&log.RigSections>0);
            float relative=Mathf.DeltaAngle(heading,windFrom+180);
            if(relative>90)relative-=180;else if(relative< -90)relative+=180;
            float trim=Mathf.Clamp(relative,-60,60);
            foreach(var yard in yards)yard.transform.localRotation=Quaternion.Euler(0,trim,0);
            Vector3 wind=Quaternion.Euler(0,windFrom+180,0)*Vector3.forward;
            foreach(var sail in cloth)if(sail.gameObject.activeInHierarchy)sail.SetWind(wind,windKnots*.514444f);
        }
    }
}
