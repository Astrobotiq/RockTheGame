Shader "UI/CircularRotationRing"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _RampTex ("Ramp Texture (1D)", 2D) = "white" {}
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.0
        _OuterRadius ("Outer Radius", Range(0, 1)) = 0.5
        _RingCount ("Ring Count", Float) = 10.0
        _OuterFadeStart ("Outer Fade Start (0-1)", Range(0, 1)) = 1.0
        _OuterFadePower ("Outer Fade Power", Range(0.1, 5)) = 1.0
        
        _Progress ("Progress (0-1)", Range(0, 1)) = 0.0
        _MinAlpha ("Minimum Alpha (Uncompleted)", Range(0, 1)) = 0.2
        _Clockwise ("Clockwise (1 = Yes, 0 = No)", Float) = 1.0

        // UI Stencil Support
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // UI Stencil Support
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _RampTex;
            float4 _RampTex_ST;
            
            float2 _Center;
            float _InnerRadius;
            float _OuterRadius;
            float _RingCount;
            float _OuterFadeStart;
            float _OuterFadePower;

            float _Progress;
            float _MinAlpha;
            float _Clockwise;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // UV merkezine olan mesafeyi hesapla
                float2 uv = IN.texcoord - _Center;
                float dist = length(uv);

                // Belirlenen halka yarıçap aralığının dışındaysa görünmez yap
                if (dist < _InnerRadius || dist > _OuterRadius)
                {
                    return fixed4(0, 0, 0, 0);
                }

                // Mesafeyi halka alanının içinde 0 ile 1 arasına normalize et
                float normalizedRadius = (dist - _InnerRadius) / (_OuterRadius - _InnerRadius);

                // Keskin halka geçişleri sağlamak için mesafeyi adımlara (basamaklara) böl
                float index = floor(normalizedRadius * _RingCount);
                
                // Taşmaları önlemek için index değerini sınırla
                index = clamp(index, 0.0, _RingCount - 1.0);
                
                // Ramp dokusunun ilgili pikselinin tam ortasını (center) okumak için U koordinatını hesapla
                float sampleU = (index + 0.5) / _RingCount;

                // Ramp dokusunu örnekle
                fixed4 color = tex2D(_RampTex, float2(sampleU, 0.5));

                // En dışa doğru şeffaflığı azalt (Fade out)
                float fade = 1.0;
                if (normalizedRadius > _OuterFadeStart)
                {
                    float t = (normalizedRadius - _OuterFadeStart) / (1.0 - _OuterFadeStart + 0.0001);
                    fade = pow(1.0 - saturate(t), _OuterFadePower);
                }
                color.a *= fade;

                // Angle-based progress and alpha calculation
                // Calculate the angular position in UV space (atan2 returns -pi to pi)
                float angle = atan2(uv.y, uv.x);
                
                // Flip the direction if clockwise
                if (_Clockwise > 0.5)
                {
                    angle = -angle;
                }
                
                // Convert from [-pi, pi] to [0, 2*pi]
                if (angle < 0.0)
                {
                    angle += 2.0 * 3.14159265;
                }
                
                // Normalize to [0, 1]
                float pixelProgress = angle / (2.0 * 3.14159265);
                
                // Determine alpha multiplier
                float progressAlpha = 1.0;
                if (pixelProgress > _Progress)
                {
                    // Uncompleted part: fade from _MinAlpha to 1.0 as _Progress approaches 1.0
                    progressAlpha = lerp(_MinAlpha, 1.0, _Progress);
                }
                
                color.a *= progressAlpha;

                // UI/Sprite rengi ve şeffaflığıyla (vertex color) çarp
                color *= IN.color;

                return color;
            }
            ENDCG
        }
    }
}
