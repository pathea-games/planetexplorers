using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: layout component that arranges children in a table with variable sizes.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UITable : MonoBehaviour
{
    public enum Direction
    {
        Down,
        Up,
    }

    public enum Sorting
    {
        None,
        Alphabetic,
        Horizontal,
        Vertical,
        Custom,
    }

    public int columns;
    public Direction direction = Direction.Down;
    public Sorting sorting = Sorting.None;
    public UIWidget.Pivot pivot = UIWidget.Pivot.TopLeft;
    public bool hideInactive = true;
    public bool keepWithinPanel;
    public Vector2 padding = Vector2.zero;
    public bool repositionNow;
    public bool sorted;
    // mVariableHeight: used as float in arithmetic expressions (e.g. -mVariableHeight).
    public float mVariableHeight;

    public System.Comparison<Transform> onCustomSort;
    // onReposition: game code assigns void() methods — no-arg action.
    public System.Action onReposition;

    public List<Transform> children
    {
        get
        {
            var list = new List<Transform>();
            var t = transform;
            for (int i = 0; i < t.childCount; ++i)
                list.Add(t.GetChild(i));
            return list;
        }
    }

    public void Reposition() { }

    public List<Transform> GetChildList()
    {
        return children;
    }
}
