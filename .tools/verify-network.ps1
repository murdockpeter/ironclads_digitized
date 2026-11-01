param([string]$BuildDirectory='Builds\Windows-TCP',[switch]$Hampton,[switch]$IronFire)
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
 if($Hampton){$hostArguments+='-hampton-network'}
 if($IronFire){$hostArguments+='-iron-fire-network'}
 $networkHost = Start-Process -FilePath $networkPlayer -ArgumentList $hostArguments -WorkingDirectory (Split-Path $networkPlayer) -WindowStyle Hidden -PassThru
 Start-Sleep -Milliseconds 500
 $guestArguments = @('-batchmode','-nographics','-verify-network','guest',$testPort,('"{0}"' -f $networkOutput),'-logFile',('"{0}"' -f (Join-Path $networkOutput 'guest.log')))
 if($Hampton){$guestArguments+='-hampton-network'}
 if($IronFire){$guestArguments+='-iron-fire-network'}
 $networkGuest = Start-Process -FilePath $networkPlayer -ArgumentList $guestArguments -WorkingDirectory (Split-Path $networkPlayer) -WindowStyle Hidden -PassThru
 if(-not $networkHost.WaitForExit(60000)){throw ('Network host verification timed out. Logs: '+$networkOutput)}
 if(-not $networkGuest.WaitForExit(1000)){throw ('Network guest verification timed out. Logs: '+$networkOutput)}
 if($networkHost.ExitCode -ne 0 -or $networkGuest.ExitCode -ne 0){throw ('Network verification failed. Logs: '+$networkOutput)}
 Get-Content (Join-Path $networkOutput 'network-host.txt')
 Get-Content (Join-Path $networkOutput 'network-guest.txt')
 'Reports: '+$networkOutput
} finally {
 foreach($testProcess in @($networkHost,$networkGuest)){if($null -ne $testProcess -and -not $testProcess.HasExited){$testProcess.Kill()}}
}
