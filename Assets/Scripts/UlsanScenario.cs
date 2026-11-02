using UnityEngine;

namespace Ironclads
{
    public sealed class CruiserSpec
    {
        public string name;public float length,beam,maxSpeed;public int side,funnels,secondary,light;public bool twinTurrets;
        public CruiserSpec(string name,float length,float beam,float speed,int side,int funnels,int secondary,int light,bool twin)
        {this.name=name;this.length=length;this.beam=beam;maxSpeed=speed;this.side=side;this.funnels=funnels;this.secondary=secondary;this.light=light;twinTurrets=twin;}
    }
    public static class UlsanScenario
    {
        // 1904 configurations; small fittings and exact hull offsets are reconstructed.
        public static readonly CruiserSpec[] Ships={
            new CruiserSpec("Rossia",146.5f,20.9f,20,0,4,16,12,false),
            new CruiserSpec("Gromoboi",146.6f,20.9f,20,0,4,16,24,false),
            new CruiserSpec("Rurik",132.6f,20.4f,18,0,2,16,6,false),
            new CruiserSpec("Izumo",132.28f,20.94f,20,1,3,14,12,true),
            new CruiserSpec("Azuma",137.9f,17.74f,20,1,3,12,12,true),
            new CruiserSpec("Tokiwa",124.36f,20.48f,21,1,2,14,12,true),
            new CruiserSpec("Iwate",132.28f,20.94f,20,1,3,14,12,true)
        };
        public static Scenario Create()
        {
            var s=new Scenario{scenarioId="ulsan",rulesetId="dob",hexMode=false,latitude=35.5,longitude=130.0,utc="1904-08-13T20:20:00Z",windKnots=6,windFrom=315,cloud=0,waveHeight=.45f,visibilityKm=25,dobVisibilityPercent=90,ships=new ShipState[7]};
            Vector3 izumo=new Vector3(0,0,3900),rossia=izumo+Quaternion.Euler(0,195,0)*Vector3.forward*(8500*.9144f);
            for(int i=0;i<7;i++){Vector3 p=i<3?rossia-Vector3.right*(i*400*.9144f):izumo-Quaternion.Euler(0,125,0)*Vector3.forward*((i-3)*500*.9144f);
                s.ships[i]=new ShipState(Ships[i].name,p.x,p.z,i<3?90:125){speed=10,orderedSpeed=10,engineLoad=.55f};}
            return s;
        }
        public static bool Inside(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.z)&&Mathf.Abs(p.x)<=200000&&Mathf.Abs(p.z)<=200000;
        public static float Distance(float knots,float seconds)=>knots*1852f*seconds/3600f;
    }
}
