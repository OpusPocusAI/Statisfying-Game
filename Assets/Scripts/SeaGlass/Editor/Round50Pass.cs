using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 50, from the forty-sixth review (all three reviewers: the round 49 fused groves read as "fused knobs" and
    // "green lobes pasted on purple mounds"; two ask for layered canopies with distinct pine or palm silhouettes, as on
    // Tchia's islands):
    // - Island pines (round25.py, far_isle_0..2): the islands are scaled about 24 x 40 m across and 36-54 m tall in the
    //   scene, so the round 49 crowns were only 2-3 m wide. Each island now carries 5-11 individual umbrella pines
    //   built in metres (a leaning tapered trunk and a broad flat-topped crown of 3-4 faceted pads in tiers, like the
    //   beach's coastal pines), tallest in the grove centres and shorter toward the edges, plus a few lone pines on the
    //   ledges. Trunks use the island's sandstone slot, which reads as bark at that distance.
    // - Island trees material: the pads are flat-shaded, so the sunlit top tint starts a little higher (.25) and only
    //   the upward facets take it; the side facets keep the green base, which gives each crown a light and shadow side.
    // Safe to re-run.
    public static class Round50Pass
    {
        [MenuItem("Sea Glass/Art Direction/54 Round 50: island pines in tiers")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();   // picks up the re-exported far_isle_0..2 with pines

            var trees = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Island trees.mat");
            if (trees)
            {
                trees.SetFloat("_TopStart", .25f);
                EditorUtility.SetDirty(trees);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 50: island groves replaced by tiers of umbrella pines");
        }
    }
}
