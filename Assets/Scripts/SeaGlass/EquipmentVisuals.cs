using UnityEngine;
namespace SeaGlass
{
    public class EquipmentVisuals : MonoBehaviour
    {
        public GameObject bucket,sack,scoop,brush,sprayer;float lastBrushing=-9,nextBrushDrip;public Transform bucketFill,sackFill;public Transform[] dividers;
        public bool Washing{get;private set;}
        Vector3[] dividerScales; Vector3 brushHome,sprayerHome; StudioStation cleaning;
        void Awake()
        {
            dividerScales=new Vector3[dividers.Length];
            for(int i=0;i<dividers.Length;i++)if(dividers[i])dividerScales[i]=dividers[i].localScale;
            if(brush)brushHome=brush.transform.localPosition;if(sprayer)sprayerHome=sprayer.transform.localPosition;
            foreach(var station in FindObjectsByType<StudioStation>(FindObjectsSortMode.None))if(station.kind==StationKind.Clean)cleaning=station;
        }
        // Round 69 (review 65: round 68 turned the forearm toward the viewer, which made it larger, hid the brush and crowded the
        // basin; cleaning fell from 4 to 3): the arm comes in from the side again, as before round 68. The brush tips a little
        // about its handle so the bristles face the camera (review 65: "a flat block with its bristles hidden").
        const float BristleTilt=18;
        void Update()
        {
            var s=GameSession.Instance;if(!s)return;bool walking=s.player.mode==PlayerMode.Walking&&!s.menu&&!s.PresentationActive&&!s.player.IsIndoors;
            bool carrying=s.player.mode==PlayerMode.Walking&&!s.menu&&!s.PresentationActive&&s.player.equippedTool==1&&(!s.player.IsIndoors||s.Count(ItemStage.Bucket,false)>0||Time.time<StudioStation.FeedingUntil);   // carrying rubbish to the press
            if(bucket)bucket.SetActive(carrying&&!s.progress.upgrades.Contains("bucket"));
            if(sack)sack.SetActive(carrying&&s.progress.upgrades.Contains("bucket"));
            if(scoop)scoop.SetActive(walking&&s.player.equippedTool==0&&s.progress.upgrades.Contains("scoop"));
            for(int i=0;i<dividers.Length;i++)if(dividers[i])dividers[i].localScale=Vector3.Scale(dividerScales[i],new Vector3(1,s.progress.upgrades.Contains("truck")?1.8f:1,1));
            bool washing=cleaning&&cleaning.active&&cleaning.selectedId!=null&&!s.menu;Washing=washing;
            if(brush)brush.SetActive(washing&&!s.progress.upgrades.Contains("washer"));if(sprayer)sprayer.SetActive(washing&&s.progress.upgrades.Contains("washer"));
            bool pressed=UnityEngine.InputSystem.Mouse.current!=null&&UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;bool brushing=washing&&!s.ControlsBlocked&&(cleaning.BrushingNow||pressed);
            if(washing&&!s.ControlsBlocked)
            {
                // Keep the brush at the glass, rather than waving an unrelated block at the edge of the camera.
                // The modelled brush pivots at its bristle tips, so .05 rests it on the domed glass.
                // Hands-on brushing: the brush sits where the mouse put it, pressed onto the glass while LMB is held and lifted otherwise,
                // and leans a little into the direction it is moving.
                bool down=StudioStation.Manual?pressed:brushing;
                Vector3 stroke=StudioStation.Manual?StudioStation.ManualOffset+(down?Vector3.zero:new Vector3(0,.04f,0)):Stroke(brushing,StudioStation.StrokeClock);
                float lean=StudioStation.Manual?Mathf.Clamp(StudioStation.ManualVelocity.x*-25,-14,14):brushing?Mathf.Cos(StudioStation.StrokeClock*7)*12:0;
                if(brush){brush.transform.position=cleaning.workingGlass.position+stroke;brush.transform.rotation=Quaternion.Euler(0,-25+lean,0)*Quaternion.Euler(BristleTilt,0,0);}
                // Round 114 (review 110: "no drips"; the stills are taken with the brush lifted, after the brushing drops have fallen): a wet
                // brush lifted off the glass keeps dripping from its bristles for a couple of seconds, each drop ringing the water below.
                if(brushing)lastBrushing=Time.time;
                else if(brush&&brush.activeSelf&&Time.time-lastBrushing<2.4f&&Time.time>=nextBrushDrip&&Feedback.Instance)
                {nextBrushDrip=Time.time+Random.Range(.07f,.16f);var tip=brush.transform.position+brush.transform.right*Random.Range(-.05f,.05f)+brush.transform.forward*Random.Range(-.025f,.025f)-Vector3.up*Random.Range(.02f,.05f);   // just below the bristles, where the eye sees them
                 Feedback.Instance.Drip(tip,cleaning.waterSurface?cleaning.waterSurface.bounds.center.y:tip.y-.06f);}
                if(sprayer){sprayer.transform.position=cleaning.workingGlass.position+new Vector3(.3f,.2f,-.12f)+stroke*.15f;var aim=cleaning.workingGlass.position+(StudioStation.Manual?StudioStation.ManualOffset:Vector3.zero)-sprayer.transform.position;sprayer.transform.rotation=Quaternion.LookRotation(aim,Vector3.up)*Quaternion.Euler(0,-90,0);}
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
        // Round 85 (review 81, all three: grime "comes off in mottled patches, not brush strokes"): the passes crept along the piece
        // a little at a time, so each overlapped the last and the cleared area grew as one soft-edged band. Now every side-to-side
        // pass runs down its own lane (far edge, near edge, then the middle, and round again), so the first passes cut separate
        // clean stripes through the crust, as a PowerWash stroke does, and the lanes close up as the brush keeps going.
        static readonly int[] Lanes={0,2,1};
        static float LaneZ(int pass)=>StudioStation.Reach.z*(1-Lanes[((pass%3)+3)%3]);
        public static Vector3 Stroke(bool brushing,float t)
        {
            if(!brushing)return new Vector3(StudioStation.Reach.x+.12f,StudioStation.Reach.y+.04f,.1f);
            float u=t*7f/Mathf.PI;int k=Mathf.FloorToInt(u);float ph=u-k,c=Mathf.Cos(t*7);
            float z=Mathf.Lerp(LaneZ(k),LaneZ(k+1),Mathf.SmoothStep(0,1,(ph-.82f)/.18f));   // steps to the next lane as the brush turns at the end of a pass
            return new Vector3(-c*StudioStation.Reach.x,StudioStation.Reach.y*(1-c*c*.25f),z);
        }
        // How fast the brush head is moving along the stroke (world units per second), for the water it flicks off.
        public static Vector3 StrokeVelocity(float t){float h=.01f;return (Stroke(true,t+h)-Stroke(true,t-h))/(2*h);}
        static void RefreshFill(Transform contents,float amount)
        {if(!contents)return;contents.gameObject.SetActive(amount>0);contents.localScale=new Vector3(.9f,Mathf.Max(.03f,amount),.9f);}
    }
}
