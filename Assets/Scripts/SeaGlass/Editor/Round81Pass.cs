using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 81, from the seventy-seventh review (all three: the wet/dry sand boundary is "a hard, angular polygon edge" made of
    // "straight polygon segments rather than a soft curve", b3, b4 and b6). The wet-sand strip along the coast ("Soft wet-sand
    // shoreline", Warm tidal sand) lay on the dry beach with a different surface: a base colour 20% darker even at its dry end, its
    // own gradient texture instead of the world-mapped painted sand, weaker patches and detail, and three times the gloss. Where it
    // met the dry sand the colour jumped along its inland edge, which runs in straight segments between the mesh's rows.
    // Warm tidal sand now copies Apricot sand exactly and turns on _WetBand: Painterly Lit (PainterlyForward.hlsl) darkens, cools and
    // glosses it across the strip, starting a fifth of the way in, along a noise-roughened tide line, and eases the shading normal
    // over the strip's bend toward the water. Safe to re-run.
    public static class Round81Pass
    {
        [MenuItem("Sea Glass/Art Direction/78 Round 81: soft wet-sand edge")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var dry = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            var wet = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShoreTransition/Warm tidal sand.mat");
            if (!dry || !wet) throw new System.InvalidOperationException("Round 81: sand materials missing.");
            // Property by property (CopyPropertiesFromMaterial left _WetBand unsettable on this material).
            wet.SetTexture("_BaseMap", dry.GetTexture("_BaseMap"));
            wet.SetTextureScale("_BaseMap", dry.GetTextureScale("_BaseMap")); wet.SetTextureOffset("_BaseMap", dry.GetTextureOffset("_BaseMap"));
            wet.SetColor("_BaseColor", dry.GetColor("_BaseColor"));
            wet.SetColor("_TopTint", dry.GetColor("_TopTint"));
            foreach (var p in new[] { "_Smoothness", "_GradientStrength", "_GradientHeight", "_Variation", "_Rim", "_WorldMapping", "_WorldTile",
                                      "_GroundPatches", "_SandDetail", "_AOScale", "_TopStart", "_SunShadow" })
                if (dry.HasProperty(p)) wet.SetFloat(p, dry.GetFloat(p));
            wet.SetFloat("_WetBand", 1f);
            EditorUtility.SetDirty(wet);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 81: wet-sand band " + wet.GetFloat("_WetBand"));
        }
    }
}
