using System.Linq;
using UnityEngine;

namespace SeaGlass
{
    // Saved tutorial milestones record what was learned; cargo determines what is useful now.
    public static class TutorialGuidance
    {
        public static string Get(GameSession session,StudioStation station=null)
        {
            var p=session.progress;
            if(p.completed.All(x=>x))return "The studio is restored. Stay awhile, or collect the remaining shoreline rubbish.";
            if(station&&station.active)
            {
                if(station.kind==StationKind.Clean)
                    return station.selectedId!=null?"Hold LMB to wash the glass. E returns you to the studio.":"This batch is washed. E to leave, then visit the sorting trays.";
                if(station.kind==StationKind.Sort)
                {
                    var piece=session.catalog.glass.FirstOrDefault(x=>x.id==station.selectedId);
                    return piece!=null?"Aim at the "+piece.color+" tray and click LMB. E to leave.":"This batch is sorted. E to leave, then fit the glass into its mosaic.";
                }
            }
            bool atStudio=session.player.transform.position.z>140;
            int bucket=session.BucketCount,cargo=session.CargoCount;
            if(session.player.mode==PlayerMode.Driving)
                return session.truck.AtStudio?"Studio reached. Space to brake, then E to leave the cab.":"W/S to drive, A/D to steer, Space to brake. Follow the coastal road.";
            if(bucket>0&&(atStudio||cargo>0||bucket>=p.BucketCapacity))
            {
                bool glass=session.Count(ItemStage.Bucket,true)>0,rubbish=session.Count(ItemStage.Bucket,false)>0;
                return "E at the truck's rear "+(glass&&rubbish?"glass and rubbish crates":glass?"glass crate":"rubbish crate")+" to load the remaining finds.";
            }
            if(atStudio)
            {
                if(session.truck.AtStudio&&session.Count(ItemStage.Truck,false)>0)return "E at the recycling press to turn the truck's rubbish into bottle caps.";
                if(session.Count(ItemStage.Studio,true)>0||(session.truck.AtStudio&&session.Count(ItemStage.Truck,true)>0))return "E at the basin, then hold LMB to wash your cloudy glass.";
                if(session.Count(ItemStage.Clean,true)>0)return "E at the sorting table. Match each polished piece to its colour tray.";
                var sorted=session.catalog.glass.FirstOrDefault(d=>p.items.Any(r=>r.id==d.id&&r.stage==ItemStage.Sorted));
                if(sorted!=null)return "E at the "+session.catalog.panelNames[sorted.panel]+" sockets to place your sorted glass.";
                if(p.upgrades.Count==0&&session.catalog.upgrades.Any(u=>p.Level>=u.level&&p.caps>=u.cost))return "Your first upgrade is affordable. Visit the studio equipment catalogue.";
                if(cargo>0)return "Bring the loaded truck to the studio parking area to process your haul.";
                return p.skipDrive?"Return to the shore for more finds. The tailgate offers Skip Drive; F reveals glass.":"Return to the shore and gather glass and rubbish. Complete a manual drive to unlock Skip Drive.";
            }
            if(cargo+bucket>=p.TruckCapacity)return "Truck allocation full. Drive to the studio to recycle rubbish and wash glass.";
            if(cargo>0)return "Drive your haul to the studio, or keep gathering if there is room in the truck.";
            if(p.tutorial==0)return "WASD to walk · Mouse to look. Follow the sand mounds.";
            if(p.tutorial==1)return "Select the shovel with 1. Hold LMB on a mound to uncover a find.";
            if(p.tutorial==2)return "Press E on uncovered rubbish and sea glass.";
            return "Gather glass and rubbish, then load the rear crates. F reveals remaining glass.";
        }
    }
}
