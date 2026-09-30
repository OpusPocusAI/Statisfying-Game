using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    // Non-traversable silhouettes beyond the existing seaward recovery boundary.
    // This pass does not move finds, alter collision, or widen the playable coast.
    public static class CoveLandmarkPolish
    {
        const string Folder="Assets/Art/CoveLandmarks";
        static Material Paint(string name,Color colour)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",colour);m.SetFloat("_Smoothness",.12f);
            EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
        }
        static Mesh Stone(int variant)
        {
            var vertices=new List<Vector3>();var faces=new[]{new List<int>(),new List<int>(),new List<int>()};
            const int sides=9;float[] y={0,.18f,.50f,.82f,1};float[] radius={.75f,1,.88f,.72f,.38f};
            var rings=new Vector3[5,sides];
            for(int row=0;row<5;row++)for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides;
                float r=radius[row]*(1+.10f*Mathf.Sin(i*2.1f+variant)+.04f*Mathf.Sin(row*3+i));
                rings[row,i]=new Vector3(Mathf.Cos(a)*r+row*.045f,y[row]+(row==0?0:.028f*Mathf.Sin(i*2+variant)),Mathf.Sin(a)*r);
            }
            System.Action<int,Vector3,Vector3[]> face=(material,normal,points)=>
            {
                if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)System.Array.Reverse(points);
                int start=vertices.Count;vertices.AddRange(points);
                for(int j=1;j<points.Length-1;j++)faces[material].AddRange(new[]{start,start+j,start+j+1});
            };
            for(int row=0;row<4;row++)for(int i=0;i<sides;i++)
            {
                int next=(i+1)%sides;var radial=rings[row,i]+rings[row,next];radial.y=0;
                face(row==0?0:row==3?2:1,radial,new[]{rings[row,i],rings[row,next],rings[row+1,next],rings[row+1,i]});
            }
            var top=new Vector3[sides];var bottom=new Vector3[sides];
            for(int i=0;i<sides;i++){top[i]=rings[4,i];bottom[i]=rings[0,i];}
            face(2,Vector3.up,top);face(0,Vector3.down,bottom);
            string path=Folder+"/Sea carved stone "+variant+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh{name="Sea carved stone "+variant};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.SetVertices(vertices);mesh.subMeshCount=3;
            for(int i=0;i<3;i++)mesh.SetTriangles(faces[i],i);
            mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);return mesh;
        }
        static void Rock(Transform parent,string name,Vector3 position,Vector3 scale,float angle,Mesh mesh,Material[] materials)
        {
            var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=position;
            go.transform.localScale=scale;go.transform.rotation=Quaternion.Euler(0,angle,0);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=materials;
            GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic);
        }
        [MenuItem("Sea Glass/Polish/Distinct coastal silhouettes")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","CoveLandmarks");
            var old=GameObject.Find("ART • cove landmarks");if(old)Object.DestroyImmediate(old);
            var root=new GameObject("ART • cove landmarks").transform;
            var wet=Paint("Wet coastal slate",new Color(.22f,.30f,.31f));
            var pale=new[]{wet,Paint("Shell cove limestone",new Color(.68f,.63f,.49f)),Paint("Chalk rock crown",new Color(.81f,.75f,.58f))};
            var tidal=new[]{wet,Paint("Tidal blue grey",new Color(.34f,.42f,.40f)),Paint("Tidal sage crown",new Color(.47f,.51f,.41f))};
            var warm=new[]{wet,Paint("Sunset sandstone",new Color(.59f,.40f,.28f)),Paint("Sun washed sandstone",new Color(.72f,.54f,.36f))};
            var a=Stone(0);var b=Stone(1);var c=Stone(2);
            Rock(root,"Shell Cove outer limestone",new Vector3(-43,-1.5f,29),new Vector3(3.4f,4.0f,4.3f),18,a,pale);
            Rock(root,"Shell Cove leaning shoulder",new Vector3(-39,-1.4f,32),new Vector3(2.0f,2.4f,2.6f),63,b,pale);
            Rock(root,"Shell Cove low stone",new Vector3(-41,-1.3f,24),new Vector3(1.8f,1.5f,2.2f),-20,c,pale);
            Rock(root,"Tide Gardens broad shelf",new Vector3(-39,-1.25f,62),new Vector3(3.1f,1.8f,4.0f),-16,b,tidal);
            Rock(root,"Tide Gardens broken shelf",new Vector3(-37,-1.3f,69),new Vector3(1.8f,1.6f,2.2f),38,c,tidal);
            Rock(root,"Tide Gardens outer shelf",new Vector3(-44,-1.4f,66),new Vector3(2.5f,2.4f,2.8f),25,a,tidal);
            Rock(root,"Sunset Point sea stack",new Vector3(-44,-1.5f,108),new Vector3(2.5f,7.5f,3.1f),-12,c,warm);
            Rock(root,"Sunset Point companion",new Vector3(-40,-1.4f,112),new Vector3(2.0f,4.0f,2.2f),52,b,warm);
            Rock(root,"Sunset Point fallen shoulder",new Vector3(-44,-1.4f,103),new Vector3(2.9f,2.1f,2.0f),-35,a,warm);
            // Existing surf must follow newly generated stone contours.
            if(GameObject.Find("ART • rock contact surf"))RockSurfPolish.Apply();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
