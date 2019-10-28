using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptionItem_N : MonoBehaviour
{
    public UILabel mContent;
    	
	public List<string> mSelections;

    public bool UseLocalization = true;
	
	public int mIndex = 0;

    private string m_Description;

    #region private methods
    void LBtnDown()
	{
		if(--mIndex < 0)
			mIndex = mSelections.Count - 1;
        UpdateContent();
	}
	
	void RBtnDown()
	{
		if(++mIndex > mSelections.Count - 1)
			mIndex = 0;
        UpdateContent();
	}

    void UpdateContent()
    {
        mContent.text = GetStringByCurIndex();
        UIOption.Instance.OnChange();
    }

    string GetStringByCurIndex()
    {
        if(UseLocalization)
        {
			int strID;
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
		if(index >= 0 && index < mSelections.Count)
			mIndex = index;
		else
			mIndex = mSelections.Count - 1;
        mContent.text = GetStringByCurIndex();
	}

    #endregion
}
