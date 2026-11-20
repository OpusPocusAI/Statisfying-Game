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
#if UNITY_EDITOR
        InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
#endif
        [UnitySetUp] public IEnumerator SetUp()
        {
            previousBackground=InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            // Background-device handling alone does not route keyboard events away from Scene view.
            previousEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            yield return SceneManager.LoadSceneAsync("SeaGlassStudio");yield return null;
            keyboard=Keyboard.current??InputSystem.AddDevice<Keyboard>();mouse=Mouse.current??InputSystem.AddDevice<Mouse>();
            Neutral();session=GameSession.Instance;session.testMode=true;session.Begin(false);session.player.SetCursor(false);yield return null;
        }
        void Neutral(){if(keyboard!=null)InputSystem.QueueStateEvent(keyboard,new KeyboardState());if(mouse!=null)InputSystem.QueueStateEvent(mouse,new MouseState());}
        [UnityTearDown] public IEnumerator TearDown()
        {
            Neutral();if(session){session.testMode=true;session.player.blocked=true;}Time.timeScale=1;InputSystem.settings.backgroundBehavior=previousBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode=previousEditorInput;
#endif
            yield return null;
        }
        IEnumerator KeyPress(Key key)
        {InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;}
        void Aim(Vector3 target)
        {
            var direction=target-session.player.view.transform.position;
            session.player.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg,0);
            session.player.LookAt(target);Physics.SyncTransforms();
        }
        IEnumerator HoldMouse(float seconds,bool gameplayTime=false,float wipe=0)
        {
            float elapsed=0,deadline=Time.realtimeSinceStartup+seconds+10;
            // Digging advances in game time, which Unity caps during slow Editor frames.
            // Pause checks deliberately use unscaled time; a real-time deadline catches stalls.
            while(elapsed<seconds&&Time.realtimeSinceStartup<deadline)
            {var state=new MouseState().WithButton(MouseButton.Left);state.delta=new Vector2(Mathf.Sin(elapsed*12)*wipe,0);InputSystem.QueueStateEvent(mouse,state);yield return null;elapsed+=gameplayTime?Time.deltaTime:Time.unscaledDeltaTime;}
            InputSystem.QueueStateEvent(mouse,new MouseState());yield return null;
            Assert.GreaterOrEqual(elapsed,seconds,"The input hold timed out before the requested duration elapsed.");
        }
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
            yield return KeyPress(Key.Digit1);yield return HoldMouse(1.9f,true);Assert.AreEqual(ItemStage.Shore,item.Record.stage);
            yield return KeyPress(Key.E);Assert.AreEqual(ItemStage.Bucket,item.Record.stage);Assert.AreEqual(1,session.BucketCount);
        }
        [UnityTest] public IEnumerator GatheringPosePausesAndCarriedFillTracksLoading()
        {
            var item=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).First(i=>i.id=="glass-03");
            var equipment=Object.FindFirstObjectByType<EquipmentVisuals>();var tool=session.player.tool;var home=tool.localPosition;
            session.player.Teleport(item.transform.position+new Vector3(0,.25f,-1.8f),Quaternion.identity);Aim(item.transform.position+Vector3.up*.15f);yield return null;
            yield return HoldMouse(.23f);Assert.Greater(Vector3.Distance(home,tool.localPosition),.005f,"A valid dig should move the visible tool.");
            session.SetPaused(true);var paused=tool.localPosition;var rotation=tool.localRotation;float dig=item.dig;
            yield return HoldMouse(.25f);Assert.AreEqual(paused,tool.localPosition);Assert.Less(Quaternion.Angle(rotation,tool.localRotation),.01f);Assert.AreEqual(dig,item.dig);
            session.SetPaused(false);yield return HoldMouse(1.9f,true);yield return new WaitForSeconds(.25f);
            Assert.AreEqual(ItemStage.Shore,item.Record.stage);Assert.Less(Vector3.Distance(home,tool.localPosition),.001f,"The tool should settle after the find is revealed.");
            yield return KeyPress(Key.Digit2);Assert.IsTrue(equipment.bucket.activeSelf);Assert.IsFalse(equipment.bucketFill.gameObject.activeSelf);
            yield return KeyPress(Key.E);Assert.AreEqual(1,session.BucketCount);Assert.IsTrue(equipment.bucketFill.gameObject.activeSelf);
            session.progress.upgrades.Add("bucket");session.Synchronize();yield return null;
            Assert.IsFalse(equipment.bucket.activeSelf);Assert.IsTrue(equipment.sack.activeSelf);Assert.IsTrue(equipment.sackFill.gameObject.activeSelf);
            Assert.IsTrue(session.truck.Load(true));yield return null;Assert.AreEqual(0,session.BucketCount);
            Assert.IsFalse(equipment.bucketFill.gameObject.activeSelf);Assert.IsFalse(equipment.sackFill.gameObject.activeSelf);
            yield return KeyPress(Key.Digit1);session.progress.upgrades.Add("scoop");session.Synchronize();yield return null;
            Assert.IsTrue(equipment.scoop.activeSelf);Assert.IsFalse(equipment.GetComponent<GatheringPresentation>().toyBlade.activeSelf);
        }
        [UnityTest] public IEnumerator WashingPausesAndRestoresTheEntryView()
        {
            var clean=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Clean);
            var record=session.progress.Item("glass-03");record.stage=ItemStage.Studio;
            yield return Enter(clean);yield return KeyPress(Key.E);
            session.player.SetLookRotation(Quaternion.Euler(-25,0,0));var before=session.player.view.transform.localRotation;
            clean.Interact();yield return HoldMouse(.3f,false,40);Assert.Less(record.grime,1);
            yield return KeyPress(Key.Escape);Assert.IsTrue(session.paused);float pausedGrime=record.grime;
            yield return HoldMouse(.35f,false,40);Assert.AreEqual(pausedGrime,record.grime);
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
            // The reveal runs ~2.75 s of frame-stepped loops; poll so slow editor frames don't overshoot a fixed wait.
            for(float t=0;t<6&&!StudioUI.Instance.ending;t+=Time.deltaTime)yield return null;
            Assert.IsTrue(StudioUI.Instance.ending);
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
        [UnityTest] public IEnumerator LoopStripFollowsTheHaul()
        {
            Assert.AreEqual(0,TutorialGuidance.Step(session,null,out var side),"A fresh visit starts on Dig.");Assert.IsNull(side);
            var glass=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).First(x=>x.id=="glass-03");
            glass.Use(2);Assert.IsTrue(glass.Collect());
            Assert.AreEqual(1,TutorialGuidance.Step(session,null,out _),"A find in the bucket lights Bucket.");
            Assert.IsTrue(session.truck.Load(true));
            Assert.AreEqual(2,TutorialGuidance.Step(session,null,out _),"A loaded truck lights Truck.");
            session.truck.transform.position=new Vector3(15,.3f,150);session.player.Teleport(new Vector3(0,.2f,153),Quaternion.identity);
            Assert.AreEqual(3,TutorialGuidance.Step(session,null,out _),"Glass in the truck at the studio lights Wash.");
            session.progress.Item("glass-03").stage=ItemStage.Clean;session.truck.RefreshCargo();
            Assert.AreEqual(4,TutorialGuidance.Step(session,null,out _));
            session.progress.Item("glass-03").stage=ItemStage.Sorted;
            Assert.AreEqual(5,TutorialGuidance.Step(session,null,out _));
            var rubbish=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).First(x=>x.section==0&&!x.glass);
            session.progress.Item(rubbish.id).stage=ItemStage.Bucket;
            Assert.AreEqual(-1,TutorialGuidance.Step(session,null,out side));StringAssert.Contains("recycle",side);
            yield return null;
        }
        [UnityTest] public IEnumerator BumpyRoadSpillsCargoThatCanBePickedUpAgain()
        {
            var hz=RoadHazards.Instance;Assert.NotNull(hz);Assert.GreaterOrEqual(hz.hazards.Count,7);
            foreach(var i in Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).Where(x=>x.section==0).Take(4).ToArray()){i.Use(10);Assert.IsTrue(i.Collect());}
            session.truck.Load(true);session.truck.Load(false);Assert.AreEqual(4,session.CargoCount);
            var speed=typeof(UtilityTruck).GetField("speed",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var first=hz.hazards.First(h=>h.ripples);var second=hz.hazards.Last(h=>h.ripples);
            var cc=session.truck.GetComponent<CharacterController>();
            void Place(float z){cc.enabled=false;session.truck.transform.SetPositionAndRotation(new Vector3(15,.3f,z),Quaternion.identity);cc.enabled=true;}
            Place(first.at.z-6);session.truck.Enter();yield return null;
            float until=Time.time+5;while(session.truck.transform.position.z<first.at.z+first.length&&Time.time<until){speed.SetValue(session.truck,4f);yield return null;}
            Assert.Greater(session.truck.transform.position.z,first.at.z+first.length-.5f);
            Assert.AreEqual(4,session.CargoCount,"Taken slowly, the sand ripples cost nothing.");
            Place(second.at.z-6);yield return null;
            until=Time.time+3;while(session.CargoCount==4&&Time.time<until){speed.SetValue(session.truck,12f);yield return null;}
            Assert.Less(session.CargoCount,4,"Taken fast, the ripples throw cargo out.");
            speed.SetValue(session.truck,0f);session.truck.Exit();yield return new WaitForSeconds(1.2f);
            var fallen=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).Where(i=>i.Record.spilled).ToArray();
            Assert.AreEqual(4-session.CargoCount,fallen.Length);
            foreach(var f in fallen)Assert.Less(Mathf.Abs(f.transform.position.x-15),5,"Fallen cargo lies on the road.");
            StringAssert.Contains("bounced off",TutorialGuidance.Get(session));
            Assert.IsTrue(TutorialGuidance.Target(session,null,out _,out var label));Assert.AreEqual("Fallen cargo",label);
            int xp=session.progress.xp;foreach(var f in fallen)Assert.IsTrue(f.Collect());
            Assert.AreEqual(xp,session.progress.xp,"Picking fallen cargo up again earns nothing twice.");
            Assert.AreEqual(0,RoadHazards.Fallen(session));
        }
        [UnityTest] public IEnumerator RubbishOnlyHaulDoesNotRequestMissingGlass()
        {
            var rubbish=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).First(x=>x.section==0&&!x.glass);
            rubbish.Use(2);Assert.IsTrue(rubbish.Collect());Assert.IsTrue(session.truck.Load(false));
            session.truck.transform.position=new Vector3(15,.3f,150);session.player.Teleport(new Vector3(0,.2f,153),Quaternion.identity);
            StringAssert.Contains("recycling press",TutorialGuidance.Get(session));
            var press=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Recycle);
            Assert.IsFalse(press.Recycle(),"The press only takes rubbish you carry to it.");
            Assert.IsTrue(session.truck.Unload());StringAssert.Contains("recycling press",TutorialGuidance.Get(session));
            Assert.IsTrue(press.Recycle());
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
        [UnityTest] public IEnumerator EveryMosaicSocketCanBeTargetedAndPlacedPrecisely()
        {
            foreach(var socket in Object.FindObjectsByType<MosaicSocket>(FindObjectsSortMode.None).OrderBy(s=>s.glassId))
            {
                session.progress.Item(socket.glassId).stage=ItemStage.Sorted;
                // Round 46: start on the studio floor (y .09), not 21 cm above it. The aim was set before the player fell that gap,
                // and on a slow frame the drop moved the aim onto the socket below (glass-14 selected glass-13).
                session.player.Teleport(new Vector3(socket.transform.position.x,.09f,173.2f),Quaternion.identity);
                Aim(socket.transform.position);yield return null;
                Assert.AreSame(socket,session.player.focus,"Aiming at "+socket.glassId+" must not select a neighbouring outline or the backing.");
                yield return KeyPress(Key.E);Assert.AreEqual(ItemStage.Placed,session.progress.Item(socket.glassId).stage);
                if(session.progress.completed[socket.panel.index])yield return new WaitForSeconds(3);
            }
            Assert.AreEqual(30,session.Count(ItemStage.Placed,true));Assert.IsTrue(StudioUI.Instance.ending);
        }
        [UnityTest] public IEnumerator RearCratesLoadSeparatelyThroughTheInteractionRay()
        {
            session.progress.Item("glass-03").stage=ItemStage.Bucket;session.progress.Item("rubbish-00").stage=ItemStage.Bucket;
            foreach(var crate in Object.FindObjectsByType<TruckCompartment>(FindObjectsSortMode.None).Where(c=>!c.skip).OrderByDescending(c=>c.glass))
            {
                session.player.Teleport(new Vector3(crate.transform.position.x,.3f,3.8f),Quaternion.identity);
                Aim(crate.transform.position);yield return null;Assert.AreSame(crate,session.player.focus);
                yield return KeyPress(Key.E);
                Assert.AreEqual(1,session.Count(ItemStage.Truck,crate.glass));
                if(crate.glass)Assert.AreEqual(ItemStage.Bucket,session.progress.Item("rubbish-00").stage);
            }
            Assert.AreEqual(0,session.BucketCount);Assert.AreEqual(2,session.CargoCount);
        }
    }
}
