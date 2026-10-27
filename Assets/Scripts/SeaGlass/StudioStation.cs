using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace SeaGlass
{
    public enum StationKind { Recycle, Clean, Sort, Upgrade }
    public class StudioStation : Interactable
    {
        public StationKind kind; public Transform workingGlass; public Renderer glassRenderer; public Renderer waterSurface; public string selectedId; public bool active; public GlassColor selectedTray;
        MaterialPropertyBlock block; Vector3 returnPosition,glassHome;Quaternion returnRotation,returnView,glassRotation;float processing,reveal,nextFoam;public bool Processing=>processing>0;
        public bool RevealingCleanGlass=>reveal>0;
        GameSession S=>GameSession.Instance;
        public override string Prompt=> kind==StationKind.Recycle?"E · recycle truck rubbish":kind==StationKind.Clean?"E · wash glass at the basin":kind==StationKind.Sort?"E · sort polished glass":"E · browse equipment catalogue";
        void Awake(){block=new MaterialPropertyBlock();if(workingGlass){glassHome=workingGlass.localPosition;glassRotation=workingGlass.localRotation;workingGlass.gameObject.SetActive(false);}}
        void RestoreGlassPose(){if(workingGlass){workingGlass.localPosition=glassHome;workingGlass.localRotation=glassRotation;}}
        public void ResetStation(){active=false;selectedId=null;processing=0;reveal=0;RestoreGlassPose();if(workingGlass)workingGlass.gameObject.SetActive(false);}
        public override void Interact()
        {
            if(kind==StationKind.Recycle){Recycle();return;}
            if(kind==StationKind.Upgrade){StudioUI.Instance.shop=!StudioUI.Instance.shop;S.RefreshControlState();return;}
            if(active){Leave();return;}
            if(kind==StationKind.Clean&&S.truck.AtStudio)
            {foreach(var r in S.progress.items)if(r.stage==ItemStage.Truck&&r.id.StartsWith("glass-"))r.stage=ItemStage.Studio;S.truck.RefreshCargo();}
            if(!SelectNext()){S.Notify(kind==StationKind.Clean?"Park the truck by the studio and bring cloudy glass.":"Wash a glass piece at the basin first.");return;}
            returnPosition=S.player.transform.position;returnRotation=S.player.transform.rotation;returnView=S.player.view.transform.localRotation;active=true;S.player.mode=PlayerMode.Workstation;
            bool cleaning=kind==StationKind.Clean;
            S.player.Teleport(transform.position-transform.forward*(cleaning?.82f:1.75f)+Vector3.up*(cleaning?.36f:.12f),transform.rotation);S.player.interactOverride=Leave;
            S.player.LookAt(cleaning?workingGlass.position:transform.position+Vector3.up*1.15f);
            S.Notify(kind==StationKind.Clean?"Hold LMB to brush away the salt. E to leave.":"Aim at a matching colour tray and press LMB. E to leave.");
        }
        public bool Recycle()
        {
            if(processing>0)return false;
            if(!S.truck.AtStudio){S.Notify("Park the truck beside the studio first.");return false;}
            int count=0;foreach(var r in S.progress.items)if(r.stage==ItemStage.Truck&&r.id.StartsWith("rubbish-")){r.stage=ItemStage.Recycled;count++;}
            if(count==0){S.Notify("No rubbish in the truck's recycling crate.");return false;}
            S.Award(count*12,count*5);S.Tutorial(7);S.truck.RefreshCargo();S.Save();processing=1.5f;Feedback.Instance.Play("recycle");S.Notify(count+" pieces recycled · +"+(count*5)+" caps · +"+(count*12)+" XP");return true;
        }
        public bool SelectNext()
        {
            reveal=0;RestoreGlassPose();
            var r=S.progress.items.FirstOrDefault(i=>i.id.StartsWith("glass-")&&i.stage==(kind==StationKind.Clean?ItemStage.Studio:ItemStage.Clean));selectedId=r?.id;
            if(workingGlass)workingGlass.gameObject.SetActive(r!=null);
            if(r!=null)UpdateGlass();return r!=null;
        }
        public void Clean(float delta)
        {
            if(string.IsNullOrEmpty(selectedId))return;var r=S.progress.Item(selectedId);if(r.stage!=ItemStage.Studio)return;
            r.grime=Mathf.Max(0,r.grime-delta/(S.progress.upgrades.Contains("washer")?3:9));
            // The salt comes off where the brush (or spray) actually touches the glass.
            var contact=workingGlass.position+EquipmentVisuals.Stroke(true,Time.time);
            StampClean(contact,S.progress.upgrades.Contains("washer")?.13f:.09f);
            UpdateGlass();Feedback.Instance.LoopAction("water");
            if(Time.time>=nextFoam){nextFoam=Time.time+.08f;Feedback.Instance.Foam(contact,.06f);}
            if(r.grime<=.001f)
            {
                r.grime=0;UpdateGlass();r.stage=ItemStage.Clean;S.Award(10);S.Tutorial(8);Feedback.Instance.Play("glass");Feedback.Instance.Burst(workingGlass.position+Vector3.up*.05f,GameCatalog.Palette[(int)S.catalog.glass.First(g=>g.id==r.id).color]*1.4f,18);S.Save();
                S.Notify("Colour revealed. Take this piece to the sorting trays.");
                // Progress commits immediately; the presentation gives the finished piece a moment
                // in the light. Leaving or loading can cancel this without losing/duplicating it.
                selectedId=null;reveal=1.1f;
            }
        }
        public bool Sort(GlassColor color)
        {
            if(string.IsNullOrEmpty(selectedId))return false;var r=S.progress.Item(selectedId);if(r.stage!=ItemStage.Clean)return false;
            var def=S.catalog.glass.First(g=>g.id==selectedId);
            if(def.color!=color){S.Notify("A gentle nudge: this piece belongs in the "+def.color+" tray.");return false;}
            r.stage=ItemStage.Sorted;S.Award(8);S.Tutorial(9);Feedback.Instance.Play("sort",1+(int)color*.09f);
            var tray=FindObjectsByType<SortingTray>(FindObjectsSortMode.None).FirstOrDefault(t=>t.color==color);
            if(tray)Feedback.Instance.Fly(def.HeldMesh,glassRenderer.sharedMaterial,GameCatalog.Palette[(int)color],workingGlass.position,tray.transform.position+Vector3.up*.15f,workingGlass.lossyScale,Quaternion.Euler(90,0,0));
            Feedback.Instance.Burst(tray?tray.transform.position+Vector3.up*.2f:workingGlass.position,GameCatalog.Palette[(int)color]);S.Save();SelectNext();return true;
        }
        // Per-piece mask of where the glass has been scrubbed (white = brushed). Presentation only:
        // completion still follows the record's grime value.
        Texture2D cleanMask;string maskFor;Color32[] maskPixels;const int MaskSize=64;
        static readonly int MaskId=Shader.PropertyToID("_CleanMask"),MaskRectId=Shader.PropertyToID("_MaskRect");
        void EnsureMask()
        {
            if(!cleanMask){cleanMask=new Texture2D(MaskSize,MaskSize,TextureFormat.R8,false){wrapMode=TextureWrapMode.Clamp,name="Clean mask"};maskPixels=new Color32[MaskSize*MaskSize];}
            if(maskFor!=selectedId){maskFor=selectedId;System.Array.Clear(maskPixels,0,maskPixels.Length);cleanMask.SetPixels32(maskPixels);cleanMask.Apply(false);}
        }
        void StampClean(Vector3 world,float radius)
        {
            var mesh=S.catalog.glass.First(g=>g.id==selectedId).HeldMesh;if(!mesh)return;EnsureMask();
            var b=mesh.bounds;var local=workingGlass.InverseTransformPoint(world);
            float u=(local.x-b.min.x)/b.size.x,v=(local.y-b.min.y)/b.size.y,ru=radius/(workingGlass.lossyScale.x*b.size.x),rv=radius/(workingGlass.lossyScale.y*b.size.y);
            int x0=Mathf.FloorToInt((u-ru)*MaskSize),x1=Mathf.CeilToInt((u+ru)*MaskSize),y0=Mathf.FloorToInt((v-rv)*MaskSize),y1=Mathf.CeilToInt((v+rv)*MaskSize);
            for(int y=Mathf.Max(0,y0);y<=Mathf.Min(MaskSize-1,y1);y++)for(int x=Mathf.Max(0,x0);x<=Mathf.Min(MaskSize-1,x1);x++)
            {
                float dx=((x+.5f)/MaskSize-u)/ru,dy=((y+.5f)/MaskSize-v)/rv,d=dx*dx+dy*dy;if(d>1)continue;
                int i=y*MaskSize+x;byte add=(byte)(Mathf.SmoothStep(1,0,d)*70);maskPixels[i].r=(byte)Mathf.Min(255,maskPixels[i].r+add);
            }
            cleanMask.SetPixels32(maskPixels);cleanMask.Apply(false);
        }
        void OnDestroy(){if(cleanMask)Destroy(cleanMask);}
        void UpdateGlass()
        {
            if(!glassRenderer||selectedId==null)return;var def=S.catalog.glass.First(g=>g.id==selectedId);var r=S.progress.Item(selectedId);var c=GameCatalog.Palette[(int)def.color];
            block??=new MaterialPropertyBlock();glassRenderer.GetPropertyBlock(block);block.SetColor("_BaseColor",c);block.SetFloat("_Grime",r.grime);block.SetFloat("_Smoothness",Mathf.Lerp(.85f,.05f,r.grime));block.SetColor("_EmissionColor",c*(1-r.grime)*.12f);
            if(kind==StationKind.Clean){EnsureMask();var mb=def.HeldMesh.bounds;block.SetTexture(MaskId,cleanMask);block.SetVector(MaskRectId,new Vector4(mb.min.x,mb.min.y,mb.size.x,mb.size.y));}
            glassRenderer.SetPropertyBlock(block);
            var filter=workingGlass.GetComponent<MeshFilter>();if(def.HeldMesh)filter.sharedMesh=def.HeldMesh;
            workingGlass.localScale=Vector3.one*((kind==StationKind.Sort?.36f:.64f)/Mathf.Max(def.HeldMesh.bounds.size.x,def.HeldMesh.bounds.size.y));
        }
        public void Leave(){active=false;reveal=0;RestoreGlassPose();if(workingGlass)workingGlass.gameObject.SetActive(false);S.player.mode=PlayerMode.Walking;S.player.interactOverride=null;S.player.Teleport(returnPosition,returnRotation);S.player.SetLookRotation(returnView);S.Save();}
        void Update()
        {
            if(!S||S.ControlsBlocked)return;
            if(processing>0){processing-=Time.deltaTime;if(Random.value<.1f)Feedback.Instance.Burst(transform.position+Vector3.up*1.1f,GameCatalog.Palette[0],5);}
            if(!active)return;
            if(reveal>0)
            {
                reveal=Mathf.Max(0,reveal-Time.deltaTime);float t=1-reveal/1.1f;
                float lift=Mathf.Sin(t*Mathf.PI);
                workingGlass.localPosition=glassHome+Vector3.up*(lift*.16f);
                workingGlass.localRotation=glassRotation*Quaternion.Euler(lift*-18,0,lift*8);
                if(reveal<=0)SelectNext();
                return;
            }
            if(kind==StationKind.Clean&&Mouse.current!=null&&Mouse.current.leftButton.isPressed)Clean(Time.deltaTime);
            if(kind==StationKind.Sort&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)
            {
                var ray=S.player.view.ViewportPointToRay(new Vector3(.5f,.5f));
                // A broad assist cast can touch a neighbouring tray before the one under the reticle.
                // Honour precise aim first, then help with near misses.
                SortingTray tray=null;
                if(Physics.Raycast(ray,out var direct,4))tray=direct.collider.GetComponent<SortingTray>();
                if(!tray)
                {
                    var hits=Physics.SphereCastAll(ray,S.progress.upgrades.Contains("magnet")?.5f:.16f,4);
                    tray=hits.Select(x=>x.collider.GetComponent<SortingTray>()).Where(x=>x)
                        .OrderBy(x=>Vector3.Angle(ray.direction,x.transform.position-ray.origin)).FirstOrDefault();
                }
                if(tray)Sort(tray.color);
            }
            if(workingGlass&&workingGlass.gameObject.activeSelf&&kind==StationKind.Sort)workingGlass.position=S.player.view.transform.position+S.player.view.transform.forward*.85f+S.player.view.transform.up*-.13f+S.player.view.transform.right*.12f;
        }
    }
}
