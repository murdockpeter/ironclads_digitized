# Close Action: ship models and canvas

This revision repairs the open hull ends, reshapes the afterbody, increases geometric detail, expands the library to ten prototypes, and makes canvas fullness respond to the table's wind. Launch using `./Start-Ironclads.ps1 -CloseAction`; the launcher prefers `Builds/Windows-CloseAction-HeadFraming`.

The current 1.8.13-dev build includes separately assembled bow framing and rounded bow cheeks, a shorter head projection, smooth planking and slimmer upper bows, open beakhead framing, closed lower hulls and [full hull contouring and weather controls](Close-Action-Hull-Contours.md). Counts below reflect that build.

## Ten prototypes

The library now contains four British and six French class/family models, compared with two and three previously. Every prototype is used by the existing scenario; the order of battle remains 23 ships. The book's optional Goliath uses the British Slade-family prototype.

| Prototype | Scenario ships using it | Reconstruction scope |
| --- | --- | --- |
| British Bellona / Arrogant family 74 | Zealous, Audacious, Vanguard, Defence, Swiftsure; optional Goliath | Related Slade-era hull and stern family. Swiftsure's Elizabeth-class details are represented by this related family, not a separate exact model. |
| British Culloden family 74 | Theseus, Culloden, Minotaur | Fuller British 74 with a different counter and gallery profile. Minotaur uses the larger-74 representative, rather than a separate Courageux-class reconstruction. |
| British Alfred class 74 | Alexander | Separate proportions, broader gallery and higher stern-board crown. |
| British Portland class 50 | Leander | Smaller two-decker with a stern informed directly by the Isis/Leander plan. |
| French Temeraire family 74 | Spartiate, Aquilon, Mercure, Timoleon, Genereux | Later French 74, with a broad horseshoe stern and finer counter. |
| French Centaure class 74 | Heureux | Separate hull proportions and more restrained stern outline. |
| French Citoyen family 74 | Conquerant | Representative of the rebuilt ship's later eighteenth-century form; its earlier 1740s configuration is not modeled. |
| French Souverain / earlier 74 family | Peuple Souverain, Guerrier | Earlier French 74 outline and more pronounced gallery curvature. Guerrier's Magnifique-class details use this earlier-family representative. |
| French Tonnant class 80 | Franklin, Tonnant, Guillaume Tell | Larger two-decker with broad galleries and a more pronounced stern-board arch. |
| French Ocean family 120 | Orient | Three gallery levels, larger counter, and three stern lanterns. |

These are original procedural reconstructions informed by surviving plans and paintings. Related-family assignments are explicit above. Dimensions, carvings, paint, lantern arrangements and window counts are representative choices; they are not claimed to reproduce every ship's exact condition on 23 June 1798. The profiles change geometry and proportions, not just names or colors. The selected model's family is displayed in Fleet / setup.

## Research and stern changes

Royal Museums Greenwich provides several particularly useful primary drawings:

- [Isis, Jupiter and Leander, ZAZ1811](https://www.rmg.co.uk/collections/objects/rmgc-object-81622): body plan, decorated stern, counter name, profile and half-breadth. Used for the small British two-decker's raked stern and two glazed levels.
- [Defence stern/body plan, ZAZ0897](https://www.rmg.co.uk/collections/objects/rmgc-object-80688) and [Defence profile, ZAZ0896](https://www.rmg.co.uk/collections/objects/rmgc-object-80687): used to study the sweep of the stern board and the transition to the much narrower lower afterbody.
- [Cornwall/Arrogant/Vanguard and related 74s, ZAZ1142](https://www.rmg.co.uk/collections/objects/rmgc-object-80933): common British 74 proportions and the side-gallery rake.
- [Alexander, Warrior, Montague and Alfred](https://www.rmg.co.uk/collections/objects/rmgc-object-80477): a separate British design reference for the Alfred-family model.
- [Pompée, ZAZ1161](https://www.rmg.co.uk/collections/objects/rmgc-object-80952): captured French 74 plan recorded before British fitting, including the stern-board decoration and name cartouche. Used for the later French 74 outline.
- [Canopus, ex-Franklin, lines and profile](https://www.rmg.co.uk/collections/objects/rmgc-object-80377) and [Tonnant deck plans, J2491](https://prints.rmg.co.uk/products/tonnant-captured-1798-j2491): used for the French 80's afterbody and poop-deck planform. These British records describe captured ships; their later fittings are not assumed to document every 1798 detail.
- [George Arnald's depiction of Orient at the Nile, BHC0509](https://www.rmg.co.uk/collections/objects/rmgc-object-12001): used as a visual reference for the large three-decker's stern silhouette and gallery hierarchy. Painted in 1825–1827, it is an interpretation of the battle rather than a measured construction drawing.

The principal correction is structural: the hull narrows toward the sternpost below the galleries, and the counter rises and projects aft. A continuous closed hull replaces the original open ends. Above it, the stern board is cambered across its width, raked in profile and crowned rather than built as stacked rectangular boxes. Quarter galleries follow swept curves, with tapered lower supports and shaped roofs. Windows, mullions, cornices, poop-deck edges and lantern supports follow those surfaces. The name sits on the lower counter, faces outward, and uses depth testing and back-face culling so it cannot show backward through the ship.

The research images are retained only in ignored local research artifacts. The models and new documentation contain original geometry and prose, not redistributed museum image assets.

## Closed interiors and decorated bows

The 1.8.9-dev revision follows the upward sweep in the supplied Victory bow photographs. A solid knee of the head now runs from near the waterline into a steep crown behind the figurehead. Painted trailboards, their gold borders and the headrails follow that curve instead of a straight ramp. The figurehead is mounted higher and farther forward, with its height limited to clear the unchanged, straight bowsprit. Bobstays pass outboard of the new structure. The sweep is applied to all ten class/family prototypes as a representative reconstruction; the photographs do not establish exact profiles for every ship.

The 1.8.8-dev revision adds an inward-facing wooden lower-hull surface, solid gun decks with undersides and closed edges, the weather-deck ceiling, transverse beams and support pillars. These surfaces close the empty views below and above the guns while retaining the modeled gunport openings. Views directly through an opposing open port can still admit daylight; the hull bottom itself is now visible from inside.

The bows gain curved supporting head timbers, gold-painted rail moldings, painted trailboards with spiral carving, shields, laurels and representative figureheads. The Culloden-family example uses a lion motif; other models use a helmeted figure and heraldic/trophy decoration, with national colors and rating-scaled proportions. These are original representative designs, not claims about the precise figurehead of every named ship.

References include the [Black Seas miniatures gallery](https://www.black-seas.com/), [Warlord's French first-rate](https://www.warlordgames.com/black-seas-french-navy-1st-rate/), the [Captain bow/headrail design dated January 1798](https://www.rmg.co.uk/collections/objects/rmgc-object-1165205), the [possible Hercule bow and figurehead drawing](https://www.rmg.co.uk/collections/objects/rmgc-object-1165232), and the supplied [Black Seas rigging guide](https://www.warlordgames.com/robbies-guide-to-rigging-black-seas-ships/). Research photographs remain local reference artifacts; the player contains original mesh geometry.

## Wind-driven canvas

The 1.8.7-dev headsail revision replaces the oversized forward triangles with a fore staysail, fore-topmast staysail and jib. Each has its own straight stay, a separate hoist and tack, a staggered clew, and its own canvas surface and gust phase. Their individual areas are bounded relative to hull length; the jib's head is set lower along its stay rather than extending a broad triangle all the way to the masthead. Plain sail shows the inner staysail, medium/full sail show all three, and furled sail hides them. Rigging loss continues to limit the effective sail setting.

This representative arrangement follows the three forward staysails identified in [David Steel's 1794 Elements and Practice of Rigging and Seamanship](https://whalesite.org/anthology/1794_Steel_Elements_and_Practice.htm). Proportions are original visual reconstructions for the ten class/family models, rather than surveyed sail inventories for each ship. The emulator's existing movement and sail-setting rules remain unchanged.

In 1.8.6-dev, the pressure response uses a square-root wind-speed curve and a larger base belly. This makes all four wind strengths fuller, with the largest proportional improvement in light wind. Gusts, traveling folds, wind incidence and fixed attachment edges remain active; calm pressure remains zero. Renderer bounds include the increased displacement. Four standalone captures compare WS 1–4, alongside bow and bowsprit views of every prototype.

Square sails use a finer cloth grid; jibs and spanker use subdivided triangular surfaces. Their attachment edges remain fixed. Wind pressure depends on wind speed in knots converted to meters per second and the angle between the wind and the sail's surface normal. A near-parallel wind fills a sail less than a wind striking its face. The sign of that pressure controls which side bulges.

The 1.8.3-dev canvas revision increases the pressure-driven belly to roughly 1.7 times its previous depth, with a bounded 3.4 m base fullness. Smooth value-noise gusts vary the load slowly, while two smaller traveling folds change the surface locally. Each sail gets a stable phase seeded from its initial placement and dimensions, so neighboring sails and ships do not pulse together. Attachment edges remain fixed; calm pressure still removes wind-driven displacement. Shader normals follow both the belly and its moving folds. Render bounds include the full animated displacement to prevent edge-of-screen clipping. An isolated-sail render check compares three animation times for visible image changes.

Yards brace toward the wind within a 60-degree limit. Cloth fullness grows with wind strength and is capped to prevent excessive distortion. Calm wind removes pressure-driven billow. Two small gust components vary the shape over time; individual sails have different phases. Deformed normals follow the curved surface. The cloth shader includes reverse-side light transmission, subdued stitched seams and a fine weave, reducing the opaque black appearance of the earlier sails.

FS/MS/PS and remaining rigging still determine which sails are set. Wind animation is a bounded visual approximation, not a fluid solver or a replacement for the published movement rules. The game continues to resolve movement using its movement charts; this revision does not derive ship motion from aerodynamic forces.

## Detail and validation

The comparison baseline is the original five-model delivery: British 74 275,561 vertices/285 meshes; British 50 216,935/285; French 74 275,449/281; French 80 290,623/281; French 120 388,639/281. Each new model is compared with its nationality/rating baseline. The build checks that both counts exceed twice that baseline while retaining its nominal gun count.

The completed pass measures **2.484?2.835 times the vertices** and **2.656?2.694 times the meshes** per prototype. The full [before/after count report](CloseAction-Geometry-Comparison.csv) records all ten models.

Added detail includes laid deck planks and caulk gaps, finer cannon barrels and rigging, iron carriage straps and bolts, elevation quoins, belaying pins, coiled lines, timber knees, curved beakhead rails and continuous molded wales. More spatial mesh sections retain that detail across the hull, spars and fittings; shared prefab meshes and materials remain reusable across ships of the same prototype.

`CloseActionDetailReview` checks all ten prototypes, including finite geometry, four hull-closure rays, wind-pressure changes at calm/light/strong wind, pressure reversal with wind direction, and the outward stern-text material. It renders bow, stern, port and starboard views plus wind comparisons. Reports and images are written to ignored `Artifacts/CloseAction/Detail` during the editor build. A detailed 23-ship fleet has a greater rendering cost than the original models; the counts are a geometry comparison, not a frame-rate guarantee.
