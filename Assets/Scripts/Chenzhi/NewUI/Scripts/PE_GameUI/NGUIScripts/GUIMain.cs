using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

//public delegate void OnGuiBtnClicked();
//public delegate void OnGuiCheckBoxSelected(bool isSelected);
//public delegate void OnGuiIndexBaseCallBack(int index);
//public delegate void OnGuiStringBaseFunc(string str);
//
//public delegate void OnMenuSelectionChange(int index, string text = "");

//public class GUIWindowBase : MonoBehaviour
//{
//	protected bool 		mWindowActive;
//	protected UIPanel	mWindowPlane;
//	[HideInInspector]
//	public bool	  		mInit = false;
//	[HideInInspector]
//	public float		mDepth;
//	
////	void Awake () 
////	{
////		if(!mInit)
////			InitWindow();
////		mDepth = transform.localPosition.z;
////	}
//	
//	public virtual void InitWindow()
//	{
//		mInit = true;
//		mWindowPlane = GetComponent<UIPanel>();
//	}
//		
//	public virtual void AwakeWindow()
//	{
//		if(!mInit)
//			InitWindow();
//		mWindowActive = true;
//		mWindowPlane.gameObject.SetActive(mWindowActive);
//		Vector3 pos = transform.localPosition;
//		if(pos.x > Screen.width/2)
//			pos.x = Screen.width/2-200;
//		if(pos.x < -Screen.width/2)
//			pos.x = -Screen.width/2+200;
//		if(pos.y > Screen.height/2)
//			pos.y = Screen.height/2-200;
//		if(pos.y < -Screen.height/2)
//			pos.y = -Screen.height/2+200;
//		transform.localPosition = pos;
//		ActiveWnd();
//	}
//	
//	public virtual bool HideWindow()
//	{
//		if(!mInit)
//			InitWindow();
//			bool oldState = mWindowActive;
//		if(mInit)
//		{
//			mWindowActive = false;
//			mWindowPlane.gameObject.SetActive(mWindowActive);
////			if (UILocalPostionMgr.Instance != null)
////				UILocalPostionMgr.Instance.SaveUIPostion();
//		}
//		return oldState;
//	}
//	
//	public virtual void ChangeWindowShowState()
//	{
//		mWindowActive = !mWindowActive;
//		if(mWindowActive)
//			AwakeWindow();
//		else
//			HideWindow();
//	}
//	
//	protected virtual void OnClose()
//	{
//		HideWindow();
//	}
//	
//	public virtual bool IsOpen()
//	{
//		return mWindowActive;
//	}
//	
//	void OnClick()
//	{
//		ActiveWnd();
//	}
//	
//	public virtual void DeActiveWnd()
//	{
//		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, mDepth);
//	}
//	
//	public virtual void ActiveWnd()
//	{
//        //if(null != GameGui_N.Instance)
//        //{
//        //    if(null != GuiStateManager.Instance.mCurrentActiveWnd)
//        //        GuiStateManager.Instance.mCurrentActiveWnd.DeActiveWnd();
//        //    GuiStateManager.Instance.mCurrentActiveWnd = this;
//        //}
//        //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, GuiStateManager.ActiveWndDepth);
//	}
//}
//
//
//
//public class StaticWindowGUIBase : MonoBehaviour
//{
//	public GameObject mWnd;
//	
//	public virtual void AwakeWindow()
//	{
//		mWnd.SetActive(true);
//	}
//	
//	public virtual void HideWindow()
//	{
//		mWnd.SetActive(false);
//	}
//	
//	public void ChangeWindowShowState()
//	{
//		if(mWnd.activeSelf)
//			HideWindow();
//		else
//			AwakeWindow();
//	}
//	
//	public virtual void InitWindow()
//	{
//		
//	}
//	
//	public virtual bool IsOpen()
//	{
//		return mWnd.activeSelf;
//	}
//}


