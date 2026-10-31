$ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-TCP\Ironclads.exe'
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { throw 'Builds\Windows-TCP\Ironclads.exe is missing. Build the multiplayer player first.' }
Start-Process -FilePath $ironcladsPlayer -WorkingDirectory (Split-Path $ironcladsPlayer) -ArgumentList @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000')
