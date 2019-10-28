using UnityEngine;
using System.Collections;
using ItemAsset;
using System.Collections.Generic;
using ItemAsset.PackageHelper;

public class RailwayOperate : Pathea.PeSingleton<RailwayOperate>
{
    static Pathea.PackageCmpt mPkg = null;
    static Pathea.PackageCmpt pkg
    {
        get
        {
            if (null == mPkg)
            {
                mPkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PackageCmpt>();
            }
            return mPkg;
        }
    }

    static bool AddItemToPlayerPkg(int instanceId)
    {
        if (null == pkg)
        {
            return false;
        }

        if (Pathea.PlayerPackageCmpt.LockStackCount)
        {
            ItemMgr.Instance.DestroyItem(instanceId);
        }

        ItemObject itemObj = ItemMgr.Instance.Get(instanceId);
        if (itemObj == null)
        {
            return false;
        }

        return pkg.Add(itemObj);
    }

    static void RemoveItemFromPlayerPkg(int itemId)
    {
        if (null == pkg)
        {
            return;
        }

        ItemObject itemObj = ItemMgr.Instance.Get(itemId);
        if (itemObj == null)
        {
            return;
        }

        pkg.Remove(itemObj);

        

        if (Pathea.PlayerPackageCmpt.LockStackCount)
        {
            Pathea.PlayerPackageCmpt playerPkg = pkg as Pathea.PlayerPackageCmpt;
            if (playerPkg != null)
            {
                playerPkg.package.Add(itemObj.protoId, 1);
            }
        }
    }

    #region Railway interface
    public void RequestAddPoint(Vector3 pos, Railway.Point.EType type, int prePointId, int itemObjId)
    {
		RemoveItemFromPlayerPkg(itemObjId);
		Railway.Point point = DoAddPoint(pos, type, prePointId);
		if (point != null)
		{
			point.itemInstanceId = itemObjId;
		}
	}

    public Railway.Point DoAddPoint(Vector3 pos, Railway.Point.EType type, int prePointId, int pointId = -1)
    {
        Railway.Point point = Railway.Manager.Instance.GetPoint(prePointId);

        // Reverse Link 
        if (null != point
            && Railway.Manager.InvalId == point.prePointId
            && Railway.Manager.InvalId != point.nextPointId)
        {
            Railway.Point.ReverseNext(point);
        }

        return Railway.Manager.Instance.AddPoint(pos, prePointId, type, pointId);
    }

    public void RequestRemovePoint(int pointID)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_Recycle, pointID);
            }
        }
        else
        {
            Railway.Point removePoint = Railway.Manager.Instance.GetPoint(pointID);
            if (null != removePoint)
            {
                AddItemToPlayerPkg(removePoint.itemInstanceId);
                DoRemovePoint(pointID);
            }
        }
    }

    public void DoRemovePoint(int pointID)
    {
        Railway.Manager.Instance.RemovePoint(pointID);
    }

    public void RequestChangePrePoint(int pointId, int preID)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_PrePointChange, pointId, preID);
        }
        else
        {
            DoChangePrePoint(pointId, preID);
        }
    }

    public void DoChangePrePoint(int pointId, int preID)
    {
        Railway.Point point = Railway.Manager.Instance.GetPoint(pointId);

        Railway.Point findPoint = Railway.Manager.Instance.GetPoint(preID);

        // Inverse Link 
        if (null != findPoint
            && Railway.Manager.InvalId == findPoint.prePointId
            && Railway.Manager.InvalId != findPoint.nextPointId)
        {
            List<Railway.Point> pointList = new List<Railway.Point>();
            pointList.Add(point);
            pointList.Insert(0, findPoint);
            while (Railway.Manager.InvalId != findPoint.nextPointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.nextPointId);
                pointList.Insert(0, findPoint);
            }

            pointList[0].ChangePrePoint(Railway.Manager.InvalId);

            for (int i = 0; i < pointList.Count - 1; i++)
            {
                pointList[i].ChangeNextPoint(pointList[i + 1].id);
            }
        }
        else
        {
            point.ChangePrePoint(preID);
        }
		
		if(null != Railway.Manager.Instance.pointChangedEventor)
			Railway.Manager.Instance.pointChangedEventor.Dispatch(new Railway.Manager.PointChanged(){ bAdd = false, point = point});
    }

    public void RequestChangeNextPoint(Railway.Point point, int nextID)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_NextPointChange, point.id, nextID);
        }
        else
        {
            DoChangeNextPoint(point, nextID);
        }
    }

    public void DoChangeNextPoint(Railway.Point point, int nextID)
    {
        Railway.Point findPoint = Railway.Manager.Instance.GetPoint(nextID);

        if (null != findPoint
            && Railway.Manager.InvalId != findPoint.prePointId
            && Railway.Manager.InvalId == findPoint.nextPointId)
        {
            List<Railway.Point> pointList = new List<Railway.Point>();
            pointList.Add(point);
            pointList.Add(findPoint);
            while (Railway.Manager.InvalId != findPoint.prePointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.prePointId);
                pointList.Add(findPoint);
            }
            for (int i = 0; i < pointList.Count - 1; i++)
                pointList[i].ChangeNextPoint(pointList[i + 1].id);
            pointList[pointList.Count - 1].ChangeNextPoint(Railway.Manager.InvalId);
        }
        else
        {
            point.ChangeNextPoint(nextID);
        }

		if(null != Railway.Manager.Instance.pointChangedEventor)
			Railway.Manager.Instance.pointChangedEventor.Dispatch(new Railway.Manager.PointChanged(){ bAdd = false, point = point});
    }

    public void RequestCreateRoute(int pointId, string routeName)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_Route, pointId, routeName);
        }
        else
        {
            DoCreateRoute(pointId, routeName);
        }
    }

    public bool IsPointInCompletedLine(int pointId)
    {
        Railway.Point point = Railway.Manager.Instance.GetPoint(pointId);

        if (point == null)
        {
            return false;
        }

        Railway.Point header = Railway.Point.GetHeader(point);
        if(null == header)
        {
            return false;
        }

        if (header.pointType != Railway.Point.EType.End)
        {
            return false;
        }

        Railway.Point tail = Railway.Point.GetTail(point);
        if(null == tail)
        {
            return false;
        }

        if (tail.pointType != Railway.Point.EType.End)
        {
            return false;
        }

        bool ret = true;
        Railway.Point.Travel(header, (p) =>
        {
            if(p != header
                && p != tail
                && p.pointType == Railway.Point.EType.End
                )
            {
                ret = false;
            }
			if(p.routeId != Railway.Manager.InvalId)
			{
				ret = false;
			}
		});

        return ret;
    }

    public Railway.Route DoCreateRoute(int pointId, string routeName)
    {
        if (!IsPointInCompletedLine(pointId))
        {
            return null;
        }

        Railway.Point point = Railway.Manager.Instance.GetPoint(pointId);

        if (point == null)
        {
            return null;
        }

        Railway.Point header = Railway.Point.GetHeader(point);
        if (header.pointType != Railway.Point.EType.End)
        {
            return null;
        }

        List<int> pointList = new List<int>();

        point = header;
        while (true)
        {
            if (null != point)
            {
                pointList.Add(point.id);

                point = point.GetNextPoint();
            }
            else
            {
                break;
            }
        }

        return Railway.Manager.Instance.CreateRoute(routeName, pointList.ToArray());
    }

    public void RequestDeleteRoute(int routeId)
    {
        Railway.Route route = Railway.Manager.Instance.GetRoute(routeId);
        if (null == route)
        {
            return;
        }

        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_DeleteRoute, routeId);
            }
        }
        else
        {
            AddItemToPlayerPkg(route.trainId);

            DoDeleteRoute(routeId);
        }
    }

    public bool DoDeleteRoute(int routeId)
    {
        Railway.Route route = Railway.Manager.Instance.GetRoute(routeId);
        if (null == route)
        {
            return false;
        }

		route.SetTrain(Pathea.IdGenerator.Invalid);

        return Railway.Manager.Instance.RemoveRoute(route.id);
    }

	public bool DoGetOnTrain(int entityId, int routeId, bool checkState = true)
    {
        Pathea.PeEntity entity = Pathea.EntityMgr.Instance.Get(entityId);
        if (null == entity)
        {
            Debug.LogError("cant find entity:" + entityId);
            return false;
        }

        Pathea.PassengerCmpt passenger = entity.GetCmpt<Pathea.PassengerCmpt>();
        if (null == passenger)
        {
            Debug.LogError("no Pathea.RailPassengerCmpt");
            return false;
        }

		return passenger.GetOn(routeId, checkState);
    }

    public bool RequestGetOnTrain(int routeId, int entityId)
    {
        if (!GameConfig.IsMultiMode)
        {
            return DoGetOnTrain(entityId, routeId);
        }
        else
        {
			Pathea.PeEntity entity = Pathea.EntityMgr.Instance.Get(entityId);
			if (null == entity)
			{
				Debug.LogError("cant find entity:" + entityId);
				return false;
			}
			
			Pathea.MotionMgrCmpt mmc = entity.GetCmpt<Pathea.MotionMgrCmpt>();
			if (null == mmc)
			{
				Debug.LogError("no Pathea.RailPassengerCmpt");
				return false;
			}
			if(mmc.CanDoAction(Pathea.PEActionType.GetOnTrain))
			{
	            if (null != PlayerNetwork.mainPlayer)
	            {
	                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_GetOnTrain, routeId, entityId);
	            }
			}

            return true;
        }

    }

    public bool DoGetOffTrain(int routeId, int entityId, Vector3 pos)
    {
        Pathea.PeEntity entity = Pathea.EntityMgr.Instance.Get(entityId);
        if (null == entity)
        {
            Debug.LogError("cant find entity:" + entityId);
            return false;
        }

        Pathea.PassengerCmpt passenger = entity.GetCmpt<Pathea.PassengerCmpt>();
        if (null == passenger)
        {
            Debug.LogError("no Pathea.RailPassengerCmpt");
            return false;
        }

        if (passenger.railRouteId != routeId)
        {
            return false;
        }

        return passenger.GetOff(pos);
    }

    public void RequestGetOffTrain(int routeId, int passengerID, Vector3 pos)
    {
        if (!GameConfig.IsMultiMode)
        {
            DoGetOffTrain(routeId, passengerID, pos);
        }
        else
        {
            if (null != PlayerNetwork.mainPlayer)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_GetOffTrain, routeId, passengerID, pos);
            }
        }
    }

    public void RequestSetRouteTrain(int routeId, int itemObjId)
    {
        if (!GameConfig.IsMultiMode)
        {
            if (DoSetRouteTrain(routeId, itemObjId))
            {
                RemoveItemFromPlayerPkg(itemObjId);
            }
        }
        else
        {
            if (null != PlayerNetwork.mainPlayer)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_SetRouteTrain, routeId, itemObjId);
            }
        }
    }

	public void RequestSetTrainToStation(int routeId, int pointId)
	{
		if (!GameConfig.IsMultiMode)
		{
			DoSetTrainToStation(routeId, pointId);
		}
		else
		{

		}
	}

    public void RequestAutoCreateRoute(int pointID, int itemObjID)
    {
        if (!GameConfig.IsMultiMode)
        {
            DoAutoCreateRoute(pointID, itemObjID);
        }
        else
        {
            if (null != PlayerNetwork.mainPlayer)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_AutoCreateRoute, pointID, itemObjID);
            }
        }
    }

    public void DoAutoCreateRoute(int pointID, int itemObjID)
    {
        Railway.Point point = Railway.Manager.Instance.GetPoint(pointID);
        DoCreateRoute(pointID, "");
        DoSetRouteTrain(point.routeId, itemObjID);
		if (DoSetRouteTrain(point.routeId, itemObjID))
			RemoveItemFromPlayerPkg(itemObjID);
        DoSetTrainToStation(point.routeId, pointID);
    }


    public void DoSetTrainToStation(int routeId, int pointId)
	{
		Railway.Route route = Railway.Manager.Instance.GetRoute (routeId);
		if (null != route) 
		{
			route.SetTrainToStation(pointId);
		}
	}

    public bool DoSetRouteTrain(int routeId, int trainItemObjId)
    {
        Railway.Route route = Railway.Manager.Instance.GetRoute(routeId);

        if (null == route)
        {
            return false;
        }
		route.SetTrain(trainItemObjId);
        return true;
    }

    public void RequestChangePointRot(int pointID, Vector3 rot)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ChangeStationRot, pointID, rot);
            }
        }
        else
        {
            DoChangePointRot(pointID, rot);
        }
    }

    public void DoChangePointRot(int pointID, Vector3 rot)
    {
        Railway.Point point = Railway.Manager.Instance.GetPoint(pointID);
        if (null != point)
        {
            point.rotation = rot;
        }
    }

    public void DoRemovePassenger(int type, int passengerID)
    {
        //foreach (Railway.Route route in Railway.Manager.Instance.RouteDic.Values)
        //{
        //    if (route.Train != null)
        //    {
        //        route.Train.ClearPassenger();
        //    }
        //}
    }

    public void RequestSetPointName(int pointID, string name)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ResetPointName, pointID, name);
        }
        else
        {
            DoResetPointName(pointID, name);
        }
    }

    public void DoResetPointName(int pointID, string name)
    {
        Railway.Point point = Railway.Manager.Instance.GetPoint(pointID);
        if (null != point)
        {
            point.name = name;
        }
    }


    public void RequestSetRouteName(int routeID, string name)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ResetRouteName, routeID, name);
        }
        else
        {
            DoResetRouteName(routeID, name);
        }
    }

    public void DoResetRouteName(int routeID, string name)
    {
        Railway.Route route = Railway.Manager.Instance.GetRoute(routeID);
        if (null != route)
        {
            route.name = name;
        }
    }

    public void RequestSetPointStayTime(int pointID, float time)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ResetPointTime, pointID, time);
        }
        else
        {
            DoResetPointTime(pointID, time);
        }
    }

    public void DoResetPointTime(int pointID, float time)
    {

        Railway.Point point = Railway.Manager.Instance.GetPoint(pointID);
        if (point == null)
        {
            return;
        }

        point.stayTime = time;
    }

    public void DoSyncRunState(int routeId,int moveDir,int nextPoint,float time)
    {
        Railway.Route route = Railway.Manager.Instance.GetRoute(routeId);
        if(route != null)
        {
            route.mRunState.SyncRunState(moveDir, nextPoint, time);
        }
    }
    #endregion
}
