// Shallow clear water over pale enamel: light enough that freshly cleaned glass colour pops.
Shader "Sea Glass/Basin Water"
{
    Properties
    {
        _BaseColor("Enamel seen through water", Color) = (.8, .88, .85, 1)
        _Deep("Water tint at the edges", Color) = (.46, .7, .7, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A { float4 p : POSITION; };
            struct V { float4 p : SV_POSITION; float2 local : TEXCOORD0; float fog : TEXCOORD1; float3 world : TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial) half4 _BaseColor, _Deep; CBUFFER_END
            V vert(A i) { V o; o.world = TransformObjectToWorld(i.p.xyz); o.p = TransformWorldToHClip(o.world); o.local = i.p.xz; o.fog = ComputeFogFactor(o.p.z); return o; }
            half4 frag(V i) : SV_Target
            {
                float2 p = i.local;
                float edge = smoothstep(.22, .5, max(abs(p.x), abs(p.y)));
                // Caustic-like light lines on the enamel floor.
                float2 w = p * 9 + float2(_Time.y * .25, -_Time.y * .18);
                float caustic = abs(sin(w.x + sin(w.y * 1.3 + _Time.y * .4) * 1.4) * sin(w.y + sin(w.x * 1.1 - _Time.y * .3) * 1.4));
                caustic = pow(1 - caustic, 6);
                float ripple = sin(length(p * float2(1.6, 1)) * 48 - _Time.y * 2 + sin(p.x * 17) * .35);
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.world));
                half3 light = sun.color * lerp(.55, 1, sun.shadowAttenuation) * .5 + SampleSH(half3(0, 1, 0)) * .55;
                half3 c = lerp(_BaseColor.rgb, _Deep.rgb, edge * .8) * light;
                c += caustic * half3(.9, 1, .95) * .05 * (1 - edge);
                c += pow(saturate(ripple), 18) * .05;
                return half4(MixFog(c, i.fog), 1);
            }
            ENDHLSL
        }
    }
}
