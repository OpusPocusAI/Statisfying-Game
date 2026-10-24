using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 13 (Documentation/ART_DIRECTION.md, section 5.12): a modelled enamel bucket with a
    // painted band and bail handle, a soft canvas sack with drawstring and straps, and contents that show
    // sand and recognisable finds. Each new mesh is fitted to the bounds of the one it replaces, so the
    // runtime fill scaling (EquipmentVisuals.RefreshFill) and transforms are unchanged.
    // Models: ArtSource/blender/carry_tools.py.
    public static class CarryToolsPass
    {
        const string Folder = "Assets/Art/GatheringPolish";

        [MenuItem("Sea Glass/Art Direction/13 Bucket, sack and contents")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var eq = Object.FindFirstObjectByType<EquipmentVisuals>();

            var enamel = Mat("Warm enamel", new Color(.96f, .92f, .82f), .45f);
            var band = Mat("Bucket coral band", new Color(.93f, .42f, .27f), .4f);
            var rim = Mat("Sieve teal", new Color(.1f, .34f, .33f), .45f);
            var grip = Mat("Honey handle", new Color(.62f, .4f, .2f), .25f);
            var canvas = Mat("Sea green canvas", new Color(.52f, .66f, .55f), .05f);
            var trim = Mat("Sack rope", new Color(.86f, .76f, .58f), .05f);
            var patch = Mat("Sack patch", new Color(.93f, .48f, .32f), .05f);

            var bucketBody = Find(eq.bucket.transform, "Open tapered bucket");
            Fit(bucketBody, ModelLibraryImporter.Load("enamel_bucket"), "Enamel bucket fitted", new[] { enamel, band, rim, grip });
            foreach (var n in new[] { "Teal rolled rim", "Bucket carry handle", "Handle comfort grip" }) Hide(eq.bucket.transform, n);

            var bag = Find(eq.sack.transform, "Soft sided open sack");
            Fit(bag, ModelLibraryImporter.Load("canvas_sack"), "Canvas sack fitted", new[] { canvas, trim, patch });
            foreach (var n in new[] { "Rolled canvas mouth", "Canvas carry loop", "Canvas front patch" }) Hide(eq.sack.transform, n);

            // Contents: sand with tumbled glass in four colours, a can, a cap and a blue bottle.
            var sand = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            var glass = new[] { 0, 2, 3, 5 }.Select(c => Mat("Carried glass " + (GlassColor)c, Color.Lerp(GameCatalog.Palette[c], Color.white, .15f), .6f)).ToArray();
            var metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Dulled aluminium.mat");
            var bottle = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Beach/Sea-blue plastic.mat");
            var mats = new[] { sand, glass[0], glass[1], glass[2], glass[3], metal, bottle };
            foreach (var fill in new[] { eq.bucketFill, eq.sackFill })
                Fit(fill.GetComponent<MeshFilter>(), ModelLibraryImporter.Load("carry_contents"), "Carried finds fitted", mats, true);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static MeshFilter Find(Transform root, string name) => root.GetComponentsInChildren<MeshFilter>(true).First(f => f.name == name);
        static void Hide(Transform root, string name) { foreach (var r in root.GetComponentsInChildren<Renderer>(true).Where(r => r.name == name)) r.enabled = false; }

        static Material Mat(string name, Color c, float smooth)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }

        // Copy `source` into a dedicated asset, scaled and moved to sit inside the old mesh's bounds:
        // same width and floor; height uniform with width unless `stretch` (contents keep the old height).
        static void Fit(MeshFilter filter, Mesh source, string suffix, Material[] materials, bool stretch = false)
        {
            var old = filter.sharedMesh;
            var ob = old.bounds;   // a fitted mesh keeps the same box, so reruns are idempotent
            string path = Folder + "/" + source.name + " " + suffix + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear(); mesh.indexFormat = source.indexFormat;
            var sb = source.bounds;
            float s = Mathf.Max(ob.size.x, ob.size.z) / Mathf.Max(sb.size.x, sb.size.z);
            var scale = stretch ? new Vector3(ob.size.x / sb.size.x, ob.size.y / sb.size.y, ob.size.z / sb.size.z) : Vector3.one * s;
            var v = source.vertices.Select(p => Vector3.Scale(p - new Vector3(sb.center.x, sb.min.y, sb.center.z), scale) + new Vector3(ob.center.x, ob.min.y, ob.center.z)).ToArray();
            mesh.vertices = v; mesh.normals = source.normals.Select(n => Vector3.Scale(n, new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z)).normalized).ToArray();
            mesh.subMeshCount = source.subMeshCount;
            for (int i = 0; i < source.subMeshCount; i++) mesh.SetTriangles(source.GetTriangles(i), i);
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            mesh.name = source.name + " " + suffix;
            EditorUtility.SetDirty(mesh);
            filter.sharedMesh = mesh;
            filter.GetComponent<Renderer>().sharedMaterials = materials;
            EditorUtility.SetDirty(filter);
        }
    }
}
