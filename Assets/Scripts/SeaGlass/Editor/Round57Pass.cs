using UnityEditor;
using UnityEditor.SceneManagement;

namespace SeaGlass.Editor
{
    // Round 57, from the fifty-third review (all three reviewers: the held clean piece is now see-through for one, but reads
    // as "a bulbous, jelly-like lump", "a lumpy blob, not a bottle-wall shard"; two: it overlaps the Amber label):
    // - FrostedGlass, held mode (_Held, set at the sorting trays): shaded as a plate, not a dome. The mesh normal separates
    //   the two near-flat faces from the narrow edge wall. Through a face the scene shifts little and evenly, like a pane;
    //   the edge wall glows a deep saturated hue (the green edge of a pane of glass), with a thin bright lip where it meets
    //   the face, and the face carries one straight soft sheen. The view-based dark rim band that rounded it is gone.
    // - StudioStation: tilted back 30 degrees (was 18) so the edge wall shows as a band of thickness, and raised a little so
    //   it is held against the room, clear of the Amber label.
    // - Runs Round56Pass (clears _Held on saved glass materials; the runtime property block sets it).
    // Safe to re-run.
    public static class Round57Pass
    {
        [MenuItem("Sea Glass/Art Direction/61 Round 57: held glass as a flat shard")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (UnityEditor.EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round56Pass.Apply();
            UnityEngine.Debug.Log("Round 57: held glass shaded as a flat shard (faces and edge wall)");
        }
    }
}
