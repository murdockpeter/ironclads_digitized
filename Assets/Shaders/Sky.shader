Shader "Ironclads/Sky"
{
 Properties { _Cloud("Cloud",Range(0,1))=.3 }
 SubShader {
  Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
  Cull Off ZWrite Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float _Cloud, _SeaTime;
   float4 _SunDirection, _SkyTint, _SunColor, _Wind;
   struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
   v2f vert(float4 v:POSITION) { v2f o; o.pos=UnityObjectToClipPos(v); o.dir=v.xyz; return o; }
   float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
   float noise(float2 p) {
    float2 f=frac(p); f=f*f*(3-2*f); float2 i=floor(p);
    return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
   }
   float fbm(float2 p) { return noise(p)*.55+noise(p*2.03)*.27+noise(p*4.01)*.13+noise(p*8.1)*.05; }
   fixed4 frag(v2f i):SV_Target {
    float3 d=normalize(i.dir); float daylight=smoothstep(-.12,.15,_SunDirection.y);
    float3 horizon=_SkyTint.rgb;
    float3 zenith=lerp(float3(.004,.009,.024),float3(.10,.28,.48),daylight);
    float3 col=lerp(horizon,zenith,pow(saturate(d.y),.4));
    float sun=dot(d,normalize(_SunDirection.xyz));
    col+=_SunColor.rgb*(pow(saturate(sun),24)*.13+smoothstep(.9997,.99988,sun)*3);
    if (d.y>.005) {
     float2 uv=d.xz/(d.y+.18)*2.1+_Wind.xy*_SeaTime*.003;
     float cloud=smoothstep(1-_Cloud*.95,1-_Cloud*.95+.14,fbm(uv));
     float3 cc=lerp(float3(.012,.02,.033),float3(.82,.85,.83),daylight)*(0.82+.18*fbm(uv*3));
     col=lerp(col,cc,cloud*smoothstep(0,.15,d.y));
    }
    return float4(col,1);
   }
   ENDCG
  }
 }
}
