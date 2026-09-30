using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    // Saved artwork only: stable find IDs, transforms and targeting colliders stay intact.
    public static class ShorelinePolish
    {
        const string Folder="Assets/Art/ShorelinePolish";
        sealed class Form
        {
            readonly List<Vector3> vertices=new();readonly List<int>[] faces={new(),new(),new()};
            public void Face(int material,Vector3 normal,params Vector3[] points)
            {
                if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)System.Array.Reverse(points);
                int start=vertices.Count;vertices.AddRange(points);
                for(int i=1;i<points.Length-1;i++)faces[material].AddRange(new[]{start,start+i,start+i+1});
            }
            public void Lathe(float[] heights,float[] radii,int material,int sides=12)
            {
                for(int row=0;row<heights.Length-1;row++)for(int i=0;i<sides;i++)
                {
                    float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                    var u=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var v=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
                    Face(material,u+v,u*radii[row]+Vector3.up*heights[row],v*radii[row]+Vector3.up*heights[row],v*radii[row+1]+Vector3.up*heights[row+1],u*radii[row+1]+Vector3.up*heights[row+1]);
                }
                for(int i=0;i<sides;i++)
                {
                    float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;var u=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var v=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));int end=heights.Length-1;
                    Face(material,Vector3.down,Vector3.up*heights[0],u*radii[0]+Vector3.up*heights[0],v*radii[0]+Vector3.up*heights[0]);
                    Face(material,Vector3.up,Vector3.up*heights[end],u*radii[end]+Vector3.up*heights[end],v*radii[end]+Vector3.up*heights[end]);
                }
            }
            public void RestOnSand(Quaternion rotation)
            {
                for(int i=0;i<vertices.Count;i++)vertices[i]=rotation*vertices[i];
                // Revealed visuals sit at root Y .04 + local Y .13; the sand surface is Y -.1.
                float shift=-.268f-vertices.Min(v=>v.y);
                for(int i=0;i<vertices.Count;i++)vertices[i]+=Vector3.up*shift;
            }
            public Mesh Save(string name)
            {
                string path=Folder+"/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(!mesh){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
                mesh.SetVertices(vertices);mesh.subMeshCount=faces.Length;for(int i=0;i<faces.Length;i++)mesh.SetTriangles(faces[i],i);
                mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);return mesh;
            }
        }
        static Material Paint(string name,Color color,float smooth=.1f)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
        }
        static GameObject Art(string name,Transform parent,Vector3 position,Vector3 scale,Mesh mesh,params Material[] materials)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=materials;return go;
        }
        static Mesh Sand(int variant)
        {
            var f=new Form();int n=14;var center=new Vector3(.08f,1,0);
            // Broad shoulders and a feathered, uneven footprint instead of a sphere or pyramid.
            for(int i=0;i<n;i++)
            {
                float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;
                var u=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(1+.09f*Mathf.Sin(i*3+variant));
                var v=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*(1+.09f*Mathf.Sin((i+1)*3+variant));
                var innerU=u*.46f+Vector3.up*(.77f+.06f*Mathf.Sin(i+variant));var innerV=v*.46f+Vector3.up*(.77f+.06f*Mathf.Sin(i+1+variant));
                f.Face(0,Vector3.up,center,innerU,innerV);f.Face(0,Vector3.up,innerU,u, v,innerV);
            }
            return f.Save("Wind brushed sand "+variant);
        }
        static Mesh Grass()
        {
            var f=new Form();for(int i=0;i<9;i++)
            {
                float a=i*2.4f,h=.38f+(i%4)*.13f;var p=new Vector3(Mathf.Cos(a)*.16f,0,Mathf.Sin(a)*.16f);var side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.035f;
                var mid=p+new Vector3(-.12f,h*.6f,.04f);var tip=p+new Vector3(-.27f,h,.09f);var normal=Vector3.Cross(side,Vector3.up).normalized;
                f.Face(i%2,normal,p-side,p+side,mid+side*.6f,tip,mid-side*.6f);
                f.Face(i%2,-normal,p-side,p+side,mid+side*.6f,tip,mid-side*.6f);
            }
            return f.Save("Bent sea grass tuft");
        }
        static float SurfaceHeight(GameObject dune,Vector3 position)
        {
            var mesh=dune.GetComponent<MeshFilter>().sharedMesh;var p=dune.transform.InverseTransformPoint(position);var v=mesh.vertices;var triangles=mesh.triangles;
            for(int i=0;i<triangles.Length;i+=3)
            {
                var a=v[triangles[i]];var b=v[triangles[i+1]];var c=v[triangles[i+2]];
                float d=(b.z-c.z)*(a.x-c.x)+(c.x-b.x)*(a.z-c.z);if(Mathf.Abs(d)<.00001f)continue;
                float u=((b.z-c.z)*(p.x-c.x)+(c.x-b.x)*(p.z-c.z))/d;
                float w=((c.z-a.z)*(p.x-c.x)+(a.x-c.x)*(p.z-c.z))/d;float t=1-u-w;
                if(u>=0&&w>=0&&t>=0)return dune.transform.TransformPoint(new Vector3(p.x,u*a.y+w*b.y+t*c.y,p.z)).y;
            }
            return -.1f;
        }
        static Mesh Parasol()
        {
            var f=new Form();const int n=48;
            // A thin closed shell avoids coincident front/back fabric. Four samples per
            // stripe soften the hem without changing the familiar twelve-panel silhouette.
            float[] radii={0,.5f,1f,1.5f,1.8f,1.8f,1.782f,1.782f,1.492f,.995f,.498f,0};
            float[] heights={.58f,.51f,.36f,.14f,0,-.08f,-.08f,-.018f,.122f,.342f,.492f,.562f};
            System.Func<int,int,Vector3> point=(row,segment)=>
            {
                float angle=segment*Mathf.PI*2/n;
                float hem=row==5||row==6?-.09f*Mathf.Sin((segment%4)*Mathf.PI/4):0;
                return new Vector3(Mathf.Cos(angle)*radii[row],heights[row]+hem,Mathf.Sin(angle)*radii[row]);
            };
            for(int i=0;i<n;i++)
            {
                float angle=(i+.5f)*Mathf.PI*2/n;var radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                for(int row=0;row<radii.Length-1;row++)
                {
                    var normal=radial*(heights[row]-heights[row+1])+Vector3.up*(radii[row+1]-radii[row]);
                    var a=point(row,i);var b=point(row+1,i);var c=point(row+1,i+1);var d=point(row,i+1);
                    if(radii[row]==0)f.Face(i/4%2,normal,a,b,c);
                    else if(radii[row+1]==0)f.Face(i/4%2,normal,a,b,d);
                    else f.Face(i/4%2,normal,a,b,c,d);
                }
            }
            // Slim underside ribs follow the canopy curve and connect visually to the pole.
            for(int i=0;i<12;i++)for(int row=0;row<4;row++)
            {
                var a=point(row,i*4)-Vector3.up*.04f;var b=point(row+1,i*4)-Vector3.up*.04f;
                var side=Vector3.Cross(b-a,Vector3.up).normalized*.012f;var down=Vector3.down*.025f;
                f.Face(2,Vector3.down,a-side+down,b-side+down,b+side+down,a+side+down);
                f.Face(2,side,a+side,b+side,b+side+down,a+side+down);
                f.Face(2,-side,a-side+down,b-side+down,b-side,a-side);
                f.Face(2,Vector3.up,a-side,a+side,b+side,b-side);
            }
            return f.Save("Striped scalloped parasol");
        }
        [MenuItem("Sea Glass/Polish/Refine canopy fabric")]
        public static void RefineCanopies()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Leave Play Mode first.");
            Parasol(); // Shared saved mesh updates the three existing canopies without rebuilding the scene.
        }
        static Mesh Rubbish(int kind,bool grounded=true)
        {
            var f=new Form();
            if(kind==0)
            {
                f.Lathe(new[]{-.14f,-.12f,.12f,.14f},new[]{.085f,.10f,.10f,.085f},0);
                f.Lathe(new[]{-.075f,.075f},new[]{.101f,.101f},1);
                f.Lathe(new[]{.14f,.147f},new[]{.087f,.087f},2);
                f.Face(1,Vector3.up,new Vector3(-.022f,.149f,-.04f),new Vector3(.022f,.149f,-.04f),new Vector3(.022f,.149f,.03f),new Vector3(-.022f,.149f,.03f));
            }
            else if(kind==1)
            {
                f.Lathe(new[]{-.19f,-.17f,.06f,.12f,.18f},new[]{.065f,.083f,.083f,.035f,.035f},0);
                f.Lathe(new[]{-.08f,.02f},new[]{.084f,.084f},1);f.Lathe(new[]{.16f,.20f},new[]{.041f,.041f},2);
            }
            else if(kind==2)
            {
                var a=new Vector3(-.17f,-.08f,-.11f);var b=new Vector3(.18f,-.08f,-.09f);var c=new Vector3(.13f,.02f,.06f);var d=new Vector3(-.13f,.01f,.1f);
                f.Face(0,Vector3.up,a,b,c,d);f.Face(0,Vector3.down,a,b,c,d);
                var e=new Vector3(-.13f,.10f,.11f);var g=new Vector3(.13f,.11f,.08f);f.Face(1,Vector3.back,d,c,g,e);f.Face(1,Vector3.forward,d,c,g,e);
            }
            else if(kind==3)f.Lathe(new[]{-.02f,0f,.025f},new[]{.115f,.125f,.11f},1,16);
            else f.Lathe(new[]{-.36f,-.29f,.22f,.34f},new[]{.035f,.075f,.058f,.026f},0,7);
            if(grounded)f.RestOnSand(Quaternion.Euler(kind==1?75:kind==4?90:0,0,kind==0?12:0));
            return f.Save(new[]{"Rolled rim can","Capped shoreline bottle","Folded metal scrap","Collected bottle cap","Worn driftwood"}[kind]+(grounded?"":" decoration"));
        }
        [MenuItem("Sea Glass/Polish/Refine shoreline props")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","ShorelinePolish");
            var old=GameObject.Find("ART • shoreline composition");if(old)Object.DestroyImmediate(old);var root=new GameObject("ART • shoreline composition").transform;
            var sand=Paint("Dry dune sand",new Color(.86f,.75f,.55f));var cream=Paint("Canvas cream",new Color(.9f,.83f,.65f));var coral=Paint("Faded coral canvas",new Color(.65f,.27f,.16f));
            var teal=Paint("Weathered sea teal",new Color(.13f,.38f,.37f));var leaf=Paint("Dune grass shadow",new Color(.19f,.36f,.28f));var tips=Paint("Dune grass sage",new Color(.44f,.52f,.34f));
            var wood=Paint("Bleached driftwood",new Color(.53f,.40f,.26f));var metal=Paint("Dulled aluminium",new Color(.48f,.52f,.51f),.26f);
            var sands=Enumerable.Range(0,4).Select(Sand).ToArray();var rubbish=Enumerable.Range(0,5).Select(i=>Rubbish(i)).ToArray();var driftwood=Rubbish(4,false);
            foreach(var item in Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None))
            {
                int seed=int.Parse(item.id.Substring(item.id.LastIndexOf('-')+1));
                item.mound.GetComponent<MeshFilter>().sharedMesh=sands[seed%4];item.mound.GetComponent<Renderer>().sharedMaterial=sand;
                item.mound.localPosition=new Vector3(0,-.14f,0);item.mound.localScale=new Vector3(.43f+(seed%3)*.035f,.20f+(seed%3)*.03f,.36f+(seed%4)*.025f);item.mound.localRotation=Quaternion.Euler(0,seed*137.5f,0);
                if(item.glass)continue;int kind=seed%5;var v=item.visual;
                v.GetComponent<MeshFilter>().sharedMesh=rubbish[kind];v.GetComponent<Renderer>().sharedMaterials=new[]{kind==1?cream:kind==4?wood:metal,kind%2==0?coral:teal,metal};
                v.localScale=Vector3.one;v.localRotation=Quaternion.Euler(0,seed*137.5f,0);
            }
            // Replace only the earlier decorative silhouettes. Existing collision remains unchanged.
            var all=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach(var t in all.Where(t=>new[]{"Dune crest","Coral beach parasol","Sea oat","Sun-bleached driftwood","Resting bench","Bench footing","Beach canopy pole"}.Contains(t.name)))
            {var r=t.GetComponent<Renderer>();if(r)r.enabled=false;}
            var grass=Grass();var parasol=Parasol();var poleForm=new Form();poleForm.Lathe(new[]{0f,3.35f},new[]{.055f,.04f},0);var pole=poleForm.Save("Parasol timber pole");
            var plankForm=new Form();plankForm.Lathe(new[]{-.5f,.5f},new[]{.08f,.08f},0,6);var timber=plankForm.Save("Rounded beach timber");
            for(int i=0;i<3;i++)
            {
                float z=15+i*38;Art("Striped beach canopy",root,new Vector3(-23,2.72f,z),Vector3.one,parasol,i==1?teal:coral,cream,cream);
                Art("Round parasol pole",root,new Vector3(-23,-.1f,z),Vector3.one,pole,wood,wood,wood);
                for(int j=0;j<3;j++)
                {var slat=Art("Rounded bench slat",root,new Vector3(-22,.53f,z+1.8f+j*.21f),new Vector3(1,2.8f,1),timber,cream,cream,cream);slat.transform.rotation=Quaternion.Euler(0,0,90);}
                foreach(float x in new[]{-23f,-21f})Art("Bench trestle",root,new Vector3(x,.2f,z+2),new Vector3(1,.6f,2),timber,wood,wood,wood);
                var log=Art("Tide worn timber",root,new Vector3(-19,.065f,z+4),new Vector3(2.2f,4,2.2f),driftwood,wood,wood,wood);log.transform.rotation=Quaternion.Euler(90,32,0);
            }
            // Group dunes and grass at the seaward edge; keep the collection field open.
            for(int i=0;i<18;i++)
            {
                float z=2+i*6.1f;var p=new Vector3(-28+Mathf.Sin(i*2.1f)*.7f,-.10f,z);
                var dune=Art("Low wind swept dune",root,p,new Vector3(2.2f+(i%3)*.3f,.38f+(i%4)*.09f,3.6f),sands[i%4],sand,sand,sand);dune.transform.rotation=Quaternion.Euler(0,i*23,0);
                for(int j=0;j<3;j++)
                {
                    var tuft=p+new Vector3((j-1)*.65f,0,Mathf.Sin(i+j)*1.7f);tuft.y=SurfaceHeight(dune,tuft)-.015f;
                    Art("Dune grass cluster",root,tuft,Vector3.one*(.8f+(j%2)*.3f),grass,leaf,tips,leaf);
                }
            }
            // Preserve a quieter inland verge along the existing route.
            for(int i=0;i<44;i++)Art("Roadside sea grass",root,new Vector3(24.7f+Mathf.Sin(i)*.6f,-.1f,-12+i*4.3f),Vector3.one*(.8f+(i%3)*.15f),grass,leaf,tips,leaf);
            foreach(var r in root.GetComponentsInChildren<Renderer>())GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.BatchingStatic);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
