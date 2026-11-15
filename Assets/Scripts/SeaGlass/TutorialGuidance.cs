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
                    return station.selectedId!=null?"Hold LMB and move the mouse to scrub every cloudy patch. E returns you to the studio.":
                        station.RevealingCleanGlass&&session.Count(ItemStage.Studio,true)>0?"Colour revealed. The next piece is coming up; E to leave.":"This batch is washed. E to leave, then visit the sorting trays.";
                if(station.kind==StationKind.Sort)
                {
                    var piece=session.catalog.glass.FirstOrDefault(x=>x.id==station.selectedId);
                    return piece!=null?"Aim at the "+piece.color+" tray and click LMB. E to leave.":"This batch is sorted. E to leave, then fit the glass into its mosaic.";
                }
            }
            bool atStudio=session.player.transform.position.z>140;
            int bucket=session.BucketCount,cargo=session.CargoCount;
            if(session.player.mode==PlayerMode.Driving)
                return session.truck.AtStudio?"Studio reached. Space to brake, then E to leave the cab.":RoadHazards.Fallen(session)>0?"Part of your load bounced off behind you. Brake, E to get out, and pick it up.":"W/S to drive, A/D to steer, Space to brake. Ease off over potholes and sand ripples, or your load bounces out.";
            int fallen=RoadHazards.Fallen(session);
            if(fallen>0)return "Pick up "+(fallen==1?"the piece":"the "+fallen+" pieces")+" that bounced off the truck, then load "+(fallen==1?"it":"them")+" back into its crates.";
            if(atStudio&&session.Count(ItemStage.Bucket,false)>0)return "Carry your rubbish to the recycling press and press E to feed it in.";
            if(bucket>0&&(atStudio||cargo>0||bucket>=p.BucketCapacity))
            {
                bool glass=session.Count(ItemStage.Bucket,true)>0,rubbish=session.Count(ItemStage.Bucket,false)>0;
                return "E at the truck's rear "+(glass&&rubbish?"glass and rubbish crates":glass?"glass crate":"rubbish crate")+" to load the remaining finds.";
            }
            if(atStudio)
            {
                if(session.truck.AtStudio&&session.Count(ItemStage.Truck,false)>0)return "E at the truck's rubbish crate to take the rubbish out, then carry it to the recycling press for bottle caps.";
                if(session.Count(ItemStage.Studio,true)>0||(session.truck.AtStudio&&session.Count(ItemStage.Truck,true)>0))return "E at the basin, then hold LMB and wipe to wash your cloudy glass.";
                if(session.Count(ItemStage.Clean,true)>0)return "E at the sorting table. Match each polished piece to its colour tray.";
                var sorted=session.catalog.glass.FirstOrDefault(d=>p.items.Any(r=>r.id==d.id&&r.stage==ItemStage.Sorted));
                if(sorted!=null)return "E at the "+session.catalog.panelNames[sorted.panel]+" sockets to place your sorted glass.";
                if(Affordable(session))return "You can afford new equipment. Spend your bottle caps at the equipment cabinet.";
                if(cargo>0)return "Bring the loaded truck to the studio parking area to process your haul.";
                return p.skipDrive?"Return to the shore for more finds. The tailgate offers Skip Drive; F reveals glass.":"Return to the shore and gather glass and rubbish. Complete a manual drive to unlock Skip Drive.";
            }
            if(cargo+bucket>=p.TruckCapacity)return "Truck allocation full. Drive to the studio to recycle rubbish and wash glass.";
            if(cargo>0)return "Drive your haul to the studio, or keep gathering if there is room in the truck.";
            if(Time.time<session.newCoveUntil&&p.SectionOpen(session.newCove))return "A new cove is open. Follow the marker along the beach to find fresh mounds.";
            if(p.tutorial==0)return "WASD to walk · Mouse to look. Follow the sand mounds.";
            if(p.tutorial==1)return "Select the shovel with 1. Hold LMB on a mound to uncover a find.";
            if(p.tutorial==2)return "Press E on uncovered rubbish and sea glass.";
            return "Gather glass and rubbish into your bucket ("+bucket+" / "+p.BucketCapacity+"), then load it into the truck's rear crates. F reveals remaining glass.";
        }
        static bool Affordable(GameSession s)=>s.catalog.upgrades.Any(u=>!s.progress.upgrades.Contains(u.id)&&s.progress.Level>=u.level&&s.progress.caps>=u.cost);
        // Clarity pass (owner playtest, November 14: "not enough direction", "don't know where to buy"): where the current step happens,
        // so the HUD can point at it. Follows the same order as Get; false where the step needs no marker (gathering, at a station).
        public static bool Target(GameSession s,StudioStation station,out Vector3 at,out string label)
        {
            at=default;label=null;var p=s.progress;
            if(p.completed.All(x=>x)||(station&&station.active))return false;
            bool atStudio=s.player.transform.position.z>140;int bucket=s.BucketCount,cargo=s.CargoCount;
            Vector3 Station(StationKind k){var x=Object.FindObjectsByType<StudioStation>(FindObjectsSortMode.None).FirstOrDefault(st=>st.kind==k);return x?x.transform.position:default;}
            Vector3 Crates(){var c=Object.FindObjectsByType<TruckCompartment>(FindObjectsSortMode.None).FirstOrDefault(x=>!x.skip);return c?c.transform.position:s.truck.transform.position;}
            Vector3 Truck()=>s.truck.transform.position+Vector3.up*1.5f;
            if(s.player.mode==PlayerMode.Driving){if(s.truck.AtStudio)return false;at=new Vector3(15,1.2f,150);label="Studio";return true;}
            if(RoadHazards.Fallen(s)>0){var f=RoadHazards.NearestFallen(s,s.player.transform.position);if(f){at=f.transform.position;label="Fallen cargo";return true;}}
            if(atStudio&&s.Count(ItemStage.Bucket,false)>0){at=Station(StationKind.Recycle);label="Recycling press";return true;}
            if(bucket>0&&(atStudio||cargo>0||bucket>=p.BucketCapacity)){at=Crates();label="Truck crates";return true;}
            if(atStudio)
            {
                if(s.truck.AtStudio&&s.Count(ItemStage.Truck,false)>0){var rc=Object.FindObjectsByType<TruckCompartment>(FindObjectsSortMode.None).FirstOrDefault(x=>!x.skip&&!x.glass);at=rc?rc.transform.position:Truck();label="Rubbish crate";return true;}
                if(s.Count(ItemStage.Studio,true)>0||(s.truck.AtStudio&&s.Count(ItemStage.Truck,true)>0)){at=Station(StationKind.Clean);label="Wash basin";return true;}
                if(s.Count(ItemStage.Clean,true)>0){at=Station(StationKind.Sort);label="Sorting trays";return true;}
                var sorted=s.catalog.glass.FirstOrDefault(d=>p.items.Any(r=>r.id==d.id&&r.stage==ItemStage.Sorted));
                if(sorted!=null){var panel=Object.FindObjectsByType<MosaicPanel>(FindObjectsSortMode.None).FirstOrDefault(x=>x.index==sorted.panel);if(panel){at=panel.transform.position;label=s.catalog.panelNames[sorted.panel]+" mosaic";return true;}}
                if(Affordable(s)){at=Station(StationKind.Upgrade);label="Equipment cabinet";return true;}
                if(cargo==0){at=Truck();label="Truck · back to the shore";return true;}
                return false;
            }
            if(cargo+bucket>=p.TruckCapacity){at=Truck();label="Truck";return true;}
            if(Time.time<s.newCoveUntil&&p.SectionOpen(s.newCove))
            {
                var items=Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).Where(i=>i.section==s.newCove&&(i.Record.stage==ItemStage.Buried||i.Record.stage==ItemStage.Shore)).ToArray();
                if(items.Length>0){at=items.Aggregate(Vector3.zero,(a,i)=>a+i.transform.position)/items.Length;label="New cove";return true;}
            }
            return false;
        }
    }
}
