// Shallow clear water over white enamel (PowerWash-style contrast: the glass must pop against it).
// Clear teal water that deepens toward the rim, bright caustic lines on the enamel floor, a soft
// shadow where the glass blocks the sun, and a lacy soap-foam line along the rim.
Shader "Sea Glass/Basin Water"
{
    Properties
    {
        _BaseColor("Enamel seen through water", Color) = (.7, .86, .86, 1)
        _Deep("Water tint at the edges", Color) = (.26, .56, .6, 1)
    }
    SubShader
    {
        // Round 10: see-through. Drawn after the glass (queue +50) so the submerged part of the piece shows
        // tinted through the water while the part above the surface keeps its own depth.
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+50" }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A { float4 p : POSITION; };
            struct V { float4 p : SV_POSITION; float2 local : TEXCOORD0; float fog : TEXCOORD1; float3 world : TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial) half4 _BaseColor, _Deep; CBUFFER_END

            float Hash(float2 p) { p = frac(p * float2(.1031, .1030)); p += dot(p, p.yx + 33.33); return frac((p.x + p.y) * p.x); }
            float Noise(float2 x)
            {
                float2 i = floor(x), f = frac(x); f = f * f * (3 - 2 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + 1), f.x), f.y);
            }

            V vert(A i) { V o; o.world = TransformObjectToWorld(i.p.xyz); o.p = TransformWorldToHClip(o.world); o.local = i.p.xz; o.fog = ComputeFogFactor(o.p.z); return o; }

            half4 frag(V i) : SV_Target
            {
                float2 p = i.local;
                float rim = max(abs(p.x), abs(p.y));
                float edge = smoothstep(.2, .5, rim);
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.world));
                half shade = lerp(.42, 1, sun.shadowAttenuation);          // the glass shades the floor below it
                half3 light = max(sun.color * shade * .55 + SampleSH(half3(0, 1, 0)) * .5, half3(.62, .6, .58));   // round 6: never a grey fog indoors (round 8: not milky either)

                // Enamel floor through clear water: bright in the middle, teal and deeper toward the rim.
                half3 c = lerp(_BaseColor.rgb, _Deep.rgb, edge * .9) * light;

                // Caustic network on the enamel, only where the sun reaches.
                float2 w = p * 8 + float2(_Time.y * .22, -_Time.y * .16);
                float caustic = abs(sin(w.x + sin(w.y * 1.3 + _Time.y * .4) * 1.4) * sin(w.y + sin(w.x * 1.1 - _Time.y * .3) * 1.4));
                caustic = pow(1 - caustic, 8);
                c += caustic * half3(1, .97, .86) * .16 * sun.shadowAttenuation * (1 - edge * .7);

                // Faint ripples and a soft sky reflection band.
                float ripple = sin(length(p * float2(1.6, 1)) * 48 - _Time.y * 2 + sin(p.x * 17) * .35);
                c += pow(saturate(ripple), 18) * .012;

                // A few drifting soap bubbles near the rim (foam and lace terms kept for tuning; unused).
                float lace = Noise(p * 38 + _Time.y * .15) * .6 + Noise(p * 90 - _Time.y * .1) * .4;
                float foamBand = smoothstep(.465, .485, rim) * (1 - smoothstep(.495, .5, rim));
                float foam = saturate(foamBand * smoothstep(.4, .6, lace));
                float bubbles = step(.965, Noise(p * 55 + float2(0, _Time.y * .05))) * smoothstep(.38, .46, rim);
                c = lerp(c, lerp(c, half3(.97, .99, .98), .8), saturate(bubbles * .3));   // only a few small bubbles: a foam band read as blotches at basin scale

                // Clear in the middle (the enamel floor and the glass show through), denser toward the rim.
                half alpha = lerp(.42, .86, edge) + caustic * .1;
                return half4(MixFog(c, i.fog), saturate(alpha));
            }
            ENDHLSL
        }
    }
}
