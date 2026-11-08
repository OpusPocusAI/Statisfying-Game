// Stylized golden-hour lighting for all matte surfaces. See Documentation/ART_DIRECTION.md.
// Keeps URP Lit property names (_BaseColor, _BaseMap, _Smoothness, _EmissionColor) so game
// scripts and property blocks keep working.
Shader "Sea Glass/Painterly Lit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Smoothness("Sheen", Range(0, 1)) = 0.15
        [HideInInspector] _Metallic("Metallic (unused)", Range(0, 1)) = 0
        [HDR] _EmissionColor("Emission", Color) = (0, 0, 0, 1)
        _GradientStrength("Base-to-crown gradient", Range(0, 0.6)) = 0.22
        _GradientHeight("Gradient height (m)", Float) = 2
        _Variation("Painted variation", Range(0, 0.3)) = 0.07
        _Rim("Rim light", Range(0, 2)) = 1
        _WorldMapping("World-space texture (0 off, 1 ground, 2 walls)", Float) = 0
        _WorldTile("World tile size (m)", Float) = 7
        _GroundPatches("Broad ground patches", Range(0, 0.4)) = 0
        _SandDetail("Beach sand detail: mid-scale tone, damp hollows, wrack lines, grit (round 76)", Range(0, 1)) = 0
        _AOScale("Screen-space occlusion strength (round 77; low on smooth painted panels)", Range(0, 1)) = 1
        [HideInInspector] _Cutoff("Alpha cutoff", Range(0, 1)) = 0.5
        [HideInInspector] _Surface("Surface", Float) = 0
        [HideInInspector] _SpecColor("Spec", Color) = (0, 0, 0, 0)
        [HideInInspector] _Cull("Cull", Float) = 2
        _TopTint("Top colour (moss on upward faces, a = amount)", Color) = (0.42, 0.55, 0.3, 0)
        _TopStart("Top colour starts at normal.y", Range(0, 1)) = 0.7
        _SunShadow("Receives sun shadow", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "SimpleLit" "IgnoreProjector" = "True" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex PainterlyVertex
            #pragma fragment PainterlyFragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #pragma multi_compile_instancing

            #include "PainterlyInput.hlsl"
            #include "PainterlyForward.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "PainterlyInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            #include "PainterlyInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_instancing
            #include "PainterlyInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
