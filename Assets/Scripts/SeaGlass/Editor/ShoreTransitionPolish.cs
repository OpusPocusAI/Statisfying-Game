using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // The visual shoulder bridges the original sand shelf; collision remains on that shelf.
    public static class ShoreTransitionPolish
    {
        const string Folder="Assets/Art/ShoreTransition";
        [MenuItem("Sea Glass/Polish/Blend dry and wet shoreline")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var shelf=GameObject.Find("Continuous sand shelf");var shoreline=GameObject.Find("Soft wet-sand shoreline");
            if(!shelf||!shoreline)throw new System.InvalidOperationException("The coastal sand shelf and shoreline must exist.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","ShoreTransition");
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            const int rows=240,columns=7;
            for(int i=0;i<=rows;i++)
            {
                float z=-39+i;
                // Match the ocean's existing coastline. No wider beach or new playable land.
                float shore=-34.5f+Mathf.Sin(z*.07f)*1.1f+Mathf.Sin(z*.19f)*.3f;
                float start=-30.65f+Mathf.Sin(z*.15f)*.22f+Mathf.Sin(z*.37f)*.07f;
                float[] x={start,-31.45f,-32.10f,Mathf.Lerp(-32.10f,shore,.28f),Mathf.Lerp(-32.10f,shore,.65f),shore,shore-2};
                float[] y={-.088f,-.088f,-.089f,-.23f,-.54f,-.85f,-1.3f};
                float[] u={0,.16f,.33f,.5f,.7f,.87f,1};
                for(int j=0;j<columns;j++)
                {
                    vertices.Add(new Vector3(x[j],y[j],z));uv.Add(new Vector2(u[j],i/(float)rows));
                    if(i<rows&&j<columns-1){int a=i*columns+j;triangles.AddRange(new[]{a,a+1,a+columns,a+1,a+columns+1,a+columns});}
                }
            }
            string meshPath=Folder+"/Blended shore shoulder.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(!mesh){mesh=new Mesh{name="Blended shore shoulder"};AssetDatabase.CreateAsset(mesh,meshPath);}else mesh.Clear();
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.RecalculateTangents();
            EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);

            string texturePath=Folder+"/Dry to wet sand wash.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if(!texture){texture=new Texture2D(256,128,TextureFormat.RGBA32,true,false){name="Dry to wet sand wash"};AssetDatabase.CreateAsset(texture,texturePath);}
            for(int y=0;y<128;y++)for(int x=0;x<256;x++)
            {
                float u=x/255f,v=y/127f;
                float variation=Mathf.Sin(v*31+Mathf.Sin(v*17))*Mathf.Sin(u*9+v*47);
                float wet=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.14f,.86f,u+variation*.045f));
                var colour=Color.Lerp(new Color(.98f,.98f,.98f),new Color(.78f,.84f,.88f),wet);
                colour*=1+variation*.012f*Mathf.SmoothStep(0,1,u*4);colour.a=1;texture.SetPixel(x,y,colour);
            }
            texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Trilinear;texture.anisoLevel=4;texture.Apply();
            EditorUtility.SetDirty(texture);AssetDatabase.SaveAssetIfDirty(texture);
            string materialPath=Folder+"/Warm tidal sand.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material){material=new Material(shelf.GetComponent<Renderer>().sharedMaterial);AssetDatabase.CreateAsset(material,materialPath);}
            material.SetColor("_BaseColor",shelf.GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor"));
            material.SetTexture("_BaseMap",texture);material.SetTextureScale("_BaseMap",Vector2.one);material.SetTextureOffset("_BaseMap",Vector2.zero);
            material.SetFloat("_Smoothness",.18f);EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);
            shoreline.GetComponent<MeshFilter>().sharedMesh=mesh;shoreline.GetComponent<Renderer>().sharedMaterial=material;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
