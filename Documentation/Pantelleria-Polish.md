# Pantelleria — surface and exhaust polish, 1.10.6-dev

This delivery keeps every ship's geometry, mesh count, supported gun mounts, animated colours and tactical behavior from 1.10.5. The [mounting-repair baseline](Pantelleria-Mounts-Baseline.csv) and [polish comparison](Pantelleria-Polish-Comparison.csv) record exact count equality across all 42 ships.

The Pantelleria-only surface shader adds slight paint variation, roughness variation, faint staggered plate seams, restrained streaking below the weather-deck edge and waterline grime. Hull effects apply to the ship body's vertical sides; independently trained gunhouses receive the paint finish without hull waterline shading. Per-ship seed offsets prevent identical weathering patterns across sister ships. Seams and narrow streaks fade when the screen footprint becomes too large to resolve them.

National identification paint palettes are slightly lighter and painted surfaces use a lower metallic response. Funnel tubes gain modest soot darkening toward their tops. The existing teak texture receives warmer, more varied plank tones and softer seams. These are provisional artistic treatments, not newly researched historical paint or refit claims. Weathering does not indicate adjudicated damage.

Pantelleria coal exhaust has gentle, different pulse phases for each funnel, reduced average opacity/emission and a less inflated growth curve. Engine load changes particle rate, shade and opacity; extinguished boilers emit no new particles, while existing smoke continues to disperse. Existing wind transport, turbulent advection, source attachments and sunk/globe handling remain. Smoke animation uses display time and does not advance the operational clock. Ulsan's exhaust parameters and ship materials retain their existing path.

The shader is explicitly included and checked during the build. Editor verification checks geometry equality, actual main-gun support/deck contact, flags, saves and exhaust-response functions. Standalone checks confirm real fleet emission and no new emission after all boilers are extinguished, then restore their initial states before the existing movement/fire/flag/module-switch checks. Fleet, representative ship and mounting close-ups are captured for visual review.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaPolish -logFile Artifacts/Pantelleria/build-polish.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Polish -OutputDirectory Artifacts/Pantelleria/Polish
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher prefers `Builds/Windows-Pantelleria-Polish/Ironclads.exe -pantelleria`. Earlier builds remain separate and open games are left running. Save the old table before closing it, then load the same Pantelleria slot in this build. Existing protocol 10 and table data are retained.
