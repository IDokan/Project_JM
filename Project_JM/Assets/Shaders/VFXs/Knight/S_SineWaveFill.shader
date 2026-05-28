// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: S_SineWaveFill.shader
// Summary: Fills a sprite/quad with a color below a sine wave boundary;
//          pixels above the wave are discarded (transparent).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

Shader "JM/VFX/SineWaveFill"
{
    Properties
    {
        // Declared to satisfy Unity UI's CanvasRenderer; not sampled.
        _MainTex            ("Main Tex",        2D)            = "white" {}

        _NoiseTex           ("Noise Map",       2D)            = "white" {}
        _FillLevel          ("Fill Level",      Range(0, 1))   = 0.5
        _WaveAmplitude      ("Wave Amplitude",  Range(0, 0.5)) = 0.05
        _WaveFrequency      ("Wave Frequency",  Range(0, 20))  = 5.0
        _WaveSpeed          ("Wave Speed",      Range(0, 10))  = 2.0
        _EdgeSoftness       ("Edge Softness",   Range(0, 0.2)) = 0.05
        _NoiseScrollSpeed   ("Noise Scroll Speed", Vector)    = (0.1, 0.05, 0, 0)

        // Required by Unity UI for Mask component support.
        _StencilComp        ("Stencil Comparison", Float) = 8
        _Stencil            ("Stencil ID",         Float) = 0
        _StencilOp          ("Stencil Operation",  Float) = 0
        _StencilReadMask    ("Stencil Read Mask",  Float) = 255
        _StencilWriteMask   ("Stencil Write Mask", Float) = 255
        _ColorMask          ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        ColorMask [_ColorMask]

        Pass
        {
            Name "SineWaveFill"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            float  _FillLevel;
            float  _WaveAmplitude;
            float  _WaveFrequency;
            float  _WaveSpeed;
            float  _EdgeSoftness;
            float4 _NoiseScrollSpeed;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv         = input.uv;
                output.color      = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // Sine wave boundary in UV space (0 = bottom, 1 = top).
                float wave = _FillLevel + _WaveAmplitude * sin(_WaveFrequency * uv.x + _Time.y * _WaveSpeed);

                // Fade alpha to 0 across the wave boundary instead of a hard clip.
                // smoothstep returns 0 below the wave and 1 above, so 1 - smoothstep
                // gives full opacity below and transparent above, with a soft transition.
                float edge      = 1.0 - smoothstep(wave - _EdgeSoftness, wave + _EdgeSoftness, uv.y);
                float2 noiseUV  = uv + _NoiseScrollSpeed.xy * _Time.y;
                float noise     = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;
                return half4(input.color.rgb, input.color.a * noise * edge);
            }
            ENDHLSL
        }
    }
}
