using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 27, from the twenty-fifth review (three reviewers averaged):
    // - Island grass was chosen face by face and read as "blocky pixel camo". The islands (round25.py) are now one
    //   ground surface; their rock material gets the Painterly top colour (_TopTint), which paints grass per pixel on
    //   upward faces with a soft, noisy edge. A copy of the rock material is used so beach rocks stay bare.
    // - The glass slabs (round21.py) are lenses: thick in the middle, thin rolled rims, no straight broken edge.
    // Shader changes this round: FrostedGlass (frosted colour shows through, patchy sand, softer brush edge),
    // TidalWater (softer shadows), Painterly (top colour). Safe to re-run.
    public static class Round27Pass
    {
        const string Folder = "Assets/Art/Round27";

        [MenuItem("Sea Glass/Art Direction/38 Round 27: island grass blend, lens glass")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Round27");
            ModelLibraryImporter.ImportAll();
            Islands();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void Islands()
        {
            var isles = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(f => f.sharedMesh && f.sharedMesh.name.StartsWith("far_isle_")).ToList();
            if (isles.Count == 0) return;
            string path = Folder + "/Island rock with grass.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var first = isles[0].GetComponent<MeshRenderer>().sharedMaterials;
            var rock = first[0].name == "Island rock with grass" ? null : first[0];
            if (!mat)
            {
                if (!rock) return;
                mat = new Material(rock) { name = "Island rock with grass" };
                AssetDatabase.CreateAsset(mat, path);
            }
            // The grass colour is the old cap material's, a touch lighter so ledges read in the haze.
            var cap = first.Length > 1 ? first[1] : null;
            var green = cap && cap.HasProperty("_BaseColor") ? cap.GetColor("_BaseColor") : new Color(.42f, .55f, .3f);
            green = Color.Lerp(green, new Color(.55f, .62f, .32f), .25f);
            mat.SetColor("_TopTint", new Color(green.r, green.g, green.b, 1));
            mat.SetFloat("_TopStart", .62f);
            EditorUtility.SetDirty(mat);
            foreach (var f in isles)
            {
                var r = f.GetComponent<MeshRenderer>(); var mats = r.sharedMaterials;
                mats[0] = mat; r.sharedMaterials = mats; EditorUtility.SetDirty(r);
            }
        }
    }
}
