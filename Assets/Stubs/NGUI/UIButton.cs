using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: clickable button with color/sprite state transitions.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UIButton : MonoBehaviour
{
    public enum State
    {
        Normal,
        Hover,
        Pressed,
        Disabled,
    }

    public static UIButton current;

    public GameObject tweenTarget;
    public bool isEnabled = true;
    public Color defaultColor = Color.white;
    public Color hover = new Color(225f / 255f, 200f / 255f, 150f / 255f, 1f);
    public Color pressed = new Color(183f / 255f, 163f / 255f, 123f / 255f, 1f);
    public Color disabledColor = Color.grey;
    public string normalSprite = "";
    public string hoverSprite = "";
    public string pressedSprite = "";
    public string disabledSprite = "";
    public bool pixelSnap;
    public float duration = 0.2f;

    public List<EventDelegate> onClick = new List<EventDelegate>();

    public State state { get; set; }

    public UISprite tweenSprite { get { return null; } }

    public void SetState(State state, bool immediate) { }
    public void UpdateColor(bool immediate) { }
    // UpdateColor(isEnabled, immediate) overload used by BonePanelUI.cs
    public void UpdateColor(bool isEnabled, bool immediate) { }
}
