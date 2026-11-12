param([switch]$Ferrol,[switch]$IronFire,[switch]$Ulsan,[switch]$PatrickHenry,[switch]$SuppliedModels,[switch]$James1865)
$ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-James1865\Ironclads.exe'
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { throw 'Builds\Windows-James1865\Ironclads.exe is missing. Build with Ironclads.Editor.ProjectBuilder.DeliverJames.' }
$ironcladsArguments = @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000')
if($James1865){$ironcladsArguments+='-james-1865'}elseif($Ulsan){$ironcladsArguments+='-ulsan'}elseif($SuppliedModels){$ironcladsArguments+='-hampton-roads';$ironcladsArguments+='-supplied-models'}elseif($PatrickHenry){$ironcladsArguments+='-hampton-roads';$ironcladsArguments+='-patrick-henry'}elseif(-not $Ferrol){$ironcladsArguments+='-hampton-roads'}
if($IronFire -and -not $Ulsan -and -not $James1865){$ironcladsArguments+='-iron-fire'}
Start-Process -FilePath $ironcladsPlayer -WorkingDirectory (Split-Path $ironcladsPlayer) -ArgumentList $ironcladsArguments
