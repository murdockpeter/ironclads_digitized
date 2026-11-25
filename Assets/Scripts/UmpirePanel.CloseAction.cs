using System;
using System.Linq;
using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        int caTab,caShown=-1,caMask=3,caVariant;
        string caPlot="0",caAddress="A1435",caSignalAddress="Fleet",caSignal="",caNote="",caBgn="",caManualR="0",caManualH="0",caManualS="0",caManualM="0";
        string[] caTasks={"GUNS","GUNS","GUNS","GUNS","FREE"};
        bool caHigh,caDismantling;
        int caWeatherSection,caWeatherDirection=3,caWeatherStrength=3,caWeatherSeenDirection=-1,caWeatherSeenStrength=-1;
        CloseActionTable CAT=>scene.closeAction;
        public void ShowCloseActionTab(int value){caTab=Mathf.Clamp(value,0,7);scroll=Vector2.zero;}
        void CloseActionGUI()
        {
            if(!scene.showOverlay){GUI.Label(new Rect(20,20,340,30),"H / Show Close Action controls",body);return;}
            CAT.Bind();var state=scene.scenario.closeAction;int selected=scene.selected;
            if(caShown!=selected){caShown=selected;var draft=CAT.Draft(selected);caPlot=draft.plot;caTasks=(string[])draft.tasks.Clone();caAddress=CloseActionRules.Address(CloseActionRules.Bow(scene.scenario.ships[selected]));caNote=state.logs[selected].notes;caBgn="";CAT.target=Enumerable.Range(0,state.logs.Length).First(i=>CloseActionScenario.Spec(scene.scenario,i).side!=CloseActionScenario.Spec(scene.scenario,selected).side);}
            GUI.DrawTexture(new Rect(0,0,330,Screen.height),panelTex);GUILayout.BeginArea(new Rect(20,15,290,Screen.height-30));
            GUILayout.Label("CLOSE ACTION",title);GUILayout.Label("NELSON vs BRUEYS / 23 JUNE 1798 / SCENARIO 6",sub);
            if(Button("Pantelleria / 42 battleships (1901)")){scene.StartPantelleria();GUILayout.EndArea();return;}
            if(Button("Breakout! / 3D campaign desk (Dec 1864)")){BreakoutDesk.Show(scene);GUILayout.EndArea();return;}
            GUILayout.BeginHorizontal();if(Button("Fleet view")){CAT.topDown=false;scene.Overview();}if(Button("Inspect ship")){CAT.topDown=false;scene.Focus(selected);}GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();if(Button("< Ship")){scene.selected=(selected+state.logs.Length-1)%state.logs.Length;caShown=-1;}if(Button("Ship >")){scene.selected=(selected+1)%state.logs.Length;caShown=-1;}GUILayout.EndHorizontal();
            Text((CloseActionScenario.Spec(scene.scenario,selected).side==0?"BRITISH / ":"FRENCH / ")+scene.scenario.ShortName(selected));
            if(Button("Weather / wind: WD "+state.windDirection+" / WS "+state.wind)){caTab=5;scroll=Vector2.zero;}
            caTab=GUILayout.SelectionGrid(caTab,new[]{"Fleet / setup","Orders / sail","Gunnery","Ship log","Signals","Weather","Briefing","Multiplayer"},2,button);
            Text("T"+scene.scenario.turn+" / "+CloseActionRules.PhaseNames[state.phase]);
            GUI.enabled=ManageTable;
            if(Button(state.phase==4?"Complete maintenance / next 200 seconds":state.phase==1?"Reveal all orders / begin movement":"Advance to next phase"))CAT.Advance();
            GUI.enabled=true;
            GUILayout.BeginHorizontal();if(Button("Save"))try{CAT.Save();}catch(Exception e){Error(e);}GUI.enabled=ManageTable;if(Button("Load"))try{CAT.Load();caShown=-1;}catch(Exception e){Error(e);}GUI.enabled=true;if(Button("CSV log"))try{CAT.Export();}catch(Exception e){Error(e);}GUILayout.EndHorizontal();
            BeginPanelScroll();
            switch(caTab){case 0:CAFleet();break;case 1:CAOrders();break;case 2:CAGunnery();break;case 3:CALog();break;case 4:CASignals();break;case 5:CAWeather();break;case 6:CABriefing();break;case 7:MultiplayerLobby();break;}
            ReleaseControls();GUILayout.Space(12);EndPanelScroll();
            GUILayout.Label("67.5 yd / hex  |  200 s / turn  |  two-hex ships\nRMB orbit / wheel zoom / MMB pan / H hide\nv"+Application.version,small);GUILayout.EndArea();
            float x=352,w=Screen.width-x-22;GUI.DrawTexture(new Rect(x,18,w,78),panelTex);
            GUI.Label(new Rect(x+14,25,w-28,19),"CLOSE ACTION / LARGE FLEET / HYPOTHETICAL ENCOUNTER OFF SICILY",sub);
            GUI.Label(new Rect(x+14,47,w-28,30),"Nelson vs Brueys / "+scene.scenario.ships.Length+" sail",big);
            if(w>720)GUI.Label(new Rect(Screen.width-335,28,300,56),scene.scenario.Date.ToString("dd MMM yyyy / HH:mm:ss")+" UTC\nWD "+state.windDirection+" (toward) / WS "+state.wind+" / sea "+state.seaState,small);
            GUI.DrawTexture(new Rect(342,Screen.height-45,Screen.width-354,33),panelTex);GUI.Label(new Rect(354,Screen.height-40,Screen.width-380,27),scene.status,small);
        }
        int CAInteger(string label,int value,int min,int max)
        {
            GUILayout.BeginHorizontal();GUILayout.Label(label,small,GUILayout.Width(180));string raw=GUILayout.TextField(value.ToString(),6,field,GUILayout.Width(78));GUILayout.EndHorizontal();return int.TryParse(raw,out int n)?Mathf.Clamp(n,min,max):value;
        }
        int CARaw(string label,ref string raw,int min=0,int max=100)
        {
            GUILayout.BeginHorizontal();GUILayout.Label(label,small,GUILayout.Width(180));raw=GUILayout.TextField(raw,6,field,GUILayout.Width(78));GUILayout.EndHorizontal();return int.TryParse(raw,out int n)?Mathf.Clamp(n,min,max):0;
        }
        void CAFleet()
        {
            var state=scene.scenario.closeAction;
            for(int side=0;side<2;side++){
                Heading(side==0?"ROYAL NAVY / NELSON":"FRENCH FLEET / BRUEYS");
                for(int i=0;i<state.logs.Length;i++){
                    var spec=CloseActionScenario.Spec(scene.scenario,i);if(spec.side!=side)continue;
                    var l=state.logs[i];GUI.color=i==scene.selected?gold:Color.white;
                    if(Button(spec.name+" / "+spec.rate+(spec.flagship?" / flag":"")+(l.struck?" / STRUCK":""))){scene.selected=i;caShown=-1;}
                    GUI.color=Color.white;
                }
            }
            Text("Selected prototype: "+CloseActionScenario.Spec(scene.scenario,scene.selected).Prototype.label+" / 10 models (4 British, 6 French).");
            Heading("DISPLAY / CAMERA");CAT.showGrid=GUILayout.Toggle(CAT.showGrid,"67.5-yard hex grid",toggle);CAT.showLabels=GUILayout.Toggle(CAT.showLabels,"Map / sea-room coordinates",toggle);CAT.showArcs=GUILayout.Toggle(CAT.showArcs,"Four 60-degree battery arcs",toggle);CAT.showPaths=GUILayout.Toggle(CAT.showPaths,"Own sealed / revealed path",toggle);CAT.topDown=GUILayout.Toggle(CAT.topDown,"Overhead chart view",toggle);
            GUILayout.BeginHorizontal();if(Button("Bow detail"))scene.InspectDetail(true);if(Button("Gun detail"))scene.InspectDetail(false);if(Button("Stern"))scene.InspectStern();GUILayout.EndHorizontal();
            GUI.enabled=ManageTable;Heading("UMPIRE SETUP / CORRECTIONS");Text("The book's nominal setup is loaded. Both fleets must scatter before play. Enter bow positions from the scattering diagram; sea-room coordinates may be extended beyond A/B.");
            state.scatterComplete=GUILayout.Toggle(state.scatterComplete,"Scattering / opening sails reviewed",toggle);
            caAddress=GUILayout.TextField(caAddress,24,field);
            Text("Bow address: A1435, B1403, or axial q,r for open sea. Shift-click also places the bow. Placement changes are umpire actions.");
            if(Button("Place selected bow here"))try{
                Hex h;if(caAddress.Contains(',')){var tokens=caAddress.Replace("SEA","").Trim().Split(',');h=new Hex(int.Parse(tokens[0]),int.Parse(tokens[1]));}else h=CloseActionRules.ParseAddress(caAddress);
                CAT.Place(scene.selected,h,CloseActionRules.Facing(scene.scenario.ships[scene.selected]));
            }catch(Exception e){Error(e);}
            int facing=CAInteger("Facing / 1-6",CloseActionRules.Facing(scene.scenario.ships[scene.selected])+1,1,6)-1;
            if(facing!=CloseActionRules.Facing(scene.scenario.ships[scene.selected]))CAT.Place(scene.selected,CloseActionRules.Bow(scene.scenario.ships[scene.selected]),facing);
            if(state.phase==0&&scene.scenario.turn==1){int sail=GUILayout.SelectionGrid(state.logs[scene.selected].sail,CloseActionRules.SailNames,2,button);if(CloseActionScenario.Spec(scene.scenario,scene.selected).side==0)state.logs[scene.selected].sail=sail;else Text("French initial sail is MS; changes after play begins use crew tasks.");}
            if(Button("Roll selected scattering d6 + modifiers")){
                var s=CloseActionScenario.Spec(scene.scenario,scene.selected);int cq=state.logs[scene.selected].quality,mod=cq==0?2:cq==1?1:cq==3?-1:cq==4?-2:0;mod+=s.speedType=="L-f"?1:0;int die=UnityEngine.Random.Range(1,7);CAT.Journal(s.name+" scattering d6="+die+", modifier "+mod+", result "+(die+mod)+". Apply Scenario Book p.4 diagram.");
            }
            caVariant=GUILayout.SelectionGrid(caVariant,CloseActionScenario.Variants,1,button);if(Button("Reset Close Action / chosen book variant")){scene.StartCloseAction(caVariant);caShown=-1;}
            GUI.enabled=true;
        }
        void CAOrders()
        {
            int i=scene.selected;var state=scene.scenario.closeAction;var l=state.logs[i];var spec=CloseActionScenario.Spec(scene.scenario,i);var ship=scene.scenario.ships[i];
            Heading("BOW / STERN / WIND");var cells=CloseActionRules.Occupied(ship);int wf=CloseActionRules.WindFacing(CloseActionRules.Facing(ship),state.windDirection);
            Text("Bow "+CloseActionRules.Address(cells[0])+" / stern "+CloseActionRules.Address(cells[1])+"\nWF "+CloseActionRules.WindName(wf)+" / "+spec.speedType+" / maneuver "+spec.Maneuver+"\nSail "+CloseActionRules.SailNames[l.sail]+" / rig sections "+l.RigSections+"\nCurrent MA "+CloseActionRules.Allowance(spec,l,state,CloseActionRules.Facing(ship))+" / prior MPs "+l.lastMP);
            GUI.enabled=OwnShip&&state.phase==1;
            Heading("SEALED MOVEMENT PLOT");caPlot=GUILayout.TextField(caPlot,24,field);Text("Examples: 3P2, 2S1, or 0. Ahead = one hex / MP; P or S = 60 degrees / MP, pivoting about the bow.");
            Heading("CREW TASKS / S1-S4 + MARINES");
            for(int section=0;section<5;section++){GUILayout.BeginHorizontal();GUILayout.Label(section==4?"M":("S"+(section+1)),small,GUILayout.Width(28));caTasks[section]=GUILayout.TextField(caTasks[section],8,field);GUILayout.EndHorizontal();}
            Text("GUNS, FREE, T, FS, MS, PS, FURL, RR, RH, RS, FF, FB, BR, BA, BD, RO, PT. One task per crew section; crew used for tasks cannot fire guns.");
            if(Button("Validate and seal selected orders"))CAT.Submit(new CloseActionOrder{ship=i,plot=caPlot,tasks=caTasks.Select(t=>t.Trim().ToUpperInvariant()).ToArray(),turn=scene.scenario.turn});
            GUI.enabled=true;Text("Sealed ships received: "+state.submitted.Length+" / "+state.logs.Length+". Opponents do not receive draft plots.");
            GUI.enabled=ManageTable;if(state.phase==1&&Button("Fill missing plots with legal straight coast"))CAT.FillMissing();
            if(state.phase==2){Heading("MOVEMENT RESOLUTION");Text("Segment "+state.segment+" / 10");if(Button("Execute next simultaneous segment"))CAT.StepMovement();
                if(!string.IsNullOrEmpty(CAT.conflict)){Text(CAT.conflict);if(Button("Allow temporary stern overlap this segment"))CAT.StepMovement(true);if(Button("Hold conflicting ships / umpire adjudication"))CAT.HoldConflict();Text("Use Fleet / setup to correct positions or logs for bearing off, contested hexes and collision damage. No invisible collision simplification is imposed.");}
                foreach(var order in state.revealed)Text(scene.scenario.ShortName(order.ship)+": "+order.plot+" / executed "+state.logs[order.ship].executed);
            }
            Heading("MOVEMENT-CARD OVERRIDE");Text("Published L-f / L-s movement values are built in. Overrides apply only to this exact wind / rig / sail profile.");
            string key=CloseActionRules.ProfileKey(spec,l,state);var card=state.profiles.FirstOrDefault(p=>p.Key==key)??new CloseActionProfile{speedType=spec.speedType,wind=state.wind,rig=l.RigSections,sail=l.MovementSail,c=CloseActionCharts.Allowance(spec.speedType,state.wind,l.RigSections,l.MovementSail,1),b=CloseActionCharts.Allowance(spec.speedType,state.wind,l.RigSections,l.MovementSail,2),r=CloseActionCharts.Allowance(spec.speedType,state.wind,l.RigSections,l.MovementSail,3)};
            Text(key);card.c=CAInteger("Close-hauled C",card.c,0,10);card.b=CAInteger("Broad reach B",card.b,0,10);card.r=CAInteger("Running R",card.r,0,10);if(Button("Apply this profile override")&&l.RigSections>0&&l.MovementSail>0)CAT.SetProfile(card);GUI.enabled=true;
        }
        void CAGunnery()
        {
            var state=scene.scenario.closeAction;int source=scene.selected;
            Heading("TARGET / TWO-HEX RANGE");
            var enemies=Enumerable.Range(0,state.logs.Length).Where(i=>CloseActionScenario.Spec(scene.scenario,i).side!=CloseActionScenario.Spec(scene.scenario,source).side).ToArray();
            int choice=Array.IndexOf(enemies,CAT.target);choice=GUILayout.SelectionGrid(Mathf.Max(0,choice),enemies.Select(scene.scenario.ShortName).ToArray(),2,button);CAT.target=enemies[choice];
            int range=CloseActionRules.Range(scene.scenario.ships[source],scene.scenario.ships[CAT.target]);Text("Range "+range+" hexes / "+(range*67.5f).ToString("0.0")+" nominal yards. Exact hex range uses the nearest of bow / stern pairs.");
            Heading("BATTERIES / ARC & LOS");string[] names={"Port bow","Port stern","Starboard bow","Starboard stern"};
            for(int b=0;b<4;b++){bool legal=CloseActionRules.CanBattery(scene.scenario,source,CAT.target,b,out string reason);Text(names[b]+": "+(legal?"arc and LOS clear":reason));bool active=(caMask&(1<<b))!=0;bool next=GUILayout.Toggle(active,"Declare "+names[b],toggle);if(next)caMask|=1<<b;else caMask&=~(1<<b);}
            int rake=CloseActionRules.Rake(scene.scenario,source,CAT.target);if(rake>0)Text(rake==2?"STERN RAKE: crew results doubled; resolve before bow rakes / ordinary fire.":"BOW RAKE: resolve before ordinary fire.");
            caHigh=GUILayout.Toggle(caHigh,"Aim high / rigging (required at 10+)",toggle);caDismantling=GUILayout.Toggle(caDismantling,"Dismantling shot / maximum 7 hexes",toggle);
            Text("Published BGN: "+CloseActionCharts.BasicGunnery(CloseActionScenario.Spec(scene.scenario,source),range)+". Leave override blank to use it.");caBgn=GUILayout.TextField(caBgn,4,field);
            GUI.enabled=OwnShip&&state.phase==3;if(Button("Seal fire declaration")){int bgn=-1;if(caBgn.Trim().Length>0&&!int.TryParse(caBgn,out bgn))scene.status="BGN override must be numeric or blank.";else CAT.DeclareFire(new CloseActionFire{ship=source,target=CAT.target,mask=caMask,high=caHigh,dismantling=caDismantling,bgn=bgn});}
            GUI.enabled=ManageTable&&state.phase==3;if(Button("Reveal all gunfire / roll simultaneously"))CAT.RevealFire();
            for(int n=0;n<state.fireOrders.Length;n++){
                var shot=state.fireOrders[n];Heading(scene.scenario.ShortName(shot.ship)+" -> "+scene.scenario.ShortName(shot.target));
                int mod=CloseActionRules.GunneryModifier(scene.scenario,shot),fgn=shot.bgn+mod+shot.die;
                Text("BGN "+shot.bgn+" / modifiers "+mod+" / d6 "+shot.die+" = FGN "+fgn+"\n"+CloseActionCharts.RawResult(fgn,shot.high,state.logs[shot.target].sail));
                if(shot.resolved)Text(shot.result);else{if(Button("Resolve published result / queue damage"))CAT.ResolveFire(n,true);if(Button("Umpire: no damage / resolved manually"))CAT.ResolveFire(n,false);}
            }
            GUI.enabled=true;Text("Results are queued until Combat ends, so one ship's damage cannot suppress its same-phase return fire. Resolve yellow-sheet criticals and boarding as umpire actions.");
            Heading("EFFECTS / DISPLAY ONLY");var effects=scene.GetComponent<BatteryEffects>();effects.cannonSound=GUILayout.Toggle(effects.cannonSound,"Cannon reports",toggle);effects.volume=Slider("Cannon volume",effects.volume,0,1);GUI.enabled=OwnShip;
            GUILayout.BeginHorizontal();if(Button("Port display"))effects.Fire(source,-1);if(Button("Starboard display"))effects.Fire(source,1);GUILayout.EndHorizontal();GUI.enabled=true;if(Button("Clear powder smoke"))effects.Clear();
        }
        void CALog()
        {
            var state=scene.scenario.closeAction;int i=scene.selected;var l=state.logs[i];var spec=CloseActionScenario.Spec(scene.scenario,i);
            Heading("SHIP LOG / "+spec.name.ToUpperInvariant());Text(spec.rate+" guns nominal / "+spec.points+" VP / "+spec.longGuns+" LG + "+spec.carronades+" CR per side\nDraft "+spec.draft+" ft / freeboard "+spec.freeboard+" / LDGM "+spec.lowerDeck+" / CH mod "+spec.critical);
            GUI.enabled=ManageTable;for(int section=0;section<4;section++){l.rig[section]=CAInteger("Rigging "+(section+1),l.rig[section],0,spec.rig[section]+1);l.hull[section]=CAInteger("Hull "+(section+1),l.hull[section],0,spec.hull[section]+1);}
            for(int section=0;section<5;section++)l.crew[section]=CAInteger(section==4?"Marines":"Sailor S"+(section+1),l.crew[section],0,spec.crew[section]);
            l.quality=CAInteger("CQ / A=0 to G=6",l.quality,0,6);l.portDismounted=-CAInteger("Port guns lost (penalty)",-l.portDismounted,0,40);l.starboardDismounted=-CAInteger("Starboard guns lost",-l.starboardDismounted,0,40);l.fires=CAInteger("Active fires",l.fires,0,100);
            l.struck=GUILayout.Toggle(l.struck,"Struck / surrendered",toggle);l.captured=GUILayout.Toggle(l.captured,"Captured / cannot sail or fire",toggle);l.removed=GUILayout.Toggle(l.removed,"Removed / exploded",toggle);l.fouled=GUILayout.Toggle(l.fouled,"Fouled / no voluntary movement",toggle);l.grappled=GUILayout.Toggle(l.grappled,"Grappled / no voluntary movement",toggle);l.inIrons=GUILayout.Toggle(l.inIrons,"In irons",toggle);l.poorManeuver=GUILayout.Toggle(l.poorManeuver,"Poor maneuvering / umpire turns",toggle);l.signalCut=GUILayout.Toggle(l.signalCut,"Signal halyards cut",toggle);
            if(Button("Take morale check / d10 (0-9)"))CAT.Morale(i,UnityEngine.Random.Range(0,10));
            Heading("UMPIRE DAMAGE / CRITICAL ADJUSTMENT");int r=CARaw("Rigging loss",ref caManualR),h=CARaw("Hull loss",ref caManualH),s=CARaw("Sailor loss",ref caManualS),m=CARaw("Marine loss",ref caManualM);
            if(Button("Apply assessed damage to this ship")){GetComponent<RuleChart>().Remember();CloseActionRules.Damage(scene.scenario,new CloseActionHit{target=i,rig=r,hull=h,sailors=s,marines=m,note="Umpire assessed damage."});CAT.Journal(spec.name+" umpire damage recorded.");}
            caNote=GUILayout.TextArea(caNote,1500,notesField,GUILayout.Width(270),GUILayout.Height(95));if(Button("Record log note"))l.notes=caNote;GUI.enabled=true;Text(l.notes);
            Heading("VICTORY POINTS / NO FIXED TURN LIMIT");Text("British "+CAT.Score(0).ToString("0.00")+" / French "+CAT.Score(1).ToString("0.00")+" VP inflicted. Complete current sections determine fractional VP; no double counting struck ships.");
            GUI.enabled=ManageTable;if(Button("Record agreed end of scenario")){state.finished=true;state.outcome="British "+CAT.Score(0).ToString("0.00")+" / French "+CAT.Score(1).ToString("0.00")+" VP";CAT.Journal("Scenario ended by agreement: "+state.outcome);}GUI.enabled=true;
            if(state.phase==4){Heading("MAINTENANCE / UMPIRE CHECKLIST");GUI.enabled=ManageTable;for(int n=0;n<7;n++)state.maintenance[n]=GUILayout.Toggle(state.maintenance[n],state.maintenanceLabels[n],toggle);GUI.enabled=true;Text("Check drift, towing, grapples, unfouling, fire damage/growth, repairs, criticals and morale using the local sheets. Sail alterations and signal delivery complete with the turn; no other checked step is silently auto-applied.");}
        }
        void CASignals()
        {
            Heading("ONE SIGNAL / PLAYER / TURN");Text("1798: flagships allow five words; ordinary captains three. Signals are delivered at end of turn. Ship names count as one word. No instant team chat, plot notation or coordinates.");
            caSignalAddress=GUILayout.TextField(caSignalAddress,30,field);caSignal=GUILayout.TextField(caSignal,150,field);Text("Address Fleet, Van, Center, Rear, or one friendly ship. Uppercase message; no punctuation, contractions or hyphens.");
            GUI.enabled=OwnShip&&scene.scenario.closeAction.phase==1;if(Button("Seal / replace my signal"))CAT.SubmitSignal(scene.selected,caSignalAddress,caSignal.ToUpperInvariant());GUI.enabled=true;
            Heading("DELIVERED SIGNALS");foreach(var s in CAT.inbox.TakeLast(25))Text("T"+s.turn+" / "+scene.scenario.ShortName(s.sender)+" -> "+s.address+"\n"+s.text);
            Text("Enemy clients never receive the other side's sealed or delivered signals. The host is the trusted umpire. Adjacent-stern direct exchanges and signal wording disputes remain umpire decisions.");
        }
        void CAWeather()
        {
            var ca=scene.scenario.closeAction;var weather=scene.scenario;
            Heading("WEATHER AT THE TABLE");Text("Current WD "+ca.windDirection+" toward / WS "+ca.wind+" / "+weather.windKnots.ToString("0")+" kn. Wind FROM "+weather.windFrom.ToString("0")+" degrees. Published sea state: "+ca.seaState+".");
            if(caWeatherSeenDirection!=ca.windDirection||caWeatherSeenStrength!=ca.wind){caWeatherDirection=ca.windDirection;caWeatherStrength=ca.wind;caWeatherSeenDirection=ca.windDirection;caWeatherSeenStrength=ca.wind;}
            if(!ManageTable)Text("The host changes shared weather. Sea sound controls below are local to you.");
            caWeatherSection=GUILayout.SelectionGrid(caWeatherSection,new[]{"Sea / sky","Wind / turns"},2,button);
            if(caWeatherSection==1){
            GUI.enabled=ManageTable;Heading("WIND / MOVEMENT AND CANVAS");
            caWeatherDirection=GUILayout.SelectionGrid(caWeatherDirection-1,new[]{"1 / North 000","2 / NE 060","3 / SE 120","4 / South 180","5 / SW 240","6 / NW 300"},2,button)+1;
            caWeatherStrength=GUILayout.SelectionGrid(caWeatherStrength-1,new[]{"WS 1 / 3 kn","WS 2 / 7 kn","WS 3 / 12 kn","WS 4 / 19 kn"},2,button)+1;
            GUI.enabled=ManageTable&&ca.phase<=1;if(Button("Apply selected wind now"))CAT.SetWindNow(caWeatherDirection,caWeatherStrength);
            GUI.enabled=ManageTable;if(Button("Queue selected wind for next turn")){ca.pendingDirection=caWeatherDirection;ca.pendingWind=caWeatherStrength;CAT.Journal("Wind queued for next turn: WD "+ca.pendingDirection+", WS "+ca.pendingWind+".");}
            GUI.enabled=true;
            Text(ca.phase<=1?"Changing current wind clears sealed movement plots so captains can resubmit.":"Current wind is fixed after movement orders reveal; queue the next turn's wind here.");
            Text(ca.pendingDirection<1&&ca.pendingWind<1?"No wind change queued.":"Next turn: WD "+(ca.pendingDirection<1?ca.windDirection:ca.pendingDirection)+" / WS "+(ca.pendingWind<1?ca.wind:ca.pendingWind)+".");
            GUI.enabled=ManageTable;if(Button("Clear queued wind")){ca.pendingDirection=ca.pendingWind=-1;}
            GUI.enabled=ManageTable&&ca.phase==0;if(Button("Roll Oceanic / Steady weather"))CAT.RollWeather();GUI.enabled=true;
            Text("Scenario 6 begins WD 3 / WS 3. Weather rolls are made during Reinforcements & weather and take effect next turn. Sea state remains 3.");
            }
            if(caWeatherSection==0){
            GUI.enabled=ManageTable;Heading("SEA / SKY / VISIBILITY");
            GUILayout.BeginHorizontal();
            if(Button("Fair")){weather.cloud=.18f;weather.rain=0;weather.waveHeight=.45f;weather.visibilityKm=20;}
            if(Button("Overcast")){weather.cloud=.85f;weather.rain=0;weather.waveHeight=.8f;weather.visibilityKm=12;}
            if(Button("Rain")){weather.cloud=.95f;weather.rain=.65f;weather.waveHeight=1.4f;weather.visibilityKm=4;}
            GUILayout.EndHorizontal();
            weather.cloud=Slider("Cloud cover",weather.cloud*100,0,100," %")/100;
            weather.rain=Slider("Rain",weather.rain*100,0,100," %")/100;
            weather.visibilityKm=Slider("Visual visibility",weather.visibilityKm,.5f,40," km");
            weather.waveHeight=Slider("Wave scale",weather.waveHeight,0,5," m");
            Text("Sea and sky controls change the scene immediately. Apply any visibility house rule as umpire; the scenario's published sea state stays 3.");
            }
            GUI.enabled=true;Heading("SEA SOUNDS");
            bool sounds=GUILayout.Toggle(scene.soundEnabled,"Ocean and wind ambience",toggle);float volume=Slider("Sea volume",scene.seaVolume,0,1);
            if(sounds!=scene.soundEnabled||!Mathf.Approximately(volume,scene.seaVolume))scene.SetSeaAudio(sounds,volume);
        }

        void CABriefing()
        {
            Heading("LARGE FLEET ACTION AT SEA");Text("23 June 1798: a hypothetical encounter off Sicily during the Nile campaign. Nelson's reduced battle fleet intercepts Brueys before the French expedition reaches Egypt. The baseline is ten British ships against thirteen French ships. The transports and the British detachment pursuing them are outside this tactical module.");
            Text("British van: Nelson in Vanguard; rear: Troubridge in Culloden. French van: du Chayla in Franklin; center: Brueys in Orient; rear: Villeneuve in Guillaume Tell. Published alternate orders of battle are selectable in Fleet / setup.");
            Text("Standard LONG setup and sea room. French begin MS; British choose sail. Both sides scatter. No anchoring permission, no automatic escape and no fixed historical turn limit. Standard fractional VP for lost sections and full VP for struck ships decides the agreed ending.");
            Text("Use the 1997 rules with the publisher's 1998 errata. The module uses verified numerical movement and gunfire tables from BGG photographs. Complex criticals, boarding, contested hexes, bearing off, drift and maintenance remain umpire-assisted. Physical gun counts are visual, not gunnery factors.");
            Heading("LOCAL REFERENCES");if(Button("Open supplied Close Action rules"))OpenReference("CloseActionRules.pdf");if(Button("Open supplied scenario book"))OpenReference("CloseActionScenarios.pdf");
            foreach(var item in new[]{new[]{"Movement / turning","movement"},new[]{"Gunnery / BGN","gunnery"},new[]{"Gunfire results","results"},new[]{"Hull critical hits","hull"},new[]{"Rigging critical hits","rigging"},new[]{"Auxiliary tables","auxiliary"}})if(Button(item[0]+" / local image"))OpenReference("CloseAction-"+item[1]+".jpg");
            if(Button("Publisher's official errata"))Application.OpenURL("https://clashofarms.com/files/close-action.pdf");if(Button("BoardGameGeek chart gallery"))Application.OpenURL("https://boardgamegeek.com/boardgame/3553/close-action-the-age-of-fighting-sail-vol-1/images");
            Heading("OTHER MODULES");GUI.enabled=ManageTable;if(Button("Stonewall at the Gates")){scene.StartJames();return;}if(Button("Hampton Roads")){scene.StartScenario(true);return;}if(Button("Ferrol")){scene.StartScenario(false);return;}if(Button("Ulsan")){scene.StartUlsan();return;}GUI.enabled=true;
        }
    }
}
