# Pantelleria — third detail upgrade, 1.10.4-dev

This pass refines every one of the 42 ships and retains their national ensigns and commissioning pennants. It is measured against the [animated-colours baseline](Pantelleria-Flags-Baseline.csv), with each ship's counts in the [third-detail comparison](Pantelleria-Detail3-Comparison.csv). Earlier baseline and comparison files remain unchanged.

Measured per-ship averages are **2.31× vertices and 2.05× render meshes** over the previous player. Individual ratios are 2.20–2.40× vertices and 1.93–2.14× meshes. The extra mesh sections preserve existing normals and independent trainable assemblies; cloth is kept outside the static mesh combiner.

Changes visible in close inspection:

- Hulls use 192 longitudinal stations, with a fitted narrow belt following the hull around the waterline.
- Weather decks have UV-mapped, mipmapped teak planks with staggered butt joints and subtle grain. The fitted outline still follows the hull.
- Gunhouses have twice as many corner samples, access hatches, handles, mounting details and access rungs. Existing gun mounts and trainable assemblies remain attached.
- Barrels and breech jackets are rounder and have reinforcing chase rings. Gun counts and muzzle positions remain unchanged.
- Ship's boats have finer shells, raised gunwales and internal ribs; boat booms and lowering tackle add detail around their stowage.
- Deck railings are finer. Military masts gain ladders and fighting-top safety rails, while hatches gain hinges and handles.
- Animated cloth has 64 by 8 mesh cells per flag, with the existing apparent-wind response, pinned hoists, national patterns and independent flutter phases.

Dimensions, fitting placements and hull offsets remain approximate visual reconstructions. This pass does not assert a historically surveyed deck plan or add operational equipment effects. Games, saved orders/results, clock controls and network protocol stay compatible with the previous Pantelleria player. Extra geometry may cost frame time; no unchanged-performance claim is made.

Latest player: `Builds/Windows-Pantelleria-Detail3/Ironclads.exe -pantelleria`. The launcher prefers it when present. Previous builds are kept separate so an open game does not need to be closed during development. Save the old table before closing that window, then load the same Pantelleria slot in the new player.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaDetail3 -logFile Artifacts/Pantelleria/build-detail3.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Detail3 -OutputDirectory Artifacts/Pantelleria/Detail3
.\Start-Ironclads.ps1 -Pantelleria
```

The editor checks all 42 mesh budgets against the preceding build, finite vertices, working guns/funnels, national flag attachments and apparent-wind response, roster/setup, save data and protocol bounds. Standalone review captures four national colours and pennants, fleet views, representative ships, gunhouse detail, gunnery and the ledger, then verifies fleet/division movement, firing and scenario switching.
