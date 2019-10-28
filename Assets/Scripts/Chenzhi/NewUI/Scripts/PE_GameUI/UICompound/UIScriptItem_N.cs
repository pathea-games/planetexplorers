using UnityEngine;
using System.Collections;
using System;

public class UIScriptItem_N : MonoBehaviour {

    [SerializeField]
    private UISprite m_BgSprite;
    [SerializeField]
    private UILabel m_NameLabel;

    public int ItemID { get; private set; }
    public int ScriptIndex{get;private set;}

    public Action<UIScriptItem_N> SelectEvent;

    #region mono methods
    void Awake()
    {
        this.ScriptIndex = -1;
        this.ItemID = -1;
        UIEventListener.Get(this.gameObject).onClick += (go) =>
        {
            this.SelectItem();
        };
    }
    #endregion

    #region public methods

    public void UpdateInfo(int itemID,int scriptIndex)
    {
        this.ItemID = itemID;
        this.ScriptIndex = scriptIndex;
        this.UpdateName(scriptIndex);
    }

    public void SelectItem(bool execEvent=true)
    {
        this.UpdateBg("scriptItemBgSelect");
        if (execEvent&&null != this.SelectEvent && this.ScriptIndex != -1)
        {
            this.SelectEvent(this);
        }
    }

    public void CanSelectItem()
    {
        this.UpdateBg("scriptItemBg");
    }

    public void Reset()
    {
        this.ItemID = -1;
        this.ScriptIndex = -1;
        this.CanSelectItem();
        this.SelectEvent = null;
    }

    #endregion

    #region private methods

    private void UpdateBg(string spriteName)
    {
        this.m_BgSprite.spriteName = spriteName;
        this.m_BgSprite.MakePixelPerfect();
    }

    private void UpdateName(int index)
    {
        this.m_NameLabel.text = (index + 1).ToString();
    }
    #endregion
}
