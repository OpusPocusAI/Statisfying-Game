using System;
using System.Linq;
using UnityEngine;
namespace SeaGlass.Editor
{
    public static class WorkflowChecks
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception("CHECK FAILED: "+message);}
        public static string FirstWave()
        {
            var s=GameSession.Instance;Check(s!=null&&Application.isPlaying,"Play Mode session exists");s.testMode=true;s.Begin(false);
            var all=UnityEngine.Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            Check(all.Length==102,"102 shoreline items");Check(all.Select(x=>x.id).Distinct().Count()==102,"Unique item IDs");
            var recycle=UnityEngine.Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(x=>x.kind==StationKind.Recycle);
            var clean=UnityEngine.Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(x=>x.kind==StationKind.Clean);
            var sort=UnityEngine.Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).First(x=>x.kind==StationKind.Sort);
            var panel=UnityEngine.Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None).First(x=>x.index==0);
            foreach(var item in all.Where(x=>x.section==0).OrderBy(x=>x.id))
            {
                if(s.BucketCount>=s.progress.BucketCapacity){s.truck.Load(true);s.truck.Load(false);}
                if(s.CargoCount+s.BucketCount>=s.progress.TruckCapacity)
                {
                    s.truck.transform.position=new Vector3(15,.3f,150);s.truck.Load(true);s.truck.Load(false);recycle.Recycle();
                    clean.Interact();if(clean.active)clean.Leave();s.truck.transform.position=new Vector3(15,.3f,8);
                }
                item.Use(2);Check(item.Record.stage==ItemStage.Shore,"Dig reveals "+item.id);Check(item.Collect(),"Collection succeeds "+item.id);
            }
            s.truck.Load(true);s.truck.Load(false);s.truck.transform.position=new Vector3(15,.3f,150);
            // Recycle press cooldown is respected by normal play; process remaining cargo after clearing its visual timer for this synchronous harness.
            typeof(StudioStation).GetField("processing",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(recycle,0f);recycle.Recycle();
            clean.Interact();Check(clean.active,"Cleaning station entered");
            // This synchronous fixture skips the reveal; timed presentation is covered in PlayMode.
            for(int i=0;i<10;i++){clean.Clean(10);clean.SelectNext();}clean.Leave();Check(s.Count(ItemStage.Clean,true)==10,"Ten pieces washed");
            sort.Interact();Check(sort.active,"Sorting entered");
            var first=s.catalog.glass.First(x=>x.id==sort.selectedId);Check(!sort.Sort((GlassColor)(((int)first.color+1)%6)),"Incorrect tray is forgiving");
            for(int i=0;i<10;i++){var d=s.catalog.glass.First(x=>x.id==sort.selectedId);Check(sort.Sort(d.color),"Correct sorting");}sort.Leave();
            Check(s.Count(ItemStage.Sorted,true)==10,"Ten pieces sorted");foreach(var d in s.catalog.glass.Where(x=>x.panel==0))Check(panel.Place(d.id),"Mosaic placement");
            Check(s.progress.completed[0],"Ocean Wave completed");Check(s.Count(ItemStage.Placed,true)==10,"Ten persistent placements");Check(s.progress.caps>=120,"Recycling currency awarded");
            int xp=s.progress.xp;Check(s.Buy("scoop"),"First gathering upgrade purchased");Check(s.progress.xp==xp,"XP never spent");
            return "PASS: 102 unique finds; dig / collect / bucket / separate truck cargo / recycle / clean / incorrect & correct sorting / ten Ocean Wave placements / completion reward / upgrade purchase. XP="+s.progress.xp+", caps="+s.progress.caps;
        }
    }
}
