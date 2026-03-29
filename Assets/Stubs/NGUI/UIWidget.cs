using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: base class for visible UI elements (labels, sprites, textures).
/// Provides color, depth, pivot, dimensions, and alpha.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UIWidget : UIRect
{
    public enum Pivot
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }

    public enum AspectRatioSource
    {
        Free,
        BasedOnWidth,
        BasedOnHeight,
    }

    public Color color = Color.white;
    public Pivot pivot = Pivot.Center;
    public int depth;
    public bool autoResizeBoxCollider;

    public override float alpha { get; set; }
    public float bufferAlpha { get; set; }

    public new int width
    {
        get { return 0; }
        set { }
    }

    public new int height
    {
        get { return 0; }
        set { }
    }

    public bool isVisible { get { return true; } }

    public Vector2 relativeSize { get { return Vector2.one; } }

    public Vector3 pivotOffset { get { return Vector3.zero; } }

    public Vector4 drawRegion { get; set; }

    public Vector2 localSize { get { return Vector2.zero; } }

    public UIPanel panel { get { return null; } }

    public List<EventDelegate> onChange = new List<EventDelegate>();

    public Bounds CalculateBounds() { return default; }

    public Bounds CalculateBounds(Transform relativeParent) { return default; }

    public virtual void MakePixelPerfect() { }

    public void ResizeCollider() { }

    public void SetDimensions(int w, int h) { }

    public static int FullCompareFunc(UIWidget left, UIWidget right) { return 0; }

    public void MarkAsChanged() { }

    public void ParentHasChanged(bool notify) { }
}
