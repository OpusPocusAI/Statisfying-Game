Shader "Sea Glass/Tidal Water"
{
 Properties { _BaseColor("Lagoon",Color)=(.12,.69,.68,1) _DeepColor("Deep teal",Color)=(.035,.3,.4,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float fog:TEXCOORD1;};
 CBUFFER_START(UnityPerMaterial) half4 _BaseColor,_DeepColor; CBUFFER_END
 V vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
  float shore=-34.5+sin(i.world.z*.07)*1.1+sin(i.world.z*.19)*.3;
  float depth=max(0,shore-i.world.x);
  float waves=sin(i.world.x*.38+i.world.z*.065+_Time.y*.55)+sin(i.world.x*.71-i.world.z*.13+_Time.y*.3)*.35;
  float shallows=1-smoothstep(0,34,depth);
  half3 c=lerp(_DeepColor.rgb,_BaseColor.rgb,shallows*.8+.12+waves*.07);
  float foam=smoothstep(.75,.95,sin(depth*2.8-_Time.y*1.25+sin(i.world.z*.18)*.6));
  foam*=1-smoothstep(0,4.5,depth);foam*=smoothstep(-.15,.5,sin(i.world.z*.9+_Time.y*.2));
  c=lerp(c,half3(.87,.96,.84),foam*.72);
  float glint=pow(saturate(sin(i.world.x*1.2+i.world.z*.43+_Time.y*.7)*sin(i.world.z*1.4-i.world.x*.18)),12);
  c+=half3(1,.89,.62)*glint*.18;
  return half4(MixFog(c,i.fog),1);
 }
 ENDHLSL
 } }
}
