using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ironclads
{
    public class BreakoutDesk : MonoBehaviour
    {
        public BreakoutCampaign state;
        public bool open;
        public int selected;
        public bool placing;
        public bool suppressAutosave;
        public bool TextInputFocused {get;private set;}
        public bool UIVisible {get;private set;}=true;
        public void ToggleUI(bool editing){if(!open||editing)return;UIVisible=!UIVisible;FocusMap();}
        bool mapFocusRequested;
        int textControlIndex;
        public void FocusMap(){mapFocusRequested=true;TextInputFocused=false;}
        public CampaignBoard board;
        public float DockWidth=>UIVisible?Mathf.Min(500,Screen.width*.43f):0;
        public static bool IsOpen(MaritimeScene scene)=>scene!=null&&scene.GetComponent<BreakoutDesk>()?.open==true;
        MaritimeScene scene;
        int tacticalSunMask;
        bool tacticalFog;
        int tab,planSide,orderSide,reportSide,stationChoice;
        Vector2 scroll;
        string message="",saveName="Breakout",activeSaveName="Breakout",advance="15",moveReason="",from="",to="",orderText="",orderDelay="0",reportSource="",reportRecipient="",reportText="",reportAssessment="Unconfirmed",reportDelay="0",observationAge="0",eventTitle="",eventNotes="",eventDelay="15",exportRecipient="",ruling="",confirm="";
        bool recon,showRoster;
        int rosterFilter,rosterSide;
        float headingChoice;
        string positionLatitude="",positionLongitude="";
        float nextAutosave;
        GUIStyle text,small,heading,field,area,button,big;
        Texture2D background;
        public string DirectoryPath=>Path.Combine(Application.persistentDataPath,"BreakoutCampaigns");
        public string SavePath=>SlotPath(activeSaveName);
        string SlotPath(string name)
        {
            if(string.IsNullOrWhiteSpace(name)||name.Length>60||name.Any(c=>!char.IsLetterOrDigit(c)&&c!='-'&&c!='_'))throw new ArgumentException("Save name: use 1–60 letters, numbers, underscores or hyphens.");
            return Path.Combine(DirectoryPath,name+".json");
        }
        public static void Show(MaritimeScene scene)
        {
            if(scene.network!=null&&scene.network.Active){scene.status="Disconnect the tactical multiplayer session before opening the private campaign desk.";return;}
            var desk=scene.GetComponent<BreakoutDesk>()??scene.gameObject.AddComponent<BreakoutDesk>();desk.scene=scene;
            if(desk.state==null) {
                desk.activeSaveName=desk.saveName=PlayerPrefs.GetString("BreakoutCampaign.ActiveSlot","Breakout");
                desk.state=BreakoutCampaign.Create();
                try{if(File.Exists(desk.SavePath)){desk.state=BreakoutCampaign.Load(desk.SavePath);desk.message=desk.state.migratedRoster?"Updated KS roster; existing markers retained, new markers staged off map. Original archived as .pre-oob-v3.":desk.state.migratedFromSchematic?"Upgraded schematic save; review assigned ships at geographic station presets. Original file archived as .schematic-v1.":"Resumed saved campaign.";}}
                catch(Exception e){desk.message="Could not resume save: "+e.Message+" Original preserved; using a recovery slot.";desk.activeSaveName=desk.saveName="Recovered-"+DateTime.Now.ToString("yyyyMMdd-HHmmss");}
            }
            if(desk.board==null){desk.board=desk.gameObject.AddComponent<CampaignBoard>();desk.board.desk=desk;desk.board.Build();}
            if(!desk.open){desk.tacticalSunMask=scene.sun.cullingMask;scene.sun.cullingMask&=~(1<<30);desk.tacticalFog=RenderSettings.fog;RenderSettings.fog=false;}
            desk.open=true;desk.board.SetVisible(true);scene.view.enabled=false;scene.animateTime=false;desk.nextAutosave=Time.unscaledTime+20;desk.SelectVessel(desk.selected,false);
        }
        public void ReturnToTable(){open=false;board.SetVisible(false);scene.sun.cullingMask=tacticalSunMask;RenderSettings.fog=tacticalFog;scene.view.enabled=true;scene.SetCamera();}
        public void SelectVessel(int index,bool navigate=true){selected=index;stationChoice=Mathf.Max(0,state.vessels[index].station);headingChoice=state.vessels[index].heading;var p=CampaignGeography.Coordinates(state.vessels[index].x,state.vessels[index].z);positionLatitude=state.vessels[index].station<0?"":p.lat.ToString("0.00000",CultureInfo.InvariantCulture);positionLongitude=state.vessels[index].station<0?"":p.lon.ToString("0.00000",CultureInfo.InvariantCulture);moveReason="";placing=false;if(navigate){tab=1;scroll=Vector2.zero;}}
        public void ShowTab(int index){tab=Mathf.Clamp(index,0,7);scroll=Vector2.zero;}
        public void PlaceOnBoard(float x,float z){Act(()=>state.Place(state.vessels[selected],stationChoice,x,z,headingChoice,moveReason));placing=false;board.Sync();}
        void Update()
        {
            if(!open)return;
            if(Application.isFocused&&Input.GetKeyDown(KeyCode.H))ToggleUI(TextInputFocused);
            if(!suppressAutosave&&Time.unscaledTime>=nextAutosave){nextAutosave=Time.unscaledTime+20;try{state.Save(SavePath);}catch(Exception e){message="Autosave failed: "+e.Message;}}
            if(Input.GetKeyDown(KeyCode.F12))scene.Screenshot();
        }
        void OnApplicationQuit(){if(!suppressAutosave&&open&&state!=null)try{state.Save(SavePath);}catch(Exception e){Debug.LogWarning("Campaign save failed: "+e.Message);}}
        void Act(Action action){try{action();state.Save(SavePath);message="Saved / "+BreakoutCampaign.Clock(state.minute);}catch(Exception e){message=e.Message;}}
        void Styles()
        {
            background=new Texture2D(1,1);background.SetPixel(0,0,new Color(.035f,.065f,.085f));background.Apply();
            text=new GUIStyle(GUI.skin.label){fontSize=13,wordWrap=true,richText=false,normal={textColor=new Color(.84f,.89f,.89f)}};
            small=new GUIStyle(text){fontSize=11};heading=new GUIStyle(text){fontSize=14,fontStyle=FontStyle.Bold,normal={textColor=new Color(.96f,.77f,.44f)},margin=new RectOffset(0,0,14,6)};
            big=new GUIStyle(heading){fontSize=22,margin=new RectOffset(0,0,0,4)};
            field=new GUIStyle(GUI.skin.textField){fontSize=13,richText=false,padding=new RectOffset(7,7,5,5)};area=new GUIStyle(field){wordWrap=true};
            button=new GUIStyle(GUI.skin.button){fontSize=12,wordWrap=true,padding=new RectOffset(7,7,8,8)};
        }
        void T(string value)=>GUILayout.Label(value??"",text);
        void H(string value)=>GUILayout.Label(value,heading);
        bool B(string value)=>GUILayout.Button(value,button);
        string F(string label,string value){GUILayout.Label(label,small);GUI.SetNextControlName("CampaignText"+(textControlIndex++));return GUILayout.TextField(value??"",1000,field);}
        string A(string label,string value,int height=75){GUILayout.Label(label,small);GUI.SetNextControlName("CampaignText"+(textControlIndex++));return GUILayout.TextArea(value??"",12000,area,GUILayout.Height(height));}
        int Number(string value,string name,int min=0,int max=10080){if(!int.TryParse(value,out int result)||result<min||result>max)throw new ArgumentException(name+": enter "+min+" to "+max+" whole minutes.");return result;}
        int SidePicker(int side)=>GUILayout.SelectionGrid(side,new[]{"Union","Confederate"},2,button);
        void OnGUI()
        {
            if(!open)return;
            if(mapFocusRequested){GUI.FocusControl(null);GUIUtility.keyboardControl=0;mapFocusRequested=false;}
            if(!UIVisible){TextInputFocused=false;return;}
            if(text==null)Styles();GUI.depth=-10;float w=DockWidth;
            textControlIndex=0;
            GUI.DrawTexture(new Rect(0,0,w,Screen.height),background);
            GUILayout.BeginArea(new Rect(14,12,w-28,Screen.height-24));
            GUILayout.Label("HAMPTON ROADS — BREAKOUT!",big);T(BreakoutCampaign.Clock(state.minute)+" / "+(state.live?"LIVE":"PREPARATION"));
            GUILayout.Label("PRIVATE UMPIRE DESK • GSHHG / USGS geographic map",small);
            int next=GUILayout.SelectionGrid(tab,new[]{"Clock","Markers","Plans","Orders","Reports","Events","Files","Ledger"},4,button);if(next!=tab){tab=next;scroll=Vector2.zero;confirm="";}
            scroll=GUILayout.BeginScrollView(scroll);
            switch(tab){case 0:Clock();break;case 1:Vessels();break;case 2:Plans();break;case 3:Orders();break;case 4:Reports();break;case 5:Events();break;case 6:Files();break;case 7:Ledger();break;}
            GUILayout.EndScrollView();GUILayout.Label(message,small,GUILayout.MinHeight(35));GUILayout.EndArea();
            GUI.DrawTexture(new Rect(w+8,8,Screen.width-w-16,102),background);
            GUILayout.BeginArea(new Rect(w+20,15,Screen.width-w-40,95));
            GUILayout.BeginHorizontal();GUILayout.Label("JAMES RIVER",heading);if(B("Overview"))board.Overview();if(B("Inspect ship"))board.Focus();if(B("North up"))board.NorthUp();if(B("Top / perspective"))board.ToggleOverhead();if(B("Return to tactical table"))Act(()=>ReturnToTable());GUILayout.EndHorizontal();
            var v=state.vessels[selected];T(v.name+" / "+(v.station<0?"LOCATION NOT ASSIGNED":CampaignBoard.StationNames[v.station]+" / "+CampaignGeography.CoordinateLabel(v.x,v.z))+" / "+v.readiness);
            GUILayout.EndArea();
            GUI.DrawTexture(new Rect(w+8,Screen.height-51,Screen.width-w-16,43),background);
            GUI.Label(new Rect(w+20,Screen.height-48,Screen.width-w-40,42),"Modern GSHHG coast + USGS NHD river banks + USGS 3DEP terrain • not an 1864 shoreline or depth chart.\nWASD pan • Shift faster • RMB orbit • MMB pan • wheel zoom • Alt-click range. H hides/shows UI. Click map to leave text editing.",small);
            TextInputFocused=GUI.GetNameOfFocusedControl().StartsWith("CampaignText",StringComparison.Ordinal);
        }
        void Clock()
        {
            H("CAMPAIGN CONTROL");T("Starts 22 December 1864, 1800. The campaign clock is independent of the tactical table. Routine increments are 15 minutes; use shorter intervals for contact and combat.");
            H("GEOGRAPHIC RIVER SECTORS");T("Zoom to a real-world area. Station presets are approximate viewing/placement anchors.");for(int i=0;i<CampaignBoard.StationNames.Length;i++)if(B("View "+CampaignBoard.StationNames[i]))board.FocusStation(i);
            if(!state.live) {
                T("Preparation: record plans, assign initial readiness and stations, and handle reconnaissance requests. Operational orders remain queued until live play.");
                if(confirm=="start") {T("Begin live play at the current campaign time? This cannot return to preparation.");if(B("Confirm begin live play")){Act(()=>state.BeginLive());confirm="";}}
                else if(B("Begin live play"))confirm="start";
            } else {
                advance=F("Minutes to advance (1–180)",advance);
                if(B("Advance campaign clock"))Act(()=>{state.Save(SavePath);state.Advance(Number(advance,"Advance",1,180));});
            }
            T(state.DueCount+" due items / "+(state.NextDue.HasValue?"Next: "+BreakoutCampaign.Clock(state.NextDue.Value):"No pending timed items"));
            T("The clock stops at the next due order, report or event. Deliver orders, release or hold reports, and adjudicate events before continuing. Ship movement and combat require your rulings.");
            foreach(var o in state.orders.Where(o=>o.status=="Queued"&&o.due<=state.minute&&(state.live||o.reconnaissance)))T(o.id+" — deliver order / "+o.to);
            foreach(var r in state.reports.Where(r=>r.released<0&&!r.held&&r.due<=state.minute))T(r.id+" — release or hold report / "+r.side+" / "+r.recipient);
            foreach(var e in state.events.Where(e=>!e.done&&e.due<=state.minute))T(e.id+" — adjudicate / "+e.title);
            state.environment=A("Umpire environment: weather / tide / current / visibility / navigability",state.environment);
            state.adjudication=A("Umpire mission / escape criteria",state.adjudication);
            state.outcome=F("Umpire outcome assessment",state.outcome);
            state.umpireNotes=A("Private campaign notes",state.umpireNotes);
        }
        void Vessels()
        {
            H(state.vessels.Count(v=>!v.supportResource)+" VESSELS / "+state.vessels.Count(v=>v.supportResource)+" SUPPORT GROUPS");T("KS problem guides / OOB v1.0. Blue is Union; red is Confederate. Off-map staging is unassigned, not a deployed position. Exact coordinates and resource quantities require umpire assignment.");
            GUILayout.BeginHorizontal();if(B("Previous marker"))SelectVessel((selected+state.vessels.Count-1)%state.vessels.Count);if(B("Next marker"))SelectVessel((selected+1)%state.vessels.Count);GUILayout.EndHorizontal();
            if(B(showRoster?"Hide full roster":"Choose from full roster"))showRoster=!showRoster;
            if(showRoster) {
                rosterFilter=GUILayout.SelectionGrid(rosterFilter,new[]{"All","Ships","Support"},3,button);rosterSide=GUILayout.SelectionGrid(rosterSide,new[]{"Both sides","Union","Confederate"},3,button);
                for(int i=0;i<state.vessels.Count;i++){var entry=state.vessels[i];if((rosterFilter==1&&entry.supportResource)||(rosterFilter==2&&!entry.supportResource)||(rosterSide>0&&(int)entry.side!=rosterSide-1))continue;if(B((i==selected?"> ":"")+entry.name+" / "+(entry.station<0?"Unassigned":CampaignBoard.StationNames[entry.station])))SelectVessel(i);}
            }
            var v=state.vessels[selected];H(v.name+" / "+v.side);
            T("Guide starting area: "+v.guideStation+"\n"+v.guideArmament+"\n"+v.guideRestrictions);
            if(v.supportResource)T("Symbolic resource group: model details do not specify quantities. Shore assets should use point or coordinate placement; station presets are river anchors. Movement remains an explicit umpire ruling.");
            v.commander=F("Assigned commander / command",v.commander);v.readiness=F("Actual readiness — umpire only",v.readiness);v.supplies=F("Actual fuel / ammunition / stores",v.supplies);v.condition=F("Actual mechanical / damage condition",v.condition);v.task=A("Current task / orders",v.task,55);v.notes=A("Private ship notes",v.notes,55);
            H(state.live?"ADJUDICATE MOVEMENT":"INITIAL DEPLOYMENT SETUP");
            stationChoice=GUILayout.SelectionGrid(stationChoice,CampaignBoard.StationNames,2,button);moveReason=A(state.live?"Movement ruling (required)":"Initial station assignment note (required)",moveReason,50);
            T("Marker heading: "+headingChoice.ToString("0")+"° true (north = 0°)");headingChoice=GUILayout.HorizontalSlider(headingChoice,0,359);
            if(B("View selected river sector"))board.FocusStation(stationChoice);
            if(B(state.live?"Apply adjudicated station change":"Assign initial station"))Act(()=>{var p=CampaignBoard.Stations[stationChoice];state.Place(v,stationChoice,p.x,p.z,headingChoice,moveReason);board.Sync();});
            if(B(placing?"Cancel map placement":"Place at a point on the 3D board")){if(placing)placing=false;else if(string.IsNullOrWhiteSpace(moveReason))message="Enter a setup note or movement ruling first.";else {placing=true;message="Click the 3D board to place "+v.name+". The station selected above will label this position.";}}
            positionLatitude=F("Placement latitude (decimal degrees north)",positionLatitude);positionLongitude=F("Placement longitude (negative degrees west)",positionLongitude);
            if(B("Apply coordinate placement"))Act(()=>{if(!double.TryParse(positionLatitude,NumberStyles.Float,CultureInfo.InvariantCulture,out double lat)||!double.TryParse(positionLongitude,NumberStyles.Float,CultureInfo.InvariantCulture,out double lon))throw new ArgumentException("Enter decimal latitude and signed longitude.");var p=CampaignGeography.Point(lat,lon);state.Place(v,stationChoice,p.x,p.z,headingChoice,moveReason);board.Sync();});
            if(v.station>=0){T("Actual position: "+CampaignGeography.CoordinateLabel(v.x,v.z));if(!v.supportResource&&CampaignGeography.Land(v.x,v.z))T("Marker is on mapped modern land. Review the placement or adjudicate grounding.");}
            T("Station anchors are approximate geographic presets. Use coordinate or point placement for exact umpire positions. The modern map includes later channel changes and reclamation; navigability, depths, tides and grounding remain your rulings. Actual positions stay out of exports unless you issue a report.");
        }
        void Plans()
        {
            planSide=SidePicker(planSide);var p=state.commands[planSide];H(p.side+" COMMAND PREPARATION");
            if(B("Open "+p.side+" commander problem PDF"))Application.OpenURL(new Uri(Path.Combine(Application.streamingAssetsPath,"CampaignBriefings","Hampton_Roads_"+p.side+"_KS_Problem.pdf")).AbsoluteUri);
            T("Guide operational window: 22 Dec 1864 1800 to 23 Dec 1864 1800. Older saves retain their edited briefing; use the bundled guide when reviewing deployment and command roles.");
            p.briefing=A("Player briefing / additional known information (included in this side's exports)",p.briefing,115);
            p.intent=A("Commander's intent / priorities",p.intent);p.responsibilities=A("Command relationships / responsibilities",p.responsibilities);p.standingOrders=A("Initial standing orders / patrol instructions",p.standingOrders);p.routes=A("Routes / formations / rendezvous",p.routes);p.communications=A("Signals / reports / communication delays",p.communications);p.contingencies=A("Emergencies / withdrawal / concentration",p.contingencies);p.intelligence=A("Reconnaissance / intelligence requirements",p.intelligence);
            T("These fields record plans. Submit actionable requests under Orders. Enter up to six player commands per side in responsibilities and assign vessels under Vessels.");
        }
        void Orders()
        {
            H("SUBMIT COMMAND ORDERS");orderSide=SidePicker(orderSide);from=F("From / commander",from);to=F("To / vessel or command",to);recon=GUILayout.Toggle(recon,"Preparatory reconnaissance request");orderText=A("Orders / request",orderText);orderDelay=F("Delivery delay in minutes (0–10080)",orderDelay);
            if(B("Queue order"))Act(()=>{state.Submit((CampaignSide)orderSide,from,to,orderText,recon,Number(orderDelay,"Delivery delay"));orderText="";});
            ruling=A("Umpire ruling for the order you resolve (private)",ruling,65);
            H("ORDER QUEUE");
            foreach(var o in state.orders.AsEnumerable().Reverse().ToArray()) {
                H(o.id+" / "+o.side+" / "+o.status);T(o.from+" → "+o.to+" / "+(o.reconnaissance?"RECON":"OPERATIONAL"));T("Submitted "+BreakoutCampaign.Clock(o.submitted)+"\nDue "+BreakoutCampaign.Clock(o.due));T(o.text);
                if(o.status=="Queued") {GUI.enabled=o.due<=state.minute&&(state.live||o.reconnaissance);if(B("Deliver "+o.id))Act(()=>state.Deliver(o));GUI.enabled=true;if(!state.live&&!o.reconnaissance)T("Held for live play.");}
                if(o.status=="Delivered"&&B("Resolve "+o.id+" with ruling above"))Act(()=>{state.Resolve(o,ruling);ruling="";});
                if(o.status!="Resolved"&&o.status!="Cancelled"&&B("Cancel "+o.id))Act(()=>state.Cancel(o));
                if(!string.IsNullOrEmpty(o.ruling))T("Private ruling: "+o.ruling);
            }
            T("Resolution changes the order record only. Apply movement, readiness or damage changes under Vessels and write a separate report for players.");
        }
        void Reports()
        {
            H("WRITE PLAYER INTELLIGENCE");reportSide=SidePicker(reportSide);reportSource=F("Source / claimed sender",reportSource);reportRecipient=F("Recipient / command",reportRecipient);reportText=A("Report text — only what the recipient should know",reportText,100);reportAssessment=F("Assessment shown to the recipient",reportAssessment);observationAge=F("Observation age in minutes (0 = now)",observationAge);reportDelay=F("Delivery delay from now (minutes)",reportDelay);
            if(B("Queue report"))Act(()=>{int age=Number(observationAge,"Observation age",0,state.minute);state.QueueReport((CampaignSide)reportSide,reportSource,reportRecipient,reportText,reportAssessment,state.minute-age,Number(reportDelay,"Delivery delay"));reportText="";});
            H("REPORT RELEASE QUEUE");T("Drafts stay private. Release is explicit after their delivery time. Hold to simulate a lost, interrupted or deliberately delayed message. Released reports are permanent; correct them with a new report.");
            foreach(var r in state.reports.AsEnumerable().Reverse().ToArray()) {
                H(r.id+" / "+r.side+" / "+(r.released>=0?"RELEASED":r.held?"HELD":"PENDING"));T(r.source+" → "+r.recipient);T("Observed "+BreakoutCampaign.Clock(r.observed)+"\nDue "+BreakoutCampaign.Clock(r.due));T(r.reliability+"\n"+r.text);
                if(r.released<0) {GUI.enabled=!r.held&&r.due<=state.minute;if(B("Release "+r.id))Act(()=>state.Release(r));GUI.enabled=true;if(B((r.held?"Unhold ":"Hold ")+r.id))Act(()=>state.Hold(r));}else T("Released "+BreakoutCampaign.Clock(r.released));
            }
        }
        void Events()
        {
            H("TIMED UMPIRE EVENTS");T("Schedule a contact check, tide review, mechanical check, reinforcement decision or other reminder. These prompts do not apply automatic effects or imply historically verified conditions.");
            eventTitle=F("Event / check",eventTitle);eventNotes=A("Private trigger / ruling notes",eventNotes);eventDelay=F("Minutes from now",eventDelay);
            if(B("Schedule event"))Act(()=>{state.AddEvent(eventTitle,eventNotes,Number(eventDelay,"Event delay"));eventTitle="";eventNotes="";});
            foreach(var e in state.events.OrderBy(e=>e.due).ToArray()) {
                H(e.id+" / "+(e.done?"DONE":"PENDING")+" / "+e.title);T(BreakoutCampaign.Clock(e.due));e.notes=A("Umpire notes / adjudication",e.notes,55);
                GUI.enabled=!e.done&&e.due<=state.minute;if(B("Mark "+e.id+" adjudicated"))Act(()=>state.Complete(e));GUI.enabled=true;
            }
        }
        void Files()
        {
            H("CAMPAIGN SAVES");T("Active slot: "+activeSaveName);saveName=F("File slot to save or load (letters, numbers, hyphens, underscores)",saveName);T("Every action and a 20-second autosave persist the active slot. Typing a filename does not change it. The previous save is retained as .bak. Tactical saves are separate.");
            if(B("Save campaign to named slot")){try{string target=SlotPath(saveName);if(saveName!=activeSaveName&&File.Exists(target))confirm="overwrite";else SaveNamedSlot();}catch(Exception e){message=e.Message;}}
            if(confirm=="overwrite"){T("That save already exists. Replace it with the current desk?");if(B("Confirm overwrite named slot")){SaveNamedSlot();confirm="";}}
            if(B("Load this save slot"))confirm="load";
            if(confirm=="load") {T("Loading replaces the current campaign desk. The active slot will be saved first.");if(B("Confirm replace desk from save")){try{var loaded=BreakoutCampaign.Load(SlotPath(saveName));state.Save(SavePath);state=loaded;activeSaveName=saveName;RememberSlot();SelectVessel(0);board.Sync();message=state.migratedFromSchematic?"Upgraded schematic save. Review assigned geographic stations; original archived as .schematic-v1.":"Loaded "+SavePath;confirm="";}catch(Exception e){message=e.Message;}}}
            if(B("Start a new December 1864 campaign"))confirm="new";
            if(confirm=="new") {T("The current desk will be replaced. A new save slot will be created.");if(B("Confirm new campaign")){Act(()=>{state.Save(SavePath);state=BreakoutCampaign.Create();activeSaveName=saveName="Breakout-"+DateTime.Now.ToString("yyyyMMdd-HHmmss");RememberSlot();SelectVessel(0);board.Sync();});confirm="";}}
            if(B("Open saves and exports folder")){Directory.CreateDirectory(DirectoryPath);Application.OpenURL(new Uri(DirectoryPath).AbsoluteUri);}
            H("EXPORT PLAYER DISPATCHES");exportRecipient=F("Optional exact recipient (blank = entire side command packet)",exportRecipient);
            T("A side packet includes that side's briefing, command plans and all its released reports. A recipient packet filters reports to the exact command name; plans remain side-wide. Actual positions, ship conditions, pending intelligence, enemy plans and umpire rulings stay private.");
            if(B("Export Union dispatch (.txt)"))Export(CampaignSide.Union);if(B("Export Confederate dispatch (.txt)"))Export(CampaignSide.Confederate);
            T("You distribute these files to players yourself. For ship readiness and stations known to players, write and release explicit reports.");
            if(Directory.Exists(DirectoryPath)){H("AVAILABLE SAVE SLOTS");foreach(var file in Directory.GetFiles(DirectoryPath,"*.json"))T(Path.GetFileNameWithoutExtension(file));}
        }
        void RememberSlot(){PlayerPrefs.SetString("BreakoutCampaign.ActiveSlot",activeSaveName);PlayerPrefs.Save();}
        void SaveNamedSlot(){try{state.Save(SlotPath(saveName));activeSaveName=saveName;RememberSlot();message="Saved "+SavePath;}catch(Exception e){message=e.Message;}}
        void Export(CampaignSide side)
        {
            try{state.Save(SavePath);string suffix=string.IsNullOrWhiteSpace(exportRecipient)?"side-command":"recipient";string path=Path.Combine(DirectoryPath,side+"-"+suffix+"-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".txt");File.WriteAllText(path,state.Dispatch(side,exportRecipient.Trim()),Encoding.UTF8);message="Exported: "+path;}catch(Exception e){message=e.Message;}
        }
        void Ledger(){H("PRIVATE UMPIRE CHRONOLOGY");T("Recorded actions and rulings. Free-text field edits are retained in saves; they do not create individual ledger entries.");foreach(var e in state.ledger.AsEnumerable().Reverse())T(BreakoutCampaign.Clock(e.minute)+"\n"+e.text+"\n");}
        void OnDestroy(){if(background!=null)Destroy(background);}
    }
}
