// Soft particle disc. With _Glint > 0 it becomes a four-point sparkle with a bright core
// (used for reward bursts); foam and sand keep the plain soft disc.
Shader "Sea Glass/Soft Feedback"
{
 Properties { _Glint("Sparkle shape", Range(0,1)) = 0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial) half _Glint; CBUFFER_END
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.color=i.color;return o;}
 half4 frag(V i):SV_Target
 {
  float2 q=i.uv*2-1;
  float distance=length(q);
  half disc=1-smoothstep(.35,1,distance);
  // Sparkle: thin cross arms plus a hot core.
  float arms=max(saturate(1-abs(q.x)*9)*saturate(1-abs(q.y)),saturate(1-abs(q.y)*9)*saturate(1-abs(q.x)));
  float core=saturate(1-distance*2.6);
  half star=saturate(arms*1.2+core*core+disc*.25);
  half a=lerp(disc,star,_Glint);
  half3 c=i.color.rgb*(1.1-distance*.2)+core*core*_Glint*.8;
  return half4(c,i.color.a*a);
 }
 ENDHLSL
 } }
}
