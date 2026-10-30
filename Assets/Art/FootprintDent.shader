// A footprint pressed into sand: a foot-shaped dent (heel and ball), shaded darker and cooler on the side away from
// the sun with a thin lit lip on the sunward rim, fading to nothing at its edge, so it reads as a hollow in the sand
// and not as a flat disc lying on it. Drawn on a flat quad just above the sand (uv x across the foot, y heel to toe).
// Round 34, from the thirty-first review ("footprints read as flat floating tan coins").
Shader "Sea Glass/Footprint Dent"
{
    Properties
    {
        _Shade("Shadow colour", Color) = (.42, .3, .38, 1)
        _Lip("Lit lip colour", Color) = (1, .93, .8, 1)
        _Strength("Strength", Range(0, 1)) = .5
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-21" "RenderType" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) half4 _Shade, _Lip; half _Strength; CBUFFER_END
            float4 _SG_SunDir;
            struct A { float4 p : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 p : SV_POSITION; float2 uv : TEXCOORD0; float3 dirOS : TEXCOORD1; };
            V vert(A i)
            {
                V o; o.p = TransformObjectToHClip(i.p.xyz); o.uv = i.uv;
                float3 sun = dot(_SG_SunDir.xyz, _SG_SunDir.xyz) > .01 ? _SG_SunDir.xyz : float3(-.8, .35, .3);
                o.dirOS = TransformWorldToObjectDir(normalize(float3(sun.x, 0, sun.z)));
                return o;
            }
            // Signed distance to a foot: a heel circle and a wider ball ellipse, smoothly joined.
            float Foot(float2 q)
            {
                float heel = length((q - float2(0, -.28)) / float2(.32, .3)) - 1;
                float ball = length((q - float2(.04, .2)) / float2(.42, .38)) - 1;
                float k = .35; float h = saturate(.5 + .5 * (ball - heel) / k);
                return lerp(ball, heel, h) - k * h * (1 - h);
            }
            half4 frag(V i) : SV_Target
            {
                float2 q = i.uv * 2 - 1;
                float d = Foot(q);
                if (d > .35) discard;
                // Depth: deepest in the middle, rising to the rim; a soft lip of displaced sand just outside it.
                half inside = saturate(-d * 2.2);
                half rim = exp(-pow((d - .08) / .12, 2));
                // The quad is a quad in the XZ plane of its object, so the object-space sun direction tells which side
                // of the dent faces the light: the far wall is lit, the near wall falls into shade.
                float2 sunQ = normalize(float2(i.dirOS.x, i.dirOS.z) + 1e-4);
                half facing = dot(normalize(q + 1e-4), sunQ);
                half shade = inside * (.55 + .45 * saturate(-facing)) ;
                half lip = rim * saturate(facing) * .9;
                half3 c = lerp(_Shade.rgb, _Lip.rgb, lip / max(lip + shade, 1e-3));
                half a = saturate(shade * .85 + lip * .55) * _Strength * (1 - smoothstep(.15, .35, d));
                return half4(c, a);
            }
            ENDHLSL
        }
    }
}
