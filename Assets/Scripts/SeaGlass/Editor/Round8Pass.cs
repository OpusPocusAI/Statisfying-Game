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

            // Round 9: extending the sea box past the horizon made the whole surface render as fog in high views
            // (cause not established), so it keeps its authored size; the horizon gap shows only from high above.
            var sea = GameObject.Find("Open turquoise water");
            if (sea) { sea.transform.position = new Vector3(-88, -1, 75); sea.transform.localScale = new Vector3(120, .3f, 360); EditorUtility.SetDirty(sea.transform); }

            // Beach zone boards read grey-on-taupe against the low sun: painted teal boards with cream lettering.
            var signTeal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round6/Sign teal.mat");
            if (!signTeal) { signTeal = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(signTeal, "Assets/Art/Round6/Sign teal.mat"); }
            signTeal.SetColor("_BaseColor", new Color(.13f, .38f, .42f)); signTeal.SetFloat("_GradientStrength", 0); EditorUtility.SetDirty(signTeal);
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                if (r.name == "Beach section sign") { r.sharedMaterial = signTeal; EditorUtility.SetDirty(r); }
            foreach (var t in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                if (t.transform.position.x > 0 && t.transform.position.z < 150 && t.text.Contains("  /  ")) { t.color = new Color(.99f, .95f, .85f); EditorUtility.SetDirty(t); }

            // Tree canopies: lighter and graded from a cool underside to a sunlit top.
            var pine = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Vegetation/Pine foliage.mat");
            if (pine) { pine.SetColor("_BaseColor", new Color(.27f, .5f, .37f)); pine.SetFloat("_GradientStrength", .55f); pine.SetFloat("_GradientHeight", 7); EditorUtility.SetDirty(pine); }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
