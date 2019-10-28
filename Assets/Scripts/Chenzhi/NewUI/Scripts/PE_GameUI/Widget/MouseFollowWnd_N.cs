using UnityEngine;
using System.Collections;

public class MouseFollowWnd_N : MonoBehaviour 
{
	static MouseFollowWnd_N mInstance;
	public static MouseFollowWnd_N Instance{ get { return mInstance; } }
	
	public Transform mWnd;
	public UILabel mText;
	
	
	void Awake()
	{
		mInstance = this;
	}
	
	// Update is called once per frame
	void Update () 
	{
        //if(null != CameraController.Instance)
        //    mWnd.localPosition = CameraController.Instance.MouseCenter + 10 * Vector3.forward;
        mWnd.localPosition = PeCamera.mousePos + 10 * Vector3.forward;
	}
	
	public void SetText(string content)
	{
		mText.text = content;
	}
}
