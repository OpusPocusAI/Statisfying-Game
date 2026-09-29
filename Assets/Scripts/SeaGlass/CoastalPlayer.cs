using UnityEngine;
using UnityEngine.InputSystem;
namespace SeaGlass
{
    [RequireComponent(typeof(CharacterController))]
    public class CoastalPlayer : MonoBehaviour
    {
        public Camera view; public PlayerMode mode; public bool blocked=true; public float sensitivity=.12f; public bool invertY;
        public Interactable focus; public Transform tool; public System.Action interactOverride;
        CharacterController body; float pitch,fall,driverYaw; public int equippedTool; public Vector3 safePosition=new(0,2,6);
        void Awake() {body=GetComponent<CharacterController>();}
        void Update()
        {
            if(blocked || Keyboard.current==null || Mouse.current==null) {focus=null;return;}
            var mouse=Mouse.current.delta.ReadValue()*sensitivity;
            pitch=Mathf.Clamp(pitch+mouse.y*(invertY?1:-1),-78,78);
            view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            if(mode!=PlayerMode.Driving) transform.Rotate(0,mouse.x,0);
            else {driverYaw=Mathf.Clamp(driverYaw+mouse.x,-65,65);view.transform.localRotation=Quaternion.Euler(pitch,driverYaw,0);}
            if(mode==PlayerMode.Walking)
            {
                var k=Keyboard.current;
                var input=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
                var movement=transform.TransformDirection(new Vector3(input.x,0,input.y).normalized)*(k.leftShiftKey.isPressed?6:3.8f);
                if(body.isGrounded) {fall=-2; if(k.spaceKey.wasPressedThisFrame)fall=5;}
                fall-=18*Time.deltaTime; movement.y=fall;body.Move(movement*Time.deltaTime);
                if(transform.position.y < -4) Teleport(safePosition,Quaternion.identity);
                if(GameSession.Instance&&transform.position.sqrMagnitude>12)GameSession.Instance.Tutorial(1);
            }
            if(Keyboard.current.digit1Key.wasPressedThisFrame)equippedTool=0;
            if(Keyboard.current.digit2Key.wasPressedThisFrame)equippedTool=1;
            if(Mouse.current.scroll.ReadValue().y!=0)equippedTool=1-equippedTool;
            focus=null;
            if(Physics.Raycast(view.ViewportPointToRay(new Vector3(.5f,.5f)),out var hit,4.2f))focus=hit.collider.GetComponentInParent<Interactable>();
            if(Keyboard.current.eKey.wasPressedThisFrame) { if(interactOverride!=null)interactOverride(); else if(focus)focus.Interact(); }
            if(Mouse.current.leftButton.isPressed && focus && equippedTool==0)focus.Use(Time.deltaTime);
            if(tool)tool.gameObject.SetActive(mode==PlayerMode.Walking&&equippedTool==0);
        }
        public void Teleport(Vector3 position,Quaternion rotation) {body.enabled=false;transform.SetPositionAndRotation(position,rotation);body.enabled=true;fall=0;}
        // Keep the controller's stored angles in sync when a station or vehicle changes the view.
        public void SetLookRotation(Quaternion localRotation)
        {pitch=Mathf.Clamp(Mathf.DeltaAngle(0,localRotation.eulerAngles.x),-78,78);driverYaw=mode==PlayerMode.Driving?Mathf.Clamp(Mathf.DeltaAngle(0,localRotation.eulerAngles.y),-65,65):0;view.transform.localRotation=Quaternion.Euler(pitch,driverYaw,0);}
        public void LookAt(Vector3 target){var local=Quaternion.Inverse(transform.rotation)*(target-view.transform.position);pitch=-Mathf.Atan2(local.y,new Vector2(local.x,local.z).magnitude)*Mathf.Rad2Deg;driverYaw=0;view.transform.localRotation=Quaternion.Euler(pitch,0,0);}
        public void SetCursor(bool locked) { Cursor.lockState=locked?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!locked; }
    }
}
