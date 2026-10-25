using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 5, from an independent blind review of the whole game against the reference games
    // (ArtReference/comparison.md). Collects the scene-side fixes of that round; shader and
    // runtime changes are recorded in Documentation/IMPLEMENTATION.md. Safe to re-run.
    public static class Round5Pass
    {
        const string Folder = "Assets/Art/Round5";

        [MenuItem("Sea Glass/Art Direction/19 Round 5: independent review fixes")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round5");

            TrayLinings();
            SinkShelves();
            Signs();
            TruckLettering();
            RoadDrifts();
            Spade();
            RecyclingPress();
            VaryMounds();
            SeatBoulders();
            BeachBenches();
            StudioWarmth();
            // Tumbled glass meshes are rebuilt from the updated profile (chunkier, irregular rim).
            SeaGlassPiecesPass.Apply();
            // Rock foam was baked from the pre-round-3 rock shapes; rebake it from the sea stacks.
            RockSurfPolish.Apply();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Felt linings with a clearly distinct tint per colour (Aqua and Emerald were both grey-teal and
        // read as flat paint). World-mapped fibre texture so the lining reads as fabric.
        static readonly Color[] Linings =
        {
            new Color(.55f, .9f, .9f),    // Aqua: light cyan
            new Color(.5f, .78f, .5f),    // Emerald: leaf green
            new Color(.5f, .58f, .92f),   // Cobalt: periwinkle
            new Color(.98f, .76f, .48f),  // Amber: apricot
            new Color(.95f, .94f, .88f),  // Pearl: oyster white
            new Color(.76f, .6f, .9f),    // Violet: lilac
        };

        static void TrayLinings()
        {
            var felt = FeltTexture();
            foreach (var t in Object.FindObjectsByType<SortingTray>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var r = t.GetComponent<MeshRenderer>(); if (!r || r.sharedMaterials.Length < 2) continue;
                var m = r.sharedMaterials[1];
                m.SetColor("_BaseColor", Linings[(int)t.color]);
                m.SetTexture("_BaseMap", felt);
                m.SetFloat("_WorldMapping", 1); m.SetFloat("_WorldTile", .35f);
                m.SetFloat("_Smoothness", .02f); m.SetFloat("_Variation", .06f);
                EditorUtility.SetDirty(m);
            }
        }

        // The ocean shader foams where the water is very shallow. A flat slab top lying just under the surface
        // therefore became a wide white plate with straight edges. Lower each layered rock by the smallest
        // amount that leaves no upward-facing top between 0.4 m below and 0.03 m above the surface, so the
        // waterline crosses a steep face and the foam becomes a thin contact line.
        static void SinkShelves()
        {
            var water = GameObject.Find("Open turquoise water");
            float level = water.GetComponent<Renderer>().bounds.max.y;
            int moved = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var m = f.sharedMesh; if (!m || !(m.name.StartsWith("sea_stack") || m.name.StartsWith("rock_stack"))) continue;
                var b = f.GetComponent<Renderer>().bounds; if (b.min.y > level || b.max.y < level) continue;
                var vs = m.vertices; var ns = m.normals;
                var tops = new System.Collections.Generic.List<float>();
                for (int i = 0; i < vs.Length; i++)
                    if (f.transform.TransformDirection(ns[i]).y > .85f) tops.Add(f.transform.TransformPoint(vs[i]).y);
                float best = 0;
                for (float dy = 0; dy >= -1.2f; dy -= .02f)
                    if (!tops.Any(y => y + dy > level - .4f && y + dy < level + .03f)) { best = dy; break; }
                if (best < 0) { f.transform.position += Vector3.up * best; EditorUtility.SetDirty(f.transform); moved++; }
            }
            Debug.Log("Round 5: lowered " + moved + " layered rocks off the shallow-foam band");
        }

        // Lettering that ran off its board: the fascia tagline hung below the fascia (and showed mirrored from
        // inside), "FEED THE FUTURE" was wider than the press, and the press hid half of the RECYCLING sign.
        static void Signs()
        {
            var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Transform T(string name, string parent = null) => all.FirstOrDefault(t => t.name == name && (parent == null || (t.parent && t.parent.name == parent)));
            void Set(Transform t, Vector3? pos, float? scale) { if (!t) return; if (pos.HasValue) t.position = pos.Value; if (scale.HasValue) t.localScale = Vector3.one * scale.Value; EditorUtility.SetDirty(t); }
            Set(T("SEA GLASS  /  STUDIO"), new Vector3(-4, 4.82f, 154.27f), .72f);
            Set(T("a little care. a little colour."), new Vector3(-4, 4.11f, 154.25f), .7f);
            Set(T("FEED THE FUTURE", "RECYCLING"), null, .62f);
            foreach (var n in new[] { "RECYCLING sign", "RECYCLING" })
            {
                var t = T(n, "RECYCLING"); if (!t) continue;
                var p = t.position; p.y = 2.05f; t.position = p; EditorUtility.SetDirty(t);
            }
        }

        // The cargo words sat on plain white plates and read as debug labels. Paint them straight onto the
        // tubs as ivory stencil lettering instead; the skip-drive plate stays (it is an interaction cue).
        static void TruckLettering()
        {
            var body = GameObject.Find("Sprung body"); if (!body) return;
            foreach (var t in body.GetComponentsInChildren<Transform>(true).ToArray())
            {
                if (t.name == "Cargo enamel label") { Object.DestroyImmediate(t.gameObject); continue; }
                if (t.name != "GLASS" && t.name != "RECYCLE") continue;
                var text = t.GetComponent<TextMesh>(); if (!text) continue;
                text.color = new Color(.98f, .94f, .84f); text.fontStyle = FontStyle.Bold;
                EditorUtility.SetDirty(text);
            }
        }

        // Sand drifts read as large cream blobs with hard edges on the road. Smaller, thinner spills at the
        // road edge (guarded so a re-run does not shrink them again).
        static void RoadDrifts()
        {
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Sand drift"))
            {
                if (r.bounds.size.y < .026f) continue;   // originals are 0.03–0.05 m tall
                var s = r.transform.localScale; r.transform.localScale = new Vector3(s.x * .7f, s.y * .5f, s.z * .7f);
                EditorUtility.SetDirty(r.transform);
            }
        }

        // The toy spade filled ~15% of the screen. Smaller and a little lower, at the screen edge.
        static void Spade()
        {
            var player = Object.FindFirstObjectByType<CoastalPlayer>(); if (!player || !player.tool) return;
            player.tool.localScale = Vector3.one * .8f;
            player.tool.localPosition = new Vector3(.37f, -.32f, .7f);
            EditorUtility.SetDirty(player.tool);
        }

        // The recycling station's press was a plain red cube. Swap in the modelled press (true size, so the
        // transform scale becomes 1 and the box collider keeps the same world size). RecyclingMotion moves
        // it by localPosition only.
        static void RecyclingPress()
        {
            ModelLibraryImporter.ImportAll();
            var mesh = ModelLibraryImporter.Load("recycling_press"); if (!mesh) return;
            var press = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Recycling press");
            if (!press) return;
            var size = Vector3.Scale(press.GetComponent<MeshFilter>().sharedMesh == mesh ? Vector3.one : press.GetComponent<MeshFilter>().sharedMesh.bounds.size, press.localScale);
            if (press.GetComponent<MeshFilter>().sharedMesh != mesh)
            {
                press.GetComponent<MeshFilter>().sharedMesh = mesh;
                press.localScale = Vector3.one;
                var box = press.GetComponent<BoxCollider>(); if (box) { box.center = Vector3.zero; box.size = size; }
            }
            var mats = new[] { Paint("Press coral", new Color(.86f, .36f, .26f), .25f), Paint("Press teal", new Color(.13f, .44f, .46f), .3f),
                               Paint("Press charcoal", new Color(.16f, .15f, .18f), .2f), Paint("Press cream", new Color(.96f, .91f, .8f), .1f),
                               Paint("Press brass", new Color(.85f, .66f, .3f), .55f) };
            press.GetComponent<MeshRenderer>().sharedMaterials = mats;
            EditorUtility.SetDirty(press.gameObject);
        }

        // Dig mounds read as identical cones. Vary each one's footprint, height and turn from a fixed seed per
        // item id (re-running sets the same values; it does not compound). ShoreItem reads the scale at runtime.
        static void VaryMounds()
        {
            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!item.mound) continue;
                var rng = new System.Random(item.id.GetHashCode() & 0x7fffffff ^ 0x5a17);
                var mf = item.mound.GetComponent<MeshFilter>();
                if (mf) mf.sharedMesh = ModelLibraryImporter.Load("sand_mound_" + rng.Next(0, 4));
                float w = .4f + (float)rng.NextDouble() * .18f, d = .3f + (float)rng.NextDouble() * .16f, h = .17f + (float)rng.NextDouble() * .1f;
                item.mound.localScale = new Vector3(w, h, d);
                item.mound.localRotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0);
                EditorUtility.SetDirty(item.mound);
            }
        }

        // The fractured rock stacks have tilted, uneven bottoms, so some headland boulders hovered over the sand.
        // Bury each one's lowest point 0.35 m below the sand/shoulder surface under its centre.
        static void SeatBoulders()
        {
            int n = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.name == "Shore boulder"))
            {
                var b = f.GetComponent<Renderer>().bounds;
                float ground = Mathf.Min(-.1f, CoastShape.ShoulderY(b.center.x, b.center.z));
                float target = ground - .35f;
                if (Mathf.Abs(b.min.y - target) < .01f) continue;
                f.transform.position += Vector3.up * (target - b.min.y); EditorUtility.SetDirty(f.transform); n++;
            }
            Debug.Log("Round 5: seated " + n + " headland boulders");
        }

        // The beach benches were three round logs on trestles. Hide those parts and set the modelled slatted
        // bench (the studio's gallery bench) in their place, with a matching collider. Run pass 17 afterwards
        // so the picnic items re-settle on the new surfaces.
        static void BeachBenches()
        {
            var model = ModelLibraryImporter.Load("gallery_bench");
            var gallery = GameObject.Find("gallery bench"); if (!model || !gallery) return;
            var mats = gallery.GetComponent<MeshRenderer>().sharedMaterials;
            var old = GameObject.Find("ART • beach benches"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • beach benches").transform;
            var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var z in new[] { 17f, 55f, 93f })
            {
                float ground = float.MaxValue;
                foreach (var t in all)
                {
                    if (Mathf.Abs(t.position.x + 22) > 1.3f || Mathf.Abs(t.position.z - z) > .5f) continue;
                    if (t.name != "Rounded bench slat" && t.name != "Bench trestle" && t.name != "Bench footing" && t.name != "Resting bench") continue;
                    var r = t.GetComponent<Renderer>(); if (r) { if (t.name == "Bench trestle") ground = Mathf.Min(ground, r.bounds.min.y); r.enabled = false; EditorUtility.SetDirty(r); }
                    foreach (var c in t.GetComponents<Collider>()) { c.enabled = false; EditorUtility.SetDirty(c); }
                }
                if (ground == float.MaxValue) ground = -.1f;
                var go = new GameObject("Slatted beach bench"); go.transform.SetParent(root);
                go.transform.SetPositionAndRotation(new Vector3(-22, ground, z), Quaternion.identity);
                go.transform.localScale = new Vector3(1.15f, 1.05f, 1.15f);
                go.AddComponent<MeshFilter>().sharedMesh = model; go.AddComponent<MeshRenderer>().sharedMaterials = mats;
                var box = go.AddComponent<BoxCollider>(); box.center = model.bounds.center; box.size = model.bounds.size;
            }
        }

        // Before restoration the studio's pendants are deliberately off (the restored room lights up as a reward;
        // a test guards this). The room still needs warm light: soft window light along the ocean wall and a
        // low lamp over the work island, separate from the restoration lamps.
        static void StudioWarmth()
        {
            var old = GameObject.Find("ART • studio warmth"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • studio warmth").transform;
            void L(string n, Vector3 p, float intensity, float range, Color c)
            {
                var go = new GameObject(n); go.transform.SetParent(root); go.transform.position = p;
                var l = go.AddComponent<Light>(); l.type = LightType.Point; l.intensity = intensity; l.range = range; l.color = c; l.shadows = LightShadows.None;
            }
            var window = new Color(1f, .78f, .55f);
            foreach (var z in new[] { 158.5f, 164.5f, 170.5f }) L("Window warmth", new Vector3(-13.2f, 2.6f, z), 7f, 9f, window);
            foreach (var z in new[] { 159.5f, 169.5f }) L("Hall warmth", new Vector3(-1.5f, 3.6f, z), 7f, 11f, new Color(1f, .8f, .6f));
            L("Work island lamp glow", new Vector3(-4.85f, 1.65f, 163.4f), 1.4f, 3.5f, new Color(1f, .74f, .45f));
            L("Design table lamp glow", new Vector3(-10.3f, 1.65f, 165.6f), .8f, 3f, new Color(1f, .74f, .45f));
        }

        static Material Paint(string name, Color c, float smooth)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_GradientStrength", .12f); m.SetFloat("_GradientHeight", 1.6f);
            EditorUtility.SetDirty(m); return m;
        }

        static Texture2D FeltTexture()
        {
            string path = Folder + "/Felt.png";
            var rng = new System.Random(5);
            const int S = 128;
            var tex = new Texture2D(S, S, TextureFormat.RGB24, false);
            var px = new Color[S * S];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white * .93f;
            // Short fibres in random directions, darker and lighter than the base: reads as felt at arm's length.
            for (int f = 0; f < 2600; f++)
            {
                float x = (float)rng.NextDouble() * S, y = (float)rng.NextDouble() * S, a = (float)rng.NextDouble() * Mathf.PI;
                float v = (float)rng.NextDouble() < .5 ? .8f : 1f; int len = 4 + rng.Next(6);
                for (int k = 0; k < len; k++)
                {
                    int ix = ((int)(x + Mathf.Cos(a) * k) % S + S) % S, iy = ((int)(y + Mathf.Sin(a) * k) % S + S) % S;
                    px[iy * S + ix] = Color.Lerp(px[iy * S + ix], Color.white * v, .55f);
                }
            }
            tex.SetPixels(px); tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG()); Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.wrapMode = TextureWrapMode.Repeat; imp.sRGBTexture = true; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
