using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 59, from the fifty-fifth review (all three reviewers: the held clean piece read as "a puffy, pillow-like
    // blister", "a hollow, curled film", "a jelly/plastic blob"; two: the room magnified through it read as a jumbled view
    // or a hollow shell. Across rounds 55-58 it read as a dish, a lump, a sheet and a pillow while it floated alone, large,
    // in front of the eye; the tray pieces, small and at rest, pass):
    // - palm_glove (ArtSource/blender/palm_glove.py): the gardening glove from glove.py, open and palm up, fused into one
    //   surface like the gripping glove (canvas glove, coral knit cuff, blue sleeve).
    // - At the sorting trays the clean piece rests convex side up in that palm, held low in front of the player, a few
    //   centimetres across (StudioStation.HoldOnPalm), with the canvas glove behind it instead of the room.
    // Safe to re-run.
    public static class Round59Pass
    {
        [MenuItem("Sea Glass/Art Direction/63 Round 59: held glass in an open palm")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var mesh = ModelLibraryImporter.Load("palm_glove");
            if (!mesh) throw new System.InvalidOperationException("Import palm_glove first (ArtSource/blender/palm_glove.py).");
            var mats = new[] { "Glove canvas", "Glove cuff", "Sleeve denim" }
                .Select(n => AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/GatheringPolish/" + n + ".mat")).ToArray();
            if (mats.Any(m => !m)) throw new System.InvalidOperationException("Run the gloved-hands pass first (glove materials missing).");
            int n2 = 0;
            foreach (var st in Object.FindObjectsByType<SeaGlass.StudioStation>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(s => s.kind == SeaGlass.StationKind.Sort))
            {
                if (st.heldPalm) Object.DestroyImmediate(st.heldPalm.gameObject);
                foreach (var old in st.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Held palm").ToArray()) Object.DestroyImmediate(old.gameObject);
                var go = new GameObject("Held palm");
                go.transform.SetParent(st.transform, false);
                var s = st.transform.lossyScale; go.transform.localScale = new Vector3(1 / s.x, 1 / s.y, 1 / s.z);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.SetActive(false);
                st.heldPalm = go.transform; EditorUtility.SetDirty(st); n2++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 59: open gloved palm added to " + n2 + " sorting station(s)");
        }
    }
}
