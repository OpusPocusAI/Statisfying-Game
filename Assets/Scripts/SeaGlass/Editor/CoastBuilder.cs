using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.ProBuilder;
using System.IO;

namespace SeaGlass.Editor
{
    public static class CoastBuilder
    {
        public const string ScenePath="Assets/Scenes/SeaGlassStudio.unity";
        static Material sand, wood, cream, teal, dark, coral, rock, water, leaf;
        static Transform root;
        static Material Mat(string name,Color color,float smooth=.15f)
        {
            var path="Assets/Materials/SeaGlass/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.color=color;m.SetFloat("_Smoothness",smooth);return m;
        }
        public static GameObject Box(string name,Vector3 p,Vector3 s,Material m,Transform parent=null,bool pb=false)
        {
            GameObject g;
            if(pb){var mesh=ShapeGenerator.GenerateCube(PivotLocation.Center,s);g=mesh.gameObject;mesh.ToMesh();mesh.Refresh();if(!g.GetComponent<Collider>())g.AddComponent<BoxCollider>();}
            else {g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.transform.localScale=s;}
            g.name=name;g.transform.SetParent(parent?parent:root,false);g.transform.position=p;g.GetComponent<Renderer>().sharedMaterial=m;return g;
        }
        static GameObject Rock(Vector3 p,Vector3 s,Material m)
        { var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Wind-shaped coastal stone";g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.transform.rotation=Quaternion.Euler(10,p.x*21,15);g.GetComponent<Renderer>().sharedMaterial=m;return g; }
        public static void Label(string text,Vector3 position,float size=1,Quaternion? rotation=null,Transform parent=null)
        {
            var g=new GameObject(text);g.transform.SetParent(parent?parent:root);g.transform.position=position;g.transform.rotation=rotation??Quaternion.identity;
            var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=size*.1f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.06f,.23f,.25f);
        }
        [MenuItem("Sea Glass/Build Foundation Scene")]
        public static void Build()
        {
            foreach(var folder in new[]{"Art","Audio","Materials/SeaGlass","Prefabs","Data","UI","VFX","Tests"}) Directory.CreateDirectory("Assets/"+folder);
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            root=new GameObject("COAST • authored environment").transform;
            sand=Mat("Apricot sand",new Color(.83f,.65f,.42f));wood=Mat("Burnt orange timber",new Color(.52f,.23f,.13f));cream=Mat("Warm chalk",new Color(.96f,.86f,.64f));
            teal=Mat("Lagoon paint",new Color(.06f,.45f,.46f));dark=Mat("Purple shadows",new Color(.15f,.16f,.23f));coral=Mat("Coral paint",new Color(.93f,.35f,.22f));
            rock=Mat("Coastal stone",new Color(.49f,.43f,.43f));water=Mat("Turquoise ocean",new Color(.08f,.61f,.64f),.6f);leaf=Mat("Blue green plants",new Color(.09f,.31f,.27f));
            Box("Continuous sand shelf",new Vector3(0,-.6f,80),new Vector3(64,1,235),sand,null,true);
            Box("Open turquoise water",new Vector3(-88,-1,75),new Vector3(120,.3f,360),water);
            Box("Coast road",new Vector3(15,-.04f,79),new Vector3(9,.18f,198),rock,null,true);
            for(int z=-16;z<178;z+=9) Box("Painted road marker",new Vector3(15,.06f,z),new Vector3(.18f,.015f,3),cream);
            for(int z=-24;z<195;z+=7)
            {
                Rock(new Vector3(30,1,z),new Vector3(8,5,9),rock);
                Rock(new Vector3(-28,-.25f,z),new Vector3(4,1,5),cream);
                if(z%3==0){Rock(new Vector3(26,3,z),new Vector3(5,6,5),leaf); Box("Plant trunk",new Vector3(26,1,z),new Vector3(.6f,3,.6f),wood);}
            }
            // Invisible recovery boundary is outside the visible rock and surf silhouettes.
            foreach(var p in new[]{new Vector3(-33,3,80),new Vector3(34,3,80)}){var g=Box("Coastal boundary",p,new Vector3(1,10,238),sand);g.GetComponent<Renderer>().enabled=false;}
            foreach(float z in new[]{-38f,198f}){var g=Box("Coastal end boundary",new Vector3(0,3,z),new Vector3(70,10,1),sand);g.GetComponent<Renderer>().enabled=false;}
            for(int i=0;i<24;i++)
            { var stripe=Box("Foam ribbon",new Vector3(-31-i%3*3,-.79f,i*11-45),new Vector3(.3f,.03f,6+i%4),cream);stripe.transform.rotation=Quaternion.Euler(0,i%2*9,0); }
            // Studio is open toward the road; the footprint leaves space around every workbench.
            Box("Studio raised slab",new Vector3(-4,0,166),new Vector3(25,.18f,23),cream,null,true);
            Box("Studio back wall",new Vector3(-4,2.8f,177),new Vector3(25,5.6f,.35f),cream,null,true);
            Box("Studio ocean wall",new Vector3(-16,2.8f,166),new Vector3(.35f,5.6f,23),teal,null,true);
            Box("Studio canopy",new Vector3(-4,5.7f,166),new Vector3(27,.35f,25),coral,null,true);
            foreach(float x in new[]{-15.5f,7.5f})foreach(float z in new[]{155.5f,176f})Box("Canopy post",new Vector3(x,2.8f,z),new Vector3(.3f,5.6f,.3f),wood);
            Box("Studio fascia",new Vector3(-4,4.6f,154.5f),new Vector3(25,1.4f,.35f),cream);
            Label("SEA GLASS  /  STUDIO",new Vector3(-4,4.65f,154.27f),1.9f);
            Label("a little care. a little colour.",new Vector3(-4,3.8f,154.25f),.75f);
            for(int i=0;i<3;i++)
            {
                float z=4+i*37;
                Box("Beach section sign",new Vector3(5,1.7f,z),new Vector3(5,1.1f,.2f),cream);
                Box("Sign post",new Vector3(5,.7f,z),new Vector3(.15f,1.6f,.15f),wood);
                Label(new[]{"01  /  SHELL COVE","02  /  TIDE GARDENS","03  /  SUNSET POINT"}[i],new Vector3(5,1.72f,z-.13f),.7f);
                for(int j=0;j<9;j++){float x=-22+j*2.8f;Rock(new Vector3(x,.1f,z+15),new Vector3(1.5f,.8f,1.1f),rock);}
            }
            // Distant islands compose the horizon without adding traversable area.
            for(int i=0;i<6;i++)Rock(new Vector3(-100-i*18,-4,30+i*36),new Vector3(28,16+i*2,45),leaf);
            var sun=new GameObject("Golden hour sun").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.77f,.52f);sun.intensity=1.65f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(26,-38,0);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.55f,.69f,.75f);RenderSettings.ambientEquatorColor=new Color(.52f,.37f,.4f);RenderSettings.ambientGroundColor=new Color(.25f,.2f,.31f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.92f,.69f,.48f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0024f;
            var sky=new Material(Shader.Find("Skybox/Procedural"));sky.SetColor("_SkyTint",new Color(.65f,.78f,.81f));sky.SetColor("_GroundColor",new Color(.86f,.53f,.37f));sky.SetFloat("_AtmosphereThickness",.75f);AssetDatabase.CreateAsset(sky,"Assets/Materials/SeaGlass/Golden sky.mat");RenderSettings.skybox=sky;
            var pgo=new GameObject("Player");pgo.transform.position=new Vector3(0,1,0);var cc=pgo.AddComponent<CharacterController>();cc.height=1.8f;cc.center=new Vector3(0,.9f,0);cc.radius=.3f;cc.stepOffset=.3f;cc.slopeLimit=48;
            var player=pgo.AddComponent<CoastalPlayer>();var cam=new GameObject("First-person camera");cam.transform.SetParent(pgo.transform,false);cam.transform.localPosition=new Vector3(0,1.6f,0);player.view=cam.AddComponent<Camera>();player.view.nearClipPlane=.05f;player.view.farClipPlane=450;cam.tag="MainCamera";cam.AddComponent<AudioListener>();
            var handle=Box("Toy shovel handle",Vector3.zero,new Vector3(.07f,.45f,.06f),wood,cam.transform);handle.transform.localPosition=new Vector3(.38f,-.37f,.6f);player.tool=handle.transform;
            var blade=Box("Toy shovel blade",Vector3.zero,new Vector3(.2f,.22f,.05f),coral,handle.transform);blade.transform.localPosition=new Vector3(0,-.25f,0);blade.transform.localScale=new Vector3(2.8f,.5f,1);
            player.blocked=false;player.SetCursor(false);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();
        }
    }
}
