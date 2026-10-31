# Map compatibility with the supplied rules

Source: `contextonly/NEW_IRONCLADS_RULES.pdf`, inspected locally, plus `contextonly/ShipSheets/Stonewall.pdf`.

| Reference | Scene behavior |
| --- | --- |
| Game scale, p. 3 | 100 yards (91.44 m) across parallel hex sides; adjacent centers are 91.44 m apart. |
| Game scale, p. 3 | End Turn increments the turn and advances UTC by 180 seconds. |
| Facing, pp. 9–10 | Six hexside headings, with 60° facing controls. |
| Movement, pp. 11–14 | Umpire places vessels by hex, clicks the board, or moves one hex forward/reverse. No MP, engine, turning or sequenced-movement limits are imposed. |
| Gunfire range, p. 18 | Cube/axial hex distance counts the shortest path, excluding source and including destination. Straight-line yards are shown separately. |
| Sheet references | Stonewall's supplied PDF and Niagara's supplied photographed 1863 card are available in Sources. The 1863 mixed outfit is reference material; the user selected the 1865 working outfit for this scene. |
| Stonewall sheet annotations | Optional visual guide: 180° forward, 120° on each side. The umpire interprets gun mounts and firing eligibility. |
| Scenario 4D | Optional north wind and visual fog at 30 hexes; the physical meaning of game visibility remains an umpire ruling. |

The six headings are grid N, NE, SE, S, SW, NW, corresponding to true bearings 0°, 60°, 120°, 180°, 240°, 300°. These are hex-grid direction names, not the conventional eight-point compass bearings. Global +Z is north and +X is east.

The chart is a flat-top hex grid. Axial `(0, 0)` is local ID **2119**. Columns are 01–41 and offset rows are 01–37. These IDs deliberately belong to a new local chart and must not be used as printed mapboard I–IV IDs. Grid scale is fixed; the camera changes scale, not the board or ship meshes.

The large ship models can cross a hex border. Occupancy and range use the center marker, like a game counter, rather than mesh overlap. Moving into an occupied hex is allowed for umpire use; no collision or ramming result is inferred. Shift-click and one-hex moves are bounded to the local chart. Free placement offers a larger meter-based sea area.

The scene provides a two-ship variant, not a recreation of the entire three-ship Scenario 4D. Sacramento, original board artwork, shoal labels AA/O/S, southwest current zones, entry/exit areas, hidden movement, gunfire templates for other vessels, damage tracking and combat charts are not implemented. Umpire notes and CSV snapshots support external adjudication.

The term “turret” in the supplied Stonewall sheet is preserved in the original sheet image. The 3D reconstruction uses stationary armored enclosures; game arcs need not imply physically rotating armor. The guides originate at the vessel center for readability and are not precise gun-emplacement geometry.
