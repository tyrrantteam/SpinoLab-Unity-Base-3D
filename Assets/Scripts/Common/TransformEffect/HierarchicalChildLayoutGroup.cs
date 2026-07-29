using UnityEngine;

/// <summary>
/// Recursively arranges descendants: each tree depth alternates layout axis (Horizontal ↔ Vertical;
/// Depth starts with Horizontal on the first nested level, then alternates). Root uses the inspector <see cref="axis"/>.
/// </summary>
[ExecuteAlways]
public class HierarchicalChildLayoutGroup : MonoBehaviour
{
    [Header("Layout")]
    public ChildLayoutAxis axis = ChildLayoutAxis.Horizontal;
    public ChildLayoutAlignment alignment = ChildLayoutAlignment.Center;
    public ChildLayoutAlignment childAlignment = ChildLayoutAlignment.Center;
 
    [Space]
    [Tooltip("Gap between neighboring children along the layout axis at each hierarchy level (local units).")]
    public float spacing = 1f;

    [Tooltip("Extra space before the first child and after the last child on the layout axis at each level.")]
    public float padding = 0f;

    [Header("Options")]
    [Tooltip("Read each child's Renderer or Collider bounds to determine its size. " +
             "When off, treats every child as zero-size and uses spacing only.")]
    public bool useBounds = true;

    [Tooltip("-1 = no limit; 0 = only this object's direct children; N includes N levels below root.")]
    public int maxDepth = -1;

    [Tooltip("Automatically re-apply layout whenever a field changes in the Inspector (Edit Mode only).")]
    public bool autoApply = false;

    public void ApplyLayout()
    {
        ApplyRecursive(transform, 0);
    }

    void ApplyRecursive(Transform node, int depth)
    {
        bool unlimited = maxDepth < 0;
        if (unlimited || depth <= maxDepth)
        {
            ChildLayoutAxis axisAtDepth = ChildLayoutUtility.GetAxisForDepth(axis, depth);
            ChildLayoutUtility.ApplySingleLevel(
                node,
                axisAtDepth,
                alignment,
                childAlignment,
                spacing,
                padding,
                useBounds);
        }

        if (!unlimited && depth >= maxDepth)
            return;

        foreach (Transform child in ChildLayoutUtility.GetActiveChildren(node))
            ApplyRecursive(child, depth + 1);
    }

    /// <summary>Sets <see cref="Transform.localPosition"/> to zero on every descendant (not on this transform).</summary>
    public void ResetPositions()
    {
        var descendants = GetComponentsInChildren<Transform>(true);
        foreach (Transform t in descendants)
        {
            if (t == transform)
                continue;
            t.localPosition = Vector3.zero;
        }
    }

    void OnValidate()
    {
        if (autoApply)
            ApplyLayout();
    }
}
