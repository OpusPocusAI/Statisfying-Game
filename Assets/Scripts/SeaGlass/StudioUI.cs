using System.IO;
using System.Linq;
using UnityEngine;
namespace SeaGlass
{
    public class StudioUI : MonoBehaviour
    {
        public static StudioUI Instance; public bool shop,ending;bool settings,confirmReset;GUIStyle title,heading,body,small,button,panel;Texture2D cream,teal,coral;
        GameSession S=>GameSession.Instance;
        void Awake(){Instance=this;}
        public void CloseShop(){shop=false;S.RefreshControlState();}
        public void CloseEnding(){ending=false;S.RefreshControlState();}
        void Start(){S.player.sensitivity=PlayerPrefs.GetFloat("sensitivity",.12f);S.player.invertY=PlayerPrefs.GetInt("invert",0)==1;Feedback.Instance.musicVolume=PlayerPrefs.GetFloat("music",.25f);Feedback.Instance.effectsVolume=PlayerPrefs.GetFloat("effects",.65f);}
        Texture2D Tex(Color c)
        {
            var t=new Texture2D(32,32);t.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<32;y++)for(int x=0;x<32;x++){var p=new Vector2(Mathf.Max(7-x,x-24),Mathf.Max(7-y,y-24));var a=c;a.a*=1-Mathf.Clamp01(new Vector2(Mathf.Max(0,p.x),Mathf.Max(0,p.y)).magnitude-6);t.SetPixel(x,y,a);}t.Apply();return t;
        }
        void Styles()
        {
            cream=Tex(new Color(.98f,.91f,.76f,.96f));teal=Tex(new Color(.035f,.18f,.19f,.92f));coral=Tex(new Color(.92f,.37f,.23f));
            title=new GUIStyle(GUI.skin.label){fontSize=54,fontStyle=FontStyle.Bold,wordWrap=true};title.normal.textColor=new Color(.05f,.25f,.26f);
            heading=new GUIStyle(title){fontSize=24};body=new GUIStyle(heading){fontSize=17,fontStyle=FontStyle.Normal};small=new GUIStyle(body){fontSize=13};
            button=new GUIStyle(GUI.skin.button){fontSize=17,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(18,12,8,8)};button.normal.background=teal;button.normal.textColor=Color.white;button.hover.background=coral;button.hover.textColor=Color.white;button.active.background=coral;
            button.border=new RectOffset(8,8,8,8);panel=new GUIStyle(GUI.skin.box){padding=new RectOffset(24,24,20,20),border=new RectOffset(8,8,8,8)};panel.normal.background=cream;
        }
        void OnGUI()
        {
            if(!S)return;if(title==null)Styles();float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1440*scale)*.5f,0,0),Quaternion.identity,new Vector3(scale,scale,1));
            if(S.menu){Menu();return;}
            GUI.Box(new Rect(28,25,330,113),GUIContent.none,panel);GUI.Label(new Rect(47,37,290,28),"SEA GLASS / STUDIO",heading);
            GUI.Label(new Rect(47,71,290,27),"LEVEL "+S.progress.Level+"    ·    "+S.progress.xp+" XP    ·    "+S.progress.caps+" CAPS",small);
            GUI.Label(new Rect(47,101,290,22),"BUCKET "+S.BucketCount+" / "+S.progress.BucketCapacity+"     TRUCK "+S.CargoCount+" / "+S.progress.TruckCapacity,small);
            GUI.Box(new Rect(1025,25,387,102),GUIContent.none,panel);GUI.Label(new Rect(1047,39,345,28),"THE SEASIDE TRIPTYCH",heading);
            GUI.Label(new Rect(1047,76,345,24),string.Join("    /    ",Enumerable.Range(0,3).Select(p=>S.progress.items.Count(r=>r.stage==ItemStage.Placed&&S.catalog.glass.Any(g=>g.id==r.id&&g.panel==p))+" of 10")),body);
            if(S.paused){Pause();return;}if(shop){Shop();return;}if(ending){Ending();return;}
            var f=S.player.focus;
            GUI.color=Color.white;GUI.DrawTexture(new Rect(717,447,6,6),cream);
            if(f){GUI.Box(new Rect(440,550,560,48),GUIContent.none,panel);GUI.Label(new Rect(457,559,530,30),f.Prompt,body);}
            var station=FindObjectsByType<StudioStation>(FindObjectsSortMode.None).FirstOrDefault(s=>s.active);
            if(!S.progress.tutorialDismissed){GUI.Box(new Rect(28,770,640,104),GUIContent.none,panel);GUI.Label(new Rect(47,784,600,55),TutorialGuidance.Get(S,station),body);}
            GUI.Label(new Rect(47,845,620,23),"H guidance    F reveal glass    R recover truck    Esc pause",small);
            GUI.Label(new Rect(1080,827,340,48),S.player.equippedTool==1?(S.progress.upgrades.Contains("bucket")?"CANVAS SACK  /  14 finds":"SMALL BUCKET  /  7 finds"):(S.progress.upgrades.Contains("scoop")?"SIEVE SCOOP  /  wide gathering":"TOY SHOVEL  /  gentle beginnings"),small);
            if(Time.unscaledTime<S.messageUntil){GUI.Box(new Rect(375,160,690,68),GUIContent.none,panel);GUI.Label(new Rect(395,176,650,48),S.message,body);}
            if(station&&station.kind==StationKind.Clean&&station.selectedId!=null){float clean=1-S.progress.Item(station.selectedId).grime;GUI.Box(new Rect(540,670,360,60),GUIContent.none,panel);GUI.Label(new Rect(555,678,340,25),"HOLD LMB  ·  POLISHING "+Mathf.RoundToInt(clean*100)+"%",body);GUI.DrawTexture(new Rect(555,713,330*clean,5),coral);}
        }
        bool Button(string label,float x,float y,float w=340)=>GUI.Button(new Rect(x,y,w,46),label,button);
        void Menu()
        {
            GUI.Box(new Rect(70,95,555,710),GUIContent.none,panel);GUI.Label(new Rect(110,125,440,30),"A SMALL COASTAL RESTORATION",small);
            GUI.Label(new Rect(105,177,460,140),"Sea Glass\nStudio",title);GUI.Label(new Rect(110,339,430,80),"Bring the shore back to life.\nTurn small discoveries into something luminous.",body);
            GUI.enabled=File.Exists(S.SavePath);if(Button("Continue your studio",110,455,440)){S.Begin(true);}GUI.enabled=true;
            if(!File.Exists(S.SavePath))GUI.Label(new Rect(110,427,440,20),"No saved studio yet — begin a new visit below.",small);
            if(Button("Begin a new visit",110,515,440)){if(File.Exists(S.SavePath))confirmReset=true;else S.Begin(false);}
            if(Button("Settings",110,575,440))settings=!settings;
            if(Button("Quit",110,635,440))Application.Quit();
            GUI.Label(new Rect(110,715,440,50),"WALK · GATHER · RESTORE\nOriginal geometry & sound. Made for a slower afternoon.",small);
            if(settings)Settings();if(confirmReset)Confirm();
        }
        void Pause()
        {
            GUI.Box(new Rect(480,175,480,550),GUIContent.none,panel);GUI.Label(new Rect(525,208,390,55),"Take a breath",heading);
            if(Button("Return to the coast",550,292))S.SetPaused(false);
            if(Button("Save studio",550,350)){S.Save();S.Notify("Studio saved.");}
            if(Button("Settings",550,408))settings=!settings;
            if(Button("Main menu",550,466))S.ReturnToMenu();
            if(Button("Reset progress…",550,524))confirmReset=true;
            if(Button("Recover truck",550,582))S.truck.ResetVehicle();
            if(settings)Settings();if(confirmReset)Confirm();
        }
        void Confirm()
        {
            GUI.Box(new Rect(480,290,500,260),GUIContent.none,panel);GUI.Label(new Rect(510,315,440,85),"Start a new studio?\nThis replaces your current progress.",heading);
            if(Button("Keep my progress",510,420,210))confirmReset=false;
            if(Button("Reset & begin",735,420,210)){confirmReset=false;S.ResetProgress();}
        }
        void Settings()
        {
            GUI.Box(new Rect(700,170,620,570),GUIContent.none,panel);GUI.Label(new Rect(735,198,550,40),"Make yourself comfortable",heading);
            float y=265;GUI.Label(new Rect(735,y,490,30),"Music",body);Feedback.Instance.musicVolume=GUI.HorizontalSlider(new Rect(735,y+38,520,22),Feedback.Instance.musicVolume,0,1);
            y+=85;GUI.Label(new Rect(735,y,490,30),"Effects",body);Feedback.Instance.effectsVolume=GUI.HorizontalSlider(new Rect(735,y+38,520,22),Feedback.Instance.effectsVolume,0,1);
            y+=85;GUI.Label(new Rect(735,y,490,30),"Mouse sensitivity",body);S.player.sensitivity=GUI.HorizontalSlider(new Rect(735,y+38,520,22),S.player.sensitivity,.025f,.35f);
            S.player.invertY=GUI.Toggle(new Rect(735,525,230,35),S.player.invertY," Invert mouse Y",body);
            if(Button("Toggle fullscreen",735,572,250))Screen.fullScreen=!Screen.fullScreen;
            if(Button("1280 × 720",1000,572,250))Screen.SetResolution(1280,720,Screen.fullScreen);
            if(Button("Save settings",735,642,250)){PlayerPrefs.SetFloat("music",Feedback.Instance.musicVolume);PlayerPrefs.SetFloat("effects",Feedback.Instance.effectsVolume);PlayerPrefs.SetFloat("sensitivity",S.player.sensitivity);PlayerPrefs.SetInt("invert",S.player.invertY?1:0);PlayerPrefs.Save();settings=false;}
            if(Button("1920 × 1080",1000,642,250))Screen.SetResolution(1920,1080,Screen.fullScreen);
        }
        void Shop()
        {
            GUI.Box(new Rect(285,140,870,610),GUIContent.none,panel);GUI.Label(new Rect(320,170,780,48),"TOOLS FOR A LITTLE MORE CARE",heading);int i=0;
            foreach(var u in S.catalog.upgrades)
            {float y=235+i++*83;bool owned=S.progress.upgrades.Contains(u.id);GUI.Label(new Rect(320,y,505,26),u.title+"   ·   LEVEL "+u.level,body);GUI.Label(new Rect(320,y+28,505,43),u.description,small);if(Button(owned?"Owned":u.cost+" caps",870,y,245)&&!owned)S.Buy(u.id);}
            if(Button("Back to the studio",780,675,340))CloseShop();
        }
        void Ending()
        {
            GUI.Box(new Rect(385,265,670,300),GUIContent.none,panel);GUI.Label(new Rect(425,298,590,65),"Three shores. One home.",heading);GUI.Label(new Rect(425,365,590,100),"Thirty small discoveries now hold the afternoon light.\nThe studio is yours again.\nThank you for leaving the coast a little better.",body);
            if(Button("Stay a little longer",525,492,390))CloseEnding();
        }
        void OnDestroy(){if(cream)Destroy(cream);if(teal)Destroy(teal);if(coral)Destroy(coral);if(Instance==this)Instance=null;}
    }
}
