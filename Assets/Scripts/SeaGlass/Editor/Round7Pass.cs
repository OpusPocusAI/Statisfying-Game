using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 7, from the fourth independent blind review (ArtReference/comparison.md). Run after pass 20.
    // Shader changes this round: PainterlyForward (lit back faces), FrostedGlass (clearer rims). Safe to re-run.
    public static class Round7Pass
    {
        const string Folder = "Assets/Art/Round7";

        [MenuItem("Sea Glass/Art Direction/21 Round 7: fourth review fixes")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round7");

            // Road sand drifts read as flat, hard-edged stickers on the asphalt; the road texture carries its own
            // sand dust now, so hide them (colliders untouched).
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(r => r.name == "Sand drift"))
            { r.enabled = false; EditorUtility.SetDirty(r); }

            // Wet sand: warm and reflective (Tchia), not a pale see-through strip. Dune grass bases lifted.
            var grass = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Dune grass shadow.mat");
            if (grass) { grass.SetColor("_BaseColor", new Color(.46f, .6f, .48f)); EditorUtility.SetDirty(grass); }
            var wet = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShoreTransition/Warm tidal sand.mat");
            if (wet) { wet.SetColor("_BaseColor", new Color(.75f, .67f, .61f)); wet.SetFloat("_Smoothness", .5f); EditorUtility.SetDirty(wet); }

            Floorboards();
            GalleryRug();
            ChunkierTrayPebbles();
            VaryTrees();
            RoundTruckPosts();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Plank grain and per-board tone so the studio floor is not one flat brown plane.
        static void Floorboards()
        {
            var grain = Grain();
            var mats = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Studio floorboard").SelectMany(r => r.sharedMaterials).Where(m => m).Distinct();
            foreach (var m in mats)
            {
                m.SetTexture("_BaseMap", grain); m.SetFloat("_WorldMapping", 1); m.SetFloat("_WorldTile", 3.2f);
                m.SetFloat("_Variation", .14f); EditorUtility.SetDirty(m);
            }
        }

        static Texture2D Grain()
        {
            string path = Folder + "/Plank grain.png";
            const int S = 256; var rng = new System.Random(7);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    // Grain runs along world Z (the boards' length): long stretched noise in v, tight in u.
                    float u = x / (float)S, v = y / (float)S;
                    float g = Mathf.PerlinNoise(u * 40 + Mathf.PerlinNoise(v * 3, u * 2) * 3, v * 2.2f);
                    float knot = Mathf.Clamp01(1 - Vector2.Distance(new Vector2(u, v), new Vector2(.3f, .7f)) * 18) * .25f;
                    float val = .9f + (g - .5f) * .2f - knot + ((float)rng.NextDouble() - .5f) * .03f;
                    px[y * S + x] = new Color(val, val * .98f, val * .96f);
                }
            var tex = new Texture2D(S, S, TextureFormat.RGB24, true); tex.SetPixels(px); tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG()); Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path); imp.wrapMode = TextureWrapMode.Repeat; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // A rug in front of the mosaic wall breaks up the bare floor (no collider; sockets stay targetable).
        static void GalleryRug()
        {
            var old = GameObject.Find("Gallery front rug"); if (old) Object.DestroyImmediate(old);
            var rug = ModelLibraryImporter.Load("woven_rug");
            var src = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).FirstOrDefault(f => f.sharedMesh == rug);
            if (!rug || !src) return;
            var go = new GameObject("Gallery front rug");
            go.transform.SetPositionAndRotation(new Vector3(-3f, .14f, 173.6f), Quaternion.identity);
            go.transform.localScale = new Vector3(1.6f, 1, .7f);
            go.AddComponent<MeshFilter>().sharedMesh = rug;
            go.AddComponent<MeshRenderer>().sharedMaterials = src.GetComponent<MeshRenderer>().sharedMaterials;
        }

        // Spare tray pebbles read as flat wafers: give them pebble thickness.
        static void ChunkierTrayPebbles()
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("Spare ") && t.name.EndsWith(" glass")))
            {
                var s = t.localScale; float k = Mathf.Max(s.x, s.y); if (s.z > k * 1.5f) continue;   // already thickened
                t.localScale = new Vector3(k, k, k * 2.2f); EditorUtility.SetDirty(t);
            }
        }

        // The roadside pines repeated in identical rows. Give each its own turn, size and lean.
        static void VaryTrees()
        {
            var rng = new System.Random(71);
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name.StartsWith("coastal_pine")).OrderBy(f => f.transform.position.z))
            {
                var t = f.transform;
                float s = .78f + (float)rng.NextDouble() * .45f;
                t.rotation = Quaternion.Euler(((float)rng.NextDouble() - .5f) * 6, (float)rng.NextDouble() * 360, ((float)rng.NextDouble() - .5f) * 6);
                t.localScale = new Vector3(s * (.9f + (float)rng.NextDouble() * .2f), s, s * (.9f + (float)rng.NextDouble() * .2f));
                EditorUtility.SetDirty(t);
            }
        }

        // Square cab posts read as boxes; round tube posts like a beach buggy.
        static void RoundTruckPosts()
        {
            var cyl = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(f => f.name == "Cab pillar" || f.name == "Cab back pillar"))
            {
                if (f.sharedMesh == cyl) continue;
                f.sharedMesh = cyl;
                var s = f.transform.localScale; f.transform.localScale = new Vector3(s.x * .9f, s.y * .5f, s.z * .9f);   // cylinder mesh is 2 units tall
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.transform);
            }
        }
    }
}
