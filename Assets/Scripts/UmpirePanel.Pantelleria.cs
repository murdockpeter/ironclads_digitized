using System;
using System.IO;
using System.Linq;
using System.Globalization;
using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        int pantTab,pantNation,pantDivision,pantTargetNation=2,pantTargetDivision=8;
        float pantCourse=105,pantSpeed=15;
        int pantShown=-1;
        public void ShowPantelleriaTab(int value){pantTab=value;scroll=Vector2.zero;}
        void PantSelect(int index){scene.selected=index;pantShown=index;pantNation=PantelleriaScenario.Ships[index].nation;pantDivision=PantelleriaScenario.Ships[index].division;scene.dawn.target=index<21?21:0;pantTargetNation=PantelleriaScenario.Ships[scene.dawn.target].nation;pantTargetDivision=PantelleriaScenario.Ships[scene.dawn.target].division;pantCourse=scene.scenario.ships[index].dobOrderedHeading;pantSpeed=scene.scenario.ships[index].orderedSpeed;}
        void PantelleriaGUI()
        {
            var s=scene.scenario;if(shownScenario!=s.scenarioId){shownScenario=s.scenarioId;SyncScenarioFields();PantSelect(scene.selected);}
            if(pantShown!=scene.selected)PantSelect(scene.selected);
            if(!scene.showOverlay){GUI.Label(new Rect(20,20,350,30),"H / Show Pantelleria controls",body);return;}
            GUI.DrawTexture(new Rect(0,0,330,Screen.height),panelTex);GUILayout.BeginArea(new Rect(20,15,290,Screen.height-30));
            GUILayout.Label("PANTELLERIA",title);GUILayout.Label("14 FEB 1901 / FICTIONAL / DAWN OF THE BATTLESHIP",sub);
            GUILayout.BeginHorizontal();if(Button("All 42 ships"))scene.Overview();if(Button("Selected division"))PantDivisionView(pantDivision);GUILayout.EndHorizontal();
            if(Button("Inspect selected ship"))scene.Focus(scene.selected);
            pantTab=GUILayout.SelectionGrid(pantTab,new[]{"Fleet / orders","Ship movement","Gunnery","Weather","Results / save","Multiplayer","Module / briefing"},2,button);
            BeginPanelScroll();
            if(pantTab<3||pantTab==4)PantShipPicker();
            switch(pantTab){case 0:PantDivisionOrders();break;case 1:DawnMovement();break;case 2:PantTargetPicker();DawnGunnery(true);break;case 3:PantWeather();break;case 4:PantResults();break;case 5:MultiplayerLobby();break;case 6:PantBriefing();break;}
            if(!scene.scenario.Pantelleria){ReleaseControls();EndPanelScroll();GUILayout.EndArea();return;}
            ReleaseControls();GUILayout.Space(12);EndPanelScroll();GUILayout.Label("WASD / MMB pan · RMB orbit · wheel zoom\nShift-click: placement · H: hide controls\nDetail pass 3 / v"+Application.version,small);GUILayout.EndArea();
            float x=352,w=Screen.width-x-22;GUI.DrawTexture(new Rect(x,18,w,82),panelTex);var spec=PantelleriaScenario.Ships[scene.selected];
            GUI.Label(new Rect(x+14,24,w-28,20),"PANTELLERIA / 21 ANGLO-ITALIAN vs 21 FRANCO-RUSSIAN / 42 SHIPS",sub);
            GUI.Label(new Rect(x+14,45,w-28,28),spec.name+" / "+spec.model.name,big);
            GUI.Label(new Rect(x+14,76,w-28,20),PantelleriaScenario.Divisions[spec.division]+" / TURN "+s.turn+" / "+s.Date.AddHours(1).ToString("dd MMM yyyy HH:mm",CultureInfo.InvariantCulture)+" scenario (UTC+1 assumption)",small);
            GUI.DrawTexture(new Rect(342,Screen.height-45,Screen.width-354,33),panelTex);GUI.Label(new Rect(354,Screen.height-40,Screen.width-380,26),scene.status,small);
            if(!scene.globeMode)PantShipLabels();
        }
        void PantShipPicker()
        {
            Heading("NATIONAL COMMAND / DIVISION");int chosen=GUILayout.SelectionGrid(pantNation,PantelleriaScenario.Nations,2,button);
            if(chosen!=pantNation)PantSelect(PantelleriaScenario.Leaders[chosen]);
            var divisions=Enumerable.Range(0,14).Where(d=>PantelleriaScenario.Ships[PantelleriaScenario.Members(d)[0]].nation==pantNation).ToArray();
            int dChoice=GUILayout.SelectionGrid(Array.IndexOf(divisions,pantDivision),divisions.Select(d=>PantelleriaScenario.Divisions[d]).ToArray(),2,button);
            if(dChoice>=0&&divisions[dChoice]!=pantDivision)PantSelect(PantelleriaScenario.Members(divisions[dChoice])[0]);
            var members=PantelleriaScenario.Members(pantDivision);int ship=GUILayout.SelectionGrid(Array.IndexOf(members,scene.selected),members.Select(i=>PantelleriaScenario.Ships[i].name+(scene.scenario.pantelleria.sunk[i]?" [SUNK]":"")).ToArray(),1,button);if(ship>=0&&members[ship]!=scene.selected)PantSelect(members[ship]);
            var model=PantelleriaScenario.Ships[scene.selected].model;Text(model.name+" / main guns "+model.MainGuns+" / representative secondary "+model.secondary+"\n"+model.layout+(model.open?" / barbette":" / gunhouse")+" / detail pass 3 reconstruction");
        }
        void PantDivisionOrders()
        {
            var s=scene.scenario;Heading("DIVISION COMMAND");Text("Group actions require command of every afloat ship in the selected division. Orders do not move ships or advance the clock.");
            pantCourse=Slider("Plotted course",pantCourse,0,359,"° true");pantSpeed=Slider("Plotted speed",pantSpeed,0,35," kn");
            if(Button("Plot course / speed for division"))scene.dawn.PlotDivision(pantDivision,pantCourse,pantSpeed);
            if(Button("Apply plotted course / speed · override"))scene.dawn.ApplyDivision(pantDivision);
            GUILayout.BeginHorizontal();if(Button("Move division 30 s"))scene.dawn.MoveDivision(pantDivision,30);if(Button("Move division 60 s"))scene.dawn.MoveDivision(pantDivision,60);GUILayout.EndHorizontal();
            GUI.enabled=ManageTable;Heading("UMPIRE RECORD OF DIVISION ORDERS");s.pantelleria.divisionOrders[pantDivision]=GUILayout.TextArea(s.pantelleria.divisionOrders[pantDivision],1000,notesField,GUILayout.Height(95));GUI.enabled=true;
            foreach(int i in PantelleriaScenario.Members(pantDivision)){var ship=s.ships[i];Text(ship.name+$": {ship.heading:0}° / {ship.speed:0.0} kn; plotted {ship.dobOrderedHeading:0}° / {ship.orderedSpeed:0.0} kn; moved {ship.dobMovedSeconds:0}s");}
            Heading("TURN CLOCK");GUI.enabled=ManageTable;if(Button("End turn / retain positions"))scene.dawn.EndTurn();GUILayout.BeginHorizontal();if(Button("3-minute turns"))SetDawnInterval(180);if(Button("30-minute turns"))SetDawnInterval(1800);GUILayout.EndHorizontal();if(Button("Undo last table action"))chart.Undo();GUI.enabled=true;SaveLoad();
            Text("Initial division columns are an implementation preset: 500 yd between ships, 800 yd between division lanes. Players may redeploy under the printed signaling constraint. Terrible begins at the scenario's 15 kn; its annex reference is 14 kn.");
        }
        void PantTargetPicker()
        {
            Heading("TARGET NATIONAL COMMAND");int nation=GUILayout.SelectionGrid(pantTargetNation,PantelleriaScenario.Nations,2,button);if(nation!=pantTargetNation){pantTargetNation=nation;pantTargetDivision=PantelleriaScenario.Ships[PantelleriaScenario.Leaders[nation]].division;}
            var divisions=Enumerable.Range(0,14).Where(d=>PantelleriaScenario.Ships[PantelleriaScenario.Members(d)[0]].nation==pantTargetNation).ToArray();int d=GUILayout.SelectionGrid(Array.IndexOf(divisions,pantTargetDivision),divisions.Select(i=>PantelleriaScenario.Divisions[i]).ToArray(),2,button);if(d>=0)pantTargetDivision=divisions[d];
            foreach(int i in PantelleriaScenario.Members(pantTargetDivision))if(Button((scene.dawn.target==i?"> ":"")+PantelleriaScenario.Ships[i].name))scene.dawn.target=i;
        }
        void PantWeather()
        {
            Heading("RIGGING FLAGS");int rigging=GUILayout.SelectionGrid(NavalColours.RiggingMode,new[]{"Colours only","Signal hoists","Full dress"},1,button);if(rigging!=NavalColours.RiggingMode)NavalColours.RiggingMode=rigging;
            Text("Decorative display for this viewer. Full dress adds bow-to-stern flag strings; no signal message or game order is encoded.");
            Heading("PRINTED ENVIRONMENT");Text("Wind FROM 260° at 20 kn; sea state 5; 60% rulebook visibility. Starts 1430 scenario time, 14 February 1901. Shipboard clock assumes UTC+1. Display waves and fog are visual aids; sea-state effects remain umpire decisions.");
            GUI.enabled=ManageTable;var s=scene.scenario;s.dobVisibilityPercent=Slider("Rulebook visibility",s.dobVisibilityPercent,0,100," %");s.windFrom=Slider("Wind FROM",s.windFrom,0,359,"°");s.windKnots=Slider("Wind",s.windKnots,0,50," kn");s.waveHeight=Slider("Display wave scale",s.waveHeight,0,5," m");s.visibilityKm=Slider("Display fog",s.visibilityKm,1,40," km");s.cloud=Slider("Cloud cover",s.cloud,0,1);s.rain=Slider("Rain",s.rain,0,1);date=GUILayout.TextField(date,field);if(Button("Set UTC clock")){try{var dt=DateTime.Parse(date,CultureInfo.InvariantCulture,DateTimeStyles.AdjustToUniversal|DateTimeStyles.AssumeUniversal);if(dt.Year<1600||dt.Year>9998)throw new ArgumentException("Year out of range");s.utc=dt.ToString("yyyy-MM-ddTHH:mm:ssZ");}catch(Exception e){Error(e);}}GUI.enabled=true;Text("Overview fog is reduced locally so distant fleet markers remain legible. This shared table does not hide undetected enemy ships.");
        }
        void PantResults()
        {
            var s=scene.scenario;Heading("UMPIRE DAMAGE / RESULTS LEDGER");Text("Record adjudicated damage here. At 50% or greater, a ship counts as crippled for the result summary. Sunk ships are hidden and cannot move/fire. Damage does not automatically calculate speed loss, armor, flooding or criticals.");
            for(int n=0;n<4;n++){var indices=Enumerable.Range(0,42).Where(i=>PantelleriaScenario.Ships[i].nation==n).ToArray();Text(PantelleriaScenario.Nations[n]+": "+indices.Count(i=>s.pantelleria.sunk[i])+" sunk / "+indices.Count(i=>!s.pantelleria.sunk[i]&&s.pantelleria.damagePercent[i]>=50)+" afloat crippled");}
            int selected=scene.selected;Heading(PantelleriaScenario.Ships[selected].name);GUI.enabled=ManageTable;s.pantelleria.damagePercent[selected]=Slider("Umpire damage",s.pantelleria.damagePercent[selected],0,100," %");bool sunk=GUILayout.Toggle(s.pantelleria.sunk[selected],"Adjudicated SUNK",toggle);if(sunk!=s.pantelleria.sunk[selected]){chart.Remember();s.pantelleria.sunk[selected]=sunk;scene.UpdateWorld();}s.umpireNotes=GUILayout.TextArea(s.umpireNotes,4000,notesField,GUILayout.Height(130));if(Button("Undo last table action"))chart.Undo();GUI.enabled=true;SaveLoad();
            Text("Use each nation's printed victory conditions in the private book. No alliance score, zero-denominator convention or automatic winner is invented by this ledger. Pantelleria uses its own save slot, separate from the other sea tables.");
        }
        void PantBriefing()
        {
            Heading("PANTELLERIA / 42-SHIP MODULE");Text("Fictional Mediterranean fleet action from In Mahan's Wake, printed pp.70–71 (PDF pp.71–72). Britain 13 + Italy 8 versus France 13 + Russia 8. Four national commands; fourteen battle divisions.");
            Text("Explicit setup: Point X lies 20 nmi on bearing 320° from an approximate Pantelleria reference. Leading ships lie 32,000 yd from X: British 290°, Italian 100°, French 345°, Russian 200°. Initial courses are 105°, 282°, 180°, 000°, all at 15 kn. Formations are player-selected; the software supplies an editable division-column preset.");
            Text("Pass-one models have illustrative dimensions, closed hulls, representative superstructures, distinct main-battery layouts, working visual muzzles, trainable end batteries and coal exhaust. Wing/echelon batteries are fixed visual positions. Paint, fittings and light weapons are provisional. Torpedo equipment, individual secondary turret training and weapon-specific arcs are not adjudicated automatically.");
            Text("Combat, spotting, ammunition, damage, movement eligibility, signaling and victory remain with the umpire. This is a shared table, not a double-blind detection simulation. All network players require protocol "+MultiplayerSession.Protocol+" builds.");
            string book=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..","..","contextonly","In_Mahan's_Wake.pdf"));if(Button("Open local scenario book")){if(File.Exists(book))Application.OpenURL(new Uri(book).AbsoluteUri);else scene.status="The private book remains in contextonly and is not bundled.";}
            GUI.enabled=ManageTable;if(Button("Reset Pantelleria / printed approach geometry")){scene.StartPantelleria();PantSelect(0);}if(Button("Ulsan / seven cruisers"))scene.StartUlsan();if(Button("Stonewall at the Gates"))scene.StartJames();if(Button("Hampton Roads"))scene.StartScenario(true);if(Button("Ferrol"))scene.StartScenario(false);if(Button("Close Action"))scene.StartCloseAction();GUI.enabled=true;
            if(Button("Breakout / campaign desk"))BreakoutDesk.Show(scene);
        }
        void PantDivisionView(int division)
        {
            var members=PantelleriaScenario.Members(division);var bounds=new Bounds(scene.scenario.ships[members[0]].Position,Vector3.zero);foreach(int i in members)bounds.Encapsulate(scene.scenario.ships[i].Position);scene.globeMode=false;scene.target=bounds.center;scene.distance=Mathf.Max(1400,bounds.size.magnitude*2.3f);scene.pitch=62;scene.UpdateWorld();scene.SetCamera();
        }
        void PantShipLabels()
        {
            for(int i=0;i<42;i++) {
                if(scene.scenario.pantelleria.sunk[i])continue;var spec=PantelleriaScenario.Ships[i];bool divisionLeader=PantelleriaScenario.Members(spec.division)[0]==i;
                var p=scene.view.WorldToScreenPoint(scene.scenario.ships[i].Position+Vector3.up*22);if(p.z<=0||p.x<350||p.x>Screen.width-125||p.y<75||p.y>Screen.height-115)continue;
                var rect=new Rect(p.x-4,Screen.height-p.y-4,8,8);GUI.color=spec.Side==0?new Color(.60f,.80f,1):new Color(1,.69f,.46f);GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=Color.white;if(GUI.Button(rect,"",GUIStyle.none))PantSelect(i);
                if(scene.distance<10000||i==scene.selected||(scene.distance<20000&&divisionLeader)||PantelleriaScenario.Leaders.Contains(i))GUI.Label(new Rect(p.x+8,Screen.height-p.y+(i%2)*10,190,24),scene.distance>=20000?PantelleriaScenario.Nations[spec.nation]:scene.distance>10000&&divisionLeader?PantelleriaScenario.Divisions[spec.division]:spec.name,small);
            }
        }
    }
}
