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
        _Bend("Bottle-wall bend of the mid-surface (mesh units, round 54)", Range(-.3, .3)) = 0
        _Held("Held up to the eye (set at runtime, round 56)", Float) = 0
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
                float4 _MaskRect; float _Facets, _MaskOn, _Bend, _Held;
            CBUFFER_END
            float4 _SG_Basin;   // round 39: basin water centre (xyz) and radius (w), set by the cleaning station
            float4 _SG_StrokeDir;   // round 92: the scrub direction in the clean mask's space (xy), set by the cleaning station

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
                // Round 54: the pieces are curved plates of bottle wall (round21.py), so the mid-surface bends with local x;
                // thickness is measured from it, not from the flat z = 0 plane.
                float lz = abs(i.local.z - _Bend * (1 - i.local.x * i.local.x * 4));
                // Round 61: 1 on the two near-flat faces of a loose piece, 0 on its edge wall (the mosaic panes count as all face).
                float faceF = lerp(1, smoothstep(.62, .95, abs(normalize(i.localN).z)), 1 - _Facets);
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
                float fromEdge = saturate((lz - .014) / .07);   // 0 at the rim, 1 in the thick middle
                float order = saturate(islands * .45 + streaks * .2 + (1 - fromEdge) * .45 - .05);
                // Each spot goes sandy dirt -> salt frost -> clear glass as grime falls past it.
                // Where the brush has passed, grime drops sharply (PowerWash-style visible strokes).
                float brushed = SAMPLE_TEXTURE2D(_CleanMask, sampler_CleanMask, (i.local.xy - _MaskRect.xy) / _MaskRect.zw).r;
                // Round 31 (reviews: "the grime ends in a straight line"): a ragged edge, so each swept stroke clears in a
                // broken, sandy outline instead of a ruler-straight one.
                // Round 65 (review 61: "hard pixel-noise edges"): a gentler, broader raggedness, so the bristle streaks in the mask carry the edge.
                brushed = saturate(brushed + (Noise(i.local * 11.0 + 3.0) - .5) * .16 * saturate(brushed * 3.0) * saturate((1 - brushed) * 3.0) * 2.0);
                float localGrime = _Grime * (1 - smoothstep(.35, .45, brushed) * .92);   // round 12: crisp brush-stroke edges
                float grime = smoothstep(order - .1, order + .1, localGrime);
                float dirt = smoothstep(order + .18, order + .4, localGrime); float dirtRim = 0;
                if (_MaskOn > .5)
                {
                    // Round 13, at the wash bowl: the brush path alone decides what is clean. The first pass
                    // takes the sand crust off, the next one the frost; both edges are hard (PowerWash).
                    // Anything the brush missed fades only over the last quarter of the clean.
                    float aa = max(fwidth(brushed) * 1.5, .05);   // round 40: a crisper cut through the crust   // round 27: softer (review 25: "dithered mask edge")   // round 21: soft brush-stroke edges (they read as hard blocks)
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
                float thickT = saturate((lz - .02) / .06);
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
                half3 glow = tint * sun.color * back;   // round 38: no shadow term (its shadow-map steps drew a ragged edge on the held piece)
                half3 clean = transmitted * .92 + body + glow;
                // Round 5: a deep, saturated core where the piece is thick, paler and clearer toward the thin
                // rounded rim (real glass), instead of one even candy colour.
                float thick = saturate((lz - .02) / .06);
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
                    // Round 44 (review 40, all three: "an even-thickness slab", against the 5.6 fixed form): the light path follows the
                    // wedge itself. The top surface sits higher where the piece is thicker, so its height above the mid-plane is
                    // the local thickness: deep colour along the thick edge, pale and clear toward the thin one.
                    half thickN = saturate((lz - .03) / .14);
                    half path = lerp(.18, 1.25, thickN) * lerp(.75, 1.0, smoothstep(.2, .92, nv));
                    // A gentler bend, so what lies behind stays legible (an edge, the felt, the bench).
                    // Round 37 (review 33, all three: "a dark inner band / seam" through the held piece): the bent image of
                    // the tray edge behind read as a seam inside the glass. A soft five-tap blur keeps the colours of what
                    // lies behind but not its hard edges, as tumbled glass with a faint inner haze does.
                    float2 suv = screenUV + viewN.xy * _Refraction * (2.0 + fresnel * 2.6);   // round 45: a stronger bend, so the background visibly shifts through the glass
                    float br = .0018 + .003 * fresnel;   // round 41 (review 37: "nothing shows through the held piece"): nearly sharp, so the felt and bench read through it   // round 38: a lighter blur, so the felt reads through the glass
                    half3 seen = SampleSceneColor(suv) * .28 + (SampleSceneColor(suv + float2(br, br * .6)) + SampleSceneColor(suv + float2(-br, br * .6))
                               + SampleSceneColor(suv + float2(br * .6, -br)) + SampleSceneColor(suv + float2(-br * .6, -br))) * .18;
                    // Beer-Lambert absorption: the hue of the glass, normalised so it keeps its brightness.
                    half3 hue = tint / max(max(tint.r, tint.g), max(tint.b, .05));
                    // Round 26 (review 24: the held piece "an opaque gummy block"): half the absorption, so the
                    // tray rim and felt stay legible through a piece held at arm's length.
                    half3 absorbed = seen * pow(saturate(lerp(1, hue, .45)), 1.15 * path) * 1.1;   // round 41: clearer still   // round 38 (review 34: "cannot see through it"): clearer   // a softened hue: pure aqua has almost no red and blacked the background out
                    // Round 42 (review 38, all three: "a muddy grey-teal"): the brown bench seen through teal glass mixed to grey.
                    // What shows through keeps its light and shade but takes the glass hue, as coloured glass does.
                    half seenL = dot(seen, half3(.3, .55, .15));
                    absorbed = lerp(absorbed, seenL * hue * lerp(1.25, .95, path), lerp(.5, .75, thickN));   // round 44: the thin edge shows what lies behind, lightly tinted (barely tinted read as grey-brown)
                    // A little scattered light inside, so the core glows its colour rather than going muddy.
                    half3 scatter = tint * (sky * .11 + sun.color * .07 * saturate(dot(n, l) * .5 + .5)) * path;
                    half3 glass = absorbed + scatter + glow * .6;
                    // Thin edges: bright and pale, catching the light that skims through the taper.
                    half thinRim = smoothstep(.74, .95, 1 - nv);   // round 26: the true silhouette only (it covered most of a tilted piece)
                    glass = lerp(glass, lerp(hue, half3(1, 1, 1), .3) * (sky * .45 + sun.color * .55 + .2), thinRim * .3);   // round 42: a rim in the glass colour; round 43: a thin line, not a glow (5.6 fixed form)
                    // Reflection of the room at grazing angles.
                    glass += sky * fresnel * .18;
                    // Round 26: light entering at the lit top gathers inside the far, lower edge as a bright
                    // saturated crescent (how real glass and gems read as lit through, not painted).
                    float2 vn2 = viewN.xy;
                    half inner = pow(saturate(dot(vn2, normalize(float2(.45, -.6))) * 1.4), 2) * smoothstep(.12, .45, nv) * (1 - thinRim);
                    glass += hue * tint * inner * (sun.color * .45 + sky * .55 + .25) * .9;
                    // Round 39 (review 35, all three: "a blob with one big highlight", "no bright edge, no sense of thickness"): a deeper,
                    // saturated core where the light crosses the most glass, and a luminous band just inside the rim where the edge
                    // gathers light, so the piece reads as a thick transmissive body with a lit edge (real glass and the glossy target).
                    half coreK = smoothstep(.45, .95, nv);
                    glass = lerp(glass, glass * (hue * .55 + .25) * .9, thickN * .32);   // round 44: deeper where the glass is thick (was view-based)
                    glass = lerp(glass, (seen * .5 + dot(seen, half3(.3, .55, .15)) * .5) * lerp(half3(1, 1, 1), hue, .55) * 1.12, (1 - thickN) * .25);   // and clearer at the thin edge
                    half litRim = smoothstep(.5, .78, 1 - nv) * (1 - smoothstep(.86, .97, 1 - nv));
                    glass += lerp(hue, half3(1, 1, 1), .15) * litRim * (sky * .5 + sun.color * .5 + .25) * .28;   // round 43: half the rim glow
                    clean = glass;
                    // Round 56 (review 52, all three: the held clean piece "an opaque lump", "an ice cube", "thick opaque jade"):
                    // the layered absorption above replaced what lies behind with its brightness in the glass hue, so nothing
                    // recognisable showed through a piece held up to the eye. Held, it is a coloured lens instead: the scene
                    // behind stays sharp, keeps its own light and dark and is only filtered by the hue (deeper where thick),
                    // bent more towards the curved rim, which reads as a darker refraction band and a thin bright edge.
                    if (_Held > .5)
                    {
                        // Round 57 (review 53, all three: the held piece "a bulbous, jelly-like lump"): a view-based rim band
                        // shaded it like a dome. A shard of bottle wall is a plate: two near-flat faces and a narrow edge wall.
                        // The mesh normal tells them apart. Through a face the scene shifts little and evenly (a pane); the
                        // edge wall, where light runs along the glass, glows a deep saturated hue (the green edge of a pane),
                        // with a thin bright lip where it meets the face, and the face carries one straight soft sheen.
                        float faceK = smoothstep(.62, .95, abs(normalize(i.localN).z));   // 1 on the faces, 0 on the edge wall; round 58: the rounded rim counts as edge too, so the thickness reads (review 54: "a thin sheet")
                        float2 huv = screenUV + viewN.xy * lerp(.07, .012, faceK);
                        half3 seenH = SampleSceneColor(huv);
                        half thickH = saturate((lz - .03) / .14);
                        half3 filt = lerp(half3(1, 1, 1), hue, lerp(.45, .92, thickH));   // round 58: clearer at the thin edge, deeper towards the thick one (the wedge)
                        half3 g = seenH * filt * lerp(1.15, .92, thickH);
                        // Round 72 (review 68, all three: the revealed basin piece "a mint jelly or soap bar", "a near-opaque mint gummy":
                        // one even tint, no clearer thin edge, little light through). Seen down into the pale basin, the same filter
                        // turned even pale water into even mint. At the basin the wedge grades from almost clear glass at its thin edge to
                        // a deep, saturated hue at its thick one, and the floor below shifts further through the thicker glass, so the
                        // caustic lines on the basin floor visibly kink and bend where they pass under the piece.
                        if (_MaskOn > .5)
                        {
                            half tB = smoothstep(.025, .085, lz);   /* round 74: the thinner hero basin shard (about 19% to 7% of the width) */
                            float2 buv = screenUV + viewN.xy * lerp(.07, .018, faceK) + float2(.012, .03) * tB * faceK;
                            half3 seenB = SampleSceneColor(buv) * half3(.7, .86, .82);   // the opaque pass has the enamel floor but not the basin water over it: the water tint
                            // Round 87 (review 83, all three: "a thick mint jelly lozenge", "a gummy sweet"): the basin water draws its
                            // bright caustic lines after the glass, and the glass hides it, so through the piece the floor was plain
                            // enamel and the piece filled with one even mint, like a solid sweet. The same caustic network
                            // (BasinWater.shader, in the bowl's unit-disc coordinates) is drawn here as seen through the glass, shifted
                            // further where the glass is thicker, and gathered a little closer (the bowl's lines are wider apart than the
                            // piece), so a few bright lines always run through the piece and visibly bend inside it.
                            float2 bp = (i.world.xz - _SG_Basin.xz) / max(_SG_Basin.w * 2, 1e-3) + viewN.xy * lerp(.012, .05, tB);
                            float2 cw = bp * 14 + float2(_Time.y * .22, -_Time.y * .16);
                            float ca = abs(sin(cw.x + sin(cw.y * 1.3 + _Time.y * .4) * 1.4) * sin(cw.y + sin(cw.x * 1.1 - _Time.y * .3) * 1.4));
                            seenB += pow(1 - ca, 8) * half3(1, .97, .86) * .55 * step(.001, _SG_Basin.w);
                            half3 filtB = lerp(lerp(half3(1, 1, 1), hue, .15), hue * hue * .9 + hue * .1, tB);
                            g = seenB * filtB * lerp(1.12, .78, tB);
                            g += tint * (sky * .05 + sun.color * .04) * tB;
                            seenH = seenB;
                        }
                        g = lerp(g, dot(g, half3(.3, .55, .15)) * hue * 1.15, _MaskOn > .5 ? .06 : .2);   /* round 72: at the basin the thin edge stays nearly clear */   // a little of the glass's own saturated colour
                        g += tint * (sky * .06 + sun.color * .04) * (.5 + thickH);
                        // Round 91 (review 87, all three: the palm piece "split into two flat tones", "two-tone like a gem", "a hard
                        // cyan/olive split"): held in the palm, the face showed the navy glove behind it, which the glass hue turned
                        // olive-brown, beside an edge wall glowing bright aqua, so one piece wore two unrelated colours. In the palm the
                        // face now carries the glass's own lit colour, deeper where thick, with the glove showing faintly through it,
                        // and the edge wall turns from it more gently.
                        if (_MaskOn < .5)
                        {
                            half3 bodyH = hue * (sky * .35 + sun.color * .3 + .3) * lerp(.85, .6, thickH);
                            g = lerp(g, bodyH + g * .35, .55);
                        }
                        half3 edgeCol = hue * hue * (dot(seenH, half3(.3, .55, .15)) * .55 + .2) * (sky * .35 + sun.color * .35 + .55);
                        // Round 87 (review 83, all three: the basin piece "a thick mint jelly lozenge", "a gummy sweet: a thick, soft,
                        // jelly rim"): its edge wall glowed pale, which is how a gummy's edge looks. The edge of a pane of bottle glass
                        // is dark and deeply saturated, because light runs a long way along it; at the basin it now is.
                        if (_MaskOn > .5) edgeCol = hue * hue * (dot(seenH, half3(.3, .55, .15)) * .3 + .05) * (sky * .3 + sun.color * .3 + .45);
                        g = lerp(g, edgeCol, (1 - faceK) * (_MaskOn > .5 ? .95 : .5));   // round 91: in the palm the edge wall turns from the face more gently
                        half lip = saturate(faceK * (1 - faceK) * 4);
                        if (_MaskOn > .5) lip = smoothstep(.12, .0, abs(faceK - .55)) * 1.6;   /* round 75: a thin, bright line where the face turns over the rim */
                        g += lerp(hue, half3(1, 1, 1), .6) * lip * .3 * (sky * .5 + sun.color * .5 + .3);
                        half sheen = (1 - smoothstep(.0, .09, abs(dot(i.local.xy, float2(.86, .5)) - .14))) * faceK;
                        g += (sky * .5 + sun.color * .3 + .15) * sheen * .1;
                        g += sky * fresnel * .12;
                        clean = g;
                    }
                }

                // ---- Frosted glass: milky version of its colour, softly lit, with salt sparkle.
                half wrap = saturate(dot(n, l) * .5 + .5);
                // Round 13: a floor on the light, so the crust reads white indoors rather than grey dough.
                half3 lightOnFrost = max(sky * .55 + sun.color * wrap * .75 * lerp(.55, 1, sun.shadowAttenuation), half3(.8, .79, .77));
                float crust = smoothstep(.5, .75, Noise(q * .9 + 3.3));
                // Round 66 (review 62, all three: the grime "dithered noise", "pixel-noise edges"): the grain was sampled up to ~360
                // cycles across the piece, finer than a pixel at the basin, so it aliased into speckle over the crust and along every
                // stroke front. Now a coarser, painterly grain that stays a few pixels across.
                float grain = Noise(q * 9.0) * .6 + Noise(q * 19.0) * .4;
                // Round 13: frosted sea glass is a matte white crust. The colour shows through only where the
                // glass is thin (the rounded rim) and as a faint tint in the crust; tiny pits break up the surface.
                half3 edgeGlass = lerp(behind * lerp(half3(1, 1, 1), tint, .9), tint * lightOnFrost * .95, .45);   // round 39: deeper colour under the frost
                half3 crustColour = lerp(_Frost.rgb, tint * .75 + _Frost.rgb * .4, .92) * lightOnFrost * (.95 + grain * .06);   // round 61 (review 57: "a stone", "a bread roll"): a milky frost in the glass colour, smoother (it was a grainy beige)   // round 27: frosted colour, not white (review 25: "a sand biscuit")
                float pits = smoothstep(.84, .9, Noise(q * 8.0 + 5.0)) * .08;   // round 66: broader, soft-edged pits (they aliased into speckle)
                crustColour *= 1 - pits;
                float crustCover = lerp(.2, .52, faceF) * (.8 + grain * .4);   // round 61 (review 57, all three: the cloudy piece "a big domed pebble", "a bread roll"): an even veil over the flat faces, thinner on the edge wall, instead of one graded by view angle (which shaded it like a dome)   // round 39 (review 35: "opaque soap / plaster"): a grainy veil, the colour shows through between the grains   // round 38 (review 34: cloudy piece "reads as soap or stone"): thinner frost, the colour shows through   // view-based; round 18: denser, so the cloudy piece stands out from the water
                half3 frostedGlass = lerp(edgeGlass, crustColour, crustCover);
                // Round 71 (review 67, all three: the cloudy basin piece "opaque grey camouflage patches over flat turquoise", "no
                // frosted translucency or colour showing through"): at the basin the frost is a milky, diffusing skin over glass you can
                // see into. What lies behind shows through as a broad, soft blur in the glass hue, deeper where the wedge is thick and
                // paler at the thin edge, under a pale veil of frost that lets the colour glow through rather than painting it flat.
                if (_MaskOn > .5)
                {
                    half3 hueF = tint / max(max(tint.r, tint.g), max(tint.b, .05));
                    half thickF = saturate((lz - .03) / .14);
                    float fb = .014;
                    half3 diffused = (SampleSceneColor(screenUV + float2(fb, fb * .6)) + SampleSceneColor(screenUV + float2(-fb, fb * .6))
                                    + SampleSceneColor(screenUV + float2(fb * .6, -fb)) + SampleSceneColor(screenUV + float2(-fb * .6, -fb))) * .25;
                    half dl = dot(diffused, half3(.3, .55, .15));
                    half3 through = lerp(diffused, dl * hueF * 1.15, lerp(.55, .9, thickF)) * lerp(1.15, .9, thickF);
                    through += tint * lightOnFrost * (.16 + .22 * thickF);   // light scattered inside the frosted body: the colour glows
                    half3 milk = lerp(_Frost.rgb, hueF * .62 + _Frost.rgb * .38, .7) * lightOnFrost;
                    half veil = lerp(.4, .66, faceF) * (.75 + grain * .45) * lerp(1.1, .78, thickF);
                    frostedGlass = lerp(through, milk, veil);
                    // Round 94 (review 90, two: "the dirty-to-clean contrast is mild", "the grime reads as streaks rather than crust"): the
                    // frosted body glowed nearly as saturated an aqua as the clean glass, so cleaning changed little. Weathered frost is dull: the
                    // colour stays as a muted hint under a greyer, chalkier skin, and the clean glass beneath reads as the jewel it becomes.
                    half fl = dot(frostedGlass, half3(.3, .55, .15));
                    // Round 99 (review 95, all three: the cloudy piece "grey-white with almost no colour under the grime", "a white shell"): the
                    // grey went too far. Frosted sea glass is a pale, matte pastel of its own colour, so the frost is now chalky but clearly tinted
                    // (a pastel of the hue, not neutral grey); the clean glass stays the deep, glossy version of the same colour.
                    half3 pastel = fl * lerp(half3(1.0, .98, .93), hueF, .55);
                    frostedGlass = lerp(frostedGlass, pastel, .55) * .94;
                }
                half3 saltCrust = _Frost.rgb * lightOnFrost * 1.04;
                half3 frosted = lerp(frostedGlass, saltCrust, crust * .2);   // round 61: fewer white salt patches
                float sparkle = smoothstep(.965, .985, Noise(i.local * 24)) * pow(wrap, 3);   // round 66: fewer, larger salt glints
                frosted += sparkle * sun.color * .4;
                // Round 11: even frosted glass shows its volume: a bright rounded rim and a slightly deeper,
                // more saturated edge band, so a cloudy piece never reads as a flat decal.
                frosted = lerp(frosted, tint * lightOnFrost * .8, (1 - faceF) * .6 + saturate(fresnel * 1.6) * .1);   // round 61: the glass hue shows along the edge wall (the broken edge reads as glass), not as a view-based rim
                frosted += lerp(tint, half3(1, 1, 1), .5) * pow(fresnel, 4) * .5 * (sky * .5 + sun.color * .5);   // round 61: half the bright rim
                // Sandy staining on top of the frost; it washes off first.
                // Round 5: only scattered patches of pale sand, so the piece reads as frosted glass, not sandpaper.
                // Round 5b: a dark olive-brown film of grime (PowerWash contrast: dirty must read clearly against
                // the vivid clean glass), broad and soft rather than speckled; it washes off first.
                // Round 13: broad, solid patches of dark dried sand over the white crust (PowerWash contrast);
                // solid, not speckled, with a faint sandy grain.
                half3 sandStain = half3(.44, .39, .31) * lightOnFrost * (.86 + grain * .22);   // round 23: a warm, sandy film (the dark bands read as shadow stripes)
                // Round 18: a soft dark edge where the crusted piece meets what it rests on, so it reads as a solid
                // pebble against pale water or felt.
                frosted *= 1 - (1 - faceF) * .12;   // round 61: a slight shade on the edge wall only; the view-based dark rim rounded it into a pebble
                // Round 14 (review 12): thin brown streaks and a few small spots of dried sand over the frost,
                // not a solid coat.
                float streakN = Noise(float3(q.x * .45 + q.z * .3, q.y * 5.5, q.z * .45) + 11.0);
                float grimeStreaks = smoothstep(.6, .63, streakN) * (1 - smoothstep(.7, .73, streakN));
                // Round 16 (review 14: round spots read as "chocolate chips"): silt gathers at the edges instead.
                float edgeSilt = smoothstep(.55, .25, nv) * smoothstep(.42, .58, Noise(q * 2.2 + 2.0));
                float film = smoothstep(.36, .62, Noise(q * 1.3 + 7.0) * .5 + Noise(float3(q.x * 2.1 + q.z * 1.3, q.y * 2.4, q.z * 2.1 - q.x * 1.3) + 2.7) * .3 + grain * .2);   // round 39 (review 35: "a straight grime seam"): two rotated octaves, a broader soft-edged film   // round 27: patches, not a full coat   // round 23: a broad mottled film with sandy speckle, over most of the piece
                // Round 40 (review 36, all three: "grime should be an opaque brown crust each stroke cuts through, as in PowerWash"): the
                // dried silt covers nearly the whole piece as a crust, mottled and lighter on its raised patches, broken by fine
                // dark cracks; the frost only shows through in a few thin places.
                float stainPatch = smoothstep(.42, .62, film + edgeSilt * .3);   // round 61 (review 57: the cloudy piece "a stone or bread roll"): patches of dried sand with frosted glass between them, not a near-solid coat   // round 54: +.12, the gentler curved plate shows less edge silt   // round 44 (review 40, all three: "a sandy biscuit, no colour"): grime in patches, the frosted hue between them
                float crackN = Noise(q * 5.5 + 13.0);
                float cracks = (1 - smoothstep(.0, .018, abs(crackN - .5))) * smoothstep(.3, .7, Noise(q * 1.7 + 4.0));
                sandStain *= lerp(.82, 1.1, film);   // round 41: no crack lines (review 37: "a jigsaw pattern")
                // Round 71: at the basin the dried sand is a warm, pale, slightly see-through film with soft edges, not opaque grey
                // camouflage over the glass; the stroke still cuts through it cleanly (the mask edge is unchanged).
                if (_MaskOn > .5) { sandStain = lerp(sandStain, half3(.66, .58, .45) * lightOnFrost, .3); stainPatch = smoothstep(.1, .85, film * .8 + edgeSilt * .5 + grain * .15) * .8; /* a graded dusting, heavier toward the edges, no hard-edged islands */ /* round 89 (review 85, all three: "weak dirty-to-clean contrast", "faint mottling"): a browner, fuller coat of dried sand, so the clean strokes stand out */
                    // Round 92 (review 88, all three: the cloudy piece "camouflage blotches", grime coming off "in patches, not brush strokes"): the
                    // round blotches of sand left bright aqua holes that read as places already cleaned, so the strokes were lost among them.
                    // The coat is now even, a little thicker toward the edges, and its only variation is a grain of fine streaks running the way
                    // the brush scrubs, so the crust reads as one dried layer and each pass cuts a clean lane along its own grain.
                    float2 muv = (i.local.xy - _MaskRect.xy) / _MaskRect.zw;
                    float2 sd = dot(_SG_StrokeDir.xy, _SG_StrokeDir.xy) > .01 ? normalize(_SG_StrokeDir.xy) : float2(1, 0);
                    float alongS = dot(muv, sd), acrossS = dot(muv, float2(-sd.y, sd.x));
                    float grainS = Noise(float3(alongS * 3.0, acrossS * 55.0, 3.1)) * .6 + Noise(float3(alongS * 7.0, acrossS * 120.0, 8.7)) * .4;
                    // Round 93 (review 89, all three: the even coat read as "a beige stone with no colour through it"): a thinner dusting, so the
                    // frosted glass and its colour glow through between the grains of sand; the streaks still run with the brush.
                    stainPatch = saturate(.52 + (grainS - .5) * .45 + (film - .5) * .25 + edgeSilt * .15);   // strong streaks read as wood grain; a faint broad drift breaks them
                    sandStain = half3(.44, .37, .28) * lightOnFrost * lerp(.66, .98, grainS); }   // a darker, warmer coat: under the teal water a paler one read grey-beige, barely apart from the clean glass
                frosted = lerp(frosted, sandStain, dirt * stainPatch * .97);   // round 40: an opaque crust   // round 39: browner, darker silt (review 35: "dirty and clean look alike")   // round 37: darker silt (review 33: "weak dirty-to-clean contrast")   // white frost between the patches
                frosted = lerp(frosted, sandStain * .85, dirtRim);   // round 21: lifted grime along the stroke edge

                // Round 94: at the basin the clean glass under the water read pale mint, close to the frost; a richer colour where it is clean.
                if (_MaskOn > .5) { half cl = dot(clean, half3(.3, .55, .15)); clean = max(lerp(cl.xxx, clean, 1.3), 0); }
                half3 c = lerp(clean, frosted, grime);

                // Crisp highlights only on clean, polished glass.
                // Round 43 (5.6 fixed form): faint micro-pitting on the glossy skin, so each glint breaks into a few small flashes.
                float3 pit = float3(Noise(i.local * 70.0), Noise(i.local * 70.0 + 5.3), Noise(i.local * 70.0 + 11.7)) - .5;
                float3 ng = normalize(n + pit * .17 * (1 - _Held) * (1 - _Facets));   // round 55 (review 51: "one smeared glint"): stronger pitting, so the glint breaks into a few flashes
                float3 h = normalize(l + v);
                half spec = pow(saturate(dot(ng, h)), lerp(120, 900, _Smoothness)) * lerp(1.5, 6.0, _Smoothness);   // small, sharp sun glint
                float3 r = reflect(-v, n);
                half window = 0;   // round 12: a sky band drew a bullseye ring on domed pebbles; specular only
                c += (spec * sun.color * sun.shadowAttenuation + window * sky) * (1 - grime);
                // Round 13: a studio key light from over the viewer's shoulder, so every clean piece shows one
                // crisp white glint and a soft sheen wherever it is seen from, plus tiny facets that flash.
                float3 keyL = normalize(mul((float3x3)UNITY_MATRIX_I_V, float3(-.45, .55, .7)));
                half nk = saturate(dot(ng, normalize(keyL + v)));
                // Round 14 (review 12): one large, crisp-edged highlight shape instead of glitter specks, with a hot
                // centre and a soft sheen around it.
                half aaK = max(fwidth(nk) * 1.5, .0008);
                half glint = smoothstep(.9985 + _Held * .0011 + _MaskOn * .00025 - aaK, .9985 + _Held * .0011 + _MaskOn * .00025 + aaK, nk) * .7 + pow(nk, 3600 + _MaskOn * 6000) * .9 + pow(nk, 160) * .05 * (1 - _MaskOn);   /* round 75: at the basin a smaller, harder key glint and no soft sheen around it */   // round 39: smaller again   // round 38 (review 34: "one big white smear"): smaller   // round 28 (review 26: "one blown-out highlight blob covers a quarter of the face"): a small, sharp glint on the domed piece
                c += glint * lerp(half3(1, 1, 1), sun.color, .25) * (1 - grime) * (1 - _Facets * .85);
                // Round 38 (review 34: "several small sharp glints along the edges, not one big highlight"): two more small
                // glints from side lights, only on the rounded rim, where tumbled glass catches light as it turns.
                float3 k2 = normalize(mul((float3x3)UNITY_MATRIX_I_V, float3(.78, -.2, .35)));
                float3 k3 = normalize(mul((float3x3)UNITY_MATRIX_I_V, float3(-.72, -.45, .4)));
                half n2 = saturate(dot(ng, normalize(k2 + v))), n3 = saturate(dot(ng, normalize(k3 + v)));
                half a2 = max(fwidth(n2) * 1.5, .0008), a3 = max(fwidth(n3) * 1.5, .0008);
                half edgeG = (smoothstep(.994 - a2, .994 + a2, n2) + smoothstep(.994 - a3, .994 + a3, n3) * .8) * smoothstep(.2, .55, 1 - nv);
                // Round 39 (review 35: "the rim glints do not show from the held view"): two more small glints from lights in
                // front of the viewer, so the worn dents on the face catch several small flashes whatever the angle.
                float3 k4 = normalize(mul((float3x3)UNITY_MATRIX_I_V, float3(.4, .5, .77))), k5 = normalize(mul((float3x3)UNITY_MATRIX_I_V, float3(.15, -.35, .92)));
                half n4 = saturate(dot(ng, normalize(k4 + v))), n5 = saturate(dot(ng, normalize(k5 + v)));
                half a4 = max(fwidth(n4) * 1.5, .0008), a5 = max(fwidth(n5) * 1.5, .0008);
                edgeG += smoothstep(.9975 + _Held * .0019 - a4, .9975 + _Held * .0019 + a4, n4) * .8 + smoothstep(.998 + _Held * .0015 - a5, .998 + _Held * .0015 + a5, n5) * .6;   // round 56: smaller on the flat face of the held piece
                c += edgeG * .95 * (1 - grime) * (1 - _Facets);
                // Round 16: the owner's glossy target names small flashing facets. A few crisp diamond-shaped
                // flashes on tumbled facets near the highlight, each winking as the view or time changes (round 14
                // removed the earlier speckle; these are fewer, larger and sharper).
                float3 fc = i.local * 26.0; float3 fcell = floor(fc); float fh = Hash(fcell + 31.0);
                float3 fd = abs(frac(fc) - .5);
                // Round 25 (reviews: "hard white squares"): a four-point star glint, thin arms and a hot centre.
                half diamond = saturate(1 - fd.x * fd.y * 110 - (fd.x + fd.y) * 1.9 - fd.z * .6);
                half wink = .45 + .55 * pow(saturate(sin(_Time.y * 2.5 + fh * 40.0 + dot(v, float3(9, 5, 7)) * 4.0)), 4);
                half facets = step(.965, fh) * diamond * wink * smoothstep(.7, .95, nk) * (1 - _Facets);
                // Round 75 (review 71, all three: the basin piece's highlights "blobby", "blobby highlight dimples"): at the basin a
                // finer scatter of small, sharp four-point flashes over the lit face stands in for the face pits, as on worn glass.
                if (_MaskOn > .5) { float3 bc = i.local * 44.0; float bh = Hash(floor(bc) + 57.0); float3 bd = abs(frac(bc) - .5);
                    half star = saturate(1 - bd.x * bd.y * 160 - (bd.x + bd.y) * 2.6 - bd.z * .8);
                    facets = max(facets, step(.93, bh) * star * wink * smoothstep(.55, .9, nk) * smoothstep(.6, .9, faceF)); }
                c += facets * .75 * (1 - grime);   // round 28: smaller, rarer, dimmer, only near the glint (reviews: "stickers")

                c += _EmissionColor.rgb;
                c = lerp(c, half3(.02, .018, .022) + spec * .05, lead);   // linear: near-black came
                c += half3(.42, .3, .18) * leadHi * (sky * .5 + sun.color * .5 + .2) * .22;   // round 20: a warm highlight on the came
                // Round 39: a bright waterline where the piece crosses the basin surface, and the glass just under it a touch deeper.
                float wlD = i.world.y - _SG_Basin.y;
                half inBasin = step(.001, _SG_Basin.w) * step(length(i.world.xz - _SG_Basin.xz), _SG_Basin.w);
                half wl = (1 - smoothstep(.0012, .0045, abs(wlD))) * inBasin * (1 - _Facets);
                c = lerp(c, c * half3(.72, .9, .92), (1 - smoothstep(-.012, -.002, wlD)) * smoothstep(-.03, -.012, wlD) * inBasin * .5);
                c = lerp(c, half3(.95, 1, 1), wl * .8);
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
                float4 _MaskRect; float _Facets, _MaskOn, _Bend, _Held;
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
