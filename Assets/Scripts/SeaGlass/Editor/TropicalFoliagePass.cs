using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Plants and foliage pass (owner, November 15: Tchia is the art target). Under the new tropical daylight the foliage
    // read dusty sage and blue-grey. Tchia's greenery is lush: deep emerald in the shade, warm yellow-green where the sun
    // catches it. Every foliage material moves that way; the lit/tip materials go yellow-green, the base/shade ones emerald.
    // Repeatable (it only sets colours).
    public static class TropicalFoliagePass
    {
        static readonly (string path, Color colour)[] Colours =
        {
            // Grass: emerald bases, sunny yellow-green tips.
            ("Assets/Art/Round21/Grass base green.mat", new Color(.30f, .58f, .26f)),
            ("Assets/Art/Round21/Grass tip green.mat", new Color(.62f, .78f, .32f)),
            ("Assets/Art/Round21/Grass base olive.mat", new Color(.44f, .62f, .26f)),
            ("Assets/Art/ShorelinePolish/Dune grass shadow.mat", new Color(.18f, .46f, .24f)),
            ("Assets/Art/ShorelinePolish/Dune grass sage.mat", new Color(.6f, .76f, .32f)),
            // Shrubs and stems.
            ("Assets/Art/Round3/Shrub dark.mat", new Color(.12f, .38f, .19f)),
            ("Assets/Art/Round3/Shrub lit.mat", new Color(.28f, .55f, .19f)),
            ("Assets/Art/StudioFurniture/Stem green.mat", new Color(.2f, .5f, .22f)),
            // Canopies, palms, island cover.
            ("Assets/Materials/SeaGlass/Blue green plants.mat", new Color(.12f, .44f, .25f)),
            ("Assets/Art/CoastalPolish/Sage canopy.mat", new Color(.2f, .54f, .25f)),
            ("Assets/Art/Vegetation/Pine foliage.mat", new Color(.18f, .5f, .26f)),
            ("Assets/Art/Round10/Palm frond.mat", new Color(.2f, .52f, .22f)),   // its sunny top tint is set by TropicalLayeredPlantingPass
            ("Assets/Art/Round3/Island trees.mat", new Color(.24f, .6f, .24f)),
            ("Assets/Art/Round3/Island grass.mat", new Color(.48f, .74f, .28f)),
            ("Assets/Art/Round3/Sea stack moss.mat", new Color(.4f, .64f, .26f)),
        };

        [MenuItem("Sea Glass/Art Direction/Plants and foliage: tropical greens")]
        public static void Apply()
        {
            int n = 0;
            foreach (var (path, colour) in Colours)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!m) { Debug.LogWarning("Tropical foliage pass: missing " + path); continue; }
                m.SetColor("_BaseColor", colour); EditorUtility.SetDirty(m); n++;
            }
            GrassyRidge();
            Planting();
            Sway();
            AssetDatabase.SaveAssets();
            Debug.Log("Tropical foliage pass: " + n + " materials recoloured.");
        }
        // Sway: grass, dune grass, shrubs and flower stems move in the wind (the painterly shader's _Sway). A swaying plant bends by
        // its height above its own origin, so those renderers leave static batching, which would bake them into world space.
        static readonly (string path, float sway)[] Swaying =
        {
            ("Assets/Art/Round21/Grass base green.mat", .07f), ("Assets/Art/Round21/Grass tip green.mat", .07f), ("Assets/Art/Round21/Grass base olive.mat", .07f),
            ("Assets/Art/ShorelinePolish/Dune grass shadow.mat", .07f), ("Assets/Art/ShorelinePolish/Dune grass sage.mat", .07f),
            ("Assets/Art/Round3/Shrub dark.mat", .03f), ("Assets/Art/Round3/Shrub lit.mat", .03f), ("Assets/Art/StudioFurniture/Stem green.mat", .06f),
        };
        static void Sway()
        {
            var mats = new System.Collections.Generic.HashSet<Material>();
            foreach (var (path, sway) in Swaying)
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(path); if (!m) continue;
                m.SetFloat("_Sway", sway); EditorUtility.SetDirty(m); mats.Add(m);
            }
            int n = 0; Scene scene = default;
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!r.sharedMaterials.Any(mats.Contains)) continue;
                var flags = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
                if ((flags & StaticEditorFlags.BatchingStatic) != 0) { GameObjectUtility.SetStaticEditorFlags(r.gameObject, flags & ~StaticEditorFlags.BatchingStatic); n++; }
                scene = r.gameObject.scene;
            }
            if (scene.IsValid()) { EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
            Debug.Log("Tropical foliage pass: " + n + " swaying plants taken out of static batching.");
        }

        // Planting: Tchia's land behind the beach is thick with growth. Patchy clumps of shrubs and tall grass now cover the green
        // ridge behind the road, gathered into drifts by a noise field rather than spread evenly. Rebuilt from scratch each run.
        static void Planting()
        {
            const string rootName = "ART • lush planting";
            var old = GameObject.Find(rootName); if (old) Object.DestroyImmediate(old);
            var ridge = GameObject.Find("Inland dune ridge"); if (!ridge) return;
            var all = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var shrub = all.FirstOrDefault(x => x.name == "Ridge shrub") ?? all.FirstOrDefault(x => x.name == "Coastal shrub");
            var grass = all.FirstOrDefault(x => x.name == "Ridge grass") ?? all.FirstOrDefault(x => x.name == "Dune grass cluster");
            if (!shrub || !grass) { Debug.LogWarning("Tropical foliage pass: no shrub or grass to plant from"); return; }
            var collider = ridge.GetComponent<MeshCollider>(); bool temp = !collider;
            if (temp) { collider = ridge.AddComponent<MeshCollider>(); collider.sharedMesh = ridge.GetComponent<MeshFilter>().sharedMesh; }
            Physics.SyncTransforms();
            var root = new GameObject(rootName).transform;
            var rng = new System.Random(1115); float R() => (float)rng.NextDouble();
            int placed = 0;
            for (int i = 0; i < 2600 && placed < 420; i++)
            {
                var p = new Vector3(21.5f + R() * 30f, 20, -14 + R() * 190);
                // Drifts: plant only where a broad noise field is high, denser toward its peaks.
                float drift = Mathf.PerlinNoise(p.x * .06f + 3.1f, p.z * .06f + 7.7f);
                if (drift < .42f || R() > (drift - .42f) * 2.6f) continue;
                if (!collider.Raycast(new Ray(p, Vector3.down), out var hit, 40)) continue;
                if (hit.normal.y < .86f) continue;   // the sandy steep faces stay bare
                bool isShrub = R() < .58f;
                var src = isShrub ? shrub : grass;
                var go = new GameObject(isShrub ? "Lush shrub" : "Lush grass");
                go.transform.SetParent(root);
                go.transform.SetPositionAndRotation(hit.point - Vector3.up * .04f, Quaternion.Euler(0, R() * 360, 0));
                float s = isShrub ? Mathf.Lerp(.9f, 1.9f, R() * drift) : Mathf.Lerp(1f, 1.7f, R());
                go.transform.localScale = src.transform.lossyScale * s;
                go.AddComponent<MeshFilter>().sharedMesh = src.GetComponent<MeshFilter>().sharedMesh;
                go.AddComponent<MeshRenderer>().sharedMaterials = src.sharedMaterials;
                placed++;
            }
            if (temp) Object.DestroyImmediate(collider);
            EditorSceneManager.MarkSceneDirty(ridge.scene); EditorSceneManager.SaveScene(ridge.scene);
            Debug.Log("Tropical foliage pass: planted " + placed + " clumps on the ridge.");
        }

        // The big inland ridge behind the road was bare sand. Tchia keeps sand to the beach and turns the land behind it green, so the
        // ridge gets its own copy of the beach sand with grass on every gentle, upward-facing slope; steeper faces stay sandy.
        static void GrassyRidge()
        {
            var ridge = GameObject.Find("Inland dune ridge"); if (!ridge) { Debug.LogWarning("Tropical foliage pass: no inland ridge"); return; }
            const string path = "Assets/Art/Vegetation/Grassy inland ridge.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/Apricot sand.mat")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_TopTint", new Color(.24f, .5f, .2f, .92f)); m.SetFloat("_TopStart", .8f);
            EditorUtility.SetDirty(m);
            var rend = ridge.GetComponent<Renderer>(); rend.sharedMaterial = m; EditorUtility.SetDirty(rend);
            EditorSceneManager.MarkSceneDirty(ridge.scene); EditorSceneManager.SaveScene(ridge.scene);
        }
    }
}
