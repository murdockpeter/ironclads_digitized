param([string]$BuildDirectory='Builds\Windows-Multiplayer',[switch]$Hampton,[switch]$IronFire,[switch]$Ulsan)
# Compatibility entry point: all modes now exercise the protocol 7 lobby.
$networkMode='ferrol'
if($Ulsan){$networkMode='ulsan'}elseif($IronFire){$networkMode='ironfire'}elseif($Hampton){$networkMode='hampton'}
& (Join-Path $PSScriptRoot 'verify-lobby.ps1') -BuildDirectory $BuildDirectory -Mode $networkMode -Guests 2
