# Pantelleria — individual hull and deck finishes, 1.10.11-dev

Each of the 42 ships now has a deterministic finish seed and separate wear, corrosion, bleaching and deck-profile values. These derive from the scenario identity and vessel's roster name, without changing Unity's random state or adding save fields. Sister ships receive different patterns. Ship movement, camera movement, restart and loading the same table preserve the finish. Separate hull-side coordinates avoid matching port/starboard stains.

The main weather deck uses a new procedural teak material: 22 cm boards, staggered 4.5 m butt joints, individual plank tones and grain, recessed caulking, wooden plugs, faded areas, mild dirt and worn walking paths. Plank arrangements do not repeat as a shared texture tile. Tangents are rebuilt after mesh batching for the shallow seam normals. Fine marks and seam contrast filter away at distance.

Hull finish includes varied rust runs below the sheer, plate-seam corrosion, faded paint, a mottled tide line and light salt staining. Wear follows the ship profile. Existing funnel soot, supported guns, fittings, boat floorboards, animated flags, rigging and wash remain.

Weathering is dynamic but reproducible. The existing rain control darkens surfaces and increases wet sheen, with ship-specific patch patterns. Wetness follows current rain immediately; it is not a stored drying or water-accumulation simulation. Aging uses elapsed scenario days after 14 February 1901, 13:30 UTC, bounded at 90 days. It adds restrained wear over days, rather than visible rust growth every frame. Rewinding the clock rewinds this cosmetic age. Saved clock/rain values reproduce the result after loading.

Geometry and mesh counts remain exactly at the [fourth detail baseline](Pantelleria-Detail4-Baseline.csv); see the [comparison](Pantelleria-Weathering-Comparison.csv). These cosmetic effects do not alter damage, movement, orders or adjudication. The separate deck shader adds surface-shading work; no unchanged-performance claim is made.

Validation: 1,263 editor checks passed, including all 42 unique finish profiles, hull/deck property bindings, post-batch deck tangents, preserved geometry, clock aging and save roundtrip. Standalone checks passed sister-ship seeds, rain response, unchanged preview clock and the existing full fleet movement/orders/firing/save/module regressions, with no logged runtime errors. Dry Canopus/Ocean and wet Canopus screenshots were visually reviewed. Reports and captures are in `Artifacts/Pantelleria/Weathering`.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaWeathering -logFile Artifacts/Pantelleria/build-weathering.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Weathering -OutputDirectory Artifacts/Pantelleria/Weathering
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher prefers `Builds/Windows-Pantelleria-Weathering/Ironclads.exe -pantelleria`. Earlier builds and open tables remain available. Protocol 10 and existing save files remain compatible.
