using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 5 (Documentation/ART_DIRECTION.md, sections 5.10 and 5.12): modelled workbenches,
    // a designed toy spade and round sieve, and storytelling props that fill the studio's empty floor.
    // Models: ArtSource/blender/studio_furniture.py.
    public static class StudioFurniturePass
    {
        const string Folder = "Assets/Art/StudioFurniture";

        [MenuItem("Sea Glass/Art Direction/5 Studio furniture, props and tools")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "StudioFurniture");
            ModelLibraryImporter.ImportAll();

            var top = Mat(Folder + "/Workbench top.mat", new Color(.72f, .46f, .27f), .2f);
            var frame = Mat(Folder + "/Workbench frame.mat", new Color(.13f, .36f, .37f), .18f);
            var brass = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CoastalPolish/Workbench brass.mat");
            Workbenches(top, frame, brass);
            Tools();
            Props(top, frame, brass);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Material Mat(string path, Color colour, float smoothness, float gradient = .15f)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", colour); m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_GradientStrength", gradient); m.SetFloat("_GradientHeight", 1.2f); m.SetFloat("_Variation", .07f);
            EditorUtility.SetDirty(m); return m;
        }

        static void Workbenches(Material top, Material frame, Material brass)
        {
            foreach (var station in Object.FindObjectsByType<StudioStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var tabletop = station.transform.Find("Timber tabletop");
                if (!tabletop) continue;
                bool wide = tabletop.GetComponent<Renderer>().bounds.size.x > 4;
                CleaningStationPass.Replace(tabletop.gameObject, ModelLibraryImporter.Load(wide ? "workbench_6m" : "workbench_3m"), new[] { top, frame, brass });
                // The bench legs keep their colliders; the modelled bench draws the legs.
                foreach (Transform child in station.transform)
                    if (child.name == "Bench leg") child.GetComponent<Renderer>().enabled = false;
            }
        }

        static void Tools()
        {
            var coral = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/GatheringPolish/Toy coral.mat");
            var handle = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/GatheringPolish/Honey handle.mat");
            var teal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/GatheringPolish/Sieve teal.mat");
            var wire = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/GatheringPolish/Warm enamel.mat");
            coral.SetColor("_BaseColor", new Color(.95f, .38f, .22f)); coral.SetFloat("_Smoothness", .45f); EditorUtility.SetDirty(coral);

            var tool = GameObject.Find("ART • gathering tool")?.transform;
            if (tool)
            {
                for (int i = tool.childCount - 1; i >= 0; i--) Object.DestroyImmediate(tool.GetChild(i).gameObject);
                var spade = new GameObject("Toy beach spade");
                spade.transform.SetParent(tool, false);
                spade.AddComponent<MeshFilter>().sharedMesh = ModelLibraryImporter.Load("toy_spade");
                var r = spade.AddComponent<MeshRenderer>(); r.sharedMaterials = new[] { coral, handle };
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var basket = GameObject.Find("ART • sieve basket")?.transform;
            if (basket)
            {
                for (int i = basket.childCount - 1; i >= 0; i--) Object.DestroyImmediate(basket.GetChild(i).gameObject);
                var sieve = new GameObject("Round beach sieve");
                sieve.transform.SetParent(basket, false);
                sieve.transform.localPosition = new Vector3(0, 0, 0);
                sieve.AddComponent<MeshFilter>().sharedMesh = ModelLibraryImporter.Load("round_sieve");
                var r = sieve.AddComponent<MeshRenderer>(); r.sharedMaterials = new[] { teal, wire };
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        static GameObject Place(Transform root, string mesh, Material[] mats, Vector3 position, float yaw, float scale = 1, bool collider = false, bool shadows = true)
        {
            var go = new GameObject(mesh.Replace('_', ' '));
            go.transform.SetParent(root);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            go.transform.localScale = Vector3.one * scale;
            go.AddComponent<MeshFilter>().sharedMesh = ModelLibraryImporter.Load(mesh);
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
            if (!shadows) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (collider) go.AddComponent<BoxCollider>();
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        static void Props(Material top, Material frame, Material brass)
        {
            var old = GameObject.Find("ART • studio storytelling"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • studio storytelling").transform;
            const float floor = .119f;   // top of the studio floorboards

            // Jars: glass shell (the sea-glass shader), coloured tumbled glass inside, cork lid.
            var jarGlass = new Material(Shader.Find("Sea Glass/Frosted Glass")) { name = "Jar glass" };
            jarGlass.SetColor("_BaseColor", new Color(.82f, .93f, .9f)); jarGlass.SetFloat("_Grime", 0); jarGlass.SetFloat("_Smoothness", .9f); jarGlass.SetFloat("_Refraction", .03f);
            jarGlass = Save(jarGlass, Folder + "/Jar glass.mat");
            var cork = Mat(Folder + "/Cork.mat", new Color(.7f, .54f, .36f), .05f, 0);
            var contents = Enumerable.Range(0, 6).Select(i => Mat(Folder + "/Jar contents " + (GlassColor)i + ".mat", Color.Lerp(GameCatalog.Palette[i], Color.white, .1f), .5f, 0)).ToArray();
            Material[] Jar(int c) => new[] { jarGlass, contents[c], cork };
            float[] shelfZ = { 162, 168, 174 };
            int k = 0;
            foreach (float z in shelfZ)
                for (int j = 0; j < 3; j++)
                    Place(root, "glass_jar", Jar(k++ % 6), new Vector3(-15.45f, 1.45f, z - .75f + j * .26f + (j > 1 ? 1.2f : 0)), k * 37, 1.1f);
            // A few jars on the sorting bench's back corners and the recycling bench.
            Place(root, "glass_jar", Jar(0), new Vector3(-11.75f, 1.09f, 160.6f), 10, 1.15f);
            Place(root, "glass_jar", Jar(3), new Vector3(-6.25f, 1.09f, 160.65f), 70, 1.0f);
            Place(root, "glass_jar", Jar(2), new Vector3(-5.95f, 1.09f, 160.35f), 120, .85f);

            // Crates of collected rubbish beside the recycling press.
            var crateWood = Mat(Folder + "/Crate pine.mat", new Color(.78f, .6f, .4f), .08f);
            var crateDark = Mat(Folder + "/Crate post.mat", new Color(.5f, .35f, .23f), .08f);
            Place(root, "wooden_crate", new[] { crateWood, crateDark }, new Vector3(7.3f, floor, 160.1f), 8, 1, true);
            Place(root, "wooden_crate", new[] { crateWood, crateDark }, new Vector3(7.25f, floor + .345f, 160.05f), -6, 1, false);
            Place(root, "wooden_crate", new[] { crateWood, crateDark }, new Vector3(7.4f, floor, 161.0f), 82, 1, true);

            // A stool at the wash bench and one by the sorting trays.
            var seat = Mat(Folder + "/Stool seat.mat", new Color(.86f, .42f, .27f), .25f);
            Place(root, "stool", new[] { seat, top }, new Vector3(1.55f, floor, 157.7f), 20, 1, true);
            Place(root, "stool", new[] { seat, top }, new Vector3(-12.6f, floor, 158.1f), 70, 1, true);

            // A striped woven rug and a gallery bench in front of the triptych.
            var rugBase = Mat(Folder + "/Rug cream.mat", new Color(.9f, .82f, .66f), .02f, 0);
            var rugStripe = Mat(Folder + "/Rug teal.mat", new Color(.2f, .5f, .5f), .02f, 0);
            var rugEdge = Mat(Folder + "/Rug coral.mat", new Color(.86f, .45f, .32f), .02f, 0);
            foreach (var m in new[] { rugBase, rugStripe, rugEdge }) { m.SetFloat("_Variation", .14f); m.SetFloat("_Rim", .1f); }
            Place(root, "woven_rug", new[] { rugBase, rugStripe, rugEdge }, new Vector3(-3f, floor, 167.5f), 0, 1, false, false);
            Place(root, "gallery_bench", new[] { top, frame }, new Vector3(-3f, floor, 167.6f), 0, 1, true);

            // The designer's easel beside the gallery with paint swatches in the triptych colours.
            var paper = Mat(Folder + "/Sketch paper.mat", new Color(.96f, .93f, .84f), .02f, 0);
            Place(root, "design_easel", new[] { top, paper, contents[0], contents[2], contents[4], contents[3] }, new Vector3(-13.6f, floor, 172.2f), 35, 1, true);
        }

        static Material Save(Material m, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing) { existing.shader = m.shader; existing.CopyPropertiesFromMaterial(m); EditorUtility.SetDirty(existing); return existing; }
            AssetDatabase.CreateAsset(m, path); return m;
        }
    }
}
