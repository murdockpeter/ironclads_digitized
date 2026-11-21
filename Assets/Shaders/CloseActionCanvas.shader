Shader "Ironclads/Close Action Canvas"
{
    Properties {
        _MainTex ("Canvas UV",2D)="white" {}
        _Color ("Weathered canvas",Color)=(.84,.79,.66,1)
        _CanvasWind ("Pressure / speed / gust phase / square",Vector)=(0,0,0,1)
        _CanvasU ("Cloth U span",Vector)=(10,0,0,0)
        _CanvasV ("Cloth V span",Vector)=(0,-8,0,0)
        _CanvasTime ("Preview time (-1 uses game clock)",Float)=-1
    }
    SubShader {
        Tags {"RenderType"="Opaque"} LOD 200 Cull Off
        CGPROGRAM
        #pragma surface surf Canvas vertex:vert addshadow fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;float4 _CanvasWind,_CanvasU,_CanvasV;float _CanvasTime;
        struct Input {float2 uv_MainTex;};
        half4 LightingCanvas(SurfaceOutput s,half3 lightDir,half atten)
        {
            half incidence=dot(s.Normal,lightDir);
            half diffuse=max(incidence,0)+.65*max(-incidence,0);
            return half4(s.Albedo*_LightColor0.rgb*(.18+.82*diffuse)*atten,s.Alpha);
        }
        float gustNoise(float t)
        {
            float cell=floor(t),f=frac(t);f=f*f*(3-2*f);
            float a=frac(sin(cell*127.1+311.7)*43758.5453);
            float b=frac(sin((cell+1)*127.1+311.7)*43758.5453);
            return lerp(a,b,f)*2-1;
        }
        void vert(inout appdata_full v)
        {
            float u=v.texcoord.x,w=v.texcoord.y;
            float square=_CanvasWind.w;
            float envelope=square>.5?sin(u*UNITY_PI)*sin(w*UNITY_PI):27*u*w*max(0,1-u-w);
            float du=square>.5?UNITY_PI*cos(u*UNITY_PI)*sin(w*UNITY_PI):27*w*(1-2*u-w);
            float dv=square>.5?UNITY_PI*sin(u*UNITY_PI)*cos(w*UNITY_PI):27*u*(1-u-2*w);
            float phase=_CanvasWind.z;
            float clock=_CanvasTime>=0?_CanvasTime:_Time.y;
            float time=clock*(.65+.025*_CanvasWind.y);
            // Smooth random gusts swell the canvas slowly; smaller traveling folds move across its belly.
            float gust=1+.17*gustNoise(time*.34+phase)+.07*gustNoise(time*.79+phase*1.73);
            float waveA=u*7.5+w*4.2-time*1.7+phase;
            float waveB=u*13.2-w*8.1+time*2.3+phase*1.39;
            float ripple=.065*sin(waveA)+.025*sin(waveB);
            float fullness=gust+ripple;
            float slopeU=.065*7.5*cos(waveA)+.025*13.2*cos(waveB);
            float slopeV=.065*4.2*cos(waveA)-.025*8.1*cos(waveB);
            float pressure=_CanvasWind.x;
            float3 normal=normalize(cross(_CanvasU.xyz,_CanvasV.xyz));
            v.vertex.xyz+=normal*(envelope*pressure*fullness);
            v.normal=normalize(cross(_CanvasU.xyz+normal*pressure*(du*fullness+envelope*slopeU),_CanvasV.xyz+normal*pressure*(dv*fullness+envelope*slopeV)));
        }
        void surf(Input i,inout SurfaceOutput o)
        {
            float stitch=pow(abs(cos(i.uv_MainTex.x*UNITY_PI*18)),36);
            float weave=.008*sin(i.uv_MainTex.x*960)*sin(i.uv_MainTex.y*720);
            o.Albedo=_Color.rgb*(.98+weave-stitch*.065);o.Emission=o.Albedo*.045;o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
