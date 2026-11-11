// Soft particle disc. With _Glint > 0 it becomes a four-point sparkle with a bright core
// (used for reward bursts). With _Suds > 0 it draws a clump of soap foam (round 19: brushing foam read as
// "flat white circles"; round 96: a creamy foam mass, not a cluster of outlined rings). With _Drop > 0 it draws
// a clear water drop (round 96). Sand keeps the plain soft disc.
Shader "Sea Glass/Soft Feedback"
{
 Properties { _Glint("Sparkle shape", Range(0,1)) = 0 _Suds("Soap foam clump", Range(0,1)) = 0 _Drop("Water drop", Range(0,1)) = 0 _Lace("Foam lace on water", Range(0,1)) = 0 _Ring("Ripple ring", Range(0,1)) = 0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial) half _Glint; half _Suds; half _Drop; half _Lace; half _Ring; CBUFFER_END
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.color=i.color;return o;}
 float2 Hash2(float2 p){p=float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3)));return frac(sin(p)*43758.5453);}
 half4 frag(V i):SV_Target
 {
  float2 q=i.uv*2-1;
  float distance=length(q);
  half disc=1-smoothstep(.35,1,distance);
  // Sparkle. Round 98 (review 94, all three: "generic, evenly scattered four-point stars", "stock"): a glint of light on wet glass,
  // not a symmetric cross: a long upright arm and a shorter level one that taper to fine points, four faint short diagonal rays,
  // a soft halo in the glass colour and a small white-hot core. Each particle turns at random, so no two read alike.
  float armV=saturate(1-abs(q.x)*16)*pow(saturate(1-abs(q.y)),2.2);
  float armH=saturate(1-abs(q.y)*16)*pow(saturate(1-abs(q.x)*1.45),2.2);
  float2 dg=float2(q.x+q.y,q.x-q.y)*.7071;
  float rays=max(saturate(1-abs(dg.x)*22)*pow(saturate(1-abs(dg.y)*2.6),3),saturate(1-abs(dg.y)*22)*pow(saturate(1-abs(dg.x)*2.6),3))*.55;
  float arms=max(armV,armH);
  float core=saturate(1-distance*3.2);
  half halo=exp(-distance*distance*10)*.5;
  half star=saturate(arms*1.25+rays+core*core*1.2+halo);
  half a=lerp(disc,star,_Glint);
  half3 c=lerp(i.color.rgb*(1.1-distance*.2),lerp(i.color.rgb*1.15,half3(1,1,1),saturate(core*1.6+arms*.45)),_Glint);
  if(_Lace>.5)
  {
   // Round 104 (review 100, all three: "stamped circle clumps", "cotton-ball spheres"): soap foam lying on water is a lacy raft: a
   // network of white bubble walls, water showing through the cells between them, densest in the middle and breaking up into
   // loose strands and islands at its ragged edge. Cell borders (the second-nearest minus the nearest point) draw the walls.
   float seed=frac(i.color.r*251.0+i.color.g*113.0)*6.2832;
   float ang=atan2(q.y,q.x);
   float edgeR=.8+.08*sin(ang*3+seed)+.05*sin(ang*5-seed*1.7);
   float2 g=q*9+seed*2.3; float2 cell=floor(g); float2 f=frac(g); float f1=8,f2=8;
   for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){float2 nb=float2(x,y);float d=length(nb+Hash2(cell+nb)-f);if(d<f1){f2=f1;f1=d;}else if(d<f2)f2=d;}
   half wall=1-smoothstep(.04,.3,f2-f1);   // soft, thick walls: thin crisp ones drew a white net
   half fill=1-smoothstep(.15,.6,f1);   // a little foam fills the middle of each cell too
   half mask=1-smoothstep(edgeR*.25,edgeR,distance);
   half lace=saturate(wall*(.45+mask*.4)+fill*mask*.5+mask*mask*.25);
   half a2=lace*smoothstep(0,.35,mask)*smoothstep(.15,.6,mask+wall*.2);   // the edge breaks into strands
   half3 col=lerp(half3(.88,.93,.95),half3(1,1,1),wall);
   return half4(col*i.color.rgb,i.color.a*a2*.8);
  }
  if(_Ring>.5)
  {
   // Round 104: a drop's ring on the water: a bright crest with a darker trough just inside it, slightly wobbly.
   float ang=atan2(q.y,q.x);
   float rr=distance+.02*sin(ang*5+i.color.r*40);
   half crest=exp(-pow((rr-.82)/.075,2));
   half trough=exp(-pow((rr-.66)/.08,2));
   half crest2=exp(-pow((rr-.5)/.04,2))*.5;
   half3 col=lerp(half3(.2,.42,.46),half3(1,1,1),saturate(crest+crest2)/(saturate(crest+crest2)+trough*.6+1e-3));
   return half4(col*i.color.rgb,i.color.a*saturate(crest+crest2*.7+trough*.55));
  }
  if(_Suds>.5)
  {
   // Round 96 (review 92, all three: "ring-outline bubble clusters", "clip-art", "close to default sprites"): every particle drew
   // the same six outlined rings. Now each is a soft, creamy mass of fine foam with an irregular edge, shaded from the top-left
   // so it has volume, with a few larger bubbles riding on it; the shape varies per particle (seeded from its colour jitter).
   float seed=frac(i.color.r*251.0+i.color.g*113.0)*6.2832;
   float ang=atan2(q.y,q.x);
   float edgeR=.74+.05*sin(ang*2+seed)+.04*sin(ang*3+seed*2.3)+.02*sin(ang*7-seed);   // gentle lobes: stronger ones read as clover at basin scale
   // Round 97: a long, soft falloff from a dense middle, so overlapping masses blend into one heap of foam (a short edge drew
   // each particle as its own white blob, "popcorn").
   half body=1-smoothstep(edgeR*.38,edgeR,distance); body=sqrt(body);   // a firmer middle: softer still read as mist
   // Fine foam: a cell pattern of tiny packed bubbles, a little greyer in the gaps between them.
   float2 g=q*6.5+seed*3.1; float2 cell=floor(g); float2 f=frac(g); float md=1;
   for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){float2 nb=float2(x,y);md=min(md,length(nb+Hash2(cell+nb)-f));}
   half micro=1-smoothstep(.3,.62,md)*.08;   // round 104: fainter, the cells read as stamped circles
   half shade=lerp(.9,1.02,saturate(.5+dot(q,float2(-.45,.55))*.7));   // round 104: flatter, the sphere shading read as cotton balls
   half3 col=half3(.97,.98,.99)*micro*shade;
   half alpha=body*.92;
   // A few larger bubbles on the mass: thin bright rims with a soap-film tint and a highlight.
   for(int k=0;k<1;k++)   // round 104: one bubble, three made each mass a cluster of circles
   {
    float2 bc=float2(sin(seed*1.7+k*2.1),cos(seed*1.3+k*2.7))*.34; float r=.13+.08*frac(seed*3.1+k*.37);
    float2 o=q-bc; float d=length(o)/r;
    half inside=1-smoothstep(.88,1,d);
    half rim=smoothstep(.7,.95,d)*inside;
    half hi=1-smoothstep(0,.3,length(o/r-float2(-.35,.38)));
    half3 film=half3(.9+.1*sin(d*8+seed),.94+.06*sin(d*8+2.1),.96+.04*sin(d*8+4.2));
    col=lerp(col,lerp(col*1.02,film*.86,rim),inside)+hi*inside*.35;
    alpha=max(alpha,inside*(rim*.5+hi*.45)*body);   // round 97: the bubbles sit in the foam, fading with it, not ringed on its edge
   }
   return half4(col*i.color.rgb,i.color.a*alpha);
  }
  if(_Drop>.5)
  {
   // Round 96: a clear water drop: a pale see-through body, a slightly deeper rim and a bright highlight, so a splash reads as water.
   half inside=1-smoothstep(.72,.95,distance);
   half rim=smoothstep(.4,.9,distance)*inside;
   half hi=1-smoothstep(0,.32,length(q-float2(-.3,.32)));
   // Round 98 (review 94: no splashes or drips seen): a deeper teal rim and a hotter highlight, so a drop stands out over the pale water.
   half3 col=lerp(half3(.78,.93,.97),half3(.22,.5,.6),rim)+hi*1.2;
   return half4(col*i.color.rgb,i.color.a*saturate(inside*(.55+rim*.45)+hi*inside*.6));
  }
  return half4(c,i.color.a*a);
 }
 ENDHLSL
 } }
}
