using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 23, from the twenty-first review (three reviewers averaged):
    // - Glass pieces were "flat-topped wedges": the slab models (round21.py) now have domed tops and tapered
    //   edges; this pass re-imports them and re-runs pass 33 so every piece picks them up.
    // - Dig piles read as "small pointed cones": every mound is a third wider and a third lower.
    // - The sea's far edge showed from the high coast view: the open-water plane now runs under the land and
    //   past the horizon (east and north).
    // - Round 22's floor clusters are rebuilt without the loose jars.
    // Shader changes this round: FrostedGlass (a warm sandy grime film instead of dark bands), PaintedSky (no
    // stray dot clouds). Safe to re-run.
    public static class Round23Pass
    {
        [MenuItem("Sea Glass/Art Direction/35 Round 23: domed glass, soft mounds, sea to the horizon")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round21Pass.Apply();
            Round22Pass.Apply();

            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(i => i.mound))
            {
                var t = item.mound; var s = t.localScale;
                if (s.y < Mathf.Min(s.x, s.z) * .42f) continue;   // already flattened
                t.localScale = new Vector3(s.x * 1.3f, s.y * .66f, s.z * 1.3f);
                EditorUtility.SetDirty(t);
            }

            var sea = GameObject.Find("Open turquoise water");
            if (sea)
            {
                var t = sea.transform;
                t.localScale = new Vector3(420, t.localScale.y, 760);
                t.position = new Vector3(-148 + 210, t.position.y, -105 + 380);
                EditorUtility.SetDirty(t);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
