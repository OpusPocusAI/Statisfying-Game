using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 68, from the sixty-fourth review (all three: the first-person sleeve still "a plain tube", now "stretching across
    // half the screen"; the open palm "a flat, stiff mitten". Round 67's folds and finger curl were in the meshes but did not
    // read from the camera under flat, single-tone shading):
    // - Cloth materials (Assets/Art/Cloth.shader) for every first-person glove and the held palm: canvas, a knit cuff, denim
    //   twill and a rubber palm coating, each with a stepped light-to-shade ramp so folds and knuckles read, and a woven
    //   surface mapped in the arm's own space.
    // - garden_glove and palm_glove carry a fourth slot, the rubber-dipped palm and fingers (glove.py, palm_glove.py).
    // - EquipmentVisuals turns the brushing arm toward a shoulder below and right of the camera; StudioStation turns the held
    //   palm so the fingers' curl shows from the side.
    // Safe to re-run.
    public static class Round68Pass
    {
        const string Folder = "Assets/Art/Round68";

        [MenuItem("Sea Glass/Art Direction/69 Round 68: cloth gloves and sleeves")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round68");
            var shader = Shader.Find("Sea Glass/Cloth");
            if (!shader) throw new System.InvalidOperationException("Sea Glass/Cloth shader missing.");
            var mats = new[]
            {
                Mat(shader, "Cloth canvas", new Color(.93f, .77f, .45f), new Color(.6f, .52f, .62f), 0, 150, .16f, 0),
                Mat(shader, "Cloth cuff knit", new Color(.88f, .42f, .29f), new Color(.6f, .45f, .6f), 2, 420, .24f, 0),
                Mat(shader, "Cloth denim", new Color(.29f, .43f, .6f), new Color(.55f, .55f, .74f), 1, 120, .14f, 0),   // coarse enough not to shimmer into moire
                Mat(shader, "Glove coating", new Color(.2f, .25f, .4f), new Color(.6f, .58f, .75f), 3, 40, .2f, .5f),   // deep navy: the aqua and green glass stand out on it
            };
            int n = 0;
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                         .Where(r => r.name == "Gloved hand" || r.name == "Held palm"))
            {
                r.sharedMaterials = mats; EditorUtility.SetDirty(r); n++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 68: cloth materials on " + n + " gloves");
        }

        static Material Mat(Shader shader, string name, Color colour, Color shade, float weave, float scale, float strength, float sheen)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            m.SetColor("_BaseColor", colour); m.SetColor("_ShadeColor", shade);
            m.SetFloat("_Weave", weave); m.SetFloat("_WeaveScale", scale); m.SetFloat("_WeaveStrength", strength);
            m.SetFloat("_Rim", .3f); m.SetFloat("_Sheen", sheen);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
