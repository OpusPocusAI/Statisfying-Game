using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 11, from the seventh independent blind review (ArtReference/comparison.md). Run after passes
    // 20-23 (it re-runs 20 and 21 for the fuller, chunkier tray piles). Models: ArtSource/blender/round11.py.
    // Shader changes this round: FrostedGlass (rim on cloudy glass), BasinWater. Safe to re-run.
    public static class Round11Pass
    {
        const string Folder = "Assets/Art/Round11";

        [MenuItem("Sea Glass/Art Direction/24 Round 11: seventh review fixes")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round11");
            ModelLibraryImporter.ImportAll();
            Round6Pass.Apply(); Round7Pass.Apply();   // tray piles: 12 per tray, chunkier

            Driftwood();
            Fenders();
            GrassDrifts();
            Brush();
            var w = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CoastalPolish/Still basin water.mat");
            if (w) { w.SetColor("_BaseColor", new Color(.42f, .72f, .77f)); w.SetColor("_Deep", new Color(.1f, .36f, .44f)); EditorUtility.SetDirty(w); }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Two overlapping logs (a cube and a hexagonal prism) became one bleached, forked branch.
        static void Driftwood()
        {
            var m = ModelLibraryImporter.Load("driftwood_branch"); if (!m) return;
            var mats = new[] { Paint("Driftwood bleached", new Color(.78f, .72f, .64f)), Paint("Driftwood end", new Color(.6f, .5f, .4f)) };
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (r.name == "Sun-bleached driftwood") { r.enabled = false; EditorUtility.SetDirty(r); }
                if (r.name == "Tide worn timber")
                {
                    var f = r.GetComponent<MeshFilter>(); if (f.sharedMesh == m) continue;
                    var p = r.bounds.center; f.sharedMesh = m; r.sharedMaterials = mats;
                    r.transform.localScale = Vector3.one; r.transform.rotation = Quaternion.Euler(0, 63, 0);
                    r.transform.position = new Vector3(p.x, Round3Pass.Ground(p) - .04f, p.z);
                    EditorUtility.SetDirty(r.transform); EditorUtility.SetDirty(f);
                }
            }
        }

        // Rounded fender shells over each wheel instead of the box arches.
        static void Fenders()
        {
            var m = ModelLibraryImporter.Load("truck_fender"); if (!m) return;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.name == "Ivory wheel arch"))
            {
                if (f.sharedMesh == m) continue;
                var b = f.GetComponent<Renderer>().bounds;
                f.sharedMesh = m;
                f.transform.rotation = f.transform.parent ? f.transform.parent.rotation : Quaternion.identity;
                f.transform.localScale = Vector3.one;
                // Wheel centre sits ~0.3 m below the old arch centre; the fender's origin is the wheel centre.
                f.transform.position = new Vector3(b.center.x, b.center.y - .32f, b.center.z);
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.transform);
            }
        }

        // Grass grew as evenly spaced single clumps. Grow drifts: 2-4 smaller companions around about half of
        // the beach clumps, each with its own size and turn.
        static void GrassDrifts()
        {
            var old = GameObject.Find("ART • grass drifts"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • grass drifts").transform;
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => i.transform.position).ToList();
            var rng = new System.Random(1111);
            foreach (var g in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Dune grass cluster").OrderBy(r => r.transform.position.z).ToList())
            {
                if (rng.NextDouble() > .5) continue;
                var mf = g.GetComponent<MeshFilter>(); var c = g.transform.position;
                int n = 2 + rng.Next(3);
                for (int k = 0; k < n; k++)
                {
                    float a = (float)rng.NextDouble() * Mathf.PI * 2, d = .5f + (float)rng.NextDouble() * .9f;
                    var p = c + new Vector3(Mathf.Cos(a) * d, 0, Mathf.Sin(a) * d);
                    if (items.Any(it => (new Vector2(it.x, it.z) - new Vector2(p.x, p.z)).sqrMagnitude < 1.2f)) continue;
                    p.y = Round3Pass.Ground(p);
                    var go = new GameObject("Grass drift"); go.transform.SetParent(root);
                    go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0));
                    go.transform.localScale = g.transform.lossyScale * (.45f + (float)rng.NextDouble() * .4f);
                    go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                    var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = g.sharedMaterials; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        // A larger scrub brush head; the glove keeps its size.
        static void Brush()
        {
            var eq = Object.FindFirstObjectByType<EquipmentVisuals>(); if (!eq || !eq.brush) return;
            var t = eq.brush.transform; if (t.localScale.x > 1.3f) return;
            t.localScale = Vector3.one * 1.45f;
            foreach (Transform c in t) if (c.name == "Gloved hand") c.localScale /= 1.45f;
            EditorUtility.SetDirty(t);
        }

        static Material Paint(string name, Color c)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .05f); m.SetFloat("_GradientStrength", .15f); m.SetFloat("_GradientHeight", .5f); m.SetFloat("_Variation", .14f);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
