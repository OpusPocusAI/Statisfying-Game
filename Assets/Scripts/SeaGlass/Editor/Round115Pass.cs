using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 115, from the hundred-and-eleventh review (C: the palm piece "a flat rhombus with a pointed tip"; A: "an opaque teal
    // plastic chip"; all three: the tray pieces "glossy gummy sweets", "lozenge outlines, not broken shards"). The hero plate passed
    // at the basin once it was a thin plate with broken edges; the palm and the trays now use the same plate:
    // - glass_plate_0..5 (round108.py): the thin hero plates with well-rounded corners. GlassDefinition.plateMesh for each piece
    //   (the palm at the trays and the sorted pieces, StudioStation and StudioPresentation).
    // - The tray spares ("Spare ... glass", on glass_slab_k) take glass_plate_k at the same size, set back down onto the felt.
    // Safe to re-run.
    public static class Round115Pass
    {
        [MenuItem("Sea Glass/Art Direction/114 Round 115: thin plates in the palm and trays")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Leave Play Mode first.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath);
            var plates = new Mesh[6];
            for (int k = 0; k < 6; k++)
            {
                ModelLibraryImporter.Import("ArtSource/blender/out/glass_plate_" + k + ".json");
                plates[k] = ModelLibraryImporter.Load("glass_plate_" + k);
                if (!plates[k]) throw new System.InvalidOperationException("Import glass_plate_" + k + " first (round108.py).");
            }
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            int n = 0;
            foreach (var def in catalog.glass)
            {
                if (!def.pebbleMesh || !def.pebbleMesh.name.StartsWith("glass_slab_")) continue;
                def.plateMesh = plates[int.Parse(def.pebbleMesh.name.Substring("glass_slab_".Length))]; n++;
            }
            EditorUtility.SetDirty(catalog);
            int s = 0;
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!f.name.StartsWith("Spare ") || !f.name.EndsWith(" glass") || !f.sharedMesh) continue;
                var name = f.sharedMesh.name;
                if (!name.StartsWith("glass_slab_")) continue;
                var r = f.GetComponent<MeshRenderer>();
                float oldSpan = Mathf.Max(f.sharedMesh.bounds.size.x, f.sharedMesh.bounds.size.y);
                float oldBottom = r.bounds.min.y;
                var plate = plates[int.Parse(name.Substring("glass_slab_".Length))];
                Undo.RecordObject(f, "Round 115"); Undo.RecordObject(f.transform, "Round 115");
                f.sharedMesh = plate;
                f.transform.localScale *= oldSpan / Mathf.Max(plate.bounds.size.x, plate.bounds.size.y);
                r.ResetBounds();
                f.transform.position += Vector3.up * (oldBottom - r.bounds.min.y);
                s++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 115: plates on " + n + " pieces, " + s + " tray spares");
        }
    }
}
