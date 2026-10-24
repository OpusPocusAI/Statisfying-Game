using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 9 (Documentation/ART_DIRECTION.md, section 5.10): a visible neglected-to-restored
    // studio. Models: ArtSource/blender/restoration.py. Behaviour: StudioRestoration.
    public static class StudioRestorationPass
    {
        const string Folder = "Assets/Art/StudioFurniture";

        [MenuItem("Sea Glass/Art Direction/9 Studio restoration")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var old = GameObject.Find("ART • studio restoration"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • studio restoration");
            var restoration = root.AddComponent<StudioRestoration>();

            var stem = Mat("Stem green", new Color(.25f, .45f, .3f), .1f);
            var petals = new[] { Mat("Petal coral", new Color(.95f, .48f, .38f), .2f), Mat("Petal cream", new Color(.98f, .9f, .74f), .2f), Mat("Petal lilac", new Color(.72f, .58f, .86f), .2f) };
            var centre = Mat("Flower centre", new Color(.98f, .76f, .3f), .2f);
            var flowers = new List<Transform>();
            int k = 0;
            foreach (var planter in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r => r.name == "Open terracotta entry planter").OrderBy(r => r.bounds.center.x))
            {
                var top = planter.bounds.center + Vector3.up * (planter.bounds.extents.y * .7f);
                for (int j = 0; j < 2; j++)
                {
                    var go = new GameObject("Flower sprig");
                    go.transform.SetParent(root.transform);
                    go.transform.SetPositionAndRotation(top + new Vector3(j == 0 ? -.12f : .14f, 0, j == 0 ? .08f : -.06f), Quaternion.Euler(0, k * 67 + j * 140, 0));
                    go.transform.localScale = Vector3.one * 1.2f;
                    go.AddComponent<MeshFilter>().sharedMesh = ModelLibraryImporter.Load("flower_sprig");
                    go.AddComponent<MeshRenderer>().sharedMaterials = new[] { stem, petals[(k + j) % 3], centre };
                    flowers.Add(go.transform);
                }
                k++;
            }
            // Row order: flower i opens with panel i % 3, so each panel opens a third spread along the entry.
            restoration.flowers = flowers.ToArray();

            var wire = Mat("Festoon wire", new Color(.12f, .12f, .14f), .3f);
            var socket = Mat("Festoon socket", new Color(.2f, .2f, .22f), .4f);
            var bulb = Mat("Festoon bulb", new Color(1f, .93f, .8f), .6f);
            bulb.EnableKeyword("_EMISSION");
            var bulbs = new List<Renderer>();
            // Three spans hang between the ceiling beams across the studio, one per panel.
            float[] zs = { 158.5f, 165.5f, 172.5f };
            float[] beams = { -13f, -7f, -1f, 5f };
            for (int span = 0; span < 3; span++)
                for (int b = 0; b < beams.Length - 1; b++)
                {
                    var anchor = new Vector3(beams[b], 5.1f, zs[span] + (b % 2) * .4f);
                    var w = Place(root.transform, "festoon_wire", new[] { wire }, anchor);
                    var bu = Place(root.transform, "festoon_bulbs", new[] { socket, bulb }, anchor);
                    bulbs.Add(bu.GetComponent<Renderer>());
                }
            // bulbs[i] lights when panel count > i % 3, so order spans [0,1,2,0,1,2...].
            restoration.bulbs = Enumerable.Range(0, 3).SelectMany(b => Enumerable.Range(0, 3).Select(s => bulbs[s * 3 + b])).ToArray();
            restoration.leaves = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r => r.name == "Succulent leaf").ToArray();
            restoration.Apply(0);   // saved scene shows the neglected state; runtime eases to progress
            EditorUtility.SetDirty(restoration);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static GameObject Place(Transform root, string mesh, Material[] mats, Vector3 at)
        {
            var go = new GameObject(mesh.Replace('_', ' '));
            go.transform.SetParent(root); go.transform.position = at;
            go.AddComponent<MeshFilter>().sharedMesh = ModelLibraryImporter.Load(mesh);
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        static Material Mat(string name, Color c, float smooth)
        {
            string path = Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_GradientStrength", 0);
            EditorUtility.SetDirty(m); return m;
        }
    }
}
