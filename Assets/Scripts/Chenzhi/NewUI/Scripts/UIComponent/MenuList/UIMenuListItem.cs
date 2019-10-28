using UnityEngine;
using System;
using PeUIEffect;

public class UIMenuListItem : MonoBehaviour
{


    public string Text
    {
        get
        {
            return LbText.text;
        }
        set
        {
            LbText.text = value;
            gameObject.name = value;
            LbText.MakePixelPerfect();
        }
    }

    public int KeyId;
    public UIOption.KeyCategory mCategory;
    public UIGameMenuCtrl.MenuItemFlag mMenuItemFlag;
    public void SetHotKeyContent(string str)
    {
        string oldStr = LbText.text;
        if (str == "Escape")
        {
            LbText.text = oldStr.Split(new Char[] { '[' })[0] + "[4169e1]" + "[" + "Esc" + "]" + "[-]";
        }
        else
        {
            LbText.text = oldStr.Split(new Char[] { '[' })[0] + "[4169e1]" + "[" + str + "]" + "[-]";
        }
    }

    public bool IsHaveChild
    {
        get
        {
            return SpHaveChild.enabled;
        }
        set
        {
            SpHaveChild.enabled = value;
        }
    }

    public BoxCollider Box_Collider
    {
        get
        {
            return GetComponent<BoxCollider>();
        }
    }

    public string icoName
    {
        set
        {
            if (mIcoSpr == null)
                return;
            if (value.Trim().Length > 0)
            {
                mIcoSpr.spriteName = value;
                string[] str = value.Split('_');
                if (str.Length >= 2)
                    defoutIcoStr = str[0] + "_" + str[1] + "_";
                else
                    defoutIcoStr = "";
            }
            else
            {
                mIcoSpr.spriteName = "null";
            }
        }
    }

    //public Vector2 
    public UIMenuListItem Parent;
    public UILabel LbText;
    public UISlicedSprite ItemSelectedBg = null;
    public UISlicedSprite SpHaveChild = null;
    public UISprite mIcoSpr;
    [HideInInspector]
    public int Index = -1;


    // events
    public delegate void BaseMsgEvent(object sender);
    public event BaseMsgEvent e_OnMouseMoveIn = null;
    public event BaseMsgEvent e_OnMouseMoveOut = null;
    public event BaseMsgEvent e_OnClick = null;

    void OnMouseMoveIn()
    {
        if (e_OnMouseMoveIn != null)
            e_OnMouseMoveIn(this);
    }

    void OnMouseMoveOut()
    {
        if (e_OnMouseMoveOut != null)
            e_OnMouseMoveOut(this);
    }

    void OnClickItem()
    {
        if (e_OnClick != null)
            e_OnClick(this);
    }

    #region effect
    MenuParticleEffect mEffect;
    void Start()
    {
        GameObject obj = GameObject.Instantiate(Resources.Load("Prefab/UIEffect/MenuParticleEffect")) as GameObject;
        obj.transform.parent = transform;
        obj.transform.localPosition = new Vector3(0, 0, -5);
        obj.transform.localScale = Vector3.one;
        mEffect = obj.GetComponent<MenuParticleEffect>();
    }

    string defoutIcoStr;
    float time;
    int Speed = 25;
    void Update()
    {
        if (mEffect != null && ItemSelectedBg != null)
        {
            mEffect.gameObject.SetActive(ItemSelectedBg.enabled);
        }

        if (ItemSelectedBg != null && defoutIcoStr.Length > 0)
        {
            if (ItemSelectedBg.enabled)
            {
                if (time < Speed)
                    mIcoSpr.spriteName = defoutIcoStr + "1";
                else if (time < 2 * Speed)
                    mIcoSpr.spriteName = defoutIcoStr + "2";
                else if (time < 3 * Speed)
                    mIcoSpr.spriteName = defoutIcoStr + "3";
                else if (time < 4 * Speed)
                    mIcoSpr.spriteName = defoutIcoStr + "4";
                else
                    time = 0;

                time += Time.deltaTime * 100;
            }
            else
            {
                mIcoSpr.spriteName = defoutIcoStr + "1";
                time = 0;
            }
        }

    }
    #endregion
}
