using System.Collections;
using System.Linq;
using UnityEngine;
namespace SeaGlass
{
    public class MosaicPanel : Interactable
    {
        public int index; public MosaicSocket[] sockets; public Light glow; public Renderer backing;
        GameSession S=>GameSession.Instance;
        public override string Prompt=>"E · fit a sorted piece into "+S.catalog.panelNames[index]+" ("+sockets.Count(x=>S.progress.Item(x.glassId).stage==ItemStage.Placed)+" / 10)";
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
            if(glow)glow.intensity=S.progress.completed[index]?3:0;
        }
        IEnumerator Snap(Transform piece)
        {
            for(float t=0;t<1;t+=Time.deltaTime/.32f){piece.localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI)*.065f);yield return null;}piece.localScale=Vector3.one;
        }
        IEnumerator Completion()
        {
            var player=S.player;bool emphasize=!S.testMode&&player.mode==PlayerMode.Walking;Quaternion oldView=player.view.transform.rotation;
            S.BeginPresentation();
            if(emphasize)player.view.transform.LookAt(transform.position);
            S.Notify(S.catalog.panelNames[index]+" complete. Let the light come through.");Feedback.Instance.Play("complete");
            yield return new WaitForSeconds(.6f);
            for(float t=0;t<2;t+=Time.deltaTime){if(glow)glow.intensity=Mathf.SmoothStep(0,3,t/2);foreach(var s in sockets)s.SetGlow(t*.6f);yield return null;}
            if(emphasize)player.view.transform.rotation=oldView;
            if(S.progress.completed.All(x=>x)) {S.Notify("THREE SHORES. ONE HOME. Your studio is restored.");StudioUI.Instance.ending=true;foreach(var r in FindObjectsByType<RestorationDecor>(FindObjectsSortMode.None))r.Restore();Feedback.Instance.Play("complete",1.25f);}
            S.EndPresentation();
        }
    }
}
