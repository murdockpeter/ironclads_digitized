Shader "Ironclads/Pantelleria Surface"
{
 Properties {
  _Color("Paint tint",Color)=(.30,.33,.35,1)
  _Metallic("Metallic",Range(0,1))=.12
  _Glossiness("Smoothness",Range(0,1))=.28
  _FinishKind("Paint 0, funnel 1",Float)=0
  _DeckHeight("Weather deck height",Float)=4.8
 }
 SubShader {
  Tags {"RenderType"="Opaque"} LOD 200
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
  #pragma target 3.0
  struct Input {float3 localPos;float3 localNormal;};
  fixed4 _Color;half _Metallic,_Glossiness;float _FinishKind,_DeckHeight,_HullPass,_FinishSeed;
  void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;o.localNormal=v.normal;}
  float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
  float noise(float2 p){float2 a=floor(p),b=frac(p);b=b*b*(3-2*b);return lerp(lerp(hash(a),hash(a+float2(1,0)),b.x),lerp(hash(a+float2(0,1)),hash(a+1),b.x),b.y);}
  float seam(float q){float pixel=max(fwidth(q),.001),d=abs(frac(q+.5)-.5);return (1-smoothstep(max(0,.004-pixel),.004+pixel,d))*(1-saturate(pixel*1.5));}
  void surf(Input IN,inout SurfaceOutputStandard o){
   float3 p=IN.localPos;float seed=_FinishSeed;
   float broad=noise(p.zy*.42+seed),fine=noise(p.zy*2.7+seed*3);
   float3 tint=_Color.rgb*(.94+.12*broad);
   float weather=0;
   if(_FinishKind>.5){
    float soot=saturate((p.y-_DeckHeight-6)/6)*(.10+.09*noise(float2(p.x*3+seed,p.y*.8)));
    tint*=1-soot;
   }else if(_HullPass>.5){
    float hull=(1-smoothstep(_DeckHeight-.15,_DeckHeight+.08,p.y))*(1-saturate(abs(IN.localNormal.y)*2));
    float plate=max(seam((p.y+.12)/1.1),seam((p.z+floor(p.y/1.1)*2)/4.8)*.65);
    tint*=1-plate*hull*.085;
    float streak=pow(noise(float2(p.z*1.7+seed,0)),9)*saturate(1-abs(p.y-(_DeckHeight-.7))/1.8);
    float grainFilter=1-saturate(max(length(ddx(p)),length(ddy(p)))*.3);
    weather=streak*hull*grainFilter*.23;
    tint=lerp(tint,float3(.26,.13,.065),weather);
    float tide=exp(-abs(p.y-.18)*2)*hull;
    tint*=1-tide*(.10+.08*broad);
    tint*=lerp(.78,1,smoothstep(-.7,.8,p.y));
   }
   o.Albedo=tint;o.Metallic=_Metallic;
   o.Smoothness=clamp(_Glossiness+(fine-.5)*.12-weather*.3,.09,.48);
  }
  ENDCG
 }
 FallBack "Standard"
}
