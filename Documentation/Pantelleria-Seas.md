# Pantelleria — hull contact and displacement wash, 1.10.12-dev

This pass replaces the soft bow/central-trail appearance with crest-dependent waterline foam, a modest raised bow shoulder, a narrow broken stern track and subdued spreading wake-wave shading. The broad wake pattern is not painted as a solid white V. Wash scales with effective speed and hull length; the display uses length Froude number and a speed-squared deep-water wavelength. Astern reverses the leading end and reduces the bow/wake intensity. Propeller count and detailed reverse propeller flow are not modeled.

The near-water mesh has 768 rather than 256 cells per side across the existing 2,400 m patch (about 3.125 m spacing). No additional overlapping water layer is introduced. Pantelleria waves use three directional components with deep-water dispersion, travelling downwind. CPU elevation, slope and vertical-velocity samples use the same constants as the ocean shader. Large hulls derive damped heave/pitch/roll from bow, stern and side samples, replacing independent sine motions. Other modules retain their existing wave and hull-motion path and original near-water mesh.

Six points on each nearby leading quarter sample crests relative to the posed hull waterline. Rising water above that line produces brief, small droplets, biased toward the windward side. Droplets have gravity, wind drift and short fading lifetimes; the shared effect is capped at 500 particles and limited to ships within 450 m. Waterline foam and wash fade from 600 to 1,500 m. Stopped ships retain wave contact in rough water but have no movement wake. Calm stopped ships produce neither contact foam nor new spray. Sunk ships, globe view and other modules stop new effects.

These are historically informed display approximations: there is no class-specific resistance curve, measured seakeeping response, impact-pressure solver, green-water flow or full fluid simulation. The existing wave-scale slider remains a display scale, not a calibrated significant wave height or an automatic conversion of the printed sea-state category. The local wake is not a saved track or evidence of movement while the umpire clock is paused. Orders, clock, damage, saves and protocol 10 remain compatible. The finishing details add actual recessed bores and steel muzzle lips to the existing guns, plus bridge-window sills and hoods. Gun counts, supports and muzzle reference points remain. This is a bounded finishing increase rather than another doubling; [model counts](Pantelleria-Seas-Comparison.csv) are measured against the [prior player](Pantelleria-Seas-Baseline.csv). The water mesh and shading also add rendering cost.

References used for the visual and physical direction:

- [Canadian Coast Guard SAR Seamanship Reference Manual](https://waves-vagues.dfo-mpo.gc.ca/Library/253768.pdf), printed pp. 9-20–9-22: bow/quarter waves and displacement-vessel behavior.
- [NUS CRISP ship-wake reference](https://crisp.nus.edu.sg/~research/shipwakes/shipwakes.htm): turbulent wake versus Kelvin waves; hull shape, screws and speed affect the near wake. This supports separating aerated foam from the spreading wave pattern.
- [National Archives battleship photograph index](https://www.archives.gov/research/military/navy-ships/battleships.html): period underway photographs including Texas in 1900.
- [USS Texas period photograph reproduced by NavSource](https://navsource.net/archives/01/001/texas48.jpg): restrained close-hull bow disturbance used as a visual reference. This is not evidence of Pantelleria's fictional weather or a specific class's measured wake.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaSeas -logFile Artifacts/Pantelleria/build-seas-finish.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Seas -OutputDirectory Artifacts/Pantelleria/Seas
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher prefers `Builds/Windows-Pantelleria-Seas/Ironclads.exe -pantelleria`. Previous builds and open player windows remain available.

Validation: 1,319 editor checks passed, including sea slopes/vertical velocity, bounded hull attitudes, wake scaling, crest-impact conditions, open muzzle centres/inward bore normals, retained gun supports and bounded finishing counts. Standalone Pantelleria checks passed rough head/beam/following sea cases, actual spray emission/cap, stopped rough-water contact, calm suppression, original water-mesh restoration outside the module, plus all 42 ships/14 divisions, orders, firing, results and save/load. No runtime errors were logged. Sea-state, bridge and gunhouse screenshots were visually reviewed. Reports/captures are in `Artifacts/Pantelleria/Seas`. Canopus adds 3.1% vertices and 0.4% render meshes over the preceding player.

The standalone Ulsan regression also passed: models, movement, bridge pivot, turn intervals, turrets, funnels, salvos, saves and scenario switching.
