using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 2 (from the side-by-side with PowerWash Simulator and Firewatch, ArtReference/comparison.md):
    // a gloved hand grips each first-person tool, so tools no longer float. Each glove is a child of its
    // tool, so the existing tool motion and visibility logic carry it. Model: ArtSource/blender/glove.py
    // (grips a bar along local X through its origin).
    public static class GlovePass
    {
        [MenuItem("Sea Glass/Art Direction/16 Gloved hands on tools")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var glove = ModelLibraryImporter.Load("garden_glove");
            var mats = new[] { Mat("Glove canvas", new Color(.93f, .76f, .42f)), Mat("Glove cuff", new Color(.86f, .4f, .27f)), Mat("Sleeve denim", new Color(.27f, .4f, .55f)) };
            foreach (var g in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == "Gloved hand").ToArray())
                Object.DestroyImmediate(g.gameObject);

            var eq = Object.FindFirstObjectByType<EquipmentVisuals>();
            // Spade: grip the shaft (local Y axis) just above the T-handle.
            var spade = GameObject.Find("Toy beach spade")?.transform;
            if (spade) Attach(spade, glove, mats, new Vector3(0, -.035f, .03f), Quaternion.Euler(0, 0, 90));
            // Bucket, sack and brush: grip their top bar (handle / straps / arch), which runs along local X.
            foreach (var (root, name) in new[] { (eq.bucket.transform, "Enamel bucket"), (eq.sack.transform, "Canvas sack"), (eq.brush.transform, "Wash brush") })
            {
                var r = root.GetComponentsInChildren<MeshRenderer>(true).Where(x => x.enabled && x.GetComponent<MeshFilter>()).OrderByDescending(x => x.bounds.max.y).First();
                var top = new Vector3(r.bounds.center.x, r.bounds.max.y - (name == "Wash brush" ? .015f : .02f), r.bounds.center.z);
                Attach(r.transform, glove, mats, r.transform.InverseTransformPoint(top), Quaternion.Euler(0, name == "Wash brush" ? -20 : -25, 0), name == "Wash brush" ? .8f : .85f);   // arm comes in from the right, clear of the contents
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void Attach(Transform parent, Mesh mesh, Material[] mats, Vector3 localPos, Quaternion localRot, float size = 1)
        {
            var go = new GameObject("Gloved hand");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos; go.transform.localRotation = localRot;
            var s = parent.lossyScale;
            go.transform.localScale = new Vector3(size / s.x, size / s.y, size / s.z);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static Material Mat(string name, Color c)
        {
            string path = "Assets/Art/GatheringPolish/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .08f); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
