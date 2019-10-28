using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;
using ItemAsset.PackageHelper;

public class DragItemMousePickTower : DragItemMousePick
{
	ItemScript_Tower mTower;

	ItemScript_Tower ammoTower
	{
		get
		{
			if(null == mTower)
			{
				mTower = GetComponent<ItemScript_Tower>();
				if(null != mTower)
					mTower.tower.onConsumeChange += UpdateAmmo;
			}

			return mTower;
		}
	}

	static DragItemMousePickTower gOperationTower;

    protected override void InitCmd(CmdList cmdList)
    {
        base.InitCmd(cmdList);
        if (CanRefill()){
			cmdList.Add("Refill", RefillByUI);
		}

		gOperationTower = this;
	}

    public bool CanRefill()
    {
        if (ammoTower == null)
            return false;

        return ammoTower.CanRefill();
    }

    public int GetMaxRefillNum()
    {
        if (ammoTower == null)
            return 0;

        if (pkg == null)
            return 0;

        int countInPkg = pkg.GetCount(ammoTower.ammoItemProtoId);
        int countNeed = ammoTower.ammoMaxCount - ammoTower.ammoCount;
        return countNeed > countInPkg  ? countInPkg : countNeed;
    }

	void RefillByUI()
	{
		OnRefill (GameUI.Instance.mItemOp.AmmoNum);
	}

	void OnRefill(int addAmmoNum)
	{
        if (addAmmoNum <= 0)
            return;

        if (ammoTower == null)
            return;

        if (pkg == null)
            return;

		int maxhas = GetMaxRefillNum();
		if(maxhas < addAmmoNum)
		{
			GameUI.Instance.mItemOp.SetRefill(ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());
			return;
		}

        if (!pkg.Destroy(ammoTower.ammoItemProtoId, addAmmoNum))
            return;

		if (GameConfig.IsMultiMode)
			PlayerNetwork.mainPlayer.RequestReload(mTower.tower.Entity.Id, mTower.itemObjectId, ammoTower.ammoItemProtoId, ammoTower.ammoItemProtoId, mTower.tower.ItemCountMax);

        ammoTower.Refill(addAmmoNum);
        //GameUI.Instance.mItemOp.Hide();
		GameUI.Instance.mItemOp.SetRefill(ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());		
		MousePicker.Instance.UpdateTis();
	}

    protected override void CheckOperate()
    {
        base.CheckOperate();
        if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd() && CanRefill())
        {
            if (ammoTower == null)
                return;

            GameUI.Instance.mItemOp.SetRefill(ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());
        }

		if (PeInput.Get (PeInput.LogicFunction.InteractWithItem) && CanCmd () && CanRefill ()) 
		{
			OnRefill(GetMaxRefillNum());
		}
    }

	public override void DoGetItem ()
	{
		if(null == pkg || null == itemObj || !pkg.CanAdd(itemObj))
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		PeMap.TowerMark findMask = PeMap.TowerMark.Mgr.Instance.Find(tower => itemObjectId == tower.ID);
		if(null != findMask)
		{
			PeMap.LabelMgr.Instance.Remove(findMask);
			PeMap.TowerMark.Mgr.Instance.Remove(findMask);
		}
		base.DoGetItem ();
	}

	void UpdateAmmo(float cost)
	{
		if (ammoTower == null)
			return;
		if (GameUI.Instance.mItemOp.Active && gOperationTower == this)
			GameUI.Instance.mItemOp.UpdateAmmoCount (ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());
	}

	/* //Test Death
	public void Update()
	{		
		if (Input.GetKey (KeyCode.CapsLock)) {
			Pathea.SkAliveEntity alive = GetComponent<Pathea.SkAliveEntity> ();
			if (alive != null)
				alive.SetAttribute(Pathea.AttribType.Hp, 0, false);
		}
	}
	*/

    protected override string tipsText
    {
        get
        {
            string tips = base.tipsText;

			if (ammoTower != null)
            {
                if (ammoTower.CostLimited())
                {
                    tips += "\n" + ammoTower.ammoCount + "/" + ammoTower.ammoMaxCount;
                }
                //lz-2016.11.04 增加耐久度显示
                tips += string.Format("\n{0} {1}/{2}", PELocalization.GetString(82220001), Mathf.FloorToInt(ammoTower.CurDurabilityValue), Mathf.FloorToInt(ammoTower.MaxDurabilityValue));
            }

            return tips;
        }
    }
}
