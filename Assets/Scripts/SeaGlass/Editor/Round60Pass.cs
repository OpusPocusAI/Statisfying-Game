using UnityEditor;
using UnityEditor.SceneManagement;

namespace SeaGlass.Editor
{
    // Round 60, from the fifty-sixth review (all three reviewers: the held piece "a shield with a pointed tip", against 5.6's
    // "no pointed tips"; the open palm "a flat paddle", "a stiff, flat mitten"):
    // - glass_slab_0..5 (round21.py): the outline radius is averaged over about +/-24 degrees around each angle, so every
    //   convex corner rounds over while the broad outline, straight broken edge and lopsidedness stay.
    // - palm_glove (palm_glove.py): fingers curl up from the knuckles and fan a little, the thumb curls in over the palm, and
    //   the little-finger edge and thumb mound are raised so the palm is cupped. The held piece sits a few millimetres higher
    //   to clear the cup (StudioStation.HoldOnPalm).
    // - Round37Pass re-imports the models and sets every tray spare and beach find back down on what it rested on.
    // Safe to re-run.
    public static class Round60Pass
    {
        [MenuItem("Sea Glass/Art Direction/64 Round 60: rounded glass corners, cupped palm")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round37Pass.Apply();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("Round 60: glass corners rounded, palm cupped");
        }
    }
}
