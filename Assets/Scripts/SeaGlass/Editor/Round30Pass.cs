using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 30, the truck cab and studio repetition from the twenty-eighth review (three reviewers averaged):
    // - Truck: doors that bulge outwards with rolled edges and cargo tubs with flared sides and a rolled rim
    //   (round30.py), replacing the flat door panels and box bins.
    // - Road: low lobed tongues of blown sand spill over both edges of the coast road, breaking its ruler line.
    // - Studio: the plants cycle through four models (the old one, a fern, a round bush, a snake plant) with varied
    //   scale and turn, and every other wicker basket becomes a low basket of folded linen.
    // Safe to re-run.
    public static class Round30Pass
    {
        [MenuItem("Sea Glass/Art Direction/41 Round 30: truck doors and tubs, road drifts, studio variety")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            Truck();
            RoadDrifts();
            StudioVariety();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void Fit(MeshFilter f, Mesh mesh)
        {
            if (!f || !mesh || f.sharedMesh == mesh) return;
            var old = f.sharedMesh; var t = f.transform;
            var ob = old.bounds; var nb = mesh.bounds; var s = t.localScale;
            var ns = new Vector3(s.x * ob.size.x / Mathf.Max(nb.size.x, 1e-4f), s.y * ob.size.y / Mathf.Max(nb.size.y, 1e-4f), s.z * ob.size.z / Mathf.Max(nb.size.z, 1e-4f));
            var oldCentreW = t.TransformPoint(ob.center);
            f.sharedMesh = mesh; t.localScale = ns;
            t.position += oldCentreW - t.TransformPoint(nb.center);
            EditorUtility.SetDirty(f); EditorUtility.SetDirty(t);
        }

        static Material Mat(string name) => AssetDatabase.FindAssets("t:Material " + name).Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(m => m && m.name == name);

        static void Truck()
        {
            var truck = Object.FindAnyObjectByType<UtilityTruck>(); if (!truck) return;
            var door = ModelLibraryImporter.Load("truck_door_r"); var tub = ModelLibraryImporter.Load("truck_tub_r");
            var rail = Mat("Rail cream");
            foreach (var f in truck.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = f.GetComponent<MeshRenderer>(); if (!r) continue;
                if (f.name == "Enamel door") Fit(f, door);
                if (f.name == "GLASS cargo crate" || f.name == "RUBBISH cargo crate")
                {
                    Fit(f, tub);
                    if (rail && r.sharedMaterials.Length < 2) { r.sharedMaterials = new[] { r.sharedMaterials[0], rail }; EditorUtility.SetDirty(r); }
                }
            }
        }

        static void RoadDrifts()
        {
            const string rootName = "ART • road sand drifts";
            var old = GameObject.Find(rootName); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(rootName).transform;
            var road = GameObject.Find("Coast road"); if (!road) return;
            var rb = road.GetComponent<Renderer>().bounds;
            var sand = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Sand road shoulder").Select(r => r.sharedMaterial).FirstOrDefault()
                       ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat");
            var drifts = Enumerable.Range(0, 3).Select(k => ModelLibraryImporter.Load("sand_drift_" + k)).Where(m => m).ToArray();
            if (drifts.Length == 0) return;
            var rng = new System.Random(3030);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            int n = 0;
            for (int side = 0; side < 2; side++)
                for (float z = rb.min.z + R(1, 5); z < rb.max.z - 2; z += R(4f, 10f))
                {
                    if (z > 1 && z < 15) continue;                                   // keep the truck's parking spot clean
                    float edge = side == 0 ? rb.min.x : rb.max.x, inward = side == 0 ? 1 : -1;
                    var g = new GameObject("Sand drift"); g.transform.SetParent(root);
                    g.AddComponent<MeshFilter>().sharedMesh = drifts[rng.Next(drifts.Length)];
                    var mr = g.AddComponent<MeshRenderer>(); mr.sharedMaterial = sand; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    float s = R(.8f, 1.5f);
                    g.transform.localScale = new Vector3(s, s, s * R(.8f, 1.3f));
                    // The long axis runs along the road; the drift sits mostly on the shoulder and tongues onto the tarmac.
                    g.transform.SetPositionAndRotation(new Vector3(edge + inward * R(.05f, .55f), rb.max.y + .004f, z), Quaternion.Euler(0, 90 + R(-18, 18), 0));
                    GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.BatchingStatic);
                    n++;
                }
            Debug.Log("Round 30: " + n + " sand drifts along the road.");
        }

        static void StudioVariety()
        {
            var plants = new[] { "potted_plant", "potted_plant_b", "potted_plant_c", "potted_plant_d" }.Select(ModelLibraryImporter.Load).ToArray();
            var rng = new System.Random(3031);
            if (plants.All(p => p))
            {
                var all = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                    .Where(f => f.sharedMesh && plants.Contains(f.sharedMesh) && f.transform.position.z > 150 && f.transform.position.z < 180)
                    .OrderBy(f => f.transform.position.x * 7 + f.transform.position.z).ToList();
                for (int i = 0; i < all.Count; i++)
                {
                    var f = all[i]; var r = f.GetComponent<Renderer>(); float bottom = r.bounds.min.y;
                    f.sharedMesh = plants[i % plants.Length];
                    var t = f.transform;
                    t.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0);
                    float baseScale = Mathf.Max(.0001f, t.localScale.y);
                    t.localScale = Vector3.one * baseScale * (.85f + (float)rng.NextDouble() * .3f);
                    t.position += Vector3.up * (bottom - r.bounds.min.y);
                    EditorUtility.SetDirty(f); EditorUtility.SetDirty(t);
                }
                Debug.Log("Round 30: " + all.Count + " studio plants varied.");
            }

            var basketB = ModelLibraryImporter.Load("wicker_basket_b");
            if (basketB)
            {
                var baskets = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                    .Where(f => f.sharedMesh && (f.sharedMesh.name == "wicker_basket" || f.sharedMesh == basketB) && f.transform.position.z > 150)
                    .OrderBy(f => f.transform.position.x + f.transform.position.z * 100).ToList();
                for (int i = 0; i < baskets.Count; i += 2)
                {
                    var f = baskets[i]; var r = f.GetComponent<Renderer>(); float bottom = r.bounds.min.y;
                    f.sharedMesh = basketB;
                    f.transform.position += Vector3.up * (bottom - r.bounds.min.y);
                    foreach (Transform c in f.transform) c.gameObject.SetActive(false);   // linen instead of loose glass
                    EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.transform);
                }
            }
        }
    }
}
