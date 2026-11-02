# CSS Patrick Henry — Hampton Roads, 1.4.0-dev

Patrick Henry joins **Virginia's Confederate side**, as confirmed by the user. Monitor and grounded Minnesota remain Union ships; Congress and Cumberland remain wreck scenery. This is a supporting ship addition, not a new battle date or automated rules engine.

Run `./Start-Ironclads.ps1 -PatrickHenry` to open Hampton Roads focused on her model and controls. Add `-IronFire` for continuous movement. The latest executable is `Builds/Windows-PatrickHenry/Ironclads.exe`. The regular launcher opens Hampton Roads; `-Ulsan` and `-Ferrol` still open those modules with the same build.

## At the table

Open **Confederate support / Patrick Henry** from the left sidebar. The panel provides:

- Ship inspection and a gunnery ruler to Monitor, with the existing adjustable sector/point measuring tools.
- Hex placement, one-hex ahead/astern moves, and 60-degree facing changes in Yaquinto mode.
- Current course/speed, next-turn speed and astern orders, exact coordinates and measured straight/curved segments in Iron & Fire mode. One tabletop inch equals 100 yards. Class 3 is an editable working turning-aid default, not a verified Patrick Henry ship record. The reference maximum is 12 knots.
- Bow, stern, port/starboard and all-guns visual salvos, with muzzle reports, flashes and wind-driven powder smoke.
- Independent boilers/engine effort, animated paddle wheels and one attached coal-smoke emitter. Engine animation alone does not move the ship.

Ending an Iron & Fire turn applies her plotted speed/astern order and clears the movement ruler, retaining her position. Placement and movements participate in global undo. Saves include her position, heading, movement/order state and engine settings; older tables initialize her supporting state. Opening coordinates are staged for play, not a reconstruction of an exact historical instant. Grounding, collisions, paddle/engine damage, firing eligibility, ammunition, hits and victory remain umpire decisions.

## Historical reconstruction

[DANFS / Naval History and Heritage Command](https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/patrick-henry-side-wheel-steamer.html) identifies the converted side-wheel steamer and gives a 250 ft length, 34 ft hull beam, 13 ft draft and 12-knot speed. Beam excludes the projecting wheels and wheel boxes. The Navy's [archived Patrick Henry collection](https://www.ibiblio.org/hyperwar/OnlineLibrary/photos/sh-us-cs/csa-sh/csash-mr/pat-hnry.htm) says she was repaired after March 8 and participated to a limited extent on March 9.

The model uses a wooden hull, side paddle wheels with upper semicircular wheel boxes, an exposed walking-beam framework, a single funnel, two masts, boats and representative rigging. **NH 57517**, Clary Ray's circa-1898 wash drawing, guides the wartime outline. **NH 42807**, Scharf's later school-ship sketch, helps identify wheel-box/engine features; the oversized later training rig and school cabins are not copied into the March 1862 model. These are drawings rather than verified 1862 photographs. Hull offsets, small fittings, paint and exact gun mount locations remain approximate.

The modeled working battery is **twelve guns**: five 32-pounder smoothbores on each broadside, a forward ten-inch shell gun and an aft eight-inch solid-shot pivot. The [official March battle report reproduced by NHHC](https://www.history.navy.mil/research/library/online-reading-room/title-list-alphabetically/u/uss-monitor-versus-css-virginia-and-the-battle-for-hampton-roads.html) lists Patrick Henry with twelve guns. [James H. Rochelle's account of Tucker's service, p.26](https://upload.wikimedia.org/wikipedia/commons/3/3c/California_Digital_Library_%28IA_lifeofrearadmira00rochiala%29.pdf) supplies this distribution. DANFS lists a different ten-gun outfit, so the distribution is explicitly a working reconstruction rather than a claim that all sources agree. The end-gun barrel/carriage profiles are representative; the stern weapon is labeled for its solid-shot role.

Initial measured geometry: **134,249 vertices, 77 render meshes, twelve guns, two independently animated paddle assemblies and one funnel**. The latest report is `Artifacts/PatrickHenry/model-counts.txt`; inspection renders are in that directory. Archived research images and private rulebook scans are not distributed with the player.

## Multiplayer

The **Virginia player commands Patrick Henry**, including her movement, engines and salvos. The Monitor player commands Minnesota's available batteries. The host retains weather, clock and table management. Commands for Virginia and Patrick Henry are queued independently; opponent edits are rejected. Both players need this **protocol 6** build. Existing direct-IP connection instructions apply; no lobby/relay service has been added.

## Build and checks

Use Unity 6000.2.12f1 and `Ironclads.Editor.ProjectBuilder.DeliverPatrickHenry`. The editor checks model/battery groups, paddle assemblies, ownership mapping, hex and continuous movement, turn orders, persistence, legacy initialization and switching to/from Ulsan. Standalone verification also exercises salvos, smoke, paddle animation and gunnery origin. Two-process TCP checks cover the Confederate player's supporting ship, ownership rejection and synchronized supporting salvos.

```powershell
& .\.tools\verify-player.ps1 -Hampton
& .\.tools\verify-network.ps1 -Hampton -IronFire
```

Both helpers default to `Builds/Windows-PatrickHenry`. The existing `-Ulsan` checks and Ferrol regression checks remain available.
