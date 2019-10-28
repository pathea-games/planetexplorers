using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HelpExtension
{
	public static class GameObjectHelpFunc 
	{
		public static void RefreshItem(this List<GameObject> old_item, int new_count, GameObject prefab, Transform parent = null)
		{
			int old_count = old_item.Count;

			if (new_count <= old_count)
			{
				for (int i = old_count - 1; i >= new_count; i--)
				{
					GameObject.Destroy(old_item[i]);
					GameObject go = old_item[i];
					go.transform.parent = null;
					old_item.RemoveAt(i);
				}

			}
			else
			{
				for (int i = old_count; i < new_count; i++)
				{
					GameObject item =  prefab.CreateNew(parent);
					old_item.Add(item);
				}
			}
		}

		public static GameObject CreateNew (this GameObject obj, Transform parent = null)
		{
			GameObject clone = GameObject.Instantiate(obj) as GameObject;
			clone.transform.parent = parent;
			clone.transform.localPosition = Vector3.zero;
			clone.transform.localScale = Vector3.one;
			clone.transform.localRotation = Quaternion.identity;
			return clone;
		}
	}
}
