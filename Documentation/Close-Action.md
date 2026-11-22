# Close Action: Nelson vs Brueys

Launch from the repository with:

```powershell
.\Start-Ironclads.ps1 -CloseAction
```

The standalone executable also accepts `-close-action`. Existing games offer the new module under **Scenarios**. This is an umpire-assisted tabletop emulator, with published movement and normal gunfire calculations. It is not a complete implementation of every Close Action rule.

## Scenario selection and historical setting

The module implements Scenario 6, **Fleet Action at Sea: Nelson vs. Brueys**, dated 23 June 1798. The supplied scenario book's printed pages 15–16 describe a hypothetical encounter during the French expedition to Egypt. Near Cape Passaro the fleets historically passed close without fighting. The scenario explores Nelson encountering the French battle fleet at sea while four of his liners pursue the expedition's transports elsewhere.

The baseline has ten British ships and thirteen French ships. The book recommends 12 players minimum and 18 ideally; players may command multiple ships. The emulator supports up to 32 connections including the host, allowing a separate umpire and one captain per ship. It retains the published alternatives: Goliath replaces Leander, Conquerant is detached and following French ships advance four hexes, or Goliath joins the British rear. Those setups have 23, 22, and 24 ships respectively.

British ships: Zealous, Audacious, Vanguard, Defence, Leander, Minotaur, Theseus, Culloden, Alexander and Swiftsure. French ships: Spartiate, Guerrier, Franklin, Aquilon, Tonnant, Peuple Souverain, Orient, Conquerant, Heureux, Mercure, Guillaume Tell, Timoleon and Genereux. Flags identify Nelson, Troubridge, du Chayla, Brueys and Villeneuve. The fleet values total 849 British and 926 French VP. Each ship has its own published crew quality, morale, gun factors, rigging, hull and crew boxes; opening damage is retained.

This is an open-sea encounter using the standard LONG arrangement. Both fleets must scatter before play. The nominal positions are supplied, with an explicit setup acknowledgment; the umpire rolls scattering and applies the book's diagram using bow addresses or Shift-click placement. French ships begin at MS; British opening sails are selectable. No anchoring permission, escape permission, or tournament turn limit is added. Victory points are tracked; the umpire records the agreed end of the action. Initially lost hull boxes do not earn new damage VP.

The publisher's errata changes this scenario's initial **wind direction to 3**. WS and sea state are both 3. Direction means the direction **toward which** the wind blows. Oceanic direction and Steady speed changes can be rolled for the next turn; sea state remains fixed. Noon UTC and the approximate sea location are lighting presets, not additional scenario facts.


The visible **Weather / wind** shortcut opens **Sea / sky**, with cloud, rain, visual visibility and wave controls. **Wind / turns** lets the host apply current wind before orders reveal, or queue wind for the next turn. Changing current wind clears sealed plots for resubmission. See the [hull and weather guide](Close-Action-Hull-Contours.md) for the full controls.

## Playing a turn

1. In **Fleet / setup**, review scattering, deployment and initial sail; check the setup acknowledgment. Advance to Plot.
2. Each captain uses **Orders / sail** to enter ahead counts and P/S turns, plus tasks for four sailor sections and marines. Submit `0` deliberately to remain stopped. Examples of plot notation are `3`, `2P1`, and `0`; legality depends on current sail, wind, acceleration and turn spacing. Seal the order. The host can supply legal straight coast orders for missing ships.
3. Reveal all orders together. Execute simultaneous movement one MP segment at a time. Turns pivot the model's two-hex footprint about the bow. Contested hexes pause movement for an umpire decision; temporary stern overlap, bearing off and collisions require that decision.
4. In **Gunnery**, choose the target, battery halves, high/low aim and shot type. Seal declarations, then reveal together. Published BGN, modifiers, dice, FGN and result notation are displayed. Resolve each shot; damage is queued until Combat ends. Record complex critical results separately in the ship log.
5. In **Ship log**, adjudicate maintenance and complete the seven checks. Adjacent sail changes complete according to their crew/time requirements. Signals are delivered, the clock advances 200 seconds, and the next turn begins.

The chart uses 67.5-yard hexes (61.722 m) and two occupied hexes per ship. Models retain physical dimensions rather than being stretched to fill the counter. A/B addresses use a 52-row join; extended sea room uses axial coordinates. **Fleet / setup** accepts either a book address such as `A1435` or an axial `q,r` pair. Overhead view, bow/stern footprints, battery arcs and plotted paths are available. Coordinates are an emulator convention; check deployment against your physical map before a match.

## Rule coverage and remaining umpire work

| Area | Implemented assistance | Umpire responsibility |
| --- | --- | --- |
| Setup and ship logs | Published force lists, variants, initial damage, sails, quality, morale and factors | Scattering diagram, seating and command organization |
| Turn sequence | Five phases, private plots, simultaneous movement, combat damage queue, 200-second turns | Completing each phase and any disputes |
| Wind and movement | Verified L-f/L-s cards, rig/sail limits, acceleration/deceleration, two turns, spacing, bow pivot, wind-facing limits | Bearing off, contested hexes, collision effects, poor maneuvering corrections |
| Stationary turns | Published tack target with d10, crew/quality/rigging adjustments | Unusual tack exceptions, drift, towing, anchoring, veering and irons effects |
| Crew and sails | Five section tasks, missing-crew checks, adjacent sail changes, continued crew and completion time | Repairs, firefighting, boats, transfers, boarding and other task outcomes |
| Gunfire | Four battery halves, two-hex range, 60-degree arcs, ship LOS and friendly clearance, initial broadsides, modifiers, high/low results, crew allocation, damage queue | Unusual LOS/rake disputes, critical effects, marine fire, boarding and special cases |
| Damage and morale | Box removal, gun penalties on new hull-section losses, strike on complete hull/crew loss, morale roll entry | Triggering/reconciling each required morale or rigging check, critical effects and captures |
| Signals | Sealed 3/5-word signals, ship names as one word, end-turn delivery, side-filtered network delivery | Wording disputes, short-range exchanges, group/address eligibility and other advanced communication rules |
| Maintenance | Explicit seven-step checklist, automatic completed sail alterations and signal delivery | Applying drift, grapples, fouling, fires, explosions, repairs and related checks |
| Victory | Fractional damage VP and full struck/captured VP; agreed outcome | Scenario termination and any tournament/house conditions |

The official errata's **−12 two-target modifier** is used instead of the original −10; it replaces the half-battery penalty. Chart-dependent overrides remain available for an umpire ruling. They are visibly entered values rather than hidden house rules. The yellow critical-hit sheets and auxiliary sheets can be opened locally from **Briefing**. A reported critical provides a roll and a reminder, not an automatically applied critical effect.

Sealed movement, fire declarations and pending signals live outside broadcast scenario state until reveal/delivery. The host is a trusted umpire and can inspect the whole table. Guests send orders only for assigned ships in the current turn and assignment generation. Ownership changes invalidate old sealed movement orders. Delivered signals are sent only to connections owning friendly ships; a player assigned ships on both sides can receive both sides' signals. The direct TCP connection uses the existing port setting (default 7777). Protocol 9 builds must match.

## Visual ship detail

Ten class/family prototypes (four British and six French) represent the fleet. [Visual revision, model assignments and museum sources](Close-Action-Visuals.md). British 50s and 74s, French 74s and 80s, and Orient's 120-gun three-decker have distinct profiles. They include coppered underbodies, planked sides with actual gunport openings, raised lids and port framing, turned barrels with open muzzles, wheeled carriages and breeching ropes, stepped decks, hatches and gratings, capstans, wheels, boats, anchors, stern galleries and glazing. Three masts carry shrouds, ratlines, stays, braced yards, topsails, courses, topgallants, royals, jibs and spanker. Sails respond to the log's sail state and remaining rigging; yards trim with wind. Each ship has its own stern name. Broadside smoke and reports originate at individual gun mounts. Sailing ships have no coal exhaust.

Physical gun counts match the nominal ratings; game gun factors come from the scenario and are independent of those visual counts. Representative dimensions and fittings are reconstructions by nationality/rating, not claims of exact individual hull plans or fittings on 23 June. National ensigns are simplified. Rigging loss reduces sail capacity; individual critical mast/yard casualties require umpire interpretation and are not separately animated.

The ten class/family prototypes retain the increased mesh detail and 50–120 muzzle attachments each. See the geometry comparison for the preceding visual revision and the generated hull reports for current offsets. Generated prefabs share meshes and materials across ships. Resources are rebuilt by `Ironclads.Editor.CloseActionReview.PrepareModels`; increment its model revision when changing procedural geometry. Source checkout play can generate a model when a baked prefab is absent, with additional startup cost. `Ironclads.Editor.ProjectBuilder.DeliverCloseAction` prepares models, checks the module and builds `Builds/Windows-CloseAction-IntegratedBows/Ironclads.exe`.

## Saves and references

**Save / Load** uses `close-action-scenario.json` in the application's persistent data directory, independently of the Ironclads table save. Offline saves retain sealed drafts, pending fire damage and signals. Online loads retain the public table but require captains to resubmit private orders. The CSV button appends public ship positions and logs to `close-action-log.csv`. Only the host loads or advances the shared table.

The supplied books remain in `Documentation/CloseActionRules.pdf` and `Documentation/CloseActionScenarios.pdf`. The launcher restores them into this local player's reference directory. Locally retrieved chart photographs are stored under ignored `Assets/StreamingAssets/References/CloseAction-*.jpg`. The scanned books and chart photographs are excluded from public source and release packaging. Numerical mechanics and the original emulator code are included. This is not an affiliation with or endorsement by the publisher.

Sources used:

- User-supplied *Close Action* rules, with particular attention to ship logs, sequence of play, weather, sails, movement, combat, maintenance, morale and signalling; scenario book, printed pp. 15–16.
- [Clash of Arms official errata](https://clashofarms.com/files/close-action.pdf), including Scenario 6 WD3 and the two-target modifier.
- [BoardGameGeek game gallery](https://boardgamegeek.com/boardgame/3553/close-action-the-age-of-fighting-sail-vol-1/images): [movement](https://boardgamegeek.com/image/518058), [gunnery](https://boardgamegeek.com/image/518054), [results](https://boardgamegeek.com/image/518048), [hull criticals](https://boardgamegeek.com/image/518056), [rigging criticals](https://boardgamegeek.com/image/518071), [auxiliary tables](https://boardgamegeek.com/image/518070).
- Royal Museums Greenwich collection references for reconstruction context: [74-gun inboard profile](https://www.rmg.co.uk/collections/objects/rmgc-object-81150), [Vanguard/Arrogant lines](https://www.rmg.co.uk/collections/objects/rmgc-object-80681), [74-gun lines](https://www.rmg.co.uk/collections/objects/rmgc-object-80933), and [Battle of the Nile painting](https://www.rmg.co.uk/collections/objects/rmgc-object-12007).
