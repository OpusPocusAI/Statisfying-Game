using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Rock and island shapes pass (owner, November 15: Tchia is the art target). The offshore islands' cliffs were smooth,
    // even slopes, so they read as green-capped cones. Stylised tropical islands show their rock as stacked strata that step
    // out into ledges, cut by a few steep gullies. Every island vertex above the water moves by one shared field of world
    // position (never by its normal), so the rock, grass and tree submeshes, which meet along duplicated seam vertices, move
    // identically and no gaps open. The underwater flanks (Round 84) are untouched. Safe to re-run: it always starts from a
    // saved copy of each island's mesh as it was before this pass.
    public static class TropicalIslandPass
    {
        const string Folder = "Assets/Art/Terrain/Island flanks";
        const float Surface = -0.85f, Band = 5f, Shelf = 3.2f, Gully = 2.6f;

        [MenuItem("Sea Glass/Art Direction/Rocks and landforms: island strata and gullies")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            var filters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(f => f.sharedMesh && AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(Folder + "/Island ")
                            && !AssetDatabase.GetAssetPath(f.sharedMesh).Contains("source")).ToList();
            int n = 0;
            foreach (var f in filters)
            {
                string outPath = AssetDatabase.GetAssetPath(f.sharedMesh), name = f.sharedMesh.name;
                string srcPath = $"{Folder}/{name} (tropical source).asset";
                var src = AssetDatabase.LoadAssetAtPath<Mesh>(srcPath);
                if (!src) { src = Object.Instantiate(f.sharedMesh); src.name = name + " (tropical source)"; AssetDatabase.CreateAsset(src, srcPath); }

                var t = f.transform; var v = src.vertices;
                var world = v.Select(p => t.TransformPoint(p)).ToArray();
                float top = world.Max(p => p.y);
                var c = new Vector2(world.Average(p => p.x), world.Average(p => p.z));
                float seed = (c.x * .013f + c.y * .007f) % 10f, phase = seed * 2.3f;
                for (int k = 0; k < v.Length; k++)
                {
                    var w = world[k];
                    float h = w.y - Surface; if (h <= 0) continue;
                    var d = new Vector2(w.x - c.x, w.z - c.y); float r = d.magnitude; if (r < .5f) continue; d /= r;
                    float a = Mathf.Atan2(d.y, d.x), t01 = Mathf.Clamp01(h / (top - Surface));
                    // Strata: bands that pull in at their foot, so each band's top stands out as a ledge; the lines tilt and wander.
                    float ts = (h + Mathf.Cos(a - phase) * 1.6f + (Mathf.PerlinNoise(w.x * .05f + seed, w.z * .05f) - .5f) * 2.4f) / Band;
                    float frac = ts - Mathf.Floor(ts);
                    float pull = Shelf * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(frac / .22f)));
                    // Gullies: a few steep grooves down the cliff, fading out on the summit.
                    float g = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        float ga = seed * 1.7f + i * 1.31f + Mathf.Sin(seed + i) * .4f, da = Mathf.DeltaAngle(a * Mathf.Rad2Deg, ga * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                        g += Mathf.Exp(-(da / .07f) * (da / .07f)) * (.6f + .4f * Mathf.Sin(i * 2.1f + seed));
                    }
                    float weight = Mathf.SmoothStep(0, 1, Mathf.Clamp01(h / 1.6f)) * (1 - Mathf.SmoothStep(.7f, .95f, t01));
                    float inward = (pull + g * Gully) * weight;
                    w.x -= d.x * inward; w.z -= d.y * inward;
                    v[k] = t.InverseTransformPoint(w); n++;
                }
                var mesh = f.sharedMesh;
                mesh.Clear(); EditorUtility.CopySerialized(src, mesh); mesh.name = name;
                mesh.vertices = v; mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
                EditorUtility.SetDirty(mesh);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log($"Tropical island pass: {filters.Count} islands, {n} vertices shaped");
        }
    }
}
