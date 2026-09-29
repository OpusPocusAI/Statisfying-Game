using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    public static class GatheringPolish
    {
        const string Folder="Assets/Art/GatheringPolish";
        static readonly List<Vector3> vertices=new();static readonly List<int> triangles=new();
        static void Face(Vector3 normal,params Vector3[] points)
        {
            if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)System.Array.Reverse(points);
            int start=vertices.Count;vertices.AddRange(points);for(int i=1;i<points.Length-1;i++)triangles.AddRange(new[]{start,start+i,start+i+1});
        }
        static Mesh Save(string name)
        {
            string path=Folder+"/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);return mesh;
        }
        static Mesh Vessel(string name,float bottom,float top,float height,float thickness,bool soft=false)
        {
            vertices.Clear();triangles.Clear();
            for(int i=0;i<20;i++)
            {
                float a=i*Mathf.PI*.1f,b=(i+1)*Mathf.PI*.1f;var u=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var v=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
                var low=Vector3.down*height*.5f;var high=Vector3.up*height*.5f;var floor=low+Vector3.up*thickness;
                if(soft)
                {
                    float belly=top*1.12f;Face(u+v,low+u*bottom,low+v*bottom,v*belly,u*belly);
                    Face(u+v,u*belly,v*belly,high+v*top,high+u*top);
                }
                else Face(u+v,low+u*bottom,low+v*bottom,high+v*top,high+u*top);
                Face(-u-v,floor+u*(bottom-thickness),high+u*(top-thickness),high+v*(top-thickness),floor+v*(bottom-thickness));
                Face(Vector3.up,high+u*top,high+v*top,high+v*(top-thickness),high+u*(top-thickness));
                Face(Vector3.up,floor,floor+u*(bottom-thickness),floor+v*(bottom-thickness));
                Face(Vector3.down,low,low+v*bottom,low+u*bottom);
            }
            return Save(name);
        }
        static Mesh Arc(string name,float radius,float tube,float sweep)
        {
            vertices.Clear();triangles.Clear();
            for(int i=0;i<24;i++)for(int j=0;j<6;j++)
            {
                var p=new Vector3[4];int[] di={0,1,1,0},dj={0,0,1,1};
                for(int k=0;k<4;k++){float a=(i+di[k])*sweep/24,b=(j+dj[k])*Mathf.PI/3;float r=radius+Mathf.Cos(b)*tube;p[k]=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,Mathf.Sin(b)*tube);}
                float mid=(i+.5f)*sweep/24;Face((p[0]+p[1]+p[2]+p[3])*.25f-new Vector3(Mathf.Cos(mid)*radius,Mathf.Sin(mid)*radius,0),p);
            }
            return Save(name);
        }
        static Mesh Blade()
        {
            vertices.Clear();triangles.Clear();
            var edge=new[]{new Vector3(-.055f,0,0),new Vector3(.055f,0,0),new Vector3(.105f,.13f,.016f),new Vector3(.072f,.22f,.032f),new Vector3(-.072f,.22f,.032f),new Vector3(-.105f,.13f,.016f)};
            var center=new Vector3(0,.11f,.036f);
            for(int i=0;i<edge.Length;i++)
            {
                var a=edge[i];var b=edge[(i+1)%edge.Length];Face(Vector3.back,center,a,b);Face(Vector3.forward,center+Vector3.forward*.018f,b+Vector3.forward*.018f,a+Vector3.forward*.018f);
                Face((a+b)*.5f-center,a,a+Vector3.forward*.018f,b+Vector3.forward*.018f,b);
            }
            return Save("Toy shovel bowl");
        }
        static Mesh Contents()
        {
            vertices.Clear();triangles.Clear();
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;var u=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var v=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
                var top=Vector3.up*.20f;Face(u+v,u*.11f,v*.11f,top+v*.13f,top+u*.13f);
                Face(Vector3.up,Vector3.up*.24f,top+u*.13f,top+v*.13f);Face(Vector3.down,Vector3.zero,v*.11f,u*.11f);
            }
            return Save("Contents mound");
        }
        static Material Paint(string name,Color color,float smooth=.12f)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
        }
        static GameObject MeshObject(string name,Transform parent,Vector3 position,Mesh mesh,Material material)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;return go;
        }
        static GameObject Shape(string name,Transform parent,Vector3 position,Vector3 size,Material material,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static Transform Root(Transform parent,string name)
        {var old=parent.Find(name);if(old)Object.DestroyImmediate(old.gameObject);var r=new GameObject(name).transform;r.SetParent(parent,false);return r;}
        static void HideOriginal(GameObject go){var renderer=go.GetComponent<Renderer>();if(renderer)renderer.enabled=false;}
        [MenuItem("Sea Glass/Polish/Refine gathering tools")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","GatheringPolish");
            var player=Object.FindFirstObjectByType<CoastalPlayer>();var equipment=Object.FindFirstObjectByType<EquipmentVisuals>();
            var cream=Paint("Warm enamel",new Color(.85f,.78f,.58f),.2f);var coral=Paint("Toy coral",new Color(.8f,.27f,.13f),.22f);var teal=Paint("Sieve teal",new Color(.09f,.30f,.27f));
            var wood=Paint("Honey handle",new Color(.53f,.32f,.15f));var cloth=Paint("Sea green canvas",new Color(.24f,.39f,.31f));var dark=Paint("Gathered contents",new Color(.25f,.24f,.18f));
            player.tool.localScale=Vector3.one;player.tool.localPosition=new Vector3(.34f,-.30f,.68f);HideOriginal(player.tool.gameObject);
            foreach(var t in player.tool.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Toy shovel blade"))HideOriginal(t.gameObject);
            var tool=Root(player.tool,"ART • gathering tool");
            Shape("Rounded wooden grip",tool,new Vector3(0,-.06f,0),new Vector3(.045f,.15f,.045f),wood,PrimitiveType.Cylinder);
            Shape("Coral handle collar",tool,new Vector3(0,.09f,0),new Vector3(.055f,.028f,.055f),coral,PrimitiveType.Cylinder);
            var blade=MeshObject("Small curved toy blade",tool,new Vector3(0,.10f,-.012f),Blade(),coral);
            Shape("Handle end cap",tool,new Vector3(0,-.21f,0),new Vector3(.06f,.025f,.06f),coral,PrimitiveType.Sphere);
            var presentation=equipment.GetComponent<GatheringPresentation>();if(!presentation)presentation=equipment.gameObject.AddComponent<GatheringPresentation>();presentation.player=player;presentation.toyBlade=blade;
            HideOriginal(equipment.scoop);equipment.scoop.transform.localScale=Vector3.one;equipment.scoop.transform.localPosition=new Vector3(0,.12f,0);equipment.scoop.transform.localRotation=Quaternion.identity;
            var sieve=Root(equipment.scoop.transform,"ART • sieve basket");
            foreach(float x in new[]{-.14f,.14f})Shape("Sieve side",sieve,new Vector3(x,.10f,0),new Vector3(.025f,.23f,.06f),teal);
            foreach(float y in new[]{0f,.21f})Shape("Sieve rim",sieve,new Vector3(0,y,0),new Vector3(.28f,.025f,.06f),teal);
            for(int i=0;i<6;i++)Shape("Sieve cross wire",sieve,new Vector3(0,.025f+i*.032f,.022f),new Vector3(.26f,.008f,.008f),cream);
            for(int i=0;i<7;i++)Shape("Sieve length wire",sieve,new Vector3(-.115f+i*.038f,.105f,.022f),new Vector3(.008f,.20f,.008f),cream);
            var bucket=equipment.bucket.transform;HideOriginal(bucket.gameObject);bucket.localScale=Vector3.one;bucket.localPosition=new Vector3(.34f,-.29f,.72f);
            var bucketArt=Root(bucket,"ART • enamel bucket");
            MeshObject("Open tapered bucket",bucketArt,Vector3.zero,Vessel("Open enamel bucket",.105f,.155f,.26f,.014f),cream);
            var rim=MeshObject("Teal rolled rim",bucketArt,new Vector3(0,.13f,0),Arc("Bucket rim",.151f,.011f,Mathf.PI*2),teal);rim.transform.localRotation=Quaternion.Euler(90,0,0);
            MeshObject("Bucket carry handle",bucketArt,new Vector3(0,.09f,0),Arc("Bucket handle",.16f,.009f,Mathf.PI),teal);
            Shape("Handle comfort grip",bucketArt,new Vector3(0,.25f,0),new Vector3(.09f,.023f,.026f),wood);
            var contents=Contents();
            // Existing fill remains count-driven, with a neutral pile instead of falsely showing clean glass.
            equipment.bucketFill.localPosition=new Vector3(0,-.11f,0);equipment.bucketFill.GetComponent<MeshFilter>().sharedMesh=contents;equipment.bucketFill.GetComponent<Renderer>().sharedMaterial=dark;
            var sack=equipment.sack.transform;HideOriginal(sack.gameObject);sack.localScale=Vector3.one;sack.localPosition=new Vector3(.37f,-.31f,.79f);
            var sackArt=Root(sack,"ART • canvas sack");var bag=MeshObject("Soft sided open sack",sackArt,Vector3.zero,Vessel("Canvas bag",.12f,.18f,.32f,.013f,true),cloth);bag.transform.localScale=new Vector3(1.15f,1,.7f);
            var seam=MeshObject("Rolled canvas mouth",sackArt,new Vector3(0,.16f,0),Arc("Sack rim",.175f,.012f,Mathf.PI*2),cream);seam.transform.localRotation=Quaternion.Euler(90,0,0);seam.transform.localScale=new Vector3(1.15f,.7f,1);
            var strap=Arc("Canvas strap",.135f,.013f,Mathf.PI);
            foreach(float z in new[]{-.085f,.085f})MeshObject("Canvas carry loop",sackArt,new Vector3(0,.15f,z),strap,wood);
            Shape("Canvas front patch",sackArt,new Vector3(0,-.015f,-.143f),new Vector3(.13f,.09f,.008f),cream);
            equipment.sackFill=MeshObject("Sack contents",sackArt,new Vector3(0,-.12f,0),contents,dark).transform;
            foreach(var r in player.view.GetComponentsInChildren<Renderer>(true))if(r.transform.IsChildOf(player.tool)||r.transform.IsChildOf(bucket)||r.transform.IsChildOf(sack))r.shadowCastingMode=ShadowCastingMode.Off;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
