# Pantelleria — second visual polish, 1.10.8-dev

This pass adds animated bow wash and a short, soft stern trail, a less repetitive teak pattern, slightly clearer paint seams/soot detail, and fine weave/hem shading on the existing cloth. Ship bodies, gun mounts, rigging arrangements and national patterns retain their current geometry. The [rigging baseline](Pantelleria-Rigging-Baseline.csv) and [comparison](Pantelleria-Polish2-Comparison.csv) record the unchanged base mesh and vertex counts.

Surface wash is cosmetic and follows **current effective speed**, rather than plotted orders. It animates in display time while the operational clock is paused. It neither moves the ship nor records where the ship has travelled: the short trail is a local effect, not movement history or evidence for adjudicating an order. Stopped/sunk ships have no wash. Astern reverses its direction. It fades between 600 and 1,500 metres from the camera, and disappears in globe view or other modules.

Wash is blended directly into the ocean shading, using a compact list of nearby ships and feathered procedural foam. It adds no meshes, draw calls or separate overlapping water surfaces. No wash is shaded in other modules or distant fleet views. The wake shapes are illustrative rather than a hydrodynamic simulation or class-specific trial record. Current smoke, fleet visibility, gun and flag behavior remain.

Teak uses a larger fifteen-plank repeat with staggered ends, varied plank tone and fine grain. Hull plate seams and restrained streaking are slightly clearer; funnel bands gain subtle seam shading. Cloth receives a narrow hem and very light weave, filtered away when too small on screen. Flag colours and existing national/decorative designs remain.

Editor checks verify base geometry equality, gun/deck supports, rigging anchors/modes, flags, saves, exhaust and bounded speed response. Standalone checks add moving/stopped/sunk wash visibility and ahead/astern screenshots to the existing full-fleet tests. These operate in a fresh verification scenario and do not edit the user's open table.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaPolish2 -logFile Artifacts/Pantelleria/build-polish2.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Polish2 -OutputDirectory Artifacts/Pantelleria/Polish2-Ocean
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher prefers `Builds/Windows-Pantelleria-Polish2/Ironclads.exe -pantelleria`. Existing player windows and earlier builds are left available. Table data and protocol 10 stay compatible.
