param([string]$OutputDirectory,[string]$BuildDirectory='Builds\Windows-Pantelleria')
$ErrorActionPreference='Stop'
$campaignRoot=Split-Path $PSScriptRoot
if(-not $OutputDirectory){$OutputDirectory=Join-Path $campaignRoot 'Artifacts\Pantelleria\Runtime'}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
if(-not ('PantelleriaReviewWindow' -as [type])){
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class PantelleriaReviewWindow {
 public delegate bool EnumProc(IntPtr h,IntPtr l);
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc p,IntPtr l);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h,out uint id);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h,int command);
 public static IntPtr Find(uint id){IntPtr found=IntPtr.Zero;EnumWindows((h,l)=>{uint p;GetWindowThreadProcessId(h,out p);if(p==id){found=h;return false;}return true;},IntPtr.Zero);return found;}
}
'@
}
$campaignPlayer=Join-Path (Join-Path $campaignRoot $BuildDirectory) 'Ironclads.exe'

$campaignArgs='-verify-pantelleria "'+$OutputDirectory+'" -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 -logFile "'+$OutputDirectory+'\player.log"'
$campaignProcess=Start-Process -FilePath $campaignPlayer -WorkingDirectory (Split-Path $campaignPlayer) -ArgumentList $campaignArgs -WindowStyle Hidden -PassThru
# A hidden Unity window returns black screenshots. Render off-screen without taking focus.
for($campaignAttempt=0;$campaignAttempt -lt 100;$campaignAttempt++){
    $campaignHandle=[PantelleriaReviewWindow]::Find($campaignProcess.Id)
    if($campaignHandle -ne [IntPtr]::Zero){
        [PantelleriaReviewWindow]::SetWindowPos($campaignHandle,[IntPtr]::Zero,-16000,-16000,0,0,0x15) | Out-Null
        [PantelleriaReviewWindow]::ShowWindow($campaignHandle,4) | Out-Null
        break
    }
    Start-Sleep -Milliseconds 100
}
if($campaignProcess.WaitForExit(45000)){
    if($campaignProcess.ExitCode -ne 0){throw "Pantelleria verification failed (exit $($campaignProcess.ExitCode)). See $OutputDirectory\player.log"}
    Get-Content -LiteralPath (Join-Path $OutputDirectory 'runtime-verification.txt')
}else{
    Write-Output "Pantelleria verification still running (PID $($campaignProcess.Id)). Output: $OutputDirectory"
}
