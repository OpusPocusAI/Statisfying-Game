using System.Linq;
using UnityEngine;
namespace SeaGlass {
    public class TruckCompartment : Interactable
    {
        public UtilityTruck truck; public bool glass,skip;
        public override string Prompt=>skip?"E · skip drive (after first journey)":"E · load "+(glass?"GLASS":"RUBBISH")+" compartment";
        public override void Interact(){if(skip)truck.SkipDrive();else truck.Load(glass);}
    }
}

