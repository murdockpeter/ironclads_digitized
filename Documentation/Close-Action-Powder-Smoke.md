# Cannon smoke and reports

The 1.8.4-dev local player adds a longer outward muzzle jet, persistent grey powder clouds and longer-range rolling reports. Launch with `./Start-Ironclads.ps1 -CloseAction`; the launcher prefers `Builds/Windows-CloseAction-Smoke`.

Smoke initially moves along each modeled barrel's actual direction. Reduced early drag lets the discharge push clear of the hull before it settles into ambient wind transport. The cloud then rises slowly, with coherent, slow eddies and small differences between puffs. Wind comes from the shared scenario weather, so strong wind still carries a cloud away rapidly across the sea. Calm weather leaves it hanging near the firing position. Smoke stays in world space when a ship moves.

Main puffs last 110–180 real-time seconds; smaller wisps last 75–110 seconds. Density stays high through the middle of a puff's life, then fades gradually. Dark, medium and pale greys are mixed at emission; gradual aging preserves that variety rather than whitening the whole cloud. Atlas texture variations, slow internal curls, soft intersections and slow expansion soften the particle outlines. Existing Clear powder smoke controls remain available.

The shared particle budget is 48,000. As accumulated smoke approaches that budget, new shots emit fewer puffs, preserving existing smoke rather than abruptly evicting older clouds. The effect is a bounded visual approximation rather than a fluid or visibility simulation. Gunnery legality and umpire decisions remain unchanged. Shared battery effects also apply to the other modules; the later-era module retains its reduced smoke amount and paler tint.

Cannon reports stay at their muzzle positions, with distance-based arrival delay and spatial audio. Their audible range is increased from 6 km to 12 km, distance attenuation is gentler, and the rolling decay lasts nine seconds. A larger voice pool supports overlapping broadsides. These values are artistic playback settings rather than a calibrated acoustic model.

The editor review checks outward travel, slow rise in calm weather, wind transport, stability at 30/60 fps, report attenuation and audio headroom. Standalone verification fires a real battery and checks emitted lifetimes, grey variation, persistence at roughly 45 and 95 seconds, and eventual disappearance. It captures the smoke at those ages for visual review.
