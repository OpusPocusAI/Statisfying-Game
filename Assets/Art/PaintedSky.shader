// Golden-hour painted sky: graded dome, crisp sun and sculpted clouds lit from the sun side.
// The sun direction comes from PainterlyLook (_SG_SunDir) so sky and lighting always agree.
Shader "Sea Glass/Painted Sky"
{
    Properties
    {
        _Horizon("Horizon (pale yellow)", Color) = (0.98, 0.86, 0.66, 1)
        _MidSky("Mid sky (peach)", Color) = (0.96, 0.68, 0.53, 1)
        _Zenith("Zenith (dusty lavender)", Color) = (0.62, 0.55, 0.72, 1)
        _SunColour("Sun", Color) = (1, 0.93, 0.74, 1)
        _SunGlow("Sun glow", Color) = (1, 0.62, 0.32, 1)
        _CloudLit("Cloud lit side", Color) = (1, 0.88, 0.72, 1)
        _CloudShade("Cloud shadow side", Color) = (0.72, 0.56, 0.68, 1)
        _CloudCover("Cloud cover", Range(0, 1)) = 0.46
        _CloudScale("Cloud scale", Float) = 1.6
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct A { float4 p : POSITION; };
            struct V { float4 p : SV_POSITION; float3 dir : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                half4 _Horizon, _MidSky, _Zenith, _SunColour, _SunGlow, _CloudLit, _CloudShade;
                half _CloudCover, _CloudScale;
            CBUFFER_END
            float4 _SG_SunDir;

            float Hash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + 1), f.x), f.y);
            }
            float Noise3(float3 p)
            {
                float3 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                float n = i.x + i.y * 57.0 + i.z * 113.0;
                float4 h0 = frac(sin(float4(n, n + 1, n + 57, n + 58)) * 43758.5453);
                float4 h1 = frac(sin(float4(n + 113, n + 114, n + 170, n + 171)) * 43758.5453);
                float4 h = lerp(h0, h1, f.z);
                return lerp(lerp(h.x, h.y, f.x), lerp(h.z, h.w, f.x), f.y);
            }
            float Fbm3(float3 p)
            {
                float v = 0, a = 0.62;
                for (int k = 0; k < 3; k++) { v += Noise3(p) * a; p = p * 2.1 + 11.7; a *= 0.38; }
                return v;
            }
            float Fbm(float2 p)
            {
                // Few octaves: big sculpted masses with gentle edge breakup, not smoke.
                float v = 0, a = 0.62;
                for (int k = 0; k < 3; k++) { v += Noise(p) * a; p = p * 2.1 + 11.7; a *= 0.38; }
                return v;
            }

            V vert(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); o.dir = i.p.xyz; return o; }

            half4 frag(V i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float3 sunDir = normalize(dot(_SG_SunDir.xyz, _SG_SunDir.xyz) > 0.01 ? _SG_SunDir.xyz : float3(-0.8, 0.35, 0.3));
                float h = d.y;

                // Graded dome: pale yellow horizon, peach band, dusty lavender overhead.
                half3 c = lerp(_Horizon.rgb, _MidSky.rgb, smoothstep(0.0, 0.12, h));
                // Round 3: the cool zenith starts lower, so eye-level views show blue above the warm band (Tchia, LMD).
                c = lerp(c, _Zenith.rgb, smoothstep(0.06, 0.4, h));
                float sunAlign = saturate(dot(d, sunDir));
                // Warmer, brighter sky around the sun; cooler sky opposite it.
                c = lerp(c, c * half3(0.88, 0.92, 1.06), (1 - sunAlign) * 0.4);
                // The wide glow stays near the horizon; high sky keeps its blue even on the sun side.
                c += _SunGlow.rgb * pow(sunAlign, 9) * 0.32 * (1 - smoothstep(0.12, 0.55, h) * 0.6) + _SunGlow.rgb * pow(sunAlign, 60) * 0.5;
                // Below the horizon fades to the haze colour so the sea edge stays soft.
                c = lerp(c, _Horizon.rgb * 0.95, smoothstep(0.0, -0.08, h));

                // Sun: crisp disc, never pure white.
                float disc = smoothstep(0.99935, 0.99965, sunAlign);
                c = lerp(c, _SunColour.rgb * 1.6, disc);

                // Clouds on a flattened dome; thicker near the horizon, clear overhead.
                // Wrap the cloud field around the horizon (no stretching, no seam):
                // banks of cumulus sitting above the sea, flattened bases, taller tops.
                float2 ring = normalize(d.xz + 1e-5);
                float3 cp = float3(ring * 3.2 * _CloudScale, h * 5.5 * _CloudScale);
                float band = smoothstep(0.065, 0.11, h) * (1 - smoothstep(0.19, 0.36, h));   // round 5: lifted clear of the dune crests
                float density = Fbm3(cp + float3(3.1, 7.4, 1.3));
                // The band erodes the noise rather than masking it, so edges stay organic.
                float cloudField = density - (1 - band) * 0.7 + 0.05;
                // Round 5: crisp, graphic silhouettes (Firewatch) — the edge is about one pixel wide,
                // instead of a fixed noise range that read as blurred, melting blobs.
                float fw = max(fwidth(cloudField) * 1.2, 0.002);
                float shape = smoothstep(1 - _CloudCover - fw, 1 - _CloudCover + fw, cloudField);
                // Self-shadowing: sample towards the sun and upwards; denser there means this side faces away.
                float3 toSun = float3(normalize(sunDir.xz + 1e-4) * 0.16, 0.22);
                float towardSun = Fbm3(cp + toSun + float3(3.1, 7.4, 1.3));
                float lit = saturate(0.5 + (density - towardSun) * 6.0 + (h - 0.1) * 1.5);
                // Paint the light in three flat-ish bands, like a hand-painted backdrop.
                float lfw = max(fwidth(lit * 3.0), 0.02);
                // Round 9: two broad value bands (lit top, shaded underside) instead of three, which broke the
                // clouds into camouflage-like inner blotches.
                lit = smoothstep(0.42 - lfw, 0.58 + lfw, lit) * 0.75 + saturate(lit) * 0.25;
                half3 cloud = lerp(_CloudShade.rgb, _CloudLit.rgb, lit);
                // Bright silver rim on cloud edges near the sun.
                float edge = shape * (1 - smoothstep(1 - _CloudCover + 0.02, 1 - _CloudCover + 0.2, cloudField));
                cloud += _SunGlow.rgb * edge * pow(sunAlign, 3) * 0.9;
                cloud = lerp(cloud, c, smoothstep(0.08, 0.0, h) * 0.6);
                c = lerp(c, cloud, shape * 0.95);

                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
