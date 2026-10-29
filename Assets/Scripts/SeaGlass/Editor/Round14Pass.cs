using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 14, from the twelfth independent blind review:
    // - Nine sea-glass silhouettes instead of five similar pebbles: four new shapes (ArtSource/blender/round14.py)
    //   join the pebble family for held, found, carried and sorted glass, and the spare tray pieces vary in
    //   shape (x0.7-1.25 size) and tilt, so the trays stop reading as rows of jelly beans.
    // - A deeper wash bowl with a visible inner wall above a lower water line.
    // - The gooseneck tap stands at the back of the bowl with its spout arcing over the water, instead of at the
    //   corner turned away (from the player's view it read as a coat stand).
    // Safe to re-run.
    public static class Round14Pass
    {
        [MenuItem("Sea Glass/Art Direction/27 Round 14: glass shapes and deeper bowl")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var family = Enumerable.Range(0, 5).Select(k => ModelLibraryImporter.Load("glass_pebble_" + k))
                .Concat(Enumerable.Range(0, 4).Select(k => ModelLibraryImporter.Load("glass_shape_" + k))).ToArray();
            if (family.Any(m => !m)) throw new System.InvalidOperationException("Import the glass_pebble and glass_shape models first.");

            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            for (int i = 0; i < catalog.glass.Length; i++) catalog.glass[i].pebbleMesh = family[(i * 7 + 3) % family.Length];
            EditorUtility.SetDirty(catalog);
            ShoreFinds(catalog);
            TraySpares(family);
            Bowl();
            Tap();
            TruckRefinePass.Apply();   // truck cargo shows the catalog shapes

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Beach finds show their catalog shape at the same size as before.
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

        // Each spare piece gets a shape from the family, a size between 0.7x and 1.25x the median, and a tilt;
        // its lowest point stays where it was, so piles keep their height.
        static void TraySpares(Mesh[] family)
        {
            var spares = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name.StartsWith("Spare ") && r.name.EndsWith(" glass"))
                .OrderBy(r => r.transform.position.x).ThenBy(r => r.transform.position.z).ToArray();
            if (spares.Length == 0) return;
            // A fixed median size (the earlier piles were about 13 cm across), measured on the mesh outline rather
            // than a tilted bounding box, so re-running does not grow the pieces.
            const float median = .13f;
            var rng = new System.Random(1414);
            foreach (var r in spares)
            {
                var t = r.transform; var f = r.GetComponent<MeshFilter>();
                float bottom = r.bounds.min.y;
                var mesh = family[rng.Next(family.Length)];
                float factor = .7f + (float)rng.NextDouble() * .55f;
                f.sharedMesh = mesh;
                // Lying flat (thickness along local Z pointing up), turned and tilted at random.
                t.rotation = Quaternion.Euler((float)(rng.NextDouble() - .5) * 36, (float)rng.NextDouble() * 360, (float)(rng.NextDouble() - .5) * 36) * Quaternion.Euler(90, 0, 0);
                t.localScale = Vector3.one;   // measure at unit scale, then fit
                float unit = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y) * t.lossyScale.x;
                t.localScale = Vector3.one * (median * factor / Mathf.Max(unit, 1e-4f));
                t.position += Vector3.up * (bottom - r.bounds.min.y);
                EditorUtility.SetDirty(t); EditorUtility.SetDirty(f);
            }
        }

        // A deeper bowl (inner wall visible above the water) and the water line lowered to match.
        static void Bowl()
        {
            var basin = GameObject.Find("Enamel basin"); var water = GameObject.Find("Basin water");
            if (!basin || !water) return;
            var s = basin.transform.localScale;
            if (s.y < 1.2f) { basin.transform.localScale = new Vector3(s.x, s.y * 1.6f, s.z); EditorUtility.SetDirty(basin.transform); }
            var b = basin.GetComponent<Renderer>().bounds;
            // At about half the bowl's height its inner wall is about 90% of the rim's width.
            water.transform.position = new Vector3(b.center.x, b.min.y + b.size.y * .48f, b.center.z);
            var ps = water.transform.parent ? water.transform.parent.lossyScale : Vector3.one;
            water.transform.localScale = new Vector3(b.size.x * .9f / ps.x, 1, b.size.z * .9f / ps.z);
            EditorUtility.SetDirty(water.transform);
        }

        // The tap stands behind the bowl's back-right rim, turned so its spout arcs in towards the middle of the
        // water and the gooseneck shows in profile from the player's view (head-on it read as a bare stick).
        static void Tap()
        {
            var tap = GameObject.Find("Brass faucet"); var basin = GameObject.Find("Enamel basin");
            if (!tap || !basin) return;
            var b = basin.GetComponent<Renderer>().bounds;
            var t = tap.transform;
            t.rotation = Quaternion.Euler(0, 42, 0);   // the model's spout points along -z; +yaw turns it towards -x
            var ps = t.parent ? t.parent.lossyScale.x : 1;
            t.localScale = Vector3.one * (1.3f / ps);
            // The base column sits at the model's pivot; put it just outside the back-right of the rim.
            t.position = new Vector3(b.center.x + b.extents.x * .45f, b.min.y, b.max.z - b.extents.z * .05f + .06f);
            EditorUtility.SetDirty(t);
            var spout = GameObject.Find("Faucet spout");
            if (spout) { var r = spout.GetComponent<Renderer>(); if (r && r.enabled) { r.enabled = false; EditorUtility.SetDirty(r); } }
        }
    }
}
