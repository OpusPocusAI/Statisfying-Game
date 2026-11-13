// Round 114 (review 110, all three: the suds "flat white cut-out shapes", "a flat white decal splat, not volumetric"): soap foam as
// real geometry, drawn as lit mesh particles (foam_clump, round114.py). A soft, bright, slightly glowing white where the light falls,
// a cool lilac-blue in the shade under and between the bubbles, a pale rim where the foam thins at its silhouette, and a small wet
// glint, so each heap reads as a mound of bubbles standing up off the water.
Shader "Sea Glass/Foam Clump"
{
 Properties { _Shade("Shadow colour", Color) = (.6, .69, .83, 1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
 Pass { Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial) half4 _Shade; CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 w:TEXCOORD1;half4 color:COLOR;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.color=i.color;return o;}
 half4 frag(V i):SV_Target
 {
  float3 n=normalize(i.n);float3 v=normalize(GetWorldSpaceViewDir(i.w));
  Light l=GetMainLight();
  half ndl=dot(n,l.direction);
  // Wrapped, softly stepped light: foam scatters light, so its shadow side is a pale cool tone, never dark.
  half lit=smoothstep(-.25,.45,ndl);
  // Soap foam stays white under the warm studio light (tinted by it, it read as peach popcorn): only a trace of the light colour.
  half3 shadeC=_Shade.rgb;
  half3 litC=lerp(half3(1,1,1),saturate(l.color),.12)*.97;
  half3 c=lerp(shadeC,litC,lit);
  half fres=pow(1-saturate(dot(n,v)),3);
  c+=fres*.22*litC;   // thin foam at the silhouette glows
  half spec=pow(saturate(dot(n,normalize(l.direction+v))),70)*.45;
  c+=spec*l.color;
  return half4(c*i.color.rgb,1);
 }
 ENDHLSL
 } } }
