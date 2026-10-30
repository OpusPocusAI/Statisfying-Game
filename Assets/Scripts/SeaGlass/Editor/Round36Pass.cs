using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 36, from the thirty-second review and the owner's note on hand-held models:
    // - toy_spade and gooseneck_tap (round36.py) are re-imported: the spade is one fused shape with a dished blade,
    //   rolled rim, foot step, collar and D-grip; the tap's riser is twice as tall, so its base and cross handle
    //   stand above the basin rim (both models keep their pivots, so the scene placement is unchanged).
    // - The pebbles on the basin floor read as "dark paw prints" and are removed.
    // StudioStation seats the working glass deeper, so the water visibly cuts across it. Safe to re-run.
    public static class Round36Pass
    {
        [MenuItem("Sea Glass/Art Direction/45 Round 36: fused spade, tall tap, clean basin")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == "Pebbles on the bowl floor").ToList())
                Object.DestroyImmediate(t.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
