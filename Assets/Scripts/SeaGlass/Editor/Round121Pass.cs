using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Round 121, from the hundred-and-seventeenth review (all three: the 05b piece "a thick jelly block"): StudioStation turns the
    // basin piece so its thin edge faces the eye, as round 119 did in the palm. Which way each basin plate thickens is measured here
    // from its mesh (the centre of its outline weighted by thickness) and kept in the catalog. Safe to re-run.
    public static class Round121Pass
    {
        [MenuItem("Sea Glass/Art Direction/120 Round 121: the basin piece's thin edge toward the eye")]
        public static void Apply()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            int n = 0;
            foreach (var def in catalog.glass)
            {
                var mesh = def.BasinMesh; if (!mesh) continue;
                // Round 121: the slope of the top face minus the slope of the bottom face, each fitted by least squares over that face
                // alone: the plate's bottle-wall bend lifts both faces alike and cancels, and the rounded rim does not count.
                var d = Slope(mesh, 1) - Slope(mesh, -1);
                def.basinThickSide = d.normalized; n++;
            }
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Round 121: thick side measured on " + n + " basin pieces");
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
