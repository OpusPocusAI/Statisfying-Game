using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 38, from the thirty-fourth review (three reviewers averaged):
    // - "The tap stands behind the bowl; its spout never reaches over it": gooseneck_tap has a longer swan neck
    //   (round36.py), and the tap turns so the spout points at the middle of the basin.
    // - "A coloured glow halo under every tray piece, like glow-sticks": the glass light pools (Round25Pass) are
    //   dimmed to a faint tint.
    // - Shader changes this round: FrostedGlass (clearer clean glass, a smaller main glint plus two small rim
    //   glints, no stepped shadow edge, thinner frost on cloudy glass) and SoftFeedback (dense white soap foam).
    // Safe to re-run.
    public static class Round38Pass
    {
        [MenuItem("Sea Glass/Art Direction/47 Round 38: tap reach, softer glass light")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();

            var filters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
            var tap = filters.FirstOrDefault(f => f.sharedMesh && f.sharedMesh.name == "gooseneck_tap");
            var bowl = filters.FirstOrDefault(f => f.sharedMesh && f.sharedMesh.name == "enamel_bowl");
            if (tap && bowl)
            {
                // The spout reaches along the model's local -Z; turn it to face the basin's centre.
                Vector3 d = bowl.GetComponent<Renderer>().bounds.center - tap.transform.position; d.y = 0;
                float yaw = Mathf.Atan2(-d.x, -d.z) * Mathf.Rad2Deg;
                tap.transform.rotation = Quaternion.Euler(0, yaw, 0);
                EditorUtility.SetDirty(tap.transform);
            }

            var pools = GameObject.Find("ART • glass light pools");
            if (pools)
                foreach (var m in pools.GetComponentsInChildren<Renderer>().Select(r => r.sharedMaterial).Where(m => m && m.HasProperty("_Strength")).Distinct())
                {
                    var c = m.GetColor("_Colour"); float lum = c.r * .3f + c.g * .55f + c.b * .15f;
                    m.SetFloat("_Strength", Mathf.Lerp(.17f, .1f, Mathf.InverseLerp(.3f, .85f, lum)));
                    EditorUtility.SetDirty(m);
                }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
