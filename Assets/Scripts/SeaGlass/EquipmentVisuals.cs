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
            var s=GameSession.Instance;if(!s)return;bool walking=s.player.mode==PlayerMode.Walking&&!s.menu&&!s.PresentationActive&&!s.player.IsIndoors;
            if(bucket)bucket.SetActive(walking&&s.player.equippedTool==1&&!s.progress.upgrades.Contains("bucket"));
            if(sack)sack.SetActive(walking&&s.player.equippedTool==1&&s.progress.upgrades.Contains("bucket"));
            if(scoop)scoop.SetActive(walking&&s.player.equippedTool==0&&s.progress.upgrades.Contains("scoop"));
            for(int i=0;i<dividers.Length;i++)if(dividers[i])dividers[i].localScale=Vector3.Scale(dividerScales[i],new Vector3(1,s.progress.upgrades.Contains("truck")?1.8f:1,1));
            bool washing=cleaning&&cleaning.active&&cleaning.selectedId!=null&&!s.menu;
            if(brush)brush.SetActive(washing&&!s.progress.upgrades.Contains("washer"));if(sprayer)sprayer.SetActive(washing&&s.progress.upgrades.Contains("washer"));
            bool brushing=washing&&!s.ControlsBlocked&&(cleaning.BrushingNow||UnityEngine.InputSystem.Mouse.current!=null&&UnityEngine.InputSystem.Mouse.current.leftButton.isPressed);
            if(washing&&!s.ControlsBlocked)
            {
                // Keep the brush at the glass, rather than waving an unrelated block at the edge of the camera.
                // The modelled brush pivots at its bristle tips, so .05 rests it on the domed glass.
                Vector3 stroke=Stroke(brushing,StudioStation.StrokeClock);
                if(brush){brush.transform.position=cleaning.workingGlass.position+stroke;brush.transform.rotation=Quaternion.Euler(0,-25+(brushing?Mathf.Cos(StudioStation.StrokeClock*7)*12:0),0);}
                if(sprayer){sprayer.transform.position=cleaning.workingGlass.position+new Vector3(.3f,.2f,-.12f)+stroke*.15f;var aim=cleaning.workingGlass.position-sprayer.transform.position;sprayer.transform.rotation=Quaternion.LookRotation(aim,Vector3.up)*Quaternion.Euler(0,-90,0);}
            }
            else if(!washing){if(brush)brush.transform.localPosition=brushHome;if(sprayer)sprayer.transform.localPosition=sprayerHome;}
            float fill=s.BucketCount/(float)s.progress.BucketCapacity;
            RefreshFill(bucketFill,fill);RefreshFill(sackFill,fill);
        }
        // Where the bristles meet the glass. A scrubbing path that sweeps the whole piece (StudioStation
        // cleans exactly here, so the reveal follows the brush).
        // Round 65 (review 61, all three: "grime comes off in blotches, not strokes"): parallel side-to-side passes that work
        // steadily from one end of the piece to the other and back (they were a Lissajous figure whose crossing sweeps left a
        // patchwork), timed by the station's brushing clock so the brush and the cleared path always agree.
        static float Tri(float u){u-=Mathf.Floor(u);return 1-Mathf.Abs(1-2*u);}
        public static Vector3 Stroke(bool brushing,float t)=>brushing?new Vector3(Mathf.Sin(t*7)*StudioStation.Reach.x,StudioStation.Reach.y*(1-Mathf.Sin(t*7)*Mathf.Sin(t*7)*.25f),StudioStation.Reach.z*(1-2*Tri(t*.11f))):new Vector3(StudioStation.Reach.x+.12f,StudioStation.Reach.y+.04f,.1f);   // round 13: sweeps the whole piece
        // How fast the brush head is moving along the stroke (world units per second), for the water it flicks off.
        public static Vector3 StrokeVelocity(float t){float h=.01f;return (Stroke(true,t+h)-Stroke(true,t-h))/(2*h);}
        static void RefreshFill(Transform contents,float amount)
        {if(!contents)return;contents.gameObject.SetActive(amount>0);contents.localScale=new Vector3(.9f,Mathf.Max(.03f,amount),.9f);}
    }
}
