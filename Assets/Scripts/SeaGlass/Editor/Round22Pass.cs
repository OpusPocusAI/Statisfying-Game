using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 22, broader content from the twentieth review (three reviewers averaged):
    // - Coastal shrubs read as "faceted green rocks": a layered, smooth-shaded leafy bush (round22.py) replaces them.
    // - The studio has "large empty floor planes": small storytelling clusters (crates of raw glass, baskets,
    //   buckets, jars, a stool) in the bare zones, kept low and out of the sight lines to the mosaic sockets.
    // - The truck's cargo bins read as closed boxes with one item: a straw packing bed in the glass bin and a
    //   crumpled sack in the rubbish bin, so they read as open, working bins.
    // - The mosaic floor light is rebuilt crisper (MosaicFinishPass.FeatherLight).
    // Shader change this round: FrostedGlass (fewer, larger mosaic panes). Safe to re-run.
    public static class Round22Pass
    {
        const string Folder = "Assets/Art/Round22";

        [MenuItem("Sea Glass/Art Direction/34 Round 22: leafy shrubs, studio clusters, truck load")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round22");
            ModelLibraryImporter.ImportAll();
            Shrubs();
            StudioClusters();
            TruckLoad();
            foreach (var panel in Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None)) MosaicFinishPass.FeatherLight(panel);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void Shrubs()
        {
            var leafy = ModelLibraryImporter.Load("coastal_shrub_leafy"); if (!leafy) return;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.sharedMesh && (f.sharedMesh.name == "coastal_shrub" || f.sharedMesh.name == "coastal_shrub_leafy")))
            {
                f.sharedMesh = leafy;
                EditorUtility.SetDirty(f);
            }
            // Lift the shaded leaf colour so the inner mass reads as deep green foliage, not a black lump.
            var dark = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh == leafy).Select(f => f.GetComponent<MeshRenderer>().sharedMaterials[0]).FirstOrDefault();
            if (dark && dark.HasProperty("_BaseColor"))
            {
                var c = dark.GetColor("_BaseColor"); float lum = c.r * .3f + c.g * .55f + c.b * .15f;
                if (lum < .3f) { dark.SetColor("_BaseColor", new Color(.3f, .46f, .3f)); EditorUtility.SetDirty(dark); }
            }
        }

        static void StudioClusters()
        {
            const string rootName = "ART • studio floor clusters";
            var old = GameObject.Find(rootName); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(rootName).transform;
            float floorY = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Studio floorboard").Max(r => r.bounds.max.y);
            var crate = Model("wooden_crate"); var basket = Model("wicker_basket"); var bucket = Model("enamel_bucket"); var jar = Model("glass_jar"); var stool = Model("stool");
            var slabs = Enumerable.Range(0, 6).Select(k => ModelLibraryImporter.Load("glass_slab_" + k)).Where(m => m).ToArray();
            var glassMats = Object.FindObjectsByType<MosaicSocket>(FindObjectsSortMode.None).Select(s => s.surface.sharedMaterial).Distinct().ToArray();
            var rng = new System.Random(2202);

            // Left wall, front: a stack of crates of raw glass beside a basket.
            Cluster(root, new Vector3(-12.2f, floorY, 162.6f), 20, rng, slabs, glassMats, crate, crate, basket, jar);
            // Right middle, near the open side: a stool with a bucket and jars of sorted glass.
            Cluster(root, new Vector3(3.6f, floorY, 162.4f), -35, rng, slabs, glassMats, stool, bucket, jar, crate);
            // In front of the mosaic wall, between the panels, kept low: baskets of glass waiting to be placed.
            Cluster(root, new Vector3(-6.6f, floorY, 171.2f), 10, rng, slabs, glassMats, basket, jar, null, null);
            Cluster(root, new Vector3(.4f, floorY, 171.4f), -15, rng, slabs, glassMats, crate, jar, null, null);
            // Back middle, between the work island and the gallery.
            Cluster(root, new Vector3(-7.4f, floorY, 166.2f), 40, rng, slabs, glassMats, bucket, basket, jar, null);
        }

        // Up to four props around a point; crates and baskets get a small heap of glass slabs on top.
        static void Cluster(Transform root, Vector3 at, float yaw, System.Random rng, Mesh[] slabs, Material[] glassMats, params (Mesh mesh, Material[] mats)?[] items)
        {
            var offsets = new[] { new Vector3(0, 0, 0), new Vector3(.62f, 0, .18f), new Vector3(-.35f, 0, .5f), new Vector3(.3f, 0, -.48f) };
            var rot = Quaternion.Euler(0, yaw, 0);
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null || !items[i].Value.mesh) continue;
                var it = items[i].Value; var go = new GameObject(it.mesh.name + " (cluster)"); go.transform.SetParent(root);
                go.AddComponent<MeshFilter>().sharedMesh = it.mesh; go.AddComponent<MeshRenderer>().sharedMaterials = it.mats;
                var p = at + rot * offsets[i];
                // A second crate stacks on the first.
                bool stack = i == 1 && items[0] != null && items[0].Value.mesh == it.mesh;
                go.transform.rotation = Quaternion.Euler(0, yaw + (float)(rng.NextDouble() - .5) * 30, 0);
                go.transform.position = stack ? at + Vector3.up * .01f : p;
                var b = go.GetComponent<Renderer>().bounds;
                float baseY = stack ? root.GetComponentsInChildren<Renderer>().Where(r => r.gameObject != go && r.bounds.Contains(new Vector3(at.x, r.bounds.center.y, at.z))).Select(r => r.bounds.max.y).DefaultIfEmpty(at.y).Max() : at.y;
                go.transform.position += Vector3.up * (baseY - b.min.y);
                if (stack) { go.transform.rotation *= Quaternion.Euler(0, 12, 0); }
                go.AddComponent<BoxCollider>();
                string n = it.mesh.name;
                if ((n.Contains("crate") || n.Contains("basket")) && slabs.Length > 0 && glassMats.Length > 0)
                {
                    var top = go.GetComponent<Renderer>().bounds;
                    for (int k = 0; k < 6; k++)
                    {
                        var s = new GameObject("Raw glass"); s.transform.SetParent(go.transform, true);
                        var m = slabs[rng.Next(slabs.Length)]; s.AddComponent<MeshFilter>().sharedMesh = m;
                        s.AddComponent<MeshRenderer>().sharedMaterial = glassMats[rng.Next(glassMats.Length)];
                        float size = .08f + (float)rng.NextDouble() * .05f;
                        s.transform.localScale = Vector3.one; float unit = Mathf.Max(m.bounds.size.x, m.bounds.size.y) * s.transform.lossyScale.x;
                        s.transform.localScale = s.transform.localScale * (size / Mathf.Max(unit, 1e-4f));
                        s.transform.rotation = Quaternion.Euler(90 + (float)(rng.NextDouble() - .5) * 50, (float)rng.NextDouble() * 360, 0);
                        s.transform.position = new Vector3(top.center.x + (float)(rng.NextDouble() - .5) * top.size.x * .55f, top.max.y - .03f + k * .012f, top.center.z + (float)(rng.NextDouble() - .5) * top.size.z * .55f);
                    }
                }
            }
        }

        static (Mesh mesh, Material[] mats)? Model(string name)
        {
            var mesh = ModelLibraryImporter.Load(name); if (!mesh) return null;
            var src = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh == mesh && !f.name.EndsWith("(cluster)"));
            if (!src) return null;
            return (mesh, src.GetComponent<MeshRenderer>().sharedMaterials);
        }

        static void TruckLoad()
        {
            foreach (var name in new[] { "GLASS cargo crate", "RUBBISH cargo crate" })
            {
                var crate = GameObject.Find(name); if (!crate) continue;
                var body = crate.transform.parent; var oldFill = body.Find(name + " packing"); if (oldFill) Object.DestroyImmediate(oldFill.gameObject);
                var b = crate.GetComponent<Renderer>().bounds;
                bool glass = name.StartsWith("GLASS");
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(go.GetComponent<Collider>());
                go.name = name + " packing"; go.transform.SetParent(body, true);
                go.transform.position = new Vector3(b.center.x, b.max.y - .07f, b.center.z);
                go.transform.rotation = crate.transform.rotation;
                var ls = body.lossyScale;
                go.transform.localScale = new Vector3(b.size.x * .86f / ls.x, .1f / ls.y, b.size.z * .9f / ls.z);
                go.GetComponent<MeshRenderer>().sharedMaterial = Paint(glass ? "Truck straw packing" : "Truck crumpled sack", glass ? new Color(.86f, .74f, .46f) : new Color(.62f, .55f, .44f));
            }
        }

        static Material Paint(string name, Color c)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .05f); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
