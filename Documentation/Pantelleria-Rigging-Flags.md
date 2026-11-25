# Pantelleria — dressing lines and yardarm hoists, 1.10.7-dev

All 42 ships can display extra animated flags in two arrangements. **Full dress** is the initial default: 26 decorative flags and pennants run from a short bow staff, over the mastheads and down to a stern staff. **Signal hoists** displays twelve decorative flags on four yardarm halyards. **Colours only** retains the existing national ensign and commissioning pennant without extra rigging flags.

Choose **Weather → Rigging flags**. The choice applies to this viewer's whole fleet and is remembered locally between launches; it does not change a saved table, other players' preferences or network orders. Full dress is a ceremonial visual option. The shorter hoists are also decorative: neither mode represents a decoded signal, a commander's rank or an operational message.

Flags in period photographs may be carried on dressing lines and signal halyards. Ratlines are the rope ladder rungs in the shrouds, rather than the name for those flag-hoisting lines. For period examples, see [Royal Museums Greenwich's HMS Gossamer at Spithead, dressed overall, 28 June 1897](https://prints.rmg.co.uk/products/hms-gossamer-1860-on-28-june-1897-g01064) and the [Naval History and Heritage Command's 1907–08 diary describing the Great White Fleet dressed with flags in rainbow fashion](https://www.history.navy.mil/research/library/manuscripts/t/rear-admiral-charles-m-thomas-great-white-fleet-diary.html). The [Navy Regulations](https://www.marines.mil/portals/1/publications/united%20states%20navy%20regulations.pdf) also describe the bow-to-masthead-to-stern full-dressing arrangement. These references establish the visual practice, not an exact four-navy 1901 code or ceremonial sequence.

The model uses sixteen original procedural geometric patterns in red, white, blue, yellow and black, varied deterministically by ship and nationality. Rectangles, tapered pennants and forked flags mix along the lines. They are inspired by maritime signal-flag geometry; no claim is made that their sequences encode a valid period signal. National ensigns and commissioning pennants retain the preceding designs.

Each hoist edge follows its dressing-line or halyard tangent, with the cloth's free edge responding to apparent wind. Gravity stays vertical in world space even where the rope slopes. Ripple/droop scale with the smaller cloth size, while independent phases keep the flags from moving in lockstep. All cloth is two-sided and runs while the operational clock is paused. Bow and stern staffs reach the deck. Ropes and ship motion share the same hull transform. This remains a visual animation, not a rope/cloth physics solver.

Full dress adds 1,092 active cloth surfaces across the fleet, alongside the 84 existing national colours. Yardarm hoists add 504 instead. Only the selected rigging group renders; both arrangements are built and cached. Their small cloth grids use 20 by 4 cells, with mipmapped shared pattern textures. The [polish baseline](Pantelleria-Polish-Baseline.csv) and [base comparison](Pantelleria-Rigging-Base-Comparison.csv) verify that ship, gun-mount and existing national-flag geometry stays unchanged; extra rigging geometry is accounted for separately.

Verification covers every ship's two extra arrangements and three mode switches, finite rope/cloth geometry, dressing-staff and halyard deck contact, existing gun/deck contacts and national wind behavior, plus table saves and protocol bounds. Standalone checks count all active flags in each mode and capture the two full-dress angles, yardarm hoists, colours only and existing views, before running fleet movement, exhaust, firing and scenario-switch tests. Verification temporarily changes the display without overwriting the user's saved preference.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaRigging -logFile Artifacts/Pantelleria/build-rigging.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Rigging -OutputDirectory Artifacts/Pantelleria/Rigging
.\Start-Ironclads.ps1 -Pantelleria
```

The launcher prefers `Builds/Windows-Pantelleria-Rigging/Ironclads.exe -pantelleria`. Earlier builds remain separate, and existing game windows are left open.
