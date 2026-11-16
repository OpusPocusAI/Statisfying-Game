// Warm daylight thrown through a four-pane window onto the studio floor: four soft-edged panes split by the mullion
// and transom shadows, brightest near the window and fading at the far end. Additive, on a flat quad laid along the
// floor in the light's direction (StudioInteriorPass). uv.x runs across the window, uv.y from its sill to its head.
Shader "Sea Glass/Window Light"
{
    Properties
    {
        _Colour("Light colour", Color) = (1, .82, .55, 1)
        _Strength("Strength", Range(0, 2)) = .35
        _Bars("Mullion u, transom v, bar half-width u, v", Vector) = (.5, .53, .014, .025)
        _Soft("Edge softness", Range(.002, .2)) = .05
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-20" "RenderType" = "Transparent" }
        Pass
        {
            Blend One One
            ZWrite Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) half4 _Colour; half _Strength; float4 _Bars; half _Soft; CBUFFER_END
            struct A { float4 p : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 p : SV_POSITION; float2 uv : TEXCOORD0; };
            V vert(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); o.uv = i.uv; return o; }
            half Band(float x, float lo, float hi, float s) { return smoothstep(lo - s, lo + s, x) * (1 - smoothstep(hi - s, hi + s, x)); }
            half4 frag(V i) : SV_Target
            {
                float2 uv = i.uv; half s = _Soft;
                half frame = Band(uv.x, .02, .98, s) * Band(uv.y, .02, .98, s);
                half mullion = 1 - Band(uv.x, _Bars.x - _Bars.z, _Bars.x + _Bars.z, s * .5);
                half transom = 1 - Band(uv.y, _Bars.y - _Bars.w, _Bars.y + _Bars.w, s * .5);
                // uv.y 0 is the sill's light, which lands nearest the wall; it is strongest and softens toward the far end.
                half fade = lerp(1, .55, uv.y);
                return half4(_Colour.rgb * frame * mullion * transom * fade * _Strength, 0);
            }
            ENDHLSL
        }
    }
}
