using UnityEngine;

/// <summary>
/// NGUI stub: legacy anchor component for positioning UI elements relative to screen/camera.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UIAnchor : MonoBehaviour
{
    public enum Side
    {
        BottomLeft,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        Center,
    }

    public Camera uiCamera;
    public GameObject container;
    public Side side = Side.Center;
    public bool runOnlyOnce = true;
    public Vector2 relativeOffset = Vector2.zero;
    public Vector2 pixelOffset = Vector2.zero;
    public float depthOffset { get; set; }
}
