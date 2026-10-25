using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    // Bake a small strip around the actual stone/water intersections. The shader animates
    // the wash, so no runtime search, depth-texture dependency or per-rock Update is needed.
    public static class RockSurfPolish
    {
        const string Folder="Assets/Art/RockSurf";
        static float Cross(Vector2 a,Vector2 b,Vector2 c)=>(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
        static List<Vector2> Waterline(MeshFilter rock,float level)
        {
            var v=rock.sharedMesh.vertices.Select(rock.transform.TransformPoint).ToArray();var t=rock.sharedMesh.triangles;
            var points=new List<Vector2>();
            for(int i=0;i<t.Length;i+=3)for(int j=0;j<3;j++)
            {
                var a=v[t[i+j]];var b=v[t[i+(j+1)%3]];
                if((a.y<level)==(b.y<level)||Mathf.Abs(a.y-b.y)<.00001f)continue;
                var p=Vector3.Lerp(a,b,(level-a.y)/(b.y-a.y));var flat=new Vector2(p.x,p.z);
                if(!points.Any(q=>(q-flat).sqrMagnitude<.000001f))points.Add(flat);
            }
            points=points.OrderBy(p=>p.x).ThenBy(p=>p.y).ToList();
            if(points.Count<3)throw new System.InvalidOperationException(rock.name+" does not cross the water surface.");
            // The authored rocks are convex at the waterline; discard collinear triangle seams.
            var hull=new List<Vector2>();
            foreach(var p in points){while(hull.Count>=2&&Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=.00001f)hull.RemoveAt(hull.Count-1);hull.Add(p);}
            int lower=hull.Count;
            for(int i=points.Count-2;i>=0;i--){var p=points[i];while(hull.Count>lower&&Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=.00001f)hull.RemoveAt(hull.Count-1);hull.Add(p);}
            hull.RemoveAt(hull.Count-1);return hull;
        }
        [MenuItem("Sea Glass/Polish/Soft rock contact surf")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var landmarks=GameObject.Find("ART • cove landmarks");var water=GameObject.Find("Open turquoise water");
            if(!landmarks||!water)throw new System.InvalidOperationException("Create the coastal landmarks and ocean first.");
            float level=water.GetComponent<Renderer>().bounds.max.y;
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
            foreach(var rock in landmarks.GetComponentsInChildren<MeshFilter>())
            {
                List<Vector2> hull;try{hull=Waterline(rock,level);}catch(System.InvalidOperationException){continue;}   // fully submerged or fully above: no contact line
                var center=hull.Aggregate(Vector2.zero,(sum,p)=>sum+p)/hull.Count;
                var path=new List<Vector2>();
                for(int i=0;i<hull.Count;i++)
                {
                    var a=hull[i];var b=hull[(i+1)%hull.Count];int steps=Mathf.CeilToInt(Vector2.Distance(a,b)/.3f);
                    for(int j=0;j<steps;j++)path.Add(Vector2.Lerp(a,b,j/(float)steps));
                }
                int start=vertices.Count;
                foreach(var p in path)
                {
                    var outward=(p-center).normalized;
                    foreach(float distance in new[]{-.10f,1.10f})
                    {var q=p+outward*distance;vertices.Add(new Vector3(q.x,level+.016f,q.y));uv.Add(new Vector2(distance,0));}
                }
                for(int i=0;i<path.Count;i++)
                {
                    int a=start+i*2,b=start+(i+1)%path.Count*2;
                    indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});
                }
            }
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","RockSurf");
            string meshPath=Folder+"/Rock waterline strips.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(!mesh){mesh=new Mesh{name="Rock waterline strips"};AssetDatabase.CreateAsset(mesh,meshPath);}else mesh.Clear();
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);
            string materialPath=Folder+"/Broken coastal foam.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material){material=new Material(Shader.Find("Sea Glass/Rock Surf"));AssetDatabase.CreateAsset(material,materialPath);}
            EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);
            var old=GameObject.Find("ART • rock contact surf");if(old)Object.DestroyImmediate(old);
            var go=new GameObject("ART • rock contact surf");go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
