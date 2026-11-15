// Ölümcül hareketli tehlikelerin ortasındaki yerçekimi makinesi (çizilmiş sprite).
// Sprite'ın kendisi değişmez; _LampMask verilirse lambalar sırayla yanıp söner.
// _LampMask kanalları: R = lamba çekirdeği, G = lambanın fazı (0..1, kovalamaca sırası), B = lamba çevresi parıltısı.
// Maske yoksa ("black" varsayılanı) shader düz sprite gibi davranır (kare çekirdek).
Shader "RockTheGame/HazardCore"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,0)
        [PerRendererData] _AlphaSplitEnabled ("Alpha Split Enabled", Float) = 0

        [Header(Lamps)]
        [NoScaleOffset] _LampMask ("Lamp Mask (R core, G phase, B glow)", 2D) = "black" {}
        _LampColor ("Lamp Color", Color) = (0.78, 1.0, 0.44, 1)   // #C7FF70, asit parlama tonu
        _LampMin ("Lamp Brightness When Off", Range(0,1)) = 0.35
        _BreathSpeed ("Breath Speed (cycles/sec)", Float) = 0.5
        _BreathAmount ("Breath Amount", Range(0,1)) = 0.25
        _PulseSpeed ("Chase Pulse Speed (cycles/sec)", Float) = 0.8
        _PulseWidth ("Chase Pulse Width (0..1 of cycle)", Range(0.02,1)) = 0.22
        _PulseBoost ("Pulse Overbright", Range(0,2)) = 0.6
        _GlowStrength ("Glow Around Lamp", Range(0,2)) = 0.55
        _PhaseOffset ("Phase Offset (per material)", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="False" // maske, sprite'ın tam doku UV'sini kullanır
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _LampMask;
            fixed4 _Color;
            fixed4 _RendererColor;
            fixed4 _LampColor;
            float _LampMin;
            float _BreathSpeed;
            float _BreathAmount;
            float _PulseSpeed;
            float _PulseWidth;
            float _PulseBoost;
            float _GlowStrength;
            float _PhaseOffset;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color * _RendererColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * i.color;
                float4 m = tex2D(_LampMask, i.uv);

                float t = _Time.y;
                // Hepsi birlikte yavaşça nefes alır...
                float breath = 0.5 + 0.5 * sin(t * _BreathSpeed * 6.2831853);
                // ...ve üstünden bir ışık sırayla lambaları dolaşır (keskin yanış, yumuşak sönüş).
                float cyc = frac(t * _PulseSpeed - m.g + _PhaseOffset);
                float pulse = saturate(1.0 - cyc / _PulseWidth);
                pulse *= pulse;

                float level = saturate(_LampMin + (1.0 - _LampMin) * max(breath * _BreathAmount, pulse));

                float3 rgb = tex.rgb;
                // lamba pikselleri: söndüğünde kararır, yandığında parlar ve beyaza kayar
                float3 lit = lerp(tex.rgb, _LampColor.rgb, 0.35) * (0.25 + 0.75 * level)
                           + _LampColor.rgb * pulse * _PulseBoost;
                rgb = lerp(rgb, lit, m.r);
                // gövdeye düşen parıltı (sprite sınırını aşmaz)
                rgb += _LampColor.rgb * m.b * (1.0 - m.r) * level * _GlowStrength * 0.5;

                return fixed4(rgb * tex.a, tex.a);
            }
        ENDCG
        }
    }
}
