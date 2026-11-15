using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Small objects pass (owner, November 15: Tchia is the art target). Tchia-style props are sun-warmed and saturated but never
    // electric: colours lean warm, whites are creamy, blues lean teal. Under the tropical daylight the cool box read pure electric
    // blue, the beach pebbles a grey-lilac speckle across the sand, the road lilac-grey and the shore wrack near-black. Those move
    // toward warm, sunny colours; shells and starfish get richer, and the beach clutter keeps enough contrast to be found.
    // Repeatable (it only sets colours).
    public static class TropicalPropPass
    {
        static readonly (string path, Color colour)[] Colours =
        {
            // Beach kit: warm, sun-faded brights.
            ("Assets/Art/Round3/Cool box blue.mat", new Color(.26f, .72f, .6f)),
            ("Assets/Art/Round3/Cool box white.mat", new Color(.98f, .95f, .88f)),
            ("Assets/Art/Round3/Tote stripe.mat", new Color(.14f, .52f, .62f)),
            ("Assets/Art/Round3/Ball blue.mat", new Color(.16f, .56f, .78f)),
            ("Assets/Art/Round3/Ball red.mat", new Color(.95f, .36f, .26f)),
            ("Assets/Art/Round3/Towel lilac.mat", new Color(.98f, .66f, .56f)),
            // Shore finds: richer, warmer naturals.
            ("Assets/Art/Beach/Shell.mat", new Color(1f, .9f, .8f)),
            ("Assets/Art/Beach/Shell inner.mat", new Color(.98f, .6f, .52f)),
            ("Assets/Art/Beach/Starfish.mat", new Color(1f, .5f, .26f)),
            ("Assets/Art/Beach/Kelp.mat", new Color(.44f, .44f, .18f)),
            ("Assets/Art/Round3/Tide wrack.mat", new Color(.34f, .36f, .16f)),
            ("Assets/Art/ShorelinePolish/Bleached driftwood.mat", new Color(.86f, .76f, .62f)),
            // Pebbles: closer to the sand, so the beach reads clean rather than speckled.
            ("Assets/Art/Round3/Scatter pebble pale.mat", new Color(.98f, .93f, .84f)),
            ("Assets/Art/Round3/Scatter pebble warm.mat", new Color(.84f, .74f, .62f)),
            ("Assets/Art/Round3/Scatter pebble cool.mat", new Color(.74f, .73f, .7f)),
            ("Assets/Art/Beach/Pebble lilac.mat", new Color(.72f, .7f, .66f)),
            ("Assets/Art/Beach/Pebble warm.mat", new Color(.86f, .74f, .6f)),
            // The road: sun-bleached warm grey instead of lilac.
            ("Assets/Art/Beach/Sun-faded road.mat", new Color(.74f, .69f, .62f)),
        };

        [MenuItem("Sea Glass/Art Direction/Small objects: sun-warmed props")]
        public static void Apply()
        {
            int n = 0;
            foreach (var (path, colour) in Colours)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m) { Debug.LogWarning("Tropical prop pass: missing " + path); continue; }
                m.SetColor("_BaseColor", colour); EditorUtility.SetDirty(m); n++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Tropical prop pass: " + n + " materials recoloured.");
        }
    }
}
