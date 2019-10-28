using UnityEngine;
using System.Collections;

public class ItemDraggingTower : ItemDraggingBase
{
    //private bool bPut = false;

	public GameObject headTips;

	public bool needVoxel;

    public override bool OnPutDown()
    {
		if (GameConfig.IsMultiClient)
		{
			if (!Pathea.PeGameMgr.IsMultiCoop && VArtifactUtil.IsInTownBallArea(transform.position))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}

			if (null != PlayerNetwork.mainPlayer)
				PlayerNetwork.mainPlayer.RequestDragTower(itemDragging.itemObj.instanceId, transform.position, transform.rotation);
		}
		else
		{
			DragTowerAgent towerAget = new DragTowerAgent(itemDragging, transform.position, transform.rotation);

			towerAget.Create();

			SceneMan.AddSceneObj(towerAget);
			PeMap.TowerMark towerMask = new PeMap.TowerMark();
			towerMask.position = transform.position;
			towerMask.ID = itemObjectId;
			towerMask.text = itemDragging.itemObj.protoData.GetName();
			towerMask.campId = Mathf.RoundToInt(Pathea.MainPlayer.Instance.entity.GetAttribute(Pathea.AttribType.CampID));
			PeMap.LabelMgr.Instance.Add(towerMask);
			PeMap.TowerMark.Mgr.Instance.Add(towerMask);
			RemoveFromBag();
		}

        return base.OnPutDown();
    }

    public override bool OnDragging(Ray ray)
    {
        bool flag = base.OnDragging(ray);

        rootGameObject.transform.position = BuildBlockManager.BestMatchPosition(rootGameObject.transform.position);

		CheckTreeAndSkEntity();

		UpdateHeadTips ();
		
        return flag && !mHasTree;
    }

    public override bool OnCheckPutDown()
    {
        //Collider[] col = Physics.OverlapSphere(transform.position, 1.5f, 1 << Pathea.Layer.AIPlayer);
        //int findCount = 0;

        //foreach (Collider c in col)
        //{
        //    if (null != c.gameObject.GetComponentInChildren<AiTower>())
        //    {
        //        return false;
        //    }
        //}

        //AiTower tower = rootGameObject.GetComponentInChildren<AiTower>();
        //AiTowerAmmo aTA = tower as AiTowerAmmo;
        //AiTowerPlayer aTP = tower as AiTowerPlayer;
        //if ((null != aTA) || (null != aTP && aTP.OnBuildTerrain))
        //{
        //    bPut = true;
        //    return true;
        //}

        return true;
    }

	void UpdateHeadTips()
	{
		bool onBlock = CheckOnBuildTerrain();
		
		if(headTips != null)
		{
			if (needVoxel && !onBlock)
				headTips.SetActive(true);
			else
				headTips.SetActive(false);
		}
	}

	bool CheckOnBuildTerrain()
	{
		//float blockLength = BuildBlockManager.MinBrushSize;
		float blockLength = BSBlock45Data.s_Scale;
		for (int x = -1; x <= 0; x++)
		{
			for (int z = -1; z <= 0; z++)
			{
				Vector3 worldPos = transform.TransformPoint(x * blockLength, -blockLength, z * blockLength);
				
				IntVector3 ipos = new IntVector3(Mathf.FloorToInt(worldPos.x * BSBlock45Data.s_ScaleInverted),
				                                 Mathf.FloorToInt(worldPos.y * BSBlock45Data.s_ScaleInverted),
				                                 Mathf.FloorToInt(worldPos.z * BSBlock45Data.s_ScaleInverted));
				B45Block block = Block45Man.self.DataSource.SafeRead(ipos.x, ipos.y, ipos.z);
				
				if (block.blockType >> 2 == 0)
					return false;
			}
		}
		
		return true;
	}
}
