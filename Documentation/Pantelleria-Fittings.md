# Pantelleria close-up fittings — 1.10.9-dev

This pass adds bridge roof and wing safety rails, bridge window mullions, a compass binnacle, white lifebuoys with retaining straps, cable reels, skylight glazing bars, and individual floorboards/stowed oars in the existing boats. The fittings are illustrative equipment on the provisional models, rather than surveyed class-specific arrangements. They add no weapons, gameplay effects or signalling meanings.

Paint gains restrained abrasion and mild staining around plate seams, filtered away at fleet distance. Teak gains slight sun-bleaching, varied worn tones and darker end grain. The open boats retain their thwarts; five individual tapered floorboards per boat sit below them, with narrow visible gaps.

All 42 ships receive the fittings. Existing hulls, decks, supported guns, animated national colours, decorative rigging, smoke and surface wash remain. Equipment is combined into a separate static group, with a maximum of 16 meshes and 10,000 vertices per ship. The [comparison](Pantelleria-Fittings-Comparison.csv) records added fittings separately from the preserved [body baseline](Pantelleria-Rigging-Baseline.csv).

Editor verification checks the fittings' above-deck bounds and mesh budget for all ships, alongside the existing gun supports, flags, rigging, saves and scenario checks. Standalone verification includes bridge/foredeck screenshots and the existing full-fleet operational checks. Table format and protocol 10 remain compatible.

Validation: 1,132 editor checks passed. Standalone verification passed all 42 ships/14 divisions, movement, orders, clock, guns, smoke, rigging, wash, results, save/load and module switching, with no logged runtime errors. Close-up bridge, foredeck and boat-floor screenshots were visually reviewed. Each ship adds 8,532 vertices and 14 batched meshes; its existing body counts are unchanged.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaFittings -logFile Artifacts/Pantelleria/build-fittings.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Fittings -OutputDirectory Artifacts/Pantelleria/Fittings
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher prefers `Builds/Windows-Pantelleria-Fittings/Ironclads.exe -pantelleria`. Prior builds and open player windows remain available. Save your table in an older player before loading the shared Pantelleria save slot in the new one.
