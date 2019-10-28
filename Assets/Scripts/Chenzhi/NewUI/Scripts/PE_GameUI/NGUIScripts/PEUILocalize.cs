using UnityEngine;
using System.Collections;

public class PEUILocalize : MonoBehaviour 
{
	public int mStringID;

    public bool mStrToUpper=false;

    public bool mStrToLower=false;
	
	UILabel mLable;
	
	void Start()
	{
		mLable = GetComponent<UILabel>();
		Localize();
	}
	
	public void Localize ()
	{
		if(mLable)
		{
            string str=PELocalization.GetString(mStringID);
            if (mStrToUpper)
            {
                mLable.text = str.ToUpper();
            }
            else if (mStrToLower)
            {
                mLable.text = str.ToLower();
            }
            else
            {
                mLable.text = str;
            }
		}
	}
	
}
