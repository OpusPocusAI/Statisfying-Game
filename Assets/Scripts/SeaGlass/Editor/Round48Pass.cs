using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 48, from the forty-fourth review (all three: the trees on the island tops are "near-black lumps with no
    // lit side"; one: the inland dunes read "near-white, like snowdrifts"):
    // - Island trees: the canopy material was a dark blue-green with no top colour, so at 100-170 m it rendered as the
    //   darkest thing in every vista. The base rises to the palette's deep blue-green, and upward faces take a sunlit
    //   yellow-green painted per pixel (the same top tint the island grass uses), so each canopy shows a lit crown over
    //   a shaded side and fades with the hill into the haze. The crowns themselves are wider and flatter and overlap
    //   within a grove (round25.py; island shapes unchanged).
    // - Dunes: round 47's cool bias made the open beach cream, but the inland dune sand, lit less directly, went
    //   snow-white. The dune material warms back toward cream; the beach sand gets a touch of warmth too.
    // - Shader: a wider swing in the ripple direction from patch to patch (review 44: "parallel stripes across the whole
    //   foreground").
    // Safe to re-run.
    public static class Round48Pass
    {
        [MenuItem("Sea Glass/Art Direction/52 Round 48: lit island trees, warmer dunes")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();   // picks up the re-exported far_isle_0..2 crowns

            var trees = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round3/Island trees.mat");
            if (trees)
            {
                trees.SetColor("_BaseColor", new Color(.36f, .56f, .42f));
                trees.SetFloat("_GradientStrength", .06f);
                trees.SetColor("_TopTint", new Color(.66f, .74f, .38f, .9f));
                trees.SetFloat("_TopStart", .15f);
                trees.SetFloat("_Rim", .5f);
                EditorUtility.SetDirty(trees);
            }
            Sand("Assets/Art/ShorelinePolish/Dry dune sand.mat", new Color(.82f, .76f, .68f));
            // The dunes are the same material as the beach; the beach lies at a low angle to the sun and reads cream, while the
            // sun-facing dune slopes blew out to white. The base drops a touch and sun-facing slopes take a warm cream cap
            // (the per-pixel top tint), so the brightest sand is warm rather than white.
            Sand("Assets/Materials/SeaGlass/Apricot sand.mat", new Color(.82f, .80f, .82f));
            var beach = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            if (beach) { beach.SetColor("_TopTint", new Color(.95f, .88f, .76f, .22f)); beach.SetFloat("_TopStart", .6f); EditorUtility.SetDirty(beach); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 48: island canopies lit with a sunlit crown; dune sand warmed back toward cream");
        }

        static void Sand(string path, Color baseColour)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path); if (!m) return;
            m.SetColor("_BaseColor", baseColour); EditorUtility.SetDirty(m);
        }
    }
}
