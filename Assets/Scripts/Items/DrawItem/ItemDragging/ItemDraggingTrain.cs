using UnityEngine;
using System.Collections;
using Pathea;

public class ItemDraggingTrain : ItemDraggingBase
{
	public override bool OnDragging(Ray cameraRay)
	{
		PEStationCtrl mousePickedStation = GetStation();
		if (null != mousePickedStation
		    && !mousePickedStation.isJoint
		    && PERailwayCtrl.CheckRoute(mousePickedStation.point)) 
		{
			rootGameObject.transform.position = mousePickedStation.station.mJointPoint.position;
			rootGameObject.transform.rotation = mousePickedStation.station.mJointPoint.rotation;
			return true;
		}

		base.OnDragging(cameraRay);

		return false;
	}

	public override bool OnPutDown ()
	{
		PEStationCtrl mousePickedStation = GetStation();
		if(null != mousePickedStation)
		{
			RailwayOperate.Instance.RequestAutoCreateRoute(mousePickedStation.point.id, itemObjectId);
		}
		return base.OnPutDown ();
	}

	PEStationCtrl GetStation()
	{
		PEStationCtrl mousePickedStation = null;
		if (null != MousePicker.Instance.curPickObj && !MousePicker.Instance.curPickObj.Equals (null)) 
			mousePickedStation = MousePicker.Instance.curPickObj as PEStationCtrl;
		return mousePickedStation;
	}


}
