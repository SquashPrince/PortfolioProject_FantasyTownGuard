Shader "UI/TutorialHoleOverlay"
{
    Properties
    {
        _Color("Overlay Color", Color) = (0, 0, 0, 0.7)
        _HoleCenter("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize("Hole Size", Vector) = (0.2, 0.2, 0, 0)
        _Softness("Edge Softness", Float) = 0.02
    }

        SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            fixed4 _Color;
            float4 _HoleCenter;
            float4 _HoleSize;
            float _Softness;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float2 halfSize = _HoleSize.xy * 0.5;

                float2 dist = abs(uv - _HoleCenter.xy);
                float2 edge = halfSize - dist;

                float inside = min(edge.x, edge.y);

                float alphaMask = smoothstep(0.0, _Softness, -inside);

                fixed4 col = _Color * i.color;
                col.a *= alphaMask;

                return col;
            }

            ENDCG
        }
    }
}