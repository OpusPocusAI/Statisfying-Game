Shader "Sea Glass/Basin Water"
{
 Properties { _BaseColor("Enamel reflected in water",Color)=(.18,.43,.43,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A { float4 p:POSITION; };
 struct V { float4 p:SV_POSITION;float2 local:TEXCOORD0;float fog:TEXCOORD1; };
 CBUFFER_START(UnityPerMaterial) half4 _BaseColor; CBUFFER_END
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.local=i.p.xz;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
  float2 p=i.local;
  float ripple=sin(length(p*float2(1.6,1))*48-_Time.y*2+sin(p.x*17)*.35);
  float edge=smoothstep(.28,.49,max(abs(p.x),abs(p.y)));
  half3 c=_BaseColor.rgb*(1-edge*.14)+half3(.32,.48,.43)*pow(saturate(ripple),18)*.035;
  c+=half3(.009,.012,.01)*sin(p.x*12+p.y*9+_Time.y*.5);
  return half4(MixFog(c,i.fog),1);
 }
 ENDHLSL
 } }
}
