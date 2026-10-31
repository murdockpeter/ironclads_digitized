$ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-v1.0.0\Ironclads.exe'
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { throw 'Builds\Windows-v1.0.0\Ironclads.exe is missing. Build the version 1.0.0 player first.' }
Start-Process -FilePath $ironcladsPlayer -WorkingDirectory (Split-Path $ironcladsPlayer) -ArgumentList @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000')
