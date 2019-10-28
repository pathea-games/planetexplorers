using UnityEngine;
using System.Collections;

//lz-2016.08.12 增加这个是因为快捷栏需要增加【当格子有东西的时候下方显示快捷键】的功能，以前用的是Grid_N，为了兼容就继承了Grid_N
public class QuickBarItem_N : Grid_N
{
    [SerializeField]
    private UISprite m_QuickBarSprite;
    private bool m_isKeyGrid;

    #region mono methods

    void Awake()
    {
        this.m_isKeyGrid = false;
        this.UpdateQuickBarKeyState();
    }

    #endregion

    #region override methods

    public override void SetItem(ItemAsset.ItemSample itemGrid, bool showNew = false)
    {
        base.SetItem(itemGrid, showNew);
        this.UpdateQuickBarKeyState();
    }

    #endregion

    #region public methods

    public void UpdateKeyInfo(int key)
    {
        if (key==-1)
        {
            base.mScriptIco.spriteName = "itemhand";
            base.mScriptIco.MakePixelPerfect();
            this.m_isKeyGrid = false;
        }
        else
        {
            base.mScriptIco.spriteName = "num_" + key.ToString();
            base.mScriptIco.MakePixelPerfect();
            this.m_QuickBarSprite.spriteName = "QuickKey_" + key.ToString();
            this.m_QuickBarSprite.MakePixelPerfect();
            this.m_isKeyGrid = true;
        }
    }

    #endregion

    #region private methods

    private void UpdateQuickBarKeyState()
    {
        this.m_QuickBarSprite.enabled=(base.Item != null&&this.m_isKeyGrid);
    }

    #endregion

}
