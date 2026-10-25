using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 3, from the second side-by-side (ArtReference/comparison.md). The four areas still at 3/5:
    //  - sand/terrain: offshore cove landmarks become tall layered sea stacks (LMD); far islands get
    //    cliff strata, a green cap and tree clumps instead of flat green slabs (Tchia, A Short Hike).
    //  - lighting: true darks in open-beach views from dark shrub clusters and a seaweed tide line (Firewatch).
    //  - props/vegetation: picnic groups under each parasol, so props tell little stories (A Short Hike).
    //  - studio: baskets, potted plants, crates, stools and a second rug fill the bare floor (Unpacking).
    // Models: ArtSource/blender/round3.py. Safe to re-run.
    public static class Round3Pass
    {
        const string Folder = "Assets/Art/Round3/";

        [MenuItem("Sea Glass/Art Direction/17 Round 3: landmarks, shrubs, picnic groups, studio clutter")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            if (!AssetDatabase.IsValidFolder("Assets/Art/Round3")) AssetDatabase.CreateFolder("Assets/Art", "Round3");

            Islands();
            SeaStacks();
            foreach (var guid in AssetDatabase.FindAssets("sand t:Material"))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (m && m.HasProperty("_GroundPatches") && (m.name == "Apricot sand" || m.name == "Warm tidal sand")) { m.SetFloat("_GroundPatches", .4f); EditorUtility.SetDirty(m); }
            }
            var root = GameObject.Find("ART • round 3 dressing");
            if (root) Object.DestroyImmediate(root);
            root = new GameObject("ART • round 3 dressing");
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(i => i.transform.position).ToList();
            Shrubs(root.transform, items);
            TideLine(root.transform, items);
            Scatter(root.transform, items);
            Picnics(root.transform);
            Studio(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Keep each object's footprint: scale the new mesh so its local bounds match the old ones.
        static void Swap(MeshFilter f, Mesh mesh, float heightBoost = 1)
        {
            var old = f.sharedMesh.bounds; var s = f.transform.localScale;
            var size = Vector3.Scale(old.size, s);
            float oldBase = f.transform.TransformPoint(old.center).y - size.y / 2;
            bool first = !f.sharedMesh.name.StartsWith(mesh.name.Substring(0, 6));
            f.sharedMesh = mesh;
            var nb = mesh.bounds;
            f.transform.localScale = new Vector3(size.x / nb.size.x, size.y * (first ? heightBoost : 1) / nb.size.y, size.z / nb.size.z);
            // Pivots differ (old meshes are centred, new ones stand on z=0): keep the base where it was.
            float newBase = f.transform.TransformPoint(nb.center).y - nb.size.y * f.transform.localScale.y / 2;
            f.transform.position += Vector3.up * (oldBase - newBase);
            EditorUtility.SetDirty(f.transform);
        }

        static void Islands()
        {
            var meshes = Enumerable.Range(0, 3).Select(k => ModelLibraryImporter.Load("far_island_" + k)).ToArray();
            var mats = new[] { Mat("Island cliff", new Color(.88f, .73f, .64f), .45f, 8), Mat("Island grass", new Color(.56f, .68f, .4f), .15f, 6), Mat("Island trees", new Color(.2f, .37f, .3f), .25f, 4) };
            int k = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                         .Where(f => f.name == "Wind-shaped coastal stone" && f.transform.position.x < -90).OrderBy(f => f.transform.position.z))
            {
                Swap(f, meshes[k % 3], 1.15f);
                // Tall enough to read as a layered island over the sea: base under water, ~15 m above it.
                var sc = f.transform.localScale; sc.y = (16 + k % 3 * 3) / f.sharedMesh.bounds.size.y; f.transform.localScale = sc;
                var pos = f.transform.position; pos.y = -5; f.transform.position = pos;
                f.GetComponent<MeshRenderer>().sharedMaterials = mats; k++;
            }
        }

        static void SeaStacks()
        {
            var meshes = Enumerable.Range(0, 3).Select(k => ModelLibraryImporter.Load("sea_stack_" + k)).ToArray();
            var moss = Mat("Sea stack moss", new Color(.42f, .56f, .34f), 0, 2);
            int k = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                         .Where(f => f.sharedMesh && (f.sharedMesh.name.StartsWith("Sea carved stone") || f.sharedMesh.name.StartsWith("sea_stack"))).OrderBy(f => f.transform.position.z))
            {
                var r = f.GetComponent<MeshRenderer>();
                // Middle slot of the old three-part material set is the stone colour of each cove.
                var stone = r.sharedMaterials.Length >= 3 ? r.sharedMaterials[1] : r.sharedMaterials[0];
                // Light tops with a darker wet base (LMD), never near-black.
                if (StoneColours.TryGetValue(stone.name, out var sc)) stone.SetColor("_BaseColor", sc);
                stone.SetFloat("_GradientStrength", .38f); stone.SetFloat("_GradientHeight", 4); EditorUtility.SetDirty(stone);
                Swap(f, meshes[k % 3], 1.2f);
                r.sharedMaterials = new[] { stone, moss }; k++;
            }
        }

        static readonly Dictionary<string, Color> StoneColours = new()
        {
            { "Shell cove limestone", new Color(.88f, .8f, .72f) },
            { "Tidal blue grey", new Color(.66f, .68f, .74f) },
            { "Sunset sandstone", new Color(.86f, .62f, .52f) },
        };

        // Small things across the open sand so the foreground is never an empty plane (A Short Hike).
        static void Scatter(Transform root, List<Vector3> items)
        {
            var pebbles = ModelLibraryImporter.Load("pebble_cluster");
            var shells = new[] { ModelLibraryImporter.Load("scallop_shell"), ModelLibraryImporter.Load("whelk_shell"), ModelLibraryImporter.Load("starfish") };
            var tuft = ModelLibraryImporter.Load("dune_grass_tuft");
            var stone = new[] { Mat("Scatter pebble warm", new Color(.72f, .64f, .6f), 0, 1), Mat("Scatter pebble cool", new Color(.55f, .58f, .66f), 0, 1), Mat("Scatter pebble pale", new Color(.92f, .88f, .8f), 0, 1) };
            var shellMats = new[] { Mat("Tide shell", new Color(.98f, .86f, .76f), 0, 1), Mat("Scatter shell coral", new Color(.96f, .62f, .5f), 0, 1) };
            var tuftMats = Existing("dune_grass_tuft");
            var rng = new System.Random(57);
            int n = 0;
            for (float z = -22; z < 150; z += 1.7f)
                for (float x = -29; x < 9; x += 1.7f)
                {
                    var p = new Vector3(x + (float)rng.NextDouble() * 1.7f, 0, z + (float)rng.NextDouble() * 1.7f);
                    if (p.x < CoastShape.ShoreX(p.z) + 4.5f || Near(items, p, 1.3f)) continue;
                    double roll = rng.NextDouble();
                    if (roll > .34) continue;
                    p.y = Ground(p);
                    float yaw = (float)rng.NextDouble() * 360;
                    if (roll < .14)
                        Place(root, "Sand pebbles", pebbles, Enumerable.Repeat(stone[rng.Next(0, 3)], pebbles.subMeshCount).ToArray(), p, yaw, Vector3.one * (1f + (float)rng.NextDouble() * .8f), false);
                    else if (roll < .22)
                    {
                        var m = shells[rng.Next(0, 3)];
                        Place(root, "Sand shell", m, Enumerable.Repeat(shellMats[rng.Next(0, 2)], m.subMeshCount).ToArray(), p, yaw, Vector3.one * (.8f + (float)rng.NextDouble() * .5f), false);
                    }
                    else if (tuftMats != null && p.x > -24)
                        Place(root, "Sand tuft", tuft, tuftMats, p, yaw, Vector3.one * (.8f + (float)rng.NextDouble() * .6f), false);
                    n++;
                }
            Debug.Log("Round 3: scattered " + n + " small beach details");
        }

        static void Shrubs(Transform root, List<Vector3> items)
        {
            var mesh = ModelLibraryImporter.Load("coastal_shrub");
            var mats = new[] { Mat("Shrub dark", new Color(.15f, .3f, .22f), .3f, 1.2f), Mat("Shrub lit", new Color(.38f, .54f, .27f), .2f, 1.2f) };
            var rng = new System.Random(73);
            int placed = 0;
            foreach (var g in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
                         .Where(r => r.name == "Dune grass cluster" || r.name == "Ridge grass" || r.name == "Roadside sea grass").OrderBy(r => r.transform.position.z).ToList())
            {
                var gp = g.transform.position;
                if (rng.NextDouble() > .3 || gp.x < -27 || (gp.x > 9.5f && gp.x < 20.5f) || gp.z > 150) continue;
                int n = rng.Next(1, 4);                                  // groups of one to three
                for (int i = 0; i < n; i++)
                {
                    var p = gp + new Vector3((float)rng.NextDouble() * 2.4f - 1.2f, 0, (float)rng.NextDouble() * 2.4f - 1.2f);
                    if (Near(items, p, 2.5f)) continue;
                    p.y = Ground(p);
                    float s = .7f + (float)rng.NextDouble() * .7f;
                    Place(root, "Coastal shrub", mesh, mats, p, (float)rng.NextDouble() * 360, new Vector3(s, s * (.8f + (float)rng.NextDouble() * .3f), s), true);
                    placed++;
                }
            }
            Debug.Log("Round 3: placed " + placed + " shrubs");
        }

        static void TideLine(Transform root, List<Vector3> items)
        {
            var kelp = ModelLibraryImporter.Load("kelp_strand");
            var shell = ModelLibraryImporter.Load("scallop_shell");
            var wrack = Mat("Tide wrack", new Color(.22f, .26f, .12f), 0, 1);
            var shellMat = Mat("Tide shell", new Color(.98f, .86f, .76f), 0, 1);
            var rng = new System.Random(19);
            for (float z = -22; z < 150; z += 1.6f + (float)rng.NextDouble() * 2.4f)
            {
                if (rng.NextDouble() < .25) { z += 4; continue; }          // gaps, so it reads as washed up
                float x = CoastShape.ShoreX(z) + 3.4f + (float)System.Math.Sin(z * .21f) * .8f + (float)rng.NextDouble() * .6f;
                var p = new Vector3(x, 0, z);
                if (Near(items, p, 1.6f)) continue;
                p.y = Ground(p) + .01f;
                float s = .8f + (float)rng.NextDouble() * .7f;
                Place(root, "Tide wrack", kelp, Enumerable.Repeat(wrack, kelp.subMeshCount).ToArray(), p, 60 + (float)rng.NextDouble() * 60, new Vector3(s, .5f, s), false);
                if (rng.NextDouble() < .35)
                {
                    var q = p + new Vector3((float)rng.NextDouble() - .5f, 0, (float)rng.NextDouble() - .5f);
                    q.y = Ground(q);
                    Place(root, "Tide shell", shell, Enumerable.Repeat(shellMat, shell.subMeshCount).ToArray(), q, (float)rng.NextDouble() * 360, Vector3.one * .9f, false);
                }
            }
        }

        static void Picnics(Transform root)
        {
            var towel = ModelLibraryImporter.Load("beach_towel"); var box = ModelLibraryImporter.Load("cool_box");
            var ball = ModelLibraryImporter.Load("beach_ball"); var tote = ModelLibraryImporter.Load("tote_bag");
            var towels = new[]
            {
                new[] { Mat("Towel coral", new Color(.95f, .45f, .36f), 0, 1), Mat("Towel cream", new Color(.98f, .92f, .8f), 0, 1) },
                new[] { Mat("Towel teal", new Color(.2f, .62f, .66f), 0, 1), Mat("Towel cream", new Color(.98f, .92f, .8f), 0, 1) },
                new[] { Mat("Towel sun", new Color(1f, .78f, .3f), 0, 1), Mat("Towel lilac", new Color(.66f, .55f, .82f), 0, 1) },
            };
            var boxMats = new[] { Mat("Cool box blue", new Color(.3f, .55f, .8f), 0, 1), Mat("Cool box white", new Color(.95f, .95f, .92f), 0, 1) };
            var ballMats = new[] { Mat("Ball white", new Color(.97f, .96f, .92f), 0, 1), Mat("Ball red", new Color(.92f, .3f, .27f), 0, 1), Mat("Ball yellow", new Color(1f, .8f, .2f), 0, 1), Mat("Ball blue", new Color(.24f, .5f, .86f), 0, 1) };
            var toteMats = new[] { Mat("Tote canvas", new Color(.9f, .82f, .66f), 0, 1), Mat("Tote stripe", new Color(.2f, .42f, .6f), 0, 1) };
            float[] zs = { 15, 53, 91 };
            for (int i = 0; i < zs.Length; i++)
            {
                float z = zs[i];
                Spot(root, "Beach towel", towel, towels[i], new Vector3(-24.3f, 0, z + 1.4f), 78 + i * 9, Vector3.one);
                Spot(root, "Cool box", box, boxMats, new Vector3(-22.1f, 0, z - 1.2f), 20 + i * 30, Vector3.one);
                Spot(root, "Tote bag", tote, toteMats, new Vector3(-22.6f, 0, z + 2.3f), 110 + i * 25, Vector3.one);
                if (i != 1) Spot(root, "Beach ball", ball, ballMats, new Vector3(-26f, 0, z + 3.6f), i * 40, Vector3.one);
            }
        }

        static void Spot(Transform root, string name, Mesh m, Material[] mats, Vector3 p, float yaw, Vector3 s)
        {
            p.y = Ground(p);
            Place(root, name, m, mats, p, yaw, s, true);
        }

        static void Studio(Transform root)
        {
            var basket = ModelLibraryImporter.Load("wicker_basket"); var plant = ModelLibraryImporter.Load("potted_plant");
            var crate = ModelLibraryImporter.Load("wooden_crate"); var stool = ModelLibraryImporter.Load("stool"); var rug = ModelLibraryImporter.Load("woven_rug");
            var basketMats = new[] { Mat("Wicker", new Color(.82f, .62f, .38f), .15f, .4f), Mat("Wicker dark", new Color(.55f, .36f, .2f), 0, 1), Mat("Rolled cloth", new Color(.4f, .66f, .7f), 0, 1) };
            var plantMats = new[] { Mat("Terracotta", new Color(.84f, .46f, .3f), .15f, .4f), Mat("Potting soil", new Color(.25f, .17f, .12f), 0, 1), Mat("Shrub dark", new Color(.15f, .3f, .22f), .3f, 1.2f), Mat("Shrub lit", new Color(.38f, .54f, .27f), .2f, 1.2f) };
            var crateMats = Existing("wooden_crate"); var stoolMats = Existing("stool");
            var rugMats = Existing("woven_rug");
            var rug2 = rugMats.Select((m, i) => i == 0 ? Mat("Rug coral", new Color(.86f, .45f, .36f), 0, 1) : m).ToArray();

            var plants = new[] { new Vector3(-15f, 0, 155.6f), new Vector3(7.7f, 0, 155.6f), new Vector3(-15f, 0, 176.1f), new Vector3(-12.6f, 0, 176.2f), new Vector3(7.7f, 0, 165.2f), new Vector3(-6.4f, 0, 176.1f) };
            foreach (var p in plants) Floor(root, "Potted plant", plant, plantMats, p, p.x * 37, 1f + Mathf.Abs(p.z % 3) * .12f);
            var baskets = new[] { new Vector3(-14.6f, 0, 158.2f), new Vector3(7.5f, 0, 162.3f), new Vector3(-13.4f, 0, 170.4f), new Vector3(1.2f, 0, 175.9f) };
            foreach (var p in baskets) Floor(root, "Wicker basket", basket, basketMats, p, p.z * 23, 1.1f);
            if (crate && crateMats != null)
            {
                Floor(root, "Stacked crate", crate, crateMats, new Vector3(7.3f, 0, 174.6f), 8, 1);
                var top = Floor(root, "Stacked crate", crate, crateMats, new Vector3(7.3f, 0, 174.6f), -6, 1);
                top.transform.position += Vector3.up * crate.bounds.size.y * top.transform.localScale.y * .98f;
                Floor(root, "Stacked crate", crate, crateMats, new Vector3(6.4f, 0, 175.4f), 31, .9f);
                Floor(root, "Stacked crate", crate, crateMats, new Vector3(-14.7f, 0, 164.6f), 84, 1);
            }
            if (stool && stoolMats != null)
            {
                Floor(root, "Work stool", stool, stoolMats, new Vector3(-9.6f, 0, 158.5f), 15, 1);
                Floor(root, "Work stool", stool, stoolMats, new Vector3(4.2f, 0, 171.7f), 50, 1);
            }
            if (rug && rugMats != null)
            {
                Floor(root, "Entry rug", rug, rug2, new Vector3(-4f, .028f, 156.6f), 0, .7f);
                Floor(root, "Sorting rug", rug, rug2, new Vector3(-9.5f, .027f, 166.2f), 90, .62f);
                // Runners break up the long central walkway (Unpacking never shows a bare floor run).
                Floor(root, "Walkway runner", rug, rugMats, new Vector3(-4f, .029f, 161.6f), 90, .5f);
                Floor(root, "Walkway runner", rug, rug2, new Vector3(3.6f, .029f, 167.3f), 0, .42f);
            }
            BenchTops(root, crate, crateMats, plant, plantMats);
            // A seating corner around the gallery bench: plants and baskets either side (Unpacking's lived-in rooms).
            Floor(root, "Potted plant", plant, plantMats, new Vector3(-7.2f, 0, 168.4f), 40, 1.25f);
            Floor(root, "Potted plant", plant, plantMats, new Vector3(-12.2f, 0, 155.4f), 110, 1.5f);
            Floor(root, "Potted plant", plant, plantMats, new Vector3(4.2f, 0, 155.4f), 290, 1.45f);
            Floor(root, "Wicker basket", basket, basketMats, new Vector3(-11.3f, 0, 155.6f), 20, 1f);
            Floor(root, "Potted plant", plant, plantMats, new Vector3(6.9f, 0, 165.7f), 200, 1.1f);
            Floor(root, "Wicker basket", basket, basketMats, new Vector3(-6.4f, 0, 167.2f), 70, 1f);
            Floor(root, "Wicker basket", basket, basketMats, new Vector3(1.9f, 0, 165.4f), 150, .9f);
        }

        // Bench ends, hanging planters and a crate shelf: Unpacking fills surfaces, not just floors.
        // Only the free ends of each bench are used, clear of the basin, press, trays and working glass.
        static void BenchTops(Transform root, Mesh crate, Material[] crateMats, Mesh plant, Material[] plantMats)
        {
            const float top = 1.095f;
            var jar = ModelLibraryImporter.Load("glass_jar"); var jarMats = Existing("glass_jar");
            var sprayer = ModelLibraryImporter.Load("hand_sprayer"); var sprayMats = Existing("hand_sprayer");
            var sieve = ModelLibraryImporter.Load("round_sieve"); var sieveMats = Existing("round_sieve");
            var brush = ModelLibraryImporter.Load("scrub_brush"); var brushMats = Existing("scrub_brush");
            void On(string n, Mesh m, Material[] mats, float x, float z, float yaw, float s) { if (m && mats != null) Place(root, n, m, mats, new Vector3(x, top, z), yaw, Vector3.one * s, true); }
            // Wash bench (x -2..1): left end free.
            On("Bench jar", jar, jarMats, -1.75f, 158.45f, 10, 1); On("Bench jar", jar, jarMats, -1.55f, 158.7f, 70, .85f);
            On("Bench sieve", sieve, sieveMats, -1.65f, 159.5f, 30, .9f);
            // Recycling bench (x 3.5..6.5): both ends free of the press.
            On("Bench jar", jar, jarMats, 3.8f, 158.5f, 0, .9f); On("Bench jar", jar, jarMats, 3.75f, 159.55f, 140, 1);
            On("Bench sprayer", sprayer, sprayMats, 6.2f, 159.55f, 200, 1); On("Bench brush", brush, brushMats, 6.25f, 158.55f, 75, 1);

            // Hanging planters from the ceiling boards, with a cord up to the ceiling. The back-wall pair hangs
            // in the gaps between the mosaic panels (x -6.5 and 0.5) so it never covers the glass.
            var cordMat = Mat("Planter cord", new Color(.45f, .32f, .22f), 0, 1);
            foreach (var p in new[] { new Vector3(-13.6f, 0, 158.2f), new Vector3(-13.6f, 0, 164.2f), new Vector3(-13.6f, 0, 170.2f), new Vector3(6.8f, 0, 163.2f), new Vector3(-6.5f, 0, 174.6f), new Vector3(.5f, 0, 174.6f) })
            {
                var pot = Place(root, "Hanging planter", plant, plantMats, new Vector3(p.x, 3.35f, p.z), p.z * 13, Vector3.one * .6f, true);
                var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(c.GetComponent<Collider>());
                c.name = "Planter cord"; c.transform.SetParent(pot.transform, true);
                c.transform.position = new Vector3(p.x, 4.5f, p.z); c.transform.localScale = new Vector3(.012f, .925f, .012f) / .6f;   // from the pot rim (3.57) to the ceiling (5.42); was 3.5 m long and ran through the pot
                c.GetComponent<MeshRenderer>().sharedMaterial = cordMat;
            }

            // A crate shelf against the right-hand wall, with jars and a plant on it.
            if (crate && crateMats != null)
            {
                float h = crate.bounds.size.y * .95f;
                for (int row = 0; row < 3; row++)
                    for (int i = 0; i < 2; i++)
                        Place(root, "Crate shelf", crate, crateMats, new Vector3(7.75f, .1f + row * h, 167.1f + i * (crate.bounds.size.x + .04f)), 90 + (row * 7 + i * 5) % 9 - 4, Vector3.one, true);
                float shelfTop = .1f + 3 * h;
                foreach (var (dz, s) in new[] { (-.1f, 1f), (.25f, .8f), (.95f, 1.1f) })
                    if (jar && jarMats != null) Place(root, "Shelf jar", jar, jarMats, new Vector3(7.75f, shelfTop, 167.1f + dz), dz * 90, Vector3.one * s, true);
                Place(root, "Shelf plant", plant, plantMats, new Vector3(7.75f, shelfTop, 167.1f + 1.45f), 30, Vector3.one * .55f, true);
            }
        }

        static GameObject Floor(Transform root, string name, Mesh m, Material[] mats, Vector3 p, float yaw, float s)
        {
            p.y = .1f + p.y;    // studio floor boards sit at y ~0.1
            return Place(root, name, m, mats, p, yaw, Vector3.one * s, true);
        }

        static Material[] Existing(string mesh)
        {
            var f = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(x => x.sharedMesh && x.sharedMesh.name == mesh && !x.transform.IsChildOf(GameObject.Find("ART • round 3 dressing").transform));
            return f ? f.GetComponent<MeshRenderer>().sharedMaterials : null;
        }

        static GameObject Place(Transform root, string name, Mesh m, Material[] mats, Vector3 p, float yaw, Vector3 s, bool shadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0));
            go.transform.localScale = s;
            go.AddComponent<MeshFilter>().sharedMesh = m;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
            r.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        static bool Near(List<Vector3> items, Vector3 p, float d) => items.Any(i => (new Vector2(i.x, i.z) - new Vector2(p.x, p.z)).sqrMagnitude < d * d);

        internal static float Ground(Vector3 p)
        {
            var hits = Physics.RaycastAll(new Vector3(p.x, 30, p.z), Vector3.down, 60);
            // Only visible ground counts: invisible boundary walls and the sea have colliders too (round 11 fix).
            var ok = hits.Where(h => !h.collider.isTrigger && h.collider.GetComponentInParent<ShoreItem>() == null
                && h.collider.GetComponent<Renderer>() is Renderer gr && gr.enabled && gr.sharedMaterial && !gr.sharedMaterial.name.Contains("ocean") && h.point.y < 8
                && h.point.y <= gr.bounds.max.y + .05f).OrderBy(h => h.distance).ToArray();   // box colliders taller than the visible mound do not count
            if (ok.Length > 0) return ok[0].point.y;
            // No collider: sample visible sand meshes.
            float best = -.1f;
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!r.sharedMaterial || !r.sharedMaterial.name.Contains("sand")) continue;
                var b = r.bounds;
                if (p.x < b.min.x || p.x > b.max.x || p.z < b.min.z || p.z > b.max.z || b.max.y > 2) continue;
                var col = r.GetComponent<MeshCollider>();
                bool added = false;
                if (!col) { col = r.gameObject.AddComponent<MeshCollider>(); added = true; }
                if (col.Raycast(new Ray(new Vector3(p.x, 30, p.z), Vector3.down), out var hit, 60)) best = Mathf.Max(best, hit.point.y);
                if (added) Object.DestroyImmediate(col);
            }
            return best;
        }

        static Material Mat(string name, Color c, float gradient, float height)
        {
            string path = Folder + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .1f);
            m.SetFloat("_GradientStrength", gradient); m.SetFloat("_GradientHeight", height); m.SetFloat("_Variation", .1f);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
