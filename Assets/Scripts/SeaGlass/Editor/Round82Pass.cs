using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 82, from the seventy-eighth review (all three: the wet-sand band is "barely visible", "very thin or missing", "almost
    // absent", b1, b3, b4, b6, 02). Round 81 matched the wet-sand strip to the dry sand, which removed its hard edge but also the
    // overall darkening it had carried; the damp tone was left on the strip's narrow slope, seen at a grazing angle.
    // - Painterly Lit (PainterlyForward.hlsl): the wet band mirrors the sky colour, most at grazing angles, with a broad soft sun glint.
    // - This pass: the strip's flat inland part reaches about a metre further inland (its first two columns of seven; the third marks where the dry shelf ends and stays), so the
    //   damp, reflective band has room before the bend. That part wears the dry sand's exact surface, so its new edge stays unseen.
    // Safe to re-run: it always starts from a saved copy of the strip as round 81 left it.
    public static class Round82Pass
    {
        const string SourcePath = "Assets/Art/ShoreTransition/Blended shore shoulder (round 82 source).asset";

        [MenuItem("Sea Glass/Art Direction/79 Round 82: wet-sand sheen")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var go = GameObject.Find("Soft wet-sand shoreline");
            if (!go) throw new System.InvalidOperationException("No wet-sand shoreline in the scene.");
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            var source = AssetDatabase.LoadAssetAtPath<Mesh>(SourcePath);
            if (!source) { source = Object.Instantiate(mesh); source.name = "Blended shore shoulder (round 82 source)"; AssetDatabase.CreateAsset(source, SourcePath); }

            var v = source.vertices; const int across = 7;
            if (v.Length % across != 0) throw new System.InvalidOperationException("Unexpected shoulder layout.");
            int rows = v.Length / across;
            var t = go.transform;
            float[] push = { 1.3f, 0.75f };   // metres inland for the first two columns; the third stays where the dry shelf ends, or the shelf pokes through the slope
            for (int r = 0; r < rows; r++)
            {
                int e = r * across;
                Vector3 w0 = t.TransformPoint(v[e]), w3 = t.TransformPoint(v[e + 3]);
                var inland = new Vector3(w0.x - w3.x, 0, w0.z - w3.z); if (inland.sqrMagnitude < 1e-6f) continue; inland.Normalize();
                for (int c = 0; c < push.Length; c++)
                    v[e + c] = t.InverseTransformPoint(t.TransformPoint(v[e + c]) + inland * push[c]);
            }
            mesh.vertices = v; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            var col = go.GetComponent<MeshCollider>(); if (col) { col.sharedMesh = null; col.sharedMesh = mesh; }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 82: wet-sand strip widened on " + rows + " rows");
        }
    }
}
