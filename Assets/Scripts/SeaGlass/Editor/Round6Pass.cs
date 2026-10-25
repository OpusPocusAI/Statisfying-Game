using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 6, from the third independent blind review (ArtReference/comparison.md). Scene-side fixes;
    // shader changes are in FrostedGlass, BasinWater and TidalWater. Run after pass 19. Safe to re-run.
    public static class Round6Pass
    {
        const string Folder = "Assets/Art/Round6";

        [MenuItem("Sea Glass/Art Direction/20 Round 6: third review fixes")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round6");
            ModelLibraryImporter.ImportAll();
            SeaGlassPiecesPass.Apply();   // domed profile

            var old = GameObject.Find("ART • round 6 dressing"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • round 6 dressing").transform;

            // A beach rock poked through the underside of each bench: move it beside the bench.
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Wind-shaped coastal stone"))
            {
                var p = r.transform.position;
                if (Mathf.Abs(p.x + 22.6f) < .5f && (Mathf.Abs(p.z - 17.4f) < 1 || Mathf.Abs(p.z - 53.1f) < 1 || Mathf.Abs(p.z - 92.4f) < 1))
                { r.transform.position = new Vector3(-24.1f, p.y, p.z + .6f); EditorUtility.SetDirty(r.transform); }
            }

            // The pale chalk slab under the floorboards showed through the plank gaps as bright seams.
            var slab = GameObject.Find("Studio raised slab");
            if (slab) { slab.GetComponent<MeshRenderer>().sharedMaterial = Paint("Under-floor shadow", new Color(.2f, .13f, .1f), 0); EditorUtility.SetDirty(slab); }

            // Road: broad patch variation and a cooler, lighter surface, so it is not one flat taupe plane.
            var road = GameObject.Find("Coast road");
            if (road)
            {
                var m = road.GetComponent<MeshRenderer>().sharedMaterial;
                if (m.HasProperty("_GroundPatches")) m.SetFloat("_GroundPatches", .35f);
                m.SetFloat("_Variation", .16f);
                m.SetTexture("_BaseMap", Asphalt()); m.SetFloat("_WorldMapping", 1); m.SetFloat("_WorldTile", 7);
                m.SetColor("_BaseColor", new Color(.66f, .62f, .62f));
                EditorUtility.SetDirty(m);
            }

            TrayPiles(root);
            FloorCrates(root);
            Lettering();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Each tray holds a small loose pile of spare pebbles of its colour (scenery, not inventory), so the
        // trays read as a working sorting bench instead of empty felt (Unpacking's filled surfaces).
        static void TrayPiles(Transform root)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            foreach (var t in Object.FindObjectsByType<SortingTray>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var defs = catalog.glass.Where(d => d.color == t.color).ToArray(); if (defs.Length == 0) continue;
                var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/GlassJourney/Glass " + t.color + ".mat")
                          ?? Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(r => r.sharedMaterial).FirstOrDefault(m => m && m.name == "Glass " + t.color);
                if (!mat) continue;
                var rng = new System.Random((int)t.color * 97 + 13);
                var top = t.GetComponent<Renderer>().bounds;
                for (int i = 0; i < 6; i++)
                {
                    var def = defs[rng.Next(defs.Length)];
                    var go = new GameObject("Spare " + t.color + " glass"); go.transform.SetParent(root);
                    // Back corner of the tray, clear of where sorted pieces are shown (StudioPresentation lays those out
                    // from the tray centre forward).
                    var p = new Vector3(top.center.x + ((float)rng.NextDouble() - .5f) * top.size.x * .55f, top.min.y + .085f + i * .012f, top.max.z - .32f - (float)rng.NextDouble() * .14f);
                    go.transform.position = p;
                    go.transform.rotation = Quaternion.Euler(90 + ((float)rng.NextDouble() - .5f) * 30, (float)rng.NextDouble() * 360, 0);
                    float size = .11f / Mathf.Max(def.VisualMesh.bounds.size.x, def.VisualMesh.bounds.size.y);
                    go.transform.localScale = Vector3.one * size;
                    go.AddComponent<MeshFilter>().sharedMesh = def.VisualMesh;
                    var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        // Fill the bare planks in front of the mosaic wall with low crates of sorted glass under the hanging
        // planters (no colliders: the socket test stands at z 173.2 and aims over them).
        static void FloorCrates(Transform root)
        {
            var crate = ModelLibraryImporter.Load("wooden_crate"); var jar = ModelLibraryImporter.Load("glass_jar");
            var src = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var crateMats = src.FirstOrDefault(f => f.sharedMesh == crate && !f.transform.IsChildOf(root))?.GetComponent<MeshRenderer>().sharedMaterials;
            var jarMats = src.FirstOrDefault(f => f.sharedMesh == jar && !f.transform.IsChildOf(root))?.GetComponent<MeshRenderer>().sharedMaterials;
            if (!crate || crateMats == null) return;
            foreach (var (x, z, yaw) in new[] { (-6.6f, 175.4f, 12f), (.4f, 175.4f, -8f), (-13.2f, 175.3f, 30f) })
            {
                var c = new GameObject("Crate of glass"); c.transform.SetParent(root);
                c.transform.SetPositionAndRotation(new Vector3(x, .12f, z), Quaternion.Euler(0, yaw, 0));
                c.AddComponent<MeshFilter>().sharedMesh = crate; c.AddComponent<MeshRenderer>().sharedMaterials = crateMats;
                if (jar && jarMats != null)
                    for (int k = 0; k < 2; k++)
                    {
                        var j = new GameObject("Jar on crate"); j.transform.SetParent(root);
                        j.transform.position = new Vector3(x - .12f + k * .24f, .12f + crate.bounds.size.y, z);
                        j.AddComponent<MeshFilter>().sharedMesh = jar; j.AddComponent<MeshRenderer>().sharedMaterials = jarMats;
                    }
            }
        }

        // Every in-world sign used Unity's default sans and read as placeholder text. Switch all lettering to the
        // bundled rounded face (Fredoka, SIL Open Font License, Assets/Art/Fonts) and the menu UI with it.
        static void Lettering()
        {
            const string fontPath = "Assets/Art/Fonts/Fredoka.ttf";
            var imp = AssetImporter.GetAtPath(fontPath) as TrueTypeFontImporter;
            if (imp && (imp.fontSize != 64 || imp.fontTextureCase != FontTextureCase.Dynamic))
            { imp.fontSize = 64; imp.fontTextureCase = FontTextureCase.Dynamic; imp.SaveAndReimport(); }
            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath); if (!font) return;
            var basis = AssetDatabase.FindAssets("World lettering t:Material").Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(m => m && m.name == "World lettering");
            string matPath = Folder + "/World lettering Fredoka.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat) { mat = basis ? new Material(basis) : new Material(Shader.Find("Sea Glass/World Lettering")); AssetDatabase.CreateAsset(mat, matPath); }
            mat.SetTexture("_MainTex", font.material.mainTexture); EditorUtility.SetDirty(mat);
            int n = 0;
            foreach (var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                t.font = font; t.fontStyle = FontStyle.Bold;   // the variable font imports at its light default weight
                var r = t.GetComponent<MeshRenderer>(); if (r) r.sharedMaterial = mat;
                EditorUtility.SetDirty(t); n++;
            }
            var ui = Object.FindFirstObjectByType<StudioUI>(FindObjectsInactive.Include);
            if (ui) { ui.uiFont = font; EditorUtility.SetDirty(ui); }
            Debug.Log("Round 6: " + n + " signs now use Fredoka");
        }

        // Sun-faded asphalt: soft repair patches, fine aggregate, a few hairline cracks and sand dust. Tiles in
        // world space (7 m), painted in values around 1 so the material colour stays in charge.
        static Texture2D Asphalt()
        {
            string path = Folder + "/Asphalt.png";
            const int S = 256; var rng = new System.Random(66);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = x / (float)S, v = y / (float)S;
                    float patch = Mathf.PerlinNoise(u * 3 + 11, v * 3 + 7) * .6f + Mathf.PerlinNoise(u * 7 + 3, v * 7 + 1) * .4f;
                    float grain = (float)rng.NextDouble();
                    float val = .9f + (patch - .5f) * .22f + (grain - .5f) * .07f;
                    // Pale wind-blown sand dust in the lower-value hollows.
                    float dust = Mathf.Clamp01((Mathf.PerlinNoise(u * 5 + 40, v * 5 + 40) - .62f) * 4);
                    var c = new Color(val, val * .985f, val * .98f);
                    px[y * S + x] = Color.Lerp(c, new Color(1.08f, 1.0f, .9f), dust * .5f);
                }
            // Hairline cracks: short wandering dark lines.
            for (int k = 0; k < 14; k++)
            {
                float x = rng.Next(S), y = rng.Next(S), a = (float)rng.NextDouble() * Mathf.PI * 2;
                int len = 30 + rng.Next(60);
                for (int i = 0; i < len; i++)
                {
                    a += ((float)rng.NextDouble() - .5f) * .6f; x += Mathf.Cos(a); y += Mathf.Sin(a);
                    int ix = (((int)x % S) + S) % S, iy = (((int)y % S) + S) % S;
                    px[iy * S + ix] *= .72f;
                }
            }
            var tex = new Texture2D(S, S, TextureFormat.RGB24, true); tex.SetPixels(px); tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG()); Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path); imp.wrapMode = TextureWrapMode.Repeat; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material Paint(string name, Color c, float smooth)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
