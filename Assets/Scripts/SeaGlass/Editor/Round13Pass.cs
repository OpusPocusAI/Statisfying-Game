using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 13, from the tenth independent blind review: the scrub brush gets a rounded wooden back and a dense
    // mass of splayed bristle tufts (ArtSource/blender/round13.py), in the hand and on the recycling bench.
    // The cleaning itself (brush-path grime, frost crust, glints, foam, ripples) is runtime and shader work.
    // Safe to re-run.
    public static class Round13Pass
    {
        [MenuItem("Sea Glass/Art Direction/26 Round 13: bristle-tuft brush")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var tuft = ModelLibraryImporter.Load("scrub_brush_tuft");
            if (!tuft) throw new System.InvalidOperationException("Import the scrub_brush_tuft model first.");
            int swapped = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name == "scrub_brush"))
            {
                f.sharedMesh = tuft; EditorUtility.SetDirty(f); swapped++;
            }
            Debug.Log("Round 13: swapped " + swapped + " scrub brushes to the bristle-tuft model.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
