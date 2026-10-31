using System;
using System.Globalization;
using UnityEngine;

namespace Ironclads
{
    public class UmpirePanel : MonoBehaviour
    {
        public MaritimeScene scene;
        RuleChart chart;
        int tab;
        public void ShowTab(int value){tab=value;scroll=Vector2.zero;}
        public void ShowSheet(bool niagara=false){sheet=Resources.Load<Texture2D>(niagara?"NiagaraSheet":"StonewallSheet");if(sheet==null){sheetOpen=false;scene.status="This optional user-supplied ship sheet is not bundled.";return;}sheetTitle=niagara?"USS NIAGARA / SUPPLIED 1863 CARD":"CSS STONEWALL / SUPPLIED SHIP SHEET";sheetZoom=niagara?.27f:.65f;sheetScroll=Vector2.zero;sheetOpen=true;}
        Vector2 scroll,sheetScroll;
        GUIStyle title,sub,body,small,button,field,section,big,toggle;
        Texture2D panelTex,buttonTex,fieldTex,white;
        Texture2D sheet;
        string sheetTitle="CSS STONEWALL / SUPPLIED SHIP SHEET";
        bool sheetOpen;
        float sheetZoom=.65f;
        string lat="43.43",lon="-8.65",date="1865-03-24T14:00:00Z",col="21",row="19",east="0",north="0";
        readonly Color gold=new Color(.92f,.70f,.37f),ink=new Color(.72f,.81f,.82f),teal=new Color(.38f,.79f,.76f);
        static Texture2D Texture(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
        void Styles()
        {
            panelTex=Texture(new Color(.035f,.065f,.082f,.97f));buttonTex=Texture(new Color(.10f,.17f,.19f));
            fieldTex=Texture(new Color(.025f,.046f,.059f));white=Texture(Color.white);
            title=new GUIStyle(GUI.skin.label){fontSize=29,fontStyle=FontStyle.Bold,normal={textColor=gold}};
            sub=new GUIStyle(GUI.skin.label){fontSize=10,normal={textColor=teal},wordWrap=true};
            body=new GUIStyle(GUI.skin.label){fontSize=13,normal={textColor=ink},wordWrap=true};
            small=new GUIStyle(body){fontSize=11};
            section=new GUIStyle(body){fontSize=11,fontStyle=FontStyle.Bold,normal={textColor=gold},margin=new RectOffset(0,0,16,7)};
            big=new GUIStyle(body){fontSize=23,normal={textColor=Color.white}};
            button=new GUIStyle(GUI.skin.button){fontSize=12,padding=new RectOffset(8,8,9,9),margin=new RectOffset(2,2,3,3),
                normal={background=buttonTex,textColor=ink},hover={background=buttonTex,textColor=gold},active={background=fieldTex,textColor=teal}};
            field=new GUIStyle(GUI.skin.textField){fontSize=13,padding=new RectOffset(8,8,6,6),normal={background=fieldTex,textColor=Color.white},focused={background=fieldTex,textColor=gold}};
            toggle=new GUIStyle(GUI.skin.toggle){fontSize=12,normal={textColor=ink},onNormal={textColor=teal}};
            sheet=Resources.Load<Texture2D>("StonewallSheet");
        }
        void Heading(string text)=>GUILayout.Label(text,section);
        bool Button(string text)=>GUILayout.Button(text,button);
        void Text(string text)=>GUILayout.Label(text,body);
        float Slider(string label,float value,float min,float max,string suffix="")
        {
            GUILayout.BeginHorizontal();GUILayout.Label(label,body);GUILayout.FlexibleSpace();GUILayout.Label(value.ToString("0.0")+suffix,small);GUILayout.EndHorizontal();
            return GUILayout.HorizontalSlider(value,min,max);
        }
        void Error(Exception e){scene.status=e.Message;}
        void OnGUI()
        {
            if(scene==null||scene.view==null)return;if(title==null)Styles();if(chart==null)chart=GetComponent<RuleChart>();
            if(!scene.showOverlay){GUI.Label(new Rect(20,20,300,30),"H  •  Show umpire controls",body);return;}
            GUI.DrawTexture(new Rect(0,0,330,Screen.height),panelTex);
            GUILayout.BeginArea(new Rect(20,16,290,Screen.height-30));
            GUILayout.Label("IRONCLADS",title);GUILayout.Label("FERROL  /  24 MARCH 1865",sub);GUILayout.Space(4);
            Text("A sea table for the umpire");GUILayout.Space(12);
            GUILayout.BeginHorizontal();if(Button(scene.globeMode?"← Return to sea":"◎  World globe"))scene.ToggleGlobe();
            if(Button("Fleet view")){chart.topDown=false;scene.Overview();}GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            string[] tabs={"Ships","Weather","Chart","Sources"};
            for(int i=0;i<tabs.Length;i++){GUI.color=i==tab?gold:Color.white;if(GUILayout.Button(tabs[i],button))tab=i;}GUI.color=Color.white;
            GUILayout.EndHorizontal();
            if(Button("Gunnery tools / bearings & arcs"))ShowTab(4);
            if(Button("Fire batteries / smoke & sound"))ShowTab(5);
            scroll=GUILayout.BeginScrollView(scroll,false,false,GUILayout.ExpandHeight(true));
            switch(tab){case 0:Vessels();break;case 1:Weather();break;case 2:Chart();break;case 3:Sources();break;case 4:Gunnery();break;case 5:FireBatteries();break;case 6:Engines();break;}
            GUILayout.Space(18);GUILayout.EndScrollView();
            GUILayout.Label("RMB orbit  ·  Wheel zoom  ·  MMB pan\n1 / 2 focus  ·  Tab globe  ·  H hide UI  ·  v"+Application.version,small);
            GUILayout.EndArea();
            Header();
            GUI.DrawTexture(new Rect(342,Screen.height-45,Screen.width-354,33),panelTex);
            GUI.Label(new Rect(354,Screen.height-40,Screen.width-380,26),scene.status,small);
            if(!scene.globeMode)ShipLabels();
            if(sheetOpen)SheetWindow();
        }
        void Header()
        {
            float x=352,w=Screen.width-x-22;GUI.DrawTexture(new Rect(x,18,w,74),panelTex);
            string location=scene.globeMode?"THE GLOBAL CHART":$"OPEN SEA BOARD  /  {Math.Abs(scene.scenario.latitude):0.00}°{(scene.scenario.latitude>=0?"N":"S")}  {Math.Abs(scene.scenario.longitude):0.00}°{(scene.scenario.longitude>=0?"E":"W")}";
            GUI.Label(new Rect(x+15,25,w-30,20),location,sub);
            GUI.Label(new Rect(x+15,48,w-30,30),scene.globeMode?"Choose a sea. Set the scene.":"Niagara  ×  Stonewall",big);
            if(w>620)GUI.Label(new Rect(Screen.width-315,30,280,45),scene.scenario.Date.ToString("dd MMM yyyy  ·  HH:mm 'UTC'",CultureInfo.InvariantCulture)+
                $"\nTURN {scene.scenario.turn:00}     SUN {scene.SunAltitude:0.0}°",small);
            if(scene.globeMode)GUI.Label(new Rect(x+16,110,w-32,45),"Click the globe to relocate the open-sea board. RMB rotates; wheel zooms.\nModern satellite land cover · geographic sea locations · no local coastal terrain yet.",body);
        }
        void Vessels()
        {
            Heading("THE TWO COMBATANTS");
            for(int i=0;i<2;i++) {GUI.color=i==scene.selected?gold:Color.white;if(Button((i==scene.selected?"●  ":"○  ")+scene.scenario.ships[i].name)) {scene.selected=i;var h=RuleChart.At(scene.scenario.ships[i].Position);col=(h.q+21).ToString();row=(h.r+Mathf.FloorToInt(h.q/2f)+19).ToString();}GUI.color=Color.white;}
            var ship=scene.scenario.ships[scene.selected];
            Text(scene.selected==0?"Wooden screw frigate · three masts\n100.23 m hull × 16.76 m beam":"Armored ram · fixed bow gunroom\n52.38 m reference hull × 9.96 m beam");
            if(Button("Inspect this ship")){chart.topDown=false;scene.Focus(scene.selected);}
            GUILayout.BeginHorizontal();
            if(Button("Bow detail")){chart.topDown=false;scene.InspectDetail(true);}
            if(Button("Gun detail")){chart.topDown=false;scene.InspectDetail(false);}
            GUILayout.EndHorizontal();
            if(Button("Stern detail")){chart.topDown=false;scene.InspectStern();}
            if(Button("Engines & funnel smoke"))ShowTab(6);
            Heading("POSITION & FACING");
            bool hex=GUILayout.Toggle(scene.scenario.hexMode,"Use Ironclads hex placement",toggle);
            if(hex!=scene.scenario.hexMode){scene.scenario.hexMode=hex;if(hex)chart.Snap();}
            Hex at=RuleChart.At(ship.Position);
            Text($"Local hex {at}    ·    {RuleChart.Facings[RuleChart.Facing(ship.heading)]} / {ship.heading:0}°");
            if(hex) {
                GUILayout.BeginHorizontal();if(Button("↶  60°"))chart.Turn(-1);if(Button("60°  ↷"))chart.Turn(1);GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();if(Button("Astern 1 hex"))chart.Move(-1);if(Button("Ahead 1 hex"))chart.Move(1);GUILayout.EndHorizontal();
                GUILayout.Label("Shift + click the sea to place the selected ship.",small);
                GUILayout.BeginHorizontal();col=GUILayout.TextField(col,2,field);row=GUILayout.TextField(row,2,field);
                if(Button("Place")) {
                    if(int.TryParse(col,out int c)&&int.TryParse(row,out int r)&&c>=1&&c<=41&&r>=1&&r<=37) {
                        int q=c-21;chart.Place(new Hex(q,r-19-Mathf.FloorToInt(q/2f)));
                    } else scene.status="Use a local column 01–41 and row 01–37.";
                }GUILayout.EndHorizontal();
            } else {
                ship.heading=Slider("Heading",ship.heading,0,359,"°");
                ship.speed=Slider("Visual speed",ship.speed,0,15," kn");
                GUILayout.Label($"East {ship.east:0} m  ·  North {ship.north:0} m",small);
                GUILayout.BeginHorizontal();east=GUILayout.TextField(east,8,field);north=GUILayout.TextField(north,8,field);
                if(Button("Set m")) {
                    if(float.TryParse(east,NumberStyles.Float,CultureInfo.InvariantCulture,out float e)&&float.TryParse(north,NumberStyles.Float,CultureInfo.InvariantCulture,out float n)&&!float.IsNaN(e)&&!float.IsNaN(n)&&Mathf.Abs(e)<=20000&&Mathf.Abs(n)<=20000) {
                        chart.Remember();ship.east=e;ship.north=n;
                    }else scene.status="Coordinates must be finite meters within ±20,000.";
                }GUILayout.EndHorizontal();
                if(Button("Advance visual movement 60 seconds")){chart.Remember();scene.Step(60);}
            }
            Heading("RANGE BETWEEN SHIPS");
            var a=scene.scenario.ships[0];var b=scene.scenario.ships[1];
            int range=RuleChart.Range(RuleChart.At(a.Position),RuleChart.At(b.Position));
            GUILayout.Label(range+" hexes",big);
            Text($"Straight line: {Vector3.Distance(a.Position,b.Position)/.9144f:0} yd  /  {Vector3.Distance(a.Position,b.Position):0} m");
            Vector3 delta=b.Position-a.Position;float bearing=Mathf.Repeat(Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,360);
            GUILayout.Label($"Niagara → Stonewall bearing {bearing:0}° true",small);
            Heading("TURN & NOTES");
            if(Button($"End turn {scene.scenario.turn}  →  +3 minutes"))chart.NextTurn();
            scene.scenario.umpireNotes=GUILayout.TextArea(scene.scenario.umpireNotes,field,GUILayout.Height(64));
            GUILayout.BeginHorizontal();if(Button("Undo placement"))chart.Undo();if(Button("Log positions"))chart.Export();GUILayout.EndHorizontal();
            SaveLoad();
        }
        void Weather()
        {
            var s=scene.scenario;
            Heading("WEATHER AT THE TABLE");
            GUILayout.BeginHorizontal();
            if(Button("Fair")){s.windKnots=12;s.cloud=.28f;s.rain=0;s.waveHeight=.8f;s.visibilityKm=20;}
            if(Button("Rain")){s.windKnots=20;s.cloud=.92f;s.rain=.65f;s.waveHeight=1.8f;s.visibilityKm=4;}
            if(Button("Gale")){s.windKnots=38;s.cloud=1;s.rain=1;s.waveHeight=4;s.visibilityKm=1.8f;}
            GUILayout.EndHorizontal();
            s.windKnots=Slider("Wind speed",s.windKnots,0,50," kn");
            s.windFrom=Slider("Wind FROM",s.windFrom,0,359,"°");
            s.waveHeight=Slider("Wave scale",s.waveHeight,0,5," m");
            s.cloud=Slider("Cloud cover",s.cloud*100,0,100," %")/100;
            s.rain=Slider("Rain",s.rain*100,0,100," %")/100;
            s.visibilityKm=Slider("Visual visibility",s.visibilityKm,.5f,40," km");
            scene.soundEnabled=GUILayout.Toggle(scene.soundEnabled,"Ocean & wind ambience",toggle);
            GUILayout.Label("Weather is set by the umpire. Wave scale and fog are visual approximations; game visibility is a separate ruling.",small);
            Heading("SUN & SCENARIO TIME");
            date=GUILayout.TextField(date,field);
            if(Button("Apply UTC date / time"))try {
                if(DateTime.TryParse(date,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out var dt)&&dt.Year>=1600&&dt.Year<=9998)
                    {chart.Remember();s.utc=dt.ToString("yyyy-MM-ddTHH:mm:ssZ");scene.status="Sun recalculated for the selected UTC date and sea location.";}
                else scene.status="Enter a UTC date, e.g. 1865-03-24T14:00:00Z (year 1600–9998).";
            }catch(Exception e){Error(e);}
            float hour=(float)s.Date.TimeOfDay.TotalHours;float changed=Slider("UTC hour",hour,0,23.99f," h");
            if(Mathf.Abs(changed-hour)>.002f){s.utc=s.Date.Date.AddHours(changed).ToString("yyyy-MM-ddTHH:mm:ssZ");date=s.utc;}
            Text($"Solar altitude: {scene.SunAltitude:0.0}°\nLatitude {s.latitude:0.0000}° · Longitude {s.longitude:0.0000}°");
            Heading("GEOGRAPHIC LOCATION");
            GUILayout.BeginHorizontal();lat=GUILayout.TextField(lat,10,field);lon=GUILayout.TextField(lon,11,field);GUILayout.EndHorizontal();
            if(Button("Set latitude / longitude")) {
                if(double.TryParse(lat,NumberStyles.Float,CultureInfo.InvariantCulture,out double a)&&double.TryParse(lon,NumberStyles.Float,CultureInfo.InvariantCulture,out double b)&&!double.IsNaN(a)&&!double.IsNaN(b)&&Math.Abs(a)<=85&&Math.Abs(b)<=180)
                    {chart.Remember();s.latitude=a;s.longitude=b;scene.status="Open-sea board relocated; ship spacing preserved.";}
                else scene.status="Use latitude −85 to 85 and longitude −180 to 180.";
            }
            SaveLoad();
        }
        void Chart()
        {
            Heading("THE IRONCLADS MAP STANDARD");
            Text("100 yards across each hex\n3 minutes per game turn\nSix facings, 60° apart");
            chart.showGrid=GUILayout.Toggle(chart.showGrid,"Hex grid",toggle);
            chart.showLabels=GUILayout.Toggle(chart.showLabels,"Local four-digit hex labels",toggle);
            chart.showRange=GUILayout.Toggle(chart.showRange,"Range line between occupied hexes",toggle);
            chart.showArcs=GUILayout.Toggle(chart.showArcs,"Stonewall sheet arc guides",toggle);
            bool top=GUILayout.Toggle(chart.topDown,"Overhead chart view (north up)",toggle);
            if(top!=chart.topDown){chart.topDown=top;if(top){scene.globeMode=false;scene.Overview();scene.distance=Mathf.Max(900,scene.distance);}}
            if(Button("Snap both ships to hexes / facings")){scene.scenario.hexMode=true;chart.Snap();}
            Heading("USING THE MAP");
            Text("Range is the shortest hex path, excluding the firing hex and including the target hex (rules p. 18).");
            Text("Placement and facing controls carry out your orders. You decide movement allowances, turning capacity, current, collisions, line of sight, fire and damage.");
            Text("The local 41 × 37 chart has its own IDs. It does not reproduce printed mapboards I–IV or their shoals and boundaries.");
            Heading("FERROL SCENARIO 4D");
            Text("The supplied rules include the three-ship breakout on 24 March 1865. This scene is your two-ship variant. Sacramento and the original board arrangement are not included.");
            if(Button("Use 4D wind / visibility as visual preset")){scene.scenario.windFrom=0;scene.scenario.visibilityKm=30*RuleChart.HexMeters/1000;scene.status="Wind FROM north; visual fog visibility set to 30 hexes (2.7432 km). Umpire decides actual sighting.";}
            if(Button("Take screenshot  ·  F12"))scene.Screenshot();
        }
        void Engines()
        {
            var exhaust=GetComponent<EngineExhaust>();
            Heading("ENGINES & FUNNEL SMOKE");
            GUILayout.BeginHorizontal();if(Button("Niagara"))scene.selected=0;if(Button("Stonewall"))scene.selected=1;GUILayout.EndHorizontal();
            var ship=scene.scenario.ships[scene.selected];Text(ship.name);
            Text(scene.selected==0?"Two funnels · single screw":"One funnel · twin screws");
            if(Button("Inspect selected ship")){chart.topDown=false;scene.Focus(scene.selected);}
            Heading("VISUAL ENGINE ORDER");
            GUILayout.BeginHorizontal();if(Button("Stopped"))exhaust.SetOrder(scene.selected,0);if(Button("Cold boilers"))exhaust.SetOrder(scene.selected,0,false);GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();if(Button("Slow"))exhaust.SetOrder(scene.selected,.25f);if(Button("Half"))exhaust.SetOrder(scene.selected,.55f);if(Button("Full"))exhaust.SetOrder(scene.selected,1);GUILayout.EndHorizontal();
            bool lit=GUILayout.Toggle(ship.boilersLit,"Boilers lit",toggle);if(lit!=ship.boilersLit)exhaust.SetOrder(scene.selected,ship.engineLoad,lit);
            GUI.enabled=ship.boilersLit;ship.engineLoad=Slider("Visual engine effort",ship.engineLoad,0,1);GUI.enabled=true;
            Text(!ship.boilersLit?"Cold: new smoke stops; the existing plume disperses.":ship.engineLoad<.01f?"Screws stopped, boilers banked: light residual smoke.":$"Screws turning · engine effort {ship.engineLoad*100:0}%");
            Heading("EXHAUST & WIND");
            Text($"Wind FROM {scene.scenario.windFrom:0}° at {scene.scenario.windKnots:0.0} kn\nPlumes drift TOWARD {Mathf.Repeat(scene.scenario.windFrom+180,360):0}°");
            if(Button("Adjust wind / weather"))ShowTab(1);
            if(Button("Clear existing funnel plumes"))exhaust.Clear();
            Text("Fresh exhaust is darker and denser under load. Coal-stoking pulses, billows and eddies vary each funnel's trail. Existing smoke stays over the sea when a ship is repositioned.");
            Text("Engine order is a visual indication, separate from speed and hex placement. It does not move ships, advance time or implement engine rules. Funnel smoke is an approximation of coal firing, not a measurement of screw RPM.");
            SaveLoad();
        }
        void FireBatteries()
        {
            var effects=GetComponent<BatteryEffects>();
            Heading("BATTERY SALVOS");
            GUILayout.BeginHorizontal();if(Button("Niagara"))scene.selected=0;if(Button("Stonewall"))scene.selected=1;GUILayout.EndHorizontal();
            Text(scene.scenario.ships[scene.selected].name);
            if(scene.globeMode)Text("Return to sea to fire a battery.");
            bool wasEnabled=GUI.enabled;GUI.enabled=!scene.globeMode&&!effects.firing[scene.selected];
            if(scene.selected==0) {
                if(Button("Fire PORT · five 150-pdr Parrotts"))effects.Fire(0,-1);
                if(Button("Fire STARBOARD · five 150-pdr Parrotts"))effects.Fire(0,1);
                if(Button("All ten guns · visual display"))effects.Fire(0,2);
            } else {
                if(Button("Fire BOW · 300-pdr Armstrong"))effects.Fire(1,0);
                if(Button("Fire PORT · 70-pdr Armstrong"))effects.Fire(1,-1);
                if(Button("Fire STARBOARD · 70-pdr Armstrong"))effects.Fire(1,1);
                if(Button("All three guns · visual display"))effects.Fire(1,2);
            }
            GUI.enabled=wasEnabled;
            if(effects.firing[scene.selected])GUILayout.Label("Battery firing…",small);
            if(Button("Inspect selected ship")){chart.topDown=false;scene.Focus(scene.selected);}
            Heading("SOUND & SALVO TIMING");
            effects.cannonSound=GUILayout.Toggle(effects.cannonSound,"Cannon reports enabled",toggle);
            effects.volume=Slider("Cannon volume",effects.volume,0,1);
            effects.ripple=Slider("Seconds between guns",effects.ripple,.12f,1.2f," s");
            GUILayout.Label("Layered Parrott and Armstrong reports: sharper crack, heavier body and longer rolling tail, with shot variations. Sound travels from each muzzle to the camera.",small);
            Heading("BLACK POWDER SMOKE");
            effects.smokeAmount=Slider("Smoke density",effects.smokeAmount,.5f,2);
            var wind=BatteryEffects.Wind(scene.scenario.windFrom,scene.scenario.windKnots);
            Text($"Wind FROM {scene.scenario.windFrom:0}° at {scene.scenario.windKnots:0.0} kn\nSmoke drifts TOWARD {Mathf.Repeat(scene.scenario.windFrom+180,360):0}° / {wind.magnitude:0.0} m/s");
            if(Button("Adjust wind / weather"))ShowTab(1);
            if(Button("Clear smoke & stop salvo / sound"))effects.Clear();
            Text("Dark dense pockets mix into pale billows and thin wisps. Gusts, eddies and height vary the drift; existing clouds follow wind changes. Smoke disperses over roughly 20–46 real seconds.");
            Text("These are umpire-triggered effects. Firing does not check loading, range or arcs, advance the turn, or resolve hits or damage. All-guns display can fire in different directions.");
        }
        void Gunnery()
        {
            var guns=GetComponent<GunneryTools>();
            Heading("UMPIRE'S GUNNERY TABLE");
            guns.visible=GUILayout.Toggle(guns.visible,"Show gunnery overlay",toggle);
            GUILayout.BeginHorizontal();
            if(Button("Niagara")){scene.selected=0;guns.preset=3;}
            if(Button("Stonewall")){scene.selected=1;guns.preset=0;}
            GUILayout.EndHorizontal();
            Text("Firing ship: "+scene.scenario.ships[scene.selected].name);
            if(scene.selected==1)for(int i=0;i<3;i++)if(Button((guns.preset==i?"• ":"")+GunneryTools.Presets[i])){guns.preset=i;guns.visible=true;chart.showArcs=false;}
            if(Button("Custom angle guide")){guns.preset=3;guns.visible=true;chart.showArcs=false;}
            if(guns.preset==3) {
                guns.customCenter=Slider("Center from bow",guns.customCenter,-180,180,"°");
                guns.customSweep=Slider("Arc width",guns.customSweep,1,360,"°");
                GUILayout.BeginHorizontal();
                foreach(int angle in new[]{90,120,180,240})if(Button(angle+"°"))guns.customSweep=angle;
                GUILayout.EndHorizontal();
                GUILayout.Label("Custom geometry is a measuring aid. Match it to the gun arrows or pattern card; it is not a verified Niagara broadside template.",small);
            }
            guns.reach=Mathf.Round(Slider("Guide radius",guns.reach,1,12," hex"));
            guns.rangeRings=GUILayout.Toggle(guns.rangeRings,"100-yard radial distance bands",toggle);
            if(Button("Measure to the other ship"))guns.pickedTarget=false;
            GUILayout.Label("Alt + click sea: measure to a hex.\nShift + click: place ship as before.",small);
            if(Button("Overhead gunnery view")){guns.visible=true;chart.topDown=true;scene.globeMode=false;scene.Overview();scene.distance=Mathf.Max(1100,scene.distance);}
            Heading("SIGHTING RULER");
            Text($"Target hex {guns.Target} · {guns.Range} hexes\nStraight distance {Vector3.Distance(guns.Origin,RuleChart.Center(guns.Target))/.9144f:0} yd");
            if(guns.SameHex)Text("Same occupied hex: bearing undefined.");
            else {
                Text($"Bearing {guns.TargetBearing:0.0}° true\nFrom bow {Mathf.DeltaAngle(scene.scenario.ships[scene.selected].heading,guns.TargetBearing):+0.0;-0.0;0}°\nGuide center {guns.AxisBearing:0}° / width {guns.Sweep:0}°");
                Text(guns.SplitHex?"Arc edge crosses target hex: consult the split-hex rule (p. 16).":(GunneryTools.Within(guns.TargetBearing,guns.AxisBearing,guns.Sweep)?"Target center lies within this guide.":"Target center lies outside this guide."));
                var target=scene.scenario.ships[1-scene.selected];
                if(!guns.pickedTarget)Text($"Observer from target bow: {Mathf.Abs(Mathf.DeltaAngle(target.heading,Mathf.Repeat(guns.TargetBearing+180,360))):0.0}°");
            }
            Heading("READING THE GUIDES");
            Text("Orange: arc edges and center. Mint: sighting ruler and target hex. Curved bands are straight distances, not hex-path range.");
            Text("Origins are occupied hex centers. Rule range excludes the firing hex and includes the target. The ruler does not detect blocked line of sight or decide fire eligibility.");
            Text("Niagara uses the large broadside pattern with Areas I / II (pp. 15–16). Separate pattern cards are absent from the supplied files; custom sectors do not reproduce their sectional restrictions.");
            Text("Stonewall: forward 180° and side 120° guides follow its supplied sheet; see also p. 17. Armor stays fixed. Loading, obstructions, eligible guns, raking fire and results remain your rulings.");
        }
        void Sources()
        {
            Heading("YOUR REFERENCE MATERIAL");
            if(Button("View Stonewall's supplied ship sheet"))ShowSheet();
            if(Button("View Niagara's supplied 1863 card"))ShowSheet(true);
            if(Button("Open original Stonewall PDF"))OpenReference("Stonewall.pdf");
            if(Button("Open supplied rules PDF"))OpenReference("NEW_IRONCLADS_RULES.pdf");
            if(Resources.Load<Texture2D>("StonewallSheet")==null)Text("Rulebook and ship-sheet scans are optional user-provided references and are not included in the public release.");
            Text("Stonewall's sheet supplies a 180° forward guide and 120° guides on each side. These are display guides; the umpire decides gun eligibility. The model's armored enclosures are fixed.");
            Heading("RECONSTRUCTION NOTES");
            Text("Procedural meshes use published dimensions, Niagara's 1863 refit photograph, and Stonewall's March 1865 Ferrol photograph. Rigging, hull lines, boats, deck fittings and exact gun mounts are approximate.");
            Text("Niagara's supplied card is labeled 1863 and shows a mixed battery. The 1865 scene currently retains the ten-Parrott working reconstruction; the card is available for the umpire's reference.");
            Text("Pass 4: Niagara has a rounded stern with curved gallery fittings and taffrail; both ships have finer hull, gun and fitting surfaces. Stonewall's bow gunroom remains built into the hull. Niagara's exact offsets remain unverified against the archived Steers lines plan; stern curvature and small fittings are approximate.");
            Text("The globe uses NASA's March 2004 Blue Marble land cover. Sun position uses the NOAA solar approximation. Local ocean is rendered separately at meter scale.");
            if(Button("NHHC historical ship records"))Application.OpenURL("https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/stonewall.html");
            Heading("SESSION");SaveLoad();
            if(Button("Reset to the two-ship setup")){chart.Remember();scene.scenario=new Scenario();chart.Snap();scene.Overview();date=scene.scenario.utc;scene.status="Default what-if setup restored.";}
        }
        void OpenReference(string name)
        {
            string path=System.IO.Path.Combine(Application.streamingAssetsPath,"References",name);
            if(System.IO.File.Exists(path))Application.OpenURL(path);else scene.status="This optional user-supplied reference is not bundled: "+name;
        }
        void SaveLoad()
        {
            GUILayout.BeginHorizontal();
            if(Button("Save table"))try{scene.scenario.Save();scene.status="Saved: "+Scenario.SavePath;}catch(Exception e){Error(e);}
            if(Button("Load table"))try{var loaded=Scenario.Load();chart.Remember();scene.scenario=loaded;date=loaded.utc;lat=loaded.latitude.ToString(CultureInfo.InvariantCulture);lon=loaded.longitude.ToString(CultureInfo.InvariantCulture);scene.Overview();scene.status="Saved table loaded.";}catch(Exception e){Error(e);}
            GUILayout.EndHorizontal();
        }
        void ShipLabels()
        {
            for(int i=0;i<2;i++) {
                var s=scene.scenario.ships[i];Vector3 p=scene.view.WorldToScreenPoint(s.Position+Vector3.up*(i==0?58:34));
                if(p.z<0||p.x<360||p.x>Screen.width-60)continue;
                GUI.Label(new Rect(p.x-70,Screen.height-p.y-10,180,21),s.name,body);
            }
        }
        void SheetWindow()
        {
            float x=350,w=Screen.width-370,h=Screen.height-125;
            GUI.DrawTexture(new Rect(x,100,w,h),panelTex);
            GUILayout.BeginArea(new Rect(x+14,112,w-28,h-24));
            GUILayout.BeginHorizontal();GUILayout.Label(sheetTitle,section);
            GUILayout.FlexibleSpace();if(Button("Close"))sheetOpen=false;GUILayout.EndHorizontal();
            sheetZoom=GUILayout.HorizontalSlider(sheetZoom,.15f,1.3f);
            sheetScroll=GUILayout.BeginScrollView(sheetScroll);
            if(sheet!=null)GUILayout.Label(sheet,GUILayout.Width(sheet.width*sheetZoom),GUILayout.Height(sheet.height*sheetZoom));
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
    }
}
