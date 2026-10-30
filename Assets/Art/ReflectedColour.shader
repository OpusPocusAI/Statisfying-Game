Shader "Sea Glass/Reflected Colour"
{
 Properties { _Intensity("Completion light", Range(0,1))=0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Blend SrcAlpha One ZWrite Off Cull Off Offset -1,-1
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 p:POSITION;half4 color:COLOR;};struct V {float4 p:SV_POSITION;half4 color:COLOR;};
 CBUFFER_START(UnityPerMaterial) half _Intensity; CBUFFER_END
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.color=i.color;return o;}
 // Round 33 (reviews: "the floor light is grey-white, not coloured"): push each patch towards its own hue; pale
 // pearl glass throws warm sunlight-amber rather than grey, and the spill is a little stronger.
 half4 frag(V i):SV_Target{half3 c=i.color.rgb;half l=dot(c,half3(.3,.55,.15));c=saturate(lerp(l.xxx,c,2.2));half mx=max(c.r,max(c.g,c.b)),sat=mx-min(c.r,min(c.g,c.b));c=lerp(half3(1,.72,.42)*mx,c,saturate(sat*5));return half4(c,i.color.a*_Intensity*1.45);}
 ENDHLSL
 } }
}
