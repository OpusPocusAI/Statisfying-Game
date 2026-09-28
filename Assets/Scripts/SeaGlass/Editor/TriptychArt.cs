using System.Linq;
using UnityEngine;
using UnityEditor;
namespace SeaGlass.Editor
{
    public static class TriptychArt
    {
        static Vector2[] P(params float[] xy)=>Enumerable.Range(0,xy.Length/2).Select(i=>new Vector2(xy[i*2],xy[i*2+1])).ToArray();
        static Vector2[] Rect(float x,float y,float w,float h)=>P(x,y,x+w,y,x+w,y+h,x,y+h);
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static float SignedArea(Vector2[] polygon)
        {float area=0;for(int i=0;i<polygon.Length;i++)area+=Cross(polygon[i],polygon[(i+1)%polygon.Length]);return area*.5f;}
        // A fan only works for convex outlines. Clip interior ears so curved wave/sun edges keep their authored gaps.
        static int[] Triangulate(Vector2[] polygon)
        {
            var remaining=Enumerable.Range(0,polygon.Length).ToList();var triangles=new System.Collections.Generic.List<int>();
            while(remaining.Count>3)
            {
                bool clipped=false;
                for(int i=0;i<remaining.Count;i++)
                {
                    int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];
                    if(Cross(polygon[b]-polygon[a],polygon[c]-polygon[b])<=.000001f)continue;
                    bool contains=remaining.Any(p=>p!=a&&p!=b&&p!=c&&
                        Cross(polygon[b]-polygon[a],polygon[p]-polygon[a])>=-.000001f&&
                        Cross(polygon[c]-polygon[b],polygon[p]-polygon[b])>=-.000001f&&
                        Cross(polygon[a]-polygon[c],polygon[p]-polygon[c])>=-.000001f);
                    if(contains)continue;
                    triangles.AddRange(new[]{a,b,c});remaining.RemoveAt(i);clipped=true;break;
                }
                if(!clipped)throw new System.InvalidOperationException("Mosaic outline cannot be triangulated without overlap.");
            }
            triangles.AddRange(remaining);return triangles.ToArray();
        }
        public static void Author(GameCatalog catalog)
        {
            Vector2[][][] polygons={
                new[]{Rect(-2,-1.45f,4,.52f),P(-2,-.88f,2,-.88f,1.1f,-.3f,-.4f,-.4f),P(-2,-.83f,-.5f,-.38f,-1,.3f,-2,.1f),P(-.45f,-.37f,1.13f,-.27f,.7f,.65f,-.22f,1.2f),P(-.18f,1.22f,.73f,.72f,.95f,1.05f,.25f,1.48f),P(.77f,.65f,1.15f,-.24f,1.52f,.15f,1.36f,.68f),P(.99f,1.03f,1.41f,.7f,1.55f,.22f,1.83f,.53f,1.6f,1.12f),P(-2,.18f,-1.04f,.35f,-.32f,1.27f,-.65f,1.45f,-2,1.45f),P(1.63f,1.18f,1.86f,.56f,2,.56f,2,1.45f,1.68f,1.45f),P(1.2f,-.85f,2,-.85f,2,.49f,1.85f,.46f,1.55f,.12f)},
                new[]{Rect(-2,-1.45f,4,.5f),P(-2,-.9f,2,-.9f,1.5f,-.45f,-1.5f,-.45f),P(-.55f,-.39f,.55f,-.39f,.3f,.86f,-.3f,.86f),Rect(-.33f,.92f,.66f,.36f),P(-.49f,1.32f,.49f,1.32f,0,1.63f),P(-2,.5f,-.4f,1.02f,-.4f,1.18f,-2,1.42f),P(.4f,1.02f,2,.5f,2,1.42f,.4f,1.18f),P(-2,-.4f,-.62f,-.4f,-.38f,.82f,-2,.42f),P(.62f,-.4f,2,-.4f,2,.42f,.38f,.82f),Rect(-.1f,-.37f,.2f,.36f)},
                new[]{P(-2,.4f,-.87f,.4f,-.55f,1.17f,0,1.48f,-2,1.48f),P(.87f,.4f,2,.4f,2,1.48f,0,1.48f,.55f,1.17f),P(-.8f,.4f,-.61f,.91f,-.28f,1.18f,-.04f,1.24f,-.04f,.4f),P(.04f,.4f,.04f,1.24f,.28f,1.18f,.61f,.91f,.8f,.4f),Rect(-2,-.05f,4,.39f),P(-2,-.51f,-.49f,-.51f,-.22f,-.11f,-2,-.11f),P(.49f,-.51f,2,-.51f,2,-.11f,.22f,-.11f),P(-.43f,-.51f,.43f,-.51f,.16f,-.11f,-.16f,-.11f),P(-2,-1.45f,-.2f,-1.45f,-.55f,-.58f,-2,-.58f),P(-.12f,-1.45f,2,-1.45f,2,-.58f,-.47f,-.58f)}
            };
            int[][] colors={new[]{2,0,2,0,4,2,4,0,3,2},new[]{2,1,4,3,3,4,4,0,0,5},new[]{5,5,3,3,0,2,2,3,1,2}};
            for(int panel=0;panel<3;panel++)for(int j=0;j<10;j++)
            {
                var d=catalog.glass[panel*10+j];var poly=polygons[panel][j];var center=poly.Aggregate(Vector2.zero,(a,b)=>a+b)/poly.Length;d.socket=center;d.color=(GlassColor)colors[panel][j];
                // The lighthouse door is an intentional overlay, not a coplanar part of the tower.
                d.depthOffset=panel==1&&j==9?-.02f:0;
                if(SignedArea(poly)<0)System.Array.Reverse(poly);
                var vertices=new Vector3[poly.Length*2];for(int i=0;i<poly.Length;i++){var v=(poly[i]-center)*.975f;vertices[i]=new Vector3(v.x,v.y,-.055f);vertices[i+poly.Length]=new Vector3(v.x,v.y,.055f);}
                var triangles=new System.Collections.Generic.List<int>();var face=Triangulate(poly);
                for(int i=0;i<face.Length;i+=3)triangles.AddRange(new[]{face[i],face[i+2],face[i+1],poly.Length+face[i],poly.Length+face[i+1],poly.Length+face[i+2]});
                for(int i=0;i<poly.Length;i++){int next=(i+1)%poly.Length;triangles.AddRange(new[]{i,next,next+poly.Length,i,next+poly.Length,i+poly.Length});}
                string path="Assets/Art/"+d.id+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.name=d.id+" original tumbled polygon";mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();d.mesh=mesh;EditorUtility.SetDirty(mesh);
            }
            EditorUtility.SetDirty(catalog);
        }
    }
}
