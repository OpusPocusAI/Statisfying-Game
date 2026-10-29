// Coloured light thrown through a clean glass piece onto the surface below it: a soft pool of the glass's
// colour with a brighter, slightly rippled focus where the domed piece gathers the light (a caustic).
// Additive, on a flat quad just above the felt. Round 25, from the twenty-third review ("no coloured light on
// the felt", "light does not pass through").
Shader "Sea Glass/Glass Light"
{
    Properties
    {
        _Colour("Glass colour", Color) = (.2, .8, .7, 1)
        _Strength("Strength", Range(0, 2)) = .5
        _Focus("Caustic focus (uv)", Vector) = (.58, .5, 0, 0)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-15" "RenderType" = "Transparent" }
        Pass
        {
            Blend One One
            ZWrite Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) half4 _Colour; half _Strength; float4 _Focus; CBUFFER_END
            struct A { float4 p : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 p : SV_POSITION; float2 uv : TEXCOORD0; };
            V vert(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); o.uv = i.uv; return o; }
            half4 frag(V i) : SV_Target
            {
                float2 d = i.uv * 2 - 1;
                half pool = 1 - smoothstep(.2, 1, length(d));
                float2 f = i.uv - _Focus.xy;
                float r = length(f * float2(1, 1.3));
                // A bright focus with a thin caustic ring around it.
                half focus = exp(-r * r * 90) * 1.6 + exp(-pow((r - .17) * 22, 2)) * .45 * (.75 + .25 * sin(atan2(f.y, f.x) * 5));
                half3 c = _Colour.rgb / max(max(_Colour.r, _Colour.g), max(_Colour.b, .05));   // full-strength hue
                return half4(c * (pool * .35 + focus * pool) * _Strength, 0);
            }
            ENDHLSL
        }
    }
}
