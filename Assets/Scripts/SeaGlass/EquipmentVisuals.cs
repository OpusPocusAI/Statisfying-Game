using UnityEngine;
namespace SeaGlass
{
    public class EquipmentVisuals : MonoBehaviour
    {
        public GameObject bucket,sack,scoop,brush,sprayer;public Transform bucketFill;public Transform[] dividers;
        Vector3[] dividerScales; Vector3 brushHome,sprayerHome; StudioStation cleaning;
        void Awake()
        {
            dividerScales=new Vector3[dividers.Length];
            for(int i=0;i<dividers.Length;i++)if(dividers[i])dividerScales[i]=dividers[i].localScale;
            if(brush)brushHome=brush.transform.localPosition;if(sprayer)sprayerHome=sprayer.transform.localPosition;
            foreach(var station in FindObjectsByType<StudioStation>(FindObjectsSortMode.None))if(station.kind==StationKind.Clean)cleaning=station;
        }
        void Update()
        {
            var s=GameSession.Instance;if(!s)return;bool walking=s.player.mode==PlayerMode.Walking&&!s.menu;
            if(bucket)bucket.SetActive(walking&&s.player.equippedTool==1&&!s.progress.upgrades.Contains("bucket"));
            if(sack)sack.SetActive(walking&&s.player.equippedTool==1&&s.progress.upgrades.Contains("bucket"));
            if(scoop)scoop.SetActive(walking&&s.player.equippedTool==0&&s.progress.upgrades.Contains("scoop"));
            for(int i=0;i<dividers.Length;i++)if(dividers[i])dividers[i].localScale=Vector3.Scale(dividerScales[i],new Vector3(1,s.progress.upgrades.Contains("truck")?1.8f:1,1));
            bool washing=cleaning&&cleaning.active&&!s.menu;
            if(brush)brush.SetActive(washing&&!s.progress.upgrades.Contains("washer"));if(sprayer)sprayer.SetActive(washing&&s.progress.upgrades.Contains("washer"));
            bool brushing=washing&&!s.paused&&UnityEngine.InputSystem.Mouse.current!=null&&UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
            Vector3 stroke=brushing?new Vector3(Mathf.Sin(Time.time*12)*.09f,0,Mathf.Cos(Time.time*12)*.035f):Vector3.zero;
            if(brush)brush.transform.localPosition=brushHome+stroke;if(sprayer)sprayer.transform.localPosition=sprayerHome+stroke*.25f;
            if(bucketFill)bucketFill.localScale=new Vector3(.9f,Mathf.Max(.03f,s.BucketCount/(float)s.progress.BucketCapacity),.9f);
        }
    }
}
