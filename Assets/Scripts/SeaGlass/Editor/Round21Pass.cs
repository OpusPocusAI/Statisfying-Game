using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 21, from the nineteenth review (three reviewers averaged; all three named these):
    // - Glass "reads as candy, the same inflated lentil": every held, found, carried and sorted piece now uses
    //   six worn slab shapes (ArtSource/blender/round21.py) - flat faces, rounded edges, some with a broken edge.
    // - Grass tufts read near-black and identical: three colour variants (fresh green, olive, sun-dried straw)
    //   with lighter bases and warmer tips, assigned at random, each with a slight lean (sizes already vary).
    // Shader changes this round: FrostedGlass (light centre, saturated edges; soft brush strokes with a grime
    // rim) and BasinWater (the floor shows through the middle). Safe to re-run.
    public static class Round21Pass
    {
        const string Folder = "Assets/Art/Round21";

        [MenuItem("Sea Glass/Art Direction/33 Round 21: glass slabs and grass variety")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round21");
            ModelLibraryImporter.ImportAll();
            var slabs = Enumerable.Range(0, 6).Select(k => ModelLibraryImporter.Load("glass_slab_" + k)).ToArray();
            if (slabs.Any(m => !m)) throw new System.InvalidOperationException("Import the glass_slab models first.");

            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            for (int i = 0; i < catalog.glass.Length; i++) catalog.glass[i].pebbleMesh = slabs[(i * 5 + 2) % slabs.Length];
            EditorUtility.SetDirty(catalog);
            ShoreFinds(catalog);
            Trays(slabs);
            Grass();
            TruckRefinePass.Apply();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void ShoreFinds(GameCatalog catalog)
        {
            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(i => i.glass && i.visual))
            {
                var def = catalog.glass.FirstOrDefault(d => d.id == item.id); var f = item.visual.GetComponent<MeshFilter>();
                if (def == null || !f || f.sharedMesh == def.pebbleMesh) continue;
                var old = f.sharedMesh.bounds.size; float oldSize = Mathf.Max(old.x, old.y) * item.visual.localScale.x;
                var nb = def.pebbleMesh.bounds.size;
                f.sharedMesh = def.pebbleMesh;
                item.visual.localScale = Vector3.one * oldSize / Mathf.Max(nb.x, nb.y);
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(item.visual);
            }
        }

        // Keep each spare's place in the heap; swap its mesh for a slab at the same size, resting on the felt.
        static void Trays(Mesh[] slabs)
        {
            var rng = new System.Random(2121);
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name.StartsWith("Spare ") && r.name.EndsWith(" glass")))
            {
                var f = r.GetComponent<MeshFilter>(); var t = r.transform;
                float size = Mathf.Max(f.sharedMesh.bounds.size.x, f.sharedMesh.bounds.size.y) * t.lossyScale.x;
                float bottom = r.bounds.min.y;
                var mesh = slabs[rng.Next(slabs.Length)];
                f.sharedMesh = mesh;
                t.localScale = Vector3.one; float unit = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y) * t.lossyScale.x;
                t.localScale = Vector3.one * (size / Mathf.Max(unit, 1e-4f));
                t.position += Vector3.up * (bottom - r.bounds.min.y);
                EditorUtility.SetDirty(t); EditorUtility.SetDirty(f);
            }
        }

        static void Grass()
        {
            var variants = new[]
            {
                new[] { Paint("Grass base green", new Color(.5f, .64f, .5f)), Paint("Grass tip green", new Color(.72f, .76f, .44f)) },
                new[] { Paint("Grass base olive", new Color(.56f, .62f, .44f)), Paint("Grass tip olive", new Color(.8f, .74f, .42f)) },
                new[] { Paint("Grass base straw", new Color(.66f, .64f, .44f)), Paint("Grass tip straw", new Color(.9f, .8f, .52f)) },
            };
            var rng = new System.Random(2122);
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name == "dune_grass_tuft"))
            {
                var r = f.GetComponent<MeshRenderer>(); var t = f.transform;
                double roll = rng.NextDouble();
                r.sharedMaterials = variants[roll < .45 ? 0 : roll < .8 ? 1 : 2];
                t.rotation = Quaternion.Euler(((float)rng.NextDouble() - .5f) * 12, (float)rng.NextDouble() * 360, ((float)rng.NextDouble() - .5f) * 12);
                EditorUtility.SetDirty(r); EditorUtility.SetDirty(t);
            }
        }

        static Material Paint(string name, Color c)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var src = AssetDatabase.FindAssets("Dune grass sage t:Material").Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();
            if (!m) { m = src ? new Material(src) : new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
