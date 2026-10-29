using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 28, from the twenty-sixth review (three reviewers averaged):
    // - Tray glass was "nearly all the same almond lens": the six slabs (round21.py) now have clearly different
    //   outlines and thicknesses; this pass re-imports them (the trays and shore finds already use them).
    // - "Grass standing in the sea": some tufts and flowers were left hanging in the air where round 24 swapped
    //   tall rock stacks for lower boulders; seen against the sea they looked planted in the water. They now sit on
    //   whatever is below them.
    // Shader change this round: FrostedGlass (a small sharp glint instead of a blown-out highlight blob, fewer and
    // dimmer star flashes). Safe to re-run.
    public static class Round28Pass
    {
        [MenuItem("Sea Glass/Art Direction/39 Round 28: glass shapes, grounded planting")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            GroundPlanting();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void GroundPlanting()
        {
            Physics.SyncTransforms();
            int moved = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).ToList())
            {
                if (!f.sharedMesh) continue;
                string n = f.sharedMesh.name;
                if (!(n.Contains("grass") || n.Contains("tuft") || n.Contains("flower") || n.Contains("shrub"))) continue;
                var p = f.transform.position;
                if (p.x > 27) continue;   // the inland ridge has no collider; its planting follows the dune height
                float ground = float.MinValue;
                foreach (var h in Physics.RaycastAll(new Vector3(p.x, p.y + .2f, p.z), Vector3.down, 30, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (h.collider.transform.IsChildOf(f.transform) || h.point.y <= ground) continue;
                    // Some rocks kept the collider of the taller mesh they replaced; only trust a hit that lies on
                    // what is actually drawn.
                    var vis = h.collider.GetComponent<Renderer>();
                    if (!vis || !vis.enabled || h.point.y > vis.bounds.max.y + .1f) continue;
                    ground = h.point.y;
                }
                if (ground == float.MinValue || p.y - ground < .3f) continue;
                f.transform.position = new Vector3(p.x, ground - .04f, p.z);
                EditorUtility.SetDirty(f.transform); moved++;
            }
            Debug.Log("Round 28: " + moved + " floating plants set down.");
        }
    }
}
