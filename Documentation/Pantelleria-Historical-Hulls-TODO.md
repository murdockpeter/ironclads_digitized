# Pantelleria Historical Hull Research TODO

Replace the common procedural hull templates with historically researched hull shapes for the module's 42 ships, using configurations appropriate to **14 February 1901**. The current **24 model variants** are the starting research groups. Research may establish that some groups need splitting, or that others can share a documented hull definition.

The [ship tracker](Pantelleria-Hull-Research.csv) has one row per vessel. The [source register](Pantelleria-Hull-Sources.csv) records drawings, photographs and dimensional references. Use the [reference worksheet](Pantelleria-Hull-Reference-Template.md) to assemble the evidence for each model group and its individual ships.

## Canopus replacement prototype

The first [Canopus swap-in prefab](Canopus-Replacement-Prototype.md) uses an independent model and runtime attachment points. Its silhouette is researched; underwater sections remain inferred. This establishes a replacement-model workflow while historical verification continues.

- [x] Deliver a Canopus-only replacement prefab with persistent meshes and materials.
- [x] Record working dimensions, interpreted stations, source IDs and inferred regions in its sidecar and prototype guide.
- [x] Provide a local comparison switch and retain the previous model as fallback.
- [x] Bind gun mounts, turret pivots, exhaust, colours, weathering and hull contact to the replacement.
- [ ] Replace inferred sections with verified lines and complete the dated configuration research.

## Existing materials

- [x] Inventory all 42 ships and their 24 current model assignments from [the scenario roster](../Assets/Scripts/PantelleriaScenario.cs).
- [x] Identify the current hull generator in [PantelleriaShips.cs](../Assets/Scripts/PantelleriaShips.cs): common station formula, length/beam scaling, freeboard and tumblehome variants.
- [x] Register the locally supplied scenario/rules PDFs and their page counts. Their relevant drawing coverage remains to be catalogued.
- [x] Create a separate reference checklist for every ship.
- [ ] Inspect the local books for useful profiles, plans, dimensional references and cited bibliographies; enter exact printed and PDF page numbers in the source register.

The local rules and scenario books provide roster and game-reference context. Dimensions in the current class definitions are provisional visual estimates. Neither is sufficient by itself to establish a historically verified hull surface.

## Starting batch

Complete four pilot reference collections before applying the workflow to the remaining groups:

| Priority | Ship | Current model group | Purpose |
| --- | --- | --- | --- |
| P1 | Canopus | Canopus | Establish the British workflow and a direct comparison with the present model. |
| P1 | Re Umberto | Re Umberto | Establish the Italian workflow and check its distinct proportions and arrangement. |
| P1 | Bouvet | Bouvet | Establish the French workflow, including evidence for its tumblehome and end shapes. |
| P1 | Tri Sviatitelia | Tri Sviatitelia | Establish the Russian workflow and drawing/transliteration conventions. |

P2 rows in the tracker follow these pilots. The pilot selection is a development sequence, not a finding that their drawings are easier to obtain.

## Evidence required for each hull

- [ ] Confirm vessel identity, historical class, alternative names/transliterations, builder and configuration date.
- [ ] Find a dated side elevation showing the stem, stern, sheer and deck heights.
- [ ] Find a deck or half-breadth plan showing the outline and major openings/positions.
- [ ] Find a body plan, station sections, waterlines or offset table sufficient to establish the hull's three-dimensional form.
- [ ] Verify principal dimensions, keeping length overall, length between perpendiculars and waterline length separate. Record maximum/waterline beam, design and loaded draft, displacement condition, freeboard and units when available.
- [ ] Gather dated bow, stern and broadside photographs; use them to cross-check the drawings and ship-specific changes.
- [ ] Record drawing scale, scan distortion, catalogue identifiers, publication/drawing dates, exact pages, links and reproduction terms.
- [ ] Compare conflicting references and document the selected interpretation.
- [ ] Decide which sister ships can share the hull and which require different geometry or fittings.

Profiles and deck plans can support a better silhouette while underwater sections remain inferred. Track that as partial coverage; reserve **lines verified** for geometry supported by adequate section/offset evidence. A named class or a unique weathering pattern does not establish a unique historical hull.

## Ship and model backlog

Each checkbox means the group's reference collection and sister-ship comparison are complete. Individual ship coverage remains visible in the tracker.

### Britain

- [ ] **Canopus:** Canopus, Ocean.
- [ ] **Majestic:** Caesar, Illustrious.
- [ ] **Renown:** Renown.
- [ ] **Royal Sovereign:** Revenge, Royal Oak, Ramillies, Empress of India, Royal Sovereign.
- [ ] **Hood:** Hood; establish which hull/freeboard features and fittings differ from the Royal Sovereign group.
- [ ] **Admiral:** Camperdown, Anson; verify whether the current shared assignment is appropriate.

### Italy

- [ ] **Re Umberto:** Re Umberto, Sicilia, Sardegna.
- [ ] **Ruggiero di Lauria:** Ruggiero di Lauria, Andrea Doria, Francesco Morosini.
- [ ] **Italia:** Italia, Lepanto; check each vessel against its own dated drawings and photographs.

### France

- [ ] **Charlemagne:** St. Louis, Gaulois, Charlemagne.
- [ ] **Bouvet:** Bouvet.
- [ ] **Charles Martel:** Charles Martel.
- [ ] **Jauréguiberry:** Jauréguiberry.
- [ ] **Brennus:** Brennus.
- [ ] **Bouvines:** Amiral Tréhouart, Bouvines.
- [ ] **Jemmapes:** Valmy, Jemmapes.
- [ ] **Terrible:** Terrible.
- [ ] **Marceau:** Marceau.

### Russia

- [ ] **Imperator Aleksandr II:** Imperator Aleksandr II.
- [ ] **Dvenadtsat' Apostolov:** Dvenadtsat' Apostolov.
- [ ] **Tri Sviatitelia:** Tri Sviatitelia.
- [ ] **Rostislav:** Rostislav.
- [ ] **Ekaterina II:** Ekaterina II, Chesma, Sinop.
- [ ] **Georgii Pobedonosets:** Georgii Pobedonosets; compare against the other current triangular-battery models rather than assuming their hulls are interchangeable.

## Research collection workflow

- [ ] Start with the local references, then search naval/museum/archive drawing catalogues and cited technical publications.
- [ ] Register each candidate source once, with a stable source ID. Enter its exact relevance and verification status before treating it as geometric evidence.
- [ ] Link the relevant source IDs to every applicable ship row. Class-wide plans may be shared, but each vessel still needs a configuration and differences check.
- [ ] Store collected research under `contextonly/PantelleriaHullResearch/<nation>/<model-group>/`. Keep original scans separate from annotated traces and converted measurements.
- [ ] Use filenames that preserve source identity, drawing date and view; retain original filenames in the register.
- [ ] Record reproduction terms before moving source imagery into distributable assets. Existing private rulebooks remain in `contextonly`.
- [ ] Complete one reference worksheet per model group, adding vessel-specific sections or separate worksheets when evidence requires a split.

## Modeling implementation TODO

- [ ] Capture sourced geometry in each replacement model, with a sidecar for dimensions, stations/offsets where available, source IDs, reference waterline and inferred regions. Use the Canopus prefab attachment contract as the first prototype.
- [ ] Distinguish measured hull geometry from the scenario's game statistics. Preserve ship identities, orders, rule ratings and save compatibility while updating visual dimensions.
- [ ] Build or import each pilot as an independent replacement model, preserving its own sourced hull form and a fallback to the current visual.
- [ ] Position barbettes, casemates, funnels, superstructures, masts and boats against the verified plan. Check gun supports and trainable clearances after the hull change.
- [ ] Replace the common analytic waterline-width approximation in wash/spray placement with interpolation from the researched hull stations.
- [ ] Use sourced waterline length where appropriate for wake scaling; keep overall length available for model bounds and camera framing.
- [ ] Establish explicit, documented sharing between sister ships and separate geometry where the evidence requires it.
- [ ] Measure model/rendering costs and provide suitable distant detail levels before replacing the whole fleet.

## Completion checks

- [ ] Overlay model profiles, deck outlines and selected sections on the source drawings, with aligned waterline, scale and station references.
- [ ] Check dimensions against the recorded source precision and document intentional compromises.
- [ ] Review bow and stern silhouettes, underwater form, sheer, tumblehome, freeboard and deck breaks.
- [ ] Review each ship's dated configuration and differences from its sisters.
- [ ] Verify gun/deck contact, turret rotation, muzzle positions, waterline contact, spray origins and camera bounds.
- [ ] Run movement, firing, save/load and module-switch regressions with representative replacement hulls, then the full fleet.
- [ ] Update the ship tracker and module guide with the achieved coverage: silhouette researched, lines verified, remaining inferred regions and model review status.
