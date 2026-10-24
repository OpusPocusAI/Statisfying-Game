using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Art direction pass 11 (Documentation/ART_DIRECTION.md, section 5.11): the truck loses its last
    // primitive boxes (chassis, cargo crates, seats), and its cargo shows the actual tumbled glass and
    // rubbish models instead of spheres. Models: ArtSource/blender/truck_parts.py.
    public static class TruckRefinePass
    {
        [MenuItem("Sea Glass/Art Direction/11 Truck refinement")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            ModelLibraryImporter.ImportAll();
            var truck = Object.FindFirstObjectByType<UtilityTruck>();
            var all = truck.GetComponentsInChildren<MeshRenderer>(true);

            Swap(all, "Painted chassis", "truck_chassis");
            Swap(all, "GLASS cargo crate", "truck_cargo_tub");
            Swap(all, "RUBBISH cargo crate", "truck_cargo_tub");
            Swap(all, "Seat", "truck_seat");
            Swap(all, "Seat back", "truck_seat_back");
            var cushion = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/TruckPolish/Faded coral.mat");
            foreach (var r in all.Where(r => r.name == "Seat" || r.name == "Seat back")) r.sharedMaterial = cushion;

            // Cargo: the real glass pebbles (in their colours) and the five rubbish models.
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            for (int i = 0; i < truck.glassCargo.Length; i++)
            {
                var t = truck.glassCargo[i]; var def = catalog.glass[(i * 7) % catalog.glass.Length];
                Cargo(t, def.VisualMesh, .3f, Quaternion.Euler(80 + i * 13 % 20, i * 53, i * 17 % 30));
            }
            string[] rubbish = { "rubbish_can", "rubbish_bottle", "rubbish_crushed", "rubbish_driftwood", "rubbish_can" };
            var metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Dulled aluminium.mat");
            var coral = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Faded coral canvas.mat");
            var teal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Weathered sea teal.mat");
            var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ShorelinePolish/Bleached driftwood.mat");
            var bottle = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Beach/Sea-blue plastic.mat");
            for (int i = 0; i < truck.rubbishCargo.Length; i++)
            {
                var t = truck.rubbishCargo[i]; int k = i % 5;
                Cargo(t, ModelLibraryImporter.Load(rubbish[k]), .34f, Quaternion.Euler(i * 31 % 40, i * 67, i * 23 % 50));
                t.GetComponent<Renderer>().sharedMaterials = new[] { k == 1 ? bottle : k == 3 ? wood : metal, k % 2 == 0 ? coral : teal, metal };
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        static void Swap(MeshRenderer[] all, string name, string mesh)
        {
            foreach (var r in all.Where(r => r.name == name))
            {
                var m = r.sharedMaterials;
                CleaningStationPass.Replace(r.gameObject, ModelLibraryImporter.Load(mesh), new[] { m[0] });
            }
        }

        // Fit a mesh into the slot's world footprint, centred on the slot.
        static void Cargo(Transform t, Mesh mesh, float size, Quaternion rotation)
        {
            var f = t.GetComponent<MeshFilter>(); f.sharedMesh = mesh;
            float extent = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z);
            var parent = t.parent ? t.parent.lossyScale : Vector3.one;
            t.localScale = new Vector3(size / extent / parent.x, size / extent / parent.y, size / extent / parent.z);
            t.localRotation = rotation;
            EditorUtility.SetDirty(t);
        }
    }
}
