// Sea glass: frosted and salt-crusted when dirty, clear and luminous when clean.
// Clean glass refracts and tints the scene behind it (URP opaque texture), glows when backlit by
// the sun and carries crisp highlights. Grime recedes in object space as cleaning progresses.
// Property names are unchanged: _BaseColor, _EmissionColor, _Smoothness, _Grime.
Shader "Sea Glass/Frosted Glass"
{
    Properties
    {
        _BaseColor("Glass colour", Color) = (.1, .8, .7, 1)
        [HDR] _EmissionColor("Inner light", Color) = (0, 0, 0, 1)
        _Smoothness("Polish", Range(0, 1)) = .8
        _Grime("Salt and sand", Range(0, 1)) = 0
        _Refraction("Refraction", Range(0, .2)) = .06
        _Frost("Frost colour", Color) = (.9, .9, .86, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "SeaGlass"
            Tags { "LightMode" = "UniversalForward" }
            // Drawn as a solid surface that composites the background itself, so the domed piece
            // never sorts its own back faces over its front.
            Blend One Zero
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct A { float4 p : POSITION; float3 n : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V
            {
                float4 p : SV_POSITION; float3 world : TEXCOORD0; float3 n : TEXCOORD1;
                float fog : TEXCOORD2; float3 local : TEXCOORD3; float3 localN : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor, _EmissionColor, _Frost;
                float _Smoothness, _Grime, _Refraction;
            CBUFFER_END

            float Hash(float3 p) { p = frac(p * .3183099 + .1); p *= 17.0; return frac(p.x * p.y * p.z * (p.x + p.y + p.z)); }
            float Noise(float3 x)
            {
                float3 i = floor(x), f = frac(x); f = f * f * (3 - 2 * f);
                return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x), lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x), lerp(Hash(i + float3(0, 1, 1)), Hash(i + 1), f.x), f.y), f.z);
            }

            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i); UNITY_TRANSFER_INSTANCE_ID(i, o);
                o.local = i.p.xyz; o.localN = i.n;
                o.world = TransformObjectToWorld(i.p.xyz);
                o.p = TransformWorldToHClip(o.world);
                o.n = TransformObjectToWorldNormal(i.n);
                o.fog = ComputeFogFactor(o.p.z);
                return o;
            }

            half4 frag(V i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 n = normalize(i.n), v = GetWorldSpaceNormalizeViewDir(i.world);
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.world));
                float3 l = sun.direction;
                half3 tint = _BaseColor.rgb;
                float nv = saturate(dot(n, v));
                float fresnel = pow(1 - nv, 3);

                // Reveal order, in object space so it stays attached to the piece:
                // the brush scrubs the middle first in streaks; the thin rounded rim clears last.
                float3 q = i.local * 4.0;
                float islands = Noise(q) * .65 + Noise(q * 2.3 + 7.1) * .35;
                float streaks = Noise(float3(i.local.x * 2.0, i.local.y * 14.0, i.local.z * 3.0));
                float fromEdge = saturate((abs(i.local.z) - .014) / .07);   // 0 at the rim, 1 in the thick middle
                float order = saturate(islands * .45 + streaks * .2 + (1 - fromEdge) * .45 - .05);
                // Each spot goes sandy dirt -> salt frost -> clear glass as grime falls past it.
                float grime = smoothstep(order - .1, order + .1, _Grime);
                float dirt = smoothstep(order + .18, order + .4, _Grime);

                // ---- Clean glass: refracted, tinted background plus body colour.
                float2 screenUV = GetNormalizedScreenSpaceUV(i.p);
                float3 viewN = mul((float3x3)UNITY_MATRIX_V, n);
                float2 offset = viewN.xy * _Refraction * (.6 + fresnel);
                half3 behind = SampleSceneColor(screenUV + offset);
                // Thicker middle tints more strongly than the thin rounded rim.
                half depthTint = lerp(.55, .9, nv);
                // Coloured filter: keep the background's brightness, take the glass's hue.
                half lum = dot(behind, half3(.3, .55, .15));
                half3 filtered = lerp(behind * tint * 1.6, lum * tint * 1.9, .55);
                half3 transmitted = lerp(behind, filtered, depthTint);
                half3 sky = SampleSH(n);
                half3 body = tint * (sky * .18 + sun.color * .1 * saturate(dot(n, l) * .5 + .5));
                // Backlit glow: looking towards the sun through the glass.
                half back = pow(saturate(dot(-v, l)), 3) * .8 + pow(saturate(dot(-v, l)), 24) * 1.3;
                half3 glow = tint * sun.color * back * sun.shadowAttenuation;
                half3 clean = transmitted * .92 + body + glow;
                // Edges seen at a glancing angle hold deeper colour (light travels further inside)...
                clean = lerp(clean, tint * tint * .7 + tint * .15, saturate(fresnel * 1.4) * .7);
                // ...with a thin bright rim where the rounded edge catches the light.
                clean += lerp(tint, half3(1, 1, 1), .6) * pow(fresnel, 5) * 1.2;

                // ---- Frosted glass: milky version of its colour, softly lit, with salt sparkle.
                half wrap = saturate(dot(n, l) * .5 + .5);
                half3 lightOnFrost = sky * .55 + sun.color * wrap * .75 * lerp(.55, 1, sun.shadowAttenuation);
                // Milky, translucent glass between patches of chalky salt crust.
                half3 milky = lerp(tint, _Frost.rgb, .45);
                // Even, fine-grained frost with a few soft, broad salt patches.
                float crust = smoothstep(.5, .75, Noise(q * .9 + 3.3));
                float grain = Noise(q * 40.0) * .5 + Noise(q * 90.0) * .5;
                half3 frostedGlass = lerp(behind * lerp(half3(1, 1, 1), tint, .7), milky * lightOnFrost, .78);
                frostedGlass *= .94 + grain * .12;
                half3 saltCrust = _Frost.rgb * lightOnFrost * (.95 + grain * .1);
                half3 frosted = lerp(frostedGlass, saltCrust, crust * .55);
                float sparkle = step(.975, Noise(i.local * 70)) * pow(wrap, 3);
                frosted += sparkle * sun.color * .4;
                // Sandy staining on top of the frost; it washes off first.
                half3 sandStain = half3(.74, .6, .44) * lightOnFrost * (.8 + grain * .35);
                float stainPatch = smoothstep(.35, .6, Noise(q * 1.6 + 11.0) * .7 + grain * .3);
                frosted = lerp(frosted, sandStain, dirt * lerp(.45, .9, stainPatch));

                half3 c = lerp(clean, frosted, grime);

                // Crisp highlights only on clean, polished glass.
                float3 h = normalize(l + v);
                half spec = pow(saturate(dot(n, h)), lerp(60, 400, _Smoothness)) * lerp(1.2, 4.0, _Smoothness);
                float3 r = reflect(-v, n);
                half window = smoothstep(.55, .6, r.y) * (1 - smoothstep(.78, .83, r.y)) * .35;   // a soft sky band
                c += (spec * sun.color * sun.shadowAttenuation + window * sky) * (1 - grime);

                c += _EmissionColor.rgb;
                c = MixFog(c, i.fog);
                return half4(c, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection; float3 _LightPosition;
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor, _EmissionColor, _Frost;
                float _Smoothness, _Grime, _Refraction;
            CBUFFER_END
            struct A { float4 p : POSITION; float3 n : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            float4 vert(A i) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 w = TransformObjectToWorld(i.p.xyz); float3 nw = TransformObjectToWorldNormal(i.n);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 ld = normalize(_LightPosition - w);
                #else
                    float3 ld = _LightDirection;
                #endif
                float4 c = TransformWorldToHClip(ApplyShadowBias(w, nw, ld));
                #if UNITY_REVERSED_Z
                    c.z = min(c.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    c.z = max(c.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return c;
            }
            half4 frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
