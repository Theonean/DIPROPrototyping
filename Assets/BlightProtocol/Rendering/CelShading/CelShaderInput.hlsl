#ifndef CEL_SHADER_INPUT_INCLUDED
#define CEL_SHADER_INPUT_INCLUDED

CBUFFER_START(UnityPerMaterial)
    sampler2D _MainTex;
    float4 _MainTex_ST;
    sampler2D _EmissionTex;
    float4 _EmissionColor;

    float4 _Color;
    float4 _ShadowTint;
    float _LightingCutoff;
    float _FalloffAmount;   
    float _AmbientStrength;
    
    float4 _SpecularColor;
    float _SpecularAlpha;
    float _Smoothness;

    float4 _FresnelColor;
    float _FresnelSize;

    float _WSBottomOpacity;
    float4 _WSBottomColor;
    float _WSBottomHeight;
    float _WSBottomFalloff;
    float _WSBottomAlphaBlend;

    float _WSTopOpacity;
    float4 _WSTopColor;
    float _WSTopHeight;
    float _WSTopFalloff;
    float _WSTopNormalStep;
CBUFFER_END

#endif // CEL_SHADER_INPUT_INCLUDED