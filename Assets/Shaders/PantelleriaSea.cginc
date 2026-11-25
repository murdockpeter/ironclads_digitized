float _PantelleriaSea,_PantelleriaFinePatch;
float4 seaComponent(float2 p,float2 d,float k,float amplitude,float phase){
 float omega=sqrt(9.81*k),q=dot(p,d)*k-_SeaTime*omega+phase,c=amplitude*cos(q);
 return float4(amplitude*sin(q),c*k*d.x,c*k*d.y,-c*omega);
}
float4 pantelleriaSeaSample(float2 p){
 float2 d=normalize(_Wind.xy);
 float2 b=float2(d.x*.819152-d.y*.573576,d.x*.573576+d.y*.819152);
 float2 c=float2(d.x*.906308+d.y*.422618,-d.x*.422618+d.y*.906308);
 return _WaveHeight*(seaComponent(p,d,.14,.32,0)+seaComponent(p,b,.29,.14,1.3)+seaComponent(p,c,.62,.04,2.4));
}
float2 pantelleriaSeaSlope(float2 p,float footprint){
 float2 d=normalize(_Wind.xy),b=float2(d.x*.819152-d.y*.573576,d.x*.573576+d.y*.819152),c=float2(d.x*.906308+d.y*.422618,-d.x*.422618+d.y*.906308);
 return _WaveHeight*(seaComponent(p,d,.14,.32,0).yz*exp(-pow(footprint*.14,2))+seaComponent(p,b,.29,.14,1.3).yz*exp(-pow(footprint*.29,2))+seaComponent(p,c,.62,.04,2.4).yz*exp(-pow(footprint*.62,2)));
}
