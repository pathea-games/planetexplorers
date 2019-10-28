using UnityEngine;
using System.Collections;

//public class CustomSettingItem_N : MonoBehaviour 
//{
//	public UISprite mIcon;
//	
//	public CreatPlayerGui_N mParent;
//	
//	int mIndex = 0;
//	
//	void Start () 
//	{
//		Transform tran = transform.FindChild("Background");
//		if(null != tran)
//		{
//			UIWidget uiw = tran.GetComponent<UIWidget>();
//			if(null != uiw)
//				uiw.MakePixelPerfect();
//		}
//	}
//	
//	public void SetItemInfo(string iconName, int index)
//	{
//		mIcon.spriteName = iconName;
//		mIndex = index;
//	}
//	
//	void OnClick ()
//	{
//		if(Input.GetMouseButtonUp(0) && mIcon.spriteName != "Null")
//			mParent.OnSettingItemClicked(mIndex);
//	}
//}
