using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    // Cargo, upgraded rails and driving colliders keep their identities. The capsule aligns to the tyre base.
    public static class TruckPolish
    {
        const string Folder="Assets/Art/TruckPolish";
        static Transform root;
        static readonly List<Vector3> vertices=new();
        static readonly List<int> indices=new();
        static void Face(Vector3 normal,params Vector3[] points)
        {
            if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)System.Array.Reverse(points);
            int start=vertices.Count;vertices.AddRange(points);
            for(int i=1;i<points.Length-1;i++)indices.AddRange(new[]{start,start+i,start+i+1});
        }
        static Mesh SaveMesh(string name)
        {
            string path=Folder+"/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);return mesh;
        }
        static Mesh Bevel(string name,Vector3 size,float radius)
        {
            vertices.Clear();indices.Clear();var h=size*.5f;float b=Mathf.Min(radius,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.8f);
            for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2)
            {
                int u=(axis+1)%3,v=(axis+2)%3;var p=new Vector3[4];int[,] corners={{-1,-1},{1,-1},{1,1},{-1,1}};
                for(int i=0;i<4;i++){p[i][axis]=sign*h[axis];p[i][u]=corners[i,0]*(h[u]-b);p[i][v]=corners[i,1]*(h[v]-b);}
                var n=Vector3.zero;n[axis]=sign;Face(n,p);
            }
            for(int axis=0;axis<3;axis++)for(int a=-1;a<=1;a+=2)for(int c=-1;c<=1;c+=2)
            {
                int u=(axis+1)%3,v=(axis+2)%3;var p=new Vector3[4];
                for(int i=0;i<4;i++){p[i][axis]=(i<2?-1:1)*(h[axis]-b);bool outer=i==0||i==3;p[i][u]=a*(h[u]-(outer?0:b));p[i][v]=c*(h[v]-(outer?b:0));}
                var n=Vector3.zero;n[u]=a;n[v]=c;Face(n,p);
            }
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                Face(new Vector3(x,y,z),new Vector3(x*h.x,y*(h.y-b),z*(h.z-b)),new Vector3(x*(h.x-b),y*h.y,z*(h.z-b)),new Vector3(x*(h.x-b),y*(h.y-b),z*h.z));
            return SaveMesh(name);
        }
        static Mesh Arch()
        {
            vertices.Clear();indices.Clear();
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;var ra=new Vector3(0,Mathf.Sin(a),Mathf.Cos(a));var rb=new Vector3(0,Mathf.Sin(b),Mathf.Cos(b));
                foreach(float side in new[]{-.09f,.09f})Face(Vector3.right*Mathf.Sign(side),Vector3.right*side+ra*.48f,Vector3.right*side+ra*.60f,Vector3.right*side+rb*.60f,Vector3.right*side+rb*.48f);
                Face(ra+rb,Vector3.left*.09f+ra*.60f,Vector3.right*.09f+ra*.60f,Vector3.right*.09f+rb*.60f,Vector3.left*.09f+rb*.60f);
                Face(-ra-rb,Vector3.left*.09f+ra*.48f,Vector3.left*.09f+rb*.48f,Vector3.right*.09f+rb*.48f,Vector3.right*.09f+ra*.48f);
            }
            return SaveMesh("Wheel arch");
        }
        static Mesh SteeringRing()
        {
            vertices.Clear();indices.Clear();
            for(int i=0;i<24;i++)for(int j=0;j<6;j++)
            {
                var p=new Vector3[4];int[] di={0,1,1,0},dj={0,0,1,1};
                for(int k=0;k<4;k++){float a=(i+di[k])*Mathf.PI*2/24,b=(j+dj[k])*Mathf.PI*2/6;float r=.24f+Mathf.Cos(b)*.023f;p[k]=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,Mathf.Sin(b)*.023f);}
                float mid=(i+.5f)*Mathf.PI*2/24;var center=new Vector3(Mathf.Cos(mid)*.24f,Mathf.Sin(mid)*.24f,0);Face((p[0]+p[1]+p[2]+p[3])*.25f-center,p);
            }
            return SaveMesh("Steering rim");
        }
        static Mesh CargoRail()
        {
            vertices.Clear();indices.Clear();
            var centers=new[]{new Vector3(0,.40f,0),new Vector3(0,-.40f,0),new Vector3(0,0,-.46f),Vector3.zero,new Vector3(0,0,.46f)};
            for(int part=0;part<centers.Length;part++)
            {
                var h=(part<2?new Vector3(1,.20f,1):new Vector3(1,.8f,.08f))*.5f;
                for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2)
                {
                    int u=(axis+1)%3,v=(axis+2)%3;var p=new Vector3[4];int[,] corners={{-1,-1},{1,-1},{1,1},{-1,1}};
                    for(int i=0;i<4;i++){p[i][axis]=sign*h[axis];p[i][u]=corners[i,0]*h[u];p[i][v]=corners[i,1]*h[v];p[i]+=centers[part];}
                    var n=Vector3.zero;n[axis]=sign;Face(n,p);
                }
            }
            return SaveMesh("Open cargo rail");
        }
        static Material Paint(string name,Color colour,float smooth=.16f)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",colour);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
        }
        static GameObject MeshObject(string name,Vector3 position,Mesh mesh,Material material)
        {
            var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.localPosition=position;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;return go;
        }
        static GameObject Box(string name,Vector3 position,Vector3 size,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=position;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static GameObject Disc(string name,Vector3 position,float diameter,float depth,Material material,Quaternion rotation)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=position;go.transform.localRotation=rotation;go.transform.localScale=new Vector3(diameter,depth*.5f,diameter);
            go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        [MenuItem("Sea Glass/Polish/Refine utility truck")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","TruckPolish");
            var truck=Object.FindFirstObjectByType<UtilityTruck>();var old=truck.body.Find("ART • truck finish");if(old)Object.DestroyImmediate(old.gameObject);
            // The effective capsule height is clamped to its two-metre diameter. Include its skin offset
            // so grounded artwork meets the road, rather than hovering a quarter-metre above it.
            truck.GetComponent<CharacterController>().center=new Vector3(0,1.1f,0);
            root=new GameObject("ART • truck finish").transform;root.SetParent(truck.body,false);
            var cream=Paint("Ivory enamel",new Color(.85f,.79f,.59f));var teal=Paint("Tidal teal",new Color(.09f,.35f,.32f));var coral=Paint("Faded coral",new Color(.72f,.25f,.13f));
            var dark=Paint("Soft charcoal",new Color(.10f,.12f,.15f));var metal=Paint("Brushed warm metal",new Color(.47f,.49f,.43f),.27f);var amber=Paint("Amber lens",new Color(.96f,.56f,.18f),.3f);
            var lamp=Paint("Cream headlamp",new Color(.95f,.9f,.69f),.35f);
            var rail=CargoRail();foreach(var t in truck.body.GetComponentsInChildren<Transform>().Where(t=>t.name=="Cargo rail"))t.GetComponent<MeshFilter>().sharedMesh=rail;
            foreach(var r in truck.body.GetComponentsInChildren<Renderer>().Where(r=>new[]{"Cream bonnet","Cab roof","Front bumper","Headlight","Door"}.Contains(r.name)))r.enabled=false;
            MeshObject("Soft bonnet",new Vector3(0,1.15f,1.4f),Bevel("Bonnet",new Vector3(2,.52f,1.16f),.13f),cream);
            MeshObject("Crowned coral roof",new Vector3(0,2.57f,.55f),Bevel("Roof",new Vector3(2.15f,.25f,1.88f),.10f),coral);
            MeshObject("Rounded front bumper",new Vector3(0,.57f,2.15f),Bevel("Bumper",new Vector3(2.22f,.18f,.22f),.06f),cream);
            var door=Bevel("Door panel",new Vector3(.14f,.58f,1.35f),.055f);var arch=Arch();
            foreach(float side in new[]{-1f,1f})
            {
                MeshObject("Enamel door",new Vector3(side*.95f,1.16f,.5f),door,teal);
                Box("Door cream belt",new Vector3(side*1.027f,1.35f,.5f),new Vector3(.025f,.065f,1.12f),cream);
                Box("Door handle",new Vector3(side*1.05f,1.2f,.05f),new Vector3(.05f,.045f,.18f),metal);
                Box("Cab step",new Vector3(side*1.03f,.49f,0),new Vector3(.28f,.075f,.65f),dark);
                Box("Mirror arm",new Vector3(side*1.09f,1.85f,1.23f),new Vector3(.28f,.035f,.035f),metal);
                MeshObject("Mirror casing",new Vector3(side*1.25f,1.88f,1.23f),Bevel("Mirror",new Vector3(.15f,.25f,.075f),.025f),dark);
                Box("Mirror inset",new Vector3(side*1.25f,1.88f,1.185f),new Vector3(.11f,.20f,.01f),metal);
                foreach(float z in new[]{-1.4f,1.4f})MeshObject("Ivory wheel arch",new Vector3(side*1.055f,.45f,z),arch,cream);
                Disc("Headlamp bezel",new Vector3(side*.70f,1.11f,1.99f),.32f,.055f,metal,Quaternion.Euler(90,0,0));
                Disc("Round headlamp",new Vector3(side*.70f,1.11f,2.022f),.245f,.018f,lamp,Quaternion.Euler(90,0,0));
                Box("Amber marker",new Vector3(side*.88f,.89f,1.99f),new Vector3(.12f,.065f,.04f),amber);
                Box("Tail lamp backing",new Vector3(side*.88f,.76f,-2.12f),new Vector3(.21f,.23f,.07f),dark);
                Box("Coral tail lamp",new Vector3(side*.88f,.80f,-2.165f),new Vector3(.15f,.09f,.028f),coral);
                Box("Amber tail marker",new Vector3(side*.88f,.71f,-2.165f),new Vector3(.15f,.055f,.028f),amber);
            }
            Box("Recessed grille",new Vector3(0,1.10f,1.99f),new Vector3(.82f,.20f,.024f),dark);
            for(int i=0;i<3;i++)Box("Grille slat",new Vector3(0,1.03f+i*.064f,2.01f),new Vector3(.75f,.018f,.025f),metal);
            Box("Bonnet centre badge",new Vector3(0,1.31f,1.986f),new Vector3(.16f,.065f,.025f),teal);
            foreach(var wheel in truck.wheels)
            {
                var oldHub=wheel.Find("Wheel hub");if(oldHub)Object.DestroyImmediate(oldHub.gameObject);
                // Parent to the existing rotating wheel, compensating its nonuniform cylinder scale.
                float side=Mathf.Sign(wheel.localPosition.x);var hub=Disc("Wheel hub",Vector3.zero,.50f,.045f,cream,Quaternion.Euler(0,0,90));
                hub.transform.SetParent(wheel,true);hub.transform.position=wheel.position+truck.transform.right*side*.20f;hub.transform.rotation=wheel.rotation;
                hub.transform.localScale=new Vector3(.50f/.85f,.0225f/.19f,.50f/.85f);
                var cap=Disc("Hub centre",Vector3.zero,.21f,.055f,metal,Quaternion.Euler(0,0,90));cap.transform.position=wheel.position+truck.transform.right*side*.23f;cap.transform.SetParent(hub.transform,true);
            }
            var steering=new GameObject("Steering assembly").transform;steering.SetParent(root,false);steering.localPosition=new Vector3(-.45f,1.60f,.89f);steering.localRotation=Quaternion.Euler(20,0,0);
            var rim=MeshObject("Steering rim",Vector3.zero,SteeringRing(),dark);rim.transform.SetParent(steering,false);
            foreach(float angle in new[]{0f,120f,240f}){float a=angle*Mathf.Deg2Rad;var spoke=Box("Steering spoke",new Vector3(Mathf.Sin(a)*.1f,Mathf.Cos(a)*.1f,0),new Vector3(.024f,.20f,.02f),metal);spoke.transform.SetParent(steering,false);spoke.transform.localRotation=Quaternion.Euler(0,0,-angle);}
            var button=Disc("Steering centre",Vector3.zero,.095f,.045f,cream,Quaternion.Euler(90,0,0));button.transform.SetParent(steering,false);
            foreach(var label in truck.body.GetComponentsInChildren<TextMesh>().Where(t=>t.text=="GLASS"||t.text=="RECYCLE"))
            {
                float x=label.text=="GLASS"?-.52f:.52f;label.characterSize=.032f;label.color=new Color(.035f,.10f,.09f);label.transform.localPosition=new Vector3(x,1.13f,-1.94f);
                float width=label.GetComponent<Renderer>().localBounds.size.x;if(width>.01f)label.characterSize*=Mathf.Min(1,.68f/width);
                Box("Cargo enamel label",new Vector3(x,1.13f,-1.915f),new Vector3(.82f,.20f,.028f),cream);
            }
            foreach(var label in truck.body.GetComponentsInChildren<TextMesh>().Where(t=>t.text=="SKIP")){label.characterSize=.026f;label.color=new Color(.035f,.10f,.09f);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
