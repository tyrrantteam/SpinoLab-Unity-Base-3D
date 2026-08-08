using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class RemoteConfig : SingletonMonoDontDestroy<RemoteConfig>
{
    public RemoteConfig(string className) : base(className)
    {
    }

    public float TimeShowAds
    {
        get
        {
            return PlayerPrefs.GetFloat("caping_time", 15f);
        }
        set
        {
            PlayerPrefs.SetFloat("caping_time", value);
        }
    }

    public float TaichiThreshold
    {
        get
        {
            return PlayerPrefs.GetFloat("taichi_threshold", 0.02f);
        }
        set
        {
            PlayerPrefs.SetFloat("taichi_threshold", value);
        }
    }


    public int ClickShowAds
    {
        get
        {
            return PlayerPrefs.GetInt("caping_click", 3);
        }
        set
        {
            PlayerPrefs.SetInt("caping_click", value);
        }
    }


    public bool AllowShowAOA
    {
        get
        {
            return PlayerPrefs.GetInt("allow_show_aoa", 1) == 1 ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt("allow_show_aoa", value ? 1 : 0);
        }
    }

    public int AOAShow
    {
        get
        {
            return PlayerPrefs.GetInt("start_show_aoa", 2);
        }
        set
        {
            PlayerPrefs.SetInt("start_show_aoa", value);
        }
    }

    public Task FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            Debug.Log("Fetch canceled.");
        }
        else if (fetchTask.IsFaulted)
        {
            Debug.Log("Fetch encountered an error.");
        }
        else if (fetchTask.IsCompleted)
        {
            Debug.Log("Fetch completed successfully!");
        }
        var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
        Debug.Log(info.LastFetchStatus);
        switch (info.LastFetchStatus)
        {
            case Firebase.RemoteConfig.LastFetchStatus.Success:
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                GetData();
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case Firebase.RemoteConfig.FetchFailureReason.Error:
                        Debug.Log("Fetch failed for unknown reason");
                        break;
                    case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                        Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                        break;
                }
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Pending:
                Debug.Log("Latest Fetch call still pending.");
                break;
        }
    }

    void GetData()
    {
        TimeShowAds = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("caping_time").LongValue;
        ClickShowAds = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("caping_click").LongValue;

        AllowShowAOA = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("allow_show_aoa").BooleanValue;
        AOAShow = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("start_show_aoa").LongValue;

        TaichiThreshold = (float)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("taichi_threshold").DoubleValue;
    }
}

