using UnityEngine;
using System.Collections;
using System;

public class UIUpdatePIngTextColor : MonoBehaviour {

	// Use this for initialization
	public UIListItemCtrl mListItemCtrl;

	private UILabel UIPingText=null;
	
	void Start () 
	{
		if(mListItemCtrl != null)
		{
			UIPingText = mListItemCtrl.mLabelList[6].GetComponent<UILabel>();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(UIPingText == null)
			return;

		if(UIPingText.text.Length > 0)
		{
			int mPing = -1;
			try
			{
				mPing = Convert.ToInt32(UIPingText.text);
			}
			catch
			{
				return;
			}

			if(mPing < 151)
				UIPingText.color = new Color(10f/255f,205f/255f,42f/255f);
			else if(mPing <251)
				UIPingText.color = new Color(1f,219/255f,108f/255f);
			else
				UIPingText.color = Color.red;
		}
	}
}
