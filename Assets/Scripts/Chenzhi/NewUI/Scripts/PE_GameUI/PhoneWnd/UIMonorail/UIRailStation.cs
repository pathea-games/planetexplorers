using UnityEngine;
using System.Collections;

public class UIRailStation : MonoBehaviour 
{
	public OnGuiIndexBaseCallBack mClickStatge = null;

	public Railway.Point mRailPointData = null;

	UISprite mSprStation = null;


	public void Init(Railway.Point _mRailPointData)
	{
		mRailPointData = _mRailPointData;
		mSprStation = this.gameObject.GetComponent<UISprite>();
	}

	void OnClickStatge()
	{
		if (mRailPointData == null)
			return;
		if (mRailPointData.pointType == Railway.Point.EType.Joint)
			return;
		if (mClickStatge != null)
			mClickStatge(mRailPointData.id);
	}


	public void SetSelected(bool isSelected)
	{
		if (mSprStation != null)
			mSprStation.color = isSelected ? new Color(0,1,0.4f,1) : new Color(1,1,1,1);
	}
}
