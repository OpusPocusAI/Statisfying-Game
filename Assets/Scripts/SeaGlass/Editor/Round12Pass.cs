using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 12, from the eighth independent blind review: one shared family of tumbled pebbles for every colour
    // (held, sorted, found and carried glass; mosaic pieces keep their socket-fitting meshes), and an oval
    // enamel bowl with clear water at the cleaning station. Models: ArtSource/blender/round12.py.
    // Runs passes 20 and 21 again for the tray piles. Safe to re-run.
    public static class Round12Pass
    {
        const string Folder = "Assets/Art/Round12";

        [MenuItem("Sea Glass/Art Direction/25 Round 12: pebble family and enamel bowl")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round12");
            ModelLibraryImporter.ImportAll();

            var pebbles = Enumerable.Range(0, 5).Select(k => ModelLibraryImporter.Load("glass_pebble_" + k)).ToArray();
            if (pebbles.Any(p => !p)) throw new System.InvalidOperationException("Import the glass_pebble models first.");
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            for (int i = 0; i < catalog.glass.Length; i++) catalog.glass[i].pebbleMesh = pebbles[(i * 7 + 3) % 5];
            EditorUtility.SetDirty(catalog);

            // Beach finds show the pebble too, at the same size as before.
            foreach (var item in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(i => i.glass && i.visual))
            {
                var def = catalog.glass.FirstOrDefault(d => d.id == item.id); var f = item.visual.GetComponent<MeshFilter>();
                if (def == null || !f || f.sharedMesh == def.pebbleMesh) continue;
                var old = f.sharedMesh.bounds.size; float oldSize = Mathf.Max(old.x, old.y) * item.visual.localScale.x;
                var nb = def.pebbleMesh.bounds.size;
                f.sharedMesh = def.pebbleMesh;
                item.visual.localScale = Vector3.one * oldSize / Mathf.Max(nb.x, nb.y);
                EditorUtility.SetDirty(f); EditorUtility.SetDirty(item.visual);
            }

            Bowl();
            Round6Pass.Apply(); Round7Pass.Apply();   // tray piles now use the pebbles
            TruckRefinePass.Apply();                  // truck cargo now uses the pebbles

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // Oval enamel wash bowl (cream enamel, dark blue rolled rim) in place of the square tray, and an oval
        // water surface, both keeping the old world footprint.
        static void Bowl()
        {
            var bowl = ModelLibraryImporter.Load("enamel_bowl"); var disc = ModelLibraryImporter.Load("bowl_water");
            var basin = GameObject.Find("Enamel basin"); var water = GameObject.Find("Basin water");
            if (!bowl || !disc || !basin || !water) return;
            var bf = basin.GetComponent<MeshFilter>();
            if (bf.sharedMesh != bowl)
            {
                var b = basin.GetComponent<Renderer>().bounds;
                bf.sharedMesh = bowl;
                var nb = bowl.bounds.size; var ls = basin.transform.lossyScale; var pl = basin.transform.localScale;
                basin.transform.localScale = new Vector3(pl.x / ls.x * b.size.x / nb.x, pl.y / ls.y * b.size.y / nb.y, pl.z / ls.z * b.size.z / nb.z);
                basin.transform.rotation = Quaternion.identity;
                basin.transform.position += new Vector3(b.center.x, b.min.y, b.center.z) - basin.GetComponent<Renderer>().bounds.center + Vector3.up * basin.GetComponent<Renderer>().bounds.extents.y;
                EditorUtility.SetDirty(basin.transform); EditorUtility.SetDirty(bf);
            }
            basin.GetComponent<MeshRenderer>().sharedMaterials = new[] { Paint("Bowl enamel", new Color(.95f, .94f, .9f), .55f), Paint("Bowl rim", new Color(.16f, .3f, .46f), .5f) };
            var wf = water.GetComponent<MeshFilter>();
            if (wf.sharedMesh != disc) { wf.sharedMesh = disc; EditorUtility.SetDirty(wf); }
            var w = water.GetComponent<MeshRenderer>().sharedMaterial;
            w.SetColor("_BaseColor", new Color(.3f, .74f, .72f)); w.SetColor("_Deep", new Color(.1f, .5f, .54f)); EditorUtility.SetDirty(w);
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
