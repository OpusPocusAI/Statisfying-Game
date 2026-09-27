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
 half4 frag(V i):SV_Target{return half4(i.color.rgb,i.color.a*_Intensity);}
 ENDHLSL
 } }
}
