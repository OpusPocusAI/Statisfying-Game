using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SeaGlass.Tests
{
    public class InteractionTests
    {
        GameSession session;Keyboard keyboard;Mouse mouse;
        InputSettings.BackgroundBehavior previousBackground;
        [UnitySetUp] public IEnumerator SetUp()
        {
            previousBackground=InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            yield return SceneManager.LoadSceneAsync("SeaGlassStudio");yield return null;
            keyboard=Keyboard.current??InputSystem.AddDevice<Keyboard>();mouse=Mouse.current??InputSystem.AddDevice<Mouse>();
            Neutral();session=GameSession.Instance;session.testMode=true;session.Begin(false);session.player.SetCursor(false);yield return null;
        }
        void Neutral(){if(keyboard!=null)InputSystem.QueueStateEvent(keyboard,new KeyboardState());if(mouse!=null)InputSystem.QueueStateEvent(mouse,new MouseState());}
        [UnityTearDown] public IEnumerator TearDown()
        {Neutral();if(session){session.testMode=true;session.player.blocked=true;}Time.timeScale=1;InputSystem.settings.backgroundBehavior=previousBackground;yield return null;}
        IEnumerator KeyPress(Key key)
        {InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;}
        void Aim(Vector3 target)
        {
            var direction=target-session.player.view.transform.position;
            session.player.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg,0);
            session.player.LookAt(target);Physics.SyncTransforms();
        }
        IEnumerator HoldMouse(float seconds)
        {float until=Time.unscaledTime+seconds;while(Time.unscaledTime<until){InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));yield return null;}InputSystem.QueueStateEvent(mouse,new MouseState());yield return null;}
        IEnumerator Enter(StudioStation station)
        {
            session.player.Teleport(station.transform.position+new Vector3(0,.2f,-2.8f),Quaternion.identity);
            Aim(station.transform.position+Vector3.up*1.03f);yield return null;
            Assert.AreSame(station,session.player.focus,"The physical bench must be reachable through the interaction ray.");
            yield return KeyPress(Key.E);Assert.IsTrue(station.active);Assert.AreEqual(PlayerMode.Workstation,session.player.mode);
        }
        [UnityTest] public IEnumerator DiggingRequiresShovelAndCollectionUsesInteractionRay()
        {
            var item=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).First(i=>i.id=="glass-03");
            session.player.Teleport(item.transform.position+new Vector3(0,.25f,-1.8f),Quaternion.identity);
            Aim(item.transform.position+Vector3.up*.15f);yield return null;
            Assert.AreSame(item,session.player.focus);yield return KeyPress(Key.Digit2);yield return HoldMouse(.25f);
            Assert.AreEqual(0,item.dig,"The carrying container must not dig.");
            StringAssert.Contains("equip shovel",item.Prompt,"A selected bucket must not show an unusable dig instruction.");
            yield return KeyPress(Key.Digit1);yield return HoldMouse(1.9f);Assert.AreEqual(ItemStage.Shore,item.Record.stage);
            yield return KeyPress(Key.E);Assert.AreEqual(ItemStage.Bucket,item.Record.stage);Assert.AreEqual(1,session.BucketCount);
        }
        [UnityTest] public IEnumerator WashingPausesAndRestoresTheEntryView()
        {
            var clean=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Clean);
            var record=session.progress.Item("glass-03");record.stage=ItemStage.Studio;
            yield return Enter(clean);yield return KeyPress(Key.E);
            session.player.SetLookRotation(Quaternion.Euler(-25,0,0));var before=session.player.view.transform.localRotation;
            clean.Interact();yield return HoldMouse(.3f);Assert.Less(record.grime,1);
            yield return KeyPress(Key.Escape);Assert.IsTrue(session.paused);float pausedGrime=record.grime;
            yield return HoldMouse(.35f);Assert.AreEqual(pausedGrime,record.grime);
            yield return KeyPress(Key.Escape);Assert.IsFalse(session.paused);
            yield return KeyPress(Key.E);Assert.IsFalse(clean.active);Assert.AreEqual(PlayerMode.Walking,session.player.mode);
            yield return null;Assert.Less(Quaternion.Angle(before,session.player.view.transform.localRotation),.1f,"Station exit must restore stored look angles, not only the camera transform.");
        }
        [UnityTest] public IEnumerator EveryColourTrayCanBeSortedWithMouseAim()
        {
            var sort=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Sort);
            for(int assistance=0;assistance<2;assistance++)
            {
                if(assistance==1)session.progress.upgrades.Add("magnet");
                foreach(var tray in Object.FindObjectsByType<SortingTray>(FindObjectsSortMode.None))
                {
                    var def=session.catalog.glass.First(d=>d.color==tray.color);var record=session.progress.Item(def.id);record.stage=ItemStage.Clean;record.grime=0;
                    yield return Enter(sort);Aim(tray.transform.position);yield return null;
                    yield return HoldMouse(.04f);Assert.AreEqual(ItemStage.Sorted,record.stage,"Mouse target failed for "+tray.color+" with assistance "+assistance);
                    yield return KeyPress(Key.E);
                }
            }
        }
        [UnityTest] public IEnumerator DriverAndNewVisitResetStoredLookAndRecoveryPoint()
        {
            session.player.SetLookRotation(Quaternion.Euler(55,0,0));session.truck.Enter();yield return null;
            Assert.Less(Quaternion.Angle(Quaternion.identity,session.player.view.transform.localRotation),.1f);
            session.truck.Exit();session.player.safePosition=new Vector3(0,.2f,153);session.player.SetLookRotation(Quaternion.Euler(-35,0,0));
            session.Begin(false);session.player.SetCursor(false);yield return null;
            Assert.Less(Quaternion.Angle(Quaternion.identity,session.player.view.transform.localRotation),.1f);
            Assert.Less(session.player.safePosition.z,10,"A new visit must not inherit the previous studio recovery point.");
        }
        MosaicPanel CompletePanel(int index)
        {
            var panel=Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None).First(p=>p.index==index);
            foreach(var socket in panel.sockets)session.progress.Item(socket.glassId).stage=ItemStage.Placed;
            var last=panel.sockets.Last().glassId;session.progress.Item(last).stage=ItemStage.Sorted;
            Assert.IsTrue(panel.Place(last));return panel;
        }
        [UnityTest] public IEnumerator PauseAndMenuPreserveTheMosaicPresentationLock()
        {
            CompletePanel(0);Assert.IsTrue(session.player.blocked);
            yield return KeyPress(Key.Escape);Assert.IsTrue(session.paused);
            yield return KeyPress(Key.Escape);Assert.IsFalse(session.paused);
            Assert.IsTrue(session.player.blocked,"Resuming must not unlock movement during the mosaic reveal.");
            session.ReturnToMenu();yield return new WaitForSeconds(3);
            Assert.IsTrue(session.menu);Assert.IsTrue(session.player.blocked,"Finishing a reveal must not unlock the main menu.");
            session.Begin(false);yield return null;
            Assert.IsFalse(session.ControlsBlocked,"A new visit must clear all old presentation locks.");
        }
        [UnityTest] public IEnumerator CompletionScreenKeepsControlUntilDismissed()
        {
            session.progress.completed[0]=true;session.progress.completed[1]=true;CompletePanel(2);
            yield return new WaitForSeconds(3);Assert.IsTrue(StudioUI.Instance.ending);
            yield return KeyPress(Key.Escape);yield return KeyPress(Key.Escape);
            Assert.IsFalse(session.paused);Assert.IsTrue(session.player.blocked,"The completion screen still owns the controls after resuming.");
            Assert.IsTrue(Cursor.visible,"The completion screen's button must remain usable.");
            var before=session.player.transform.position;
            yield return KeyPress(Key.W);Assert.Less(Vector3.Distance(before,session.player.transform.position),.001f);
            session.truck.transform.position=new Vector3(15,.3f,25);yield return KeyPress(Key.R);
            Assert.AreEqual(25,session.truck.transform.position.z,"Gameplay shortcuts must not run behind an open screen.");
            StudioUI.Instance.CloseEnding();yield return null;Assert.IsFalse(session.player.blocked);
        }
        [UnityTest] public IEnumerator SkipDriveRecoveryStaysAtTheDestination()
        {
            session.progress.skipDrive=true;
            for(int trip=0;trip<2;trip++)
            {
                session.truck.SkipDrive();var destination=session.player.transform.position;
                Assert.AreEqual(destination,session.player.safePosition);
                session.player.Teleport(destination+Vector3.down*10,Quaternion.identity);yield return null;yield return null;
                Assert.Less(Vector3.Distance(destination,session.player.transform.position),.3f,"Recovery must stay at the destination after either direction of Skip Drive.");
            }
        }
        [UnityTest] public IEnumerator MixedCargoGuidanceWaitsForBothCrates()
        {
            var items=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None);
            foreach(var item in new[]{items.First(x=>x.id=="glass-03"),items.First(x=>x.section==0&&!x.glass)})
            {item.Use(2);Assert.IsTrue(item.Collect());}
            Assert.IsTrue(session.truck.Load(true));Assert.AreEqual(1,session.BucketCount);
            StringAssert.Contains("rubbish crate",TutorialGuidance.Get(session));
            Assert.IsTrue(session.truck.Load(false));
            StringAssert.Contains("Drive your haul",TutorialGuidance.Get(session));
            yield return KeyPress(Key.H);Assert.IsTrue(session.progress.tutorialDismissed);
            yield return KeyPress(Key.H);Assert.IsFalse(session.progress.tutorialDismissed);
        }
        [UnityTest] public IEnumerator RubbishOnlyHaulDoesNotRequestMissingGlass()
        {
            var rubbish=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).First(x=>x.section==0&&!x.glass);
            rubbish.Use(2);Assert.IsTrue(rubbish.Collect());Assert.IsTrue(session.truck.Load(false));
            session.truck.transform.position=new Vector3(15,.3f,150);session.player.Teleport(new Vector3(0,.2f,153),Quaternion.identity);
            StringAssert.Contains("recycling press",TutorialGuidance.Get(session));
            Assert.IsTrue(Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Recycle).Recycle());
            StringAssert.Contains("Return to the shore",TutorialGuidance.Get(session));yield return null;
        }
        [UnityTest] public IEnumerator GuidanceFollowsProcessingAfterAnEarlyUpgrade()
        {
            session.progress.tutorial=11;session.progress.upgrades.Add("bucket");
            session.player.Teleport(new Vector3(0,.2f,153),Quaternion.identity);
            var glass=session.catalog.glass.First(g=>g.panel==2);var record=session.progress.Item(glass.id);record.stage=ItemStage.Studio;
            StringAssert.Contains("basin",TutorialGuidance.Get(session));
            var stations=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None);
            var clean=stations.First(s=>s.kind==StationKind.Clean);clean.Interact();clean.Clean(10);
            StringAssert.Contains("E to leave",TutorialGuidance.Get(session,clean));clean.Leave();
            StringAssert.Contains("sorting table",TutorialGuidance.Get(session));
            var sort=stations.First(s=>s.kind==StationKind.Sort);sort.Interact();
            StringAssert.Contains(glass.color+" tray",TutorialGuidance.Get(session,sort));Assert.IsTrue(sort.Sort(glass.color));
            StringAssert.Contains("E to leave",TutorialGuidance.Get(session,sort));sort.Leave();
            StringAssert.Contains("Coastal Sunset",TutorialGuidance.Get(session));yield return null;
        }
    }
}
