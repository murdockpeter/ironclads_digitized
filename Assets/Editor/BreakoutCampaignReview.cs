using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class BreakoutCampaignReview
    {
        static int checks;
        static void Check(bool value,string label){if(!value)throw new Exception("Campaign verification failed: "+label);checks++;Debug.Log("CAMPAIGN PASS: "+label);}
        static void Reject(Action action,string label){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}catch(InvalidOperationException){rejected=true;}catch(InvalidDataException){rejected=true;}Check(rejected,label);}
        [MenuItem("Ironclads/Verify Breakout campaign")]
        public static void Run()
        {
            checks=0;var c=BreakoutCampaign.Create();c.Validate();
            Check(c.vessels.Count==56&&c.vessels.Count(v=>!v.supportResource&&v.side==CampaignSide.Confederate)==10&&c.vessels.Count(v=>!v.supportResource&&v.side==CampaignSide.Union)==15,"supplied 25-vessel roster and 31 support-resource groups");
            Check(c.vessels.Any(v=>v.name=="CSS Richmond")&&!c.vessels.Any(v=>v.name.Contains("Stonewall")),"December roster distinct from March module");
            Check(c.vessels.All(v=>v.station==-1&&v.readiness=="Not supplied")&&BreakoutCampaign.Clock(0).StartsWith("22 Dec 1864 18:00"),"unknown readiness and locations; correct start");
            Reject(()=>c.Advance(15),"no clock advancement in preparation");
            var op=c.Submit(CampaignSide.Confederate,"Squadron","CSS Richmond","Get under way",false,0);
            Reject(()=>c.Deliver(op),"no operational delivery in preparation");Reject(()=>c.Resolve(op,"Move"),"no operational execution in preparation");
            var recon=c.Submit(CampaignSide.Union,"Division","Umpire","Request existing chart information",true,0);c.Deliver(recon);c.Resolve(recon,"Information request answered without executing a mission.");
            Check(recon.status=="Resolved"&&c.minute==0,"preparation intelligence request can be answered without time or movement");
            c.BeginLive();Reject(()=>c.Advance(15),"due orders stop clock at current time");c.Deliver(op);c.Resolve(op,"Await vessel movement ruling.");
            var positions=c.vessels.Select(v=>new Vector2(v.x,v.z)).ToArray();
            var report=c.QueueReport(CampaignSide.Union,"Picket","Division","Reported smoke upriver","Unconfirmed",0,7);c.AddEvent("Mechanical review","Manual check",12);
            Check(c.Advance(15)==7&&c.minute==7,"clock stops at report inside a fifteen-minute step");Reject(()=>c.Advance(15),"unreleased due report blocks next step");
            c.Hold(report);Check(c.Advance(15)==5&&c.minute==12,"held reports do not block; next event stops clock");Reject(()=>c.Complete(new CampaignEvent{due=13}),"future event cannot be completed");c.Complete(c.events[0]);
            c.Hold(report);c.Release(report);Check(report.released==12,"late report retains actual release time");Reject(()=>c.Release(report),"released report is immutable through release API");
            var draft=c.QueueReport(CampaignSide.Union,"PRIVATE-SOURCE","Division","UNRELEASED-SECRET","private",12,10);
            var enemy=c.QueueReport(CampaignSide.Confederate,"Enemy","Squadron","ENEMY-SECRET","uncertain",12,0);c.Release(enemy);
            var other=c.QueueReport(CampaignSide.Union,"Station","Captain","CAPTAIN-ONLY","uncertain",12,0);c.Release(other);
            c.commands[1].intent="ENEMY-PLAN";c.vessels[0].notes="TRUTH-SECRET";c.umpireNotes="UMPIRE-SECRET";c.environment="ENVIRONMENT-SECRET";c.Log("LEDGER-SECRET");
            string union=c.Dispatch(CampaignSide.Union);string specific=c.Dispatch(CampaignSide.Union,"Division");
            Check(union.Contains("Reported smoke upriver")&&union.Contains("CAPTAIN-ONLY"),"side export contains released friendly reports");
            foreach(string secret in new[]{"UNRELEASED-SECRET","ENEMY-SECRET","ENEMY-PLAN","TRUTH-SECRET","UMPIRE-SECRET","ENVIRONMENT-SECRET","LEDGER-SECRET","Await vessel movement ruling."})Check(!union.Contains(secret),"export excludes "+secret);
            Check(!specific.Contains("CAPTAIN-ONLY")&&specific.Contains("Reported smoke upriver"),"recipient-specific report isolation");
            Check(c.vessels.Select(v=>new Vector2(v.x,v.z)).SequenceEqual(positions),"orders and clock do not automatically move vessels");
            Reject(()=>c.Place(c.vessels[0],0,0,0,180,""),"movement ruling required");c.Place(c.vessels[0],3,120,170,180,"Umpire adjudicates arrival.");
            var copy=BreakoutCampaign.Parse(JsonUtility.ToJson(c));Check(copy.minute==12&&copy.live&&copy.orders.Count==2&&copy.reports.Count==4&&copy.events[0].done&&copy.vessels[0].station==3&&copy.Dispatch(CampaignSide.Union)==union,"save round trip preserves phase, clock, orders, releases, events, positions and dispatch");
            Reject(()=>BreakoutCampaign.Parse("{}"),"incomplete save rejected");var invalid=BreakoutCampaign.Create();invalid.schema=4;Reject(()=>BreakoutCampaign.Parse(JsonUtility.ToJson(invalid)),"future schema rejected");invalid=BreakoutCampaign.Create();invalid.vessels[0].x=float.PositiveInfinity;Reject(()=>invalid.Validate(),"nonfinite board coordinates rejected");invalid=BreakoutCampaign.Create();invalid.vessels[0].id=invalid.vessels[1].id;Reject(()=>invalid.Validate(),"duplicate vessel identity rejected");
            var legacy=BreakoutCampaign.Parse(JsonUtility.ToJson(c));legacy.schema=1;legacy.vessels=CampaignOrderOfBattle.Legacy();foreach(var v in legacy.vessels){v.x=120;v.z=170;}legacy.vessels[0].station=1;
            var migrated=BreakoutCampaign.Parse(JsonUtility.ToJson(legacy));var anchor=CampaignBoard.Stations[1];
            Check(migrated.schema==3&&migrated.minute==c.minute&&migrated.live&&migrated.vessels[0].x==anchor.x&&migrated.vessels[0].z==anchor.z&&migrated.Dispatch(CampaignSide.Union)==union&&migrated.vessels[0].notes.Contains("Original schematic position"),"legacy save migration retains campaign and dispatches, maps assigned stations and records original positions");
            Check(migrated.vessels.Where(v=>v.station<0).All(v=>!CampaignGeography.InBounds(v.x,v.z)),"unassigned staging stays outside geographic map");
            var oldGeo=JsonUtility.FromJson<BreakoutCampaign>(JsonUtility.ToJson(c));oldGeo.schema=2;oldGeo.vessels=CampaignOrderOfBattle.Legacy();int staging=0;foreach(var v in oldGeo.vessels){var p=CampaignGeography.Staging(staging++,v.side);v.x=p.x;v.z=p.z;}oldGeo.vessels[0].station=1;oldGeo.vessels[0].x=anchor.x+3;oldGeo.vessels[0].z=anchor.z;oldGeo.vessels[0].readiness="Existing umpire assessment";
            var updated=BreakoutCampaign.Parse(JsonUtility.ToJson(oldGeo));
            Check(updated.schema==3&&updated.vessels.Count==57&&updated.vessels.Any(v=>v.id=="CSS-8")&&updated.vessels[0].x==oldGeo.vessels[0].x&&updated.vessels[0].readiness==oldGeo.vessels[0].readiness&&updated.Dispatch(CampaignSide.Union)==union,"geographic save upgrade preserves exact positions, user assessment, legacy Scorpion and dispatches while adding missing assets");
            Check(updated.vessels.Where(v=>!oldGeo.vessels.Any(e=>e.id==v.id)).All(v=>v.station==-1),"new OOB markers remain unassigned during migration");
            var resource=c.vessels.First(v=>v.supportResource);var shore=CampaignGeography.Point(37.003,-76.308);c.Place(resource,5,shore.x,shore.z,90,"Allocate shore resource");
            Check(resource.station==5&&resource.x==shore.x&&BreakoutCampaign.Parse(JsonUtility.ToJson(c)).vessels.Single(v=>v.id==resource.id).x==shore.x,"support resource placement and save round trip");
            Reject(()=>c.Advance(0),"zero clock step rejected");Reject(()=>c.Advance(181),"oversized clock step rejected");Reject(()=>c.QueueReport(CampaignSide.Union,"Picket","Division","Impossible future report","",c.minute+1,0),"future observation rejected");
            string folder=Path.Combine("Artifacts","CampaignVerification");Directory.CreateDirectory(folder);string path=Path.Combine(folder,"roundtrip.json");if(File.Exists(path))File.Delete(path);c.Save(path);c.Hold(draft);c.Save(path);Check(File.Exists(path+".bak")&&BreakoutCampaign.Parse(File.ReadAllText(path)).reports[1].held&&!BreakoutCampaign.Parse(File.ReadAllText(path+".bak")).reports[1].held,"atomic save and previous-state backup");
            File.WriteAllText(Path.Combine(folder,"editor-verification.txt"),"PASS: "+checks+" campaign checks, including preparation, due-time boundaries, export secrecy, recipient isolation and disk persistence.");
        }
    }
}
