using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 16, from the fourteenth independent blind review: the aqua, cobalt and violet tray linings matched
    // their glass, so the pieces vanished into them. Every lining becomes a neutral linen with only a faint hint
    // of its colour, and a narrow band in the full glass colour runs along each tray's front rim as the cue.
    // Shader changes this round: FrostedGlass (crisp flashing facets, edge silt instead of spots) and TidalWater
    // (glints no longer form straight-edged bars). Safe to re-run.
    public static class Round16Pass
    {
        const string Folder = "Assets/Art/Round16";

        [MenuItem("Sea Glass/Art Direction/29 Round 16: neutral tray linings")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round16");
            var linen = new Color(.88f, .84f, .76f);
            foreach (var tray in Object.FindObjectsByType<SortingTray>(FindObjectsSortMode.None))
            {
                var colour = GameCatalog.Palette[(int)tray.color];
                var r = tray.GetComponent<MeshRenderer>();
                var lining = r ? r.sharedMaterials.FirstOrDefault(m => m && m.name.StartsWith("Lining")) : null;
                if (lining) { lining.SetColor("_BaseColor", Color.Lerp(linen, colour, .12f)); EditorUtility.SetDirty(lining); }
                Band(tray, colour);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // A thin painted strip on top of the tray's front rim, in the tray's glass colour.
        static void Band(SortingTray tray, Color colour)
        {
            string name = "Tray colour band";
            var old = tray.transform.parent ? tray.transform.parent.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == name + " " + tray.color) : null;
            if (old) Object.DestroyImmediate(old.gameObject);
            var b = tray.GetComponent<Collider>() ? tray.GetComponent<Collider>().bounds : tray.GetComponent<Renderer>().bounds;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name + " " + tray.color;
            go.transform.SetParent(tray.transform.parent, true);
            go.transform.position = new Vector3(b.center.x, b.max.y + .004f, b.min.z + .035f);
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var ls = go.transform.lossyScale;
            go.transform.localScale = new Vector3(b.size.x * .84f / ls.x, .008f / ls.y, .03f / ls.z);
            go.GetComponent<MeshRenderer>().sharedMaterial = Paint("Tray band " + tray.color, colour);
        }

        static Material Paint(string name, Color c)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .3f); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
