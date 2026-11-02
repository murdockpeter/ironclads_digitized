Shader "Ironclads/Worked Iron" {
 Properties { _Color("Iron tint",Color)=(.13,.155,.16,1) _Metallic("Metallic",Range(0,1))=.55 _Glossiness("Smoothness",Range(0,1))=.25 _SurfaceKind("Turret or casemate",Float)=1 }
 SubShader { Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
 struct Input { float3 localPos; };
 fixed4 _Color;half _Metallic,_Glossiness;float _SurfaceKind;
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;}
 float shot(float2 delta){float r=length(delta);return exp(-r*r/.018)*-.10+exp(-pow(r-.13,2)/.0012)*.32;}
 float turretShot(float a,float y,float angle,float height){float d=atan2(sin(a-angle),cos(a-angle))*3.2766;return shot(float2(d,y-height));}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 p=IN.localPos;float wear=0;
  if(_SurfaceKind<1.5){float a=atan2(p.x,p.z);wear+=turretShot(a,p.y,radians(-66),1.25);wear+=turretShot(a,p.y,radians(74),1.8);wear+=turretShot(a,p.y,radians(137),.62);wear+=turretShot(a,p.y,radians(-145),2.12);wear+=turretShot(a,p.y,radians(213),1.55);wear+=turretShot(a,p.y,radians(98),.75);}
  else {for(int i=0;i<4;i++){float z=-11+i*7.3+sign(p.x)*.6,y=(i%2==0)?2.7:1.65;wear+=shot(float2(p.z-z,p.y-y));}}
  float mottling=.985+.015*sin(p.x*17+p.z*11)*sin(p.y*23+p.z*5);
  o.Albedo=_Color.rgb*(mottling+wear);o.Metallic=_Metallic;o.Smoothness=max(.08,_Glossiness-abs(wear)*.25);
 }
 ENDCG
 } FallBack "Standard"
}
