using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 31, from the twenty-ninth review (three reviewers averaged):
    // - "The front canopy posts pass straight through the hood": the front pillars stood inside the crowned bonnet's
    //   footprint. They move back to the bonnet's rear edge, still under the canopy.
    // - "Hanging planters float in the sky beyond the roof edge": planters hung past the open side move inside.
    // - Glass pieces were "thin almond and arrow slivers": round21.py slabs are thicker, rounder pebbles with a
    //   smoother outline; this pass re-imports them.
    // Shader change: FrostedGlass (the brush stroke clears with a ragged edge, not a straight capsule line).
    // Safe to re-run.
    public static class Round31Pass
    {
        [MenuItem("Sea Glass/Art Direction/42 Round 31: pillars, planters, rounder glass")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();

            var truck = Object.FindAnyObjectByType<UtilityTruck>();
            var bonnet = truck ? truck.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name == "Soft bonnet") : null;
            if (truck && bonnet)
            {
                float rear = bonnet.bounds.min.z;
                foreach (var r in truck.GetComponentsInChildren<Renderer>(true).Where(r => r.name == "Cab pillar"))
                {
                    var b = r.bounds; if (b.min.z < rear - .02f) continue;     // already behind the bonnet
                    r.transform.position += Vector3.forward * (rear - .06f - b.max.z);
                    EditorUtility.SetDirty(r.transform);
                }
            }

            // Planters inside the roof line (the open side of the studio is at about x = 6).
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "Hanging planter" && t.position.z > 150 && t.position.z < 180))
                if (t.position.x > 4.0f) { t.position = new Vector3(3.4f, t.position.y, t.position.z); EditorUtility.SetDirty(t); }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
