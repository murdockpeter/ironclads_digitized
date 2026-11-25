# Pantelleria — 1901, hull contact and seas (1.10.12-dev)

Historical hull replacement is tracked in the [research TODO](Pantelleria-Historical-Hulls-TODO.md) and [per-ship reference tracker](Pantelleria-Hull-Research.csv). The current 24 model variants are implementation groups to verify against drawings and individual ship configurations.

Launch the additional module with:

```powershell
.\Start-Ironclads.ps1 -Pantelleria
```

Latest player: `Builds/Windows-Pantelleria-Seas/Ironclads.exe -pantelleria`. The [hull-contact and seas pass](Pantelleria-Seas.md) adds wave-driven hull motion, crest foam and droplets, finer near water and displacement wash. The [individual weathering pass](Pantelleria-Weathering.md) remains. The [fourth detail pass](Pantelleria-Detail4.md), [close-up fittings pass](Pantelleria-Fittings.md), [second visual polish](Pantelleria-Polish2.md), [dressing lines and yardarm hoists](Pantelleria-Rigging-Flags.md), [gun-mount repair](Pantelleria-Gun-Mounts.md), [third detail pass](Pantelleria-Detail3.md) and animated national colours remain. The scenario is available from Scenarios and the Ulsan/Close Action module controls in this build. All existing modules, including the private Breakout campaign desk, remain available.

## Scenario and opening

This is the **fictional Battle of Pantelleria**, *In Mahan's Wake*, printed pp.70–71 / PDF pp.71–72. It is not a claim that this fleet action historically occurred. The supplied private book stays in `contextonly`; it is not bundled with the player.

| Alliance | National force | Ships | Battle divisions |
| --- | --- | ---: | ---: |
| Britain / Italy | Britain | 13 | 5 |
| Britain / Italy | Italy | 8 | 3 |
| France / Russia | France | 13 | 4 |
| France / Russia | Russia | 8 | 2 |
| Total | Four commands | 42 | 14 |

All 42 named vessels from the scenario are included. The [scenario selection brief](Pantelleria-Module-Proposal.md) records the roster and source setup.

The opening clock is **14 February 1901, 1430 scenario time**. UTC+1 is an explicit display/sunlight assumption, giving 1330 UTC. An approximate Pantelleria reference of 36.78° N, 11.99° E anchors the printed Point X, 20 nautical miles on bearing 320° from the island reference. The source introduction loosely says north; the module follows the explicit setup's northwest bearing.

| Fleet | Leading ship bears from X | Leading ship range | Course | Speed |
| --- | ---: | ---: | ---: | ---: |
| British | 290° | 32,000 yd | 105° | 15 kn |
| Italian | 100° | 32,000 yd | 282° | 15 kn |
| French | 345° | 32,000 yd | 180° | 15 kn |
| Russian | 200° | 32,000 yd | 000° | 15 kn |

The book permits players to choose formations subject to signaling constraints. The module supplies **editable division columns**: ships 500 yd apart, division lanes 800 yd apart, oriented to each national course. These are implementation presets, not a prescribed book formation or a signaling-legality check. The first national division leader follows the printed reference exactly; other divisions occupy the preset lanes. Redeploy before play as agreed.

Printed weather is wind **from 260° at 20 knots, sea state 5, visibility 60%**. The display uses a provisional 2.1 m wave scale. Render fog and wave amplitude are visual settings, distinct from rulebook visibility and sea-state effects. Overview fog is reduced locally to keep distant fleet markers readable; this is a shared table without hidden detection.

**Source discrepancy:** the scenario starts all ships at 15 kn, while the annex gives Terrible a 14-kn reference speed. The module preserves the printed 15-kn opening and exposes the reference speed for umpire review. It does not silently rewrite the scenario or enforce reference maximums.

## Selecting ships and issuing orders

Select a national command, then a battle division, then a ship. The main view also provides selectable ship markers. The All 42 ships button frames the fleets; Selected division frames that formation; Inspect selected ship moves close to its model. WASD/middle mouse pan, right mouse orbits, and the wheel zooms. Shift-click places the selected ship as an umpire override.

Fleet / orders provides:

- A division's plotted course and speed, stored separately from current effective course/speed.
- Explicit application of plotted division orders as an umpire override, without displacement.
- Straight division advances of 30 or 60 seconds using each ship's effective speed and remaining turn allowance.
- A host/umpire text record of division orders.

Group actions require control of every afloat ship in the selected division. A group advance is rejected before any change if one member lacks allowance or would leave the board. Sunk vessels are omitted from group actions. Division moves do not automatically preserve a formation when ships have different effective speeds.

Ship movement retains the DoB measured-movement tools: effective/plotted speed, exact course/placement, remaining allowance, and advance-then-pivot bridge turns. The selected ships use the supplied B/Medium movement aid (300 yd standard / 200 yd hard-rudder advance, up to 45°). The modeled bridge pivot is an illustrative point at 22% of hull length forward of the center. Weather, damage, special ship restrictions, acceleration, turn losses and movement legality remain umpire decisions. Applying a plotted course is a setup/order override, not a legal turning maneuver.

Tactical turns are three minutes; intermediate turns are thirty minutes. End turn advances the clock, clears moved time and retains positions. Undo covers committed table actions and group orders/moves; continuously edited text and damage sliders are saved but do not each create an undo snapshot.

## Models and gunnery

**The third detail upgrade refines the full fleet again.** See the [third-pass guide](Pantelleria-Detail3.md) for current model counts, planked decks, 192-station hulls, refined gunhouses, boats, mast and deck fittings. It builds on the second pass's rounded gunhouses, finer funnels/barrels/railings, breech jackets, ventilators, bridge ladders, searchlights, anchors and hull scuttles. Hull outlines, dimensions and fittings remain approximate visual reconstructions. Major battery layouts distinguish end turrets/barbettes, diamond arrangements, echelon mounts, forward-only main batteries and Russian three-barbette arrangements.

The original [pass-one baseline](Pantelleria-Pass1-Baseline.csv) is preserved. [Detail-pass comparison](Pantelleria-Detail1-Comparison.csv) records actual post-combination mesh and vertex counts for every ship. The extra mesh sections preserve normals and independently trainable turret assemblies. The new fittings do not add fictional guns or change existing muzzle groups, movement, saves or network protocol.

The first upgrade's measured per-ship averages were **2.23× vertices and 2.08× render meshes** over the original models. Individual vertex ratios ranged from 1.97× to 2.44×; mesh ratios ranged from 1.98× to 2.16×. Smaller ships receive fewer rail sections to keep the detail increase proportional. These counts describe geometry, not a guarantee of unchanged frame rate.

The second upgrade is measured against the [retained first-detail baseline](Pantelleria-Detail1-Baseline.csv), rather than the original simple models. [Second-detail comparison](Pantelleria-Detail2-Comparison.csv) records each ship's new counts and ratios: averages **2.25× vertices and 1.98× render meshes** over detail pass 1. Individual vertex ratios are 2.15–2.31×; mesh ratios are 1.86–2.10×. Gun counts, muzzle groups, trainable assemblies, ship dimensions, table rules and protocol remain unchanged. Additional fittings represent plausible period equipment; exact class/refit placements remain provisional.

Latest build: `Builds/Windows-Pantelleria-Seas/Ironclads.exe -pantelleria`, version 1.10.12-dev. The launcher prefers this build when present. All ships retain [animated national ensigns and commissioning pennants](Pantelleria-Flags.md), with wind/ship-velocity response and animation while the game clock is paused. Extra dressing/hoist flags are a local decorative display, not an encoded command. Earlier players remain available and can stay open; save their table before closing and load that same Pantelleria slot in the new player. These players share the existing save location.

Ocean normals now filter wave frequencies smaller than a screen pixel, and camera depth precision increases with overview distance to reduce striped water at fleet scale.

Main/representative-secondary muzzles and six representative light guns per ship support visual fire and powder smoke. Main calibers and major gun counts follow local annex research, including Terrible's post-1898 340 mm battery. Secondary positions, twin secondary turrets, barbette hoods, light batteries and mixed secondary calibers are simplified. Torpedo equipment is not modeled. Some class-based speed references differ between sister ships/refits; use the private annexes to adjudicate exact configurations.

Trainable end batteries follow saved forward/after bearings and retain their muzzle bindings. Wing and echelon batteries have fixed representative firing bearings. The gunnery tab filters targets by national command and division, measures any ship-to-ship range, and provides custom sectors, broadside guides, and visual battery salvos. These guides do not validate individual weapon arcs, cross-deck obstructions or firing eligibility.

Paint, funnel colors, pennants, hull offsets and fittings are provisional identification/visual choices rather than verified 1901 reconstructions. The hull lengths and beams are first-pass modeling estimates, not physical dimensions asserted by the rulebook. Gun reports are generic existing heavy/medium/light effects, not calibrated recordings for each caliber.

The measured [pass-one baseline](Pantelleria-Pass1-Baseline.csv) records original vertices, render meshes, guns and funnel outlets for every ship. A second copy is in `Artifacts/Pantelleria/pass-1-model-baseline.csv`. Both upgrade comparisons and the first-detail baseline are retained, so the two successive approximate doublings can be reviewed separately.

## Geographic setting

The module reuses the repository's cached **GSHHG 2.3.7 full-resolution shoreline** for Pantelleria and surrounding Mediterranean land. The crop covers 11.0–12.9° E and 36.45–37.7° N. It is geographically positioned relative to the scenario anchor. **Land uses a flat two-meter display platform**: no island elevation dataset or depth survey is claimed. Modern shorelines, historical channel interpretation, grounding and coast-fire rules remain separate.

Rebuild geographic outlines with:

```powershell
python Tools/build-pantelleria-coast.py
node Tools/triangulate-pantelleria-coast.cjs
```

The generated resource is `Assets/Resources/Geography/PantelleriaCoast.json`. Derived source polygons, a notice and the existing LGPL/GPL texts are in `Assets/StreamingAssets/GeographySource`. GSHHG-derived data remain LGPL-3.0-or-later, separately from code. No geographic service is contacted during play.

## Results and saves

Results / save records adjudicated damage percentages and sunk status. It summarizes sunk ships and afloat ships at least 50% damaged for each national force. Sunk ships are hidden and blocked from movement and visual firing; clearing the sunk flag restores their saved position. Other damage consequences are not automatically applied.

The ledger does **not** determine a winner. Use the book's separate national victory conditions; agree how sunk/crippled totals and zero denominators are treated. No unprinted alliance score or automatic damage calculation has been added.

Pantelleria saves to **`pantelleria-1901.json`** under the app's persistent-data directory. It has a separate slot from the existing shared tactical save and from campaign saves. Save/load includes all 42 positions, effective/plotted courses and speeds, moved time, engine settings, turret bearings, clock/weather, fourteen division-order texts and the result ledger.

Combat, spotting, loading, ammunition, torpedoes, criticals, damage, signaling and victory remain umpire-run. Visual salvos are not automated DoB combat.

## Multiplayer

Use matching **protocol 10** players. The lobby supports up to **32 participants including host**, with either a separate umpire or a host commanding ships. Alliance preferences are Britain/Italy and France/Russia; ships remain individually assigned, allowing division or national commands to hold multiple vessels. Host-approved cross-side transfers remain possible under the existing lobby model.

The underlying transport's former eight-peer cap has been raised to 31 guests. Assignments, reconnect tokens, ownership epochs, stale-order rejection, exact positions, orders, turrets, salvos and scenario switching use the existing authenticated lobby workflow. Division text records and adjudicated results are host/umpire fields. See [connection instructions](MULTIPLAYER.md).

## Build and verification

Unity 6000.2.12f1:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaSeas -logFile Artifacts/Pantelleria/build-seas-finish.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Seas -OutputDirectory Artifacts/Pantelleria/Seas
```

Editor checks cover national counts, all opening bearings/ranges, every model/battery/funnel, detail budgets against preserved mesh/vertex baselines, 42-ship persistence, result/orders serialization, final-slot protocol bounds and existing-module parsing. The original delivery also passed ten simultaneous loopback peers and three-player live lobby verification; the detail build retains protocol 10.

Standalone checks move all 42 ships and fourteen divisions; verify plot/apply behavior, atomic group rejection, clock separation, bridge turning, save/load and sunk-state controls; fire samples from all four nations; verify every funnel; capture fleet/ship/gunnery/ledger views; and switch through Ulsan, Stonewall and Ferrol. Real multi-process lobby verification exercises assignments, readiness, commands, last-slot orders/salvos, control transfers, reconnect and scenario switching. Reports and screenshots are in `Artifacts/Pantelleria` and `Artifacts/Lobby`.
