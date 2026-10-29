using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 25, from the twenty-third review (three reviewers averaged):
    // - Islands read as "layer cakes" with a grassy lid: far_isle_* are re-sculpted (round25.py) as one heightfield
    //   with a slumped rock apron, tapering cliffs broken by ledges and grass on every ledge; this pass re-imports.
    // - Clean glass read as "gummy sweets": FrostedGlass now absorbs light through the piece (deep core, pale thin
    //   edge, the background legible behind it) and its glints are stars. Here every clean piece lying in a sorting
    //   tray also throws a soft pool of its colour onto the felt (GlassLight.shader).
    // Safe to re-run.
    public static class Round25Pass
    {
        const string Folder = "Assets/Art/Round25";

        [MenuItem("Sea Glass/Art Direction/37 Round 25: sculpted islands, glass light")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round25");
            ModelLibraryImporter.ImportAll();
            GlassLight();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void GlassLight()
        {
            var shader = Shader.Find("Sea Glass/Glass Light"); if (!shader) return;
            var glass = Shader.Find("Sea Glass/Frosted Glass");
            var oldRoot = GameObject.Find("ART • glass light pools"); if (oldRoot) Object.DestroyImmediate(oldRoot);
            var root = new GameObject("ART • glass light pools").transform;
            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            var trays = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name == "sorting_tray").Select(f => f.GetComponent<Renderer>().bounds).ToList();
            int n = 0;
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var m = r.sharedMaterial;
                if (!m || m.shader != glass || m.GetFloat("_Facets") > .5f || m.GetFloat("_Grime") > .05f) continue;
                var b = r.bounds;
                if (!trays.Any(t => t.Contains(new Vector3(b.center.x, t.center.y, b.center.z)))) continue;
                var mat = Paint(m);
                var go = new GameObject("Glass light pool");
                go.transform.SetParent(root, false);
                float size = Mathf.Max(b.size.x, b.size.z) * 1.7f;
                // Light enters from the ocean-side windows (-x), so the pool falls a little towards +x.
                go.transform.SetPositionAndRotation(new Vector3(b.center.x + size * .12f, b.min.y + .003f, b.center.z), Quaternion.Euler(90, 0, 0));
                go.transform.localScale = new Vector3(size, size, 1);
                go.AddComponent<MeshFilter>().sharedMesh = quad;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
                n++;
            }
            Debug.Log("Round 25: " + n + " glass light pools in the trays.");
        }

        static Material Paint(Material glass)
        {
            var c = glass.GetColor("_BaseColor");
            string path = Folder + "/Glass light " + ColorUtility.ToHtmlStringRGB(c) + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Glass Light")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_Colour", c);
            // Pale glass throws a weaker pool; dark glass a stronger one, so every colour reads on grey felt.
            float lum = c.r * .3f + c.g * .55f + c.b * .15f;
            m.SetFloat("_Strength", Mathf.Lerp(.42f, .24f, Mathf.InverseLerp(.3f, .85f, lum)));
            EditorUtility.SetDirty(m); return m;
        }
    }
}
