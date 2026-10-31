using System;
using UnityEngine;

namespace Ironclads
{
    // Smooth the combined peaks of overlapping guns; low-level audio passes unchanged.
    [RequireComponent(typeof(AudioListener))]
    public class SeaAudioOutput : MonoBehaviour
    {
        public static float Limit(float sample)
        {
            float magnitude=Math.Abs(sample);
            if(magnitude<=.78f)return sample;
            return Math.Sign(sample)*(.78f+.20f*(float)Math.Tanh((magnitude-.78f)/.20f));
        }
        void OnAudioFilterRead(float[] data,int channels)
        {
            for(int i=0;i<data.Length;i++)data[i]=Limit(data[i]);
        }
    }
}
