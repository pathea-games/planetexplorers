using ItemAsset.PackageHelper;
using System.Linq;
using Pathea;

public class DragItemMousePickItemList : DragItemMousePick
{
	ItemScript_ItemList mItemList;

	ItemScript_ItemList itemList
	{
		get
		{
			if (mItemList == null)
			{
				mItemList = GetComponent<ItemScript_ItemList>();
			}

			return mItemList;
		}
	}

	PlayerPackageCmpt mPkgCmpt = null;
	protected PlayerPackageCmpt pkgCmpt
	{
		get
		{
			if (null == mPkgCmpt)
				mPkgCmpt = PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
			
			return mPkgCmpt;
		}
	}

	public override void DoGetItem()
	{
		ItemAsset.MaterialItem[] items = itemList.GetItems();

		if(null == pkg || !pkg.CanAdd(items))
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return;
		}

		if (!GameConfig.IsMultiMode)
		{
			for (int i = 0; i < items.Length; ++i)
			{
				pkgCmpt.Add(items[i].protoId, items[i].count);
			}

			ItemAsset.ItemMgr.Instance.DestroyItem(itemObjectId);

			DragItemAgent agent = DragItemAgent.GetById(id);
			if (agent != null)
			{
				DragItemAgent.Destory(agent);
			}

			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		else
		{
			//CommonInterface target = GetComponentInChildren<CommonInterface>();
			//if (null == target || null == target.Netlayer)
			//    return;

			//if (null != target.OwnerView)
			//    PlayerFactory.mMainPlayer.RPCServer(EPacketType.PT_InGame_GetItemListBack, m_ItemID,
			//        itemObjectId, target.OwnerView.viewID, mItemIDList.ToArray(), mNumList.ToArray());

			if (null != PlayerNetwork.mainPlayer)
			{
				var getItems = items.Select(iter => new ItemAsset.ItemSample(iter.protoId, iter.count)).ToArray();
				PlayerNetwork.mainPlayer.RequestGetItemListBack(itemObjectId, getItems);
			}
		}

		GameUI.Instance.mItemOp.Hide();
	}
}
