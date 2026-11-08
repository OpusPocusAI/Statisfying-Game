using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 84, from the eightieth review (all three: "a hard-edged lighter turquoise sheet around the island base", b5; two also
    // in b1 and b3). The offshore islands ("Wind-shaped coastal stone") end about 4.6 m under the surface. The water is see-through
    // to about that depth (TidalWater.shader, since round 51), so each island's underwater flank showed as a pale turquoise skirt
    // that stopped dead along the mesh's bottom rim, with deep, opaque water right beside it.
    // Each island gets its own copy of its mesh in which everything under the waterline plunges away: a vertex's depth below the
    // surface grows by the square of how deep it already is (a little near the surface, about 9.5 m more at the old rim), so the
    // flank turns steep and sinks out of sight before it ends. Above the water nothing moves. Safe to re-run: it always starts
    // from a saved copy of each island's original mesh.
    public static class Round84Pass
    {
        const string Folder = "Assets/Art/Terrain/Island flanks";
        const float Surface = -0.85f, OldRim = 4.6f, Extra = 9.5f;

        [MenuItem("Sea Glass/Art Direction/80 Round 84: island flanks sink out of sight")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var islands = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(r => r.name == "Wind-shaped coastal stone" && r.bounds.min.y < Surface - 1f
                            && r.bounds.center.x < CoastShape.ShoreX(r.bounds.center.z) - 4f)   // only offshore ones that reach under the water
                .Select(r => r.GetComponent<MeshFilter>())
                .Where(f => f && f.sharedMesh).OrderBy(f => f.transform.position.z).ToList();
            if (islands.Count == 0) throw new System.InvalidOperationException("No offshore islands in the scene.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art/Terrain", "Island flanks");

            int moved = 0;
            for (int i = 0; i < islands.Count; i++)
            {
                var f = islands[i]; var t = f.transform;
                string srcPath = $"{Folder}/Island {i + 1} (round 84 source).asset", outPath = $"{Folder}/Island {i + 1}.asset";
                var src = AssetDatabase.LoadAssetAtPath<Mesh>(srcPath);
                if (!src) { src = Object.Instantiate(f.sharedMesh); src.name = $"Island {i + 1} (round 84 source)"; AssetDatabase.CreateAsset(src, srcPath); }

                var v = src.vertices;
                for (int k = 0; k < v.Length; k++)
                {
                    var w = t.TransformPoint(v[k]);
                    float depth = Surface - w.y; if (depth <= 0) continue;
                    float r = depth / OldRim;
                    w.y = Surface - (depth + r * r * Extra);
                    v[k] = t.InverseTransformPoint(w); moved++;
                }
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(outPath);
                if (!mesh) { mesh = Object.Instantiate(src); AssetDatabase.CreateAsset(mesh, outPath); }
                else { mesh.Clear(); EditorUtility.CopySerialized(src, mesh); }
                mesh.name = $"Island {i + 1}";
                mesh.vertices = v; mesh.RecalculateBounds();   // normals kept: the flank's shading above the water stays exactly as it was
                EditorUtility.SetDirty(mesh);
                f.sharedMesh = mesh;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Round 84: {islands.Count} island flanks, {moved} underwater vertices sunk");
        }
    }
}
