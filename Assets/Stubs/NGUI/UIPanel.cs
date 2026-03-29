using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: container that manages widget draw calls, clipping, and sorting.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UIPanel : UIRect
{
    public enum RenderQueue
    {
        Automatic,
        StartAt,
        Explicit,
    }

    public static List<UIPanel> list = new List<UIPanel>();

    public override float alpha { get; set; }
    public int depth;
    public UIDrawCall.Clipping clipping = UIDrawCall.Clipping.None;
    public Vector2 clipOffset = Vector2.zero;
    public Vector4 baseClipRegion = Vector4.zero;
    public bool widgetsAreStatic;
    public bool cullWhileDragging;
    public bool alwaysOnScreen;
    public RenderQueue renderQueue = RenderQueue.Automatic;
    public int startingRenderQueue = 3000;
    public bool useSortingOrder;
    public bool sortByDepth;
    public bool showInPanelTool;

    public UIPanel parentPanel { get { return null; } }

    public int sortingOrder { get; set; }

    public Vector2 clipSoftness { get; set; }

    public Vector4 clipRange
    {
        get { return baseClipRegion; }
        set { baseClipRegion = value; }
    }

    public static UIPanel Find(Transform trans) { return null; }

    public static UIPanel Find(Transform trans, bool createIfMissing) { return null; }

    public static UIPanel Find(Transform trans, bool createIfMissing, int layer) { return null; }

    public Bounds CalculateBounds() { return default; }

    public void SetDirty() { }

    public void Refresh() { }

    public void RebuildAllDrawCalls() { }

    public Vector3 CalculateConstrainOffset(Vector2 min, Vector2 max) { return Vector3.zero; }

    public bool ConstrainTargetToBounds(Transform target, bool immediate) { return false; }

    public bool ConstrainTargetToBounds(Transform target, ref Bounds targetBounds, bool immediate) { return false; }

    public void SetActive(bool state) { gameObject.SetActive(state); }

    public bool IsVisible(Vector3 worldPoint) { return true; }
    public bool IsVisible(UIWidget widget) { return true; }
}
