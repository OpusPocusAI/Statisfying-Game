using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 2 (from the side-by-side with Lonely Mountains: Downhill and A Short Hike, ArtReference/comparison.md):
    // headland boulders become layered rock stacks with bright tops and cool sides, and clumps of
    // yellow, coral and lilac flowers grow among the dune grass. Models: ArtSource/blender/rock_stacks.py.
    public static class RockAndFlowerPass
    {
        [MenuItem("Sea Glass/Art Direction/14 Rock stacks and flowers")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();

            var stacks = Enumerable.Range(0, 4).Select(k => ModelLibraryImporter.Load("rock_stack_" + k)).ToArray();
            var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Beach/Shore boulder.mat");
            stone.SetColor("_BaseColor", new Color(.74f, .68f, .66f)); stone.SetFloat("_GradientStrength", .45f); stone.SetFloat("_GradientHeight", 3.2f);
            stone.SetFloat("_Variation", .12f); EditorUtility.SetDirty(stone);
            int k = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.name == "Shore boulder"))
            {
                bool first = !f.sharedMesh || !f.sharedMesh.name.StartsWith("rock_stack");
                f.sharedMesh = stacks[k % 4];
                var s = f.transform.localScale;
                // Stacks are 1 m nominal and rise from z=0: a little taller than the old boulders (once only).
                if (first) f.transform.localScale = new Vector3(s.x * 1.05f, s.y * 1.25f, s.z * 1.05f);
                f.transform.rotation = Quaternion.Euler(0, k * 67 % 360, 0);
                EditorUtility.SetDirty(f.transform); k++;
            }

            var old = GameObject.Find("ART • dune flowers"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • dune flowers");
            var stem = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/StudioFurniture/Stem green.mat");
            var centre = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/StudioFurniture/Flower centre.mat");
            var petals = new[] { Mat("Petal sun yellow", new Color(1f, .82f, .2f)), Mat("Petal coral", new Color(.95f, .48f, .38f)), Mat("Petal lilac", new Color(.72f, .58f, .86f)) };
            var sprig = ModelLibraryImporter.Load("flower_sprig");
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => i.transform.position).ToList();
            var rng = new System.Random(31);
            int placed = 0;
            foreach (var g in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Dune grass cluster" || r.name == "Ridge grass" || r.name == "Roadside sea grass").ToList())
            {
                if (rng.NextDouble() > .45) continue;
                var p = g.transform.position + new Vector3((float)rng.NextDouble() - .5f, 0, (float)rng.NextDouble() - .5f) * .8f;
                if (items.Any(i => (new Vector2(i.x, i.z) - new Vector2(p.x, p.z)).sqrMagnitude < 2.2f)) continue;
                var go = new GameObject("Dune flowers");
                go.transform.SetParent(root.transform);
                go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0));
                { float w = 2.2f + (float)rng.NextDouble() * 1f; go.transform.localScale = new Vector3(w, 1.1f, w); }   // low, wide clumps (LMD)
                go.AddComponent<MeshFilter>().sharedMesh = sprig;
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = new[] { stem, petals[rng.Next(0, 3)], centre };
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
                placed++;
            }
            Debug.Log("Rock stacks: " + k + ", flower clumps: " + placed);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Material Mat(string name, Color c)
        {
            string path = "Assets/Art/StudioFurniture/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .2f); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
