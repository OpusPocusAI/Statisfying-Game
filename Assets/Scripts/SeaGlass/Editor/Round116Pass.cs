using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 116, from the hundred-and-twelfth review (all three: the tray plates "flat, white-rimmed chips lying in one layer",
    // "acrylic sequins", "no low heaps"): the spare plates in each tray settle into low heaps, as loose glass does. The trays were
    // dressed in layers of the old thick pebbles; after round 115 swapped them for thin plates at the same heights, the upper ones
    // hung in the air over the lower ones or lay flat beside them. Now, from the lowest up, every piece tips a little off the flat
    // (each its own way) and comes to rest on the felt or on the pieces already beneath it, by how much it overlaps them, leaning
    // further where it rests on another; so each tray holds a heap up to two or three layers deep.
    // Run once after Round115Pass (a second run would tip the pieces again).
    public static class Round116Pass
    {
        [MenuItem("Sea Glass/Art Direction/115 Round 116: heaps in the trays")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Leave Play Mode first.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath);
            var spares = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(r => r.name.StartsWith("Spare ") && r.name.EndsWith(" glass") && r.GetComponent<MeshFilter>().sharedMesh.name.StartsWith("glass_plate_"))
                .ToArray();
            var rng = new System.Random(1160);
            int resting = 0;
            foreach (var group in spares.GroupBy(r => r.sharedMaterial.name).OrderBy(g => g.Key))
            {
                var list = group.OrderBy(r => r.bounds.min.y).ThenBy(r => r.name).ToList();
                float felt = list.Min(r => r.bounds.min.y);
                var placed = new List<Renderer>();
                foreach (var r in list)
                {
                    Undo.RecordObject(r.transform, "Round 116");
                    float a = (float)(rng.NextDouble() * 360), t = 5f + (float)rng.NextDouble() * 9f;
                    r.transform.rotation = Quaternion.AngleAxis(t, Quaternion.Euler(0, a, 0) * Vector3.right) * r.transform.rotation;
                    r.ResetBounds();
                    float support = felt; Renderer on = null;
                    foreach (var q in placed)
                    {
                        float ov = Overlap(r.bounds, q.bounds);
                        if (ov < .04f) continue;
                        float h = felt + (q.bounds.max.y - felt) * Mathf.Min(1f, ov * 1.8f) - .002f;
                        if (h > support) { support = h; on = q; }
                    }
                    if (on)
                    {
                        // Resting on another piece: it leans down off its edge toward the felt.
                        var away = r.bounds.center - on.bounds.center; away.y = 0;
                        if (away.sqrMagnitude < 1e-8f) away = Vector3.right;
                        float lean = 6f + (float)rng.NextDouble() * 8f;
                        r.transform.rotation = Quaternion.AngleAxis(lean, Vector3.Cross(Vector3.up, away.normalized)) * r.transform.rotation;
                        r.ResetBounds();
                        resting++;
                    }
                    r.transform.position += Vector3.up * (support - r.bounds.min.y);
                    r.ResetBounds();
                    placed.Add(r);
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 116: " + spares.Length + " tray plates settled, " + resting + " resting on another");
        }

        // The share of a's footprint (in x and z) that lies over b's.
        static float Overlap(Bounds a, Bounds b)
        {
            float ox = Mathf.Max(0, Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x));
            float oz = Mathf.Max(0, Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z));
            return ox * oz / Mathf.Max(a.size.x * a.size.z, 1e-6f);
        }
    }
}
