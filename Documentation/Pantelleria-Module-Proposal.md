# Pantelleria, 1901 — scenario selection and design brief

Status: the first-pass playable module is implemented. See the [module guide](Pantelleria-Module.md) for current controls, model limits, saves and verification. The brief below records the selection and intended scope.

Recommended source: **Battle of Pantelleria**, *In Mahan's Wake*, printed pp. **70–71** (PDF pages **71–72**), in the supplied `contextonly/In_Mahan's_Wake.pdf`. This is an explicitly **fictional** fleet action within the book's Great Maritime War setting, not an engagement that historically occurred in 1901.

## Why this scenario

It meets the requested Mediterranean setting and substantial forces on each side: **42 battleships, 21 per alliance**. Four national commands and fourteen battle divisions offer several useful command levels. The common ship category supports a focused fleet-action module without requiring destroyers, transport convoys or mine warfare in the initial scenario. Equal hull counts do not establish equal combat strength or tested balance.

| Alliance | National command | Ships | Divisions |
| --- | --- | ---: | ---: |
| Britain / Italy | Britain | 13 | 5 |
| Britain / Italy | Italy | 8 | 3 |
| France / Russia | France | 13 | 4 |
| France / Russia | Russia | 8 | 2 |
| Total | Four commands | 42 | 14 |

The divisions can be distributed among player commanders; the four national commands provide a smaller-group option. The commander structure would be an implementation aid, not a change to the supplied scenario.

## Forces to represent

Britain: Canopus, Ocean; Caesar, Illustrious, Renown; Revenge, Royal Oak, Ramillies; Empress of India, Royal Sovereign; Hood, Camperdown, Anson.

Italy: Re Umberto, Sicilia, Sardegna; Ruggiero di Lauria, Andrea Doria, Francesco Morosini; Italia, Lepanto.

France: St. Louis, Gaulois, Charlemagne; Bouvet, Charles Martel, Jauréguiberry, Brennus; Amiral Tréhouart, Bouvines, Valmy, Jemmapes; Terrible, Marceau.

Russia: Imperator Aleksandr II, Dvenadtsat' Apostolov, Tri Sviatitelia, Rostislav; Ekaterina II, Chesma, Sinop, Georgii Pobedonosets.

Semicolons separate the book's battle divisions. The same volume includes the ship annexes needed to research the configurations. Class families/configurations must be checked against those annexes before assigning batteries, armor, bridge pivots or movement ratings. The existing Ulsan cruiser models do not supply these battleship configurations.

## Opening and umpire requirements

The printed scenario begins at **1430 on 14 February 1901**, near Pantelleria in the central Mediterranean. It uses strong westerly weather: wind from 260° at 20 knots, sea state 5 and 60% rulebook visibility. These conditions create a useful fleet-handling problem and help explain the battleship-only forces.

The four fleets approach a common rendezvous area from different directions at **15 knots**. The setup section places Point X **20 nautical miles on bearing 320° from Pantelleria**. Each leading ship is 32,000 yards from Point X, with the following reference bearings and ordered courses:

| Fleet | Point X bears from its leading ship | Leading ship bears from Point X | Initial course |
| --- | ---: | ---: | ---: |
| British | 110° | 290° | 105° |
| Italian | 280° | 100° | 282° |
| French | 165° | 345° | 180° |
| Russian | 020° | 200° | 000° |

Source clarification: the introductory location loosely describes the battle as north of the island, while the explicit setup gives 320°. Use the explicit setup if implementing the printed deployment. Do not substitute the incoming bearing for the reciprocal bearing when placing leading ships.

Players choose formations within the book's signaling constraint. A starting column or division layout supplied by the software would be a labeled convenience preset; the source does not prescribe a single formation or spacing. Date/time conversion for geographic sunlight also needs an explicit implementation assumption; the book's shipboard time is not silently UTC.

Victory should remain umpire-scored using the national conditions in the private book. A local result ledger can track sunk and crippled ships, including the supplied 50% damage threshold and differing national denominators. Avoid automatically combining the separate national conditions into an invented alliance score or dividing a ratio by zero without an agreed convention.

## Proposed software scope

- Add a distinct `pantelleria-1901` scenario and launcher/module selector entry, retaining Ulsan and all other modules.
- Generalize the existing Dawn of the Battleship table from its current seven-ship Ulsan assumptions to variable fleets, national commands and division selection.
- Add the required battleship class data and 3D representations, with selectable ships, gun origins, turret training where appropriate, engine smoke and visual salvos.
- Reuse three-minute tactical and thirty-minute intermediate clocks, measured movement and gunnery aids. Turning and bridge pivots need the appropriate battleship ratings rather than Ulsan's cruiser assumptions.
- Add per-ship and division orders, placement/formation presets, save/load and a lobby large enough for the chosen command structure. Verify ownership and synchronization across all 42 ships.
- Use geographic positioning near Pantelleria. The existing cached global coastline can support the island setting; the Hampton-only local terrain cannot simply be relabeled Mediterranean geography.
- Keep combat, detection, ammunition, damage and victory umpire-run unless separately implemented and verified. Existing visual salvos do not constitute automated Dawn of the Battleship combat.

The private source PDFs and extracted annexes remain in ignored local research storage. A public module guide should summarize setup and document original implementation decisions rather than distribute the books or reproduce their scenario prose and annex tables.

## Smaller Mediterranean alternatives reviewed

| Scenario | Source, printed pages | Forces | Assessment |
| --- | --- | --- | --- |
| Cape Teulada, 28 March 1900 | *In Mahan's Wake*, p.64 | 5 British vs. 6 French battleships | Good compact introductory fleet action; less substantial than the requested larger module. |
| Ibiza, 10 March 1900 | *In Mahan's Wake*, pp.62–63 | 3 British vs. 6 French battleships | Mediterranean and battleship-focused, but fewer ships and asymmetric hull counts. |
| Cape Corse, 25 August 1900 | *In Mahan's Wake*, p.68 | 5 Italian vs. 4 French cruisers | A useful cruiser action; similar overall scale to Ulsan. |
| Gavdos, 21 April 1900 in the scenario body | *In Mahan's Wake*, pp.65–66 | 17 Anglo-Italian ships vs. 43 Russian/French vessels, including 10 transports | A larger convoy action, with much greater small-craft and transport-management demands. The contents page gives 1899; the scenario body gives 1900. |

*Monroe's Legacy* focuses on American naval emergence and its contents provide no stronger match for a large Mediterranean coalition action. Pantelleria is the recommended selection from the supplied books inspected.
