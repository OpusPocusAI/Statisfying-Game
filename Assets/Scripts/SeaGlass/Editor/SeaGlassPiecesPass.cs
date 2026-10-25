using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 2 (Documentation/ART_DIRECTION.md, section 5.6): every authored glass outline
    // becomes a tumbled pebble of sea glass with rounded edges, a soft dome and uneven thickness.
    // The authored flat polygon (GlassDefinition.mesh) is untouched: gameplay, tests and the mosaic
    // light spill still use it. Displays use GlassDefinition.visualMesh.
    public static class SeaGlassPiecesPass
    {
        const string Folder = "Assets/Art/SeaGlassPieces";
        const string CatalogPath = "Assets/Data/SeaGlassCatalog.asset";

        // Profile in the authored polygon's units (pieces are roughly 0.5–2 units across).
        const float EdgeHalf = .014f;     // half thickness at the very edge
        const float CentreHalf = .09f;    // half thickness once past the rounded edge (round 5: chunkier, less slab-like)
        const float RoundRadius = .22f;   // width of the rounded edge
        const float Dome = .045f;         // extra swell towards the middle
        const int Levels = 3;             // cap subdivision (8 segments per outline edge)

        [MenuItem("Sea Glass/Art Direction/2a Tumbled sea-glass pieces")]
        public static void Apply()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "SeaGlassPieces");
            var swap = new Dictionary<Mesh, Mesh>();
            for (int k = 0; k < catalog.glass.Length; k++)
            {
                var def = catalog.glass[k];
                string path = Folder + "/" + def.id + " tumbled.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, path); }
                Build(def.mesh, mesh, k * 7.31f);
                mesh.name = def.id + " tumbled";
                EditorUtility.SetDirty(mesh);
                def.visualMesh = mesh;
                swap[def.mesh] = mesh;
            }
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            // Point every scene display of an authored polygon at its tumbled version.
            var scene = EditorSceneManager.GetActiveScene();
            if (!EditorApplication.isPlaying && scene.path == CoastBuilder.ScenePath)
            {
                int swapped = 0;
                foreach (var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (filter.sharedMesh && swap.TryGetValue(filter.sharedMesh, out var visual)) { filter.sharedMesh = visual; EditorUtility.SetDirty(filter); swapped++; }
                Debug.Log("Sea glass pieces: " + catalog.glass.Length + " tumbled meshes, " + swapped + " scene displays updated.");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        public static void Build(Mesh source, Mesh target, float seed)
        {
            var src = source.vertices; var srcTris = source.triangles;
            int sourceCount = src.Length / 2;
            var authored = new Vector2[sourceCount];
            for (int i = 0; i < sourceCount; i++) authored[i] = new Vector2(src[i].x, src[i].y);
            // Which way the authored front faces wind (front normals point to -z).
            bool frontClockwise = true;
            for (int i = 0; i < srcTris.Length; i += 3)
                if (srcTris[i] < sourceCount && srcTris[i + 1] < sourceCount && srcTris[i + 2] < sourceCount)
                {
                    var a = src[srcTris[i]]; var b = src[srcTris[i + 1]]; var c = src[srcTris[i + 2]];
                    frontClockwise = Vector3.Cross(b - a, c - a).z < 0; break;
                }

            // Tumbling softens every corner. The rounded outline stays inside the socket shape;
            // the small corner gaps read as lead came once installed.
            var outline = RoundCorners(authored, .32f, 6);   // round 5: well-tumbled corners, pebble-like
            // Round 5: tumbled rims are never ruler-straight. Nudge each outline point inward by a little
            // noise (never outward, so the piece still sits inside its mosaic socket).
            {
                Vector2 c0 = outline.Aggregate(Vector2.zero, (s, p) => s + p) / outline.Length;
                for (int i = 0; i < outline.Length; i++)
                {
                    float w = Mathf.PerlinNoise(outline[i].x * 3.1f + seed * 1.7f, outline[i].y * 3.1f - seed) * .028f;
                    outline[i] += (c0 - outline[i]).normalized * w;
                }
            }
            int n = outline.Length;
            var tris = EarClip(outline);
            // Match the authored front-face orientation.
            if (tris.Count >= 3)
            {
                var a = outline[tris[0]]; var b = outline[tris[1]]; var c = outline[tris[2]];
                bool clockwise = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) < 0;
                if (clockwise != frontClockwise) for (int i = 0; i < tris.Count; i += 3) { int t = tris[i + 1]; tris[i + 1] = tris[i + 2]; tris[i + 2] = t; }
            }

            var pts = new List<Vector2>(outline);
            for (int level = 0; level < Levels; level++)
            {
                var mids = new Dictionary<long, int>();
                int Mid(int a, int b)
                {
                    long key = a < b ? (long)a * 1000003 + b : (long)b * 1000003 + a;
                    if (mids.TryGetValue(key, out int m)) return m;
                    pts.Add((pts[a] + pts[b]) * .5f); mids[key] = pts.Count - 1; return pts.Count - 1;
                }
                var next = new List<int>();
                for (int i = 0; i < tris.Count; i += 3)
                {
                    int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                    int ab = Mid(a, b), bc = Mid(b, c), ca = Mid(c, a);
                    next.AddRange(new[] { a, ab, ca, ab, b, bc, ca, bc, c, ab, bc, ca });
                }
                tris = next;
            }

            // Distance to the outline drives the rounded edge and the dome.
            var dist = new float[pts.Count]; float maxDist = 1e-4f;
            for (int i = 0; i < pts.Count; i++)
            {
                float d = float.MaxValue;
                for (int e = 0; e < n; e++) d = Mathf.Min(d, SegmentDistance(pts[i], outline[e], outline[(e + 1) % n]));
                dist[i] = d < 1e-5f ? 0 : d; maxDist = Mathf.Max(maxDist, dist[i]);
            }

            float Half(int i, float side)
            {
                float d = dist[i];
                float t = Mathf.Clamp01(d / RoundRadius);
                float round = Mathf.Sqrt(1 - (1 - t) * (1 - t));              // quarter-circle edge
                float dome = Mathf.SmoothStep(0, 1, Mathf.Clamp01(d / Mathf.Max(maxDist, RoundRadius)));
                float h = EdgeHalf + (CentreHalf - EdgeHalf) * round + Dome * dome * side;
                // Tumbled pieces are never perfectly even.
                float wobble = Mathf.PerlinNoise(pts[i].x * 1.7f + seed, pts[i].y * 1.7f - seed) - .5f;
                // Broad, gentle undulation over the face (worn hollows and swells), not a flat plate.
                float swell = Mathf.PerlinNoise(pts[i].x * 3.3f - seed * .7f, pts[i].y * 3.3f + seed * .4f) - .5f;
                return h * (1 + wobble * .4f * round) + swell * .03f * round * side;
            }

            int count = pts.Count;
            var vertices = new List<Vector3>(count * 2 + n * 2 * 16);
            for (int i = 0; i < count; i++) vertices.Add(new Vector3(pts[i].x, pts[i].y, -Half(i, 1f)));     // front (towards player)
            for (int i = 0; i < count; i++) vertices.Add(new Vector3(pts[i].x, pts[i].y, Half(i, .55f)));     // flatter back
            var triangles = new List<int>(tris.Count * 2);
            triangles.AddRange(tris);
            for (int i = 0; i < tris.Count; i += 3) triangles.AddRange(new[] { count + tris[i], count + tris[i + 2], count + tris[i + 1] });

            // Thin edge wall around the outline, from the ordered boundary vertices.
            var ring = new List<int>();
            for (int e = 0; e < n; e++)
            {
                Vector2 a = outline[e], b = outline[(e + 1) % n]; float len = (b - a).sqrMagnitude;
                var onEdge = new List<(float t, int i)>();
                for (int i = 0; i < count; i++)
                {
                    if (dist[i] > 0) continue;
                    if (SegmentDistance(pts[i], a, b) > 1e-5f) continue;
                    float t = Vector2.Dot(pts[i] - a, b - a) / len;
                    if (t >= 1 - 1e-5f) continue;   // the end vertex starts the next edge
                    onEdge.Add((t, i));
                }
                ring.AddRange(onEdge.OrderBy(x => x.t).Select(x => x.i));
            }
            Vector2 centre = outline.Aggregate(Vector2.zero, (s, p) => s + p) / n;
            for (int r = 0; r < ring.Count; r++)
            {
                int a = ring[r], b = ring[(r + 1) % ring.Count];
                int fa = vertices.Count; vertices.Add(vertices[a]); vertices.Add(vertices[b]); vertices.Add(vertices[count + b]); vertices.Add(vertices[count + a]);
                var normal = Vector3.Cross(vertices[fa + 1] - vertices[fa], vertices[fa + 3] - vertices[fa]);
                Vector2 mid = (pts[a] + pts[b]) * .5f;
                bool outward = Vector2.Dot(new Vector2(normal.x, normal.y), mid - centre) > 0;
                if (outward) triangles.AddRange(new[] { fa, fa + 1, fa + 2, fa, fa + 2, fa + 3 });
                else triangles.AddRange(new[] { fa, fa + 2, fa + 1, fa, fa + 3, fa + 2 });
            }

            target.Clear();
            target.indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            target.SetVertices(vertices);
            var uv = vertices.Select(v => new Vector2(v.x, v.y)).ToList();
            target.SetUVs(0, uv);
            target.SetTriangles(triangles, 0);
            target.RecalculateNormals();
            target.RecalculateBounds();
            target.RecalculateTangents();
        }

        // Replace each corner with a short arc. The radius shrinks on short edges so slivers survive.
        static Vector2[] RoundCorners(Vector2[] poly, float radius, int segments)
        {
            int n = poly.Length; var result = new List<Vector2>();
            float area = 0; for (int i = 0; i < n; i++) area += poly[i].x * poly[(i + 1) % n].y - poly[(i + 1) % n].x * poly[i].y;
            for (int i = 0; i < n; i++)
            {
                Vector2 prev = poly[(i + n - 1) % n], p = poly[i], next = poly[(i + 1) % n];
                Vector2 toPrev = prev - p, toNext = next - p;
                float r = Mathf.Min(radius, toPrev.magnitude * .42f, toNext.magnitude * .42f);
                Vector2 a = p + toPrev.normalized * r, b = p + toNext.normalized * r;
                // Quadratic Bezier through the corner: a smooth, inset curve that never crosses the outline.
                for (int s = 0; s <= segments; s++)
                {
                    float t = s / (float)segments;
                    result.Add((1 - t) * (1 - t) * a + 2 * (1 - t) * t * p + t * t * b);
                }
            }
            // Drop near-duplicate points.
            var clean = new List<Vector2>();
            foreach (var q in result) if (clean.Count == 0 || (clean[clean.Count - 1] - q).sqrMagnitude > 1e-7f) clean.Add(q);
            if ((clean[0] - clean[clean.Count - 1]).sqrMagnitude < 1e-7f) clean.RemoveAt(clean.Count - 1);
            return clean.ToArray();
        }

        static List<int> EarClip(Vector2[] poly)
        {
            int n = poly.Length; var idx = Enumerable.Range(0, n).ToList();
            float area = 0; for (int i = 0; i < n; i++) area += poly[i].x * poly[(i + 1) % n].y - poly[(i + 1) % n].x * poly[i].y;
            if (area < 0) idx.Reverse();
            var tris = new List<int>(); int guard = 0;
            while (idx.Count > 3 && guard++ < 10000)
            {
                bool clipped = false;
                for (int i = 0; i < idx.Count; i++)
                {
                    int ia = idx[(i + idx.Count - 1) % idx.Count], ib = idx[i], ic = idx[(i + 1) % idx.Count];
                    Vector2 a = poly[ia], b = poly[ib], c = poly[ic];
                    if ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) <= 1e-9f) continue;   // reflex
                    bool inside = false;
                    foreach (int j in idx)
                    {
                        if (j == ia || j == ib || j == ic) continue;
                        if (InTriangle(poly[j], a, b, c)) { inside = true; break; }
                    }
                    if (inside) continue;
                    tris.AddRange(new[] { ia, ib, ic }); idx.RemoveAt(i); clipped = true; break;
                }
                if (!clipped) break;
            }
            if (idx.Count == 3) tris.AddRange(idx);
            return tris;
        }

        static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
            float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
            float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-9f));
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
