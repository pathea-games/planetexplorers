using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIRailLine : MonoBehaviour 
{
	public delegate void ClickRailStation(bool isConnect,int mLineIndex,int mStationID);
	public event ClickRailStation mSelectedLine = null;

	public GameObject mLine;
	public GameObject mStage;
	public GameObject mEndStage;
	public UISprite mSprCar;// huo che tu biao


	public Railway.Route mRoute;
	public int mScale = 1;

	public bool mCanSeleted = true;
	public bool isSelected = false;

	public List<UIRailStation> mStationList = new List<UIRailStation>();
	public List<Vector2> mStagePosList = new List<Vector2>();

	private List<UISprite> mSprStageList = new List<UISprite>();
	private List<UITiledSprite> mTsLineList = new List<UITiledSprite>();


	public void Init(Railway.Route _Route)
	{
		mRoute = _Route;
	}

	public UIRailStation FindStation(int stationID)
	{
		UIRailStation station = mStationList.Find(
		delegate(UIRailStation st)
		{
			return st.mRailPointData.id == stationID;
		});

		return station;
	}


	public int FindStationIndex(int stationID)
	{
		int index = mStationList.FindIndex(
			delegate(UIRailStation st)
			{
			return st.mRailPointData.id == stationID;
		});
		
		return index;
	}

	public void CreateLine()
	{
		if(mLine == null || mStage == null || mStagePosList == null)
			return;

		for(int i=0;i<mRoute.pointCount;i++)
		{
			GameObject osg;
            if (mRoute.GetPointByIndex(i).pointType == Railway.Point.EType.End)
				osg = GameObject.Instantiate(mEndStage) as GameObject;
			else
				osg = GameObject.Instantiate(mStage) as GameObject;
			osg.transform.parent = this.gameObject.transform;
			if (mRoute.GetPointByIndex(i).pointType == Railway.Point.EType.Joint)
				osg.transform.localScale = new Vector3(5*mScale,5*mScale,-1);
			else if (mRoute.GetPointByIndex(i).pointType == Railway.Point.EType.End)
				osg.transform.localScale = new Vector3(14*mScale,12*mScale,-1);
			else
				osg.transform.localScale = new Vector3(10*mScale,10*mScale,-1);

			UIRailStation station = osg.GetComponent<UIRailStation>();
			if (station != null)
			{
				station.Init(mRoute.GetPointByIndex(i));
				station.mClickStatge = OnClickStatge;
			}
			mStationList.Add(station);

			osg.SetActive(true);
			UISprite spr = osg.GetComponent<UISprite>();
			mSprStageList.Add(spr);

			if(i != 0)
			{
				GameObject ots = GameObject.Instantiate(mLine) as GameObject;
				ots.transform.parent = this.gameObject.transform;
				ots.transform.localScale = new Vector3(10*mScale,10*mScale,-1);
				ots.SetActive(true);
				UITiledSprite ts = ots.GetComponent<UITiledSprite>();
				mTsLineList.Add(ts);

				station = ots.GetComponent<UIRailStation>();
				if (station != null && i-1 >= 0)
				{
					station.Init(mRoute.GetPointByIndex(i-1));
					station.mClickStatge = OnClickStatge;
				}
			}
		}
	}

	public void ResetLinePos(UIMapCtrl mMapCtrl)
	{
		mStagePosList.Clear();
		for (int i=0;i<mRoute.pointCount;i++)
		{
			Vector3 mRoutePos = new Vector3(mRoute.GetPointByIndex(i).position.x,mRoute.GetPointByIndex(i).position.z,0); 
			Vector3 pos = mRoutePos - mMapCtrl.mMapCamera.gameObject.transform.localPosition;
			int count = mMapCtrl.mCameraSizeCount;
			mStagePosList.Add(new Vector2(pos.x/count,pos.y/count));
		}

		UpdateLinePos();
	}


 	void UpdateLinePos()
	{
		if(mLine == null || mStage == null || mStagePosList == null )
			return;

		for(int i=0;i<mStagePosList.Count;i++)
		{
			// 存在异步更新问题，保证数组不越界
			if ( i>= mSprStageList.Count )
				return;

            //lz-2016.09.20 因为移动和缩放看地图的相机用的是浮点数，这里也用浮点数统一单位，避免地图和轻轨位置和缩放不同步的问题(错误 #3575)
            float pos_x =  mStagePosList[i].x;
            float pos_y = mStagePosList[i].y;
			mSprStageList[i].gameObject.transform.localPosition = new Vector3(pos_x,pos_y,-1);

			if(i != 0)
			{
				pos_x =  ((mStagePosList[i].x + mStagePosList[i-1].x) / 2 ) ; 
				pos_y =	 ((mStagePosList[i].y + mStagePosList[i-1].y) / 2 ) ;

                float scrol_x =  (mStagePosList[i].x - mStagePosList[i-1].x);
                float scrol_y =  (mStagePosList[i].y - mStagePosList[i-1].y);

				mTsLineList[i-1].gameObject.transform.localPosition = new Vector3(pos_x,pos_y,-1);
				mTsLineList[i-1].gameObject.transform.localEulerAngles = new Vector3(0,0,Mathf.Atan2(scrol_y, scrol_x) * Mathf.Rad2Deg);

                float leng =  Mathf.Sqrt(scrol_x * scrol_x + scrol_y * scrol_y);
				mTsLineList[i-1].gameObject.transform.localScale = new Vector3(leng,4,-1);

			}
		}
	}
	
	void OnClickStatge(int mStationID)
	{
		if(mCanSeleted == false)
			return;
		if(mSelectedLine != null)
			mSelectedLine(true,mRoute.id,mStationID);
	}
	

	public void SetSelected(bool _isSelected)
	{

		for(int i=0; i< mSprStageList.Count;i++)
		{
			if (i<mRoute.pointCount)
			{
				if (mRoute.GetPointByIndex(i).pointType == Railway.Point.EType.End)
					mSprStageList[i].spriteName = _isSelected ? "railbegin" : "railbegin_1";
				else
					mSprStageList[i].spriteName = _isSelected ? "railpoint" : "railpoint_1";
			}
		}
		foreach(UITiledSprite ts in mTsLineList)
		{
			ts.spriteName = _isSelected ? "railline" : "railline_1";
		}
		isSelected = _isSelected;
	}

	public void UpdateCarPos(UIMapCtrl mMapCtrl)
	{
        if (mRoute.trainId == Pathea.IdGenerator.Invalid || this.gameObject == null || mSprCar == null)
		{
			if(mSprCar != null)
				mSprCar.enabled = false;
			return;
		}
		
		mSprCar.enabled = true;

		Vector3 carAng = new Vector3();
        Vector3 carUp = new Vector3();
        Vector3 carPos = mRoute.GetTrainPosition(out carAng, out carUp);
		Vector3 mRoutePos = new Vector3(carPos.x,carPos.z,0); 
		Vector3 pos = mRoutePos - mMapCtrl.mMapCamera.gameObject.transform.localPosition;
		int count = mMapCtrl.mCameraSizeCount;
		
		Vector2 v2CarPos = new Vector2(pos.x/count,pos.y/count);
		mSprCar.transform.localPosition = new Vector3(v2CarPos.x,v2CarPos.y,0);
		if (mRoute.moveDir == -1)
			mSprCar.transform.localEulerAngles = new Vector3(180,0, Mathf.Atan2(carAng.x,carAng.z) * Mathf.Rad2Deg + 180);
		else
			mSprCar.transform.localEulerAngles = new Vector3(180,0, Mathf.Atan2(carAng.x,carAng.z) * Mathf.Rad2Deg);
	}

	// Use this for initialization
	void Start () 
	{
//		List<Vector2> testVector = new List<Vector2>();
//		testVector.Add(new Vector2 (-40,-15));
//		testVector.Add(new Vector2 (0,0));
//		testVector.Add(new Vector2 (30,20));
//		testVector.Add(new Vector2 (70,90));
//		Init(0,testVector);
	}


}
