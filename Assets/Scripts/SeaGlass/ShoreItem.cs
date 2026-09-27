using UnityEngine;
namespace SeaGlass
{
    public class ShoreItem : Interactable
    {
        public string id; public int section; public bool glass; public int colorIndex; public Transform visual,mound;
        public Vector3 home; public float dig; Renderer surface; MaterialPropertyBlock block; float highlightUntil;Vector3 moundSize;
        GameSession S=>GameSession.Instance;
        public ItemRecord Record=>S.progress.Item(id);
        public override string Prompt=> !S.progress.SectionOpen(section)?"This cove opens at level "+(section+1):Record.stage==ItemStage.Buried?(S.player.equippedTool==0?"Hold LMB · scoop sand":"1 · equip shovel, then hold LMB to scoop"):"E · collect "+(glass?"cloudy "+((GlassColor)colorIndex)+" glass":"shoreline rubbish");
        void Awake(){home=transform.position;surface=visual?visual.GetComponent<Renderer>():GetComponent<Renderer>();block=new MaterialPropertyBlock();if(mound)moundSize=mound.localScale;}
        public void Refresh()
        {
            var stage=Record.stage;gameObject.SetActive(stage==ItemStage.Buried||stage==ItemStage.Shore);
            if(mound)mound.gameObject.SetActive(stage==ItemStage.Buried);
            if(stage==ItemStage.Buried){dig=0;if(mound)mound.localScale=moundSize;}
            if(visual)visual.localPosition=new Vector3(0,stage==ItemStage.Buried?-.06f:.13f,0);
            if(glass&&surface){block??=new MaterialPropertyBlock();surface.GetPropertyBlock(block);block.SetFloat("_Grime",Record.grime);block.SetFloat("_Smoothness",.08f);surface.SetPropertyBlock(block);}
        }
        public override void Use(float delta)
        {
            if(!S.progress.SectionOpen(section)||Record.stage!=ItemStage.Buried)return;
            dig+=delta*(S.progress.upgrades.Contains("scoop")?2.7f:1);
            Feedback.Instance?.LoopAction("sand");
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
            Record.stage=ItemStage.Bucket;S.Award(glass?12:5,id.EndsWith("7")?2:0);S.Tutorial(S.BucketCount>=S.progress.BucketCapacity?4:3);
            Feedback.Instance?.Play(glass?"glass":id.EndsWith("0")?"metal":"plastic",1+colorIndex*.08f);S.Notify(glass?"A cloudy treasure. Load it into the truck.":"One less piece on the shore.");Refresh();S.Save();return true;
        }
        public void Highlight(){if(glass&&gameObject.activeSelf)highlightUntil=Time.time+12;}
        void Update()
        {
            if(transform.position.y < -2 || Vector3.Distance(transform.position,home)>80)transform.position=home;
            if(!surface)return;block??=new MaterialPropertyBlock();surface.GetPropertyBlock(block);
            block.SetColor("_EmissionColor",Time.time<highlightUntil?GameCatalog.Palette[colorIndex]*(1+Mathf.Sin(Time.time*4)*.5f):Color.black);surface.SetPropertyBlock(block);
        }
    }
}
