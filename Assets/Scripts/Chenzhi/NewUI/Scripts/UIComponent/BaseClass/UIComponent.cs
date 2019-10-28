using UnityEngine;
using System.Collections;

//老的UI事件
public delegate void OnGuiBtnClicked();
public delegate void OnGuiCheckBoxSelected(bool isSelected);
public delegate void OnGuiIndexBaseCallBack(int index);
public delegate void OnGuiStringBaseFunc(string str);
public delegate void OnMenuSelectionChange(int index, string text = "");


public abstract class UIComponent : MonoBehaviour 
{
	public GameObject eventReceiver = null;
}
