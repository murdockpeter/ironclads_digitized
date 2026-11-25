param([string]$BuildDirectory='Builds\Windows-Multiplayer',[ValidateSet('james','ferrol','hampton','ironfire','ulsan','pantelleria')][string]$Mode='james',[int]$Guests=6)
$ErrorActionPreference='Stop'
if($Guests -lt 2 -or $Guests -gt 6){throw 'Guests must be from 2 to 6.'}
if($Mode -ne 'james' -and $Mode -ne 'ulsan' -and $Guests -gt 2){throw 'Use two guests for the two-ship regression scenarios.'}
$lobbyProject=Split-Path $PSScriptRoot -Parent
$lobbyPlayer=Join-Path (Join-Path $lobbyProject $BuildDirectory) 'Ironclads.exe'
$lobbyOutput=Join-Path $lobbyProject ('Artifacts\Lobby\'+$Mode+'-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $lobbyOutput -Force | Out-Null
$lobbyProbe=[System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback,0)
$lobbyProbe.Start();$lobbyPort=([System.Net.IPEndPoint]$lobbyProbe.LocalEndpoint).Port;$lobbyProbe.Stop()
$lobbyProcesses=@()
try {
 for($lobbyRole=0;$lobbyRole -le $Guests;$lobbyRole++){
  $lobbyArguments=@('-batchmode','-nographics','-verify-lobby',$lobbyRole,$lobbyPort,('"{0}"' -f $lobbyOutput),$Guests,$Mode,'-logFile',('"{0}"' -f (Join-Path $lobbyOutput ('player-'+$lobbyRole+'.log'))))
  $lobbyProcesses+=Start-Process -FilePath $lobbyPlayer -ArgumentList $lobbyArguments -WorkingDirectory (Split-Path $lobbyPlayer) -WindowStyle Hidden -PassThru
  if($lobbyRole -eq 0){
   $lobbyReadyDeadline=[DateTime]::UtcNow.AddSeconds(55)
   while(-not (Test-Path -LiteralPath (Join-Path $lobbyOutput 'host-ready.txt'))){
    if($lobbyProcesses[0].HasExited -or [DateTime]::UtcNow -gt $lobbyReadyDeadline){throw ('Host failed to start. Reports: '+$lobbyOutput)}
    Start-Sleep -Milliseconds 200
   }
  }
 }
 $lobbyDeadline=[DateTime]::UtcNow.AddSeconds(150)
 while(@($lobbyProcesses | Where-Object {-not $_.HasExited}).Count -gt 0){
  if([DateTime]::UtcNow -gt $lobbyDeadline){throw ('Verification timed out. Reports: '+$lobbyOutput)}
  Start-Sleep -Milliseconds 300
 }
 for($lobbyRole=0;$lobbyRole -le $Guests;$lobbyRole++){
  $lobbyReport=Join-Path $lobbyOutput ('lobby-'+$lobbyRole+'.txt')
  if(-not (Test-Path -LiteralPath $lobbyReport)){throw ('Missing report: '+$lobbyReport)}
  Get-Content -LiteralPath $lobbyReport
 }
 if(@($lobbyProcesses | Where-Object {$_.ExitCode -ne 0}).Count -gt 0){throw ('Verification failed. Reports: '+$lobbyOutput)}
 'Reports: '+$lobbyOutput
} finally {
 foreach($lobbyProcess in $lobbyProcesses){if(-not $lobbyProcess.HasExited){$lobbyProcess.Kill()}}
}
