Shader "Ironclads/Powder Smoke" {
 Properties { _MainTex("Smoke density",2D)="white" {} }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 sampler2D _MainTex; UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
 float3 _SunDirection; float4 _SunColor;float4 _SkyTint;
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;float4 screen:TEXCOORD1;UNITY_FOG_COORDS(2)};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.screen=ComputeScreenPos(o.pos);COMPUTE_EYEDEPTH(o.screen.z);o.uv=v.uv;o.color=v.color;UNITY_TRANSFER_FOG(o,o.pos);return o;}
 float4 frag(v2f i):SV_Target {
  float2 tile=min(floor(i.uv*2),1)*.5;float2 uv=frac(i.uv*2);
  float2 curl=.023*float2(sin(uv.y*11+_Time.y*.61),cos(uv.x*13-_Time.y*.49));
  float4 density=tex2D(_MainTex,tile+clamp(uv+curl,.004,.996)*.5);float2 xy=uv*2-1;float z=sqrt(saturate(1-dot(xy,xy)));
  float3 normal=normalize(mul((float3x3)unity_CameraToWorld,float3(xy,-z)));
  float lighting=.58+.42*saturate(dot(normal,_SunDirection));
  float3 illumination=clamp(_SkyTint.rgb*.7+_SunColor.rgb*lighting,.10,1.2);
  float sceneZ=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.screen)));
  float fade=saturate((sceneZ-i.screen.z)*.7);
  float4 c=float4(i.color.rgb*density.rgb*illumination,density.a*i.color.a*fade);UNITY_APPLY_FOG(i.fogCoord,c);return c;
 }
 ENDCG }
 }
}
