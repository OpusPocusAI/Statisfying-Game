using System.Linq;
using UnityEngine;
namespace SeaGlass
{
    public class ShoreItem : Interactable
    {
        public string id; public int section; public bool glass; public int colorIndex; public Transform visual,mound;
        public Vector3 home; public float dig; Renderer surface; MaterialPropertyBlock block; float highlightUntil;Vector3 moundSize;bool spilledHere;
        public const float FieldGrime=.5f;   // out in the field a piece shows half its recorded crust (see Refresh)
        GameSession S=>GameSession.Instance;
        public ItemRecord Record=>S.progress.Item(id);
        public override string Prompt=> !S.progress.SectionOpen(section)?"This cove opens at level "+(section+1):Record.stage==ItemStage.Buried?(S.player.equippedTool==0?"Hold LMB · scoop sand":"1 · equip shovel, then hold LMB to scoop"):"E · collect "+(glass?"cloudy "+((GlassColor)colorIndex)+" glass":"shoreline rubbish");
        GameObject contact;   // the soft contact shadow on the sand (GroundingPass): only while the find lies on the surface
        void Awake(){var cs=transform.Find("Find contact shadow");contact=cs?cs.gameObject:null;home=transform.position;surface=visual?visual.GetComponent<Renderer>():GetComponent<Renderer>();block=new MaterialPropertyBlock();if(mound)moundSize=mound.localScale;}
        public void Refresh()
        {
            var stage=Record.stage;gameObject.SetActive(stage==ItemStage.Buried||stage==ItemStage.Shore);
            if(mound)mound.gameObject.SetActive(stage==ItemStage.Buried);
            if(contact)contact.SetActive(stage!=ItemStage.Buried);
            if(stage==ItemStage.Buried){dig=0;if(mound)mound.localScale=moundSize;}
            if(visual)visual.localPosition=new Vector3(0,stage==ItemStage.Buried?-.06f:.13f,0);
            spilledHere=stage==ItemStage.Shore&&Record.spilled;
            if(spilledHere)
            {   // Drive pass: cargo that bounced off the truck rests where it landed on the road.
                transform.position=Record.spot;var rs=GetComponentsInChildren<Renderer>().Where(r=>!contact||r.gameObject!=contact).ToArray();
                if(rs.Length>0)transform.position+=Vector3.up*(Record.spot.y+.004f-rs.Min(r=>r.bounds.min.y));
                // The contact shadow lies flat on the road right under the piece (it was laid on the sand where the find was placed).
                if(contact&&rs.Length>0){var b=rs[0].bounds;contact.transform.SetPositionAndRotation(new Vector3(b.center.x,Record.spot.y+.008f,b.center.z),Quaternion.Euler(90,0,0));}
                foreach(var c in GetComponentsInChildren<Collider>())c.isTrigger=true;
            }
            // Rubbish and fallen glass pass (Nov 16, review: fallen cargo glass "reads as a rock"): out in the field, wet from the sand or
            // the sea, a piece shows its frosted colour with only a light sand crust; the full dried salt-and-sand crust is what the wash
            // bowl shows. Display only: the record's grime is unchanged.
            if(glass&&surface){block??=new MaterialPropertyBlock();surface.GetPropertyBlock(block);block.SetFloat("_Grime",Record.grime*FieldGrime);block.SetFloat("_Smoothness",.08f);surface.SetPropertyBlock(block);}
        }
        public override void Use(float delta)
        {
            if(!S.progress.SectionOpen(section)||Record.stage!=ItemStage.Buried)return;
            dig+=delta*(S.progress.upgrades.Contains("scoop")?2.7f:1);
            Feedback.Instance?.LoopAction("sand");Feedback.Instance?.Dust(transform.position);
            if(mound)mound.localScale=moundSize*Mathf.Lerp(1,.05f,dig/1.7f);
            if(dig<1.7f)return;
            Record.stage=ItemStage.Shore;S.Tutorial(2);Refresh();Feedback.Instance?.Burst(transform.position,GameCatalog.Palette[colorIndex],8);
            if(S.progress.upgrades.Contains("scoop"))
                foreach(var c in Physics.OverlapSphere(transform.position,2.6f)){var i=c.GetComponentInParent<ShoreItem>();if(i&&i!=this&&S.progress.SectionOpen(i.section)&&i.Record.stage==ItemStage.Buried){i.Record.stage=ItemStage.Shore;i.Refresh();}}
        }
        public override void Interact()
        {
            if(!S.progress.SectionOpen(section)){S.Notify("Restore glass and recycle rubbish to unlock this cove.");return;}
            if(Record.stage==ItemStage.Buried){S.Notify(S.player.equippedTool==0?"Hold the left mouse button to uncover this find.":"Press 1 to equip the shovel, then hold the left mouse button.");return;}
            Collect();
            if(S.progress.upgrades.Contains("scoop"))foreach(var c in Physics.OverlapSphere(transform.position,2.4f)){var i=c.GetComponentInParent<ShoreItem>();if(i&&i!=this&&i.Record.stage==ItemStage.Shore&&S.progress.SectionOpen(i.section))i.Collect();}
        }
        public bool Collect()
        {
            if(Record.stage!=ItemStage.Shore)return false;
            if(S.BucketCount>=S.progress.BucketCapacity){S.Notify("Bucket full. Load the truck's rear compartments.");return false;}
            if(S.CargoCount+S.BucketCount>=S.progress.TruckCapacity){S.Notify("Truck allocation full. Return to the studio to unload.");return false;}
            if(glass&&visual&&surface)Feedback.Instance?.Fly(visual.GetComponent<MeshFilter>().sharedMesh,surface.sharedMaterial,GameCatalog.Palette[colorIndex],visual.position,S.player.view.transform.position+S.player.view.transform.forward*.65f-S.player.view.transform.up*.35f,visual.lossyScale,visual.rotation);
            bool first=S.progress.tutorial<3,returned=Record.spilled;Record.stage=ItemStage.Bucket;Record.spilled=false;spilledHere=false;if(!returned)S.Award(glass?12:5,id.EndsWith("7")?2:0);S.Tutorial(S.BucketCount>=S.progress.BucketCapacity?4:3);
            Feedback.Instance?.Play(glass?"glass":id.EndsWith("0")?"metal":"plastic",1+colorIndex*.08f);// Clarity pass (owner playtest: "not initially how much I can carry"): the first find says what the bucket holds.
            S.Notify(returned?"Back in your bucket. Load it into the truck again.":first?"Into your bucket. It holds "+S.progress.BucketCapacity+" finds; when it is full, load it into the truck's rear crates.":glass?"A cloudy treasure. Load it into the truck.":"One less piece on the shore.");Refresh();S.Save();return true;
        }
        public void Highlight(){if(glass&&gameObject.activeSelf)highlightUntil=Time.time+12;}
        void Update()
        {
            if(!spilledHere&&(transform.position.y < -2 || Vector3.Distance(transform.position,home)>80))transform.position=home;
            if(!surface)return;block??=new MaterialPropertyBlock();surface.GetPropertyBlock(block);
            block.SetColor("_EmissionColor",Time.time<highlightUntil?GameCatalog.Palette[colorIndex]*(1+Mathf.Sin(Time.time*4)*.5f):Color.black);surface.SetPropertyBlock(block);
        }
    }
}
