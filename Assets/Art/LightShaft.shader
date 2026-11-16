// Soft shafts of daylight through the studio's ocean-wall windows (owner playtest, Nov 16: "maybe add some light rays").
// Each shaft is the slanted box swept by a four-pane window along the daylight direction (PlaytestFixesPass). Its inside
// faces are drawn; the fragment marches the camera ray through the box in the box's own coordinates (a across the window,
// b from sill to head, c from the window to the floor), so the haze is soft from every angle with no card edges, shows the
// mullion and transom as darker lanes, fades in from the glass and out toward the floor, and stops at whatever is in front
// (scene depth). The box's inverse matrix rides in the mesh's UV2..UV4 (constant per shaft), so one material serves all.
Shader "Sea Glass/Light Shaft"
{
    Properties
    {
        _Colour("Light colour", Color) = (1, .88, .66, 1)
        _Density("Haze per metre", Range(0, 1)) = .28
        _Bars("Mullion a, transom b, bar half-width a, b", Vector) = (.5, .53, .02, .035)
        _Soft("Edge softness", Range(.005, .3)) = .09
        _Forward("Brighter looking toward the light", Range(0, 3)) = 1.2
        _LightDir("Daylight direction (into the room)", Vector) = (.7, -.7, 0, 0)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-10" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            Blend One One
            ZWrite Off
            ZTest Always
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial) half4 _Colour; half _Density; float4 _Bars; half _Soft; half _Forward; float4 _LightDir; CBUFFER_END
            struct A { float4 p : POSITION; float4 r0 : TEXCOORD2; float4 r1 : TEXCOORD3; float4 r2 : TEXCOORD4; };
            struct V { float4 p : SV_POSITION; float3 w : TEXCOORD0; float4 r0 : TEXCOORD1; float4 r1 : TEXCOORD2; float4 r2 : TEXCOORD3; float4 s : TEXCOORD4; };
            V vert(A i)
            {
                V o; o.w = TransformObjectToWorld(i.p.xyz); o.p = TransformWorldToHClip(o.w);
                o.r0 = i.r0; o.r1 = i.r1; o.r2 = i.r2; o.s = ComputeScreenPos(o.p); return o;
            }
            float3 Box(V i, float3 p) { return float3(dot(i.r0.xyz, p) + i.r0.w, dot(i.r1.xyz, p) + i.r1.w, dot(i.r2.xyz, p) + i.r2.w); }
            half Band(float x, float lo, float hi, float s) { return smoothstep(lo - s, lo + s, x) * (1 - smoothstep(hi - s, hi + s, x)); }
            half Haze(float3 q)
            {
                half s = _Soft;
                half frame = Band(q.x, 0, 1, s) * Band(q.y, 0, 1, s);
                half lanes = (1 - .75 * Band(q.x, _Bars.x - _Bars.z, _Bars.x + _Bars.z, s * .4)) * (1 - .75 * Band(q.y, _Bars.y - _Bars.w, _Bars.y + _Bars.w, s * .4));
                half along = smoothstep(0, .12, q.z) * (1 - smoothstep(.55, 1, q.z));   // fades in from the glass, out near the floor
                return frame * lanes * along;
            }
            half4 frag(V i) : SV_Target
            {
                float3 cam = GetCameraPositionWS(), far = i.w;
                float3 c0 = Box(i, cam), c1 = Box(i, far), d = c1 - c0;
                // Where the ray enters the box (slab test in box space); 0 if the camera is inside.
                float3 inv = 1 / (abs(d) > 1e-5 ? d : 1e-5);
                float3 t0 = (0 - c0) * inv, t1 = (1 - c0) * inv;
                float enter = saturate(max(max(min(t0.x, t1.x), min(t0.y, t1.y)), min(t0.z, t1.z)));
                // Stop at the scene: eye depth scales linearly along the ray.
                float2 uv = i.s.xy / i.s.w;
                float scene = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float farEye = -TransformWorldToView(far).z;
                float leave = saturate(scene / max(farEye, 1e-4));
                if (leave <= enter) return 0;
                // March, jittered per pixel so the steps don't band.
                const int N = 10;
                float jitter = frac(52.9829189 * frac(dot(i.p.xy, float2(.06711056, .00583715))));
                float len = length(far - cam) * (leave - enter), sum = 0;
                [unroll] for (int k = 0; k < N; k++)
                {
                    float s = lerp(enter, leave, (k + jitter) / N);
                    sum += Haze(c0 + d * s);
                }
                float3 view = normalize(far - cam);
                half forward = 1 + _Forward * pow(saturate(dot(view, -normalize(_LightDir.xyz))), 6);   // looking back up the shaft toward the window
                return half4(_Colour.rgb * _Density * sum / N * len * forward, 0);
            }
            ENDHLSL
        }
    }
}
