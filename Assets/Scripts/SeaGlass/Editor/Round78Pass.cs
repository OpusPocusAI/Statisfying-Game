using UnityEditor;
using UnityEditor.SceneManagement;

namespace SeaGlass.Editor
{
    // Round 78, from the seventy-fourth review (all three: "radial streaks fanning across the foreground sand" in b1). The fix
    // is in Painterly Lit (PainterlyForward.hlsl): the near-field wind ripples break into short crests, fade before they alias,
    // and fade where their crests run along the line of sight (those converged in perspective into spokes); on beach sand the
    // streaky painted texture is faded too. Nothing in the scene changes; this pass only saves, so the round has a menu entry. Safe to re-run.
    public static class Round78Pass
    {
        [MenuItem("Sea Glass/Art Direction/75 Round 78: short sand ripples")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("Round 78: sand ripples");
        }
    }
}
