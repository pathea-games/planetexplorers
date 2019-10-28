using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

internal class ActionManager : MonoBehaviour
{
	private static List<ActionDelegate> _syncList = new List<ActionDelegate>();

	void Start()
	{
		StartCoroutine(CheckActions());
	}

	void OnApplicationQuit()
	{
		_syncList.Clear ();
	}

	IEnumerator CheckActions()
	{
		while (true)
		{
			if (_syncList.Count > 0)
			{
				ActionDelegate action = _syncList[0];
				if (null != action)
				{
					action.OnAction();
					_syncList.RemoveAt(0);
				}
			}

			yield return null;
		}
	}
	
	internal static void AddAction(ActionDelegate action)
	{
		_syncList.Add(action);
	}
}
