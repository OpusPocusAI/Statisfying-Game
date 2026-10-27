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
        [NoScaleOffset] _CleanMask("Brushed mask (set at runtime)", 2D) = "black" {}
        _MaskRect("Mask rect (min xy, size xy)", Vector) = (0, 0, 1, 1)
        _Facets("Stained-glass fragments (mosaic)", Range(0, 1)) = 0
        _MaskOn("Brush mask drives the layers (set at runtime)", Float) = 0
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
            TEXTURE2D(_CleanMask); SAMPLER(sampler_CleanMask);

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
                float4 _MaskRect; float _Facets, _MaskOn;
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
                // Hand-made glass is never one flat colour: soft density streaks and a few tiny seeds.
                float streak = Noise(i.local * float3(2.2, 7.5, 2.2) + 4.1);
                tint *= .84 + streak * .3;
                float seeds = step(.982, Noise(i.local * 34.0 + 9.3));
                tint = lerp(tint, tint * 1.7 + .12, seeds * .6);   // tiny trapped bubbles
                // Mosaic pieces read as several smaller fragments (Glass Masquerade): Voronoi cells in object
                // space, each with its own shade of the colour, separated by fine lead lines.
                float lead = 0;
                if (_Facets > 0)
                {
                    float2 g = i.local.xy * 2.6; float2 cell = floor(g); float f1 = 9, f2 = 9; float2 id = 0;
                    for (int oy = -1; oy <= 1; oy++) for (int ox = -1; ox <= 1; ox++)
                    {
                        float2 cc = cell + float2(ox, oy); float2 jit = float2(Hash(float3(cc, 1)), Hash(float3(cc, 7)));
                        float d = length(g - cc - jit);
                        if (d < f1) { f2 = f1; f1 = d; id = cc; } else if (d < f2) f2 = d;
                    }
                    float shade = Hash(float3(id, 3));
                    tint = lerp(tint, tint * lerp(.72, 1.3, shade) + (shade - .5) * .06, _Facets);
                    float w = fwidth(f2 - f1) * 1.0 + .022;
                    lead = (1 - smoothstep(w, w + .03, f2 - f1)) * _Facets;
                }
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
                // Where the brush has passed, grime drops sharply (PowerWash-style visible strokes).
                float brushed = SAMPLE_TEXTURE2D(_CleanMask, sampler_CleanMask, (i.local.xy - _MaskRect.xy) / _MaskRect.zw).r;
                float localGrime = _Grime * (1 - smoothstep(.35, .45, brushed) * .92);   // round 12: crisp brush-stroke edges
                float grime = smoothstep(order - .1, order + .1, localGrime);
                float dirt = smoothstep(order + .18, order + .4, localGrime);
                if (_MaskOn > .5)
                {
                    // Round 13, at the wash bowl: the brush path alone decides what is clean. The first pass
                    // takes the sand crust off, the next one the frost; both edges are hard (PowerWash).
                    // Anything the brush missed fades only over the last quarter of the clean.
                    float aa = max(fwidth(brushed) * .75, .015);
                    float rest = saturate(_Grime * 4);
                    dirt = (1 - smoothstep(.3 - aa, .3 + aa, brushed)) * rest;
                    grime = (1 - smoothstep(.68 - aa, .68 + aa, brushed)) * rest;
                }

                // ---- Clean glass: refracted, tinted background plus body colour.
                float2 screenUV = GetNormalizedScreenSpaceUV(i.p);
                float3 viewN = mul((float3x3)UNITY_MATRIX_V, n);
                float2 offset = viewN.xy * _Refraction * (.6 + fresnel);
                half3 behind = SampleSceneColor(screenUV + offset);
                // Thicker middle tints more strongly than the thin rounded rim.
                // Round 7: thin rims and glancing edges let the background through almost untinted; only the
                // thick core filters strongly, so clean glass reads as glass rather than opaque candy.
                float thickT = saturate((abs(i.local.z) - .02) / .06);
                half depthTint = lerp(.3, .85, nv) * lerp(.5, 1, thickT);
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
                // Round 5: a deep, saturated core where the piece is thick, paler and clearer toward the thin
                // rounded rim (real glass), instead of one even candy colour.
                float thick = saturate((abs(i.local.z) - .02) / .06);
                clean = lerp(clean * lerp(1.12, .78, thick), tint * tint * .9 + tint * .05, thick * .35);
                // Edges seen at a glancing angle hold deeper colour (light travels further inside)...
                clean = lerp(clean, tint * tint * .7 + tint * .15, saturate(fresnel * 1.4) * .6);
                // ...with a thin bright rim where the rounded edge catches the light.
                clean += lerp(tint, half3(1, 1, 1), .6) * pow(fresnel, 5) * 2.0;   // round 13: brighter rim
                // Round 5b: light gathered inside the piece; a soft inner glow so clean glass reads as lit
                // through, not as an opaque jelly.
                clean += tint * (.3 + .35 * (1 - thick)) * (1 - fresnel) * (sun.color * .6 + sky * .4);
                // Round 6: sea glass is frosted even when clean. A light, fine satin haze over the polished
                // piece, thickest at the thin rim, keeps it from reading as glossy jelly.
                float satin = Noise(i.local * 55.0) * .5 + Noise(i.local * 120.0) * .5;
                clean = lerp(clean, lerp(tint, _Frost.rgb, .55) * (sky * .5 + sun.color * .45), (.06 + .06 * (1 - thick)) * (.75 + satin * .5));

                // ---- Frosted glass: milky version of its colour, softly lit, with salt sparkle.
                half wrap = saturate(dot(n, l) * .5 + .5);
                // Round 13: a floor on the light, so the crust reads white indoors rather than grey dough.
                half3 lightOnFrost = max(sky * .55 + sun.color * wrap * .75 * lerp(.55, 1, sun.shadowAttenuation), half3(.74, .73, .71));
                float crust = smoothstep(.5, .75, Noise(q * .9 + 3.3));
                float grain = Noise(q * 40.0) * .5 + Noise(q * 90.0) * .5;
                // Round 13: frosted sea glass is a matte white crust. The colour shows through only where the
                // glass is thin (the rounded rim) and as a faint tint in the crust; tiny pits break up the surface.
                half3 edgeGlass = lerp(behind * lerp(half3(1, 1, 1), tint, .75), tint * lightOnFrost * 1.15, .55);
                half3 crustColour = lerp(_Frost.rgb, tint * .6 + _Frost.rgb * .55, .16) * lightOnFrost * (.93 + grain * .1);
                float pits = step(.86, Noise(q * 26.0 + 5.0)) * .12;
                crustColour *= 1 - pits;
                float crustCover = lerp(.42, .94, thick);
                half3 frostedGlass = lerp(edgeGlass, crustColour, crustCover);
                half3 saltCrust = _Frost.rgb * lightOnFrost * 1.04;
                half3 frosted = lerp(frostedGlass, saltCrust, crust * .4);
                float sparkle = step(.975, Noise(i.local * 70)) * pow(wrap, 3);
                frosted += sparkle * sun.color * .4;
                // Round 11: even frosted glass shows its volume: a bright rounded rim and a slightly deeper,
                // more saturated edge band, so a cloudy piece never reads as a flat decal.
                frosted = lerp(frosted, tint * lightOnFrost * .8, saturate(fresnel * 1.6) * .45);
                frosted += lerp(tint, half3(1, 1, 1), .5) * pow(fresnel, 4) * .9 * (sky * .5 + sun.color * .5);
                // Sandy staining on top of the frost; it washes off first.
                // Round 5: only scattered patches of pale sand, so the piece reads as frosted glass, not sandpaper.
                // Round 5b: a dark olive-brown film of grime (PowerWash contrast: dirty must read clearly against
                // the vivid clean glass), broad and soft rather than speckled; it washes off first.
                // Round 13: broad, solid patches of dark dried sand over the white crust (PowerWash contrast);
                // solid, not speckled, with a faint sandy grain.
                half3 sandStain = half3(.31, .25, .16) * lightOnFrost * (.9 + grain * .16);
                float stainPatch = smoothstep(.5, .55, Noise(q * 1.1 + 11.0) * .9 + grain * .1);   // about half the piece
                frosted = lerp(frosted, sandStain, dirt * stainPatch * .92);   // white frost between the patches

                half3 c = lerp(clean, frosted, grime);

                // Crisp highlights only on clean, polished glass.
                float3 h = normalize(l + v);
                half spec = pow(saturate(dot(n, h)), lerp(120, 900, _Smoothness)) * lerp(1.5, 6.0, _Smoothness);   // small, sharp sun glint
                float3 r = reflect(-v, n);
                half window = 0;   // round 12: a sky band drew a bullseye ring on domed pebbles; specular only
                c += (spec * sun.color * sun.shadowAttenuation + window * sky) * (1 - grime);
                // Round 13: a studio key light from over the viewer's shoulder, so every clean piece shows one
                // crisp white glint and a soft sheen wherever it is seen from, plus tiny facets that flash.
                float3 keyL = normalize(mul((float3x3)UNITY_MATRIX_I_V, float3(-.45, .55, .7)));
                half nk = saturate(dot(n, normalize(keyL + v)));
                half glint = pow(nk, 700) * 4.0 + pow(nk, 80) * .3;
                float facet = Hash(floor(i.local * 70.0) + 17.0);
                half flash = step(.985, facet) * pow(saturate(sin(_Time.y * 4.0 + facet * 60.0 + dot(v, float3(13, 7, 11)) * 3.0)), 6);
                c += (glint * lerp(half3(1, 1, 1), sun.color, .25) + flash * 2.5 * (1 - _Facets)) * (1 - grime) * (1 - _Facets * .85);

                c += _EmissionColor.rgb;
                c = lerp(c, half3(.012, .012, .016) + spec * .05, lead);   // linear: near-black came
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
                float4 _MaskRect; float _Facets, _MaskOn;
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
