using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 15, from the thirteenth independent blind review (round 14 made glass and sorting worse):
    // - Held, washed, found and carried glass uses only chunky shapes (the five pebbles and the nugget). The
    //   thin oval read as "a pancake" lying on the bowl water; it is no longer used anywhere.
    // - Tray spares form a loose heap across the middle of each tray, two or three layers high at the centre,
    //   with a few strays (they sat in one strip under the label, so the trays looked 80% empty). Shapes: the
    //   pebbles, the rounded shard, the sliver and the nugget.
    // - The equipment pegboard in front of the Coastal Sunset mosaic is hidden (see Pegboard).
    // Safe to re-run.
    public static class Round15Pass
    {
        [MenuItem("Sea Glass/Art Direction/28 Round 15: chunky held glass, heaped trays, pegboard")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var pebbles = Enumerable.Range(0, 5).Select(k => ModelLibraryImporter.Load("glass_pebble_" + k)).ToArray();
            var shapes = Enumerable.Range(0, 4).Select(k => ModelLibraryImporter.Load("glass_shape_" + k)).ToArray();
            if (pebbles.Concat(shapes).Any(m => !m)) throw new System.InvalidOperationException("Import the glass_pebble and glass_shape models first.");

            var held = pebbles.Concat(new[] { shapes[2] }).ToArray();
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            for (int i = 0; i < catalog.glass.Length; i++) catalog.glass[i].pebbleMesh = held[(i * 7 + 3) % held.Length];
            EditorUtility.SetDirty(catalog);
            ShoreFinds(catalog);
            HeapTrays(pebbles.Concat(new[] { shapes[0], shapes[1], shapes[2] }).ToArray());
            Pegboard();
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

        // A loose heap in the middle 60% of each tray: pieces nearer the centre sit higher (on top of others),
        // each turned and tilted at random, and the last two stray towards the edge.
        static void HeapTrays(Mesh[] family)
        {
            const float median = .13f;
            var rng = new System.Random(1515);
            foreach (var tray in Object.FindObjectsByType<SortingTray>(FindObjectsSortMode.None))
            {
                var spares = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Spare " + tray.color + " glass").OrderBy(r => r.GetInstanceID()).ToArray();
                if (spares.Length == 0) continue;
                var tb = tray.GetComponent<Collider>() ? tray.GetComponent<Collider>().bounds : tray.GetComponent<Renderer>().bounds;
                float felt = spares.Min(r => r.bounds.min.y);   // the lowest piece rests on the felt
                Vector2 half = new Vector2(tb.extents.x * .6f, tb.extents.z * .6f);
                for (int i = 0; i < spares.Length; i++)
                {
                    var r = spares[i]; var t = r.transform; var f = r.GetComponent<MeshFilter>();
                    var mesh = family[rng.Next(family.Length)];
                    f.sharedMesh = mesh;
                    t.rotation = Quaternion.Euler((float)(rng.NextDouble() - .5) * 40, (float)rng.NextDouble() * 360, (float)(rng.NextDouble() - .5) * 40) * Quaternion.Euler(90, 0, 0);
                    t.localScale = Vector3.one;
                    float unit = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y) * t.lossyScale.x;
                    t.localScale = Vector3.one * (median * (.75f + (float)rng.NextDouble() * .45f) / Mathf.Max(unit, 1e-4f));
                    // Polar spread with more pieces near the centre; the last two are strays near the edge.
                    bool stray = i >= spares.Length - 2;
                    float rad = stray ? .85f + (float)rng.NextDouble() * .2f : Mathf.Sqrt((float)rng.NextDouble()) * .9f;
                    float ang = (float)rng.NextDouble() * Mathf.PI * 2;
                    var p = new Vector3(tb.center.x + Mathf.Cos(ang) * rad * half.x * (stray ? 1.4f : 1), 0, tb.center.z + Mathf.Sin(ang) * rad * half.y * (stray ? 1.3f : 1));
                    float heap = stray ? 0 : .055f * (1 - rad * rad) * (.6f + (float)rng.NextDouble() * .4f);
                    t.position = p;
                    t.position += Vector3.up * (felt + heap - r.bounds.min.y);
                    EditorUtility.SetDirty(t); EditorUtility.SetDirty(f);
                }
            }
        }

        // The free-standing pegboard hid half of the Coastal Sunset mosaic, and every review read its tools as
        // "rectangles on sticks". Moved to the open side edge it showed edge-on, with its tools floating in front
        // of the same mosaic. Hidden instead; the EQUIPMENT counter and its sign stay.
        static void Pegboard()
        {
            var parts = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(r => r.name == "Equipment pegboard" || r.name == "Tool on pegboard" || (r.name == "Tool head" && r.bounds.center.z > 170.5f && r.bounds.center.y > 2.5f));
            foreach (var r in parts) { if (!r.enabled) continue; r.enabled = false; EditorUtility.SetDirty(r); }
        }
    }
}
