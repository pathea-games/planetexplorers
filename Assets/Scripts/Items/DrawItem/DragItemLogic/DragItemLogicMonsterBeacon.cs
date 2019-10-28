using UnityEngine;
using System.Collections;
using System;

public class DragItemLogicMonsterBeacon : DragItemLogic
{
	public event Action DragItemActivateEvent;

	public override void OnActivate()
	{
        base.OnActivate();

        if (null != DragItemActivateEvent)
			DragItemActivateEvent();
	}
}
