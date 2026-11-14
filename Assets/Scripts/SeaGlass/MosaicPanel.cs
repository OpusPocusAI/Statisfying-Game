using System.Collections;
using System.Linq;
using UnityEngine;
namespace SeaGlass
{
    public class MosaicPanel : Interactable
    {
        public int index; public MosaicSocket[] sockets; public Light glow; public Renderer backing;
        public float Illumination {get;private set;}
        public bool IsRevealing {get;private set;}
        CoastalPlayer presentationPlayer;Quaternion entryView;bool emphasizing;float motesUntil,nextMote;
        // Round 106 (review 102, all three: no response in 08): once a panel lights, motes of its colours drift slowly in its light, in
        // front of the glass and down through the beam toward the floor, for a while after it completes.
        void Update()
        {
            if(Time.time>=motesUntil||Time.time<nextMote||sockets==null||sockets.Length==0||!S)return;nextMote=Time.time+.015f;
            var slot=sockets[Random.Range(0,sockets.Length)];if(!slot)return;var col=GameCatalog.Palette[(int)S.catalog.glass.First(x=>x.id==slot.glassId).color];
            float out_=Random.Range(.12f,2.6f);
            Feedback.Instance.Mote(slot.transform.position-transform.forward*out_+Vector3.down*out_*Random.Range(.3f,.75f)+Random.insideUnitSphere*.35f,col);
        }
        GameSession S=>GameSession.Instance;
        // Clarity pass (owner playtest: "we need a manner to make it feel more clear"): how many of your sorted pieces belong here, and where.
        public override string Prompt{get{int ready=sockets.Count(x=>S.progress.Item(x.glassId).stage==ItemStage.Sorted);return (ready>0?"E · place "+ready+" sorted piece"+(ready>1?"s":"")+" in the glowing sockets of ":"Wash and sort matching glass for ")+S.catalog.panelNames[index]+" ("+sockets.Count(x=>S.progress.Item(x.glassId).stage==ItemStage.Placed)+" / 10)";}}
        public override void Interact()
        {
            var slot=sockets.FirstOrDefault(x=>S.progress.Item(x.glassId).stage==ItemStage.Sorted);
            if(!slot){S.Notify("Wash and sort matching glass for this panel. F reveals remaining beach glass.");return;}Place(slot.glassId);
        }
        public bool Place(string id)
        {
            var slot=sockets.FirstOrDefault(s=>s.glassId==id);if(!slot||S.progress.Item(id).stage!=ItemStage.Sorted)return false;
            S.progress.Item(id).stage=ItemStage.Placed;S.Award(12);S.Tutorial(10);Feedback.Instance.Play("glass",1+System.Array.IndexOf(sockets,slot)*.06f);Feedback.Instance.Burst(slot.transform.position,GameCatalog.Palette[(int)S.catalog.glass.First(x=>x.id==id).color],8);Refresh();
            StartCoroutine(Snap(slot.piece.transform));
            if(sockets.All(s=>S.progress.Item(s.glassId).stage==ItemStage.Placed)&&!S.progress.completed[index])
            {S.progress.completed[index]=true;S.Award(180,40);StartCoroutine(Completion());}
            S.Save();return true;
        }
        public void Refresh()
        {
            if(!S)return;
            foreach(var slot in sockets)slot.Refresh();
            SetIllumination(IsRevealing?Illumination:S.progress.completed[index]?1:0);
        }
        void SetIllumination(float value)
        {
            Illumination=Mathf.Clamp01(value);if(glow)glow.intensity=Illumination*3;
            foreach(var slot in sockets)slot.SetGlow(Mathf.Lerp(.12f,.65f,Illumination));
        }
        void RestoreView(){if(emphasizing&&presentationPlayer)presentationPlayer.SetLookRotation(entryView);emphasizing=false;}
        public void ResetPresentation()
        {
            StopAllCoroutines();RestoreView();IsRevealing=false;
            foreach(var slot in sockets)slot.ResetPose();
        }
        IEnumerator Snap(Transform piece)
        {
            var home=piece.localPosition;
            for(float t=0;t<1;t+=Time.deltaTime/.32f)
            {
                float ease=1-Mathf.Pow(1-t,3);
                piece.localPosition=home+Vector3.back*(.18f*(1-ease));
                piece.localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI)*.035f);yield return null;
            }
            piece.localPosition=home;piece.localScale=Vector3.one;
        }
        IEnumerator Completion()
        {
            var player=S.player;presentationPlayer=player;entryView=player.view.transform.localRotation;
            emphasizing=player.mode==PlayerMode.Walking&&Vector3.Distance(player.view.transform.position,transform.position)<6;
            Quaternion from=player.view.transform.rotation,to=Quaternion.LookRotation(transform.position-player.view.transform.position);
            IsRevealing=true;SetIllumination(0);
            S.BeginPresentation();
            S.Notify(S.catalog.panelNames[index]+" complete. Let the light come through.");
            // The snap lands first. A short breath separates placement from the harmonic/light resolution.
            for(float t=0;t<.45f;t+=Time.deltaTime){if(emphasizing)player.view.transform.rotation=Quaternion.Slerp(from,to,Mathf.SmoothStep(0,1,t/.45f));yield return null;}
            Feedback.Instance.Play("complete");motesUntil=Time.time+14;
            for(float t=0;t<1.9f;t+=Time.deltaTime){SetIllumination(Mathf.SmoothStep(0,1,t/1.9f));yield return null;}
            SetIllumination(1);yield return new WaitForSeconds(.2f);
            for(float t=0;t<.2f;t+=Time.deltaTime){if(emphasizing)player.view.transform.rotation=Quaternion.Slerp(to,from,Mathf.SmoothStep(0,1,t/.2f));yield return null;}
            RestoreView();IsRevealing=false;
            if(S.progress.completed.All(x=>x)) {S.Notify("THREE SHORES. ONE HOME. Your studio is restored.");StudioUI.Instance.ending=true;foreach(var r in FindObjectsByType<RestorationDecor>(FindObjectsSortMode.None))r.Restore();Feedback.Instance.Play("complete",1.25f);}
            S.EndPresentation();
        }
    }
}
