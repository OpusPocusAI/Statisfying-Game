using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaGlass.Editor
{
    // Studio interior pass (owner, November 16: Tchia is the art target; no primitive assemblies). The studio is well
    // dressed, but about 250 of its pieces were still Unity's sharp-edged cube: the floorboards, ceiling joints and beams,
    // wall battens, rails and sills, window frames, gallery frames, shelf brackets and lips, sign boards, folded linen. Hard
    // edges catch no light, so the room read flat and boxy. This pass:
    // - softens every indoor cube into a bevelled piece of its own size (rounded edges with a constant radius in metres,
    //   whatever the object's scale), and gives long timbers a slight, seeded hand-made sag, so boards and battens stop
    //   reading as ruled lines. Transforms, colliders and children are untouched (only the mesh changes);
    // - lays warm window light on the floor: each ocean-wall window throws its four panes, mullion and transom as a soft
    //   additive patch (Sea Glass/Window Light) along the daylight direction.
    // Repeatable: the softened meshes live in one asset that is rebuilt each run, and the light patches are rebuilt.
    public static class StudioInteriorPass
    {
        const string Folder = "Assets/Art/StudioInterior", MeshAsset = Folder + "/Softened timber.asset", LightRoot = "ART • window light";
        static readonly string[] Skip = { "Dark grout" };   // the mosaic's grout lines stay crisp

        [MenuItem("Sea Glass/Art Direction/Studio interior: softened timber and window light")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "StudioInterior");

            var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var targets = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(f => f.sharedMesh && (f.sharedMesh == cube || AssetDatabase.GetAssetPath(f.sharedMesh) == MeshAsset))
                .Where(f => { var r = f.GetComponent<MeshRenderer>(); return r && CoastalPlayer.Indoors(r.bounds.center); })
                .Where(f => { if (Skip.Contains(f.name)) return false; var mc = f.GetComponent<MeshCollider>(); return !mc || mc.sharedMesh != f.sharedMesh; })
                .Where(f => f.transform.localScale.x != 0 && f.transform.localScale.y != 0 && f.transform.localScale.z != 0)
                .ToList();
            AssetDatabase.DeleteAsset(MeshAsset);
            var holder = new Mesh { name = "Softened timber" }; AssetDatabase.CreateAsset(holder, MeshAsset);
            int n = 0;
            foreach (var f in targets)
            {
                var m = Softened(f.transform, n);
                AssetDatabase.AddObjectToAsset(m, holder);
                f.sharedMesh = m; n++;
            }
            AssetDatabase.SaveAssets();

            int patches = WindowLight();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Studio interior: {n} indoor boxes softened, {patches} window light patches");
        }

        // A box of the transform's own size with rounded edges, built in metres and scaled back into the unit cube the
        // transform expects, so every bevel has the same real radius. Long, thin pieces sag a few millimetres.
        static Mesh Softened(Transform t, int seed)
        {
            var S = new Vector3(Mathf.Abs(t.localScale.x), Mathf.Abs(t.localScale.y), Mathf.Abs(t.localScale.z));
            var h = S * .5f; float min = Mathf.Min(S.x, Mathf.Min(S.y, S.z));
            float r = Mathf.Clamp(min * .32f, .003f, .028f);
            float[] Coords(float half)
            {
                var list = new List<float>();
                float rr = Mathf.Min(r, half * .999f);
                foreach (float a in new[] { 0f, 30f, 60f }) list.Add(-half + rr * (1 - Mathf.Cos(a * Mathf.Deg2Rad)));   // samples round the bevel
                list.Add(-half + rr);
                int inner = Mathf.Clamp(Mathf.CeilToInt((half * 2 - rr * 2) / .4f), 1, 24);
                for (int i = 1; i < inner; i++) list.Add(Mathf.Lerp(-half + rr, half - rr, i / (float)inner));
                list.Add(half - rr);
                foreach (float a in new[] { 60f, 30f, 0f }) list.Add(half - (a == 0 ? 0 : rr * (1 - Mathf.Cos(a * Mathf.Deg2Rad))));
                return list.Distinct().OrderBy(v => v).ToArray();
            }
            var cx = Coords(h.x); var cy = Coords(h.y); var cz = Coords(h.z);

            // Long axis and its sag direction (the next-thinnest axis, so boards bow through their thickness).
            int longAxis = S.x >= S.y && S.x >= S.z ? 0 : (S.y >= S.z ? 1 : 2);
            float L = S[longAxis];
            int thin = Enumerable.Range(0, 3).Where(a => a != longAxis).OrderBy(a => S[a]).First();
            var rnd = new System.Random(seed * 7919 + Mathf.RoundToInt(t.position.x * 13 + t.position.z * 7));
            float sag = L > 1.2f && S[thin] < .5f ? (float)(rnd.NextDouble() * 2 - 1) * Mathf.Min(.007f, .0025f * L) : 0;
            float twist = L > 1.2f ? (float)(rnd.NextDouble() * 2 - 1) * .004f : 0;

            var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
            void Face(int axis, float sign)
            {
                int a = (axis + 1) % 3, b = (axis + 2) % 3;
                float[] ca = a == 0 ? cx : a == 1 ? cy : cz, cb = b == 0 ? cx : b == 1 ? cy : cz;
                int start = verts.Count;
                for (int i = 0; i < ca.Length; i++)
                    for (int j = 0; j < cb.Length; j++)
                    {
                        var p = Vector3.zero; p[axis] = sign * h[axis]; p[a] = ca[i]; p[b] = cb[j];
                        var inner = new Vector3(Mathf.Clamp(p.x, -h.x + r, h.x - r), Mathf.Clamp(p.y, -h.y + r, h.y - r), Mathf.Clamp(p.z, -h.z + r, h.z - r));
                        if (h.x <= r) inner.x = 0; if (h.y <= r) inner.y = 0; if (h.z <= r) inner.z = 0;
                        var nrm = (p - inner); nrm = nrm.sqrMagnitude > 1e-12f ? nrm.normalized : Vector3.zero; if (nrm == Vector3.zero) nrm[axis] = sign;
                        float rad = Mathf.Min(r, Mathf.Min(h.x, Mathf.Min(h.y, h.z)));
                        var q = inner + nrm * rad;
                        // keep flat faces exactly on the box where the radius had to shrink
                        q = Vector3.Max(Vector3.Min(q, h), -h);
                        float u = (q[longAxis] / Mathf.Max(h[longAxis], 1e-4f));      // -1..1 along the long axis
                        q[thin] += sag * (1 - u * u);
                        int side = 3 - longAxis - thin; q[side] += twist * u * (q[thin] / Mathf.Max(h[thin], 1e-4f));
                        verts.Add(new Vector3(q.x / S.x, q.y / S.y, q.z / S.z));
                        // normals into unit space: the inverse transpose of the 1/S scale is S
                        norms.Add(new Vector3(nrm.x * S.x, nrm.y * S.y, nrm.z * S.z).normalized);
                        uvs.Add(new Vector2((ca[i] / (h[a] * 2)) + .5f, (cb[j] / (h[b] * 2)) + .5f));
                    }
                for (int i = 0; i < ca.Length - 1; i++)
                    for (int j = 0; j < cb.Length - 1; j++)
                    {
                        int v0 = start + i * cb.Length + j, v1 = v0 + 1, v2 = v0 + cb.Length, v3 = v2 + 1;
                        foreach (var tri in new[] { new[] { v0, v1, v3 }, new[] { v0, v3, v2 } })
                        {
                            // Unity takes Cross(B - A, C - A) as a triangle's front: keep triangles whose front faces out, flip the rest.
                            var A = Vector3.Scale(verts[tri[0]], S); var B = Vector3.Scale(verts[tri[1]], S); var C = Vector3.Scale(verts[tri[2]], S);
                            var nn = Vector3.Cross(B - A, C - A); var want = Vector3.zero; want[axis] = sign;
                            if (Vector3.Dot(nn, want) > 0) tris.AddRange(tri); else tris.AddRange(new[] { tri[0], tri[2], tri[1] });
                        }
                    }
            }
            for (int axis = 0; axis < 3; axis++) { Face(axis, 1); Face(axis, -1); }
            var mesh = new Mesh { name = t.name + " softened" };
            if (verts.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts); mesh.SetNormals(norms); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }

        // Warm daylight through each ocean-wall window, laid on the floor along the light's direction.
        static int WindowLight()
        {
            var old = GameObject.Find(LightRoot); if (old) Object.DestroyImmediate(old);
            var shader = Shader.Find("Sea Glass/Window Light"); if (!shader) { Debug.LogWarning("Window Light shader missing"); return 0; }
            var root = new GameObject(LightRoot).transform;
            string matPath = Folder + "/Window daylight.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); }
            mat.SetColor("_Colour", new Color(1f, .9f, .72f)); mat.SetFloat("_Strength", .6f); mat.SetFloat("_Soft", .03f);
            EditorUtility.SetDirty(mat);

            float floor = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Studio floorboard").Select(r => r.bounds.max.y).DefaultIfEmpty(.1f).Max();
            // Daylight from the sea side, falling steeply into the room, angled a little along the wall with the sun.
            var sun = RenderSettings.sun ? RenderSettings.sun.transform.forward : new Vector3(.6f, -.7f, .2f);
            var dir = new Vector3(1, -1.05f, Mathf.Clamp(sun.z / Mathf.Max(Mathf.Abs(sun.y), .2f), -.35f, .35f)).normalized;
            var centres = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Window centre").Select(r => r.bounds).ToList();
            int n = 0;
            foreach (var b in centres)
            {
                var sills = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Window sill trim" && Mathf.Abs(r.bounds.center.z - b.center.z) < .5f).Select(r => r.bounds).OrderBy(s => s.center.y).ToList();
                if (sills.Count < 2) continue;
                float bottom = sills[0].max.y, top = sills[1].min.y, x = b.max.x;
                float zLo = b.center.z - 1.69f, zHi = b.center.z + 1.69f;
                Vector3 Hit(float y, float z) { var p = new Vector3(x, y, z); float t = (floor + .004f - p.y) / dir.y; return p + dir * t; }
                var quad = new[] { Hit(bottom, zLo), Hit(bottom, zHi), Hit(top, zLo), Hit(top, zHi) };
                var mesh = new Mesh { name = "Window light " + n };
                mesh.SetVertices(quad); mesh.SetUVs(0, new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });
                mesh.SetTriangles(new[] { 0, 2, 1, 1, 2, 3 }, 0); mesh.RecalculateNormals();
                if (mesh.normals[0].y < 0) { mesh.SetTriangles(new[] { 0, 1, 2, 1, 3, 2 }, 0); mesh.RecalculateNormals(); }
                mesh.RecalculateBounds();
                AssetDatabase.AddObjectToAsset(mesh, AssetDatabase.LoadAssetAtPath<Mesh>(MeshAsset));
                var go = new GameObject("Window light"); go.transform.SetParent(root, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                n++;
            }
            return n;
        }
    }
}
