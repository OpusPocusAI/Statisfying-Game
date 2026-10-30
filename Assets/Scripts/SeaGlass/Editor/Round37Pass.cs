using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 37, from the thirty-third review (three reviewers averaged):
    // - Sea glass "plump almonds", "gummy sweets": glass_slab_0..5 are re-modelled flat (about a quarter as thick as
    //   wide), lopsided, mostly with one worn straighter edge and broad dents on the faces (round21.py). Every piece
    //   uses these meshes, so re-importing changes them all; this pass then sets each tray spare and beach find back
    //   down on what it rested on, since the thinner meshes would otherwise hover.
    // - Shader changes this round: FrostedGlass (soft refraction, so the tray edge behind no longer draws a seam
    //   through the held piece; darker silt) and BasinWater (deep middle, sun sparkles, brighter caustics, wobbly rings).
    // Safe to re-run.
    public static class Round37Pass
    {
        [MenuItem("Sea Glass/Art Direction/46 Round 37: flat sea glass")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");

            // Where each static glass piece rests now (its lowest point), before the meshes change.
            var resting = new Dictionary<Renderer, float>();
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var f = r.GetComponent<MeshFilter>();
                if (f && f.sharedMesh && f.sharedMesh.name.StartsWith("glass_slab_")) resting[r] = r.bounds.min.y;
            }

            ModelLibraryImporter.ImportAll();

            int moved = 0;
            foreach (var kv in resting)
            {
                var r = kv.Key; if (!r) continue;
                float dy = kv.Value - r.bounds.min.y;
                if (Mathf.Abs(dy) < 1e-5f) continue;
                r.transform.position += Vector3.up * dy;
                EditorUtility.SetDirty(r.transform); moved++;
            }
            Debug.Log("Round 37: re-seated " + moved + " of " + resting.Count + " glass pieces");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
