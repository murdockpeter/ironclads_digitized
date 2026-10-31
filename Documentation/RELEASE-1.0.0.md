# Ironclads — Ferrol 1.0.0

A Unity-based 3D virtual sea table for an umpire-run USS Niagara versus CSS Stonewall encounter. The umpire controls movement and adjudicates all combat.

- Procedural ship reconstructions with the rounded Niagara stern and Stonewall's fixed, hull-integrated forward gunroom.
- Ocean, geographic globe, historical-date sun position, adjustable wind/cloud/rain/waves and ambience.
- 100-yard hex chart, six facings, range/bearing rulers, adjustable gun-arc guides, manual turn clock, notes, undo, table saves and CSV position logging.
- Ship-specific battery salvos, muzzle flashes, stronger synthesized cannon reports and variable wind-driven powder smoke.
- Dark drifting coal exhaust from Niagara's two funnels and Stonewall's single funnel, with independent visual engine orders and saved boiler settings.

## Run on Windows

Download `Ironclads-Ferrol-v1.0.0-Windows-x64.zip`, extract the whole archive, and launch `Ironclads.exe`. Retain the accompanying `_Data` folder, `UnityPlayer.dll` and all other files. This is a Windows x64 desktop build. Unity is required only for source development (6000.2.12f1).

Use the Ships tab to inspect/place ships and access **Engines & funnel smoke**. **Gunnery tools / bearings & arcs** and **Fire batteries / smoke & sound** open the measuring and visual-firing controls. Right mouse orbits; the wheel zooms; middle mouse pans; Shift-click places; Alt-click measures a target hex; H hides the interface.

## Scope and exclusions

This release does not automate loading, hits, damage, movement allowances, collisions or victory conditions. Engine orders do not move ships. Historical hull lines, fittings, atmosphere and sound remain reconstructions/approximations. Niagara's exact broadside pattern card is not reproduced; custom sectors are measuring guides. The globe is separate from the local sea board; detailed coastal terrain is not included.

Rulebook and ship-sheet scans, book-page photographs, third-party research photos and the private `contextonly` folder are not distributed. Missing optional references produce an explanatory message. NASA Blue Marble imagery is credited in the accompanying notices and historical notes. No third-party game license or publisher endorsement is claimed.

## Validation

The public player is built from a clean source-only staging project without private reference assets. Preparation verifies chart/nav/solar invariants and regenerated model assets. Standalone checks exercise placement/undo, turn timing, gun arcs, salvos, cannon waveform bounds, variable smoke drift, all three exhaust outlets, engine setting persistence, and the globe/weather/reference-panel paths. Automated cannon tests are muted. Direct3D 11 is used for unattended graphics captures.
