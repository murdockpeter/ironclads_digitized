using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        int dawnTab;bool hardRudder;float turnDegrees=45;
        public void ShowDawnTab(int value){dawnTab=value;scroll=Vector2.zero;}
        string dawnEast="0",dawnNorth="0";
        string[] DawnNames {get{var names=new string[7];for(int i=0;i<7;i++)names[i]=(i<3?"R · ":"J · ")+UlsanScenario.Ships[i].name;return names;}}
        void DawnGUI()
        {
            if(shownScenario!=scene.scenario.scenarioId){shownScenario=scene.scenario.scenarioId;SyncScenarioFields();}
            if(!scene.showOverlay){GUI.Label(new Rect(20,20,320,30),"H · Show Ulsan controls",body);return;}
            var s=scene.scenario;var naval=scene.dawn;
            GUI.DrawTexture(new Rect(0,0,330,Screen.height),panelTex);GUILayout.BeginArea(new Rect(20,15,290,Screen.height-30));
            GUILayout.Label("ULSAN",title);GUILayout.Label("DAWN OF THE BATTLESHIP · 14 AUG 1904",sub);
            GUILayout.BeginHorizontal();if(Button("Fleet view"))scene.Overview();if(Button(scene.globeMode?"Return to sea":"World globe"))scene.ToggleGlobe();GUILayout.EndHorizontal();
            dawnTab=GUILayout.SelectionGrid(dawnTab,new[]{"Ships / move","Gunnery","Weather","Notes / save","TCP fleets","Modules"},3,button);
            scroll=GUILayout.BeginScrollView(scroll);
            if(dawnTab<3){int selected=GUILayout.SelectionGrid(scene.selected,DawnNames,2,button);if(selected!=scene.selected){scene.selected=selected;naval.target=selected<3?3:0;}
                Text(s.ships[scene.selected].name+" · "+(scene.selected<3?"Russian":"Japanese")+" armored cruiser");
                if(Button("Inspect selected cruiser"))scene.Focus(scene.selected);}
            switch(dawnTab){case 0:DawnMovement();break;case 1:DawnGunnery();break;case 2:GUI.enabled=ManageTable;DawnWeather();GUI.enabled=true;break;case 3:DawnNotes();break;case 4:DawnNetwork();break;case 5:DawnModules();break;}
            GUILayout.Space(12);GUILayout.EndScrollView();naval.lowLightAid=GUILayout.Toggle(naval.lowLightAid,"Low-light viewing aid · local",toggle);GUILayout.Label("RMB orbit · Wheel zoom · MMB/WASD pan\nShift + click: exact placement · H hide UI\nv"+Application.version,small);GUILayout.EndArea();
            float x=352,w=Screen.width-x-22;GUI.DrawTexture(new Rect(x,18,w,74),panelTex);GUI.Label(new Rect(x+14,26,w-28,20),"ULSAN · OPEN SEA · RUSSIAN AND JAPANESE CRUISER DIVISIONS",sub);
            GUI.Label(new Rect(x+14,49,w-28,30),s.ships[scene.selected].name+"  /  "+(scene.selected<3?"VLADIVOSTOK SQUADRON":"SECOND DIVISION"),big);
            if(w>700)GUI.Label(new Rect(Screen.width-315,28,280,55),s.Date.AddHours(9).ToString("dd MMM yyyy · HH:mm",CultureInfo.InvariantCulture)+" scenario (UTC+9)\n"+$"TURN {s.turn:00} · {s.dobTurnSeconds/60} min · SUN {scene.SunAltitude:0.0}°",small);
            GUI.DrawTexture(new Rect(342,Screen.height-45,Screen.width-354,33),panelTex);GUI.Label(new Rect(354,Screen.height-40,Screen.width-380,26),scene.status,small);
            if(!scene.globeMode)for(int i=0;i<7;i++){Vector3 p=scene.view.WorldToScreenPoint(s.ships[i].Position+Vector3.up*36);if(p.z>0&&p.x>350&&p.x<Screen.width-60&&p.y>70&&p.y<Screen.height-95)GUI.Label(new Rect(p.x-55,Screen.height-p.y+(scene.distance>3000?(i%3)*12:0),150,20),s.ships[i].name,body);}
        }
        void DawnMovement()
        {
            var s=scene.scenario;var ship=s.ships[scene.selected];var naval=scene.dawn;bool own=naval.Own(scene.selected);
            Heading("PLOTTED ORDERS & EFFECTIVE MOVEMENT");GUI.enabled=own;
            float heading=Slider("Current course · setup / override",ship.heading,0,359.9f,"°");
            float speed=Slider("Effective speed · umpire",ship.speed,0,35," kn");
            float order=Slider("Plotted speed",ship.orderedSpeed,0,35," kn");bool astern=GUILayout.Toggle(ship.astern,"Movement astern",toggle);
            if(heading!=ship.heading||speed!=ship.speed||order!=ship.orderedSpeed||astern!=ship.astern){ship.heading=heading;ship.speed=speed;ship.orderedSpeed=order;ship.astern=astern;scene.network?.SubmitShip(scene.selected);}
            if(Button("Use plotted speed as effective speed")){chart.Remember();ship.speed=ship.orderedSpeed;scene.network?.SubmitShip(scene.selected);}
            Text($"Reference maximum: {UlsanScenario.Ships[scene.selected].maxSpeed:0.0} kn · size B/Medium\nTime moved {ship.dobMovedSeconds:0.0} s / {s.dobTurnSeconds} s\nRemaining {naval.Remaining(scene.selected):0.0} s / {UlsanScenario.Distance(ship.speed,naval.Remaining(scene.selected))/1852:0.000} nmi");
            GUILayout.BeginHorizontal();if(Button("Move 30 s"))naval.Move(scene.selected,30);if(Button("Move 60 s"))naval.Move(scene.selected,60);GUILayout.EndHorizontal();
            if(Button("Move remaining turn allowance"))naval.Move(scene.selected,naval.Remaining(scene.selected));
            Heading("ADVANCE THEN PIVOT AT BRIDGE");hardRudder=GUILayout.Toggle(hardRudder,"Hard rudder · 200 yd advance",toggle);turnDegrees=Slider("Turn angle",turnDegrees,1,45,"°");
            GUILayout.BeginHorizontal();if(Button("Turn PORT"))naval.AdvanceTurn(scene.selected,-turnDegrees,hardRudder);if(Button("Turn STARBOARD"))naval.AdvanceTurn(scene.selected,turnDegrees,hardRudder);GUILayout.EndHorizontal();
            Text("Size B: standard advance 300 yd; hard advance 200 yd. Pivot up to 45° after each advance. The advance spends movement time. Speed losses are 3/4 knots per 45° for standard/hard rudder; apply acceleration, weather, damage and hard-rudder risks yourself.");
            Text("Course corrections of 10° or less per Tactical Turn are an umpire exception; use the course override. These are tools, not automatic move legality checks.");
            if(Button("Reset selected movement ruler · override")){chart.Remember();ship.dobMovedSeconds=0;scene.network?.SubmitShip(scene.selected);}
            Heading("EXACT POSITION · UMPIRE SETUP");GUILayout.BeginHorizontal();dawnEast=GUILayout.TextField(dawnEast,field);dawnNorth=GUILayout.TextField(dawnNorth,field);GUILayout.EndHorizontal();
            if(Button("Set east / north meters")){if(float.TryParse(dawnEast,NumberStyles.Float,CultureInfo.InvariantCulture,out float east)&&float.TryParse(dawnNorth,NumberStyles.Float,CultureInfo.InvariantCulture,out float north))naval.Place(scene.selected,new Vector3(east,0,north));}
            GUI.enabled=true;if(!own)Text("Opponent's fleet: inspect and measure only.");
            Heading("TURN CLOCK · HOST / UMPIRE");GUI.enabled=ManageTable;
            if(Button("End turn · advance clock / reset rulers"))naval.EndTurn();
            GUILayout.BeginHorizontal();if(Button("Tactical · 3 min"))SetDawnInterval(180);if(Button("Intermediate · 30 min"))SetDawnInterval(1800);GUILayout.EndHorizontal();GUI.enabled=true;
            Text("Changing the interval resets movement rulers. Ending a turn retains positions. Plotting, detection, combat, torpedoes, damage and victory remain manual.");
        }
        void SetDawnInterval(int seconds){chart.Remember();scene.scenario.dobTurnSeconds=seconds;foreach(var ship in scene.scenario.ships)ship.dobMovedSeconds=0;}
        void DawnGunnery()
        {
            var table=scene.dawn;var ship=scene.scenario.ships[scene.selected];var effects=GetComponent<BatteryEffects>();
            Heading("TARGET & RANGE RULER");table.target=GUILayout.SelectionGrid(table.target,DawnNames,2,button);
            Vector3 delta=scene.scenario.ships[table.target].Position-ship.Position;
            Text($"{ship.name} → {scene.scenario.ships[table.target].name}\n{delta.magnitude/.9144f:0} yards / {delta.magnitude/914.4f:0.00} kyd\n{delta.magnitude/1852:0.000} nautical miles\n{delta.magnitude/(250*.9144f):0.00} tabletop inches at 250 yd/in\nBearing {GunneryTools.Bearing(delta):0.0}° true · relative {Mathf.DeltaAngle(ship.heading,GunneryTools.Bearing(delta)):+0.0;-0.0;0}°");
            table.measure=GUILayout.Toggle(table.measure,"Show range / bearing line",toggle);table.arcVisible=GUILayout.Toggle(table.arcVisible,"Show custom gun-angle aid",toggle);
            table.arcCenter=Slider("Sector center from bow",table.arcCenter,-180,180,"°");table.arcWidth=Slider("Sector width",table.arcWidth,1,360,"°");table.arcKyd=Slider("Sector reach",table.arcKyd,1,25," kyd");
            Heading("VISUAL BATTERY SALVOS");GUI.enabled=table.Own(scene.selected)&&!scene.globeMode&&!effects.firing[scene.selected];
            if(UlsanScenario.Ships[scene.selected].twinTurrets){float f=Slider("Forward turret bearing from bow",ship.turretHeading,0,359,"°"),a=Slider("After turret bearing from bow",ship.aftTurretHeading,0,359,"°");if(f!=ship.turretHeading||a!=ship.aftTurretHeading){ship.turretHeading=f;ship.aftTurretHeading=a;scene.network?.SubmitShip(scene.selected);}if(Button("Fire forward twin 8-inch turret")){scene.UpdateWorld();effects.Fire(scene.selected,0);}if(Button("Fire after twin 8-inch turret")){scene.UpdateWorld();effects.Fire(scene.selected,3);}}
            if(scene.selected<2){if(Button("Fire bow six-inch pivot"))effects.Fire(scene.selected,0);if(Button("Fire stern six-inch pivot"))effects.Fire(scene.selected,3);}
            if(Button("Fire PORT modeled battery"))effects.Fire(scene.selected,-1);if(Button("Fire STARBOARD modeled battery"))effects.Fire(scene.selected,1);if(Button("All modeled guns · display"))effects.Fire(scene.selected,2);GUI.enabled=true;
            effects.cannonSound=GUILayout.Toggle(effects.cannonSound,"Cannon reports",toggle);effects.volume=Slider("Cannon volume",effects.volume,0,1);if(Button("Clear smoke / stop salvos"))effects.Clear();
            Text("Measuring sectors do not verify weapon-specific firing arcs. Rotating turrets and effects are visual; loading, ammunition, torpedo attacks, hits and damage remain umpire rulings.");
        }
        void DawnWeather()
        {
            var s=scene.scenario;Heading("ULSAN ENVIRONMENT");Text("Scenario reference: sea state 2, clear sky, visibility 90%, wind FROM 315° at 6 knots. Percentage visibility is a rulebook condition, separate from render fog.");
            s.dobVisibilityPercent=Slider("Rulebook visibility",s.dobVisibilityPercent,0,100," %");s.visibilityKm=Slider("Render fog visibility",s.visibilityKm,1,40," km");s.windFrom=Slider("Wind FROM",s.windFrom,0,359,"°");s.windKnots=Slider("Wind speed",s.windKnots,0,50," kn");s.waveHeight=Slider("Rendered wave scale",s.waveHeight,0,5," m");s.cloud=Slider("Cloud cover",s.cloud,0,1);s.rain=Slider("Rain",s.rain,0,1);
            Heading("SUN & TIME");date=GUILayout.TextField(date,field);if(Button("Set UTC time")){try{var dt=DateTime.Parse(date,CultureInfo.InvariantCulture,DateTimeStyles.AdjustToUniversal|DateTimeStyles.AssumeUniversal);if(dt.Year>=1600&&dt.Year<=9998)s.utc=dt.ToString("yyyy-MM-ddTHH:mm:ssZ");}catch(Exception e){Error(e);}}
            Text("The 05:20 scenario clock is displayed using a UTC+9 assumption. The open-sea anchor near Ulsan is approximate; shipboard historical time/GPS precision is not asserted. Sun uses the selected UTC time and location.");
        }
        void DawnNotes()
        {
            Heading("UMPIRE LOG");GUI.enabled=ManageTable;scene.scenario.umpireNotes=GUILayout.TextArea(scene.scenario.umpireNotes,4000,field,GUILayout.Height(180));if(Button("Undo last table action"))chart.Undo();GUI.enabled=true;SaveLoad();
            Text("No detection, ammunition, damage or victory rules are executed. Use your owned books and ship records. Saves include the fleet, orders, clocks, turret bearings and engine settings.");
        }
        void DawnNetwork()
        {
            var net=scene.network;Heading("ONE VS ONE · FLEET COMMAND");Text(net.ConnectionStatus);
            if(net.Active){Text("Your command: "+(net.LocalShip==0?"Russian fleet":"Japanese fleet"));if(net.IsHost)Text($"Host IPv4: {net.HostAddresses}\nTCP {net.Port}\nCode {net.SessionCode}");if(Button("Disconnect / stop hosting"))net.Stop();}
            else{hostShip=GUILayout.SelectionGrid(hostShip,new[]{"Host Russian fleet","Host Japanese fleet"},1,button);networkPort=GUILayout.TextField(networkPort,5,field);networkCode=GUILayout.TextField(networkCode,64,field);if(Button("New session code"))networkCode=MultiplayerSession.NewCode();if(Button("Host Ulsan table")){if(int.TryParse(networkPort,out int port))net.Host(port,hostShip,networkCode);}Heading("JOIN");networkAddress=GUILayout.TextField(networkAddress,45,field);if(Button("Join host IP / port / code")){if(int.TryParse(networkPort,out int port))net.Join(networkAddress,port,networkCode);}}
            Text("Each player commands every ship in their fleet. The host controls the shared clock, weather, notes and loading. Both players need this build. Direct IPv4 requires LAN/VPN reachability or router forwarding; no lobby/relay service is used.");
        }
        void DawnModules()
        {
            Heading("SCENARIO MODULES");GUI.enabled=ManageTable;if(Button("Reset Ulsan · printed seven-cruiser setup"))scene.StartUlsan();if(Button("Hampton Roads")){scene.StartScenario(true);ShowTab(8);}if(Button("Ferrol")){scene.StartScenario(false);ShowTab(8);}GUI.enabled=true;
            Heading("SUPPLIED BOOKS / REFERENCES");Text("Dawn of the Battleship v1.1 (17 Feb 2023); Dawn of the Rising Sun scenario pp.49–51 and annex configurations. Initial courses 090° / 125°, 10 knots; 400/500 yd column spacing; Rossia 195° from Izumo at 8.5 kyd. This is the base scenario; optional distant protected-cruiser reinforcements are not deployed.");
            Text("Cruiser hulls, gun mounts, fittings and paint are procedural reconstructions. The model includes main/secondary batteries and representative light guns, not every small weapon. No private PDF scans or complete combat tables are bundled.");
            foreach(string name in new[]{"Dawn_of_the_Battleship_1.1_Rules_17_Feb_23.pdf","Dawn_of_the_Rising_Sun_Scenarios.pdf","Dawn_of_the_Rising_Sun_Annexes.pdf"})if(Button("Open local "+name.Replace('_',' '))){string path=Path.GetFullPath(Path.Combine(Application.dataPath,"..","contextonly",name));if(File.Exists(path))Application.OpenURL(path);else scene.status="This private reference is not bundled with the player.";}
        }
    }
}
