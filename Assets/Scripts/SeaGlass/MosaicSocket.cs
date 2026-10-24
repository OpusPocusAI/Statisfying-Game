using System.Linq;
using UnityEngine;
namespace SeaGlass {
    public class MosaicSocket : Interactable
    {
        public string glassId;public MosaicPanel panel;public GameObject piece;public Renderer surface;public Color color;
        MaterialPropertyBlock block;
        Vector3 homePosition,homeScale;
        void Awake(){homePosition=piece.transform.localPosition;homeScale=piece.transform.localScale;}
        public void ResetPose(){piece.transform.localPosition=homePosition;piece.transform.localScale=homeScale;}
        public override string Prompt=>GameSession.Instance.progress.Item(glassId).stage==ItemStage.Placed?"A little piece of the coast":"E · place "+colorName+" glass";
        string colorName=>GameSession.Instance.catalog.glass.First(x=>x.id==glassId).color.ToString();
        public override void Interact(){if(!panel.Place(glassId))panel.Interact();}
        public void Refresh(){var stage=GameSession.Instance.progress.Item(glassId).stage;piece.SetActive(stage==ItemStage.Placed);SetGlow(Mathf.Lerp(.12f,.65f,panel.Illumination));}
        public void SetGlow(float value){if(!surface)return;block??=new MaterialPropertyBlock();surface.GetPropertyBlock(block);block.SetColor("_EmissionColor",color*value);block.SetFloat("_Facets",1);surface.SetPropertyBlock(block);}
        void Update(){if(!GameSession.Instance||piece.activeSelf)return;var r=GetComponent<Renderer>();if(r){block??=new MaterialPropertyBlock();// Empty sockets keep a faint tint of their intended colour so the picture is readable before it is filled.
                block.SetColor("_BaseColor",GameSession.Instance.progress.Item(glassId).stage==ItemStage.Sorted?Color.Lerp(new Color(.15f,.16f,.23f),color,.4f+.12f*Mathf.Sin(Time.time*3)):Color.Lerp(new Color(.15f,.16f,.23f),color,.22f));r.SetPropertyBlock(block);}}
    }
}
