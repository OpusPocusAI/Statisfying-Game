using System.Collections;
using System.Linq;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace SeaGlass.Tests
{
    public class FullJourneyTests
    {
        static IEnumerator Brake(GameSession session,Keyboard keyboard)
        {
            float deadline=Time.time+5;
            do {InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;}
            while(Mathf.Abs(session.truck.Speed)>.05f&&Time.time<deadline);
            Assert.LessOrEqual(Mathf.Abs(session.truck.Speed),.05f,"Brake must bring the truck to a measured stop.");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
        }
        [UnityTest] public IEnumerator FullJourneyAndRecovery()
        {
            yield return SceneManager.LoadSceneAsync("SeaGlassStudio");
            yield return null;
            var s=GameSession.Instance;Assert.NotNull(s);s.testMode=true;s.Begin(false);s.player.blocked=true;
            yield return null;
            Assert.AreEqual(0,GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Count(x=>x==null));
            // Drive through the actual update loop with Input System keyboard events.
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            var keyboard=Keyboard.current??InputSystem.AddDevice<Keyboard>();
            s.truck.Enter();Vector3 start=s.truck.transform.position;
            float until=Time.time+2;while(Time.time<until){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return null;}
            yield return Brake(s,keyboard);
            Assert.Greater(s.truck.transform.position.z,start.z+1,"Manual acceleration moves the truck; obstacle="+s.truck.lastObstacle+" speed="+s.truck.Speed+" driving="+s.truck.driving+" menu="+s.menu);
            until=Time.time+40;while(!s.progress.skipDrive&&Time.time<until){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return null;}
            Assert.IsTrue(s.progress.skipDrive,"Complete manual road journey unlocks skip");
            yield return Brake(s,keyboard);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());s.truck.Exit();Assert.AreEqual(PlayerMode.Walking,s.player.mode);
            s.truck.transform.position=new Vector3(80,-6,8);yield return null;Assert.Less(Mathf.Abs(s.truck.transform.position.x),30,"Out-of-world truck recovered");
            string result=SeaGlass.Editor.WorkflowChecks.FirstWave();StringAssert.StartsWith("PASS",result);yield return new WaitForSeconds(3);
            var all=Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            var stations=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None);var recycle=stations.First(x=>x.kind==StationKind.Recycle);var clean=stations.First(x=>x.kind==StationKind.Clean);var sort=stations.First(x=>x.kind==StationKind.Sort);
            var panels=Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None);
            Assert.IsFalse(panels.First(x=>x.index==0).Place("glass-00"),"Duplicate placement rejected");
            for(int section=1;section<3;section++)
            {
                Assert.IsTrue(s.progress.SectionOpen(section));s.truck.transform.position=new Vector3(15,.3f,8);
                foreach(var item in all.Where(x=>x.section==section).OrderBy(x=>x.id))
                {
                    if(s.BucketCount>=s.progress.BucketCapacity){s.truck.Load(true);s.truck.Load(false);}
                    if(s.CargoCount+s.BucketCount>=s.progress.TruckCapacity)
                    {s.truck.transform.position=new Vector3(15,.3f,150);s.truck.Load(true);s.truck.Load(false);recycle.Recycle();clean.Interact();if(clean.active)clean.Leave();yield return new WaitForSeconds(1.6f);s.truck.transform.position=new Vector3(15,.3f,8);}
                    item.Use(2);Assert.IsTrue(item.Collect(),item.id);
                }
                s.truck.Load(true);s.truck.Load(false);s.truck.transform.position=new Vector3(15,.3f,150);recycle.Recycle();yield return new WaitForSeconds(1.6f);
                clean.Interact();for(int i=0;i<10;i++)clean.Clean(10);clean.Leave();sort.Interact();
                for(int i=0;i<10;i++){var def=s.catalog.glass.First(x=>x.id==sort.selectedId);Assert.IsTrue(sort.Sort(def.color));}sort.Leave();
                var panel=panels.First(x=>x.index==section);foreach(var def in s.catalog.glass.Where(x=>x.panel==section))Assert.IsTrue(panel.Place(def.id));yield return new WaitForSeconds(3);
            }
            Assert.AreEqual(30,s.Count(ItemStage.Placed,true));Assert.IsTrue(s.progress.completed.All(x=>x));Assert.IsTrue(StudioUI.Instance.ending,"Final sequence completed");
            foreach(var u in s.catalog.upgrades)if(!s.progress.upgrades.Contains(u.id))Assert.IsTrue(s.Buy(u.id),u.id);
            Assert.AreEqual(5,s.progress.upgrades.Count);int caps=s.progress.caps;Assert.IsFalse(s.Buy("truck"));Assert.AreEqual(caps,s.progress.caps);
            s.saveFileName="seaglass-automated-test.json";s.testMode=false;s.progress.skipDrive=true;s.Save();Assert.IsTrue(File.Exists(s.SavePath));int xp=s.progress.xp;s.progress=new Progress();Assert.IsTrue(s.TryLoad());Assert.AreEqual(xp,s.progress.xp);Assert.AreEqual(30,s.Count(ItemStage.Placed,true));Assert.IsTrue(s.progress.skipDrive);s.Synchronize();
            string valid=File.ReadAllText(s.SavePath);File.WriteAllText(s.SavePath,"damaged save");Assert.IsFalse(s.TryLoad());File.WriteAllText(s.SavePath,valid);Assert.IsTrue(s.TryLoad());
            File.Delete(s.SavePath);if(File.Exists(s.SavePath+".bak"))File.Delete(s.SavePath+".bak");s.testMode=true;
            s.truck.SkipDrive();Assert.IsFalse(s.truck.AtStudio);s.truck.SkipDrive();Assert.IsTrue(s.truck.AtStudio);
            Assert.IsTrue(s.player.GetComponent<CharacterController>().enabled);LogAssert.NoUnexpectedReceived();
            yield return null;
        }
    }
}
