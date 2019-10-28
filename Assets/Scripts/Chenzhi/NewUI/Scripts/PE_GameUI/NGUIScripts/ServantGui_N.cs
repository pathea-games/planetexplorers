using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;

/*
public class ServantGui_N : GUIWindowBase
{
	public Grid_N  mGridPrefab;
	
	public UILabel			mName;
	public UILabel			mLifeTex;
	public UILabel			mComfortTex;
	public UILabel			mOxygenTex;
	public UILabel			mShield;
	public UILabel			mEnergy;
	public UILabel			mAtk;
	public UILabel			mDef;
	
	public UITexture		mEqTex;
	
	public UILabel			mFirstBtnLabel;
	
	
	List<Grid_N> 	mEquipItemGrid = new List<Grid_N>();
	List<Grid_N> 	mUseItemGrid = new List<Grid_N>();
	List<Grid_N> 	mSkillGrid = new List<Grid_N>();
	
    //public NpcRandom mHero;
	
	bool mUpdateShowState = false;
	
	public override void InitWindow ()
	{
		base.InitWindow ();
		
		for(int i=0;i<10;i++)
		{
			mEquipItemGrid.Add(Instantiate(mGridPrefab) as Grid_N);
			mEquipItemGrid[i].transform.parent = this.transform;
			mEquipItemGrid[i].transform.localPosition = new Vector3(-111 + i/5*226,89 - i%5*54,-3);
			mEquipItemGrid[i].transform.localRotation = Quaternion.identity;
			mEquipItemGrid[i].transform.localScale = Vector3.one;
			mEquipItemGrid[i].SetItemPlace(ItemPlaceType.IPT_ServantEqu,i);
			mEquipItemGrid[i].SetGridMask((GridMask)(1<<i));
			mEquipItemGrid[i].onLeftMouseClicked = OnPickUpEqu;
			mEquipItemGrid[i].onRightMouseClicked = OnRemoveEquip;
			mEquipItemGrid[i].onDropItem = OnEquDrop;
		}
		
		
		for(int i=0;i<5;i++)
		{
			mUseItemGrid.Add(Instantiate(mGridPrefab) as Grid_N);
			mUseItemGrid[i].transform.parent = this.transform;
			mUseItemGrid[i].transform.localPosition = new Vector3(-380 + i*48,-79,-1);
			mUseItemGrid[i].transform.localRotation = Quaternion.identity;
			mUseItemGrid[i].transform.localScale = Vector3.one;
			mUseItemGrid[i].SetItemPlace(ItemPlaceType.IPT_ServantUse,i);
			mUseItemGrid[i].SetGridMask(GridMask.GM_Item);
			mUseItemGrid[i].onLeftMouseClicked = OnPickItem;
			mUseItemGrid[i].onRightMouseClicked = OnRemoveItem;
			mUseItemGrid[i].onDropItem = OnUseDrop;
		}
		
		for(int i=0;i<5;i++)
		{
			mSkillGrid.Add(Instantiate(mGridPrefab) as Grid_N);
			mSkillGrid[i].transform.parent = this.transform;
			mSkillGrid[i].transform.localPosition = new Vector3(-380 + i*48,-129,-1);
			mSkillGrid[i].transform.localRotation = Quaternion.identity;
			mSkillGrid[i].transform.localScale = Vector3.one;
			mSkillGrid[i].SetItemPlace(ItemPlaceType.IPT_ServantSkill,i);
			mSkillGrid[i].SetGridMask(GridMask.GM_Any);
		}
		HideWindow();
	}
	
	void Update ()
	{
        //if(mHero != null)
        //{
        //    mName.text = mHero.NpcName;
			
        //    mLifeTex.text = mHero.life.ToString() + "/" + mHero.maxLife.ToString();
        //    mComfortTex.text = Mathf.FloorToInt(mHero.comfort).ToString() + "/" + Mathf.FloorToInt(mHero.maxComfort).ToString();
        //    mOxygenTex.text = Mathf.FloorToInt(mHero.oxygen).ToString() + "/" + Mathf.FloorToInt(mHero.maxOxygen).ToString();
        //    mShield.text = "0/0";
        //    //mEnergy.text = "0/0";

        //    //ItemObject shield = mHero.ShieldEnergy;
        //    ItemObject energy = mHero.Battery;
        //    //if (null != shield)
        //    //{
        //    //    if (null != energy)
        //    //        mShield.text = ((int)shield.GetProperty(ItemProperty.ShieldEnergy)).ToString() + "/" + (int)shield.GetProperty(ItemProperty.ShieldMax);
        //    //    else
        //    //        mShield.text = "0/" + (int)shield.GetProperty(ItemProperty.ShieldMax);
        //    //}
        //    //else
        //    //{
        //    //    mShield.text = "0/0";
        //    //}

        //    if (null != energy)
        //    {
        //        //mEnergy.text = ((int)energy.GetProperty(ItemProperty.BatteryPower)).ToString() + "/" + (int)energy.GetProperty(ItemProperty.BatteryPowerMax);
        //    }
        //    else
        //    {
        //        mEnergy.text = "0/0";
        //    }

        //    mAtk.text = mHero.damage.ToString();
        //    mDef.text = mHero.defence.ToString();
        //    for (int i = 0; i < mUseItemGrid.Count; i++)
        //    {
        //        if (mUseItemGrid[i].ItemObj != null && mUseItemGrid[i].ItemObj.GetCount() == 0)
        //            mUseItemGrid[i].SetItem(null);
        //    }

        //    if (mHero.CanGainResource())
        //    {
        //        mFirstBtnLabel.transform.parent.gameObject.SetActive(true);
        //        if (mHero.IsGainResource())
        //        {
        //            mFirstBtnLabel.text = "rest";
        //        }
        //        else
        //        {
        //            mFirstBtnLabel.text = "work";
        //        }
        //    }
        //    else
        //    {
        //        mFirstBtnLabel.transform.parent.gameObject.SetActive(false);
        //    }

        //    mEqTex.mainTexture = mHero.GetModelViewTex();
        //}
        //else
        //{
        //    mName.text = "";
        //    mAtk.text = "0";
        //    mDef.text = "0";
        //}
	}

    //public override void AwakeWindow()
    //{
    //    base.AwakeWindow();

    //    if (null != mHero)
    //    {
    //        mHero.RebuildViewModel();
    //    }
    //}
	
//	public void UpdateHero()
//    {
//        if (!mInit)
//            InitWindow();
//
//        //mHero = PlayerFactory.mMainPlayer.m_MyHeroMap[m_HeroIdx].Hero;
//        //if(null != mHero)
//        //{
//        //    mEqTex.mainTexture = mHero.mBodyViewCam.GetTex();
//			
//        //    mFirstBtnLabel.text = "Rest";
//			
//        //    if(mHero.HasResourceSkill())
//        //        mFirstBtnLabel.text = "Work";
//	
//        //    if(mHero.m_WorkID > 0 && mHero.m_bRest)
//        //        mFirstBtnLabel.text = "ReCall";
//        //}
//    }
//	
//	void InsertEquipment(ItemObject item)
//    {
//		if(item == null)
//			return ;
//		
//        for (int i = 0; i < 10; i++)
//        {
//            if (((int)mEquipItemGrid[i].ItemMask & item.mItemData.m_Position) != 0)
//            {
//                mEquipItemGrid[i].SetItem(item);
//
//                if(i == 4 && ((int)GridMask.GM_TwoHandMask ^ item.mItemData.m_Position) == 0)
//                    mEquipItemGrid[9].SetItem(item);
//                return ;
//            }
//        }
//    }
	
    public void SetHeroIdx()
    {
        //mHero = hero;
        //UpdateServant(true);
    }
	
	public void UpdateServant(bool bUpdate)
	{
        for(int i=0; i<10; i++)
        {
            if(mEquipItemGrid[i].ItemObj != null)
                mEquipItemGrid[i].SetItem(null);
        }
		
        for(int i = 0; i<5; i++)
        {
            if(mUseItemGrid[i].ItemObj != null)
                mUseItemGrid[i].SetItem(null);
        }
		
		for(int i = 0; i<5; i++)
        	mSkillGrid[i].SetSkill(0,i);

        if(bUpdate)
        {
//            if(mHero == null)
//                return ;

//            int count = mHero.GetBagItemCount();
//            count = Mathf.Min(5, count);

//            for(int i=0; i<count; i++)
//            {
//                mUseItemGrid[i].SetItem(mHero.GetBagItem(i));
//            }
			
//            UpdateEquipShowState();
////		
////            for(int i=0; i<mHero.GetEquipmentCount(); i++)
////            {
////                InsertEquipment(mHero.GetEquipment(i));
////            }

//            if (mHero.mRandomSkillList == null)
//            {
//                return;
//            }

//            List<int> skillList = mHero.mRandomSkillList;
//            for (int i = 0; i < skillList.Count; i++)
//            {
//                Debug.Log(mHero.name+ "Add "+ skillList[i]);
//                SetSkill(skillList[i], i);
//            }
        }
	}
	
//	bool AddEqu(ItemObject item, int index)
//	{
//		if(item == null)
//            return false;
//		
//        if (null != mEquipItemGrid[index].ItemObj && item.mObjectID == mEquipItemGrid[index].ItemObj.mObjectID)
//            return false;
//
//        if (item.mItemData.mEquiSex != 0 && item.mItemData.mEquiSex != mHero.GetSex())
//            return false;
//
//        int otherHandIndex = (index == 4) ? 9 : 4;
////        bool bHadItem = false;
//	
//        ItemPackage pack = PlayerFactory.mMainPlayer.GetItemPackage();
//		
//		if((item.mItemData.m_Position ^ (int)GridMask.GM_TwoHandMask) == 0)
//		{
//            if ((mEquipItemGrid[otherHandIndex].ItemObj != null)
//				&& (mEquipItemGrid[otherHandIndex].ItemObj.mItemData.m_Position ^ (int)GridMask.GM_TwoHandMask) != 0 )
//            {
//				if(pack.GetEmptyGridIndex(1) != -1)
//				{
//					PlayerFactory.mMainPlayer.AddItem(mEquipItemGrid[otherHandIndex].ItemObj);
//					RemoveEqu(mEquipItemGrid[otherHandIndex].ItemObj, otherHandIndex);
//				}
//				else
//					return false;
//            }
//			
//	        if (mEquipItemGrid[index].ItemObj != null)
//	        {
//				RemoveEqu(mEquipItemGrid[index].ItemObj, index);
//	        }
//			mEquipItemGrid[index].SetItem(item);
//			mEquipItemGrid[otherHandIndex].SetItem(item);
//
//            mHero.PutOnEquip(mEquipItemGrid[index].ItemObj);
//		}		
//		else
//		{
//			if(mEquipItemGrid[otherHandIndex].ItemObj != null && (index == 4 || index == 9)
//				&&(mEquipItemGrid[otherHandIndex].ItemObj.mItemData.m_Position ^ (int)GridMask.GM_TwoHandMask) == 0)
//				mEquipItemGrid[otherHandIndex].SetItem(null);
//			
//			if(mEquipItemGrid[index].ItemObj != null)
//				RemoveEqu(mEquipItemGrid[index].ItemObj,index);
//			
//			mEquipItemGrid[index].SetItem(item);
//            mHero.PutOnEquip(mEquipItemGrid[index].ItemObj);
//		}
//		GameGui_N.Instance.mItemPackGui.ResetItem();
////        GameMainGUI.m_Pack.ResetItem();
//		
//		return true;
//	}
	
	void RemoveEqu(ItemObject item, int index)
	{
		if(item == null)
			return ;

        //mHero.TakeOffEquip(item);
	}
	
	public void OnPickUpEqu(Grid_N grid)
	{
		ActiveWnd();
		SelectItem_N.Instance.SetItem(grid.ItemObj,grid.ItemPlace,grid.ItemIndex);
	}
	
	public void OnPickItem(Grid_N grid)
	{
		ActiveWnd();
		SelectItem_N.Instance.SetItem(grid.ItemObj,grid.ItemPlace,grid.ItemIndex);
	}
	
	public void OnRemoveEquip(Grid_N grid)
	{
		ActiveWnd();
		if(null != grid.ItemObj)
		{
			if (GameConfig.IsMultiMode)
			{
                //mHero.NetWorkTakeOffEquip(grid.ItemObj);
			}
			else
			{
                //if (PlayerFactory.mMainPlayer.AddItem(grid.ItemObj))
                //{
                //    RemoveEquip(grid.ItemIndex);
                //    GameUI.Instance.mUIItemPackageCtrl.ResetItem();
                //}
			}
		}
	}

	public void OnRemoveItem(Grid_N grid)
	{
		if(null != grid.ItemObj)
		{
			if (GameConfig.IsMultiMode)
			{
                //mHero.NetWorkNpcDeleteItem(grid.ItemObj.instanceId);
			}
			else
			{
                //if (PlayerFactory.mMainPlayer.AddItem(grid.ItemObj))
                //{
                //    RemoveItem(grid.ItemIndex);
                //    GameUI.Instance.mUIItemPackageCtrl.ResetItem();
                //}
			}
		}
	}
	
	public void OnUseItem(Grid_N grid)
	{
		//if(GameConfig.IsMultiClient)
		//{
		//	PlayerFactory.mMainPlayer.AddItem(grid.Item);
		//	OnServantUseItem(grid.Item);
		//	GameGui_N.Instance.mItemPackGui.ResetItem();
		//}
		if (!GameConfig.IsMultiMode)
		{
            //PlayerFactory.mMainPlayer.AddItem(grid.ItemObj);
            //mHero.RemoveFromBag(grid.ItemObj);
			mUseItemGrid[grid.ItemIndex].SetItem(null);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		else
		{
            //mHero.NetWorkNpcUseItem(grid.ItemObj.instanceId);
		}
	}

	public void OnEquDrop(Grid_N grid)
	{
		//if(SelectItem_N.Instance.ItemObj == null || SelectItem_N.Instance.ItemObj.mItemData.mWeaponType == WeaponType.Bomb)
		//    return ;

        //if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar
        //    || ((int)grid.ItemMask & SelectItem_N.Instance.ItemObj.protoData.equipPos) == 0
        //    || (SelectItem_N.Instance.ItemObj.protoData.equipSex != 0 && SelectItem_N.Instance.ItemObj.protoData.equipSex != mHero.GetSex()))
        //    return;

        //if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar
        //    || !UIPlayerInfoCtrl.CanEquip(SelectItem_N.Instance.ItemObj, mHero.GetSex()))
        //{
        //    return;
        //}

        //if (GameConfig.IsMultiMode)
        //{
        //    mHero.NetWorkPutOnEquip(SelectItem_N.Instance.ItemObj, SelectItem_N.Instance.Place, grid.ItemIndex);
        //    SelectItem_N.Instance.SetItem(null);
        //}
        //else
        //{
        //    if (mHero.PutOnEquip(SelectItem_N.Instance.ItemObj))
        //    {
        //        SelectItem_N.Instance.RemoveOriginItem();
        //        SelectItem_N.Instance.SetItem(null);
        //    }
        //}
	}
	
	public void OnUseDrop(Grid_N grid)
	{
        //if(SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
        //    return;
		
        //if(!GameConfig.IsMultiMode)
        //{
        //    if(grid.ItemObj == null)
        //    {
        //        SelectItem_N.Instance.RemoveOriginItem();
        //        mUseItemGrid[grid.ItemIndex].SetItem(SelectItem_N.Instance.ItemObj);
        //        mHero.AddToBag(SelectItem_N.Instance.ItemObj);
        //        SelectItem_N.Instance.SetItem(null);
        //    }
        //}
        //else
        //{
        //    mHero.NetWorkNpcGetItem(SelectItem_N.Instance.ItemObj.instanceId, (int)SelectItem_N.Instance.Place);
        //    SelectItem_N.Instance.SetItem(null);
        //}
		
	}

    public void SyncItem()
    {
        //if (npc == mHero && IsOpen())
        //{
        //    for (int i = 0; i < mUseItemGrid.Count; i++)
        //        mUseItemGrid[i].SetItem(null);
        //    for (int i = 0; i < mHero.Package.Count; i++)
        //        mUseItemGrid[i].SetItem(mHero.Package[i]);
        //}
    }
	
	public void RemoveItem(int index)
	{
        //mHero.RemoveFromBag(mUseItemGrid[index].ItemObj);
		mUseItemGrid[index].SetItem(null);
	}
	
	public void RemoveEquip(int index)
	{
		RemoveEqu(mEquipItemGrid[index].ItemObj,index);
	}
	
	void OnFreeBtn()
	{
		
	}
	
	public void FreeHero()
    {
        //if(mHero == null)
        //    return ;

        //ChangeWindowShowState();

        //mHero.Dismiss();
    }

	void OnServantUseItem(ItemObject item)
	{
		//eg.  see Player.UseItem
	}
	
	void OnFirstBtn()
	{
        //if (mHero == null)
        //    return;

        //if (!mHero.IsFollower)
        //    return;

        //if (mHero.IsGainResource())
        //{
        //    mHero.ReturnGainResource();
        //}
        //else
        //{
        //    mHero.GoOutGainResource();
        //}
	}
	
	void OnSecBtn()
	{
        //if (null == mHero)
        //{
        //    return;
        //}

        //if (mHero.IsGainResource())
        //{
        //    mHero.ReturnGainResource();
        //}
        //else
        //{
        //    Vector3 pos = AiUtil.GetRandomPosition(PlayerFactory.mMainPlayer.GetPosition(), 2f, 6f, 20, AiManager.Manager.groundedLayer, 1);
        //    if (Vector3.zero != pos)
        //    {
        //        mHero.transform.position = pos + Vector3.up * 0.2f;
        //    }
        //    else
        //    {
        //        Debug.LogWarning("cant find call postion for npc:"+mHero.NpcName);
        //    }
        //}
	}
	
	void OnThirdBtn()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000031), FreeHero);
	}
	
	public void OnUseSkill(int id)
	{
		
	}
	
	void SetSkill(int skillID, int index)
	{
		if(index >= mSkillGrid.Count)
			return;

        EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillID));
        if (skill == null)
            return;


		mSkillGrid[index].SetSkill(skillID,index, skill.m_desc[0]);
	}
	
	public void UpdateEquipShowState()
	{
		if(!mUpdateShowState)
		{
			mUpdateShowState = true;
			Invoke("DoUpadateShowState", 0.1f);
		}
	}
	
	void DoUpadateShowState()
	{
        //mUpdateShowState = false;
        //if(null == mHero)
        //    return;
        //foreach(Grid_N grid in mEquipItemGrid)
        //{
        //    grid.SetItem(null);
        //}
        //foreach(ItemObject item in mHero.Equipments)
        //{
        //    ItemAsset.Equip equip = item.GetCmpt<ItemAsset.Equip>();
        //    for(int i = 0; i < 10; i++)
        //    {
        //        if(0 != (equip.equipPos & (int)mEquipItemGrid[i].ItemMask))
        //            mEquipItemGrid[i].SetItem(item);
        //    }
        //}
	}
}
*/