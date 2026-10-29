using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 17, from the fifteenth independent blind review: the long golden-hour shadows on the beach were
    // "near-opaque violet slabs". The sun's shadow strength drops from 1 to 0.75 so shadowed sand keeps its
    // lilac tint but stays lit (Firewatch, LMD). Shader changes this round: FrostedGlass (the background shows
    // through clean glass) and BasinWater (room reflection, clearer middle). Safe to re-run.
    public static class Round17Pass
    {
        [MenuItem("Sea Glass/Art Direction/30 Round 17: softer sun shadows")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional && l.name == "Golden hour sun") { l.shadowStrength = .75f; EditorUtility.SetDirty(l); }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
