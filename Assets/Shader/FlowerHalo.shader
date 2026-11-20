Shader "Flower/Halo"
{
    // Açılışta çiçekten dışa yayılan halka dalgası (top-down'da açılmayı okutan 2B ipucu).
    // UV merkezi (0.5, 0.5). _Ring = halkanın yarıçapı (quad yarıçapının oranı), _Alpha = görünürlük; ikisini FlowerBloom sürer.
    Properties
    {
        _Color ("Color", Color) = (1.0, 0.70, 0.82, 1)
        _Width ("Ring Width", Range(0.01, 0.4)) = 0.12
        [HideInInspector] _Ring ("Ring (script)", Range(0, 1)) = 0.3
        [HideInInspector] _Alpha ("Alpha (script)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            fixed4 _Color;
            float _Width, _Ring, _Alpha;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float r = length((IN.uv - 0.5) * 2.0);
                float d = abs(r - _Ring) - _Width * 0.5;
                float aa = max(fwidth(d), 1e-5);
                float a = (1.0 - smoothstep(-aa, aa, d)) * _Alpha;
                return fixed4(_Color.rgb, a);
            }
            ENDCG
        }
    }
}
