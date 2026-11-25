# Pantelleria — fourth detail upgrade, 1.10.10-dev

This pass targets another doubling of vertices and render mesh sections across all 42 ships, measured against the retained [close-up fittings baseline](Pantelleria-Fittings-Baseline.csv). The [comparison](Pantelleria-Detail4-Comparison.csv) records each ship's measured totals, excluding decorative rigging from both deliveries consistently. National ensigns and commissioning pennants remain included in the counts.

Measured averages are **2.09× vertices and 2.35× render meshes**. Individual ships range from 1.99–2.15× vertices and 2.24–2.40× meshes. Canopus has 148,788 vertices and 948 meshes, up from 71,250 and 404. All 1,132 editor checks passed, covering the new budgets alongside existing supported guns, rigging, saves and scenario checks.

Standalone verification passed all 42 ships and 14 divisions: movement, plotted/applied orders, clock, bridge pivots, firing, funnels, results, save/load and scenario switching, with no logged runtime errors. Anchor-chain, boat-floor, hull and representative national ship screenshots were visually reviewed. Reports and captures are in `Artifacts/Pantelleria/Detail4`.

Hulls have 384 longitudinal stations and 64 cross-section samples. Bilges gain quarter-step Catmull–Rom samples; straight upper sections preserve the existing waterline, sheer and fitted teak deck. Round fittings, funnels and gun barrels have finer radial resolution, scoped to Pantelleria. Gunhouse corners and boat shells have twice as many samples. Boat gunwales and internal framing are finer; deck and fighting-top railings have more stanchions. Anchor cables now have alternating, interlocking oval links.

The weathering, boat floorboards, bridge fittings, decorative rigging and animated national colours remain. Main-gun locations, calibres, trainable assemblies and muzzle positions are preserved. Shapes and equipment remain provisional visual reconstructions. Equipment detail adds no operational effects, weapons or encoded signals.

Static bodies and independent gun assemblies use more longitudinal mesh sections, preserving normals at the boundaries. Close-up fittings remain batched. Higher geometry and draw-call counts may increase frame time and memory use; the earlier build remains available at `Builds/Windows-Pantelleria-Fittings`. Save and protocol formats remain compatible.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaDetail4 -logFile Artifacts/Pantelleria/build-detail4.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Detail4 -OutputDirectory Artifacts/Pantelleria/Detail4
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher selects `Builds/Windows-Pantelleria-Detail4/Ironclads.exe -pantelleria` when present. Existing open player windows are left available; they use their own earlier model geometry.
