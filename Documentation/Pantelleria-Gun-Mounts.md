# Pantelleria — supported gun mountings, 1.10.5-dev

Open barbettes deliberately expose the main guns above their armour. For example, [Royal Museums Greenwich describes the Royal Sovereign class's main guns as mounted in open barbettes](https://www.rmg.co.uk/collections/objects/rmgc-object-67177). The flat circular base in the model is not meant to represent an enclosed gunhouse.

The previous models omitted the supporting carriages, leaving a visible gap between each barrel and its platform. The barbette bases also started above the weather deck. This repair extends each foundation down to the deck and adds a carriage bed overlapping the platform, vertical cheeks, trunnion bearings and a recoil slide under every exposed main barrel. The old decorative trunnions at the outer edges of open mounts are removed; the new bearings sit at each gun's actual axis. Closed gunhouses retain their barrels at the existing height, through the gunhouse front.

Secondary guns receive pedestal feet, columns and trunnions. Light guns receive the same support on individual sponson platforms, with braces connecting the platforms to the central battery structure. All existing gun axes, muzzle coordinates, battery groups, saved bearings and firing effects are retained. Carriage shapes are illustrative rather than surveyed class/refit-specific mechanisms. Open barbette armour shapes remain simplified.

The [previous detail baseline](Pantelleria-Detail3-Baseline.csv) and [repair comparison](Pantelleria-Mounts-Comparison.csv) preserve the earlier counts. This is a mounting correction, not another doubling pass.

Editor verification checks every main gun. For exposed guns it examines the actual combined triangles for a support edge reaching from platform level to the trunnion level beneath that individual barrel. The former floating-barrel geometry would fail this test. For closed mounts it checks that each barrel axis remains within the gunhouse's vertical extent. Existing mesh, battery, flag, wind and save checks also run. Standalone review adds two low-angle close-ups of an exposed barbette to the existing ship/gunnery/flag checks.

Every barbette foundation is also checked against the ship's weather-deck height using the actual combined mesh vertices, so an unsupported gap beneath the armour base fails verification.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaMounts -logFile Artifacts/Pantelleria/build-mounts.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Mounts -OutputDirectory Artifacts/Pantelleria/Mounts-Verified
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher selects `Builds/Windows-Pantelleria-Mounts/Ironclads.exe -pantelleria` when present. Previous player windows remain open during development; save before closing them and load the existing Pantelleria table in this build.
