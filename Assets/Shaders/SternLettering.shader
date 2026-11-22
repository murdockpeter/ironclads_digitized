Shader "Ironclads/Stern Lettering"
{
    Properties { _MainTex ("Font atlas", 2D) = "white" {} }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Back
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR; };
            struct v2f { float4 position:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR; UNITY_FOG_COORDS(1) };
            sampler2D _MainTex;
            v2f vert(appdata v){v2f o;o.position=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;UNITY_TRANSFER_FOG(o,o.position);return o;}
            fixed4 frag(v2f i):SV_Target {fixed4 color=i.color;color.a*=tex2D(_MainTex,i.uv).a;clip(color.a-.01);UNITY_APPLY_FOG(i.fogCoord,color);return color;}
            ENDCG
        }
    }
}
