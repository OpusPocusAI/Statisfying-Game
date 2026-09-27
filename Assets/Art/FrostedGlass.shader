Shader "Sea Glass/Frosted Glass"
{
 Properties
 {
  _BaseColor("Glass colour",Color)=(.1,.8,.7,1)
  _EmissionColor("Inner light",Color)=(0,0,0,1)
  _Smoothness("Polish",Range(0,1))=.8
  _Grime("Salt and sand",Range(0,1))=0
 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
  Pass
  {
   Tags { "LightMode"="UniversalForward" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A { float4 p:POSITION;float3 n:NORMAL; };
   struct V { float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2; };
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor, _EmissionColor; float _Smoothness, _Grime;
   CBUFFER_END
   V vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);return o;}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.n),v=GetWorldSpaceNormalizeViewDir(i.world);
    float rim=pow(saturate(1-abs(dot(n,v))),2.3);
    Light sun=GetMainLight();
    float glint=pow(saturate(abs(dot(n,normalize(sun.direction+v)))),lerp(12,90,_Smoothness));
    float salt=frac(sin(dot(floor(i.world*160),float3(12.9,78.2,43.1)))*43758.5);
    half3 tint=lerp(_BaseColor.rgb,half3(.62,.58,.46),_Grime*.88);
    half3 c=tint*(.8+.2*abs(dot(n,sun.direction)))+rim*half3(.48,.7,.68)*(1-_Grime*.7);
    c+=glint*sun.color*.45*(1-_Grime)+_EmissionColor.rgb;
    c+=(salt-.5)*lerp(.025,.16,_Grime);
    return half4(MixFog(c,i.fog),saturate(lerp(.7,.99,_Grime)+rim*.25));
   }
   ENDHLSL
  }
 }
}
