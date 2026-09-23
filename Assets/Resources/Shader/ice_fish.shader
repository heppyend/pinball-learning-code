Shader "Shader Forge/ICE_URP"
{
    Properties
    {
        [Toggle]_Hit("Hit", Float) = 0
        [Toggle]_Texandice("Texandice", Float) = 0
        _Diffuse("Diffuse", 2D) = "white" {}
        _Tex_ice("Tex_ice", 2D) = "white" {}
        _Ice_texglow("Ice_texglow", Range(0, 2)) = 0.7292595
        _Ice_glow("Ice_glow", Range(0, 2)) = 1
        _Ice_duibidu("Ice_duibidu", Range(0, 4)) = 2
        _Tex_glow("Tex_glow", Range(0, 2)) = 1.19646
        _hit_color_glow("hit_color_glow", Range(0, 2)) = 2
        _Ice_color("Ice_color", Color) = (1,1,1,1)
        _hit_color("hit_color", Color) = (1,0,0,1)
        _xiaosanmask("xiaosanmask", 2D) = "white" {}
        _xiaosan("xiaosan", Range(0, 5)) = 5
        _xiaosancolor("xiaosancolor", Color) = (1,0.2939665,0,1)
        [HideInInspector]_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float fogCoord : TEXCOORD1;
            };

            TEXTURE2D(_Diffuse); SAMPLER(sampler_Diffuse);
            TEXTURE2D(_Tex_ice); SAMPLER(sampler_Tex_ice);
            TEXTURE2D(_xiaosanmask); SAMPLER(sampler_xiaosanmask);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _Diffuse_ST;
            float4 _Tex_ice_ST;
            float4 _xiaosanmask_ST;
            float _Ice_texglow;
            float _Tex_glow;
            float _hit_color_glow;
            float4 _hit_color;
            float _Texandice;
            float _Hit;
            float4 _Ice_color;
            float _Ice_glow;
            float _Ice_duibidu;
            float _xiaosan;
            float4 _xiaosancolor;
            float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag(Varyings IN, half facing : VFACE) : SV_Target
            {
                float4 mask = SAMPLE_TEXTURE2D(_xiaosanmask, sampler_xiaosanmask, TRANSFORM_TEX(IN.uv, _xiaosanmask));
                float dissolve = mask.r * _xiaosan;
                clip(dissolve - _Cutoff);

                // Texture Samples
                float4 diffuseTex = SAMPLE_TEXTURE2D(_Diffuse, sampler_Diffuse, TRANSFORM_TEX(IN.uv, _Diffuse));
                float4 iceTex = SAMPLE_TEXTURE2D(_Tex_ice, sampler_Tex_ice, TRANSFORM_TEX(IN.uv, _Tex_ice));

                // Ice Effect
                float3 iceColor = pow(iceTex.rgb * _Ice_color.rgb * _Ice_glow, _Ice_duibidu);
                float3 texIceCombined = iceColor + (diffuseTex.rgb * _Ice_texglow);
                
                // Main Color Logic
                float3 baseColor = lerp(diffuseTex.rgb * _Tex_glow, texIceCombined, _Texandice);
                float3 hitColor = diffuseTex.rgb * _hit_color_glow * _hit_color.rgb;
                float3 finalColor = lerp(baseColor, hitColor, _Hit);

                // Dissolve Edge Effect
                float dissolveEdge = saturate(step(dissolve, 0.53) * step(0.5, dissolve));
                finalColor += dissolveEdge * _xiaosancolor.rgb;

                // Fog & Output
                finalColor = MixFog(finalColor, IN.fogCoord);
                return half4(finalColor, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            TEXTURE2D(_xiaosanmask);
            SAMPLER(sampler_xiaosanmask);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _xiaosanmask_ST;
            float _xiaosan;
            float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 mask = SAMPLE_TEXTURE2D(_xiaosanmask, sampler_xiaosanmask, TRANSFORM_TEX(IN.uv, _xiaosanmask));
                float dissolve = mask.r * _xiaosan;
                clip(dissolve - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
    CustomEditor "ShaderForgeMaterialInspector"
}