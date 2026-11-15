using UnityEngine;
using UnityEngine.InputSystem;
namespace SeaGlass
{
    [RequireComponent(typeof(CharacterController))]
    public class UtilityTruck : Interactable
    {
        public Transform seat,body; public Transform[] glassCargo,rubbishCargo; public Transform[] wheels;
        // Drive pass (owner playtest, November 14: "slow, pointless, a chore"): top speed 5 → 13 m/s, eased steering that tightens
        // less at speed, a wider view and dust when fast, and a body that jolts over the road's potholes and sand ripples.
        public const float TopSpeed=13f; float steer,joltAmp,joltTime=9,joltRoll,sag,baseFov,nextDust; Vector3 viewBase;
        float speed; CharacterController controller; float arrivalTimer; Vector3 departure; public bool driving;public string lastObstacle;public float Speed=>speed;
        void OnControllerColliderHit(ControllerColliderHit hit){if(hit.normal.y<.5f)lastObstacle=hit.collider.name;}
        GameSession S=>GameSession.Instance;
        public bool AtStudio=>transform.position.z>145;
        public override string Prompt=>"E · drive utility truck";
        void Awake(){controller=GetComponent<CharacterController>();foreach(var col in GetComponentsInChildren<Collider>())if(col!=controller)col.isTrigger=true;}
        public override void Interact(){if(driving)Exit();else Enter();}
        public void Enter()
        {
            var p=S.player;driving=true;departure=transform.position;arrivalTimer=0;p.mode=PlayerMode.Driving;p.GetComponent<CharacterController>().enabled=false;
            baseFov=p.view.fieldOfView;viewBase=p.view.transform.localPosition;steer=0;p.transform.SetParent(seat);p.transform.localPosition=new Vector3(0,-1.6f,0);p.transform.localRotation=Quaternion.identity;p.SetLookRotation(Quaternion.identity);p.interactOverride=Exit;S.Notify("W/S accelerate & reverse · A/D steer · Space brake · E exit · R recover");
        }
        public void Exit()
        {
            if(Mathf.Abs(speed)>1){S.Notify("Brake to a stop before getting out.");return;}
            driving=false;var p=S.player;if(baseFov>0){p.view.fieldOfView=baseFov;p.view.transform.localPosition=viewBase;}p.transform.SetParent(null);p.mode=PlayerMode.Walking;p.interactOverride=null;
            Vector3 exit=transform.position-transform.right*2.4f+Vector3.up*.3f;
            if(Physics.CheckCapsule(exit+Vector3.up*.4f,exit+Vector3.up*1.5f,.28f))exit=transform.position+transform.right*2.4f+Vector3.up*.3f;
            if(Physics.CheckCapsule(exit+Vector3.up*.4f,exit+Vector3.up*1.5f,.28f))exit=new Vector3(9,.4f,Mathf.Clamp(transform.position.z,-15,180));
            p.Teleport(exit,transform.rotation);p.SetLookRotation(p.view.transform.localRotation);p.safePosition=exit;S.Save();
        }
        public bool Load(bool glass)
        {
            int count=0;
            foreach(var r in S.progress.items)if(r.stage==ItemStage.Bucket&&r.id.StartsWith(glass?"glass-":"rubbish-")&&S.CargoCount<S.progress.TruckCapacity){r.stage=ItemStage.Truck;count++;}
            if(count==0){S.Notify("No "+(glass?"glass":"rubbish")+" to load, or the truck is full.");return false;}
            S.Tutorial(5);RefreshCargo();S.Save();Feedback.Instance?.Play(glass?"glass":"metal");Feedback.Instance?.Burst(transform.position-transform.forward*1.5f+Vector3.up,glass?GameCatalog.Palette[0]:new Color(1f,.84f,.55f),12);S.Notify(count+" pieces loaded into the "+(glass?"glass":"recycling")+" crate.");return true;
        }
        // Carrying rubbish (owner playtest, November 14: "rubbish should have to be carried to the recycler"). At the studio the rubbish
        // crate empties into your bucket, as much as it holds; you walk it to the press and feed it in.
        public bool Unload()
        {
            if(!AtStudio){S.Notify("Park the truck beside the studio first.");return false;}
            int room=S.progress.BucketCapacity-S.BucketCount,count=0;
            foreach(var r in S.progress.items)if(count<room&&r.stage==ItemStage.Truck&&r.id.StartsWith("rubbish-")){r.stage=ItemStage.Bucket;count++;}
            if(count==0){S.Notify(S.Count(ItemStage.Truck,false)==0?"The rubbish crate is empty.":"Your bucket is full. Feed it to the recycling press first.");return false;}
            S.player.equippedTool=1;RefreshCargo();S.Save();
            Feedback.Instance?.Play("metal",.9f);Feedback.Instance?.Burst(transform.position-transform.forward*1.5f+Vector3.up,new Color(1f,.84f,.55f),8);
            S.Notify(count+" pieces of rubbish in your bucket. Carry them to the recycling press.");return true;
        }
        public void RefreshCargo()
        {
            int gc=S.Count(ItemStage.Truck,true),rc=S.Count(ItemStage.Truck,false);
            // Round 29 (reviews: "one glass piece and one can on a sand fill"): each loaded find shows as a small heap of three.
            for(int i=0;i<glassCargo.Length;i++)glassCargo[i].gameObject.SetActive(i<gc*3);
            for(int i=0;i<rubbishCargo.Length;i++)rubbishCargo[i].gameObject.SetActive(i<rc*3);
            sag=-S.CargoCount*.0025f;if(body)body.localPosition=new Vector3(0,sag,0);
        }
        // A jolt rings through the body and the driver's view and dies away.
        public void Jolt(float amount){joltAmp=amount;joltTime=0;joltRoll=Random.Range(-1.6f,1.6f);}
        public void Bump(float keep){speed*=keep;}
        void Settle()
        {
            joltTime+=Time.deltaTime;float j=joltAmp*Mathf.Exp(-joltTime*7)*Mathf.Sin(joltTime*38);
            if(body){body.localPosition=new Vector3(0,sag+j*.05f,0);body.localRotation=Quaternion.Euler(Mathf.Sin(Time.time*8)*Mathf.Min(Mathf.Abs(speed),6)*.15f+j*2.5f,0,-steer*Mathf.Min(Mathf.Abs(speed),8)*.4f+j*joltRoll);}
            if(driving&&baseFov>0)S.player.view.transform.localPosition=viewBase+Vector3.up*j*.04f;
        }
        public void ResetVehicle(bool useCurrentRegion=true)
        { bool studio=useCurrentRegion&&AtStudio;speed=0;controller.enabled=false;transform.SetPositionAndRotation(new Vector3(15,.3f,studio?150:8),studio?Quaternion.Euler(0,180,0):Quaternion.identity);controller.enabled=true;S.Notify("Truck recovered safely beside the road."); }
        public void SkipDrive()
        {
            if(!S.progress.skipDrive){S.Notify("Complete one manual drive to the studio first.");return;}
            bool studio=AtStudio;controller.enabled=false;transform.SetPositionAndRotation(new Vector3(15,.3f,studio?8:150),studio?Quaternion.identity:Quaternion.Euler(0,180,0));controller.enabled=true;
            var arrival=new Vector3(10,.3f,studio?8:150);
            if(!driving)S.player.Teleport(arrival,Quaternion.identity);S.player.safePosition=arrival;speed=0;S.Save();S.Notify(studio?"Back at Shell Cove.":"Welcome to your seaside studio.");
        }
        void Update()
        {
            if(!S||S.ControlsBlocked)return;
            if(transform.position.y < -3 || Mathf.Abs(transform.position.x)>31){ResetVehicle();return;}
            if(!driving){speed=Mathf.MoveTowards(speed,0,Time.deltaTime*8);controller.Move(Vector3.down*3*Time.deltaTime);if(joltTime<2)Settle();return;}
            var k=Keyboard.current;if(k==null)return;
            float throttle=(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0),steerIn=(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0);
            steer=Mathf.MoveTowards(steer,steerIn,Time.deltaTime*(steerIn==0?5f:3.5f));
            bool reversing=throttle!=0&&Mathf.Abs(speed)>.2f&&Mathf.Sign(throttle)!=Mathf.Sign(speed);
            speed=Mathf.MoveTowards(speed,throttle*(throttle<0?4f:TopSpeed),Time.deltaTime*(k.spaceKey.isPressed?14:reversing?9:throttle!=0?4.2f:2.8f));if(k.spaceKey.isPressed)speed=Mathf.MoveTowards(speed,0,Time.deltaTime*25);
            transform.Rotate(0,steer*Mathf.Min(Mathf.Abs(speed)*18,70)*Mathf.Sign(speed)*Time.deltaTime,0);var collision=controller.Move((transform.forward*speed+Vector3.down*3)*Time.deltaTime);if((collision&CollisionFlags.Sides)!=0)speed*=.85f;
            // Cylinder wheels use local Y as their axle, including the attached hub artwork.
            foreach(var w in wheels)w.Rotate(0,-speed*Time.deltaTime*100,0,Space.Self);
            Settle();
            Feedback.Instance?.Engine(Mathf.Abs(speed)/TopSpeed);
            if(Mathf.Abs(speed)>7&&Time.time>nextDust){nextDust=Time.time+.15f;Feedback.Instance?.Dust(transform.position-transform.forward*1.9f+transform.right*Random.Range(-1f,1f));}
            var cam=S.player.view;if(baseFov>0)cam.fieldOfView=Mathf.Lerp(cam.fieldOfView,baseFov+8*Mathf.Clamp01(Mathf.Abs(speed)/TopSpeed),Time.deltaTime*3);
            if(AtStudio&&departure.z<100){arrivalTimer+=Time.deltaTime;if(arrivalTimer>1&&!S.progress.skipDrive){S.progress.skipDrive=true;S.Tutorial(6);S.Save();S.Notify("Studio reached. Optional Skip Drive is now available at the tailgate.");}}
        }
    }
}
