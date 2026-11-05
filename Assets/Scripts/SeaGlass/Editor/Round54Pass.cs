using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 54, from the fiftieth review (all three reviewers: the held clean piece "reads as a creased jelly lump" or "a
    // pillowy dome", not a worn shard of bottle wall, ART_DIRECTION 5.6):
    // - glass_slab_0..5 (round21.py) had both faces bulging outwards, so every outline became a lens or pillow. A shard
    //   of bottle wall is a curved plate: convex outside, concave inside, both faces bent the same way. Each piece now
    //   follows one cylindrical bend (about 10% of its width) with the wedge taper and blunt rim kept.
    // - FrostedGlass measures thickness (deep colour where thick, clear at the thin edge, the brush's reveal order) from
    //   the same bent mid-surface: _Bend on every glass material, matching round21.py's BEND.
    // - Round37Pass re-imports the models and sets every tray spare and beach find back down on what it rested on.
    // Safe to re-run.
    public static class Round54Pass
    {
        public const float Bend = -.1f;   // negative: matches round21.py; the work pieces lie outer (convex) face up

        [MenuItem("Sea Glass/Art Direction/58 Round 54: sea glass as curved bottle wall")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            Round37Pass.Apply();
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material"))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (!m || !m.shader || m.shader.name != "Sea Glass/Frosted Glass" || !m.HasProperty("_Bend")) continue;
                if (!AssetDatabase.GetAssetPath(m).StartsWith("Assets/Materials/SeaGlass/Glass ")) continue;   // only the six sea-glass colours (not the jar or the mosaic panes)
                m.SetFloat("_Bend", Bend); EditorUtility.SetDirty(m); AssetDatabase.SaveAssetIfDirty(m); n++;
            }
            // The basin piece lies outer (convex) face up, as sea glass rests, so its ends stay under the water. The piece held
            // up at the sorting trays is turned over: seen convex side on, the bend refracted the dark tray rim into a crease
            // through it; the inner face reads as clear curved glass.
            int turned = 0;
            foreach (var st in Object.FindObjectsByType<SeaGlass.StudioStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (st.kind == SeaGlass.StationKind.Sort && st.workingGlass)
                { st.workingGlass.rotation = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 180, 0); EditorUtility.SetDirty(st.workingGlass); turned++; }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 54: sea glass bent like bottle wall; _Bend set on " + n + " glass materials; " + turned + " held sort piece turned inner face out");
        }
    }
}
