using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 58, from the fifty-fourth review (all three reviewers: the held clean piece reads as "a thin curved sheet, like
    // cellophane", "acetate"; the tray pieces "thin chips or flakes", not the 30%-to-12% wedge of ART_DIRECTION 5.6):
    // - glass_slab_0..5 (round21.py) are thicker: about 30% of the width at the thick edge and 11% at the thin edge (they
    //   measured about 20% and 8%), with a gentler bottle-wall curl (BEND -.06, was -.1) so the held piece is no bent film.
    // - FrostedGlass _Bend follows the mesh (-.06) on the six sea-glass colours only.
    // - Held mode: the rounded rim counts as edge wall, so the thick edge shows as a deep-coloured band, and the hue runs
    //   from nearly clear at the thin edge to deep at the thick one.
    // - Round37Pass re-imports the models and sets every tray spare and beach find back down on what it rested on.
    // Safe to re-run.
    public static class Round58Pass
    {
        public const float Bend = -.06f;   // matches round21.py BEND

        [MenuItem("Sea Glass/Art Direction/62 Round 58: thicker sea glass")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round37Pass.Apply();
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/Materials/SeaGlass/Glass ")) continue;   // the six sea-glass colours only
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m || !m.shader || m.shader.name != "Sea Glass/Frosted Glass" || !m.HasProperty("_Bend")) continue;
                m.SetFloat("_Bend", Bend); EditorUtility.SetDirty(m); AssetDatabase.SaveAssetIfDirty(m); n++;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 58: thicker sea glass; _Bend " + Bend + " on " + n + " glass materials");
        }
    }
}
