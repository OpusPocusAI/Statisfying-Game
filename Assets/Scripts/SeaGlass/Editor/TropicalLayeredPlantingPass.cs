using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Denser, layered plants pass (owner, November 15: Tchia is the art target). Stylised tropical coasts read in layers: tall
    // palms leaning over the sand, a mid layer of big-leaved plants under and between the shrubs, ferns and grass at the
    // ground. The coast had one even layer of shrubs and a single line of short palms along the foot of the ridge. This pass:
    // - plants palm groves (2-4 trees, leaning seaward) along the back of the beach and up the green ridge,
    // - fills the ridge's shrub drifts and its roadside edge with broadleaf clumps and ferns (ArtSource/blender/tropical_plants.py),
    // - makes palms and pines sway along their whole height (the painterly shader's _SwayHeight), with the trunk, fronds and nuts
    //   on one shared bend so nothing separates, and gives the olive and straw grass tips the same sway as their bases.
    // The planted roots are rebuilt from scratch each run, so the pass is repeatable.
    public static class TropicalLayeredPlantingPass
    {
        const string GroveRoot = "ART • palm groves", UnderRoot = "ART • layered planting";
        const string Folder = "Assets/Art/Vegetation";

        static readonly (string path, float sway, float height)[] Swaying =
        {
            ("Assets/Art/Round10/Palm bark.mat", .018f, 11f), ("Assets/Art/Round10/Palm ring.mat", .018f, 11f),
            ("Assets/Art/Round10/Palm frond.mat", .018f, 11f), ("Assets/Art/Round10/Coconut.mat", .018f, 11f),
            ("Assets/Art/Vegetation/Pine bark.mat", .01f, 9f), ("Assets/Art/Vegetation/Pine foliage.mat", .01f, 9f),
            // The olive and straw tufts' tips stayed still while their bases swayed (the foliage pass covered only the green ones).
            ("Assets/Art/Round21/Grass tip olive.mat", .07f, 0f), ("Assets/Art/Round21/Grass base straw.mat", .07f, 0f), ("Assets/Art/Round21/Grass tip straw.mat", .07f, 0f),
        };

        [MenuItem("Sea Glass/Art Direction/Plants and foliage: layered planting and palm groves")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);

            var frond = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round10/Palm frond.mat");
            // Fronds: deep green underneath, sunny yellow-green where the leaflets face the sky.
            frond.SetColor("_BaseColor", new Color(.2f, .52f, .22f)); frond.SetColor("_TopTint", new Color(.62f, .8f, .3f, .75f)); frond.SetFloat("_TopStart", .2f); EditorUtility.SetDirty(frond);
            var broadDeep = Leaf("Broadleaf deep", frond, new Color(.1f, .42f, .21f), .045f);
            var broadLit = Leaf("Broadleaf lit", frond, new Color(.36f, .64f, .2f), .045f);
            var fernDark = Leaf("Fern green", frond, new Color(.17f, .5f, .22f), .06f);
            var fernLit = Leaf("Fern lit", frond, new Color(.48f, .72f, .26f), .06f);
            var palmMats = new[] { "Palm bark", "Palm ring", "Palm frond", "Coconut" }.Select(n => AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round10/" + n + ".mat")).ToArray();

            var palms = Enumerable.Range(0, 3).Select(k => ModelLibraryImporter.Load("palm_" + k)).ToArray();
            var broad = Enumerable.Range(0, 2).Select(k => ModelLibraryImporter.Load("broadleaf_" + k)).ToArray();
            var ferns = Enumerable.Range(0, 2).Select(k => ModelLibraryImporter.Load("fern_" + k)).ToArray();
            if (palms.Concat(broad).Concat(ferns).Any(m => !m)) { Debug.LogWarning("Layered planting: import the tropical_plants.py models first"); return; }

            foreach (var n in new[] { GroveRoot, UnderRoot }) { var old = GameObject.Find(n); if (old) Object.DestroyImmediate(old); }
            var ridge = GameObject.Find("Inland dune ridge");
            var ridgeCollider = ridge ? ridge.GetComponent<MeshCollider>() : null; bool temp = ridge && !ridgeCollider;
            if (temp) { ridgeCollider = ridge.AddComponent<MeshCollider>(); ridgeCollider.sharedMesh = ridge.GetComponent<MeshFilter>().sharedMesh; }
            Physics.SyncTransforms();

            var groves = new GameObject(GroveRoot).transform; var under = new GameObject(UnderRoot).transform;
            var rng = new System.Random(1116); float R() => (float)rng.NextDouble();
            var taken = new List<Vector3>(Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Where(f => f.sharedMesh && (f.sharedMesh.name.StartsWith("palm_") || f.sharedMesh.name.StartsWith("coastal_pine"))).Select(f => f.transform.position));

            void Plant(Transform root, string name, Mesh mesh, Material[] mats, Vector3 at, float yaw, float scale, bool collide)
            {
                var go = new GameObject(name); go.transform.SetParent(root);
                go.transform.SetPositionAndRotation(at, Quaternion.Euler(0, yaw, 0)); go.transform.localScale = Vector3.one * scale;
                go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterials = mats;
                if (collide) { var c = go.AddComponent<CapsuleCollider>(); c.radius = .28f; c.height = 3f; c.center = new Vector3(0, 1.5f, 0); }
            }
            bool Ground(Vector3 p, out RaycastHit hit, bool ridgeOnly)
            {
                hit = default;
                if (ridgeOnly) return ridgeCollider && ridgeCollider.Raycast(new Ray(p + Vector3.up * 30, Vector3.down), out hit, 60) && hit.normal.y > .8f;
                var hits = Physics.RaycastAll(p + Vector3.up * 30, Vector3.down, 60).OrderBy(h => h.distance).ToArray();
                if (hits.Length == 0) return false; hit = hits[0];
                return hit.collider.name == "Beach surface" || hit.collider.name == "Continuous sand shelf";
            }
            // Keep clear of gameplay: anything with a collider near the trunk (props, rubbish, benches, signs) rules a spot out.
            bool Free(Vector3 p, float r) => !Physics.OverlapSphere(p + Vector3.up * .9f, r).Any(c => c.name != "Beach surface" && c.name != "Continuous sand shelf" && c.name != "Inland dune ridge" && c.name != "Grove palm");
            bool Clear(Vector3 p, float r) => Free(p, r)
                                               && !taken.Any(t => Vector2.Distance(new Vector2(t.x, t.z), new Vector2(p.x, p.z)) < 3.2f);
            bool NearStudio(Vector3 p) => p.z > 140 && p.z < 186 && p.x < 12;

            // Palm groves: 2-4 trees per grove, leaning out toward the sea (the models lean along +X, the sea is -X).
            int palmCount = 0;
            var grovePlaces = new List<(Vector3 c, bool ridgeSide)>();
            for (float z = -14; z < 196; z += 15f + R() * 9f) grovePlaces.Add((new Vector3(1.5f + R() * 6f, 0, z), false));        // back of the beach, seaward of the road
            for (float z = -30; z < 205; z += 19f + R() * 12f) grovePlaces.Add((new Vector3(33f + R() * 14f, 0, z), true));        // up the green ridge
            foreach (var (c, ridgeSide) in grovePlaces)
            {
                int n = 2 + rng.Next(3);
                for (int i = 0; i < n; i++)
                {
                    var p = c + new Vector3((R() - .5f) * 5f, 0, (R() - .5f) * 6f);
                    if (NearStudio(p) || !Ground(p, out var hit, ridgeSide) || !Clear(hit.point, 1.4f)) continue;
                    float yaw = 180 + (R() - .5f) * 90f;
                    Plant(groves, "Grove palm", palms[rng.Next(3)], palmMats, hit.point - Vector3.up * .08f, yaw, .82f + R() * .32f, !ridgeSide);
                    taken.Add(hit.point); palmCount++;
                    // the grove floor: a fern or broadleaf at the foot of most palms
                    for (int j = 0, m = 1 + rng.Next(3); j < m; j++)
                    {
                        float a = R() * Mathf.PI * 2, r = 1.1f + R() * 1.6f;
                        var q = hit.point + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
                        if (!Ground(q, out var h2, ridgeSide) || !Free(h2.point, .6f)) continue;
                        bool isFern = R() < .55f;
                        Plant(under, isFern ? "Grove fern" : "Grove broadleaf", isFern ? ferns[rng.Next(2)] : broad[rng.Next(2)],
                              isFern ? new[] { fernDark, fernLit } : new[] { broadDeep, broadLit }, h2.point - Vector3.up * .03f, R() * 360, isFern ? .8f + R() * .5f : .75f + R() * .45f, false);
                    }
                }
            }

            // Ridge understory: broadleaf and ferns in the same drifts as the shrubs (TropicalFoliagePass uses this noise field),
            // and a denser band along the ridge foot so the roadside edge reads as fern, then leaf, then shrub.
            int underCount = 0;
            for (int i = 0; i < 6000 && underCount < 520 && ridgeCollider; i++)
            {
                bool edge = i % 3 == 0;
                var p = edge ? new Vector3(21.8f + R() * 5.5f, 0, -40 + R() * 245) : new Vector3(21.5f + R() * 30f, 0, -14 + R() * 190);
                float drift = Mathf.PerlinNoise(p.x * .06f + 3.1f, p.z * .06f + 7.7f);
                if (!edge && (drift < .36f || R() > (drift - .36f) * 2.4f)) continue;
                if (!Ground(p, out var hit, true)) continue;
                float edgeDepth = Mathf.InverseLerp(21.8f, 27.3f, hit.point.x);
                bool isFern = edge ? R() > edgeDepth * .8f : R() < .45f;
                Plant(under, isFern ? "Understory fern" : "Understory broadleaf", isFern ? ferns[rng.Next(2)] : broad[rng.Next(2)],
                      isFern ? new[] { fernDark, fernLit } : new[] { broadDeep, broadLit }, hit.point - Vector3.up * .03f, R() * 360,
                      isFern ? .9f + R() * .6f : .85f + R() * .7f * (edge ? edgeDepth + .3f : drift + .3f), false);
                underCount++;
            }
            if (temp) Object.DestroyImmediate(ridgeCollider);

            Sway();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Layered planting: {palmCount} grove palms, {underCount} understory plants on the ridge, {under.childCount} plants in all under the palms and ridge.");
        }

        static Material Leaf(string name, Material template, Color colour, float sway)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(template); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_TopTint", new Color(0, 0, 0, 0));
            m.SetColor("_BaseColor", colour); m.SetFloat("_Sway", sway); m.SetFloat("_SwayHeight", 0);
            EditorUtility.SetDirty(m); return m;
        }

        // Palms and pines sway along their whole height: every part of the tree shares one bend, so fronds stay on the trunk.
        // Swaying renderers leave static batching (it would bake them into world space and lose the origin they bend from).
        static void Sway()
        {
            var mats = new HashSet<Material>();
            foreach (var (path, sway, height) in Swaying)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path); if (!m) { Debug.LogWarning("Layered planting: missing " + path); continue; }
                m.SetFloat("_Sway", sway); m.SetFloat("_SwayHeight", height); EditorUtility.SetDirty(m); mats.Add(m);
            }
            foreach (var n in new[] { "Broadleaf deep", "Broadleaf lit", "Fern green", "Fern lit" }) { var m = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/" + n + ".mat"); if (m) mats.Add(m); }
            int n2 = 0;
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!r.sharedMaterials.Any(mats.Contains)) continue;
                var flags = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
                if ((flags & StaticEditorFlags.BatchingStatic) != 0) { GameObjectUtility.SetStaticEditorFlags(r.gameObject, flags & ~StaticEditorFlags.BatchingStatic); n2++; }
            }
            Debug.Log("Layered planting: " + n2 + " swaying renderers taken out of static batching.");
        }
    }
}
