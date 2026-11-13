using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 125, from the hundred-and-twenty-first review (all three: the suds "popcorn clusters of white spheres"): the brushing foam
    // is drawn with foam_mound (round125.py), a low, lobed mound of foam, in place of foam_clump, a dome ringed by distinct balls; the
    // many small bubbles are drawn on it by FoamClump.shader. Safe to re-run.
    public static class Round125Pass
    {
        [MenuItem("Sea Glass/Art Direction/121 Round 125: soft foam mounds")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Leave Play Mode first.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath);
            ModelLibraryImporter.Import("ArtSource/blender/out/foam_mound.json");
            var mesh = ModelLibraryImporter.Load("foam_mound");
            if (!mesh) throw new System.InvalidOperationException("Import foam_mound first (round125.py).");
            var fb = Object.FindFirstObjectByType<Feedback>(FindObjectsInactive.Include);
            if (!fb) throw new System.InvalidOperationException("No Feedback in the scene.");
            fb.foamMesh = mesh;
            EditorUtility.SetDirty(fb);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 125: foam mounds wired to " + fb.name);
        }
    }
}
