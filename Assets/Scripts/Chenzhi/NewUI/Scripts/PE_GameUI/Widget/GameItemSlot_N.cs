using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public class GameItemSlot_N : UIItemSlot
//{
//	public InvEquipment equipment;
//	public InvBaseItem.Slot slot;
//
//	override protected InvGameItem observedItem
//	{
//		get
//		{
//			return (equipment != null) ? equipment.GetItem(slot) : null;
//		}
//	}
//
//	/// <summary>
//	/// Replace the observed item with the specified value. Should return the item that was replaced.
//	/// </summary>
//
//	override protected InvGameItem Replace (InvGameItem item)
//	{
//		return (equipment != null) ? equipment.Replace(slot, item) : item;
//	}
//	
//	protected override void OnTooltip (bool show)
//	{
//		InvGameItem item = show ? mItem : null;
//
//		if (item != null)
//		{
//			InvBaseItem bi = item.baseItem;
//
//			if (bi != null)
//			{
//				string t = "[" + NGUITools.EncodeColor(item.color) + "]" + item.name + "[-]\n";
//
//				t += "[AFAFAF]Level " + item.itemLevel + " " + bi.slot;
//
//				List<InvStat> stats = item.CalculateStats();
//
//				for (int i = 0, imax = stats.Count; i < imax; ++i)
//				{
//					InvStat stat = stats[i];
//					if (stat.amount == 0) continue;
//
//					if (stat.amount < 0)
//					{
//						t += "\n[FF0000]" + stat.amount;
//					}
//					else
//					{
//						t += "\n[00FF00]+" + stat.amount;
//					}
//
//					if (stat.modifier == InvStat.Modifier.Percent) t += "%";
//					t += " " + stat.id;
//					t += "[-]";
//				}
//
//				if (!string.IsNullOrEmpty(bi.description)) t += "\n[FF9900]" + bi.description;
//				UITooltip.ShowText(t);
//				return;
//			}
//		}
//		UITooltip.ShowText(null);
//	}
//}
