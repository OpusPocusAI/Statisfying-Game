using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 8, from the fifth independent blind review (ArtReference/comparison.md). Run after pass 21.
    // Shader changes this round: TidalWater (receives shadows), BasinWater, FrostedGlass; models: sea stacks
    // as sculpted tapering columns, rounder island tree clumps (ArtSource/blender/round3.py). Safe to re-run.
    public static class Round8Pass
    {
        [MenuItem("Sea Glass/Art Direction/22 Round 8: fifth review fixes")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            SeaGlassPiecesPass.Apply();   // flatter, less puffy profile

            // Basin: teal water over the enamel, not a milky sheet.
            var w = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CoastalPolish/Still basin water.mat");
            if (w) { w.SetColor("_BaseColor", new Color(.58f, .82f, .84f)); w.SetColor("_Deep", new Color(.12f, .4f, .46f)); EditorUtility.SetDirty(w); }

            // The road slab's 15 cm side face showed against the sand. Soft sand shoulders slope from the road
            // edge down to the beach on both sides (no colliders, so driving and recovery are unchanged).
            var old = GameObject.Find("ART • road shoulders"); if (old) Object.DestroyImmediate(old);
            var root = new GameObject("ART • road shoulders").transform;
            var sand = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            foreach (var (x, angle) in new[] { (9.95f, 8.5f), (20.05f, -8.5f) })
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = "Sand road shoulder";
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.transform.SetParent(root);
                go.transform.SetPositionAndRotation(new Vector3(x, -.03f, 79), Quaternion.Euler(0, 0, angle));
                go.transform.localScale = new Vector3(1.25f, .06f, 198);
                go.GetComponent<MeshRenderer>().sharedMaterial = sand;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
