param([string]$BuildDirectory='Builds\Windows-ShipSheets',[switch]$Hampton,[switch]$IronFire,[switch]$Ulsan)
$networkProject = Split-Path $PSScriptRoot -Parent
$networkPlayer = Join-Path (Join-Path $networkProject $BuildDirectory) 'Ironclads.exe'
$networkOutput = Join-Path $networkProject ('Artifacts\Network\'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $networkOutput -Force | Out-Null
$portProbe = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback,0)
$portProbe.Start()
$testPort = ([System.Net.IPEndPoint]$portProbe.LocalEndpoint).Port
$portProbe.Stop()
$networkHost = $null
$networkGuest = $null
try {
 $hostArguments = @('-batchmode','-nographics','-verify-network','host',$testPort,('"{0}"' -f $networkOutput),'-logFile',('"{0}"' -f (Join-Path $networkOutput 'host.log')))
 if($Ulsan){$hostArguments=$hostArguments | ForEach-Object {if($_ -eq '-verify-network'){'-verify-ulsan-network'}else{$_}}}
 if($Hampton){$hostArguments+='-hampton-network'}
 if($IronFire){$hostArguments+='-iron-fire-network'}
 $networkHost = Start-Process -FilePath $networkPlayer -ArgumentList $hostArguments -WorkingDirectory (Split-Path $networkPlayer) -WindowStyle Hidden -PassThru
 for($readyAttempt=0;$readyAttempt -lt 400 -and -not (Test-Path -LiteralPath (Join-Path $networkOutput 'host-ready.txt'));$readyAttempt++) {
  if($networkHost.HasExited){throw ('Network host exited before ready. Logs: '+$networkOutput)}
  Start-Sleep -Milliseconds 100
 }
 if(-not (Test-Path -LiteralPath (Join-Path $networkOutput 'host-ready.txt'))){throw ('Network host did not finish scene preparation. Logs: '+$networkOutput)}
 $guestArguments = @('-batchmode','-nographics','-verify-network','guest',$testPort,('"{0}"' -f $networkOutput),'-logFile',('"{0}"' -f (Join-Path $networkOutput 'guest.log')))
 if($Ulsan){$guestArguments=$guestArguments | ForEach-Object {if($_ -eq '-verify-network'){'-verify-ulsan-network'}else{$_}}}
 if($Hampton){$guestArguments+='-hampton-network'}
 if($IronFire){$guestArguments+='-iron-fire-network'}
 $networkGuest = Start-Process -FilePath $networkPlayer -ArgumentList $guestArguments -WorkingDirectory (Split-Path $networkPlayer) -WindowStyle Hidden -PassThru
 if(-not $networkHost.WaitForExit(60000)){throw ('Network host verification timed out. Logs: '+$networkOutput)}
 if(-not $networkGuest.WaitForExit(1000)){throw ('Network guest verification timed out. Logs: '+$networkOutput)}
 if($networkHost.ExitCode -ne 0 -or $networkGuest.ExitCode -ne 0){throw ('Network verification failed. Logs: '+$networkOutput)}
 if($Ulsan){Get-Content (Join-Path $networkOutput 'ulsan-network-host.txt');Get-Content (Join-Path $networkOutput 'ulsan-network-guest.txt')}else{Get-Content (Join-Path $networkOutput 'network-host.txt');Get-Content (Join-Path $networkOutput 'network-guest.txt')}
 'Reports: '+$networkOutput
} finally {
 foreach($testProcess in @($networkHost,$networkGuest)){if($null -ne $testProcess -and -not $testProcess.HasExited){$testProcess.Kill()}}
}
