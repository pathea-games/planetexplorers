using UnityEngine;
using System.Collections.Generic;
using System;

public static class UIUtility
{
    public static T CreateItem<T> (T prefab, Transform parent) where T : MonoBehaviour
    {
        T item = MonoBehaviour.Instantiate<T>(prefab);
        Transform trans = item.transform;
        trans.parent = parent;
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = Vector3.one;
        trans.gameObject.SetActive(true);
  
        return item;
    }

    public static void UpdateListGos (List<GameObject> goList, GameObject prefab, Transform parent, int count,
        Action<int, GameObject> setItemContent, Action<GameObject> destroyItem)
    {
        if (count > goList.Count)
        {
            for (int i = 0; i < goList.Count; i++)
            {
                if (setItemContent != null)
                {
                    setItemContent(i, goList[i]);
                }
            }

            int cnt = count;
            for (int i = goList.Count; i < cnt; i++)
            {
                GameObject item = GameObject.Instantiate(prefab);
                Transform trans = item.transform;
                item.transform.parent = parent.transform;
                trans.localPosition = Vector3.zero;
                trans.localRotation = Quaternion.identity;
                trans.localScale = Vector3.one;
                trans.gameObject.SetActive(true);

                goList.Add(item);

                if (setItemContent != null)
                {
                    setItemContent(i, item);
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (setItemContent != null)
                {
                    setItemContent(i, goList[i]);
                }
            }

            for (int i = goList.Count - 1; i >= count; i--)
            {
                if (destroyItem != null)
                    destroyItem(goList[i]);
                GameObject.Destroy(goList[i]);
                goList[i].transform.parent = null;
                goList.RemoveAt(i);
            }
        }
    }

    public static void UpdateListItems<T>(List<T> itemList, T prefab, Transform parent, int count,
        Action<int, T> setItemContent, Action<T> destroyItem)  where T : MonoBehaviour
    {
        count = count < 0 ? 0 : count;
        if (count > itemList.Count)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (setItemContent != null)
                {
                    setItemContent(i, itemList[i]);
                }
            }

            int cnt = count;
            for (int i = itemList.Count; i < cnt; i++)
            {
                T item = MonoBehaviour.Instantiate<T>(prefab);
                Transform trans = item.transform;
                item.transform.parent = parent.transform;
                trans.localPosition = Vector3.zero;
                trans.localRotation = Quaternion.identity;
                trans.localScale = Vector3.one;
                trans.gameObject.SetActive(true);

                itemList.Add(item);

                if (setItemContent != null)
                {
                    setItemContent(i, item);
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (setItemContent != null)
                {
                    setItemContent(i, itemList[i]);
                }
            }

            for (int i = itemList.Count - 1; i >= count; i--)
            {
                if (destroyItem != null)
                    destroyItem(itemList[i]);
                GameObject.Destroy(itemList[i].gameObject);
                itemList[i].transform.parent = null;
                itemList.RemoveAt(i);
            }
        }
    }

}
