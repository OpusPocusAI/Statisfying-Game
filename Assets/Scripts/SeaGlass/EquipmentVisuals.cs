using UnityEngine;
namespace SeaGlass
{
    public class EquipmentVisuals : MonoBehaviour
    {
        public GameObject bucket,sack,scoop,brush,sprayer;public Transform bucketFill,sackFill;public Transform[] dividers;
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
            var s=GameSession.Instance;if(!s)return;bool walking=s.player.mode==PlayerMode.Walking&&!s.menu&&!s.PresentationActive;
            if(bucket)bucket.SetActive(walking&&s.player.equippedTool==1&&!s.progress.upgrades.Contains("bucket"));
            if(sack)sack.SetActive(walking&&s.player.equippedTool==1&&s.progress.upgrades.Contains("bucket"));
            if(scoop)scoop.SetActive(walking&&s.player.equippedTool==0&&s.progress.upgrades.Contains("scoop"));
            for(int i=0;i<dividers.Length;i++)if(dividers[i])dividers[i].localScale=Vector3.Scale(dividerScales[i],new Vector3(1,s.progress.upgrades.Contains("truck")?1.8f:1,1));
            bool washing=cleaning&&cleaning.active&&cleaning.selectedId!=null&&!s.menu;
            if(brush)brush.SetActive(washing&&!s.progress.upgrades.Contains("washer"));if(sprayer)sprayer.SetActive(washing&&s.progress.upgrades.Contains("washer"));
            bool brushing=washing&&!s.ControlsBlocked&&UnityEngine.InputSystem.Mouse.current!=null&&UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
            if(washing&&!s.ControlsBlocked)
            {
                // Keep the brush at the glass, rather than waving an unrelated block at the edge of the camera.
                // The modelled brush pivots at its bristle tips, so .05 rests it on the domed glass.
                Vector3 stroke=Stroke(brushing,Time.time);
                if(brush){brush.transform.position=cleaning.workingGlass.position+stroke;brush.transform.rotation=Quaternion.Euler(0,-25+(brushing?Mathf.Cos(Time.time*7)*12:0),0);}
                if(sprayer){sprayer.transform.position=cleaning.workingGlass.position+new Vector3(.3f,.2f,-.12f)+stroke*.15f;var aim=cleaning.workingGlass.position-sprayer.transform.position;sprayer.transform.rotation=Quaternion.LookRotation(aim,Vector3.up)*Quaternion.Euler(0,-90,0);}
            }
            else if(!washing){if(brush)brush.transform.localPosition=brushHome;if(sprayer)sprayer.transform.localPosition=sprayerHome;}
            float fill=s.BucketCount/(float)s.progress.BucketCapacity;
            RefreshFill(bucketFill,fill);RefreshFill(sackFill,fill);
        }
        // Where the bristles meet the glass. A scrubbing path that sweeps the whole piece (StudioStation
        // cleans exactly here, so the reveal follows the brush).
        public static Vector3 Stroke(bool brushing,float t)=>brushing?new Vector3(Mathf.Sin(t*7)*.17f,.05f,Mathf.Sin(t*2.3f)*.11f):new Vector3(.3f,.09f,.1f);
        static void RefreshFill(Transform contents,float amount)
        {if(!contents)return;contents.gameObject.SetActive(amount>0);contents.localScale=new Vector3(.9f,Mathf.Max(.03f,amount),.9f);}
    }
}
