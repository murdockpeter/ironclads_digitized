# Animated naval colours — Pantelleria 1.10.3-dev

All 42 ships carry two animated cloth meshes: a national naval ensign on the after mast and a commissioning pennant on the foremast. These replace the previous blue-side/neutral identification pennant. Existing tactical markers remain selectable.

| Navy | Ensign | Pennant interpretation |
| --- | --- | --- |
| Britain | White Ensign, red St George's cross and Union canton | White, red cross at the hoist |
| Italy | Green/white/red with crowned Savoy shield | Tricolour commissioning flame |
| France | Blue/white/red, 30:33:37 naval stripe widths | Tapered blue/white/red, 1:1:3 lengths |
| Russia | Blue St Andrew's saltire on white | Saltire hoist with long white, forked fly |

The artwork is generated in code, shared across the fleet and mipmapped. No museum images or third-party drawings are redistributed. Crown, shield, Union canton and pennant heraldry are simplified. Flag dimensions (about 4–4.6 m fly; pennants 8 m), mast positions, hoist details and flutter are illustrative rather than a surveyed ship/refit configuration. The Italian pennant omits the small royal device. These are commissioning pennants, not automatic admiral/commodore rank flags or operational signal hoists. Flags remain displayed after sunset; watch routines are not automated.

Cloth streams along apparent wind: the scenario's wind-from bearing is converted to a downwind vector, then ship velocity is subtracted. The shader pins the hoist, increases flutter with wind speed, adds gravity droop in calm conditions and varies phases between ships. Animation uses elapsed display time, so it runs while the operational clock is paused without advancing that clock. Patterns are visible on both faces; reverse faces naturally show the reverse design. No cloth physics solver or extra network state is used. Current weather and ship motion drive the flags on every client.

The two cloth surfaces add 574 vertices per ship before removal of the old static rectangle, and two render meshes. The retained [second-detail baseline](Pantelleria-Detail2-Baseline.csv) and [flags comparison](Pantelleria-Flags-Comparison.csv) preserve the previous model measurements. Static geometry stays at detail pass 2.

## References

- [Royal Museums Greenwich: White Ensign, after 1801](https://www.rmg.co.uk/collections/objects/rmgc-object-681).
- [Italian Navy: history of the naval ensign](https://www.marina.difesa.it/media-cultura/Notiziario-online/Pagine/20191109_La_Bandiera_della_Marina.aspx), including the crowned royal naval design regulated in 1860.
- [France: naval ensign and pennant](https://www.fotw.info/flags/fr~ens.html), citing the 1794 decree and 1853 regulations for proportions.
- [Royal Museums Greenwich: Russian naval ensign, before 1917](https://www.rmg.co.uk/collections/objects/rmgc-object-570).
- [Historical Russian pennants](https://www.crwflags.com/fotw/Flags/ru~xviii.html), including the 1865–1917 pattern, and [Italian naval flags](https://www.crwflags.com/fotw/flags/it~king.html). Pennant taper and small devices are simplified at this model scale.

## Build and check

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverPantelleriaFlags -logFile Artifacts/Pantelleria/build-flags.log
.\Tools\Verify-Pantelleria.ps1 -BuildDirectory Builds/Windows-Pantelleria-Flags -OutputDirectory Artifacts/Pantelleria/Flags
.\Start-Ironclads.ps1 -Pantelleria
```

Editor checks cover each nation's attachment, apparent-wind subtraction, changes of ship course and finite calm orientation, plus existing roster/model/battery/save checks. Standalone review checks all 84 cloth renderers and a frozen operational clock, captures successive animation frames for all four nations, a reverse face and each pennant, then runs the fleet's existing movement/fire/module-switch tests. The shader is explicitly included and checked during the build.
