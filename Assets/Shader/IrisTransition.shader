Shader "UI/IrisTransition"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Transition Color", Color) = (0,0,0,1)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 1.5
        _EdgeSmoothing ("Edge Smoothing", Float) = 0.02
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
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
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float2 _Center;
            float _Radius;
            float _EdgeSmoothing;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                
                float2 uv = IN.texcoord;
                uv.x *= aspect;
                
                float2 center = _Center;
                center.x *= aspect;

                float dist = distance(uv, center);
                float alpha = smoothstep(_Radius - _EdgeSmoothing, _Radius + _EdgeSmoothing, dist);

                return fixed4(IN.color.rgb, IN.color.a * alpha);
            }
            ENDCG
        }
    }
}