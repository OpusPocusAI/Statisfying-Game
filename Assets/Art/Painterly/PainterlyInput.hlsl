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
    half _SandDetail;
    half _AOScale;
    half _WetBand;
    half _LightWrap;
    half _Cull;            // round 21: 0 marks two-sided foliage and fabric, which lets light through
    half4 _TopTint;        // round 27: rgb grass/moss colour on upward faces, a = amount (0 off)
    half _TopStart;        // round 27: world normal.y where the top colour begins
    half _SunShadow;       // round 52: how much the sun shadow darkens this surface (1 all, 0 none: the seabed under the water)
    half _Sway;            // Nov 15 foliage pass: wind sway in metres per metre above the object origin (0 off)
    half _SwayHeight;      // Nov 15 layered plants: height (m) above the origin where the bend stops growing (0 = 2 m: grass and shrubs)
    UNITY_TEXTURE_STREAMING_DEBUG_VARS;
CBUFFER_END

TEXTURE2D(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);

half4 SampleSpecularSmoothness(float2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    return half4(0, 0, 0, 1);
}

// Wind sway (Nov 15, Tchia foliage pass). Every pass, the shadow and depth passes included, bends the vertex the same way, so
// swaying plants keep their shadows and never fight the depth prepass. Each plant leans by its height above its own origin
// (its base), so it needs its own transform: swaying renderers are kept out of static batching by TropicalFoliagePass.
float3 SG_Wind(float3 positionOS)
{
    if (_Sway <= 0.0001) return positionOS;
    float3 originWS = TransformObjectToWorld(float3(0, 0, 0));
    float3 positionWS = TransformObjectToWorld(positionOS);
    float h = clamp(positionWS.y - originWS.y, 0.0, _SwayHeight > 0.01 ? _SwayHeight : 2.0);   // palms bend along their whole trunk
    float t = _Time.y;
    float phase = dot(originWS.xz, float2(0.13, 0.21)) + dot(positionWS.xz, float2(0.35, 0.27));
    float gust = 0.7 + 0.3 * sin(t * 0.37 + originWS.x * 0.045 + originWS.z * 0.03);
    float lean = (sin(t * 1.55 + phase) * 0.6 + sin(t * 2.9 + phase * 1.7) * 0.25 + 0.3) * gust;
    float flutter = sin(t * 4.3 + phase * 2.3) * 0.18;
    float2 wind = float2(0.6, 0.8);
    float bend = _Sway * h * h / max(h, 0.35);     // stiff at the base, freer toward the tip
    positionWS.xz += (wind * lean + float2(-wind.y, wind.x) * flutter) * bend;
    positionWS.y -= abs(lean) * bend * 0.25 * saturate(h) * saturate(2.0 / max(h, 0.01));   // tips dip a little as they bend (not whole palm crowns)
    return TransformWorldToObject(positionWS);
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
