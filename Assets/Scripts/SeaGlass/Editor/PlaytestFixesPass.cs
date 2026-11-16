using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaGlass.Editor
{
    // Playtest fixes (owner playtest checklist, November 16). The truck's undercarriage is in TruckFusionPass; this pass does
    // the rest:
    // - Finds rest on the sand: "glass still seems to float when undug". Every find's origin sat 0.16 above the beach (the sand
    //   shelf was lowered after the finds were placed) and a piece lying on the surface was lifted a further fixed 0.13, so it
    //   hovered about 0.27 up. Each find's origin now sits on the sand under it, and its resting height (ShoreItem.rest) puts
    //   the bottom of the piece just into the sand. GroundingPass then re-seats the dig mounds and contact shadows.
    // - Butterflies stay above the ground: "they fly too much through the ground". Each gets the visible ground height around
    //   its flowers baked (dunes, rocks, plants), and AmbientCreature keeps it above that.
    // - Seabirds: "an odd white flashing around the bird". The white body caught the full warm rim light, which flickers along
    //   the flapping wing edges against the bright sky; the rim and the sheen are turned down, and no brush noise.
    // - Light rays: soft shafts of daylight through each ocean-wall window (Sea Glass/Light Shaft), over the floor patches.
    // Repeatable: finds are set from the ground, not offset again; the bake and shafts are rebuilt.
    public static class PlaytestFixesPass
    {
        const string ShaftRoot = "ART • window light shafts", Folder = "Assets/Art/StudioInterior", ShaftAsset = Folder + "/Window light shafts.asset";

        [MenuItem("Sea Glass/Art Direction/Playtest fixes: finds on the sand, butterflies, seabirds, light rays")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            Physics.SyncTransforms();
            int seated = SeatFinds();
            GroundingPass.Apply();                       // mounds and contact shadows follow the finds
            scene = EditorSceneManager.GetActiveScene();
            Physics.SyncTransforms();
            int flies = Butterflies();
            Seabirds();
            int shafts = Shafts();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Playtest fixes: {seated} finds seated on the sand, {flies} butterflies given a ground bake, seabird rim softened, {shafts} window light shafts");
        }

        static bool Ground(Collider c) => !c.isTrigger && (c.name == "Beach surface" || c.name == "Continuous sand shelf" || c.name == "Inland dune ridge" || c.name.ToLower().Contains("sand") || c.name.ToLower().Contains("road"));

        static int SeatFinds()
        {
            int n = 0;
            foreach (var i in Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var p = i.transform.position; float ground = float.NaN;
                foreach (var h in Physics.RaycastAll(new Ray(p + Vector3.up * 2, Vector3.down), 4).OrderBy(h => h.distance))
                    if (Ground(h.collider) && !h.collider.transform.IsChildOf(i.transform)) { ground = h.point.y; break; }
                if (float.IsNaN(ground) || Mathf.Abs(ground - p.y) > .6f) continue;
                i.transform.position = new Vector3(p.x, ground, p.z);
                if (i.visual)
                {
                    // the piece's lowest point relative to its pivot, then sink glass 1.5 cm and rubbish 2.5 cm into the sand
                    var r = i.visual.GetComponentInChildren<Renderer>(true);
                    var lp = i.visual.localPosition; i.visual.localPosition = Vector3.zero;
                    float below = i.transform.position.y - r.bounds.min.y;
                    i.visual.localPosition = lp;
                    i.rest = below - (i.glass ? .015f : .025f);
                    EditorUtility.SetDirty(i.visual);
                }
                EditorUtility.SetDirty(i); n++;
            }
            return n;
        }

        static int Butterflies()
        {
            var flies = Object.FindObjectsByType<AmbientCreature>(FindObjectsSortMode.None).Where(c => c.kind == AmbientCreature.Kind.Butterfly).ToList();
            if (flies.Count == 0) return 0;
            // Everything visible near the flowers takes part, including plants without colliders (temporary mesh colliders).
            var temp = new List<MeshCollider>();
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || r.GetComponent<AmbientCreature>() || r.GetComponentInParent<ShoreItem>() || r.GetComponent<Collider>() || r.shadowCastingMode == ShadowCastingMode.Off && r.name.Contains("shadow")) continue;
                var mf = r.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh) continue;
                var b = r.bounds;
                if (!flies.Any(f => b.min.x < f.home.x + 5 && b.max.x > f.home.x - 5 && b.min.z < f.home.z + 5 && b.max.z > f.home.z - 5)) continue;
                var mc = r.gameObject.AddComponent<MeshCollider>(); mc.sharedMesh = mf.sharedMesh; temp.Add(mc);
            }
            Physics.SyncTransforms();
            foreach (var f in flies)
            {
                const int N = 17; float half = f.radius.x + 1.6f; var heights = new float[N * N];
                for (int j = 0; j < N; j++) for (int i = 0; i < N; i++)
                {
                    var q = f.home + new Vector3(-half + 2 * half * i / (N - 1), 0, -half + 2 * half * j / (N - 1));
                    float best = f.home.y - 1;
                    foreach (var h in Physics.RaycastAll(new Ray(q + Vector3.up * 4, Vector3.down), 8))
                        if (!h.collider.isTrigger && !(h.collider is CharacterController) && h.point.y < f.home.y + 1.6f) best = Mathf.Max(best, h.point.y);   // not palm crowns
                    heights[j * N + i] = best;
                }
                f.groundHeights = heights; f.groundN = N; f.groundHalf = half;
                // start in the air above the ground at its spot
                var p = f.transform.position; float g = heights[(N / 2) * N + N / 2];
                if (p.y < g + .4f) f.transform.position = new Vector3(p.x, g + .6f, p.z);
                EditorUtility.SetDirty(f);
            }
            foreach (var mc in temp) Object.DestroyImmediate(mc);
            return flies.Count;
        }

        static void Seabirds()
        {
            foreach (var name in new[] { "Seabird white", "Seabird wing tip", "Seabird beak" })
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Ambient/" + name + ".mat"); if (!m) continue;
                m.SetFloat("_Rim", .15f); m.SetFloat("_Smoothness", .04f); m.SetFloat("_Brush", 0); m.SetFloat("_Variation", 0);
                EditorUtility.SetDirty(m);
            }
        }

        // Daylight shafts: for each window, the box swept from the window opening along the daylight direction down to the floor.
        static int Shafts()
        {
            var old = GameObject.Find(ShaftRoot); if (old) Object.DestroyImmediate(old);
            var shader = Shader.Find("Sea Glass/Light Shaft"); if (!shader) { Debug.LogWarning("Light Shaft shader missing"); return 0; }
            string matPath = Folder + "/Window light shaft.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); }
            var sun = RenderSettings.sun ? RenderSettings.sun.transform.forward : new Vector3(.6f, -.7f, .2f);
            var dir = new Vector3(1, -1.05f, Mathf.Clamp(sun.z / Mathf.Max(Mathf.Abs(sun.y), .2f), -.35f, .35f)).normalized;   // as the floor patches
            mat.SetColor("_Colour", new Color(1f, .87f, .64f)); mat.SetFloat("_Density", .28f); mat.SetVector("_LightDir", dir);
            EditorUtility.SetDirty(mat);

            var store = AssetDatabase.LoadAssetAtPath<Mesh>(ShaftAsset);
            if (store) foreach (var a in AssetDatabase.LoadAllAssetsAtPath(ShaftAsset).Where(a => a != store)) Object.DestroyImmediate(a, true);
            else { store = new Mesh { name = "Window light shafts" }; AssetDatabase.CreateAsset(store, ShaftAsset); }

            float floor = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Studio floorboard").Select(r => r.bounds.max.y).DefaultIfEmpty(.1f).Max();
            var root = new GameObject(ShaftRoot).transform; int n = 0;
            foreach (var b in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Window centre").Select(r => r.bounds).ToList())
            {
                var sills = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Window sill trim" && Mathf.Abs(r.bounds.center.z - b.center.z) < .5f).Select(r => r.bounds).OrderBy(s => s.center.y).ToList();
                if (sills.Count < 2) continue;
                float bottom = sills[0].max.y, top = sills[1].min.y, x = b.max.x, zLo = b.center.z - 1.69f, zHi = b.center.z + 1.69f;
                var O = new Vector3(x, bottom, zLo); var U = new Vector3(0, 0, zHi - zLo); var V = new Vector3(0, top - bottom, 0);
                var D = dir * ((floor - top) / dir.y);           // long enough that the head's light reaches the floor
                var M = new Matrix4x4(U, V, D, new Vector4(O.x, O.y, O.z, 1)); var Mi = M.inverse;
                Vector4 row(int k) => Mi.GetRow(k);

                var corners = new Vector3[8];
                for (int k = 0; k < 8; k++) corners[k] = O + U * (k & 1) + V * ((k >> 1) & 1) + D * ((k >> 2) & 1);
                var centre = corners.Aggregate(Vector3.zero, (s, c) => s + c) / 8;
                int[][] faces = { new[] { 0, 1, 3, 2 }, new[] { 4, 6, 7, 5 }, new[] { 0, 4, 5, 1 }, new[] { 2, 3, 7, 6 }, new[] { 0, 2, 6, 4 }, new[] { 1, 5, 7, 3 } };
                var tris = new List<int>();
                foreach (var f in faces)
                {
                    var nrm = Vector3.Cross(corners[f[1]] - corners[f[0]], corners[f[2]] - corners[f[0]]);
                    var fc = (corners[f[0]] + corners[f[1]] + corners[f[2]] + corners[f[3]]) / 4;
                    // Front faces wind clockwise seen from outside, which (left-handed) makes the cross product point outward. The
                    // shader draws the inside (Cull Front), so every face is wound to face out.
                    bool outward = Vector3.Dot(nrm, fc - centre) > 0;
                    var q = outward ? f : new[] { f[0], f[3], f[2], f[1] };
                    tris.AddRange(new[] { q[0], q[1], q[2], q[0], q[2], q[3] });
                }
                var mesh = new Mesh { name = "Window light shaft " + n };
                mesh.SetVertices(corners); mesh.SetTriangles(tris, 0);
                mesh.SetUVs(2, Enumerable.Repeat(row(0), 8).ToList()); mesh.SetUVs(3, Enumerable.Repeat(row(1), 8).ToList()); mesh.SetUVs(4, Enumerable.Repeat(row(2), 8).ToList());
                mesh.RecalculateBounds();
                AssetDatabase.AddObjectToAsset(mesh, store);
                var go = new GameObject("Window light shaft"); go.transform.SetParent(root, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                mr.lightProbeUsage = LightProbeUsage.Off; mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
                n++;
            }
            return n;
        }
    }
}
