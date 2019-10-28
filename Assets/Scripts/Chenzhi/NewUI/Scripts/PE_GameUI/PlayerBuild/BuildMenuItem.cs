using UnityEngine;
using System.Collections;

public class BuildMenuItem : MonoBehaviour 
{
	[SerializeField] UICheckbox mCheckBox;

	int mItemPos_x;
	// Update is called once per frame
	void Update () 
	{
		if (mCheckBox != null)
		{
			if (mCheckBox.isChecked)
				mItemPos_x = -15;
			else 
				mItemPos_x = 10;
		}
		Vector3 pos = gameObject.transform.localPosition;
		gameObject.transform.localPosition = Vector3.Lerp(pos,new Vector3(mItemPos_x,pos.y,pos.z),0.1f);
		if (Mathf.Abs(pos.x - mItemPos_x) < 1)
		{
			gameObject.transform.localPosition = new Vector3(mItemPos_x,pos.y,pos.z);
		}
	}
}
