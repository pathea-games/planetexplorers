using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RailwayStation))]
public class PEStationCtrl : MousePickableChildCollider 
{
    [SerializeField]
    Transform GetOffPos;

	RailwayStation mStation;
	Railway.Route mRoute;

	public RailwayStation station{ get { return mStation; } }

	public Vector3 getOffPos { get { return null != GetOffPos ? GetOffPos.position : transform.position + 1.5f * Vector3.up; } }

    bool openMenuEnable
    {
        get
        {
            return null == mRoute;
        }
    }
	
	const float MouseOpRange = 150f;

	public bool isJoint
	{
		get
		{
			return mStation.Point.pointType == Railway.Point.EType.Joint;
		}
	}

    public Railway.Point point
    {
        get
        {
            return Railway.Manager.Instance.GetPoint(mStation.pointId);
        }
    }

    Pathea.PeTrans playerTrans;

    Vector3 playerPos
    {
        get
        {
            if (playerTrans == null && HasMainPlayer())
            {
                playerTrans = Pathea.PeCreature.Instance.mainPlayer.peTrans;
            }

            if (playerTrans != null)
            {
                return playerTrans.position;
            }
            else
            {
                return Vector3.zero;
            }
        }
        set
        {
//            if (playerPos == null && HasMainPlayer())
//            {
//                playerTrans = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PeTrans>();
//            }
			// unreachable code !

            if (playerTrans != null)
            {
                playerTrans.position = value;
            }
        }
    }

    bool HasMainPlayer()
    {
        return Pathea.IdGenerator.Invalid != Pathea.PeCreature.Instance.mainPlayerId;
    }

    int mainPlayerId
    {
        get
        {
            return Pathea.PeCreature.Instance.mainPlayerId;
        }
    }

    Pathea.PassengerCmpt mMainPlayerPassenger = null;
    Pathea.PassengerCmpt mainPlayerPassenger
    {
        get
        {
            if(!HasMainPlayer())
            {
                return null;
            }

            if (mMainPlayerPassenger == null)
            {
                mMainPlayerPassenger = Pathea.PeCreature.Instance.mainPlayer.passengerCmpt;
            }

            return mMainPlayerPassenger;
        }
    }

    bool MainPlayerOnTrain()
    {
        if (null == mainPlayerPassenger)
        {
            return false;
        }

        return mainPlayerPassenger.IsOnRail;
    }

    void CheckMenu()
    {
        if (openMenuEnable && PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
        {
            if (null == RailwayPointGuiCtrl.Instance)
            {
                return;
            }

            RailwayPointGuiCtrl.Instance.SetInfo(point);
        }
    }

    void CheckPlayerTakeOnOrTakeOff()
    {
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem))
        {
            if (!GetGetOnGetOffEnable())
            {
                return;
            }

            if (MainPlayerOnTrain())
            {
                RailwayOperate.Instance.RequestGetOffTrain(mRoute.id, mainPlayerId, GetOffPos.position);
            }
            else
            {
                RailwayOperate.Instance.RequestGetOnTrain(mRoute.id, mainPlayerId);
            }
        }
    }

    bool rotating = false;
    Vector3 rotation = Vector3.zero;
    void RotUpDir(float angle)
    {
        Quaternion tranRot = Quaternion.Euler(rotation);
        Vector3 foward = tranRot * Vector3.forward;

        tranRot = Quaternion.AngleAxis(angle, foward) * tranRot;
        rotation = tranRot.eulerAngles;
    }

    void CheckRot()
    {
		if (isJoint && PeInput.Get (PeInput.LogicFunction.Item_RotateItemPress))
        {
            if (!rotating)
            {
                rotation = point.rotation;
            }

            RotUpDir(180f * Time.deltaTime);

            rotating = true;
        }
        else if (rotating)
        {
            rotating = false;
            RailwayOperate.Instance.RequestChangePointRot(mStation.Point.id, rotation);
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        mStation = GetComponent<RailwayStation>();
    }
    
    protected override void CheckOperate()
	{
		base.CheckOperate ();

        CheckMenu();

        CheckPlayerTakeOnOrTakeOff();

        CheckRot();
	}

    protected override bool CheckPick(Ray camMouseRay, out float dis)
    {
        if(base.CheckPick(camMouseRay, out dis))
        {
            if(dis < MouseOpRange)
            {
                mRoute = Railway.Manager.Instance.GetRoute(mStation.Point.routeId);

                MousePicker.Instance.UpdateTis();
                return true;
            }
        }

        return false;
    }

    bool GetGetOnGetOffEnable()
    {
        if (isJoint)
        {
            return false;
        }

        if (mRoute == null)
        {
            return false;
        }

        if (null == mRoute.train)
        {
            return false;
        }

        if (!HasMainPlayer())
        {
            return false;
        }

        if(Vector3.Distance(mRoute.train.transform.position, mStation.mJointPoint.position) > 1f)
        {
            return false;
        }

        if (Vector3.Distance(mRoute.train.transform.position, playerPos) > 10f)
        {
            return false;
        }

        return true;
    }

    protected override string tipsText
    {
		get
        {
			string showStr = mStation.Point.name;

            if (openMenuEnable)
            {
                showStr += "\n" + PELocalization.GetString(8000129);
            }

            if(GetGetOnGetOffEnable())
			{
                if (MainPlayerOnTrain())
                {
                    showStr += "\n" + PELocalization.GetString(8000131);
                }
                else
                {
                    showStr += "\n" + PELocalization.GetString(8000130);
                }
			}

            if (isJoint)
            {
                showStr += "\n" + PELocalization.GetString(8000150);
            }

			return showStr;
		}
	}
}
