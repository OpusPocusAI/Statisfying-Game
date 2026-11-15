using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Plants and foliage pass (owner, November 15: Tchia is the art target). Under the new tropical daylight the foliage
    // read dusty sage and blue-grey. Tchia's greenery is lush: deep emerald in the shade, warm yellow-green where the sun
    // catches it. Every foliage material moves that way; the lit/tip materials go yellow-green, the base/shade ones emerald.
    // Repeatable (it only sets colours).
    public static class TropicalFoliagePass
    {
        static readonly (string path, Color colour)[] Colours =
        {
            // Grass: emerald bases, sunny yellow-green tips.
            ("Assets/Art/Round21/Grass base green.mat", new Color(.30f, .58f, .26f)),
            ("Assets/Art/Round21/Grass tip green.mat", new Color(.62f, .78f, .32f)),
            ("Assets/Art/Round21/Grass base olive.mat", new Color(.44f, .62f, .26f)),
            ("Assets/Art/ShorelinePolish/Dune grass shadow.mat", new Color(.18f, .46f, .24f)),
            ("Assets/Art/ShorelinePolish/Dune grass sage.mat", new Color(.6f, .76f, .32f)),
            // Shrubs and stems.
            ("Assets/Art/Round3/Shrub dark.mat", new Color(.15f, .44f, .21f)),
            ("Assets/Art/Round3/Shrub lit.mat", new Color(.36f, .64f, .2f)),
            ("Assets/Art/StudioFurniture/Stem green.mat", new Color(.2f, .5f, .22f)),
            // Canopies, palms, island cover.
            ("Assets/Materials/SeaGlass/Blue green plants.mat", new Color(.12f, .44f, .25f)),
            ("Assets/Art/CoastalPolish/Sage canopy.mat", new Color(.2f, .54f, .25f)),
            ("Assets/Art/Vegetation/Pine foliage.mat", new Color(.18f, .5f, .26f)),
            ("Assets/Art/Round10/Palm frond.mat", new Color(.26f, .6f, .22f)),
            ("Assets/Art/Round3/Island trees.mat", new Color(.24f, .6f, .24f)),
            ("Assets/Art/Round3/Island grass.mat", new Color(.48f, .74f, .28f)),
            ("Assets/Art/Round3/Sea stack moss.mat", new Color(.4f, .64f, .26f)),
        };

        [MenuItem("Sea Glass/Art Direction/Plants and foliage: tropical greens")]
        public static void Apply()
        {
            int n = 0;
            foreach (var (path, colour) in Colours)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m) { Debug.LogWarning("Tropical foliage pass: missing " + path); continue; }
                m.SetColor("_BaseColor", colour); EditorUtility.SetDirty(m); n++;
            }
            GrassyRidge();
            AssetDatabase.SaveAssets();
            Debug.Log("Tropical foliage pass: " + n + " materials recoloured.");
        }
        // The big inland ridge behind the road was bare sand. Tchia keeps sand to the beach and turns the land behind it green, so the
        // ridge gets its own copy of the beach sand with grass on every gentle, upward-facing slope; steeper faces stay sandy.
        static void GrassyRidge()
        {
            var ridge = GameObject.Find("Inland dune ridge"); if (!ridge) { Debug.LogWarning("Tropical foliage pass: no inland ridge"); return; }
            const string path = "Assets/Art/Vegetation/Grassy inland ridge.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_TopTint", new Color(.24f, .5f, .2f, .92f)); m.SetFloat("_TopStart", .8f);
            EditorUtility.SetDirty(m);
            var rend = ridge.GetComponent<Renderer>(); rend.sharedMaterial = m; EditorUtility.SetDirty(rend);
            EditorSceneManager.MarkSceneDirty(ridge.scene); EditorSceneManager.SaveScene(ridge.scene);
        }
    }
}
