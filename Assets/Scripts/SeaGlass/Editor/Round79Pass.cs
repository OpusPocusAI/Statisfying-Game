using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 79, from the seventy-fifth review (two of three: "a quilted cell pattern across the sand" in b4, "faceted cellular
    // patches, like a visible mesh tessellation"; reported on and off since review 40 as "faceted dunes in b4"). The open beach's
    // height came from 48 separate "Beach swell" mounds (TerrainShapePass), each its own mesh, overlapping each other: wherever two
    // cut through one another the surface folded along a hard crease, and from b4's height those creases drew a net of polygonal
    // cells. Round 78 faded the streaky sand texture that had half hidden them.
    // The swells now become one fused surface: the same mounds, sampled from their transforms with the same cosine profile, joined
    // by a smooth blend (a 4-norm of their heights: where one dominates it is that swell, where two meet the surface rounds over
    // instead of creasing) into a single 0.4 m heightfield with shared normals. Only cells that rise above the flat sand shelf are
    // kept, so the surface meets the shelf tangentially at its edges, as the swells did. The swell objects are switched off, not
    // deleted, so the pass can re-read them. Safe to re-run.
    public static class Round79Pass
    {
        const string MeshPath = "Assets/Art/Terrain/Beach surface.asset";
        const float Step = 0.4f, P = 4f;

        [MenuItem("Sea Glass/Art Direction/76 Round 79: one fused beach surface")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var root = GameObject.Find("ART • beach swells");
            if (!root) throw new System.InvalidOperationException("No beach swells in the scene.");
            var swells = root.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.name == "Beach swell").Select(r => r.transform).ToList();
            if (swells.Count == 0) throw new System.InvalidOperationException("No beach swells in the scene.");
            var sand = swells[0].GetComponent<MeshRenderer>().sharedMaterial;
            float baseY = swells[0].position.y;

            float x0 = float.MaxValue, x1 = float.MinValue, z0 = float.MaxValue, z1 = float.MinValue;
            foreach (var t in swells)
            {
                float r = Mathf.Max(t.localScale.x, t.localScale.z) + 1f;
                x0 = Mathf.Min(x0, t.position.x - r); x1 = Mathf.Max(x1, t.position.x + r);
                z0 = Mathf.Min(z0, t.position.z - r); z1 = Mathf.Max(z1, t.position.z + r);
            }
            int nx = Mathf.CeilToInt((x1 - x0) / Step) + 1, nz = Mathf.CeilToInt((z1 - z0) / Step) + 1;
            var v = new Vector3[nx * nz]; var raised = new bool[nx * nz];
            for (int j = 0; j < nz; j++)
                for (int i = 0; i < nx; i++)
                {
                    var w = new Vector3(x0 + i * Step, 0, z0 + j * Step);
                    float sum = 0;
                    foreach (var t in swells)
                    {
                        // Unit swell (TerrainShapePass.SwellMesh): radius 1, height .5 + .5 cos(pi r), meeting the ground tangentially.
                        var l = Quaternion.Inverse(t.rotation) * (w - new Vector3(t.position.x, 0, t.position.z));
                        float rr = Mathf.Sqrt(l.x * l.x / (t.localScale.x * t.localScale.x) + l.z * l.z / (t.localScale.z * t.localScale.z));
                        if (rr >= 1) continue;
                        float h = (.5f + .5f * Mathf.Cos(rr * Mathf.PI)) * t.localScale.y;
                        sum += Mathf.Pow(h, P);
                    }
                    float H = sum > 0 ? Mathf.Pow(sum, 1 / P) : 0;
                    v[j * nx + i] = new Vector3(w.x, baseY + H, w.z);
                    raised[j * nx + i] = H > .002f;
                }
            var tris = new List<int>();
            for (int j = 0; j < nz - 1; j++)
                for (int i = 0; i < nx - 1; i++)
                {
                    int a = j * nx + i, b = a + 1, c = a + nx, d = c + 1;
                    if (!(raised[a] || raised[b] || raised[c] || raised[d])) continue;   // flat shelf: leave it to the shelf
                    tris.AddRange(new[] { a, c, b, b, c, d });
                }
            // Drop unused vertices so the mesh stays lean.
            var map = new int[v.Length]; for (int k = 0; k < map.Length; k++) map[k] = -1;
            var nv = new List<Vector3>();
            for (int k = 0; k < tris.Count; k++) { int o = tris[k]; if (map[o] < 0) { map[o] = nv.Count; nv.Add(v[o]); } tris[k] = map[o]; }

            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, MeshPath); }
            mesh.Clear(); mesh.name = "Beach surface";
            mesh.indexFormat = nv.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(nv); mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
            EditorUtility.SetDirty(mesh);

            var go = root.transform.Find("Beach surface")?.gameObject;
            if (!go)
            {
                go = new GameObject("Beach surface", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                go.transform.SetParent(root.transform, false);
            }
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); go.transform.localScale = Vector3.one;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = sand;
            go.GetComponent<MeshCollider>().sharedMesh = mesh;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            foreach (var t in swells) t.gameObject.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 79: beach surface from " + swells.Count + " swells, " + nv.Count + " vertices, " + tris.Count / 3 + " triangles");
        }
    }
}
