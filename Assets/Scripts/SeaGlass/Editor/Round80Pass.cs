using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 80, from the seventy-sixth review (all three: the b1, b2 and s6 foregrounds are "large, low-detail cream fields",
    // "mostly empty cream planes, far sparser than the ground detail in Lonely Mountains or Tchia").
    // - Painterly Lit (PainterlyForward.hlsl), beach sand only: near-field scuffs and a fine grain stipple.
    // - This pass: small groups of beach debris across the whole open beach, not only at the grass clumps (round 10): shell-and-grit
    //   patches of small shells and pebbles (twigs and kelp strands were tried: they read as dark sticks and black squiggles). Density follows a broad noise, so some stretches are busy and
    //   others stay clear, as a real beach is after a tide. Collectibles keep a clear metre around them, so the game's pickups
    //   still read. Safe to re-run (rebuilds its own group).
    public static class Round80Pass
    {
        const string RootName = "ART • sand foreground detail";

        [MenuItem("Sea Glass/Art Direction/77 Round 80: sand foreground detail")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var old = GameObject.Find(RootName); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(RootName).transform;
            Physics.SyncTransforms();

            var shells = new[] { ModelLibraryImporter.Load("scallop_shell"), ModelLibraryImporter.Load("whelk_shell") };
            var pebbles = new[] { "pebble_cluster", "pebble_cluster_b", "pebble_cluster_c", "pebble_cluster_d", "pebble_cluster_e" }.Select(ModelLibraryImporter.Load).Where(m => m).ToArray();
            var shellMats = new[] { Mat("Assets/Art/Round3/Tide shell.mat"), Mat("Assets/Art/Round3/Scatter shell coral.mat") };
            var pebbleMats = new[] { Mat("Assets/Art/Round3/Scatter pebble warm.mat"), Mat("Assets/Art/Round3/Scatter pebble pale.mat") };
            if (shells.Any(s => !s) || pebbles.Length == 0 || shellMats.Any(m => !m) || pebbleMats.Any(m => !m))
                throw new System.InvalidOperationException("Round 80: scatter meshes or materials missing.");

            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => new Vector2(i.transform.position.x, i.transform.position.z)).ToList();
            var rng = new System.Random(8080);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            int groups = 0, pieces = 0;
            const float cell = 1.7f;
            for (float z = -26; z < 148; z += cell)
                for (float x = -27; x < 7.5f; x += cell)
                {
                    var c = new Vector2(x + R(0, cell), z + R(0, cell));
                    float dens = Mathf.PerlinNoise(c.x * .09f + 40.3f, c.y * .09f + 11.7f);
                    if (rng.NextDouble() > Mathf.Clamp01((dens - .22f) * .75f)) continue;   // 0 on clear stretches, up to ~0.5 on busy ones
                    if (items.Any(p => (p - c).sqrMagnitude < 1.4f * 1.4f)) continue;
                    if (!Sand(c, out _)) continue;
                    // Shell-and-grit patch: a few small shells and pebble groups in a loose clump.
                    int n = 2 + rng.Next(4);
                    for (int k = 0; k < n; k++)
                    {
                        var q = c + new Vector2(R(-.45f, .45f), R(-.45f, .45f));
                        if (!Sand(q, out var h)) continue;
                        bool sh = rng.NextDouble() < .45;
                        var m = sh ? shells[rng.Next(2)] : pebbles[rng.Next(pebbles.Length)];
                        var mat = sh ? shellMats[rng.Next(2)] : pebbleMats[rng.Next(2)];
                        Place(root, "Sand grit", m, mat, h, R(0, 360), sh ? R(.45f, .85f) : R(.4f, .75f)); pieces++;
                    }
                    groups++;
                }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 80: " + groups + " debris groups, " + pieces + " pieces");
        }

        static Material Mat(string path) => AssetDatabase.LoadAssetAtPath<Material>(path);

        // Open beach sand only: the top visible surface must be sand (not the wet band, a deck, a rock or the road), nearly level.
        static bool Sand(Vector2 q, out Vector3 point)
        {
            point = Vector3.zero;
            var hits = Physics.RaycastAll(new Vector3(q.x, 30, q.y), Vector3.down, 60, ~0, QueryTriggerInteraction.Ignore)
                .Where(h => h.collider.GetComponentInParent<ShoreItem>() == null && h.collider.GetComponent<Renderer>() is Renderer r && r.enabled && h.point.y < 8)
                .OrderBy(h => h.distance).ToArray();
            if (hits.Length == 0) return false;
            var top = hits[0]; var mat = top.collider.GetComponent<Renderer>().sharedMaterial;
            if (!mat || (mat.name != "Apricot sand" && mat.name != "Dry dune sand") || top.normal.y < .94f || top.point.y > 1.5f) return false;
            point = top.point; return true;
        }

        static void Place(Transform root, string name, Mesh mesh, Material mat, Vector3 p, float yaw, float scale) =>
            Place(root, name, mesh, Enumerable.Repeat(mat, mesh.subMeshCount).ToArray(), p, yaw, Vector3.one * scale);

        static void Place(Transform root, string name, Mesh mesh, Material[] mats, Vector3 p, float yaw, Vector3 scale)
        {
            var go = new GameObject(name); go.transform.SetParent(root);
            go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0));
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
        }
    }
}
