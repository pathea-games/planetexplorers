using UnityEngine;
using System.Collections;

public class ItemDraggingFlag : ItemDraggingArticle
{
    public override bool OnPutDown()
    {
		if (GameConfig.IsMultiClient)
		{
			if (VArtifactUtil.IsInTownBallArea(transform.position))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}

			if (null != PlayerNetwork.mainPlayer)
				PlayerNetwork.mainPlayer.RequestDragFlag(itemDragging.itemObj.instanceId, transform.position, transform.rotation);

			return true;
		}

		return base.OnPutDown();
	}

}
