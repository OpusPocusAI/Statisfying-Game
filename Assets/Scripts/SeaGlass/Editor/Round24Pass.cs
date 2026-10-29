using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 24, rocks, dunes and planting from the twenty-second review (three reviewers averaged):
    // - Shore rocks read as "stacked boxes": rock_stack_* become weathered boulder groups (round24.py, shore_rock_*).
    // - Islands showed "box strata", a pink stripe: far_island_* become one fluted cliff under a draped grassy cap.
    // - Inland dunes were "flat yellow slopes": the ridge is rebuilt as real dunes, gentle lit backs and steep slip
    //   faces turned away from the low north-west sun but towards the beach, so every dune has a shaded side.
    // - Grass and planting were "evenly spaced confetti": beach grass, flowers and shrubs gather into planting
    //   clumps around anchor shrubs; ridge grass grows in clumps behind the crests; the roadside row becomes groups.
    // Safe to re-run: the ridge mesh is named "Inland dune ridge r24" so the reseat uses the right old heights.
    public static class Round24Pass
    {
        const string Folder = "Assets/Art/Terrain";
        const string RidgeMeshName = "Inland dune ridge r24";
        const float X0 = 22f, X1 = 72f, Z0 = -45f, Z1 = 210f, Step = .75f;

        [MenuItem("Sea Glass/Art Direction/36 Round 24: boulders, islands, dunes, clumped planting")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            SwapMeshes("rock_stack_", "shore_rock_", 4);
            SwapMeshes("far_island_", "far_isle_", 3);
            LowShelves();
            Dunes();
            ClumpBeachPlanting();
            RoadsideGroups();
            Signs();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void SwapMeshes(string from, string to, int n)
        {
            for (int k = 0; k < n; k++)
            {
                var m = ModelLibraryImporter.Load(to + k); if (!m) continue;
                foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name == from + k))
                {
                    f.sharedMesh = m;
                    var mc = f.GetComponent<MeshCollider>(); if (mc) mc.sharedMesh = m;
                    EditorUtility.SetDirty(f);
                }
            }
        }

        // The broad, low cove shelves were tall sea stacks squashed flat, so their terraces read as stacked tiers.
        // Low ones become boulder groups; the tall stacks stay.
        static void LowShelves()
        {
            var boulders = Enumerable.Range(0, 4).Select(k => ModelLibraryImporter.Load("shore_rock_" + k)).ToArray();
            if (boulders.Any(b => !b)) return;
            var root = GameObject.Find("ART • cove landmarks"); if (!root) return;
            int k2 = 0;
            foreach (var f in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!f.sharedMesh || !f.sharedMesh.name.StartsWith("sea_stack")) continue;
                var b = f.GetComponent<Renderer>().bounds;
                if (b.size.y > 3.5f || b.size.y > Mathf.Max(b.size.x, b.size.z) * .45f) continue;
                var mats = f.GetComponent<MeshRenderer>().sharedMaterials;
                f.sharedMesh = boulders[k2++ % boulders.Length];
                f.GetComponent<MeshRenderer>().sharedMaterials = new[] { mats[0] };
                var mc = f.GetComponent<MeshCollider>(); if (mc) mc.sharedMesh = f.sharedMesh;
                // Refit to the old footprint and height.
                var nb = f.GetComponent<Renderer>().bounds;
                var s = f.transform.localScale;
                f.transform.localScale = new Vector3(s.x * b.size.x / nb.size.x, s.y * b.size.y / nb.size.y, s.z * b.size.z / nb.size.z);
                nb = f.GetComponent<Renderer>().bounds;
                f.transform.position += new Vector3(b.center.x - nb.center.x, b.min.y - nb.min.y, b.center.z - nb.center.z);
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.transform);
            }
        }

        // The beach section signs stood edge-on to the beach; they turn 40° to face the sand and the road.
        static void Signs()
        {
            var env = GameObject.Find("COAST • authored environment"); if (!env) return;
            var posts = env.transform.Cast<Transform>().Where(t => t.name == "Sign post").ToList();
            foreach (var s in env.transform.Cast<Transform>().Where(t => t.name == "Beach section sign"))
            {
                if (Mathf.Abs(Mathf.DeltaAngle(s.eulerAngles.y, 0)) > 1) continue;   // already turned
                var post = posts.OrderBy(p => Flat(p.position - s.position)).FirstOrDefault();
                var pivot = post ? post.position : s.position;
                s.RotateAround(pivot, Vector3.up, 40);
                if (post) post.RotateAround(pivot, Vector3.up, 40);
                EditorUtility.SetDirty(s);
            }
            // The section names are separate text objects beside each sign.
            foreach (var t in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None).Select(m => m.transform).Where(t => t.parent == env.transform && t.name.Contains("  /  ")))
            {
                if (Mathf.Abs(Mathf.DeltaAngle(t.eulerAngles.y, 0)) > 1) continue;
                var post = posts.OrderBy(p => Flat(p.position - t.position)).FirstOrDefault();
                if (!post || Flat(post.position - t.position) > 3) continue;
                t.RotateAround(post.position, Vector3.up, 40);
                EditorUtility.SetDirty(t);
            }
        }

        // ------------------------------------------------------------------ dunes
        static float P(float x, float z, int seed) => Mathf.PerlinNoise(x * .045f + seed * 13.1f, z * .045f + seed * 7.3f);

        // Dune phase across the crests: crests run nearly inland (along x), slip faces face south-south-east.
        static float Phase(float x, float z, out float along)
        {
            float s = -.3f * x + .95f * z;
            along = .95f * x + .3f * z;
            float warp = (P(x * .6f, z * .6f, 4) - .5f) * 22f;
            float ph = (s + warp) / Wave;
            return ph - Mathf.Floor(ph);
        }

        const float Wave = 30f;

        static float SMin(float a, float b, float k)
        {
            float h = Mathf.Max(k - Mathf.Abs(a - b), 0) / k;
            return Mathf.Min(a, b) - h * h * k * .25f;
        }

        public static float DuneHeight(float x, float z)
        {
            float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(27f, 40f, x));
            float baseH = .9f + P(x * .5f, z * .5f, 1) * 4.5f;
            float f = Phase(x, z, out float along);
            // The low sun sits behind the beach views, so only slip faces turned south-south-east AND steeper than about
            // 34° fall past the light edge while still being seen side-on from the beach: a short, steep face.
            const float cut = .18f;
            // Steep slip face, then a long gently falling back: one asymmetric dune per wavelength. The two meet in
            // a smooth minimum, so the crest is a rounded brow of sand, not a knife edge or a peak.
            float face = f / cut, back = Mathf.Pow(Mathf.Max(0, 1 - (f - cut) / (1 - cut)), 1.5f);
            if (f < cut) back = 1 + (cut - f) * .4f;
            float p = Mathf.Max(0, SMin(face, back, .35f) / (1 - .35f * .25f));
            float amp = (2.6f + P(x * .8f + 9, z * .4f, 5) * 4f) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(30f, 42f, x));
            // Crest height swells and dips along each dune (smoothly, so no peaks or steps).
            amp *= .6f + .4f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.25f, .7f, P(along * .7f, (-.3f * x + .95f * z) * .3f, 6)));
            return Mathf.Lerp(-.1f, baseH + amp * Mathf.Max(0, p), rise);
        }

        static void Dunes()
        {
            var root = GameObject.Find("ART • inland dune ridge"); if (!root) return;
            var ridge = root.transform.Find("Inland dune ridge"); if (!ridge) return;
            var mf = ridge.GetComponent<MeshFilter>();
            bool already = mf.sharedMesh && mf.sharedMesh.name == RidgeMeshName;
            System.Func<float, float, float> oldH = TerrainShapePass.RidgeHeight;
            if (already)
            {
                // Re-run: the old surface is whatever the saved mesh holds (the height function may have changed since).
                var ov = mf.sharedMesh.vertices; int onx = Mathf.RoundToInt((X1 - X0) / Step) + 1;
                oldH = (x, z) =>
                {
                    float gx = Mathf.Clamp((x - X0) / Step, 0, onx - 1.001f), gz = Mathf.Clamp((z - Z0) / Step, 0, ov.Length / onx - 1.001f);
                    int i = (int)gx, j = (int)gz; float fx = gx - i, fz = gz - j;
                    float a = Mathf.Lerp(ov[j * onx + i].y, ov[j * onx + i + 1].y, fx), b = Mathf.Lerp(ov[(j + 1) * onx + i].y, ov[(j + 1) * onx + i + 1].y, fx);
                    return Mathf.Lerp(a, b, fz);
                };
            }

            // Everything standing on the ridge (shrubs, flowers, trees) follows the new surface.
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var t = r.transform; var p = t.position;
                if (p.x < 27.2f || p.x > X1 || p.z < Z0 || p.z > Z1 || t.IsChildOf(root.transform)) continue;
                if (r.bounds.size.x > 14 || r.bounds.size.z > 14) continue;
                float h0 = oldH(p.x, p.z);
                if (Mathf.Abs(p.y - h0) > 1.6f) continue;
                t.position = new Vector3(p.x, p.y + DuneHeight(p.x, p.z) - h0, p.z);
                EditorUtility.SetDirty(t);
            }

            // Mesh.
            int nx = Mathf.RoundToInt((X1 - X0) / Step) + 1, nz = Mathf.RoundToInt((Z1 - Z0) / Step) + 1;
            var v = new Vector3[nx * nz];
            for (int j = 0; j < nz; j++)
                for (int i = 0; i < nx; i++)
                {
                    float x = X0 + i * Step, z = Z0 + j * Step;
                    v[j * nx + i] = new Vector3(x, DuneHeight(x, z), z);
                }
            var tris = new List<int>();
            for (int j = 0; j < nz - 1; j++)
                for (int i = 0; i < nx - 1; i++)
                {
                    int a = j * nx + i, b = a + 1, c = a + nx, d = c + 1;
                    tris.AddRange(new[] { a, c, b, b, c, d });
                }
            string path = Folder + "/" + RidgeMeshName + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear(); mesh.name = RidgeMeshName;
            mesh.indexFormat = v.Length > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.vertices = v; mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
            EditorUtility.SetDirty(mesh);
            mf.sharedMesh = mesh; EditorUtility.SetDirty(mf);

            // Grass and outcrops: rebuilt in clumps.
            var tuft = ModelLibraryImporter.Load("dune_grass_tuft");
            Material[] grassMats = null; Material[] rockMats = null;
            foreach (Transform c in root.transform)
            {
                var r = c.GetComponent<MeshRenderer>(); if (!r) continue;
                if (c.name == "Ridge grass" && grassMats == null) grassMats = r.sharedMaterials;
                if (c.name == "Ridge outcrop" && rockMats == null) rockMats = r.sharedMaterials;
            }
            var shrubSrc = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh && f.sharedMesh.name == "coastal_shrub_leafy");
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                var c = root.transform.GetChild(i);
                if (c.name == "Ridge grass" || c.name == "Ridge outcrop" || c.name == "Ridge shrub") Object.DestroyImmediate(c.gameObject);
            }
            if (!tuft || grassMats == null) return;
            var rng = new System.Random(2424);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            float Gauss() { double u1 = 1 - rng.NextDouble(), u2 = rng.NextDouble(); return (float)(System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2)); }
            int clumps = 0, tufts = 0;
            for (int k = 0; k < 4000 && clumps < 62; k++)
            {
                float x = R(29, 70), z = R(Z0 + 3, Z1 - 3);
                float f = Phase(x, z, out _);
                // Marram grass holds the gentle backs just behind the crest.
                if (f < .23f || f > .55f) continue;
                clumps++;
                int n = rng.Next(5, 12); float spread = R(1.2f, 2.6f);
                for (int t = 0; t < n; t++)
                {
                    float gx = x + Gauss() * spread * .6f, gz = z + Gauss() * spread;
                    float centre = Mathf.Exp(-((gx - x) * (gx - x) + (gz - z) * (gz - z)) / (spread * spread * 1.4f));
                    Place("Ridge grass", root.transform, tuft, grassMats, new Vector3(gx, DuneHeight(gx, gz) - .05f, gz), R(0, 360), (1.2f + centre * 1.4f) * R(.85f, 1.15f), false);
                    tufts++;
                }
            }
            // Dark shrubs in the hollows at the foot of the slip faces, each with a little grass.
            var hollows = new List<Vector2>();
            if (shrubSrc)
            {
                var sm = shrubSrc.GetComponent<MeshRenderer>().sharedMaterials;
                int shrubs = 0;
                for (int k = 0; k < 4000 && shrubs < 40; k++)
                {
                    float x = R(30, 68), z = R(Z0 + 3, Z1 - 3);
                    float f = Phase(x, z, out _);
                    if (f > .07f && f < .9f) continue;
                    shrubs++;
                    hollows.Add(new Vector2(x, z));
                    int m = rng.Next(1, 3);
                    for (int s = 0; s < m; s++)
                    {
                        float sx = x + R(-1.2f, 1.2f), sz = z + R(-1.2f, 1.2f);
                        Place("Ridge shrub", root.transform, shrubSrc.sharedMesh, sm, new Vector3(sx, DuneHeight(sx, sz) - .15f, sz), R(0, 360), R(1.3f, 2.1f), true);
                    }
                    for (int t = 0; t < 3; t++)
                    {
                        float gx = x + Gauss() * 1.6f, gz = z + Gauss() * 1.6f;
                        Place("Ridge grass", root.transform, tuft, grassMats, new Vector3(gx, DuneHeight(gx, gz) - .05f, gz), R(0, 360), R(1.1f, 1.7f), false);
                    }
                }
            }
            // The older, evenly spread ridge shrubs gather into those hollows too; some are retired.
            if (hollows.Count > 0)
            {
                int kept = 0, hidden = 0;
                foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name == "coastal_shrub_leafy" && f.transform.root.name == "ART • round 3 dressing" && f.transform.position.x >= 27.2f).ToList())
                {
                    var t = f.transform; var p = t.position;
                    if (!already && rng.NextDouble() < .45) { t.gameObject.SetActive(false); hidden++; continue; }
                    var c = hollows.OrderBy(h => (h - new Vector2(p.x, p.z)).sqrMagnitude).First();
                    float ang = R(0, Mathf.PI * 2), rad = Mathf.Abs(Gauss()) * 1.6f + .8f;
                    float nx2 = c.x + Mathf.Cos(ang) * rad, nz2 = c.y + Mathf.Sin(ang) * rad;
                    t.position = new Vector3(nx2, p.y - DuneHeight(p.x, p.z) + DuneHeight(nx2, nz2), nz2);
                    EditorUtility.SetDirty(t); kept++;
                }
                Debug.Log("Round 24 ridge shrubs: " + kept + " gathered into " + hollows.Count + " hollows, " + hidden + " retired.");
            }
            // A few boulder outcrops breaking through the slip faces.
            var boulders = Enumerable.Range(0, 4).Select(k => ModelLibraryImporter.Load("shore_rock_" + k)).Where(m => m).ToArray();
            if (rockMats == null) rockMats = new[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Coastal stone.mat") };
            int outcrops = 0;
            for (int k = 0; k < 4000 && outcrops < 9 && boulders.Length > 0; k++)
            {
                float x = R(36, 66), z = R(Z0 + 10, Z1 - 10);
                float f = Phase(x, z, out _);
                if (f > .16f || f < .04f) continue;
                outcrops++;
                var o = Place("Ridge outcrop", root.transform, boulders[rng.Next(boulders.Length)], rockMats, new Vector3(x, DuneHeight(x, z) - .5f, z), R(0, 360), 1, true);
                o.transform.localScale = new Vector3(R(3, 4.5f), R(2f, 3f), R(3, 4.5f));
            }
            Debug.Log("Round 24 dunes: " + clumps + " grass clumps (" + tufts + " tufts), " + outcrops + " outcrops.");
        }

        static GameObject Place(string name, Transform parent, Mesh mesh, Material[] mats, Vector3 pos, float yaw, float scale, bool shadows)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent);
            g.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            g.transform.localScale = Vector3.one * scale;
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
            if (!shadows) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.BatchingStatic);
            return g;
        }

        // ------------------------------------------------------------------ beach planting
        static float Ground(float x, float z, out bool ok)
        {
            ok = false; float best = -999;
            foreach (var h in Physics.RaycastAll(new Vector3(x, 40, z), Vector3.down, 80, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.GetComponentInParent<ShoreItem>()) continue;
                if (h.point.y > best && h.point.y < 3f) { best = h.point.y; ok = true; }
            }
            return best;
        }

        static void ClumpBeachPlanting()
        {
            Physics.SyncTransforms();
            var tuft = ModelLibraryImporter.Load("dune_grass_tuft");
            var all = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh).ToList();
            var shrubs = all.Where(f => f.sharedMesh.name == "coastal_shrub_leafy" && f.transform.position.x < 25 && f.transform.root.name == "ART • round 3 dressing").Select(f => f.transform).ToList();
            var grass = all.Where(f => f.sharedMesh == tuft && f.transform.position.x < 25 && (f.transform.root.name == "ART • round 3 dressing" || f.transform.root.name == "ART • shoreline composition")).Select(f => f.transform).ToList();
            var flowers = all.Where(f => f.sharedMesh.name == "flower_sprig" && f.transform.position.x < 25).Select(f => f.transform).ToList();
            if (grass.Count == 0) return;

            // Clump anchors: well-spread tufts (farthest-point sampling, at least 11 m apart, so bare sand lies between). The few beach shrubs
            // stand along the road edge already and stay where they are.
            var anchors = new List<Transform> { grass[0] };
            while (anchors.Count < 34)
            {
                Transform bestT = null; float bestD = 0;
                foreach (var s in grass)
                {
                    float d = anchors.Min(a => Flat(a.position - s.position));
                    if (d > bestD) { bestD = d; bestT = s; }
                }
                if (bestT == null || bestD < 11f) break;
                anchors.Add(bestT);
            }
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => i.transform.position).ToList();
            var avoid = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .Where(r => r.name == "Resting bench" || r.name == "Round parasol pole" || r.name == "Beach section sign" || r.name == "Sign post" || r.name.Contains("picnic") || r.name.Contains("Picnic"))
                .Select(r => r.bounds.center).ToList();
            var rng = new System.Random(2425);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            float Gauss() { double u1 = 1 - rng.NextDouble(), u2 = rng.NextDouble(); return (float)(System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2)); }
            int moved = 0, lone = 0;

            void Gather(Transform t, float spread, float keepAlone)
            {
                if (anchors.Contains(t)) return;
                var p = t.position;
                var a = anchors.OrderBy(an => Flat(an.position - p)).First();
                if (Flat(a.position - p) > 14f || rng.NextDouble() < keepAlone) { lone++; return; }
                for (int tries = 0; tries < 8; tries++)
                {
                    float ang = R(0, Mathf.PI * 2), rad = Mathf.Abs(Gauss()) * spread + .5f;
                    var q = a.position + new Vector3(Mathf.Cos(ang) * rad, 0, Mathf.Sin(ang) * rad);
                    if (q.x > 9.5f && q.x < 21f) continue;                                         // the road
                    if (q.x > -12f && q.x < 3f && q.z > 128f && q.z < 153f) continue;          // the walk up to the studio
                    if (items.Any(i => Flat(i - q) < 1.3f) || avoid.Any(i => Flat(i - q) < 1.6f)) continue;
                    float g0 = Ground(p.x, p.z, out bool ok0), g1 = Ground(q.x, q.z, out bool ok1);
                    if (!ok0 || !ok1 || g1 < g0 - .3f) continue;
                    t.position = new Vector3(q.x, p.y + g1 - g0, q.z);
                    // Tufts near the clump centre grow taller.
                    if (t.GetComponent<MeshFilter>().sharedMesh == tuft) t.localScale *= Mathf.Lerp(.85f, 1.25f, Mathf.Exp(-rad * rad / (spread * spread)));
                    EditorUtility.SetDirty(t); moved++; return;
                }
                lone++;
            }
            foreach (var g in grass) Gather(g, 1.3f, .04f);
            foreach (var f in flowers) Gather(f, 1.5f, .08f);
            Debug.Log("Round 24 beach planting: " + anchors.Count + " clumps, " + moved + " moved, " + lone + " left alone.");
        }

        static float Flat(Vector3 d) => Mathf.Sqrt(d.x * d.x + d.z * d.z);

        // ------------------------------------------------------------------ roadside
        static void RoadsideGroups()
        {
            var rootGo = GameObject.Find("ART • coastal pines"); if (!rootGo) return;
            var root = rootGo.transform;
            if (root.Find("Roadside companion")) return;   // already grouped
            var trees = root.Cast<Transform>().ToList();
            var shrubSrc = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh && f.sharedMesh.name == "coastal_shrub_leafy");
            var palmSrc = trees.FirstOrDefault(t => t.GetComponent<MeshFilter>() && t.GetComponent<MeshFilter>().sharedMesh.name.StartsWith("palm"));
            var rng = new System.Random(2426);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            var groups = new List<Vector3>();
            foreach (var t in trees)
            {
                var p = t.position;
                float x = R(25.4f, 28.5f), z = p.z + R(-6f, 6f);
                t.position = new Vector3(x, p.y + DuneHeight(x, z) - DuneHeight(p.x, p.z), z);
                t.rotation = Quaternion.Euler(R(-4, 4), R(0, 360), R(-4, 4));
                EditorUtility.SetDirty(t);
                groups.Add(t.position);
                if (palmSrc && rng.NextDouble() < .7)
                {
                    var c = Object.Instantiate(palmSrc.gameObject, root); c.name = "Roadside companion";
                    float cx = Mathf.Max(25.2f, x + R(-1f, 2.5f)), cz = z + (rng.NextDouble() < .5 ? -1 : 1) * R(2f, 3.5f);
                    c.transform.position = new Vector3(cx, t.position.y + DuneHeight(cx, cz) - DuneHeight(x, z) - .1f, cz);
                    c.transform.rotation = Quaternion.Euler(R(-6, 6), R(0, 360), R(-6, 6));
                    c.transform.localScale = palmSrc.localScale * R(.72f, .92f);
                }
                if (shrubSrc)
                    for (int s = 0; s < rng.Next(1, 3); s++)
                    {
                        float sx = Mathf.Max(24.4f, x + R(-1.8f, 1.8f)), sz = z + R(-1.8f, 1.8f);
                        var b = Object.Instantiate(shrubSrc.gameObject, root); b.name = "Roadside shrub";
                        b.transform.position = new Vector3(sx, DuneHeight(sx, sz) - .12f, sz);
                        b.transform.rotation = Quaternion.Euler(0, R(0, 360), 0);
                        b.transform.localScale = shrubSrc.transform.lossyScale * R(.9f, 1.3f);
                    }
            }
            // The evenly spaced roadside grass gathers round the tree groups.
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Roadside sea grass"))
            {
                var t = r.transform; var p = t.position;
                var g = groups.OrderBy(q => Mathf.Abs(q.z - p.z)).First();
                if (rng.NextDouble() < .15) continue;
                float ang = R(0, Mathf.PI * 2), rad = R(.8f, 3.2f);
                float x = Mathf.Max(23.6f, g.x + Mathf.Cos(ang) * rad), z = g.z + Mathf.Sin(ang) * rad;
                t.position = new Vector3(x, p.y + DuneHeight(x, z) - DuneHeight(p.x, p.z), z);
                EditorUtility.SetDirty(t);
            }
        }
    }
}
