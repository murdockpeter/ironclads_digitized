param([switch]$Ferrol,[switch]$IronFire,[switch]$Ulsan,[switch]$PatrickHenry,[switch]$SuppliedModels)
$ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-SeaAudio\Ironclads.exe'
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { throw 'Builds\Windows-SeaAudio\Ironclads.exe is missing. Build with Ironclads.Editor.ProjectBuilder.DeliverSeaAudio.' }
$ironcladsArguments = @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000')
if($Ulsan){$ironcladsArguments+='-ulsan'}elseif($SuppliedModels){$ironcladsArguments+='-hampton-roads';$ironcladsArguments+='-supplied-models'}elseif($PatrickHenry){$ironcladsArguments+='-hampton-roads';$ironcladsArguments+='-patrick-henry'}elseif(-not $Ferrol){$ironcladsArguments+='-hampton-roads'}
if($IronFire -and -not $Ulsan){$ironcladsArguments+='-iron-fire'}
Start-Process -FilePath $ironcladsPlayer -WorkingDirectory (Split-Path $ironcladsPlayer) -ArgumentList $ironcladsArguments
