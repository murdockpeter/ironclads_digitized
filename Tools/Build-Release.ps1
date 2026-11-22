param([string]$UnityEditor='C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$releaseRoot=Split-Path $PSScriptRoot -Parent
$releaseVersion='1.7.0'
$releaseStage=Join-Path $releaseRoot ('Artifacts\Release-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $releaseStage -Force | Out-Null
$releaseSource=Join-Path $releaseStage 'source.zip'
git -C $releaseRoot archive --format=zip --output=$releaseSource HEAD
if($LASTEXITCODE -ne 0){throw 'Source archive failed.'}
$releaseProject=Join-Path $releaseStage 'Project'
Expand-Archive -LiteralPath $releaseSource -DestinationPath $releaseProject
$releaseLog=Join-Path $releaseStage 'build.log'
$releaseProcess=Start-Process -FilePath $UnityEditor -WindowStyle Hidden -PassThru -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$releaseProject+'"'),'-executeMethod','Ironclads.Editor.ProjectBuilder.DeliverRelease','-logFile',('"'+$releaseLog+'"'))
$releaseProcess.WaitForExit()
if($releaseProcess.ExitCode -ne 0){throw ('Unity build failed. See '+$releaseLog)}
$releasePlayer=Join-Path $releaseProject ('Builds\Windows-'+$releaseVersion)
if(-not (Test-Path -LiteralPath (Join-Path $releasePlayer 'Ironclads.exe'))){throw 'Expected release player is missing.'}
Copy-Item -LiteralPath (Join-Path $releaseProject 'Documentation') -Destination (Join-Path $releasePlayer 'Documentation') -Recurse
foreach($releaseFile in @('README.md','THIRD-PARTY-NOTICES.md','Start-Ironclads.ps1')){Copy-Item -LiteralPath (Join-Path $releaseProject $releaseFile) -Destination $releasePlayer}
$releaseDestination=Join-Path $releaseRoot ('Builds\Windows-v'+$releaseVersion)
if(Test-Path -LiteralPath $releaseDestination){throw ('Destination already exists: '+$releaseDestination)}
Copy-Item -LiteralPath $releasePlayer -Destination $releaseDestination -Recurse
Write-Output ('Release player: '+$releaseDestination)
Write-Output ('Clean staging project and log: '+$releaseStage)
