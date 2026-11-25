Shader "Ironclads/Naval Cloth"
{
 Properties { _MainTex("National pattern",2D)="white" {} }
 SubShader {
  Tags {"RenderType"="Opaque"} Cull Off
  Pass {
   Tags {"LightMode"="ForwardBase"}
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #pragma multi_compile_fog
   #include "UnityCG.cginc"
   #include "Lighting.cginc"
   sampler2D _MainTex;
   float _ClothTime,_ClothWind,_Phase,_ClothScale;
   float3 _ClothGravity;
   struct input {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
   struct output {float4 position:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;UNITY_FOG_COORDS(2)};
   output vert(input v) {
    output o;float u=v.uv.x,speed=saturate(_ClothWind/16),spread=lerp(.16,1,smoothstep(0,12,_ClothWind));
    float t=_ClothTime*(1.5+speed*3)+_Phase;
    float envelope=u*u;
    float flutter=(sin(u*11-t)+.32*sin(u*23-t*1.7+v.uv.y*3))*envelope;
    v.vertex.x*=spread;
    v.vertex.xyz+=_ClothGravity*((1-spread)*u*u*3*_ClothScale);
    v.vertex.y+=sin(u*8-t*.8)*envelope*(.025+.12*speed)*_ClothScale;
    v.vertex.z+=flutter*(.035+.42*speed)*_ClothScale;
    o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.position=UnityWorldToClipPos(o.world);o.uv=v.uv;UNITY_TRANSFER_FOG(o,o.position);return o;
   }
   fixed4 frag(output i):SV_Target {
    float3 normal=normalize(cross(ddy(i.world),ddx(i.world)));
    float lighting=.62+.38*abs(dot(normal,normalize(_WorldSpaceLightPos0.xyz)));
    fixed4 colour=tex2D(_MainTex,i.uv);colour.rgb*=lighting;
    UNITY_APPLY_FOG(i.fogCoord,colour);return colour;
   }
   ENDCG
  }
 }
}
