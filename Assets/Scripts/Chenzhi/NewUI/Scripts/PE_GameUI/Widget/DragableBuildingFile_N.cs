using UnityEngine;
using System.Collections;

public class DragableBuildingFile_N : MonoBehaviour 
{
	public UILabel mFileName;
	
	GameObject mEventReceiver;
	
	public string FileName{ get { return mFileName.text;}}
	
	public void SetFile(string fileName, GameObject eventRecv)
	{
		mFileName.text = fileName;
		mEventReceiver = eventRecv;
	}
	
	void OnDrag (Vector2 delta)
	{
		if(mEventReceiver)
			mEventReceiver.SendMessage("OnFileDrag", this, SendMessageOptions.DontRequireReceiver);
	}
}
