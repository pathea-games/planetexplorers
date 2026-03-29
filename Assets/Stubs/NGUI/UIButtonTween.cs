// Stub: NGUI UIButtonTween
// Triggers a UITweener when button state changes.

using System.Collections.Generic;
using UnityEngine;

public class UIButtonTween : MonoBehaviour
{
    public GameObject tweenTarget;
    public int tweenGroup = 0;
    public bool includeChildren = false;
    public bool disableWhenFinished = false;

    public enum Trigger { OnHover, OnPress, OnClick, OnDoubleClick, OnSelect, OnActivate, OnHoverTrue, OnHoverFalse, OnPressTrue, OnPressFalse, OnSelectTrue, OnSelectFalse }
    public Trigger trigger = Trigger.OnClick;

    // onFinished: game code assigns void(UITweener) method groups directly.
    public System.Action<UITweener> onFinished;

    public void Play(bool forward) { }
}
