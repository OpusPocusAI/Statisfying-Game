using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Studio interior round 4, from the side-by-side with Unpacking (ArtReference/comparison.md):
    // the hall read as a long bare walkway. A central work island and an open shelving divider break
    // it into zones, and every free surface carries clutter. Models: ArtSource/blender/studio_island.py.
    //
    // Collision: the player capsule has a 1 m radius, so every gap kept for walking is at least 2.2 m:
    // around the island (x -5.2..-2.8, z 162.65..163.75), between the island and the gallery bench,
    // and the corridor x -0.9..1.4 that leads past the shelf to the equipment bench. The mosaic band
    // (z > 171.5) and the verification camera at (-3, 165) stay clear. Clutter has no colliders, so it
    // never blocks the interaction reticle. Safe to re-run.
    public static class StudioLayoutPass
    {
        const string Folder = "Assets/Art/StudioLayout/";
        const float FloorY = .12f;

        [MenuItem("Sea Glass/Art Direction/18 Studio layout: work island, shelving, surface clutter")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            if (!AssetDatabase.IsValidFolder("Assets/Art/StudioLayout")) AssetDatabase.CreateFolder("Assets/Art", "StudioLayout");
            var old = GameObject.Find("ART • studio layout"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • studio layout").transform;

            var island = ModelLibraryImporter.Load("work_island");
            var shelf = ModelLibraryImporter.Load("open_shelf");
            var goods = ModelLibraryImporter.Load("shelf_goods");
            var table = ModelLibraryImporter.Load("table_clutter");
            var bench = ModelLibraryImporter.Load("bench_clutter");

            // Central work island across the walkway, with its desk set on top.
            var isl = Place(root, "Work island", island, new Vector3(-4f, FloorY, 163.2f), 0, true);
            Place(root, "Work island clutter", table, new Vector3(-4f, FloorY + .92f, 163.2f), 0, false);
            // A second table on the sorting rug, turned lengthwise (x -10.55..-9.45, z 165.3..167.7):
            // >4 m to the ocean wall, the first island and the sorting bench, clear of the mosaic band.
            Place(root, "Design table", island, new Vector3(-10f, FloorY, 166.5f), 90, true);
            Place(root, "Design table clutter", table, new Vector3(-10f, FloorY + .92f, 166.5f), 270, false);

            // Open shelving divider: two 2 m units between the recycling front and the equipment corner.
            for (int i = 0; i < 2; i++)
            {
                var p = new Vector3(3.4f + i * 2.02f, FloorY, 164.6f);
                Place(root, "Open shelf", shelf, p, 0, true);
                Place(root, "Shelf goods", goods, p, i * 180, false);    // second unit turned round, so it isn't a copy
            }

            // Bench tops: the equipment bench was bare (wash and recycling ends were dressed in round 3).
            const float top = 1.095f;
            Place(root, "Bench clutter", bench, new Vector3(3.95f, top, 169.55f), 20, false);
            Place(root, "Bench clutter", bench, new Vector3(6.05f, top, 169.5f), 200, false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Studio layout: island bounds " + isl.GetComponent<Collider>().bounds);
        }

        static readonly Dictionary<string, Color> Colours = new()
        {
            { "wood_top", new Color(.8f, .58f, .36f) }, { "wood_frame", new Color(.52f, .33f, .2f) }, { "drawer_paint", new Color(.2f, .52f, .54f) },
            { "brass", new Color(.88f, .7f, .32f) }, { "crate_wood", new Color(.74f, .56f, .36f) }, { "shelf_wood", new Color(.82f, .62f, .4f) },
            { "shelf_paint", new Color(.93f, .87f, .76f) }, { "book_a", new Color(.78f, .3f, .25f) }, { "book_b", new Color(.22f, .42f, .58f) },
            { "book_c", new Color(.9f, .72f, .3f) }, { "book_d", new Color(.36f, .56f, .42f) }, { "jar_glass", new Color(.78f, .9f, .88f) },
            { "cork", new Color(.72f, .56f, .38f) }, { "glass_aqua", new Color(.3f, .82f, .8f) }, { "glass_amber", new Color(.92f, .6f, .2f) },
            { "glass_green", new Color(.36f, .72f, .4f) }, { "glass_cobalt", new Color(.22f, .38f, .82f) }, { "pottery_cream", new Color(.95f, .9f, .8f) },
            { "pottery_teal", new Color(.26f, .56f, .58f) }, { "basket", new Color(.8f, .62f, .38f) }, { "cutting_mat", new Color(.26f, .5f, .4f) },
            { "paper", new Color(.97f, .95f, .9f) }, { "paper_sketch", new Color(.92f, .88f, .8f) }, { "lamp", new Color(.2f, .42f, .46f) },
        };

        // Submesh order follows the export lists in studio_island.py.
        static readonly Dictionary<string, string[]> Slots = new()
        {
            { "work_island", new[] { "wood_top", "wood_frame", "drawer_paint", "brass", "crate_wood" } },
            { "open_shelf", new[] { "shelf_wood", "shelf_paint" } },
            { "shelf_goods", new[] { "book_a", "book_b", "book_c", "book_d", "jar_glass", "cork", "glass_aqua", "glass_amber", "glass_green", "glass_cobalt", "pottery_cream", "pottery_teal", "basket" } },
            { "table_clutter", new[] { "cutting_mat", "paper", "paper_sketch", "book_a", "book_b", "book_c", "book_d", "jar_glass", "cork", "pottery_teal", "pottery_cream", "wood_frame", "lamp", "glass_aqua", "glass_amber", "glass_green", "glass_cobalt" } },
            { "bench_clutter", new[] { "book_a", "book_d", "pottery_teal", "jar_glass", "cork", "glass_aqua", "glass_amber", "glass_green", "glass_cobalt" } },
        };

        static GameObject Place(Transform root, string name, Mesh m, Vector3 p, float yaw, bool solid)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0));
            go.AddComponent<MeshFilter>().sharedMesh = m;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterials = Slots[m.name].Select(Mat).ToArray();
            if (solid) { var c = go.AddComponent<BoxCollider>(); c.center = m.bounds.center; c.size = m.bounds.size; }
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        static Material Mat(string name)
        {
            string path = Folder + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", Colours[name]);
            m.SetFloat("_Smoothness", name.StartsWith("glass") || name == "jar_glass" ? .6f : .12f);
            m.SetFloat("_GradientStrength", 0); m.SetFloat("_Variation", .08f);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
