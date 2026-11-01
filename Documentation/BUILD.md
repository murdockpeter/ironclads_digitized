# Unity development and builds

**Latest: 1.2.1-dev supporting Union ships.** Use `Ironclads.Editor.ProjectBuilder.DeliverUnionShips` and `Builds/Windows-HamptonRoads-Union`. The same player/network verification helpers below accept this build directory. The launcher now uses this path. See [supporting-ship notes](Hampton-Union-Ships.md); TCP protocol 4 requires matching players. Earlier delivery paths below are retained as historical workflows.

**Current: Hampton Roads + Iron & Fire, 1.2.0-dev.** Build with `Ironclads.Editor.ProjectBuilder.DeliverHamptonRoads` into `Builds/Windows-HamptonRoads`. `Start-Ironclads.ps1 -IronFire` launches Hampton Roads in miniatures mode; omit `-IronFire` for hex play, or add `-Ferrol` for the earlier scenario. The game also provides scenario/ruleset selection inside the UI. The workflows below describe earlier deliveries.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverHamptonRoads -logFile hampton-build.log
& .\.tools\verify-player.ps1 -BuildDirectory Builds/Windows-HamptonRoads -Hampton
& .\.tools\verify-network.ps1 -BuildDirectory Builds/Windows-HamptonRoads -Hampton -IronFire
```

The editor review checks new batteries, turret mesh/muzzle movement, old-save defaults, terrain normals and scenario switching, and renders ship/fort/coast previews. The Hampton runtime check also exercises continuous movement, curved paths, speed orders, drift and persistence. Run player verification without `-Hampton` for the original Ferrol graphics/effects regression. Network checks without `-Hampton -IronFire` exercise Ferrol hex play; `-Hampton` alone exercises Hampton hex play. Private PDFs and source scans remain local.

Current development build: `Ironclads.Editor.ProjectBuilder.DeliverMultiplayer` produces `Builds/Windows-TCP` (`1.1.0-dev`). Run `.tools/verify-network.ps1` for a real two-process loopback test and `.tools/verify-player.ps1 -BuildDirectory 'Builds\Windows-TCP'` for graphical/offline regression checks. The stable v1.0.0 tag/release is unchanged.

Public 1.0.0 release build (works without private reference assets):

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverRelease -logFile release-build.log
```

Wait for the editor to finish and inspect `release-build.log`. The player is `Builds/Windows-v1.0.0/Ironclads.exe`. Validate with `.tools/verify-player.ps1 -BuildDirectory 'Builds\Windows-v1.0.0'`. Optional private reference viewers fall back to an explanatory status message when their assets are absent. Development model-comparison workflows below require local historical baselines excluded from the public repository; use `DeliverRelease` for a clean checkout.

Current engine-exhaust delivery: `Ironclads.Editor.ProjectBuilder.DeliverEngineSmoke` builds `Builds/Windows-EngineSmoke/Ironclads.exe`. Validate with `.tools/verify-player.ps1 -BuildDirectory 'Builds\Windows-EngineSmoke'` (Direct3D 11 for reliable unattended backbuffer captures). `Start-Ironclads.ps1` launches this build; previous paths below remain historical workflows.

Latest realism delivery: `Ironclads.Editor.ProjectBuilder.DeliverSalvoRealism` builds `Builds/Windows-Salvos-Realism/Ironclads.exe`. It verifies body/tail audio impact against the retained first-salvo baseline and retains pass-four geometry. Test with `.tools/verify-player.ps1 -DefaultGraphics -BuildDirectory 'Builds\Windows-Salvos-Realism'`. `Start-Ironclads.ps1` launches this version.

Current effects delivery: `Ironclads.Editor.ProjectBuilder.DeliverSalvos` builds `Builds/Windows-Salvos/Ironclads.exe`, retaining pass-four geometry and adding battery reports/smoke. Validate with `.tools/verify-player.ps1 -DefaultGraphics -BuildDirectory 'Builds\Windows-Salvos'`. `Start-Ironclads.ps1` now launches this build.

Current delivery: `Ironclads.Editor.ProjectBuilder.DeliverPass4` prepares assets, verifies the preserved pass-4 baseline and geometry, and builds `Builds/Windows-Pass4/Ironclads.exe`. Validate with `.tools/verify-player.ps1 -DefaultGraphics -BuildDirectory 'Builds\Windows-Pass4'`. `Start-Ironclads.ps1` launches this version explicitly. Earlier workflows below are retained for reference; do not recapture the existing pass-4 baseline.

Tested target editor: Unity 6000.2.12f1. Built-in render pipeline with custom water, sky, atmospheric glow, weather sprites and chart shaders. Only built-in Unity modules are required.

Open `Assets/Scenes/Ferrol.unity` in the editor and press Play. Its controller generates the environment. To edit individual ship parts, invoke `Shipwright.Create(false, false)` for Niagara or `Shipwright.Create(true, false)` for Stonewall from a temporary editor script. Regeneration uses parameters in `Assets/Scripts/Shipwright.cs`; changes to saved prefabs do not automatically change that generator. The current runtime also regenerates from this source.

From PowerShell in the project root:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.Prepare -logFile Artifacts/unity-prepare.log
```

On Windows the shell can return before the GUI executable exits. Wait for its completion and inspect the log. Preparation saves `Ferrol.unity`, reusable combined ship prefabs/materials/meshes, OBJ geometry exports, rendered PNG previews and verification results.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.BuildWindows -logFile Artifacts/unity-build.log
```

The Windows standalone is `Builds/Windows/Ironclads.exe`. A normal player can capture its full interface and exit using:

```powershell
& .\Builds\Windows\Ironclads.exe -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 -capture "$((Get-Location).Path)\Artifacts\Player-interface.png" -logFile "$((Get-Location).Path)\Artifacts\player.log"
```

The runtime `-capture` path is intended for a graphics-capable player, not Unity's headless server mode. Editor camera previews show 3D rendering; the standalone capture includes the actual umpire interface.

Behavioral checks exercise meaningful coordinate/range/scale invariants and solar cardinal directions. They do not establish historical hull accuracy, weather physics, combat correctness, or performance on other hardware. See the generated verification file and logs for the outcome of the current run.

For unattended player validation, `.tools/verify-player.ps1` launches the player without activation outside the visible desktop. It checks movement/undo, hex facings and the three-minute clock, captures the interface/chart/ships/weather/globe/sheet, rejects uniform screenshots, and writes `Artifacts/runtime-verification.txt`. Pass `-DefaultGraphics` to use Unity's default graphics API instead of forcing Direct3D 11. A fully hidden Unity player may produce black backbuffer captures, so the helper positions an unactivated render window offscreen. Render previews are separately produced by the editor.

Saves, CSV logs and in-game screenshots default to `%USERPROFILE%\AppData\LocalLow\Ironclads\Ironclads — Ferrol`. The status bar reports the exact path. Runtime reads only bundled assets and the selected local save; no network access is required.

The `.tools/read-pdfs.mjs` helper was used only to inspect local supplied reference PDFs. Its npm packages are development helpers and are not used by Unity or the standalone player.

The historical model-pass-2 workflow was `Ironclads.Editor.GeometryReview.Run`, comparing geometry against retained pass-1 OBJ files. Current work should use the pass-3 workflow below so the retained second-pass images and baselines remain available. Review fill lighting appears only in inspection renders, not in the game.

For the current **pass 3**, use `Ironclads.Editor.ProjectBuilder.DeliverPass3`. It prepares reusable assets, runs `GeometryReview.RunPass3`, then builds the Windows player. The review additionally checks twice the render mesh count, a 1.95–2.25× source-vertex budget against the retained pass-2 baseline, triangle preservation during fore/aft grouping, and Stonewall battery-base support. It renders isolated bow/stern/profile/overhead/battery views to `Artifacts/Pass3`. `PassThreeReview.CaptureBaseline` was used before the edits and must not be run again unless intentionally establishing a new baseline.

For the subsequent **integrated forecastle revision**, run `Ironclads.Editor.ProjectBuilder.DeliverBowRevision`. It builds to `Builds/Windows-BowRevision` and writes its geometry review/renders to `Artifacts/BowRevision`. Its checks allow the three intentional embrasures in Stonewall's now unified hull/forecastle and reject an independent circular forward battery. Test this player with `.tools/verify-player.ps1 -DefaultGraphics -BuildDirectory 'Builds\Windows-BowRevision'`.
