using UnityEditor;
using UnityEditor.SceneManagement;

namespace SeaGlass.Editor
{
    // Round 67, from the sixty-third review (all three: the first-person sleeve is "a long, uniform tapered tube", "a huge
    // featureless cylinder", ending in a visible cut and crossing the basin rim; two: the open palm is "a stiff, flat mannequin
    // glove" with "sausage" fingers):
    // - garden_glove and palm_glove (glove.py, palm_glove.py) share a shaped work-shirt sleeve (sleeve_shape.py): a forearm
    //   swell, uneven compression folds above the cuff, drape creases, a pushed-up gathered band and an underside seam,
    //   longer so its end leaves the frame. The brushing arm rises at 13 degrees (was 8) so it clears the basin rim.
    // - palm_glove: fingers built joint by joint (tapering, with knuckles, bending at three joints), close together and curled
    //   up around the glass; the thumb curls in beside them; the coral cuff sits nearer the hand so it shows below the palm.
    // Both meshes update in place (same names). Safe to re-run.
    public static class Round67Pass
    {
        [MenuItem("Sea Glass/Art Direction/68 Round 67: shaped sleeve, relaxed palm")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("Round 67: shaped sleeve and relaxed palm imported");
        }
    }
}
