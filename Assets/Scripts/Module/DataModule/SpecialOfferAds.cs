using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecialOfferAds", menuName = "Data/SpecialOfferAds")]
public class SpecialOfferAds : ScriptableObject
{
    [Tooltip("Bật/tắt toàn bộ tính năng Special Offer Ads.")]
    public bool isActive;

    [Tooltip("Số level win sau khi unlock booster mới thì hiện popup.")]
    public int activeAfterLevel = 3;

    public List<SpecialOfferAdsEntry> entries = new List<SpecialOfferAdsEntry>();

    public bool TryGetEntry(BoosterType boosterType, out SpecialOfferAdsEntry entry)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].boosterType == boosterType)
            {
                entry = entries[i];
                return true;
            }
        }

        entry = default;
        return false;
    }
}

[Serializable]
public struct SpecialOfferAdsEntry
{
    public BoosterType boosterType;
    public int value;
    public string name;
    public string description;
}
