using System.Linq;
using UnityEngine;
namespace SeaGlass {
    public class TruckCompartment : Interactable
    {
        public UtilityTruck truck; public bool glass,skip;
        GameSession S=>GameSession.Instance;
        bool Bucketed=>S.Count(ItemStage.Bucket,glass)>0;
        public override string Prompt=>skip?"E · skip drive (after first journey)":
            truck.AtStudio&&!Bucketed?(glass?"Glass crate · the wash basin takes it in":"E · take rubbish out to carry to the press ("+S.Count(ItemStage.Truck,false)+")"):
            "E · load "+(glass?"GLASS":"RUBBISH")+" compartment";
        public override void Interact()
        {
            if(skip){truck.SkipDrive();return;}
            if(truck.AtStudio&&!Bucketed){if(glass)S.Notify("Your glass waits here. E at the wash basin brings it in.");else truck.Unload();return;}
            truck.Load(glass);
        }
    }
}

