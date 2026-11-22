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
            var seating=new GameObject("Bowsprit seating and gammoning").transform;seating.SetParent(root,false);
            var lines=CloseActionHullLines.Get(spec.Length,spec.Beam);
            for(int side=-1;side<=1;side+=2){
                Vector3 foot=new Vector3(side*.58f,lines.DeckHeight(heel.z)+1.05f,heel.z+.25f);
                Box(seating,"Bowsprit knighthead",foot+Vector3.up*.55f,new Vector3(.30f,1.25f,.36f),caOak);
                Box(seating,"Knighthead cap",foot+Vector3.up*1.22f,new Vector3(.38f,.14f,.44f),Hull);
            }
            float lashingZ=HeadStationZ(spec,.54f),kneeT=HeadKneeAtZ(spec,lashingZ);
            Vector3 knee=(HeadKneePoint(spec,kneeT)+HeadKneeInside(spec,kneeT))*.5f;
            Vector3 spar=Vector3.LerpUnclamped(heel,end,(lashingZ-heel.z)/(end.z-heel.z));
            for(int turn=0;turn<8;turn++){
                float offset=(turn-3.5f)*.085f;Vector3 shift=axis*offset;
                Vector3[] loop={spar+above*.40f+shift,spar+Vector3.right*.46f+shift,knee+Vector3.right*.32f+shift,knee-Vector3.up*.10f+shift,knee-Vector3.right*.32f+shift,spar-Vector3.right*.46f+shift};
                for(int i=0;i<loop.Length;i++)SailingRod(seating,"Gammoning rope turn",loop[i],loop[(i+1)%loop.Length],.038f,Rope,sides:6);
            }
            // Short frapping turns bind the two sides midway down the lashing.
            Vector3 bind=Vector3.Lerp(knee,spar,.65f);
            for(int n=0;n<4;n++){
                Vector3 p=bind+Vector3.up*(n*.08f);
                SailingRod(seating,"Gammoning frapping",p+Vector3.left*.40f-axis*.37f,p+Vector3.right*.40f-axis*.37f,.035f,Rope,sides:6);
                SailingRod(seating,"Gammoning frapping",p+Vector3.left*.40f+axis*.37f,p+Vector3.right*.40f+axis*.37f,.035f,Rope,sides:6);
            }
            Combine(seating,3);
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
