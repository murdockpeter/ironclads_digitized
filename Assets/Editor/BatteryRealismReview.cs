using System;
using System.IO;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class BatteryRealismReview
    {
        [Serializable] public class Levels { public float peak,bodyRms,tailRms; }
        public static Levels Measure(AudioClip clip)
        {
            var data=new float[clip.samples];clip.GetData(data,0);float peak=0;double body=0,tail=0;int bodyCount=0,tailCount=0;
            for(int i=0;i<data.Length;i++) {
                float t=i/(float)clip.frequency;peak=Mathf.Max(peak,Mathf.Abs(data[i]));
                if(t<.65f){body+=data[i]*data[i];bodyCount++;}if(t>=1&&t<3){tail+=data[i]*data[i];tailCount++;}
            }
            return new Levels{peak=peak,bodyRms=(float)Math.Sqrt(body/bodyCount),tailRms=(float)Math.Sqrt(tail/tailCount)};
        }
        public static void CaptureBaseline()
        {
            const string path="Artifacts/SalvoRealism/Baseline";
            if(Directory.Exists(path))throw new Exception("Refusing to overwrite original cannon baseline.");Directory.CreateDirectory(path);
            for(int kind=0;kind<3;kind++){var clip=BatteryEffects.MakeReport(kind);File.WriteAllText(path+"/report-"+kind+".json",JsonUtility.ToJson(Measure(clip),true));UnityEngine.Object.DestroyImmediate(clip);}
            File.Copy("Assets/Scripts/BatteryEffects.cs",path+"/BatteryEffects.cs.txt");File.Copy("Assets/Shaders/PowderSmoke.shader",path+"/PowderSmoke.shader.txt");
        }
        public static void Verify()
        {
            var text=new System.Text.StringBuilder("Cannon body/tail RMS levels (linear amplitude, before distance attenuation)\n");
            for(int kind=0;kind<3;kind++) {
                var before=JsonUtility.FromJson<Levels>(File.ReadAllText("Artifacts/SalvoRealism/Baseline/report-"+kind+".json"));var clip=BatteryEffects.MakeReport(kind);var after=Measure(clip);
                text.AppendLine($"Gun {kind}: body {before.bodyRms:F4} -> {after.bodyRms:F4}; tail {before.tailRms:F4} -> {after.tailRms:F4}; peak {after.peak:F4}");
                if(after.peak>.96f||after.bodyRms<before.bodyRms*1.1f||after.tailRms<before.tailRms*1.15f)throw new Exception("Cannon body/tail impact or headroom failed for "+kind+": "+JsonUtility.ToJson(after));
                WriteWav(clip,"Artifacts/SalvoRealism/Cannon-"+kind+".wav");UnityEngine.Object.DestroyImmediate(clip);
            }
            File.WriteAllText("Artifacts/SalvoRealism/audio-levels.txt",text.ToString());Debug.Log(text.ToString());
        }
        static void WriteWav(AudioClip clip,string path)
        {
            var data=new float[clip.samples];clip.GetData(data,0);using(var stream=new BinaryWriter(File.Create(path))) {
                stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));stream.Write(36+data.Length*2);stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));stream.Write(16);stream.Write((short)1);stream.Write((short)1);stream.Write(clip.frequency);stream.Write(clip.frequency*2);stream.Write((short)2);stream.Write((short)16);stream.Write(System.Text.Encoding.ASCII.GetBytes("data"));stream.Write(data.Length*2);foreach(float sample in data)stream.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample,-1,1)*32767));
            }
        }
    }
}
