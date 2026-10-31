Shader "Ironclads/Chart" {
 Properties { _Color("Color",Color)=(.3,.7,.7,.23) }
 SubShader { Tags { "Queue"="Transparent+10" "RenderType"="Transparent" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color, _Wind;float _SeaTime,_ChartWaveHeight;
 float4 vert(float4 v:POSITION):SV_POSITION {
 float3 p=mul(unity_ObjectToWorld,v).xyz;
 // The chart is a stable horizontal measuring plane above wave crests. Distorting long
 // edges by individual wave samples makes them intersect the sea and breaks the grid.
 p.y+=_ChartWaveHeight*.51;
 return mul(UNITY_MATRIX_VP,float4(p,1)); }
 fixed4 frag():SV_Target {return _Color;}
 ENDCG }
 }
}
