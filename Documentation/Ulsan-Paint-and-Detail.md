# Ulsan cruiser paint and detail pass — 1.3.1-dev

**Current follow-up: 1.3.2-dev.** Launch `./Start-Ironclads.ps1 -Ulsan` for `Builds/Windows-Ulsan-BowFix/Ironclads.exe`. The earlier detail build remains in its original directory.

The shared cruiser hull now closes both stem and stern end profiles and joins its deck with exact matching edge coordinates. Previously the narrow finite-width stem profile was left open. Geometry checks verify paired edges and outward-facing end plates; low-angle renders inspect all three Russian bows.

At the user's request, Russian funnels now have yellow bodies and black caps, while hulls and upperworks retain olive paint. This is an intentional scenario identification choice; it does not resolve the uncertainty about their historical Ulsan funnel colors. Japanese funnels retain blue-gray bodies and black caps.

## Paint interpretation

The main color guide is **Dawn of the Rising Sun, Appendix 3, printed p.89** in the user's reference folder. Its author explicitly describes conflicting accounts for the Vladivostok squadron. Period photographs establish shapes, fittings and tonal relationships, but do not establish exact paint hues. No precise historical RGB match is claimed.

| Ships | Applied scheme | Basis and confidence |
| --- | --- | --- |
| Rossia, Gromoboi, Rurik | Dark olive hull, upperworks, shields and lower masts; scenario-selected yellow funnels with black caps | One of the wartime Vladivostok interpretations recorded in Appendix 3. The three ships share the same paint family, with small illustrative shade differences. The alternative black and tan accounts remain unresolved. |
| Izumo, Azuma, Tokiwa, Iwate | Blue-gray hull, upperworks, turrets and funnels; black caps and upper mast sections | Appendix 3's wartime overall blue-gray scheme. The former exaggerated light upperworks have been removed. Small differences between hulls are weathering choices, not documented individual paint recipes. |
| All seven | Muted scrubbed timber decks, dark gun metal, subtle surface variation | Period photographs show deck planking, painted steel surfaces and dark fittings. Boot topping and submerged red-brown antifouling are plausible reconstruction choices. |

The paint is used across the modeled fittings as well as the hull. New materials have low metallic response and restrained variation. A dedicated shader adds fine teak plank seams, small paint variations and a discreet waterline treatment. The opening table does not include invented battle damage or national hull stripes. Yellow funnels are the explicit scenario identification override described above.

## Photographs reviewed

- [Rossia after Ulsan: close-up of damaged side and upperworks](https://commons.wikimedia.org/wiki/File:RossiaSideDamage.jpg). Useful for funnel bands, steam pipes, cowl ventilators, deck railings and mast platforms. Damage belongs to the post-battle photograph and has not been baked into the opening model.
- [Gromoboi, August 1904 after Ulsan](https://commons.wikimedia.org/wiki/File:Gromoboy1904.jpg). Useful for deck planking, bridge rails and the Russian naval ensign; it is a close deck view rather than a full hull color record.
- [Yoshimoto Kozo's Russo-Japanese War photographic album](https://commons.wikimedia.org/wiki/Category:Russo-Japanese_War_photography), preserved by the Geidai Archives: Yoshimoto017 shows Azuma, Tokiwa and Iwate at battle from Izumo; Yoshimoto043 is aboard Izumo in August 1904. The fleet view is distant and faded, so its value for fine detail and shade is limited.
- [Izumo photographic collection](https://www.tynebuiltships.co.uk/I-Ships/Izumo1900.html). The harbor view helps with bridge wings, boats, mast platforms, capped funnels and gunhouse relationships. Its earlier appearance is not treated as proof of the August 1904 paint recipe.
- [Rurik period photograph reproduced by the Russian Military Historical Society](https://rvio.ru/activities/news/opredelen-luchshij-proekt-pamyatnika-krejseru-ryurik). Used to check the earlier three-masted, two-funnel vessel's fittings and outline, not to recover a wartime hue from a monochrome image.

Research copies and rendered comparisons are local artifacts under `Artifacts/Ulsan-Pass2`, not bundled with the player. The owned rulebook is not redistributed.

## Added geometry

The second pass adds continuous deck rails, bridge-wing rails and braces, mast ladders and fighting-top guardrails, funnel strengthening bands, steam pipes and access rungs, porthole rims, scuppers, searchlights, hatch coamings/hinges, boat frames/davits, anchors and chain links. Guns gain trunnions, carriage cheeks, recoil cylinders, reinforcing collars and elevation handwheels. Hull sections and Japanese gunhouses use finer curved surfaces.

These fittings remain historically informed reconstructions rather than traced shipyard plans. Existing gun counts, muzzle positions, moving turret groups, funnel attachment points and physical scale are retained. Mesh counts are the final combined MeshFilter/render meshes, including independently moving turrets. Additional spatial sections preserve normals at shared boundaries. Mesh subdivision contributes to the requested count increase; the visible fittings account for much of the additional vertex budget.

The editor writes `Artifacts/Ulsan-Pass2/comparison.csv` with measured first/second-pass counts and multipliers for every ship. It checks that each model reaches at least twice its previous vertices and render meshes, retains its battery/funnel counts, has finite geometry and uses the new paint/deck finishes. Separate daylight inspection renders preserve the original 05:20 scenario time.


Counts below record the **1.3.1 detail pass**, before the small end-closure/yellow-funnel follow-up. Current measured counts are in `Artifacts/ulsan-model-counts.csv`.

| Ship | Vertices | Render meshes |
| --- | ---: | ---: |
| Rossia | 118,574 | 123 |
| Gromoboi | 134,622 | 123 |
| Rurik | 117,838 | 119 |
| Izumo | 114,943 | 144 |
| Azuma | 111,269 | 142 |
| Tokiwa | 112,550 | 144 |
| Iwate | 114,943 | 144 |
| **Fleet total** | **824,739** | **939** |

Vertices are 2.20?2.32 times the first pass; render meshes are 2.02?2.09 times the first pass. Counts exclude the ocean, scenery and smoke particles.

## Build and checks

Build the current follow-up using Unity 6000.2.12f1 and `Ironclads.Editor.ProjectBuilder.DeliverUlsanBowFix`. Verify the standalone player and two-process fleet commands with:

```powershell
& .\.tools\verify-player.ps1 -BuildDirectory Builds/Windows-Ulsan-BowFix -Ulsan
& .\.tools\verify-network.ps1 -BuildDirectory Builds/Windows-Ulsan-BowFix -Ulsan
```

The wire protocol remains 5. Matching 1.3.1-dev players show identical models; an older protocol-5 player can synchronize the table but retains its own earlier visual assets.
