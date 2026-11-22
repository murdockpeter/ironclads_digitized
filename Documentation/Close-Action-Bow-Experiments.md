# Close Action bow experiments

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

Validated on 22 November 2026: the final Unity build exited 0; all seven geometry review suites passed across the ten prototypes; 40 inspection images were produced. The standalone verification exited 0 with `ALL PASS` in `Artifacts/CloseAction/Bows2/Runtime/bows2-runtime.txt`, including the existing network, weather, sail and powder-smoke checks. Runtime screenshots are beside that report. The preserved Bows 1.0 `Assembly-CSharp.dll` still matches the SHA-256 recorded in `Artifacts/Bows-1.0/manifest.json`. Source edits remain uncommitted on the experiment branch.
