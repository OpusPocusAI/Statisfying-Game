// Round 68 (review 64, all three: the first-person sleeve "a plain tube", the open palm "a flat, stiff mitten"; the folds and
// knuckles were modelled but did not read under flat, single-tone shading). Cloth for the first-person gloves and sleeves:
// - a stepped light-to-shade ramp toward a cool shade colour, so every fold, crease and knuckle reads as a band of shade;
// - a woven surface mapped in the object's own space (it stays on the moving arm): canvas, denim twill, knit rib or rubber
//   coating, faded out where it would be finer than a pixel;
// - a soft warm rim on the edges facing away from the camera.
// The meshes have no UVs; the weave is projected from the three object axes and blended by the surface normal.
Shader "Sea Glass/Cloth"
{
    Properties
    {
        _BaseColor("Colour", Color) = (1, 1, 1, 1)
        _ShadeColor("Shade colour (multiplies the colour)", Color) = (.52, .48, .66, 1)
        _Weave("Weave (0 canvas, 1 twill, 2 knit rib, 3 rubber)", Float) = 0
        _WeaveScale("Weave threads per metre", Float) = 240
        _WeaveStrength("Weave strength", Range(0, .5)) = .12
        _Rim("Rim light", Range(0, 1)) = .3
        _Sheen("Sheen (rubber)", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor, _ShadeColor;
            float _Weave, _WeaveScale, _WeaveStrength, _Rim, _Sheen;
        CBUFFER_END
        ENDHLSL
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A { float4 p : POSITION; float3 n : NORMAL; };
            struct V { float4 p : SV_POSITION; float3 os : TEXCOORD0; float3 on : TEXCOORD1; float3 wn : TEXCOORD2; float3 wp : TEXCOORD3; };
            V vert(A i)
            {
                V o; o.wp = TransformObjectToWorld(i.p.xyz); o.p = TransformWorldToHClip(o.wp);
                o.os = i.p.xyz * length(TransformObjectToWorldDir(float3(1, 0, 0), false));   // object space, in metres
                o.on = i.n; o.wn = TransformObjectToWorldNormal(i.n); return o;
            }
            float Weave(float2 q)
            {
                float2 s = sin(q * 6.2832);
                if (_Weave < .5) return s.x * s.y;                                            // canvas: over-and-under threads
                if (_Weave < 1.5) return sin((q.x + q.y) * 6.2832) * .8 + s.y * .2;            // twill: diagonal lines
                if (_Weave < 2.5) return sin(q.x * 6.2832 * .35);                              // knit: broad ribs
                return sin(q.x * 2.1) * sin(q.y * 2.7) * .5;                                   // rubber: a soft pebbled dip
            }
            half4 frag(V i) : SV_Target
            {
                float3 wn = normalize(i.wn), v = normalize(GetWorldSpaceViewDir(i.wp));
                float3 q = i.os * _WeaveScale;
                float3 w = pow(abs(normalize(i.on)), 4); w /= w.x + w.y + w.z;
                float h = Weave(q.yz) * w.x + Weave(q.xz) * w.y + Weave(q.xy) * w.z;
                float fade = saturate(1.6 - length(fwidth(q)) * 1.4);                         // no weave finer than a pixel
                Light L = GetMainLight();
                float wrap = dot(wn, L.direction) * .5 + .5;
                float lit = smoothstep(.36, .66, wrap) * .8 + wrap * .2;                       // a stepped ramp: folds read as bands
                half3 baseC = _BaseColor.rgb * (1 + h * _WeaveStrength * fade);
                half3 shade = baseC * _ShadeColor.rgb;
                half3 c = lerp(shade, baseC * (L.color * .85 + .15), lit);
                c += baseC * SampleSH(wn) * .22;
                float nv = saturate(dot(wn, v));
                c += pow(1 - nv, 3) * _Rim * L.color * lerp(baseC, half3(1, .9, .75), .5) * .6;
                if (_Sheen > 0) { float3 hv = normalize(L.direction + v); c += pow(saturate(dot(wn, hv)), 40) * _Sheen * L.color * .35; }
                return half4(c, 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct A { float4 p : POSITION; };
            float4 vert(A i) : SV_POSITION { return TransformObjectToHClip(i.p.xyz); }
            half frag() : SV_Target { return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct A { float4 p : POSITION; float3 n : NORMAL; };
            struct V { float4 p : SV_POSITION; float3 n : TEXCOORD0; };
            V vert(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); o.n = TransformObjectToWorldNormal(i.n); return o; }
            half4 frag(V i) : SV_Target { return half4(normalize(i.n), 0); }
            ENDHLSL
        }
    }
}
