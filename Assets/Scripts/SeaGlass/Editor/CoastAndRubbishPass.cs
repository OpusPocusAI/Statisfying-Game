using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 7 (Documentation/ART_DIRECTION.md, sections 5.2, 5.5): an authored coastline of
    // bays and headlands (CoastShape) and recognisable rubbish (ArtSource/blender/rubbish.py).
    public static class CoastAndRubbishPass
    {
        static readonly string[] OldRubbish = { "Rolled rim can", "Capped shoreline bottle", "Folded metal scrap", "Collected bottle cap", "Worn driftwood" };
        static readonly string[] NewRubbish = { "rubbish_can", "rubbish_bottle", "rubbish_crushed", "rubbish_cap", "rubbish_driftwood" };

        [MenuItem("Sea Glass/Art Direction/7 Coastline and rubbish")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");

            // Coastline: the wet-sand shoulder follows CoastShape; the beach pass moves tide-line
            // details and headland boulders to match (it re-imports the Blender models too).
            ShoreTransitionPolish.Apply();
            BeachPass.Apply();
            Rubbish();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void Rubbish()
        {
            var meshes = new Mesh[5];
            for (int k = 0; k < 5; k++)
            {
                var old = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/ShorelinePolish/" + OldRubbish[k] + ".asset");
                var mesh = ModelLibraryImporter.Load(NewRubbish[k]);
                // Sit the new model exactly where the old one met the sand at the reveal height.
                // Game scale, not real-world scale: match the old footprint so it reads at 5 m.
                var v = mesh.vertices;
                float scale = Mathf.Max(old.bounds.size.x, old.bounds.size.z) / Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.z);
                for (int i = 0; i < v.Length; i++) v[i] *= scale;
                mesh.vertices = v; mesh.RecalculateBounds();
                var b = mesh.bounds;
                var shift = new Vector3(old.bounds.center.x - b.center.x, old.bounds.min.y - b.min.y, old.bounds.center.z - b.center.z);
                for (int i = 0; i < v.Length; i++) v[i] += shift;
                mesh.vertices = v; mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
                meshes[k] = mesh;
            }
            var metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Dulled aluminium.mat");
            metal.SetColor("_BaseColor", new Color(.66f, .7f, .72f)); metal.SetFloat("_Smoothness", .5f); EditorUtility.SetDirty(metal);
            var cream = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Canvas cream.mat");
            var coral = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Faded coral canvas.mat");
            coral.SetColor("_BaseColor", new Color(.86f, .36f, .22f)); EditorUtility.SetDirty(coral);
            var teal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Weathered sea teal.mat");
            var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Bleached driftwood.mat");
            wood.SetColor("_BaseColor", new Color(.72f, .62f, .5f)); EditorUtility.SetDirty(wood);
            var bottle = Painterly("Assets/Art/Beach/Sea-blue plastic.mat", new Color(.55f, .78f, .86f), .55f);
            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (item.glass || !item.visual) continue;
                int seed = int.Parse(item.id.Substring(item.id.LastIndexOf('-') + 1)), kind = seed % 5;
                item.visual.GetComponent<MeshFilter>().sharedMesh = meshes[kind];
                var primary = kind == 1 ? bottle : kind == 4 ? wood : metal;
                item.visual.GetComponent<Renderer>().sharedMaterials = new[] { primary, kind % 2 == 0 ? coral : teal, metal };
                EditorUtility.SetDirty(item.visual);
            }
        }

        static Material Painterly(string path, Color colour, float smoothness)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", colour); m.SetFloat("_Smoothness", smoothness); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
