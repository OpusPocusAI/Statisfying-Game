using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    // Decoration stays outside station surfaces and the gallery's completion sightline.
    public static class StudioAtmospherePolish
    {
        const string Folder="Assets/Art/StudioAtmosphere";
        static Transform root;
        static readonly List<Vector3> vertices=new();static readonly List<int> indices=new();
        static void Face(Vector3 normal,params Vector3[] points)
        {
            if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)System.Array.Reverse(points);
            int start=vertices.Count;vertices.AddRange(points);for(int i=1;i<points.Length-1;i++)indices.AddRange(new[]{start,start+i,start+i+1});
        }
        static Mesh Save(string name)
        {
            string path=Folder+"/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);return mesh;
        }
        static Mesh Turned(string name,float[] height,float[] radius)
        {
            vertices.Clear();indices.Clear();
            for(int row=0;row<height.Length-1;row++)for(int i=0;i<18;i++)
            {
                float a=i*Mathf.PI*2/18,b=(i+1)*Mathf.PI*2/18;var u=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var v=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
                var normal=(u+v)*(height[row+1]-height[row])+Vector3.up*(radius[row]-radius[row+1])*2;
                Face(normal,u*radius[row]+Vector3.up*height[row],v*radius[row]+Vector3.up*height[row],v*radius[row+1]+Vector3.up*height[row+1],u*radius[row+1]+Vector3.up*height[row+1]);
            }
            return Save(name);
        }
        static Mesh Curtain()
        {
            vertices.Clear();indices.Clear();
            Vector3 Point(int x,int y)
            {
                float v=y/6f,u=x/12f;float width=Mathf.Lerp(.23f,.49f,Mathf.Abs(v-.48f)*2);
                return new Vector3(Mathf.Cos(u*Mathf.PI*8)*.04f,1.94f*v,(u-.5f)*width+.045f*Mathf.Sin(v*Mathf.PI));
            }
            for(int y=0;y<6;y++)for(int x=0;x<12;x++)
            {var a=Point(x,y);var b=Point(x+1,y);var c=Point(x+1,y+1);var d=Point(x,y+1);Face(Vector3.right,a,b,c,d);Face(Vector3.left,a,b,c,d);}
            return Save("Tied linen folds");
        }
        static Material Paint(string name,Color color,float smooth=.12f)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
        }
        static GameObject Art(string name,Vector3 position,Vector3 scale,Mesh mesh,Material material)
        {
            var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=position;go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;return go;
        }
        static GameObject Box(string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);go.transform.position=position;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        [MenuItem("Sea Glass/Polish/Warm studio atmosphere")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","StudioAtmosphere");
            var old=GameObject.Find("ART • studio atmosphere");if(old)Object.DestroyImmediate(old);root=new GameObject("ART • studio atmosphere").transform;
            var chalk=Paint("Limewashed ceiling",new Color(.79f,.76f,.64f));var seam=Paint("Ceiling joint",new Color(.58f,.54f,.42f));
            var linen=Paint("Natural linen",new Color(.86f,.79f,.60f));var teal=Paint("Weathered studio teal",new Color(.18f,.35f,.34f));var honey=Paint("Shelf oak",new Color(.46f,.30f,.17f));
            var clay=Paint("Muted terracotta",new Color(.62f,.32f,.21f));var porcelain=Paint("Cream ceramic",new Color(.82f,.82f,.69f),.26f);var brass=Paint("Aged brass",new Color(.48f,.36f,.20f),.3f);
            var shade=Paint("Pendant enamel",new Color(.26f,.40f,.36f),.2f);var diffuser=Paint("Unlit pendant glass",new Color(.40f,.37f,.28f));
            // A separate interior lining keeps the exterior's coral roof and existing beams intact.
            Box("Pale timber ceiling lining",new Vector3(-4,5.46f,166),new Vector3(24.6f,.07f,22.7f),chalk);
            for(int i=0;i<24;i++)Box("Quiet ceiling board joint",new Vector3(-15.7f+i,5.42f,166),new Vector3(.015f,.008f,22.65f),seam);
            for(int i=0;i<22;i++)Box("Ocean wall lower batten",new Vector3(-15.80f,.85f,155.2f+i),new Vector3(.025f,1.38f,.035f),teal);
            Box("Ocean wall dado rail",new Vector3(-15.76f,1.48f,166),new Vector3(.09f,.08f,22.6f),honey);
            var curtain=Curtain();
            foreach(float z in new[]{159f,165f,172f})
            {
                Box("Curtain rail",new Vector3(-15.58f,3.77f,z),new Vector3(.075f,.075f,4.02f),honey);
                foreach(float side in new[]{-1f,1f})
                {
                    Art("Folded linen curtain",new Vector3(-15.60f,1.67f,z+side*1.53f),Vector3.one,curtain,linen);
                    Box("Linen tieback",new Vector3(-15.54f,2.58f,z+side*1.53f+.045f),new Vector3(.08f,.065f,.25f),brass);
                }
            }
            var lampMesh=Turned("Open bell pendant",new[]{.20f,.20f,.15f,-.07f,-.13f,-.13f,-.08f,.13f,.17f},new[]{.06f,.10f,.19f,.40f,.43f,.39f,.37f,.16f,.06f});
            var diffuserMesh=Turned("Pendant diffuser",new[]{-.12f,-.12f,-.09f,-.09f},new[]{0f,.35f,.35f,0f});
            foreach(var pendant in Object.FindObjectsByType<RestorationDecor>(FindObjectsSortMode.None).Where(r=>r.lamp&&r.name=="Warm pendant"))
            {
                pendant.GetComponent<Renderer>().enabled=false;
                Art("Enamel bell shade",pendant.transform.position,Vector3.one,lampMesh,shade);
                var glass=Art("Restoring pendant diffuser",pendant.transform.position,Vector3.one,diffuserMesh,diffuser);
                pendant.surface=glass.GetComponent<Renderer>();pendant.restored=new Color(1,.83f,.48f);
                Box("Pendant brass collar",pendant.transform.position+Vector3.up*.22f,new Vector3(.10f,.07f,.10f),brass);
            }
            var vase=Turned("Open ceramic vase",new[]{0f,0f,.06f,.26f,.36f,.40f,.40f,.35f,.23f,.055f,.055f},new[]{0f,.10f,.145f,.155f,.075f,.075f,.056f,.056f,.12f,.07f,0f});
            var bowl=Turned("Thrown ceramic bowl",new[]{0f,0f,.04f,.16f,.17f,.17f,.14f,.045f,.04f},new[]{0f,.10f,.13f,.22f,.22f,.19f,.185f,.085f,0f});
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Studio pottery"))
            {t.GetComponent<Renderer>().enabled=false;Art("Shelf ceramic vessel",new Vector3(t.position.x,1.46f,t.position.z),Vector3.one,vase,Mathf.RoundToInt(t.position.z*2)%2==0?porcelain:clay);}
            foreach(float z in new[]{162f,168f,174f})
            {
                foreach(float offset in new[]{-.67f,.67f})Box("Shelf bracket",new Vector3(-15.54f,1.21f,z+offset),new Vector3(.34f,.30f,.06f),honey);
                Box("Ceramic shelf front lip",new Vector3(-15.08f,1.47f,z),new Vector3(.045f,.07f,1.92f),honey);
            }
            // A few useful-looking supplies sit against the wall, clear of the workbench approach.
            Box("Linen storage shelf",new Vector3(-15.35f,.49f,168),new Vector3(.67f,.07f,2.0f),honey);
            for(int i=0;i<3;i++)Box("Folded spare linen",new Vector3(-15.31f,.55f+i*.065f,168.5f),new Vector3(.46f,.06f,.55f),i==1?teal:linen);
            Art("Spare ceramic wash bowl",new Vector3(-15.3f,.53f,167.5f),Vector3.one,bowl,porcelain);
            var pot=Turned("Rolled terracotta planter",new[]{-.35f,-.35f,.27f,.30f,.34f,.34f,.28f,-.27f,-.27f},new[]{0f,.22f,.34f,.37f,.37f,.31f,.30f,.18f,0f});
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Terracotta planter"))
            {t.GetComponent<Renderer>().enabled=false;Art("Open terracotta entry planter",new Vector3(t.position.x,.27f,t.position.z),Vector3.one,pot,clay);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
