# Niagara stern and paint revision - 1.5.2-dev

The user confirmed USS Niagara at Ferrol as the intended vessel. Minnesota at Hampton Roads is unaffected.

The previous round stern had nearly vertical longitudinal sections from gallery to waterline, giving a barrel-like end in the stern inspection view. The revised afterbody keeps the rounded deck outline but brings the lower stations forward progressively under the counter. The displacement fades smoothly to zero at the aft fifth of the hull; the bow, midbody, overall deck length and beam remain unchanged. This is a qualitative visual adjustment, not a claimed measured lines-plan reconstruction.

Gallery lights and moldings are evaluated against the revised hull surface. The exposed vertical center post has been replaced by a post/rudder below the counter. The quarterdeck house is lower and narrower, with a small roof overhang. Representative gallery window count and house dimensions remain uncertain.

The locally retained period photograph `Documentation/References/Niagara-1863-NH75895.jpg` shows Niagara off Boston after modification in 1863. The user's two supplied illustrations provide additional silhouette context, including an earlier cable-laying depiction. They do not replace the selected post-refit 1865 working configuration. The photograph supports the dark hull / contrasting gun-deck stripe layout; it cannot establish exact paint color values, timber shades or underwater color. The game uses near-black timber and iron, a brighter off-white band, and darker gallery trim. Band segments now end at the actual port boundaries instead of leaving extra gaps around each port. Natural spar/deck colors are retained.

The source engineering drawings identified in the hull review remain archival leads; their index cards are not hull lines and have not been used as recovered offsets.

Run `.\Start-Ironclads.ps1 -Ferrol` from the project directory. The current executable is `Builds/Windows-Niagara-Stern/Ironclads.exe`. Under Ships, select Niagara and use **Stern detail**; **Inspect this ship** shows the overall appearance. The command-line flag `-niagara-stern` opens directly on that view. The 82 by 74 Ferrol map and 2,000-yard opening setup are retained.

Editor comparison renders and geometry checks are written to `Artifacts/NiagaraStern/`. Runtime verification covers ship meshes, movement/undo, battery operation, smoke and reference views.
