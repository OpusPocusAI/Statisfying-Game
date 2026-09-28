using UnityEngine;
namespace SeaGlass
{
    // Authored stained-light shapes avoid a projector/render texture for this fixed golden-hour scene.
    public class MosaicLightSpill : MonoBehaviour
    {
        public MosaicPanel panel;public Renderer surface;
        MaterialPropertyBlock properties;
        void LateUpdate()
        {
            if(!panel||!surface||!GameSession.Instance)return;
            float strength=panel.Illumination;
            surface.enabled=strength>0;
            properties??=new MaterialPropertyBlock();properties.SetFloat("_Intensity",strength);surface.SetPropertyBlock(properties);
        }
    }
}
