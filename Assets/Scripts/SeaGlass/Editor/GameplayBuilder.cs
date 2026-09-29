using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SeaGlass.Editor
{
    public static class GameplayBuilder
    {
        static Transform root;static Material cream,teal,coral,dark,wood,sand;static Material[] glass;
        static Material M(string name)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SeaGlass/"+name+".mat");
        static GameObject B(string n,Vector3 p,Vector3 size,Material mat,Transform parent=null)=>CoastBuilder.Box(n,p,size,mat,parent?parent:root);
        static GameObject Sphere(string n,Vector3 p,Vector3 scale,Material m,Transform parent)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=n;g.transform.SetParent(parent);g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
        static void NoCollider(GameObject g){foreach(var c in g.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);}
        public static GameCatalog Catalog()
        {
            var c=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");if(!c){c=ScriptableObject.CreateInstance<GameCatalog>();AssetDatabase.CreateAsset(c,"Assets/Data/SeaGlassCatalog.asset");}
            // The ten colour/shape assignments are fixed for each panel and are also used by its beach pieces.
            int[][] colors={new[]{2,2,0,0,4,2,0,0,4,0},new[]{2,4,3,4,2,0,4,3,4,0},new[]{5,3,3,3,5,2,0,3,0,2}};
            c.glass=Enumerable.Range(0,30).Select(i=>new GlassDefinition{id="glass-"+i.ToString("D2"),color=(GlassColor)colors[i/10][i%10],shape=i%5,section=i/10,panel=i/10,socket=new Vector2((i%5-2)*.69f,(i%10<5?.5f:-.35f))}).ToArray();
            c.upgrades=new[]{new UpgradeDefinition{id="scoop",title="Sieve scoop",description="Uncover and collect neighbouring finds in a wide circle.",level=2,cost=35},new UpgradeDefinition{id="bucket",title="Canvas collection sack",description="Carry 14 finds. Fewer trips across the sand.",level=2,cost=40},new UpgradeDefinition{id="truck",title="Tall cargo dividers",description="Forty secure cargo spaces for longer beach visits.",level=3,cost=65},new UpgradeDefinition{id="washer",title="Hand sprayer",description="A visible spray wand cleans glass in three seconds.",level=2,cost=50},new UpgradeDefinition{id="magnet",title="Sorting magnet & light table",description="Wide tray targeting and bright, generous mosaic guidance.",level=3,cost=55}};
            TriptychArt.Author(c);EditorUtility.SetDirty(c);return c;
        }
        [MenuItem("Sea Glass/Populate Gameplay")]
        public static void Populate()
        {
            var old=GameObject.Find("GAMEPLAY • Sea Glass Studio");if(old)Object.DestroyImmediate(old);
            root=new GameObject("GAMEPLAY • Sea Glass Studio").transform;
            cream=M("Warm chalk");teal=M("Lagoon paint");coral=M("Coral paint");dark=M("Purple shadows");wood=M("Burnt orange timber");sand=M("Apricot sand");
            glass=new Material[6];for(int i=0;i<6;i++){string path="Assets/Materials/SeaGlass/Glass "+(GlassColor)i+".mat";glass[i]=AssetDatabase.LoadAssetAtPath<Material>(path);if(!glass[i]){glass[i]=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(glass[i],path);}glass[i].color=GameCatalog.Palette[i];glass[i].SetFloat("_Smoothness",.72f);glass[i].SetFloat("_Metallic",.15f);glass[i].EnableKeyword("_EMISSION");glass[i].SetColor("_EmissionColor",GameCatalog.Palette[i]*.12f);}
            var session=root.gameObject.AddComponent<GameSession>();session.catalog=Catalog();session.player=Object.FindFirstObjectByType<CoastalPlayer>();session.player.blocked=true;
            foreach(var c in session.player.view.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
            root.gameObject.AddComponent<Feedback>();root.gameObject.AddComponent<StudioUI>();
            CreateTruck(session);
            var rng=new System.Random(41);
            foreach(var def in session.catalog.glass)
            {int local=int.Parse(def.id.Substring(6))%10;var p=new Vector3(-3-(local%5)*4.3f,.06f,7+def.section*37+(local/5)*7);CreateItem(def.id,def.section,true,(int)def.color,def.shape,p);}
            for(int i=0;i<72;i++)CreateItem("rubbish-"+i.ToString("D2"),i/24,false,i%6,i%5,new Vector3(-2-(float)rng.NextDouble()*22,.04f,3+i/24*37+(float)rng.NextDouble()*23));
            MakeStation("RECYCLING",StationKind.Recycle,new Vector3(5,0,159));
            MakeStation("WASH & REVEAL",StationKind.Clean,new Vector3(-.5f,0,159));
            MakeStation("SORT BY COLOUR",StationKind.Sort,new Vector3(-9,0,160));
            MakeStation("EQUIPMENT",StationKind.Upgrade,new Vector3(5,0,170));
            foreach(var item in Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).Where(i=>i.glass))
            {var def=session.catalog.glass.First(d=>d.id==item.id);item.visual.GetComponent<MeshFilter>().sharedMesh=def.mesh;item.visual.localScale=Vector3.one*(.55f/Mathf.Max(def.mesh.bounds.size.x,def.mesh.bounds.size.y));}
            foreach(var mat in glass)mat.shader=Shader.Find("Sea Glass/Frosted Glass");
            CreatePanels(3,session.catalog);
            PlayerSettings.runInBackground=true;
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void CreateItem(string id,int section,bool isGlass,int color,int shape,Vector3 position)
        {
            var go=new GameObject(id);go.transform.SetParent(root);go.transform.position=position;var item=go.AddComponent<ShoreItem>();item.id=id;item.section=section;item.glass=isGlass;item.colorIndex=color;item.home=position;
            var mound=Sphere("Soft sand mound",position,new Vector3(.9f,.24f,.75f),sand,go.transform);NoCollider(mound);item.mound=mound.transform;
            GameObject v;
            if(isGlass) {v=new GameObject("Cloudy glass shard");v.transform.SetParent(go.transform);v.transform.position=position;var f=v.AddComponent<MeshFilter>();f.sharedMesh=Shard(shape);v.AddComponent<MeshRenderer>().sharedMaterial=glass[color];v.transform.localScale=new Vector3(.65f,.22f,.65f);v.transform.rotation=Quaternion.Euler(90,shape*21,0);}
            else {v=GameObject.CreatePrimitive(shape==0?PrimitiveType.Cylinder:shape==1?PrimitiveType.Capsule:PrimitiveType.Cube);v.name=new[]{"Metal can","Plastic bottle","Scrap metal","Bottle cap","Driftwood"}[shape];v.transform.SetParent(go.transform);v.transform.position=position;v.transform.localScale=shape==4?new Vector3(.75f,.13f,.15f):new Vector3(.22f,.16f+shape*.035f,.22f);v.transform.rotation=Quaternion.Euler(0,shape*37,shape==1?65:0);v.GetComponent<Renderer>().sharedMaterial=shape==1?cream:shape==4?wood:dark;NoCollider(v);}
            item.visual=v.transform;go.AddComponent<BoxCollider>().size=new Vector3(.95f,.4f,.9f);
        }
        public static Mesh Shard(int shape)
        {
            var path="Assets/Art/GlassShape"+shape+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
            Vector2[][] polygons={new[]{new Vector2(-.5f,-.5f),new Vector2(.5f,-.4f),new Vector2(.1f,.5f)},new[]{new Vector2(-.5f,-.3f),new Vector2(.3f,-.5f),new Vector2(.5f,.3f),new Vector2(-.3f,.5f)},new[]{new Vector2(0,-.5f),new Vector2(.5f,0),new Vector2(0,.5f),new Vector2(-.5f,0)},new[]{new Vector2(-.45f,-.5f),new Vector2(.45f,-.35f),new Vector2(.4f,.4f),new Vector2(-.4f,.5f)},new[]{new Vector2(-.5f,-.2f),new Vector2(-.2f,-.5f),new Vector2(.4f,-.35f),new Vector2(.5f,.3f),new Vector2(0,.5f)}};
            var p=polygons[shape%5];var verts=new Vector3[p.Length*2];for(int i=0;i<p.Length;i++){verts[i]=new Vector3(p[i].x,p[i].y,-.07f);verts[i+p.Length]=new Vector3(p[i].x,p[i].y,.07f);}
            var tris=new System.Collections.Generic.List<int>();for(int i=1;i<p.Length-1;i++){tris.AddRange(new[]{0,i+1,i,p.Length,p.Length+i,p.Length+i+1});}for(int i=0;i<p.Length;i++){int n=(i+1)%p.Length;tris.AddRange(new[]{i,n,n+p.Length,i,n+p.Length,i+p.Length});}
            var m=new Mesh{name="Tumbled shard "+shape,vertices=verts,triangles=tris.ToArray()};m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);return m;
        }
        static void CreateTruck(GameSession s)
        {
            var go=new GameObject("TIDELINE • utility truck");go.transform.SetParent(root);go.transform.position=new Vector3(15,.3f,8);var truck=go.AddComponent<UtilityTruck>();s.truck=truck;
            var cc=go.GetComponent<CharacterController>();cc.center=new Vector3(0,1.1f,0);cc.radius=1;cc.height=1.8f;cc.stepOffset=.3f;
            var body=new GameObject("Sprung body").transform;body.SetParent(go.transform,false);truck.body=body;
            Vector3 P(float x,float y,float z)=>go.transform.position+new Vector3(x,y,z);
            B("Painted chassis",P(0,.65f,0),new Vector3(2,.4f,4.2f),teal,body);
            B("Cream bonnet",P(0,1.15f,1.4f),new Vector3(1.95f,.5f,1.1f),cream,body);
            B("Cab roof",P(0,2.6f,.55f),new Vector3(2.1f,.16f,1.8f),coral,body);
            foreach(float x in new[]{-.94f,.94f}){B("Cab pillar",P(x,1.93f,1.35f),new Vector3(.1f,1.3f,.1f),cream,body);B("Cab back pillar",P(x,1.93f,-.25f),new Vector3(.1f,1.3f,.1f),cream,body);B("Door",P(x,1.15f,.5f),new Vector3(.12f,.5f,1.3f),teal,body);}
            B("Seat",P(-.45f,1,.4f),new Vector3(.7f,.2f,.7f),dark,body);B("Seat back",P(-.45f,1.4f,.05f),new Vector3(.7f,.7f,.18f),dark,body);
            B("Dashboard",P(0,1.45f,1.05f),new Vector3(1.8f,.2f,.28f),dark,body);
            B("Front bumper",P(0,.57f,2.15f),new Vector3(2.2f,.16f,.15f),cream,body);
            foreach(float x in new[]{-.7f,.7f})B("Headlight",P(x,1.1f,1.97f),new Vector3(.33f,.23f,.04f),glass[4],body);
            truck.seat=new GameObject("Driver viewpoint").transform;truck.seat.SetParent(go.transform,false);truck.seat.localPosition=new Vector3(-.45f,2.1f,.5f);
            var w=new System.Collections.Generic.List<Transform>();foreach(float x in new[]{-1.04f,1.04f})foreach(float z in new[]{-1.4f,1.4f}){var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="Rubber wheel";wheel.transform.SetParent(body);wheel.transform.position=P(x,.45f,z);wheel.transform.localScale=new Vector3(.85f,.19f,.85f);wheel.transform.rotation=Quaternion.Euler(0,0,90);wheel.GetComponent<Renderer>().sharedMaterial=dark;w.Add(wheel.transform);}truck.wheels=w.ToArray();
            var gc=new System.Collections.Generic.List<Transform>();var rc=new System.Collections.Generic.List<Transform>();
            for(int i=0;i<2;i++)
            {
                float x=i==0?-.52f:.52f;var crate=B(i==0?"GLASS cargo crate":"RUBBISH cargo crate",P(x,1,-1.1f),new Vector3(.94f,.5f,1.6f),i==0?teal:coral,body);
                for(int side=0;side<2;side++)B("Cargo rail",P(x+(side==0?-.46f:.46f),1.4f,-1.1f),new Vector3(.05f,.55f,1.6f),cream,body);
                var comp=crate.AddComponent<TruckCompartment>();comp.truck=truck;comp.glass=i==0;
                CoastBuilder.Label(i==0?"GLASS":"RECYCLE",P(x,1.25f,-1.92f),.43f,null,body);
                for(int j=0;j<20;j++){var g=Sphere("Visible cargo "+j,P(x+(j%2-.5f)*.35f,1.32f+(j/6)*.13f,-1.65f+j%3*.38f),new Vector3(.25f,.17f,.3f),i==0?glass[j%6]:dark,body);NoCollider(g);g.SetActive(false);(i==0?gc:rc).Add(g.transform);}
            }
            truck.glassCargo=gc.ToArray();truck.rubbishCargo=rc.ToArray();
            var skip=B("Optional skip-drive plate",P(0,.71f,-2.14f),new Vector3(.7f,.3f,.09f),cream,body);var si=skip.AddComponent<TruckCompartment>();si.truck=truck;si.skip=true;CoastBuilder.Label("SKIP",P(0,.73f,-2.21f),.4f,null,body);
            foreach(var col in go.GetComponentsInChildren<Collider>())if(col!=cc&&!col.GetComponent<TruckCompartment>())Object.DestroyImmediate(col);
            var door=go.AddComponent<BoxCollider>();door.center=new Vector3(0,1.5f,.5f);door.size=new Vector3(2,1,1.5f);door.isTrigger=true;
            foreach(var col in go.GetComponentsInChildren<Collider>())if(col!=cc)col.isTrigger=true;
        }
        static void MakeStation(string title,StationKind kind,Vector3 p)
        {
            var go=new GameObject(title);go.transform.SetParent(root);go.transform.position=p;var station=go.AddComponent<StudioStation>();station.kind=kind;
            B("Timber tabletop",p+Vector3.up, new Vector3(kind==StationKind.Sort?6:3,.18f,1.8f),wood,go.transform);
            foreach(float x in new[]{-1.15f,1.15f})B("Bench leg",p+new Vector3(x,.45f,0),new Vector3(.18f,.9f,1.3f),teal,go.transform);
            var sign=B(title+" sign",p+new Vector3(0,2.2f,.85f),new Vector3(kind==StationKind.Sort?5.8f:3.1f,.65f,.12f),cream,go.transform);
            CoastBuilder.Label(title,p+new Vector3(0,2.2f,.77f),.65f,null,go.transform);
            if(kind==StationKind.Recycle){B("Recycling press",p+new Vector3(0,1.4f,.3f),new Vector3(1.7f,.7f,1),coral,go.transform);CoastBuilder.Label("FEED THE FUTURE",p+new Vector3(0,1.5f,-.22f),.42f,null,go.transform);}
            if(kind==StationKind.Clean)
            {
                B("Enamel basin",p+new Vector3(0,1.12f,0),new Vector3(2,.2f,1.3f),cream,go.transform);
                station.waterSurface=B("Basin water",p+new Vector3(0,1.24f,0),new Vector3(1.75f,.02f,1.1f),teal,go.transform).GetComponent<Renderer>();
                B("Brass faucet",p+new Vector3(.8f,1.5f,.5f),new Vector3(.07f,.6f,.07f),cream,go.transform);
            }
            if(kind==StationKind.Clean||kind==StationKind.Sort)
            {
                var piece=new GameObject("Working glass");piece.transform.SetParent(go.transform);piece.transform.position=p+new Vector3(0,1.35f,-.1f);piece.transform.rotation=Quaternion.Euler(90,0,0);piece.AddComponent<MeshFilter>().sharedMesh=Shard(1);station.glassRenderer=piece.AddComponent<MeshRenderer>();station.glassRenderer.sharedMaterial=glass[0];station.workingGlass=piece.transform;piece.transform.localScale=new Vector3(.5f,.5f,.5f);
            }
            if(kind==StationKind.Sort)for(int i=0;i<6;i++)
            {var tray=B(((GlassColor)i)+" tray",p+new Vector3((i-2.5f)*.85f,1.15f,0),new Vector3(.76f,.12f,1.2f),glass[i],go.transform);tray.AddComponent<SortingTray>().color=(GlassColor)i;CoastBuilder.Label(((GlassColor)i).ToString(),p+new Vector3((i-2.5f)*.85f,1.4f,.6f),.26f,null,go.transform);}
        }
        public static void CreatePanels(int count,GameCatalog catalog)
        {
            var panels=GameObject.Find("Triptych");if(panels)Object.DestroyImmediate(panels);panels=new GameObject("Triptych");panels.transform.SetParent(root?root:GameObject.Find("GAMEPLAY • Sea Glass Studio").transform);
            if(glass==null){glass=Enumerable.Range(0,6).Select(i=>M("Glass "+(GlassColor)i)).ToArray();wood=M("Burnt orange timber");dark=M("Purple shadows");cream=M("Warm chalk");}
            for(int i=0;i<count;i++)
            {
                var p=new Vector3(-10+i*7,2.75f,176.6f);var g=new GameObject(catalog.panelNames[i]);g.transform.SetParent(panels.transform);g.transform.position=p;var panel=g.AddComponent<MosaicPanel>();panel.index=i;
                B("Deep timber frame",p,new Vector3(5.2f,3.9f,.26f),wood,g.transform);panel.backing=B("Dark grout",p+new Vector3(0,0,-.17f),new Vector3(4.7f,3.4f,.1f),dark,g.transform).GetComponent<Renderer>();
                CoastBuilder.Label("0"+(i+1)+" / "+catalog.panelNames[i].ToUpperInvariant(),p+new Vector3(0,-2.28f,-.15f),.72f,null,g.transform);
                var sockets=new System.Collections.Generic.List<MosaicSocket>();foreach(var d in catalog.glass.Where(x=>x.panel==i))
                {
                    var at=p+new Vector3(d.socket.x,d.socket.y,-.27f+d.depthOffset);var socket=new GameObject("Socket "+d.id);socket.transform.SetParent(g.transform);socket.transform.position=at;socket.AddComponent<MeshFilter>().sharedMesh=d.mesh;socket.AddComponent<MeshRenderer>().sharedMaterial=dark;socket.AddComponent<MeshCollider>().sharedMesh=d.mesh;var s=socket.AddComponent<MosaicSocket>();s.glassId=d.id;s.panel=panel;s.color=GameCatalog.Palette[(int)d.color];
                    var piece=new GameObject(d.id+" · installed glass");piece.transform.SetParent(socket.transform,false);piece.transform.localPosition=new Vector3(0,0,-.07f);piece.transform.localScale=Vector3.one;piece.AddComponent<MeshFilter>().sharedMesh=d.mesh;s.surface=piece.AddComponent<MeshRenderer>();s.surface.sharedMaterial=glass[(int)d.color];s.piece=piece;piece.SetActive(false);sockets.Add(s);
                }
                panel.sockets=sockets.ToArray();var light=new GameObject("Transmitted "+catalog.panelNames[i]+" light");light.transform.SetParent(g.transform);light.transform.position=p+new Vector3(0,0,-1);panel.glow=light.AddComponent<Light>();panel.glow.type=LightType.Point;panel.glow.range=10;panel.glow.color=GameCatalog.Palette[i==0?0:i==1?4:3];panel.glow.intensity=0;
            }
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        [MenuItem("Sea Glass/Complete Triptych")]
        public static void CompleteTriptych(){CreatePanels(3,AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset"));}
    }
}
