Shader "Ironclads/Supplied Ship" {
 Properties { _MainTex("Supplied texture atlas",2D)="white" {} _NeutralizeRed("Neutralize red surface regions",Float)=0 }
 SubShader { Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 struct Input { float2 uv_MainTex; };
 sampler2D _MainTex;float _NeutralizeRed;
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 c=tex2D(_MainTex,IN.uv_MainTex).rgb;
  float red=smoothstep(.12,.28,c.r-max(c.g,c.b))*_NeutralizeRed;
  float shade=.15+.09*c.r;
  c=lerp(c,float3(shade*.94,shade,shade*1.02),red);
  o.Albedo=c;o.Metallic=.10;o.Smoothness=.18;
 }
 ENDCG
 } FallBack "Standard"
}
