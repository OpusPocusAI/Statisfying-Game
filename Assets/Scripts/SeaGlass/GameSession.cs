using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace SeaGlass
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance {get;private set;}
        public GameCatalog catalog; public CoastalPlayer player; public UtilityTruck truck; public Progress progress=new();
        public string message="Welcome to Shell Cove"; public float messageUntil; public bool menu=true,paused; public bool testMode;
        public event Action Changed;
        public string saveFileName="seaglass-v1.json";
        public string SavePath => Path.Combine(Application.persistentDataPath,saveFileName);
        public int Count(ItemStage stage,bool glass) => progress.items.Count(r=>r.stage==stage && r.id.StartsWith(glass?"glass-":"rubbish-"));
        public int BucketCount => progress.items.Count(r=>r.stage==ItemStage.Bucket);
        public int CargoCount => progress.items.Count(r=>r.stage==ItemStage.Truck);
        void Awake(){Instance=this;Application.runInBackground=true;QualitySettings.vSyncCount=1;Application.targetFrameRate=90;}
        void Start(){player.blocked=true;player.SetCursor(false);Notify("A little care. A little colour.");}
        void Update()
        {
            var k=UnityEngine.InputSystem.Keyboard.current;
            if(k==null)return;
            if(k.escapeKey.wasPressedThisFrame && !menu){if(StudioUI.Instance.shop){StudioUI.Instance.CloseShop();return;}paused=!paused;Time.timeScale=paused?0:1;player.blocked=paused;player.SetCursor(!paused);if(paused)Save();}
            if(!menu&&!paused && k.hKey.wasPressedThisFrame) {progress.tutorialDismissed=!progress.tutorialDismissed;Notify(progress.tutorialDismissed?"Guidance hidden. H brings it back.":"Guidance restored.");Save();}
            if(!menu&&!paused && k.rKey.wasPressedThisFrame)truck.ResetVehicle();
            if(!menu&&!paused && k.fKey.wasPressedThisFrame){foreach(var item in FindObjectsByType<ShoreItem>(FindObjectsInactive.Include,FindObjectsSortMode.None))item.Highlight();Notify("Remaining glass is glowing along the shore.");}
        }
        public void Begin(bool load)
        {
            if(load&&!TryLoad())return;
            if(!load)progress=new Progress();
            foreach(var station in FindObjectsByType<StudioStation>(FindObjectsSortMode.None))station.ResetStation();
            foreach(var panel in FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None))panel.StopAllCoroutines();
            truck.driving=false;truck.ResetVehicle(false);StudioUI.Instance.shop=false;StudioUI.Instance.ending=false;
            menu=false;paused=false;Time.timeScale=1;player.mode=PlayerMode.Walking;player.interactOverride=null;player.transform.SetParent(null);player.blocked=false;
            player.Teleport(new Vector3(0,.2f,0),Quaternion.identity);player.SetCursor(true);
            if(load&&progress.returnToStudio){truck.transform.position=new Vector3(15,.3f,150);truck.ResetVehicle();player.Teleport(new Vector3(0,.2f,153),Quaternion.identity);}
            Synchronize();Notify(load?"Welcome back. Your studio is just up the coast.":"WASD to walk · mouse to look. Find the little sand mounds.");
        }
        public void Tutorial(int step){if(progress.tutorial<step){progress.tutorial=step;}}
        public void Award(int xp,int caps=0){progress.xp+=xp;progress.caps+=caps;Changed?.Invoke();}
        public void Notify(string text){message=text;messageUntil=Time.unscaledTime+5;Changed?.Invoke();}
        public bool Buy(string id)
        {
            var u=catalog.upgrades.FirstOrDefault(x=>x.id==id);if(u==null||progress.upgrades.Contains(id))return false;
            if(progress.Level<u.level){Notify("Reach level "+u.level+" to unlock this equipment.");return false;}
            if(progress.caps<u.cost){Notify("Recycle more rubbish to earn bottle caps.");return false;}
            progress.caps-=u.cost;progress.upgrades.Add(id);Tutorial(11);Notify(u.title+" is ready to use.");Feedback.Instance?.Play("upgrade");Synchronize();Save();return true;
        }
        public void Synchronize()
        {
            foreach(var i in FindObjectsByType<ShoreItem>(FindObjectsInactive.Include,FindObjectsSortMode.None))i.Refresh();
            foreach(var m in FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None))m.Refresh();
            foreach(var r in FindObjectsByType<RestorationDecor>(FindObjectsSortMode.None))r.SetRestored(progress.completed.All(x=>x));
            truck.RefreshCargo();Changed?.Invoke();
        }
        public bool TryLoad()
        {
            try {if(!File.Exists(SavePath))return false;var p=JsonUtility.FromJson<Progress>(File.ReadAllText(SavePath));
                if(p==null||p.version!=1||p.items==null||p.upgrades==null||p.completed==null||p.completed.Length!=3||p.items.Any(i=>i==null||string.IsNullOrEmpty(i.id))||p.items.Select(i=>i.id).Distinct().Count()!=p.items.Count)throw new InvalidDataException("Unsupported or damaged save");
                if(p.xp<0||p.caps<0||p.items.Any(i=>!Enum.IsDefined(typeof(ItemStage),i.stage)||float.IsNaN(i.grime)||float.IsInfinity(i.grime)||i.grime<0||i.grime>1)||p.upgrades.Any(u=>!catalog.upgrades.Any(d=>d.id==u)))throw new InvalidDataException("Invalid progress values");
                var known=FindObjectsByType<ShoreItem>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(i=>i.id).ToHashSet();
                if(p.items.Any(i=>!known.Contains(i.id)))throw new InvalidDataException("Unknown collectible ID");
                progress=p;return true;
            } catch(Exception ex){Notify("Could not load this save. Your file has been preserved. "+ex.Message);return false;}
        }
        public void Save()
        {
            if(menu||testMode)return;
            progress.returnToStudio=player.transform.position.z>140;
            try {Directory.CreateDirectory(Application.persistentDataPath);string tmp=SavePath+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(progress,true));if(File.Exists(SavePath))File.Replace(tmp,SavePath,SavePath+".bak");else File.Move(tmp,SavePath);}
            catch(Exception ex){Notify("Save failed: "+ex.Message);Debug.LogWarning("Sea Glass save: "+ex.Message);}
        }
        public void ReturnToMenu(){Save();menu=true;paused=false;Time.timeScale=1;player.blocked=true;player.SetCursor(false);}
        public void ResetProgress(){if(File.Exists(SavePath))File.Delete(SavePath);if(File.Exists(SavePath+".bak"))File.Delete(SavePath+".bak");Begin(false);}
        void OnApplicationQuit(){Save();Time.timeScale=1;}
        void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;}
    }
}
