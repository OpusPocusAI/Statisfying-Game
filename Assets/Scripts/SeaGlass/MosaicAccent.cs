using UnityEngine;

namespace SeaGlass
{
    // Picture lighting follows the same reveal value as the glass and floor, including loaded saves.
    public class MosaicAccent : MonoBehaviour
    {
        public MosaicPanel panel;public Light lamp;public Renderer lens;
        MaterialPropertyBlock properties;
        void LateUpdate()
        {
            if(!panel)return;float light=panel.Illumination;
            if(lamp)lamp.intensity=light*1.4f;
            if(lens)
            {
                properties??=new MaterialPropertyBlock();lens.GetPropertyBlock(properties);
                properties.SetColor("_BaseColor",Color.Lerp(new Color(.3f,.26f,.17f),new Color(1,.8f,.48f),light));
                properties.SetColor("_EmissionColor",new Color(1,.65f,.3f)*light*1.3f);lens.SetPropertyBlock(properties);
            }
        }
    }
}
