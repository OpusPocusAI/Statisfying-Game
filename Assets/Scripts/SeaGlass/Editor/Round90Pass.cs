using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 90, from the eighty-sixth review (all three, and one or two in nearly every review since 77: "a pale horizontal band"
    // across the boulders by the water, "like a texture seam", b1, b3, b6, b9). The boulders are faceted, and two things switched
    // whole facets on or off: the low sun lit only a narrow strip of them (tan against purple, with a hard turn between), and a pale
    // sand tint dusted the facets facing up (Painterly Lit _TopTint, 55%, from normals 0.45 up). Together they drew one hard pale
    // strip round the rock. Shore boulder now wraps the light (_LightWrap, a wider lit-to-shade turn, so the light rolls across the
    // facets in steps of shade), drops the top tint and takes a cooler, greyer stone colour (it was a warm pink-beige, 0.8, 0.72, 0.7). Safe to re-run.
    public static class Round90Pass
    {
        [MenuItem("Sea Glass/Art Direction/81 Round 90: no pale band on the boulders")]
        public static void Apply()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Beach/Shore boulder.mat");
            if (!mat) throw new System.InvalidOperationException("Round 90: Shore boulder material missing.");
            var top = mat.GetColor("_TopTint"); top.a = 0; mat.SetColor("_TopTint", top);
            mat.SetFloat("_LightWrap", .6f);
            // The sunlit cut planes turned pale tan against the purple rock, which read as a different material; a cooler, greyer
            // stone keeps a lit plane the same rock in sunlight.
            mat.SetColor("_BaseColor", new Color(.68f, .64f, .74f));
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 90: Shore boulder top tint " + mat.GetColor("_TopTint").a + ", light wrap " + mat.GetFloat("_LightWrap"));
        }
    }
}
