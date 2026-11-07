using UnityEditor;
using UnityEditor.SceneManagement;

namespace SeaGlass.Editor
{
    // Round 61, from the fifty-seventh review (all three reviewers: round 60's corner rounding also softened the straight
    // broken edges, "rounded jelly lozenges, no straight broken edge"; the cloudy basin piece "a big domed pebble", "a bread
    // roll", "a large domed pillow"):
    // - glass_slab_0..5 (round21.py): the outline is averaged over about +/-10 degrees (was 24), so only sharp tips round over.
    // - FrostedGlass, frosted state: shaded by face and edge wall (the mesh normal), not by view angle. An even frost veil over
    //   the flat faces, the glass hue along a thinner-frosted edge wall, half the bright rim, and no view-based dark rim (it
    //   shaded every cloudy piece like a dome).
    // - The basin piece is a fifth smaller (StudioStation: 42 cm across the mesh, was 52), and the dried sand on a cloudy piece
    //   lies in distinct patches with frosted glass between them (it was a near-solid half-strength coat), and the frost is a
    //   smoother milky veil in the glass colour with fewer white salt patches (it was a grainy beige).
    // - Every glass_slab has one straight broken edge (two had none).
    // - Round37Pass re-imports the models and sets every tray spare and beach find back down on what it rested on.
    // Safe to re-run.
    public static class Round61Pass
    {
        [MenuItem("Sea Glass/Art Direction/65 Round 61: broken edges, flat frosted piece")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round37Pass.Apply();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("Round 61: straight broken edges kept, frosted piece shaded flat");
        }
    }
}
