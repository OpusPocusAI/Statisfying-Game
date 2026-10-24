using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 8 (Documentation/ART_DIRECTION.md, section 5.4): windswept coastal pines with
    // layered, faceted canopies and fuller two-tone dune grass (ArtSource/blender/vegetation.py).
    public static class VegetationPass
    {
        const string Folder = "Assets/Art/Vegetation";

        [MenuItem("Sea Glass/Art Direction/8 Trees and dune grass")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Vegetation");
            ModelLibraryImporter.ImportAll();

            var foliage = Mat(Folder + "/Pine foliage.mat", new Color(.2f, .42f, .33f), .45f, 6f);
            var bark = Mat(Folder + "/Pine bark.mat", new Color(.45f, .3f, .22f), .25f, 5f);
            Trees(foliage, bark);

            var grassBase = Mat("Assets/Art/ShorelinePolish/Dune grass shadow.mat", new Color(.2f, .36f, .27f), .2f, .8f);
            var grassTip = Mat("Assets/Art/ShorelinePolish/Dune grass sage.mat", new Color(.66f, .66f, .4f), .1f, .8f);
            foreach (var m in new[] { grassBase, grassTip }) { m.SetFloat("_Cull", 0); m.SetFloat("_Rim", 1.4f); EditorUtility.SetDirty(m); }
            var tuft = ModelLibraryImporter.Load("dune_grass_tuft");
            int grass = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!f.sharedMesh || f.sharedMesh.name != "Bent sea grass tuft" && f.sharedMesh != tuft) continue;
                f.sharedMesh = tuft;
                f.GetComponent<Renderer>().sharedMaterials = new[] { grassBase, grassTip };
                EditorUtility.SetDirty(f); grass++;
            }
            Debug.Log("Vegetation: " + grass + " grass tufts updated.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Material Mat(string path, Color colour, float gradient, float height)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", colour); m.SetFloat("_GradientStrength", gradient); m.SetFloat("_GradientHeight", height);
            m.SetFloat("_Variation", .1f); m.SetFloat("_Smoothness", .08f);
            EditorUtility.SetDirty(m); return m;
        }

        static void Trees(Material foliage, Material bark)
        {
            var old = GameObject.Find("ART • coastal pines"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • coastal pines").transform;
            var pines = Enumerable.Range(0, 3).Select(k => ModelLibraryImporter.Load("coastal_pine_" + k)).ToArray();
            var canopies = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(r => r.name == "Wind-shaped coastal canopy" || r.name == "Coastal canopy shoulder" || r.name == "Sunlit canopy crown").ToArray();
            int k = 0;
            foreach (var trunk in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(r => r.name == "Plant trunk").OrderBy(r => r.transform.position.z))
            {
                trunk.enabled = false;   // collider stays
                var p = trunk.transform.position; var basePoint = new Vector3(p.x, p.y - trunk.transform.lossyScale.y * .5f, p.z);
                foreach (var c in canopies)
                    if (Vector2.Distance(new Vector2(c.transform.position.x, c.transform.position.z), new Vector2(p.x, p.z)) < 3.5f) c.enabled = false;
                var go = new GameObject("Coastal pine");
                go.transform.SetParent(root);
                // Lean inland, away from the sea wind (the sea is towards -X).
                go.transform.SetPositionAndRotation(basePoint, Quaternion.Euler(0, (k * 41) % 50 - 25, 0));
                go.transform.localScale = Vector3.one * (.95f + (k % 3) * .08f);
                go.AddComponent<MeshFilter>().sharedMesh = pines[k % 3];
                go.AddComponent<MeshRenderer>().sharedMaterials = new[] { foliage, bark };
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
                k++;
            }
            Debug.Log("Vegetation: " + k + " coastal pines placed.");
        }
    }
}
