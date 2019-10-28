using UnityEngine;
using System.Collections;

public class CSUI_BuffItem : MonoBehaviour
{
    [HideInInspector]
    public string _describe = null;
    [HideInInspector]
    public string _icon = null;

    public UISprite _iconSpr;

    public void SetInfo(string _icon, string _describe)
    {
        this._icon = _icon;
        this._describe = _describe;
        if (_iconSpr != null)
        {
            _iconSpr.spriteName = _icon;
            _iconSpr.MakePixelPerfect();
        }
    }

    void OnTooltip(bool active)
    {
        if (!active)
            UITooltip.ShowText(null);
        if (active && _describe != null)
            UITooltip.ShowText(_describe);
    }
}
