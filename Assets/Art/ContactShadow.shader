// Soft contact shadow under furniture: a rounded-rectangle blob, darkest under the object and fading to
// nothing at its edge, tinted towards the scene's lilac shadow colour (never grey). Drawn on a flat quad
// just above the floor. Round 19, from the seventeenth review ("furniture floats, no contact shadows").
Shader "Sea Glass/Contact Shadow"
{
    Properties
    {
        _Colour("Shadow colour", Color) = (.22, .14, .26, 1)
        _Strength("Strength", Range(0, 1)) = .45
        _Softness("Edge softness", Range(.05, 1)) = .55
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-20" "RenderType" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) half4 _Colour; half _Strength, _Softness; CBUFFER_END
            struct A { float4 p : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 p : SV_POSITION; float2 uv : TEXCOORD0; };
            V vert(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); o.uv = i.uv; return o; }
            half4 frag(V i) : SV_Target
            {
                float2 q = abs(i.uv * 2 - 1);
                // Rounded-rectangle distance: 0 in the middle, 1 at the quad's edge.
                float2 k = max(q - (1 - _Softness), 0) / _Softness;
                float d = length(k);
                half a = (1 - smoothstep(0, 1, d)) * _Strength;
                return half4(_Colour.rgb, a);
            }
            ENDHLSL
        }
    }
}
