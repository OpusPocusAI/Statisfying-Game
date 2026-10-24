#ifndef SEA_GLASS_PAINTERLY_FORWARD_INCLUDED
#define SEA_GLASS_PAINTERLY_FORWARD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Scene-wide look, set by the PainterlyLook component.
float4 _SG_ShadowTint;   // rgb: colour of the shadow side, a: its brightness
float4 _SG_Terminator;   // x: light/shadow edge position, y: edge softness, z: sun strength, w: sky fill
float4 _SG_Rim;          // rgb: warm rim colour, a: strength
float4 _SG_Paint;        // x: variation scale (1/m), y: warm/cool hue drift, z: AO strength, w: specular strength

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

half4 PainterlyFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // Ground surfaces can map their texture in world space so it tiles seamlessly across meshes.
    float2 uv = _WorldMapping > 0.5 ? input.positionWS.xz / max(_WorldTile, 0.01) : input.uv;
    half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb * _BaseColor.rgb;
    float3 normalWS = normalize(input.normalWS);
    float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);

    // Painted variation: value shifts plus a slight warm/cool drift.
    float paint = SG_PaintNoise(input.positionWS, normalWS) - 0.5;
    albedo *= 1.0 + paint * _Variation * 2.0;
    albedo = lerp(albedo, albedo * half3(1.06, 0.98, 0.9), saturate(paint * _SG_Paint.y * 4.0));
    albedo = lerp(albedo, albedo * half3(0.92, 0.97, 1.06), saturate(-paint * _SG_Paint.y * 4.0));

    // Height gradient: darker, cooler base and a sunlit crown.
    float height = saturate(input.positionWS.y / max(_GradientHeight, 0.05));
    albedo *= lerp(1.0 - _GradientStrength, 1.0 + _GradientStrength * 0.3, smoothstep(0.0, 1.0, height));

    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    half4 shadowMask = half4(1, 1, 1, 1);
    Light mainLight = GetMainLight(shadowCoord, input.positionWS, shadowMask);

    half nDotL = dot(normalWS, mainLight.direction);
    half edge = smoothstep(_SG_Terminator.x - _SG_Terminator.y, _SG_Terminator.x + _SG_Terminator.y, nDotL);
    half lightAmount = edge * mainLight.shadowAttenuation * mainLight.distanceAttenuation;

    half3 sky = SampleSH(normalWS);
    half3 shadowSide = albedo * (_SG_ShadowTint.rgb * _SG_ShadowTint.a + sky * _SG_Terminator.w);
    // Graded light within the lit side so gentle slopes (dunes, swells) still read as form.
    half form = 0.55 + 0.45 * saturate(nDotL / 0.5);
    half3 litSide = albedo * (mainLight.color * _SG_Terminator.z * form + sky * _SG_Terminator.w * 0.5);
    half3 colour = lerp(shadowSide, litSide, lightAmount);

    // Ambient occlusion pools into the shadow colour instead of grey.
    AmbientOcclusionFactor ao = GetScreenSpaceAmbientOcclusion(screenUV);
    half occlusion = lerp(1.0, ao.indirectAmbientOcclusion, _SG_Paint.z);
    colour = lerp(colour * _SG_ShadowTint.rgb * 0.9, colour, occlusion);

    // Soft sun sheen for enamel, glass-like paint and metal.
    float3 halfDir = SafeNormalize(mainLight.direction + viewWS);
    half spec = pow(saturate(dot(normalWS, halfDir)), exp2(9.0 * _Smoothness + 2.0));
    colour += mainLight.color * spec * _Smoothness * lightAmount * _SG_Paint.w;

    // Warm rim, strongest when looking towards the sun.
    half fresnel = pow(1.0 - saturate(dot(normalWS, viewWS)), 4.0);
    half towardSun = saturate(dot(-viewWS, mainLight.direction)) * 0.7 + 0.3;
    colour += _SG_Rim.rgb * _SG_Rim.a * _Rim * fresnel * towardSun * (0.4 + 0.6 * lightAmount);

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
