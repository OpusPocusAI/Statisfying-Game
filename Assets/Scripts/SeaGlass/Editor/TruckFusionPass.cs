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
    // - The canopy frame is part of the body shell (ArtSource/blender/truck_body.py, after the second review: a separate frame
    //   read as "bone-like tubes" pushed into the body): tapered pillars grow out of the cab rim into a roof rail and cross
    //   bars, painted with the old posts' chalk as the shell's fifth region. The same rebuild removes the rear-wheel flap.
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
            foreach (var n in new[] { "truck_wheel_well" })
            {
                var f = Path.Combine(ModelLibraryImporter.SourceFolder, n + ".json");
                if (File.Exists(f)) ModelLibraryImporter.Import(f); else { Debug.LogWarning("Truck fusion: run truck_fusion.py first"); return; }
            }
            AssetDatabase.SaveAssets();
            var wellMesh = ModelLibraryImporter.Load("truck_wheel_well");

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

            // Canopy frame: since the second review it is part of the body shell itself (truck_body.py fuses tapered pillars, roof rail
            // and cross bars into the bodywork), as a fifth colour region painted with the posts' chalk. The old posts are hidden.
            var posts = all.Where(t => t.name == "Cab pillar" || t.name == "Cab back pillar").ToArray();
            var shellFile = Path.Combine(ModelLibraryImporter.SourceFolder, "truck_shell.json");
            if (File.Exists(shellFile)) ModelLibraryImporter.Import(shellFile);
            var shell = all.First(t => t.name == "Fused body shell").GetComponent<MeshRenderer>();
            var shellMats = shell.sharedMaterials.Take(4).ToList(); shellMats.Add(M(posts.FirstOrDefault()));
            shell.sharedMaterials = shellMats.ToArray();
            foreach (var p in posts) Hide(p);

            // Mirrors on the front posts: the arm starts at the post's outer face, at the same height as before.
            foreach (var m in all.Where(t => t.name == "Hero mirror"))
            {
                float side = Mathf.Sign(m.localPosition.x);
                m.localPosition = new Vector3(side * .97f, m.localPosition.y, .67f);   // the pillar's outer face
            }

            // Fenders: the fused shell has its own arches; the older separate pieces under them stuck out as flaps.
            var fenders = all.Where(t => t.name == "Ivory wheel arch").ToArray();
            foreach (var f in fenders) Hide(f);
            // The painted chassis (round 10's body, under the fused shell) poked its ivory corner out behind each rear wheel: the "torn
            // flap" every review flagged. The shell covers the body, so the chassis is hidden.
            Hide(all.FirstOrDefault(t => t.name == "Painted chassis"));

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

            // Undercarriage (owner playtest, Nov 16: "Wheel is floating, nothing attached to it"). With the painted chassis hidden,
            // nothing held the wheels: one fused dark running gear (truck_undercarriage.py) under the shell floor, rails, beam axles
            // into each hub, differentials, springs and a gearbox. Not a "Hero ..." name, so HeroPropsPass leaves it alone.
            var underFile = Path.Combine(ModelLibraryImporter.SourceFolder, "truck_undercarriage.json");
            foreach (var u in all.Where(t => t && t.name == "Undercarriage").ToList()) Object.DestroyImmediate(u.gameObject);
            if (File.Exists(underFile))
            {
                ModelLibraryImporter.Import(underFile); AssetDatabase.SaveAssets();
                const string chassisPath = "Assets/Art/TruckPolish/Truck running gear.mat";
                var chassis = AssetDatabase.LoadAssetAtPath<Material>(chassisPath);
                if (!chassis) { chassis = new Material(dark); AssetDatabase.CreateAsset(chassis, chassisPath); }
                chassis.CopyPropertiesFromMaterial(dark); EditorUtility.SetDirty(chassis);
                var u = Put("x", ModelLibraryImporter.Load("truck_undercarriage"), chassis, body, Vector3.zero, Vector3.one); u.name = "Undercarriage";
            }
            else Debug.LogWarning("Truck fusion: run truck_undercarriage.py for the running gear");

            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Truck fusion:canopy frame replaces {posts.Length} posts, mirrors on the posts, {fenders.Length} fender pieces hidden, wheel wells lined");
        }
    }
}
