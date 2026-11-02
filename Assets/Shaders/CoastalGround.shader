Shader "Ironclads/Coastal Ground" {
 Properties { _Color("Tint",Color)=(1,1,1,1) _MainTex("Detail",2D)="white" {} _Beach("Beach profile",Float)=0 }
 SubShader { Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 struct Input { float3 worldPos;float4 color:COLOR; };
 fixed4 _Color;float _Beach;
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float2 p=IN.worldPos.xz;float patch=noise(p*.0018)*.55+noise(p*.009)*.3+noise(p*.045)*.15;
  float grass=noise(p*.003+51);float3 meadow=lerp(float3(.22,.35,.14),float3(.42,.43,.23),patch);
  float3 woods=lerp(float3(.13,.27,.12),float3(.26,.36,.18),patch);
  float3 c=lerp(meadow,woods,smoothstep(.42,.72,grass));
  float wet=(1-smoothstep(.7,3.0,IN.worldPos.y))*.45;
  c=lerp(c,float3(.33,.31,.20),wet);c*=.92+.16*noise(p*.35);
  float3 drySand=lerp(float3(.52,.46,.32),float3(.66,.60,.44),noise(p*.09));
  float3 wetSand=drySand*float3(.59,.62,.65);
  float3 beach=lerp(wetSand,drySand,smoothstep(-.10,.70,IN.worldPos.y));
  beach*=.97+.03*sin(p.x*1.7+p.y*.45+noise(p*.025)*4);
  c=lerp(c,beach,_Beach*IN.color.a);
  o.Albedo=c*_Color.rgb;o.Metallic=0;o.Smoothness=.08;
 }
 ENDCG
 } FallBack "Standard"
}
