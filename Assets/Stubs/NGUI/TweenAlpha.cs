using UnityEngine;

/// <summary>
/// NGUI stub: tweens a UIWidget's alpha from one value to another.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class TweenAlpha : UITweener
{
    public float from = 1f;
    public float to = 1f;
    public float alpha { get; set; }

    public float value
    {
        get
        {
            var widget = GetComponent<UIWidget>();
            return widget != null ? widget.alpha : 1f;
        }
        set
        {
            var widget = GetComponent<UIWidget>();
            if (widget != null) widget.alpha = value;
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        value = Mathf.Lerp(from, to, factor);
    }

    public static TweenAlpha Begin(GameObject go, float duration, float alpha)
    {
        var tween = UITweener.Begin<TweenAlpha>(go, duration);
        tween.from = tween.value;
        tween.to = alpha;
        return tween;
    }

    public static TweenAlpha Begin(GameObject go, float duration, float from, float to)
    {
        var tween = UITweener.Begin<TweenAlpha>(go, duration);
        tween.from = from;
        tween.to = to;
        return tween;
    }
}
