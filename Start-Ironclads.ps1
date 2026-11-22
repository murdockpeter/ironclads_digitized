param([switch]$Ferrol,[switch]$IronFire,[switch]$Ulsan,[switch]$PatrickHenry,[switch]$SuppliedModels,[switch]$James1865,[switch]$CloseAction)
$ironcladsPlayer = Join-Path $PSScriptRoot 'Ironclads.exe'
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Bows3\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Bows2\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-HeadFraming\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-RoundedBows\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-SlimBows\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-IntegratedBows\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-CurvedHeads\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Interiors\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Headsails\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Spars\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-SmokeScale\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Smoke\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Canvas\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Hull\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-Detail\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction-SternFix\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-CloseAction\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-ExpandedChart\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-v1.7.0\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { $ironcladsPlayer = Join-Path $PSScriptRoot 'Builds\Windows-Multiplayer\Ironclads.exe' }
if (-not (Test-Path -LiteralPath $ironcladsPlayer)) { throw 'Builds\Windows-Multiplayer\Ironclads.exe is missing. Build with Ironclads.Editor.ProjectBuilder.DeliverLobby.' }
$ironcladsArguments = @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000')
if($CloseAction){$ironcladsArguments+="-close-action"}elseif($James1865){$ironcladsArguments+='-james-1865'}elseif($Ulsan){$ironcladsArguments+='-ulsan'}elseif($SuppliedModels){$ironcladsArguments+='-hampton-roads';$ironcladsArguments+='-supplied-models'}elseif($PatrickHenry){$ironcladsArguments+='-hampton-roads';$ironcladsArguments+='-patrick-henry'}elseif(-not $Ferrol){$ironcladsArguments+='-hampton-roads'}
if($IronFire -and -not $CloseAction -and -not $Ulsan -and -not $James1865){$ironcladsArguments+='-iron-fire'}
# Restore optional private references only from this local checkout, after public builds.
$ironcladsLocalReferences = Join-Path $PSScriptRoot 'Assets\StreamingAssets\References'
if (Test-Path -LiteralPath $ironcladsLocalReferences) {
    $ironcladsPlayerReferences = Join-Path (Split-Path $ironcladsPlayer) 'Ironclads_Data\StreamingAssets\References'
    New-Item -ItemType Directory -Path $ironcladsPlayerReferences -Force | Out-Null
    Get-ChildItem -LiteralPath $ironcladsLocalReferences -File | Where-Object { $_.Extension -in '.pdf','.jpg','.jpeg','.png' } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $ironcladsPlayerReferences -Force
    }
}
foreach ($caBook in @('CloseActionRules.pdf','CloseActionScenarios.pdf')) {
    $caBookSource=Join-Path $PSScriptRoot ('Documentation\'+$caBook)
    if(Test-Path -LiteralPath $caBookSource){
        $caRefs=Join-Path (Split-Path $ironcladsPlayer) 'Ironclads_Data\StreamingAssets\References'
        New-Item -ItemType Directory -Path $caRefs -Force | Out-Null
        Copy-Item -LiteralPath $caBookSource -Destination $caRefs -Force
    }
}
Start-Process -FilePath $ironcladsPlayer -WorkingDirectory (Split-Path $ironcladsPlayer) -ArgumentList $ironcladsArguments
