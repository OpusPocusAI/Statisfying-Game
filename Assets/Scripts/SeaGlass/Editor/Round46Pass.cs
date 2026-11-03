using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 46, from the forty-second review (every reviewer, every review: the truck "reads as boxes stuck together,
    // with a canopy fringe of discs"; bushes are "piles of balls"; the owner's rule is no primitive assemblies):
    // - truck_shell (round46.py): the bodywork as ONE fused surface (lower body, cab sides and doors, crowned bonnet,
    //   wheel arches flaring out of the body, rounded bumpers, wheel wells cut out), coloured by region. It goes under
    //   the truck's sprung body; the separate pieces it replaces are hidden (renderers off, not deleted): the painted
    //   chassis, both door slabs and enamel doors, the door belts, both bonnets, the four wheel arches and both bumpers.
    //   Lamps, grille, badge, handles, mirrors, wheels, cab posts, seat and the cargo tubs stay.
    // - truck_canopy: one continuous scalloped valance instead of separate discs (same name, so it updates in place).
    // - coastal_shrub_leafy: one fused leafy canopy with leaves along the outline (same name; all 200 bushes update).
    // Safe to re-run.
    public static class Round46Pass
    {
        static readonly string[] Replaced = { "Painted chassis", "Door", "Enamel door", "Door cream belt", "Cream bonnet", "Soft bonnet", "Ivory wheel arch", "Front bumper", "Rounded front bumper", "Dashboard" };   // the old dashboard box poked up through the new bonnet

        [MenuItem("Sea Glass/Art Direction/50 Round 46: fused truck body, valance, leafy bushes")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();

            var truck = GameObject.Find("TIDELINE • utility truck");
            if (!truck) throw new System.InvalidOperationException("No truck in the scene.");
            var sprung = truck.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Sprung body") ?? truck.transform;

            var mats = new Dictionary<string, Material>();
            foreach (var r in truck.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials) if (m && !mats.ContainsKey(m.name)) mats[m.name] = m;
            Material Get(string n) => mats.TryGetValue(n, out var m) ? m : null;
            var slots = new[] { Get("Lagoon paint"), Get("Tidal teal") ?? Get("Lagoon paint"), Get("Ivory enamel"), Get("Soft charcoal") };
            if (slots.Any(m => !m)) throw new System.InvalidOperationException("Missing a truck paint material.");

            var mesh = ModelLibraryImporter.Load("truck_shell");
            if (!mesh) throw new System.InvalidOperationException("Import truck_shell first (round46.py).");
            var old = sprung.Find("Fused body shell"); if (old) Object.DestroyImmediate(old.gameObject);
            var shell = new GameObject("Fused body shell");
            shell.transform.SetParent(sprung, false);
            shell.transform.localPosition = Vector3.zero; shell.transform.localRotation = Quaternion.identity; shell.transform.localScale = Vector3.one;
            shell.AddComponent<MeshFilter>().sharedMesh = mesh;
            shell.AddComponent<MeshRenderer>().sharedMaterials = slots;

            int hidden = 0;
            foreach (var r in truck.GetComponentsInChildren<MeshRenderer>(true))
                if (Replaced.Contains(r.name) && r.gameObject != shell) { r.enabled = false; EditorUtility.SetDirty(r); hidden++; }
            Debug.Log("Round 46: fused truck body placed, " + hidden + " separate body pieces hidden");

            // The fused bush: sunlit, warm light-green tops (painted per pixel from the surface facing, as on the island
            // grass) over a lighter base, so it reads as foliage at sunset, not a dark lump.
            foreach (var pair in new[] { ("Shrub dark", new Color(.34f, .5f, .32f)), ("Shrub lit", new Color(.46f, .62f, .3f)) })
            {
                var m = AssetDatabase.FindAssets("t:Material " + pair.Item1).Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(x => x && x.name == pair.Item1);
                if (!m) continue;
                m.SetColor("_BaseColor", pair.Item2);
                if (m.HasProperty("_TopTint")) { m.SetColor("_TopTint", new Color(.68f, .78f, .38f, .85f)); m.SetFloat("_TopStart", .4f); }
                EditorUtility.SetDirty(m);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
