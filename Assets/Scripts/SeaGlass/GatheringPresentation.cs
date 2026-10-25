using UnityEngine;
using UnityEngine.InputSystem;

namespace SeaGlass
{
    // First-person artwork moves; the camera and forgiving interaction ray do not.
    public class GatheringPresentation : MonoBehaviour
    {
        public CoastalPlayer player;public GameObject toyBlade;
        Vector3 home;float phase,weight;Progress visit;
        void Awake(){home=player.tool.localPosition;}
        void LateUpdate()
        {
            var s=GameSession.Instance;if(!s)return;
            if(visit!=s.progress){visit=s.progress;phase=0;weight=0;}
            bool screen=StudioUI.Instance&&(StudioUI.Instance.ending||StudioUI.Instance.shop);
            bool visible=player.mode==PlayerMode.Walking&&!s.menu&&!s.PresentationActive&&!screen&&player.equippedTool==0&&!player.IsIndoors;
            player.tool.gameObject.SetActive(visible);
            if(toyBlade)toyBlade.SetActive(!s.progress.upgrades.Contains("scoop"));
            if(!visible){phase=0;weight=0;player.tool.localPosition=home;player.tool.localRotation=Quaternion.Euler(0,0,-15);return;}
            if(s.ControlsBlocked)return;
            var find=player.focus as ShoreItem;
            bool digging=find&&s.progress.SectionOpen(find.section)&&find.Record.stage==ItemStage.Buried&&Mouse.current!=null&&Mouse.current.leftButton.isPressed;
            weight=Mathf.MoveTowards(weight,digging?1:0,Time.deltaTime*7);
            if(digging||weight>0)phase+=Time.deltaTime*9;
            float stroke=(1-Mathf.Cos(phase))*.5f;
            player.tool.localPosition=home+new Vector3(-.04f,-.07f,.08f)*stroke*weight;
            player.tool.localRotation=Quaternion.Euler(28*stroke*weight,-10*stroke*weight,-15+7*Mathf.Sin(phase)*weight);
        }
    }
}
