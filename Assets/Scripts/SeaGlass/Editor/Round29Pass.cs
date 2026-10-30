using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 29, the truck, studio and props from the twenty-seventh review (three reviewers averaged), the areas
    // every review still called "boxes" or "empty":
    // - Truck: a crowned bonnet, a body with rolled sills, tapered canopy posts with foot and cap (round29.py), a
    //   painted dashboard instead of black blocks, metal steps; UtilityTruck shows three pieces per loaded find.
    // - Props: a rounded two-tone cooler with lid lip and hinges, routed wooden section signs, bushes shaded dark at
    //   the base and sunlit at the crown, rubbish half again as large so cans and bottles read at 5 m.
    // - Studio: the repeated basket-with-blue-glass varies (buckets, crates, plants), gallery benches with jars sit
    //   in front of the outer mosaics, and the mosaic name plaques are cream with larger dark text.
    // Safe to re-run.
    public static class Round29Pass
    {
        const string Folder = "Assets/Art/Round29";

        [MenuItem("Sea Glass/Art Direction/40 Round 29: truck, props, studio dressing")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round29");
            ModelLibraryImporter.ImportAll();
            Truck();
            Props();
            Studio();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Swap a mesh and keep the part's size and centre, measured in its own local space (rotation-safe).
        static void Fit(MeshFilter f, Mesh mesh)
        {
            if (!f || !mesh || f.sharedMesh == mesh) return;
            var old = f.sharedMesh; var t = f.transform;
            var ob = old.bounds; var nb = mesh.bounds; var s = t.localScale;
            var ns = new Vector3(s.x * ob.size.x / Mathf.Max(nb.size.x, 1e-4f), s.y * ob.size.y / Mathf.Max(nb.size.y, 1e-4f), s.z * ob.size.z / Mathf.Max(nb.size.z, 1e-4f));
            var oldCentreW = t.TransformPoint(ob.center);
            f.sharedMesh = mesh; t.localScale = ns;
            var newCentreW = t.TransformPoint(nb.center);
            t.position += oldCentreW - newCentreW;
            var mc = f.GetComponent<MeshCollider>(); if (mc) mc.sharedMesh = mesh;
            EditorUtility.SetDirty(f); EditorUtility.SetDirty(t);
        }

        static Material Mat(string name) => AssetDatabase.FindAssets("t:Material " + name).Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(m => m && m.name == name);

        static void Truck()
        {
            var truck = Object.FindAnyObjectByType<UtilityTruck>(); if (!truck) return;
            var parts = truck.GetComponentsInChildren<MeshFilter>(true);
            var bonnet = ModelLibraryImporter.Load("truck_bonnet_r"); var body = ModelLibraryImporter.Load("truck_body_r"); var post = ModelLibraryImporter.Load("truck_post_r");
            var teal = Mat("Tidal teal"); var ivory = Mat("Ivory enamel"); var metal = Mat("Brushed warm metal");
            foreach (var f in parts)
            {
                var r = f.GetComponent<MeshRenderer>(); if (!r) continue;
                switch (f.name)
                {
                    case "Soft bonnet": Fit(f, bonnet); break;
                    case "Painted chassis":
                        Fit(f, body);
                        if (ivory) r.sharedMaterials = new[] { r.sharedMaterials[0], ivory };
                        break;
                    case "Cab pillar": case "Cab back pillar": Fit(f, post); break;
                    case "Dashboard": if (teal) r.sharedMaterial = teal; break;                 // was a black block
                    case "Cab step": if (metal) r.sharedMaterial = metal; break;
                }
                EditorUtility.SetDirty(r);
            }
        }

        static void Props()
        {
            var cooler = ModelLibraryImporter.Load("cool_box_r");
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name == "cool_box").ToList())
                Fit(f, cooler);

            // Section signs: a routed wooden board on the existing post (the board mesh is built at true size).
            var board = ModelLibraryImporter.Load("sign_board_r");
            var timber = Mat("Burnt orange timber");
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "Beach section sign").ToList())
            {
                var f = t.GetComponent<MeshFilter>(); var r = t.GetComponent<MeshRenderer>(); if (!f || !board) continue;
                if (f.sharedMesh != board)
                {
                    f.sharedMesh = board; t.localScale = Vector3.one;
                    var mc = t.GetComponent<BoxCollider>(); if (mc) { mc.center = Vector3.zero; mc.size = board.bounds.size; }
                }
                var teal = r.sharedMaterials[0];
                r.sharedMaterials = new[] { teal, timber ? timber : teal };
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(r);
            }

            // Bushes: dark and cool at the base, sunlit at the crown (Painterly base-to-crown gradient).
            var shrub = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh && f.sharedMesh.name == "coastal_shrub_leafy");
            if (shrub)
                foreach (var m in shrub.GetComponent<MeshRenderer>().sharedMaterials.Where(m => m && m.HasProperty("_GradientStrength")))
                { m.SetFloat("_GradientStrength", .5f); m.SetFloat("_GradientHeight", 1.1f); EditorUtility.SetDirty(m); }

            // Rubbish: half again as large, so a can or bottle reads as one at 5 m.
            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(i => i.visual && !i.glass))
            {
                var v = item.visual;
                if (v.name.EndsWith(" (r29)")) continue;
                v.localScale *= 1.5f; v.name += " (r29)";
                EditorUtility.SetDirty(v);
            }
        }

        static (Mesh mesh, Material[] mats)? Model(string name)
        {
            var mesh = ModelLibraryImporter.Load(name); if (!mesh) return null;
            var src = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh == mesh);
            return src ? (mesh, src.GetComponent<MeshRenderer>().sharedMaterials) : ((Mesh, Material[])?)null;
        }

        static void Studio()
        {
            // Vary the repeated basket of blue glass: every second one becomes a bucket, a crate or a plant.
            var swaps = new[] { Model("enamel_bucket"), Model("wooden_crate"), Model("potted_plant") }.Where(m => m != null).Select(m => m.Value).ToArray();
            var baskets = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Where(f => f.sharedMesh && f.sharedMesh.name == "wicker_basket" && f.transform.position.z > 150).OrderBy(f => f.transform.position.x + f.transform.position.z * 100).ToList();
            for (int i = 1; i < baskets.Count && swaps.Length > 0; i += 2)
            {
                var f = baskets[i]; var r = f.GetComponent<MeshRenderer>(); var sw = swaps[(i / 2) % swaps.Length];
                float oldTop = r.bounds.max.y, oldBottom = r.bounds.min.y;
                f.sharedMesh = sw.mesh; r.sharedMaterials = sw.mats;
                var t = f.transform; t.localScale = Vector3.one;
                var b = r.bounds; t.position += Vector3.up * (oldBottom - b.min.y);
                float dy = r.bounds.max.y - oldTop;
                foreach (Transform c in t) c.position += Vector3.up * dy;                       // keep the glass on top
                if (sw.mesh.name == "potted_plant") foreach (Transform c in t) c.gameObject.SetActive(false);
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(r); EditorUtility.SetDirty(t);
            }

            // Gallery benches in front of the outer mosaics, with a few jars of glass on them (no colliders: the
            // walk to every socket stays open).
            const string rootName = "ART • gallery seating";
            var old = GameObject.Find(rootName); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(rootName).transform;
            var bench = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).FirstOrDefault(f => f.name == "gallery bench");
            var jar = Model("glass_jar");
            if (bench)
                foreach (var x in new[] { -10f, 4f })
                {
                    var b = Object.Instantiate(bench.gameObject, root); b.name = "gallery bench (round 29)";
                    b.transform.position = new Vector3(x, bench.transform.position.y, 169.6f);
                    foreach (var c in b.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                    if (jar != null)
                    {
                        var top = b.GetComponent<Renderer>().bounds;
                        for (int k = 0; k < 2; k++)
                        {
                            var j = new GameObject("Jar of glass"); j.transform.SetParent(b.transform, true);
                            j.AddComponent<MeshFilter>().sharedMesh = jar.Value.mesh; j.AddComponent<MeshRenderer>().sharedMaterials = jar.Value.mats;
                            j.transform.position = new Vector3(top.center.x + (k == 0 ? -.7f : .55f), top.max.y, top.center.z);
                            j.transform.rotation = Quaternion.Euler(0, k * 40, 0);
                            var jb = j.GetComponent<Renderer>().bounds; j.transform.position += Vector3.up * (top.max.y - jb.min.y);
                        }
                    }
                }

            // Mosaic name plaques: cream boards with larger dark lettering, readable from the middle of the room.
            var plaqueMat = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Mosaic title plaque").Select(r => r.sharedMaterial).FirstOrDefault();
            if (plaqueMat && plaqueMat.HasProperty("_BaseColor")) { plaqueMat.SetColor("_BaseColor", new Color(.93f, .87f, .74f)); EditorUtility.SetDirty(plaqueMat); }
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Mosaic title plaque"))
            {
                var s = r.transform.localScale; if (s.y < .3f) { r.transform.localScale = new Vector3(s.x, .36f, s.z); EditorUtility.SetDirty(r.transform); }
            }
            foreach (var t in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None).Where(t => t.text.Contains("  /  ") && t.transform.position.z > 176))
            {
                if (t.characterSize < .035f) t.characterSize = .041f;
                t.color = new Color(.12f, .2f, .22f);
                EditorUtility.SetDirty(t);
            }
        }
    }
}
