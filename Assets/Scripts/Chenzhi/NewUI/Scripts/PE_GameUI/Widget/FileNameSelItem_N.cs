using UnityEngine;
using System.Collections;


public class FileNameSelItem_N : MonoBehaviour
{
	public UILabel mTextLabel;
	
	GameObject mEnventReceiver;
	
	public void SetText(string fileName, GameObject eventReceiver)
	{
		mTextLabel.text = fileName;
		mEnventReceiver = eventReceiver;
		GetComponent<UICheckbox>().radioButtonRoot = transform.parent;
	}
	
	void OnSelected(bool selected)
	{
		if(selected && null != mEnventReceiver)
			mEnventReceiver.SendMessage("OnFileSelected", mTextLabel.text, SendMessageOptions.DontRequireReceiver);
	}
}
