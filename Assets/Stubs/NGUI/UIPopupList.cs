using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: popup/dropdown list component.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UIPopupList : MonoBehaviour
{
    public enum Position
    {
        Auto,
        Above,
        Below,
    }

    public enum Selection
    {
        OnPress,
        OnClick,
    }

    public static UIPopupList current;

    public UIAtlas atlas;
    public UIFont font;
    public UIFont bitmapFont;
    public Font trueTypeFont;
    public int fontSize = 16;
    public string backgroundSprite;
    public string highlightSprite;
    public Color textColor = Color.white;
    public Color backgroundColor = Color.white;
    public Color highlightColor = new Color(0.88f, 0.78f, 0.55f, 1f);
    public Position position = Position.Auto;
    public Selection openOn = Selection.OnPress;
    public UILabel textLabel;
    public bool isAnimated = true;
    public bool isLocalized;
    public float animSpeed = 0.15f;

    public List<string> items = new List<string>();
    public List<object> itemData = new List<object>();

    // onChange/onSelectionChange: game code uses += or = with method groups/lambdas taking string.
    public System.Action<string> onChange;
    public System.Action<string> onSelectionChange;

    public string value { get; set; }

    /// <summary>Current selected item text. Alias for value.</summary>
    public string selection
    {
        get { return value; }
        set { this.value = value; }
    }

    public object data
    {
        get { return null; }
        set { }
    }

    public bool isOpen { get; set; }
    public UIPopupList ChildPopupMenu { get; set; }

    public void AddItem(string text) { items.Add(text); }

    public void AddItem(string text, object data) { items.Add(text); itemData.Add(data); }

    public void RemoveItem(string text) { items.Remove(text); }

    public void RemoveItemAt(int index) { if (index >= 0 && index < items.Count) items.RemoveAt(index); }

    public void Clear() { items.Clear(); itemData.Clear(); }

    public void Close() { isOpen = false; }
}
