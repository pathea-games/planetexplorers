using UnityEngine;

public class ItemDraggingArticle : ItemDraggingBase
{
    public override bool OnPutDown()
    {
		if(!CheckMonsterBeaconEnable())
			return true;

		if (NetworkInterface.IsClient)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				IntVector3 safePos = new IntVector3(transform.position + 0.1f * Vector3.down);
				byte mTerrianType = VFVoxelTerrain.self.Voxels.SafeRead(safePos.x, safePos.y, safePos.z).Type;
				PlayerNetwork.mainPlayer.RequestDragOut(
					itemDragging.itemObj.instanceId,
					transform.position,
					transform.localScale,
					transform.rotation,
					mTerrianType);
			}
		}
		else
		{
			PutDown();
		}

        return base.OnPutDown();
    }

    protected void PutDown(bool isCreation = false)
    {
		ItemAsset.Drag dragging = itemDragging;

		//some item stack count > 1, seed eg.
		if (itemDragging.itemObj.stackCount > 1)
		{
			ItemAsset.ItemObject itemobj = ItemAsset.ItemMgr.Instance.CreateItem(itemDragging.itemObj.protoId);
			dragging = itemobj.GetCmpt<ItemAsset.Drag>();
		}

		/*DragArticleAgent agent = */
		DragArticleAgent.Create(dragging, transform.position, transform.localScale, transform.rotation, SceneMan.InvalidID, null, isCreation);

		RemoveFromBag();
	}

	bool CheckMonsterBeaconEnable()
	{
		ItemScript_MonsterBeacon monsterBeacon = GetComponent<ItemScript_MonsterBeacon>();
		if(null != monsterBeacon && EntityMonsterBeacon.IsRunning())
		{
			PeTipMsg.Register(PELocalization.GetString(82201076), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		return true;
	}
}
