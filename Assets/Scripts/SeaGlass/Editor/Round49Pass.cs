using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 49, from the forty-fifth review (all three reviewers: the round 48 island canopies are no longer black, but
    // they now read as "stacked green discs", "green lumps sitting on the rock" and "lump clusters"; Tchia's foliage is
    // layered, with a lit top over shadowed volume):
    // - Island groves (round25.py, far_isle_0..2): each grove is one fused canopy mass instead of 4-7 equal domes.
    //   Crowns of different heights (taller than wide) are voxel-merged and smoothed into rounded lobes, so a grove has
    //   a lobed, broken silhouette that rises off the hill, and the mass is rooted a third of the way into the ground
    //   rather than resting on it. A crinkled crust was tried and dropped: at 100-170 m it only scatters the normals and
    //   the whole mass reads dark; the lobes come from the crown overlap, with fully smooth shading.
    // - Island trees material: the beach views look west into the sun, so the sides of the islands facing the camera
    //   are shadow sides. The base rises to a lighter green so the shaded lobes stay green rather than near-black, the
    //   sunlit top tint starts at a normal.y of .2 (round 48's .15 painted whole domes one flat yellow-green; .42 left
    //   the crowns dark), and the warm rim light rises so the crowns read backlit at the edges.
    // Safe to re-run.
    public static class Round49Pass
    {
        [MenuItem("Sea Glass/Art Direction/53 Round 49: fused island groves")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();   // picks up the re-exported far_isle_0..2 with fused groves

            var trees = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Island trees.mat");
            if (trees)
            {
                trees.SetColor("_BaseColor", new Color(.50f, .72f, .50f));
                trees.SetColor("_TopTint", new Color(.76f, .84f, .44f, .9f));
                trees.SetFloat("_TopStart", .1f);
                trees.SetFloat("_Rim", .6f);
                EditorUtility.SetDirty(trees);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 49: island groves fused into lobed canopy masses; lighter, rim-lit crowns");
        }
    }
}
