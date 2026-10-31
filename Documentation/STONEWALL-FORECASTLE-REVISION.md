# Stonewall integrated forecastle correction

The third-pass forward battery still appeared as a circular drum perched on the weather deck. Extending its lower wall below the deck and checking its footprint did not correct that visual/structural interpretation.

This revision removes the independent forward battery wall and circular roof from the model. The raised forecastle sides, sloping bow and upper armor now form part of the same hull loft, with the plated forecastle deck sharing its boundary vertices. The forward gun fires through an opening in the raked bow; two further embrasures are cut into the hull's side armor. The aft gun enclosure remains fixed and the three-gun outfit is preserved. No turret-rotation behavior has been added.

The source photographs and schematic are retained in `Documentation/References`, particularly `NavSource-098651603.jpg` and `NavSource-098651614.jpg`. The new external form is a visual interpretation of those references, not a claim of exact builder-plan geometry.

Checks cover finite vertices, shared hull/deck edges, three intentional forecastle openings, absence of the old forward drum and roof, the presence of raised armor in the hull mesh, unchanged battery counts and recessed bores. Renders and the report are in `Artifacts/BowRevision`. Previous third-pass sources and OBJ files are retained in `Artifacts/Pass3/Baseline`.

Because the user's existing player was open, the revision is built separately at `Builds/Windows-BowRevision/Ironclads.exe`. The running third-pass session and its files are left intact. A later normal Windows build from the revised source can update `Builds/Windows` once that executable is no longer in use.
