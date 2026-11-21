using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Ironclads
{
    public sealed class ReleaseChecker : MonoBehaviour
    {
        public const string ReleasesUrl="https://github.com/murdockpeter/ironclads_digitized/releases/latest";
        const string ApiUrl="https://api.github.com/repos/murdockpeter/ironclads_digitized/releases/latest";
        public string Status {get;private set;}="";
        public bool Checking {get;private set;}
        public bool UpdateAvailable {get;private set;}
        public bool Automatic=>PlayerPrefs.GetInt("Updates.Automatic",1)!=0;
        UnityWebRequest active;
        [Serializable] public sealed class Release {public string tag_name;public bool draft;public bool prerelease;}
        public static bool TryVersion(string tag,out Version version)
        {
            version=null;
            if(string.IsNullOrWhiteSpace(tag))return false;
            return Version.TryParse(tag.Trim().TrimStart('v','V').Split('-','+')[0],out version);
        }
        public static bool IsNewer(string tag,string current)
        {
            return TryVersion(tag,out var latest)&&TryVersion(current,out var installed)&&latest>installed;
        }
        void Start()
        {
            if(Application.isBatchMode||Application.isEditor)return;
            if(Automatic)Check();
        }
        public void SetAutomatic(bool enabled){PlayerPrefs.SetInt("Updates.Automatic",enabled?1:0);PlayerPrefs.Save();}
        public void Check(){if(!Checking)StartCoroutine(Fetch());}
        IEnumerator Fetch()
        {
            Checking=true;UpdateAvailable=false;Status="Checking GitHub releases...";
            using(var request=UnityWebRequest.Get(ApiUrl))
            {
                active=request;request.timeout=15;
                request.SetRequestHeader("Accept","application/vnd.github+json");
                request.SetRequestHeader("User-Agent","Ironclads/"+Application.version);
                yield return request.SendWebRequest();
                if(request.result!=UnityWebRequest.Result.Success)
                    Status=request.responseCode==403||request.responseCode==429?"GitHub limit reached. Try again later.":"Update check unavailable. You can keep playing offline.";
                else
                {
                    Release release=null;
                    try{release=JsonUtility.FromJson<Release>(request.downloadHandler.text);}catch(ArgumentException){}
                    if(release==null||release.draft||release.prerelease||!TryVersion(release.tag_name,out _))Status="No stable release information available.";
                    else
                    {
                        UpdateAvailable=IsNewer(release.tag_name,Application.version);
                        Status=UpdateAvailable?"New release available: "+release.tag_name:"You have the latest release (v"+Application.version+").";
                    }
                }
                active=null;
            }
            Checking=false;Debug.Log("RELEASE CHECK: "+Status);
        }
        void OnDestroy(){if(active!=null)active.Abort();}
    }
}
