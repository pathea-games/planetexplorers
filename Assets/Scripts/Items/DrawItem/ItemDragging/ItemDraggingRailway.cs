using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDraggingRailway : ItemDraggingBase
{
    public Railway.Point.EType mType;
    public CrossLine mLine;
    public Transform mLinkPoint;

	public CrossLine subLine;

    bool mShowPositionErrorTips = false;
    bool mShowDistanceErrorTips;
    bool mShowLinkErrorTips;

    UTimer mNoticeTimer;

    Railway.Point mPrePoint;

	Vector3 m_LinkPos1, m_LinkPos2;

	public static readonly float StepHeight = 0.5f;
	public static readonly int MaxStepTime = 30;

    public override void OnDragOut()
    {
        base.OnDragOut();

        mNoticeTimer = new UTimer();
        mNoticeTimer.ElapseSpeed = -1f;
        mNoticeTimer.Second = 1;
    }

    public override bool OnPutDown()
    {
        int prePointId = Railway.Manager.InvalId;
        if (mPrePoint != null)
        {
            prePointId = mPrePoint.id;
        }

		if (GameConfig.IsMultiClient)
		{
			if (VArtifactUtil.IsInTownBallArea(transform.position))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}

			if (null != PlayerNetwork.mainPlayer)
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_AddPoint, transform.position, mType, prePointId, itemDragging.itemObj.instanceId);
		}
		else
		{
			RemoveTrees();
			RailwayOperate.Instance.RequestAddPoint(transform.position, mType, prePointId, itemDragging.itemObj.instanceId);
		}

		return base.OnPutDown();
	}

    public override bool OnDragging(Ray cameraRay)
    {
        bool putoutEnable = base.OnDragging(cameraRay);

        mShowPositionErrorTips = !putoutEnable;

        if (putoutEnable)
        {
            TryLink();
        }
		else
			BreakLink();

        return putoutEnable;
    }

	public override void OnRotate ()
	{
//		base.OnRotate ();
	}

    void EstablishLink(Vector3 pos1, Vector3 pos2)
    {
        mLine.m_Begin = pos1;
        mLine.m_End = pos2;

        if (!mLine.gameObject.activeSelf)
        {
            mLine.gameObject.SetActive(true);
        }
    }
	void EstablishSubLink(Vector3 pos1, Vector3 pos2)
	{
		subLine.m_Begin = pos1;
		subLine.m_End = pos2;
		
		if (!subLine.gameObject.activeSelf)
		{
			subLine.gameObject.SetActive(true);
		}
	}

    void BreakLink()
    {
        if (mLine.gameObject.activeSelf)
        {
            mLine.gameObject.SetActive(false);
        }

		if(subLine.gameObject.activeSelf)
		{
			subLine.gameObject.SetActive(false);
		}
    }

    bool TryLink()
    {
        mPrePoint = FindNearestIsolatePoint(mLinkPoint.position);

        if (mPrePoint == null)
        {
			BreakLink();
            return false;
        }

        m_LinkPos1 = mLinkPoint.position;
        m_LinkPos2 = mPrePoint.GetLinkPosition();
		Transform preTran = mPrePoint.GetTrans();

		//if can't link move higher
		bool getPos = false;
		for(int i = 0; i < MaxStepTime; ++i)
		{
			if (Railway.Manager.CheckLinkState(m_LinkPos1, m_LinkPos2, transform, preTran))
			{
				getPos = true;
				EstablishSubLink(mLinkPoint.position, m_LinkPos2);
				transform.position += i * StepHeight * Vector3.up;
				EstablishLink(m_LinkPos1, m_LinkPos2);
				break;
			}
			m_LinkPos1 += StepHeight * Vector3.up;
		}

		if (!getPos)
        {
            BreakLink();

            mPrePoint = null;

            return false;
        }
        return true;
    }

    List<int> mIsolatePoints = null;
    List<int> isolatePoints
    {
        get
        {
            if (mIsolatePoints == null)
            {
                mIsolatePoints = Railway.Manager.Instance.GetIsolatePoint();
            }

            return mIsolatePoints;
        }
    }

    Railway.Point FindNearestIsolatePoint(Vector3 pos, float dis = Railway.Manager.JointMaxDistance)
    {
        Railway.Point retPoint = null;
        float minSqrDis = dis * dis;

        foreach (int pointId in isolatePoints)
        {
            Railway.Point point = Railway.Manager.Instance.GetPoint(pointId);
            if (null == point)
            {
                continue;
            }

            float sqrDis = (pos - point.GetLinkPosition()).sqrMagnitude;
            if (sqrDis < minSqrDis)
            {
                minSqrDis = sqrDis;
                retPoint = point;
            }
        }

        return retPoint;
    }

    void Update()
    {
        UpdateTips();
    }

    void OnGUI()
    {
        GUI.color = Color.green;

        if(mShowPositionErrorTips)
        {
            GUILayout.Label(UIMsgBoxInfo.CannotPut.GetString());
        }

        if (mTooFar)
        {
            GUILayout.Label(UIMsgBoxInfo.DistanceNotMatch.GetString());
        }
    }

    void UpdateTips()
    {
        mNoticeTimer.Update(Time.deltaTime);
        if (mNoticeTimer.Second < 0)
        {
            mNoticeTimer.Second = 1f;
            if (mShowPositionErrorTips)
            {
                GlobalShowGui_N.ShowString(UIMsgBoxInfo.CannotPut.GetString());
                mShowPositionErrorTips = false;
            }
            else
            {
                if (mShowDistanceErrorTips)
                {
                    GlobalShowGui_N.ShowString(UIMsgBoxInfo.DistanceNotMatch.GetString());
                    mShowDistanceErrorTips = false;
                }
                else if (mShowLinkErrorTips)
                {
                    GlobalShowGui_N.ShowString(UIMsgBoxInfo.ConnectError.GetString());
                    mShowLinkErrorTips = false;
                }
            }
        }
    }

	void RemoveTrees()
	{
		Vector3 dir = m_LinkPos1 - m_LinkPos2;
		Vector3 normalDir = dir.normalized;
		RaycastHit[] hitTreeCols = Physics.RaycastAll(m_LinkPos2, normalDir, dir.magnitude, 1 << Pathea.Layer.NearTreePhysics, QueryTriggerInteraction.Ignore);
		for(int i = 0; i < hitTreeCols.Length; ++i)
		{
			GlobalTreeInfo gTreeinfo = PETools.PEUtil.GetTreeinfo(hitTreeCols[i].collider);
			if(null != gTreeinfo)
				DigTerrainManager.RemoveTree(gTreeinfo);
		}
	}
}
