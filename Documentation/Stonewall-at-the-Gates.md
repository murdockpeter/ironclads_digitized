# Stonewall at the Gates - Yaquinto Ironclads module

A six-ironclad alternate-history encounter in Hampton Roads, 28 March 1865, 14:00 UTC. Local umpire play or six human commanders over direct TCP; version 1.7.0-dev. This is an additional module: Ferrol, the 1862 Hampton Roads duel and Ulsan remain available.

## The divergence

Stonewall completes her European repairs much earlier, departs Europe in February and reaches Virginia during March. A separate successful Confederate sortie gets Virginia II and Fredericksburg through the upper-James obstructions and past the guarding batteries. They rendezvous with Stonewall in the lower river. Union intelligence brings Onondaga, Mahopac and Sangamon into the roadstead to intercept them. The scenario begins after the hypothetical river passage; it does not recreate or assert that such an escape actually occurred.

Historically, Stonewall was still at Ferrol on 24 March and reached Nassau on 6 May. The unsuccessful January sortie at Trent's Reach involved Virginia II, Richmond and Fredericksburg, with Onondaga opposing them. Our substitution of Stonewall for Richmond, accelerated Atlantic arrival and successful river escape are explicit counterfactuals. Niagara is wooden and therefore does not count toward an all-ironclad force; her ten-rifle Ferrol setup is retained separately.

## Forces and modeled batteries

| Side | Ship | Model battery | Model character |
| --- | --- | --- | --- |
| Union | USS Onondaga | Two XV-inch Dahlgrens and two 150-pounder Parrotts; one of each per turret | New twin-turret monitor, independently trainable turrets |
| Union | USS Mahopac | Two XV-inch Dahlgrens | New Canonicus-class monitor with a single turret |
| Union | USS Sangamon | One XV-inch and one XI-inch Dahlgren | New broader Passaic-class single-turret monitor |
| Confederate | CSS Stonewall | One 300-pounder and two 70-pounder Armstrong rifles | Existing detailed integrated-battery ram |
| Confederate | CSS Virginia II | One forward 7-inch Brooke rifle, two broadside 6.4-inch rifles, one aft XI-inch smoothbore | New short angular fixed casemate; supplied Yaquinto card outfit |
| Confederate | CSS Fredericksburg | One forward 8-inch rifle, two broadside 6.4-inch rifles, one aft XI-inch smoothbore | New narrower fixed casemate; provisional DANFS outfit |

Nineteen modeled and functioning guns in total. Cannon reports, powder clouds and wind-driven coal exhaust are available on all six ships. Turret training moves the barrels, muzzles and firing effects. Casemate guns use representative fixed ready-to-fire positions; multiple pivot firing positions and exact card templates remain umpire interpretations.

The supplied `contextonly/ShipSheets/Virginia2.pdf` shows a 7-inch forward rifle. The NHHC Virginia II summary gives an 8-inch rifle: this game follows the supplied card for that ship. Fredericksburg's recorded armament varies by source and date (including 7-inch/10-inch alternatives); the DANFS 8-inch/XI-inch summary is a working visual choice, to reconcile with your card before play. Confederate XI-inch barrels are represented as banded smoothbores, without claiming a verified foundry-specific barrel profile. The card uses a Dahlgren performance table.

Mahopac uses the Canonicus-class overall raft dimensions (235 feet by 43 feet 8 inches); her DANFS entry's 190-foot/37-foot-8-inch figures differ. Onondaga uses the 226-foot length and 49-foot-3-inch breadth in DANFS. These are dimension-controlled visual reconstructions, not traced original hull plans. Fine fittings and armor seams are representative. Mahopac's pale turret band is an identification aid, not a verified individual marking. Monitors use dark iron paint; Confederate casemates use muted gray iron. Sources do not establish exact paint shades.

## Playtest setup

- Board: 82 columns by 74 rows; adjacent centers 100 yards apart. Three-minute turns.
- Opening: two three-ship lines, corresponding opponents 20 hexes / 2,000 yards apart. Ships start stationary with reciprocal headings; deploy from written orders using the normal hex controls. Use the models' center hex for occupancy and range.
- Suggested duration: 20 turns / one hour. This is a proposed scenario limit, not a quoted published scenario.
- Suggested Confederate objective: bring Stonewall and at least one companion off the eastern chart edge in a condition eligible to escape under your agreed damage rules.
- Suggested Union objective: prevent that breakout within 20 turns.
- Agree precise victory/draw criteria, firing templates, ammunition, ship ratings, engine capacity, turning limits and any handicaps from your Yaquinto cards before play. Numerical game ratings are not inferred from naval dimensions or visual guns. No automatic combat, damage, movement legality, loading or victory evaluation is applied.
- The existing modern-data Hampton Roads shore and fort reconstruction provides visual scenery. No depth survey or historical 1865 battery reconstruction is claimed. Declare navigable hexes, shoals, shore restrictions, fort activity and any currents before beginning; the game does not enforce grounding or fort fire. Existing 1862 support ships and wrecks are not deployed in this module.
- This scenario has not yet been balanced through human play. The Union's large smoothbores and the Confederate ram/pivot batteries make a different tactical problem from Ferrol; parity of ship count is not proof of equal strength.

## Controls and launching

```powershell
.\Start-Ironclads.ps1 -James1865
```

Executable: `Builds/Windows-Multiplayer/Ironclads.exe -james-1865`.

From another module, choose Scenarios and then **Stonewall at the Gates / 3 vs 3 (1865)**. Starting a scenario replaces the current table, so use Save table first if needed. All six hulls can be selected and inspected. Shift-click sea or enter a column/row to place the selected ship. Ahead/astern and port/starboard controls operate on that ship. End Turn changes time and retains all six positions. Undo and CSV logging cover the entire table. Save/load includes all six ships, weather, notes, heading and both Onondaga turret angles.

Gunnery permits any selected ship to measure any target ship or an Alt-clicked hex. Custom arc guides remain measuring aids; match them to the printed firing template. Fire buttons emit only the selected battery. The local sea-sound preference and separate cannon-volume controls are retained. For network play, open **Multiplayer**, host or join the internal TCP lobby, request ships, and have the host approve assignments. Each participant marks ready before the host starts the match. The lobby remains available for host-approved control transfers during play. One captain can command several ships; a separate host can act as umpire. All participants need protocol 7. See [multiplayer instructions](MULTIPLAYER.md).

## Sources and research limits

- [NHHC Stonewall](https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/stonewall.html): actual departure/arrival sequence and three-gun armament.
- [NHHC Onondaga](https://www.history.navy.mil/research/histories/ship-histories/danfs/o/onondaga-i.html): double turrets, battery, dimensions and James River service.
- [NHHC Mahopac](https://www.history.navy.mil/research/histories/ship-histories/danfs/m/mahopac-i.html): Canonicus class, twin XV-inch battery and March return to Chesapeake Bay.
- [NHHC Sangamon](https://www.history.navy.mil/research/histories/ship-histories/danfs/s/sangamon-i.html): Passaic class, mixed XV/XI battery and James River service.
- [NHHC Virginia II](https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/virginia-ii-ironclad-steam-sloop.html): shortened casemate, dimensions, armament and failed sortie.
- NHHC/DANFS indexed Fredericksburg summary (retrieved 12 November 2026): 188 feet, 40 feet 3 inches, 5 knots and XI/8/two 6.4-inch guns. The full record could not be fetched. The [archived Naval Historical Center Fredericksburg page](https://www.ibiblio.org/hyperwar/OnlineLibrary/photos/sh-us-cs/csa-sh/csash-ag/fredrkbg.htm) separately supports the 188-foot ironclad identity and James River service.
- [NHHC photograph 111-B-1961](https://www.history.navy.mil/content/history/nhhc/our-collections/photography/numerical-list-of-images/nara-series/misc-series/111-B-1961.html): period James River fleet; the caption identifies Sangamon only tentatively.
- Supplied Yaquinto rules and Virginia II sheet inspected locally. Books/cards remain private and ignored by Git. Only Stonewall and Virginia II cards from this force are currently supplied locally; use your own cards for the remaining four ships.

Research performed 12 November 2026. NHHC page fetching was intermittent; indexed DANFS text was available. No new archival lines plans were obtained. The five additions are a first modeling pass and can be refined against further drawings and photographs.

## Validation

Editor and runtime reports, model counts and review images are in `Artifacts/James1865/`. Checks cover fleet creation, finite geometry, battery counts, opening hexes on visual water, placement/undo for every ship, manual turn handling, independent twin-turret aiming, six-ship save parsing, firing all 19 guns, exhaust attachment/emission, target selection and return to the older modules. These are functional checks, not a historical-accuracy or play-balance certification.
