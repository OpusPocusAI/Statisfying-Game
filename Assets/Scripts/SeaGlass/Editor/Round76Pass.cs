using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 76, from the seventy-second review (all three: the b1, b2 and b7 foregrounds "large near-single-colour areas",
    // against 5.3's rule on one flat colour over a third of the screen). Painterly Lit gains _SandDetail (mid-scale tone,
    // damp hollows and shell grit near the player); this pass turns it on for the beach and dune sands only.
    // Safe to re-run.
    public static class Round76Pass
    {
        static readonly (string path, float amount)[] Sands =
        {
            ("Assets/Materials/SeaGlass/Apricot sand.mat", 1f),
            ("Assets/Art/ShorelinePolish/Dry dune sand.mat", .8f),
            ("Assets/Art/ShoreTransition/Warm tidal sand.mat", .6f),
            ("Assets/Art/Beach/Sun-faded road.mat", .45f),   // its sandy shoulders fill the b7 foreground
        };

        [MenuItem("Sea Glass/Art Direction/73 Round 76: sand foregrounds")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            int n = 0;
            foreach (var (path, amount) in Sands)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m || !m.HasProperty("_SandDetail")) continue;
                m.SetFloat("_SandDetail", amount); EditorUtility.SetDirty(m); n++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 76: sand detail on " + n + " materials");
        }
    }
}
