using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 117, from the hundred-and-thirteenth review (all three: the tray plates "flat wafers", "sparse and scattered flat",
    // "a single layer"; one: neighbouring pieces cutting into each other). Round 116 settled each plate by how much its box overlapped
    // the boxes beneath it, which sank some into their neighbours and left the heaps one piece deep. Now:
    // - Each tray holds 20 plates (was 12): the new ones are copies of the tray's own pieces, turned and resized.
    // - All 20 are spread again over the middle of the tray, denser toward its centre, with two strays near the rim (the old ones
    //   lay in two clusters, and the first try piled the new ones on them six deep).
    // - Every plate is laid flat again and settled from the edge of the tray inward, so the middle pieces come down on the outer ones
    //   and the tray holds a mound. Each tips a few degrees its own way and drops straight down until it touches the felt or a piece
    //   beneath it, found by casting rays from its corners and rims down onto the pieces below and up from theirs onto it, so no
    //   piece cuts into another. A piece that lands on another leans down off it and is dropped again.
    // Safe to re-run (it tops each tray up to 20 and settles them all again).
    public static class Round117Pass
    {
        const int PerTray = 20;

        [MenuItem("Sea Glass/Art Direction/116 Round 117: fuller heaps in the trays")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Leave Play Mode first.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath);
            var rng = new System.Random(1170);
            int added = 0, total = 0, resting = 0;
            var temp = new List<MeshCollider>();
            try
            {
                foreach (var tray in Object.FindObjectsByType<SortingTray>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(t => t.color))
                {
                    var spares = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                        .Where(r => r.name == "Spare " + tray.color + " glass").OrderBy(r => r.transform.GetSiblingIndex()).ToList();
                    if (spares.Count == 0) continue;
                    var tb = tray.GetComponent<Collider>() ? tray.GetComponent<Collider>().bounds : tray.GetComponent<Renderer>().bounds;
                    float felt = spares.Min(r => r.bounds.min.y);
                    var mid = new Vector3(tb.center.x, 0, tb.center.z);
                    int start = spares.Count;
                    for (int i = start; i < PerTray; i++)
                    {
                        var src = spares[rng.Next(start)];
                        var go = Object.Instantiate(src.gameObject, src.transform.parent);
                        go.name = src.name;
                        Undo.RegisterCreatedObjectUndo(go, "Round 117");
                        go.transform.rotation = Quaternion.AngleAxis((float)rng.NextDouble() * 360, Vector3.up) * go.transform.rotation;
                        go.transform.localScale *= .85f + (float)rng.NextDouble() * .25f;
                        spares.Add(go.GetComponent<MeshRenderer>()); added++;
                    }
                    // Spread over the middle of the tray (the old pieces lay in two clusters, and piled on them the new ones stood six deep),
                    // denser toward the middle so the heap there is two pieces deep; the last two stray toward the rim.
                    for (int i = 0; i < spares.Count; i++)
                    {
                        bool stray = i >= spares.Count - 2;
                        float rad = stray ? .95f + (float)rng.NextDouble() * .15f : Mathf.Pow((float)rng.NextDouble(), .6f) * .9f;
                        float ang = (float)(rng.NextDouble() * Mathf.PI * 2);
                        var tr = spares[i].transform;
                        Undo.RecordObject(tr, "Round 117");
                        tr.position = new Vector3(mid.x + Mathf.Cos(ang) * rad * (tb.extents.x - .12f), felt + .3f, mid.z + Mathf.Sin(ang) * rad * (tb.extents.z - .16f));
                    }
                    // From the rim of the tray inward: the middle pieces come down on the outer ones.
                    var order = spares.OrderByDescending(r => Flat(r.bounds.center - mid).sqrMagnitude).ToList();
                    var placed = new List<(MeshRenderer r, MeshCollider c)>();
                    foreach (var r in order)
                    {
                        Undo.RecordObject(r.transform, "Round 117");
                        var t = r.transform; var mesh = r.GetComponent<MeshFilter>().sharedMesh;
                        // Lay it flat: its thinnest local axis straight up, keeping its turn.
                        var s = mesh.bounds.size;
                        var thin = s.x < s.y && s.x < s.z ? Vector3.right : s.y < s.z ? Vector3.up : Vector3.forward;
                        var w = t.rotation * thin; if (w.y < 0) w = -w;
                        t.rotation = Quaternion.FromToRotation(w, Vector3.up) * t.rotation;
                        float tilt = 3f + (float)rng.NextDouble() * 7f, a = (float)(rng.NextDouble() * 360);
                        t.rotation = Quaternion.AngleAxis(tilt, Quaternion.Euler(0, a, 0) * Vector3.right) * t.rotation;
                        var c = r.gameObject.AddComponent<MeshCollider>(); c.sharedMesh = mesh; temp.Add(c);
                        var on = Drop(r, c, placed, felt);
                        if (on)
                        {
                            var away = Flat(r.bounds.center - on.bounds.center);
                            if (away.sqrMagnitude < 1e-8f) away = Vector3.right;
                            float lean = 5f + (float)rng.NextDouble() * 7f;
                            t.rotation = Quaternion.AngleAxis(lean, Vector3.Cross(Vector3.up, away.normalized)) * t.rotation;
                            Drop(r, c, placed, felt);
                            resting++;
                        }
                        placed.Add((r, c)); total++;
                    }
                }
            }
            finally
            {
                foreach (var c in temp) if (c) Object.DestroyImmediate(c);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 117: " + total + " tray plates settled (" + added + " added), " + resting + " resting on another");
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        // Lifts r clear of everything placed, then drops it straight down until it touches the felt or a placed piece; returns the
        // piece it came to rest on (null for the felt).
        static MeshRenderer Drop(MeshRenderer r, MeshCollider rc, List<(MeshRenderer r, MeshCollider c)> placed, float felt)
        {
            var t = r.transform; r.ResetBounds();
            float top = placed.Count > 0 ? Mathf.Max(felt, placed.Max(p => p.r.bounds.max.y)) : felt;
            t.position += Vector3.up * (top + .03f - r.bounds.min.y);
            r.ResetBounds(); Physics.SyncTransforms();
            var verts = World(r);
            float best = verts.Min(v => v.y) - felt; MeshRenderer on = null;
            var rb = r.bounds;
            foreach (var (q, qc) in placed)
            {
                var b = q.bounds;
                if (b.max.x < rb.min.x || b.min.x > rb.max.x || b.max.z < rb.min.z || b.min.z > rb.max.z) continue;
                foreach (var v in verts)
                    if (qc.Raycast(new Ray(v, Vector3.down), out var hit, best) && hit.distance < best) { best = hit.distance; on = q; }
                foreach (var v in World(q))
                {
                    if (v.x < rb.min.x || v.x > rb.max.x || v.z < rb.min.z || v.z > rb.max.z) continue;
                    if (rc.Raycast(new Ray(v, Vector3.up), out var hit, best) && hit.distance < best) { best = hit.distance; on = q; }
                }
            }
            t.position += Vector3.down * Mathf.Max(0, best - .0008f);
            r.ResetBounds(); Physics.SyncTransforms();
            return on;
        }

        static Vector3[] World(MeshRenderer r)
        {
            var m = r.transform.localToWorldMatrix;
            return r.GetComponent<MeshFilter>().sharedMesh.vertices.Select(v => m.MultiplyPoint3x4(v)).ToArray();
        }
    }
}
