Shader "Ironclads/Ocean"
{
 Properties { _SeaColor("Sea", Color) = (.018,.095,.13,1) _WaveHeight("Wave height", Float) = 0.8 _NormalTex("Water detail",2D)="gray" {} }
 SubShader
 {
  Tags { "RenderType"="Opaque" }
  Pass
  {
   Tags { "LightMode"="ForwardBase" }
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "UnityCG.cginc"
   #include "Lighting.cginc"
   float4 _SeaColor;
   sampler2D _NormalTex;
   float _WaveHeight, _SeaTime, _WindSpeed;
   float4 _Wind, _SunDirection, _SunColor, _SkyTint;
   struct appdata { float4 vertex:POSITION; };
   struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; UNITY_FOG_COORDS(1) };
   float wave(float2 p) {
    float2 d = normalize(_Wind.xy + .00001);
    return _WaveHeight * (.32*sin(dot(p,d)*.14+_SeaTime*1.2) + .14*sin(dot(p,float2(d.y,-d.x))*.29+_SeaTime*1.7) + .04*sin(p.x*.73+p.y*.6+_SeaTime*2.3));
   }
   v2f vert(appdata v) {
    v2f o; float3 w = mul(unity_ObjectToWorld,v.vertex).xyz;
    w.y += wave(w.xz); o.world=w; o.pos=mul(UNITY_MATRIX_VP,float4(w,1));
    UNITY_TRANSFER_FOG(o,o.pos); return o;
   }
   fixed4 frag(v2f i):SV_Target {
    float2 p=i.world.xz; float e=.16;
    float dx=(wave(p+float2(e,0))-wave(p-float2(e,0)))/(2*e);
    float dz=(wave(p+float2(0,e))-wave(p-float2(0,e)))/(2*e);
    float2 wind=normalize(_Wind.xy+.00001);
    float2 a=tex2D(_NormalTex,p*.024+wind*_SeaTime*.006).rg*2-1;
    float2 b=tex2D(_NormalTex,float2(p.y,-p.x)*.061-wind*_SeaTime*.009).rg*2-1;
    float detail=.14+saturate(_WindSpeed/40)*.26;
    float3 n=normalize(float3(-dx+(a.x+b.x*.5)*detail,1,-dz+(a.y+b.y*.5)*detail));
    float3 v=normalize(_WorldSpaceCameraPos-i.world);
    float fresnel=.025+.75*pow(1-saturate(dot(n,v)),5);
    float3 reflection=lerp(_SkyTint.rgb*.72,_SkyTint.rgb*1.12,saturate(reflect(-v,n).y));
    float spec=pow(saturate(dot(normalize(v+_SunDirection.xyz),n)),380)*4.5;
    float3 sea=_SeaColor.rgb*(.65+.4*saturate(dot(n,_SunDirection.xyz)));
    float3 col=lerp(sea,reflection,fresnel)+spec*_SunColor.rgb;
    float foam=smoothstep(.45,.8,length(float2(dx,dz)))*saturate((_WindSpeed-20)/25);
    col=lerp(col,_SkyTint.rgb*.8,foam*.6);
    fixed4 c=fixed4(col,1); UNITY_APPLY_FOG(i.fogCoord,c); return c;
   }
   ENDCG
  }
 }
}
