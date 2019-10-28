using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIDisconnectionLine : MonoBehaviour 
{
	public delegate void ClickRailStation(bool isConnect,int mLineIndex,int mStationID);
	public event ClickRailStation mSelectedLine = null;

	public GameObject mLine;
	public GameObject mStage;
	public GameObject mEndStage;

	public bool mCanSeleted = true;
	public bool isSelected = false;
	public int mScale = 1;

	public List<Railway.Point> mPointList;
	public int mIndex = -1;

	public List<UIRailStation> mStationList = new List<UIRailStation>();
	public List<Vector2> mStagePosList = new List<Vector2>();
	
	private List<UISprite> mSprStageList = new List<UISprite>();
	private List<UITiledSprite> mTsLineList = new List<UITiledSprite>();

	// Use this for initialization
	void Start () 
	{
	    
	}
	
	// Update is called once per frame
	void Update () 
	{
	
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

	public void Init(List<Railway.Point> _PointList,int index)
	{
		mPointList = _PointList;
		mIndex = index;
	}


	public void CreateLine()
	{
		if(mLine == null || mStage == null || mStagePosList == null)
			return;
		
		for(int i=0;i<mPointList.Count;i++)
		{
			GameObject osg;
			if (mPointList[i].pointType == Railway.Point.EType.End)
				osg = GameObject.Instantiate(mEndStage) as GameObject;
			else
				osg = GameObject.Instantiate(mStage) as GameObject;
			osg.transform.parent = this.gameObject.transform;
			if (mPointList[i].pointType == Railway.Point.EType.Joint)
				osg.transform.localScale = new Vector3(5*mScale,5*mScale,-1);
			else if (mPointList[i].pointType == Railway.Point.EType.End)
				osg.transform.localScale = new Vector3(14*mScale,12*mScale,-1);
			else
				osg.transform.localScale = new Vector3(10*mScale,10*mScale,-1);
			
			UIRailStation station = osg.GetComponent<UIRailStation>();
			if (station != null)
			{
				station.Init(mPointList[i]);
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
				ots.SetActive(false);
				UITiledSprite ts = ots.GetComponent<UITiledSprite>();
				mTsLineList.Add(ts);
				
				station = ots.GetComponent<UIRailStation>();
				if (station != null && i-1 >= 0)
				{
					station.Init(mPointList[i-1]);
					station.mClickStatge = OnClickStatge;
				}
			}
		}
	}

	void OnClickStatge(int mStationID)
	{
		if(mCanSeleted == false)
			return;
		if(mSelectedLine != null)
			mSelectedLine(false, mIndex,mStationID);
	}

	public void SetSelected(bool _isSelected)
	{
		for(int i=0; i< mSprStageList.Count;i++)
		{
			if (i<mPointList.Count)
			{
				if (mPointList[i].pointType == Railway.Point.EType.End)
					mSprStageList[i].spriteName = _isSelected ? "railbegin" : "railbegin_1";
				else
					mSprStageList[i].spriteName = _isSelected ? "railpoint" : "railpoint_1";
			}
		}
		foreach(UITiledSprite ts in mTsLineList)
		{
			ts.gameObject.SetActive(_isSelected) ;
		}
		isSelected = _isSelected;
	}

	public void ResetLinePos(UIMapCtrl mMapCtrl)
	{
		mStagePosList.Clear();
		for (int i=0;i<mPointList.Count;i++)
		{
			Vector3 mRoutePos = new Vector3(mPointList[i].position.x,mPointList[i].position.z,0); 
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
			
			int pos_x = (int) mStagePosList[i].x;
			int pos_y = (int) mStagePosList[i].y;
			mSprStageList[i].gameObject.transform.localPosition = new Vector3(pos_x,pos_y,-1);
			
			if(i != 0)
			{
				pos_x = (int) ((mStagePosList[i].x + mStagePosList[i-1].x) / 2 ) ; 
				pos_y =	(int) ((mStagePosList[i].y + mStagePosList[i-1].y) / 2 ) ; 
				
				int scrol_x = (int) (mStagePosList[i].x - mStagePosList[i-1].x);
				int scrol_y = (int) (mStagePosList[i].y - mStagePosList[i-1].y);
				
				mTsLineList[i-1].gameObject.transform.localPosition = new Vector3(pos_x,pos_y,-1);
				mTsLineList[i-1].gameObject.transform.localEulerAngles = new Vector3(0,0,Mathf.Atan2(scrol_y, scrol_x) * Mathf.Rad2Deg);
				
				int leng = (int) Mathf.Sqrt(scrol_x * scrol_x + scrol_y * scrol_y);
				mTsLineList[i-1].gameObject.transform.localScale = new Vector3(leng,4,-1);
				
			}
		}
	}
}
