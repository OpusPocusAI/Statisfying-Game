Shader "Sea Glass/Painted Sky"
{
 Properties { _Horizon("Peach horizon",Color)=(1,.79,.6,1) _Zenith("Soft blue sky",Color)=(.3,.56,.66,1) }
 SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 dir:TEXCOORD0;};
 CBUFFER_START(UnityPerMaterial) half4 _Horizon,_Zenith; CBUFFER_END
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.dir=i.p.xyz;return o;}
 half4 frag(V i):SV_Target
 {
  float3 d=normalize(i.dir);float height=saturate(d.y);
  half3 c=lerp(_Horizon.rgb,_Zenith.rgb,smoothstep(0,.72,height));
  float sun=saturate(dot(d,normalize(float3(-.8,.35,.3))));
  c+=half3(1,.5,.19)*pow(sun,12)*.22;
  c=lerp(c,half3(1,.93,.74),smoothstep(.998,.9991,sun));
  float cloud=sin(d.x*16+d.z*7)+sin(d.x*31-d.z*12)*.28;
  float cloudBand=smoothstep(.17,.23,height)*(1-smoothstep(.27,.34,height));
  c=lerp(c,half3(1,.88,.76),smoothstep(.65,1.2,cloud)*cloudBand*.28);
  return half4(c,1);
 }
 ENDHLSL }
 }
}
