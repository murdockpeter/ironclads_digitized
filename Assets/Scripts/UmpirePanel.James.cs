using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        int jamesTab;string jamesColumn="29",jamesRow="17";
        public void ShowJamesTab(int value){jamesTab=value;scroll=Vector2.zero;}
        void JamesGUI()
        {
            if(!scene.showOverlay){GUI.Label(new Rect(20,20,320,30),"H - Show umpire controls",body);return;}
            var s=scene.scenario;var ship=s.ships[scene.selected];var guns=scene.GetComponent<GunneryTools>();var effects=scene.GetComponent<BatteryEffects>();
            GUI.DrawTexture(new Rect(0,0,330,Screen.height),panelTex);GUILayout.BeginArea(new Rect(20,15,290,Screen.height-30));
            GUILayout.Label("IRONCLADS",title);GUILayout.Label("STONEWALL AT THE GATES / 28 MAR 1865",sub);
            GUILayout.BeginHorizontal();if(Button("Fleet view")){chart.topDown=false;scene.Overview();}if(Button(scene.globeMode?"Return to sea":"World globe"))scene.ToggleGlobe();GUILayout.EndHorizontal();
            if(Button(scene.soundEnabled?"Sea sounds: ON / mute":"Sea sounds: OFF / enable"))scene.SetSeaAudio(!scene.soundEnabled,scene.seaVolume);
            if(scene.network.Active&&Button("Lobby / control requests ("+scene.network.Lobby.requests.Length+")"))ShowJamesTab(5);
            jamesTab=GUILayout.SelectionGrid(jamesTab,new[]{"Ships / orders","Gunnery / fire","Weather","Briefing / save","Scenarios","Multiplayer"},2,button);
            scroll=GUILayout.BeginScrollView(scroll);
            if(jamesTab<2){
            Heading("UNION IRONCLADS");GUILayout.BeginHorizontal();for(int i=0;i<3;i++)if(Button(JamesScenario.Names[i].Replace("USS ","")))SelectJames(i);GUILayout.EndHorizontal();
            Heading("CONFEDERATE IRONCLADS");GUILayout.BeginHorizontal();for(int i=3;i<6;i++)if(Button(JamesScenario.Names[i].Replace("CSS ","")))SelectJames(i);GUILayout.EndHorizontal();
            Text(JamesScenario.Names[scene.selected]);if(Button("Inspect selected ship"))scene.Focus(scene.selected);
            }
            switch(jamesTab) {
                case 0:
                    GUI.enabled=OwnShip;GUI.changed=false;
                    Heading("HEX MOVEMENT / UMPIRE ORDERS");Text($"Local hex {RuleChart.At(ship.Position)} / {RuleChart.Facings[RuleChart.Facing(ship.heading)]} / {ship.heading:0} degrees");
                    GUILayout.BeginHorizontal();if(Button("Turn port 60"))chart.Turn(-1);if(Button("Turn starboard 60"))chart.Turn(1);GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();if(Button("Astern 1 hex"))chart.Move(-1);if(Button("Ahead 1 hex"))chart.Move(1);GUILayout.EndHorizontal();
                    Text("Shift-click the sea to place this ship, or enter local column and row.");GUILayout.BeginHorizontal();jamesColumn=GUILayout.TextField(jamesColumn,2,field);jamesRow=GUILayout.TextField(jamesRow,2,field);GUILayout.EndHorizontal();
                    if(Button("Place in hex")){if(int.TryParse(jamesColumn,out int c)&&int.TryParse(jamesRow,out int r)&&c>=1&&c<=82&&r>=1&&r<=74){int q=c-21;chart.Place(new Hex(q,r-19-Mathf.FloorToInt(q/2f)));}else scene.status="Use columns 01-82 and rows 01-74.";}
                    if(scene.selected<3){Heading("TURRET TRAINING");ship.turretHeading=Slider("Forward turret",ship.turretHeading,0,359," degrees");if(scene.selected==0)ship.aftTurretHeading=Slider("Aft turret",ship.aftTurretHeading,0,359," degrees");Text("Train turrets visually; use your ship card for permitted arcs and obstructions.");}
                    Heading("VISUAL ENGINE EFFORT");ship.boilersLit=GUILayout.Toggle(ship.boilersLit,"Boilers lit",toggle);ship.engineLoad=Slider("Engine effort",ship.engineLoad*100,0,100," %")/100;Text("Engine effort changes smoke. Move ships manually using your written orders and ship cards.");
                    if(GUI.changed&&OwnShip)scene.network.SubmitShip(scene.selected);GUI.enabled=ManageTable;
                    if(Button($"End turn {s.turn} / +3 minutes"))chart.NextTurn();if(Button("Undo last placement / turn"))chart.Undo();if(Button("Log all six positions"))chart.Export();GUI.enabled=true;SaveLoad();break;
                case 1:
                    Heading("SELECT A TARGET");int chosen=GUILayout.SelectionGrid(guns.TargetShip,JamesScenario.Names,2,button);if(chosen!=guns.TargetShip){guns.fleetTarget=chosen;guns.pickedTarget=false;}
                    Text($"Range {guns.Range} hexes / {Vector3.Distance(ship.Position,guns.TargetPoint)/.9144f:0} straight-line yd\nBearing {guns.TargetBearing:0.0} degrees true");
                    guns.visible=GUILayout.Toggle(guns.visible,"Show custom arc / sighting ruler",toggle);guns.preset=3;guns.customCenter=Slider("Arc center from bow",guns.customCenter,-180,180," degrees");guns.customSweep=Slider("Arc width",guns.customSweep,1,360," degrees");guns.reach=Slider("Guide radius",guns.reach,1,12," hex");guns.rangeRings=GUILayout.Toggle(guns.rangeRings,"100-yard range bands",toggle);
                    if(Button("Measure selected target again"))guns.pickedTarget=false;Text("Alt-click sea to measure a hex. Guides are measuring tools; adjudicate the card's actual firing template yourself.");
                    BroadsideMeasurement();
                    Heading("MODELED BATTERY");Text(JamesScenario.Batteries[scene.selected]);
                    GUI.enabled=OwnShip;
                    if(Button("Fire all available guns / visual"))effects.Fire(scene.selected,2);
                    GUILayout.BeginHorizontal();if(Button("Forward / bow"))effects.Fire(scene.selected,0);if(Button("Aft / stern"))effects.Fire(scene.selected,3);GUILayout.EndHorizontal();
                    if(scene.selected>=3){GUILayout.BeginHorizontal();if(Button("Port battery"))effects.Fire(scene.selected,-1);if(Button("Starboard battery"))effects.Fire(scene.selected,1);GUILayout.EndHorizontal();}
                    GUI.enabled=true;effects.cannonSound=GUILayout.Toggle(effects.cannonSound,"Cannon reports",toggle);effects.volume=Slider("Cannon volume",effects.volume,0,1);if(Button("Clear powder smoke"))effects.Clear();break;
                case 2:GUI.enabled=ManageTable;Weather();GUI.enabled=true;break;
                case 3:
                    Heading("ALTERNATE HISTORY / UMPIRE BRIEFING");Text("28 March 1865: an early Atlantic passage puts Stonewall in Hampton Roads. Virginia II and Fredericksburg have escaped the upper James to meet her. Onondaga, Mahopac and Sangamon are concentrated to stop them. These departures from history create the encounter.");
                    Text("Suggested game: 20 turns, 100-yard hexes, three-minute turns. Confederates must bring Stonewall and at least one companion off the eastern chart edge; Union must prevent this. Agree damage, escape eligibility and victory terms using your Yaquinto rules and cards before play. Objectives and end conditions are manual.");
                    Text("All six begin stationary, roughly 2,000 yards between lines. Apply written movement orders yourself. No automatic movement allowances, loading, hits, damage or grounding. Modern geographic coast shapes and existing fort scenery are visual context; declare playable water, shoals and fort activity before starting.");
                    Text("Virginia II follows your supplied 7-inch bow card; historical summaries differ. Fredericksburg uses a provisional DANFS battery; reconcile with your own card. Exact hull lines, fittings and small paint markings are reconstructions. Niagara remains available in Ferrol with ten rifles; she is wooden and is outside this all-ironclad force.");
                    if(Button("Open supplied Virginia II sheet"))OpenReference("Virginia2.pdf");
                    GUI.enabled=ManageTable;s.umpireNotes=GUILayout.TextArea(s.umpireNotes,4000,field,GUILayout.Height(100));GUI.enabled=true;SaveLoad();if(Button("Log all six positions"))chart.Export();GUI.enabled=ManageTable;if(Button("Reset six-ship deployment"))scene.StartJames();GUI.enabled=true;break;
                case 4:Scenarios();break;
                case 5:MultiplayerLobby();break;
            }
            ReleaseControls();
            GUILayout.Space(15);GUILayout.EndScrollView();GUILayout.Label("RMB orbit / wheel zoom / MMB pan / H hide UI\nv"+Application.version,small);GUILayout.EndArea();
            float x=352,w=Screen.width-x-22;GUI.DrawTexture(new Rect(x,18,w,74),panelTex);GUI.Label(new Rect(x+15,25,w-30,20),"HAMPTON ROADS / ALTERNATE HISTORY / YAQUINTO IRONCLADS",sub);GUI.Label(new Rect(x+15,48,w-30,34),"Stonewall at the Gates / 3 vs 3",big);
            GUI.DrawTexture(new Rect(342,Screen.height-45,Screen.width-354,33),panelTex);GUI.Label(new Rect(354,Screen.height-40,Screen.width-380,26),$"TURN {s.turn:00} / {s.Date:dd MMM yyyy HH:mm} UTC / "+scene.status,small);if(!scene.globeMode)ShipLabels();
        }
        void SelectJames(int i){scene.selected=i;var guns=scene.GetComponent<GunneryTools>();guns.fleetTarget=i<3?3:0;guns.pickedTarget=false;jamesColumn=(RuleChart.At(scene.scenario.ships[i].Position).q+21).ToString("00");jamesRow=(RuleChart.At(scene.scenario.ships[i].Position).r+Mathf.FloorToInt(RuleChart.At(scene.scenario.ships[i].Position).q/2f)+19).ToString("00");}
    }
}
