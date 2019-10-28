using UnityEngine;
using System;
using System.Collections.Generic;
using Pathea;


public class UIMenuList : UIComponent
{
    //Components

    static UIMenuList minstance = null;
    public static UIMenuList Instance { get { return minstance; } }

    public GameObject UIMeunItemPrefab = null;
    public UISlicedSprite SlicedSpriteBg = null;
    public GameObject ItemsContent = null;
    public Vector4 Margin = new Vector4(12, 15, 5, 15);  // left top right buttom				

    public Vector2 ItemSize = new Vector2(188, 50);
    public List<UIMenuListItem> Items = new List<UIMenuListItem>();
    [HideInInspector]
    public bool IsShow = true;
    [HideInInspector]

    private const int MouseMoveInAudioID = 2494;
    public bool mouseMoveOn
    {
        get
        {
            bool ok = false;
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].mouseMoveOn)
                {
                    ok = true;
                    break;
                }
            }
            return ok;
        }
    }

    public Vector2 PanelMargin = new Vector2(200, 0); // left top
    public List<UIMenuPanel> panels = new List<UIMenuPanel>();
    public UIMenuPanel rootPanel = null;

    public UIMenuListItem mMouseMoveInItem = null;

    // events
    public UIConpomentEvent e_ItemOnClick = new UIConpomentEvent();
    public UIConpomentEvent e_ItemOnMouseMoveIn = new UIConpomentEvent();
    public UIConpomentEvent e_ItemOnMouseMoveOut = new UIConpomentEvent();

    void Awake()
    {
        InitializeComponent();
        minstance = this;
    }

    public void InitializeComponent()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].e_OnClick += ItemOnClick;
            Items[i].e_OnMouseMoveIn += ItemOnMouseMoveIn;
            Items[i].e_OnMouseMoveOut += ItemOnMouseMoveOut;
        }

        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].gameObject.name == "root")
                rootPanel = panels[i];
            panels[i].Hide();
        }
        if (rootPanel != null)
            rootPanel.Show();
    }

    public void Hide()
    {
        Hide_ChildPanel(null);
        rootPanel.Hide();
        IsShow = false;
    }

    public void Show()
    {
        rootPanel.Show();
        IsShow = true;
    }

    public UIMenuListItem AddItem(int parentIndex, string text,UIGameMenuCtrl.MenuItemFlag flag,string icoName = "")
    {
        return AddItem(Items[parentIndex], text,flag, icoName);
    }

    public UIMenuListItem AddItem(UIMenuListItem parent, string text,UIGameMenuCtrl.MenuItemFlag flag, string icoName = "")
    {
        GameObject obj = AddObj();

        UIMenuPanel panel = FindMenuPanel(parent);
        if (panel == null)
            panel = CreatePanel(parent, this);

        UIMenuListItem item = obj.GetComponent<UIMenuListItem>();
        item.Text = text;
        item.Parent = parent;
        item.icoName = icoName;
        item.mMenuItemFlag = flag;
        item.e_OnClick += ItemOnClick;
        item.e_OnMouseMoveIn += ItemOnMouseMoveIn;
        item.e_OnMouseMoveOut += ItemOnMouseMoveOut;

        obj.transform.parent = panel.content.transform;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        Items.Insert(Items.Count, item);
        UpdateIndex();
        panel.UpdatePosition();

        if (parent != null)
            parent.IsHaveChild = true;

        return item;
    }



    public UIMenuPanel FindMenuPanel(UIMenuListItem parent)
    {
        UIMenuPanel panel = panels.Find(
            delegate(UIMenuPanel mp)
            {
                return mp.parent == parent;

            });
        return panel;
    }



    public bool DeleteItem(int index)
    {
        if (index >= Items.Count || index < 0)
            return false;

        return DeleteItem(Items[index]);
    }


    public bool DeleteItem(UIMenuListItem item)
    {
        DeleteChildItem(item);

        Debug.Log(item.gameObject.name);
        if (item.gameObject != null)
        {
            item.gameObject.transform.parent = null;
            if (Application.isEditor)
                GameObject.DestroyImmediate(item.gameObject);
            else
                GameObject.Destroy(item.gameObject);
        }
        Items.Remove(item);

        UpdateIndex();

        UIMenuPanel panel = FindMenuPanel(item.Parent);
        if (panel != null)
            panel.UpdatePosition();

        int count = GetChildItems(item.Parent).Count;
        if (count == 0)
        {
            DeletePanel(item.Parent);
            if (item.Parent != null)
                item.Parent.IsHaveChild = false;
        }

        return true;
    }




    public void UpdatePanelPositon()
    {
        foreach (UIMenuPanel panel in panels)
        {
            panel.UpdatePosition();
        }
    }



    public List<UIMenuListItem> GetChildItems(UIMenuListItem parent)
    {
        List<UIMenuListItem> childItems = new List<UIMenuListItem>();
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Parent == parent)
                childItems.Add(Items[i]);
        }
        return childItems;
    }

    private GameObject AddObj()
    {
        if (UIMeunItemPrefab == null)
        {
            Debug.LogError("Error: 'UIMenuList.UIMeunItemPrefab = null' !");
            return null;
        }

        if (ItemsContent == null)
        {
            Debug.LogError("Error: 'UIMenuList.SlicedSpriteBg = null' !");
            return null;
        }

        GameObject obj = GameObject.Instantiate(UIMeunItemPrefab) as GameObject;
        obj.SetActive(true);
        return obj;
    }

    private void UpdateIndex()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] != null)
                Items[i].Index = i;
        }
    }

    private void DeleteChildItem(UIMenuListItem parent)
    {
        List<UIMenuListItem> childList = GetChildItems(parent);
        foreach (UIMenuListItem item in childList)
            DeleteItem(item);

        DeletePanel(parent);
    }

    private void DeletePanel(UIMenuListItem parent)
    {
        UIMenuPanel panel = FindMenuPanel(parent);

        if (panel == null)
            return;

        if (panel.gameObject != null)
        {
            panel.gameObject.transform.parent = null;
            if (Application.isEditor)
                GameObject.DestroyImmediate(panel.gameObject);
            else
                GameObject.Destroy(panel.gameObject);
        }
        panels.Remove(panel);
    }

    private UIMenuPanel CreatePanel(UIMenuListItem _parent, UIMenuList _list)
    {
        string name = (_parent == null) ? "root" : "panel_" + _parent.Text;
        GameObject obj = new GameObject(name);
        obj.transform.parent = _list.ItemsContent.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        UIMenuPanel panel = obj.AddComponent<UIMenuPanel>();
        panel.Init(_parent, _list);
        panels.Add(panel);
        if (panel.parent == null)
            rootPanel = panel;
        return panel;
    }


    private void Hide_BotherPanels(UIMenuListItem item)
    {
        List<UIMenuListItem> childList = GetChildItems(item.Parent);

        for (int i = 0; i < childList.Count; i++)
        {
            if (childList[i] != item)
            {
                UIMenuPanel panel_bother = FindMenuPanel(childList[i]);
                if (panel_bother != null)
                {
                    if (panel_bother.isShow)
                    {
                        Hide_ChildPanel(childList[i]);
                        panel_bother.Hide();
                    }
                }
            }
        }
    }

    private void Hide_ChildPanel(UIMenuListItem item)
    {
        List<UIMenuListItem> childList = GetChildItems(item);
        for (int i = 0; i < childList.Count; i++)
        {
            if (childList[i].IsHaveChild)
            {
                UIMenuPanel panel_child = FindMenuPanel(childList[i]);
                if (panel_child != null)
                {
                    if (panel_child.isShow)
                    {
                        if (childList[i].IsHaveChild)
                            Hide_ChildPanel(childList[i]);
                        panel_child.Hide();
                    }
                }
            }
        }
    }


    // event
    void ItemOnMouseMoveIn(object sender)
    {
        UIMenuListItem item = sender as UIMenuListItem;
        if (item != null)
        {

            //lz-2016.05.30 Diplomacy菜单是在游戏里面过了344剧情才开的，所有这里根据条件动态的添加和移除
            if (item.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_Phone)
            {
                UIMenuListItem diplomacyItem = Items.Find(a => a.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_Diplomacy);
                //lz-2016.10.10 错误 #4221 历险模式中右下角菜单中没有外交选项
                if (PeGameMgr.IsAdventure||ReputationSystem.Instance.GetActiveState((int)PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID)))
                {
                    if(null==diplomacyItem)
                        this.AddItem(item, NewUIText.mMenuDiplomacy.GetString(), UIGameMenuCtrl.MenuItemFlag.Flag_Diplomacy, "listico_22_1");
                }
                else
                {
                    if (diplomacyItem != null)
                        this.DeleteItem(diplomacyItem);
                }

                //lz-2016.07.25 检测怪物图鉴
                UIMenuListItem speciesWikiItem = Items.Find(a => a.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_SpeciesWiki);

                //lz-2016.10.18 Adventure默认开启，Story检测开启
                if (PeGameMgr.IsAdventure||(PeGameMgr.IsStory&&null != StroyManager.Instance&&StroyManager.Instance.enableBook))
                {
                    if (null == speciesWikiItem)
                        this.AddItem(item, NewUIText.mMenuSpeciesWiki.GetString(), UIGameMenuCtrl.MenuItemFlag.Flag_SpeciesWiki, "listico_24_1");
                }
                else
                {
                    if (speciesWikiItem != null)
                        this.DeleteItem(speciesWikiItem);
                }

                //lz-2016.07.25 检测开启播放器
                UIMenuListItem radioItem = Items.Find(a => a.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_Radio);
                if (null != GameUI.Instance)
                {
                    if (!GameUI.Instance.mPhoneWnd.InitRadio) GameUI.Instance.mPhoneWnd.InitRadioData();
                    if (GameUI.Instance.mPhoneWnd.CheckOpenRadio())
                    {
                        if (null == radioItem)
                            this.AddItem(item, NewUIText.mMenuRadio.GetString(), UIGameMenuCtrl.MenuItemFlag.Flag_Radio, "listico_25_1");
                    }
                    else
                    {
                        if (radioItem != null)
                            this.DeleteItem(radioItem);
                    }
                }
            }

            item.ItemSelectedBg.enabled = true;
            //lz-2016.09.12 播放进入菜单音效
            AudioManager.instance.Create(Vector3.zero, MouseMoveInAudioID);

            if (item != mMouseMoveInItem)
            {
                Hide_BotherPanels(item);
                if (item.IsHaveChild)
                {
                    UIMenuPanel panel = FindMenuPanel(item);
                    if (panel != null)
                        panel.Show();
                }
            }
        }
        mMouseMoveInItem = item;
        e_ItemOnMouseMoveIn.Send(eventReceiver, sender);
    }

    void ItemOnMouseMoveOut(object sender)
    {
        UIMenuListItem item = sender as UIMenuListItem;
        if (item != null)
        {
            bool ok = true;
            UIMenuPanel panel = FindMenuPanel(item);
            if (panel != null)
                if (panel.isShow)
                    ok = false;
            if (ok)
                item.ItemSelectedBg.enabled = false;
        }
        e_ItemOnMouseMoveOut.Send(eventReceiver, sender);
    }

    void ItemOnClick(object sender)
    {
        e_ItemOnClick.Send(eventReceiver, sender);
    }

    public void RefreshHotKeyName()
    {
        if (Items.Count == 0)
            return;
        string str;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].KeyId != -1)
            {
                switch (Items[i].mCategory)
                {
                    case UIOption.KeyCategory.Common:
                        str = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[0][Items[i].KeyId])._key.ToStr();
                        Items[i].SetHotKeyContent(str);
                        break;
                    case UIOption.KeyCategory.Character:
                        str = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[1][Items[i].KeyId])._key.ToStr();
                        Items[i].SetHotKeyContent(str);
                        break;
                    case UIOption.KeyCategory.Construct:
                        str = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[2][Items[i].KeyId])._key.ToStr();
                        Items[i].SetHotKeyContent(str);
                        break;
                    case UIOption.KeyCategory.Carrier:
                        str = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[3][Items[i].KeyId])._key.ToStr();
                        Items[i].SetHotKeyContent(str);
                        break;
                }
            }
        }
    }

}













