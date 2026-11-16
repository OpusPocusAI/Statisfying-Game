using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Palms and wet sand pass (owner, November 16, from the independent art review: all three reviewers flagged aliased,
    // shimmering palm fronds and a missing wet-sand band).
    // - Palms: tropical_plants.py now builds each frond as one broad blade per side (leaflets fused at the base, splitting into
    //   bold teeth) instead of 24 thin leaflets; this pass re-imports palm_0..2 in place, so every placed palm updates.
    // - Anti-aliasing: the game ran with none (PC render pipeline MSAA off, camera AA off), so every thin edge crawled. The PC
    //   pipeline asset now uses 4x MSAA (Assets/Settings/PC_RPAsset.asset).
    // - Wet sand: PainterlyForward.hlsl widens the damp band on the wet-sand strip and gives it a broader sun sheen.
    // Repeatable.
    public static class PalmsAndWetSandPass
    {
        [MenuItem("Sea Glass/Art Direction/Palms and wet sand: blade fronds")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            for (int k = 0; k < 3; k++)
            {
                var file = Path.Combine(ModelLibraryImporter.SourceFolder, "palm_" + k + ".json");
                if (File.Exists(file)) ModelLibraryImporter.Import(file); else Debug.LogWarning("Palms: run tropical_plants.py first");
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log("Palms and wet sand: palms re-imported with blade fronds");
        }
    }
}
