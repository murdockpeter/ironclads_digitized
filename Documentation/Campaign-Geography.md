# Geographic James River campaign map — 1.9.2-dev

The Breakout campaign now uses real-world shoreline, river-area and elevation data. Simplified ship markers remain independent of the geographic basemap. Launch with `./Start-Ironclads.ps1 -Campaign`.

## Data already in the repository

The existing Hampton Roads geographic pipeline provided the starting point:

- `Artifacts/Coastline/gshhg-shp-2.3.7.zip` and `Artifacts/Coastline/GSHHS_shp/f/GSHHS_f_L1.*` cache the **global full-resolution GSHHG 2.3.7 shoreline database**. The campaign builder crops that same local input; it does not download another global coastline.
- `Tools/fetch-hampton-coastline.py` and `Tools/build-hampton-coastline.py` establish the existing **USGS 3DEP** elevation acquisition and terrain packaging approach.
- `Assets/Resources/Geography/HamptonRoads.json` and `HamptonSurface.bytes` cover only the harbor-area crop, not the full upper James. They remain the tactical modules' resources.
- `Assets/StreamingAssets/GeographySource` already contains GSHHG attribution and LGPL/GPL license texts, retained for the expanded geographic map.

The older Natural Earth / NOAA ETOPO pipeline is also recorded in `Tools/build-hampton-geography.cjs` and the Hampton geography documentation. The campaign uses the more detailed GSHHG / 3DEP inputs rather than replacing those existing resources.

## Expanded theater and river banks

The theater crop is **77.60–75.80° W, 36.75–37.65° N**. It includes the upper James, Trent's Reach area, Deep Bottom, City Point, lower James, Hampton Roads, Virginia Capes and nearby Atlantic.

GSHHG is a coastal dataset and does not by itself supply all narrow upper-river water areas. The builder supplements it with **256 USGS NHD high-resolution river/bay/canal-area features** from the [National Map NHDArea service](https://hydro.nationalmap.gov/arcgis/rest/services/nhd/MapServer/9). It requests FTYPE 460 (river), 312 (bay/inlet) and 336 (canal/ditch), using bounding-box filtering and paginated OBJECTID order. The actual polygon areas, not arbitrary buffered centerlines, cut the river water out of the coastal land polygons. Interior holes and islands are retained. A topology-preserving roughly 1–2 m simplification removes redundant coordinates; it does not widen the river for display.

The derived land contains **282,009 shoreline coordinates**. A 250 m land grid is clipped to those banks, keeping the more detailed source vertices in boundary cells. The packaged terrain has **505,896 vertices and 686,232 triangles**. Float32 winding checks discard five collapsed boundary slivers and ensure faces point upward at theater-sized meter coordinates.

Elevation comes from the [USGS 3DEP bare-earth service](https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer), sampled to **1,025 × 641** cells across the full theater. The source pixel-center and north/south orientation are preserved. Source data are finite throughout this crop. Terrain samples that grid without vertical exaggeration; rendered land is raised to at least one meter above the visual sea surface. Elevation and vegetation coloring describe present-day source relief and procedural appearance, not historic land cover.

The [USGS NHD product page](https://www.usgs.gov/national-hydrography/access-national-hydrography-products) describes this dataset and service access. GSHHG comes from the [official 2.3.7 release](https://github.com/GenericMappingTools/gshhg-gmt/releases/tag/2.3.7). The [Virginia DWR Deep Bottom landing](https://dwr.virginia.gov/place/deep-bottom/) provides a geographic reference near that station preset.

## Coordinates and controls

The map uses an east/north local meter projection centered on **37.20° N, 76.70° W**, consistent with the project's existing local geographic convention. Positive X is east; positive Z is north. Latitude and longitude convert using a spherical Earth radius of 6,371,000 m and a fixed longitude scale at the origin latitude. This is a regional approximation, not a survey projection: scale distortion grows away from the reference latitude.

North up gives a top-down geographic view. Sector buttons zoom into narrow parts of the upper river. **WASD pans relative to the current view; hold Shift for three times the speed.** In north-up view W is north and D is east. Pan speed scales with zoom. Typing in text fields suspends keyboard panning; click the map to resume. Middle mouse drag also pans, right mouse drag orbits, and the wheel zooms. A camera-dependent scale bar reports projected meters/kilometers at the view center. Alt-click measures projected straight-line distance from an assigned ship, in nautical miles and yards; it does not calculate a navigable river route.

Station anchors are approximate operational presets selected near the named areas, checked against the derived water polygon and, where necessary, snapped to adjacent water. The build report records coordinates and the snap distances (0–280 m for these presets). They are not historical position evidence. Exact umpire placements can use latitude/longitude or direct map clicks. Marker headings now use degrees true, with north at zero.

Ship meshes enlarge with overview zoom so they remain selectable. Their center positions retain geographic coordinates; the drawn hull lengths and selection rings are not to scale. Unassigned vessels occupy explicitly labeled staging panels outside the geographic crop. The panels do not represent real naval stations or known positions.

The entire board is private umpire truth. Real coordinates are not automatically added to player exports. Release reports explicitly when players should learn a position.

## Historical and navigation limits

This is a **modern geographic basemap**, not an 1864 reconstruction. Later river cutoffs, canals, reclamation and shoreline changes may be present. For the December 1864 KS, declare historically applicable channels, obstructions and forbidden modern shortcuts through umpire orders and rulings. A modern water polygon establishes neither 1864 accessibility nor sufficient depth for an ironclad.

No bathymetry, tide model, dredged-channel clearance, grounding, line of sight, detection, shore fire or automatic route legality has been added. The data provide a real geographic setting and measured relative positions. Combat and navigation remain freely adjudicated.

## Resources, licensing and rebuild

Runtime resources:

- `Assets/Resources/Geography/JamesCampaign.json` — bounds, source metadata, station coordinates, shoreline rings and elevation grid.
- `Assets/Resources/Geography/JamesCampaignSurface.bytes` — clipped terrain mesh in projected meters.

Redistributed geographic source and attribution:

- `Assets/StreamingAssets/GeographySource/JamesCampaign-land.geojson` — derived clipped land geometry.
- `Assets/StreamingAssets/GeographySource/JamesCampaign-NHD.geojson` — retrieved USGS polygon features.
- `Assets/StreamingAssets/GeographySource/JamesCampaign-NOTICE.txt` — source/license information and exact service requests, alongside the existing GSHHG license texts.

GSHHG-derived geographic data remain **LGPL-3.0-or-later**, separate from project code. USGS geographic/elevation data are public-domain government data. Geometry builders and source provenance are retained so the geographic package can be regenerated. No GIS downloads occur during play.

With the same local Python/Node dependencies as the Hampton coastline pipeline:

```powershell
python Tools/build-campaign-geography.py
node Tools/triangulate-campaign-geography.cjs
python Tools/verify-campaign-geography.py
```

The Python builder reuses cached GSHHG inputs and caches the NHD query and 3DEP TIFF under `Artifacts/CampaignGeography`. Subsequent builds use those cached inputs. It emits source geometry and an intermediate mesh grid; Node Earcut triangulates the clipped boundary cells and writes the runtime files.

Build the Windows player with `Ironclads.Editor.ProjectBuilder.DeliverBreakoutGeography`. Output is `Builds/Windows-CampaignGeography`. Checks include connected water from all station presets to the Atlantic, source bounds, land/water references, elevation dimensions, binary mesh layout, coordinate round trips, metric scale, terrain normals, campaign save migration and report secrecy. `Tools/Verify-Campaign.ps1` runs the standalone player and captures the overview, upper James, Hampton Roads, ship inspection and campaign tabs.


## Land-cover coloring and marker detail (1.10.16-dev)

The campaign land shader now reads a georeferenced **4096 x 2560 NLCD 2021 thematic color texture**, replacing the invented procedural forest/meadow pattern on this board. The existing tactical coast shader path remains the default elsewhere. The source crop uses nearest-neighbor class sampling, approximately 39 m east/west and north/south near the map center; its source classification is 30 m NLCD. Neither the sampling interval nor this visual pass establishes a twofold statistical accuracy improvement.

Forest, pasture/crops, grass/shrub, developed land and wetlands follow the mapped classifications. A muted late-season palette interprets these classes; it is not satellite RGB or a verified December 1864 reconstruction. Modern developed areas remain present and are identified by the board's modern-land-cover label. NoData uses a neutral fallback; source water classes occurring inside the separate shoreline mask use a muted wet-bank color. Land/water boundaries, station coordinates, elevations, distances and navigation adjudication are unchanged.

Source: [USGS/MRLC NLCD](https://www.mrlc.gov/about), [class legend](https://www.mrlc.gov/data/legends/annual-nlcd-land-cover-legend). The WCS request, 2021 layer identifier, bounds, palette, source SHA-256 and class histogram are saved in `Assets/StreamingAssets/GeographySource/JamesCampaign-landcover-NOTICE.json`. This public-domain government land-cover input is separate from the existing GSHHG-derived geometry licenses.

Rebuild the cached color resource with:

```powershell
python Tools/build-campaign-landcover.py
```

The script downloads only the theater crop, caches the raw TIFF under `Artifacts/CampaignGeography`, validates its EPSG:4326 transformation and generates `Assets/Resources/Geography/JamesCampaignLandCover.png`. The player performs no GIS requests. The editor explicitly retains the full texture dimensions with clamp addressing and mipmaps.

Generic operational ship markers now include refined rounded bows, separate barrels, bridge glazing, stowed boats, hatches, mooring bitts, rails, funnel bands and yards. The monitor adds ventilators, access covers and a pilot position. They remain enlarged operational symbols, not replacements for the tactical ship models or researched individual silhouettes.

| Operational marker | Previous vertices | New vertices | Previous meshes | New meshes |
| --- | ---: | ---: | ---: | ---: |
| Ironclad / gunboat / support | 787 | 3,223 | 7 | 28 |
| Double-turret monitor | 987 | 4,386 | 10 | 40 |

The marker comparison is written by `CampaignVisualReview` to `Artifacts/CampaignGeography/marker-detail-comparison.csv`. Counts include all primitive mesh components, with repeated instances counted individually. Mesh count is exactly four times the previous marker; vertex count must be between four and five times its previous value. This measures geometry, not draw calls or frame rate.

Build and verify:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverBreakoutVisuals -logFile Artifacts/CampaignGeography/build-visuals-final.log
.\Tools\Verify-Campaign.ps1 -BuildDirectory Builds/Windows-CampaignVisuals -OutputDirectory Artifacts/CampaignGeography/Runtime-Visuals-Final
.\Start-Ironclads.ps1 -Campaign
```

The launcher prefers `Builds/Windows-CampaignVisuals`; earlier campaign builds remain available. The standalone verifier captures the map, ironclad, gunboat and monitor and checks camera navigation, map material binding, marker counts, placement, tactical-state isolation and campaign save/report behavior.

Validation: 37 campaign editor checks and 31 geography checks passed, alongside the land-cover import and marker geometry/budget checks. The final standalone suite passed all 19 marker counts, mapped shader/texture binding, WASD/typing/Shift controls, placement, live clock, tactical isolation, saves and camera restoration, with no logged runtime errors. The map and marker captures were visually reviewed. **Inspect ship** now leaves a prior north-up/top-down mode and opens the marker in perspective. Final reports and images are in `Artifacts/CampaignGeography/Runtime-Visuals-Final/`.


## Revised KS problems and second marker pass (1.10.17-dev)

The user-supplied `Hampton_Roads_Confederate_KS_Problem.pdf` and `Hampton_Roads_Union_KS_Problem.pdf` supersede the initial vessel inventory for new campaigns. Their scenario OOB v1.0 names **10 Confederate and 15 Union ships**. Each named vessel has its own selectable, placeable, movable marker. A further **31 symbolic support-resource groups** cover the listed defenses, signals, working parties, stores, workshops, transports, boats, pilots, tugs, torpedo fields and obstructions. Group icon details do not specify the number of actual boats, personnel, guns or mines available.

The [full marker catalog](Hampton-KS-Marker-Catalog.csv) records all 56 identities, sides, types, guide starting areas, guide armament/restrictions and supplied commander assignments. The runtime source is `Assets/Resources/Campaign/Hampton1864OOB.json`, with source PDF checksums. Both source PDFs are retained under `Assets/StreamingAssets/CampaignBriefings` for deliberate viewing from the private desk's Plans panel. Opposing briefings are not merged into side exports.

Added named ships are CSS Patrick Henry and Roanoke, plus USS Massasoit, Spuyten Duyvil, General Putnam, Commodore Barney and Dawn. Scorpion is absent from these revised guides and is not created in new campaigns. Existing saves retain their Scorpion as an explicitly identified legacy marker for umpire review.

The printed guide window is 22 December 1864 1800 to 23 December 1864 1800. It is recorded as scenario guidance, without an automatic game-ending rule. The PDFs' printed live-event date is 19 December 2026; the tool does not schedule a calendar event. Their planning, command and authorization instructions are scenario content for the human commanders and umpire, not requests for the software to execute operations.

### Placement and live use

1. Open **Markers**, choose **Choose from full roster**, and filter Ships/Support and Union/Confederate as needed.
2. Select an asset. Its source starting area, armament and restrictions appear above the umpire's actual-status fields. Exact readiness, supplies and positions remain unassigned until confirmed.
3. Enter an initial setup note or live movement ruling, then place by map click, decimal coordinates or river-sector preset. Shore assets should use point/coordinate placement; river presets are water anchors.
4. During live play, apply the umpire's adjudicated movement by the same controls. The clock and queued orders do not move assets automatically.

All source starting areas are labels, not invented exact geographic coordinates. Patrick Henry's authorization requirement, Roanoke's repairs and Torpedo's uncertain machinery are retained as visible source restrictions. Guide gun counts of one, two and four control the corresponding Confederate ship symbols; unspecified Union armaments remain representative rather than researched battery reconstructions.

### Marker geometry

Compared with the preceding 1.10.16 markers:

| Ship symbol | Previous vertices | New vertices | Previous meshes | New meshes |
| --- | ---: | ---: | ---: | ---: |
| Ironclad / gunboat / support / school ship / torpedo vessel | 3,223 | 14,167 | 28 | 112 |
| Double-turret monitor | 4,386 | 18,242 | 40 | 160 |

Mesh components are exactly quadrupled. Vertex counts increase about 4.40 and 4.16 times, respectively. Counts include repeated mesh instances; they are not draw-call measurements. Added detail includes denser bows, individual rail stanchions, armor seams, coamings, ventilators, boat thwarts, funnel stays, mooring rings and monitor turret fittings. These remain generic enlarged operational symbols, not individually researched ship models. New resource symbols have no previous-version geometry baseline.

### Save compatibility

Campaign schema 3 accepts the revised catalog. Versions 1 and 2 migrate without changing existing assigned geographic positions (version 1 retains its established schematic-to-geographic migration), live clock, commander edits, readiness, orders or released reports. Missing OOB assets are staged off map; they are not deployed automatically. Legacy Scorpion is retained. The original loaded file is archived as `.pre-oob-v3` before a later save, and normal save backups continue. A migrated campaign can therefore contain 57 markers rather than the fresh campaign's 56.

The board rebuilds its marker list when a different roster is loaded. Navigation and selection use the actual roster length rather than a fixed 19-ship list. New and migrated rosters can be loaded during the same player session.

Build with `Ironclads.Editor.ProjectBuilder.DeliverBreakoutOob`; output is `Builds/Windows-CampaignOOB`. Launch with `./Start-Ironclads.ps1 -Campaign`. Run `Tools/Verify-Campaign.ps1 -BuildDirectory Builds/Windows-CampaignOOB -OutputDirectory Artifacts/CampaignGeography/Runtime-OOB` for movement, save and visual checks.

Validation for 1.10.17-dev: 40 campaign editor checks and 31 geography checks passed, plus six ship-family geometry comparisons and all 31 support icon constructions. Independent extraction matched all 10 Confederate and 15 Union ship names to the supplied PDFs and checked their retained SHA-256 values. The standalone suite passed placement/movement and click-target/layer checks for all 56 fresh-campaign markers, ship mesh budgets, camera controls, save round trips and tactical-state isolation, with no logged runtime errors. Ship and shore-battery captures were visually reviewed; both source PDFs were confirmed in the packaged player. Reports/captures: `Artifacts/CampaignGeography/Runtime-OOB/`.


## H: full-window map view (1.10.18-dev)

Press **H** while the operational map has focus to hide or restore the entire campaign UI. The hidden view fills the window and suppresses the dock, toolbar, footer, labels, scale/range overlays and selection ring. WASD, Shift, orbit, pan and zoom continue to work across the full viewport. Press H again to restore the previous controls and selected marker.

Typing H in a campaign text field does not toggle the UI. Click the map to leave text editing first. Hidden mode does not accept placement clicks, preserving any pending placement until the controls are restored. The toggle is local visual state; it does not alter the live clock, orders, campaign data or save format. Autosaving continues normally.

Build: `Ironclads.Editor.ProjectBuilder.DeliverBreakoutScreenshots`, output `Builds/Windows-CampaignScreenshots`. The campaign launcher now prefers this player. Clean-map capture and regression reports are under `Artifacts/CampaignGeography/Runtime-UI/`.

Validation for 1.10.18-dev: the standalone suite passed the typing guard, full-window camera rect with zero dock width, camera movement while hidden, and dock restoration with unchanged campaign JSON. The clean-wide capture was visually reviewed for absent panels, labels and selection/range overlays. Existing marker movement, save/load, tactical isolation and camera-restoration checks also passed with no logged runtime errors.
