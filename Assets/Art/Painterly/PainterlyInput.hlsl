#ifndef SEA_GLASS_PAINTERLY_INPUT_INCLUDED
#define SEA_GLASS_PAINTERLY_INPUT_INCLUDED

// Shared by every pass so the material buffer is identical (SRP Batcher compatible).
// Mirrors URP SimpleLitInput so URP's shadow, depth and depth-normal passes can be reused.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _BaseMap_TexelSize;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
    half _Smoothness;
    half _Metallic;
    half _GradientStrength;
    half _GradientHeight;
    half _Variation;
    half _Rim;
    half _WorldMapping;
    half _WorldTile;
    half _GroundPatches;
    half _Cull;            // round 21: 0 marks two-sided foliage and fabric, which lets light through
    half4 _TopTint;        // round 27: rgb grass/moss colour on upward faces, a = amount (0 off)
    half _TopStart;        // round 27: world normal.y where the top colour begins
    UNITY_TEXTURE_STREAMING_DEBUG_VARS;
CBUFFER_END

TEXTURE2D(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);

half4 SampleSpecularSmoothness(float2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    return half4(0, 0, 0, 1);
}

inline void InitializeSimpleLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.normalTS = half3(0, 0, 1);
    outSurfaceData.occlusion = 1.0;
}

#endif
