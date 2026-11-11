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
 float VNoise(float2 p){float2 c=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash2(c).x,Hash2(c+float2(1,0)).x,f.x),lerp(Hash2(c+float2(0,1)).x,Hash2(c+float2(1,1)).x,f.x),f.y);}
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
   // Round 104 (review 100: "stamped circle clumps", "cotton-ball spheres"): soap foam lies on the water as a flat raft.
   // Round 105 (review 101, all three: the rafts "a hexagon wire-mesh tile", "a white lattice that reads as netting"): the raft drew
   // its bubble walls as a cell network, which is a regular net at any softness. It is now soft, irregular foam: a cloudy density
   // (layered value noise) that is thick and creamy in the middle, thins into drifting wisps, holes and islands toward a ragged
   // edge, and carries a fine, uneven speckle of tiny bubbles of mixed size, never in a pattern.
   float seed=frac(i.color.r*251.0+i.color.g*113.0)*6.2832;
   float2 o=float2(seed*3.7,seed*1.9);
   float n=VNoise(q*2.6+o)*.45+VNoise(q*5.9+o*1.3)*.33+VNoise(q*13+o*.7)*.22;
   half radial=1-smoothstep(.1,.95,distance+(n-.5)*.7);
   half dens=smoothstep(.5,.6,radial*.6+n*.7);   // a crisper, ragged edge with holes (a soft one read as a white smudge)
   half spk=smoothstep(.62,.72,VNoise(q*34+o*2.1))*smoothstep(.05,.4,radial)*(1-dens);   // tiny bubbles drifting off the edge
   half pore=smoothstep(.6,.75,VNoise(q*22+o*3.3))*dens;   // open pores in the foam, a touch darker
   half3 col=lerp(half3(.84,.9,.93),half3(.99,1,1),saturate(dens*.85+spk))*(1-pore*.16);
   half a2=saturate(dens*(.9-pore*.35)+spk*.7);
   return half4(col*i.color.rgb,i.color.a*a2*.85);
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
   for(int k=0;k<0;k++)   // round 105: none (review 101: "plain white sphere bubbles"); round 104: one, three made each mass a cluster of circles
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
