using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 18, from the sixteenth independent blind review: round 17's lighter wash water matched the pale
    // frosted piece, so the cloudy glass "barely shows up at all". The water goes back to a deeper teal.
    // Shader changes this round: FrostedGlass (deeper clean colour, denser crust with a soft dark edge),
    // BasinWater (denser middle), PaintedSky (clouds shaded by a clean ramp, higher band). Safe to re-run.
    public static class Round18Pass
    {
        [MenuItem("Sea Glass/Art Direction/31 Round 18: deeper wash water")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var water = GameObject.Find("Basin water");
            if (water)
            {
                var m = water.GetComponent<MeshRenderer>().sharedMaterial;
                m.SetColor("_BaseColor", new Color(.12f, .5f, .54f)); m.SetColor("_Deep", new Color(.05f, .33f, .4f));
                EditorUtility.SetDirty(m);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
