using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class ItemScript_ItemList : ItemScript
{
    [System.Serializable]
	public class Item : MaterialItem
    {
    }

    [SerializeField]
    Item[] m_items;

    public Item[] GetItems()
    {
        return m_items;
    }
}
