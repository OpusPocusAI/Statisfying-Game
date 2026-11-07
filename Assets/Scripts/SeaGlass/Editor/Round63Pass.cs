using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 63, from the fifty-ninth review (all three, and every review since 46: the island trees are "stacks of flat
    // plates", "flat umbrella layers on bare rock", far from Tchia's layered foliage):
    // - far_isle_0..2 (round25.py): the faceted umbrella pines are replaced by groves of broadleaf crowns, each crown a
    //   broad lower tier and a rounded upper tier of lobes with undergrowth at the trunk foot, every grove fused into one
    //   crinkled, smooth-shaded canopy surface over the hilltop, plus lone crowns on the ledges.
    // - Island trees material: a fresher green body and a lighter, less mustard sunlit top that starts lower and blends
    //   in at a lower amount, so the smooth crowns grade from lit tops to shaded sides instead of a hard khaki band.
    // - No waterline talus boulders: they read as dark "hexagon patches" on the cliff (b8, every review since 45).
    // Safe to re-run.
    public static class Round63Pass
    {
        [MenuItem("Sea Glass/Art Direction/66 Round 63: layered island canopies")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();   // picks up the re-exported far_isle_0..2
            var trees = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Island trees.mat");
            if (trees)
            {
                trees.SetColor("_BaseColor", new Color(.36f, .64f, .44f));
                trees.SetColor("_TopTint", new Color(.66f, .84f, .42f, .6f));
                trees.SetFloat("_TopStart", .4f);
                EditorUtility.SetDirty(trees); AssetDatabase.SaveAssetIfDirty(trees);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 63: island groves rebuilt as layered, fused canopies");
        }
    }
}
