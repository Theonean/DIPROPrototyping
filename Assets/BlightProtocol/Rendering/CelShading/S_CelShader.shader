Shader "Unlit/S_CelShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}

        _ShadowTint ("Shadow Tint", Color) = (0,0,0,0)
        _LightingCutoff ("Lighting Cutoff", Range(0,1)) = 0.05
        _FalloffAmount ("Falloff amount", Range(0,1)) = 0
        _AmbientStrength ("Ambient Strength", Range(0,1)) = 0.5

        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionTex ("Emission Texture", 2D) = "black" {}

        _SpecularColor ("Specular Color", Color) = (0,0,0,0)
        _Smoothness ("Smoothness", Float) = 100
        _SpecularAlpha ("Specular Alpha", Range(0,1)) = 0

        _FresnelColor ("Fresnel Color", Color) = (0,0,0,0)
        _FresnelSize ("Fresnel Size", Float) = 0.2

        _WSBottomOpacity ("WS Bottom Opacity", Range(0,1)) = 0
        _WSBottomColor ("WS Bottom Color", Color) = (1,1,1,1)
        _WSBottomHeight ("WS Bottom Height", Float) = 0
        _WSBottomFalloff ("WS Bottom Falloff", Range(0,1)) = 0
        _WSBottomAlphaBlend ("WS Bottom Alpha Blend", Range(0,1)) = 0

        _WSTopOpacity ("WS Top Opacity", Range(0,1)) = 0
        _WSTopColor ("WS Top Color", Color) = (1,1,1,1)
        _WSTopHeight ("WS Top Height", Float) = 10
        _WSTopFalloff ("WS Top Falloff", Range(0,1)) = 0
        _WSTopNormalStep ("WS TOp Normal Step", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="AlphaTest" }

        Pass // ShadowCaster
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass // GBuffer
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite[_ZWrite]
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            // Deferred Rendering Path does not support the OpenGL-based graphics API:
            // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
            #pragma exclude_renderers gles3 glcore

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitGBufferPassVertex
            #pragma fragment LitGBufferPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitGBufferPass.hlsl"
            ENDHLSL
        }

        Pass // DepthOnly
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass // DepthNormals
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            // -------------------------------------
            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

        Pass // Forward
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _DBUFFER
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
            #include "CelShaderInput.hlsl"
            #if (SHADERPASS != SHADERPASS_FORWARD)
                #undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.fogFactor = ComputeFogFactor(o.vertex.z);
                o.shadowCoord = TransformWorldToShadowCoord(o.worldPos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample shadow attenuation from URP shadow map
                OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
                
                float3 worldNormal = normalize(i.worldNormal);

                float4 albedo = tex2D(_MainTex, i.uv) * _Color;
                float4 emission = tex2D(_EmissionTex, i.uv) * _EmissionColor;
		        
                // Planar Mapping Bottom
                float bottomHeight = saturate(smoothstep(
                    1 - i.worldPos.g,
                    _WSBottomHeight - _WSBottomFalloff,
                    _WSBottomHeight + _WSBottomFalloff
                ));
                albedo = lerp(_WSBottomColor, albedo, bottomHeight*_WSBottomOpacity);

                // Planar Mapping Top with normals
                float topHeight = saturate(smoothstep(
                    worldNormal.b,
                    _WSTopNormalStep - _WSTopFalloff,
                    _WSTopNormalStep + _WSTopFalloff
                ));

                topHeight = smoothstep(
                    topHeight,
                    _WSTopHeight - _WSBottomFalloff,
                    _WSBottomHeight + _WSTopFalloff
                );
                albedo = lerp(_WSTopColor, albedo, topHeight*_WSTopOpacity);
                

                // Light
                float3 lightDir = GetMainLight().direction;
                float3 lightColor = GetMainLight().color;

                // Shadows
                float4 shadowmask = SAMPLE_SHADOWMASK(lightmapUV);
                float shadowAtten = 1.0;
                float4 shadowCoord = ComputeScreenPos(TransformWorldToHClip(i.worldPos));
                shadowAtten = MainLightShadow(shadowCoord, i.worldPos, shadowmask, _MainLightOcclusionProbes);

                
                float NdotL = saturate(dot(worldNormal, lightDir));

                float lighting = smoothstep(
                    clamp(_LightingCutoff, _FalloffAmount, 1) - _FalloffAmount,
                    _LightingCutoff + _FalloffAmount,
                    NdotL
                );
                lighting = max(lighting, _AmbientStrength);

                // Apply shadow attenuation to lighting
                float3 coloredLighting = lerp(_ShadowTint.rgb, lightColor, lighting) * max(shadowAtten, _ShadowTint.rgb);
                float3 diffuse = albedo.rgb * coloredLighting;

                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                float3 halfDir = normalize(lightDir + viewDir);

                float specular = pow(saturate(dot(worldNormal, halfDir)), _Smoothness);
                specular = smoothstep(
                    clamp(_LightingCutoff, _FalloffAmount, 1) - _FalloffAmount,
                    _LightingCutoff + _FalloffAmount,
                    specular
                );
                float3 specularColor = specular * _SpecularColor.rgb * lightColor * shadowAtten;

                float fresnel = pow(1.0 - saturate(dot(viewDir, worldNormal)), rcp(_FresnelSize));
                fresnel = smoothstep(
                    clamp(_LightingCutoff, _FalloffAmount, 1) - _FalloffAmount,
                    _LightingCutoff + _FalloffAmount,
                    fresnel
                );
                float3 fresnelColor = fresnel * _FresnelColor.rgb;

                float3 finalColor = diffuse + specularColor*_SpecularAlpha + fresnelColor + emission.rgb;

                #if defined(_DBUFFER)
                    // Placeholder decal blend
                    float3 decalColor = float3(0.05, 0.05, 0.05);
                    finalColor = lerp(finalColor, decalColor, 0.0);
                #endif

                finalColor = MixFog(finalColor, i.fogFactor);
                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }
    }
}

