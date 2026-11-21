using System;
using System.IO;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class PowderSmokeReview
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception("Powder smoke: "+message);Debug.Log("POWDER PASS: "+message);}
        static Vector3 Track(Vector3 wind,float duration,float step)
        {
            Vector3 p=Vector3.up*3,v=Vector3.right*40;
            for(float t=0;t<duration-step*.5f;t+=step){v=BatteryEffects.TransportVelocity(v,wind,t,step,17,t,p);p+=v*step;}
            return p;
        }
        public static void Run()
        {
            Vector3 burst=Track(Vector3.zero,1,1f/60),calm=Track(Vector3.zero,120,1f/60),wind=Vector3.forward*5;
            Check(burst.x>30,"muzzle jet retains outward throw during first second");
            Check(calm.y>8&&calm.y<45,"calm cloud rises slowly over two minutes");
            Vector3 fine=Track(wind,60,1f/60),coarse=Track(wind,60,1f/30);
            Check(fine.z>240&&fine.z<360,"aged smoke travels with ambient wind");
            Check(Vector3.Distance(fine,coarse)<4,"transport remains stable across frame rates");
            Check(BatteryEffects.ReportRolloff().Evaluate(.16f)>.6f,"report carries beyond the immediate ship");
            for(int kind=0;kind<3;kind++){
                var clip=BatteryEffects.MakeReport(kind);var levels=BatteryRealismReview.Measure(clip);
                Check(clip.length>=8.9f&&levels.peak<=.96f,"long report tail retains audio headroom / gun "+kind);
                UnityEngine.Object.DestroyImmediate(clip);
            }
            Directory.CreateDirectory("Artifacts/CloseAction/Smoke");File.WriteAllText("Artifacts/CloseAction/Smoke/transport.txt","PASS: outward jet; slow calm rise; ambient wind transport; 30/60 fps stability; report distance and audio headroom.\n");
        }
    }
}
