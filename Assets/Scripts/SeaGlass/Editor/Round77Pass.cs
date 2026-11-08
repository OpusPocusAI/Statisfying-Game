using UnityEditor;
using UnityEditor.SceneManagement;

namespace SeaGlass.Editor
{
    // Round 77, from the seventy-third review (all three, and every review since 66: the truck bonnet and fenders "lumpy",
    // "crumpled", "mottled"; two: "possibly bad normals"). truck_shell is rebuilt by round77.py with a finer, smoother fuse,
    // a lighter decimation and normals carried over from the smooth full-resolution surface, so the ivory enamel shades
    // as clean curves. Same name, so the truck updates in place.
    // The blotches themselves were screen-space occlusion noise on the large smooth panels, pooled into the shadow colour:
    // the truck's painted shell materials turn it down (Painterly Lit _AOScale). Safe to re-run.
    public static class Round77Pass
    {
        [MenuItem("Sea Glass/Art Direction/74 Round 77: smooth truck bodywork")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            int n = 0;
            foreach (var name in new[] { "Lagoon paint", "Tidal teal", "Ivory enamel", "Soft charcoal" })
                foreach (var guid in AssetDatabase.FindAssets(name + " t:Material"))
                {
                    var m = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(AssetDatabase.GUIDToAssetPath(guid));
                    if (!m || m.name != name || !m.HasProperty("_AOScale")) continue;
                    m.SetFloat("_AOScale", .15f); EditorUtility.SetDirty(m); n++;
                }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("Round 77: truck shell re-imported, occlusion lowered on " + n + " paint materials");
        }
    }
}
