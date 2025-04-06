void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color,
    out float DistanceAtten, out float ShadowAtten)
{
    #ifdef SHADERGRAPH_PREVIEW
        Direction = normalize(float3(0.5f, 0.5f, 0.25f));
        Color = float3(1.0f, 1.0f, 1.0f);
        DistanceAtten = 1.0f;
        ShadowAtten = 1.0f;
    #else
        #if SHADOWS_SCREEN
            half4 clipPos = TransformWorldToHClip(WorldPos);
            half4 shadowCoord = ComputeScreenPos(clipPos);
            shadowCoord.xyz /= shadowCoord.w; // Important perspective division
        #else
            half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif
        
        Light mainLight = GetMainLight(shadowCoord);
        Direction = mainLight.direction;
        Color = mainLight.color;
        DistanceAtten = mainLight.distanceAttenuation;
        ShadowAtten = 1.0; // Default value

        #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
            #if SHADOWS_SCREEN
                ShadowAtten = SampleScreenSpaceShadowmap(shadowCoord);
            #else
                ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
                half shadowStrength = GetMainLightShadowStrength();
                ShadowAtten = SampleShadowmap(
                    shadowCoord, 
                    TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),
                    shadowSamplingData, 
                    shadowStrength, 
                    true // Enable shadow bias to prevent acne
                );
            #endif
        #endif
    #endif
}