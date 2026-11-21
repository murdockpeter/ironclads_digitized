# Close Action: hull contours and weather access

The hull work introduced in 1.8.2-dev refines the complete hull and restores a usable weather panel. Launch it with `./Start-Ironclads.ps1 -CloseAction`; the preferred build is now `Builds/Windows-CloseAction-Smoke` (1.8.4-dev), with fuller animated canvas and persistent powder smoke.

## Hull references and fitting

The contour work uses the body sections, longitudinal profile and half-breadth drawings already reviewed for the stern revision:

- [British 74s: Cornwall, Arrogant, Vanguard and related ships](https://www.rmg.co.uk/collections/objects/rmgc-object-80933).
- [Isis, Jupiter and Leander](https://www.rmg.co.uk/collections/objects/rmgc-object-81622).
- [Pompée, a captured French 74 recorded before British fitting](https://www.rmg.co.uk/collections/objects/rmgc-object-80952).
- [Canopus, formerly Franklin, French 80-gun lines and profile](https://www.rmg.co.uk/collections/objects/rmgc-object-80377).

The new normalized station curves are visual fits to those drawings. They are representative offsets for the ten class/family models, rather than a transcription of a complete surveyed set of offsets for each individual ship. The source scans remain local research material. Existing [prototype assignments and reconstruction limits](Close-Action-Visuals.md) still apply.

The old sinusoidal planform and elliptical underbody have been replaced by interpolated waterline and upper-body station curves. The body sections now have rounded bilges, rising floors toward the ends, a fine after run and inward-sloping upper sides. The bow narrows continuously to a raked stem and forefoot. Upper breadth, bow and stern fullness, sheer and tumblehome vary between the class/family profiles. Monotone cubic interpolation keeps the sampled offsets smooth without overshooting into ripples.

The weather deck follows longitudinal sheer and has transverse camber. Forecastle and quarterdeck surfaces follow the hull outline, including their side closures and deck breaks. Hull openings use the exact gunport edges as mesh boundaries; each barrel and port frame is seated on the same surface. Open port lids hinge outward from the upper edge. Rails, plating seams, wales, knees and deck fittings follow the new shape. Keel segments follow the rising floor, while shrouds and spars retain the existing wind-responsive rig.

The geometry remains at physical model scale. The two-hex counters, movement plots, gun factors, ranges and scenario positions retain their tabletop meaning. Visual draft is based on the existing rating-level draft values and is not a hydrostatic loading calculation.

`CloseActionHullReview` writes a station-offset CSV and checks the ten hull forms, their bounded breadths, upper-side narrowing, fine ends, cannon alignment and muzzle clearance. The existing geometry review also checks gun counts, bow/stern closures and sail response, and renders four views of every prototype.

## Weather controls

An always-visible **Weather / wind** button opens the module's Weather tab.

**Sea / sky** opens first. It provides Fair, Overcast and Rain presets, cloud cover, rainfall, visual visibility and wave-scale sliders. These shared environmental settings are editable by the host throughout play. Sea sound and volume controls are local to each player.

**Wind / turns** provides the six wind directions and four published wind-strength categories. It displays both the table's toward-direction and the corresponding from-bearing. Apply current wind during setup or Plot; changing it clears sealed movement orders so captains can submit legal replacements. Once orders are revealed, queue changes for the next turn. Oceanic/Steady weather rolls remain available during Reinforcements & weather.

Queued changes, wind and environmental settings are saved with the table and synchronized by the host. Scenario 6's published sea state remains 3; the wave slider controls appearance. Visibility effects beyond the visual scene remain umpire decisions.
