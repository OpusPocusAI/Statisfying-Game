using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 32, the studio floor and the wash water from the thirtieth review (three reviewers averaged):
    // - "Large empty plank floor in front of the mosaic wall": the walk-up to the middle mosaic stays open, and the two
    //   gaps between the frames carry the work of the studio: a trestle table with a panel in progress (left gap) and a
    //   stepladder leaning on the wall beside a bucket of glass (right gap). round32.py.
    // - "Rugs are flat zero-thickness quads": woven_rug is re-modelled thicker with a rolled edge and fringe (same
    //   footprint, picked up by every rug on import).
    // - "Opaque basin water, the floor never shows": a few pebbles and some sand grit lie on the bowl floor under the
    //   water, so the teal reads as water over a bottom. Feedback.cs flicks more and larger droplets while brushing.
    // No colliders on the new dressing, so every socket stays reachable. Safe to re-run.
    public static class Round32Pass
    {
        [MenuItem("Sea Glass/Art Direction/43 Round 32: studio work dressing, rugs, basin floor")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            const string rootName = "ART • studio work in progress";
            var old = GameObject.Find(rootName); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(rootName).transform;
            Gallery(root);
            BasinFloor(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Material Mat(string name) => AssetDatabase.FindAssets("t:Material " + name).Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(m => m && m.name == name);

        static GameObject Place(Transform root, string name, Mesh mesh, Material[] mats, Vector3 pos, float yaw)
        {
            var g = new GameObject(name); g.transform.SetParent(root);
            g.AddComponent<MeshFilter>().sharedMesh = mesh; g.AddComponent<MeshRenderer>().sharedMaterials = mats;
            g.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            return g;
        }

        static void Gallery(Transform root)
        {
            float floorY = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Studio floorboard").Max(r => r.bounds.max.y);
            var wood = Mat("Workbench top"); var lead = Mat("Lead came");
            var ga = Mat("Glass Aqua"); var gb = Mat("Glass Amber"); var gc = Mat("Glass Cobalt");
            var trestle = ModelLibraryImporter.Load("trestle_wip");
            if (trestle && wood && lead && ga && gb && gc)
            {
                // Left gap between Ocean Wave (x = -10) and Lighthouse (x = -3), long side towards the room.
                var t = Place(root, "Trestle with a panel in progress", trestle, new[] { wood, lead, ga, gb, gc }, new Vector3(-6.5f, floorY, 169.9f), 4);
                var b = t.GetComponent<Renderer>().bounds; t.transform.position += Vector3.up * (floorY - b.min.y);
            }
            var ladder = ModelLibraryImporter.Load("stepladder");
            if (ladder && wood)
            {
                // Right gap between Lighthouse and Coastal Sunset, leaning on the wall: it faces the room, tilted back.
                var l = Place(root, "Stepladder", ladder, new[] { wood }, new Vector3(.5f, floorY, 175.1f), 180);
                l.transform.rotation = Quaternion.Euler(-9, 180, 0);
                var b = l.GetComponent<Renderer>().bounds; l.transform.position += Vector3.up * (floorY - b.min.y);
            }
            // A bucket of glass at the ladder's foot.
            var bucketSrc = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh && f.sharedMesh.name == "enamel_bucket");
            if (bucketSrc)
            {
                var bk = Object.Instantiate(bucketSrc.gameObject, root); bk.name = "Bucket of glass by the ladder";
                foreach (var c in bk.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                bk.transform.SetPositionAndRotation(new Vector3(1.5f, floorY, 174.4f), Quaternion.Euler(0, 30, 0));
                var b = bk.GetComponent<Renderer>().bounds; bk.transform.position += Vector3.up * (floorY - b.min.y);
            }
        }

        static void BasinFloor(Transform root)
        {
            return;   // round 36: the pebbles read as "dark paw prints"; the bowl floor stays clean
#pragma warning disable CS0162
            var water = GameObject.Find("Basin water"); if (!water) return;
            var wb = water.GetComponent<Renderer>().bounds;
            var pebble = ModelLibraryImporter.Load("pebble_cluster");
            var stone = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh == pebble).Select(f => f.GetComponent<MeshRenderer>().sharedMaterials).FirstOrDefault();
            if (!pebble || stone == null) return;
            // Round 33 (reviews: "the pebbles float on the water surface"): seated on the enamel itself, found with a ray
            // against a temporary collider made from the bowl mesh, away from the rim and the working glass.
            var bowl = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh && f.sharedMesh.name == "enamel_bowl");
            if (!bowl) return;
            var probe = new GameObject("TEMP bowl probe"); probe.transform.SetPositionAndRotation(bowl.transform.position, bowl.transform.rotation); probe.transform.localScale = bowl.transform.lossyScale;
            var mc = probe.AddComponent<MeshCollider>(); mc.sharedMesh = bowl.sharedMesh;
            Physics.SyncTransforms();
            var spots = new[] { new Vector2(-.24f, .22f), new Vector2(.26f, -.2f), new Vector2(.3f, .18f) };
            foreach (var s in spots)
            {
                var top = new Vector3(wb.center.x + s.x * wb.size.x, wb.max.y + .02f, wb.center.z + s.y * wb.size.z);
                if (!mc.Raycast(new Ray(top, Vector3.down), out var hit, 1f)) continue;
                var g = Place(root, "Pebbles on the bowl floor", pebble, stone, hit.point, s.x * 200);
                g.transform.localScale = Vector3.one * .5f;
                var b = g.GetComponent<Renderer>().bounds; g.transform.position += Vector3.up * (hit.point.y - b.min.y - .004f);
                var r = g.GetComponent<MeshRenderer>(); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            Object.DestroyImmediate(probe);
        }
    }
}
