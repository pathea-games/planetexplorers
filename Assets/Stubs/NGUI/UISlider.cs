using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: slider control extending UIProgressBar with step and direction support.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UISlider : UIProgressBar
{
    public enum Direction
    {
        Horizontal,
        Vertical,
        Upgraded,
    }

    public new static UISlider current;

    public Direction direction = Direction.Horizontal;
    public UnityEngine.Vector2 fullSize { get; set; }
}
