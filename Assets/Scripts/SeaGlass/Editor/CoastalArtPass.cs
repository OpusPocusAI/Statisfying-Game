using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace SeaGlass.Editor
{
    public static class CoastalArtPass
    {
        static Transform root;static Material cream,wood,teal,coral,dark,leaf;static Mesh stone;
        static Material M(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/"+n+".mat");
        static GameObject B(string n,Vector3 p,Vector3 s,Material m,Transform parent=null)=>CoastBuilder.Box(n,p,s,m,parent?parent:root);
        static GameObject Facet(string n,Vector3 p,Vector3 s,Material m)
        {var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.AddComponent<MeshFilter>().sharedMesh=stone;g.AddComponent<MeshRenderer>().sharedMaterial=m;return g;}
        static Mesh Stone()
        {
            string path="Assets/Art/FacetedCoastalStone.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh)return mesh;
            var a=new[]{new Vector3(0,.62f,0),new Vector3(.5f,.15f,0),new Vector3(.2f,.12f,.47f),new Vector3(-.35f,.1f,.35f),new Vector3(-.48f,.13f,-.2f),new Vector3(.2f,.13f,-.45f),new Vector3(.25f,-.35f,.02f),new Vector3(-.2f,-.32f,.15f),new Vector3(-.1f,-.3f,-.25f)};
            int[] ix={0,2,1,0,3,2,0,4,3,0,5,4,0,1,5,1,2,6,2,7,6,2,3,7,3,4,7,4,8,7,4,5,8,5,6,8,5,1,6,6,7,8};
            mesh=new Mesh{name="Wind-carved stone"};mesh.vertices=ix.Select(i=>a[i]).ToArray();mesh.triangles=Enumerable.Range(0,ix.Length).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        [MenuItem("Sea Glass/Apply Coastal Art")]
        public static void Apply()
        {
            var old=GameObject.Find("ART • finishing details");if(old)Object.DestroyImmediate(old);root=new GameObject("ART • finishing details").transform;
            cream=M("Warm chalk");wood=M("Burnt orange timber");teal=M("Lagoon paint");coral=M("Coral paint");dark=M("Purple shadows");leaf=M("Blue green plants");stone=Stone();
            foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(x=>x.name=="Wind-shaped coastal stone")){f.sharedMesh=stone;f.transform.rotation=Quaternion.Euler(0,f.transform.position.z*19,0);}
            var ocean=GameObject.Find("Open turquoise water").GetComponent<Renderer>();ocean.sharedMaterial.shader=Shader.Find("Sea Glass/Tidal Water");
            var rng=new System.Random(93);
            for(int i=0;i<95;i++)
            {
                float z=-15+i*2.1f;float x=i%2==0?24+(float)rng.NextDouble()*2:-24+(float)rng.NextDouble()*2;
                for(int j=0;j<3;j++){var blade=B("Sea oat",new Vector3(x+j*.13f,.33f,z),new Vector3(.07f,.65f+(float)rng.NextDouble()*.45f,.025f),leaf);blade.transform.rotation=Quaternion.Euler(0,j*60,(j-1)*20);Object.DestroyImmediate(blade.GetComponent<Collider>());}
                if(i%5==0)Facet("Dune crest",new Vector3(x,-.3f,z),new Vector3(5,1.7f,7),M("Apricot sand"));
            }
            // A handful of deliberate landmarks helps orientation along the short road.
            for(int i=0;i<3;i++)
            {
                float z=15+i*38;B("Beach canopy pole",new Vector3(-23,1.65f,z),new Vector3(.12f,3.3f,.12f),wood);
                Facet("Coral beach parasol",new Vector3(-23,3,z),new Vector3(4,.8f,4),i==1?teal:coral);
                B("Resting bench",new Vector3(-22,.52f,z+2),new Vector3(2.8f,.15f,.65f),cream);B("Bench footing",new Vector3(-22,.22f,z+2),new Vector3(2,.5f,.2f),wood);
                var log=B("Sun-bleached driftwood",new Vector3(-19,.18f,z+4),new Vector3(2.8f,.27f,.32f),wood);log.transform.rotation=Quaternion.Euler(0,32,0);
            }
            // Boardwalk and mooring posts frame the ocean-facing side of the studio.
            for(int i=0;i<20;i++)B("Boardwalk plank",new Vector3(-20,.03f,155+i*.85f),new Vector3(5,.16f,.8f),wood);
            for(int i=0;i<6;i++){B("Mooring post",new Vector3(-23,.65f,155+i*3),new Vector3(.22f,1.3f,.22f),cream);B("Boardwalk rail",new Vector3(-23,1,156.5f+i*3),new Vector3(.09f,.09f,3),wood);}
            for(int i=0;i<18;i++)B("Roof seam",new Vector3(-16+i*1.4f,5.91f,166),new Vector3(.08f,.05f,25),wood);
            // Studio beams, curtains, hanging lamps, pots and shelving are intentionally sparse.
            for(int i=0;i<4;i++)
            {
                float x=-13+i*6;B("Exposed ceiling beam",new Vector3(x,5.25f,166),new Vector3(.18f,.25f,22),wood);
                B("Pendant cord",new Vector3(x,4.8f,166),new Vector3(.025f,.7f,.025f),dark);
                var lamp=Facet("Warm pendant",new Vector3(x,4.3f,166),new Vector3(.7f,.4f,.7f),cream);var l=lamp.AddComponent<Light>();l.type=LightType.Point;l.range=7;l.color=new Color(1,.68f,.32f);l.intensity=0;var restore=lamp.AddComponent<RestorationDecor>();restore.lamp=l;
            }
            foreach(float z in new[]{159f,165f,172f})
            {
                B("Ocean window frame",new Vector3(-15.75f,2.7f,z),new Vector3(.12f,2.3f,3.2f),wood);
                B("Ocean window pane",new Vector3(-15.65f,2.7f,z),new Vector3(.06f,1.95f,2.9f),M("Glass Aqua"));
                B("Window mullion",new Vector3(-15.59f,2.7f,z),new Vector3(.06f,2,.07f),cream);
            }
            for(int i=0;i<7;i++)
            {
                var p=new Vector3(-14+i*3.3f,.4f,153);var pot=Facet("Terracotta planter",p,new Vector3(.75f,.8f,.75f),coral);pot.AddComponent<BoxCollider>();
                var plant=Facet("Restoring coastal succulent",p+Vector3.up*.55f,new Vector3(.85f,.9f,.85f),wood);var r=plant.AddComponent<RestorationDecor>();r.surface=plant.GetComponent<Renderer>();r.restored=new Color(.14f,.59f,.38f);
            }
            for(int i=0;i<12;i++)
            {var p=new Vector3(-15+i*2,4.05f,155.1f);var flag=Facet("Studio bunting",p,new Vector3(.6f,.7f,.08f),i%2==0?coral:teal);}
            B("Equipment pegboard",new Vector3(5,2.65f,171.4f),new Vector3(3.4f,2,.12f),wood);
            for(int i=0;i<4;i++){B("Tool on pegboard",new Vector3(3.9f+i*.72f,2.7f,171.2f),new Vector3(.09f,1,.09f),cream);B("Tool head",new Vector3(3.9f+i*.72f,3.2f,171.15f),new Vector3(.4f,.3f,.09f),i%2==0?teal:coral);}
            var player=Object.FindFirstObjectByType<CoastalPlayer>();var visuals=root.gameObject.AddComponent<EquipmentVisuals>();
            var bucket=B("Handheld cream bucket",Vector3.zero,new Vector3(.32f,.34f,.32f),cream,player.view.transform);bucket.transform.localPosition=new Vector3(.38f,-.38f,.65f);visuals.bucket=bucket;
            var fill=B("Bucket contents",Vector3.zero,new Vector3(.28f,.28f,.28f),dark,bucket.transform);fill.transform.localPosition=Vector3.zero;visuals.bucketFill=fill.transform;
            var sack=B("Canvas gathering sack",Vector3.zero,new Vector3(.36f,.43f,.31f),teal,player.view.transform);sack.transform.localPosition=new Vector3(.4f,-.4f,.7f);visuals.sack=sack;
            var sieve=B("Wide sieve attachment",Vector3.zero,new Vector3(.3f,.25f,.12f),teal,player.tool);sieve.transform.localPosition=new Vector3(0,-.3f,0);sieve.transform.localScale=new Vector3(4.4f,.65f,2);visuals.scoop=sieve;
            var brush=B("Wash brush",Vector3.zero,new Vector3(.1f,.07f,.3f),wood,player.view.transform);brush.transform.localPosition=new Vector3(.3f,-.3f,.7f);visuals.brush=brush;
            var spray=B("Hand sprayer",Vector3.zero,new Vector3(.1f,.13f,.4f),coral,player.view.transform);spray.transform.localPosition=new Vector3(.3f,-.3f,.7f);visuals.sprayer=spray;
            foreach(var c in player.view.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
            visuals.dividers=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Cargo rail").ToArray();
            RenderSettings.skybox.SetColor("_SkyTint",new Color(.65f,.62f,.53f));RenderSettings.skybox.SetFloat("_SunSize",.06f);
            RenderSettings.fogColor=new Color(.85f,.68f,.53f);RenderSettings.fogDensity=.0033f;
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
    }
}
