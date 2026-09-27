using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace SeaGlass.Tests
{
    public class ProgressTests
    {
        [Test] public void NewGameHasDeliberatelyLimitedEquipment(){var p=new Progress();Assert.AreEqual(7,p.BucketCapacity);Assert.AreEqual(22,p.TruckCapacity);Assert.AreEqual(1,p.Level);Assert.IsFalse(p.SectionOpen(1));Assert.IsFalse(p.SectionOpen(2));}
        [Test] public void StableIdsReturnSameRecord(){var p=new Progress();p.Item("glass-00").stage=ItemStage.Sorted;Assert.AreEqual(ItemStage.Sorted,p.Item("glass-00").stage);Assert.AreEqual(1,p.items.Count);}
        [Test] public void EveryProgressStageRoundTripsThroughJson(){var p=new Progress{xp=720,caps=43,skipDrive=true,tutorial=9};p.completed[0]=true;p.upgrades.Add("truck");foreach(ItemStage stage in System.Enum.GetValues(typeof(ItemStage))){var i=p.Item("item-"+stage);i.stage=stage;i.grime=.37f;}var copy=JsonUtility.FromJson<Progress>(JsonUtility.ToJson(p));Assert.AreEqual(p.items.Count,copy.items.Count);Assert.IsTrue(copy.completed[0]);Assert.IsTrue(copy.skipDrive);Assert.AreEqual(40,copy.TruckCapacity);Assert.AreEqual(720,copy.xp);Assert.AreEqual(.37f,copy.items[3].grime,.001f);}
        [Test] public void SectionsUnlockMonotonically(){var p=new Progress{xp=179};Assert.IsFalse(p.SectionOpen(1));p.xp=180;Assert.IsTrue(p.SectionOpen(1));Assert.IsFalse(p.SectionOpen(2));p.xp=550;Assert.IsTrue(p.SectionOpen(2));}
        [Test] public void CatalogHasThirtyUniqueAuthoredPiecesAndAffordableEquipment(){var c=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");Assert.NotNull(c);Assert.AreEqual(30,c.glass.Length);Assert.AreEqual(30,c.glass.Select(g=>g.id).Distinct().Count());Assert.AreEqual(6,c.glass.Select(g=>g.color).Distinct().Count());foreach(var d in c.glass)Assert.NotNull(d.mesh);for(int i=0;i<3;i++)Assert.AreEqual(10,c.glass.Count(g=>g.panel==i));Assert.AreEqual(5,c.upgrades.Length);Assert.LessOrEqual(c.upgrades.Sum(u=>u.cost),72*5+120);}
    }
}
