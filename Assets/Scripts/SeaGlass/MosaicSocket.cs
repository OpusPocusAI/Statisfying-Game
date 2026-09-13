using System.Linq;
using UnityEngine;
namespace SeaGlass {
    public class MosaicSocket : Interactable
    {
        public string glassId;public MosaicPanel panel;public GameObject piece;public Renderer surface;public Color color;
        MaterialPropertyBlock block;
        public override string Prompt=>GameSession.Instance.progress.Item(glassId).stage==ItemStage.Placed?"A little piece of the coast":"E · place "+colorName+" glass";
        string colorName=>GameSession.Instance.catalog.glass.First(x=>x.id==glassId).color.ToString();
        public override void Interact(){if(!panel.Place(glassId))panel.Interact();}
        public void Refresh(){var stage=GameSession.Instance.progress.Item(glassId).stage;piece.SetActive(stage==ItemStage.Placed);SetGlow(GameSession.Instance.progress.completed[panel.index]?.9f:.12f);}
        public void SetGlow(float value){if(!surface)return;block??=new MaterialPropertyBlock();surface.GetPropertyBlock(block);block.SetColor("_EmissionColor",color*value);surface.SetPropertyBlock(block);}
        void Update(){if(!GameSession.Instance||piece.activeSelf)return;var r=GetComponent<Renderer>();if(r){block??=new MaterialPropertyBlock();block.SetColor("_BaseColor",GameSession.Instance.progress.Item(glassId).stage==ItemStage.Sorted?Color.Lerp(new Color(.15f,.16f,.23f),color,.4f+.12f*Mathf.Sin(Time.time*3)):new Color(.15f,.16f,.23f));r.SetPropertyBlock(block);}}
    }
}
