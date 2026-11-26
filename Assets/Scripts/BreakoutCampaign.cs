using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ironclads
{
    public enum CampaignSide { Union, Confederate }
    [Serializable] public class CampaignCommand
    {
        public CampaignSide side;
        public string intent="", responsibilities="", standingOrders="", routes="", communications="", contingencies="", intelligence="", briefing="";
    }
    [Serializable] public class CampaignVessel
    {
        public string id, name, type, commander="Unassigned", readiness="Not supplied", supplies="Not supplied", condition="Not supplied", task="", notes="";
        public CampaignSide side;
        public bool supportResource;
        public string guideStation="",guideArmament="",guideRestrictions="";
        public int station=-1;
        public float x,z,heading;
    }
    [Serializable] public class CampaignOrder
    {
        public string id, from, to, text, ruling="";
        public CampaignSide side;
        public bool reconnaissance;
        public int submitted, due;
        public string status="Queued";
    }
    [Serializable] public class CampaignReport
    {
        public string id, source, recipient, text, reliability="Umpire assessment pending";
        public CampaignSide side;
        public int observed, due, released=-1;
        public bool held;
    }
    [Serializable] public class CampaignEvent
    {
        public string id, title, notes="";
        public int due;
        public bool done;
    }
    [Serializable] public class CampaignLedgerEntry { public int minute; public string text; }

    // Authoritative umpire state. Player dispatches are built by an explicit allowlist below.
    [Serializable] public class BreakoutCampaign
    {
        public int schema=3, minute, nextId=1;
        public bool live;
        [NonSerialized] public bool migratedFromSchematic,migratedRoster;
        public string title="Hampton Roads — Breakout!", environment="Weather, tide, current, visibility and navigability: umpire to supply.", adjudication="Define a viable escaping force and Atlantic arrival criteria before play. No preset ship count or turn limit.", outcome="Undecided", umpireNotes="";
        public List<CampaignCommand> commands=new List<CampaignCommand>();
        public List<CampaignVessel> vessels=new List<CampaignVessel>();
        public List<CampaignOrder> orders=new List<CampaignOrder>();
        public List<CampaignReport> reports=new List<CampaignReport>();
        public List<CampaignEvent> events=new List<CampaignEvent>();
        public List<CampaignLedgerEntry> ledger=new List<CampaignLedgerEntry>();
        public static readonly DateTime Epoch=new DateTime(1864,12,22,18,0,0,DateTimeKind.Unspecified);
        public static string Clock(int value)=>Epoch.AddMinutes(value).ToString("dd MMM yyyy HH:mm")+" scenario time";
        public string Id(string prefix)=>prefix+(nextId++).ToString("0000");
        public void Log(string text)=>ledger.Add(new CampaignLedgerEntry{minute=minute,text=text});
        public static BreakoutCampaign Create()
        {
            var c=new BreakoutCampaign();
            c.commands.Add(new CampaignCommand{side=CampaignSide.Union,briefing=CampaignOrderOfBattle.Briefing(CampaignSide.Union)});
            c.commands.Add(new CampaignCommand{side=CampaignSide.Confederate,briefing=CampaignOrderOfBattle.Briefing(CampaignSide.Confederate)});
            c.vessels=CampaignOrderOfBattle.Markers();
            c.adjudication="Guide operational window: 22 Dec 1864 1800 to 23 Dec 1864 1800. Umpire defines viable escaping force and Atlantic arrival criteria. The clock does not automatically end play or execute operations.";
            c.Log("Campaign prepared at 1800 on 22 December 1864. All locations, readiness and supplies await umpire assignment. Staging markers do not imply deployed positions.");return c;
        }
        public void BeginLive(){if(live)throw new InvalidOperationException("Live play has already begun.");live=true;Log("Live play begins. Operational orders may now be delivered and resolved.");}
        public CampaignOrder Submit(CampaignSide side,string from,string to,string text,bool recon,int delay)
        {
            Required(from,"Sender");Required(to,"Recipient");Required(text,"Order");Delay(delay);
            var o=new CampaignOrder{id=Id("O"),side=side,from=from,to=to,text=text,reconnaissance=recon,submitted=minute,due=minute+delay};orders.Add(o);Log(o.id+" submitted / "+side+" / "+(recon?"reconnaissance":"operational"));return o;
        }
        public void Deliver(CampaignOrder o)
        {
            if(!orders.Contains(o)||o.status!="Queued"||o.due>minute||(!live&&!o.reconnaissance))throw new InvalidOperationException("This order cannot be delivered yet.");
            o.status="Delivered";Log(o.id+" delivered to "+o.to);
        }
        public void Resolve(CampaignOrder o,string ruling)
        {
            if(!orders.Contains(o)||o.status!="Delivered"||(!live&&!o.reconnaissance))throw new InvalidOperationException("Deliver this order during the permitted phase first.");
            Required(ruling,"Umpire ruling");o.ruling=ruling;o.status="Resolved";Log(o.id+" resolved: "+ruling);
        }
        public void Cancel(CampaignOrder o){if(!orders.Contains(o)||o.status=="Resolved"||o.status=="Cancelled")throw new InvalidOperationException("Order is already closed.");o.status="Cancelled";Log(o.id+" cancelled by umpire.");}
        public CampaignReport QueueReport(CampaignSide side,string source,string recipient,string text,string reliability,int observed,int delay)
        {
            Required(source,"Source");Required(recipient,"Recipient");Required(text,"Report");Delay(delay);if(observed<0||observed>minute)throw new ArgumentException("Observation time must be between campaign start and now.");
            var r=new CampaignReport{id=Id("R"),side=side,source=source,recipient=recipient,text=text,reliability=reliability,observed=observed,due=minute+delay};reports.Add(r);Log(r.id+" report queued for "+side+" / "+recipient);return r;
        }
        public void Release(CampaignReport r)
        {
            if(!reports.Contains(r)||r.released>=0||r.held||r.due>minute)throw new InvalidOperationException("Only a due, unheld report can be released.");
            r.released=minute;Log(r.id+" released to "+r.side+" / "+r.recipient);
        }
        public void Hold(CampaignReport r){if(r.released>=0)throw new InvalidOperationException("Released reports cannot be recalled.");r.held=!r.held;Log(r.id+(r.held?" held":" unheld"));}
        public void AddEvent(string title,string notes,int delay){Required(title,"Event title");Delay(delay);events.Add(new CampaignEvent{id=Id("E"),title=title,notes=notes,due=minute+delay});Log("Reminder queued: "+title);}
        public void Complete(CampaignEvent e){if(!events.Contains(e)||e.done||e.due>minute)throw new InvalidOperationException("This event is not due.");e.done=true;Log(e.id+" adjudicated: "+e.title+" / "+e.notes);}
        IEnumerable<int> PendingTimes()=>orders.Where(o=>o.status=="Queued"&&(live||o.reconnaissance)).Select(o=>o.due).Concat(reports.Where(r=>r.released<0&&!r.held).Select(r=>r.due)).Concat(events.Where(e=>!e.done).Select(e=>e.due));
        public int DueCount=>PendingTimes().Count(t=>t<=minute);
        public int? NextDue { get {var times=PendingTimes().OrderBy(t=>t).ToArray();return times.Length==0?(int?)null:times[0];} }
        public int Advance(int minutes)
        {
            if(!live)throw new InvalidOperationException("Begin live play before advancing the clock.");
            if(minutes<1||minutes>180)throw new ArgumentException("Use 1 to 180 minutes per advance.");
            if(DueCount>0)throw new InvalidOperationException("Handle due orders, reports and events first. Reports can be held.");
            int target=checked(minute+minutes);if(target>525600)throw new ArgumentException("Campaign clock exceeds the supported one-year span.");int? next=NextDue;if(next.HasValue&&next.Value<target)target=next.Value;
            int before=minute;minute=target;Log("Clock advanced "+(minute-before)+" minutes; no vessel movement or combat was automated.");return minute-before;
        }
        public void Place(CampaignVessel v,int station,float x,float z,float heading,string reason)
        {
            if(!vessels.Contains(v)||station<0||station>=CampaignBoard.StationNames.Length)throw new ArgumentException("Choose a named campaign station.");Required(reason,live?"Movement ruling":"Initial setup note");
            if(!CampaignGeography.InBounds(x,z))throw new ArgumentException("Position is outside the geographic campaign map.");
            if(!float.IsFinite(heading))throw new ArgumentException("Heading must be finite.");v.station=station;v.x=x;v.z=z;v.heading=Mathf.Repeat(heading,360);Log((live?"Adjudicated movement: ":"Initial setup: ")+v.name+" / "+CampaignBoard.StationNames[station]+" / "+reason);
        }
        public string Dispatch(CampaignSide side,string recipient="")
        {
            var p=commands.Single(c=>c.side==side);var b=new StringBuilder();
            b.AppendLine(title+" — "+side+" dispatch").AppendLine(Clock(minute)).AppendLine(live?"LIVE PLAY":"PREPARATION — operational missions have not begun").AppendLine();
            b.AppendLine("COMMAND BRIEFING").AppendLine(p.briefing).AppendLine();
            string[] names={"Commander's intent","Responsibilities","Standing orders","Routes and formations","Communications","Contingencies","Intelligence requirements"};
            string[] values={p.intent,p.responsibilities,p.standingOrders,p.routes,p.communications,p.contingencies,p.intelligence};
            for(int i=0;i<names.Length;i++)b.AppendLine(names[i]).AppendLine(values[i]).AppendLine();
            b.AppendLine("RELEASED REPORTS" + (string.IsNullOrWhiteSpace(recipient)?" — SIDE COMMAND PACKET":" — "+recipient));
            foreach(var r in reports.Where(r=>r.side==side&&r.released>=0&&(string.IsNullOrWhiteSpace(recipient)||string.Equals(r.recipient,recipient,StringComparison.OrdinalIgnoreCase))).OrderBy(r=>r.released))
                b.AppendLine(r.id+" / observed "+Clock(r.observed)+" / received "+Clock(r.released)).AppendLine("From: "+r.source+" / To: "+r.recipient).AppendLine("Assessment: "+r.reliability).AppendLine(r.text).AppendLine();
            // Actual vessel locations, readiness, opposite-side plans, rulings and the umpire ledger are intentionally absent.
            return b.ToString();
        }
        static void Required(string text,string name){if(string.IsNullOrWhiteSpace(text))throw new ArgumentException(name+" is required.");}
        static void Delay(int delay){if(delay<0||delay>10080)throw new ArgumentException("Delay must be 0 to 10080 minutes.");}
        public void Validate()
        {
            if((schema!=1&&schema!=2&&schema!=3)||minute<0||minute>525600||nextId<1||commands==null||vessels==null||orders==null||reports==null||events==null||ledger==null)throw new InvalidDataException("Invalid or unsupported campaign save.");
            if(commands.Count!=2||commands.Any(c=>c==null)||commands[0].side!=CampaignSide.Union||commands[1].side!=CampaignSide.Confederate)throw new InvalidDataException("Campaign commands or roster are incomplete.");
            var expected=schema<=2?CampaignOrderOfBattle.Legacy():CampaignOrderOfBattle.Markers();
            var optional=CampaignOrderOfBattle.Legacy().Single(v=>v.id=="CSS-8");
            var allowed=schema<=2?expected:expected.Concat(new[]{optional}).ToList();
            if(!expected.All(e=>vessels.Any(v=>v!=null&&v.id==e.id))||vessels.Count<expected.Count||vessels.Count>expected.Count+(schema==3?1:0))throw new InvalidDataException("Campaign roster is incomplete.");
            float limit=schema==1?3000:120000;
            if(vessels.Any(v=>v==null||!allowed.Any(e=>e.id==v.id&&e.side==v.side&&e.name==v.name&&e.type==v.type&&e.supportResource==v.supportResource)||v.station< -1||v.station>=CampaignBoard.StationNames.Length||!float.IsFinite(v.x)||!float.IsFinite(v.z)||!float.IsFinite(v.heading)||Math.Abs(v.x)>limit||Math.Abs(v.z)>limit||(schema>=2&&v.station>=0&&!CampaignGeography.InBounds(v.x,v.z)))||vessels.Select(v=>v.id).Distinct().Count()!=vessels.Count)throw new InvalidDataException("Invalid campaign vessels.");
            if(orders.Any(o=>o==null||!Enum.IsDefined(typeof(CampaignSide),o.side)||!new[]{"Queued","Delivered","Resolved","Cancelled"}.Contains(o.status)||o.submitted<0||o.submitted>minute||o.due<o.submitted||(!live&&!o.reconnaissance&&(o.status=="Delivered"||o.status=="Resolved")))||reports.Any(r=>r==null||!Enum.IsDefined(typeof(CampaignSide),r.side)||r.observed<0||r.observed>minute||r.due<r.observed||r.released< -1||r.released>minute||(r.released>=0&&r.released<r.due))||events.Any(e=>e==null||e.due<0)||ledger.Any(e=>e==null||e.minute<0||e.minute>minute))throw new InvalidDataException("Invalid campaign queue or timeline.");
            var ids=orders.Select(o=>o.id).Concat(reports.Select(r=>r.id)).Concat(events.Select(e=>e.id)).ToArray();
            if(ids.Any(string.IsNullOrWhiteSpace)||ids.Distinct().Count()!=ids.Length||ids.Any(id=>id.Length<2||!int.TryParse(id.Substring(1),out int n)||n<1||n>=nextId))throw new InvalidDataException("Invalid campaign identifiers.");
        }
        public static BreakoutCampaign Parse(string json)
        {
            var c=JsonUtility.FromJson<BreakoutCampaign>(json);if(c==null)throw new InvalidDataException("Empty campaign save.");c.Validate();
            if(c.schema==1){var stations=CampaignBoard.Stations;for(int i=0;i<c.vessels.Count;i++){var v=c.vessels[i];v.notes+=(string.IsNullOrEmpty(v.notes)?"":"\n")+$"Original schematic position: {v.x:0.###}, {v.z:0.###}.";var p=v.station>=0?stations[v.station]:CampaignGeography.Staging(i<9?i:i-9,v.side);v.x=p.x;v.z=p.z;}c.schema=2;c.migratedFromSchematic=true;c.Log("Migrated schematic save to geographic map. Assigned vessels are placed at approximate station anchors; review deployment. Original schematic coordinates retained in private vessel notes. All plans, orders and report release states retained.");c.Validate();}
            if(c.schema==2) {
                var roster=CampaignOrderOfBattle.Markers();
                foreach(var v in c.vessels) {
                    var updated=roster.FirstOrDefault(e=>e.id==v.id);
                    if(updated!=null){v.type=updated.type;v.guideStation=updated.guideStation;v.guideArmament=updated.guideArmament;v.guideRestrictions=updated.guideRestrictions;}
                    else v.guideRestrictions="Retained from legacy save; absent from supplied OOB v1.0. Umpire authorization required.";
                }
                foreach(var v in roster)if(!c.vessels.Any(e=>e.id==v.id))c.vessels.Add(v);
                c.schema=3;c.migratedRoster=true;c.Log("Updated to KS OOB v1.0: new markers staged off map. Existing positions, readiness, plans, orders and report release states retained. Legacy Scorpion retained for umpire review.");c.Validate();
            }
            return c;
        }
        public static BreakoutCampaign Load(string path)
        {
            var c=Parse(File.ReadAllText(path));if(c.migratedRoster&&!File.Exists(path+".pre-oob-v3"))File.Copy(path,path+".pre-oob-v3");if(c.migratedFromSchematic&&!File.Exists(path+".schematic-v1"))File.Copy(path,path+".schematic-v1");return c;
        }
        public void Save(string path)
        {
            Validate();Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(this,true),Encoding.UTF8);
            if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
        }
    }
}
