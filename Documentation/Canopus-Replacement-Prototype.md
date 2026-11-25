# Canopus Replacement Model Prototype

Canopus now has a separate swap-in visual asset for the Pantelleria module. Only ship slot 0 uses it; Ocean and the other 40 ships retain their existing models. The local **Compare with previous model** control under Canopus switches that single ship without changing the scenario, orders, clock or rule statistics.

The player loads `Assets/Resources/PantelleriaReplacements/Canopus.prefab` with saved mesh/material assets. It does not run the authoring generator to create the prototype during play. The old template remains the fallback if the optional replacement asset is absent or the previous-model option is selected. This is the first prototype of the replacement-model approach, not a fleet-wide hull-engine rewrite.

## Research coverage

**Silhouette researched; underwater sections and fine fittings inferred.** The supplied Wikipedia overview was checked alongside period drawings and catalogue records. Low-resolution previews support a distinct reconstruction, but do not provide a sufficiently legible offset survey to label the hull lines verified.

| Source ID | Reference | Use and limit |
| --- | --- | --- |
| RMG-NPA8065 | [Greenwich lines-plan catalogue](https://www.rmg.co.uk/collections/objects/rmgc-object-52023), drawing dated 30 May 1896 | Body/sheer/half-breadth reference. Preview used for broad form; individual offsets are not verified. |
| RMG-NPA8075 | [Greenwich as-fitted profile](https://www.rmg.co.uk/collections/objects/rmgc-object-52033), drawing dated 2 March 1900 | Early profile reference. The catalogue identifies later 1903–04 and 1908 annotations, which are not adopted as the target configuration. |
| BRASSEY-1906 | [Class elevation, deck plan and section](https://commons.wikimedia.org/wiki/File:Canopus_class_diagrams_Brasseys_1906.jpg) | Period class layout comparison. Its 1906 date is not proof of every 1901 fitting. Commons identifies the reproduction as public domain. |
| BRASSEY-1899 | [Canopus illustration in the 1899 Naval Annual](https://commons.wikimedia.org/wiki/File:HMS_Canopus_-_Brassey%27s_Naval_Annual_1899.jpg) | Appearance and silhouette comparison; an illustration, not a measured plan or dated operational photograph. |
| USNI-1899-GLORY | [Contemporary class description](https://www.usni.org/magazines/proceedings/1899/april/professional-notes) | Class beam, draft and freeboard context. It describes Glory rather than a measured Canopus loading condition. |
| USER-WIKI-CANOPUS | [User-supplied class overview](https://en.wikipedia.org/wiki/Canopus-class_battleship) | Secondary overview and dimension cross-check; primary drawing evidence takes precedence. |

Research previews are stored under `contextonly/PantelleriaHullResearch/Britain/Canopus/`. Museum previews remain research material outside the player assets. The delivered asset contains original meshes and materials, without embedded source scans.

## Prototype geometry

The hull is lofted from a Canopus-specific 21-station data file, with separate waterline/deck outlines, changing sheer, an aft counter and a submerged ram. Widths and sections are manually interpreted working values. The underwater cross sections remain a reconstruction rather than a digitisation of verified offsets.

Working dimensions are 128.47 m overall length, 118.96 m between perpendiculars, 22.5552 m beam and 7.9248 m nominal draft. The working waterline span is 121.92 m; its exact endpoints need checking against a higher-resolution plan. Decimal conversions do not imply matching historical measurement accuracy or an established February 1901 loading condition.

The upperworks include a raised central battery, projecting bridge wings, taller military masts, tandem funnels, boat stowage and separate lower/upper secondary positions. The charcoal hull, pale upperworks and buff funnels/spars are an early-appearance interpretation, not a verified paint specification. Four main, twelve secondary and six representative light gun slots retain the existing game arrangement. The complete historical light battery is still to be represented.

## Replacement asset contract

- Metres, Y up, bow along +Z, reference waterline at Y = 0.
- Root `CanopusReplacement` component records turret transforms/groups and exhaust transforms. Runtime attachment restores the existing turret/funnel component types without relying on their prefab serialisation.
- Existing `GunMount` components provide muzzle positions and battery bindings. The two main assemblies remain independently trainable.
- National colours, pennants and decorative hoists are attached at runtime using the replacement's mast and deck anchors.
- Existing weathering binds to the new materials. Hull motion, surface wash and spray use the replacement's visual waterline dimensions and station widths.
- Game identities, speeds, movement rules, orders, saves and network packets use the existing scenario data. The model preference is local and stored separately.

A later hand-authored replacement can use the same prefab entry and attachment contract. Normal delivery builds retain an existing replacement prefab and generate the prototype only when it is absent. The explicit **Ironclads > Rebuild Canopus replacement asset** editor command regenerates this prototype's three named outputs; use it only when regeneration is intended, rather than after manually editing or importing a final model.

## Build and compare

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaCanopus -logFile Artifacts/Pantelleria/build-canopus-verified.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Canopus -OutputDirectory Artifacts/Pantelleria/Canopus/Runtime-Verified
.\Start-Ironclads.ps1 -Pantelleria
```

Select Canopus in the British 8th division, then use **Compare with previous model** or **Use Canopus prototype**. The launcher prefers `Builds/Windows-Pantelleria-CanopusDetail`, version 1.10.15-dev. Previous players remain available.

## Validation

Version 1.10.13-dev passed 1,319 existing fleet editor checks and 39 replacement-specific checks. The standalone suite passed movement for all 42 ships and 14 divisions, orders, clock, results/save round trips, national salvos, exhaust, sunk-state controls and module switching, with no logged runtime errors. The Canopus comparison also preserved scenario state and the existing Ocean instance.

Reports and comparison captures are in `Artifacts/Pantelleria/Canopus/editor-verification.txt` and `Artifacts/Pantelleria/Canopus/Runtime-Verified/`. The saved replacement body contains 208 meshes and 125,286 vertices before runtime colours and pennants. These checks validate integration and geometry consistency, not historical accuracy of inferred sections.

## Hull seam repair (1.10.14-dev)

The bow gap was caused by inward-facing triangles where a radial normal estimate disagreed with the raked stem and counter. Hull faces now retain the consistent winding of the lofted sections. Twelve exterior ray checks cover both halves of the bow and stern at three heights, with back-face culling. The geometry dimensions and source interpretation are unchanged.

The repaired prefab passed 51 replacement checks, including the twelve added end-surface checks. The standalone fleet regression also passed with no logged runtime errors. Front and stern captures and the report are under `Artifacts/Pantelleria/Canopus/Runtime-BowRepair/`.

The explicit `DeliverPantelleriaCanopusBow` repair build regenerates the prototype assets before packaging `Builds/Windows-Pantelleria-CanopusBow`. Use the normal `DeliverPantelleriaCanopus` method when preserving an existing hand-edited prefab.

## Additional fittings (1.10.15-dev)

The next visual pass adds scuttle visors, deck-edge drains, raised hatch coamings with timber lids and hinges, ventilator necks/cowls, and military mast access ladders. These are interpreted details; their exact placement is not newly verified historical evidence. Existing gun mounts, hull stations and the repaired bow/stern winding are preserved.

| Count basis: ensign and pennant included; decorative hoists excluded | Previous replacement | This pass |
| --- | ---: | ---: |
| Mesh components | 210 | 420 |
| Vertices summed over components | 126,456 | 147,900 |

The saved body has 418 mesh components referencing 259 mesh assets; repeated simple fittings share geometry. Runtime ensign and pennant add two components. Mesh component counts describe model parts, not measured draw calls. The prior generated Canopus remains at 153,336 vertices and 952 mesh components on the same basis.

The explicit `DeliverPantelleriaCanopusDetail` method regenerates this prototype's assets and packages the detail build. Replacement checks cover the added fittings and model budget as well as the bow/stern, gun and attachment checks. Player captures and regression results are written to `Artifacts/Pantelleria/Canopus/Runtime-Detail/`.

Validation: 53 replacement editor checks passed. The standalone regression passed fleet/division movement, orders, saves, salvos, exhaust, sunk-state controls and module switching with no logged runtime errors. The foredeck close-up was visually reviewed for hatch/drain seating and the repaired bow.

## Remaining research

- Obtain readable, rights-documented lines/offset material and replace inferred underwater sections.
- Reconcile overall, perpendicular and waterline lengths against the original dimensional table.
- Verify exact 1901 mast heights, fittings, casemate details, paint and loading condition.
- Add dated bow/stern/broadside photographs and distinguish later alterations.
- Verify Ocean separately before sharing the replacement with it.
