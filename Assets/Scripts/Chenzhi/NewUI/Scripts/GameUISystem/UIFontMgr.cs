using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIFontMgr : MonoBehaviour 
{

	static UIFontMgr mInstance = null;
	public static UIFontMgr Instance{ get{ return mInstance; } }

	public enum LanguageType
	{
		English,
		Chinese
	}

	public LanguageType mLanguageType = LanguageType.English ;

	public List<UIFont> mEnglish = new List<UIFont>();
	
	public List<UIFont> mChinese  = new List<UIFont>();

	void Awake()
	{
		mInstance = this;
		if (SystemSettingData.Instance == null)
			return;
		if (SystemSettingData.Instance.IsChinese)
			mLanguageType = LanguageType.Chinese;
		else
			mLanguageType = LanguageType.English;

	}


	public UIFont GetFontForLanguage(UIFont oldFont)
	{

		int index = mEnglish.IndexOf(oldFont);
		if (index == -1)
			index = mChinese.IndexOf(oldFont);

		if (index > -1)
		{
			if (mLanguageType == LanguageType.English  && index < mEnglish.Count)
				return mEnglish[index];
			else if (mLanguageType == LanguageType.Chinese  && index < mChinese.Count)
				return mChinese[index];
			
		}
		return oldFont;
	}

}
