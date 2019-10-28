using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RailwayPointGuiCtrl : MonoBehaviour 
{
	static RailwayPointGuiCtrl mInstance;
	public static RailwayPointGuiCtrl Instance{ get { return mInstance; } }
	public RailwayPointGui_N mPointGui {get { return GameUI.Instance.mRailwayPoint;}}
	
	Railway.Point mOpPoint;
	bool mInputSel;
	
	List<Railway.Point> mNearPoints;
	List<bool> mLinkEnable;
	
	void Awake()
	{
		mInstance = this;
		mNearPoints = new List<Railway.Point>();
		mLinkEnable = new List<bool>();

	}


	void Start()
	{
		mPointGui.e_PreMenuChange += OnPrePointChange;
		mPointGui.e_NextPointChange += OnNextPointChange;
		mPointGui.RecycleEvent += OnRecycleBtn;
	}
	
	public void SetInfo(Railway.Point point)
	{
		if(null == point)
		{
			if(mPointGui.IsOpen())
				mPointGui.Hide();
			return;
		}
		
		if(!mPointGui.IsOpen())
			mPointGui.Show();
		
		mOpPoint = point;
		mNearPoints = Railway.Manager.Instance.GetNearPoint(mOpPoint);
		mLinkEnable.Clear();
        for (int i = 0; i < mNearPoints.Count; i++)
        {
            if (mNearPoints[i].routeId != Railway.Manager.InvalId)
            {
                mLinkEnable.Add(false);
            }
            else
            {
                mLinkEnable.Add(mOpPoint.nextPointId != mNearPoints[i].id
                                && mOpPoint.prePointId != mNearPoints[i].id
                                && Railway.Manager.CheckLinkState(mOpPoint, mNearPoints[i])
                                );
            }
        }
		
		mPointGui.PointName = mOpPoint.name;
		ResetPrePoint();
		ResetNextPoint();
        /*Railway.Route route = */Railway.Manager.Instance.GetRouteByPointId(mOpPoint.id);
	}
	
	void ResetPrePoint()
	{
		List<string> menuList = new List<string>();
		int preIndex = -1;
		menuList.Add("Null");
		for(int i = 0; i < mNearPoints.Count; i++)
		{
			if(mLinkEnable[i])
				menuList.Add(mNearPoints[i].name);
			else
				menuList.Add("[ff0000]" + mNearPoints[i].name + "[-]");

			if(mNearPoints[i].id == mOpPoint.prePointId)
				preIndex = i;
		}
		mPointGui.SetPrePoint(menuList, preIndex + 1);
	}
	
	void ResetNextPoint()
	{
		List<string> menuList = new List<string>();
		int nextIndex = -1;
		menuList.Add("Null");
		for(int i = 0; i < mNearPoints.Count; i++)
		{
			if(mLinkEnable[i])
				menuList.Add(mNearPoints[i].name);
			else
				menuList.Add("[ff0000]" + mNearPoints[i].name + "[-]");
			if(mNearPoints[i].id == mOpPoint.nextPointId)
				nextIndex = i;
		}
		mPointGui.SetNextPoint(menuList, nextIndex + 1);
	}
	
	public void OnRecycleBtn()
	{
        RailwayOperate.Instance.RequestRemovePoint(mOpPoint.id);
	}
	

	public void OnPrePointChange(int index, string text)
	{
        if (index == 0)
            RailwayOperate.Instance.RequestChangePrePoint(mOpPoint.id, Railway.Manager.InvalId);
        else if (mLinkEnable[index - 1])
            RailwayOperate.Instance.RequestChangePrePoint(mOpPoint.id, mNearPoints[index - 1].id);
        mPointGui.Hide();
	}
	
	public void OnNextPointChange(int index, string text)
	{
        if (index == 0)
            RailwayOperate.Instance.RequestChangeNextPoint(mOpPoint, Railway.Manager.InvalId);
        else if (mLinkEnable[index - 1])
            RailwayOperate.Instance.RequestChangeNextPoint(mOpPoint, mNearPoints[index - 1].id);
        mPointGui.Hide();
	}
	
	void Update()
	{
		if(mPointGui.IsOpen())
		{
			if(mInputSel && !mPointGui.mNameInput.selected  && null != mOpPoint
			   && mPointGui.PointName != mOpPoint.name && mPointGui.PointName.Trim() != "")
			{
                if (Railway.Manager.Instance.IsPointNameExist(mPointGui.mNameInput.text))
                {
                    MessageBox_N.ShowOkBox(UIMsgBoxInfo.ReNameNotice.GetString());
                }
                else
                {
                    mOpPoint.name = mPointGui.mNameInput.text;
                }

				mPointGui.PointName = mOpPoint.name;
			}
			mInputSel = mPointGui.mNameInput.selected;

			if( null != GameUI.Instance.mMainPlayer && null != mOpPoint && Vector3.Distance( GameUI.Instance.mMainPlayer.position, mOpPoint.position) > 30f)
                mPointGui.Hide();
		}
	}
}
