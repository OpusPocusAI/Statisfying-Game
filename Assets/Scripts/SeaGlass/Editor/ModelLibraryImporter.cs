using System.IO;
using UnityEditor;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Imports meshes modelled by the Blender scripts in ArtSource/blender (see sg_export.py).
    // Blender Z-up right-handed -> Unity Y-up left-handed: (x, y, z) -> (x, z, y), winding flipped.
    public static class ModelLibraryImporter
    {
        public const string SourceFolder = "ArtSource/blender/out";
        public const string TargetFolder = "Assets/Art/Models";

        [System.Serializable] class Submesh { public int[] indices; }
        [System.Serializable] class ModelData { public string name; public string[] materials; public float[] positions; public float[] normals; public Submesh[] submeshes; }

        [MenuItem("Sea Glass/Art Direction/Import Blender models")]
        public static void ImportAll()
        {
            if (!Directory.Exists(SourceFolder)) throw new DirectoryNotFoundException(SourceFolder);
            if (!AssetDatabase.IsValidFolder(TargetFolder)) AssetDatabase.CreateFolder("Assets/Art", "Models");
            int count = 0;
            foreach (var file in Directory.GetFiles(SourceFolder, "*.json")) { Import(file); count++; }
            AssetDatabase.SaveAssets();
            Debug.Log("Imported " + count + " Blender models into " + TargetFolder);
        }

        public static Mesh Load(string name) => AssetDatabase.LoadAssetAtPath<Mesh>(TargetFolder + "/" + name + ".asset");

        public static Mesh Import(string file)
        {
            var data = JsonUtility.FromJson<ModelData>(File.ReadAllText(file));
            int n = data.positions.Length / 3;
            var vertices = new Vector3[n];
            var normals = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                vertices[i] = new Vector3(data.positions[i * 3], data.positions[i * 3 + 2], data.positions[i * 3 + 1]);
                normals[i] = new Vector3(data.normals[i * 3], data.normals[i * 3 + 2], data.normals[i * 3 + 1]).normalized;
            }
            string path = TargetFolder + "/" + data.name + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear();
            mesh.name = data.name;
            mesh.indexFormat = n > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.subMeshCount = data.submeshes.Length;
            for (int s = 0; s < data.submeshes.Length; s++)
            {
                var src = data.submeshes[s].indices;
                var tris = new int[src.Length];
                for (int i = 0; i < src.Length; i += 3) { tris[i] = src[i]; tris[i + 1] = src[i + 2]; tris[i + 2] = src[i + 1]; }
                mesh.SetTriangles(tris, s);
            }
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }
    }
}
