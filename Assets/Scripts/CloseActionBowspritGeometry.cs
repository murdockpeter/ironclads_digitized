using UnityEngine;
namespace Ironclads
{
    public static partial class Shipwright
    {
        // One rigid frame supplies the spars and the fore-rigging; deck sheer is sampled only at the heel.
        static void SailingBowspritFrame(CloseActionSpec spec,out Vector3 heel,out Vector3 end,out Vector3 boomHeel,out Vector3 boomEnd)
        {
            float z=spec.Length*.43f;
            heel=new Vector3(0,CloseActionHullLines.Get(spec.Length,spec.Beam).DeckHeight(z)+1.15f,z);
            Vector3 span=new Vector3(0,5,spec.Length*.29f),above=Vector3.Cross(span.normalized,Vector3.right).normalized;
            end=heel+span;
            // The jibboom overlaps above the bowsprit, with a parallel axis rather than a kinked extension.
            boomHeel=heel+span*.87f+above*.29f;boomEnd=heel+span*1.45f+above*.29f;
        }
        static void SailingBowsprit(Transform root,CloseActionSpec spec)
        {
            SailingBowspritFrame(spec,out var heel,out var end,out var boomHeel,out var boomEnd);
            var rig=new GameObject("Rigid bowsprit assembly").transform;rig.SetParent(root,false);
            SailingRod(rig,"Bowsprit",heel,end,.42f,Wood,.12f,sides:14);
            SailingRod(rig,"Jibboom",boomHeel,boomEnd,.13f,Wood,.045f,sides:10);
            Vector3 axis=(end-heel).normalized,above=Vector3.Cross(axis,Vector3.right).normalized;
            var cap=Box(rig,"Bowsprit cap",end+above*.12f,new Vector3(.48f,.76f,.24f),Wood);cap.transform.localRotation=Quaternion.LookRotation(axis,above);
            Vector3 strikerHead=end-above*.3f,strikerFoot=strikerHead+Vector3.down*3.7f;
            SailingRod(rig,"Dolphin striker",strikerHead,strikerFoot,.09f,Wood,sides:8);
            for(int side=-1;side<=1;side+=2)SailingRod(rig,"Bowsprit bobstay",new Vector3(side*.65f,1.5f,spec.Length*.47f),end-above*.12f+Vector3.right*side*.10f,.032f,Rope,sides:6);
            SailingRod(rig,"Jibboom martingale",strikerFoot,boomEnd,.025f,Rope,sides:6);
        }
        static void SailingHeadsails(Transform mast,CloseActionSpec spec,float mastZ,float deck,float height)
        {
            SailingBowspritFrame(spec,out var heel,out var end,out var boomHeel,out var boomEnd);
            // Separate luffs and staggered clews leave a visible fan of modest-sized headsails.
            Vector3 lowerHead=new Vector3(0,deck+height*.49f,mastZ),upperHead=new Vector3(0,deck+height*.76f,mastZ);
            SailingHeadsail(mast,"Fore staysail",lowerHead,Vector3.Lerp(heel,end,.15f),new Vector3(0,deck+2.3f,spec.Length*.33f),.12f,1);
            SailingHeadsail(mast,"Fore-topmast staysail",upperHead,Vector3.Lerp(heel,end,.84f),new Vector3(0,deck+height*.13f,spec.Length*.48f),.20f,2);
            SailingHeadsail(mast,"Jib",upperHead,Vector3.Lerp(boomHeel,boomEnd,.92f),new Vector3(0,deck+height*.18f,spec.Length*.65f),.42f,2);
        }
        static void SailingHeadsail(Transform mast,string name,Vector3 stayHead,Vector3 tack,Vector3 clew,float headFraction,int minimumSail)
        {
            SailingRod(mast,name+" stay",stayHead,tack,.026f,Rope,sides:6);
            SailingTriangle(mast,Vector3.Lerp(stayHead,tack,headFraction),tack,clew,minimumSail,name);
        }
    }
}
