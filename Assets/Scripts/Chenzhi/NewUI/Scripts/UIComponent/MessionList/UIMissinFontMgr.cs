using UnityEngine;
using System.Collections;

public class UIMissinFontMgr : MonoBehaviour 
{

	// Use this for initialization

	public UIFont mEnglish = null ;
	public UIFont mChinese = null ;

	UILabel mUIlable;
	void Awake()
	{
		mUIlable = this.transform.GetComponent<UILabel >();

		if (SystemSettingData.Instance == null)
			return;
		if (SystemSettingData.Instance.IsChinese)
			mUIlable.font = mChinese;
		else 
			mUIlable.font = mEnglish;
	}

}
