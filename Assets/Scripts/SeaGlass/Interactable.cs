using UnityEngine;
namespace SeaGlass
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public abstract void Interact();
        public virtual void Use(float delta) {}
    }
}
