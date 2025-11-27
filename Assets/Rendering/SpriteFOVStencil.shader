Shader "Custom/StencilWriter"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        // 화면에 색상은 안 그림 (투명)
        ColorMask 0 
        ZWrite Off
        
        // 스텐실 버퍼에 1을 기록 (Replace)
        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; };
            v2f vert (appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag (v2f i) : SV_Target { return 0; }
            ENDCG
        }
    }
}
