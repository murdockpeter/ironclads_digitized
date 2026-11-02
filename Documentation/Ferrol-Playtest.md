# Ferrol offline playtest - 1.5.1-dev

Run from the project directory in PowerShell:

```powershell
.\Start-Ironclads.ps1 -Ferrol
```

Executable: `Builds/Windows-Ferrol-Playtest/Ironclads.exe`. Directly launching this executable also opens Ferrol. The launcher without switches still opens Hampton Roads.

Ferrol has 82 columns by 74 rows (6,068 hexes), twice each previous dimension and four times the cell count. Adjacent centers remain 100 yards apart. The chart extends east and north, preserving existing local IDs and saved positions. Hampton Roads keeps its 41 by 37 board.

Opening positions: Niagara hex **4127**, heading **000 degrees**; Stonewall hex **4147**, heading **180 degrees**. Their centers are exactly twenty hexes / 2,000 yards / 1,828.8 meters apart. Both begin stationary, facing toward each other, near the expanded board center. This is an umpire-selected encounter setup for the two-ship what-if, not a historical reconstruction of a fought battle. Weather, date and working batteries are retained.

For Discord remote desktop, leave Multiplayer disconnected. Players can submit orders in the agreed text channel. Select each ship under Ships, then place by column/row or Shift-click and set facing with the 60-degree buttons. End Turn advances the clock three minutes and leaves both positions where the umpire placed them. Use Save table for the current position and Log positions for a CSV record. Fleet view fits both ships; 1/2 focuses an individual ship.

Existing saved tables load their saved positions. Restore the Ferrol scenario under Scenarios to get these new opening positions; it does not overwrite a saved table until you choose Save table.

The displayed grid, placement bounds, coordinate entry and gunnery target picking all use the scenario's chart dimensions. Offline play needs no network connection or lobby.
