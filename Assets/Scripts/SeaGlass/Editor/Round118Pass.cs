using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 118, from the hundred-and-fourteenth review (all three: the tray heaps "scattered flat petals", "flat glossy flakes,
    // almost confetti", "paper-thin, like petals or coins, with no wedge visible"). The tray plates were the hero plate's thickness
    // (round108.py), a few millimetres at their size, which from across the bench is no edge at all; and the pieces resting on others
    // lay almost flat, so a heap read as one layer.
    // - The spare plates are 1.8 times thicker (their local z, the plate's thickness; the shader grades colour in object space, so
    //   the wedge from clear to deep is unchanged). The sorted pieces match (StudioPresentation.Round118Thick).
    // - They are settled again as in round 117 (Round117Pass.Settle), but a piece that lands on another leans 12-22 degrees off
    //   it (was 5-12), so its edge faces up and the layers show.
    // - FrostedGlass.shader gives the tray plates' edge wall the deep colour of the glass under a soft lip of light.
    // - The spares take "Glass <colour> tray" materials with _Plate on: it was set only at runtime, so the editor and the s10 still
    //   showed them without the plate shading of rounds 115-116.
    // - Round117Pass now measures the felt on the tray's own mesh: it took the lowest piece, which sank with every thickening, and
    //   one try buried most of the Emerald tray under the felt.
    // Safe to re-run (the thickness is set, not multiplied).
    public static class Round118Pass
    {
        [MenuItem("Sea Glass/Art Direction/117 Round 118: tray plates with visible edges")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Leave Play Mode first.");
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) UnityEditor.SceneManagement.EditorSceneManager.OpenScene(CoastBuilder.ScenePath);
            int n = 0;
            var trayMats = new System.Collections.Generic.Dictionary<Material, Material>();
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                         .Where(r => r.name.StartsWith("Spare ") && r.name.EndsWith(" glass")))
            {
                var mesh = r.GetComponent<MeshFilter>().sharedMesh;
                if (!mesh.name.StartsWith("glass_plate_")) continue;
                // The plate shading (_Plate) was switched on only at runtime (StudioPresentation.MarkPlate), so the editor and the
                // review stills showed the tray plates without it. They get their own copy of their colour's material with it on.
                var src = r.sharedMaterial;
                if (!trayMats.TryGetValue(src, out var tm))
                {
                    if (src.name.EndsWith(" tray")) tm = src;
                    else
                    {
                        var path = "Assets/Materials/SeaGlass/" + src.name + " tray.mat";
                        tm = AssetDatabase.LoadAssetAtPath<Material>(path);
                        if (!tm) { tm = new Material(src) { name = src.name + " tray" }; AssetDatabase.CreateAsset(tm, path); }
                        tm.CopyPropertiesFromMaterial(src);
                    }
                    trayMats[src] = tm;
                }
                tm.SetFloat("_Plate", 1); EditorUtility.SetDirty(tm);
                if (r.sharedMaterial != tm) { Undo.RecordObject(r, "Round 118"); r.sharedMaterial = tm; }
                var s = mesh.bounds.size;
                if (!(s.z < s.x && s.z < s.y)) throw new System.InvalidOperationException(mesh.name + " is not thinnest along local z.");
                var t = r.transform; Undo.RecordObject(t, "Round 118");
                float w = t.localScale.x;
                t.localScale = new Vector3(w, w, w * StudioPresentation.Round118Thick);
                n++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Round 118: " + n + " tray plates thickened, " + trayMats.Count + " tray materials");
            Round117Pass.Settle(12f, 10f);
        }
    }
}
