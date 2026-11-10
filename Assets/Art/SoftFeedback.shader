// Soft particle disc. With _Glint > 0 it becomes a four-point sparkle with a bright core
// (used for reward bursts). With _Suds > 0 it draws a clump of soap foam (round 19: brushing foam read as
// "flat white circles"; round 96: a creamy foam mass, not a cluster of outlined rings). With _Drop > 0 it draws
// a clear water drop (round 96). Sand keeps the plain soft disc.
Shader "Sea Glass/Soft Feedback"
{
 Properties { _Glint("Sparkle shape", Range(0,1)) = 0 _Suds("Soap foam clump", Range(0,1)) = 0 _Drop("Water drop", Range(0,1)) = 0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial) half _Glint; half _Suds; half _Drop; CBUFFER_END
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.color=i.color;return o;}
 float2 Hash2(float2 p){p=float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3)));return frac(sin(p)*43758.5453);}
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
   // Round 96 (review 92, all three: "ring-outline bubble clusters", "clip-art", "close to default sprites"): every particle drew
   // the same six outlined rings. Now each is a soft, creamy mass of fine foam with an irregular edge, shaded from the top-left
   // so it has volume, with a few larger bubbles riding on it; the shape varies per particle (seeded from its colour jitter).
   float seed=frac(i.color.r*251.0+i.color.g*113.0)*6.2832;
   float ang=atan2(q.y,q.x);
   float edgeR=.74+.05*sin(ang*2+seed)+.04*sin(ang*3+seed*2.3)+.02*sin(ang*7-seed);   // gentle lobes: stronger ones read as clover at basin scale
   half body=1-smoothstep(edgeR-.32,edgeR,distance);
   // Fine foam: a cell pattern of tiny packed bubbles, a little greyer in the gaps between them.
   float2 g=q*6.5+seed*3.1; float2 cell=floor(g); float2 f=frac(g); float md=1;
   for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){float2 nb=float2(x,y);md=min(md,length(nb+Hash2(cell+nb)-f));}
   half micro=1-smoothstep(.3,.62,md)*.09;
   half shade=lerp(.8,1.04,saturate(.5+dot(q,float2(-.45,.55))*.7));
   half3 col=half3(.97,.98,.99)*micro*shade;
   half alpha=body*.92;
   // A few larger bubbles on the mass: thin bright rims with a soap-film tint and a highlight.
   for(int k=0;k<3;k++)
   {
    float2 bc=float2(sin(seed*1.7+k*2.1),cos(seed*1.3+k*2.7))*.34; float r=.13+.08*frac(seed*3.1+k*.37);
    float2 o=q-bc; float d=length(o)/r;
    half inside=1-smoothstep(.88,1,d);
    half rim=smoothstep(.7,.95,d)*inside;
    half hi=1-smoothstep(0,.3,length(o/r-float2(-.35,.38)));
    half3 film=half3(.9+.1*sin(d*8+seed),.94+.06*sin(d*8+2.1),.96+.04*sin(d*8+4.2));
    col=lerp(col,lerp(col*1.02,film*.86,rim),inside)+hi*inside*.35;
    alpha=max(alpha,inside*(rim*.85+hi*.6));
   }
   return half4(col*i.color.rgb,i.color.a*alpha);
  }
  if(_Drop>.5)
  {
   // Round 96: a clear water drop: a pale see-through body, a slightly deeper rim and a bright highlight, so a splash reads as water.
   half inside=1-smoothstep(.72,.95,distance);
   half rim=smoothstep(.4,.9,distance)*inside;
   half hi=1-smoothstep(0,.32,length(q-float2(-.3,.32)));
   half3 col=lerp(half3(.8,.94,.97),half3(.5,.74,.82),rim)+hi*.8;
   return half4(col*i.color.rgb,i.color.a*saturate(inside*(.45+rim*.4)+hi*inside*.5));
  }
  return half4(c,i.color.a*a);
 }
 ENDHLSL
 } }
}
