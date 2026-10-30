using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 34, the complaints every recent review repeated (three reviewers averaged):
    // - "Footprints read as flat floating tan coins": each footprint becomes a foot-shaped dent drawn on a flat quad,
    //   shaded on the side away from the sun with a lit lip of sand (FootprintDent.shader), no disc edge.
    // - "The same three-pebble cluster stamped dozens of times": the beach scatter mixes five cluster shapes
    //   (round34.py), each turned and sized at random, and about a third of them are removed.
    // - "The sleeve is a long segmented blue pipe": the glove model's sleeve (glove.py) is one shorter, tapered cloth
    //   sleeve with soft wrinkles and a loose cuff; this pass re-imports it.
    // - "The wing mirrors are flat black paddles": ivory casings with pale glass.
    // Safe to re-run.
    public static class Round34Pass
    {
        const string Folder = "Assets/Art/Round34";

        [MenuItem("Sea Glass/Art Direction/44 Round 34: footprint dents, varied pebbles, sleeve, mirrors")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round34");
            ModelLibraryImporter.ImportAll();
            Footprints();
            Pebbles();
            Mirrors();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Material Mat(string name) => AssetDatabase.FindAssets("t:Material " + name).Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(m => m && m.name == name);

        static void Footprints()
        {
            string path = Folder + "/Footprint dent.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(Shader.Find("Sea Glass/Footprint Dent")); AssetDatabase.CreateAsset(mat, path); }
            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            int n = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.name == "Footprint").ToList())
            {
                if (f.sharedMesh == quad) continue;
                var t = f.transform; var ob = f.sharedMesh.bounds; var s = t.lossyScale;
                float w = ob.size.x * s.x, l = ob.size.z * s.z; float yaw = t.eulerAngles.y;
                f.sharedMesh = quad; f.GetComponent<MeshRenderer>().sharedMaterial = mat;
                f.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                t.rotation = Quaternion.Euler(90, yaw, 0);
                var parentScale = t.parent ? t.parent.lossyScale : Vector3.one;
                t.localScale = new Vector3(w * 1.25f / parentScale.x, l * 1.15f / parentScale.z, 1);
                t.position += Vector3.up * .006f;
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(t); n++;
            }
            Debug.Log("Round 34: " + n + " footprints as dents.");
        }

        static void Pebbles()
        {
            var variants = new[] { "pebble_cluster", "pebble_cluster_b", "pebble_cluster_c", "pebble_cluster_d", "pebble_cluster_e" }.Select(ModelLibraryImporter.Load).ToArray();
            if (variants.Any(v => !v)) return;
            var rng = new System.Random(3434);
            int kept = 0, hidden = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                         .Where(f => f.sharedMesh && variants.Contains(f.sharedMesh) && f.name != "Pebbles on the bowl floor").OrderBy(f => f.transform.position.x * 31 + f.transform.position.z).ToList())
            {
                var t = f.transform;
                if (f.name == "Sand pebbles" && rng.NextDouble() < .35) { if (f.gameObject.activeSelf) { f.gameObject.SetActive(false); hidden++; } continue; }
                var r = f.GetComponent<Renderer>(); float bottom = r.bounds.min.y;
                f.sharedMesh = variants[rng.Next(variants.Length)];
                t.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0);
                t.localScale = t.localScale.normalized * t.localScale.magnitude * (.75f + (float)rng.NextDouble() * .5f);
                t.position += Vector3.up * (bottom - r.bounds.min.y);
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(t); kept++;
            }
            Debug.Log("Round 34: " + kept + " pebble clusters varied, " + hidden + " removed.");
        }

        static void Mirrors()
        {
            var truck = Object.FindAnyObjectByType<UtilityTruck>(); if (!truck) return;
            var ivory = Mat("Ivory enamel"); var glass = Mat("Cream headlamp");
            foreach (var r in truck.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.name == "Mirror casing" && ivory) { r.sharedMaterial = ivory; EditorUtility.SetDirty(r); }
                if (r.name == "Mirror inset" && glass) { r.sharedMaterial = glass; EditorUtility.SetDirty(r); }
            }
        }
    }
}
