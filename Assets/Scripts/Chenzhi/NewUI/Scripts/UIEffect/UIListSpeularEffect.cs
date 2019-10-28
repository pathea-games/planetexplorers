using UnityEngine;
using System.Collections;

public class UIListSpeularEffect : MonoBehaviour 
{
	UIMenuPanel panel = null; 
	[SerializeField] UITexture texSPeaular;
	[SerializeField] UISprite sprBg_1;
//	[SerializeField] UIFlowing mTopFlow;
//	[SerializeField] UIFlowing mButtomFlow;

	// Use this for initialization
	void Start () 
	{
		panel = transform.parent.GetComponent<UIMenuPanel>();

		if (panel == null)
			return;
		// pivot
		sprBg_1.pivot = panel.spBg.pivot;
		texSPeaular.pivot = panel.spBg.pivot;

		// pos
		Vector3 vec = panel.spBg.transform.localPosition;
		texSPeaular.transform.localPosition = vec;
		sprBg_1.transform.localPosition = vec;



//		if (panel.spBg.pivot == UIWidget.Pivot.Left)
//		{
//			float y = panel.spBg.transform.localScale.y/2; 
//			mTopFlow.BeginPosition = new Vector3 (mTopFlow.BeginPosition.x,y,mTopFlow.BeginPosition.z);
//			mTopFlow.EndPosition = new Vector3 (mTopFlow.EndPosition.x,y,mTopFlow.EndPosition.z);
//			y = panel.spBg.transform.localScale.y/2 - 4; 
//			mButtomFlow.BeginPosition = new Vector3 (mButtomFlow.BeginPosition.x,-y,mButtomFlow.BeginPosition.z);
//			mButtomFlow.EndPosition = new Vector3 (mButtomFlow.EndPosition.x,-y,mButtomFlow.EndPosition.z);
//		}
//		else  if (panel.spBg.pivot == UIWidget.Pivot.Bottom )
//		{
//			float y = panel.spBg.transform.localScale.y/2 - 3; 
//			mTopFlow.BeginPosition = new Vector3 (mTopFlow.BeginPosition.x,y,mTopFlow.BeginPosition.z);
//			mTopFlow.EndPosition = new Vector3 (mTopFlow.EndPosition.x,y,mTopFlow.EndPosition.z);
//			y = panel.spBg.transform.localScale.y/2 - 3; 
//			mButtomFlow.BeginPosition = new Vector3 (mButtomFlow.BeginPosition.x,-y,mButtomFlow.BeginPosition.z);
//			mButtomFlow.EndPosition = new Vector3 (mButtomFlow.EndPosition.x,-y,mButtomFlow.EndPosition.z);
//		}



	}
	
	// Update is called once per frame
	void Update () 
	{
		if (panel != null)
		{
			// scale
			Vector3 vec = panel.spBg.transform.localScale;
			texSPeaular.transform.localScale = vec;
			sprBg_1.transform.localScale = vec;

		}
	}
}
