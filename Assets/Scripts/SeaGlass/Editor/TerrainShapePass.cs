using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 12 (Documentation/ART_DIRECTION.md, section 5.3):
    // - the row of separate blocky rocks behind the road becomes one continuous, sculpted dune ridge
    //   with wind-cut crests, grassy tops and a few rock outcrops (scenery: beyond the X = 34 boundary);
    // - the open beach gets broad, low, walkable swells that keep clear of every collectible.
    public static class TerrainShapePass
    {
        const string Folder = "Assets/Art/Terrain";

        [MenuItem("Sea Glass/Art Direction/12 Dune ridge and beach swells")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Terrain");
            ModelLibraryImporter.ImportAll();
            var sand = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            Ridge(sand);
            Swells(sand);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static float Noise(float x, float z, int seed) => Mathf.PerlinNoise(x * .045f + seed * 13.1f, z * .045f + seed * 7.3f);

        // Height of the inland ridge. Flat near the road and trees (x < 27), rising to rolling dunes.
        public static float RidgeHeight(float x, float z)
        {
            float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(27f, 40f, x));
            float big = Noise(x * .7f, z, 1) * 7f + Noise(x * 1.8f, z * 1.8f, 2) * 2.5f;
            // Wind-cut crests: sharpen the noise into ridges running along the coast.
            float crest = 1 - Mathf.Abs(Noise(x * 2.2f, z * .9f, 3) * 2 - 1);
            float h = 1.2f + big + crest * crest * 2.2f;
            return Mathf.Lerp(-.1f, h, rise);
        }

        static void Ridge(Material sand)
        {
            var old = GameObject.Find("ART • inland dune ridge"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • inland dune ridge");

            // Hide the old inland rock row (colliders stay; they sit beyond the play boundary).
            var rocks = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(r => r.name == "Wind-shaped coastal stone" && r.transform.position.x > 28 && r.transform.position.x < 40).ToList();
            foreach (var r in rocks) r.enabled = false;

            const float x0 = 22f, x1 = 72f, z0 = -45f, z1 = 210f, step = 1f;
            int nx = Mathf.RoundToInt((x1 - x0) / step) + 1, nz = Mathf.RoundToInt((z1 - z0) / step) + 1;
            var v = new Vector3[nx * nz];
            for (int j = 0; j < nz; j++)
                for (int i = 0; i < nx; i++)
                {
                    float x = x0 + i * step, z = z0 + j * step;
                    v[j * nx + i] = new Vector3(x, RidgeHeight(x, z), z);
                }
            var t = new List<int>();
            for (int j = 0; j < nz - 1; j++)
                for (int i = 0; i < nx - 1; i++)
                {
                    int a = j * nx + i, b = a + 1, c = a + nx, d = c + 1;
                    t.AddRange(new[] { a, c, b, b, c, d });
                }
            var mesh = Save(Folder + "/Inland dune ridge.asset", v, t);
            var go = new GameObject("Inland dune ridge");
            go.transform.SetParent(root.transform);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = sand;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);

            // Grass along the crests and a few weathered outcrops breaking through the sand.
            var tuft = ModelLibraryImporter.Load("dune_grass_tuft");
            var grassMats = new[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Dune grass shadow.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Dune grass sage.mat") };
            var rock = new[] { "faceted_rock_0", "faceted_rock_1", "faceted_rock_2", "faceted_rock_3" }.Select(ModelLibraryImporter.Load).ToArray();
            var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Coastal stone.mat");
            var rng = new System.Random(19);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            for (int k = 0; k < 420; k++)
            {
                float x = R(29, 70), z = R(z0 + 2, z1 - 2), h = RidgeHeight(x, z);
                // Grass prefers higher ground: skip most low spots.
                if (h < 2.5f && rng.NextDouble() < .7) continue;
                var g = new GameObject("Ridge grass");
                g.transform.SetParent(root.transform);
                g.transform.SetPositionAndRotation(new Vector3(x, h - .05f, z), Quaternion.Euler(0, R(0, 360), 0));
                g.transform.localScale = Vector3.one * R(1.4f, 2.4f);
                g.AddComponent<MeshFilter>().sharedMesh = tuft;
                var gr = g.AddComponent<MeshRenderer>(); gr.sharedMaterials = grassMats; gr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.BatchingStatic);
            }
            foreach (var r in rocks.Where((_, i) => i % 3 == 0))
            {
                var p = r.transform.position; float x = p.x + R(4, 14);
                var o = new GameObject("Ridge outcrop");
                o.transform.SetParent(root.transform);
                o.transform.SetPositionAndRotation(new Vector3(x, RidgeHeight(x, p.z) - .6f, p.z), Quaternion.Euler(R(-10, 10), R(0, 360), R(-10, 10)));
                o.transform.localScale = new Vector3(R(3, 5), R(1.6f, 2.6f), R(3, 5));
                o.AddComponent<MeshFilter>().sharedMesh = rock[rng.Next(0, 4)];
                o.AddComponent<MeshRenderer>().sharedMaterial = stone;
                GameObjectUtility.SetStaticEditorFlags(o, StaticEditorFlags.BatchingStatic);
            }
        }

        static void Swells(Material sand)
        {
            var old = GameObject.Find("ART • beach swells"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • beach swells");
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => new Vector2(i.transform.position.x, i.transform.position.z)).ToList();
            var avoid = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .Where(r => r.name == "Resting bench" || r.name == "Round parasol pole" || r.name == "Beach section sign" || r.name == "Sign post")
                .Select(r => new Vector2(r.bounds.center.x, r.bounds.center.z)).ToList();
            var rng = new System.Random(8);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            int placed = 0;
            for (int k = 0; k < 3000 && placed < 48; k++)
            {
                var c = new Vector2(R(-26, 7), R(-30, 146));
                float rx = R(3.2f, 8), rz = R(4.5f, 12);
                // Keep every collectible (and its dig mound) on flat sand, and keep clear of props.
                if (items.Any(p => Ellipse(p - c, rx + 1.2f, rz + 1.2f) < 1)) continue;
                if (avoid.Any(p => Ellipse(p - c, rx + 1f, rz + 1f) < 1)) continue;
                var go = new GameObject("Beach swell");
                go.transform.SetParent(root.transform);
                go.transform.SetPositionAndRotation(new Vector3(c.x, -.1f, c.y), Quaternion.Euler(0, R(-20, 20), 0));
                go.AddComponent<MeshFilter>().sharedMesh = SwellMesh();
                go.transform.localScale = new Vector3(rx, Mathf.Min(R(.75f, 1.1f), rx * .15f), rz);
                go.AddComponent<MeshRenderer>().sharedMaterial = sand;
                // Walkable: the slope stays far below the controller's 48° limit.
                go.AddComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
                placed++;
            }
            Debug.Log("Terrain: " + placed + " beach swells placed.");
        }

        static float Ellipse(Vector2 d, float rx, float rz) => d.x * d.x / (rx * rx) + d.y * d.y / (rz * rz);

        // Unit swell: radius 1, height 1, smooth cosine profile that meets the ground tangentially.
        static Mesh SwellMesh()
        {
            const string path = Folder + "/Beach swell.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing && existing.vertexCount > 0) return existing;
            int rings = 12, segs = 40;
            var v = new List<Vector3> { new Vector3(0, 1, 0) };
            for (int r = 1; r <= rings; r++)
                for (int s = 0; s < segs; s++)
                {
                    float rad = r / (float)rings, a = s * Mathf.PI * 2 / segs;
                    v.Add(new Vector3(Mathf.Cos(a) * rad, .5f + .5f * Mathf.Cos(rad * Mathf.PI), Mathf.Sin(a) * rad));
                }
            var t = new List<int>();
            for (int s = 0; s < segs; s++) t.AddRange(new[] { 0, 1 + (s + 1) % segs, 1 + s });
            for (int r = 0; r < rings - 1; r++)
                for (int s = 0; s < segs; s++)
                {
                    int a = 1 + r * segs + s, b = 1 + r * segs + (s + 1) % segs, c = a + segs, d = b + segs;
                    t.AddRange(new[] { a, b, c, b, d, c });
                }
            return Save(path, v.ToArray(), t);
        }

        static Mesh Save(string path, Vector3[] v, List<int> t)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear();
            mesh.indexFormat = v.Length > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.vertices = v; mesh.SetTriangles(t, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
            EditorUtility.SetDirty(mesh); return mesh;
        }
    }
}
