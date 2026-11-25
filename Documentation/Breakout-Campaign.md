# Hampton Roads — Breakout! campaign desk

Launch the private umpire campaign with:

```powershell
.\Start-Ironclads.ps1 -Campaign
```

The current Windows player is `Builds/Windows-CampaignGeography/Ironclads.exe -breakout-campaign` (1.9.2-dev). The campaign desk is also available from the Stonewall sidebar and the Scenarios panel in this build. The launcher prefers this build when there is no root-level executable.

This implements the supplied **22 December 1864, 1800** command problems. It is separate from the **28 March 1865** Stonewall at the Gates encounter. Stonewall, Mahopac and Sangamon are not silently added to the December roster, and the March module's fixed gate, 60-turn limit and one-ironclad escape rule do not apply. Establish your own viable-force and Atlantic-arrival criteria under Clock.

## 3D operational board

The map uses the repository's cached **GSHHG full-resolution coastline**, supplemented with **USGS NHD river-area polygons** and a **USGS 3DEP elevation crop**. Coverage extends from the upper James through Hampton Roads and the Virginia Capes into the Atlantic, across 36.75–37.65° N and 77.60–75.80° W. Banks, islands and terrain follow geographic source data rather than a drawn river ribbon. See the [geographic data and rebuild guide](Campaign-Geography.md).

The eight named stations are approximate geographic viewing/placement presets. They do not mark surveyed historical obstructions or initial deployments. The basemap is modern: later channel cuts, dredging and reclamation may appear. It is **not an 1864 shoreline reconstruction or a bathymetric/depth chart**. Navigability and successful escape remain umpire rulings.

All 19 vessels have selectable 3D markers. Ironclads have generic casemates; Onondaga has a generic double-turret marker; other vessels use generic supporting-ship markers. These are campaign tokens, not new historical ship reconstructions. Unassigned vessels appear in side-colored staging boxes, which do not imply deployed positions or intelligence known to either side.

- Click a ship to select it. The Vessels tab contains its actual status and commander assignment.
- WASD pans relative to the view; hold Shift for faster movement. Keyboard panning pauses while typing in a field; click the map to resume. Right mouse drag orbits, middle mouse drag pans, and the wheel zooms. Use Overview, Inspect ship, North up and Top / perspective as needed. Clock includes buttons to zoom to each river sector; these are useful for seeing the narrow upper James.
- Under Vessels, choose a named station, enter an initial setup note or movement ruling, set a heading in degrees true, and apply the station change. Alternatively, enter decimal latitude and signed longitude, or arm point placement and click the 3D board. Exact placements use geographic coordinates; the station label supplies operational context.
- The camera scale bar shows projected metric distance at the view center. Alt-click the map to measure projected straight-line range from an assigned ship in nautical miles and yards. This is not river-route mileage. Ship markers enlarge when zoomed out for selection; their drawn hull sizes are not a geographic measuring tool.
- In preparation these are initial deployment assignments. During live play they are adjudicated movements recorded in the private ledger. The board does not move ships when the clock advances or an order resolves.

The board is always the umpire's actual-state view. **Do not screen-share this window as a player's intelligence view.** Export dispatches to distribute information.

## Before live play

The initial roster follows the supplied briefings:

| Command | Vessels |
| --- | --- |
| Confederate principal force | Virginia II, Richmond, Fredericksburg, Hampton, Nansemond, Drewry |
| Confederate support | Beaufort, Torpedo, Scorpion |
| Union armored ship | Onondaga |
| Union support | Hunchback, Eutaw, Daylight, Miami, Mendota, Commodore Perry, Western World, Henry Brinker, Commodore Morris |

Readiness, supplies, mechanical condition and station assignments start as **not supplied**, rather than invented historical ratings. Add the values provided by the umpire. Supporting shore facilities, boats, pilots, Army batteries, pickets, signal stations and tugs can be represented through command plans, reports and timed events; they are not automatically functioning 3D units.

1. Assign actual vessel readiness, supplies, condition, initial stations and commanders. Responsibilities can describe up to six commands per side; a commander can control several vessels. There are no player accounts in this private desk.
2. Enter the umpire's environment and adjudication criteria under Clock. These fields remain private. To tell players about conditions, issue reports or update their briefing.
3. Record each side's intent, responsibilities, standing orders, route/formation, communications, contingencies and intelligence requirements under Plans. These fields are included in that side's dispatches.
4. Submit preparatory intelligence requests under Orders and mark them as reconnaissance requests. Deliver and resolve those that can be answered from existing information. This is an information-request workflow; do not use it to execute scouting missions before live play.
5. Operational orders may be drafted and queued in preparation, but cannot be delivered or resolved. The clock cannot advance until Begin live play is confirmed. Once begun, the phase does not return to preparation.

## Running the live KS

The campaign clock displays **scenario time** without imposing a modern time-zone conversion. It starts at 1800 on 22 December 1864. It is independent of the tactical table's clock and weather.

Use 15-minute advances for routine operations. Any 1–180 minute increment is available for contact, combat or other significant events. The clock stops at the earliest pending order delivery, report delivery or timed event inside the requested interval. Due items must be handled before the next advance:

- **Orders:** sender, recipient, side, submitted time, delivery delay and free-text instructions. Deliver a due order, then resolve it with a private umpire ruling when appropriate. Delivered orders can remain unresolved while time continues. Cancel an order to close it without execution. Nothing automatically executes its instructions.
- **Reports:** side, recipient, claimed source, observation time, player-visible assessment, report text and delivery delay. Release is explicit and available only when due. Held reports do not block time; unhold to make them eligible again. The actual release time is recorded. Correct a released report with a new report.
- **Events:** private reminders for contacts, tide reviews, navigation or mechanical checks, reinforcement decisions, or other rulings. Add adjudication notes and mark the due event complete. Then apply any consequences to ships and issue appropriate reports yourself.

A typical cycle is: collect orders, deliver them with communication delays, advance to the next decision point, adjudicate actual effects, update the 3D markers and ship conditions, write partial or uncertain reports, release them at their proper times, then export dispatches. Communications may be delayed, held, incomplete or deliberately misleading; the umpire writes what players receive.

Damage, combat, grounding, obstructions, detection, movement rates, fuel consumption, currents, reinforcement availability, fort fire and victory remain freely adjudicated. No random outcome tables or unverified ship ratings are imposed. The private chronology records committed actions and rulings; individual free-text edits are saved but do not each produce a chronology entry.

## Dispatches and secrecy

Files exports a `.txt` packet for either side. A full side command packet contains:

- That side's player briefing and recorded command plans.
- Only that side's explicitly released reports, with source, recipient, assessment, observation time and actual receipt time.

An optional exact recipient name filters reports to that command. This is a case-insensitive exact match, not a distribution-list or hierarchical permissions system. Plans remain side-wide in recipient packets. Blank recipient means all released reports for the selected side, suitable for a side commander. Recipient file names use timestamps rather than command names.

Actual vessel positions, readiness, supplies, condition, tasks and private notes are **not automatically exported**. Neither are queued orders, private rulings, the other side's plans/reports, unreleased intelligence, events, environment notes, outcome assessment or the private chronology. Supply known readiness and station information through explicit reports. Review each packet and distribute it through your event's existing communication channels. Exports are files only; the app does not send messages to players.

## Saves

Campaign files and dispatches are in `Application.persistentDataPath/BreakoutCampaigns`. Files includes an Open folder button. On Windows, the usual location is:

```text
%USERPROFILE%\AppData\LocalLow\Ironclads\Ironclads — Ferrol\BreakoutCampaigns
```

The app's configured product name determines the exact folder; use the button to find it.

Each committed action and a 20-second autosave write the **active slot**. Free-text edits are also captured by autosave. Typing a different file-slot name does not redirect autosave. Save to named slot activates that slot; overwriting another existing slot requires a confirmation inside the app. Load validates the incoming file before replacing the desk and preserves the outgoing active state. Starting a new campaign saves the old slot and creates a timestamped slot. The last selected slot resumes next launch.

Saves use a temporary file and replacement, retaining the previous version as `.json.bak`. To recover a backup, copy it to a new `.json` slot in the folder, then load that slot. Keep additional copies before a live event. Campaign saves are independent of tactical table saves and include the roster, placements, plans, orders, report release state, events, clock, phase and chronology.

Version 1 schematic saves load into schema 2 automatically. Plans, readiness, orders, reports, phase and time are retained. Assigned ships move to their approximate named geographic station anchor, because schematic offsets cannot be converted into real positions. Original schematic coordinates are retained in each ship's private notes and the chronology records the migration. The original save is archived as `.json.schematic-v1` before the first migrated save; this archive is not rotated by autosave. Unassigned ships stay in off-map staging. The desk displays an upgrade notice. Review any migrated deployment before play; subsequent saves use geographic meter coordinates.

Return to tactical table preserves the table that was open beneath the desk. This is a manual handoff: the December roster, contact positions and damage are not automatically converted into the March six-ship tactical encounter. Open campaign again to resume the operational desk. Disconnect any tactical multiplayer session before entering the private campaign desk.

## Build and validation

Unity 6000.2.12f1:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverBreakoutGeography -logFile Artifacts/CampaignGeography/build.log
```

The build runs `BreakoutCampaignReview.Run`: preparation controls, exact due-time boundaries, held and released reports, export secrecy, recipient isolation, malformed-save rejection, schema migration and disk save/backup checks. Its result is written to `Artifacts/CampaignVerification/editor-verification.txt`. `CampaignGeographyReview.Run` also checks station water placement, coordinate conversion, metric scale, terrain loading and upward surface normals.

Run `./Tools/Verify-Campaign.ps1` for standalone verification. It renders an off-screen window without taking focus, captures the overview, selected ship and all eight tabs, checks all 19 markers, marker placement, clock/save behavior, tactical-state isolation, visible rendering and camera restoration. It writes `runtime-verification.txt`, example dispatches and screenshots to `Artifacts/CampaignVerification/Runtime`. Verification uses temporary campaign state and does not overwrite the umpire's campaign save. Direct `-verify-campaign <output-folder>` is also supported, but a fully hidden Unity window produces black screenshots and fails the rendering check.
