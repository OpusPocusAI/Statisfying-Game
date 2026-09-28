using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace SeaGlass.Tests
{
    public class ProgressTests
    {
        [Test] public void AuthoredGlassFacesHaveOutwardWindingAndNoExcessTriangleArea()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");
            foreach(var definition in catalog.glass)
            {
                var vertices=definition.mesh.vertices;var indices=definition.mesh.triangles;int count=vertices.Length/2;
                float polygonArea=0,triangleArea=0;int frontTriangles=0;
                for(int i=0;i<count;i++){var a=vertices[i];var b=vertices[(i+1)%count];polygonArea+=a.x*b.y-b.x*a.y;}
                for(int i=0;i<indices.Length;i+=3)
                {
                    if(indices[i]>=count||indices[i+1]>=count||indices[i+2]>=count)continue;
                    var normal=Vector3.Cross(vertices[indices[i+1]]-vertices[indices[i]],vertices[indices[i+2]]-vertices[indices[i]]);
                    Assert.Less(normal.z,0,definition.id+" front face must point toward the player.");
                    triangleArea+=normal.magnitude*.5f;frontTriangles++;
                }
                Assert.AreEqual(count-2,frontTriangles,definition.id);
                Assert.AreEqual(Mathf.Abs(polygonArea)*.5f,triangleArea,.00001f,definition.id+" must not fill concave gaps with overlapping triangles.");
            }
        }
        [Test] public void NewGameHasDeliberatelyLimitedEquipment(){var p=new Progress();Assert.AreEqual(7,p.BucketCapacity);Assert.AreEqual(22,p.TruckCapacity);Assert.AreEqual(1,p.Level);Assert.IsFalse(p.SectionOpen(1));Assert.IsFalse(p.SectionOpen(2));}
        [Test] public void StableIdsReturnSameRecord(){var p=new Progress();p.Item("glass-00").stage=ItemStage.Sorted;Assert.AreEqual(ItemStage.Sorted,p.Item("glass-00").stage);Assert.AreEqual(1,p.items.Count);}
        [Test] public void EveryProgressStageRoundTripsThroughJson(){var p=new Progress{xp=720,caps=43,skipDrive=true,tutorial=9};p.completed[0]=true;p.upgrades.Add("truck");foreach(ItemStage stage in System.Enum.GetValues(typeof(ItemStage))){var i=p.Item("item-"+stage);i.stage=stage;i.grime=.37f;}var copy=JsonUtility.FromJson<Progress>(JsonUtility.ToJson(p));Assert.AreEqual(p.items.Count,copy.items.Count);Assert.IsTrue(copy.completed[0]);Assert.IsTrue(copy.skipDrive);Assert.AreEqual(40,copy.TruckCapacity);Assert.AreEqual(720,copy.xp);Assert.AreEqual(.37f,copy.items[3].grime,.001f);}
        [Test] public void SectionsUnlockMonotonically(){var p=new Progress{xp=179};Assert.IsFalse(p.SectionOpen(1));p.xp=180;Assert.IsTrue(p.SectionOpen(1));Assert.IsFalse(p.SectionOpen(2));p.xp=550;Assert.IsTrue(p.SectionOpen(2));}
        [Test] public void CatalogHasThirtyUniqueAuthoredPiecesAndAffordableEquipment(){var c=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/SeaGlassCatalog.asset");Assert.NotNull(c);Assert.AreEqual(30,c.glass.Length);Assert.AreEqual(30,c.glass.Select(g=>g.id).Distinct().Count());Assert.AreEqual(6,c.glass.Select(g=>g.color).Distinct().Count());foreach(var d in c.glass)Assert.NotNull(d.mesh);for(int i=0;i<3;i++)Assert.AreEqual(10,c.glass.Count(g=>g.panel==i));Assert.AreEqual(5,c.upgrades.Length);Assert.LessOrEqual(c.upgrades.Sum(u=>u.cost),72*5+120);}
    }
}
