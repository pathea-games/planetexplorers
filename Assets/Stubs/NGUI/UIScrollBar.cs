using UnityEngine;

/// <summary>
/// NGUI stub: scroll bar control extending UISlider with bar size.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UIScrollBar : UISlider
{
    public float barSize { get; set; }
    public float scrollValue { get; set; }

    public override float alpha { get; set; }

    // onChange on UIScrollBar takes UIScrollBar (not float). Hides base UIProgressBar.onChange.
    public new System.Action<UIScrollBar> onChange;
}
