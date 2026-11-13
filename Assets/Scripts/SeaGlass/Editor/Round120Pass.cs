using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 120, from the hundred-and-sixteenth review (all three: the Pearl tray pieces "frosted plastic", "close to plastic"):
    // the Pearl tray shared the beach pebble's cream (.92, .94, .84), which under the frost reads as white plastic. White sea glass
    // is clear glass with a faint cool cast; the tray copy (Round118Pass) takes one. The beach pebbles and the mosaic keep theirs.
    // Run after Round118Pass (which copies the source colour over the tray copy). Safe to re-run.
    public static class Round120Pass
    {
        [MenuItem("Sea Glass/Art Direction/119 Round 120: a cool cast for the Pearl tray")]
        public static void Apply()
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Glass Pearl tray.mat");
            if (!m) throw new System.InvalidOperationException("Run Round118Pass first.");
            var c = new Color(.86f, .95f, .93f, 1);
            m.SetColor("_BaseColor", c); if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
            Debug.Log("Round 120: Pearl tray " + c);
        }
    }
}
