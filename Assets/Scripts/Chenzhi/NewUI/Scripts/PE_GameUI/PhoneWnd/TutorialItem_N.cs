using UnityEngine;
using System.Collections;

public class TutorialItem_N : MonoBehaviour 
{
	public UICheckbox 	mCheckBox;
	public UILabel		mLabel;
	public int 			mID = 0;
	public delegate void OnClickEvent(object sender);
	public event OnClickEvent e_OnClick = null;

	
	public void SetItem(int ID, string content = "")
	{
		mID = ID;
		mLabel.text = content;
	}
	
	void OnClick()
	{
		if(Input.GetMouseButtonUp(0) && -1 != mID && e_OnClick != null)
			e_OnClick(this);
	}
}
