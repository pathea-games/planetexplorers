using UnityEngine;

public class ItemScript_Tower : ItemScript
{
    Pathea.TowerCmpt mTower = null;
	Pathea.SkAliveEntity mAlive = null;
	ItemAsset.Tower mItemTower = null;
	ItemAsset.Energy mItemEnergy = null;
	ItemAsset.LifeLimit mLifeLimit = null;
	ItemAsset.Durability mDurability = null;

    public Pathea.TowerCmpt tower {
		get{
            if (mTower == null)
			{
                mTower = GetComponent<Pathea.TowerCmpt>();
				if(null != mTower)
				{
					mTower.onConsumeChange += OnAttrChanged;
				}
            }
            return mTower;
        }
    }

	public ItemAsset.Energy energy
	{
		get
		{
			if(null == mItemEnergy)
				mItemEnergy = mItemObj.GetCmpt<ItemAsset.Energy>();
			return mItemEnergy;
		}
	}

	public Pathea.SkAliveEntity alive{
		get{
			if (mAlive == null)
			{
				mAlive = GetComponent<Pathea.SkAliveEntity>();
			}			
			return mAlive;
		}
	}

	public override void SetItemObject (ItemAsset.ItemObject itemObj)
	{
		base.SetItemObject (itemObj);
		
		mItemTower = mItemObj.GetCmpt<ItemAsset.Tower>();

		mItemEnergy = mItemObj.GetCmpt<ItemAsset.Energy>();

		mLifeLimit = mItemObj.GetCmpt<ItemAsset.LifeLimit>();
		
		mDurability = mItemObj.GetCmpt<ItemAsset.Durability>();

		if(!Pathea.PeGameMgr.IsMulti && null != tower && null != mLifeLimit)
			tower.Entity.SetAttribute(Pathea.AttribType.Hp, mLifeLimit.floatValue.current);
	}

    void Start()
    {
        RegisterDeadEvent();
        LoadFromItem();
    }

	/* test code
	void Update()
	{
		if (Input.GetKey (KeyCode.P)) {
			alive.SetAttribute(Pathea.AttribType.Hp, 5, false);
		}
	}
	*/


    void OnDestroy()
    {
        OnAttrChanged();		
		if(null != mTower)
			mTower.onConsumeChange -= OnAttrChanged;
    }

    void RegisterDeadEvent()
    {
        if (alive == null)
        {
            return;
        }
		alive.onHpChange += OnHpChange;
		alive.deathEvent += OnDeath;
    }

	void OnHpChange(SkillSystem.SkEntity caster, float hpChange)
	{
		float hp = alive.GetAttribute(Pathea.AttribType.Hp);
		if (mItemObj != null) 
		{
			if(mLifeLimit != null){
				mLifeLimit.floatValue.current = hp;
			}
			if(mDurability != null){
				mDurability.floatValue.current = hp;
			}
		}
	}
	void OnDeath(SkillSystem.SkEntity caster, SkillSystem.SkEntity target)
	{
		PeMap.TowerMark findMask = PeMap.TowerMark.Mgr.Instance.Find(tower => itemObjectId == tower.ID);
		if(null != findMask)
		{
			PeMap.LabelMgr.Instance.Remove(findMask);
			PeMap.TowerMark.Mgr.Instance.Remove(findMask);
		}
		Invoke("TowerDestroy", 10f);
	}
    void TowerDestroy()
    {
        ItemAsset.ItemMgr.Instance.DestroyItem(itemObjectId);
        DragArticleAgent.Destory(id);
    }

	void OnAttrChanged(float cost = 0)
    {
        if (mItemObj == null)
            return;

        if (mItemTower == null)
            return;

		if (tower.ConsumeType == Pathea.ECostType.Item)
        {
            mItemTower.curCostValue = tower.ItemCount;
        }
        else if (tower.ConsumeType == Pathea.ECostType.Energy && mItemEnergy != null)
        {
			mItemEnergy.energy.current = tower.EnergyCount;
        }

		if (GameConfig.IsMultiMode && cost != 0)
			StartCoroutine(DelayRequestAttrChanged(cost));
    }

	int lastReqeustTime = 0;
	float totalCost = 0;
	System.Collections.IEnumerator DelayRequestAttrChanged(float cost)
	{
		totalCost += cost;
		if (0 != lastReqeustTime)
			yield break;

		lastReqeustTime = 3;

		while (0 != lastReqeustTime)
		{
			yield return new WaitForSeconds(1f);
			lastReqeustTime--;
		}

		if (null != PlayerNetwork.mainPlayer)
			PlayerNetwork.mainPlayer.RequestAttrChanged(tower.Entity.Id, itemObjectId, totalCost, ammoItemProtoId);

		totalCost = 0;
	}

    void LoadFromItem()
    {
        if (mItemObj == null)
        {
            return;
        }

        if (mItemTower == null)
        {
            return;
        }

		if (mItemTower.costType == Pathea.ECostType.Item)
        {
            tower.ItemCount = mItemTower.curCostValue;
        }
		else if (mItemTower.costType == Pathea.ECostType.Energy && mItemEnergy != null)
        {
			if(mItemEnergy.energy.current == -1)
				mItemEnergy.energy.SetToMax();
			tower.EnergyCount = (int)mItemEnergy.energy.current;
        }
    }

    public int ammoItemProtoId
    {
        get
        {
            return tower.ItemID;
        }
    }

    public int ammoCount
    {
        get
        {
            if (tower.CostType == Pathea.ECostType.Item)
            {
                return tower.ItemCount;
            }
            else if (tower.CostType == Pathea.ECostType.Energy)
            {
                return tower.EnergyCount;
            }
            else
            {
                return int.MaxValue;
            }
        }
    }

    public int ammoMaxCount
    {
        get
        {
            if (tower.CostType == Pathea.ECostType.Item)
            {
                return tower.ItemCountMax;
            }
            else if (tower.CostType == Pathea.ECostType.Energy)
            {
                return tower.EnergyCountMax;
            }
            else
            {
                return int.MaxValue;
            }
        }
    }

    public bool CostLimited()
    {
        if (tower.ConsumeType == Pathea.ECostType.Item
            || tower.ConsumeType == Pathea.ECostType.Energy)
        {
            return true;
        }

        return false;
    }

    public bool CanRefill()
    {
        if (tower.ConsumeType != Pathea.ECostType.Item)
        {
            return false;
        }

        return true;
    }

    public void Refill(int number)
    {
        if (!CanRefill())
        {
            return;
        }
        tower.ItemCount += number;		
		OnAttrChanged();
    }

    //lz-2016.11.04 tipsText 显示耐久度使用
    public float CurDurabilityValue
    {
        get { return null == mDurability ? (null==mLifeLimit ?0:mLifeLimit.floatValue.current) : mDurability.floatValue.current; }
    }

    public float MaxDurabilityValue
    {
        get { return null == mDurability ? (null == mLifeLimit ? 0 : mLifeLimit.valueMax)  : mDurability.valueMax; }
    }
}