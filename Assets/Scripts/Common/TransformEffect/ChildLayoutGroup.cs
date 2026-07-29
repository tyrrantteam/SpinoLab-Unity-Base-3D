using System.Collections.Generic;
using UnityEngine;

public enum ChildLayoutAxis
{
    Horizontal,
    Vertical,
    Depth
}

public enum ChildLayoutAlignment
{
    Start,
    Center,
    End
}

/// <summary>Single-level layout along one main axis (shared by <see cref="ChildLayoutGroup"/> and <see cref="HierarchicalChildLayoutGroup"/>).</summary>
public static class ChildLayoutUtility
{
    public static ChildLayoutAxis OrthogonalAxis(ChildLayoutAxis a) => a switch
    {
        ChildLayoutAxis.Horizontal => ChildLayoutAxis.Vertical,
        ChildLayoutAxis.Vertical   => ChildLayoutAxis.Horizontal,
        ChildLayoutAxis.Depth      => ChildLayoutAxis.Horizontal,
        _                          => ChildLayoutAxis.Horizontal
    };

    /// <summary>Axis used at <paramref name="depth"/> below a root laid out with <paramref name="rootAxis"/> (depth 0 = root axis).</summary>
    public static ChildLayoutAxis GetAxisForDepth(ChildLayoutAxis rootAxis, int depth)
    {
        var a = rootAxis;
        for (int i = 0; i < depth; i++)
            a = OrthogonalAxis(a);
        return a;
    }

    public static void ApplySingleLevel(
        Transform parent,
        ChildLayoutAxis axis,
        ChildLayoutAlignment alignment,
        ChildLayoutAlignment childAlignment,
        float spacing,
        float mainAxisPadding,
        bool useBounds)
    {
        List<Transform> children = GetActiveChildren(parent);
        if (children.Count == 0) return;

        int mainAxis  = GetAxisIndex(axis);
        int crossAxis = GetCrossAxisIndex(axis);

        var mainSizes  = new float[children.Count];
        var crossSizes = new float[children.Count];

        for (int i = 0; i < children.Count; i++)
        {
            Vector3 size = useBounds ? GetLocalBoundsSize(parent, children[i]) : Vector3.zero;
            mainSizes[i]  = size[mainAxis];
            crossSizes[i] = size[crossAxis];
        }

        float innerMainSize = Sum(mainSizes) + spacing * Mathf.Max(0, children.Count - 1);
        float totalMainSize = innerMainSize + 2f * mainAxisPadding;

        float startOffset = alignment switch
        {
            ChildLayoutAlignment.Center => -totalMainSize * 0.5f,
            ChildLayoutAlignment.End    => -totalMainSize,
            _                           => 0f
        };

        float maxCrossHalf = 0f;
        for (int i = 0; i < children.Count; i++)
            maxCrossHalf = Mathf.Max(maxCrossHalf, crossSizes[i] * 0.5f);

        float crossOffset = childAlignment switch
        {
            ChildLayoutAlignment.End   => +maxCrossHalf,
            ChildLayoutAlignment.Start => -maxCrossHalf,
            _                          => 0f
        };

        float cursor = startOffset + mainAxisPadding;
        for (int i = 0; i < children.Count; i++)
        {
            Vector3 pos = children[i].localPosition;
            pos[mainAxis]  = cursor + mainSizes[i] * 0.5f;
            pos[crossAxis] = crossOffset;
            children[i].localPosition = pos;
            cursor += mainSizes[i] + spacing;
        }
    }

    public static List<Transform> GetActiveChildren(Transform parent)
    {
        var result = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.gameObject.activeSelf)
                result.Add(child);
        }
        return result;
    }

    static int GetAxisIndex(ChildLayoutAxis a) => a switch
    {
        ChildLayoutAxis.Horizontal => 0,
        ChildLayoutAxis.Vertical   => 1,
        ChildLayoutAxis.Depth      => 2,
        _                          => 0
    };

    static int GetCrossAxisIndex(ChildLayoutAxis a) => a switch
    {
        ChildLayoutAxis.Horizontal => 1,
        ChildLayoutAxis.Vertical   => 0,
        ChildLayoutAxis.Depth      => 0,
        _                          => 1
    };

    static Vector3 GetLocalBoundsSize(Transform parent, Transform child)
    {
        Renderer rend = child.GetComponentInChildren<Renderer>();
        if (rend != null)
            return parent.InverseTransformVector(rend.bounds.size);

        Collider col = child.GetComponentInChildren<Collider>();
        if (col != null)
            return parent.InverseTransformVector(col.bounds.size);

        return Vector3.zero;
    }

    static float Sum(float[] values)
    {
        float total = 0f;
        foreach (float v in values) total += v;
        return total;
    }
}

/// <summary>
/// Arranges direct children of this Transform in a horizontal, vertical, or depth line,
/// similar to Unity UI LayoutGroup but operating on world-space 3D Transforms.
/// </summary>
[ExecuteAlways]
public class ChildLayoutGroup : MonoBehaviour
{
    [Header("Layout")]
    public ChildLayoutAxis axis = ChildLayoutAxis.Horizontal;
    public ChildLayoutAlignment alignment = ChildLayoutAlignment.Center;
    public ChildLayoutAlignment childAlignment = ChildLayoutAlignment.Center;

    [Space]
    [Tooltip("Gap between neighboring children along the layout axis (local units).")]
    public float spacing = 1f;

    [Tooltip("Extra space before the first child and after the last child on the layout axis.")]
    public float padding = 0f;

    [Header("Options")]
    [Tooltip("Read each child's Renderer or Collider bounds to determine its size. " +
             "When off, treats every child as zero-size and uses spacing only.")]
    public bool useBounds = true;

    [Tooltip("Automatically re-apply layout whenever a field changes in the Inspector (Edit Mode only).")]
    public bool autoApply = false;

    public void ApplyLayout()
    {
        ChildLayoutUtility.ApplySingleLevel(transform, axis, alignment, childAlignment, spacing, padding, useBounds);
    }

    public void ResetPositions()
    {
        foreach (Transform child in ChildLayoutUtility.GetActiveChildren(transform))
            child.localPosition = Vector3.zero;
    }

    void OnValidate()
    {
        if (autoApply)
            ApplyLayout();
    }
}
