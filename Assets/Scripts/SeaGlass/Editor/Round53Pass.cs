using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 53, from the forty-ninth review (all three reviewers, two as a bug: "the shore foam sits on an opaque
    // dark-blue strip", b8 and b6):
    // - The first 2 m of water lie over the wet-sand shoulder (ShoreTransitionPolish), not the round 51 seabed. The
    //   shoulder receives hard sun shadows; the seabed does not (round 52). Where the dunes or rocks shade the beach,
    //   the swash showed shadowed wet sand through the water, tinted cool, so a navy ribbon ran along the waterline
    //   with a hard edge against the turquoise. In sunlit stretches the same strip read pale instead (b6).
    // - The seabed now reaches in to 0.25 m from the waterline, 3 cm above the shoulder's underwater strip and always
    //   below the surface (Round51Pass offsets), so everything seen through the water is evenly lit sand and the only
    //   shade on the shallows is the water's own soft one. Above the waterline the shoulder keeps its shadows.
    // Safe to re-run: rebuilds the seabed through rounds 51 and 52.
    public static class Round53Pass
    {
        [MenuItem("Sea Glass/Art Direction/57 Round 53: seabed under the swash, no shadow ribbon")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round52Pass.Apply();
            Debug.Log("Round 53: seabed now covers the swash strip, so the shoulder's shadows stay above the water");
        }
    }
}
