using UnityEngine;
using System;
using System.Collections.Generic;

// menuListPanel

public class UIMenuPanel : MonoBehaviour
{
    public UIMenuListItem parent;
    public UISlicedSprite spBg = null;
    public GameObject content = null;
    UIMenuList list = null;
    public BoxCollider mBoxCollider = null;
    [HideInInspector]
    public bool mouseMoveOn = false;
    
    public bool isShow
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    public void Init(UIMenuListItem _parent, UIMenuList _list)
    {
        list = _list;
        parent = _parent;

        content = new GameObject("content");
        content.transform.parent = gameObject.transform;
        content.transform.localPosition = Vector3.zero;
        content.transform.localScale = Vector3.one;

        GameObject bg = GameObject.Instantiate(list.SlicedSpriteBg.gameObject) as GameObject;
        bg.transform.parent = gameObject.transform;
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = Vector3.one;
        bg.SetActive(true);

        spBg = bg.GetComponent<UISlicedSprite>();
        mBoxCollider = bg.GetComponent<BoxCollider>();

        //spBg.

        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (list == null)
            return;

        List<UIMenuListItem> childItems = list.GetChildItems(parent);

        int count = childItems.Count;
        for (int i = 0; i < childItems.Count; i++)
        {
            if (childItems[i] != null)
            {
                GameObject obj = childItems[i].gameObject;
                obj.transform.localPosition = new Vector3(0f, -i * list.ItemSize.y, 0f);
                childItems[i].Box_Collider.center = new Vector3(list.ItemSize.x / 2-29, 0, 0);
                childItems[i].Box_Collider.size = new Vector3(list.ItemSize.x, list.ItemSize.y, -2);
                obj.SetActive(true);
            }
        }

        //int bgScale_x = Convert.ToInt32( list.Margin.x + list.ItemSize.x + list.Margin.z);
        int bgScale_x = Convert.ToInt32(list.ItemSize.x);
        int bgScale_y = Convert.ToInt32(list.Margin.y + list.ItemSize.y * count + list.Margin.w);
        spBg.transform.localScale = new Vector3(bgScale_x, bgScale_y, 1);

        int content_x = Convert.ToInt32(list.Margin.x);
        int content_y = Convert.ToInt32(-list.Margin.y + (list.ItemSize.y * count) / 2);
        content.transform.localPosition = new Vector3(content_x-4, content_y, 0);

        // update panel position
        if (parent != null)
        {
            Vector3 parentPos = parent.gameObject.transform.localPosition;
            parentPos += parent.gameObject.transform.parent.localPosition;
            parentPos += parent.gameObject.transform.parent.parent.localPosition;

            int panel_x = Convert.ToInt32(parentPos.x + list.PanelMargin.x);
            int panel_y = Convert.ToInt32(parentPos.y + list.PanelMargin.y);
            this.gameObject.transform.localPosition = new Vector3(panel_x-5, panel_y, 0);

            spBg.pivot = UIWidget.Pivot.Left;
            mBoxCollider.center = new Vector3(0.5f, 0f, 0);
        }
        else
        {
            spBg.pivot = UIWidget.Pivot.Bottom;
            mBoxCollider.center = new Vector3(0f, 0.5f, 0);
        }
    }

    void Update()
    {
        if (parent != null && parent.IsHaveChild && gameObject.activeSelf)
        {
            parent.ItemSelectedBg.enabled = true;
        }


        if (mBoxCollider != null)
        {
            if (UICamera.mainCamera == null)
                return;
            Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            mouseMoveOn = mBoxCollider.Raycast(ray, out rayHit, 300);
        }
    }



    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (parent != null)
            parent.ItemSelectedBg.enabled = false;
        gameObject.SetActive(false);

    }

    void OnDisable()
    {
        mouseMoveOn = false;
    }
}