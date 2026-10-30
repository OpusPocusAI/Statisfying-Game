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
                float lead = 0, leadHi = 0;
                if (_Facets > 0)
                {
                    // Round 20 (reviews: "uniform crackle, no colour change per cell, flat lead"): larger cells with
                    // their own hue shift and streak direction, thinner came with a warm highlight along its crest.
                    float2 g = i.local.xy * .95; float2 cell = floor(g); float f1 = 9, f2 = 9; float2 id = 0;   // round 22: fewer, larger panes per region (reviews: "uniform crackle")
                    for (int oy = -1; oy <= 1; oy++) for (int ox = -1; ox <= 1; ox++)
                    {
                        float2 cc = cell + float2(ox, oy); float2 jit = float2(Hash(float3(cc, 1)), Hash(float3(cc, 7)));
                        float d = length(g - cc - jit);
                        if (d < f1) { f2 = f1; f1 = d; id = cc; } else if (d < f2) f2 = d;
                    }
                    float shade = Hash(float3(id, 3)), hueK = Hash(float3(id, 5)) - .5, ang = Hash(float3(id, 9)) * 6.283;
                    half3 cellTint = tint * lerp(.7, 1.3, shade);
                    cellTint = lerp(cellTint, cellTint.gbr, abs(hueK) * .22 * step(0, hueK)) ; cellTint = lerp(cellTint, cellTint.brg, abs(hueK) * .22 * step(hueK, 0));
                    float2 sd = float2(cos(ang), sin(ang)); float cs = Noise(float3(dot(i.local.xy, sd) * 22.0, dot(i.local.xy, sd.yx * float2(-1, 1)) * 2.0, shade * 9.0));
                    cellTint *= .9 + cs * .2;   // streaky hand-rolled glass, a different direction per pane
                    tint = lerp(tint, cellTint, _Facets);
                    float e = f2 - f1, w = fwidth(e) * 1.0 + .014;
                    lead = (1 - smoothstep(w, w + .022, e)) * _Facets;
                    leadHi = (1 - smoothstep(0, w * .5 + .003, abs(e - w * .35))) * lead;
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
                // Round 31 (reviews: "the grime ends in a straight line"): a ragged edge, so each swept stroke clears in a
                // broken, sandy outline instead of a ruler-straight one.
                brushed = saturate(brushed + (Noise(i.local * 22.0 + 3.0) - .5) * .32 * saturate(brushed * 3.0) * saturate((1 - brushed) * 3.0) * 2.0);
                float localGrime = _Grime * (1 - smoothstep(.35, .45, brushed) * .92);   // round 12: crisp brush-stroke edges
                float grime = smoothstep(order - .1, order + .1, localGrime);
                float dirt = smoothstep(order + .18, order + .4, localGrime); float dirtRim = 0;
                if (_MaskOn > .5)
                {
                    // Round 13, at the wash bowl: the brush path alone decides what is clean. The first pass
                    // takes the sand crust off, the next one the frost; both edges are hard (PowerWash).
                    // Anything the brush missed fades only over the last quarter of the clean.
                    float aa = max(fwidth(brushed) * 1.5, .08);   // round 27: softer (review 25: "dithered mask edge")   // round 21: soft brush-stroke edges (they read as hard blocks)
                    float rest = saturate(_Grime * 4);
                    dirt = (1 - smoothstep(.3 - aa, .3 + aa, brushed)) * rest;
                    grime = (1 - smoothstep(.68 - aa, .68 + aa, brushed)) * rest;
                    // Lifted grime gathers in a thin darker rim along the edge of each stroke.
                    dirtRim = 0;   // round 25: the lifted-grime line traced every stroke edge (reviews: "contour rings")
                }

                // ---- Clean glass: refracted, tinted background plus body colour.
                float2 screenUV = GetNormalizedScreenSpaceUV(i.p);
                float3 viewN = mul((float3x3)UNITY_MATRIX_V, n);
                float2 offset = viewN.xy * _Refraction * (2.4 + fresnel * 1.5);   // round 19: stronger, so the bend reads at normal viewing size
                half3 behind = SampleSceneColor(screenUV + offset);
                // Thicker middle tints more strongly than the thin rounded rim.
                // Round 7: thin rims and glancing edges let the background through almost untinted; only the
                // thick core filters strongly, so clean glass reads as glass rather than opaque candy.
                float thickT = saturate((abs(i.local.z) - .02) / .06);
                half depthTint = lerp(.3, .72, nv) * lerp(.5, 1, thickT);   // round 19: a little less face-on tint, so the bent background stays legible
                // Coloured filter: keep the background's brightness, take the glass's hue.
                half lum = dot(behind, half3(.3, .55, .15));
                // Round 18 (review 16: "milky pastel"): a deeper, more saturated filter colour; the background still
                // shows through, now strongly tinted.
                half3 deep = tint * tint * 1.35 + tint * .18;
                half3 filtered = lerp(behind * deep * 1.7, lum * deep * 2.1, .6);
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
                clean = lerp(clean * lerp(1.12, .9, thick), tint * tint * .9 + tint * .05, thick * .1);   // round 17: lighter, so the felt shows through
                // Edges seen at a glancing angle hold deeper colour (light travels further inside)...
                clean = lerp(clean, tint * tint * .85 + tint * .05, saturate(fresnel * 1.8) * .75);   // round 21: deeper, more saturated edges (light travels further there)
                // ...with a thin bright rim where the rounded edge catches the light.
                clean += lerp(tint, half3(1, 1, 1), .6) * pow(fresnel, 5) * 2.0;   // round 13: brighter rim
                // Round 5b: light gathered inside the piece; a soft inner glow so clean glass reads as lit
                // through, not as an opaque jelly.
                clean += tint * (.12 + .14 * (1 - thick)) * (1 - fresnel) * (sun.color * .6 + sky * .4);   // round 17: weaker, it flattened the see-through core
                // Round 14 (review 12): a darker, saturated core where the domed piece faces the viewer (light
                // crosses the most glass there) and a bright, thin transmitting band just inside the silhouette.
                // Based on the view angle, so it works for every pebble whatever its thickness in object units.
                float coreT = smoothstep(.35, .95, nv) * (1 - _Facets * .7);
                clean = lerp(clean, tint * tint * .72 + tint * .05, coreT * .0);   // round 21: no dark core; the centre stays the lightest, most see-through part   // round 17 (review 15: "opaque, no transmission")
                float rimBand = smoothstep(.6, .82, 1 - nv) * (1 - _Facets);
                clean += lerp(tint, half3(1, 1, 1), .55) * rimBand * .9 * (sky * .4 + sun.color * .6 + .25);
                // Round 6: sea glass is frosted even when clean. A light, fine satin haze over the polished
                // piece, thickest at the thin rim, keeps it from reading as glossy jelly.
                float satin = Noise(i.local * 55.0) * .5 + Noise(i.local * 120.0) * .5;
                clean = lerp(clean, lerp(tint, _Frost.rgb, .55) * (sky * .5 + sun.color * .45), (.06 + .06 * (1 - thick)) * (.75 + satin * .5));

                // Round 25 (review 23, all three: "gummy sweets", "light does not pass through", "no darker thick
                // core, no lighter thin rim"): loose pebbles get a physically based look instead of the layered
                // glows above, which mostly added light and read as pastel candy. The mosaic keeps its own path.
                if (_Facets < .5)
                {
                    // Path length through the piece: long through the domed middle, short at the tapered edge.
                    half path = lerp(.25, 1.0, smoothstep(.2, .92, nv)) * lerp(.75, 1.15, thickT);
                    // A gentler bend, so what lies behind stays legible (an edge, the felt, the bench).
                    half3 seen = SampleSceneColor(screenUV + viewN.xy * _Refraction * (1.1 + fresnel * 2.2));
                    // Beer-Lambert absorption: the hue of the glass, normalised so it keeps its brightness.
                    half3 hue = tint / max(max(tint.r, tint.g), max(tint.b, .05));
                    // Round 26 (review 24: the held piece "an opaque gummy block"): half the absorption, so the
                    // tray rim and felt stay legible through a piece held at arm's length.
                    half3 absorbed = seen * pow(saturate(lerp(1, hue, .6)), 2.0 * path) * 1.06;   // a softened hue: pure aqua has almost no red and blacked the background out
                    // A little scattered light inside, so the core glows its colour rather than going muddy.
                    half3 scatter = tint * (sky * .16 + sun.color * .1 * saturate(dot(n, l) * .5 + .5)) * path;
                    half3 glass = absorbed + scatter + glow * .6;
                    // Thin edges: bright and pale, catching the light that skims through the taper.
                    half thinRim = smoothstep(.74, .95, 1 - nv);   // round 26: the true silhouette only (it covered most of a tilted piece)
                    glass = lerp(glass, lerp(tint, half3(1, 1, 1), .55) * (sky * .45 + sun.color * .55 + .2), thinRim * .45);
                    // Reflection of the room at grazing angles.
                    glass += sky * fresnel * .18;
                    // Round 26: light entering at the lit top gathers inside the far, lower edge as a bright
                    // saturated crescent (how real glass and gems read as lit through, not painted).
                    float2 vn2 = viewN.xy;
                    half inner = pow(saturate(dot(vn2, normalize(float2(.45, -.6))) * 1.4), 2) * smoothstep(.12, .45, nv) * (1 - thinRim);
                    glass += hue * tint * inner * (sun.color * .45 + sky * .55 + .25) * .9;
                    clean = glass;
                }

                // ---- Frosted glass: milky version of its colour, softly lit, with salt sparkle.
                half wrap = saturate(dot(n, l) * .5 + .5);
                // Round 13: a floor on the light, so the crust reads white indoors rather than grey dough.
                half3 lightOnFrost = max(sky * .55 + sun.color * wrap * .75 * lerp(.55, 1, sun.shadowAttenuation), half3(.8, .79, .77));
                float crust = smoothstep(.5, .75, Noise(q * .9 + 3.3));
                float grain = Noise(q * 40.0) * .5 + Noise(q * 90.0) * .5;
                // Round 13: frosted sea glass is a matte white crust. The colour shows through only where the
                // glass is thin (the rounded rim) and as a faint tint in the crust; tiny pits break up the surface.
                half3 edgeGlass = lerp(behind * lerp(half3(1, 1, 1), tint, .75), tint * lightOnFrost * 1.15, .55);
                half3 crustColour = lerp(_Frost.rgb, tint * .75 + _Frost.rgb * .45, .5) * lightOnFrost * (.93 + grain * .1);   // round 27: frosted colour, not white (review 25: "a sand biscuit")
                float pits = step(.86, Noise(q * 26.0 + 5.0)) * .12;
                crustColour *= 1 - pits;
                float crustCover = lerp(.35, .78, smoothstep(.15, .7, nv));   // view-based; round 18: denser, so the cloudy piece stands out from the water
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
                half3 sandStain = half3(.58, .48, .33) * lightOnFrost * (.86 + grain * .22);   // round 23: a warm, sandy film (the dark bands read as shadow stripes)
                // Round 18: a soft dark edge where the crusted piece meets what it rests on, so it reads as a solid
                // pebble against pale water or felt.
                frosted *= 1 - smoothstep(.72, .95, 1 - nv) * .28;
                // Round 14 (review 12): thin brown streaks and a few small spots of dried sand over the frost,
                // not a solid coat.
                float streakN = Noise(float3(q.x * .45 + q.z * .3, q.y * 5.5, q.z * .45) + 11.0);
                float grimeStreaks = smoothstep(.6, .63, streakN) * (1 - smoothstep(.7, .73, streakN));
                // Round 16 (review 14: round spots read as "chocolate chips"): silt gathers at the edges instead.
                float edgeSilt = smoothstep(.55, .25, nv) * smoothstep(.42, .58, Noise(q * 2.2 + 2.0));
                float film = smoothstep(.46, .6, Noise(q * 1.3 + 7.0) * .75 + grain * .25);   // round 27: patches, not a full coat   // round 23: a broad mottled film with sandy speckle, over most of the piece
                float stainPatch = saturate(film * .9 + edgeSilt * .6);
                frosted = lerp(frosted, sandStain, dirt * stainPatch * .72);   // white frost between the patches
                frosted = lerp(frosted, sandStain * .85, dirtRim);   // round 21: lifted grime along the stroke edge

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
                // Round 14 (review 12): one large, crisp-edged highlight shape instead of glitter specks, with a hot
                // centre and a soft sheen around it.
                half aaK = max(fwidth(nk) * 1.5, .0008);
                half glint = smoothstep(.9965 - aaK, .9965 + aaK, nk) * .9 + pow(nk, 1800) * 1.4 + pow(nk, 140) * .16;   // round 28 (review 26: "one blown-out highlight blob covers a quarter of the face"): a small, sharp glint on the domed piece
                c += glint * lerp(half3(1, 1, 1), sun.color, .25) * (1 - grime) * (1 - _Facets * .85);
                // Round 16: the owner's glossy target names small flashing facets. A few crisp diamond-shaped
                // flashes on tumbled facets near the highlight, each winking as the view or time changes (round 14
                // removed the earlier speckle; these are fewer, larger and sharper).
                float3 fc = i.local * 26.0; float3 fcell = floor(fc); float fh = Hash(fcell + 31.0);
                float3 fd = abs(frac(fc) - .5);
                // Round 25 (reviews: "hard white squares"): a four-point star glint, thin arms and a hot centre.
                half diamond = saturate(1 - fd.x * fd.y * 110 - (fd.x + fd.y) * 1.9 - fd.z * .6);
                half wink = .45 + .55 * pow(saturate(sin(_Time.y * 2.5 + fh * 40.0 + dot(v, float3(9, 5, 7)) * 4.0)), 4);
                half facets = step(.965, fh) * diamond * wink * smoothstep(.7, .95, nk) * (1 - _Facets);
                c += facets * .75 * (1 - grime);   // round 28: smaller, rarer, dimmer, only near the glint (reviews: "stickers")

                c += _EmissionColor.rgb;
                c = lerp(c, half3(.02, .018, .022) + spec * .05, lead);   // linear: near-black came
                c += half3(.42, .3, .18) * leadHi * (sky * .5 + sun.color * .5 + .2) * .22;   // round 20: a warm highlight on the came
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
