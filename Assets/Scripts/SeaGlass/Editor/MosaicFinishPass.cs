using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    public static class MosaicFinishPass
    {
        const string Folder="Assets/Art/CoastalPolish";
        static Transform root;
        static Material Paint(string name,Color color,float smooth=.15f)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
        }
        static GameObject Shape(string name,Vector3 position,Vector3 scale,Material material,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(root);go.transform.position=position;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        internal static void FeatherLight(MosaicPanel panel)
        {
            var spill=Object.FindObjectsByType<MosaicLightSpill>(FindObjectsSortMode.None).First(s=>s.panel==panel);
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();var p=panel.transform.position;
            foreach(var slot in panel.sockets)
            {
                // Use the authored flat polygon: the installed piece may show the rounded display mesh.
                var filter=slot.piece.GetComponent<MeshFilter>();var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
                var mesh=catalog.glass.First(g=>g.id==slot.glassId).mesh;int n=mesh.vertexCount/2,start=vertices.Count;
                var outline=new Vector2[n];
                for(int i=0;i<n;i++)
                {
                    var world=filter.transform.TransformPoint(mesh.vertices[i]);
                    outline[i]=new Vector2(p.x+(world.x-p.x)*1.12f,p.z-3.7f-(world.y-p.y)*1.65f);
                    vertices.Add(new Vector3(outline[i].x,.138f,outline[i].y));var c=slot.color;c.a=.22f;colors.Add(c);
                }
                // Keep the exact concave triangulation for the lit interior.
                var source=mesh.triangles;
                for(int i=0;i<source.Length;i+=3)if(source[i]<n&&source[i+1]<n&&source[i+2]<n)
                    triangles.AddRange(new[]{start+source[i],start+source[i+1],start+source[i+2]});
                float area=0;for(int i=0;i<n;i++){var a=outline[i];var b=outline[(i+1)%n];area+=a.x*b.y-b.x*a.y;}
                float winding=Mathf.Sign(area);
                for(int i=0;i<n;i++)
                {
                    var incoming=(outline[i]-outline[(i+n-1)%n]).normalized;var outgoing=(outline[(i+1)%n]-outline[i]).normalized;
                    var a=new Vector2(incoming.y,-incoming.x)*winding;var b=new Vector2(outgoing.y,-outgoing.x)*winding;
                    var normal=(a+b).normalized;float miter=.05f/Mathf.Max(.45f,Vector2.Dot(normal,b));var edge=outline[i]+normal*miter;
                    vertices.Add(new Vector3(edge.x,.138f,edge.y));var c=slot.color;c.a=0;colors.Add(c);
                }
                // A transparent outer ring feathers only the silhouette, not the internal triangle edges.
                for(int i=0;i<n;i++){int next=(i+1)%n;triangles.AddRange(new[]{start+i,start+next,start+n+next,start+i,start+n+next,start+n+i});}
            }
            var target=spill.GetComponent<MeshFilter>().sharedMesh;target.Clear();target.SetVertices(vertices);target.SetColors(colors);target.SetTriangles(triangles,0);target.RecalculateBounds();
            EditorUtility.SetDirty(target);AssetDatabase.SaveAssetIfDirty(target);
        }
        [MenuItem("Sea Glass/Polish/Compose mosaic gallery")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var old=GameObject.Find("ART • mosaic gallery");if(old)Object.DestroyImmediate(old);root=new GameObject("ART • mosaic gallery").transform;
            var teal=Paint("Gallery sea green",new Color(.12f,.25f,.24f));
            var timber=Paint("Gallery walnut",new Color(.32f,.17f,.105f));
            var brass=Paint("Gallery aged brass",new Color(.64f,.43f,.22f),.3f);
            var paper=Paint("Gallery title plaques",new Color(.86f,.79f,.62f));
            var lens=Paint("Gallery warm lens",new Color(.3f,.26f,.17f),.35f);lens.EnableKeyword("_EMISSION");EditorUtility.SetDirty(lens);AssetDatabase.SaveAssetIfDirty(lens);
            Shape("Gallery lower wall",new Vector3(-4,.38f,176.77f),new Vector3(24.5f,.65f,.1f),teal);
            Shape("Gallery chair rail",new Vector3(-4,.73f,176.67f),new Vector3(24.5f,.07f,.12f),timber);
            foreach(var panel in Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None))
            {
                var p=panel.transform.position;
                foreach(float x in new[]{-2.57f,2.57f})Shape("Gallery frame edge",p+new Vector3(x,0,-.2f),new Vector3(.11f,4.02f,.13f),timber);
                foreach(float y in new[]{-1.96f,1.96f})Shape("Gallery frame crown",p+new Vector3(0,y,-.2f),new Vector3(5.25f,.11f,.13f),timber);
                Shape("Mosaic title plaque",p+new Vector3(0,-2.22f,-.23f),new Vector3(2.7f,.28f,.07f),paper);
                var label=panel.GetComponentInChildren<TextMesh>();label.text="0"+(panel.index+1)+"  /  "+Object.FindFirstObjectByType<GameSession>().catalog.panelNames[panel.index];
                label.color=new Color(.08f,.24f,.23f);label.transform.position=p+new Vector3(0,-2.22f,-.276f);label.characterSize=.03f;
                float width=label.GetComponent<Renderer>().bounds.size.x;if(width>.01f)label.characterSize*=Mathf.Min(1,2.35f/width);
                foreach(float x in new[]{-.5f,.5f})Shape("Picture lamp bracket",p+new Vector3(x,2.1f,-.17f),new Vector3(.045f,.08f,.42f),brass);
                var shade=Shape("Picture lamp shade",p+new Vector3(0,2.1f,-.4f),new Vector3(.12f,.72f,.12f),brass,PrimitiveType.Cylinder);shade.transform.rotation=Quaternion.Euler(0,0,90);
                var glass=Shape("Picture lamp diffuser",p+new Vector3(0,2.04f,-.4f),new Vector3(1.25f,.022f,.075f),lens);
                var lightObject=new GameObject("Warm gallery pool");lightObject.transform.SetParent(root);lightObject.transform.position=p+new Vector3(0,1.45f,-.85f);
                var light=lightObject.AddComponent<Light>();light.type=LightType.Point;light.range=4;light.color=new Color(1,.77f,.49f);light.intensity=0;light.shadows=LightShadows.None;
                var accent=glass.AddComponent<MosaicAccent>();accent.panel=panel;accent.lamp=light;accent.lens=glass.GetComponent<Renderer>();
                FeatherLight(panel);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
