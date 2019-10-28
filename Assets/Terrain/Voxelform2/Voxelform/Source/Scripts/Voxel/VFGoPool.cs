//#define DBG_NOPOOLING
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IRecyclable
{
	void OnRecycle();
}

public static class VFGoPool<T> where T : MonoBehaviour,IRecyclable
{
	private static Stack<T> _itemGos = new Stack<T> ();
#if UNITY_EDITOR	
	private static int nSum = 0;
#endif
	public static void PreAlloc(int nPreAlloc)
	{
		_itemGos = new Stack<T> (nPreAlloc);
		_itemGos.Clear();
		for(int i = 0; i < nPreAlloc; i++)
		{
			GameObject go = new GameObject();
			T vfGo = go.AddComponent<T>();
			go.SetActive(false);
#if UNITY_EDITOR
			nSum++;
#endif
			_itemGos.Push(vfGo);
		}
	}
	public static T GetGo ()
	{
#if DBG_NOPOOLING
		GameObject go = new GameObject();
		T vfGo = go.AddComponent<T>();
		return vfGo;
#else
#if UNITY_EDITOR
		//int n = _itemGos.Count;
		//int m = nSum;
#endif
		if (_itemGos.Count == 0) {
			GameObject go = new GameObject();
			T vfGo = go.AddComponent<T>();
			go.SetActive(false);
#if UNITY_EDITOR
			nSum++;
#endif
			return vfGo;
		} else {
			return _itemGos.Pop();
		}
#endif
	}
	public static void FreeGo (T item)
	{
#if DBG_NOPOOLING
		GameObject.Destroy(item);
#else
		item.OnRecycle();
		_itemGos.Push(item);
#endif
	}

	// For threading
	private static List<T> _itemsToFreeInThread = new List<T>();
	private static List<T> _itemsToFreeInMain = new List<T>();
	public static void ReqFreeGo(T item) // Each request would cause lock but there is another strategy: collect reqs and then batch the reqs
	{
		lock(_itemsToFreeInThread)
		{
			_itemsToFreeInThread.Add(item);
		}
	}
	public static void SwapReq()
	{
		if(_itemsToFreeInThread.Count > 0)
		{
			lock(_itemsToFreeInThread)
			{
				_itemsToFreeInMain.AddRange(_itemsToFreeInThread);
				_itemsToFreeInThread.Clear();
			}
		}
	}
	public static void ExecFreeGo()
	{
		if(_itemsToFreeInMain.Count > 0)
		{
			int n = _itemsToFreeInMain.Count;
			for(int i = 0; i < n; i++)
			{
				if(_itemsToFreeInMain[i] != null)
					FreeGo(_itemsToFreeInMain[i]);	// Unity Api can not used in thread other than main
#if UNITY_EDITOR
				else{
					Debug.Log("[VFGOPOOL]: unexpected null go to free");
				}
#endif
			}
			_itemsToFreeInMain.Clear();
		}
	}


}

