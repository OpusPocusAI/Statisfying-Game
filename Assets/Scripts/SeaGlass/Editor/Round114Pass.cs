using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 114, from the hundred-and-tenth review (all three: the basin suds "flat white cut-out shapes with ring bubbles",
    // "a flat white decal splat, not volumetric"): the suds become real geometry.
    // - foam_clump (round114.py): a heap of merged soap bubbles with a flat underside.
    // - Assets/Art/FoamClump.mat (Sea Glass/Foam Clump): soft lit foam, white in the light, cool lilac-blue in the shade.
    // - Feedback.foamMesh / foamMaterial: the brushing foam is drawn as lit mesh particles with these.
    // Safe to re-run.
    public static class Round114Pass
    {
        const string MaterialPath = "Assets/Art/FoamClump.mat";

        [MenuItem("Sea Glass/Art Direction/113 Round 114: foam clumps")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Leave Play Mode first.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath);
            ModelLibraryImporter.Import("ArtSource/blender/out/foam_clump.json");
            var mesh = ModelLibraryImporter.Load("foam_clump");
            if (!mesh) throw new System.InvalidOperationException("Import foam_clump first (round114.py).");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (!mat)
            {
                var shader = Shader.Find("Sea Glass/Foam Clump");
                if (!shader) throw new System.InvalidOperationException("Sea Glass/Foam Clump shader is missing.");
                mat = new Material(shader) { name = "FoamClump" };
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            var fb = Object.FindFirstObjectByType<Feedback>(FindObjectsInactive.Include);
            if (!fb) throw new System.InvalidOperationException("No Feedback in the scene.");
            fb.foamMesh = mesh; fb.foamMaterial = mat;
            EditorUtility.SetDirty(fb);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 114: foam clumps wired to " + fb.name);
        }
    }
}
