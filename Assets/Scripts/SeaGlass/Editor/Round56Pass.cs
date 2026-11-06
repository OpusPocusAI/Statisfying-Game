using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 56, from the fifty-second review (all three reviewers: the held clean piece reads as "an opaque lump", "an ice
    // cube", "thick opaque jade, muddy"; it needs to be clearly see-through and saturated, ART_DIRECTION 5.6):
    // - FrostedGlass gains _Held. The sorting station sets it on the piece held up to the eye (StudioStation.UpdateGlass):
    //   the scene behind stays sharp and keeps its own light and shade, filtered by the glass hue (deeper where the wedge is
    //   thick), bent more towards the curved rim, with a darker refraction band and a thin bright edge. Fine pitting is
    //   gentler there, so the glint no longer spatters white over the face.
    // - The held piece is smaller and nearer (22 cm across the mesh at 0.7 m, was 36 cm at 0.85 m), raised against the lit room
    //   rather than the plain felt and tilted back 18 degrees (was 35) so its face shows: a jewel held up to the
    //   light rather than a slab filling the view.
    // - This pass sets _Held back to 0 on every saved glass material, so only the runtime property block turns it on.
    // Safe to re-run.
    public static class Round56Pass
    {
        [MenuItem("Sea Glass/Art Direction/60 Round 56: see-through held glass")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material"))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (!m || !m.shader || m.shader.name != "Sea Glass/Frosted Glass" || !m.HasProperty("_Held")) continue;
                if (m.GetFloat("_Held") == 0) continue;
                m.SetFloat("_Held", 0); EditorUtility.SetDirty(m); AssetDatabase.SaveAssetIfDirty(m); n++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Round 56: see-through held glass; _Held cleared on " + n + " saved glass materials");
        }
    }
}
