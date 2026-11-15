using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Rocks and landforms pass (owner, November 15: Tchia is the art target). Under the tropical daylight the shore boulders read
    // lavender, the Sunset Point stacks brick-red and the island cliffs pink-tan. Tchia-style coastal rock is a warm grey volcanic
    // stone that goes cool blue-grey in shadow, with moss and grass taking over every face that looks up; the islands are mostly
    // green, with rock showing only on the steep faces. Every rock material moves that way. Repeatable (it only sets colours).
    public static class TropicalRockPass
    {
        // path, base colour, moss colour (a = amount), moss starts at normal.y
        static readonly (string path, Color colour, Color moss, float start)[] Rocks =
        {
            ("Assets/Art/Beach/Shore boulder.mat", new Color(.7f, .66f, .6f), new Color(.34f, .54f, .22f, .8f), .74f),
            ("Assets/Materials/SeaGlass/Coastal stone.mat", new Color(.72f, .67f, .6f), new Color(.36f, .56f, .22f, .7f), .76f),
            ("Assets/Art/CoveLandmarks/Shell cove limestone.mat", new Color(.78f, .75f, .68f), new Color(.38f, .6f, .24f, .75f), .72f),
            ("Assets/Art/CoveLandmarks/Sunset sandstone.mat", new Color(.74f, .67f, .59f), new Color(.32f, .56f, .22f, .9f), .7f),
            ("Assets/Art/Round27/Island rock with grass.mat", new Color(.66f, .61f, .55f), new Color(.36f, .62f, .24f, 1f), .5f),
        };
        static readonly (string path, Color colour)[] Pebbles =
        {
            // (Pebbles are set by TropicalPropPass.)
            ("Assets/Art/Round3/Sea stack moss.mat", new Color(.34f, .58f, .24f)),
        };

        [MenuItem("Sea Glass/Art Direction/Rocks and landforms: mossy grey stone")]
        public static void Apply()
        {
            int n = 0;
            foreach (var (path, colour, moss, start) in Rocks)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m) { Debug.LogWarning("Tropical rock pass: missing " + path); continue; }
                m.SetColor("_BaseColor", colour); m.SetColor("_TopTint", moss); m.SetFloat("_TopStart", start);
                EditorUtility.SetDirty(m); n++;
            }
            foreach (var (path, colour) in Pebbles)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m) { Debug.LogWarning("Tropical rock pass: missing " + path); continue; }
                m.SetColor("_BaseColor", colour); EditorUtility.SetDirty(m); n++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Tropical rock pass: " + n + " materials recoloured.");
        }
    }
}
