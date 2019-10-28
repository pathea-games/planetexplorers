using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using PatheaScript;
using ItemAsset;

public class UIMonoRailCtrl : UIBaseWidget 
{
	static UIMonoRailCtrl mInstance;
	public static UIMonoRailCtrl Instance{ get { return mInstance; } }

	public delegate void OnWayPointEnevt(Railway.Point mRailPointData);
	public event OnWayPointEnevt e_BtnStop = null;
	

	public delegate void OnCreateWayEnevt(Railway.Point mRailPointData,string _LineName);
	public event OnCreateWayEnevt e_BtnStar = null;
	
	public event System.Action e_InitWindow;

	public event Action<int, ItemObject> e_SetTrain;
	public event Action<int, int> e_SetTrainToStation;

	public event Action<int, string> e_ResetPointName;
	public event Action<int, string> e_ResetRouteName;
	public event Action<int, float> e_ResetPointTime;

 
	public UILabel mTitleLineName;
	public UIInput mLineName;
	public UILabel mLine;
	public UILabel mLbStationCount;
	public UILabel mLbNodeCount;
	public UILabel mLbRangeAblity;
	public UILabel mLbOneWaytime;
	public Grid_N mRailIcon;

	public GameObject mBtnStop;

	public UILabel mLbBtnStop; 

	public GameObject mLineInfo;
	public GameObject mStationInfo;

	public UILabel mTitleStationName;
	public UIInput mStationName;
	public UILabel mArriveTime;
	public UIInput mRedestanceTime;

	public GameObject mLinesContent;
	public GameObject mLinePrefab;
	public GameObject mDisLinePrefab;
	public UIMapCtrl mMapCtrl;
	public GameObject mMapMash;
	public UISprite mPlayerPos;

	private List<UIRailLine> mRailLineList = new List<UIRailLine>();
	private List<UIDisconnectionLine> mDisRailLineList = new List<UIDisconnectionLine>();

	List<Railway.Route> mRouteList = new List<Railway.Route>();

	int mSelectedLineId = -1;
	int mSelectedLineIndex = -1;
	
	// Use this for initialization
	void Start () 
	{
//---------------------------Test code----------------------------------------
//		for (int j=1;j<4;j++)
//		{
//
//			Railway.Route  ra = new Railway.Route();
//			ra.mPointData = new List<Railway.Point>();
//			ra.mID = j;
//			ra.mName = "Lin" + j.ToString();
//
//			for (int i=0;i<4;i++)
//			{
//				Railway.Point data = new Railway.Point();
//				data.mPosition = new Vector3(8200 + i*300,10 ,7800 + 100*i + 100*j);
//				data.mPointID = i;
//				data.mName = "satation " + i.ToString();
//				data.mStopTime = 120;
//				if (i==0 || i==3)
//					data.mType = Railway.Point.Type.TrackEnd;
//				else if (i==1)
//					data.mType = Railway.Point.Type.TrackStation;
//				else
//					data.mType = Railway.Point.Type.TrackJoint;
//
//				ra.mPointData.Add(data);
//			}
//			AddMonoRail(ra);
//
//		}
//
//
//		List<List<Railway.Point>> mTestList = new List<List<Railway.Point>>();
//		List<Railway.Point> LinePoint = new List<Railway.Point>();
//		for (int i=0;i<4;i++)
//		{
//			Railway.Point data = new Railway.Point();
//			data.mPosition = new Vector3(8200 + i*300,10 ,7700 + 100*i);
//			data.mPointID = i;
//			data.mName = "satation " + i.ToString();
//			data.mStopTime = 120;
//			if (i==0 || i==3)
//				data.mType = Railway.Point.Type.TrackEnd;
//			else if (i==1)
//				data.mType = Railway.Point.Type.TrackStation;
//			else
//				data.mType = Railway.Point.Type.TrackJoint;
//			LinePoint.Add(data);
//		}
//		mTestList.Add(LinePoint);
//
//		ReDrawDisRailLine(mTestList);
// ---------------------------------------------
	}

	void Update()
	{
		if (GameUI.Instance.mMainPlayer == null)
			return;
		UpdatePlayerPos();
		UpdateCarState();
		UpdateInputState();
		UpdateStationArriveTime();
	}

	UIInput mLastFouceInput;

	void UpdateInputState()
	{
		if(mLineName.selected)
			mLastFouceInput = mLineName;
		else if(mLastFouceInput == mLineName)
		{
			OnLineNameChange(mLineName.text);
			mLastFouceInput = null;
		}

		if(mStationName.selected)
			mLastFouceInput = mStationName;
		else if(mLastFouceInput == mStationName)
		{
			OnStationNameChange(mStationName.text);
			mLastFouceInput = null;
		}

		if(mRedestanceTime.selected)
			mLastFouceInput = mRedestanceTime;
		else if(mLastFouceInput == mRedestanceTime)
		{
			OnStationResidenceTimeChange(mRedestanceTime.text);
			mLastFouceInput = null;
		}
	}
	
	// update line car 
	void UpdateCarState()
	{
		for (int i=0;i<mRailLineList.Count;i++)
			mRailLineList[i].UpdateCarPos(mMapCtrl);
	}

	void UpdatePlayerPos()
	{
		Vector3 playerPos = GameUI.Instance.mMainPlayer.position;
		Vector3 mPos = new Vector3(playerPos.x,playerPos.z,0) - mMapCtrl.mMapCamera.gameObject.transform.localPosition; 
		int count = mMapCtrl.mCameraSizeCount;
		Vector2 v2Pos = new Vector2(mPos.x/count,mPos.y/count);
		mPlayerPos.transform.localPosition = new Vector3(v2Pos.x,v2Pos.y,0);
		mPlayerPos.transform.localRotation = Quaternion.Euler(0, 0, -GameUI.Instance.mMainPlayer.tr.rotation.eulerAngles.y);
	}

	public override void OnCreate ()
	{
		mInstance = this;
		mMapCtrl.mMapMove += UpdateAllRailLinePos;
		mMapCtrl.mMapZoomed += UpdateAllRailLinePos;
		mRailIcon.SetItemPlace(ItemPlaceType.IPT_Rail, 0);
		mRailIcon.onLeftMouseClicked = PickTrain;
		mRailIcon.onDropItem = DropTrain;
		ClearUIInfo();
		mMapMash.SetActive(!GameConfig.IsMultiMode);
		base.OnCreate ();
	}

   	protected override void InitWindow()
	{
		base.InitWindow ();
		if (e_InitWindow != null)
			e_InitWindow();
	}

	void ClearUIInfo()
	{
		mTitleLineName.text = "";

     	mLineName.text = "";
		mLbStationCount.text = "";
     	mLbNodeCount.text = "";
     	mLbRangeAblity.text = "";
    	mLbOneWaytime.text = ""; 

		mTitleStationName.text = "";
    	mStationName.text = "";
     	mArriveTime.text = "";
     	mRedestanceTime.text = "";

		mBtnStop.SetActive(false);
		mRailIcon.SetItem(null);
	}

	public override void Show ()
	{
		if (mMapCtrl != null && null != GameUI.Instance.mMainPlayer)
		{
			Vector3 playerPos = GameUI.Instance.mMainPlayer.position;
			Vector3 cameraPos = mMapCtrl.mMapCamera.transform.localPosition;
			cameraPos.y = playerPos.z;
			cameraPos.x = playerPos.x;
			mMapCtrl.mMapCamera.transform.localPosition = cameraPos;
		}
		UpdateAllRailLinePos();

		base.Show ();


	}


	public void AddMonoRail(Railway.Route route)
	{
		if(!mRouteList.Contains(route))
		{
			mRouteList.Add(route);
			//Do UI Update
			AddUIRailLine(route);
		}
	}
	
	public void RemoveMonoRail(Railway.Route route)
	{
		if(mRouteList.Contains(route))
		{
			DeleteUIRailLine(route);
			mRouteList.Remove(route);
			mRailIcon.SetItem(null);
		}
	}

	public void ReDrawDisRailLine(List<List<Railway.Point>> mRailPointList)
	{
		RemoveAllDisLine();
		for (int i=0;i<mRailPointList.Count;i++)
		{
			GameObject disLine = GameObject.Instantiate(mDisLinePrefab) as GameObject;
			disLine.transform.parent = mLinesContent.transform;
			disLine.transform.localPosition = new Vector3(0,0,0);
			disLine.transform.localScale = new Vector3(1,1,1);
			
			UIDisconnectionLine uiDisLine = disLine.GetComponent<UIDisconnectionLine>();
			uiDisLine.Init(mRailPointList[i],i);
			uiDisLine.CreateLine();
			uiDisLine.ResetLinePos(mMapCtrl);
			
			uiDisLine.mSelectedLine += OnSeletedLine;
			mDisRailLineList.Add(uiDisLine); 
		}
	}


	void RemoveAllDisLine()
	{
		foreach (UIDisconnectionLine disLine in mDisRailLineList)
		{
			disLine.transform.parent = null;
			GameObject.Destroy(disLine.gameObject);
		}
		mDisRailLineList.Clear();
	}
	
	void AddUIRailLine(Railway.Route _rail)
	{
		GameObject line = GameObject.Instantiate(mLinePrefab) as GameObject;
		line.transform.parent = mLinesContent.transform;
		line.transform.localPosition = new Vector3(0,0,0);
		line.transform.localScale = new Vector3(1,1,1);
		
		UIRailLine uiLine = line.GetComponent<UIRailLine>();
		uiLine.Init(_rail);
		uiLine.CreateLine();
		uiLine.ResetLinePos(mMapCtrl);

		uiLine.mSelectedLine += OnSeletedLine;

		mRailLineList.Add(uiLine); 
	}
	
	
	void DeleteUIRailLine(Railway.Route _rail)
	{
		UIRailLine line = mRailLineList.Find(
			delegate(UIRailLine li)
			{
			return li.mRoute == _rail;
		});
		
		if(line != null)
		{
			GameObject.Destroy(line.gameObject);
			mRailLineList.Remove(line);
		}
	}

	public void UpdateAllRailLinePos()
	{
		UpdateCarState();
		for (int i=0;i<mRailLineList.Count;i++)
		{
			mRailLineList[i].ResetLinePos(mMapCtrl);
		}
		for (int i=0; i<mDisRailLineList.Count; i++)
		{
			mDisRailLineList[i].ResetLinePos(mMapCtrl);
		}
	}

	public void UpdateSelectedStation()
	{
		if (null != mSelectedStation)
		{
			Railway.Point point = Railway.Manager.Instance.GetPoint(mSelectedStation.mRailPointData.id);
			if (point != null)
				OnSeletedLine(point.routeId != Railway.Manager.InvalId, point.id);
			else
			{
				ClearUIInfo();
			}
		}
	}

	public void OnSeletedLine(bool isConnect,int stationID)
	{
		int index = -1;
		if (isConnect)
		{
			for(int i=0;i<mRailLineList.Count;i++)
			{
				index = mRailLineList[i].FindStationIndex(stationID);
				if ( -1 != index)
				{
					OnSeletedLine(true, mRailLineList[i].mRoute.id, stationID);
					break;
				}
			}
		}
		else
		{
			for(int i=0;i<mDisRailLineList.Count;i++)
			{
				index = mDisRailLineList[i].FindStationIndex(stationID);
				if ( -1 != index)
				{
					OnSeletedLine(false, mDisRailLineList[i].mIndex, stationID);
					break;
				}
			}
		}
	}

	bool isConnectLine = false;
	UIRailStation mSelectedStation = null;
	void OnSeletedLine(bool isConnect,int routeID, int stationID)
	{
		isConnectLine = isConnect;

		ChangeUILineInfo(isConnect);

		if(mSelectedLineId != -1)
		{
			UIRailLine lineOld = mRailLineList.Find(
				delegate(UIRailLine li)
				{
				return li.mRoute.id == mSelectedLineId;
			});
			if (lineOld != null)
				lineOld.SetSelected(false);
			
			UIDisconnectionLine disLineOld = mDisRailLineList.Find(
				delegate(UIDisconnectionLine li)
				{
				return li.mIndex == mSelectedLineId;
			});
			if (disLineOld != null)
				disLineOld.SetSelected(false);
		}
		
		
		if (isConnect)
		{
			UIRailLine line = mRailLineList.Find(
				delegate(UIRailLine li)
				{
				return li.mRoute.id == routeID;
			});
			if (line == null)
			{
				ClearLineInfo();
				return;
			}

			mSelectedLineIndex = mRailLineList.FindIndex(
				delegate(UIRailLine li)
				{
				return li == line;
			});


			line.SetSelected(true);
			mSelectedLineId =  routeID;
			SetLineInfo(line.mRoute);

			if (mSelectedStation != null)
				mSelectedStation.SetSelected(false);

			UIRailStation station = line.FindStation(stationID);
			if (station != null)
			{
				station.SetSelected(true);
				mSelectedStation = station;
			}
			SetSatationInfo(station.mRailPointData);
			EnableBtn(mBtnStop,true);
		}

		else
		{
			int index = routeID;

			UIDisconnectionLine disLine = mDisRailLineList.Find(
				delegate(UIDisconnectionLine li)
				{
				return li.mIndex == index;
			});

			if (disLine == null)
			{
				ClearLineInfo();
				return;
			}

			disLine.SetSelected(true);
			mSelectedLineId =  index;
       
			if (mSelectedStation != null)
				mSelectedStation.SetSelected(false);
			
			UIRailStation station = disLine.FindStation(stationID);
			if (station != null)
			{
				station.SetSelected(true);
				mSelectedStation = station;
			}
			SetSatationInfo(station.mRailPointData);
			bool ok = PERailwayCtrl.CheckRoute(station.mRailPointData);
			EnableBtn(mBtnStop,ok);
		}
	}

	void EnableBtn(GameObject btn,bool value)
	{
		UISlicedSprite btnSp = btn.GetComponentInChildren<UISlicedSprite>();
		BoxCollider btnBc = btn.GetComponent<BoxCollider>();
		if (btnSp != null)
		{
			btnSp.spriteName = value ? "LoginBtn_on" : "LoginBtn_off";
		}
		if (btnBc != null)
			btnBc.enabled = value;
	}


	void ChangeUILineInfo(bool isConnect)
	{
		mLine.gameObject.SetActive(isConnect);
        //lz-2016.07.26 站点未运行的时候不显示线路名称,因为没有信息
        mTitleLineName.gameObject.SetActive(isConnect);
		mLineName.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbStationCount.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbNodeCount.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbRangeAblity.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbOneWaytime.gameObject.transform.parent.gameObject.SetActive(isConnect);
        mLbBtnStop.text = isConnect ? PELocalization.GetString(8000604) : PELocalization.GetString(8000605);

        //lz-2016.10.24 站点运行的时候不能修改站点停靠时间
        mRedestanceTime.enabled = !isConnect;

        mBtnStop.SetActive(true);
		mRailIcon.SetItem(null);
		mRailIcon.gameObject.SetActive (isConnect);
	}


	void SetLineInfo(Railway.Route route)
	{
        Railway.Route.Stats stats = route.GetStats();

		mTitleLineName.text = route.name;
		mLineName.text = route.name;
        mLbStationCount.text = stats.stationNum.ToString();
        mLbNodeCount.text = stats.jointNum.ToString();
        mLbRangeAblity.text = stats.totalIntDis.ToString() + " M";
		PETimer time = PETimerUtil.GetTmpTimer();
		time.Second = route.singleTripTime;
		mLbOneWaytime.text = time.GetStrHhMmSs(); 
		mRailIcon.SetItem(route.trainId);
	}

	void ClearLineInfo()
	{
		mTitleLineName.text = "";
		mLineName.text = "";
		mLbStationCount.text = "";
		mLbNodeCount.text = "";
		mLbRangeAblity.text = "";

		mLbOneWaytime.text = ""; 
		mRailIcon.SetItem(null);
	}

	void SetSatationInfo(Railway.Point satation)
	{
		mTitleStationName.text = satation.name;
		mStationName.text = satation.name;
		PETimer rTime = PETimerUtil.GetTmpTimer();
		rTime.Second = satation.realStayTime;
		int min = Convert.ToInt32(rTime.Minute);
     	mRedestanceTime.text = min.ToString();
	}

	void UpdateStationArriveTime()
	{
		if (mSelectedStation == null)
			return;

		float arriveTime= mSelectedStation.mRailPointData.GetArriveTime();
        if (arriveTime - 0f < PETools.PEMath.Epsilon)
        {
            mArriveTime.text = "--";
        }
        else
        {
			PETimer time = PETimerUtil.GetTmpTimer();
            time.Second = arriveTime;
            mArriveTime.text = time.GetStrHhMmSs();
        }
	}

	void OnLineNameChange(string text)
	{
		if ("" != text.Trim() && isConnectLine)
		{
			UIRailLine line = mRailLineList.Find(
				delegate(UIRailLine li)
				{
				return li.mRoute.id == mSelectedLineId;
			});

			if (line != null && line.mRoute.name != text)
			{
                if (!Railway.Manager.Instance.IsRouteNameExist(text))
				{
                    line.mRoute.name = text;
					mTitleLineName.text = text;
					if(null != e_ResetRouteName)
						e_ResetRouteName(line.mRoute.id, text);
				}
				else
					MessageBox_N.ShowOkBox(UIMsgBoxInfo.ReNameNotice.GetString());
				mLineName.text = line.mRoute.name;
			}
		}
	}


	void OnStationNameChange(string text)
	{
		if ("" != text.Trim() && mSelectedStation != null && mSelectedStation.mRailPointData.name != text)
		{
            if(!Railway.Manager.Instance.IsPointNameExist(text))			
			{
                mSelectedStation.mRailPointData.name = text;
				mTitleStationName.text = text;
				if(null != e_ResetPointName)
					e_ResetPointName(mSelectedStation.mRailPointData.id, text);
			}
			else
				MessageBox_N.ShowOkBox(UIMsgBoxInfo.ReNameNotice.GetString());
			mStationName.text = mSelectedStation.mRailPointData.name;
		}
	}

	void OnStationResidenceTimeChange(string text)
	{
		if (mSelectedStation != null && text.Length > 0)
		{
			PETimer time = PETimerUtil.GetTmpTimer();
			time.Minute = Convert.ToDouble(text);
			if(time.Minute > 0)
			{
                Railway.Route route = Railway.Manager.Instance.GetRouteByPointId(mSelectedStation.mRailPointData.id);
                if (route != null)
                {
                    route.SetStayTime(mSelectedStation.mRailPointData.id, Convert.ToSingle(time.Second));
                }
                else
                {
                    Railway.Point point = Railway.Manager.Instance.GetPoint(mSelectedStation.mRailPointData.id);
                    point.stayTime = Convert.ToSingle(time.Second);
                }


                if (null != e_ResetPointTime)
                    e_ResetPointTime(mSelectedStation.mRailPointData.id, Convert.ToSingle(time.Second));
            }
            time.Second = mSelectedStation.mRailPointData.realStayTime;
			mRedestanceTime.text = ((int)time.Minute).ToString();
		}
	}
	

	void BtnStopOnClick()
	{
		if (mSelectedStation != null)
		{
			if (e_BtnStop != null  && isConnectLine)
			{
				e_BtnStop(mSelectedStation.mRailPointData);
				//OnSeletedLine(false, mSelectedStation.mRailPointData.id);
			}
			else if (e_BtnStar != null && !isConnectLine)
			{
				e_BtnStar(mSelectedStation.mRailPointData,mLineName.text);
				//OnSeletedLine(true,mSelectedStation.mRailPointData.id);
			}

		}
	}
	
	void BtnLineTitleLeftOnClick()
	{
		if (isConnectLine == false)
			return;
		mSelectedLineIndex--;
		if (mSelectedLineIndex >=0 && mSelectedLineIndex< mRailLineList.Count)
		{
			UIRailLine line = mRailLineList[mSelectedLineIndex];
				OnSeletedLine(true,line.mRoute.id,line.mRoute.GetPointByIndex(0).id);
		}
		else
		{
			
			if (mRailLineList.Count > 0)
			{
				mSelectedLineIndex = mRailLineList.Count - 1;
				UIRailLine line = mRailLineList[mSelectedLineIndex];
				if (line.mRoute.pointCount > 0)
					OnSeletedLine(true,line.mRoute.id,line.mRoute.GetPointByIndex(0).id);
			}
		}
	}

	void BtnLineTitleRightOnClick()
	{
		if (isConnectLine == false)
			return;
		mSelectedLineIndex++;
		if (mSelectedLineIndex >=0 && mSelectedLineIndex< mRailLineList.Count)
		{
			UIRailLine line = mRailLineList[mSelectedLineIndex];
			if (line.mRoute.pointCount > 0)
				OnSeletedLine(true,line.mRoute.id,line.mRoute.GetPointByIndex(0).id);
		}
		else
		{
			if (mRailLineList.Count > 0)
			{	
				mSelectedLineIndex = 0;
				UIRailLine line = mRailLineList[mSelectedLineIndex];
				if (line.mRoute.pointCount > 0)
					OnSeletedLine(true,line.mRoute.id,line.mRoute.GetPointByIndex(0).id);
			}
		}
	}



	void BtnStationTitleLeftOnClick()
	{
		if (mSelectedStation == null && mSelectedStation)
			return;

		if (isConnectLine)
		{
			UIRailLine Line = mRailLineList.Find(
				delegate(UIRailLine li)
				{
				return li.mRoute.id == mSelectedLineId;
			});

			if (Line == null || Line.mRoute.pointCount == 0 )
				return;

			int index = Line.FindStationIndex(mSelectedStation.mRailPointData.id);
			do
			{
				index -- ;
				if (index <0 || index >= Line.mRoute.pointCount)
					break;
			}while(Line.mRoute.GetPointByIndex(index).pointType == Railway.Point.EType.Joint);

            if (index < 0 || index >= Line.mRoute.pointCount)
			{
                index = Line.mRoute.pointCount - 1;
			}

            if (Line.mRoute.pointCount > 0)
				OnSeletedLine(true,Line.mRoute.id,Line.mRoute.GetPointByIndex(index).id);
		}
		else
		{
			UIDisconnectionLine Line = mDisRailLineList.Find(
				delegate(UIDisconnectionLine li)
				{
				return li.mIndex == mSelectedLineId;
			});

			if (Line == null || Line.mPointList.Count ==0 )
				return;

			int index = Line.FindStationIndex(mSelectedStation.mRailPointData.id);
			do
			{
				index -- ;
				if (index <0 || index >= Line.mPointList.Count)
					break;
			}while(Line.mPointList[index].pointType == Railway.Point.EType.Joint);
			if (index < 0 || index >= Line.mPointList.Count)
			{
				index = Line.mPointList.Count -1;
			}
			
			if (Line.mPointList.Count > 0)
				OnSeletedLine(false,Line.mIndex,Line.mPointList[index].id);
		}
	}
	void BtnStationTitleRightOnClick()
	{
		if (mSelectedStation == null)
			return;

		if (isConnectLine)
		{
			UIRailLine Line = mRailLineList.Find(
				delegate(UIRailLine li)
				{
				return li.mRoute.id == mSelectedLineId;
			});
			
			if (Line == null || Line.mRoute.pointCount ==0 )
				return;
			
			int index = Line.FindStationIndex(mSelectedStation.mRailPointData.id);
			do
			{
				index ++ ;
				if (index <0 || index >= Line.mRoute.pointCount)
					break;
			}while(Line.mRoute.GetPointByIndex(index).pointType == Railway.Point.EType.Joint);

			if (index < 0 || index >= Line.mRoute.pointCount)
				index = 0;

			if (Line.mRoute.pointCount > 0)
				OnSeletedLine(true,Line.mRoute.id,Line.mRoute.GetPointByIndex(index).id);
		}
		else
		{
			UIDisconnectionLine Line = mDisRailLineList.Find(
				delegate(UIDisconnectionLine li)
				{
				return li.mIndex == mSelectedLineId;
			});
			
			if (Line == null || Line.mPointList.Count ==0 )
				return;
			
			int index = Line.FindStationIndex(mSelectedStation.mRailPointData.id);
			do
			{
				index ++ ;
				if (index <0 || index >= Line.mPointList.Count)
					break;
			}while(Line.mPointList[index].pointType == Railway.Point.EType.Joint);
			
			if (index < 0 || index >= Line.mPointList.Count)
				index = 0;
			
			if (Line.mPointList.Count > 0)
				OnSeletedLine(false,Line.mIndex,Line.mPointList[index].id);
		}
	}

	public void PickTrain(Grid_N grid)
	{
		if(null != mSelectedStation)
		{
			Railway.Route route = Railway.Manager.Instance.GetRoute(mSelectedStation.mRailPointData.routeId);
			if (route.trainRunning)
				return ;
			if(null != route)
				SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
		}
	}

	public void DropTrain(Grid_N grid)
	{
        if (null == mSelectedStation || null == e_SetTrain)
        {
            return;
        }

		ItemObject itemObj = SelectItem_N.Instance.ItemObj;
        if (null == itemObj)
        {
            return;
        }

        ItemAsset.Train train = itemObj.GetCmpt<ItemAsset.Train>();
        if (null == train)
        {
            return;
        }
		int routeId = mSelectedStation.mRailPointData.routeId;
		Railway.Route route = Railway.Manager.Instance.GetRoute(routeId);
		if (!route.trainRunning)
		{
			mRailIcon.SetItem(itemObj);
			e_SetTrain(routeId, itemObj);
			if(null != e_SetTrainToStation)
				e_SetTrainToStation(mSelectedStation.mRailPointData.routeId, mSelectedStation.mRailPointData.id);
		}
	}

	public Railway.Point GetSelPoint()
	{
		if (null != mSelectedStation)
			return mSelectedStation.mRailPointData;
		return null;
	}
}
