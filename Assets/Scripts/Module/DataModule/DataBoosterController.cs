using NorskaLib.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DataProcessMechanic;
[CreateAssetMenu(fileName = "DataBooster", menuName = "DaTa/DataBooster")]
public class DataBoosterController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] ListBooster content;
    public ListBooster ContentContent => content;

  
    public int BoosterCount => content.listBoooster?.Count ?? 0;

    public bool TryGetBoosterByLevel(int level, out ProcessBoosterData data)
    {
        foreach (var t in content.listBoooster)
        {
            if (t.level == level)
            {
                data = t;
                return true;
            }
        }
        data = default;
        return false;
    }

    public ProcessBoosterData GetDataBoosterByType(BoosterType boosterType)
    {
        foreach (var t in content.listBoooster)
        {
            if (t.boosterType == boosterType)
            {
                return t;
            }
        }
        return default;
    }

    public ProcessBoosterData GetDataBoosterByLevel(int level)
    {
        foreach (var t in content.listBoooster)
        {
            if (t.level == level)
            {
                return t;
            }
        }
        return default;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Link Booster Sprites")]
    public void AutoLinkSprites(bool saveAsset = true)
    {
        if (content?.listBoooster == null || content.listBoooster.Count == 0) return;

        bool changed = false;
        for (int i = 0; i < content.listBoooster.Count; i++)
        {
            var item = content.listBoooster[i];

            // 1. Link imgMechanic (Icon preview booster)
            string mechanicPath = GetMechanicPath(item.boosterType);
            var mechanicSprite = LoadSpriteAtPath(mechanicPath);
            if (mechanicSprite != null && item.imgMechanic != mechanicSprite)
            {
                item.imgMechanic = mechanicSprite;
                changed = true;
            }

            // 2. Link imgIconBooster (Icon button booster)
            string buttonIconPath = GetButtonIconPath(item.boosterType);
            var buttonIconSprite = LoadSpriteAtPath(buttonIconPath);
            if (buttonIconSprite != null && item.imgIconBooster != buttonIconSprite)
            {
                item.imgIconBooster = buttonIconSprite;
                changed = true;
            }

            content.listBoooster[i] = item;
        }

        if (changed)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            if (saveAsset)
            {
                UnityEditor.AssetDatabase.SaveAssets();
            }
            Debug.Log("<color=green>[DataBoosterController]</color> Auto linked imgMechanic and imgIconBooster for boosters successfully!");
        }
    }

    public void AutoLinkMechanicIcons(bool saveAsset = true) => AutoLinkSprites(saveAsset);

    public static string GetMechanicPath(BoosterType boosterType)
    {
        return boosterType switch
        {
            BoosterType.booster1 => "Assets/Texture/UI/Booster/IconPreviewBooster/Booster_pre_1.png",
            BoosterType.booster2 => "Assets/Texture/UI/Booster/IconPreviewBooster/Booster_pre_2.png",
            BoosterType.booster3 => "Assets/Texture/UI/Booster/IconPreviewBooster/Booster_pre_3.png",
            BoosterType.booster4 => "Assets/Texture/UI/Booster/IconPreviewBooster/Booster_pre_4.png",
            _ => (int)boosterType > 0
                ? $"Assets/Texture/UI/Booster/IconPreviewBooster/Booster_pre_{(int)boosterType}.png"
                : null
        };
    }

    public static string GetButtonIconPath(BoosterType boosterType)
    {
        return boosterType switch
        {
            BoosterType.booster1 => "Assets/Texture/UI/Booster/IconBooster/Icon_Booster 1.Png",
            BoosterType.booster2 => "Assets/Texture/UI/Booster/IconBooster/Icon_Booster 2.Png",
            BoosterType.booster3 => "Assets/Texture/UI/Booster/IconBooster/Icon_Booster 3.Png",
            BoosterType.booster4 => "Assets/Texture/UI/Booster/IconBooster/Icon_Booster 4.Png",
            _ => (int)boosterType > 0
                ? $"Assets/Texture/UI/Booster/IconBooster/Icon_Booster {(int)boosterType}.Png"
                : null
        };
    }

    private static Sprite LoadSpriteAtPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets != null)
        {
            foreach (var a in assets)
            {
                if (a is Sprite s)
                    return s;
            }
        }
        return null;
    }

    private void OnValidate()
    {
        AutoLinkSprites(false);
    }
#endif
}

[Serializable]
public class ListBooster
{
    [SpreadsheetPage("ListBooster")]
    public List<ProcessBoosterData> listBoooster = new List<ProcessBoosterData>();
}

[Serializable]
public struct ProcessBoosterData
{
    public int level;
    public BoosterType boosterType;
    [Header("icon Preview Booster")]
    public Sprite imgMechanic;
    [Header("icon Button Booster")]
    public Sprite imgIconBooster;
    public string description;
    public string nameBooster;
    public int price;
}

