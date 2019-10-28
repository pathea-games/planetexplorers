using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ItemAsset;

namespace NetworkHelper
{
	public static class AccessorHelper
	{
		public static IEnumerable<ItemObject> GetItems(this ItemMgr mgr, int[] ids)
		{
			foreach (int id in ids)
			{
				ItemObject item = mgr.Get(id);
				if (null == item)
					yield return item;
			}
		}

		public static void ResetPackageItems(this ItemPackage package, int tab, int index, int id)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)tab);
			if (-1 == id)
			{
				slotList[index] = null;
			}
			else
			{
				ItemObject item = ItemMgr.Instance.Get(id);
				slotList[index] = item;
			}
		}

		public static void ResetPackageItems(this ItemPackage package, int tab, int[] ids)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)tab);
			int count = Mathf.Min(slotList.Count, ids.Length);

			for (int i = 0; i < count; i++)
			{
				if (-1 == ids[i])
				{
					slotList[i] = null;
				}
				else
				{
					ItemObject item = ItemMgr.Instance.Get(ids[i]);
					slotList[i] = item;
				}
			}
		}
	}
}