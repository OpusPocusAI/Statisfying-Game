using UnityEngine;
namespace SeaGlass
{
    public class RecyclingMotion : MonoBehaviour
    {
        public StudioStation station;public Transform press;
        Vector3 home;float elapsed;
        void Awake(){if(press)home=press.localPosition;}
        void LateUpdate()
        {
            if(!press||!station)return;
            elapsed=station.Processing?elapsed+Time.deltaTime:0;
            // One restrained compression-and-return cycle follows the existing processing sound.
            press.localPosition=home-Vector3.up*(Mathf.Sin(Mathf.Clamp01(elapsed/1.5f)*Mathf.PI)*.22f);
        }
    }
}
