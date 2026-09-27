using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    // An additive, repeatable art pass. It does not regenerate gameplay or collectible IDs.
    public static class CoastalFinishPass
    {
        const string Folder="Assets/Art/CoastalPolish";
        static Transform root;
        static Material cream,wood,teal,coral,leaf,dark,sand,stone,brass;
        static Mesh rounded;
        static Material Original(string name)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/"+name+".mat");
        static Material Paint(string name,Color color,float smoothness=.15f)
        {
            string path=Folder+"/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smoothness);EditorUtility.SetDirty(m);return m;
        }
        static GameObject Box(string name,Vector3 position,Vector3 scale,Material mat,Transform parent=null,bool collision=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent?parent:root);
            go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static GameObject Round(string name,Vector3 position,Vector3 scale,Material mat,Transform parent=null)
        {
            var go=new GameObject(name);go.transform.SetParent(parent?parent:root);go.transform.position=position;go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=rounded;go.AddComponent<MeshRenderer>().sharedMaterial=mat;return go;
        }
        static void DisableNamed(params string[] names)
        {foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))if(names.Contains(t.name))t.gameObject.SetActive(false);}
        static Mesh RoundedStone()
        {
            string path=Folder+"/TumbledStone.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh)return mesh;
            var vertices=new List<Vector3>();var indices=new List<int>();
            // Four offset rings make flat, irregular faces without pointed pyramid silhouettes.
            for(int row=0;row<5;row++)for(int j=0;j<9;j++)
            {
                float latitude=Mathf.PI*row/4,angle=j*Mathf.PI*2/9+row*.11f;
                float radius=Mathf.Sin(latitude)*(.48f+.035f*Mathf.Sin(j*17+row*2));
                vertices.Add(new Vector3(Mathf.Cos(angle)*radius,Mathf.Cos(latitude)*.46f,Mathf.Sin(angle)*radius));
            }
            for(int row=0;row<4;row++)for(int j=0;j<9;j++)
            {int a=row*9+j,b=row*9+(j+1)%9,c=a+9,d=b+9;indices.AddRange(new[]{a,b,c,b,d,c});}
            mesh=new Mesh{name="Tumbled coastal stone"};mesh.vertices=indices.Select(i=>vertices[i]).ToArray();mesh.triangles=Enumerable.Range(0,indices.Count).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static void SandTexture()
        {
            string path=Folder+"/PaintedSand.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(!texture){texture=new Texture2D(128,128,TextureFormat.RGB24,true);texture.name="Quiet sand brushwork";AssetDatabase.CreateAsset(texture,path);}
            // Periodic coordinates keep the broad brush variation seamless at tile boundaries.
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {float u=x*Mathf.PI/64,v=y*Mathf.PI/64;float n=.97f+.02f*Mathf.Sin(u+Mathf.Sin(v))+.015f*Mathf.Sin(u*3-v*2);texture.SetPixel(x,y,new Color(n,n,n));}
            texture.wrapMode=TextureWrapMode.Repeat;texture.filterMode=FilterMode.Bilinear;texture.Apply();EditorUtility.SetDirty(texture);
            sand.mainTexture=texture;sand.mainTextureScale=new Vector2(.4f,1.2f);EditorUtility.SetDirty(sand);
        }
        static void Shoreline()
        {
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            // Sloping wet sand closes the abrupt drop from the original flat shelf into the sea.
            for(int i=0;i<=80;i++)
            {
                float z=-39+i*3,shore=-34.5f+Mathf.Sin(z*.07f)*1.1f+Mathf.Sin(z*.19f)*.3f;
                vertices.Add(new Vector3(-31.8f,-.09f,z));vertices.Add(new Vector3(shore,-.8f,z));vertices.Add(new Vector3(shore-2,-1.3f,z));
                colors.Add(Color.white);colors.Add(new Color(.84f,.86f,.85f));colors.Add(new Color(.7f,.8f,.78f));
                if(i<80)for(int j=0;j<2;j++){int a=i*3+j;triangles.AddRange(new[]{a,a+1,a+3,a+1,a+4,a+3});}
            }
            var path=Folder+"/ShoreSlope.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var go=new GameObject("Soft wet-sand shoreline");go.transform.SetParent(root);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Paint("Wet sand",new Color(.72f,.67f,.49f),.35f);
            DisableNamed("Foam ribbon");
            // Small, irregular drift lines replace the nine-stone rows across each cove.
            var rng=new System.Random(61);
            foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Wind-shaped coastal stone"))
            {
                f.sharedMesh=rounded;
                if(f.transform.position.x<25&&f.transform.position.x>-25)
                {var p=f.transform.position;p.z=Mathf.Round((p.z-19)/37)*37+19+(float)rng.NextDouble()*6-3;p.x=Mathf.Round((p.x+22)/2.8f)*2.8f-22+(float)rng.NextDouble()*1.8f-.9f;f.transform.position=p;f.transform.localScale=new Vector3(.975f,.52f,.715f);}
            }
            for(int i=0;i<16;i++)
            {
                float z=8+i*10.5f;var p=new Vector3(-26.5f+Mathf.Sin(i*2.4f)*1.3f,-.04f,z);
                Round("Beach pebble cluster",p,new Vector3(.55f,.25f,.45f),stone);
                Round("Beach pebble",p+new Vector3(.5f,0,.25f),new Vector3(.3f,.16f,.25f),cream);
            }
        }
        static void Vegetation()
        {
            // Readable wind-shaped canopy silhouettes, using three masses rather than diamond trees.
            foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Wind-shaped coastal stone"&&f.transform.position.x>23&&f.transform.position.y>2).ToArray())
            {
                f.GetComponent<Renderer>().enabled=false;var p=f.transform.position;
                Round("Wind-shaped coastal canopy",p+new Vector3(-.6f,.3f,0),new Vector3(4.5f,2.4f,3.7f),leaf);
                Round("Sunlit canopy crown",p+new Vector3(.7f,1.4f,.15f),new Vector3(3,1.7f,2.7f),Paint("Sage canopy",new Color(.29f,.47f,.32f)));
                Round("Coastal canopy shoulder",p+new Vector3(-1.7f,-.1f,.6f),new Vector3(2.6f,1.65f,2.6f),leaf);
            }
            DisableNamed("Restoring coastal succulent");
            for(int i=0;i<7;i++)
            {
                var p=new Vector3(-14+i*3.3f,.85f,153);
                for(int j=0;j<5;j++){float a=j*Mathf.PI*2/5;var g=Round("Succulent leaf",p+new Vector3(Mathf.Cos(a)*.15f,.1f,Mathf.Sin(a)*.15f),new Vector3(.18f,.52f,.12f),leaf);g.transform.rotation=Quaternion.Euler(Mathf.Sin(a)*35,0,Mathf.Cos(a)*35);var restore=g.AddComponent<RestorationDecor>();restore.surface=g.GetComponent<Renderer>();restore.restored=new Color(.39f,.66f,.43f);}
            }
        }
        static void Studio()
        {
            DisableNamed("Studio ocean wall","Ocean window frame","Ocean window pane","Window mullion");
            // Actual openings: no opaque wall or solid frame behind the ocean-facing windows.
            Box("Ocean wall sill",new Vector3(-16,.8f,166),new Vector3(.32f,1.6f,23),teal,null,true);
            Box("Ocean wall lintel",new Vector3(-16,4.6f,166),new Vector3(.32f,2,23),teal,null,true);
            float[] edges={154.5f,157.25f,160.75f,163.25f,166.75f,170.25f,173.75f,177.5f};
            for(int i=0;i<edges.Length;i+=2)Box("Ocean wall pier",new Vector3(-16,2.6f,(edges[i]+edges[i+1])*.5f),new Vector3(.32f,2,edges[i+1]-edges[i]),teal,null,true);
            foreach(float z in new[]{159f,165f,172f})
            {
                foreach(float y in new[]{1.6f,3.6f})Box("Window sill trim",new Vector3(-15.77f,y,z),new Vector3(.2f,.13f,3.65f),cream);
                foreach(float zz in new[]{z-1.75f,z+1.75f})Box("Window jamb",new Vector3(-15.77f,2.6f,zz),new Vector3(.18f,2.1f,.12f),cream);
                Box("Window crossbar",new Vector3(-15.78f,2.65f,z),new Vector3(.12f,.08f,3.5f),cream);
                Box("Window centre",new Vector3(-15.78f,2.6f,z),new Vector3(.12f,2,.08f),cream);
            }
            var paleWood=Paint("Honey floorboards",new Color(.66f,.48f,.3f));
            for(int i=0;i<42;i++)Box("Studio floorboard",new Vector3(-16.15f+i*.585f,.106f,166),new Vector3(.57f,.026f,22.7f),i%4==0?Paint("Sun-washed floorboard",new Color(.72f,.55f,.36f)):paleWood);
            Box("Entry threshold",new Vector3(-4,.14f,154.55f),new Vector3(24.5f,.12f,.3f),wood);
            for(int i=0;i<3;i++)
            {
                var p=new Vector3(-15.45f,1.4f,162+i*6);Box("Display shelf",p,new Vector3(.65f,.1f,1.9f),wood);
                for(int j=0;j<3;j++)Round("Studio pottery",p+new Vector3(0,.24f,(j-1)*.5f),new Vector3(.27f,.4f,.27f),j%2==0?cream:teal);
            }
            foreach(var st in Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None))
            {
                var sign=st.transform.Find(st.name+" sign");
                if(sign){sign.localScale=new Vector3(st.kind==StationKind.Sort?3.2f:2.1f,.38f,.12f);sign.localPosition=new Vector3(0,1.82f,.85f);}
                var label=st.GetComponentsInChildren<TextMesh>().First(t=>t.text==st.name);label.transform.localPosition=new Vector3(0,1.82f,.77f);
                if(st.kind==StationKind.Recycle)
                {
                    var feedback=st.GetComponent<RecyclingMotion>();if(!feedback)feedback=st.gameObject.AddComponent<RecyclingMotion>();
                    feedback.station=st;feedback.press=st.transform.Find("Recycling press");
                }
                if(st.kind==StationKind.Clean)
                {
                    var p=st.transform.position;
                    st.waterSurface.sharedMaterial=Original("Turquoise ocean");
                    foreach(float x in new[]{-1f,1f})Box("Enamel basin rim",p+new Vector3(x,1.31f,0),new Vector3(.13f,.16f,1.5f),cream);
                    foreach(float z in new[]{-.72f,.72f})Box("Enamel basin rim",p+new Vector3(0,1.31f,z),new Vector3(2.12f,.16f,.13f),cream);
                    Box("Faucet spout",p+new Vector3(.8f,1.8f,.32f),new Vector3(.08f,.08f,.4f),brass);
                    Box("Folded linen",p+new Vector3(-1.3f,1.15f,.1f),new Vector3(.34f,.08f,.65f),cream);
                }
                if(st.kind==StationKind.Sort)
                {
                    foreach(var tray in st.GetComponentsInChildren<SortingTray>())
                    {var p=tray.transform.position;foreach(float x in new[]{-.39f,.39f})Box("Sorting tray wooden rim",p+new Vector3(x,.07f,0),new Vector3(.045f,.13f,1.25f),cream);foreach(float z in new[]{-.625f,.625f})Box("Sorting tray wooden rim",p+new Vector3(0,.07f,z),new Vector3(.81f,.13f,.045f),cream);}
                }
            }
        }
        static void Lettering()
        {
            Material font=null;
            foreach(var text in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
            {
                var r=text.GetComponent<Renderer>();
                if(!font){font=Paint("World lettering",Color.white);font.shader=Shader.Find("Sea Glass/World Lettering");font.mainTexture=r.sharedMaterial.mainTexture;EditorUtility.SetDirty(font);}
                r.sharedMaterial=font;text.color=new Color(.06f,.23f,.25f);
                float maxWidth=0;
                var station=text.GetComponentInParent<StudioStation>();
                if(station&&text.text==station.name)maxWidth=station.kind==StationKind.Sort?2.9f:1.85f;
                if(text.text=="GLASS"||text.text=="RECYCLE")maxWidth=.78f;
                if(text.text.StartsWith("0")&&text.GetComponentInParent<MosaicPanel>()){maxWidth=4.6f;text.color=new Color(.95f,.86f,.65f);}
                if(maxWidth>0&&r.bounds.size.x>.001f)text.characterSize*=Mathf.Min(1,maxWidth/r.bounds.size.x);
            }
        }
        static void Mosaics()
        {
            foreach(var panel in Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None))
            {
                panel.backing.sharedMaterial=Paint("Slate grout",new Color(.23f,.29f,.32f));
                var p=panel.transform.position;
                foreach(float y in new[]{-1.76f,1.76f})Box("Mosaic brass inner frame",p+new Vector3(0,y,-.24f),new Vector3(4.85f,.055f,.045f),brass);
                foreach(float x in new[]{-2.4f,2.4f})Box("Mosaic brass inner frame",p+new Vector3(x,0,-.24f),new Vector3(.055f,3.55f,.045f),brass);
                var vertices=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();
                foreach(var slot in panel.sockets)
                {
                    var source=slot.piece.GetComponent<MeshFilter>();int offset=vertices.Count;
                    foreach(var v in source.sharedMesh.vertices)
                    {
                        var world=source.transform.TransformPoint(v);
                        vertices.Add(new Vector3(p.x+(world.x-p.x)*1.12f,.138f,p.z-3.7f-(world.y-p.y)*1.65f));
                        var color=slot.color;color.a=.3f;colors.Add(color);
                    }
                    // Only the front polygon: projecting both faces would double the light.
                    var sourceIndices=source.sharedMesh.triangles;int frontVertices=source.sharedMesh.vertexCount/2;
                    for(int i=0;i<sourceIndices.Length;i+=3)
                        if(sourceIndices[i]<frontVertices&&sourceIndices[i+1]<frontVertices&&sourceIndices[i+2]<frontVertices)
                            indices.AddRange(new[]{sourceIndices[i]+offset,sourceIndices[i+1]+offset,sourceIndices[i+2]+offset});
                }
                string path=Folder+"/ReflectedPanel"+panel.index+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.SetColors(colors);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
                var go=new GameObject("Reflected mosaic colours");go.transform.SetParent(root);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>();var material=Paint("Reflected glass light",Color.white);material.shader=Shader.Find("Sea Glass/Reflected Colour");renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                var spill=go.AddComponent<MosaicLightSpill>();spill.panel=panel;spill.surface=renderer;
            }
        }
        [MenuItem("Sea Glass/Polish/Apply painterly coast")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying||EditorSceneManager.GetActiveScene().path!=CoastBuilder.ScenePath)throw new InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","CoastalPolish");
            var old=GameObject.Find("ART • coastal polish");if(old)Object.DestroyImmediate(old);root=new GameObject("ART • coastal polish").transform;
            cream=Original("Warm chalk");wood=Original("Burnt orange timber");teal=Original("Lagoon paint");coral=Original("Coral paint");leaf=Original("Blue green plants");dark=Original("Purple shadows");sand=Original("Apricot sand");stone=Original("Coastal stone");brass=Paint("Brushed brass",new Color(.74f,.54f,.25f),.3f);
            sand.SetColor("_BaseColor",new Color(.88f,.77f,.58f));stone.SetColor("_BaseColor",new Color(.47f,.5f,.49f));leaf.SetColor("_BaseColor",new Color(.12f,.36f,.3f));
            rounded=RoundedStone();SandTexture();Shoreline();Vegetation();Studio();Lettering();Mosaics();
            var sky=Paint("Painted golden-hour sky",Color.white);sky.shader=Shader.Find("Sea Glass/Painted Sky");RenderSettings.skybox=sky;
            RenderSettings.fogColor=new Color(.92f,.79f,.64f);RenderSettings.fogDensity=.004f;
            RenderSettings.ambientSkyColor=new Color(.6f,.72f,.75f);RenderSettings.ambientEquatorColor=new Color(.6f,.48f,.47f);RenderSettings.ambientGroundColor=new Color(.3f,.27f,.35f);
            var sun=GameObject.Find("Golden hour sun").GetComponent<Light>();sun.color=new Color(1,.86f,.68f);sun.intensity=1.35f;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>())GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.BatchingStatic);
            foreach(var material in new[]{sand,stone,leaf,sky})EditorUtility.SetDirty(material);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
