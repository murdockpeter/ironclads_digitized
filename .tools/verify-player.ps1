param([switch]$DefaultGraphics,[string]$BuildDirectory='Builds\Windows',[switch]$Hampton)
$projectPath = Split-Path $PSScriptRoot -Parent
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class IroncladsWindow {
 public delegate bool EnumProc(IntPtr h, IntPtr l);
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback, IntPtr l);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint id);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int w, int ht, uint flags);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int command);
 public static IntPtr Find(uint process) {
  IntPtr result=IntPtr.Zero;
  EnumWindows((h,l)=>{uint p; GetWindowThreadProcessId(h,out p); if(p==process){result=h;return false;}return true;},IntPtr.Zero);
  return result;
 }
}
'@
$playerArgs=@('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000','-verify-runtime',('"{0}"' -f (Join-Path $projectPath 'Artifacts')),'-logFile',('"{0}"' -f (Join-Path $projectPath 'Artifacts\player.log')))
if($Hampton){$playerArgs=@('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000','-verify-hampton',('"{0}"' -f (Join-Path $projectPath 'Artifacts')),'-logFile',('"{0}"' -f (Join-Path $projectPath 'Artifacts\hampton-player.log')))}
if(-not $DefaultGraphics){$playerArgs=@('-force-d3d11')+$playerArgs}
$buildPath=Join-Path $projectPath $BuildDirectory
$playerProcess=Start-Process -FilePath (Join-Path $buildPath 'Ironclads.exe') -ArgumentList $playerArgs -WorkingDirectory $buildPath -WindowStyle Hidden -PassThru
# Keep the automated graphics window entirely outside the visible desktop and never activate it.
for($attempt=0;$attempt -lt 100;$attempt++) {
 $handle=[IroncladsWindow]::Find($playerProcess.Id)
 if($handle -ne [IntPtr]::Zero) {
  [IroncladsWindow]::SetWindowPos($handle,[IntPtr]::Zero,-16000,-16000,0,0,0x15) | Out-Null
  [IroncladsWindow]::ShowWindow($handle,4) | Out-Null
  break
 }
 Start-Sleep -Milliseconds 100
}
if(-not $playerProcess.WaitForExit(60000)){throw 'Player verification has not finished; refusing to read an older report.'}
if($playerProcess.ExitCode -ne 0){throw "Player verification exited with code $($playerProcess.ExitCode). See Artifacts/player.log and runtime-verification.txt."}
if($Hampton){Get-Content (Join-Path $projectPath 'Artifacts\hampton-runtime-verification.txt')}else{Get-Content (Join-Path $projectPath 'Artifacts\runtime-verification.txt')}
Get-ChildItem (Join-Path $projectPath 'Artifacts') -Filter 'Player*.png' | Select-Object Name,Length
