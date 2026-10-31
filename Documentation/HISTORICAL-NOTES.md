# Reconstruction basis and limitations

## Ships

Niagara is depicted in her post-refit configuration, guided by the 1863 NH 75895 photograph: three masts, two funnels, dark hull and a pale gun-deck stripe. Reference hull length is 328 ft 10 in (100.228 m), beam 55 ft (16.764 m), draft 24 ft 5 in (7.442 m). Bowsprit and rig extend beyond hull dimensions. The ten visible gun positions are a provisional arrangement; gun ports, carriage details and exact 1865 mounting positions require further plan research. The DANFS heading lists an earlier armament and is not used as a verified 1865 battery list.

- [NHHC Niagara ship history](https://www.history.navy.mil/research/histories/ship-histories/danfs/n/niagara-ii.html)
- [Naval Historical Center photo collection, preserved by ibiblio](https://www.ibiblio.org/hyperwar/OnlineLibrary/photos/sh-usn/usnsh-n/niagra2.htm)
- Local reference: `References/Niagara-1863-NH75895.jpg`.

Stonewall uses the NHHC reference length 171 ft 10 in (52.3756 m), beam 32 ft 8 in (9.9568 m) and draft 14 ft 4 in (4.3688 m). The separate projecting ram and bowsprit make overall bounds longer. Her model has two masts, one funnel, a forward armored battery for the 300-pounder, and an aft fixed armored enclosure with two 70-pounders. Published hull/overall lengths differ; this is not a measured lines-plan reconstruction. The March 1865 Ferrol photograph guides the silhouette. The bow ram, enclosure shapes and positions remain approximate.

- [NHHC Stonewall ship history](https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/stonewall.html)
- [NHHC H-Gram 063](https://www.history.navy.mil/content/dam/nhhc/about-us/leadership/hgram_pdfs/H-Gram_063.pdf), discussion of fixed gun shelters and propulsion.
- [Naval Historical Center Stonewall photographs, preserved by ibiblio](https://www.ibiblio.org/hyperwar/OnlineLibrary/photos/sh-us-cs/csa-sh/csash-sz/stonewll.htm)
- Local reference: `References/Stonewall-Ferrol-1865-NH42861.jpg`.

Both models are procedurally built meshes, not image-generator mockups or scanned models. They have hull shells, deck seams, masts, yards, furled canvas, standing rigging, ratlines, funnels, visible gun tubes, rails, boats and fittings. Those details are illustrative. Colors, exact wood species, plank spacing, rope runs, casemate armor layout and deck furniture are not independently verified. Flags and crew are omitted. Sails stay furled. Props, rudders and interior machinery are not modeled. No claim of museum or engineering accuracy is made.

## Second model pass — 31 October 2026

The original hull generator evaluated a fractional power of a slightly negative floating-point sine at the bow. This produced invalid final-section vertices; combined OBJ geometry contained 17 invalid vertices per ship. The revised loft clamps that domain, uses 128 longitudinal stations and 24 cross-section bands, and explicitly closes the bow and stern. The deck shares the shell's exact sheer and perimeter. Geometry checks weld coincident positions and reject unintended boundary edges; Niagara's genuine gun-port openings are the only accepted shell boundaries.

Barrels now use revolved exterior profiles and modeled inner bores, with muzzle annuli, recessed bore bottoms, sights, cascabels, crosswise trunnions and naval truck carriages. Parrott models have a conspicuous breech reinforce; the Armstrong models have distinct dimensions and reinforced profiles. Stonewall's batteries have actual openings and armor reveals, and her ram is a closed loft joined into the bow. Barrel lengths, carriage fittings and exterior profiles are representative reconstructions, not measured replicas of surviving guns.

The bore controls are 8 inches for the working 150-pounder Parrott depiction, 10 inches for the 300-pounder Armstrong and 6.4 inches for the 70-pounder Armstrong. Sources for gun families and construction:

- [NHHC Stonewall gun calibers and fixed armored enclosures](https://www.history.navy.mil/about-us/leadership/director/directors-corner/h-grams/h-gram-063/h-063-3.html)
- [NPS West Point Foundry nomination, including naval Parrott guns and carriages](https://www.nps.gov/subjects/nationalhistoriclandmarks/upload/West_Point_Foundry_NHL_WSecFinal_2021-08-03_Redacted.pdf)
- [NAVSEA collection notes on Parrott rifles and preserved naval ordnance](https://www.navsea.navy.mil/Home/Shipyards/Norfolk/About-Us/History/CannonsatTrophyPark/)

The newly supplied `contextonly/ShipSheets/uss_niagara.jpg` is included unchanged in the reference viewer. Its diagram is explicitly labeled **B—Niagara (1863)** and depicts a mixed Parrott/Dahlgren outfit. At the user's direction, the current Ferrol model retains the ten-Parrott working reconstruction; the card has not been treated as proof of the 1865 battery. Exact 1865 positions remain provisional. Stonewall retains one 300-pounder and two 70-pounders. No battery counts were increased merely to add model complexity.

The first-pass sources, geometry and full-ship views are retained under `Artifacts/Pass1`. The second-pass geometry report and bow, stern, battery and isolated gun renders are under `Artifacts/Pass2`. These provide a baseline for later third and fourth passes.

## Third model pass — 31 October 2026

Stonewall now uses a separate hull generator informed by the locally retained NavSource plan, dry-dock and NHHC photographs. The model has a long region of nearly parallel sides, a rounded stern, a descending armored forward deck, and a curved ram shoulder. The forward battery is rounder in plan; the after battery is broader across the beam. Their armor begins below the weather deck and slopes inward to the roofs. All sampled battery roots fit inside the hull planform; the unsupported overhang seen in pass 2 is removed. These remain inferred shapes rather than a measured reconstruction of a verified builder's lines plan.

Niagara's hull now has a broad stern transom instead of the earlier pointed stern. Hull loft resolution, barrel profiles, mast platforms and fittings are refined. Both ships have additional rigging fittings, railed fighting tops, anchors/chains/hawse rims, deck cleats, staggered plank butt seams, gun carriage straps and bolts. Stonewall also has modeled plating joints and rivets, midship bulwarks and stern rails. Small fittings and exact placement are representative. The two aft Armstrong guns are displayed in aft-quarter poses corresponding approximately to openings in the reference plan; no fire eligibility is inferred from these poses.

Measured source geometry compared with pass 2:

| Ship | Vertices | Source mesh parts | Render mesh sections |
| --- | --- | --- | --- |
| Niagara | 58,945 → 125,093 (2.12×) | 1,659 → 3,533 | 9 → 18 |
| Stonewall | 43,379 → 89,803 (2.07×) | 1,156 → 2,789 | 8 → 16 |

Render meshes are grouped by material and fore/aft section. The partition preserves triangle counts and original normals, avoiding a lighting seam at the section boundary. Slight extra vertex duplication at that boundary is recorded separately in the JSON count reports. The actual modeled batteries remain ten Parrott rifles on Niagara and three Armstrong rifles on Stonewall, per the selected scenario configuration.

Verification now checks the vertex budget, doubled render mesh counts, source part growth, preserved triangle counts, finite vertices, closed hull/deck boundaries, outward end caps, recessed bores, battery base support at 1,440 locations and the corrected hex-heading markers. The latter now start at the hex center with a separate arrow; the occupied-hex outline closes without its former diagonal. Pass-2 baseline source, OBJ files and counts are retained in `Artifacts/Pass2/Baseline`, and pass-3 inspection views and reports are in `Artifacts/Pass3`.

## Scenario

Additional Stonewall reference review: [NavSource photograph and plan findings](STONEWALL-REFERENCE-REVIEW.md). This records the evidence used for pass 3 and identifies details still requiring measured plans.

Default UTC time 14:00, weather, ship positions and the offshore location near 43.43°N, 8.65°W are chosen for presentation. They are not a reconstruction of observed weather or exact positions. The supplied rules date Scenario 4D to 24 March 1865; the user's duel omits Sacramento. Historical source accounts differ on some departure dates. This is explicitly a what-if.

## Earth, sun and weather

The globe uses the March 2004 NASA Blue Marble Next Generation cloud-free composite (5400 × 2700 source JPEG), credited to NASA Earth Observatory. It provides modern geographic land cover, not an 1865 map. No NASA endorsement is implied.

- [NASA Blue Marble base-map source and monthly downloads](https://science.nasa.gov/earth/earth-observatory/blue-marble-next-generation/base-map/)
- [Downloaded March texture](https://eoimages.gsfc.nasa.gov/images/imagerecords/73000/73992/world.200403.3x5400x2700.jpg)

Sun direction uses NOAA's fractional-year approximation with equation of time and solar declination. UTC and east-positive longitude are used; local axes are east/up/north. The globe transforms the same solar geometry into Earth-fixed coordinates for its day/night lighting. This is an approximate solar position, not an ephemeris-grade calculation. Atmospheric refraction is omitted.

- [NOAA solar equations](https://gml.noaa.gov/grad/solcalc/solareqns.PDF)

Ocean waves, ship roll, clouds, rain, fog, smoke and audio are generated locally. Wave scale is an artistic control, not a certified significant-wave-height model. Visual fog does not determine legal sighting or line of fire. The scene has no live weather feed or historical weather archive. There are no rendered local shorelines, harbor structures, tides, shoals, bathymetry, water-depth validation or terrain occlusion.
