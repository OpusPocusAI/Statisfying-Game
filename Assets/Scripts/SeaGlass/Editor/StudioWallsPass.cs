using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 2 (from the side-by-side with Tiny Glade and Unpacking, ArtReference/comparison.md): studio walls
    // get real material texture (teal shiplap, lime plaster, roof tiles) and the bare walls get coastal
    // workshop clutter: fishing nets with floats, life rings, coiled rope, paintings and a chalkboard.
    // Models: ArtSource/blender/wall_clutter.py.
    public static class StudioWallsPass
    {
        const string Folder = "Assets/Art/StudioWalls";

        [MenuItem("Sea Glass/Art Direction/15 Studio walls and clutter")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "StudioWalls");
            ModelLibraryImporter.ImportAll();

            var shiplap = WallMat("Teal shiplap", Tex("Shiplap", Shiplap), new Color(.2f, .42f, .44f), 2f);
            var plaster = WallMat("Lime plaster", Tex("Plaster", Plaster), new Color(.97f, .9f, .78f), 3f);
            var tiles = WallMat("Coral roof tiles", Tex("Roof tiles", Tiles), new Color(.93f, .44f, .3f), 2.4f);
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (r.name == "Ocean wall pier" || r.name == "Ocean wall lintel" || r.name == "Ocean wall sill") r.sharedMaterial = shiplap;
                else if (r.name == "Studio back wall" || r.name == "Studio fascia") r.sharedMaterial = plaster;
                else if (r.name == "Studio canopy") r.sharedMaterial = tiles;
                else continue;
                EditorUtility.SetDirty(r);
            }
            Clutter();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Material Mat(string name, Color c, float smooth = .15f)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_GradientStrength", 0); m.SetFloat("_Variation", .04f);
            EditorUtility.SetDirty(m); return m;
        }
        static Material WallMat(string name, Texture2D tex, Color c, float tile)
        {
            var m = Mat(name, c); m.SetTexture("_BaseMap", tex); m.SetFloat("_WorldMapping", 2); m.SetFloat("_WorldTile", tile); m.SetFloat("_Variation", .05f);
            EditorUtility.SetDirty(m); return m;
        }

        // ------------------------------------------------------------------ painted textures
        delegate Color Painter(float u, float v, System.Random rng);
        static Texture2D Tex(string name, Painter paint, int w = 512, int h = 512, bool repeat = true)
        {
            string path = Folder + "/" + name + ".png";
            var t = new Texture2D(w, h, TextureFormat.RGB24, false);
            var rng = new System.Random(name.GetHashCode());
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = paint((x + .5f) / w, (y + .5f) / h, rng);
            t.SetPixels(px); t.Apply();
            File.WriteAllBytes(path, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp; imp.anisoLevel = 8; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static float N(float x, float y, int s) => Mathf.PerlinNoise(x + s * 17.3f, y + s * 9.1f);
        // Tileable noise: blend periodic samples.
        static float TN(float u, float v, float scale, int s)
        {
            float a = N(u * scale, v * scale, s), b = N((u - 1) * scale, v * scale, s), c = N(u * scale, (v - 1) * scale, s), d = N((u - 1) * scale, (v - 1) * scale, s);
            return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
        }

        static Color Shiplap(float u, float v, System.Random rng)
        {
            const int boards = 10; float bv = v * boards; int board = Mathf.FloorToInt(bv); float f = bv - board;
            float tone = .88f + ((board * 37) % 7) / 7f * .1f;
            float grain = TN(u * 8, v * .5f, 6, board) * .08f;
            float groove = f < .07f ? .55f + f * 5 : f > .93f ? 1.06f : 1;      // shadowed groove, lit lip
            float v2 = tone * groove + grain;
            return new Color(v2, v2, v2 * .98f);
        }
        static Color Plaster(float u, float v, System.Random rng)
        {
            float m = TN(u, v, 5, 1) * .07f + TN(u, v, 13, 2) * .04f;
            float trowel = Mathf.Sin((u * 3 + v * 2 + TN(u, v, 4, 3) * 2) * Mathf.PI * 2) * .015f;
            float val = .93f + m + trowel;
            return new Color(val, val * .985f, val * .96f);
        }
        static Color Tiles(float u, float v, System.Random rng)
        {
            const int rows = 8, cols = 8; float rv = v * rows; int row = Mathf.FloorToInt(rv); float fy = rv - row;
            float cu = u * cols + (row % 2) * .5f; int col = Mathf.FloorToInt(cu); float fx = cu - col;
            float round = Mathf.Sin(fx * Mathf.PI);                      // barrel tile
            float lap = Mathf.SmoothStep(0, 1, fy / .25f);               // shadow under the row above
            float tone = .82f + ((row * 13 + col * 7) % 5) / 5f * .12f;
            float val = tone * (.72f + .28f * round) * (.7f + .3f * lap);
            return new Color(val, val * .97f, val * .95f);
        }
        static Color Seascape(float u, float v, System.Random rng)
        {
            Color c;
            if (v > .45f) c = Color.Lerp(new Color(1f, .78f, .55f), new Color(.5f, .6f, .9f), Mathf.SmoothStep(.45f, 1f, v));
            else c = Color.Lerp(new Color(.08f, .45f, .6f), new Color(.2f, .75f, .78f), v / .45f);
            float sun = Vector2.Distance(new Vector2(u, v * .75f), new Vector2(.68f, .55f * .75f));
            if (sun < .09f) c = new Color(1f, .93f, .7f);
            if (v < .45f && Mathf.Abs(u - .68f) < .05f + (.45f - v) * .2f && Mathf.Sin(v * 90) > .3f) c = Color.Lerp(c, new Color(1f, .85f, .6f), .6f);
            // a little sailboat
            if (u > .25f && u < .36f && v > .45f && v < .45f + (u - .25f) * 1.6f) c = new Color(.97f, .95f, .9f);
            if (u > .22f && u < .39f && v > .41f && v < .45f) c = new Color(.6f, .3f, .2f);
            return c * (.94f + TN(u, v, 20, 4) * .1f);
        }
        static Color Swatches(float u, float v, System.Random rng)
        {
            var c = new Color(.95f, .91f, .82f);
            for (int i = 0; i < 6; i++)
            {
                var p = new Vector2(.2f + (i % 3) * .3f, .32f + (i / 3) * .38f);
                float d = Vector2.Distance(new Vector2(u, v * .75f), new Vector2(p.x, p.y * .75f));
                float r = .1f + TN(u, v, 9, i) * .03f;
                if (d < r) c = Color.Lerp(GameCatalog.Palette[i], Color.white, .15f + (d / r) * .25f);
                else if (d < r + .012f) c = c * .85f;
            }
            return c;
        }
        static Color CoastMap(float u, float v, System.Random rng)
        {
            float shore = .45f + Mathf.Sin(v * 9) * .07f + Mathf.Sin(v * 23) * .02f;
            var c = u < shore ? new Color(.55f, .78f, .8f) : new Color(.94f, .86f, .68f);
            if (Mathf.Abs(u - shore) < .008f) c = new Color(.35f, .3f, .3f);
            float path = .7f + Mathf.Sin(v * 6) * .04f;
            if (Mathf.Abs(u - path) < .006f && Mathf.Repeat(v * 40, 1) < .5f) c = new Color(.8f, .3f, .2f);
            foreach (var y in new[] { .2f, .5f, .8f }) if (Vector2.Distance(new Vector2(u, v), new Vector2(path, y)) < .025f) c = new Color(.85f, .3f, .22f);
            return c * (.92f + TN(u, v, 6, 7) * .12f);
        }
        static Color Chalk(float u, float v, System.Random rng)
        {
            var c = new Color(.14f, .22f, .2f) * (.9f + TN(u, v, 10, 8) * .2f);
            bool line = false;
            for (int i = 0; i < 3; i++)                                      // the triptych plan: three panel boxes
            {
                float x0 = .1f + i * .29f, x1 = x0 + .23f;
                if (v > .45f && v < .85f && (Mathf.Abs(u - x0) < .006f || Mathf.Abs(u - x1) < .006f) || u > x0 && u < x1 && (Mathf.Abs(v - .45f) < .008f || Mathf.Abs(v - .85f) < .008f)) line = true;
                for (int t = 0; t < 10; t++) if (Mathf.Abs(u - (x0 + .02f + t * .021f)) < .004f && v > .22f && v < .34f) line = true;   // tally marks
            }
            if (line) c = Color.Lerp(c, new Color(.92f, .92f, .86f), .85f);
            return c;
        }

        // ------------------------------------------------------------------ clutter
        static void Clutter()
        {
            var old = GameObject.Find("ART • studio wall clutter"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • studio wall clutter").transform;
            var walnut = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CoastalPolish/Gallery walnut.mat");
            var oak = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/StudioAtmosphere/Shelf oak.mat");
            var white = Mat("Ring white", new Color(.97f, .95f, .9f), .35f);
            var red = Mat("Ring red", new Color(.9f, .3f, .22f), .35f);
            var rope = Mat("Rope", new Color(.84f, .72f, .52f), .05f);
            var net = Mat("Net twine", new Color(.42f, .36f, .3f), .05f);
            var cork = Mat("Cork float", new Color(.78f, .6f, .38f), .05f);
            var floatGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/StudioFurniture/Jar contents Aqua.mat");

            Material Picture(string n, Painter p) { var m = Mat(n, Color.white, .05f); m.SetTexture("_BaseMap", Tex(n, p, 256, 192, false)); m.SetFloat("_Variation", 0); EditorUtility.SetDirty(m); return m; }
            var sea = Picture("Painting seascape", Seascape);
            var sw = Picture("Painting glass swatches", Swatches);
            var map = Picture("Painting coast map", CoastMap);
            var chalk = Picture("Chalkboard plan", Chalk);
            var frameMesh = WithPlanarUV(ModelLibraryImporter.Load("picture_frame"));
            var boardMesh = WithPlanarUV(ModelLibraryImporter.Load("chalkboard"));

            GameObject Put(string name, Mesh mesh, Material[] mats, Vector3 at, float yaw, float scale = 1, bool shadows = true)
            {
                var go = new GameObject(name); go.transform.SetParent(root);
                go.transform.SetPositionAndRotation(at, Quaternion.Euler(0, yaw, 0)); go.transform.localScale = Vector3.one * scale;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
                if (!shadows) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic); return go;
            }
            // Back wall (faces -Z into the room), left of the triptych and to its right.
            Put("Painting", frameMesh, new[] { walnut, sea }, new Vector3(-14.6f, 3.9f, 176.74f), 0, 1.1f);
            Put("Painting", frameMesh, new[] { oak, map }, new Vector3(-14.6f, 2.45f, 176.74f), 0, .9f);
            Put("Chalkboard", boardMesh, new[] { oak, chalk }, new Vector3(7.55f, 2.9f, 176.74f), 0, 1f);
            // West (ocean) wall piers, facing +X.
            Put("Painting", frameMesh, new[] { walnut, sw }, new Vector3(-15.78f, 3.0f, 162f), -90, .85f);
            Put("Painting", frameMesh, new[] { oak, sea }, new Vector3(-15.78f, 3.0f, 168.5f), -90, .85f);
            // Nets with floats hung along the lintel above the windows.
            var netMesh = ModelLibraryImporter.Load("fishing_net");
            Put("Fishing net", netMesh, new[] { net, floatGlass, cork }, new Vector3(-15.75f, 4.85f, 157.2f), -90, 1, false);
            Put("Fishing net", netMesh, new[] { net, floatGlass, cork }, new Vector3(-15.75f, 4.85f, 170.2f), -90, 1, false);
            // Life rings on the front canopy posts, facing out to the beach.
            var ringMesh = ModelLibraryImporter.Load("life_ring");
            Put("Life ring", ringMesh, new[] { white, red, rope }, new Vector3(-16f, 2.7f, 155.78f), 0);
            Put("Life ring", ringMesh, new[] { white, red, rope }, new Vector3(8f, 2.7f, 155.78f), 0);
            // Coiled rope by the recycling crates and on the boardwalk.
            var coil = ModelLibraryImporter.Load("coiled_rope");
            Put("Coiled rope", coil, new[] { rope }, new Vector3(6.9f, .12f, 163.2f), 30);
            Put("Coiled rope", coil, new[] { rope }, new Vector3(-19.2f, .2f, 163f), 80);
        }

        // Planar UVs from local X/Z (Unity X/Y after import) normalised to the mesh bounds.
        static Mesh WithPlanarUV(Mesh mesh)
        {
            var b = mesh.bounds;
            mesh.uv = mesh.vertices.Select(p => new Vector2((p.x - b.min.x) / b.size.x, (p.y - b.min.y) / b.size.y)).ToArray();
            EditorUtility.SetDirty(mesh); return mesh;
        }
    }
}
