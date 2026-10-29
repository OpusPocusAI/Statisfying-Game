using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 19, from the seventeenth independent blind review: studio furniture "floats" with no contact shadow.
    // SSAO was on but faint (intensity 1.1, 0.45 m radius, 30% on direct light); it now darkens contact points
    // clearly. Shader changes this round: SoftFeedback (soap-bubble clumps), BasinWater (deeper middle, crisp
    // window streak), FrostedGlass (stronger refraction). Safe to re-run.
    public static class Round19Pass
    {
        [MenuItem("Sea Glass/Art Direction/32 Round 19: stronger contact shadows")]
        public static void Apply()
        {
            var rd = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
            if (!rd) throw new System.InvalidOperationException("PC_Renderer not found.");
            foreach (var f in rd.rendererFeatures)
            {
                if (!f || f.GetType().Name != "ScreenSpaceAmbientOcclusion") continue;
                var so = new SerializedObject(f);
                so.FindProperty("m_Settings.Intensity").floatValue = 2.2f;
                so.FindProperty("m_Settings.Radius").floatValue = .7f;
                so.FindProperty("m_Settings.DirectLightingStrength").floatValue = .6f;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(f);
            }
            ContactShadows();
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // SSAO alone did not visibly ground the furniture in the review views, so every floor-standing piece in
        // the studio also gets a soft lilac contact-shadow blob on the boards beneath it.
        static void ContactShadows()
        {
            const string rootName = "ART • contact shadows";
            var old = GameObject.Find(rootName); if (old) Object.DestroyImmediate(old);
            if (!AssetDatabase.IsValidFolder("Assets/Art/Round19")) AssetDatabase.CreateFolder("Assets/Art", "Round19");
            string matPath = "Assets/Art/Round19/Contact shadow.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat) { mat = new Material(Shader.Find("Sea Glass/Contact Shadow")); AssetDatabase.CreateAsset(mat, matPath); }
            mat.SetFloat("_Strength", .62f); mat.SetFloat("_Softness", .7f); EditorUtility.SetDirty(mat);
            var studio = new Bounds(new Vector3(-4, 1.5f, 165), new Vector3(23, 4, 23));
            float floorY = 0;
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)) if (r.name == "Studio floorboard") floorY = Mathf.Max(floorY, r.bounds.max.y);
            var root = new GameObject(rootName);
            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            int count = 0;
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var b = r.bounds; if (!studio.Contains(b.center)) continue;
                bool table = r.name == "Timber tabletop" || r.name == "Bench leg";
                if (b.min.y > floorY + .08f || b.min.y < floorY - (table ? .2f : .1f)) continue;
                if (b.size.y < .15f || b.size.y > 2.6f || (!table && (b.size.x > 3.2f || b.size.z > 3.2f))) continue;
                string n = r.name.ToLower(); if (n.Contains("floor") || n.Contains("rug") || n.Contains("wall")) continue;
                var go = new GameObject("Contact shadow"); go.transform.SetParent(root.transform);
                go.transform.position = new Vector3(b.center.x, floorY + .004f, b.center.z);
                go.transform.rotation = Quaternion.Euler(90, 0, 0);
                go.transform.localScale = new Vector3(b.size.x * 1.45f + .35f, b.size.z * 1.45f + .35f, 1);   // reaches past the footprint, where it shows
                go.AddComponent<MeshFilter>().sharedMesh = quad;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
                count++;
            }
            Debug.Log("Round 19: " + count + " contact shadows under studio furniture.");
        }
    }
}
