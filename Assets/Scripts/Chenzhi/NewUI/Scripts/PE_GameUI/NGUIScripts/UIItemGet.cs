using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ItemAsset;
using ItemAsset.PackageHelper;
//backpack
public class UIItemGet : UIBaseWnd
{
    public UIGrid mGrid;

    public ItemGetItem_N mPrefab;

    public UIScrollBar mSlider;

    List<ItemGetItem_N> mItemList = new List<ItemGetItem_N>();
    IItemDrop mItemDrop = null;
    Vector3 mPostionShow = Vector3.zero;

    public override void Show()
    {
        mSlider.scrollValue = 0;
        mPostionShow = GameUI.Instance.mMainPlayer.position;
        base.Show();
    }

    protected override void OnHide()
    {
        mItemDrop = null;
        Clear();
        base.OnHide();
    }


    void Clear()
    {
        for (int i = 0; i < mItemList.Count; i++)
        {
            GameObject.Destroy(mItemList[i].gameObject);
            mItemList[i].gameObject.transform.parent = null;
        }
        mItemList.Clear();
    }

    public void SetItemDrop(IItemDrop itemDrop)
    {
        mItemDrop = itemDrop;
        Reflash();
    }

    public void Reflash()
    {
        if (mItemDrop == null)
            return;

        //Clear();

        int i = 0;
        for (; i < mItemDrop.GetCount(); i++)
        {
            if (mItemList.Count <= i)
            {
                AddItem(mItemDrop.Get(i));
                mGrid.repositionNow = true;
            }
            else
            {
                SetItem(i, mItemDrop.Get(i));
            }
        }

        for (int j = i; j < mItemList.Count; )
            RemoveItem(mItemList[j]);

        if (mItemList.Count == 0)
            Hide();
    }

    void AddItem(ItemSample itemGrid)
    {
        if (null == itemGrid)
            return;
        ItemGetItem_N item = Instantiate(mPrefab) as ItemGetItem_N;
        item.transform.parent = mGrid.transform;
        item.transform.localPosition = new Vector3(0, 0, -1);
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
        item.SetItem(itemGrid, mItemList.Count);
        item.e_GetItem += GetItem;
        mItemList.Add(item);
    }

    void SetItem(int index, ItemSample item)
    {
        if (null == item || index <= -1 || index >= mItemList.Count)
            return;

        mItemList[index].SetItem(item, index);
        mItemList[index].e_GetItem -= GetItem;
        mItemList[index].e_GetItem += GetItem;
    }

    void RemoveItem(ItemGetItem_N item)
    {
        GameObject.Destroy(item.gameObject);
        item.gameObject.transform.parent = null;
        mItemList.Remove(item);
    }

    void Update()
    {
        float dis = Vector3.Distance(mPostionShow, GameUI.Instance.mMainPlayer.position);
        if (dis > 10f || mItemDrop == null || mItemDrop.Equals(null))
        {
            Hide();
        }
    }

    void BtnGetAll_OnClick()
    {
		if (!(mItemDrop == null || mItemDrop.Equals (null))) {
			mItemDrop.FetchAll ();
		}
        Hide();
    }

    void GetItem(ItemGetItem_N item)
    {
        if (mItemList.Contains(item))
        {
            int index = mItemList.FindIndex(itr => itr == item);
            if (index != -1)
            {
				if (!(mItemDrop == null || mItemDrop.Equals (null))) {
                	mItemDrop.Fetch(index);
	                if (!GameConfig.IsMultiClient)
	                    Reflash();
				}
            }

            if (mItemList.Count == 0)
                Hide();
        }
    }
}
