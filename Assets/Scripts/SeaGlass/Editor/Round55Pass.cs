using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 55, from the fifty-first review (all three reviewers: the held clean piece "reads as a dished saucer with a
    // raised lip", not a domed wedge of bottle wall):
    // - Round 54 turned the piece held at the sorting trays inner (concave) face out, to lose a crease. Sea glass is seen
    //   convex side on, so it is turned back (this pass sets its scene rotation; Round54Pass's turn is undone if it re-runs
    //   first).
    // - The crease was the dark tray rim behind the piece, bent by the curved face. StudioStation now holds the piece
    //   10 cm higher, so the light felt and heaped glass show through it instead.
    // Safe to re-run.
    public static class Round55Pass
    {
        [MenuItem("Sea Glass/Art Direction/59 Round 55: held glass convex side out, over the felt")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            int turned = 0;
            foreach (var st in Object.FindObjectsByType<SeaGlass.StudioStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (st.kind == SeaGlass.StationKind.Sort && st.workingGlass)
                { st.workingGlass.rotation = Quaternion.Euler(90, 0, 0); EditorUtility.SetDirty(st.workingGlass); turned++; }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 55: " + turned + " held sort piece turned convex side out");
        }
    }
}
