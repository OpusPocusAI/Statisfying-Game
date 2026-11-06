using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace SeaGlass
{
    public enum StationKind { Recycle, Clean, Sort, Upgrade }
    public class StudioStation : Interactable
    {
        public StationKind kind; public Transform workingGlass; public Transform heldPalm; public Renderer glassRenderer; public Renderer waterSurface; public string selectedId; public bool active; public GlassColor selectedTray;
        MaterialPropertyBlock block; Vector3 returnPosition,glassHome;Quaternion returnRotation,returnView,glassRotation;float processing,reveal,nextFoam;public bool Processing=>processing>0;
        public bool RevealingCleanGlass=>reveal>0;
        float lastBrush=-9;Vector3 lastContact;static readonly int RippleId=Shader.PropertyToID("_SG_BrushRipple"),BasinId=Shader.PropertyToID("_SG_Basin");
        // True while the glass is being brushed (by the mouse or by the verification run); drives the brush animation.
        public bool BrushingNow=>active&&Time.time-lastBrush<.15f;
        // Half-extents (world x, z) the brush stroke sweeps over and the height it rests at, so it covers the whole piece.
        public static Vector3 Reach=new Vector3(.17f,.05f,.11f);
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
            // Round 13: the brush sweeps a hard-edged path from where it was last frame, so the grime clears in
            // visible strokes (PowerWash), and the water ripples from the contact point.
            bool continuing=Time.time-lastBrush<.15f;StampClean(continuing?lastContact:contact,contact,S.progress.upgrades.Contains("washer")?.085f:.06f);
            lastBrush=Time.time;lastContact=contact;
            UpdateGlass();Feedback.Instance.LoopAction("water");
            if(Time.time>=nextFoam){nextFoam=Time.time+.06f;Feedback.Instance.Foam(contact,.05f);}
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
        // Round 13: two passes over a spot take the sand crust off, a third reveals the clear glass; the
        // shader draws each threshold as a hard edge.
        Texture2D cleanMask;string maskFor;Color32[] maskPixels;const int MaskSize=128;
        static readonly int MaskId=Shader.PropertyToID("_CleanMask"),MaskRectId=Shader.PropertyToID("_MaskRect"),MaskOnId=Shader.PropertyToID("_MaskOn");
        void EnsureMask()
        {
            if(!cleanMask){cleanMask=new Texture2D(MaskSize,MaskSize,TextureFormat.R8,false){wrapMode=TextureWrapMode.Clamp,name="Clean mask"};maskPixels=new Color32[MaskSize*MaskSize];}
            if(maskFor!=selectedId){maskFor=selectedId;System.Array.Clear(maskPixels,0,maskPixels.Length);cleanMask.SetPixels32(maskPixels);cleanMask.Apply(false);}
        }
        // Sweeps the brush head from one contact point to the next (a capsule in mask space), adding a fixed
        // amount once per sweep so overlapping strokes build up pass by pass.
        void StampClean(Vector3 fromWorld,Vector3 toWorld,float radius)
        {
            var mesh=S.catalog.glass.First(g=>g.id==selectedId).HeldMesh;if(!mesh)return;EnsureMask();
            var b=mesh.bounds;
            Vector2 UV(Vector3 w){var l=workingGlass.InverseTransformPoint(w);return new Vector2((l.x-b.min.x)/b.size.x,(l.y-b.min.y)/b.size.y);}
            Vector2 a=UV(fromWorld),c=UV(toWorld);
            float ru=radius/(workingGlass.lossyScale.x*b.size.x),rv=radius/(workingGlass.lossyScale.y*b.size.y);
            int x0=Mathf.FloorToInt((Mathf.Min(a.x,c.x)-ru)*MaskSize),x1=Mathf.CeilToInt((Mathf.Max(a.x,c.x)+ru)*MaskSize);
            int y0=Mathf.FloorToInt((Mathf.Min(a.y,c.y)-rv)*MaskSize),y1=Mathf.CeilToInt((Mathf.Max(a.y,c.y)+rv)*MaskSize);
            // Work in brush-radius units so the head stays round on a non-square piece.
            Vector2 A=new Vector2(a.x/ru,a.y/rv),C=new Vector2(c.x/ru,c.y/rv),AC=C-A;float len2=Mathf.Max(1e-6f,AC.sqrMagnitude);
            // Time-based, so the strokes build up at the same rate at any frame rate.
            byte add=(byte)Mathf.Clamp(Time.deltaTime*1700f,4,60);
            for(int y=Mathf.Max(0,y0);y<=Mathf.Min(MaskSize-1,y1);y++)for(int x=Mathf.Max(0,x0);x<=Mathf.Min(MaskSize-1,x1);x++)
            {
                var P=new Vector2((x+.5f)/MaskSize/ru,(y+.5f)/MaskSize/rv);float t=Mathf.Clamp01(Vector2.Dot(P-A,AC)/len2);
                float dd=(P-(A+AC*t)).sqrMagnitude;if(dd>1)continue;
                int i=y*MaskSize+x;maskPixels[i].r=(byte)Mathf.Min(255,maskPixels[i].r+Mathf.RoundToInt(add*(1-dd*dd)));   // round 21: a soft round brush head, strongest at its centre
            }
            cleanMask.SetPixels32(maskPixels);cleanMask.Apply(false);
        }
        void OnDestroy(){if(cleanMask)Destroy(cleanMask);}
        void UpdateGlass()
        {
            if(!glassRenderer||selectedId==null)return;var def=S.catalog.glass.First(g=>g.id==selectedId);var r=S.progress.Item(selectedId);var c=GameCatalog.Palette[(int)def.color];
            block??=new MaterialPropertyBlock();glassRenderer.GetPropertyBlock(block);block.SetColor("_BaseColor",c);block.SetFloat("_Grime",r.grime);block.SetFloat("_Smoothness",Mathf.Lerp(.85f,.05f,r.grime));block.SetColor("_EmissionColor",c*(1-r.grime)*.12f);
            // Round 55: held up to the eye at the trays, the piece is large on screen; full refraction magnified the tray divider
            // behind it into a dark band across it, so it bends the background more gently there.
            block.SetFloat("_Refraction",kind==StationKind.Sort?.025f:glassRenderer.sharedMaterial.GetFloat("_Refraction"));
            block.SetFloat("_Held",kind==StationKind.Sort?1:0);   // round 56 (review 52: "an opaque lump"): held up, the piece is a coloured lens the scene shows through
            if(kind==StationKind.Clean){EnsureMask();var mb=def.HeldMesh.bounds;block.SetTexture(MaskId,cleanMask);block.SetVector(MaskRectId,new Vector4(mb.min.x,mb.min.y,mb.size.x,mb.size.y));block.SetFloat(MaskOnId,1);}
            glassRenderer.SetPropertyBlock(block);
            var filter=workingGlass.GetComponent<MeshFilter>();if(def.HeldMesh)filter.sharedMesh=def.HeldMesh;
            workingGlass.localScale=Vector3.one*((kind==StationKind.Sort?.065f:.52f)/Mathf.Max(def.HeldMesh.bounds.size.x,def.HeldMesh.bounds.size.y));
            if(kind==StationKind.Clean)
            {
                // Round 14: each piece floats just at the waterline whatever its thickness (thin pieces sank
                // out of sight in the deeper bowl). The reveal lift and pose resets use this as home.
                // Round 36 (reviews: "the glass sits on top of the water, no waterline"): about 40% under, so the surface
                // visibly cuts across the piece and tints its lower part.
                if(waterSurface){var p=workingGlass.position;workingGlass.position=new Vector3(p.x,waterSurface.bounds.center.y+glassRenderer.bounds.extents.y*.2f,p.z);glassHome=workingGlass.localPosition;}
                var wb=glassRenderer.bounds.extents;Reach=new Vector3(wb.x*.78f,wb.y*.85f,wb.z*.72f);
            }
        }
        public void Leave(){active=false;reveal=0;RestoreGlassPose();if(workingGlass)workingGlass.gameObject.SetActive(false);S.player.mode=PlayerMode.Walking;S.player.interactOverride=null;S.player.Teleport(returnPosition,returnRotation);S.player.SetLookRotation(returnView);S.Save();}
        void Update()
        {
            // Round 13: where the brush last touched and how long ago, for the ripple rings in the bowl water.
            if(kind==StationKind.Clean)Shader.SetGlobalVector(RippleId,new Vector4(lastContact.x,lastContact.z,Time.time-lastBrush,active?1:0));
            // Round 39 (review 35: "no waterline on the piece"): the basin surface, so the glass draws a bright waterline where it crosses it.
            if(kind==StationKind.Clean&&waterSurface){var wb=waterSurface.bounds;Shader.SetGlobalVector(BasinId,new Vector4(wb.center.x,wb.center.y,wb.center.z,Mathf.Max(wb.extents.x,wb.extents.z)));}
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
            // Round 59: the held piece is placed on the open gloved palm in LateUpdate (HoldOnPalm).
        }
        // Round 59 (review 55, all three: the held clean piece read as "a pillow", "a hollow shell", "a jelly blob" while it floated
        // alone, large, with the room magnified through it; the tray pieces, small and at rest, pass): at the sorting trays the
        // piece rests convex side up in an open gloved palm held low in front of the player, a few centimetres across, with the
        // canvas glove behind it rather than the room. Rounds 55-58 held it up to the eye on its own.
        static readonly Vector3 PalmAt=new Vector3(.1f,-.15f,.42f), PalmTilt=new Vector3(-26,-14,10);
        void LateUpdate()
        {
            if(kind!=StationKind.Sort||!heldPalm)return;
            bool on=active&&workingGlass&&workingGlass.gameObject.activeSelf&&S&&S.player;
            if(heldPalm.gameObject.activeSelf!=on)heldPalm.gameObject.SetActive(on);
            if(on)HoldOnPalm();
        }
        void HoldOnPalm()
        {
            var v=S.player.view.transform;
            heldPalm.SetPositionAndRotation(v.position+v.right*PalmAt.x+v.up*PalmAt.y+v.forward*PalmAt.z,Quaternion.LookRotation(v.forward,v.up)*Quaternion.Euler(PalmTilt));
            // Local +z (the concave inner face) down into the palm, so the convex outer face is up; turned a little on the palm.
            workingGlass.rotation=Quaternion.LookRotation(-heldPalm.up,heldPalm.forward)*Quaternion.Euler(0,0,25);
            var mf=workingGlass.GetComponent<MeshFilter>();var mb=mf&&mf.sharedMesh?mf.sharedMesh.bounds:new Bounds(Vector3.zero,Vector3.one);
            float k=workingGlass.lossyScale.x;
            workingGlass.position=heldPalm.position+heldPalm.up*(mb.max.z*k+.001f)-workingGlass.rotation*new Vector3(mb.center.x*k,mb.center.y*k,0)+heldPalm.forward*.012f;
        }
    }
}
