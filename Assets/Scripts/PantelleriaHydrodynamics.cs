using UnityEngine;

namespace Ironclads
{
    // Display model, not a resistance or seakeeping solver. Keep constants in
    // PantelleriaSea.cginc synchronized with these deep-water wave components.
    public static class PantelleriaHydrodynamics
    {
        public static Vector4 Sample(Vector3 p,float time,float scale,float windFrom)
        {
            Vector3 d=Quaternion.Euler(0,windFrom+180,0)*Vector3.forward;
            var a=new Vector2(d.x,d.z);var b=new Vector2(a.x*.819152f-a.y*.573576f,a.x*.573576f+a.y*.819152f);
            var c=new Vector2(a.x*.906308f+a.y*.422618f,-a.x*.422618f+a.y*.906308f);
            Vector4 sample=Vector4.zero;
            Add(ref sample,p,time,a,.14f,.32f,0);Add(ref sample,p,time,b,.29f,.14f,1.3f);Add(ref sample,p,time,c,.62f,.04f,2.4f);
            return sample*Mathf.Max(0,scale);
        }
        static void Add(ref Vector4 result,Vector3 p,float time,Vector2 d,float k,float amplitude,float phase)
        {
            float omega=Mathf.Sqrt(9.81f*k),q=(p.x*d.x+p.z*d.y)*k-time*omega+phase,c=amplitude*Mathf.Cos(q);
            result+=new Vector4(amplitude*Mathf.Sin(q),c*k*d.x,c*k*d.y,-c*omega);
        }
        // x = heave, y/z = pitch/roll degrees, w = heave velocity.
        public static Vector4 Pose(ShipState ship,BattleshipClass model,float time,float scale,float windFrom)
        {
            var rotation=Quaternion.Euler(0,ship.heading,0);Vector3 forward=rotation*Vector3.forward,right=rotation*Vector3.right;
            var bow=Sample(ship.Position+forward*model.length*.32f,time,scale,windFrom);
            var stern=Sample(ship.Position-forward*model.length*.32f,time,scale,windFrom);
            var port=Sample(ship.Position-right*model.beam*.34f,time,scale,windFrom);
            var starboard=Sample(ship.Position+right*model.beam*.34f,time,scale,windFrom);
            return new Vector4((bow.x+stern.x+port.x+starboard.x)*.1625f,
                Mathf.Clamp(Mathf.Atan2(stern.x-bow.x,model.length*.64f)*Mathf.Rad2Deg*.65f,-2,2),
                Mathf.Clamp(Mathf.Atan2(starboard.x-port.x,model.beam*.68f)*Mathf.Rad2Deg*.55f,-3.5f,3.5f),
                (bow.w+stern.w+port.w+starboard.w)*.1625f);
        }
        public static float Froude(float knots,float length)=>Mathf.Max(0,knots)*.514444f/Mathf.Sqrt(9.81f*Mathf.Max(length,1));
        public static float WakeLength(float knots){float speed=Mathf.Max(0,knots)*.514444f;return 2*Mathf.PI*speed*speed/9.81f;}
        public static float Impact(float crestAboveHull,float risingSpeed,float scale)=>Mathf.Clamp01(crestAboveHull*.65f)*Mathf.Clamp(risingSpeed,0,2)*Mathf.Clamp01(scale);
        public static float HalfWidth(float length,float beam,float z)=>beam*.5f*(.035f+.965f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(Mathf.Clamp01(z/length+.5f)*Mathf.PI)),.55f));
    }
}
