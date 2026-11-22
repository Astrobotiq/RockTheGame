Shader "Flower/Halo"
{
    // Çiçeğin etrafındaki halka, iki kullanım:
    //  1) Açılış dalgası (top-down'da açılmayı okutan 2B ipucu): düz halka, _Ring büyür, _Alpha söner (_Dashes = 0).
    //  2) Kapanış uyarı halkası: _Dashes > 0 iken kesikli amber halka, etrafında koyu kasa (turuncu zeminde ve renk körlüğünde
    //     amber tek başına kaybolur; kasa onu ayırır). Yanıp sönmeyi FlowerBloom _Alpha ile sürer (FlowerWarnMath).
    // UV merkezi (0.5, 0.5). _Ring = halkanın yarıçapı (quad yarıçapının oranı), _Alpha = görünürlük; ikisini FlowerBloom sürer.
    Properties
    {
        _Color ("Açılış dalgası", Color) = (0.898, 0.914, 1.0, 1)
        _Width ("Ring Width", Range(0.01, 0.4)) = 0.12
        _WarnCol ("Uyarı halkası (amber)", Color) = (1.0, 0.788, 0.302, 1)
        _CasingCol ("Uyarı halkası kasası", Color) = (0.188, 0.216, 0.471, 1)
        _DashFill ("Dash dolgusu (aralığın oranı)", Range(0.2, 0.9)) = 0.6
        _Casing ("Kasa kalınlığı (quad oranı)", Range(0, 0.1)) = 0.025
        [HideInInspector] _Dashes ("Dash sayısı (script; 0 = düz halka)", Float) = 0
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

            #define TAU 6.28318530718

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

            fixed4 _Color, _WarnCol, _CasingCol;
            float _Width, _DashFill, _Casing, _Dashes, _Ring, _Alpha;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = (IN.uv - 0.5) * 2.0;
                float r = length(p);
                float radial = abs(r - _Ring) - _Width * 0.5;

                // Kesikli halka: açıya göre dilimlenmiş çubuklar. Çubuk boyu yay uzunluğunda ölçülür (dash ortasından uzaklık).
                float d = radial;
                if (_Dashes > 0.5)
                {
                    float sector = TAU / _Dashes;
                    float local = frac(atan2(p.y, p.x) / sector) - 0.5;
                    float arc = abs(local) * sector * max(r, 1e-3);
                    float halfLen = _DashFill * 0.5 * sector * _Ring;
                    d = max(radial, arc - halfLen);
                }

                float aa = max(fwidth(d), 1e-5);
                float fill = 1.0 - smoothstep(-aa, aa, d);

                if (_Dashes <= 0.5)
                    return fixed4(_Color.rgb, fill * _Alpha);

                float casing = 1.0 - smoothstep(-aa, aa, d - _Casing);
                return fixed4(lerp(_CasingCol.rgb, _WarnCol.rgb, fill), casing * _Alpha);
            }
            ENDCG
        }
    }
}
