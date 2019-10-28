using ItemAsset;
using System.Collections.Generic;
using UnityEngine;

public class PERailwayCtrl : MonoBehaviour
{
    static PERailwayCtrl mInstance;
    public static PERailwayCtrl Instance { get { return mInstance; } }

    UIMonoRailCtrl railwayView
    {
        get
        {
            return UIMonoRailCtrl.Instance;
        }
    }

    void Awake()
    {
        mInstance = this;
    }

	void Update()
	{
		if (!mInit && railwayView != null)
			Init();
	}

	bool mInit = false;
    void Init()
    {
		if (railwayView.isShow)
        {
			UpdateRailway();

            railwayView.e_BtnStop += OnDeleteRoute;
            railwayView.e_BtnStar += OnCreateRoute;
			railwayView.e_SetTrain += OnSetRouteTrain;
			railwayView.e_SetTrainToStation += OnSetTrainToStation;

            railwayView.e_ResetPointName += OnResetPointName;
            railwayView.e_ResetRouteName += OnResetRouteName;
            railwayView.e_ResetPointTime += OnResetPointTime;

			Railway.Manager.Instance.pointChangedEventor.Subscribe(PointChange);
			Railway.Manager.Instance.routeChangedEventor.Subscribe(RouteChanged);

			mInit = true;
        }
    }



	void RouteChanged(object sender,Railway.Manager.RouteChanged routeEventor)
	{
		if (routeEventor.bAdd)
			railwayView.AddMonoRail(routeEventor.route);
		else 
			railwayView.RemoveMonoRail(routeEventor.route);
		
		UpdateUnActiveLinks();
        //lz-2016.07.26 开启或关闭消息回来之后刷新选择的Station的UI信息
        railwayView.UpdateSelectedStation();
	}


	void PointChange(object sender,Railway.Manager.PointChanged pointEventor)
	{
		UpdateUnActiveLinks();
	}


    void UpdateRailway()
    {
        foreach (Railway.Route route in Railway.Manager.Instance.GetRoutes())
        {
            if (railwayView)
            {
                railwayView.AddMonoRail(route);
            }
        }
		UpdateUnActiveLinks();
    }

    void OnDeleteRoute(Railway.Point point)
    {
        Railway.Route route = Railway.Manager.Instance.GetRoute(point.routeId);
        if (null != route)
        {
            if (route.HasPassenger())
            {
                MessageBox_N.ShowOkBox(UIMsgBoxInfo.RailwayDeleteNotice.GetString());
                return;
            }

            RailwayOperate.Instance.RequestDeleteRoute(point.routeId);
        }
    }


    public void OnSetRouteTrain(int routeId, ItemObject trainItem)
    {
        if (trainItem == null)
        {
            return;
        }

        RailwayOperate.Instance.RequestSetRouteTrain(routeId, trainItem.instanceId);
    }

	public void OnSetTrainToStation(int routeId, int pointId)
	{
		RailwayOperate.Instance.RequestSetTrainToStation (routeId, pointId);
	}

    public void RemoveTrain(int packageIndex)
    {
        Railway.Point getPoint = railwayView.GetSelPoint();
        if (null != getPoint)
        {
            /*Railway.Route route = */Railway.Manager.Instance.GetRoute(getPoint.routeId);
        }
    }

    public void OnResetPointName(int pointID, string name)
    {
        RailwayOperate.Instance.RequestSetPointName(pointID, name);
    }


    public void OnResetRouteName(int routeID, string name)
    {
        RailwayOperate.Instance.RequestSetRouteName(routeID, name);
    }

    public void OnResetPointTime(int pointID, float time)
    {
        RailwayOperate.Instance.RequestSetPointStayTime(pointID, time);
    }


    void UpdateUnActiveLinks()
    {
        List<List<Railway.Point>> linkList = new List<List<Railway.Point>>();

        List<Railway.Point> pointList = new List<Railway.Point>();

        List<int> isolatePoints = Railway.Manager.Instance.GetIsolatePoint();
        foreach (int pointID in isolatePoints)
        {
            pointList.Add(Railway.Manager.Instance.GetPoint(pointID));
        }

        while (pointList.Count > 0)
        {
            List<Railway.Point> addList = new List<Railway.Point>();
            Railway.Point opPoint = pointList[0];
            addList.Add(opPoint);
            pointList.Remove(opPoint);

            Railway.Point findPoint = opPoint;
            while (Railway.Manager.InvalId != findPoint.prePointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.prePointId);
                if (findPoint.id == opPoint.id)
                    break;
                if (pointList.Remove(findPoint))
                    addList.Insert(0, findPoint);
            }

            findPoint = opPoint;
            while (Railway.Manager.InvalId != findPoint.nextPointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.nextPointId);
                if (findPoint.id == opPoint.id)
                    break;
                if (pointList.Remove(findPoint))
                    addList.Add(findPoint);
            }

            linkList.Add(addList);
        }

        if (railwayView != null)
        {
            railwayView.ReDrawDisRailLine(linkList);
        }
    }

    public static bool CheckRoute(Railway.Point point)
    {
        if (point.routeId != Railway.Manager.InvalId)
            return false;
        if (point.pointType == Railway.Point.EType.End)
        {
            Railway.Point findPoint = point;
            while (Railway.Manager.InvalId != findPoint.prePointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.prePointId);
                if (findPoint == point)
                    return false;
                if (findPoint.pointType == Railway.Point.EType.End)
                    return true;
            }

            findPoint = point;
            while (Railway.Manager.InvalId != findPoint.nextPointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.nextPointId);
                if (findPoint == point)
                    return false;
                if (findPoint.pointType == Railway.Point.EType.End)
                    return true;
            }
        }
        else
        {
            if (point.prePointId == Railway.Manager.InvalId || point.nextPointId == Railway.Manager.InvalId)
                return false;
            Railway.Point startPoint = null;
            Railway.Point endPoint = null;

            Railway.Point findPoint = point;
            while (Railway.Manager.InvalId != findPoint.prePointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.prePointId);
                if (findPoint == point)
                    return false;
                if (findPoint.pointType == Railway.Point.EType.End)
                {
                    startPoint = findPoint;
                    break;
                }
            }

            findPoint = point;
            while (Railway.Manager.InvalId != findPoint.nextPointId)
            {
                findPoint = Railway.Manager.Instance.GetPoint(findPoint.nextPointId);
                if (findPoint == point)
                    return false;
                if (findPoint.pointType == Railway.Point.EType.End)
                {
                    endPoint = findPoint;
                    break;
                }
            }

            return null != startPoint && null != endPoint;
        }
        return false;
    }

    public void OnCreateRoute(Railway.Point point, string routeName)
    {
        RailwayOperate.Instance.RequestCreateRoute(point.id, routeName);
    }

    public static bool HasRoute(Vector3 pos1, Vector3 pos2)
    {
        List<Railway.Point> pointList1 = Railway.Manager.Instance.GetNearPoint(pos1, 150f);
        List<Railway.Point> pointList2 = Railway.Manager.Instance.GetNearPoint(pos2, 150f);

        foreach (Railway.Point point1 in pointList1)
        {
            foreach (Railway.Point point2 in pointList2)
            {
                if (point1.pointType != Railway.Point.EType.Joint
                        && point2.pointType != Railway.Point.EType.Joint
                        && point1.routeId == point2.routeId
                        && point1.routeId != Railway.Manager.InvalId)
                {
                    Railway.Route route = Railway.Manager.Instance.GetRoute(point1.routeId);
                    if (null != route)
                        return route.trainId != Railway.Manager.InvalId;
                }
            }
        }
        return false;
    }

    public static void RemoveTrain(ItemObject trainItem)
    {

    }
}
