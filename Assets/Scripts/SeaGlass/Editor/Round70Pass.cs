using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 70, from the sixty-sixth review (all three: the brushing sleeve still "a long hose-like tube across the basin";
    // two: its coarse twill reads as stripes). Four arm rounds changed the sleeve's shape, width, cloth and angle without
    // moving that reading; what the panel sees is the stretch of arm between the brush and the frame edge, set by the camera:
    // - StudioStation: the basin camera stands closer and a little left and aims past the piece, so the glass sits low and
    //   right of centre and only the hand, cuff and a short stretch of sleeve show.
    // - Cloth denim: plain cloth (the weave all but gone), so the sleeve no longer reads as stripes.
    // Safe to re-run.
    public static class Round70Pass
    {
        [MenuItem("Sea Glass/Art Direction/71 Round 70: basin framing, plain sleeve")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var denim = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round68/Cloth denim.mat");
            if (denim) { denim.SetFloat("_WeaveStrength", .025f); EditorUtility.SetDirty(denim); }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 70: plain sleeve");
        }
    }
}
