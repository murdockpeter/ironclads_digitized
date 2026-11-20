using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        void ReleaseControls()
        {
            var updates=GetComponent<ReleaseChecker>();if(updates==null)return;
            if(Button(updates.Checking?"Checking for updates...":"Check for updates"))updates.Check();
            if(!string.IsNullOrEmpty(updates.Status))GUILayout.Label(updates.Status,small);
            if(updates.UpdateAvailable&&Button("Download newer release on GitHub"))Application.OpenURL(ReleaseChecker.ReleasesUrl);
            bool automatic=GUILayout.Toggle(updates.Automatic,"Check for updates at startup",toggle);
            if(automatic!=updates.Automatic)updates.SetAutomatic(automatic);
            if(!string.IsNullOrEmpty(updates.Status)&&Button("GitHub releases"))Application.OpenURL(ReleaseChecker.ReleasesUrl);
        }
    }
}
