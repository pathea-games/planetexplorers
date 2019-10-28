using UnityEngine;
using System.Collections;

public class DragItemMousePickKickStarter : MousePickableChildCollider
{
	protected override void CheckOperate ()
	{
		if(PeInput.Get(PeInput.LogicFunction.OpenItemMenu) || PeInput.Get(PeInput.LogicFunction.InteractWithItem))
		{
			GameUI.Instance.mKickstarterCtrl.Show();
		}
	}
}
