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
  float4 _WeatherProfile;float _PantelleriaRain,_PantelleriaAgeDays;
  void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;o.localNormal=v.normal;}
  float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
  float noise(float2 p){float2 a=floor(p),b=frac(p);b=b*b*(3-2*b);return lerp(lerp(hash(a),hash(a+float2(1,0)),b.x),lerp(hash(a+float2(0,1)),hash(a+1),b.x),b.y);}
  float seam(float q){float pixel=max(fwidth(q),.001),d=abs(frac(q+.5)-.5);return (1-smoothstep(max(0,.004-pixel),.004+pixel,d))*(1-saturate(pixel*1.5));}
  void surf(Input IN,inout SurfaceOutputStandard o){
   float3 p=IN.localPos;float seed=_FinishSeed;
   float2 face=p.zy+float2(step(0,p.x)*37.3,0);
   float broad=noise(face*.42+seed),fine=noise(face*2.7+seed*3);
   float wear=saturate(_WeatherProfile.x+_PantelleriaAgeDays*.002);
   float3 tint=_Color.rgb*(.95+.10*broad);
   tint=lerp(tint,tint*.88+float3(.055,.052,.045),_WeatherProfile.z*(.06+.08*broad));
   float closeFilter=1-saturate(max(length(ddx(p)),length(ddy(p)))*.7);
   float fleck=smoothstep(.64,.85,noise(p.zy*9+seed*5))*closeFilter;
   // Restrained fading and fine paint abrasion, filtered away at fleet distance.
   tint=lerp(tint,tint*.88+float3(.018,.014,.009),fleck*.12);
   float weather=0;
   if(_FinishKind>.5){
    float soot=saturate((p.y-_DeckHeight-6)/6)*(.10+.09*noise(float2(p.x*3+seed,p.y*.8)));
    tint*=1-soot;
    tint*=1-.04*seam((p.y-_DeckHeight)/1.75);
   }else if(_HullPass>.5){
    float hull=(1-smoothstep(_DeckHeight-.15,_DeckHeight+.08,p.y))*(1-saturate(abs(IN.localNormal.y)*2));
    float plate=max(seam((p.y+.12)/1.1),seam((p.z+floor(p.y/1.1)*2)/4.8)*.65);
    tint*=1-plate*hull*.11;
    float drip=pow(noise(float2(face.x*1.7+seed,seed*.17)),7);
    float streak=drip*saturate(1-abs(p.y-(_DeckHeight-.85))/(1.6+wear));
    float grainFilter=1-saturate(max(length(ddx(p)),length(ddy(p)))*.3);
    weather=streak*hull*grainFilter*(.22+.38*_WeatherProfile.y);
    weather+=plate*hull*smoothstep(.45,.8,fine)*closeFilter*(.025+.065*wear);
    tint=lerp(tint,float3(.26,.13,.065),weather);
    float tide=exp(-abs(p.y-(.16+.06*noise(float2(face.x*.6,seed))))*2)*hull;
    tint*=1-tide*(.08+.10*_WeatherProfile.y+.06*broad);
    float salt=exp(-abs(p.y-(.65+.20*broad))*3)*hull*(.025+.045*wear);
    tint=lerp(tint,float3(.59,.59,.54),salt);
    tint*=lerp(.78,1,smoothstep(-.7,.8,p.y));
   }
   float wet=saturate(_PantelleriaRain)*(.5+.25*broad);
   o.Albedo=tint*(1-wet*.13);o.Metallic=_Metallic;
   o.Smoothness=clamp(_Glossiness+(fine-.5)*.12-weather*.3+wet*.19,.09,.53);
  }
  ENDCG
 }
 FallBack "Standard"
}
