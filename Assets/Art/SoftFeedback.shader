// Soft particle disc. With _Glint > 0 it becomes a four-point sparkle with a bright core
// (used for reward bursts). With _Suds > 0 it draws a clump of soap foam (round 19: brushing foam read as
// "flat white circles"; round 96: a creamy foam mass, not a cluster of outlined rings). With _Drop > 0 it draws
// a clear water drop (round 96). Sand keeps the plain soft disc.
Shader "Sea Glass/Soft Feedback"
{
 Properties { _Glint("Sparkle shape", Range(0,1)) = 0 _Suds("Soap foam clump", Range(0,1)) = 0 _Drop("Water drop", Range(0,1)) = 0 _Lace("Foam lace on water", Range(0,1)) = 0 _Ring("Ripple ring", Range(0,1)) = 0 _Mote("Light mote", Range(0,1)) = 0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial) half _Glint; half _Suds; half _Drop; half _Lace; half _Ring; half _Mote; CBUFFER_END
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
   // Round 113 (review 109, all three: the suds "a speckled noise decal", "dithered noise", "flat round decal stamps"): the raft's
   // pores and drifting specks were pixel-sized noise, and its density had no light and shade, so it read as a stamped texture. It is
   // now drawn as a stylised foam raft: a smooth, lobed outline with a few clean open holes, cel-shaded with a bright top and a cool
   // shadow along its far side (so it has thickness on the water), crisp anti-aliased edges, and a handful of separate round bubbles of
   // different sizes breaking away from its rim, shaded the same way. No pixel-scale grain anywhere.
   float seed=frac(i.color.r*251.0+i.color.g*113.0)*6.2832;
   float2 o=float2(seed*3.7,seed*1.9);
   float2 lo=float2(-.07,.09);   // towards the light, in the raft's own plane
   float nA=VNoise(q*2.4+o)*.62+VNoise(q*5.2+o*1.3)*.38;
   float nB=VNoise((q+lo)*2.4+o)*.62+VNoise((q+lo)*5.2+o*1.3)*.38;
   float F=1-length(q)*1.05+(nA-.5)*.75;            // > .25 inside the raft
   float Ft=1-length(q+lo)*1.05+(nB-.5)*.75;        // the same raft, shifted: where both are inside is the lit top
   float hole=VNoise(q*3.4+o*2.2);
   float aw=max(fwidth(F),.004);
   half body=smoothstep(.25-aw,.25+aw,F)*(1-smoothstep(.7-aw*3,.7+aw*3,hole)*smoothstep(.45,.6,F));
   half top=smoothstep(.31-aw,.31+aw,Ft)*body;
   half bub=0, bubTop=0;
   for(int k=0;k<6;k++)
   {
    float ka=seed*1.3+k*1.047+frac(seed*7.1+k*.31)*.6;
    float kr=.62+.3*frac(seed*3.3+k*.53);
    float rad=lerp(.035,.085,frac(seed*5.9+k*.71));
    float2 c=float2(cos(ka),sin(ka))*kr;
    float d=length(q-c)/rad;
    float bw=max(fwidth(d),.02);
    half inB=1-smoothstep(1-bw,1+bw,d);
    half tB=(1-smoothstep(.7,.95,length((q-c)/rad+float2(.25,-.3))))*inB;
    bub=max(bub,inB); bubTop=max(bubTop,tB);
   }
   half lit=max(top,bubTop);
   half3 col=lerp(half3(.66,.76,.84),half3(1,1,1),lit);
   half a2=max(body,bub);
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
   float bwS=max(fwidth(distance),.01); half body=1-smoothstep(edgeR-bwS,edgeR+bwS,distance);   /* round 113: a crisp, lobed outline (a long soft falloff read as a stamp) */   // a firmer middle: softer still read as mist
   // Fine foam: a cell pattern of tiny packed bubbles, a little greyer in the gaps between them.
   float2 g=q*6.5+seed*3.1; float2 cell=floor(g); float2 f=frac(g); float md=1;
   for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){float2 nb=float2(x,y);md=min(md,length(nb+Hash2(cell+nb)-f));}
   half micro=1;   // round 113: no cell pattern (it read as noise at the basin)
   // Round 113 (review 109: the masses "flat round decal stamps"): a clump of foam with a lit top and a cool shaded underside, cel-shaded
   // like the raft, so it has volume without the cotton-ball gradient (round 104).
   float tl=distance+.035*sin(ang*3+seed)-dot(q,float2(-.45,.55))*.55;
   half shade=lerp(.72,1.02,smoothstep(.5,.56,1-tl));
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
   // Round 114: a falling drop (_Drop 2) is a teardrop, round below and drawn to a point above, so it reads as a drip in a still.
   float dd=distance;
   if(_Drop>1.5){float yy=q.y+.18;dd=yy<0?length(float2(q.x,yy))/.78:abs(q.x)/max(.78*(1-yy/.95),1e-3);dd=yy>.95?2:dd;dd*=.95;}
   half inside=1-smoothstep(.72,.95,dd);
   half rim=smoothstep(.4,.9,dd)*inside;
   half hi=1-smoothstep(0,.32,length(q-float2(-.3,.32)));
   // Round 98 (review 94: no splashes or drips seen): a deeper teal rim and a hotter highlight, so a drop stands out over the pale water.
   half3 col=lerp(half3(.78,.93,.97),half3(.22,.5,.6),rim)+hi*1.2;
   if(_Drop>1.5){col=lerp(half3(.72,.9,.95),half3(.12,.36,.46),rim)+hi*1.4;return half4(col*i.color.rgb,i.color.a*saturate(inside*(.72+rim*.5)+hi*inside*.8));}   // round 114: a teardrop, higher contrast over the pale water
   return half4(col*i.color.rgb,i.color.a*saturate(inside*(.55+rim*.45)+hi*inside*.6));
  }
  if(_Mote>.5)
  {
   // Round 113 (review 109, all three: the mosaic motes "plain round dots", "generic soft dots", "close to default sprites"): a fleck of
   // coloured light thrown by the glass: a slender, tapering lozenge with a white-hot core, a coloured body and a faint halo, its
   // length varying per particle; each turns at random, so the motes read as glinting flecks rather than round dots.
   float asp=lerp(.22,.45,frac(i.color.r*97.0+i.color.b*41.0));
   float dm=abs(q.x)/asp+abs(q.y);
   half flk=pow(saturate(1-dm),1.6);
   half coreM=pow(saturate(1-dm*2.2),2);
   half haloM=exp(-distance*distance*7)*.18;
   half3 cm=lerp(i.color.rgb,half3(1,1,1),saturate(coreM*1.4));
   return half4(cm,i.color.a*saturate(flk*.9+coreM+haloM));
  }
  return half4(c,i.color.a*a);
 }
 ENDHLSL
 } }
}
