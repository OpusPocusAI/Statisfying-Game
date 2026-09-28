using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace SeaGlass.Editor
{
    // Focused, repeatable presentation pass: existing station positions and interaction colliders stay intact.
    public static class WorkbenchPolish
    {
        const string Folder="Assets/Art/CoastalPolish";
        static Transform root;
        static Material Paint(string name,Color color,float polish=.15f,string shader="Universal Render Pipeline/Lit")
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}
            m.shader=Shader.Find(shader);m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",polish);
            EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
        }
        static GameObject Detail(string name,Vector3 p,Vector3 size,Material m,Transform parent=null)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent?parent:root);
            go.transform.position=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=m;
            Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static void Fit(TextMesh text,float width)
        {
            text.characterSize=.025f;float actual=text.GetComponent<Renderer>().bounds.size.x;
            if(actual>.001f)text.characterSize*=Mathf.Min(1,width/actual);
        }
        [MenuItem("Sea Glass/Polish/Compose studio workbenches")]
        public static void Apply()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");
            var old=GameObject.Find("ART • workbench polish");if(old)Object.DestroyImmediate(old);
            root=new GameObject("ART • workbench polish").transform;
            var enamel=Paint("Basin porcelain",new Color(.64f,.81f,.76f),.32f);
            var timber=Paint("Tray honey timber",new Color(.48f,.3f,.16f));
            var paper=Paint("Workbench labels",new Color(.91f,.85f,.67f));
            var brass=Paint("Workbench brass",new Color(.62f,.42f,.19f),.4f);
            var water=Paint("Still basin water",new Color(.17f,.39f,.38f),.4f,"Sea Glass/Basin Water");
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name=="Enamel basin rim"||t.name=="Sorting tray wooden rim")t.gameObject.SetActive(false);
            foreach(var st in Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None))
            {
                if(st.kind!=StationKind.Clean&&st.kind!=StationKind.Sort)continue;
                var sign=st.transform.Find(st.name+" sign");
                sign.localPosition=new Vector3(st.kind==StationKind.Clean?-.75f:-2.2f,.72f,-.94f);sign.localScale=new Vector3(1.3f,.2f,.04f);
                sign.GetComponent<Renderer>().sharedMaterial=paper;
                var label=st.GetComponentsInChildren<TextMesh>().First(t=>t.text==st.name);
                label.transform.localPosition=sign.localPosition+new Vector3(0,0,-.028f);Fit(label,1.12f);
                var p=st.transform.position;
                if(st.kind==StationKind.Clean)
                {
                    st.transform.Find("Enamel basin").GetComponent<Renderer>().sharedMaterial=enamel;
                    st.waterSurface.sharedMaterial=water;
                    foreach(float x in new[]{-1f,1f})Detail("Low porcelain basin rim",p+new Vector3(x,1.27f,0),new Vector3(.075f,.075f,1.5f),enamel);
                    foreach(float z in new[]{-.72f,.72f})Detail("Low porcelain basin rim",p+new Vector3(0,1.27f,z),new Vector3(2.07f,.075f,.075f),enamel);
                    st.transform.Find("Brass faucet").GetComponent<Renderer>().sharedMaterial=brass;
                    var drain=GameObject.CreatePrimitive(PrimitiveType.Cylinder);drain.name="Basin drain";drain.transform.SetParent(root);
                    drain.transform.position=p+new Vector3(.67f,1.254f,.26f);drain.transform.localScale=new Vector3(.12f,.003f,.12f);
                    drain.GetComponent<Renderer>().sharedMaterial=brass;Object.DestroyImmediate(drain.GetComponent<Collider>());
                }
                else foreach(var tray in st.GetComponentsInChildren<SortingTray>())
                {
                    var color=GameCatalog.Palette[(int)tray.color];var pos=tray.transform.position;
                    tray.GetComponent<Renderer>().sharedMaterial=Paint("Matte tray "+tray.color,Color.Lerp(new Color(.19f,.28f,.26f),color,.18f));
                    foreach(float x in new[]{-.39f,.39f})Detail("Fine timber tray edge",pos+new Vector3(x,.06f,0),new Vector3(.035f,.07f,1.25f),timber);
                    foreach(float z in new[]{-.625f,.625f})Detail("Fine timber tray edge",pos+new Vector3(0,.06f,z),new Vector3(.81f,.07f,.035f),timber);
                    var plaque=Detail("Tray colour label",pos+new Vector3(0,.105f,.5f),new Vector3(.63f,.145f,.025f),paper);
                    plaque.transform.rotation=Quaternion.Euler(32,0,0);
                    var text=st.GetComponentsInChildren<TextMesh>().First(t=>t.text==tray.color.ToString());
                    text.transform.position=plaque.transform.position-plaque.transform.forward*.016f;
                    text.transform.rotation=plaque.transform.rotation;Fit(text,.4f);
                    // Small swatch keeps colour matching readable without making the entire tray glow.
                    Detail("Tray colour swatch",pos+new Vector3(0,.064f,-.5f),new Vector3(.28f,.009f,.09f),Paint("Swatch "+tray.color,color));
                }
            }
            // Brush silhouette: a wooden grip, brass ferrule and visible bristles rather than one block.
            var equipment=Object.FindFirstObjectByType<EquipmentVisuals>();
            var brush=equipment.brush.transform;var detail=brush.Find("Brush detail");if(detail)Object.DestroyImmediate(detail.gameObject);
            var brushRoot=new GameObject("Brush detail").transform;brushRoot.SetParent(brush,false);
            // Parent has a non-uniform primitive scale; use local dimensions deliberately.
            var band=Detail("Brush ferrule",Vector3.zero,Vector3.one,brass,brushRoot);
            band.transform.localPosition=new Vector3(0,-.25f,.16f);band.transform.localScale=new Vector3(1.08f,.5f,.36f);
            for(int i=0;i<6;i++)
            {
                var bristles=Detail("Soft brush bristles",Vector3.zero,Vector3.one,paper,brushRoot);
                bristles.transform.localPosition=new Vector3((i-2.5f)*.15f,-.68f,.16f);bristles.transform.localScale=new Vector3(.11f,.6f,.32f);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
