# Model pass 4

Niagara's stern now has a short elliptical end, fuller quarters and a tangent-matched transition into the existing midbody. Its taffrail, gallery frames and moldings follow the shell rather than a flat transom. The pointed preliminary revision was rejected during visual review. Exact curvature remains a reconstruction: the located 1854 Steers lines-plan index and 1863 stern-profile index identify the drawings, but the engineering sheets have not been obtained. Contemporary evidence supports a rounded stern; no claim of measured frame offsets is made.

Both ships have twice the longitudinal hull samples and finer circular surfaces on spars, rigging, cannon profiles, chains and fittings. Added geometry includes cutter internal frames, inwales and oarlocks; companionway handrails; gun tackle blocks/falls; and paired roof rivet lines and transverse seams on Stonewall's fixed forecastle. These fittings are representative rather than recovered from a complete yard drawing. The armor enclosures stay fixed and Niagara retains the selected ten-Parrott 1865 working configuration.

Render meshes are divided into port/starboard fore/aft sections per material with preserved normals. This doubles render mesh counts while retaining every source triangle. Source mesh part counts are reported separately; they are not claimed to have doubled.

| Model | Source vertices before → after | Render meshes before → after |
| --- | --- | --- |
| Niagara | 125,093 → 264,565 (2.115×) | 18 → 36 |
| Stonewall | 87,226 → 190,148 (2.180×) | 16 → 32 |

Baseline sources, counts and OBJ geometry are retained in `Artifacts/Pass4/Baseline`. Current counts, isolated renders and geometry report are in `Artifacts/Pass4`. The new player is `Builds/Windows-Pass4/Ironclads.exe`; older player directories are preserved. **Stern detail** under Ships provides a close camera view of either vessel.

Checks cover the source/combined triangle count, vertex budget, doubled render counts, finite geometry, hull boundary edges, cannon count and recessed bores, Stonewall battery support and integrated forecastle, monotonic Niagara stern breadth, tangent continuity, and stern-window surface attachment. The player validation exercises gunnery, placement, turn/undo, weather, globe and reference viewers, and captures the new Niagara stern view. These checks do not establish exact historical accuracy or performance on other hardware. Added geometry and render meshes can increase GPU/CPU cost.
