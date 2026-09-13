using System.Linq;
using UnityEngine;
namespace SeaGlass {
    public class RestorationDecor : MonoBehaviour
    {
        public Light lamp;public Renderer surface;public Color restored=new Color(.96f,.75f,.4f);
        void Start(){if(GameSession.Instance.progress.completed.All(x=>x))Restore();}
        public void Restore(){if(lamp)lamp.intensity=2;if(surface){var b=new MaterialPropertyBlock();b.SetColor("_BaseColor",restored);surface.SetPropertyBlock(b);}}
    }
}


