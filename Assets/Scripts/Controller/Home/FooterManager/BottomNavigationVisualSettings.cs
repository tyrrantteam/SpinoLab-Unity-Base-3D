using System;
using UnityEngine;

/// <summary>
/// Per-tab visual tuning for selected and unselected bottom navigation states.
/// </summary>
[Serializable]
public struct BottomNavigationVisualSettings
{
    [Tooltip("Icon tint when this tab is selected.")]
    public Color selectedIconColor;

    [Tooltip("Icon tint when this tab is not selected.")]
    public Color unselectedIconColor;

    [Tooltip("Label color when this tab is selected.")]
    public Color selectedLabelColor;

    [Tooltip("Label color when this tab is not selected.")]
    public Color unselectedLabelColor;

    public static BottomNavigationVisualSettings Default => new()
    {
        selectedIconColor = Color.white,
        unselectedIconColor = new Color(1f, 1f, 1f, 0.55f),
        selectedLabelColor = Color.white,
        unselectedLabelColor = new Color(1f, 1f, 1f, 0.55f),
    };
}
