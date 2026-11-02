# Battle of Ulsan module — 1.3.0-dev

Run `./Start-Ironclads.ps1 -Ulsan`, or run `Builds/Windows-Ulsan-BowFix/Ironclads.exe -ulsan`. The Modules tab switches to Ferrol or Hampton Roads; their scenario panel also opens Ulsan. The default launcher still opens Hampton Roads.

The module follows the base Ulsan scenario in the supplied **Dawn of the Rising Sun**, pp.49–51, with **Dawn of the Battleship v1.1 (17 February 2023)** movement conventions and the supplied Rising Sun annexes. Combat remains umpire-run. Private books, extracted texts and scanned ship records are excluded from distributable assets and Git.

## Opening table

14 August 1904, 05:20 scenario time, off Korea in the Sea of Japan:

| Fleet | Column, leader first | Course | Speed | Spacing |
| --- | --- | --- | --- | --- |
| Russia | Rossia, Gromoboi, Rurik (1892) | 090° | 10 knots | 400 yards |
| Japan | Izumo, Azuma, Tokiwa, Iwate | 125° | 10 knots | 500 yards |

Rossia bears 195° from Izumo at **8,500 yards**. Conditions are sea state 2, clear, 90% rulebook visibility, wind from 315° at 6 knots. Optional distant protected-cruiser reinforcements are not deployed. This is an open-sea module; no Ulsan coastal terrain has been added.

The geographic anchor (35.5°N, 130°E) and UTC+9 conversion are explicit approximations. They place the sun using the existing solar calculation, without claiming exact historical shipboard time. At the opening hour the sun is below the horizon. A labeled, local **Low-light viewing aid** brightens the environment while retaining the sun's actual direction and intensity; disable it for the natural dawn view. Rulebook percentage visibility and render fog are separate controls.

## Movement and measuring

Select a ship in **Ships / move**. Set its current course and effective speed, then move 30 seconds, 60 seconds, or its remaining allowance. Plotted speed records the next order; the umpire decides when to apply it. Tactical turns last three minutes; intermediate turns last 30 minutes. Ending a turn advances the clock and clears movement time used, leaving positions and effective speeds intact.

DoB turns use **advance, then pivot at the bridge**, rather than an Iron & Fire turning circle. All seven cruisers are size B / Medium in the supplied annex. The standard aid advances 300 yards before a pivot up to 45°; hard rudder advances 200 yards. The advance consumes movement time. The pivot preserves the modeled bridge position. Standard/hard 45° speed losses (3/4 knots), acceleration, recovery, damage restrictions, small-course exceptions and rudder-jam eligibility remain manual. The course slider and exact placement are setup/umpire overrides, not legal-move checks.

Distance uses exact nautical units: 1 knot = 1,852 meters/hour. The book's convenient rounded yards-per-three-minutes shortcut differs slightly. Gunnery rulers show yards, thousands of yards, nautical miles, relative bearings and tabletop inches at 250 yards/inch. Custom sectors are measuring aids; they do not validate individual weapons' arcs. Loading, firing eligibility, ammunition, spotting, torpedoes, hits, damage and victory remain with the umpire.

## Ships and effects

Seven new procedural cruiser reconstructions include main and secondary batteries, hull casemates, bridge fittings, masts, boats and the appropriate number of funnels. Japanese cruisers have separately trained forward/after twin 8-inch gunhouses. Russian cruisers use four single broadside 8-inch mounts; Rossia and Gromoboi also have modeled bow/stern six-inch guns. Rurik is the earlier ship lost at Ulsan, not the later armored cruiser of the same name.

These are first-pass reconstructions, approximately 48,000–58,000 vertices per ship. Dimensions and battery layouts use the annexes and historical references; hull offsets, fittings and paint are approximate. Light guns are representative, particularly on Rurik; not every small weapon or torpedo fitting is reproduced. Salvos originate at individual muzzles with sound and wind-driven powder smoke. Twenty-one funnel emitters provide coal exhaust, controlled by each ship's engines/boilers.

Historical dimensional cross-checks include [Jane's contemporary Russian navy survey](https://visit-saint-petersburg.ru/data/documents/THE-IMPERIAL-RUSSIAN-NAVY.-ITS-PAST-PRESENT-AND-FUTURE-BY-FRED.-T.-JANE.pdf) and the [US Office of Naval Intelligence's 1893 survey](https://upload.wikimedia.org/wikipedia/commons/8/8c/General_Information_Series_%28United_States._Office_of_Naval_Intelligence%29_No._XII%2C_1893_%28IA_generalinformati3530unit%29.pdf).

## Fleet TCP play and saves

Open **TCP fleets**. The host chooses Russia or Japan; the guest commands the complementary fleet. All three Russian or four Japanese ships can be moved and fired by their owner. The host controls clock, weather, shared notes, reset and load. Both players need a **protocol 5** build. Cameras, ruler settings and the low-light aid are local. This is a shared table: hidden orders, detection and fog of war are not implemented. Existing [direct-IP connection instructions](MULTIPLAYER.md) apply.

Save/Load includes all seven ships, courses, speeds, movement time, turret bearings and environment. It uses the existing shared `ferrol-scenario.json` save slot, so save another copy before replacing a table you wish to retain.

## Build and verification

Build with `Ironclads.Editor.ProjectBuilder.DeliverUlsan` in Unity 6000.2.12f1. Output is `Builds/Windows-Ulsan`. Editor checks and previews go to `Artifacts`; measured counts are in `Artifacts/ulsan-model-counts.csv`.

```powershell
& .\.tools\verify-player.ps1 -BuildDirectory Builds/Windows-Ulsan -Ulsan
& .\.tools\verify-network.ps1 -BuildDirectory Builds/Windows-Ulsan -Ulsan
```

The two-process network test moves all four Japanese ships, checks ownership rejection, both turret angles, synchronized salvos and turn clocks. Existing Ferrol and Hampton verification helpers remain available with this build.
