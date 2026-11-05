using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaGlass.Editor
{
    // Round 51, from the forty-seventh review (two of three reviewers name it as the single biggest gap to Tchia: "the
    // water is opaque; no sand or rock visible through the shallows, no depth gradient at the waterline"):
    // - The water shader has sampled the scene behind it since the October side-by-side rounds, but there was nothing
    //   to see: the wet-sand shoulder (ShoreTransitionPolish) ends 2 m past the waterline at 0.45 m depth, and beyond it
    //   the depth buffer held open sky, so every pixel past that strip rendered as solid lagoon colour.
    // - A seabed: one sloping sand mesh that follows the authored coastline (CoastShape.ShoreX) out to 100 m offshore,
    //   shelving gently (about 1.2 m deep at 10 m out, 3 m at 30 m) with low sandbars, then dropping away. It starts
    //   just under the shoulder's last row so the two meet without a step. Sand material is the beach sand, a little
    //   warmer and darker, world-mapped so its ripples show through the water. No collider, no shadow casting.
    // - TidalWater.shader: the bed's colour is absorbed with depth red first and takes a cool tint, so the sand turns
    //   turquoise as it deepens (a first try with gentler red absorption read pale yellow-green), and the water
    //   only reaches its opaque lagoon colour at about 4.6 m depth instead of 2.6 m.
    // Safe to re-run: the mesh asset and the seabed object are replaced in place.
    public static class Round51Pass
    {
        const string Folder = "Assets/Art/Round51";
        const string ObjectName = "ART \u2022 seabed";

        // Offshore distance from the waterline (m) and seabed height (world Y). The water surface is at Y -0.85; the
        // shoulder reaches Y -1.3 at 2 m out.
        // Round 53 (review 49, all three: "a dark ribbon under the shore foam", b8): under the first 2 m of water the bed
        // seen through the surface was the wet-sand shoulder, which takes hard sun shadows, so wherever a dune or rock
        // shaded the beach the swash turned navy beside the evenly lit seabed. The bed now starts 0.1 m out and lies
        // just above the shoulder's underwater strip (Y -0.85 at the waterline to -1.3 at 2 m, same coastline rows):
        // 6 mm at 0.1 m, 3 cm from 0.6 m on, always below the surface, so everything under the water is the unshadowed
        // seabed except a sliver inside the solid white edge line.
        static readonly float[] Offsets = { .1f, .25f, .6f, 1, 1.5f, 2, 4, 8, 14, 22, 34, 55, 100 };
        static readonly float[] Heights = { -.866f, -.89f, -.955f, -1.045f, -1.158f, -1.27f, -1.45f, -1.7f, -2.1f, -2.8f, -4.0f, -7.0f, -14.0f };

        [MenuItem("Sea Glass/Art Direction/55 Round 51: see-through shallows over a sandbed")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");

            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            int columns = Offsets.Length;
            const float z0 = -110, z1 = 330; int rows = Mathf.RoundToInt(z1 - z0);   // past both ends of the authored coast, so the bed never ends in view
            for (int i = 0; i <= rows; i++)
            {
                float z = z0 + i;
                float shore = CoastShape.ShoreX(z);
                for (int j = 0; j < columns; j++)
                {
                    float d = Offsets[j];
                    // Low sandbars running along the shore, strongest a few metres out, none at the start row.
                    float bar = (Mathf.Sin(z * .21f + d * .35f) * .6f + Mathf.Sin(z * .057f - d * .2f) * .4f) * .12f
                              * Mathf.Clamp01((d - 3) / 6) * (1 - Mathf.Clamp01((d - 34) / 30));
                    // Round 52 (b4 review frame: a straight-edged shallow plate past the end of the beach): beyond the
                    // ends of the sand (the shoulder runs Z -39..201) the bed sinks away over 30 m, so it fades into deep water.
                    float beyond = Mathf.Max(Mathf.Clamp01((z - 201) / 30), Mathf.Clamp01((-39 - z) / 30));
                    float y = Mathf.Lerp(Heights[j] + bar, -14, beyond * beyond * (3 - 2 * beyond));
                    vertices.Add(new Vector3(shore - d, y, z));
                    uv.Add(new Vector2(j / (float)(columns - 1), i / (float)rows));
                    if (i < rows && j < columns - 1)
                    {
                        int a = i * columns + j;
                        triangles.AddRange(new[] { a, a + 1, a + columns, a + 1, a + columns + 1, a + columns });   // faces up (same winding as the shoulder)
                    }
                }
            }
            string meshPath = Folder + "/Seabed.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (!mesh) { mesh = new Mesh { name = "Seabed" }; AssetDatabase.CreateAsset(mesh, meshPath); } else mesh.Clear();
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            EditorUtility.SetDirty(mesh); AssetDatabase.SaveAssetIfDirty(mesh);

            string matPath = Folder + "/Seabed sand.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var beach = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            if (!mat) { mat = new Material(beach) { name = "Seabed sand" }; AssetDatabase.CreateAsset(mat, matPath); }
            // Copied from the beach sand only when first created: re-copying on a re-run left properties the beach never
            // stored (round 52: _SunShadow) unsettable for the rest of the editor session.
            mat.SetColor("_BaseColor", new Color(.84f, .76f, .64f));
            if (mat.HasProperty("_TopTint")) mat.SetColor("_TopTint", new Color(0, 0, 0, 0));
            if (mat.HasProperty("_GroundPatches")) mat.SetFloat("_GroundPatches", .3f);
            if (mat.HasProperty("_Rim")) mat.SetFloat("_Rim", 0);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", .1f);
            EditorUtility.SetDirty(mat); AssetDatabase.SaveAssetIfDirty(mat);

            var go = GameObject.Find(ObjectName);
            if (!go) { go = new GameObject(ObjectName, typeof(MeshFilter), typeof(MeshRenderer)); }
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); go.transform.localScale = Vector3.one;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = true;
            foreach (var c in go.GetComponents<Collider>()) Object.DestroyImmediate(c);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Round 51: seabed under the shallows (" + vertices.Count + " verts); water absorbs the bed more gently");
        }
    }
}
