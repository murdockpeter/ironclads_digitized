Shader "Ironclads/Pantelleria Teak"
{
 Properties { _Color("Teak tint",Color)=(.53,.43,.30,1) }
 SubShader {
  Tags {"RenderType"="Opaque"} LOD 200
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
  #pragma target 3.0
  struct Input {float3 localPos;};
  fixed4 _Color;float _FinishSeed,_ShipLength,_ShipBeam,_PantelleriaRain,_PantelleriaAgeDays;float4 _WeatherProfile;
  void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;}
  float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
  float noise(float2 p){float2 a=floor(p),b=frac(p);b=b*b*(3-2*b);return lerp(lerp(hash(a),hash(a+float2(1,0)),b.x),lerp(hash(a+float2(0,1)),hash(a+1),b.x),b.y);}
  void surf(Input IN,inout SurfaceOutputStandard o){
   float2 p=IN.localPos.xz;float seed=_FinishSeed;
   float u=p.x/.22,row=floor(u),offset=hash(float2(row,seed))*4.5;
   float v=(p.y+offset)/4.5;float2 board=float2(row,floor(v));
   float tone=hash(board+seed),wear=saturate(_WeatherProfile.x+_PantelleriaAgeDays*.002);
   float2 f=frac(float2(u,v)),metres=float2(.22,4.5);
   float2 edge=min(f,1-f)*metres,aa=max(fwidth(p),float2(.0005,.0005));
   float crossSeam=1-smoothstep(max(0,.002-aa.x),.002+aa.x,edge.x);
   float endSeam=1-smoothstep(max(0,.002-aa.y),.002+aa.y,edge.y);
   float detail=1-saturate(max(fwidth(u),fwidth(v))*2);
   float caulk=max(crossSeam,endSeam)*detail;
   float broad=noise(p*.27+seed),grain=noise(float2(p.x*58,p.y*.65)+seed);
   float grainFade=1-saturate(fwidth(p.x)*65);
   float3 wood=_Color.rgb*(.88+.22*tone+(grain-.5)*.07*grainFade);
   float fade=saturate((_WeatherProfile.z*.23+broad*.18+wear*.10));
   wood=lerp(wood,float3(.47,.45,.40)*(.90+.18*tone),fade);
   float track=exp(-pow((abs(p.x)-_ShipBeam*(.29+.025*_WeatherProfile.w))/.65,2));
   track*=smoothstep(.02,.12,abs(p.y)/max(_ShipLength,.1));
   wood=lerp(wood,wood*1.12,track*wear*.32);
   float dirt=smoothstep(.55,.88,noise(p*float2(.8,.22)+seed*2))*(.05+wear*.07);
   wood*=1-dirt;
   // Small wooden screw plugs close to butt joints; subpixel marks fade away.
   float plugX=min(abs(f.x*.22-.045),abs(f.x*.22-.175));
   float plugZ=abs(edge.y-.055);
   float plug=(1-smoothstep(.007,.011,length(float2(plugX,plugZ))))*grainFade;
   wood=lerp(wood,wood*.74,plug*.45);
   wood=lerp(wood,float3(.095,.080,.060),caulk*.66);
   float wet=saturate(_PantelleriaRain)*(.55+.30*noise(p*.65+seed*3));
   wood*=1-wet*.25;
   float nx=(f.x<.5?-1:1)*exp(-edge.x/.004)*.16*detail;
   float ny=(f.y<.5?-1:1)*exp(-edge.y/.004)*.10*detail;
   o.Normal=normalize(float3(nx,ny,1));
   o.Albedo=wood;o.Metallic=0;o.Smoothness=lerp(.19,.47,wet);
   o.Occlusion=1-caulk*.12;
  }
  ENDCG
 }
 FallBack "Standard"
}
