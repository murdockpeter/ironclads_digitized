# Detailed Hampton Roads coastlines — 1.4.1-dev

The modeled Hampton Roads coasts now use **GSHHG 2.3.7 full-resolution shoreline polygons** and **USGS 3DEP bare-earth elevation**, replacing the coarse Natural Earth / ETOPO crop. The geometry remains modern geographic data with historical scenery and is not an exact 1862 coast or a navigational chart.

| Measure | Previous | Updated |
| --- | ---: | ---: |
| Geographic shoreline points, excluding repeated closing points | 65 | 1,392 |
| Elevation sample grid | 95 × 68 | 377 × 269 |
| Elevation intervals along each axis | 94 × 67 | 376 × 268 |
| Rendered ground spacing | Adaptive triangles up to 220 m edge | Regular 55 m cells clipped to shore |
| Beach geometry | Independent straight strips | Continuous shared-edge curved profile |

Elevation sampling is **four times finer along each axis**, yielding approximately sixteen times the sample count. Geographic shoreline detail increases by over twenty times before bounded smoothing. Those measurements describe resolution; they do not establish a fourfold reduction in historical positional error.

## Shore appearance

The full-resolution geographic outline supplies smaller coves, points and islands. A quadratic corner treatment operates within a maximum 15-meter neighborhood, then resamples long edges at no more than 25 meters. This produces 16,002 rendered coastline samples without inventing large headlands. The unsmoothed clipped source remains in `Assets/StreamingAssets/GeographySource/Hampton-GSHHG-source.geojson`.

Beach strips share their corner vertices and wrap the actual geographic rings, including islands. Profiles run from submerged sand through wet sand, the waterline, dry sand and an inland meadow transition. Width varies smoothly and is constrained at narrow islands and channels. Vertex colors carry the inland fade through mesh combining. Artificial rectangular crop boundaries do not receive beaches.

Low irregular stones and bank outcrops appear in sparse groups; Rip Raps gains a ring of low stones around its existing reconstructed island. These are procedural scenery, not a geological survey. Hampton Roads remains predominantly low, sandy/marshy terrain rather than a cliff coastline. New beach and outcrop placement uses deterministic calculations so both TCP players see the same scenery.

The ground grid is clipped offline to the same shoreline. Shared vertex coordinates avoid cracks between independently subdivided triangles. Very small clipping slivers are removed, and surface normals are checked. Spatial indexes accelerate land tests and near-coast distance calculations; no live geographic downloads occur in the player. Fort Monroe's local platform and the existing forts, batteries, wharves, foliage and ship controls remain part of the scene.

Open **Scenarios / shores & forts → View Curved beach / shoreline outcrops** for a close inspection.

## Sources and redistribution

- [NOAA's GSHHG description](https://www.ngdc.noaa.gov/mgg/shorelines/shorelines.html) identifies the full-resolution shoreline database and its LGPL license. Downloads use the [official GMT GSHHG 2.3.7 release](https://github.com/GenericMappingTools/gshhg-gmt/releases/tag/2.3.7), since the older NOAA download endpoint has been retired.
- [USGS 3DEP elevation service](https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer) supplies floating-point bare-earth elevations. The requested crop covers 36.85–37.13° N, 76.56–76.17° W; pixel centers and north/south TIFF orientation are preserved when packaging the grid. See [USGS product information](https://www.usgs.gov/3d-elevation-program/about-3dep-products-services).
- Derived GSHHG data are **LGPL-3.0-or-later**, separately from project code. Original notices, LGPL/GPL texts and the clipped source accompany the player under `StreamingAssets/GeographySource`. The builder source in `Tools` records the crop, refinement, grid clipping and triangulation steps. USGS elevations are public-domain government data.

The detail can include modern land change or reclamation. Buildings, modern road bridges and container terminals are not generated. Elevation source resolution is distinct from shoreline horizontal accuracy. Beach profiles, boulders, foliage, fort corrections and surface colors remain visual interpretation. Grounding, collision, line of sight, shore fire and combat are still manual.

## Rebuild and test

```powershell
python -m pip install --target .tools/terrain-python numpy tifffile pyshp shapely
npm install --prefix .tools earcut --no-audit --no-fund
python Tools/fetch-hampton-coastline.py
python Tools/build-hampton-coastline.py
node Tools/triangulate-hampton-coastline.cjs
```

Use Unity 6000.2.12f1 and `Ironclads.Editor.ProjectBuilder.DeliverCoastlines` to build `Builds/Windows-Coastlines`. Geographic resources are bundled locally. `Artifacts/Coastline` contains the source cache, measurement report and close inspection render. The global globe and open-sea Ferrol/Ulsan modules retain their existing geography behavior; this changes the local coastal terrain supported by Hampton Roads.

The editor verifies elevation dimensions, shoreline/beach density, rocks, preserved beach fade colors, terrain normals, duel water and Fort Monroe land placement, ship batteries and switching modules. Use `.tools/verify-player.ps1 -Hampton` for the standalone regression.
