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

            // Round 62 (reviews 39-58, all three: "smooth cut-out blobs", "smeared, puffy blobs, not shaped cumulus",
            // "stray cloud dots"; Tchia's clouds are built-up, lobed cumulus with lit tops and shaded undersides): the
            // clouds are no longer a thresholded noise field, which made blobs, smeared tails and lone specks. Each one is
            // a designed cumulus: a flat base and a domed top of overlapping rounded lobes, fused into one outline, and
            // each lobe shaded as a rounded volume against the sun, so every cloud shows its cauliflower build-up.
            float Hash1(float n) { return frac(sin(n * 127.1 + 311.7) * 43758.5453); }
            float SMin(float a, float b, float k) { float h = saturate(.5 + .5 * (b - a) / k); return lerp(b, a, h) - k * h * (1 - h); }

            // One cumulus. q: position from the middle of its base, in radians (x already scaled by cos(elevation));
            // W: half-width. Returns the cloud colour and its coverage.
            half4 Cumulus(float2 q, float W, float seed, float3 right, float3 upL, float3 back, float3 sunDir, float sunAlign)
            {
                float tower = Hash1(seed * 3.1), lean = (Hash1(seed * 2.3) - .5) * .5;   // how tall the middle builds up, and to which side
                float sdf = 1e3, zm = -1e3, sw = 0;
                float3 sn = 0;
                float soft = W * .05;   // how far the shading blends across the join of two lobes
                [unroll] for (int k = 0; k < 13; k++)
                {
                    float hk = Hash1(seed + k * 7.13), hk2 = Hash1(seed * 1.7 + k * 3.91);
                    float2 c; float r;
                    if (k < 7)
                    {
                        // The skirt: lobes of uneven size along the base, biggest in the middle.
                        float t = ((k + .5) / 7) * 2 - 1 + (hk - .5) * .2, dome = 1 - t * t;
                        r = W * (.15 + .32 * dome) * (.68 + .64 * hk2);
                        c = float2(t * W * .9, r * .4 + dome * W * .08);
                    }
                    else if (k >= 10)
                    {
                        // Small lobes along the upper contour: the cauliflower edge of a cumulus top.
                        float t = lean * .6 + (k - 11) * .42 + (hk - .5) * .25, dome = 1 - t * t;
                        r = W * (.1 + .06 * hk2);
                        c = float2(t * W, W * (.2 + .42 * tower * dome + .22 * dome));
                    }
                    else
                    {
                        // The build-up: lobes stacked over the middle, leaning to one side, taller on towering clouds.
                        float t = (k == 9 ? lean * 1.3 : lean + (k - 7.5) * .5) + (hk - .5) * .2;
                        r = W * (.2 + .16 * tower) * (.75 + .5 * hk2) * (k == 9 ? .8 : 1);
                        c = float2(t * W, W * (.27 + .34 * tower) * (k == 9 ? 1.45 : 1) + r * .1);
                    }
                    float2 dq = q - c; float dl = length(dq);
                    sdf = SMin(sdf, dl - r, W * .045);
                    // Height of this lobe's rounded front; the front-most lobe at each point is the one seen.
                    float z = dl < r ? sqrt(r * r - dl * dl) : r - dl;
                    float3 nk = normalize(float3(dq / r, sqrt(saturate(1 - dot(dq, dq) / (r * r)))));
                    // A running soft maximum over the lobes, so the shading never jumps from one lobe to the next.
                    if (z > zm) { float sc = exp((zm - z) / soft); sw = sw * sc + 1; sn = sn * sc + nk; zm = z; }
                    else { float wk = exp((z - zm) / soft); sw += wk; sn += nk * wk; }
                }
                // A flat, slightly rounded base.
                float base = (Hash1(seed * 5.3) - .5) * W * .05 * q.x / W;
                sdf = -SMin(-sdf, -(base - q.y), W * .035);
                // A little painterly unevenness on the outline, too small to make specks.
                sdf += (Noise(q / W * 5.5 + seed * 13.7) - .5) * W * .035;

                // Lobe shading: where two lobes meet, their normals blend and the join is softly creased.
                float3 n = normalize(sn);
                float crease = saturate((sw - 1) * .8) * step(0, zm);
                float baseK = 1 - smoothstep(0, W * .2, q.y);
                n = normalize(lerp(n, float3(0, -.85, .55), baseK * .75));   // the flat underside faces down
                float3 N = right * n.x + upL * n.y + back * n.z;
                float ndl = dot(N, sunDir);
                float lit = saturate(ndl * .5 + .55) * .65 + saturate(q.y / (W * .85)) * .35;
                lit *= 1 - crease * .18;
                // Three painted tones with soft steps: lilac underside, warm peach-rose middle, cream tops.
                half3 mid = lerp(_CloudShade.rgb, _CloudLit.rgb, .62) * half3(1.07, .9, .85);
                half3 col = lerp(_CloudShade.rgb, mid, smoothstep(.28, .38, lit));
                col = lerp(col, _CloudLit.rgb, smoothstep(.53, .63, lit));
                col *= .97 + .06 * Noise(q / W * 9 + seed * 3.3);
                // Toward the sun the clouds are backlit: light glows through their thin upper edges.
                float thin = (1 - saturate(-sdf / (W * .22))) * smoothstep(W * .1, W * .45, q.y);
                col = lerp(col, _CloudLit.rgb * half3(1.04, .9, .76), thin * pow(sunAlign, 2) * .6);
                // Bright rim around the outline, gold toward the sun.
                float edge = (1 - smoothstep(0, W * .06, -sdf)) * smoothstep(0, W * .15, q.y);
                col += _SunGlow.rgb * edge * (pow(sunAlign, 3) * 1.1 + .1);
                float fw = .0013;   // fixed width in radians: about one pixel, crisp but not aliased
                return half4(col, smoothstep(fw, -fw, sdf));
            }

            // A ring of cloud cells around the horizon; each cell holds one cumulus or clear sky.
            half4 CloudRow(float th, float e, float ce, float cells, float occ, float wMin, float wMax, float eMin, float eMax,
                           float rowSeed, float3 right, float3 upL, float3 back, float3 sunDir, float sunAlign)
            {
                float cw = 6.2831853 / cells, ci = floor(th / cw);
                half4 o = 0;
                [unroll] for (int j = -1; j <= 1; j++)
                {
                    float idx = ci + j, wrapped = idx - cells * floor(idx / cells);
                    float seed = wrapped * 17.31 + rowSeed;
                    if (Hash1(seed) < occ)
                    {
                        float W = lerp(wMin, wMax, Hash1(seed + .37)) * _CloudScale / 1.5;
                        float eb = lerp(eMin, eMax, Hash1(seed + .71));
                        float xc = (idx + .5 + (Hash1(seed + .93) - .5) * .5) * cw;
                        half4 cl = Cumulus(float2((th - xc) * ce, e - eb), W, seed, right, upL, back, sunDir, sunAlign);
                        o.rgb = lerp(o.rgb, cl.rgb, o.a > 0 ? cl.a : 1); o.a = max(o.a, cl.a);   // never blended toward black at an edge
                    }
                }
                return o;
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

                // Round 62: two rows of built cumulus around the horizon, clear overhead and clear of the dune crests
                // (round 18). The far row sits lower, smaller and a little hazed; the near row higher and larger.
                if (h > .09)
                {
                    float th = atan2(d.z, d.x), e = asin(saturate(h)), ce = cos(e);
                    float3 right = normalize(float3(-d.z, 0, d.x) + 1e-5), upL = normalize(float3(0, 1, 0) - d * h), back = -d;
                    half4 cFar = CloudRow(th, e, ce, 26, _CloudCover, .065, .11, .12, .16, 11.0, right, upL, back, sunDir, sunAlign);
                    half4 cNear = CloudRow(th + .9, e, ce, 18, _CloudCover * .85, .1, .16, .19, .25, 47.0, right, upL, back, sunDir, sunAlign);
                    c = lerp(c, lerp(cFar.rgb, c, .22), cFar.a * .96);
                    c = lerp(c, lerp(cNear.rgb, c, .06), cNear.a * .97);
                }

                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
