using UnityEngine;
using System.Collections;
using Pathea;

public class ItemDraggingWareHouse : ItemDraggingBase 
{
	public int doodadID = 143;

	public override bool OnPutDown()
	{
		if(!PeGameMgr.IsMulti)
		{
			DoodadEntityCreator.CreateRandTerDoodad(doodadID, transform.position, Vector3.one, transform.rotation);
			RemoveFromBag();
		}
		else
		{
			IntVector3 safePos = new IntVector3(transform.position + 0.1f * Vector3.down);

			if (VArtifactUtil.IsInTownBallArea(safePos))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}

			if (null != PlayerNetwork.mainPlayer)
			{
				byte mTerrianType = VFVoxelTerrain.self.Voxels.SafeRead(safePos.x, safePos.y, safePos.z).Type;
				PlayerNetwork.mainPlayer.RequestDragOut(itemDragging.itemObj.instanceId, transform.position, transform.localScale, transform.rotation, mTerrianType);
			}
		}

		return base.OnPutDown();
	}
}
