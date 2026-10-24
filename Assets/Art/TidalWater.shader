Shader "Sea Glass/Tidal Water"
{
 Properties { _BaseColor("Lagoon",Color)=(.12,.69,.68,1) _DeepColor("Deep teal",Color)=(.035,.3,.4,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Transparent-50" }
 Pass {
 ZWrite On
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float fog:TEXCOORD1;float eye:TEXCOORD2;};
 CBUFFER_START(UnityPerMaterial) half4 _BaseColor,_DeepColor; CBUFFER_END
 V vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.p.z);o.eye=-TransformWorldToView(o.world).z;return o;}
 float Hash(float2 p)
 {
  float3 q=frac(float3(p.x,p.y,p.x)*.1031);
  q+=dot(q,q.yzx+33.33);
  return frac((q.x+q.y)*q.z);
 }
 float BrushNoise(float2 p)
 {
  float2 cell=floor(p),f=frac(p);f=f*f*(3-2*f);
  return lerp(lerp(Hash(cell),Hash(cell+float2(1,0)),f.x),
              lerp(Hash(cell+float2(0,1)),Hash(cell+1),f.x),f.y);
 }
 half4 frag(V i):SV_Target
 {
  float2 p=i.world.xz;float t=_Time.y;
  float shore=-34.5+sin(p.y*.07)*1.1+sin(p.y*.19)*.3;
  float depth=max(0,shore-p.x);
  float broad=BrushNoise(p*.065+float2(t*.006,-t*.004));
  float drift=BrushNoise(p*float2(.17,.31)+float2(-t*.018,t*.024));
  float phase=dot(p,float2(.38,.065))-t*.42+broad*2.7;
  float crossing=dot(p,float2(.19,-.32))-t*.28+drift*1.6;
  float waves=sin(phase)*.65+sin(crossing)*.35;
  float shallows=1-smoothstep(0,34,depth);
  half3 c=lerp(_DeepColor.rgb,_BaseColor.rgb,saturate(shallows*.8+.12+waves*.07+(broad-.5)*.12));

  // Irregular elongated brush marks replace the uniform lattice of bright specks.
  float2 brushUV=p*float2(.38,1.25)+float2(-t*.07,t*.035)+float2(broad*1.7,drift*.7);
  float brush=BrushNoise(brushUV);
  float brushAA=max(.035,fwidth(brush));
  float detail=1-smoothstep(.25,1.2,length(fwidth(brushUV)));
  float marks=smoothstep(.72-brushAA,.82+brushAA,brush)*detail;
  c+=half3(.42,.64,.59)*marks*.13;

  // A small directional sheen follows the actual golden-hour sun. No reflection camera.
  float3 view=GetWorldSpaceNormalizeViewDir(i.world);
  float3 normal=normalize(float3(cos(phase)*.09,1,cos(crossing)*.07));
  Light sun=GetMainLight();
  float sheen=pow(saturate(dot(normal,normalize(sun.direction+view))),128);
  c+=sun.color*sheen*(.045+marks*.22);

  // Broken crests roll toward the sloping beach; their width is filtered at distance.
  float foamPhase=depth*2.1+t*.8+BrushNoise(float2(p.y*.23,t*.025))*2.2;
  float crest=sin(foamPhase);float foamAA=max(.035,fwidth(crest));
  float foam=smoothstep(.82-foamAA,.97+foamAA,crest);
  foam*=smoothstep(.30,.64,BrushNoise(p*float2(.55,.85)+float2(t*.04,-t*.055)));
  foam*=(1-smoothstep(.2,4.8,depth))*detail;
  c=lerp(c,half3(.87,.96,.84),foam*.58);

  // True water depth from the scene depth buffer: bright turquoise shallows over sand,
  // and a soft, lapping foam line wherever the water touches the beach or a rock.
  float2 suv=GetNormalizedScreenSpaceUV(i.p);
  float sceneEye=LinearEyeDepth(SampleSceneDepth(suv),_ZBufferParams);
  float vdepth=max(0,(sceneEye-i.eye)*abs(view.y));
  half3 shallowTint=half3(.42,.9,.8);
  c=lerp(shallowTint*(.85+.15*sun.color),c,smoothstep(.0,1.1,vdepth));
  float lap=.07+.06*sin(t*.7+p.y*.11+BrushNoise(float2(p.y*.15,t*.05))*2.0);
  // Clamp: across underwater silhouettes the depth jumps, and an unclamped fwidth paints false foam.
  float edgeAA=clamp(fwidth(vdepth),.01,.04);
  float contact=1-smoothstep(lap-edgeAA,lap+.04+edgeAA,vdepth);
  float trail=smoothstep(lap+.05,lap+.09,vdepth)*(1-smoothstep(lap+.11,lap+.2,vdepth));
  float breakup=smoothstep(.25,.6,BrushNoise(p*float2(1.6,.9)+float2(t*.15,t*.05)));
  float shoreFoam=saturate(contact*.95+trail*breakup*.6);
  half3 foamColour=half3(.99,.96,.9)*(.75+.35*sun.color);
  c=lerp(c,foamColour,shoreFoam*.92);
  return half4(MixFog(c,i.fog),1);
 }
 ENDHLSL
 } }
}
