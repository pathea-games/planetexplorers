using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Railway;
using Pathea;

public class RailwayTrain : MousePickableChildCollider 
{
    [SerializeField]
	List<RailwaySeat> m_SeatList;

	public Railway.Route mRoute;

	PeEntity mMainPlayer;
	PeEntity mainPlayer{ get { if(null == mMainPlayer) mMainPlayer = MainPlayer.Instance.entity; return mMainPlayer; } }

	Point stayStation;

	const int ArriveSoundID = 2495;
	const int DepartSoundID = 2496;
	const int RunSoundID = 2497;

	AudioController mRunAudio;
	AudioController runAudio{ get { if(null == mRunAudio) mRunAudio = AudioManager.instance.Create(transform.position, RunSoundID, transform, false, false); return mRunAudio; } }

    void Awake()
    {
        Transform points = transform.FindChild("monorail_cart/Master_Point");
        m_SeatList = new List<RailwaySeat>(points.childCount);
        foreach (Transform tran in points)
        {
            m_SeatList.Add(tran.GetComponent<RailwaySeat>());
        }
    }

    public bool HasPassenger()
    {
        foreach (RailwaySeat seat in m_SeatList)
        {
            if (seat.passenger != null)
            {
                return true;
            }
        }

        return false;
    }

	public bool AddPassenger(IPassenger pas)
	{
        RailwaySeat seat = m_SeatList.Find((s) =>
        {
            if (s.passenger != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        });

        if (null == seat)
        {
            return false;
        }

        return seat.SetPassenger(pas);
	}

	public bool HasEmptySeat()
	{
		if(m_SeatList == null)
			return false;
		
		for(int i = 0; i < m_SeatList.Count; ++i)
		{
			if(m_SeatList[i].passenger == null)
				return true;
		}
		return false;
	}

    public bool RemovePassenger(IPassenger pas, Vector3 getOffPos)
    {
        RailwaySeat seat = GetSeat(pas);

        if (null == seat)
        {
            return false;
        }

        return seat.ResetPassenger(getOffPos);
    }

    public bool RemovePassenger(IPassenger pas)
	{
        return RemovePassenger(pas, GetGetOffPos());
	}

    private RailwaySeat GetSeat(IPassenger pas)
    {
        RailwaySeat seat = m_SeatList.Find((s) =>
        {
            if (s.passenger != pas)
            {
                return false;
            }
            else
            {
                return true;
            }
        });
        return seat;
    }

    public void ClearPassenger()
    {
        m_SeatList.ForEach((s) =>
        {
            s.ResetPassenger(GetGetOffPos());
        });
    }

    Vector3 GetGetOffPos()
    {
        return PETools.PEUtil.GetRandomPosition(transform.position + transform.right * 3, 0f, 1f);
    }
	
	bool GetGetOnGetOffEnable()
	{
		if (mRoute == null)
		{
			return false;
		}
		
		if(null == stayStation || stayStation.pointType == Point.EType.Joint)
		{
			return false;
		}
		
		if (Vector3.Distance(transform.position, mainPlayer.position) > 10f)
		{
			return false;
		}
		
		return true;
	}

	void Update()
	{
		if(null == mRoute)
			return;

		if(null != stayStation && null == mRoute.stayStation)
		{
			AudioManager.instance.Create(transform.position, DepartSoundID, transform);
			runAudio.PlayAudio(0.5f);
		}
		else if(null == stayStation && null != mRoute.stayStation)
		{
			AudioManager.instance.Create(transform.position, ArriveSoundID, transform);
			runAudio.StopAudio(0.2f);
		}
		stayStation = mRoute.stayStation;
	}

	protected override bool CheckPick(Ray camMouseRay, out float dis)
	{
		if(base.CheckPick(camMouseRay, out dis))
		{
			MousePicker.Instance.UpdateTis();
			return true;
		}
		return false;
	}

	protected override void CheckOperate()
	{		
		if(null == mainPlayer || null == mRoute) return;
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem))
		{
			if (!GetGetOnGetOffEnable())
			{
				return;
			}
			
			if (mainPlayer.passengerCmpt.IsOnRail)
			{
				PEStationCtrl stationCtrl = stayStation.station.GetComponent<PEStationCtrl>();
				RailwayOperate.Instance.RequestGetOffTrain(mRoute.id, mainPlayer.Id, null != stationCtrl ? stationCtrl.getOffPos : transform.position + 2.5f * Vector3.up);
			}
			else
			{
				RailwayOperate.Instance.RequestGetOnTrain(mRoute.id, mainPlayer.Id);
			}
		}
	}
	
	protected override string tipsText
	{
		get
		{
			if(null == mainPlayer || null == mRoute) return "";
			string showStr = mRoute.name;
			
			if(GetGetOnGetOffEnable())
			{
				if (mainPlayer.passengerCmpt.IsOnRail)
				{
					showStr += "\n" + PELocalization.GetString(8000131);
				}
				else
				{
					showStr += "\n" + PELocalization.GetString(8000130);
				}
			}
			
			return showStr;
		}
	}
}
