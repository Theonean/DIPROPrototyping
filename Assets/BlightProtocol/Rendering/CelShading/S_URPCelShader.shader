Shader "Unlit/S_URPCelShader"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}

        _ShadowTint ("Shadow Tint", Color) = (0,0,0,0)
        _LightingCutoff ("Lighting Cutoff", Range(0,1)) = 0.05
        _FalloffAmount ("Falloff amount", Range(0,1)) = 0
        _AmbientStrength ("Ambient Strength", Range(0,1)) = 0.5

        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionMap ("Emission Texture", 2D) = "black" {}

        _SpecularColor ("Specular Color", Color) = (0,0,0,0)
        _Smoothness ("Smoothness", Float) = 100
        _SpecularAlpha ("Specular Alpha", Range(0,1)) = 0

        _FresnelColor ("Fresnel Color", Color) = (0,0,0,0)
        _FresnelSize ("Fresnel Size", Float) = 0.2
    }
    SubShader
    {
        Tags {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }
        LOD 100

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                TEXTURE2D(_BaseMap);
                SAMPLER(sampler_BaseMap);
                float4 _BaseMap_ST;
                
                TEXTURE2D(_EmissionMap);
                SAMPLER(sampler_EmissionMap);
                float4 _EmissionColor;
                
                float4 _ShadowTint;
                float _LightingCutoff;
                float _FalloffAmount;   
                float _AmbientStrength;
                
                float4 _SpecularColor;
                float _SpecularAlpha;
                float _Smoothness;

                float4 _FresnelColor;
                float _FresnelSize;
            CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags {"LightMode"="UniversalForward"}

            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #pragma multi_compile _ _DBUFFER
            #pragma multi_compile_local_fragment _ _BOTTOMGRADIENT _TOPGRADIENT

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            InputData BuildInputData(Varyings IN)
            {
                InputData inputData;
                inputData.positionWS = IN.worldPos;
                inputData.normalWS = normalize(IN.worldNormal);
                inputData.viewDirectionWS = normalize(_WorldSpaceCameraPos - IN.worldPos);
                inputData.shadowCoord = IN.shadowCoord;
                inputData.fogCoord = IN.fogFactor;
                inputData.vertexLighting = float3(0, 0, 0);       // Optional if not using per-vertex lighting
                inputData.bakedGI = float3(0, 0, 0);              // Optional if not using lightmaps
                inputData.normalizedScreenSpaceUV = float2(0, 0); // Optional
                inputData.shadowMask = float4(1,1,1,1);           // Optional
                return inputData;
            }

            Varyings UnlitPassVertex(Attributes IN) {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normal);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.fogFactor = ComputeFogFactor(OUT.positionCS.z);
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.worldPos);
                return OUT;
            }

            half4 UnlitPassFragment(Varyings IN) : SV_Target
            {
                float4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float3 albedo = albedoTex.rgb * _BaseColor.rgb;

                InputData inputData = BuildInputData(IN);
                Light mainLight = GetMainLight(inputData.shadowCoord);

                float NdotL = dot(inputData.normalWS, mainLight.direction);
                float lightingStep = smoothstep(
                    clamp(_LightingCutoff, _FalloffAmount, 1) - _FalloffAmount,
                    _LightingCutoff + _FalloffAmount,
                    saturate(NdotL)
                );
                lightingStep = max(lightingStep, _AmbientStrength);

                float3 litColor = lerp(_ShadowTint.rgb, mainLight.color.rgb, lightingStep);
                float3 finalColor = albedo * litColor;

                finalColor = MixFog(finalColor, inputData.fogCoord);
                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
        

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ColorMask 0
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            //#pragma shader_feature_local _PARALLAXMAP
            //#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            
            ENDHLSL
        }
    }
}
