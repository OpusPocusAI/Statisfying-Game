using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Painted surface pass (owner, November 16: Tchia is the art target). Stylised tropical games paint their surfaces by hand:
    // rock, bark, leaves and painted wood all carry visible brush work, strokes of lighter, warmer paint and darker, cooler paint
    // laid over the base colour, and the light/shadow edge is brushed rather than a clean line. The painterly shader now draws
    // procedural strokes (_Brush strength, _BrushScale strokes per metre) in each object's own space, so they stay on moving
    // things. This pass sets them by kind of surface: strongest and broadest on rock and land, finer on bark, leaves and wood,
    // light on painted props, faint on the truck's smooth panels (reviews flagged mottling there), and off where a surface
    // already carries its own detail (beach sand, the road) or must stay clean (glass, lenses, paintings, labels).
    // Repeatable (it only sets two values per material). Rules are checked in order; the first match wins.
    public static class TropicalBrushPass
    {
        static readonly (string[] keys, float brush, float scale)[] Rules =
        {
            // Off: surfaces with their own detail, or that must read clean.
            (new[] { "Butterfly", "sand", "Sun-faded road", "glass", "Glass", "lens", "Lens", "bulb", "Painting", "Chalkboard", "label", "plaque", "Under-floor", "Purple shadows", "Jar contents", "headlamp", "paper", "Sketch", "Lining", "Tray band" }, 0f, 3f),
            // Ambient creatures: fine, light strokes (AmbientLifePass).
            (new[] { "Ambient/" }, .15f, 8f),
            // The truck and other big smooth enamel panels: faint.
            (new[] { "TruckPolish/", "Lagoon paint", "Warm chalk", "Coral paint", "enamel", "Enamel" }, .08f, 5f),
            // Rock and land: broad, bold strokes.
            (new[] { "Shore boulder", "Coastal stone", "limestone", "sandstone", "Tidal blue grey", "Sea stack moss", "Pebble", "pebble" }, .6f, 2.2f),
            (new[] { "Island rock", "Island grass", "Island trees" }, .55f, .7f),
            // Foliage: leafy dabs.
            (new[] { "Shrub", "Broadleaf", "Fern", "Pine foliage", "Palm frond", "Blue green plants", "Kelp", "wrack" }, .45f, 3f),
            (new[] { "Grass", "Stem green", "Petal", "Flower" }, .3f, 4f),
            // Bark and timber: tighter strokes.
            (new[] { "bark", "Palm ring", "Coconut", "driftwood", "Driftwood", "timber", "Timber", "Crate", "crate", "Workbench", "oak", "floorboard", "wood", "walnut", "Stool", "Wicker", "wicker", "basket", "Cork", "cork", "Rope", "twine" }, .38f, 4.5f),
            // Studio walls, roof and fabric: light, broad.
            (new[] { "plaster", "shiplap", "ceiling", "Ceiling", "Sign", "Lime" }, .12f, 3f),
            (new[] { "roof", "Rug", "Towel", "canvas", "Canvas", "linen", "cloth", "Tote", "Sack", "sack", "packing" }, .28f, 3f),
            (new[] { "Terracotta", "terracotta", "Potting soil", "pottery", "ceramic" }, .32f, 6f),
        };
        const float DefaultBrush = .22f, DefaultScale = 5f;

        [MenuItem("Sea Glass/Art Direction/Painted surface: brush strokes")]
        public static void Apply()
        {
            var shader = Shader.Find("Sea Glass/Painterly Lit");
            int n = 0, off = 0;
            foreach (var path in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m || m.shader != shader || path.StartsWith("Assets/_Recovery")) continue;
                // Keys match the file name (a folder named SeaGlass must not count as glass); a key with a slash matches the folder.
                string file = System.IO.Path.GetFileName(path);
                var rule = Rules.FirstOrDefault(r => r.keys.Any(k => k.Contains("/") ? path.Contains(k) : file.Contains(k)));
                float brush = rule.keys != null ? rule.brush : DefaultBrush, scale = rule.keys != null ? rule.scale : DefaultScale;
                // The green inland ridge is beach sand underneath with grass painted on top: brush it like land, the sand
                // faces stay quiet because the shader's own sand detail dominates there.
                if (path.EndsWith("Grassy inland ridge.mat")) { brush = .4f; scale = .9f; }
                m.SetFloat("_Brush", brush); m.SetFloat("_BrushScale", scale); EditorUtility.SetDirty(m);
                n++; if (brush == 0) off++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Painted surface pass: {n} materials set, {off} left without strokes.");
        }
    }
}
