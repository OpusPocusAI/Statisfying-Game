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
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
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
 // Authored coastline (bays and headlands). Keep in sync with Editor/CoastShape.cs.
 static const float2 SHORE[13]={float2(-45,-35.0),float2(-20,-37.6),float2(0,-34.4),float2(18,-33.9),float2(33,-33.6),float2(48,-38.2),float2(63,-33.8),float2(85,-38.4),float2(105,-33.8),float2(127,-37.4),float2(150,-35.0),float2(174,-37.8),float2(205,-35.0)};
 float ShoreX(float z)
 {
  if(z<=SHORE[0].x)return SHORE[0].y;if(z>=SHORE[12].x)return SHORE[12].y;
  int i=0;[unroll]for(int k=0;k<12;k++)if(z>SHORE[k+1].x)i=k+1;
  float p0=SHORE[max(i-1,0)].y,p1=SHORE[i].y,p2=SHORE[i+1].y,p3=SHORE[min(i+2,12)].y;
  float t=(z-SHORE[i].x)/(SHORE[i+1].x-SHORE[i].x);
  return .5*(2*p1+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t);
 }
 half4 frag(V i):SV_Target
 {
  // Round 9: fog per pixel. The sea is one large box, so fog interpolated from its far corners hazed
  // the whole surface.
  i.fog=ComputeFogFactor(TransformWorldToHClip(i.world).z);
  float2 p=i.world.xz;float t=_Time.y;
  float shore=ShoreX(p.y);
  float depth=max(0,shore-p.x);
  float broad=BrushNoise(p*.065+float2(t*.006,-t*.004));
  float drift=BrushNoise(p*float2(.17,.31)+float2(-t*.018,t*.024));
  float phase=dot(p,float2(.38,.065))-t*.42+broad*2.7;
  float crossing=dot(p,float2(.19,-.32))-t*.28+drift*1.6;
  float waves=sin(phase)*.65+sin(crossing)*.35;
  float shallows=1-smoothstep(0,34,depth);
  half3 c=lerp(_DeepColor.rgb,_BaseColor.rgb,saturate(shallows*.8+.12+waves*.07+(broad-.5)*.12));
  // Round 86 (review 82, all three: the deep water "a flat cobalt", "electric blue", b3, b4): since round 84 the open sea is
  // fully opaque past 18-36 m, so it showed the plain deep colour edge to edge. Out there it now drifts between deep blue and
  // a deeper teal in broad, slow patches (two octaves, tens of metres across), the way open water varies under a sunset.
  float open=smoothstep(8,60,depth);
  float swell=BrushNoise(p*.019+float2(t*.004,-t*.003))*.6+BrushNoise(p*.047+3.3)*.4;
  c=lerp(c,half3(.04,.42,.68)*(.86+swell*.3),open*smoothstep(.2,.7,swell)*.8);   // Nov 15 daylight pass: azure, not deep teal

  // Irregular elongated brush marks replace the uniform lattice of bright specks.
  // Round 16: rotated and warped by a second noise, so the strokes curve; axis-aligned stretched noise drew
  // straight-edged rectangles that the sun sheen lit up as bright bars (reported by four reviews).
  float2 pr=mul(float2x2(.86,-.5,.5,.86),p);
  float2 brushUV=pr*float2(.38,1.0)+float2(-t*.07,t*.035)+float2(broad*1.7,drift*.7)+float2(BrushNoise(p*.21+3.1),BrushNoise(p*.23+7.7))*float2(2.4,1.2);
  float brush=BrushNoise(brushUV);
  float brushAA=max(.035,fwidth(brush));
  float detail=1-smoothstep(.25,1.2,length(fwidth(brushUV)));
  float marks=smoothstep(.7-brushAA,.88+brushAA,brush)*detail;
  c+=half3(.62,.9,.95)*marks*.15;

  // A small directional sheen follows the actual golden-hour sun. No reflection camera.
  float3 view=GetWorldSpaceNormalizeViewDir(i.world);
  float3 normal=normalize(float3(cos(phase)*.09,1,cos(crossing)*.07));
  // Round 8: the sea receives shadows, so rock and cliff shadows continue onto the water.
  Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
  c*=lerp(.85,1,sun.shadowAttenuation);   // round 27: softer; long low-sun stack shadows read as light-shaft wedges (review 25)
  float sheen=pow(saturate(dot(normal,normalize(sun.direction+view))),128);
  c+=sun.color*sheen*(.045+marks*.07);

  // Broken crests roll toward the sloping beach; their width is filtered at distance.
  float foamPhase=depth*2.1+t*.8+BrushNoise(float2(p.y*.23,t*.025))*2.2;
  float crest=sin(foamPhase);float foamAA=max(.035,fwidth(crest));
  float foam=smoothstep(.82-foamAA,.97+foamAA,crest);
  foam*=smoothstep(.30,.64,BrushNoise(p*float2(.55,.85)+float2(t*.04,-t*.055)));
  foam*=(1-smoothstep(.2,4.8,depth))*detail;
  c=lerp(c,half3(.95,.99,1.),foam*.68);

  // True water depth from the scene depth buffer: bright turquoise shallows over sand,
  // and a soft, lapping foam line wherever the water touches the beach or a rock.
  float2 suv=GetNormalizedScreenSpaceUV(i.p);
  float sceneEye=LinearEyeDepth(SampleSceneDepth(suv),_ZBufferParams);
  float vdepth=max(0,(sceneEye-i.eye)*abs(view.y));
  // See-through shallows (Tchia): the sand bed shows through, tinted turquoise, fading with depth.
  float2 refractUV=suv+float2(cos(phase),cos(crossing))*.004*saturate(vdepth);
  half3 bed=SampleSceneColor(refractUV);
  // Round 51 (review 47: "opaque; no sand visible through the shallows"): a real seabed now lies under the water
  // (Round51Pass), so the bed is absorbed more gently, red first, and stays visible out to about 4.6 m depth.
  // A first try (.9,.32,.28) left the shallows a pale yellow-green: red goes first, and the bed takes a cool tint.
  // Round 53: the bed now reaches in under the swash (Round51Pass), so the thinnest water showed raw cream sand and the
  // white lace vanished into it; the bed is absorbed as if 18 cm deeper, so even the swash is faintly turquoise.
  half3 transmit=exp(-(vdepth+.18)*half3(1.6,.33,.24));
  half3 seen=bed*transmit*half3(.72,.98,1.02)+half3(.12,.8,.82)*(1-transmit)*.9;   // round 8: wet sand under the swash stays warm, not milky green
  // Round 52 (review 48, all three: the stack shadows read as "inverted, hard-edged bright wedges"): the seabed no longer
  // takes the sun shadow (its material's _SunShadow is 0), so the bed is evenly lit; the shade is laid on the water
  // surface instead, softly (to 86%), the way a shadow darkens clear water.
  seen*=lerp(.86,1,sun.shadowAttenuation);
  // Round 84 (review 80, all three: "a hard-edged lighter turquoise sheet" round the island base, b5; two also b1, b3): far
  // offshore the sea is open and deep, so the see-through bed fades out with distance from the beach (18 to 36 m, where the
  // seabed is already about 3 m down). The islands' underwater flanks, which used to show as pale skirts ending in a hard rim,
  // now sit in opaque sea; the shallows along the beach and round the near rocks are unchanged. The distance is taken at
  // whichever is further out, the water surface or what lies under it: from the beach a sightline can enter the water close
  // in and meet an island's flank only a little under the surface, far out (eye depth scales linearly along the ray).
  float3 bedPos=_WorldSpaceCameraPos+(i.world-_WorldSpaceCameraPos)*min(sceneEye/max(i.eye,.01),1000.);
  float bedOff=ShoreX(bedPos.z)-bedPos.x;
  c=lerp(seen,c,max(smoothstep(.3,4.6,vdepth),smoothstep(18,36,max(depth,bedOff))));
  // Round 86: open water seen at a low angle mirrors the sky (Fresnel), so the far sea takes the sunset's lilac and peach
  // instead of staying saturated cobalt to the horizon. The beach shallows keep their own turquoise.
  float fres=pow(1-saturate(dot(normal,view)),5);
  c=lerp(c,SampleSH(reflect(-view,normal)),fres*.36*smoothstep(12,45,depth));
  // Sun path: broken glints where ripple normals catch the low sun.
  // Round 16: two rotated, finer octaves; one grid-aligned octave drew the glints as straight-edged bars.
  float2 g1=mul(float2x2(.8,-.6,.6,.8),p)*3.1,g2=mul(float2x2(.28,-.96,.96,.28),p)*5.3;
  float rx=(BrushNoise(g1+float2(t*.6,0))*.6+BrushNoise(g2-float2(t*.4,t*.2))*.4)-.5,rz=(BrushNoise(g1.yx+float2(0,-t*.5))*.6+BrushNoise(g2.yx+float2(t*.3,0))*.4)-.5;
  float3 rip=normalize(float3(rx*.55+cos(phase)*.12,1,rz*.55+cos(crossing)*.1));
  float glint=pow(saturate(dot(reflect(-view,rip),sun.direction)),220);
  float glintAA=1-smoothstep(.4,1.6,length(fwidth(g2)));
  c+=sun.color*smoothstep(.15,.6,glint)*2.2*lerp(.35,1,glintAA)*smoothstep(3.,12.,i.eye)*sun.shadowAttenuation;   // round 6: no huge glint shapes right at the camera
  // Round 44 (review 40, all three: "a constant-width ribbon with a repeating grey blotch pattern"): the wash reach varies
  // along the shore at two scales, so the foam band widens into tongues and thins to a line.
  float reach=BrushNoise(float2(p.x*.045+p.y*.06,p.y*.045-p.x*.03))*.75+BrushNoise(float2(p.y*.19,p.x*.17))*.25;
  float lap=.025+.15*smoothstep(.2,.85,reach)+.04*sin(t*.7+p.y*.11+BrushNoise(float2(p.y*.15,t*.05))*2.0);
  // Clamp: across underwater silhouettes the depth jumps, and an unclamped fwidth paints false foam.
  float edgeAA=clamp(fwidth(vdepth),.01,.04);
  // Round 5 (independent review: "a constant-width plastic tube"): only a thin line right at the
  // water's edge is solid; behind it the foam is lace with holes that drift, so its width varies.
  float edgeLine=1-smoothstep(.012,.03+edgeAA,vdepth);
  float contact=1-smoothstep(lap-edgeAA,lap+.04+edgeAA,vdepth);
  float lace=BrushNoise(p*3.4+float2(t*.12,-t*.08))*.6+BrushNoise(p*7.7-float2(t*.07,t*.1))*.4;
  float laceMask=smoothstep(.5,.56,lace)*(1-smoothstep(.66,.72,lace));   // round 44: crisp lace threads with holes, not grey blotches
  float trail=smoothstep(lap+.05,lap+.09,vdepth)*(1-smoothstep(lap+.11,lap+.2,vdepth));
  float breakup=smoothstep(.25,.6,BrushNoise(p*float2(1.6,.9)+float2(t*.15,t*.05)));
  float shoreFoam=saturate(edgeLine*.95+contact*laceMask*.95+trail*breakup*laceMask*.7);
  half3 foamColour=half3(.99,.96,.9)*(.75+.35*sun.color);
  c=lerp(c,foamColour,shoreFoam*.95);
  return half4(MixFog(c,i.fog),1);
 }
 ENDHLSL
 } }
}
