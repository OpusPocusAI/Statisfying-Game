using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaGlass.Editor
{
    // Hero props and truck pass (owner, November 16: Tchia is the art target; no primitive assemblies). The truck's body is
    // one fused shell, but 40-odd Unity cubes, cylinders and spheres still showed on it: disc wheels with stacked hub discs,
    // box headlamp bezels, a box-and-slat grille, box tail lamps, markers and badge, stick mirrors and handles, box steps,
    // stick steering spokes, a box skip plate and squashed-sphere cargo packing. Outside, the studio boardwalk was 20 boxes
    // with a fence of square posts and rails, and the cove signs stood on square sticks. This pass swaps each for a fused,
    // original model (ArtSource/blender/hero_props.py), reusing the old pieces' materials.
    // Old pieces are hidden, not deleted: their transforms stay where gameplay expects them (the wheels still spin, the skip
    // plate keeps its collider and compartment, boardwalk planks and posts keep their colliders). Wheels and the steering
    // wheel are parented under the old transforms so they move with them. Repeatable: every "Hero ..." object is rebuilt.
    public static class HeroPropsPass
    {
        static readonly string[] Models =
        {
            "truck_wheel", "truck_headlamp", "truck_grille", "truck_taillamp", "truck_marker", "truck_badge", "truck_mirror",
            "truck_handle", "truck_step", "truck_steering", "truck_skip_plate", "cargo_straw", "cargo_sack",
            "boardwalk_deck", "mooring_post", "rope_swag", "sign_post",
        };

        static Material M(Transform t) { var r = t ? t.GetComponent<MeshRenderer>() : null; return r ? r.sharedMaterial : null; }
        static void Hide(Transform t) { var r = t ? t.GetComponent<MeshRenderer>() : null; if (r) r.enabled = false; }

        static GameObject Put(string name, Mesh mesh, Material[] mats, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 localScale, bool shadows = true)
        {
            var go = new GameObject("Hero " + name); go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos; go.transform.localRotation = localRot; go.transform.localScale = localScale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
            r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            return go;
        }

        [MenuItem("Sea Glass/Art Direction/Hero props and truck: fused truck details, boardwalk and sign posts")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            foreach (var n in Models) { var f = Path.Combine(ModelLibraryImporter.SourceFolder, n + ".json"); if (File.Exists(f)) ModelLibraryImporter.Import(f); }
            AssetDatabase.SaveAssets();
            var mesh = Models.ToDictionary(n => n, ModelLibraryImporter.Load);
            if (mesh.Values.Any(m => !m)) { Debug.LogWarning("Hero props: run ArtSource/blender/hero_props.py first"); return; }

            // Rebuild from scratch: remove earlier hero pieces on the truck (the outdoor ones live under their own root).
            var truck = Object.FindFirstObjectByType<UtilityTruck>();
            foreach (var t in truck.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Hero ")).ToList())
                if (t) Object.DestroyImmediate(t.gameObject);
            var all = truck.GetComponentsInChildren<Transform>(true);
            Transform[] Named(string n) => all.Where(t => t.name == n).ToArray();
            int swapped = 0;

            // Wheels: one fused tyre and hub under each spinning wheel, undoing its squashed cylinder scale; the dish faces out.
            foreach (var w in Named("Rubber wheel"))
            {
                var hub = w.Cast<Transform>().FirstOrDefault(c => c.name == "Wheel hub"); var centre = hub ? hub.Cast<Transform>().FirstOrDefault() : null;
                var mats = new[] { M(w), M(hub), M(centre) };
                float outward = hub && hub.localPosition.y < 0 ? -1 : 1; var s = w.localScale;
                Put("wheel", mesh["truck_wheel"], mats, w, Vector3.zero, Quaternion.identity, new Vector3(1 / s.x, outward / s.y, 1 / s.z));
                Hide(w); Hide(hub); Hide(centre); swapped++;
            }
            // Front: headlamp pods, grille, badge, amber markers.
            var enamel = M(Named("Mirror casing").FirstOrDefault());
            foreach (var b in Named("Headlamp bezel"))
            {
                var lamp = Named("Round headlamp").OrderBy(l => Vector3.Distance(l.localPosition, b.localPosition)).First();
                Put("headlamp", mesh["truck_headlamp"], new[] { enamel, M(b), M(lamp) }, b.parent, new Vector3(b.localPosition.x, b.localPosition.y, 1.95f), Quaternion.identity, Vector3.one);
                Hide(b); Hide(lamp); swapped++;
            }
            var grille = Named("Recessed grille").First(); var slat = Named("Grille slat").First();
            Put("grille", mesh["truck_grille"], new[] { M(grille), M(slat) }, grille.parent, grille.localPosition + new Vector3(0, 0, .005f), Quaternion.identity, Vector3.one);
            Hide(grille); foreach (var s in Named("Grille slat")) Hide(s); swapped++;
            var badge = Named("Bonnet centre badge").First();
            Put("badge", mesh["truck_badge"], new[] { M(badge), M(slat) }, badge.parent, badge.localPosition + new Vector3(0, -.01f, .005f), Quaternion.identity, Vector3.one);
            Hide(badge); swapped++;
            foreach (var m in Named("Amber marker")) { Put("marker", mesh["truck_marker"], new[] { M(m) }, m.parent, m.localPosition, Quaternion.identity, Vector3.one); Hide(m); swapped++; }
            // Rear: tail lamp units, skip plate.
            foreach (var b in Named("Tail lamp backing"))
            {
                var coral = Named("Coral tail lamp").OrderBy(l => Vector3.Distance(l.localPosition, b.localPosition)).First();
                var amber = Named("Amber tail marker").OrderBy(l => Vector3.Distance(l.localPosition, b.localPosition)).First();
                Put("tail lamp", mesh["truck_taillamp"], new[] { M(b), M(coral), M(amber) }, b.parent, b.localPosition, Quaternion.identity, Vector3.one);
                Hide(b); Hide(coral); Hide(amber); swapped++;
            }
            var plate = Named("Optional skip-drive plate").First();
            Put("skip plate", mesh["truck_skip_plate"], new[] { M(plate) }, plate.parent, plate.localPosition, Quaternion.identity, Vector3.one);
            Hide(plate); swapped++;
            // Sides: mirrors (arm out of the cab pillar into the casing), door handles, cab steps; right-hand ones mirrored.
            foreach (var arm in Named("Mirror arm"))
            {
                float side = Mathf.Sign(arm.localPosition.x);
                var casing = Named("Mirror casing").OrderBy(c => Vector3.Distance(c.localPosition, arm.localPosition)).First();
                var inset = Named("Mirror inset").OrderBy(c => Vector3.Distance(c.localPosition, arm.localPosition)).First();
                var at = arm.localPosition - new Vector3(side * arm.localScale.x * .5f, 0, 0);
                Put("mirror", mesh["truck_mirror"], new[] { M(casing), M(arm), M(inset) }, arm.parent, at, Quaternion.identity, new Vector3(side < 0 ? 1 : -1, 1, 1));
                Hide(arm); Hide(casing); Hide(inset); swapped++;
            }
            foreach (var h in Named("Door handle"))
            {
                float side = Mathf.Sign(h.localPosition.x);
                Put("door handle", mesh["truck_handle"], new[] { M(h) }, h.parent, h.localPosition + new Vector3(-side * .025f, 0, 0), Quaternion.identity, new Vector3(side < 0 ? 1 : -1, 1, 1));
                Hide(h); swapped++;
            }
            foreach (var st in Named("Cab step"))
            {
                float side = Mathf.Sign(st.localPosition.x);
                Put("cab step", mesh["truck_step"], new[] { M(st) }, st.parent, st.localPosition, Quaternion.identity, new Vector3(side < 0 ? 1 : -1, 1, 1));
                Hide(st); swapped++;
            }
            // Steering wheel: rim, spokes and hub in one, inside the existing (tilted) steering assembly.
            var steer = Named("Steering assembly").First();
            var rim = steer.Cast<Transform>().First(c => c.name == "Steering rim"); var spoke = steer.Cast<Transform>().First(c => c.name == "Steering spoke"); var hubC = steer.Cast<Transform>().First(c => c.name == "Steering centre");
            Put("steering wheel", mesh["truck_steering"], new[] { M(rim), M(spoke), M(hubC) }, steer, Vector3.zero, Quaternion.identity, Vector3.one);
            foreach (Transform c in steer) if (!c.name.StartsWith("Hero ")) Hide(c);
            swapped++;
            // Cargo packing: lumpy straw in the glass tub, a crumpled sack in the rubbish tub.
            foreach (var (n, model) in new[] { ("GLASS cargo crate packing", "cargo_straw"), ("RUBBISH cargo crate packing", "cargo_sack") })
            {
                var p = Named(n).FirstOrDefault(); if (!p) continue;
                Put(model == "cargo_straw" ? "straw packing" : "sack packing", mesh[model], new[] { M(p) }, p.parent, p.localPosition - new Vector3(0, .02f, 0), Quaternion.identity, Vector3.one, false);
                Hide(p); swapped++;
            }

            // Boardwalk beside the studio: one deck of bevelled boards, rounded mooring posts, rope swags between them.
            var planks = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Boardwalk plank").Select(r => r.transform).OrderBy(t => t.position.z).ToList();
            var posts = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Mooring post").Select(r => r.transform).OrderBy(t => t.position.z).ToList();
            var rails = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Boardwalk rail").Select(r => r.transform).ToList();
            var heroRoot = GameObject.Find("ART • hero props"); if (heroRoot) Object.DestroyImmediate(heroRoot);
            var outside = new GameObject("ART • hero props").transform;
            if (planks.Count > 0)
            {
                var wood = M(planks[0]);
                var dark = Variant(wood, "Boardwalk timber dark", .82f);
                var first = planks[0].position;
                Put("boardwalk deck", mesh["boardwalk_deck"], new[] { wood, dark }, outside, new Vector3(first.x, first.y - .03f, first.z), Quaternion.identity, Vector3.one);
                foreach (var p in planks) Hide(p);
            }
            var rope = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/StudioWalls/Rope.mat");
            for (int i = 0; i < posts.Count; i++)
            {
                var p = posts[i]; var ground = new Vector3(p.position.x, p.position.y - p.localScale.y * .5f, p.position.z);
                Put("mooring post", mesh["mooring_post"], new[] { M(p), rope }, outside, ground, Quaternion.Euler(0, i * 47, 0), Vector3.one);
                if (i + 1 < posts.Count) Put("rope swag", mesh["rope_swag"], new[] { rope }, outside, ground, Quaternion.identity, new Vector3(1, 1, (posts[i + 1].position.z - p.position.z) / 3f), false);
                Hide(p);
            }
            foreach (var r in rails) Hide(r);
            // Cove sign posts.
            int signs = 0;
            foreach (var sp in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Sign post").Select(r => r.transform).ToList())
            {
                var mf = sp.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh || !AssetDatabase.GetAssetPath(mf.sharedMesh).StartsWith("Library")) continue;
                float h = sp.lossyScale.y;
                Put("sign post", mesh["sign_post"], new[] { M(sp) }, outside, sp.position, Quaternion.Euler(0, sp.eulerAngles.y, 0), new Vector3(1, h / 1.6f, 1));
                Hide(sp); signs++;
            }

            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Hero props: {swapped} truck details swapped, boardwalk {planks.Count} planks and {posts.Count} posts, {signs} sign posts");
        }

        static Material Variant(Material src, string name, float shade)
        {
            string path = "Assets/Art/Coastline/" + name + ".mat";
            if (!AssetDatabase.IsValidFolder("Assets/Art/Coastline")) AssetDatabase.CreateFolder("Assets/Art", "Coastline");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(src); AssetDatabase.CreateAsset(m, path); }
            var c = src.GetColor("_BaseColor"); m.SetColor("_BaseColor", new Color(c.r * shade, c.g * shade, c.b * shade * .97f, c.a));
            EditorUtility.SetDirty(m); return m;
        }
    }
}
