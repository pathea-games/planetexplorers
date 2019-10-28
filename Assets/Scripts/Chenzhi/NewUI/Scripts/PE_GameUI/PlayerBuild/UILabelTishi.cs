using UnityEngine;
using System.Collections;

public class UILabelTishi : MonoBehaviour 
{
	UILabel mLabel;
	int tempCount = 0;
	// Use this for initialization
	void Start () 
	{
		mLabel = this.GetComponent<UILabel>();
	}

	public void ShowText(string text)
	{
		tempCount = 0;
		mLabel.enabled = true;
		mLabel.text = text;
	}

	// Update is called once per frame
	void Update () 
	{
		if (mLabel == null)
			return;
		if (mLabel.enabled == true)
			tempCount ++ ;
		else 
			tempCount = 0;

		if (tempCount > 100)
			mLabel.enabled = false;
	}
}
