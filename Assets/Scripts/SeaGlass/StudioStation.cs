using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace SeaGlass
{
    public enum StationKind { Recycle, Clean, Sort, Upgrade }
    public class StudioStation : Interactable
    {
        public StationKind kind; public Transform workingGlass; public Renderer glassRenderer; public Renderer waterSurface; public string selectedId; public bool active; public GlassColor selectedTray;
        MaterialPropertyBlock block; Vector3 returnPosition,glassHome;Quaternion returnRotation;float processing;public bool Processing=>processing>0;
        GameSession S=>GameSession.Instance;
        public override string Prompt=> kind==StationKind.Recycle?"E · recycle truck rubbish":kind==StationKind.Clean?"E · wash glass at the basin":kind==StationKind.Sort?"E · sort polished glass":"E · browse equipment catalogue";
        void Awake(){block=new MaterialPropertyBlock();if(workingGlass){glassHome=workingGlass.localPosition;workingGlass.gameObject.SetActive(false);}}
        public void ResetStation(){active=false;selectedId=null;processing=0;if(workingGlass){workingGlass.localPosition=glassHome;workingGlass.gameObject.SetActive(false);}}
        public override void Interact()
        {
            if(kind==StationKind.Recycle){Recycle();return;}
            if(kind==StationKind.Upgrade){StudioUI.Instance.shop=!StudioUI.Instance.shop;S.player.blocked=StudioUI.Instance.shop;S.player.SetCursor(!StudioUI.Instance.shop);return;}
            if(active){Leave();return;}
            if(kind==StationKind.Clean&&S.truck.AtStudio)
            {foreach(var r in S.progress.items)if(r.stage==ItemStage.Truck&&r.id.StartsWith("glass-"))r.stage=ItemStage.Studio;S.truck.RefreshCargo();}
            if(!SelectNext()){S.Notify(kind==StationKind.Clean?"Park the truck by the studio and bring cloudy glass.":"Wash a glass piece at the basin first.");return;}
            returnPosition=S.player.transform.position;returnRotation=S.player.transform.rotation;active=true;S.player.mode=PlayerMode.Workstation;
            S.player.Teleport(transform.position-transform.forward*1.75f+Vector3.up*.12f,transform.rotation);S.player.interactOverride=Leave;
            S.player.LookAt(transform.position+Vector3.up*1.15f);
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
            var r=S.progress.items.FirstOrDefault(i=>i.id.StartsWith("glass-")&&i.stage==(kind==StationKind.Clean?ItemStage.Studio:ItemStage.Clean));selectedId=r?.id;
            if(workingGlass)workingGlass.gameObject.SetActive(r!=null);
            if(r!=null)UpdateGlass();return r!=null;
        }
        public void Clean(float delta)
        {
            if(string.IsNullOrEmpty(selectedId))return;var r=S.progress.Item(selectedId);if(r.stage!=ItemStage.Studio)return;
            r.grime=Mathf.Max(0,r.grime-delta/(S.progress.upgrades.Contains("washer")?3:9));UpdateGlass();Feedback.Instance.LoopAction("water");
            if(Random.value<.25f)Feedback.Instance.Burst(workingGlass.position,Color.white,2);
            if(r.grime<=.001f){r.grime=0;r.stage=ItemStage.Clean;S.Award(10);S.Tutorial(8);Feedback.Instance.Play("glass");S.Save();S.Notify("Colour revealed. Take this piece to the sorting trays.");SelectNext();}
        }
        public bool Sort(GlassColor color)
        {
            if(string.IsNullOrEmpty(selectedId))return false;var r=S.progress.Item(selectedId);if(r.stage!=ItemStage.Clean)return false;
            var def=S.catalog.glass.First(g=>g.id==selectedId);
            if(def.color!=color){S.Notify("A gentle nudge: this piece belongs in the "+def.color+" tray.");return false;}
            r.stage=ItemStage.Sorted;S.Award(8);S.Tutorial(9);Feedback.Instance.Play("sort",1+(int)color*.09f);
            var tray=FindObjectsByType<SortingTray>(FindObjectsSortMode.None).FirstOrDefault(t=>t.color==color);
            if(tray)Feedback.Instance.Fly(def.mesh,glassRenderer.sharedMaterial,GameCatalog.Palette[(int)color],workingGlass.position,tray.transform.position+Vector3.up*.15f,workingGlass.lossyScale,Quaternion.Euler(90,0,0));
            Feedback.Instance.Burst(tray?tray.transform.position+Vector3.up*.2f:workingGlass.position,GameCatalog.Palette[(int)color]);S.Save();SelectNext();return true;
        }
        void UpdateGlass()
        {
            if(!glassRenderer||selectedId==null)return;var def=S.catalog.glass.First(g=>g.id==selectedId);var r=S.progress.Item(selectedId);var c=GameCatalog.Palette[(int)def.color];
            block??=new MaterialPropertyBlock();glassRenderer.GetPropertyBlock(block);block.SetColor("_BaseColor",c);block.SetFloat("_Grime",r.grime);block.SetFloat("_Smoothness",Mathf.Lerp(.85f,.05f,r.grime));block.SetColor("_EmissionColor",c*(1-r.grime)*.12f);glassRenderer.SetPropertyBlock(block);
            var filter=workingGlass.GetComponent<MeshFilter>();if(def.mesh)filter.sharedMesh=def.mesh;
            workingGlass.localScale=Vector3.one*((kind==StationKind.Sort?.42f:.75f)/Mathf.Max(def.mesh.bounds.size.x,def.mesh.bounds.size.y));
        }
        public void Leave(){active=false;if(workingGlass)workingGlass.localPosition=glassHome;S.player.mode=PlayerMode.Walking;S.player.interactOverride=null;S.player.Teleport(returnPosition,returnRotation);S.Save();}
        void Update()
        {
            if(!S||S.menu||S.paused)return;
            if(processing>0){processing-=Time.deltaTime;if(Random.value<.1f)Feedback.Instance.Burst(transform.position+Vector3.up*1.1f,GameCatalog.Palette[0],5);}
            if(!active)return;
            if(kind==StationKind.Clean&&Mouse.current!=null&&Mouse.current.leftButton.isPressed)Clean(Time.deltaTime);
            if(kind==StationKind.Sort&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)
            {var ray=S.player.view.ViewportPointToRay(new Vector3(.5f,.5f));var hits=Physics.SphereCastAll(ray,S.progress.upgrades.Contains("magnet")?.5f:.16f,4);var tray=hits.OrderBy(x=>x.distance).Select(x=>x.collider.GetComponent<SortingTray>()).FirstOrDefault(x=>x);if(tray)Sort(tray.color);}
            if(workingGlass&&workingGlass.gameObject.activeSelf&&kind==StationKind.Sort)workingGlass.position=S.player.view.transform.position+S.player.view.transform.forward*.85f+S.player.view.transform.up*-.25f;
        }
    }
}
