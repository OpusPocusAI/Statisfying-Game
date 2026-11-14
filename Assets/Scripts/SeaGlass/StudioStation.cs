using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace SeaGlass
{
    public enum StationKind { Recycle, Clean, Sort, Upgrade }
    public class StudioStation : Interactable
    {
        public StationKind kind; public Transform workingGlass; public Transform heldPalm; public Renderer glassRenderer; public Renderer waterSurface; public string selectedId; public bool active; public GlassColor selectedTray;
        MaterialPropertyBlock block; Vector3 returnPosition,glassHome;Quaternion returnRotation,returnView,glassRotation;float processing,reveal,nextFoam,nextDrip;public bool Processing=>processing>0;
        public bool RevealingCleanGlass=>reveal>0;
        float lastBrush=-9;Vector3 lastContact;static readonly int RippleId=Shader.PropertyToID("_SG_BrushRipple"),BasinId=Shader.PropertyToID("_SG_Basin");
        // True while the glass is being brushed (by the mouse or by the verification run); drives the brush animation.
        public bool BrushingNow=>active&&Time.time-lastBrush<.15f;
        // Half-extents (world x, z) the brush stroke sweeps over and the height it rests at, so it covers the whole piece.
        public static Vector3 Reach=new Vector3(.17f,.05f,.11f);
        // Round 65: seconds of brushing on the current piece; drives the stroke path (EquipmentVisuals.Stroke).
        public static float StrokeClock;Color revealColour;
        // Hands-on brushing (owner playtest, November 14: "brushing should be real mouse wiping"). At the basin the view holds still and
        // the mouse moves the brush over the glass; holding LMB presses it down, and the salt comes off only where it is wiped.
        // ManualOffset is where the bristles are, relative to the piece (world axes, as Stroke returns); Manual is false while the
        // automated scrub (Clean, used by the verification run and tests) is driving the brush instead.
        public static bool Manual;public static Vector3 ManualOffset,ManualVelocity;const float WipeTarget=.88f;
        bool[] silhouette;int silhouetteCount,cleanedCount;
        GameSession S=>GameSession.Instance;
        public override string Prompt=> kind==StationKind.Recycle?(S&&S.Count(ItemStage.Bucket,false)>0?"E · feed "+S.Count(ItemStage.Bucket,false)+" pieces of rubbish into the press":"Recycling press · bring rubbish here in your bucket"):kind==StationKind.Clean?"E · wash glass at the basin":kind==StationKind.Sort?"E · sort polished glass":"E · browse equipment catalogue";
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
            // Round 70 (review 66, all three: the brushing sleeve "a long hose-like tube across the basin"): at the basin the
            // camera stands closer and a little to the left, and aims past the piece, so the glass sits right of centre and
            // low in the frame and the brushing hand leaves the frame after a short stretch of sleeve.
            S.player.Teleport(transform.position-transform.forward*(cleaning?BasinBack:1.75f)-transform.right*(cleaning?BasinLeft:0)+Vector3.up*(cleaning?BasinUp:.12f),transform.rotation);S.player.interactOverride=Leave;
            S.player.LookAt(cleaning?workingGlass.position+transform.forward*BasinAimAhead-transform.right*BasinLeft:transform.position+Vector3.up*1.15f);
            if(cleaning){Manual=true;ManualOffset=new Vector3(Reach.x*.9f,Reach.y,-Reach.z*.6f);ManualVelocity=Vector3.zero;}
            S.Notify(kind==StationKind.Clean?"Hold LMB and move the mouse to scrub the salt off. E to leave.":"Aim at a matching colour tray and press LMB. E to leave.");
        }
        public bool Recycle()
        {
            // Carrying rubbish: the press takes the rubbish in your bucket. Progress commits at once; the pieces then drop in one by one.
            var fed=S.progress.items.Where(r=>r.stage==ItemStage.Bucket&&r.id.StartsWith("rubbish-")).ToList();
            if(fed.Count==0){S.Notify(S.truck.AtStudio&&S.Count(ItemStage.Truck,false)>0?"Take the rubbish out of the truck's rubbish crate first, then carry it here.":"Nothing to recycle. Bring rubbish here in your bucket.");return false;}
            foreach(var r in fed)r.stage=ItemStage.Recycled;int count=fed.Count;
            S.Award(count*12,count*5);S.Tutorial(7);S.Save();processing=Mathf.Max(processing,0)+count*.22f+.6f;
            FeedingUntil=Time.time+count*.22f+.3f;StartCoroutine(Feed(fed.Select(r=>r.id).ToArray(),count));return true;
        }
        public static float FeedingUntil;   // the bucket stays in hand while its rubbish drops into the press
        System.Collections.IEnumerator Feed(string[] ids,int count)
        {
            var items=FindObjectsByType<ShoreItem>(FindObjectsInactive.Include,FindObjectsSortMode.None).ToDictionary(i=>i.id);
            var mouth=transform.position+Vector3.up*1.1f;
            for(int k=0;k<ids.Length;k++)
            {
                var hand=S.player.view.transform.position+S.player.view.transform.forward*.55f-S.player.view.transform.up*.35f;
                if(items.TryGetValue(ids[k],out var item)&&Feedback.Instance)
                {
                    var mf=(item.visual?item.visual:item.transform).GetComponentInChildren<MeshFilter>(true);var mr=mf?mf.GetComponent<MeshRenderer>():null;
                    if(mf&&mr&&mr.sharedMaterial){var m=mr.sharedMaterial;Feedback.Instance.Fly(mf.sharedMesh,m,m.HasProperty("_BaseColor")?m.GetColor("_BaseColor"):Color.white,hand,mouth,mf.transform.lossyScale,mf.transform.rotation);}
                }
                Feedback.Instance?.Play(ids[k].EndsWith("0")?"metal":"plastic",.9f+k*.06f);
                yield return new WaitForSeconds(.22f);
                Feedback.Instance?.Burst(mouth,new Color(1f,.84f,.55f),5);
            }
            yield return new WaitForSeconds(.25f);
            Feedback.Instance?.Play("recycle");Feedback.Instance?.Burst(mouth,GameCatalog.Palette[0],16);
            int left=S.Count(ItemStage.Truck,false);
            S.Notify(count+" pieces recycled · +"+(count*5)+" caps · +"+(count*12)+" XP"+(left>0?". "+left+" more in the truck's rubbish crate.":""));
        }
        public bool SelectNext()
        {
            reveal=0;RestoreGlassPose();StrokeClock=0;
            var r=S.progress.items.FirstOrDefault(i=>i.id.StartsWith("glass-")&&i.stage==(kind==StationKind.Clean?ItemStage.Studio:ItemStage.Clean));selectedId=r?.id;
            if(workingGlass)workingGlass.gameObject.SetActive(r!=null);
            if(r!=null)UpdateGlass();return r!=null;
        }
        public void Clean(float delta)
        {
            if(string.IsNullOrEmpty(selectedId))return;var r=S.progress.Item(selectedId);if(r.stage!=ItemStage.Studio)return;
            r.grime=Mathf.Max(0,r.grime-delta/(S.progress.upgrades.Contains("washer")?3:9));
            // The salt comes off where the brush (or spray) actually touches the glass.
            StrokeClock+=Time.deltaTime;Manual=false;
            var contact=workingGlass.position+EquipmentVisuals.Stroke(true,StrokeClock);
            // Round 13: the brush sweeps a hard-edged path from where it was last frame, so the grime clears in
            // visible strokes (PowerWash), and the water ripples from the contact point.
            bool continuing=Time.time-lastBrush<.15f;StampClean(continuing?lastContact:contact,contact,Mathf.Min(S.progress.upgrades.Contains("washer")?.07f:.045f,Reach.z*.42f));   // round 85: narrower than a lane, so the first passes leave crust between them
            lastBrush=Time.time;lastContact=contact;
            UpdateGlass();Feedback.Instance.LoopAction("water");
            if(Time.time>=nextFoam){nextFoam=Time.time+.06f;Feedback.Instance.Foam(contact,.05f);Feedback.Instance.Splash(contact,EquipmentVisuals.StrokeVelocity(StrokeClock));
                // Round 114 (review 110: "no drips"): water running off the brush falls back into the bowl as drops and rings.
                if(Random.value<.55f)Feedback.Instance.Drip(contact+new Vector3(Random.Range(-.035f,.035f),Random.Range(.035f,.07f),Random.Range(-.025f,.025f)),waterSurface?waterSurface.bounds.center.y:contact.y-.01f);}
            Finish(r);
        }
        // Hands-on wipe: the mouse moves the brush; pressed, it scrubs the path it travels. Holding it still does nothing.
        public void Wipe(Vector2 mouseDelta,bool pressed)
        {
            if(string.IsNullOrEmpty(selectedId))return;var r=S.progress.Item(selectedId);if(r.stage!=ItemStage.Studio)return;
            var cam=S.player.view.transform;Vector3 right=Vector3.ProjectOnPlane(cam.right,Vector3.up).normalized,fwd=Vector3.ProjectOnPlane(cam.forward,Vector3.up).normalized;
            var limit=new Vector2(Reach.x/.78f+.02f,Reach.z/.72f+.02f);   // the whole piece, to its rim
            var before=ManualOffset;var move=(right*mouseDelta.x+fwd*mouseDelta.y)*.0011f;
            ManualOffset=new Vector3(Mathf.Clamp(ManualOffset.x+move.x,-limit.x,limit.x),Reach.y,Mathf.Clamp(ManualOffset.z+move.z,-limit.y,limit.y));
            ManualVelocity=Vector3.Lerp(ManualVelocity,(ManualOffset-before)/Mathf.Max(Time.deltaTime,1e-4f),.35f);
            if(!pressed){lastBrush=-9;return;}
            var from=workingGlass.position+before;var contact=workingGlass.position+ManualOffset;
            bool moving=(ManualOffset-before).sqrMagnitude>1e-8f;if(!moving)return;
            StampClean(from,contact,Mathf.Min(S.progress.upgrades.Contains("washer")?.075f:.05f,Reach.z*.46f));
            lastBrush=Time.time;lastContact=contact;StrokeClock+=Time.deltaTime;
            float wiped=silhouetteCount>0?cleanedCount/(float)silhouetteCount:0;r.grime=Mathf.Min(r.grime,Mathf.Clamp01(1-wiped/WipeTarget));
            UpdateGlass();Feedback.Instance.LoopAction("water");
            if(Time.time>=nextFoam){nextFoam=Time.time+.06f;Feedback.Instance.Foam(contact,.05f);Feedback.Instance.Splash(contact,ManualVelocity);
                if(Random.value<.55f)Feedback.Instance.Drip(contact+new Vector3(Random.Range(-.035f,.035f),Random.Range(.035f,.07f),Random.Range(-.025f,.025f)),waterSurface?waterSurface.bounds.center.y:contact.y-.01f);}
            Finish(r);
        }
        void Finish(ItemRecord r)
        {
            if(r.grime<=.001f)
            {
                r.grime=0;UpdateGlass();r.stage=ItemStage.Clean;S.Award(10);S.Tutorial(8);Feedback.Instance.Play("glass");
                // Round 65 (review 61, all three: "no clean-moment sparkle"): the colour-revealed moment is a ring of glints in the
                // glass colour, a few water droplets thrown up as the piece lifts, and a short glow from inside the glass.
                revealColour=GameCatalog.Palette[(int)S.catalog.glass.First(g=>g.id==r.id).color];
                Feedback.Instance.Reveal(workingGlass.position+Vector3.up*.04f,revealColour,glassRenderer?glassRenderer.bounds.extents.x:.15f);S.Save();
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
            if(tray)Feedback.Instance.Fly(def.PlateMesh,glassRenderer.sharedMaterial,GameCatalog.Palette[(int)color],workingGlass.position,tray.transform.position+Vector3.up*.15f,workingGlass.lossyScale,Quaternion.Euler(90,0,0));
            Feedback.Instance.Burst(tray?tray.transform.position+Vector3.up*.2f:workingGlass.position,GameCatalog.Palette[(int)color]);S.Save();SelectNext();return true;
        }
        // Per-piece mask of where the glass has been scrubbed (white = brushed). Presentation only:
        // completion still follows the record's grime value.
        // Round 13: two passes over a spot take the sand crust off, a third reveals the clear glass; the
        // shader draws each threshold as a hard edge.
        Texture2D cleanMask;string maskFor;Color32[] maskPixels;const int MaskSize=128;
        static readonly int MaskId=Shader.PropertyToID("_CleanMask"),MaskRectId=Shader.PropertyToID("_MaskRect"),MaskOnId=Shader.PropertyToID("_MaskOn"),StrokeDirId=Shader.PropertyToID("_SG_StrokeDir");
        void EnsureMask()
        {
            if(!cleanMask){cleanMask=new Texture2D(MaskSize,MaskSize,TextureFormat.R8,false){wrapMode=TextureWrapMode.Clamp,name="Clean mask"};maskPixels=new Color32[MaskSize*MaskSize];}
            if(maskFor!=selectedId){maskFor=selectedId;System.Array.Clear(maskPixels,0,maskPixels.Length);cleanMask.SetPixels32(maskPixels);cleanMask.Apply(false);BuildSilhouette();}
        }
        // Sweeps the brush head from one contact point to the next (a capsule in mask space), adding a fixed
        // amount once per sweep so overlapping strokes build up pass by pass.
        void StampClean(Vector3 fromWorld,Vector3 toWorld,float radius)
        {
            var mesh=S.catalog.glass.First(g=>g.id==selectedId).BasinMesh;if(!mesh)return;EnsureMask();
            var b=mesh.bounds;
            Vector2 UV(Vector3 w){var l=workingGlass.InverseTransformPoint(w);return new Vector2((l.x-b.min.x)/b.size.x,(l.y-b.min.y)/b.size.y);}
            Vector2 a=UV(fromWorld),c=UV(toWorld);
            float ru=radius/(workingGlass.lossyScale.x*b.size.x),rv=radius/(workingGlass.lossyScale.y*b.size.y);
            int x0=Mathf.FloorToInt((Mathf.Min(a.x,c.x)-ru)*MaskSize),x1=Mathf.CeilToInt((Mathf.Max(a.x,c.x)+ru)*MaskSize);
            int y0=Mathf.FloorToInt((Mathf.Min(a.y,c.y)-rv)*MaskSize),y1=Mathf.CeilToInt((Mathf.Max(a.y,c.y)+rv)*MaskSize);
            // Work in brush-radius units so the head stays round on a non-square piece.
            Vector2 A=new Vector2(a.x/ru,a.y/rv),C=new Vector2(c.x/ru,c.y/rv),AC=C-A;float len2=Mathf.Max(1e-6f,AC.sqrMagnitude);
            // Round 65: bristle streaks. Across the stroke the bristles clear unevenly in fine lines that run with the stroke and stay
            // put in the glass, so a part-cleaned pass shows scrubbed lines along the brush direction, not a soft round dab.
            Vector2 across=len2>1e-4f?new Vector2(-AC.y,AC.x).normalized:new Vector2(0,1);
            // Round 85: a fixed amount per crossing, whatever the brush speed or frame rate (each frame adds its share: the distance
            // swept over the head's width). One pass down a lane takes the crust off and most of the frost (it took three or four
            // overlapping passes), leaving the bristle lines frosted; the next pass along that lane clears them.
            float add=255f*2.1f*Mathf.Min(1f,Vector3.Distance(fromWorld,toWorld)/(2*radius));
            for(int y=Mathf.Max(0,y0);y<=Mathf.Min(MaskSize-1,y1);y++)for(int x=Mathf.Max(0,x0);x<=Mathf.Min(MaskSize-1,x1);x++)
            {
                var P=new Vector2((x+.5f)/MaskSize/ru,(y+.5f)/MaskSize/rv);float t=Mathf.Clamp01(Vector2.Dot(P-A,AC)/len2);
                float dd=(P-(A+AC*t)).sqrMagnitude;if(dd>1)continue;
                float s=Vector2.Dot(P,across)*1.6f;   // about 11 mask pixels per streak: finer aliases into speckle
               // Round 88 (review 84, all three: "dark comb-like stripes" in the half-clean piece): with the bristles leaving most of the
               // lane frosted, one pass cut rows of short clear dashes that read as a comb. The bristles now vary the pass only gently,
               // so the middle of a lane clears in one stroke and the bristle streaks show only along its edges, where the head is lighter.
               float bristle=.62f+.38f*(.5f+.5f*Mathf.Sin(s*6.2832f+Mathf.Sin(s*1.37f)*2.1f));
                int i=y*MaskSize+x;byte was=maskPixels[i].r;maskPixels[i].r=(byte)Mathf.Min(255,was+Mathf.RoundToInt(add*(1-dd*dd)*bristle));
                if(was<WipedLevel&&maskPixels[i].r>=WipedLevel&&silhouette!=null&&silhouette[i])cleanedCount++;   // round 21: a soft round brush head, strongest at its centre
            }
            cleanMask.SetPixels32(maskPixels);cleanMask.Apply(false);
        }
        const byte WipedLevel=170;   // a mask value the shader draws as clear glass
        // Which mask pixels the piece actually covers (its outline seen from above), so "how much is wiped" counts glass, not the empty
        // corners of its bounding box.
        void BuildSilhouette()
        {
            silhouette??=new bool[MaskSize*MaskSize];System.Array.Clear(silhouette,0,silhouette.Length);silhouetteCount=cleanedCount=0;
            if(string.IsNullOrEmpty(selectedId))return;var mesh=S.catalog.glass.First(g=>g.id==selectedId).BasinMesh;if(!mesh)return;
            var b=mesh.bounds;var v=mesh.vertices;var t=mesh.triangles;
            Vector2 UV(Vector3 l)=>new Vector2((l.x-b.min.x)/b.size.x*MaskSize,(l.y-b.min.y)/b.size.y*MaskSize);
            for(int k=0;k<t.Length;k+=3)
            {
                Vector2 a=UV(v[t[k]]),c=UV(v[t[k+1]]),d=UV(v[t[k+2]]);float area=(c.x-a.x)*(d.y-a.y)-(c.y-a.y)*(d.x-a.x);if(Mathf.Abs(area)<1e-6f)continue;
                int x0=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(c.x,d.x)))),x1=Mathf.Min(MaskSize-1,Mathf.CeilToInt(Mathf.Max(a.x,Mathf.Max(c.x,d.x))));
                int y0=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.y,Mathf.Min(c.y,d.y)))),y1=Mathf.Min(MaskSize-1,Mathf.CeilToInt(Mathf.Max(a.y,Mathf.Max(c.y,d.y))));
                for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(x+.5f,y+.5f);
                    float w0=((c.x-p.x)*(d.y-p.y)-(c.y-p.y)*(d.x-p.x))/area,w1=((d.x-p.x)*(a.y-p.y)-(d.y-p.y)*(a.x-p.x))/area,w2=1-w0-w1;
                    if(w0>=0&&w1>=0&&w2>=0)silhouette[y*MaskSize+x]=true;
                }
            }
            foreach(var s in silhouette)if(s)silhouetteCount++;
        }
        void OnDestroy(){if(cleanMask)Destroy(cleanMask);}
        void UpdateGlass()
        {
            if(!glassRenderer||selectedId==null)return;var def=S.catalog.glass.First(g=>g.id==selectedId);var r=S.progress.Item(selectedId);var c=GameCatalog.Palette[(int)def.color];
            block??=new MaterialPropertyBlock();glassRenderer.GetPropertyBlock(block);block.SetColor("_BaseColor",c);block.SetFloat("_Grime",r.grime);block.SetFloat("_Smoothness",Mathf.Lerp(.85f,.05f,r.grime));block.SetColor("_EmissionColor",c*(1-r.grime)*(kind==StationKind.Clean?.05f:.12f));   /* round 121 (review 117, all three: the 05b piece "a thick jelly block"): at the basin the clean glass glowed from within like a gummy; less */
            // Round 55: held up to the eye at the trays, the piece is large on screen; full refraction magnified the tray divider
            // behind it into a dark band across it, so it bends the background more gently there.
            block.SetFloat("_Refraction",kind==StationKind.Sort?.025f:glassRenderer.sharedMaterial.GetFloat("_Refraction"));
            block.SetFloat("_Held",1);   // round 71 (review 67: the basin piece "a pillowy jelly blob", "a soap blob"; the palm piece, shaded as a plate, passes): the basin piece too is shaded as a shard of bottle wall, two flat faces and a narrow edge wall, not as a dome   // round 56 (review 52: "an opaque lump"): held up, the piece is a coloured lens the scene shows through
            if(kind==StationKind.Clean){BasinThinEdge(def.basinThickSide);EnsureMask();var mb=def.BasinMesh.bounds;block.SetTexture(MaskId,cleanMask);block.SetVector(MaskRectId,new Vector4(mb.min.x,mb.min.y,mb.size.x,mb.size.y));block.SetFloat(MaskOnId,1);
                // Round 92: the brush scrubs along world x (EquipmentVisuals.Stroke); the crust's grain runs the same way in mask space.
                var sd=workingGlass.InverseTransformDirection(Vector3.right);Shader.SetGlobalVector(StrokeDirId,new Vector4(sd.x/mb.size.x,sd.y/mb.size.y,0,0));}
            glassRenderer.SetPropertyBlock(block);
            var shown=kind==StationKind.Sort?def.PlateMesh:def.BasinMesh;   /* round 115 (review 111, C: the palm piece "a flat rhombus with a pointed tip"): in the palm, the plate with rounded corners */   /* round 108 (review 104, all three: the palm piece "pillowy green jelly", "thick gummy candy"): the piece held up at the trays is the same thin hero plate as at the basin, not the thicker tray pebble */var filter=workingGlass.GetComponent<MeshFilter>();if(shown)filter.sharedMesh=shown;
            // Round 73 (review 69: at 0.42 m across the basin piece filled a fifth of the frame and read as a moulded sweet): smaller.
            workingGlass.localScale=Vector3.one*((kind==StationKind.Sort?.065f:.3f)/Mathf.Max(shown.bounds.size.x,shown.bounds.size.y));
            if(kind==StationKind.Sort)thickSide=def.plateThickSide;
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
        // Round 119 (review 115, all three: the palm piece "thick and gummy, more like candy than a wedge"): each plate is a wedge
        // (round108.py), but in the palm its thick side often faced the eye, so the nearest edge was its tallest wall, a pale band
        // like a gummy's side. The piece is turned in its own plane so the thick side points away: the near edge is the thin one,
        // and the colour deepens across the face away from the eye.
        Vector2 thickSide;   // GlassDefinition.plateThickSide of the piece in the palm
        // Round 121 (review 117, all three: the 05b piece "a thick jelly block"): as in the palm, the basin piece is turned in its own
        // plane so its thick side points away from the eye and the near edge, the one the reveal tips up, is the thin one.
        Quaternion basinSpin=Quaternion.identity;
        void BasinThinEdge(Vector2 d)
        {
            basinSpin=Quaternion.identity;workingGlass.localRotation=glassRotation;
            if(d.sqrMagnitude<1e-10f)return;
            var eye=transform.position-transform.forward*BasinBack-transform.right*BasinLeft+Vector3.up*(BasinUp+1.6f);   // where Enter stands the player
            var view=workingGlass.InverseTransformDirection(workingGlass.position-eye);
            var v2=new Vector2(view.x,view.y);if(v2.sqrMagnitude<1e-8f)return;
            basinSpin=Quaternion.AngleAxis(Vector2.SignedAngle(d,v2),Vector3.forward);
            workingGlass.localRotation=glassRotation*basinSpin;
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
                workingGlass.localRotation=glassRotation*Quaternion.Euler(lift*-8,0,lift*4)*basinSpin;   // round 121: tipped less toward the eye, so less of the edge wall shows (it read as a thick block)
                // Round 98 (review 94, all three: "no splashes, drips or runoff"): water runs off the lifted piece and drips from its lower edge.
                if(kind==StationKind.Clean&&glassRenderer&&lift>.2f&&Time.time>=nextDrip){nextDrip=Time.time+.035f;var gb=glassRenderer.bounds;var e=Random.insideUnitCircle.normalized*.85f;
                    Feedback.Instance.Drip(new Vector3(gb.center.x+e.x*gb.extents.x,gb.min.y+.004f,gb.center.z+e.y*gb.extents.z),waterSurface?waterSurface.bounds.center.y:gb.min.y-.03f);}
                if(glassRenderer&&kind==StationKind.Clean){glassRenderer.GetPropertyBlock(block);block.SetColor("_EmissionColor",revealColour*(.05f+.06f*Mathf.Pow(Mathf.Sin(t*Mathf.PI),2)));/* round 121: a fainter glow *//* round 66 (review 62: "uniform opaque cyan, jelly"): a faint glow; the glints carry the moment */glassRenderer.SetPropertyBlock(block);}
                if(reveal<=0)SelectNext();
                return;
            }
            if(kind==StationKind.Clean&&Mouse.current!=null)Wipe(Mouse.current.delta.ReadValue(),Mouse.current.leftButton.isPressed);
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
        // Round 68 (review 64: the open palm "a flat mitten"): turned so the fingers point partly across the view (yaw -40, was -14),
        // so their curl shows from the side instead of foreshortening away from the camera.
        const float BasinBack=.55f,BasinLeft=.26f,BasinUp=.3f,BasinAimAhead=.32f;   // round 70: the basin camera
        static readonly Vector3 PalmAt=new Vector3(.12f,-.15f,.42f), PalmTilt=new Vector3(-26,-40,14);
        void LateUpdate()
        {
            if(kind!=StationKind.Sort||!heldPalm)return;
            bool on=active&&workingGlass&&workingGlass.gameObject.activeSelf&&S&&S.player;
            if(heldPalm.gameObject.activeSelf!=on)heldPalm.gameObject.SetActive(on);
            if(on){HoldOnPalm();HeldTwinkle();}
        }
        float nextTwinkle;
        // Round 106 (review 102, all three: nothing happens in 06): the clean piece catches the light in the palm: small flashes of its
        // own colour run over its top face every moment or two, a little in front of it so the glass does not hide them.
        void HeldTwinkle()
        {
            if(Time.time<nextTwinkle||!glassRenderer||string.IsNullOrEmpty(selectedId))return;nextTwinkle=Time.time+Random.Range(.05f,.1f);
            var g=S.catalog.glass.FirstOrDefault(x=>x.id==selectedId);var col=g!=null?GameCatalog.Palette[(int)g.color]:Color.white;
            var b=glassRenderer.bounds;var cam=S.player.view.transform;var o=Random.insideUnitCircle*.8f;
            var at=b.center+new Vector3(o.x*b.extents.x,b.extents.y*.6f,o.y*b.extents.z);at+=(cam.position-at).normalized*.012f;
            Feedback.Instance.Twinkle(at,col,Random.value<.35f?Random.Range(.03f,.045f):Random.Range(.012f,.022f));
        }
        void HoldOnPalm()
        {
            var v=S.player.view.transform;
            heldPalm.SetPositionAndRotation(v.position+v.right*PalmAt.x+v.up*PalmAt.y+v.forward*PalmAt.z,Quaternion.LookRotation(v.forward,v.up)*Quaternion.Euler(PalmTilt));
            // Local +z (the concave inner face) down into the palm, so the convex outer face is up; turned a little on the palm.
            workingGlass.rotation=Quaternion.LookRotation(-heldPalm.up,heldPalm.forward)*Quaternion.Euler(0,0,25);
            // Round 119: turned on the palm so the thick side points away from the eye (the turn above when it cannot be told).
            if(thickSide.sqrMagnitude>1e-10f)
            {
                var flat=Quaternion.LookRotation(-heldPalm.up,heldPalm.forward);
                var view=Quaternion.Inverse(flat)*(heldPalm.position-v.position);
                var v2=new Vector2(view.x,view.y);
                if(v2.sqrMagnitude>1e-8f)workingGlass.rotation=flat*Quaternion.AngleAxis(Vector2.SignedAngle(thickSide,v2),Vector3.forward);
            }
            var mf=workingGlass.GetComponent<MeshFilter>();var mb=mf&&mf.sharedMesh?mf.sharedMesh.bounds:new Bounds(Vector3.zero,Vector3.one);
            float k=workingGlass.lossyScale.x;
            workingGlass.position=heldPalm.position+heldPalm.up*(mb.max.z*k+.004f)-workingGlass.rotation*new Vector3(mb.center.x*k,mb.center.y*k,0)+heldPalm.forward*.012f;
        }
    }
}
