using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 40, from the thirty-sixth review (three reviewers averaged; all three: "nearly all one almond / lens
    // silhouette", "spread flat in a single layer, not piled"):
    // - glass_slab_0..5 are blunt, broad pieces (round21.py): a squarish chip, a rounded triangle, a D-shaped half-disc,
    //   a pentagon chip, a hexagonal nugget and a triangle shard. Every static piece is set back down on its surface.
    // - The loose glass in each sorting tray gathers into two heaps where pieces overlap and lean on each other,
    //   instead of one even single layer. The light pools under the tray glass follow (Round25Pass, Round38Pass).
    // - Shader change this round: FrostedGlass grime is an opaque, cracked crust the brush cuts clean tracks through.
    // Safe to re-run (heaping is seeded and starts from the same spread each time only on the first run; re-running
    // keeps heaps as heaps).
    public static class Round40Pass
    {
        [MenuItem("Sea Glass/Art Direction/48 Round 40: blunt glass shapes, heaped trays")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");

            // New meshes; re-seat every static piece on what it rested on (Round37Pass records, imports, re-seats).
            Round37Pass.Apply();

            var trays = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name == "sorting_tray")
                .Select(f => f.GetComponent<Renderer>().bounds).ToList();
            var spares = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name.StartsWith("Spare ") && r.name.EndsWith(" glass")).ToList();
            var rng = new System.Random(4040);
            int heaped = 0;
            foreach (var group in spares.GroupBy(r => r.name))
            {
                var pieces = group.OrderBy(r => r.transform.position.x).ThenBy(r => r.transform.position.z).ToList();
                if (pieces.Count < 3) continue;
                float felt = pieces.Min(r => r.bounds.min.y);
                var c = new Vector3(pieces.Average(r => r.transform.position.x), 0, pieces.Average(r => r.transform.position.z));
                var trayB = trays.Where(t => t.Contains(new Vector3(c.x, t.center.y, c.z))).Select(t => (Bounds?)t).FirstOrDefault();
                if (trayB == null) continue;
                var tb = trayB.Value;
                // Two heaps along the tray's long side, a little apart, so the felt still shows between them.
                bool alongX = tb.size.x >= tb.size.z;
                Vector3 axis = alongX ? Vector3.right : Vector3.forward;
                float half = (alongX ? tb.size.x : tb.size.z) * .2f;
                var heaps = new[] { new Vector3(tb.center.x, 0, tb.center.z) - axis * half + (alongX ? Vector3.forward : Vector3.right) * .03f,
                                    new Vector3(tb.center.x, 0, tb.center.z) + axis * half - (alongX ? Vector3.forward : Vector3.right) * .04f };
                var placed = new List<Renderer>();
                for (int i = 0; i < pieces.Count; i++)
                {
                    var r = pieces[i]; var t = r.transform;
                    var h = heaps[i % 2];
                    // Lie flat with a fresh turn first.
                    var flat = Quaternion.FromToRotation(t.TransformDirection(ThinAxis(r)), Vector3.up) * t.rotation;
                    t.rotation = Quaternion.AngleAxis((float)rng.NextDouble() * 360, Vector3.up) * flat;
                    // A few candidate spots around the heap; take the lowest, and never more than about two layers up
                    // (an early version stacked towers).
                    float best = float.MaxValue; Vector3 bestXZ = Vector3.zero; bool bestOn = false;
                    for (int tryN = 0; tryN < 8; tryN++)
                    {
                        float ang = (float)rng.NextDouble() * Mathf.PI * 2, rad = (float)System.Math.Sqrt(rng.NextDouble()) * Mathf.Min(tb.size.x, tb.size.z) * .26f;
                        var xz = h + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * rad;
                        xz.x = Mathf.Clamp(xz.x, tb.min.x + tb.size.x * .16f, tb.max.x - tb.size.x * .16f);
                        xz.z = Mathf.Clamp(xz.z, tb.min.z + tb.size.z * .16f, tb.max.z - tb.size.z * .16f);
                        t.position = new Vector3(xz.x, t.position.y, xz.z);
                        var b = r.bounds; float rest0 = felt; bool on = false;
                        foreach (var q in placed)
                        {
                            var qb = q.bounds;
                            bool overlap = Mathf.Abs(qb.center.x - b.center.x) < (qb.extents.x + b.extents.x) * .7f && Mathf.Abs(qb.center.z - b.center.z) < (qb.extents.z + b.extents.z) * .7f;
                            if (overlap && qb.max.y - qb.size.y * .3f > rest0) { rest0 = qb.max.y - qb.size.y * .3f; on = true; }
                        }
                        float cap = felt + b.size.y * 1.4f;
                        float score = rest0 > cap ? 99 + rest0 : rest0 + (float)rng.NextDouble() * b.size.y * .5f;   // a little randomness so some do rest on others
                        if (score < best) { best = score; bestXZ = xz; bestOn = on && rest0 <= cap; }
                    }
                    t.position = new Vector3(bestXZ.x, t.position.y, bestXZ.z);
                    var bb = r.bounds; float rest = felt; bool onGlass = false;
                    foreach (var q in placed)
                    {
                        var qb = q.bounds;
                        bool overlap = Mathf.Abs(qb.center.x - bb.center.x) < (qb.extents.x + bb.extents.x) * .7f && Mathf.Abs(qb.center.z - bb.center.z) < (qb.extents.z + bb.extents.z) * .7f;
                        if (overlap && qb.max.y - qb.size.y * .3f > rest) { rest = qb.max.y - qb.size.y * .3f; onGlass = true; }
                    }
                    rest = Mathf.Min(rest, felt + bb.size.y * 1.4f); onGlass = onGlass && bestOn;
                    if (onGlass)
                    {
                        // Leaning on the piece below: tipped a little about a random level axis.
                        var tiltAxis = Quaternion.AngleAxis((float)rng.NextDouble() * 360, Vector3.up) * Vector3.right;
                        t.rotation = Quaternion.AngleAxis(8 + (float)rng.NextDouble() * 12, tiltAxis) * t.rotation;
                    }
                    t.position += Vector3.up * (rest - r.bounds.min.y);
                    EditorUtility.SetDirty(t); placed.Add(r); heaped++;
                }
            }
            Debug.Log("Round 40: heaped " + heaped + " tray pieces");

            // Light pools follow the glass; then dimmed again; then laid back on the felt (a pool under a piece that
            // rests on another would otherwise hover on the glass below).
            Round25Pass.Apply();
            Round38Pass.Apply();
            var pools = GameObject.Find("ART • glass light pools");
            if (pools)
                foreach (var tb in trays)
                {
                    var inTray = pools.GetComponentsInChildren<Transform>().Where(p => p != pools.transform && tb.Contains(new Vector3(p.position.x, tb.center.y, p.position.z))).ToList();
                    if (inTray.Count == 0) continue;
                    float y = inTray.Min(p => p.position.y);
                    foreach (var p in inTray) { p.position = new Vector3(p.position.x, y, p.position.z); EditorUtility.SetDirty(p); }
                }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Vector3 ThinAxis(Renderer r)
        {
            var s = r.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            return s.x < s.y && s.x < s.z ? Vector3.right : (s.y < s.z ? Vector3.up : Vector3.forward);
        }
    }
}
