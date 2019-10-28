using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using CustomData;
using ItemAsset;

public class AccountItems
{
	public static AccountItems self = new AccountItems();
	public string account;
	public float balance;
	Dictionary <int,int> itemList = new Dictionary<int, int>();

	public void ImportData( byte[] data)
	{
		itemList.Clear ();
		if (data.Length == 0)
			return;
		using (MemoryStream ms = new MemoryStream(data))
			using (BinaryReader read = new BinaryReader(ms))
		{
			//read learnt skills
			int count = BufferHelper.ReadInt32(read);
			
			for (int i = 0; i < count; i++)
			{
				itemList[BufferHelper.ReadInt32(read)] = BufferHelper.ReadInt32(read);
			}
		}
	}

	public byte[] ExportData( )
	{
		using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(ms))
		{
			BufferHelper.Serialize(writer, itemList.Count);
			foreach (var iter in itemList)
			{
				BufferHelper.Serialize(writer, iter.Key);
				BufferHelper.Serialize(writer, iter.Value);
			}
			return ms.ToArray();
		}
		//return null;
	}

	public void AddItems(int itemType,int amount)
	{
		if(itemList.ContainsKey(itemType))
		{
			itemList[itemType] += amount;
		}
		else
		{
			itemList[itemType] = amount;
		}
	}
	public bool DeleteItems(int itemType,int amount)
	{
		if(itemList.ContainsKey(itemType))
		{
			if(itemList[itemType] > amount)
				itemList[itemType] -= amount;
			else if(itemList[itemType] == amount)
				itemList.Remove(itemType);
			else
				return false;
			return true;
		}
		return false;
	}
	public bool CheckCreateItems(int itemType,int amount)
	{
		if(itemList.ContainsKey(itemType))
		{
			if(itemList[itemType] >= amount)
				return true;
			else
				return false;
		}
		return false;
	}
	public Dictionary <int,int> MyShopItems
	{
		get
		{
			return itemList;
		}
	}
}



