using System.IO;
using System.Linq;
using UnityEngine;
namespace SeaGlass
{
    public class StudioUI : MonoBehaviour
    {
        public static StudioUI Instance; public bool shop,ending,intro;bool settings,confirmReset;[System.NonSerialized] GUIStyle title,heading,body,small,button,panel;[System.NonSerialized] Texture2D cream,teal,coral;
        GameSession S=>GameSession.Instance;
        void Awake(){Instance=this;}
        public void CloseShop(){shop=false;S.RefreshControlState();}
        public void CloseEnding(){ending=false;S.RefreshControlState();}
        public void CloseIntro(){intro=false;S.progress.introSeen=true;S.RefreshControlState();S.Save();}
        void Start(){S.player.sensitivity=PlayerPrefs.GetFloat("sensitivity",.12f);S.player.invertY=PlayerPrefs.GetInt("invert",0)==1;Feedback.Instance.musicVolume=PlayerPrefs.GetFloat("music",.25f);Feedback.Instance.effectsVolume=PlayerPrefs.GetFloat("effects",.65f);}
        Texture2D Tex(Color c)
        {
            var t=new Texture2D(32,32);t.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<32;y++)for(int x=0;x<32;x++){var p=new Vector2(Mathf.Max(7-x,x-24),Mathf.Max(7-y,y-24));var a=c;a.a*=1-Mathf.Clamp01(new Vector2(Mathf.Max(0,p.x),Mathf.Max(0,p.y)).magnitude-6);t.SetPixel(x,y,a);}t.Apply();return t;
        }
        // Paper card (Lil Gator Game reference): warm paper with fine grain, a slightly uneven edge
        // and a soft drop shadow baked into the 9-slice border.
        Texture2D Paper()
        {
            const int S=64;var t=new Texture2D(S,S){wrapMode=TextureWrapMode.Clamp};var rng=new System.Random(4);
            for(int y=0;y<S;y++)for(int x=0;x<S;x++)
            {
                float dx=Mathf.Max(6-x,x-(S-11)),dy=Mathf.Max(11-y,y-(S-6));   // card inset, shifted up-left of the shadow
                float edge=Mathf.Max(dx,dy)+(float)rng.NextDouble()*.6f;
                float card=Mathf.Clamp01(1-edge);
                float sx=Mathf.Max(10-x,x-(S-7)),sy=Mathf.Max(7-y,y-(S-10));
                float shadow=Mathf.Clamp01(1-Mathf.Max(sx,sy)/4f)*.35f;
                float grain=.97f+(float)rng.NextDouble()*.04f;
                var paper=new Color(.99f*grain,.94f*grain,.82f*grain,1);
                var c=Color.Lerp(new Color(.25f,.15f,.1f,shadow),paper,card);c.a=Mathf.Max(card,shadow);
                t.SetPixel(x,y,c);
            }
            t.Apply();return t;
        }
        [System.NonSerialized] GUIStyle marker;[System.NonSerialized] Texture2D tape;[System.NonSerialized] Font friendly;public Font uiFont;   // bundled Fredoka (OFL, Assets/Art/Fonts); OS fallback only if unset
        void Panel(Rect r){GUI.Box(r,GUIContent.none,panel);GUI.color=new Color(1,1,1,.85f);GUI.DrawTexture(new Rect(r.x+18,r.y-8,64,20),tape);GUI.color=Color.white;}
        void Styles()
        {
            cream=Paper();teal=Tex(new Color(.035f,.18f,.19f,.92f));coral=Tex(new Color(.92f,.37f,.23f));
            tape=Tex(new Color(.95f,.55f,.42f,.75f));
            // Bundled Fredoka (SIL OFL); the OS fallback is only used if the field is unset.
            friendly=uiFont?uiFont:Font.CreateDynamicFontFromOSFont(new[]{"Candara","Corbel","Segoe UI"},24);
            title=new GUIStyle(GUI.skin.label){fontSize=54,fontStyle=FontStyle.Bold,wordWrap=true,font=friendly};title.normal.textColor=new Color(.05f,.25f,.26f);
            heading=new GUIStyle(title){fontSize=24};marker=new GUIStyle(GUI.skin.box){font=friendly,fontSize=15,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(10,10,4,4),border=new RectOffset(8,8,8,8)};marker.normal.background=teal;marker.normal.textColor=Color.white;body=new GUIStyle(heading){fontSize=17,fontStyle=FontStyle.Normal};small=new GUIStyle(body){fontSize=13};
            button=new GUIStyle(GUI.skin.button){font=friendly,fontSize=18,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(18,12,8,8)};button.normal.background=teal;button.normal.textColor=Color.white;button.hover.background=coral;button.hover.textColor=Color.white;button.active.background=coral;
            button.border=new RectOffset(8,8,8,8);panel=new GUIStyle(GUI.skin.box){padding=new RectOffset(24,24,20,20),border=new RectOffset(14,14,14,14)};panel.normal.background=cream;
        }
        void OnGUI()
        {
            if(!S)return;if(title==null)Styles();float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1440*scale)*.5f,0,0),Quaternion.identity,new Vector3(scale,scale,1));
            if(S.menu){Menu();return;}
            if(intro&&!S.paused){Intro();return;}
            if(ending&&!S.paused&&!shop){Ending();return;}
            if(S.PresentationActive&&!S.paused&&!shop)
            {
                Panel(new Rect(420,782,600,84));
                GUI.Label(new Rect(445,797,550,56),S.message,body);return;
            }
            var f=S.player.focus;
            var station=FindObjectsByType<StudioStation>(FindObjectsSortMode.None).FirstOrDefault(s=>s.active);
            // Status panels step aside while the player works at a station, so the glass stays the focus.
            if(!station||S.paused||shop)
            {
                // Clarity pass (owner playtest, November 14: "I don't understand what I get to unlock", "not initially how much I can carry"):
                // every number is labelled, and the panel says what the next level opens and what caps are for.
                Panel(new Rect(28,25,470,140));GUI.Label(new Rect(47,33,380,36),"SEA GLASS / STUDIO",heading);
                int next=S.progress.NextLevelXp;
                Stat("xp",47,71,"Level "+S.progress.Level+"  ·  "+(next>0?S.progress.xp+" / "+next+" XP":S.progress.xp+" XP"));Stat("caps",300,71,S.progress.caps+" caps");
                Stat("bucket",47,101,"Bucket "+S.BucketCount+" / "+S.progress.BucketCapacity);Stat("truck",222,101,"Truck "+S.CargoCount+" / "+S.progress.TruckCapacity);
                GUI.Label(new Rect(47,131,440,24),"Recycling earns caps. Spend them at the studio's equipment cabinet.",small);
                Goals();
            }
            if(S.paused){Pause();return;}if(shop){Shop();return;}
            GUI.color=Color.white;GUI.DrawTexture(new Rect(717,447,6,6),cream);
            if(f&&!station){Panel(new Rect(440,550,560,48));GUI.Label(new Rect(457,559,530,30),f.Prompt,body);}
            if(!S.progress.tutorialDismissed){Panel(new Rect(28,770,640,104));GUI.Label(new Rect(47,784,600,55),TutorialGuidance.Get(S,station),body);}
            bool driving=S.player.mode==PlayerMode.Driving;
            GUI.Label(new Rect(47,845,620,23),driving?"W/S drive    A/D steer    Space brake    E exit    R recover":"H guidance    F reveal glass    R recover truck    Esc pause",small);
            if(!station&&!driving)
            {
                bool carrying=S.player.equippedTool==1;string icon=carrying?(S.progress.upgrades.Contains("bucket")?"sack":"bucket"):(S.progress.upgrades.Contains("scoop")?"sieve":"spade");
                Panel(new Rect(1025,812,395,68));GUI.DrawTexture(new Rect(1040,827,34,34),HudIcons.Get(icon));
                GUI.Label(new Rect(1082,822,330,52),(carrying?(S.progress.upgrades.Contains("bucket")?"CANVAS SACK":"SMALL BUCKET")+"  ·  holding "+S.BucketCount+" / "+S.progress.BucketCapacity+" finds":(S.progress.upgrades.Contains("scoop")?"SIEVE SCOOP":"TOY SHOVEL")+"  ·  hold LMB on a mound to dig")+"\n1 shovel to dig   ·   2 bucket to see your finds",small);
            }
            // Messages sit centred under the loop strip, clear of the goal tracker on the right.
            if(Time.unscaledTime<S.messageUntil){Panel(new Rect(395,200,650,68));GUI.Label(new Rect(415,216,610,48),S.message,body);}
            if(Time.unscaledTime<S.levelUpUntil&&!station&&!shop){Panel(new Rect(395,278,650,76));GUI.Label(new Rect(415,290,610,56),S.LevelUp,body);}
            if(!S.progress.tutorialDismissed){Loop(station);Marker(station,scale);}
            if(station&&station.kind==StationKind.Clean&&station.selectedId!=null){float clean=1-S.progress.Item(station.selectedId).grime;Panel(new Rect(540,670,360,60));GUI.Label(new Rect(555,678,340,25),"HOLD LMB + MOVE  ·  CLEAN "+Mathf.RoundToInt(clean*100)+"%",body);GUI.DrawTexture(new Rect(555,713,330*clean,5),coral);}
        }
        // Clarity pass (owner playtest round 2, November 17: "Next tasks/overall tasks could be made more clear"; owner chose a goal
        // tracker plus a loop strip on November 20). The tracker answers "what am I working toward": each mosaic's progress and where its
        // glass is, which cove holds it and when that cove opens, and what the next level brings.
        [System.NonSerialized] GUIStyle bodyRight,stepOn,stepOff,stepCount;
        void TrackerStyles()
        {
            bodyRight=new GUIStyle(body){alignment=TextAnchor.UpperRight};
            stepOff=new GUIStyle(body){fontSize=15,alignment=TextAnchor.MiddleCenter};stepOff.normal.textColor=new Color(.05f,.25f,.26f,.55f);
            stepOn=new GUIStyle(stepOff){fontStyle=FontStyle.Bold};stepOn.normal.textColor=Color.white;
            stepCount=new GUIStyle(small){fontSize=12,alignment=TextAnchor.MiddleCenter};
        }
        // Read-only lookup: Progress.Item would add a record for every piece the HUD asks about.
        ItemStage Stage(string id){var r=S.progress.items.Find(i=>i.id==id);return r!=null?r.stage:ItemStage.Buried;}
        int PanelGlass(int panel,params ItemStage[] stages)=>S.catalog.glass.Count(g=>g.panel==panel&&stages.Contains(Stage(g.id)));
        void Goals()
        {
            if(bodyRight==null)TrackerStyles();var p=S.progress;
            Panel(new Rect(1025,25,387,272));GUI.Label(new Rect(1047,35,345,36),"RESTORE THE STUDIO",heading);
            for(int m=0;m<3;m++)
            {
                float y=74+m*50;bool open=p.SectionOpen(m);int placed=PanelGlass(m,ItemStage.Placed);
                GUI.color=GameCatalog.Palette[new[]{0,2,5}[m]]*(open?1:.6f);GUI.DrawTexture(new Rect(1047,y+2,20,20),HudIcons.Get("gem"));GUI.color=open?Color.white:new Color(1,1,1,.6f);
                GUI.Label(new Rect(1074,y,220,24),S.catalog.panelNames[m],body);GUI.Label(new Rect(1290,y,100,24),placed+" / 10",bodyRight);
                string status;
                if(p.completed[m])status="Complete. The window glows.";
                else if(!open)status="Its glass is at "+GameCatalog.CoveNames[m]+", which opens at level "+(m+1)+".";
                else
                {
                    var parts=new System.Collections.Generic.List<string>();
                    void Add(int n,string what){if(n>0)parts.Add(n+" "+what);}
                    Add(PanelGlass(m,ItemStage.Buried,ItemStage.Shore),"at "+GameCatalog.CoveNames[m]);Add(PanelGlass(m,ItemStage.Bucket,ItemStage.Truck),"carried");
                    Add(PanelGlass(m,ItemStage.Studio),"to wash");Add(PanelGlass(m,ItemStage.Clean),"to sort");Add(PanelGlass(m,ItemStage.Sorted),"to place");
                    status=string.Join("  ·  ",parts);
                }
                GUI.Label(new Rect(1074,y+25,320,20),status,small);GUI.color=Color.white;
            }
            int lv=p.Level,next=p.NextLevelXp;
            if(next>0)
            {
                int from=Progress.LevelXp[lv-1];float frac=Mathf.Clamp01((p.xp-from)/(float)(next-from));
                GUI.Label(new Rect(1047,228,200,24),"Next: level "+(lv+1),body);GUI.Label(new Rect(1240,228,150,24),p.xp+" / "+next+" XP",bodyRight);
                GUI.color=new Color(1,1,1,.25f);GUI.DrawTexture(new Rect(1047,255,343,5),teal);GUI.color=Color.white;GUI.DrawTexture(new Rect(1047,255,343*frac,5),coral);
                GUI.Label(new Rect(1047,263,345,22),NextUnlock(),small);
            }
            else{GUI.Label(new Rect(1047,228,345,24),"Level "+lv,body);GUI.Label(new Rect(1047,256,345,22),NextUnlock(),small);}
        }
        // The loop strip: Dig → Bucket → Truck → Wash → Sort → Place, with the current step lit and what is waiting at each step.
        // Side jobs (recycling, the cabinet) light no step and say so underneath. Hidden with the rest of the guidance (H).
        void Loop(StudioStation station)
        {
            if(bodyRight==null)TrackerStyles();
            int step=TutorialGuidance.Step(S,station,out var side);if(step<0&&side==null)return;
            var p=S.progress;bool truckHere=S.truck.AtStudio;
            string[] counts={
                CountOrBlank(S.catalog.glass.Count(g=>p.SectionOpen(g.section)&&Stage(g.id)<=ItemStage.Shore),"left"),
                S.BucketCount+" / "+p.BucketCapacity,S.CargoCount+" / "+p.TruckCapacity,
                CountOrBlank(S.Count(ItemStage.Studio,true)+(truckHere?S.Count(ItemStage.Truck,true):0),"waiting"),
                CountOrBlank(S.Count(ItemStage.Clean,true),"waiting"),CountOrBlank(S.Count(ItemStage.Sorted,true),"ready")};
            Panel(new Rect(490,25,460,76));
            for(int i=0;i<6;i++)
            {
                float x=507+i*72;
                if(i==step)GUI.DrawTexture(new Rect(x-5,33,68,60),coral);
                GUI.Label(new Rect(x-4,35,66,26),TutorialGuidance.LoopSteps[i],i==step?stepOn:stepOff);
                if(i==step)stepCount.normal.textColor=Color.white;else stepCount.normal.textColor=new Color(.05f,.25f,.26f,.75f);
                GUI.Label(new Rect(x-4,61,66,22),counts[i],stepCount);
                if(i<5)GUI.Label(new Rect(x+62,35,10,26),"›",stepOff);
            }
            if(side!=null){var size=marker.CalcSize(new GUIContent(side));GUI.Box(new Rect(720-size.x*.5f,106,size.x,size.y),side,marker);}
        }
        static string CountOrBlank(int n,string what)=>n>0?n+" "+what:"";
        void Stat(string icon,float x,float y,string value){GUI.DrawTexture(new Rect(x,y+1,22,22),HudIcons.Get(icon));GUI.Label(new Rect(x+28,y,190,26),value,body);}
        bool Button(string label,float x,float y,float w=340)=>GUI.Button(new Rect(x,y,w,46),label,button);
        void Menu()
        {
            Panel(new Rect(70,95,555,710));GUI.Label(new Rect(110,125,440,30),"A SMALL COASTAL RESTORATION",small);
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
            Panel(new Rect(480,175,480,550));GUI.Label(new Rect(525,208,390,55),"Take a breath",heading);
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
            Panel(new Rect(480,290,500,260));GUI.Label(new Rect(510,315,440,85),"Start a new studio?\nThis replaces your current progress.",heading);
            if(Button("Keep my progress",510,420,210))confirmReset=false;
            if(Button("Reset & begin",735,420,210)){confirmReset=false;S.ResetProgress();}
        }
        void Settings()
        {
            Panel(new Rect(700,170,620,570));GUI.Label(new Rect(735,198,550,40),"Make yourself comfortable",heading);
            float y=265;GUI.Label(new Rect(735,y,490,30),Feedback.PlayMusic?"Music":"Music (off for now)",body);Feedback.Instance.musicVolume=GUI.HorizontalSlider(new Rect(735,y+38,520,22),Feedback.Instance.musicVolume,0,1);
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
            Panel(new Rect(285,140,870,610));GUI.Label(new Rect(320,170,780,34),"EQUIPMENT CABINET",heading);int i=0;
            // Clarity pass: the cabinet says what you have and what each tool needs, and only offers what you can buy.
            GUI.Label(new Rect(320,204,800,24),"You have "+S.progress.caps+" bottle caps  ·  Level "+S.progress.Level+".   Recycle rubbish for caps; every find earns XP toward the next level.",small);
            foreach(var u in S.catalog.upgrades)
            {float y=240+i++*82;bool owned=S.progress.upgrades.Contains(u.id);bool levelOk=S.progress.Level>=u.level,capsOk=S.progress.caps>=u.cost;
             GUI.Label(new Rect(320,y,505,26),u.title+"   ·   "+u.cost+" caps",body);GUI.Label(new Rect(320,y+28,505,43),u.description,small);
             string state=owned?"Owned":!levelOk?"Needs level "+u.level:!capsOk?"Need "+(u.cost-S.progress.caps)+" more caps":"Buy  ·  "+u.cost+" caps";
             GUI.enabled=!owned&&levelOk&&capsOk;if(Button(state,870,y,245))S.Buy(u.id);GUI.enabled=true;}
            if(Button("Back to the studio",780,675,340))CloseShop();
        }
        string NextUnlock()
        {
            int lv=S.progress.Level;if(lv>=4)return "Top level. Every cove and tool is open.";
            int gear=S.catalog.upgrades.Count(u=>u.level==lv+1);
            return "Opens "+(lv+1<=3?GameCatalog.CoveNames[lv]:"")+(lv+1<=3&&gear>0?" and ":"")+(gear>0?gear+" new tool"+(gear>1?"s":""):"")+".";
        }
        // Clarity pass (owner playtest: "needs more of an introduction"; "not enough direction on why I am doing what I am doing").
        void Intro()
        {
            Panel(new Rect(270,115,900,610));
            GUI.Label(new Rect(315,148,800,22),"A SMALL COASTAL RESTORATION",small);
            GUI.Label(new Rect(312,172,820,40),"The old glass studio up the coast is yours now.",heading);
            GUI.Label(new Rect(315,222,810,90),"It has stood empty for years, and the beach below it is littered with rubbish. Buried in the sand is sea glass: bits of old bottles, worn cloudy by the waves. Clean up the shore, polish the glass, and set it into the studio's three empty mosaic windows to bring the place back to life.",body);
            string[] icons={"spade","truck","gem","gem"};string[] steps={"GATHER","HAUL","CLEAN & SORT","CREATE"};
            string[] text={"Dig up rubbish and sea glass on the beach with your shovel.","Load your bucket into the truck's crates and drive to the studio.","Recycle the rubbish, then wash and sort the glass by colour.","Set each piece into its mosaic window until all three glow."};
            for(int k=0;k<4;k++){float x=315+k*205;GUI.color=k==3?GameCatalog.Palette[0]:Color.white;GUI.DrawTexture(new Rect(x,330,34,34),HudIcons.Get(icons[k]));GUI.color=Color.white;
                GUI.Label(new Rect(x,372,190,26),(k+1)+"  "+steps[k],body);GUI.Label(new Rect(x,400,185,70),text[k],small);}
            Row("xp",315,490,"XP comes from every find. Each new level opens more of the beach and new tools.");
            Row("caps",315,524,"Bottle caps come from recycling rubbish. Spend them at the studio's equipment cabinet.");
            GUI.Label(new Rect(315,566,800,24),"The strip at the top lights the step you are on, the panel at the bottom left says what to do, and a marker points the way. H hides them.",small);
            if(Button("Start on the shore",315,612,330))CloseIntro();
        }
        void Row(string icon,float x,float y,string value){GUI.DrawTexture(new Rect(x,y+1,22,22),HudIcons.Get(icon));GUI.Label(new Rect(x+30,y,780,26),value,body);}
        // The objective marker: a small tag over where the current step happens, kept on screen at the edge when it is out of view.
        void Marker(StudioStation station,float scale)
        {
            if(!TutorialGuidance.Target(S,station,out var at,out var label))return;
            var cam=S.player.view?S.player.view:Camera.main;if(!cam)return;
            float dist=Vector3.Distance(S.player.transform.position,at);if(dist<5f)return;
            var sp=cam.WorldToScreenPoint(at);float ox=(Screen.width-1440*scale)*.5f;
            float vx=(sp.x-ox)/scale,vy=(Screen.height-sp.y)/scale;
            bool off=sp.z<0||vx<60||vx>1380||vy<200||vy>740;
            if(sp.z<0){vx=720;vy=700;}
            vx=Mathf.Clamp(vx,90,1350);vy=Mathf.Clamp(vy,200,740);
            bool behind=sp.z<0;string text=(behind?"Behind you  ·  ":off&&vx<=90?"<  ":"")+label+"  ·  "+Mathf.RoundToInt(dist)+" m"+(!behind&&off&&vx>=1350?"  >":"");
            var size=marker.CalcSize(new GUIContent(text));
            GUI.color=new Color(1,1,1,.92f);GUI.DrawTexture(new Rect(vx-11,vy-30,22,22),HudIcons.Get("gem"));GUI.color=Color.white;
            GUI.Box(new Rect(vx-size.x*.5f,vy-4,size.x,size.y),text,marker);
        }
        void Ending()
        {
            Panel(new Rect(320,682,800,188));
            GUI.Label(new Rect(350,700,730,40),"Three shores. One home.",heading);
            GUI.Label(new Rect(350,742,730,45),"Thirty small discoveries now hold the afternoon light. The studio is yours again.",body);
            if(Button("Stay a little longer",760,804,330))CloseEnding();
        }
        void OnDestroy(){if(cream)Destroy(cream);if(teal)Destroy(teal);if(coral)Destroy(coral);if(Instance==this)Instance=null;}
    }
}
