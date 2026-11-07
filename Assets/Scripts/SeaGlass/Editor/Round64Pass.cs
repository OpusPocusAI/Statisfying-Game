using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 64, from the sixtieth review (all three: the beach bushes are "lumpy single blobs", "uniform yellow-green
    // blobs", repeated across b2, b5, b7 and 02; Tchia's coastal shrubs are layered, sunlit foliage):
    // - coastal_shrub_leafy, _b, _c (ArtSource/blender/round64.py): three different bushes (a broad mound, a taller rounded
    //   bush, a two-lobed spreading one), each a fused, tiered inner mass shingled with broad leaves laid along the surface,
    //   with the bush's rounded volume normal on every vertex so it shades as one soft lit form with a leafy outline.
    // - Every bush in the scene takes one of the three, chosen from its position, so neighbours differ.
    // Safe to re-run.
    public static class Round64Pass
    {
        static readonly string[] Variants = { "coastal_shrub_leafy", "coastal_shrub_leafy_b", "coastal_shrub_leafy_c" };

        [MenuItem("Sea Glass/Art Direction/67 Round 64: layered beach bushes in three variants")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var meshes = Variants.Select(ModelLibraryImporter.Load).ToArray();
            if (meshes.Any(m => !m)) throw new System.InvalidOperationException("Run ArtSource/blender/round64.py first.");
            int[] counts = new int[3];
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                         .Where(f => f.sharedMesh && Variants.Contains(f.sharedMesh.name)))
            {
                var p = f.transform.position;
                int v = Mathf.Abs(Mathf.FloorToInt(p.x * 7.31f + p.z * 3.17f + Mathf.Sin(p.x * .37f + p.z * .91f) * 5)) % 3;
                f.sharedMesh = meshes[v]; counts[v]++;
                EditorUtility.SetDirty(f);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 64: bushes rebuilt; variants " + string.Join("/", counts));
        }
    }
}
