using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 44, from the fortieth review (three reviewers averaged; all three: "the wet sand is a constant-width strip with
    // a hard, ruled edge against the dry sand"): the wet-sand shoulder along the coast ("Soft wet-sand shoreline", 241
    // rows of 7 vertices, the first of each row on the inland edge) had its inland edge on one straight line. That edge,
    // and the vertex next to it, now wander inland and back along the shore at two scales, so the tide line reads as a
    // wavy, uneven reach.
    // Shader changes this round: FrostedGlass (clarity and colour follow the wedge's real thickness; the frosted hue shows
    // between grime patches) and TidalWater (wash reach and foam width vary along the shore; crisp lace).
    // Safe to re-run: it always starts from a saved copy of the original strip.
    public static class Round44Pass
    {
        const string SourcePath = "Assets/Art/ShoreTransition/Blended shore shoulder (round 44 source).asset";

        [MenuItem("Sea Glass/Art Direction/49 Round 44: wavy wet-sand edge")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var go = GameObject.Find("Soft wet-sand shoreline");
            if (!go) throw new System.InvalidOperationException("No wet-sand shoreline in the scene.");
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            var source = AssetDatabase.LoadAssetAtPath<Mesh>(SourcePath);
            if (!source) { source = Object.Instantiate(mesh); source.name = "Blended shore shoulder (round 44 source)"; AssetDatabase.CreateAsset(source, SourcePath); }

            var v = source.vertices; const int across = 7;
            if (v.Length % across != 0) throw new System.InvalidOperationException("Unexpected shoulder layout.");
            int rows = v.Length / across;
            var t = go.transform;
            for (int r = 0; r < rows; r++)
            {
                int e = r * across, n = e + 1;
                Vector3 we = t.TransformPoint(v[e]), wn = t.TransformPoint(v[n]);
                var dir = new Vector3(we.x - wn.x, 0, we.z - wn.z); float gap = dir.magnitude; if (gap < 1e-4f) continue; dir /= gap;
                float z = we.z;
                float wobble = Mathf.Sin(z * .21f) * .6f + Mathf.Sin(z * .53f + 1.3f) * .3f + Mathf.Sin(z * 1.31f + .7f) * .15f;   // about -1 .. 1
                float push = wobble * 1.4f;   // metres inland (+) or seaward (-)
                push = Mathf.Max(push, -(gap - .3f));   // never cross the next vertex
                we += dir * push; wn += dir * push * .35f;
                v[e] = t.InverseTransformPoint(we); v[n] = t.InverseTransformPoint(wn);
            }
            mesh.vertices = v; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            Debug.Log("Round 44: wavy inland edge on " + rows + " shoreline rows");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
