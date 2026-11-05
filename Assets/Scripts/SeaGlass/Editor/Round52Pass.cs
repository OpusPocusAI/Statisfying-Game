using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 52, from the forty-eighth review (all three reviewers, as a bug: the sea-stack shadows on the round 51
    // seabed read as "inverted, hard-edged bright wedges", one running to the horizon in b4):
    // - The long low-sun shadows of the stacks fell on the seabed, and the sunlit sand between them showed through the
    //   water as bright yellow-green bands, so the shadowed water looked like ordinary water and the lit water looked
    //   like the anomaly. The painterly shader gains a per-material _SunShadow amount (default 1, so every other
    //   material is unchanged); the seabed sets it to 0 and is evenly lit. TidalWater.shader lays the shade on the
    //   water surface instead, softly (to 86%), which darkens the shallows where a stack shades them.
    // - The seabed sand is cooler and a touch darker, so the lit bed under the low orange sun reads turquoise through
    //   the water rather than yellow.
    // - Round 51's seabed ran on past both ends of the beach as a straight-edged shallow plate (b4); it now sinks away
    //   over 30 m beyond the sand (Round51Pass), and this pass rebuilds it.
    // Safe to re-run.
    public static class Round52Pass
    {
        [MenuItem("Sea Glass/Art Direction/56 Round 52: evenly lit seabed, soft shade on the shallows")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round51Pass.Apply();   // rebuilds the seabed mesh with the sunken ends (and resets its material, set again below)
            var bed = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round51/Seabed sand.mat");
            if (!bed) throw new System.InvalidOperationException("Run Round 51 first (no seabed material).");
            bed.SetFloat("_SunShadow", 0);
            bed.SetColor("_BaseColor", new Color(.76f, .75f, .70f));
            EditorUtility.SetDirty(bed); AssetDatabase.SaveAssetIfDirty(bed);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 52: seabed evenly lit (no sun shadow), cooler; shade laid softly on the water surface");
        }
    }
}
