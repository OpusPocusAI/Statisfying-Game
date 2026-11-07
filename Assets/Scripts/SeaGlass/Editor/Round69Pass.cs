using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 69, from the sixty-fifth review (all three: round 68's forearm, turned toward the viewer, became "a huge sleeve tube
    // that dominates the frame", hid the brush and crowded the basin; cleaning fell from 4 to 3; one: moire on the sleeve):
    // - The brushing arm comes in from the side again (EquipmentVisuals), with the brush tipped so its bristles face the camera.
    // - garden_glove (glove.py): the sleeve is about a quarter slimmer.
    // - Cloth denim: a coarser, fainter twill, so the near sleeve no longer shimmers into moire.
    // Safe to re-run.
    public static class Round69Pass
    {
        [MenuItem("Sea Glass/Art Direction/70 Round 69: slimmer sleeve at the basin")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var denim = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round68/Cloth denim.mat");
            if (denim) { denim.SetFloat("_WeaveScale", 70); denim.SetFloat("_WeaveStrength", .1f); EditorUtility.SetDirty(denim); }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 69: slimmer sleeve, coarser twill");
        }
    }
}
