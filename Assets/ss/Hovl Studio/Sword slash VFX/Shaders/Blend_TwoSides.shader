Shader "Custom/TestWithMaskAndNoise"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Mask("Mask Texture", 2D) = "white" {}
        _Noise("Noise Texture", 2D) = "white" {}
        _Speed("Noise Scroll Speed", Vector) = (0, 0, 0, 0)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            sampler2D _Mask;
            float4 _Mask_ST;

            sampler2D _Noise;
            float4 _Noise_ST;

            float4 _Speed; // xy = noise UV speed, zw = unused
            float _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvMain : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
                float2 uvNoise : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvMask = TRANSFORM_TEX(v.uv, _Mask);
                o.uvNoise = TRANSFORM_TEX(v.uv, _Noise);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 noiseUV = i.uvNoise + _Speed.xy * _Time.y;

                fixed4 col = tex2D(_MainTex, i.uvMain) * _Color;
                fixed mask = tex2D(_Mask, i.uvMask).r;
                fixed noise = tex2D(_Noise, noiseUV).r;

                // Kết hợp mask * noise để tạo alpha động
                fixed alpha = mask * noise;

                // Cắt nếu alpha nhỏ hơn ngưỡng
                clip(alpha - _Cutoff);

                col.a *= alpha;

                return col;
            }
            ENDCG
        }
    }
}
