Shader "RockTheGame/GhostOutline"
{
    // Rakibin platform/tehlike hayalet konturu: LineRenderer'a kesik çizgi (dash) çizer. Kesik, dokuyla değil burada:
    // LineRenderer "Tile" doku kipinde uv.x çizgi boyunca dünya birimi olarak artar, _Period dünya birimi cinsinden
    // (dolu + boş), _Duty dolu oranı. Her ikisini GhostOutlineView kameranın yakınlığına göre ekran pikseline
    // sabitler. Renk ve saydamlık LineRenderer köşe renginden gelir.
    Properties
    {
        _Period ("Periyot (dünya birimi, script)", Float) = 0.3
        _Duty ("Dolu oran (script)", Range(0.05, 0.95)) = 0.6
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
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Period;
            float _Duty;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color = IN.color;
                OUT.uv = IN.uv;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float phase = frac(IN.uv.x / max(_Period, 1e-4));
                // Kenarda ~yarım piksellik yumuşatma: ekran uzayı türevi kadar.
                float aa = max(fwidth(IN.uv.x) / max(_Period, 1e-4), 1e-4);
                float dash = smoothstep(0.0, aa, phase) * (1.0 - smoothstep(_Duty - aa, _Duty, phase));
                fixed4 c = IN.color;
                c.a *= dash;
                return c;
            }
            ENDCG
        }
    }
}
