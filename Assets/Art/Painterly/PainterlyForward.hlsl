#ifndef SEA_GLASS_PAINTERLY_FORWARD_INCLUDED
#define SEA_GLASS_PAINTERLY_FORWARD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Scene-wide look, set by the PainterlyLook component.
float4 _SG_ShadowTint;   // rgb: colour of the shadow side, a: its brightness
float4 _SG_Terminator;   // x: light/shadow edge position, y: edge softness, z: sun strength, w: sky fill
float4 _SG_Rim;          // rgb: warm rim colour, a: strength
float4 _SG_Paint;        // x: variation scale (1/m), y: warm/cool hue drift, z: AO strength, w: specular strength
float _SG_Dust;           // round 10: neglect inside the studio, 1 at the start, 0 when restored (StudioRestoration)
float4 _SG_DustBox;       // xz min (xy) and xz max (zw) of the studio interior

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 uv         : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv         : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3  normalWS   : TEXCOORD2;
    half   fogFactor  : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

float SG_Hash(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float SG_ValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = SG_Hash(i);
    float b = SG_Hash(i + float2(1, 0));
    float c = SG_Hash(i + float2(0, 1));
    float d = SG_Hash(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// Two octaves of soft noise in world space: broad painted patches, not texture.
float SG_PaintNoise(float3 positionWS, float3 normalWS)
{
    float3 w = abs(normalWS);
    w /= (w.x + w.y + w.z + 1e-4);
    float scale = _SG_Paint.x;
    float n = SG_ValueNoise(positionWS.zy * scale) * w.x
            + SG_ValueNoise(positionWS.xz * scale) * w.y
            + SG_ValueNoise(positionWS.xy * scale) * w.z;
    float n2 = SG_ValueNoise(positionWS.xz * scale * 3.1 + 17.0);
    return n * 0.7 + n2 * 0.3;
}

Varyings PainterlyVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
    output.positionCS = positionInputs.positionCS;
    output.positionWS = positionInputs.positionWS;
    output.normalWS = normalInputs.normalWS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
    return output;
}

half4 PainterlyFragment(Varyings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // Ground surfaces can map their texture in world space so it tiles seamlessly across meshes.
    // Mode 1: ground (xz). Mode 2: walls and roofs, picking the world plane the surface faces
    // (texture v runs up the wall, so boards and courses stay horizontal).
    // Two-sided surfaces (grass blades, leaves, fabric) light their back faces with the flipped normal,
    // so they are not rendered near-black from behind (round 7).
    float3 normalWS = normalize(input.normalWS) * IS_FRONT_VFACE(face, 1.0, -1.0);
    float2 uv = input.uv;
    float tile = max(_WorldTile, 0.01);
    if (_WorldMapping > 1.5)
    {
        float3 an = abs(normalWS);
        uv = an.y > max(an.x, an.z) ? input.positionWS.xz / tile
           : an.x > an.z ? input.positionWS.zy / tile : input.positionWS.xy / tile;
    }
    else if (_WorldMapping > 0.5) uv = input.positionWS.xz / tile;
    half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb * _BaseColor.rgb;
    float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);

    // Painted variation: value shifts plus a slight warm/cool drift.
    float paint = SG_PaintNoise(input.positionWS, normalWS) - 0.5;
    albedo *= 1.0 + paint * _Variation * 2.0;
    albedo = lerp(albedo, albedo * half3(1.06, 0.98, 0.9), saturate(paint * _SG_Paint.y * 4.0));
    albedo = lerp(albedo, albedo * half3(0.92, 0.97, 1.06), saturate(-paint * _SG_Paint.y * 4.0));

    // Broad ground patches (round 3): 10-15 m areas of lighter golden and darker rosy sand, so open
    // beach never reads as one flat colour (A Short Hike, Tchia).
    if (_GroundPatches > 0.001)
    {
        float big = SG_ValueNoise(input.positionWS.xz * 0.07 + 3.0) * 0.6 + SG_ValueNoise(input.positionWS.xz * 0.21 + 11.0) * 0.4 - 0.5;
        albedo *= 1.0 + big * _GroundPatches * 1.8;
        albedo = lerp(albedo, albedo * half3(0.97, 0.9, 0.9), saturate(-big * 4.0) * _GroundPatches * 2.4);
        albedo = lerp(albedo, albedo * half3(1.04, 1.01, 0.9), saturate(big * 4.0) * _GroundPatches * 2.4);
        // Round 10: wind ripples in the near field (fade out by ~25 m), lit crest and cooler trough, so the
        // sand at the player's feet has detail without speckling the distance.
        // Round 47 (review 43, two reviewers: "one parallel ripple pattern at one scale across the whole beach"): the
        // ripples run in a direction that turns from patch to patch, at two scales, and fade out in patches, as wind
        // ripples do between smoother sweeps. A broad cool/warm drift (40-60 m) sits under the 10-15 m patches.
        float2 wq = input.positionWS.xz;
        float turn = (SG_ValueNoise(wq * 0.045 + 17.0) - 0.5) * 2.6;   // round 48: a wider swing, so no one stripe direction dominates a view
        float2 rdir = float2(cos(0.38 + turn), sin(0.38 + turn));
        float warp = SG_ValueNoise(wq * 0.35) * 5.0 + SG_ValueNoise(wq * 1.3) * 1.2;
        float rip = sin(dot(wq, rdir) * 7.5 + warp);
        float rip2 = sin(dot(wq, float2(-rdir.y, rdir.x) * 0.3 + rdir) * 16.0 + warp * 0.6);
        float ripMask = smoothstep(0.42, 0.8, SG_ValueNoise(wq * 0.09 + 5.0)) * 0.85 + 0.15;   // real gaps between rippled sweeps
        float near = 1.0 - smoothstep(6.0, 25.0, distance(input.positionWS, _WorldSpaceCameraPos));
        float ripK = near * ripMask * (_GroundPatches / 0.4);
        albedo *= 1.0 + (rip * 0.045 + rip2 * 0.022) * ripK;
        albedo = lerp(albedo, albedo * half3(0.93, 0.95, 1.04), saturate(-rip) * 0.26 * ripK);
        float drift = SG_ValueNoise(wq * 0.022 + 31.0) - 0.5;
        albedo *= 1.0 + drift * 0.14 * (_GroundPatches / 0.4);
        albedo = lerp(albedo, albedo * half3(0.95, 0.97, 1.03), saturate(-drift * 3.0) * 0.5 * (_GroundPatches / 0.4));
    }

    // Round 76 (review 72, all three: the b1, b2 and b7 foregrounds "large near-single-colour areas", against 5.3's rule on one
    // flat colour over a third of the screen; Tchia's sand carries far more value change). Beach sand only (_SandDetail):
    // - mid-scale tone (2-5 m): lighter, warmer wind-swept sweeps and slightly darker, cooler packed sand;
    // - damp hollows: soft-edged darker, cooler patches where the sand holds water;
    // - near the player, scattered shell grit: small pale and dark flecks, faded out before they would shimmer.
    if (_SandDetail > 0.001)
    {
        float2 sq = input.positionWS.xz;
        float mid = saturate((SG_ValueNoise(sq * 0.32 + 41.0) * 0.65 + SG_ValueNoise(sq * 0.85 + 9.0) * 0.35 - 0.5) * 2.6 + 0.5) - 0.5;   // stretched: two blended octaves stay near their middle
        albedo *= 1.0 + mid * 0.45 * _SandDetail;
        albedo = lerp(albedo, albedo * half3(1.05, 1.0, 0.9), saturate(mid * 3.0) * 0.8 * _SandDetail);
        albedo = lerp(albedo, albedo * half3(0.94, 0.95, 1.02), saturate(-mid * 3.0) * 0.7 * _SandDetail);
        float dampN = SG_ValueNoise(sq * 0.11 + 63.0) * 0.7 + SG_ValueNoise(sq * 0.5 + 2.0) * 0.3;
        half damp = smoothstep(0.6, 0.66, dampN) * _SandDetail;   // the lit sand sits near white, so the change must be strong to read
        albedo = lerp(albedo, albedo * half3(0.6, 0.62, 0.74), damp * 0.9);
        albedo *= 1.0 - saturate(damp * (1.0 - damp) * 4.0) * 0.1 * _SandDetail;   // a slightly darker drying rim
        float camD = distance(input.positionWS, _WorldSpaceCameraPos);
        float2 gc = floor(sq * 2.2); float gh = SG_Hash(gc + 13.0);
        float2 gp = frac(sq * 2.2) - (0.25 + 0.5 * float2(SG_Hash(gc + 3.0), SG_Hash(gc + 29.0)));
        half fleck = (1.0 - smoothstep(0.04, 0.07, length(gp))) * step(0.7, gh) * (1.0 - smoothstep(4.0, 12.0, camD));
        albedo = lerp(albedo, gh > 0.88 ? albedo * half3(1.22, 1.18, 1.1) : albedo * half3(0.66, 0.6, 0.58), fleck * 0.8 * _SandDetail);
    }

    // Round 27 (review 25: island grass read as "blocky pixel camo"): grass or moss painted per pixel on faces
    // that point up, with a soft, noisy edge, instead of a per-face material split.
    if (_TopTint.a > 0.001)
    {
        float topN = SG_ValueNoise(input.positionWS.xz * 0.35) * 0.6 + SG_ValueNoise(input.positionWS.xz * 1.4 + 7.0) * 0.4;
        half top = smoothstep(_TopStart - 0.06, _TopStart + 0.08, normalWS.y + (topN - 0.5) * 0.28);
        half3 grass = _TopTint.rgb * (0.88 + SG_ValueNoise(input.positionWS.xz * 0.9 + 3.0) * 0.24);
        albedo = lerp(albedo, grass, top * _TopTint.a);
    }

    // Height gradient: darker, cooler base and a sunlit crown.
    float height = saturate(input.positionWS.y / max(_GradientHeight, 0.05));
    albedo *= lerp(1.0 - _GradientStrength, 1.0 + _GradientStrength * 0.3, smoothstep(0.0, 1.0, height));

    // Neglected studio (round 10): dusty, greyed and dimmer before restoration; clears panel by panel.
    if (_SG_Dust > 0.001)
    {
        float2 q = input.positionWS.xz;
        float inside = step(_SG_DustBox.x, q.x) * step(q.x, _SG_DustBox.z) * step(_SG_DustBox.y, q.y) * step(q.y, _SG_DustBox.w) * step(input.positionWS.y, 6.0);
        // Nothing within arm's reach is dusted: held tools, the hands and the station in use keep their colour.
        inside *= smoothstep(2.2, 3.5, distance(input.positionWS, _WorldSpaceCameraPos));
        float grime = SG_ValueNoise(q * 3.1) * 0.5 + SG_ValueNoise(q * 11.0) * 0.5;
        half lumA = dot(albedo, half3(0.3, 0.55, 0.15));
        half3 dusty = lerp(albedo, lumA * half3(0.86, 0.82, 0.78) + 0.05, 0.62) * (0.84 + grime * 0.12);
        albedo = lerp(albedo, dusty, _SG_Dust * inside);
    }

    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    half4 shadowMask = half4(1, 1, 1, 1);
    Light mainLight = GetMainLight(shadowCoord, input.positionWS, shadowMask);

    half nDotL = dot(normalWS, mainLight.direction);
    half edge = smoothstep(_SG_Terminator.x - _SG_Terminator.y, _SG_Terminator.x + _SG_Terminator.y, nDotL);
    half lightAmount = edge * lerp(1.0h, mainLight.shadowAttenuation, _SunShadow) * mainLight.distanceAttenuation;

    half3 sky = SampleSH(normalWS);
    half3 shadowSide = albedo * (_SG_ShadowTint.rgb * _SG_ShadowTint.a + sky * _SG_Terminator.w);
    // Graded light within the lit side so gentle slopes (dunes, swells) still read as form.
    half form = 0.55 + 0.45 * saturate(nDotL / 0.5);
    half3 litSide = albedo * (mainLight.color * _SG_Terminator.z * form + sky * _SG_Terminator.w * 0.5);
    half3 colour = lerp(shadowSide, litSide, lightAmount);

    // Ambient occlusion pools into the shadow colour instead of grey.
    AmbientOcclusionFactor ao = GetScreenSpaceAmbientOcclusion(screenUV);
    // Round 77 (review 73, all three, and every review since 66: the truck bonnet and fenders "crumpled", "mottled", "smeared"):
    // on large smooth painted panels the screen-space occlusion is noise, and it pooled into the shadow colour in blotches.
    // Those materials turn it down (_AOScale); creases and prop bases elsewhere keep it.
    half occlusion = lerp(1.0, ao.indirectAmbientOcclusion, _SG_Paint.z * _AOScale);
    colour = lerp(colour * _SG_ShadowTint.rgb * 0.9, colour, occlusion);

    // Soft sun sheen for enamel, glass-like paint and metal.
    float3 halfDir = SafeNormalize(mainLight.direction + viewWS);
    half spec = pow(saturate(dot(normalWS, halfDir)), exp2(9.0 * _Smoothness + 2.0));
    colour += mainLight.color * spec * _Smoothness * lightAmount * _SG_Paint.w;

    // Warm rim, strongest when looking towards the sun.
    half fresnel = pow(1.0 - saturate(dot(normalWS, viewWS)), 4.0);
    half towardSun = saturate(dot(-viewWS, mainLight.direction)) * 0.7 + 0.3;
    colour += _SG_Rim.rgb * _SG_Rim.a * _Rim * fresnel * towardSun * (0.4 + 0.6 * lightAmount);

    // Round 21 (reviews: grass "near-black spiky silhouettes"): thin two-sided foliage and fabric let the low sun
    // through. Looking towards the sun, their shaded side glows warm instead of going dark (Tchia, LMD).
    if (_Cull < 0.5)
    {
        half through = pow(saturate(dot(-viewWS, mainLight.direction)), 1.5);
        colour += albedo * mainLight.color * (0.12 + 0.5 * through) * mainLight.shadowAttenuation * (1.0 - lightAmount * 0.6);
    }

    #if defined(_ADDITIONAL_LIGHTS) || USE_CLUSTER_LIGHT_LOOP
    InputData inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    inputData.normalizedScreenSpaceUV = screenUV;
    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, input.positionWS, shadowMask);
        half wrap = saturate(dot(normalWS, light.direction) * 0.6 + 0.4);
        colour += albedo * light.color * wrap * wrap * light.distanceAttenuation * light.shadowAttenuation;
    LIGHT_LOOP_END
    #endif

    colour += _EmissionColor.rgb;
    colour = MixFog(colour, input.fogFactor);
    return half4(colour, 1.0);
}

#endif
