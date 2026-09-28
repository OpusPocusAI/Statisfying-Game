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
   struct V { float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;float3 local:TEXCOORD3; };
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor, _EmissionColor; float _Smoothness, _Grime;
   CBUFFER_END
   V vert(A i){V o;o.local=i.p.xyz;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);return o;}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.n),v=GetWorldSpaceNormalizeViewDir(i.world);
    float rim=pow(saturate(1-abs(dot(n,v))),2.3);
    Light sun=GetMainLight();
    float glint=pow(saturate(abs(dot(n,normalize(sun.direction+v)))),lerp(12,90,_Smoothness));
    // Salt islands recede as the scalar cleaning progress changes. Object-space coordinates
    // keep the surface attached to the shard during pickup, washing and placement.
    float clouds=sin(i.local.x*8+sin(i.local.y*5))*sin(i.local.y*7-i.local.x*3);
    float deposits=.5+clouds*.22+sin(i.local.x*19+i.local.y*13)*.08;
    float grime=smoothstep(deposits-.18,deposits+.18,_Grime);
    float frost=sin(i.local.x*43+sin(i.local.y*57))*sin(i.local.y*39-i.local.x*23);
    half3 tint=lerp(_BaseColor.rgb,half3(.66,.63,.53),grime*.94);
    half3 c=tint*(.76+.24*abs(dot(n,sun.direction)))+rim*half3(.38,.59,.57)*(1-grime*.8);
    c+=glint*sun.color*.55*(1-grime)+_EmissionColor.rgb;
    c+=frost*lerp(.003,.016,grime);
    return half4(MixFog(c,i.fog),saturate(lerp(.68,.98,grime)+rim*.24));
   }
   ENDHLSL
  }
 }
}
