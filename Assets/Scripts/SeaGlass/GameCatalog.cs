using System;
using UnityEngine;

namespace SeaGlass
{
    public enum GlassColor { Aqua, Emerald, Cobalt, Amber, Pearl, Violet }
    public enum ItemStage { Buried, Shore, Bucket, Truck, Studio, Clean, Sorted, Placed, Recycled }
    public enum PlayerMode { Walking, Driving, Workstation }
    [Serializable] public class GlassDefinition { public string id; public GlassColor color; public int shape; public int section; public int panel; public Vector2 socket; public float depthOffset; public Mesh mesh;
        // Rounded, tumbled display mesh with the same outline as the authored polygon (mesh stays the gameplay/test source).
        public Mesh visualMesh; public Mesh VisualMesh=>visualMesh?visualMesh:mesh;
        // Round 12: one shared family of tumbled pebbles for everything held, sorted, found or carried; the mosaic
        // keeps VisualMesh so installed pieces still fit their sockets.
        public Mesh pebbleMesh; public Mesh HeldMesh=>pebbleMesh?pebbleMesh:VisualMesh; }
    [Serializable] public class UpgradeDefinition { public string id; public string title; public string description; public int level; public int cost; }
    [CreateAssetMenu(menuName="Sea Glass/Game Catalog")]
    public class GameCatalog : ScriptableObject
    {
        public GlassDefinition[] glass;
        public UpgradeDefinition[] upgrades;
        public string[] panelNames = { "Ocean Wave", "Lighthouse", "Coastal Sunset" };
        public static readonly Color[] Palette = { new Color(.16f,.85f,.79f), new Color(.16f,.66f,.42f), new Color(.17f,.34f,.85f), new Color(1,.58f,.16f), new Color(.92f,.94f,.84f), new Color(.65f,.35f,.83f) };
    }
    [Serializable] public class ItemRecord { public string id; public ItemStage stage; public float grime = 1; }
    [Serializable] public class Progress
    {
        public int version=1; public int xp; public int caps; public int tutorial; public bool skipDrive;
        public bool returnToStudio, tutorialDismissed;
        public bool[] completed = new bool[3];
        public System.Collections.Generic.List<string> upgrades = new();
        public System.Collections.Generic.List<ItemRecord> items = new();
        public int Level => xp >= 1100 ? 4 : xp >= 550 ? 3 : xp >= 180 ? 2 : 1;
        public int BucketCapacity => upgrades.Contains("bucket") ? 14 : 7;
        public int TruckCapacity => upgrades.Contains("truck") ? 40 : 22;
        public bool SectionOpen(int section) => section == 0 || (section == 1 ? Level >= 2 : Level >= 3);
        public ItemRecord Item(string id) { var r=items.Find(i=>i.id==id); if(r==null) {r=new ItemRecord{id=id,stage=ItemStage.Buried};items.Add(r);} return r; }
    }
}
