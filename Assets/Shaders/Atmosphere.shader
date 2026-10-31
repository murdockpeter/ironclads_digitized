Shader "Ironclads/Atmosphere" {
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Blend SrcAlpha One ZWrite Off Cull Front
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 view:TEXCOORD1; };
   v2f vert(float4 v:POSITION,float3 n:NORMAL) { v2f o; o.pos=UnityObjectToClipPos(v); o.normal=UnityObjectToWorldNormal(n); o.view=WorldSpaceViewDir(v); return o; }
   fixed4 frag(v2f i):SV_Target { float r=pow(saturate(1-abs(dot(normalize(i.normal),normalize(i.view)))),3); return float4(.15,.48,1,r*.45); }
  ENDCG }
 }
}
