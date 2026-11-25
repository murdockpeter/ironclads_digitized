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
