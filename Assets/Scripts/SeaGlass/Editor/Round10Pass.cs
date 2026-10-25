using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 10: the bigger modelling work the sixth independent review called for (ArtReference/comparison.md).
    // Truck canopy and tube rails, a second tree species (palms), see-through basin water with the glass sitting
    // in it, and near-field sand detail. The dusty-to-restored studio is runtime (StudioRestoration + Painterly
    // Lit). Models: ArtSource/blender/round10.py. Run after pass 22. Safe to re-run.
    public static class Round10Pass
    {
        const string Folder = "Assets/Art/Round10";

        [MenuItem("Sea Glass/Art Direction/23 Round 10: truck, palms, basin, sand detail")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round10");
            ModelLibraryImporter.ImportAll();

            Truck();
            Palms();
            Basin();
            var old = GameObject.Find("ART • round 10 sand detail"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • round 10 sand detail").transform;
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => i.transform.position).ToList();
            Footprints(root, items);
            ShellDrifts(root, items);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Keep each part's world size: scale the new mesh so its bounds match the old one's.
        static void SwapKeepSize(MeshFilter f, Mesh m)
        {
            if (f.sharedMesh == m) return;
            var size = Vector3.Scale(f.sharedMesh.bounds.size, f.transform.localScale);
            var centre = f.transform.TransformPoint(f.sharedMesh.bounds.center);
            f.sharedMesh = m;
            var nb = m.bounds.size;
            f.transform.localScale = new Vector3(size.x / Mathf.Max(nb.x, 1e-4f), size.y / Mathf.Max(nb.y, 1e-4f), size.z / Mathf.Max(nb.z, 1e-4f));
            f.transform.position += centre - f.transform.TransformPoint(m.bounds.center);
            EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.transform);
        }

        static void Truck()
        {
            var canopy = ModelLibraryImporter.Load("truck_canopy"); var rail = ModelLibraryImporter.Load("cargo_rail_tube");
            var coral = Paint("Canopy coral", new Color(.86f, .36f, .27f), .15f, true);
            var cream = Paint("Canopy cream", new Color(.97f, .91f, .8f), .15f, true);
            var railMat = Paint("Rail cream", new Color(.95f, .9f, .8f), .35f, false);
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (f.name == "Crowned coral roof" && canopy) { SwapKeepSize(f, canopy); f.GetComponent<MeshRenderer>().sharedMaterials = new[] { coral, cream }; }
                else if (f.name == "Cargo rail" && rail)
                {
                    // The truck upgrade stretches rails by transform scale (and a test checks the scale stays rail-thin),
                    // so the rail uses a unit-sized copy of the mesh and keeps its size in the transform.
                    var unit = UnitMesh(rail);
                    var size = Vector3.Scale(f.sharedMesh.bounds.size, f.transform.localScale);
                    var centre = f.transform.TransformPoint(f.sharedMesh.bounds.center);
                    f.sharedMesh = unit; f.transform.localScale = size;
                    f.transform.position += centre - f.transform.TransformPoint(unit.bounds.center);
                    f.GetComponent<MeshRenderer>().sharedMaterials = new[] { railMat };
                    EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.transform);
                }
            }
        }

        // Palms as a second species: every other roadside pine becomes a palm, and a few palm groups stand at the
        // back of the beach (trunk colliders, clear of every find by 2.5 m).
        // A copy of the mesh squeezed into a 1 x 1 x 1 box centred on the origin.
        static Mesh UnitMesh(Mesh m)
        {
            string path = Folder + "/" + m.name + " unit.asset";
            var u = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!u) { u = new Mesh(); AssetDatabase.CreateAsset(u, path); }
            var b = m.bounds;
            u.Clear(); u.name = m.name + " unit";
            u.vertices = m.vertices.Select(v => new Vector3((v.x - b.center.x) / b.size.x, (v.y - b.center.y) / b.size.y, (v.z - b.center.z) / b.size.z)).ToArray();
            u.subMeshCount = m.subMeshCount;
            for (int s = 0; s < m.subMeshCount; s++) u.SetTriangles(m.GetTriangles(s), s);
            u.RecalculateNormals(); u.RecalculateBounds();
            EditorUtility.SetDirty(u); return u;
        }

        static void Palms()
        {
            var palms = Enumerable.Range(0, 3).Select(k => ModelLibraryImporter.Load("palm_" + k)).ToArray();
            if (palms.Any(p => !p)) return;
            var mats = new[] { Paint("Palm bark", new Color(.6f, .45f, .33f), .05f, false), Paint("Palm ring", new Color(.47f, .34f, .25f), .05f, false),
                               Paint("Palm frond", new Color(.34f, .58f, .33f), .1f, true), Paint("Coconut", new Color(.42f, .3f, .17f), .2f, false) };
            mats[2].SetFloat("_GradientStrength", .45f); mats[2].SetFloat("_GradientHeight", 6.5f);
            var rng = new System.Random(101);
            int i = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh && (f.sharedMesh.name.StartsWith("coastal_pine") || f.sharedMesh.name.StartsWith("palm_"))).OrderBy(f => f.transform.position.z).ToList())
            {
                if (i++ % 2 == 1) { f.sharedMesh = palms[rng.Next(3)]; f.GetComponent<MeshRenderer>().sharedMaterials = mats; EditorUtility.SetDirty(f); }
            }
            var old = GameObject.Find("ART • beach palms"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • beach palms").transform;
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(it => it.transform.position).ToList();
            foreach (var (x, z) in new[] { (-6f, 8f), (-4.5f, 9.5f), (-7f, 46f), (-5f, 47.5f), (-6.5f, 98f), (-4.2f, 99f), (-5.5f, 128f) })
            {
                var p = new Vector3(x, 0, z);
                if (items.Any(it => (new Vector2(it.x, it.z) - new Vector2(x, z)).sqrMagnitude < 6.25f)) continue;
                p.y = Round3Pass.Ground(p) - .05f;
                var go = new GameObject("Beach palm"); go.transform.SetParent(root);
                go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0));
                float s = .85f + (float)rng.NextDouble() * .3f; go.transform.localScale = Vector3.one * s;
                go.AddComponent<MeshFilter>().sharedMesh = palms[rng.Next(3)];
                go.AddComponent<MeshRenderer>().sharedMaterials = mats;
                var cap = go.AddComponent<CapsuleCollider>(); cap.radius = .22f; cap.height = 3.2f; cap.center = new Vector3(0, 1.6f, 0);
            }
        }

        // See-through water just under the rim, with the working piece lowered so its underside sits in it.
        static void Basin()
        {
            var water = GameObject.Find("Basin water");
            if (water)
            {
                var p = water.transform.position; if (p.y < 1.26f) { water.transform.position = new Vector3(p.x, 1.275f, p.z); EditorUtility.SetDirty(water.transform); }
                var m = water.GetComponent<MeshRenderer>().sharedMaterial; m.renderQueue = -1; EditorUtility.SetDirty(m);
            }
            var clean = Object.FindObjectsByType<StudioStation>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(s => s.kind == StationKind.Clean);
            if (clean && clean.workingGlass)
            {
                var lp = clean.workingGlass.localPosition; if (lp.y > 1.32f) { clean.workingGlass.localPosition = new Vector3(lp.x, 1.3f, lp.z); EditorUtility.SetDirty(clean.workingGlass); }
            }
        }

        // Footprint trails wander across the near beach (someone came here before you).
        static void Footprints(Transform root, List<Vector3> items)
        {
            var mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
            var mat = Paint("Footprint sand", new Color(.8f, .71f, .63f), .05f, false);
            var trails = new[] { new[] { new Vector2(-3, 4), new Vector2(-12, 16), new Vector2(-20, 26), new Vector2(-27, 33) },
                                 new[] { new Vector2(-8, 52), new Vector2(-15, 60), new Vector2(-24, 66), new Vector2(-28, 76) },
                                 new[] { new Vector2(-4, 110), new Vector2(-13, 118), new Vector2(-21, 124), new Vector2(-27, 131) } };
            foreach (var trail in trails)
            {
                int step = 0;
                for (int s = 0; s < trail.Length - 1; s++)
                {
                    var a = trail[s]; var b = trail[s + 1]; float len = Vector2.Distance(a, b);
                    var dir = (b - a).normalized; var side = new Vector2(-dir.y, dir.x);
                    for (float d = 0; d < len; d += .72f, step++)
                    {
                        var q = a + dir * d + side * ((step % 2 == 0 ? 1 : -1) * .14f);
                        var p = new Vector3(q.x, 0, q.y);
                        if (items.Any(it => (new Vector2(it.x, it.z) - q).sqrMagnitude < 1f)) continue;
                        p.y = Round3Pass.Ground(p) + .004f;
                        var go = new GameObject("Footprint"); go.transform.SetParent(root);
                        go.transform.SetPositionAndRotation(p, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y)));
                        go.transform.localScale = new Vector3(.11f, .004f, .26f);
                        go.AddComponent<MeshFilter>().sharedMesh = mesh;
                        var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                }
            }
        }

        // Clustered drifts of shells and pebbles at the foot of grass clumps, so near views have detail groups
        // rather than evenly sprinkled dots.
        static void ShellDrifts(Transform root, List<Vector3> items)
        {
            var shells = new[] { ModelLibraryImporter.Load("scallop_shell"), ModelLibraryImporter.Load("whelk_shell"), ModelLibraryImporter.Load("pebble_cluster") };
            var mats = new[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Tide shell.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Scatter shell coral.mat"),
                               AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Scatter pebble warm.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Scatter pebble pale.mat") };
            if (shells.Any(s => !s) || mats.Any(m => !m)) return;
            var rng = new System.Random(303);
            foreach (var g in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Dune grass cluster" || r.name == "Sand tuft").OrderBy(r => r.transform.position.z).ToList())
            {
                var c = g.transform.position;
                if (c.z > 150 || rng.NextDouble() > .35) continue;
                int n = 4 + rng.Next(5);
                for (int k = 0; k < n; k++)
                {
                    var p = c + new Vector3(((float)rng.NextDouble() - .5f) * 1.2f, 0, ((float)rng.NextDouble() - .5f) * 1.2f);
                    if (items.Any(it => (new Vector2(it.x, it.z) - new Vector2(p.x, p.z)).sqrMagnitude < 1.4f)) continue;
                    p.y = Round3Pass.Ground(p);
                    int kind = rng.Next(3); var m = shells[kind];
                    var go = new GameObject("Shell drift"); go.transform.SetParent(root);
                    go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0));
                    go.transform.localScale = Vector3.one * (.55f + (float)rng.NextDouble() * .5f);
                    go.AddComponent<MeshFilter>().sharedMesh = m;
                    var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = Enumerable.Repeat(mats[kind == 2 ? 2 + rng.Next(2) : rng.Next(2)], m.subMeshCount).ToArray();
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        static Material Paint(string name, Color c, float smooth, bool twoSided)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_GradientStrength", .12f); m.SetFloat("_GradientHeight", 3f);
            m.SetFloat("_Cull", twoSided ? 0 : 2);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
