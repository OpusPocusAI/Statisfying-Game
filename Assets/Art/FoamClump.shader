// Round 114 (review 110, all three: the suds "flat white cut-out shapes", "a flat white decal splat, not volumetric"): soap foam as
// real geometry, drawn as lit mesh particles. A soft, bright, slightly glowing white where the light falls, a cool lilac-blue in the
// shade, a pale rim where the foam thins at its silhouette, and a small wet glint.
// Round 125 (review 121, all three: the suds "popcorn clusters of white spheres", "blobby sphere clumps"): the heaps were a dome ringed
// by distinct balls (foam_clump). They are now a low, lobed mound of foam (foam_mound, round125.py), and the many small bubbles of
// soap suds are drawn on it here: a fine net of bubble cells (in world space, as mesh particles are), each with a faint cool wall and a tiny glint, under a
// softer light, and the foot, where the foam sits in the water, takes on the water's tint.
// Round 126 (review 122, A: "oversized cauliflower lumps"; C: "solid white cotton or marshmallow lumps, not foam"): the mounds are
// small dabs now (Feedback), with finer bubbles, a softer, less bright white, and edges that thin out so the water shows through
// them (drawn after the basin water, blended), as a thin rim of real suds does.
Shader "Sea Glass/Foam Clump"
{
 Properties { _Shade("Shadow colour", Color) = (.6, .69, .83, 1) _Bubbles("Bubble cells per metre", Float) = 320 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+60" "RenderType"="Transparent" }
 Pass { Tags { "LightMode"="UniversalForward" } Blend SrcAlpha OneMinusSrcAlpha ZWrite On
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial) half4 _Shade; float _Bubbles; CBUFFER_END
 float4 _SG_Basin;   // the basin water surface (centre xyz, radius w), set by the cleaning station
 struct A {float4 p:POSITION;float3 n:NORMAL;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 w:TEXCOORD1;float3 o:TEXCOORD2;half4 color:COLOR;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.o=i.p.xyz;o.color=i.color;return o;}
 float3 H3(float3 p){p=frac(p*float3(.1031,.103,.0973));p+=dot(p,p.yxz+33.33);return frac((p.xxy+p.yxx)*p.zyx);}
 half4 frag(V i):SV_Target
 {
  float3 n=normalize(i.n);float3 v=normalize(GetWorldSpaceViewDir(i.w));
  Light l=GetMainLight();
  half ndl=dot(n,l.direction);
  // Wrapped, very soft light: foam scatters light, so its shadow side is a pale cool tone, never dark.
  half lit=smoothstep(-.4,.6,ndl);
  half3 shadeC=_Shade.rgb;
  half3 litC=lerp(half3(1,1,1),saturate(l.color),.12)*.9;
  half3 c=lerp(shadeC,litC,lit);
  // Bubble cells: the nearest and second-nearest cell centres (3D Voronoi; mesh particles are drawn in world space, so per metre).
  float3 q=i.w*_Bubbles;float3 cell=floor(q);float f1=9,f2=9;float3 c1=0;
  for(int z=-1;z<=1;z++)for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
  {float3 cc=cell+float3(x,y,z);float3 pt=cc+H3(cc)*.85;float d=length(q-pt);if(d<f1){f2=f1;f1=d;c1=pt;}else if(d<f2)f2=d;}
  half wall=1-smoothstep(.0,.09,f2-f1);              // the thin wall between two bubbles
  c=lerp(c,c*half3(.84,.88,.95),wall*.55);
  // A tiny glint on each lit bubble, a little toward the light.
  float3 toC=normalize(q-c1+1e-4);
  half glint=pow(saturate(dot(toC,normalize(l.direction+v))),18)*smoothstep(.15,.6,lit);
  c+=glint*.22*litC;
  half fres=pow(1-saturate(dot(n,v)),3);
  c+=fres*.06*litC;   // round 126: a faint glow at the thin silhouette (was .18, a bright outline)
  // The foot sits in the water and takes its tint.
  half foot=(1-smoothstep(.0,.012,i.w.y-_SG_Basin.y))*step(.001,_SG_Basin.w);
  c=lerp(c,c*half3(.8,.92,.95),foot*.55);
  // Round 126: the foam thins toward its silhouette and its foot, so the water shows through there.
  half a=lerp(1,.35,smoothstep(.15,.85,fres));a*=lerp(1,.55,foot);a*=lerp(.85,1,1-wall*.6);
  return half4(c*i.color.rgb,saturate(a*i.color.a));
 }
 ENDHLSL
 } } }
