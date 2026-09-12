Shader "Battle Brews/Object Outline"
{
    Properties
    {
        [HDR] _HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
        _Strength("Outline Width (Pixels)", Range(0, 10)) = 2.38
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry+20"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _HighlightColor;
                float _Strength;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 normalVS = TransformWorldToViewDir(normalWS, true);
                float2 outlineDirection = normalVS.xy;
                float directionLength = length(outlineDirection);

                if (directionLength > 0.0001)
                    outlineDirection /= directionLength;

                float2 pixelToClip = 2.0 / _ScreenParams.xy;
                positionCS.xy += outlineDirection * max(_Strength, 0.0) * pixelToClip * positionCS.w;
                output.positionCS = positionCS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _HighlightColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
