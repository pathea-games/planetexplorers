using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using CustomData;
using SkillAsset;
using ItemAsset;
public partial class PlayerNetwork
{

	void RPC_S2C_Railway_AddPoint(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3> ();
		int type = stream.Read<int> ();
		int prePointId = stream.Read<int> ();
        RailwayOperate.Instance.DoAddPoint(pos, (Railway.Point.EType)type, prePointId);
	}

	void RPC_S2C_Railway_PrePointChange(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int pointId = stream.Read<int>();
		int prePointId = stream.Read<int> ();

        RailwayOperate.Instance.DoChangePrePoint(pointId, prePointId);
	}

	void RPC_S2C_Raileway_NextPointChange(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int pointId = stream.Read<int>();
		int nextPointId = stream.Read<int> ();
		Railway.Point mOpPoint = Railway.Manager.Instance.GetPoint (pointId);
		if(mOpPoint != null)
            RailwayOperate.Instance.DoChangeNextPoint(mOpPoint, nextPointId);
	}

	void RPC_S2C_Railway_Recycle(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int pointId = stream.Read<int>();
        RailwayOperate.Instance.DoRemovePoint(pointId);
	}

	void RPC_S2C_Railway_Route(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int pointId = stream.Read<int>();
		string name = stream.Read<string>();
        int moveDir = stream.Read<int>();
        float leaveTime = stream.Read<float>();

        Railway.Route route = RailwayOperate.Instance.DoCreateRoute(pointId, name);
        if(route != null)
        {
            route.moveDir = moveDir;
            route.TimeToLeavePoint = leaveTime;
        }
	}

	void RPC_S2C_Railway_GetOnTrain(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int routeId = stream.Read<int>();
		int passengerID = stream.Read<int>();

        RailwayOperate.Instance.DoGetOnTrain(passengerID, routeId, false);
	}
	
	void RPC_S2C_Railway_GetOffTrain(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int routeId = stream.Read<int>();
		int passengerID = stream.Read<int>();
		Vector3 pos =  stream.Read<Vector3>();

        RailwayOperate.Instance.DoGetOffTrain(routeId, passengerID, pos);
	}

	void RPC_S2C_Railway_DeleteRoute(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>();
        RailwayOperate.Instance.DoDeleteRoute(id);
	}

	void RPC_S2C_Railway_SetRouteTrain(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int routeId = stream.Read<int>();
		int newId = stream.Read<int>();

        RailwayOperate.Instance.DoSetRouteTrain(routeId, newId);
		
	}

	void RPC_S2C_Railway_ChangeStationRot(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int pointId = stream.Read<int>();
		Vector3 rot = stream.Read<Vector3>();
        RailwayOperate.Instance.DoChangePointRot(pointId, rot);
	}

	void RPC_S2C_Railway_GetOffTrainEx(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int type = stream.Read<int>();
		int passengerID = stream.Read<int>();
        RailwayOperate.Instance.DoRemovePassenger(type, passengerID);

    }
	void RPC_S2C_Railway_ResetPointName(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>();
		string pointName = stream.Read<string>();
        RailwayOperate.Instance.DoResetPointName(pointID, pointName);     
	}
	void RPC_S2C_Railway_ResetRouteName(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int routeID = stream.Read<int>();
		string routeName = stream.Read<string>();
        RailwayOperate.Instance.DoResetRouteName(routeID, routeName);       
	}
	void RPC_S2C_Railway_ResetPointTime(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>();
		float time = stream.Read<float>();
        RailwayOperate.Instance.DoResetPointTime(pointID, time);             
	}
    void RPC_S2C_Railway_AutoCreateRoute(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int pointID = stream.Read<int>();
        int time = stream.Read<int>();
        RailwayOperate.Instance.DoAutoCreateRoute(pointID, time);
    }
    void RPC_S2C_Railway_UpdateRoute(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int routeId = stream.Read<int>();
        int moveDir = stream.Read<int>();
        int next = stream.Read<int>();
        float time = stream.Read<float>();
        RailwayOperate.Instance.DoSyncRunState(routeId, moveDir, next, time);
    }
}

