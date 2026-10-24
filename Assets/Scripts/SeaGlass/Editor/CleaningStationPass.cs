using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 2 (Documentation/ART_DIRECTION.md, section 5.7): the wash station gets a
    // modelled enamel basin, gooseneck tap, scrub brush and hand sprayer (ArtSource/blender/cleaning_station.py).
    public static class CleaningStationPass
    {
        [MenuItem("Sea Glass/Art Direction/2b Cleaning station props")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();

            var station = Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s => s.kind == StationKind.Clean);
            var t = station.transform;
            var enamel = Mat("Assets/Art/CoastalPolish/Basin porcelain.mat");
            var teal = Mat("Assets/Art/ShorelinePolish/Weathered sea teal.mat");
            var brass = Mat("Assets/Art/CoastalPolish/Workbench brass.mat");
            var cream = Mat("Assets/Art/StudioAtmosphere/Cream ceramic.mat");
            var handle = Mat("Assets/Art/GatheringPolish/Honey handle.mat");
            var walnut = Mat("Assets/Art/CoastalPolish/Gallery walnut.mat");
            var bristle = Mat("Assets/Art/StudioAtmosphere/Natural linen.mat");
            var coral = Mat("Assets/Materials/SeaGlass/Coral paint.mat");
            var label = Mat("Assets/Art/CoastalPolish/Workbench labels.mat");
            var charcoal = Mat("Assets/Art/TruckPolish/Soft charcoal.mat");

            // Tabletop surface sits at y = 1.09 under this station.
            var basin = t.Find("Enamel basin").gameObject;
            Replace(basin, ModelLibraryImporter.Load("enamel_basin"), new[] { enamel, teal });
            basin.transform.position = t.position + new Vector3(-0f, 1.09f, 0f);

            var water = t.Find("Basin water").gameObject;
            water.GetComponent<MeshFilter>().sharedMesh = RoundedWaterPlane();
            water.transform.position = t.position + new Vector3(0, 1.235f, 0);
            water.transform.localScale = new Vector3(1.78f, 1f, 1.13f);

            var tap = t.Find("Brass faucet").gameObject;
            Replace(tap, ModelLibraryImporter.Load("gooseneck_tap"), new[] { brass, walnut, cream });
            tap.transform.position = t.position + new Vector3(.55f, 1.09f, .78f);
            tap.transform.rotation = Quaternion.Euler(0, 20, 0);
            tap.transform.localScale = Vector3.one * 1.5f;

            // The modelled basin has its own rolled rim; remove the earlier box rims and floating drain.
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if ((r.name == "Low porcelain basin rim" || r.name == "Basin drain") && Vector3.Distance(r.transform.position, t.position + Vector3.up * 1.2f) < 2f)
                    Object.DestroyImmediate(r.gameObject);

            var equipment = Object.FindFirstObjectByType<EquipmentVisuals>();
            Replace(equipment.brush, ModelLibraryImporter.Load("scrub_brush"), new[] { handle, walnut, bristle }, true);
            Replace(equipment.sprayer, ModelLibraryImporter.Load("hand_sprayer"), new[] { coral, label, teal, charcoal }, true);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static Material Mat(string path) => AssetDatabase.LoadAssetAtPath<Material>(path);

        // Swap a primitive for a modelled mesh at true scale, keeping any collider's world size.
        public static void Replace(GameObject go, Mesh mesh, Material[] materials, bool removeChildren = false)
        {
            var oldScale = go.transform.lossyScale;
            var box = go.GetComponent<BoxCollider>();
            Vector3 worldSize = box ? Vector3.Scale(box.size, oldScale) : Vector3.zero;
            Vector3 worldCentre = box ? go.transform.TransformPoint(box.center) : Vector3.zero;
            if (removeChildren) for (int i = go.transform.childCount - 1; i >= 0; i--) Object.DestroyImmediate(go.transform.GetChild(i).gameObject);
            var parentScale = go.transform.parent ? go.transform.parent.lossyScale : Vector3.one;
            go.transform.localScale = new Vector3(1 / parentScale.x, 1 / parentScale.y, 1 / parentScale.z);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = materials;
            if (box) { box.size = worldSize; box.center = go.transform.InverseTransformPoint(worldCentre); }
            EditorUtility.SetDirty(go);
        }

        // Unit-square water plane with rounded corners (the Basin Water shader expects local -0.5..0.5).
        static Mesh RoundedWaterPlane()
        {
            const string path = "Assets/Art/Models/basin_water_plane.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, path); }
            var pts = new System.Collections.Generic.List<Vector3> { Vector3.zero };
            float rx = .19f / 1.78f, rz = .19f / 1.13f; int seg = 8;
            Vector2[] centres = { new Vector2(.5f - rx, .5f - rz), new Vector2(-.5f + rx, .5f - rz), new Vector2(-.5f + rx, -.5f + rz), new Vector2(.5f - rx, -.5f + rz) };
            for (int c = 0; c < 4; c++)
                for (int s = 0; s <= seg; s++)
                {
                    float a = (c * 90 + s * 90f / seg) * Mathf.Deg2Rad;
                    pts.Add(new Vector3(centres[c].x + Mathf.Cos(a) * rx, 0, centres[c].y + Mathf.Sin(a) * rz));
                }
            var tris = new System.Collections.Generic.List<int>();
            int ring = pts.Count - 1;
            for (int i = 1; i <= ring; i++) { int next = i % ring + 1; tris.AddRange(new[] { 0, next, i }); }
            mesh.Clear(); mesh.SetVertices(pts); mesh.SetTriangles(tris, 0);
            mesh.SetNormals(pts.Select(_ => Vector3.up).ToList()); mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }
    }
}
