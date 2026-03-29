using UnityEngine;

/// <summary>
/// NGUI stub: legacy draggable panel component (renamed to UIScrollView in newer NGUI).
/// Used extensively across 85+ files for scroll containers.
/// Will be replaced with functional uGUI ScrollRect in Phase 8.
/// </summary>
// UIDragPanelContents: legacy NGUI class (renamed/replaced by UIDraggablePanel internals).
public class UIDragPanelContents : MonoBehaviour
{
    public UIDraggablePanel panel => null;
}

public class UIDraggablePanel : MonoBehaviour
{
    public delegate void OnDragFinished();

    public UIPanel m_CurPanel;
    public Vector3 scale = Vector3.one;
    public OnDragFinished onDragFinished;

    public void SetDragAmount(float x, float y, bool updateScrollbars) { }

    public void ResetPosition() { }

    public void MoveRelative(Vector3 relative) { }

    public void UpdateScrollbars(bool recalculateBounds) { }

    public void DisableSpring() { }

    public void RestrictWithinBounds(bool instant) { }

    public UnityEngine.Bounds bounds
    {
        get { return default; }
    }
}
