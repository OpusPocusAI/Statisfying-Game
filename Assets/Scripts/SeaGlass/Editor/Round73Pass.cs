using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 73, from the sixty-ninth review (all three: the revealed basin piece still "a thick jelly or gummy shield", "a mint
    // jelly or soap bar", "pillowy and evenly thick, like a gummy sweet", after two shader rounds on it). The glass_slab meshes
    // pass in the trays and the palm at a few centimetres across; at the basin the piece filled a fifth of the frame:
    // - glass_basin_0..5 (round73.py): hero versions of each slab with the same outline, wedge and bend, a crisper edge over a
    //   narrow worn bevel, conchoidal chips along the rim with flake scars beside them, and fine pitting over both faces.
    //   Each catalog piece gets the hero version of its own slab (GlassDefinition.basinMesh), used only at the basin.
    // - StudioStation: the basin piece is 0.3 m across (was 0.42).
    // Safe to re-run.
    public static class Round73Pass
    {
        [MenuItem("Sea Glass/Art Direction/72 Round 73: hero basin shards")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            int n = 0;
            foreach (var def in catalog.glass)
            {
                if (!def.pebbleMesh || !def.pebbleMesh.name.StartsWith("glass_slab_")) continue;
                var hero = ModelLibraryImporter.Load(def.pebbleMesh.name.Replace("glass_slab_", "glass_basin_"));
                if (!hero) throw new System.InvalidOperationException("Import the glass_basin models first.");
                def.basinMesh = hero; n++;
            }
            EditorUtility.SetDirty(catalog);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 73: hero basin shards on " + n + " pieces");
        }
    }
}
