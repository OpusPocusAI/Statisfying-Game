// Soft particle disc. With _Glint > 0 it becomes a four-point sparkle with a bright core
// (used for reward bursts). With _Suds > 0 it draws a clump of soap bubbles with soft rims and
// highlights (round 19: brushing foam read as "flat white circles"). Sand keeps the plain soft disc.
Shader "Sea Glass/Soft Feedback"
{
 Properties { _Glint("Sparkle shape", Range(0,1)) = 0 _Suds("Soap bubble clump", Range(0,1)) = 0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial) half _Glint; half _Suds; CBUFFER_END
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 // Bubble centres (xy) and radii (z) inside the particle quad, in -1..1 space.
 static const float3 BUBBLES[6] = { float3(-.18,-.1,.42), float3(.32,.12,.32), float3(-.05,.42,.27), float3(.38,-.38,.22), float3(-.5,.3,.2), float3(-.42,-.48,.17) };
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
  if(_Suds>.5)
  {
   // A clump of bubbles: a pale translucent fill, a soft blue-grey rim and a small white highlight each.
   half sa=0; half3 sc=0;
   for(int k=0;k<6;k++)
   {
    float2 o=q-BUBBLES[k].xy; float r=BUBBLES[k].z; float d=length(o)/r;
    half inside=1-smoothstep(.9,1,d);
    half rim=smoothstep(.62,.9,d)*inside;
    half hi=1-smoothstep(.0,.28,length(o/r-float2(-.35,.38)));
    half ba=inside*(.38+rim*.5)+hi*inside*.4;
    half3 bc=lerp(half3(.94,.97,1),half3(.72,.82,.88),rim)+hi*.3;
    sc=lerp(sc,bc,step(sa,ba)); sa=max(sa,ba);
   }
   return half4(sc*i.color.rgb,i.color.a*sa);
  }
  return half4(c,i.color.a*a);
 }
 ENDHLSL
 } }
}
