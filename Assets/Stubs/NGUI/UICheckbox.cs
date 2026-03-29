using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: legacy checkbox component (replaced by UIToggle in newer NGUI versions).
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UICheckbox : MonoBehaviour
{
    public static UICheckbox current;

    public UISprite checkSprite;
    public Animation checkAnimation;
    public bool startsChecked;
    public int optionGroup;

    // onStateChange: game code uses += with method groups/lambdas taking bool (isChecked).
    public System.Action<bool> onStateChange;

    public bool isChecked { get; set; }
    public bool IsUseSelfOnClick { get; set; }
    // radioButtonRoot: game code assigns Transform (not GameObject) to this field.
    public Transform radioButtonRoot { get; set; }

    public bool value
    {
        get { return isChecked; }
        set { isChecked = value; }
    }
}
