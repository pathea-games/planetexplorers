using ItemAsset;
using System.Collections.Generic;
using UnityEngine;

public class DragItemMousePickPlant : DragItemMousePick 
{
	int			mGrowTimeIndex;
	bool		mIsDead = false;
	GameObject	mModel;
	
	byte        mTerrainType;
	
	public GameObject mLowerWaterTex;
	public GameObject mLowerCleanTex;
	
	const double VarPerOp = 30.0;

    FarmPlantLogic mPlant;
    FarmPlantLogic plant
    {
        get
        {
            if (mPlant == null)
            {
                mPlant = GetComponentInParent<FarmPlantLogic>();
            }

            return mPlant;
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        ItemScript_Plant plant = GetComponent<ItemScript_Plant>();
        if (plant != null)
        {
            plant.plantUpdated = PlantUpdated;
        }
    }

    void PlantUpdated()
    {
        CollectColliders();
        //UpdateCmdList();
    }

    protected override void InitCmd(CmdList cmdList)    
	{
        cmdList.Clear();
        if (plant == null)
        {
            return;
        }

        base.InitCmd(cmdList);

        if (!plant.mDead)
        {
            if (!plant.IsRipe)
            {
                cmdList.Remove("Get");

                if (plant.NeedWater)
                {
                    cmdList.Add("Water", OnWaterBtn);
                }

                if (plant.NeedClean)
                {
                    cmdList.Add("Clean", OnCleanBtn);
                }
            }
        }
        else
        {
            cmdList.Remove("Get");
        }
        cmdList.Add("Remove", OnClearBtn);
	}
	
	public override void DoGetItem ()
	{
		if (!GameConfig.IsMultiMode)
        {
            plant.UpdateStatus();
			int itemGetNum = (int)(((int)(plant.mLife / 20f) + 1) * 0.2f * plant.mPlantInfo.mItemGetNum);
			Dictionary<int, int> retItems = new Dictionary<int, int>();
			for (int i = 0; i < itemGetNum; i++)
			{
				float randomVar = Random.Range(0f, 1f);
				for (int j = 0; j < plant.mPlantInfo.mItemGetPro.Count; j++)
				{
					if (randomVar < plant.mPlantInfo.mItemGetPro[j].m_probablity)
					{
						if (!retItems.ContainsKey(plant.mPlantInfo.mItemGetPro[j].m_id))
							retItems[plant.mPlantInfo.mItemGetPro[j].m_id] = 0;
						retItems[plant.mPlantInfo.mItemGetPro[j].m_id] += 1;
					}
				}
			}

			List<MaterialItem> items = new List<MaterialItem>();
			foreach (int protoTypeId in retItems.Keys)
			{
				MaterialItem item = new MaterialItem();
				item.protoId = protoTypeId;
				item.count = retItems[protoTypeId];
				items.Add(item);
				//ItemSample addItem = new ItemSample(itemid, retItems[itemid]);
				//PlayerFactory.mMainPlayer.AddItem(addItem);
//				Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>().Add(protoTypeId, retItems[protoTypeId]);
			}

			if(null == pkg || !pkg.CanAdd(items))
			{
				PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
				return;
			}
			pkg.Add(items);

			FarmManager.Instance.RemovePlant(itemObjectId);

            //ItemMgr.Instance.DestroyItem(mItemObj.instanceId);
            DragArticleAgent.Destory(id);
			
			GameUI.Instance.mItemPackageCtrl.ResetItem();
            HideItemOpGui();
		}
		else 
		{
			if (null != PlayerNetwork.mainPlayer)
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_GetBack, itemObjectId);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
            HideItemOpGui();
        }

        //UpdateCmdList();
	}
	
	public void OnWaterBtn()
    {   
        if(!GameConfig.IsMultiMode)
		{	
            plant.UpdateStatus();
		    int needNum = (int)((plant.mPlantInfo.mWaterLevel[1] - plant.mWater)/VarPerOp);
            Pathea.PlayerPackageCmpt packageCmpt =Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
            int haveNum = packageCmpt.GetItemCount(ProtoTypeId.WATER);
		    if(haveNum <= 0)
		    {
			    MessageBox_N.ShowOkBox(PELocalization.GetString(8000090));
		    }
		    else 
		    {
				plant.mWater += VarPerOp * Mathf.Min(haveNum, needNum);
                packageCmpt.Destory(ProtoTypeId.WATER, Mathf.Min(haveNum, needNum));
                plant.UpdateStatus();
		    }
        }
        else
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_Water, plant.mPlantInstanceId);
        }
        HideItemOpGui();
        //UpdateCmdList();
	}
	
	public void OnCleanBtn()
    {
        if(!GameConfig.IsMultiMode)
		{
            plant.UpdateStatus();
		    int needNum = (int)((plant.mPlantInfo.mCleanLevel[1] - plant.mClean)/VarPerOp);
            Pathea.PlayerPackageCmpt packageCmpt = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
            int haveNum = packageCmpt.GetItemCount(ProtoTypeId.INSECTICIDE);
		    if( haveNum <= 0)
		    {
			    MessageBox_N.ShowOkBox(PELocalization.GetString(8000091));
		    }
		    else
		    {
				plant.mClean += VarPerOp * Mathf.Min(haveNum, needNum);
                packageCmpt.Destory(ProtoTypeId.INSECTICIDE, Mathf.Min(haveNum, needNum));
                plant.UpdateStatus();
		    }
        }
        else
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_Clean, plant.mPlantInstanceId);
        }
        HideItemOpGui();
        //UpdateCmdList();
	}

    public void OnClearBtn()
    {
        if (!plant.mDead)
        {
            MessageBox_N.ShowYNBox(UIMsgBoxInfo.mRemovePlantConfirm.GetString(), OnClear);
        }
        else
        {
            OnClear();
        }
    }

	public void OnClear()
	{
		if(!GameConfig.IsMultiMode)
		{
			FarmManager.Instance.RemovePlant(itemObjectId);

            //ItemMgr.Instance.DestroyItem(mItemObj.instanceId);
            DragArticleAgent.Destory(id);			
		}
		else
		{
			if (null != PlayerNetwork.mainPlayer)
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_Clear, plant.mPlantInstanceId);
		}
        HideItemOpGui();
        //UpdateCmdList();
	}

    //public override void Turn90Degree()
    //{
    //    base.Turn90Degree();
    //}

    protected override void CheckOperate()
	{
        base.CheckOperate();

		if(CanCmd())
		{
            if(!PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && PeInput.Get(PeInput.LogicFunction.InteractWithItem))
			{
				if(plant.mDead)
					OnClear();
				else if(plant.IsRipe)
					OnGetBtn();
				else if(plant.NeedWater)
					OnWaterBtn();
				else if(plant.NeedClean)
					OnCleanBtn();
				MousePicker.Instance.UpdateTis();
			}
		}
	}

    protected override string tipsText
    {
		get {
			if(mIsDead)
				return base.tipsText + "\n" + UIMsgBoxInfo.ClearPlant.GetString();
			if(plant.IsRipe)
				return base.tipsText + "\n" + UIMsgBoxInfo.GetPlant.GetString();
			if(plant.NeedWater)
				return base.tipsText + "\n" + UIMsgBoxInfo.WaterPlant.GetString();
			if(plant.NeedClean)
				return base.tipsText + "\n" + UIMsgBoxInfo.CleanPlant.GetString();
			return base.tipsText;
		}
	}
}
