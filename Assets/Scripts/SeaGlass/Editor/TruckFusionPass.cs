using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Truck fusion pass (owner, November 16, from the independent art review: all three reviewers said the truck still reads as
    // posts, rails and tubs attached to a body; posts "plunge into the bonnet with no join", mirrors "float", a "torn fender
    // flap" behind the rear wheel and the coral underside showing inside the arches).
    // - The four canopy posts become one fused frame (ArtSource/blender/truck_fusion.py): flared feet on the bodywork, tapered
    //   posts bending into a rounded roof rail with cross bars under the canvas.
    // - The mirrors mount on the front posts instead of hanging in the air ahead of them.
    // - The separate ivory fender pieces (an older layer under the fused shell's own arches, whose tails stuck out as flaps)
    //   are hidden; dark linings fill each wheel arch.
    // - The tyre tread is a staggered band of broad, low blocks (hero_props.py), not separate bumps.
    // It runs HeroPropsPass first (which rebuilds every "Hero ..." piece on the truck), so it is repeatable.
    public static class TruckFusionPass
    {
        [MenuItem("Sea Glass/Art Direction/Truck fusion: one canopy frame, mounted mirrors, lined arches")]
        public static void Apply()
        {
            HeroPropsPass.Apply();
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var n in new[] { "truck_frame", "truck_wheel_well" })
            {
                var f = Path.Combine(ModelLibraryImporter.SourceFolder, n + ".json");
                if (File.Exists(f)) ModelLibraryImporter.Import(f); else { Debug.LogWarning("Truck fusion: run truck_fusion.py first"); return; }
            }
            AssetDatabase.SaveAssets();
            var frameMesh = ModelLibraryImporter.Load("truck_frame"); var wellMesh = ModelLibraryImporter.Load("truck_wheel_well");

            var truck = Object.FindFirstObjectByType<UtilityTruck>();
            var all = truck.GetComponentsInChildren<Transform>(true);
            var body = all.First(t => t.name == "Sprung body");
            Material M(Transform t) { var r = t ? t.GetComponent<MeshRenderer>() : null; return r ? r.sharedMaterial : null; }
            void Hide(Transform t) { var r = t ? t.GetComponent<MeshRenderer>() : null; if (r) r.enabled = false; }
            GameObject Put(string name, Mesh mesh, Material mat, Transform parent, Vector3 pos, Vector3 scale)
            {
                var go = new GameObject("Hero " + name); go.transform.SetParent(parent, false);
                go.transform.localPosition = pos; go.transform.localScale = scale;
                go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                return go;
            }

            // Canopy frame in place of the four posts (same chalk paint).
            var posts = all.Where(t => t.name == "Cab pillar" || t.name == "Cab back pillar").ToArray();
            Put("canopy frame", frameMesh, M(posts.FirstOrDefault()), body, Vector3.zero, Vector3.one);
            foreach (var p in posts) Hide(p);

            // Mirrors on the front posts: the arm starts at the post's outer face, at the same height as before.
            foreach (var m in all.Where(t => t.name == "Hero mirror"))
            {
                float side = Mathf.Sign(m.localPosition.x);
                m.localPosition = new Vector3(side * .985f, m.localPosition.y, .67f);
            }

            // Fenders: the fused shell has its own arches; the older separate pieces under them stuck out as flaps.
            var fenders = all.Where(t => t.name == "Ivory wheel arch").ToArray();
            foreach (var f in fenders) Hide(f);

            // Dark linings inside each arch, around the wheel.
            // (Two-sided charcoal: the inner wall is seen from outside the truck through the arch.)
            var dark = M(all.FirstOrDefault(t => t.name == "Recessed grille")) ?? M(all.FirstOrDefault(t => t.name == "Tail lamp backing"));
            const string linerPath = "Assets/Art/TruckPolish/Truck arch liner.mat";
            var liner = AssetDatabase.LoadAssetAtPath<Material>(linerPath);
            if (!liner) { liner = new Material(dark); AssetDatabase.CreateAsset(liner, linerPath); }
            liner.CopyPropertiesFromMaterial(dark); liner.SetFloat("_Cull", 0); liner.doubleSidedGI = true; EditorUtility.SetDirty(liner);
            foreach (var w in all.Where(t => t.name == "Rubber wheel"))
            {
                var at = body.InverseTransformPoint(w.position); float side = Mathf.Sign(at.x);
                // the band spans 0.26 m inside the arch, its inner wall toward the truck centre (the model has it on +X)
                Put("wheel well", wellMesh, liner, body, new Vector3(side * .97f, at.y, at.z), new Vector3(side < 0 ? 1 : -1, 1, 1));
            }

            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Truck fusion: canopy frame replaces {posts.Length} posts, mirrors on the posts, {fenders.Length} fender pieces hidden, wheel wells lined");
        }
    }
}
