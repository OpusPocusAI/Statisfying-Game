Shader "Sea Glass/Rock Surf"
{
 Properties { _FoamColor("Warm sea foam",Color)=(.85,.94,.84,.58) }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A { float4 p:POSITION;float2 uv:TEXCOORD0; };
   struct V { float4 p:SV_POSITION;float3 world:TEXCOORD0;float distance:TEXCOORD1;float fog:TEXCOORD2; };
   CBUFFER_START(UnityPerMaterial)
    half4 _FoamColor;
   CBUFFER_END
   V vert(A i)
   {
    V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);
    o.distance=i.uv.x;o.fog=ComputeFogFactor(o.p.z);return o;
   }
   half4 frag(V i):SV_Target
   {
    float t=_Time.y, d=i.distance;
    float irregular=sin(i.world.x*1.8+sin(i.world.z*1.1)+t*.23)*sin(i.world.z*2.1-t*.17);
    float patches=smoothstep(-.45,.45,irregular);
    float aa=max(.025,fwidth(d)*1.2);
    float edge=.09+.045*sin(i.world.z*1.7+i.world.x*.8+t*.5);
    float contact=1-smoothstep(.035,.035+aa,abs(d-edge));
    // Each quiet surge travels outward and fades before its phase wraps.
    float cycle=frac(t*.13+sin(i.world.z*.15+i.world.x*.09)*.12);
    float swell=.18+cycle*.65+.035*irregular;
    float ripple=(1-smoothstep(.025,.025+aa,abs(d-swell)))*sin(cycle*PI);
    float wash=(1-smoothstep(.05,.65,d))*.10;
    float alpha=(contact*.55+ripple*.42+wash)*patches*_FoamColor.a;
    alpha*=smoothstep(-.10,.015,d)*(1-smoothstep(.85,1.10,d));
    return half4(MixFog(_FoamColor.rgb,i.fog),saturate(alpha));
   }
   ENDHLSL
  }
 }
}
