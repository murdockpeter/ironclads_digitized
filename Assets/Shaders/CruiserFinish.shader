Shader "Ironclads/Cruiser Finish" {
 Properties { _Color("Paint or teak tint",Color)=(.30,.33,.35,1) _Metallic("Metallic",Range(0,1))=.12 _Glossiness("Smoothness",Range(0,1))=.2 _FinishKind("Paint 0, hull 1, teak 2",Float)=0 }
 SubShader { Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
 struct Input { float3 localPos; };
 fixed4 _Color;half _Metallic,_Glossiness;float _FinishKind;
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 p=IN.localPos;float variation=1+.025*sin(p.z*1.7+p.x*.8)*sin(p.y*3.1+p.z*.23);
  float3 tint=_Color.rgb*variation;
  if(_FinishKind>1.5){
   float plank=floor(p.x/.19),offset=frac(sin(plank*13.13)*43758.5)*4;
   float seam=1-smoothstep(.006,.018,min(frac(p.x/.19),1-frac(p.x/.19))*.19);
   float butt=1-smoothstep(.004,.014,min(frac((p.z+offset)/4),1-frac((p.z+offset)/4))*4);
   float grain=sin(p.z*43+sin(p.x*251)*2)*.018;
   tint*=.96+.05*sin(plank*7.1)+grain-.20*max(seam,butt); 
  }else if(_FinishKind>.5){
   // Waterline stripe and submerged antifouling are reconstruction choices.
   tint=lerp(tint,float3(.065,.075,.073),1-smoothstep(-.35,-.15,p.y));
   tint=lerp(tint,float3(.24,.075,.055),1-smoothstep(-1.05,-.95,p.y));
   float horizontal=1-smoothstep(.003,.012,min(frac((p.y+.2)/1.25),1-frac((p.y+.2)/1.25))*1.25);
   tint*=1-.065*horizontal;
  }
  o.Albedo=tint;o.Metallic=_Metallic;o.Smoothness=_Glossiness;
 }
 ENDCG
 } FallBack "Standard"
}
