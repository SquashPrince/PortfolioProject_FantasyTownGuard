Shader "Custom/SpriteScaledGlowOutline"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Sprite Tint", Color) = (1,1,1,1)

        _OutlineColor("Outline Color", Color) = (1,1,0,1)
        _Scale("Outline Scale", Range(1, 1.5)) = 1.1
        _GlowPower("Glow Power", Range(0, 5)) = 1
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "IgnoreProjector" = "True"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            // 뒤쪽 확대된 스프라이트
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _OutlineColor;
                float _Scale;
                float _GlowPower;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    fixed4 color : COLOR;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata v)
                {
                    v2f o;

                    float4 pos = v.vertex;
                    pos.xy *= _Scale;

                    o.vertex = UnityObjectToClipPos(pos);
                    o.uv = v.uv;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 tex = tex2D(_MainTex, i.uv);

                    fixed4 col = _OutlineColor;
                    col.rgb *= 1 + _GlowPower;
                    col.a *= tex.a;

                    return col;
                }
                ENDCG
            }

            // 원본 스프라이트
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _Color;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    fixed4 color : COLOR;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    fixed4 color : COLOR;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.color = v.color * _Color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return tex2D(_MainTex, i.uv) * i.color;
                }
                ENDCG
            }
        }
}