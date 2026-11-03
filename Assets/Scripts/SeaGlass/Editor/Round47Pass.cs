using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 47, from the forty-third review (two reviewers: the sand "is a saturated apricot-yellow, not the cream
    // target", and the shore boulders "render near-black with no lit top"):
    // - Sand: the materials were already cream, but the golden-hour sun (1, .82, .6) multiplies every colour, so cream
    //   came out apricot. The sand albedo takes a slight cool, blue-grey bias so the LIT result lands near the cream
    //   target (#EED3A2) under that sun; shadows stay purple, as the brief asks. Stronger broad patches.
    // - Boulders: the material carried a 45% base-to-crown darkening over 3.2 m, meant for cliffs; a boulder on the sand
    //   sits in the dark end of it. The gradient is nearly off, the base value is raised, and upward faces get a
    //   sun-bleached warm cap (the per-pixel top tint), so each rock shows a lit top plane over a cooler side.
    // - Shader change this round (PainterlyForward.hlsl): ripples at two scales that turn and fade from patch to patch,
    //   plus a broad tonal drift.
    // Safe to re-run.
    public static class Round47Pass
    {
        [MenuItem("Sea Glass/Art Direction/51 Round 47: cream sand, lit boulders")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");

            Sand("Assets/Materials/SeaGlass/Apricot sand.mat", new Color(.84f, .85f, .90f), .46f);
            Sand("Assets/Art/ShorelinePolish/Dry dune sand.mat", new Color(.85f, .85f, .89f), .36f);
            // The wet-sand shoulder along the water: darker, slightly cooler damp cream (it was an orange band once the
            // dunes went cream), with fewer ripples, as damp sand has.
            Sand("Assets/Art/ShoreTransition/Warm tidal sand.mat", new Color(.66f, .64f, .66f), .22f);

            var boulder = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Beach/Shore boulder.mat");
            if (boulder)
            {
                boulder.SetColor("_BaseColor", new Color(.80f, .72f, .70f));
                boulder.SetFloat("_GradientStrength", .12f);
                boulder.SetFloat("_GradientHeight", 2.4f);
                boulder.SetFloat("_Rim", .7f);
                boulder.SetColor("_TopTint", new Color(.93f, .82f, .70f, .55f));   // sun-bleached top faces
                boulder.SetFloat("_TopStart", .45f);
                EditorUtility.SetDirty(boulder);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Round 47: sand cooled toward the cream target under the golden sun; boulders lifted with a lit top");
        }

        static void Sand(string path, Color baseColour, float patches)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path); if (!m) return;
            m.SetColor("_BaseColor", baseColour);
            m.SetFloat("_GroundPatches", patches);
            EditorUtility.SetDirty(m);
        }
    }
}
