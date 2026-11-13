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
                // Round 121: the slope of the top face minus the slope of the bottom face, each fitted by least squares over that face
                // alone: the plate's bottle-wall bend lifts both faces alike and cancels, and the rounded rim does not count.
                var d = Slope(mesh, 1) - Slope(mesh, -1);
                def.plateThickSide = d.normalized; n++;
                Debug.Log("Round 119: " + def.id + " " + mesh.name + " thick side " + def.plateThickSide.ToString("F4"));
            }
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 119: thick side measured on " + n + " pieces");
        }

        // The least-squares slope (dz/dx, dz/dy) of one face of a plate: side 1 the top (normals up), -1 the bottom.
        internal static Vector2 Slope(Mesh mesh, int side)
        {
            var all = mesh.vertices; var nrm = mesh.normals; var vs = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < all.Length; i++) if (nrm[i].z * side > .92f) vs.Add(all[i]);
            if (vs.Count < 3) return Vector2.zero;
            Vector3 m = Vector3.zero; foreach (var v in vs) m += v; m /= vs.Count;
            float sxx = 0, sxy = 0, syy = 0, sxz = 0, syz = 0;
            foreach (var v in vs) { float x = v.x - m.x, y = v.y - m.y, z = v.z - m.z; sxx += x * x; sxy += x * y; syy += y * y; sxz += x * z; syz += y * z; }
            float det = sxx * syy - sxy * sxy; if (Mathf.Abs(det) < 1e-12f) return Vector2.zero;
            return new Vector2(syy * sxz - sxy * syz, sxx * syz - sxy * sxz) / det;
        }
    }
}
