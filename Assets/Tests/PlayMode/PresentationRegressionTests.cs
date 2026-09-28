using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SeaGlass.Tests
{
    public class PresentationRegressionTests
    {
        GameSession session;
        string path;

        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("SeaGlassStudio");
            yield return null;
            session=GameSession.Instance;session.testMode=true;
            session.saveFileName="seaglass-regression-"+System.Guid.NewGuid().ToString("N")+".json";
            path=session.SavePath;session.Begin(false);session.player.blocked=true;
        }

        [UnityTearDown] public IEnumerator TearDown()
        {
            // A failed assertion must never leave the test session writing to the player's save.
            if(session)session.testMode=true;
            foreach(var suffix in new[]{"",".bak",".tmp"})if(path!=null&&File.Exists(path+suffix))File.Delete(path+suffix);
            yield return null;
        }

        [UnityTest] public IEnumerator CargoUpgradePreservesRailWidthAndDepth()
        {
            var rails=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Cargo rail").ToArray();
            Assert.IsNotEmpty(rails);var before=rails.Select(t=>t.localScale).ToArray();
            session.progress.upgrades.Add("truck");yield return null;
            for(int i=0;i<rails.Length;i++)
            {
                Assert.AreEqual(before[i].x,rails[i].localScale.x,.0001f);
                Assert.AreEqual(before[i].z,rails[i].localScale.z,.0001f);
                Assert.AreEqual(before[i].y*1.8f,rails[i].localScale.y,.0001f);
                Assert.Less(rails[i].localScale.x,.15f,"Cargo rails must not become solid metre-wide blocks.");
            }
        }

        [UnityTest] public IEnumerator CleaningChangesGrimeContinuouslyAndSortingPersistsInTray()
        {
            var clean=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Clean);
            var sort=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Sort);
            var def=session.catalog.glass[0];var record=session.progress.Item(def.id);record.stage=ItemStage.Studio;
            clean.Interact();clean.Clean(4.5f);
            var properties=new MaterialPropertyBlock();clean.glassRenderer.GetPropertyBlock(properties);
            Assert.AreEqual(.5f,properties.GetFloat("_Grime"),.01f);
            Assert.IsTrue(clean.glassRenderer.sharedMaterial.HasProperty("_Grime"));
            Assert.AreEqual(ItemStage.Studio,record.stage);
            clean.Clean(4.6f);clean.Leave();sort.Interact();
            Assert.IsFalse(sort.Sort((GlassColor)(((int)def.color+1)%6)));
            Assert.AreEqual(ItemStage.Clean,record.stage);
            Assert.IsTrue(sort.Sort(def.color));yield return null;
            var display=GameObject.Find("Sorted "+def.id);Assert.NotNull(display,"Sorted glass is physically visible in its tray.");
            sort.Leave();Assert.IsTrue(Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None).First(p=>p.index==def.panel).Place(def.id));
            yield return null;Assert.IsFalse(display.activeSelf,"Placed pieces must disappear from their tray.");
        }

        [UnityTest] public IEnumerator CleanRevealPausesAndCanBeLeftWithoutLosingTheNextPiece()
        {
            var clean=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(s=>s.kind==StationKind.Clean);
            var first=session.progress.Item("glass-03");var next=session.progress.Item("glass-04");
            first.stage=ItemStage.Studio;next.stage=ItemStage.Studio;
            clean.Interact();
            // Inventory order follows discovery; do not assume the lower catalogue ID is selected first.
            first=session.progress.Item(clean.selectedId);next=session.progress.Item(first.id=="glass-03"?"glass-04":"glass-03");
            clean.Clean(10);int xp=session.progress.xp;
            Assert.IsTrue(clean.RevealingCleanGlass);Assert.IsTrue(clean.workingGlass.gameObject.activeSelf);
            Assert.AreEqual(ItemStage.Clean,first.stage);Assert.AreEqual(ItemStage.Studio,next.stage);
            yield return new WaitForSeconds(.2f);session.SetPaused(true);var pose=clean.workingGlass.localPosition;
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.IsTrue(clean.RevealingCleanGlass);Assert.AreEqual(pose,clean.workingGlass.localPosition);
            session.SetPaused(false);session.player.blocked=true;
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(next.id,clean.selectedId);Assert.IsFalse(clean.RevealingCleanGlass);
            clean.Clean(10);Assert.IsTrue(clean.RevealingCleanGlass);clean.Leave();
            Assert.IsFalse(clean.RevealingCleanGlass);Assert.IsFalse(clean.workingGlass.gameObject.activeSelf);
            Assert.AreEqual(ItemStage.Clean,next.stage);Assert.AreEqual(xp+10,session.progress.xp);
            clean.Interact();Assert.IsFalse(clean.active,"A finished batch must not be washed twice.");
            Assert.AreEqual(xp+10,session.progress.xp);
        }

        [UnityTest] public IEnumerator MosaicRevealPausesAndMatchesLoadedLighting()
        {
            var panel=Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None).First(p=>p.index==0);
            session.player.Teleport(panel.transform.position+new Vector3(0,-2.45f,-3.5f),Quaternion.identity);
            session.player.SetLookRotation(Quaternion.Euler(12,0,0));var entry=session.player.view.transform.localRotation;
            foreach(var slot in panel.sockets)session.progress.Item(slot.glassId).stage=ItemStage.Placed;
            var last=panel.sockets.Last();session.progress.Item(last.glassId).stage=ItemStage.Sorted;
            Assert.IsTrue(panel.Place(last.glassId));Assert.IsTrue(panel.IsRevealing);Assert.AreEqual(0,panel.Illumination);
            yield return new WaitForSeconds(.8f);float during=panel.Illumination;
            Assert.Greater(during,0);Assert.Less(during,1);
            session.SetPaused(true);session.Synchronize();
            yield return new WaitForSecondsRealtime(.25f);Assert.AreEqual(during,panel.Illumination,"Refresh and pause must not jump to the saved final lighting.");
            session.SetPaused(false);yield return new WaitForSeconds(2.3f);
            Assert.IsFalse(panel.IsRevealing);Assert.AreEqual(1,panel.Illumination);Assert.AreEqual(3,panel.glow.intensity);
            Assert.Less(Quaternion.Angle(entry,session.player.view.transform.localRotation),.1f,"Emphasis must restore the controller's look state.");
            var block=new MaterialPropertyBlock();last.surface.GetPropertyBlock(block);var emission=block.GetColor("_EmissionColor");
            session.testMode=false;session.Save();session.testMode=true;session.progress=new Progress();
            Assert.IsTrue(session.TryLoad());session.Synchronize();yield return null;
            last.surface.GetPropertyBlock(block);Assert.AreEqual(emission,block.GetColor("_EmissionColor"));Assert.AreEqual(1,panel.Illumination);
            var accent=Object.FindObjectsByType<MosaicAccent>(FindObjectsSortMode.None).First(a=>a.panel==panel);
            Assert.AreEqual(1.4f,accent.lamp.intensity,.001f);
        }

        [UnityTest] public IEnumerator NewVisitCancelsMosaicSnapAndRevealPoses()
        {
            var panel=Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None).First(p=>p.index==0);
            var slot=panel.sockets[0];var home=slot.piece.transform.localPosition;
            session.progress.Item(slot.glassId).stage=ItemStage.Sorted;Assert.IsTrue(panel.Place(slot.glassId));
            Assert.AreNotEqual(home,slot.piece.transform.localPosition);session.Begin(false);
            Assert.AreEqual(home,slot.piece.transform.localPosition);Assert.AreEqual(Vector3.one,slot.piece.transform.localScale);
            foreach(var socket in panel.sockets)session.progress.Item(socket.glassId).stage=ItemStage.Placed;
            session.progress.Item(slot.glassId).stage=ItemStage.Sorted;Assert.IsTrue(panel.Place(slot.glassId));
            yield return new WaitForSeconds(.7f);session.Begin(false);yield return null;
            Assert.IsFalse(panel.IsRevealing);Assert.IsFalse(session.PresentationActive);Assert.AreEqual(0,panel.Illumination);
            Assert.IsFalse(slot.piece.activeSelf);Assert.AreEqual(home,slot.piece.transform.localPosition);
        }

        [UnityTest] public IEnumerator InvalidSaveIsPreservedAndStudioResumeIsSafe()
        {
            session.player.Teleport(new Vector3(0,.2f,153),Quaternion.identity);
            session.progress.xp=800;session.progress.tutorialDismissed=true;
            session.testMode=false;session.Save();session.testMode=true;
            var valid=File.ReadAllText(path);
            foreach(var bad in new[]{valid.Replace("\"xp\": 800","\"xp\": -1"),"{\"version\":1,\"completed\":[false,false,false],\"upgrades\":[],\"items\":[{\"id\":\"unknown-object\",\"stage\":3,\"grime\":1}]}"})
            {
                Assert.AreNotEqual(valid,bad);File.WriteAllText(path,bad);
                Assert.IsFalse(session.TryLoad());Assert.AreEqual(bad,File.ReadAllText(path));Assert.AreEqual(800,session.progress.xp);
            }
            File.WriteAllText(path,valid);session.Begin(true);session.player.blocked=true;
            Assert.Greater(session.player.transform.position.z,140);
            Assert.IsTrue(session.truck.AtStudio);Assert.IsTrue(session.progress.tutorialDismissed);
            Assert.IsTrue(session.player.GetComponent<CharacterController>().enabled);
            yield return null;
        }
    }
}
