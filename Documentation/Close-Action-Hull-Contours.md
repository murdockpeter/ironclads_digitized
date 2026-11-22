# Close Action: hull contours and weather access

The hull work introduced in 1.8.2-dev refines the complete hull and restores a usable weather panel. Launch it with `./Start-Ironclads.ps1 -CloseAction`; the preferred build is now `Builds/Windows-CloseAction-HeadFraming` (1.8.13-dev), with integrated hulls and upward-sweeping bows, closed interiors, decorated bows, separate headsails and fuller animated canvas and persistent powder smoke.

## Hull references and fitting

The 1.8.13-dev revision removes forward shell stretching entirely. The rounded hull closes at its own stem; a separate attached bow assembly contains the bowsprit/jibboom, headrail framing and carved knee/figurehead. Rectangular curved timber rails and open transverse supports replace the former round rail rods. The narrow central knee seats back into the stem rather than pulling the hull out to its curve. This supersedes the shell-extension approach described in the earlier revision notes below.

The 1.8.12-dev revision rounds the bow cheeks into a compact centerline entry, reducing the forward shoulder beneath the headrails. The knee and figurehead move aft to shorten their projection, with the curved rails following their new attachment. Planking now uses continuous surface normals across the paint bands, removing the faceted appearance of separate skin patches; port reveals retain sharp timber edges. Front, side, overhead and quarter views are captured for all ten prototypes. The supplied frontal bow illustration and painting guide the silhouette alongside the earlier photographs and plans.

The 1.8.11-dev revision reduces the solid upper bow seen in the side profile. The lower shell still joins the swept knee, but its forward extension fades out toward the weather deck. The forecastle ends farther aft, with a closed head bulkhead, leaving open headrail framing beneath the bowsprit. Forward upper sections narrow by up to 18 percent; the ten prototypes retain their separate proportions. This follows the relationship between rounded lower hull, narrow rising knee and open beakhead visible in the supplied bow photographs and cutaway.

The 1.8.10-dev revision lofts the forward hull directly into the curved knee of the head. At each height, the stem follows the back of the knee with a small structural overlap, blended progressively into the existing bow stations. The forecastle extends forward and its lifted deck and side closures follow the same contour. Planking, ports, cannon seating, wales, deck edges and interior floors use that common shell mapping. This fills the former triangular opening between the old near-vertical hull end and the curved head. The underwater body and waterline are retained.

The 1.8.6-dev revision rounds the last upper-bow stations, increases the stem sweep and rolls the forefoot aft below the waterline. Forecastle head bulkheads now close the forward deck break, and beakhead rails follow the sheer with a rising sweep. These changes apply to all ten class/family prototypes. The bowsprit and overlapping jibboom are built as rigid, parallel spars outside the deck-fitting deformation pass; fore-stays, jib corners, the cap and dolphin striker share their attachment frame.

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
