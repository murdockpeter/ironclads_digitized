# Supplied Monitor and Virginia models — 1.5.0-dev

The OBJ/texture pairs supplied in `contextonly` have been compared and integrated as optional **local hybrid appearances** for Hampton Roads. The original procedural models remain available. Position, heading, speed orders, turret bearing, battery ownership and the shared table are retained when appearance changes.

Run `./Start-Ironclads.ps1 -SuppliedModels` to open the hybrid comparison in `Builds/Windows-SuppliedShips`. Under **Sources → Local model comparison**, toggle either ship and inspect it. Omit the switch to start with the existing procedural appearances. Choices are local to the current session, like the camera; they are not written into the shared scenario/save or transmitted to the other player.

## Comparison

| Ship | Supplied OBJ position vertices | Supplied triangles | Existing procedural vertices / triangles | Hybrid vertices / render meshes |
| --- | ---: | ---: | ---: | ---: |
| USS Monitor | 3,538 | 6,514 | 121,668 / 103,476 | 95,107 / 78 |
| CSS Virginia | 1,815 | 3,167 | 136,072 / 116,616 | 76,862 / 52 |

OBJ position counts exclude additional splits required for UV seams and flat normals. More triangles do not establish historical accuracy: our existing meshes spend much of their budget on armor surfaces, fastenings, gun carriages and small fittings. The supplied models give useful alternative silhouettes with far fewer faces.

The supplied Monitor has more pointed deck ends, a turret-top framework and a different exhaust/fitting arrangement. Virginia has a tapered casemate, a narrower roof, boats and an aft extension with a different hull outline. These models have not been verified against original shipyard plans. They provide alternate reconstruction geometry rather than proof that every supplied fitting is historically preferable.

## What is reused

**Monitor:** the supplied textured upper hull/deck replaces our armored raft outline. Our submerged lower hull, detailed rotating turret with two XI-inch gun mounts, open embrasures, original-style pilothouse, low battle exhaust, hatches and other suitable fittings remain. The supplied turret-top framework, turret shell and different exhaust are visible in the raw source comparison but are not used in the hybrid. Old deck seam/rivet geometry that would extend beyond the new outline is removed.

**Virginia:** most of the supplied exterior is reused, including its hull, casemate, funnel, boats, fittings and atlas. The source's separate barrel meshes are replaced by our nominal ten working guns. Broadside guns are fitted using the source barrel locations; end guns are positioned using the supplied casemate surface. Four port, four starboard, one bow and one stern firing group remain available. Our submerged hull supplies the underwater body; the imported funnel defines the coal-smoke attachment point.

The files contain one named OBJ object/material each, with disconnected components inside that object. The preparation tool separates those components by connectivity, preserves each corner's UV/normal indices, and rotates the source X-longitudinal coordinate system into Unity's +Z bow convention. Uniform scale uses a 52.4256 m Monitor raft and an 80.16 m Virginia hull; vertical placement aligns each weather deck to the current waterline. Resulting supplied hull beams are about 12.566 m and 15.815 m. Virginia's extra aft projection extends beyond the reference hull length.

Both referenced `.mtl` files are absent. The matching PNG/JPG atlases are assigned explicitly. Virginia's supplied atlas contains saturated red surface regions; the hybrid material replaces those red regions with neutral gray while preserving texture variation. Flag faces retain their original atlas colors. This is a reversible shader treatment: no source bitmap or OBJ is edited. Monitor retains its supplied dark atlas.

## Files and reproducibility

Source inputs:

- `contextonly/Monitor_1.obj`
- `contextonly/Monitor_hull.001_Monitor_turret.001.png`
- `contextonly/CSS Virginia.obj`
- `contextonly/CSS_Virginia_texture.jpg`

Run `python Tools/prepare-supplied-ships.py` to generate `Assets/Resources/SuppliedShips`. Each baked model records the source OBJ SHA-256, normalization and separated component meshes. Source files and prepared third-party resources remain ignored by Git; this local player includes the prepared resources. Their author/license was not supplied, so provenance is recorded as user-supplied rather than assigning an invented creator or license. This change does not publish them or upload them to GitHub.

The raw/procedural/hybrid comparison renders and `comparison.csv` are under `Artifacts/ImportedShips`:

- `Monitor-procedural.png`, `Monitor-supplied-original.png`, `Monitor-hybrid.png`
- `Virginia-procedural.png`, `Virginia-supplied-original.png`, `Virginia-hybrid.png`
- Separate supplied deck views show component/layout differences.

## Verification and build

Build with Unity 6000.2.12f1 and `Ironclads.Editor.ProjectBuilder.DeliverSuppliedShips`. The editor checks UVs/textures, both nominal batteries, four Virginia groups, Monitor turret/muzzle rotation, smoke binding and unchanged table state when switching appearances. The standalone Hampton check switches models, fires two plus ten guns, checks the imported funnel outlet and restores the originals.

```powershell
& .\.tools\verify-player.ps1 -Hampton
```

TCP protocol remains 6. Salvo commands still address the same ship/battery groups, while each player renders their selected local model. Exact mesh geometry is not used to adjudicate firing arcs, hits, damage, collision or line of sight.
