using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 3 (Documentation/ART_DIRECTION.md, sections 5.2 and 5.3): painted sand with
    // wind ripples, soft dig mounds, faceted rocks, a sun-faded road, small shoreline details and
    // rocky points that break up the straight coastline. Models: ArtSource/blender/beach_assets.py.
    public static class BeachPass
    {
        const string Folder = "Assets/Art/Beach";
        const string SandTexturePath = Folder + "/Painted sand.png";
        const float SandTop = -0.1f;

        [MenuItem("Sea Glass/Art Direction/3 Beach, sand and rocks")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Beach");
            ModelLibraryImporter.ImportAll();
            var sand = SandTexture();

            // ---- Sand: one painted texture mapped in world space so every sand surface joins up.
            Ground("Assets/Materials/SeaGlass/Apricot sand.mat", sand, new Color(.93f, .85f, .73f), .18f);
            Ground("Assets/Art/ShorelinePolish/Dry dune sand.mat", sand, new Color(.93f, .85f, .72f), .14f);
            // Wet sand: same base as the dry sand; the dry-to-wet wash texture darkens and cools it.
            var wet = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShoreTransition/Warm tidal sand.mat");
            if (wet) { wet.SetColor("_BaseColor", new Color(.93f, .85f, .73f)); wet.SetFloat("_Smoothness", .4f); wet.SetFloat("_Variation", .12f); EditorUtility.SetDirty(wet); }
            var dug = Painterly(Folder + "/Turned damp sand.mat", new Color(.92f, .82f, .67f), 0, .1f);
            dug.SetTexture("_BaseMap", sand); dug.SetFloat("_WorldMapping", 1); dug.SetFloat("_WorldTile", 7);

            // ---- Dig mounds: soft, lopsided heaps of turned sand with a scoop mark.
            var mounds = Enumerable.Range(0, 4).Select(k => ModelLibraryImporter.Load("sand_mound_" + k)).ToArray();
            int index = 0;
            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(i => i.id))
            {
                if (!item.mound) continue;
                item.mound.GetComponent<MeshFilter>().sharedMesh = mounds[index % 4];
                // Same sand as the ground: the mound reads by its shape and shading, not a colour patch.
                item.mound.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
                item.mound.localRotation = Quaternion.Euler(0, index * 47 % 360, 0);
                EditorUtility.SetDirty(item.mound); index++;
            }

            // ---- Rocks: faceted planes instead of tumbled blobs. Colliders are spheres and unchanged.
            var rocks = Enumerable.Range(0, 4).Select(k => ModelLibraryImporter.Load("faceted_rock_" + k)).ToArray();
            var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Coastal stone.mat");
            var chalk = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Warm chalk.mat");
            var groundSandMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            stone.SetColor("_BaseColor", new Color(.7f, .63f, .57f)); stone.SetFloat("_GradientStrength", .3f); stone.SetFloat("_GradientHeight", 4f);
            EditorUtility.SetDirty(stone);
            index = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.name == "Wind-shaped coastal stone").OrderBy(f => f.transform.position.z).ThenBy(f => f.transform.position.x))
            {
                var r = f.GetComponent<Renderer>();
                var m = r.sharedMaterial;
                if (m == chalk || (m == groundSandMaterial && f.sharedMesh && f.sharedMesh.name.StartsWith("sand_mound")))
                {
                    // The pale half-buried "chalk" slabs read as sand humps: make them soft sand.
                    if (f.sharedMesh && f.sharedMesh.name != null && !f.sharedMesh.name.StartsWith("sand_mound"))
                    {
                        var s = f.transform.localScale;
                        f.transform.localPosition -= new Vector3(0, .46f * s.y, 0);
                        f.transform.localScale = new Vector3(s.x * 1.1f, s.y * .92f, s.z * 1.1f);
                    }
                    f.sharedMesh = mounds[index++ % 4]; r.sharedMaterials = new[] { groundSandMaterial }; EditorUtility.SetDirty(f); continue;
                }
                if (m != stone) continue;     // distant green hills keep their shape
                f.sharedMesh = rocks[index++ % 4]; EditorUtility.SetDirty(f);
            }

            // ---- Road: its own sun-faded surface (it used to share the grey stone).
            var road = Painterly(Folder + "/Sun-faded road.mat", new Color(.55f, .51f, .52f), 0, .13f);
            road.SetFloat("_Rim", .2f);
            var roadObject = GameObject.Find("Coast road");
            if (roadObject) { roadObject.GetComponent<Renderer>().sharedMaterial = road; EditorUtility.SetDirty(roadObject); }

            // ---- Dunes: sand is soft. Replace origami facets with smooth normals.
            // They reused 14-42 triangle stone/mound meshes; give them the smooth mound shapes.
            var groundSand = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            index = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.name == "Low wind swept dune" || f.name == "Dune crest").OrderBy(f => f.transform.position.z))
            {
                // The old crest mesh was centred (+-0.46); the mound sits on y=0 with height 1.
                // Keep the same base and top when converting (only once, so reruns don't compound).
                if (f.sharedMesh && f.sharedMesh.name == "FacetedCoastalStone")
                {
                    var s = f.transform.localScale;
                    f.transform.localPosition -= new Vector3(0, .46f * s.y, 0);
                    f.transform.localScale = new Vector3(s.x * 1.1f, s.y * .92f, s.z * 1.1f);
                }
                f.sharedMesh = mounds[index++ % 4];
                f.GetComponent<Renderer>().sharedMaterials = new[] { groundSand };   // one submesh, one material
                EditorUtility.SetDirty(f);
            }

            Details();
            RockyPoints(rocks);
            RoadDrifts(mounds);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Average the face normals of every vertex sharing a position (area-weighted).
        static void SmoothNormals(Mesh mesh)
        {
            if (!AssetDatabase.Contains(mesh)) return;
            var v = mesh.vertices; var t = mesh.triangles;
            var sums = new Dictionary<Vector3Int, Vector3>();
            Vector3Int Key(Vector3 p) => new Vector3Int(Mathf.RoundToInt(p.x * 1000), Mathf.RoundToInt(p.y * 1000), Mathf.RoundToInt(p.z * 1000));
            for (int i = 0; i < t.Length; i += 3)
            {
                var n = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
                for (int k = 0; k < 3; k++) { var key = Key(v[t[i + k]]); sums[key] = (sums.TryGetValue(key, out var s) ? s : Vector3.zero) + n; }
            }
            var normals = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++) normals[i] = sums[Key(v[i])].normalized;
            mesh.normals = normals;
            EditorUtility.SetDirty(mesh);
        }

        static Material Painterly(string path, Color colour, float gradient, float variation)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", colour); m.SetFloat("_GradientStrength", gradient); m.SetFloat("_GradientHeight", 2);
            m.SetFloat("_Variation", variation); m.SetFloat("_Smoothness", .1f);
            EditorUtility.SetDirty(m); return m;
        }

        static void Ground(string path, Texture2D tex, Color colour, float variation)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            m.SetTexture("_BaseMap", tex); m.SetTextureScale("_BaseMap", Vector2.one); m.SetTextureOffset("_BaseMap", Vector2.zero);
            m.SetColor("_BaseColor", colour); m.SetFloat("_WorldMapping", 1); m.SetFloat("_WorldTile", 7);
            m.SetFloat("_Variation", variation); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m);
        }

        // ------------------------------------------------------------------ painted sand texture
        static Texture2D SandTexture()
        {
            const int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
            var px = new Color[size * size];
            var rng = new System.Random(11);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    // Broad warm/cool patches.
                    float patch = Fbm(u * 3, v * 3, 3, 3, 1) ;
                    // Wind ripples: crests run along v (north-south), spaced ~35 cm on a 7 m tile.
                    float warp = Fbm(u * 4, v * 4, 4, 2, 7) * 2.4f + Fbm(u * 12, v * 12, 12, 2, 9) * .5f;
                    float phase = (u * 20 + warp) * Mathf.PI * 2;
                    float s = Mathf.Sin(phase);
                    float ripple = s > 0 ? Mathf.Pow(s, 3) : s * .35f;   // sharp sunlit crest, soft trough
                    float rippleFade = Mathf.SmoothStep(.15f, .6f, Fbm(u * 2, v * 2, 2, 2, 13));
                    float value = 1 + (patch - .5f) * .16f + ripple * .07f * rippleFade;
                    var c = Color.Lerp(new Color(.97f, .98f, 1.02f), new Color(1.02f, .99f, .94f), patch) * value;
                    // Sparse grains.
                    double g = rng.NextDouble();
                    if (g > .992) c *= .82f; else if (g < .006) c *= 1.08f;
                    c.a = 1; px[y * size + x] = c;
                }
            tex.SetPixels(px); tex.Apply();
            File.WriteAllBytes(SandTexturePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SandTexturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SandTexturePath);
            importer.wrapMode = TextureWrapMode.Repeat; importer.anisoLevel = 8; importer.mipmapEnabled = true; importer.sRGBTexture = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SandTexturePath);
        }

        // Tileable value noise (lattice wraps every `period` cells).
        static float Lattice(int x, int y, int period, int seed)
        {
            x = ((x % period) + period) % period; y = ((y % period) + period) % period;
            unchecked { int h = x * 374761393 + y * 668265263 + seed * 1442695041; h = (h ^ (h >> 13)) * 1274126177; return ((h ^ (h >> 16)) & 0xffff) / 65535f; }
        }
        static float TileNoise(float x, float y, int period, int seed)
        {
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y); float fx = x - ix, fy = y - iy;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            float a = Lattice(ix, iy, period, seed), b = Lattice(ix + 1, iy, period, seed), c = Lattice(ix, iy + 1, period, seed), d = Lattice(ix + 1, iy + 1, period, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }
        static float Fbm(float x, float y, int period, int octaves, int seed)
        {
            float v = 0, a = .5f, norm = 0;
            for (int o = 0; o < octaves; o++) { v += TileNoise(x, y, period, seed + o) * a; norm += a; x *= 2; y *= 2; period *= 2; a *= .5f; }
            return v / norm;
        }

        // ------------------------------------------------------------------ shoreline details
        static void Details()
        {
            var old = GameObject.Find("ART • beach details"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • beach details");
            var shell = Painterly(Folder + "/Shell.mat", new Color(.95f, .86f, .78f), 0, .05f);
            var shellInner = Painterly(Folder + "/Shell inner.mat", new Color(.93f, .62f, .55f), 0, .05f);
            var star = Painterly(Folder + "/Starfish.mat", new Color(.92f, .45f, .3f), 0, .08f);
            var pebbleA = Painterly(Folder + "/Pebble lilac.mat", new Color(.62f, .6f, .66f), 0, .1f);
            var pebbleB = Painterly(Folder + "/Pebble warm.mat", new Color(.78f, .68f, .56f), 0, .1f);
            var kelp = Painterly(Folder + "/Kelp.mat", new Color(.42f, .38f, .2f), 0, .1f);
            foreach (var m in new[] { shell, shellInner, star, pebbleA, pebbleB }) m.SetFloat("_Smoothness", .3f);
            var kinds = new (string mesh, Material[] mats, float scale)[]
            {
                ("scallop_shell", new[] { shell }, 2.4f), ("whelk_shell", new[] { shell, shellInner }, 2.2f),
                ("starfish", new[] { star }, 2.1f), ("pebble_cluster", new[] { pebbleA, pebbleB }, 1.8f), ("kelp_strand", new[] { kelp }, 1.5f),
            };
            var avoid = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => i.transform.position).ToList();
            var rng = new System.Random(41);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            int placed = 0;
            void Place(int kind, float x, float z)
            {
                var p = new Vector3(x, SandTop + .005f, z);
                if (avoid.Any(a => (new Vector2(a.x, a.z) - new Vector2(x, z)).sqrMagnitude < 1.4f)) return;
                if (z > 146 && x > -16) return;   // keep the studio forecourt tidy
                var (mesh, mats, scale) = kinds[kind];
                var go = new GameObject(mesh.Replace('_', ' '));
                go.transform.SetParent(root.transform);
                go.transform.position = p;
                go.transform.rotation = Quaternion.Euler(R(-6, 6), R(0, 360), R(-6, 6));
                go.transform.localScale = Vector3.one * scale * R(.8f, 1.25f);
                go.AddComponent<MeshFilter>().sharedMesh = ModelLibraryImporter.Load(mesh);
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
                placed++;
            }
            // Tide line: kelp, shells and the odd starfish along the wet edge.
            for (int i = 0; i < 170; i++)
            {
                float z = R(-34, 192), x = R(-31.9f, -29.6f);
                float pick = R(0, 1);
                Place(pick < .22f ? 4 : pick < .55f ? 0 : pick < .76f ? 1 : pick < .9f ? 3 : 2, x, z);
            }
            // Dry beach: a light scatter of shells and pebbles, mostly in small groups.
            for (int g = 0; g < 34; g++)
            {
                float cx = R(-28, 6), cz = R(-30, 142);
                int n = rng.Next(1, 4);
                for (int i = 0; i < n; i++) Place(R(0, 1) < .5f ? 3 : R(0, 1) < .6f ? 0 : 1, cx + R(-.6f, .6f), cz + R(-.6f, .6f));
            }
            Debug.Log("Beach details placed: " + placed);
        }

        // ------------------------------------------------------------------ road drifts
        // Thin, wind-blown sand tongues creep over both road edges and break the ruler-straight line.
        // Visual only: no colliders, and at most 4 cm high so the truck drives straight through.
        static void RoadDrifts(Mesh[] mounds)
        {
            var old = GameObject.Find("ART • road sand drifts"); if (old) Object.DestroyImmediate(old);
            var road = GameObject.Find("Coast road"); if (!road) return;
            var b = road.GetComponent<Renderer>().bounds;
            var root = new GameObject("ART • road sand drifts");
            var sand = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            var rng = new System.Random(23);
            float R(float a, float c) => a + (float)rng.NextDouble() * (c - a);
            for (float z = b.min.z + 4; z < b.max.z - 6; z += R(3.5f, 8f))
                foreach (int side in new[] { -1, 1 })
                {
                    if (rng.NextDouble() < .3) continue;
                    float edge = side < 0 ? b.min.x : b.max.x;
                    var go = new GameObject("Sand drift");
                    go.transform.SetParent(root.transform);
                    go.transform.position = new Vector3(edge + side * R(-.6f, .3f), b.max.y - .015f, z);
                    go.transform.rotation = Quaternion.Euler(0, R(-25, 25) + (side < 0 ? 0 : 180), 0);
                    go.transform.localScale = new Vector3(R(.9f, 1.8f), R(.025f, .045f), R(1.6f, 3.2f));
                    go.AddComponent<MeshFilter>().sharedMesh = mounds[rng.Next(0, 4)];
                    var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = sand;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
                }
        }

        // ------------------------------------------------------------------ rocky points
        static void RockyPoints(Mesh[] rocks)
        {
            var old = GameObject.Find("ART • rocky points"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • rocky points");
            var water = GameObject.Find("Open turquoise water");
            float waterY = water ? water.GetComponent<Renderer>().bounds.max.y : -.6f;
            var boulder = Painterly(Folder + "/Shore boulder.mat", new Color(.66f, .58f, .53f), .38f, .09f);
            boulder.SetFloat("_GradientHeight", 2.2f);
            var landmarks = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r => r.transform.root.name.Contains("cove landmarks")).Select(r => r.bounds.center.z).ToList();
            var rng = new System.Random(5);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            foreach (float z in new[] { 6f, 44f, 86f, 128f, 170f })
            {
                if (landmarks.Any(l => Mathf.Abs(l - z) < 9)) continue;
                int count = rng.Next(3, 6);
                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)(count - 1);
                    var s = new Vector3(R(2.2f, 3.6f), R(1.4f, 2.4f), R(2.2f, 3.4f)) * Mathf.Lerp(1.15f, .6f, t);
                    var p = new Vector3(Mathf.Lerp(-33.8f, -38.5f, t) + R(-.5f, .5f), waterY + s.y * R(.1f, .25f), z + R(-2.5f, 2.5f));
                    var go = new GameObject("Shore boulder");
                    go.transform.SetParent(root.transform);
                    go.transform.SetPositionAndRotation(p, Quaternion.Euler(R(-8, 8), R(0, 360), R(-8, 8)));
                    go.transform.localScale = s;
                    go.AddComponent<MeshFilter>().sharedMesh = rocks[rng.Next(0, 4)];
                    go.AddComponent<MeshRenderer>().sharedMaterial = boulder;
                    GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
                }
            }
        }
    }
}
