// Surface-composited displacement-ship wash, not a recorded voyage track.
float4 _PantelleriaWashPositions[42],_PantelleriaWashProfiles[42],_PantelleriaHullMotion[42],_PantelleriaWashHydro[42];
int _PantelleriaWashCount;
float washHash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float washNoise(float2 p){float2 a=floor(p),b=frac(p);b=b*b*(3-2*b);return lerp(lerp(washHash(a),washHash(a+float2(1,0)),b.x),lerp(washHash(a+float2(0,1)),washHash(a+1),b.x),b.y);}
float washWidth(float length,float beam,float z){return beam*.5*(.035+.965*pow(max(0,sin(saturate(z/length+.5)*UNITY_PI)),.55));}
float2 washLocal(float2 offset,float4 state){return float2(offset.x*state.w-offset.y*state.z,dot(offset,state.zw));}
float wakeRise(float2 p,float4 profile){
 float edge=washWidth(profile.x,profile.y,p.y),side=abs(p.x)-edge;
 float front=smoothstep(.12,.38,p.y/profile.x)*(1-smoothstep(.48,.53,p.y/profile.x));
 float q=(side-1.2)/2.8;
 return exp(-q*q)*front*profile.z*.24;
}
float pantelleriaWakeHeight(float2 world){
 float height=0;
 for(int ship=0;ship<clamp(_PantelleriaWashCount,0,42);ship++){
  float4 state=_PantelleriaWashPositions[ship],profile=_PantelleriaWashProfiles[ship];float2 offset=world-state.xy;
  if(dot(offset,offset)>profile.x*profile.x)continue;
  height+=wakeRise(washLocal(offset,state),profile)*saturate(_PantelleriaWashHydro[ship].y/.28);
 }
 return min(height,.35);
}
float3 paintPantelleriaWash(float3 ocean,float3 world){
 float footprint=max(length(ddx(world.xz)),length(ddy(world.xz)));
 for(int ship=0;ship<clamp(_PantelleriaWashCount,0,42);ship++) {
  float4 state=_PantelleriaWashPositions[ship],profile=_PantelleriaWashProfiles[ship],motion=_PantelleriaHullMotion[ship],hydro=_PantelleriaWashHydro[ship];float2 offset=world.xz-state.xy;
  if(dot(offset,offset)>profile.x*profile.x*4)continue;
  float2 p=washLocal(offset,state);float edge=washWidth(profile.x,profile.y,p.y),side=abs(p.x)-edge;
  float along=1-smoothstep(.46,.51,abs(p.y)/profile.x);
  float outside=smoothstep(-.10,.25,side)*along;
  float lineHeight=motion.x+motion.y*p.x+motion.z*p.y;
  float crest=saturate((world.y-lineHeight+.12)/max(.4,hydro.w*.5));
  float broken=washNoise(float2(p.x*1.3,p.y*.7+_SeaTime*1.4)+profile.w);
  float contact=exp(-max(0,side)*max(0,side)/.7)*outside*crest*saturate(hydro.w)*motion.w;
  float q=(side-.5)/1.0;
  float bow=exp(-q*q)*outside*smoothstep(.08,.36,p.y/profile.x)*profile.z*(.6+.4*saturate(hydro.y/.28));
  float behind=-p.y-profile.x*.48,u=max(0,behind)/max(1,profile.x*1.15);
  float width=profile.y*.10+max(0,behind)*.075;
  float track=exp(-p.x*p.x/max(1,width*width)*2.5)*smoothstep(0,.05,u)*(1-smoothstep(.12,1,u))*step(0,behind)*profile.z;
  float cells=washNoise(float2(p.x*.85,p.y*.34+_SeaTime*1.8)+profile.w);
  float foam=max(contact*.6,bow*.65)*(.25+.65*broken)+track*.45*smoothstep(.22,.8,cells);
  // Kelvin arms are wave shading, not a solid white V of propeller foam.
  float fromBow=profile.x*.5-p.y,arm=fromBow*.353553;
  float inside=smoothstep(0,2,arm-abs(p.x))*step(0,fromBow)*(1-smoothstep(profile.x,profile.x*1.8,fromBow));
  float wavelength=max(2,hydro.x),filter=exp(-pow(footprint*6.283185/wavelength,2));
  float ripple=cos(6.283185*(fromBow+abs(p.x)*.35)/wavelength)*inside*profile.z*filter*.035;
  ocean*=1+ripple;
  float light=saturate(max(_SkyTint.r,max(_SkyTint.g,_SkyTint.b))*2);
  float3 foamColor=lerp(_SkyTint.rgb*.80,float3(.72,.78,.76)*light,.45);
  ocean=lerp(ocean,foamColor,saturate(foam*.65));
 }
 return ocean;
}
