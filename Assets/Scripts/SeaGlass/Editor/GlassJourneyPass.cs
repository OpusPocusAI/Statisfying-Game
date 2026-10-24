using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 4 (Documentation/ART_DIRECTION.md, sections 5.8 and 5.9): lined wooden sorting
    // trays you can read across the room, and stained-glass mosaics with raised lead came that also
    // outlines each empty socket.
    public static class GlassJourneyPass
    {
        const string Folder = "Assets/Art/GlassJourney";

        [MenuItem("Sea Glass/Art Direction/4 Sorting trays and mosaics")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "GlassJourney");
            ModelLibraryImporter.ImportAll();
            Trays();
            Came();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static Material Painterly(string path, Color colour, float smoothness, float gradient = 0)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Sea Glass/Painterly Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", colour); m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_GradientStrength", gradient); m.SetFloat("_Variation", .06f);
            EditorUtility.SetDirty(m); return m;
        }

        // ------------------------------------------------------------------ trays
        static void Trays()
        {
            var tray = ModelLibraryImporter.Load("sorting_tray");
            var timber = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CoastalPolish/Tray honey timber.mat");
            timber.SetColor("_BaseColor", new Color(.66f, .44f, .26f)); EditorUtility.SetDirty(timber);
            foreach (var t in Object.FindObjectsByType<SortingTray>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                // A pale felt lining tinted with the tray's glass colour reads from across the room.
                var glass = GameCatalog.Palette[(int)t.color];
                var lining = Painterly(Folder + "/Lining " + t.color + ".mat", Color.Lerp(glass, new Color(.95f, .92f, .84f), .5f), .05f);
                CleaningStationPass.Replace(t.gameObject, tray, new[] { timber, lining });
            }
            // The modelled tray has its own rim and colour; remove the old strips and swatches.
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToArray())
                if (r.name == "Fine timber tray edge" || r.name == "Tray colour swatch" || r.name == "Sorting tray wooden rim") Object.DestroyImmediate(r.gameObject);
        }

        // ------------------------------------------------------------------ lead came
        static void Came()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            var lead = Painterly(Folder + "/Lead came.mat", new Color(.2f, .2f, .24f), .55f);
            lead.SetFloat("_Rim", .6f);
            foreach (var panel in Object.FindObjectsByType<MosaicPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var name = "Lead came";
                var old = panel.transform.Find(name); if (old) Object.DestroyImmediate(old.gameObject);
                var verts = new List<Vector3>(); var normals = new List<Vector3>(); var tris = new List<int>();
                foreach (var socket in panel.sockets)
                {
                    var def = catalog.glass.First(g => g.id == socket.glassId);
                    var src = def.mesh.vertices; int n = src.Length / 2;
                    var piece = socket.piece.transform;
                    // Authored outline (mesh stores it shrunk by 0.975 around the centre).
                    var outline = Enumerable.Range(0, n).Select(i => new Vector2(src[i].x, src[i].y) / .975f).ToArray();
                    for (int e = 0; e < n; e++)
                        Strip(outline[e], outline[(e + 1) % n], piece, panel.transform, verts, normals, tris);
                }
                var mesh = new Mesh { name = panel.name + " lead came" };
                mesh.indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
                mesh.SetVertices(verts); mesh.SetNormals(normals); mesh.SetTriangles(tris, 0); mesh.RecalculateBounds();
                string path = Folder + "/" + panel.name + " lead came.asset";
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing) { existing.Clear(); EditorUtility.CopySerialized(mesh, existing); mesh = existing; } else AssetDatabase.CreateAsset(mesh, path);
                var go = new GameObject(name);
                go.transform.SetParent(panel.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = lead;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        // A half-round strip along one outline edge, in the piece's local space, stored in panel space.
        // Ends extend by half the width so neighbouring strips overlap at corners.
        static void Strip(Vector2 a, Vector2 b, Transform piece, Transform panel, List<Vector3> verts, List<Vector3> normals, List<int> tris)
        {
            const float halfWidth = .045f, height = .05f, baseZ = -.01f; const int segs = 6;
            var dir = (b - a).normalized; var side = new Vector2(-dir.y, dir.x);
            a -= dir * halfWidth; b += dir * halfWidth;
            int start = verts.Count;
            for (int s = 0; s <= segs; s++)
            {
                float ang = Mathf.PI * s / segs;                       // 0..pi across the strip
                var offset = side * Mathf.Cos(ang) * halfWidth;
                float z = baseZ - Mathf.Sin(ang) * height;             // towards the viewer is -z
                var nLocal = new Vector3(side.x * Mathf.Cos(ang), side.y * Mathf.Cos(ang), -Mathf.Sin(ang));
                foreach (var p in new[] { a, b })
                {
                    var world = piece.TransformPoint(new Vector3(p.x + offset.x, p.y + offset.y, z));
                    verts.Add(panel.InverseTransformPoint(world));
                    normals.Add(panel.InverseTransformDirection(piece.TransformDirection(nLocal)).normalized);
                }
            }
            for (int s = 0; s < segs; s++)
            {
                int i = start + s * 2;
                tris.AddRange(new[] { i, i + 2, i + 1, i + 1, i + 2, i + 3 });
                tris.AddRange(new[] { i, i + 1, i + 2, i + 1, i + 3, i + 2 });   // double-sided: winding depends on edge direction
            }
        }
    }
}
