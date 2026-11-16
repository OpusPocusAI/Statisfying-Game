using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Ambient life pass (owner, November 16: Tchia is the art target). The coast was beautiful but empty: nothing moved
    // except water and leaves. This pass adds life that never gets in the way (no colliders, no interaction):
    // - seabirds wheeling over the coves, a pair skimming the waterline and one high over the beach,
    // - crabs on the wet sand that scuttle sideways, flee when the player comes close and dig in,
    // - butterflies over the dune flowers,
    // - schools of little fish looping in the clear shallows.
    // Models: ArtSource/blender/ambient_life.py. Behaviour: AmbientCreature. The root is rebuilt each run (repeatable).
    public static class AmbientLifePass
    {
        const string Root = "ART • ambient life", Folder = "Assets/Art/Ambient";

        [MenuItem("Sea Glass/Art Direction/Ambient life: birds, crabs, butterflies and fish")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            foreach (var name in new[] { "seabird", "beach_crab", "butterfly", "reef_fish" })
            {
                var file = Path.Combine(ModelLibraryImporter.SourceFolder, name + ".json");
                if (File.Exists(file)) ModelLibraryImporter.Import(file);
            }
            AssetDatabase.SaveAssets();
            var bird = ModelLibraryImporter.Load("seabird"); var crab = ModelLibraryImporter.Load("beach_crab");
            var fly = ModelLibraryImporter.Load("butterfly"); var fish = ModelLibraryImporter.Load("reef_fish");
            if (!bird || !crab || !fly || !fish) { Debug.LogWarning("Ambient life: run ArtSource/blender/ambient_life.py first"); return; }
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Ambient");

            // Seabird: white, the upper wing a soft blue-grey, dark tips, a warm orange beak.
            var birdMats = new[] { Paint("Seabird white", new Color(.97f, .96f, .92f), new Color(.66f, .72f, .79f, .75f), .15f),
                                   Paint("Seabird wing tip", new Color(.24f, .28f, .34f)), Paint("Seabird beak", new Color(1f, .6f, .18f)) };
            // Crab: sunny coral shell, paler legs, dark eyes; the shell's top catches a warmer orange.
            var crabMats = new[] { Paint("Crab shell", new Color(.93f, .36f, .2f), new Color(1f, .55f, .3f, .55f), .55f),
                                   Paint("Crab legs", new Color(1f, .54f, .32f)), Paint("Crab eye", new Color(.07f, .07f, .09f)) };
            // Butterflies: lemon, cream and soft tangerine wings with a dark rim, two-sided. Kept below the glass's saturation.
            var rim = Paint("Butterfly rim", new Color(.2f, .15f, .12f), cull: 0); var body = Paint("Butterfly body", new Color(.17f, .13f, .11f));
            var wings = new[] { Paint("Butterfly wing lemon", new Color(1f, .87f, .32f), cull: 0), Paint("Butterfly wing cream", new Color(.98f, .96f, .86f), cull: 0),
                                Paint("Butterfly wing tangerine", new Color(1f, .62f, .25f), cull: 0) };
            // Fish: pale silver belly under a deep blue-teal back (top tint), sunny yellow fins and tail.
            var fishMats = new[] { Paint("Fish body", new Color(.8f, .88f, .86f), new Color(.1f, .26f, .38f, 1f), 0f), Paint("Fish fin", new Color(1f, .78f, .25f), cull: 0) };

            var old = GameObject.Find(Root); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(Root).transform;
            var rng = new System.Random(1116); float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            float water = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
                .Where(r => r.sharedMaterial && r.sharedMaterial.shader && r.sharedMaterial.shader.name == "Sea Glass/Tidal Water").Select(r => r.transform.position.y).DefaultIfEmpty(-.85f).Max();

            AmbientCreature Spawn(Transform parent, string name, Mesh mesh, Material[] mats, AmbientCreature.Kind kind, Vector3 at, float yaw, float scale, bool shadows)
            {
                var go = new GameObject(name); go.transform.SetParent(parent);
                go.transform.SetPositionAndRotation(at, Quaternion.Euler(0, yaw, 0)); go.transform.localScale = Vector3.one * scale;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterials = mats;
                mr.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                var c = go.AddComponent<AmbientCreature>(); c.kind = kind; return c;
            }

            // ---- Seabirds: groups wheel over the coves; two skim the waterline; one rides high over the beach (two long loops reach in over the waterline).
            var birds = new GameObject("Seabirds").transform; birds.SetParent(root);
            var flocks = new (Vector3 centre, Vector2 r, int count, float dir)[]
            {
                (new Vector3(-53, 15, 22), new Vector2(9, 14), 3, 1), (new Vector3(-50, 21, 96), new Vector2(12, 16), 3, -1),
                (new Vector3(-58, 17, 150), new Vector2(10, 13), 2, 1), (new Vector3(-12, 24, 62), new Vector2(14, 20), 1, -1),
                (new Vector3(-45, water + 3.4f, 44), new Vector2(11, 36), 1, 1), (new Vector3(-45, water + 3.6f, 122), new Vector2(11, 36), 1, -1),
            };
            foreach (var f in flocks)
                for (int i = 0; i < f.count; i++)
                {
                    var c = Spawn(birds, "Seabird", bird, birdMats, AmbientCreature.Kind.Seabird, f.centre, 0, R(1.15f, 1.4f), true);
                    c.home = f.centre + new Vector3(R(-1.5f, 1.5f), f.centre.y < 6 ? R(0, .5f) : R(-1.5f, 2.5f), R(-1.5f, 1.5f)); c.radius = f.r * R(.85f, 1.1f);
                    c.speed = R(6f, 7.5f); c.direction = f.dir; c.phase = i * 2.1f + R(0, .8f) + f.centre.z;
                    float a = c.phase * c.direction * c.speed / ((c.radius.x + c.radius.y) * .5f);
                    c.transform.position = c.home + new Vector3(Mathf.Cos(a) * c.radius.x, 0, Mathf.Sin(a) * c.radius.y);
                }

            // ---- Crabs: on the wet-to-damp sand band, clear of props, rubbish and the studio.
            var crabs = new GameObject("Beach crabs").transform; crabs.SetParent(root);
            Physics.SyncTransforms();
            int placed = 0;
            for (int tries = 0; tries < 400 && placed < 16; tries++)
            {
                float z = R(-14, 196), x = R(-30.2f, -23.5f);
                var p = new Vector3(x, 2, z);
                if (CoastalPlayer.Indoors(p) || !Sand(p, out var hit)) continue;
                if (Physics.OverlapSphere(hit.point + Vector3.up * .3f, .9f).Any(c => !IsGround(c))) continue;
                if (crabs.Cast<Transform>().Any(t => Mathf.Abs(t.position.z - z) < 7)) continue;
                var c = Spawn(crabs, "Beach crab", crab, crabMats, AmbientCreature.Kind.Crab, hit.point, R(0, 360), R(.9f, 1.2f), true);
                c.home = hit.point; c.radius = new Vector2(R(2f, 3.2f), 0); c.minX = CoastShape.ShoreX(z) + 2.6f; c.phase = R(0, 6); placed++;
            }

            // ---- Butterflies: a few around each of the spread-out flower patches.
            var flies = new GameObject("Butterflies").transform; flies.SetParent(root);
            var flowers = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "Dune flowers" || t.name == "Flower sprig")
                .Select(t => t.position).Where(p => !CoastalPlayer.Indoors(p)).OrderBy(_ => rng.Next()).ToList();
            var anchors = new List<Vector3>();
            foreach (var p in flowers) if (anchors.Count < 10 && anchors.All(a => Vector3.Distance(a, p) > 14)) anchors.Add(p);
            int flyCount = 0;
            foreach (var a in anchors)
                for (int i = 0, n = rng.Next(1, 4); i < n; i++)
                {
                    var c = Spawn(flies, "Butterfly", fly, new[] { wings[flyCount % wings.Length], rim, body }, AmbientCreature.Kind.Butterfly,
                                  a + new Vector3(R(-1, 1), R(.4f, 1.1f), R(-1, 1)), R(0, 360), R(1.6f, 1.9f), false);
                    c.home = a; c.radius = new Vector2(R(1.4f, 2.4f), 0); c.speed = R(.9f, 1.4f); c.phase = R(0, 6); flyCount++;
                }

            // ---- Fish: schools loop the see-through shallows a few metres off the sand.
            var schools = new GameObject("Shallows fish").transform; schools.SetParent(root);
            int fishCount = 0;
            // Rocks and stacks standing in the shallows (by their bounds; most scenery seaward of the beach has no collider).
            var rocks = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
                .Where(r => !r.GetComponent<AmbientCreature>() && r.bounds.size.x < 40 && r.bounds.size.z < 40 && r.bounds.max.y > water - .6f && r.bounds.min.y < water && r.bounds.max.x < -30 && r.bounds.size.y > .4f)
                .Select(r => (b: r.bounds, name: r.name)).ToList();
            foreach (float z0 in new[] { 8f, 38f, 70f, 112f, 141f, 186f })
            {
                float phase = R(0, 50), dir = rng.Next(2) == 0 ? -1 : 1; var radius = new Vector2(R(1.8f, 2.6f), R(3.5f, 5.5f)); float speed = R(1f, 1.4f);
                Vector3 centre = default; bool ok = false; var radius2 = radius;
                for (int k = 0; k < 40 && !ok; k++)
                {
                    float z = z0 + R(-20, 20) * Mathf.Min(1, k / 4f); var rr = radius * (1 - k / 90f);
                    centre = new Vector3(CoastShape.ShoreX(z) - R(4f, 9f), water - .34f, z);
                    var swim = new Bounds(centre, new Vector3(rr.x * 2 + 2, 1, rr.y * 2 + 2)); radius2 = rr;
                    ok = !rocks.Any(r => r.b.Intersects(swim));
                    if (!ok && k == 39) Debug.LogWarning("Ambient life: shallows blocked by " + string.Join(", ", rocks.Where(r => r.b.Intersects(swim)).Select(r => r.name + " " + r.b.size.ToString("F1"))));
                }
                if (!ok) { Debug.LogWarning("Ambient life: no rock-free shallows near z " + z0); continue; }
                radius = radius2;
                for (int i = 0, n = rng.Next(6, 10); i < n; i++)
                {
                    var c = Spawn(schools, "Shallows fish", fish, fishMats, AmbientCreature.Kind.Fish, centre, 0, R(1.3f, 1.6f), true);
                    c.home = centre; c.radius = radius; c.speed = speed; c.direction = dir; c.phase = phase;
                    c.schoolOffset = new Vector3(R(-.9f, .9f), R(-.1f, .12f), R(-1.3f, 1.1f));
                    c.transform.position = centre + c.schoolOffset + new Vector3(radius.x, 0, 0); fishCount++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Ambient life: {birds.childCount} seabirds, {placed} crabs, {flyCount} butterflies, {fishCount} fish (water at {water:F2}).");
        }

        static bool IsGround(Collider c) => c.name == "Beach surface" || c.name == "Continuous sand shelf" || c.name.Contains("sand") || c.name.Contains("Sand");

        static bool Sand(Vector3 p, out RaycastHit hit)
        {
            hit = default;
            var hits = Physics.RaycastAll(p + Vector3.up * 20, Vector3.down, 40).Where(h => !h.collider.isTrigger).OrderBy(h => h.distance).ToArray();
            if (hits.Length == 0) return false; hit = hits[0];
            return IsGround(hit.collider) && hit.normal.y > .9f;
        }

        static Material Paint(string name, Color colour, Color top = default, float topStart = .5f, float cull = 2)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", colour); m.SetColor("_TopTint", top); m.SetFloat("_TopStart", topStart);
            m.SetFloat("_Cull", cull); m.doubleSidedGI = cull < .5f; m.SetFloat("_Sway", 0);
            m.SetFloat("_Brush", name.StartsWith("Butterfly") ? 0 : .15f); m.SetFloat("_BrushScale", 8);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
