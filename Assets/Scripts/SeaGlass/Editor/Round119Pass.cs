using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 119, from the hundred-and-fifteenth review (all three: the palm piece "thick and gummy, more like candy than a wedge"):
    // StudioStation turns the piece in the palm so its thin edge faces the eye. Which way each plate thickens is measured here, from
    // its mesh (the centre of its outline weighted by thickness), and kept in the catalog, as the built game cannot read the meshes.
    // Safe to re-run.
    public static class Round119Pass
    {
        [MenuItem("Sea Glass/Art Direction/118 Round 119: the palm piece's thin edge toward the eye")]
        public static void Apply()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            int n = 0;
            foreach (var def in catalog.glass)
            {
                var mesh = def.PlateMesh; if (!mesh) continue;
                Vector2 d = Vector2.zero; float w = 0;
                foreach (var v in mesh.vertices) { float a = Mathf.Abs(v.z); d += new Vector2(v.x, v.y) * a; w += a; }
                def.plateThickSide = w > 0 ? d / w : Vector2.zero; n++;
                Debug.Log("Round 119: " + def.id + " " + mesh.name + " thick side " + def.plateThickSide.ToString("F4"));
            }
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 119: thick side measured on " + n + " pieces");
        }
    }
}
