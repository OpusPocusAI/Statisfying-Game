using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Rubbish and fallen glass pass (owner, November 16, from the independent art review: all three reviewers said rubbish was
    // "tiny specks at 5 m" and the fallen cargo glass "reads as a rock").
    // - Rubbish on the beach is scaled up toy-fashion (cans 21 -> 34 cm, crushed scrap 17 -> 29 cm, caps 11 -> 22 cm, bottles
    //   36 -> 47 cm; driftwood unchanged), and wears its own brighter materials (cooler, lighter aluminium against the warm sand,
    //   a clear coral label, a bluer bottle, a teal cap) instead of the dulled shared ones, so each piece reads at 5 m without
    //   outshining the glass. The truck's visible rubbish cargo gets the same materials. Colliders are unchanged.
    // - Glass lying out in the field shows a lighter crust (ShoreItem.FieldGrime), so it reads as frosted coloured glass.
    // Repeatable: scales are set from the recorded originals, not multiplied again.
    public static class RubbishReadabilityPass
    {
        const string Folder = "Assets/Art/Rubbish";
        static readonly Dictionary<string, float> Scale = new() { { "Metal can (r29)", 1.6f }, { "Scrap metal (r29)", 1.7f }, { "Bottle cap (r29)", 2f }, { "Plastic bottle (r29)", 1.3f }, { "Driftwood (r29)", 1f } };
        const float BaseScale = 1.5f;   // the r29 visuals' original uniform scale

        [MenuItem("Sea Glass/Art Direction/Rubbish and fallen glass: readable rubbish")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Rubbish");
            var swap = new Dictionary<string, Material>
            {
                { "Dulled aluminium", Variant("Assets/Art/ShorelinePolish/Dulled aluminium.mat", "Rubbish aluminium", new Color(.7f, .74f, .8f), .45f) },
                { "Faded coral canvas", Variant("Assets/Art/ShorelinePolish/Faded coral canvas.mat", "Rubbish coral label", new Color(.95f, .4f, .3f), .25f) },
                { "Sea-blue plastic", Variant("Assets/Art/Beach/Sea-blue plastic.mat", "Rubbish blue plastic", new Color(.3f, .58f, .88f), .55f) },
                { "Weathered sea teal", Variant("Assets/Art/ShorelinePolish/Weathered sea teal.mat", "Rubbish teal cap", new Color(.1f, .6f, .58f), .35f) },
            };
            int n = 0;
            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(i => !i.glass && i.visual))
            {
                if (Scale.TryGetValue(item.visual.name, out float s)) item.visual.localScale = Vector3.one * BaseScale * s;
                Recolour(item.visual, swap); EditorUtility.SetDirty(item.visual); n++;
            }
            // The truck's visible rubbish cargo: same materials (its own scales are set for the crate).
            var truck = Object.FindFirstObjectByType<UtilityTruck>();
            if (truck && truck.rubbishCargo != null) foreach (var c in truck.rubbishCargo) if (c) Recolour(c, swap);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Rubbish and fallen glass: {n} rubbish pieces scaled and recoloured");
        }

        static void Recolour(Transform t, Dictionary<string, Material> swap)
        {
            foreach (var r in t.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) if (mats[i] && swap.TryGetValue(mats[i].name, out var m)) mats[i] = m;
                r.sharedMaterials = mats;
            }
        }

        static Material Variant(string srcPath, string name, Color colour, float smooth)
        {
            var src = AssetDatabase.LoadAssetAtPath<Material>(srcPath);
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(src); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", colour); if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Brush")) m.SetFloat("_Brush", .1f);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
