using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SeaGlass
{
    // Explicit command-line acceptance check for the built player. Normal launches never create this object.
    public class PlayerVerification : MonoBehaviour
    {
        [Serializable] class Report
        {
            public string result,unity,gpu,cpu,resolution,performanceMode="Hidden player, explicit URP camera rendering; not a presented-window FPS benchmark";
            public float averageFrameMs,p95FrameMs;public long managedBytes;
            public string[] errors;public int placed,lightSpills;
        }
        readonly List<string> errors=new();string folder;GameSession session;RenderTexture captureTarget;float deadline;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Launch()
        {
            if(Application.isEditor||!Environment.GetCommandLineArgs().Contains("--verify-seaglass"))return;
            new GameObject("Standalone acceptance check").AddComponent<PlayerVerification>();
        }
        void Awake(){Application.logMessageReceived+=Log;deadline=Time.realtimeSinceStartup+120;}
        void LateUpdate()
        {
            if(Time.realtimeSinceStartup>deadline){Debug.LogError("Standalone verification timed out.");Application.Quit(1);return;}
            if(captureTarget&&session)
                RenderPipeline.SubmitRenderRequest(session.player.view,new UniversalRenderPipeline.SingleCameraRequest{destination=captureTarget});
        }
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void OnDestroy(){Application.logMessageReceived-=Log;if(captureTarget){captureTarget.Release();Destroy(captureTarget);}}
        IEnumerator Start()
        {
            folder=Path.Combine(Path.GetDirectoryName(Application.dataPath),"Verification");Directory.CreateDirectory(folder);
            var stack=new Stack<IEnumerator>();stack.Push(Run());
            while(stack.Count>0)
            {
                var flow=stack.Peek();
                bool next;
                try{next=flow.MoveNext();}
                catch(Exception e){File.WriteAllText(Path.Combine(folder,"failure.txt"),e.ToString());Debug.LogException(e);Application.Quit(1);yield break;}
                if(!next){stack.Pop();continue;}
                if(flow.Current is IEnumerator nested){stack.Push(nested);continue;}
                yield return flow.Current;
            }
        }
        void Require(bool value,string reason){if(!value)throw new InvalidOperationException(reason);}
        IEnumerator Shot(string name)
        {
            yield return new WaitForSeconds(.25f);yield return null;
            var previous=RenderTexture.active;RenderTexture.active=captureTarget;
            var t=new Texture2D(captureTarget.width,captureTarget.height,TextureFormat.RGB24,false);
            t.ReadPixels(new Rect(0,0,t.width,t.height),0,0);t.Apply();RenderTexture.active=previous;
            Require(t.GetPixels32().Any(p=>p.r>12||p.g>12||p.b>12),"Rendered camera image is not black");
            File.WriteAllBytes(Path.Combine(folder,name+".png"),t.EncodeToPNG());Destroy(t);
        }
        void View(Vector3 position,float yaw,Vector3 target)
        {session.player.Teleport(position,Quaternion.Euler(0,yaw,0));session.player.LookAt(target);session.player.blocked=true;session.player.SetCursor(false);session.player.tool.gameObject.SetActive(false);}
        IEnumerator Run()
        {
            yield return null;session=GameSession.Instance;Require(session,"Session loaded");session.testMode=true;
            // A hidden Windows player may suspend its swapchain. Render into an explicit camera target.
            // These captures verify 3D rendering; IMGUI menus are inspected separately in the live Editor.
            captureTarget=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);captureTarget.Create();session.player.view.enabled=false;
            yield return new WaitForSeconds(1);session.Begin(false);session.player.blocked=true;session.player.SetCursor(false);
            View(new Vector3(-15,.3f,14),-75,new Vector3(-34,0,24));yield return Shot("02-shoreline");
            var items=FindObjectsByType<ShoreItem>(FindObjectsSortMode.None);Require(items.Length==102,"102 shore finds");
            var glass=items.First(i=>i.id=="glass-03");glass.Use(10);Require(glass.Collect(),"Glass collection");
            var rubbish=items.First(i=>i.section==0&&i.id.StartsWith("rubbish-"));rubbish.Use(10);Require(rubbish.Collect(),"Rubbish collection");
            Require(session.truck.Load(true)&&session.truck.Load(false),"Separate truck loading");
            View(new Vector3(15,.3f,2),0,new Vector3(15,1.2f,7));yield return Shot("03-loaded-truck");
            session.truck.transform.position=new Vector3(15,.3f,150);
            var stations=FindObjectsByType<StudioStation>(FindObjectsSortMode.None);
            var clean=stations.First(s=>s.kind==StationKind.Clean);var sort=stations.First(s=>s.kind==StationKind.Sort);
            Require(stations.First(s=>s.kind==StationKind.Recycle).Recycle(),"Recycling");
            clean.Interact();yield return Shot("04-cloudy-glass");clean.Clean(4.5f);yield return Shot("05-half-clean");
            clean.Clean(5);clean.Leave();sort.Interact();yield return Shot("06-clean-glass");
            Require(sort.Sort(session.catalog.glass.First(d=>d.id==glass.id).color),"Correct sorting");sort.Leave();
            {var sortedTray=FindObjectsByType<SortingTray>(FindObjectsSortMode.None).First(t=>t.color==session.catalog.glass.First(d=>d.id==glass.id).color).transform.position;
            View(new Vector3(sortedTray.x,.3f,sortedTray.z-2.1f),0,sortedTray);}yield return Shot("07-sorted-tray");   // frame the tray that just received the piece
            foreach(var p in FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None))foreach(var socket in p.sockets)
            {session.progress.Item(socket.glassId).stage=ItemStage.Sorted;Require(p.Place(socket.glassId),"Placement "+socket.glassId);}
            View(new Vector3(-3,.3f,165),0,new Vector3(-3,1.5f,176));yield return new WaitForSeconds(3);
            Require(StudioUI.Instance.ending,"Restoration ending");yield return Shot("08-ending");
            StudioUI.Instance.ending=false;session.progress.tutorialDismissed=true;session.messageUntil=0;yield return Shot("09-restored-studio");
            foreach(var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))foreach(var m in r.sharedMaterials)
                Require(m&&m.shader&&m.shader.isSupported&&m.shader.name!="Hidden/InternalErrorShader","Material/shader on "+r.name);
            var samples=new List<float>();yield return new WaitForSeconds(1);
            for(int i=0;i<300;i++){yield return null;samples.Add(Time.unscaledDeltaTime*1000);}
            samples.Sort();var report=new Report{result=errors.Count==0?"PASS":"FAIL",unity=Application.unityVersion,gpu=SystemInfo.graphicsDeviceName,cpu=SystemInfo.processorType,resolution=Screen.width+"x"+Screen.height,averageFrameMs=samples.Average(),p95FrameMs=samples[(int)(samples.Count*.95f)],managedBytes=GC.GetTotalMemory(false),errors=errors.ToArray(),placed=session.Count(ItemStage.Placed,true),lightSpills=FindObjectsByType<MosaicLightSpill>(FindObjectsSortMode.None).Count(s=>s.surface.enabled)};
            Require(report.placed==30&&report.lightSpills==3,"Complete illuminated triptych");
            File.WriteAllText(Path.Combine(folder,"report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("Sea Glass standalone verification: "+report.result);Application.Quit(errors.Count==0?0:1);
        }
    }
}
