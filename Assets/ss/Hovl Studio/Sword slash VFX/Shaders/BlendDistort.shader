Shader "URP/Hovl/Particles/BlendDistortURP"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _Flow("Flow", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}
        _NormalMap("NormalMap", 2D) = "bump" {}
        _Color("Color", Color) = (0.5,0.5,0.5,1)
        _DistortionPower("Distortion Power", Float) = 0.1
        _Speed("Speed", Vector) = (0, 0, 0, 0)
        _Emission("Emission", Float) = 1
        _Opacity("Opacity", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Flow_ST;
                float4 _Mask_ST;
                float4 _Noise_ST;
                float4 _NormalMap_ST;
                float4 _Color;
                float _DistortionPower;
                float4 _Speed;
                float _Emission;
                float _Opacity;
            CBUFFER_END

            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_Flow);          SAMPLER(sampler_Flow);
            TEXTURE2D(_Mask);          SAMPLER(sampler_Mask);
            TEXTURE2D(_Noise);         SAMPLER(sampler_Noise);
            TEXTURE2D(_NormalMap);     SAMPLER(sampler_NormalMap);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv + _Time.y * _Speed.xy;
                float4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float4 noise = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uv);
                float4 flow = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, uv);
                float4 mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv);

                float alpha = baseCol.a * noise.a * mask.a * _Opacity * IN.color.a;
                float3 finalColor = baseCol.rgb * noise.rgb * mask.rgb * _Color.rgb * _Emission;

                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
