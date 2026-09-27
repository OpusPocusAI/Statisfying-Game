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
            float strength=GameSession.Instance.progress.completed[panel.index]&&panel.glow?Mathf.Clamp01(panel.glow.intensity/3):0;
            surface.enabled=strength>0;
            properties??=new MaterialPropertyBlock();properties.SetFloat("_Intensity",strength);surface.SetPropertyBlock(properties);
        }
    }
}
