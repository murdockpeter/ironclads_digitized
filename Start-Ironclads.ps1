param([switch]$Ferrol,[switch]$IronFire)
$ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-HamptonRoads-Union\Ironclads.exe'
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { throw 'Builds\Windows-HamptonRoads-Union\Ironclads.exe is missing. Build with Ironclads.Editor.ProjectBuilder.DeliverUnionShips.' }
$ironcladsArguments = @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000')
if(-not $Ferrol){$ironcladsArguments+='-hampton-roads'}
if($IronFire){$ironcladsArguments+='-iron-fire'}
Start-Process -FilePath $ironcladsPlayer -WorkingDirectory (Split-Path $ironcladsPlayer) -ArgumentList $ironcladsArguments
