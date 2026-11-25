using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ShopLayout
{
    public static void PrepareSpawnedItem(RectTransform item)
    {
        if (item == null)
        {
            return;
        }

        item.anchorMin = new Vector2(0f, 1f);
        item.anchorMax = new Vector2(0f, 1f);
        item.pivot = new Vector2(0.5f, 0.5f);
        item.localScale = Vector3.one;
        item.localRotation = Quaternion.identity;
        item.anchoredPosition = Vector2.zero;
    }

    public static void RebuildSection(RectTransform section)
    {
        if (!RebuildImmediate(section))
        {
            return;
        }

        RebuildImmediate(section.parent as RectTransform);
    }

    public static void RebuildSections(IEnumerable<RectTransform> sections)
    {
        RectTransform content = null;
        foreach (var section in sections)
        {
            if (!RebuildImmediate(section))
            {
                continue;
            }

            if (content == null)
            {
                content = section.parent as RectTransform;
            }
        }

        RebuildImmediate(content);
    }

    private static bool RebuildImmediate(RectTransform rect)
    {
        if (rect == null || !rect.gameObject.activeInHierarchy)
        {
            return false;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        return true;
    }
}
