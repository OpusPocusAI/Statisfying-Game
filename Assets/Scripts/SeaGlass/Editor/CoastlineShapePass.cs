using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaGlass.Editor
{
    // Coastline shape pass (owner, November 16: Tchia is the art target). From above and from the beach the coast read as one
    // long, nearly straight strip: the authored headlands only pushed the waterline 4-5 m out. Stylised tropical coasts are a
    // chain of curving coves between sandy, rocky points. This pass:
    // - reshapes the authored coastline (CoastShape.Points, mirrored in TidalWater.shader): the five headlands now run out
    //   10-16 m into the sea as rounded points, the bays between them stay where they were, so each bay is a real cove;
    // - remaps everything that was laid out against the old waterline: the wet-sand shoulder (and its saved round 44/82
    //   sources), the rock surf strips, the headland boulder spines, cove landmark rocks, tide-line shells, kelp and wrack.
    //   Shoulder geometry stretches proportionally between the dry-shelf edge (X -32.1) and the waterline; anything in the
    //   water moves out with the waterline, fading to no move 30-60 m offshore (the far islands stay put);
    // - rebuilds the seabed along the new line (rounds 51-53);
    // - lays a raised dry-sand lobe on each point (ART • sand points), rising gently from the beach edge to a low crest and
    //   tucking under the wet sand near the water, dressed with leaning palms and grass;
    // - re-places the fish schools (AmbientLifePass) against the new shallows.
    // All of it is scenery seaward of the invisible boundary at X -33: playable land, collision and finds are unchanged.
    // The remap runs once (guarded by the sand-points root); later runs only rebuild the lobes and their dressing.
    public static class CoastlineShapePass
    {
        const string Root = "ART • sand points";
        const float Edge = -32.1f;   // where the dry shelf ends and the wet shoulder starts to slope

        // The coastline before this pass (CoastShape.Points as of November 16).
        static readonly Vector2[] OldPoints =
        {
            new(-45, -35.0f), new(-20, -37.6f), new(0, -34.4f), new(18, -33.9f), new(33, -33.6f),
            new(48, -38.2f), new(63, -33.8f), new(85, -38.4f), new(105, -33.8f), new(127, -37.4f),
            new(150, -35.0f), new(174, -37.8f), new(205, -35.0f),
        };

        static float OldShore(float z)
        {
            var saved = (Vector2[])CoastShape.Points.Clone();
            for (int i = 0; i < saved.Length; i++) CoastShape.Points[i] = OldPoints[i];
            float x = CoastShape.ShoreX(z);
            for (int i = 0; i < saved.Length; i++) CoastShape.Points[i] = saved[i];
            return x;
        }

        static float RemapX(float x, float z)
        {
            float o = OldShore(z), n = CoastShape.ShoreX(z);
            if (x >= Edge) return x;
            if (x >= o) return Edge + (x - Edge) / (o - Edge) * (n - Edge);     // on the shoulder: proportional
            float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(30, 60, o - x));
            return x + (n - o) * fade;                                       // in the water: moves with the waterline
        }

        static Vector3 Remap(Vector3 p) => new(RemapX(p.x, p.z), p.y, p.z);

        static void RemapMesh(Mesh mesh, Transform t)
        {
            if (!mesh) return;
            var v = mesh.vertices;
            for (int i = 0; i < v.Length; i++) v[i] = t ? t.InverseTransformPoint(Remap(t.TransformPoint(v[i]))) : Remap(v[i]);
            mesh.vertices = v; mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
        }

        [MenuItem("Sea Glass/Art Direction/Coastline shape: coves and sandy points")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            if (OldPoints.Length != CoastShape.Points.Length) throw new System.InvalidOperationException("CoastShape point count changed.");

            var shoulder = GameObject.Find("Soft wet-sand shoreline");
            if (!shoulder) throw new System.InvalidOperationException("No wet-sand shoreline in the scene.");
            var moved = new List<(Transform t, float oldGround)>();
            bool first = !GameObject.Find(Root);
            if (first)
            {
                // Shoulder: the live mesh and the saved sources later rounds rebuild from.
                var live = shoulder.GetComponent<MeshFilter>().sharedMesh;
                RemapMesh(live, shoulder.transform);
                foreach (var path in new[] { "Assets/Art/ShoreTransition/Blended shore shoulder (round 44 source).asset", "Assets/Art/ShoreTransition/Blended shore shoulder (round 82 source).asset" })
                {
                    var m = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (m && m != live) RemapMesh(m, shoulder.transform);
                }
                var col = shoulder.GetComponent<MeshCollider>(); if (col) { col.sharedMesh = null; col.sharedMesh = live; }
                // Rock surf strips around the cove landmarks.
                var surf = GameObject.Find("ART • rock contact surf");
                if (surf) foreach (var f in surf.GetComponentsInChildren<MeshFilter>()) RemapMesh(f.sharedMesh, f.transform);
                // Rocks and tide-line dressing laid out against the old waterline.
                foreach (var rootName in new[] { "ART • rocky points", "ART • cove landmarks", "ART • beach details", "ART • round 3 dressing" })
                {
                    var r = GameObject.Find(rootName); if (!r) continue;
                    foreach (Transform t in r.transform.Cast<Transform>().ToList())
                    {
                        var p = t.position; if (p.x >= Edge) continue;
                        moved.Add((t, OldGround(p.x, p.z)));
                        t.position = Remap(p);
                    }
                }
                Debug.Log($"Coastline shape: remapped the shoulder, rock surf and {moved.Count} rocks and tide-line pieces");
            }

            // Seabed along the new line (rebuilds rounds 51-53).
            Round53Pass.Apply();
            scene = EditorSceneManager.GetActiveScene();
            shoulder = GameObject.Find("Soft wet-sand shoreline");

            // Sand lobes on the points.
            var old = GameObject.Find(Root); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(Root).transform;
            var shelf = GameObject.Find("Continuous sand shelf");
            // The lobes wear the shoulder's own material and wash coordinates, so where they meet it there is no change of tone,
            // and near the water they darken into the wet band at the same distance from the waterline as in the bays.
            var sand = shoulder.GetComponent<MeshRenderer>().sharedMaterial;
            var profile = WetProfile(shoulder);
            bool tempShoulder = !shoulder.GetComponent<MeshCollider>();
            var shoulderCol = shoulder.GetComponent<MeshCollider>(); if (!shoulderCol) shoulderCol = shoulder.AddComponent<MeshCollider>();
            shoulderCol.sharedMesh = shoulder.GetComponent<MeshFilter>().sharedMesh;
            Physics.SyncTransforms();

            var lobes = new List<MeshCollider>();
            var rng = new System.Random(1116); float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            foreach (float zc in CoastShape.Headlands)
            {
                var mesh = Lobe(zc, shoulderCol, rng, profile);
                if (!mesh) continue;
                var go = new GameObject("Sand point", typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(root);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.GetComponent<MeshRenderer>(); mr.sharedMaterial = sand; mr.shadowCastingMode = ShadowCastingMode.Off;
                var mc = go.AddComponent<MeshCollider>(); mc.sharedMesh = mesh; lobes.Add(mc);
            }
            Physics.SyncTransforms();

            float NewGround(Vector3 p)
            {
                float best = float.MinValue;
                var ray = new Ray(new Vector3(p.x, 20, p.z), Vector3.down);
                foreach (var c in lobes.Cast<Collider>().Append(shoulderCol)) if (c.Raycast(ray, out var hit, 40)) best = Mathf.Max(best, hit.point.y);
                return best > float.MinValue ? best : -1.3f;
            }
            // Pieces that sat on the old shoulder keep their footing on the new ground (rocks in the water stay at their depth).
            foreach (var (t, g0) in moved)
            {
                if (g0 <= -1.29f) continue;
                var p = t.position; p.y += NewGround(p) - g0; t.position = p;
            }

            // Dressing: one to three palms leaning out to sea, grass and tufts on the crest of each point.
            var palm = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Grove palm").OrderBy(r => r.transform.position.z).ToArray();
            var grass = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Dune grass cluster" || r.name == "Sand tuft").ToArray();
            int palms = 0, tufts = 0;
            foreach (var lobe in lobes)
            {
                var b = lobe.bounds; var spots = new List<Vector3>();
                for (int k = 0; k < 400 && spots.Count < 26; k++)
                {
                    var p = new Vector3(R(b.min.x, b.max.x), 20, R(b.min.z, b.max.z));
                    if (!lobe.Raycast(new Ray(p, Vector3.down), out var hit, 40) || hit.point.y < .02f || hit.point.x > -34) continue;
                    if (spots.Any(s => (s - hit.point).sqrMagnitude < 1.4f)) continue;
                    spots.Add(hit.point);
                }
                spots = spots.OrderByDescending(s => s.y).ToList();
                int n = palm.Length > 0 ? rng.Next(1, 4) : 0;
                for (int i = 0; i < n && i < spots.Count; i++)
                {
                    if (spots.Take(i).Any(s => Vector3.Distance(s, spots[i]) < 3)) continue;
                    var src = palm[rng.Next(palm.Length)];
                    var go = Object.Instantiate(src.gameObject, root); go.name = "Point palm";
                    foreach (var c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                    go.transform.position = spots[i] - Vector3.up * .05f;
                    go.transform.rotation = Quaternion.Euler(0, 180 + R(-50, 50), 0);   // the groves' palms lean along yaw 180 (seaward)
                    go.transform.localScale = src.transform.localScale * R(.85f, 1.05f); palms++;
                }
                foreach (var s in spots.Skip(n))
                {
                    if (grass.Length == 0 || R(0, 1) < .35f) continue;
                    var src = grass[rng.Next(grass.Length)];
                    var go = Object.Instantiate(src.gameObject, root); go.name = src.name;
                    foreach (var c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                    go.transform.SetPositionAndRotation(s - Vector3.up * .03f, Quaternion.Euler(0, R(0, 360), 0));
                    go.transform.localScale = src.transform.localScale * R(.8f, 1.2f); tufts++;
                }
            }
            foreach (var mc in lobes) Object.DestroyImmediate(mc);
            if (tempShoulder) Object.DestroyImmediate(shoulderCol);

            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            AmbientLifePass.Apply();   // fish schools follow the new shallows
            Debug.Log($"Coastline shape: {lobes.Count} sand points, {palms} palms, {tufts} grass tufts");
        }

        // The shoulder's wash coordinate (uv.x) against distance inland of the waterline, read from a bay row the reshaping left
        // at its usual width, so a point's wet band is as wide as a bay's.
        static (float d, float u)[] WetProfile(GameObject shoulder)
        {
            var mesh = shoulder.GetComponent<MeshFilter>().sharedMesh; var v = mesh.vertices; var uv = mesh.uv; var t = shoulder.transform;
            int rows = v.Length / 7, best = 0; float bestDz = float.MaxValue;
            for (int r = 0; r < rows; r++) { float dz = Mathf.Abs(t.TransformPoint(v[r * 7]).z - 105); if (dz < bestDz) { bestDz = dz; best = r; } }
            var list = new List<(float d, float u)>();
            for (int c = 2; c < 7; c++) { var w = t.TransformPoint(v[best * 7 + c]); list.Add((w.x - CoastShape.ShoreX(w.z), uv[best * 7 + c].x)); }
            return list.OrderBy(p => p.d).ToArray();
        }

        static float Profile((float d, float u)[] p, float d)
        {
            if (d <= p[0].d) return p[0].u; if (d >= p[p.Length - 1].d) return p[p.Length - 1].u;
            for (int i = 0; i < p.Length - 1; i++) if (d <= p[i + 1].d) return Mathf.Lerp(p[i].u, p[i + 1].u, (d - p[i].d) / (p[i + 1].d - p[i].d));
            return p[p.Length - 1].u;
        }

        // Height of the old wet-sand shoulder (CoastShape.ShoulderY against the old waterline).
        static float OldGround(float x, float z)
        {
            var saved = (Vector2[])CoastShape.Points.Clone();
            for (int i = 0; i < saved.Length; i++) CoastShape.Points[i] = OldPoints[i];
            float y = CoastShape.ShoulderY(x, z);
            for (int i = 0; i < saved.Length; i++) CoastShape.Points[i] = saved[i];
            return y;
        }

        // A raised dry-sand lobe over a point: it starts just under the dry shelf's edge, rises seaward to a low crest along
        // the point, and dips back under the wet shoulder about a metre from the water, so the wet band still shows.
        static Mesh Lobe(float zc, MeshCollider shoulder, System.Random rng, (float d, float u)[] profile)
        {
            float zs = zc, ze = zc;
            while (zs > zc - 40 && CoastShape.ShoreX(zs - .5f) < -35.2f) zs -= .5f;
            while (ze < zc + 40 && CoastShape.ShoreX(ze + .5f) < -35.2f) ze += .5f;
            if (ze - zs < 6) return null;
            // Waterline samples for the true horizontal distance to the water (the flanks run steeply across Z).
            var line = new List<Vector2>();
            for (float z = zs - 25; z <= ze + 25; z += .25f) line.Add(new Vector2(CoastShape.ShoreX(z), z));
            float tip = Enumerable.Range(0, 161).Select(i => CoastShape.ShoreX(zs + (ze - zs) * i / 160f)).Min();
            float height = Mathf.Clamp((Edge - tip) * .045f, .35f, .7f);
            float seed = (float)rng.NextDouble() * 50;

            const int cols = 44; var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
            int rows = Mathf.CeilToInt((ze - zs) / .5f);
            for (int r = 0; r <= rows; r++)
            {
                float z = zs + r * .5f, xa = -31.7f, xb = CoastShape.ShoreX(z) + .2f;
                for (int c = 0; c <= cols; c++)
                {
                    float x = Mathf.Lerp(xa, xb, c / (float)cols);
                    bool onShoulder = shoulder.Raycast(new Ray(new Vector3(x, 20, z), Vector3.down), out var hit, 40);
                    float gy = onShoulder ? hit.point.y : CoastShape.ShoulderY(x, z);
                    float dist = line.Min(q => (q - new Vector2(x, z)).sqrMagnitude); dist = Mathf.Sqrt(dist);
                    float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(Edge + .2f, Edge - 4.5f, x));          // from the beach edge out
                    float shore = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.9f, 5.5f, dist));                   // down to the wet band
                    float ripple = .85f + .3f * Mathf.PerlinNoise(x * .18f + seed, z * .18f);
                    float crest = height * rise * shore * ripple;
                    // Edges sit 3 cm under the shoulder, the body sits on top of it.
                    float tuck = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.5f, 1.4f, dist)) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(Edge + .35f, Edge - .4f, x));
                    verts.Add(new Vector3(x, gy - .03f + tuck * .05f + crest, z));
                    float u = x > Edge && onShoulder ? hit.textureCoord.x : Profile(profile, x - CoastShape.ShoreX(z));
                    uvs.Add(new Vector2(u, onShoulder ? hit.textureCoord.y : 0));
                    if (r < rows && c < cols)
                    {
                        int a = r * (cols + 1) + c, b = a + cols + 1;
                        tris.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
                    }
                }
            }
            var mesh = new Mesh { name = "Sand point " + zc };
            mesh.indexFormat = verts.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(verts); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals(); if (mesh.normals.Length > 0 && mesh.normals[verts.Count / 2].y < 0) { tris.Reverse(); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); }
            mesh.RecalculateBounds();
            string path = "Assets/Art/Coastline/Sand point " + zc + ".asset";
            if (!AssetDatabase.IsValidFolder("Assets/Art/Coastline")) AssetDatabase.CreateFolder("Assets/Art", "Coastline");
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing) { existing.Clear(); EditorUtility.CopySerialized(mesh, existing); Object.DestroyImmediate(mesh); return existing; }
            AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
    }
}
