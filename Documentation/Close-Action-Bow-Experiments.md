# Close Action bow experiments

| Version | Commit | Local tag | Status |
| --- | --- | --- | --- |
| Bows 1.0 | `9aed352` | `bows-1.0` | Original baseline |
| Bows 2.0 | `318c13c` | `bows-2.0` | First experiment, committed by user |
| Bows 3.0 | `579807e` | `bows-3.0` | Cover silhouette, committed by user |
| Bows 4 | Pending user commit | Not tagged yet | Shorter head projection and seated rail tails on `experiment/bows-4.0` |

## Bows 1.0 rollback point

Recorded on 22 November 2026, before changing the bows. The working tree was clean.

- Commit: `9aed3523638ed5a4eea1145815cba2d84a53d058` — `Up and over for close action.`
- Original branch: `feature/hampton-roads`.
- Annotated local tag: `bows-1.0` (resolves to that commit).
- Experiment branch: `experiment/bows-2.0`.
- Existing playable baseline: `Builds/Windows-CloseAction-HeadFraming/Ironclads.exe`, version 1.8.13-dev. The experiment builds to a separate directory.
- Local backup: `Artifacts/Bows-1.0/source-9aed352.zip`, `CloseActionModels/` (generated revision 19), and `Previews/` (the original ten prototypes' side/top/front views).

The tag protects tracked source. Generated models, builds and Artifacts are ignored by Git; the separate local backup protects the model cache and comparison images. The tag and backups are local and have not been published.

To play the preserved baseline immediately, run from the project root:

```powershell
& ./Builds/Windows-CloseAction-HeadFraming/Ironclads.exe -close-action
```

For a source rollback that keeps the experiment intact, create a separate checkout:

```powershell
git worktree add --detach ../ironclads-bows-1.0 bows-1.0
Copy-Item -LiteralPath ./Artifacts/Bows-1.0/CloseActionModels -Destination ../ironclads-bows-1.0/Assets/Resources/CloseActionModels -Recurse
```

Use an unused destination for the new worktree. The ZIP also contains the complete tracked baseline source. No reset, clean or deletion is needed. To rebuild that checkout, use its original `Ironclads.Editor.ProjectBuilder.DeliverCloseAction` method with Unity 6000.2.12f1.

## Bows 2 experiment

The separate attached assembly remains, with structural connections added across all ten British and French prototypes:

- Three headrails fan apart at the bow cheek and converge toward the figurehead.
- Curved head timbers start at the knee, or bury into the bow cheek for the aft supports, and rise through the rail assembly.
- An open timber grating follows the main headrails, with transverse ledges underneath.
- Paired knightheads seat the heel; eight gammoning turns and frapping bind the bowsprit to the knee.

The hull station mapping, bowsprit axes, headsails and representative figureheads retain their baseline definitions. These are representative visual fittings, not measured reconstructions of each ship's individual headwork.

The user's [image search](https://www.google.com/search?udm=2&q=age+of+fighting+sail+shipbuilding+diagrams+bow+sprit) supplied the reference direction. Figure 46, “Plan of timbering at bows,” in the [Longridge book preview](https://www.monkshillbooks.co.uk/images/prod/anatomy_of_nelsons_ships4.jpg) informed the relationship between open grating, rails, head timbers, knee and knightheads. The [Royal Museums Greenwich Portland-family drawing](https://www.rmg.co.uk/collections/objects/rmgc-object-81507) provides contemporary body, sheer and half-breadth context. References guide proportions and construction relationships; no reference bitmap is shipped as a game asset.

The user also supplied `contextonly/Osprey - OWG 009 - Fighting Sail - Fleet Actions 1775-1815.pdf` (68 scanned PDF pages). Particularly useful visual references are the Victory bow on printed page 12 / PDF page 14, and the Chesapeake–Shannon illustration on printed page 19 / PDF page 21. The Victory view shows the separation of rails and curved head timbers around the central knee. The latter is useful for comparing the lighter silhouette of frigate headwork; it is not a dimensional source for the ships of the line. The book remains a local reference. Contact sheets and inspection extracts are under `Artifacts/Bows2-References/`.

Build with `Ironclads.Editor.ProjectBuilder.DeliverCloseActionBowExperiment`. This regenerates model revision 21, runs the bow experiment, bow join, curve, interior, spar, headsail and hull reviews, and builds `Builds/Windows-CloseAction-Bows2/Ironclads.exe` (1.8.14-dev). Run that executable with `-close-action`, or use `./Start-Ironclads.ps1 -CloseAction`, to compare it with Bows 1.0. When only the attached assembly changes, `Ironclads.Editor.CloseActionBowExperimentReview.RefitAndDeliver` replaces that assembly in the generated prefabs before running the same checks and build; it reuses the saved materials and leaves the hull, guns and rigging cache intact.

Review output is under `Artifacts/CloseAction/Bows2/`: four inspection views per prototype and `verification.txt`. The inspection views furl sails to expose the framing. The first inspection exposed overly deep aft ribs; the final refit seats those ribs against the actual cheek surface, extends the grating aft to the head bulkhead, and moves the lashing forward onto the higher knee. Initial build output is in `Artifacts/Bows2-build.log`; final refit checks and build output are in `Artifacts/Bows2-refit.log`.

Validated on 22 November 2026: the final Unity build exited 0; all seven geometry review suites passed across the ten prototypes; 40 inspection images were produced. The standalone verification exited 0 with `ALL PASS` in `Artifacts/CloseAction/Bows2/Runtime/bows2-runtime.txt`, including the existing network, weather, sail and powder-smoke checks. Runtime screenshots are beside that report. The preserved Bows 1.0 `Assembly-CSharp.dll` still matches the SHA-256 recorded in `Artifacts/Bows-1.0/manifest.json`.

### Bows 2.0 committed checkpoint

Before the next pass on 22 November 2026, the user committed the first experiment as `318c13cb16557cca25ffea14d57d8a1fd1ab7e17` — `1st pass at bow changes.` The tree was clean. Annotated local tag `bows-2.0` now fixes that commit on `experiment/bows-2.0`. Its player remains at `Builds/Windows-CloseAction-Bows2/Ironclads.exe`.

Local rollback backup: `Artifacts/Bows-2.0/source-318c13c.zip`, `CloseActionModels/` (revision 21), and `Previews/`. A separate source checkout can be created with `git worktree add --detach ../ironclads-bows-2.0 bows-2.0`; copy that backup's model folder into the new checkout's `Assets/Resources/CloseActionModels` if needed. The Bows 1.0 tag and player are also retained.

## Bows 3 experiment: Fighting Sail cover

Branch: `experiment/bows-3.0`, starting at the Bows 2.0 checkpoint. The explicit visual target is the bow of the ship on the right of the Osprey Fighting Sail cover, PDF page 1 of the supplied local book. This pass emphasizes the basket's deep rail curves, broad forward shoulders, substantial rectangular ribs and a taller, narrower rising central stem. These are visual interpretations of a perspective painting, not traced construction offsets.

All ten prototypes share the new shape with their existing dimensions and richness scaling. The grating follows the wider curved main rails. The transverse ribs now pass through the lower, middle and upper rails rather than merely ending at the main rail. Figurehead and trailboard placement follow the reshaped central knee; the rigid bowsprit frame retains its previous axis.

Model cache revision: 22. Build: `Builds/Windows-CloseAction-Bows3/Ironclads.exe`, version 1.8.15-dev. Current launcher: `./Start-Ironclads.ps1 -CloseAction`. Use `Ironclads.Editor.CloseActionBowExperimentReview.RefitAndDeliver` to refit the attached assemblies and run the checks/build; a full clean rebuild uses `Ironclads.Editor.ProjectBuilder.DeliverCloseActionBowExperiment`. Review images and reports go to `Artifacts/CloseAction/Bows3/`, preserving the previous pass's output.

Validated on 22 November 2026: all seven geometry review suites passed and the final build exited 0 (`Artifacts/Bows3-final.log`). The bow checks cover the deep rail hollow and raised head, broad forward shoulders, symmetry, separation of all three rails, solid rectangular rib faces, grating openings and fastening geometry. The original hull closure, knee joins, gun counts, gunport clearance, interiors, straight spars and headsails also passed. Fifty inspection views cover all ten prototypes, including a low quarter angle for comparison with the cover.

The standalone check exited 0 with `ALL PASS` in `Artifacts/CloseAction/Bows3/Runtime/bows3-runtime.txt`; runtime screenshots are beside it. The preserved Bows 2 player assembly still matches `Artifacts/Bows-2.0/manifest.json`. On this branch, the normal Close Action delivery methods also target the Bows 3 directory. The user subsequently committed this pass; its checkpoint is recorded below.


### Bows 3.0 committed checkpoint

The user committed the cover-inspired pass as `579807e521af7b46a16f7e318b455f605780f435` - `2nd pass at the bow shaping.` The tree was clean before this refinement. Annotated local tag `bows-3.0` now fixes that commit on `experiment/bows-3.0`; the user also reports pushing that branch. The preserved player is `Builds/Windows-CloseAction-Bows3/Ironclads.exe`.

Local backup: `Artifacts/Bows-3.0/source-579807e.zip`, `CloseActionModels/` (revision 22), `Previews/`, and `manifest.json` with the previous player's assembly hash. A separate checkout can be created with `git worktree add --detach ../ironclads-bows-3.0 bows-3.0`.

## Bows 4 refinement: projection and rail tails

Branch: `experiment/bows-4.0`, starting at Bows 3.0. The user requested about 15 percent less forward projection and shorter tube-like rail tails along the hull, and confirmed that the trimming applies to the headrail tails only.

The knee's forward reach and figurehead mounting station move from 0.585 hull lengths to 0.57225, measured on the same centered hull coordinate system. Relative to the stem at 0.5 hull lengths, that changes the projection from 0.085 to 0.07225 hull lengths, exactly 85 percent of the previous distance. Forward rail controls, the grating end and gammoning attachment follow the same compression. The previous vertical sweep and basket width are retained.

The aft rail tails are trimmed back, bringing their ends from station 0.34 to approximately 0.39 hull lengths. Their caps start inside the cheek planking and emerge smoothly into the exposed rails. The ribs retain their positions along the basket. Review rays check that all six shortened rail caps are seated inside the hull, alongside exterior molding and hull-join checks.

Model revision: 23. Player version: 1.8.16-dev. Build: `Builds/Windows-CloseAction-Bows4/Ironclads.exe`; all Close Action delivery entry points on this branch use this separate directory. Launch with `./Start-Ironclads.ps1 -CloseAction`. Review images and reports: `Artifacts/CloseAction/Bows4/`. Build log: `Artifacts/Bows4-refit.log`. Previous players and review images remain available in their original directories. Source edits are uncommitted until the user chooses to checkpoint this refinement.


Validated on 22 November 2026: the build exited 0 and all seven geometry review suites passed. The measured 15 percent projection check passed for all ten prototypes; all 60 rail-cap seating rays passed. Fifty inspection renders were saved. The standalone verification exited 0 with `ALL PASS` in `Artifacts/CloseAction/Bows4/Runtime/bows4-runtime.txt`, including the existing fleet, sails, weather, network and powder checks. The Bows 3 player assembly hash still matches the preserved manifest. Current source remains uncommitted on `experiment/bows-4.0`.
