using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScrollItem : MonoBehaviour
{
	public UILabel 		mContent;
	
	public UIScrollBar	mScroll;
	
	public List<string> mSelections; // Count must greater than 1

    public bool UseLocalLocation = true;
	
	public int mIndex = 0;
	
	float lastValue = 0;
    private string m_Description;

    #region private methods
    void Update ()
	{
		if(gameObject.name != "Quality" && Mathf.Abs(lastValue - mScroll.scrollValue) > 0.01f)
		{
			lastValue = mScroll.scrollValue;
			UIOption.Instance.OnChange();
		}
		mIndex = (int)Mathf.Round(mScroll.scrollValue*(mSelections.Count - 1));
        mContent.text = GetStrByCurIndex();
	}

    string GetStrByCurIndex()
    {
        if (UseLocalLocation)
        {
            int strID = 0;
			string str = mSelections[mIndex].Trim();
			if(str == string.Empty){
				return string.Empty;
			} else {
				if (int.TryParse(str, out strID)) {
	                return PELocalization.GetString(strID);
	            } else {
	                Debug.Log("Not find localLization keyID:"+mSelections[mIndex]);
					return string.Empty;
				}
			}
        }
        return mSelections[mIndex].ToString();
    }

    
    #endregion

    #region public methods
    public void SetIndex(int index)
	{
		if(index >=0 && index < mSelections.Count)
			mIndex = index;
		else
			mIndex = mSelections.Count - 1;
        mContent.text = GetStrByCurIndex();
		mScroll.scrollValue = ((float)mIndex)/(mSelections.Count - 1);
		lastValue = mScroll.scrollValue;
	}
	
	public void SetValue(float setValue)
	{
		lastValue = mScroll.scrollValue = Mathf.Clamp01(setValue);
		mContent.text = "";
	}

    #endregion
}
