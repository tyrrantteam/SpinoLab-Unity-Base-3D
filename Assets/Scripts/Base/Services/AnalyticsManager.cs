using Firebase.Analytics;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using Base.Core.Services;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    private Dictionary<AdsEvent, string> AdsKey = new Dictionary<AdsEvent, string>
        {
            {AdsEvent.RewardOffer, "ads_reward_offer"},
            {AdsEvent.RewardClick, "ads_reward_click"},
            {AdsEvent.RewardShow, "ads_reward_show"},
            {AdsEvent.RewardFail, "ads_reward_fail"},
            {AdsEvent.RewardComplete, "ads_reward_complete"},
            {AdsEvent.RewardLoad, "ads_reward_load"},
            {AdsEvent.InterFail, "ad_inter_fail"},
            {AdsEvent.InterLoad, "ad_inter_load"},
            {AdsEvent.InterShow, "ad_inter_show"},
            {AdsEvent.InterClick, "ad_inter_click"},
            {AdsEvent.InterImpression, "ad_inter_impression"},
        };

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    // Use this for initialization
    void Start()
    {
        // Initialize Firebase
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            Debug.Log(dependencyStatus);
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Debug.Log("[Firebase] 1 - Available");

                Firebase.FirebaseApp app = null;

                try
                {
                    Debug.Log("[Firebase] 2 - Creating DefaultInstance");

                    app = Firebase.FirebaseApp.DefaultInstance;

                    Debug.Log("[Firebase] 3 - DefaultInstance created");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Firebase] DefaultInstance FAILED:\n{e}");
                    return;
                }

                Debug.Log("[Firebase] 4 - Project ID: " + app.Options.ProjectId);
                Debug.Log("[Firebase] 5 - App ID: " + app.Options.AppId);

                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);

                Debug.Log("[Firebase] 6 - Analytics event sent");

                RemoteConfig.Instance.FetchDataAsync();

                Debug.Log("[Firebase] 7 - Remote Config fetch started");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    public void LogAdsEvent(AdsEvent eventType, string placement, string errorMsg = "")
    {
        if (AdsKey.ContainsKey(eventType))
        {
            var param = new List<Parameter>();

            if (placement != null)
            {
                if (!placement.Equals(string.Empty))
                {
                    param.Add(new Parameter("placement", placement));
                }

                if (!errorMsg.Equals(string.Empty))
                {
                    param.Add(new Parameter("errormsg", errorMsg));
                }
            }

            FirebaseAnalytics.LogEvent(AdsKey[eventType], param.ToArray());
        }
        else
        {
            Debug.Log("gg No such event type " + eventType);
        }
    }

    public void SetUserProperty(string name, string value)
    {
        Debug.Log(name + ": " + value);
        FirebaseAnalytics.SetUserProperty(name, value);
    }

    public void LogGameEvent(string eventName)
    {
        FirebaseAnalytics.LogEvent(eventName);
        Debug.Log(eventName);
    }

    public void LogGameEvent(string eventName, string paramName, string paramValue)
    {
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    public void LogGameEvent(string eventName, Dictionary<string, object> parameters)
    {
        Parameter[] fireBaseParameters = new Parameter[parameters.Count];

        int index = 0;
        foreach (KeyValuePair<string, object> kv in parameters)
        {
            fireBaseParameters[index] = ParseParameter(kv.Key, kv.Value);
            Debug.Log("FirebaseAnalytics: " + eventName + "- Parameters: " + kv.Key + "--" + kv.Value);
            index++;
        }
        FirebaseAnalytics.LogEvent(eventName, fireBaseParameters);
    }

    public string GetConfigData(string cfgName)
    {
        return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(cfgName).StringValue;
    }

    Firebase.Analytics.Parameter ParseParameter(string paramName, object paramValue)
    {
        if (paramValue is string)
        {
            return new Firebase.Analytics.Parameter(paramName, paramValue as string);
        }
        else if (paramValue is float)
        {
            return new Firebase.Analytics.Parameter(paramName, (float)paramValue);
        }
        else if (paramValue is double)
        {
            return new Firebase.Analytics.Parameter(paramName, (double)paramValue);
        }
        else if (paramValue is decimal)
        {
            return new Firebase.Analytics.Parameter(paramName, (double)((decimal)paramValue));
        }
        else if (paramValue is int)
        {
            return new Firebase.Analytics.Parameter(paramName, (int)paramValue);
        }
        else if (paramValue is long)
        {
            return new Firebase.Analytics.Parameter(paramName, (long)paramValue);
        }
        else
        {
            return new Firebase.Analytics.Parameter(paramName, paramValue.ToString());
        }
    }
}

