using UnityEngine;
using UnityEngine.InputSystem;
namespace SeaGlass
{
    [RequireComponent(typeof(CharacterController))]
    public class UtilityTruck : Interactable
    {
        public Transform seat,body; public Transform[] glassCargo,rubbishCargo; public Transform[] wheels;
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
            p.transform.SetParent(seat);p.transform.localPosition=new Vector3(0,-1.6f,0);p.transform.localRotation=Quaternion.identity;p.view.transform.localRotation=Quaternion.identity;p.interactOverride=Exit;S.Notify("W/S accelerate & reverse · A/D steer · Space brake · E exit · R recover");
        }
        public void Exit()
        {
            if(Mathf.Abs(speed)>1){S.Notify("Brake to a stop before getting out.");return;}
            driving=false;var p=S.player;p.transform.SetParent(null);p.mode=PlayerMode.Walking;p.interactOverride=null;
            Vector3 exit=transform.position-transform.right*2.4f+Vector3.up*.3f;
            if(Physics.CheckCapsule(exit+Vector3.up*.4f,exit+Vector3.up*1.5f,.28f))exit=transform.position+transform.right*2.4f+Vector3.up*.3f;
            if(Physics.CheckCapsule(exit+Vector3.up*.4f,exit+Vector3.up*1.5f,.28f))exit=new Vector3(9,.4f,Mathf.Clamp(transform.position.z,-15,180));
            p.Teleport(exit,transform.rotation);p.safePosition=exit;S.Save();
        }
        public bool Load(bool glass)
        {
            int count=0;
            foreach(var r in S.progress.items)if(r.stage==ItemStage.Bucket&&r.id.StartsWith(glass?"glass-":"rubbish-")&&S.CargoCount<S.progress.TruckCapacity){r.stage=ItemStage.Truck;count++;}
            if(count==0){S.Notify("No "+(glass?"glass":"rubbish")+" to load, or the truck is full.");return false;}
            S.Tutorial(5);RefreshCargo();S.Save();Feedback.Instance?.Play(glass?"glass":"metal");Feedback.Instance?.Burst(transform.position-transform.forward*1.5f+Vector3.up,glass?GameCatalog.Palette[0]:Color.gray,12);S.Notify(count+" pieces loaded into the "+(glass?"glass":"recycling")+" crate.");return true;
        }
        public void RefreshCargo()
        {
            int gc=S.Count(ItemStage.Truck,true),rc=S.Count(ItemStage.Truck,false);
            for(int i=0;i<glassCargo.Length;i++)glassCargo[i].gameObject.SetActive(i<gc);
            for(int i=0;i<rubbishCargo.Length;i++)rubbishCargo[i].gameObject.SetActive(i<rc);
            if(body)body.localPosition=new Vector3(0,-S.CargoCount*.0025f,0);
        }
        public void ResetVehicle(bool useCurrentRegion=true)
        { bool studio=useCurrentRegion&&AtStudio;speed=0;controller.enabled=false;transform.SetPositionAndRotation(new Vector3(15,.3f,studio?150:8),studio?Quaternion.Euler(0,180,0):Quaternion.identity);controller.enabled=true;S.Notify("Truck recovered safely beside the road."); }
        public void SkipDrive()
        {
            if(!S.progress.skipDrive){S.Notify("Complete one manual drive to the studio first.");return;}
            bool studio=AtStudio;controller.enabled=false;transform.SetPositionAndRotation(new Vector3(15,.3f,studio?8:150),studio?Quaternion.identity:Quaternion.Euler(0,180,0));controller.enabled=true;
            if(!driving)S.player.Teleport(new Vector3(10,.3f,studio?8:150),Quaternion.identity);speed=0;S.Save();S.Notify(studio?"Back at Shell Cove.":"Welcome to your seaside studio.");
        }
        void Update()
        {
            if(!S||S.menu||S.paused)return;
            if(transform.position.y < -3 || Mathf.Abs(transform.position.x)>31){ResetVehicle();return;}
            if(!driving){speed=Mathf.MoveTowards(speed,0,Time.deltaTime*8);return;}
            var k=Keyboard.current;if(k==null)return;
            float throttle=(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0),steer=(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0);
            speed=Mathf.MoveTowards(speed,throttle*(throttle<0?2.5f:5f),Time.deltaTime*(k.spaceKey.isPressed?14:2.8f));if(k.spaceKey.isPressed)speed=Mathf.MoveTowards(speed,0,Time.deltaTime*25);
            transform.Rotate(0,steer*speed*16*Time.deltaTime,0);var collision=controller.Move((transform.forward*speed+Vector3.down*3)*Time.deltaTime);if((collision&CollisionFlags.Sides)!=0)speed*=.85f;
            foreach(var w in wheels)w.Rotate(speed*Time.deltaTime*100,0,0,Space.Self);
            if(body)body.localRotation=Quaternion.Euler(Mathf.Sin(Time.time*8)*Mathf.Abs(speed)*.2f,0,-steer*speed*.6f);
            Feedback.Instance?.Engine(Mathf.Abs(speed)/5);
            if(AtStudio&&departure.z<100){arrivalTimer+=Time.deltaTime;if(arrivalTimer>1&&!S.progress.skipDrive){S.progress.skipDrive=true;S.Tutorial(6);S.Save();S.Notify("Studio reached. Optional Skip Drive is now available at the tailgate.");}}
        }
    }
}
