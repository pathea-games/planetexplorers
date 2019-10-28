/*
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using SkillAsset;

public class PlayerEquipGui_N : GUIWindowBase
{
	int EquipPart = 10;
	
	public Grid_N  		mPrefab;
	
	public UITexture	mEqTex;
	
	public UICheckbox	mHatShowEnable;
	
	ViewCameraControler mBodyViewCtr;
	ViewCameraControler mHeadViewCtr;
	
	List<Grid_N> mEquipment = new List<Grid_N>();
	
	Player mPlayer;
	
	GameObject mPlayerObj;
	
	bool mUpdateShowState = false;
	
	public override void InitWindow ()
	{
		base.InitWindow ();
		for(int i=0;i<10;i++)
		{
			mEquipment.Add(Instantiate(mPrefab) as Grid_N);
			mEquipment[i].gameObject.name = "HotKey" + i;
			mEquipment[i].transform.parent = this.transform;
			mEquipment[i].transform.localPosition = new Vector3(-111 + i/(EquipPart/2)*226,89 - i%(EquipPart/2)*54,-3);
			mEquipment[i].transform.localRotation = Quaternion.identity;
			mEquipment[i].transform.localScale = Vector3.one;
			mEquipment[i].SetItemPlace(ItemPlaceType.IPT_Equipment,i);
			mEquipment[i].SetGridMask((GridMask)(1<<i));
			mEquipment[i].onLeftMouseClicked = OnLeftMouseCliked;
			mEquipment[i].onRightMouseClicked = OnRightMouseCliked;
			mEquipment[i].onDropItem = OnDropItem;
		}
	}
	
	public void SetCurPlayer(Player player)
    {
        mPlayer = player;
		mBodyViewCtr = ViewCameraControler.CreatViewCamera();
		mBodyViewCtr.Init(true);
		Camera mCam = mBodyViewCtr.camera;
		mCam.depth = 0;
		mPlayerObj = GameObject.Instantiate(player.gameObject, new Vector3(-100,-1150,10), Quaternion.identity) as GameObject;
		mPlayerObj.name = "PlayerEq";
		mPlayerObj.GetComponent<Player>().mModel.layer = LayerMask.NameToLayer("Show Model");
		
		mBodyViewCtr.SetTarget(mPlayerObj.GetComponent<Player>().mModel, mPlayer.mPlayerDataBlock.mSex,ViewCameraControler.ViewPart.VP_All);
		
		Component[] comps = mPlayerObj.GetComponents<Component>();
		
		foreach(Component comp in comps)
		{
			if(null != (comp as Rigidbody))
				mPlayerObj.rigidbody .constraints = RigidbodyConstraints.FreezeAll;
			else if(null == (comp as Animator) 
				&& null == (comp as SkinnedMeshRenderer)
				&& null == (comp as Transform)
				&& null == (comp as Collider))
				Destroy(comp);
		}

		mBodyViewCtr.SetTarget(mPlayerObj,mPlayer.mPlayerDataBlock.mSex,ViewCameraControler.ViewPart.VP_All);
		mCam.transform.parent = null;
		mCam.transform.position = mPlayerObj.transform.position + new Vector3(0,0.8871288f,1.883728f);
		mCam.transform.rotation = Quaternion.AngleAxis(180,Vector3.up);
		mCam.cullingMask = 1 << LayerMask.NameToLayer("Show Model");
		mCam.nearClipPlane = 0.1f;
		mEqTex.mainTexture = mBodyViewCtr.GetTex();
		mBodyViewCtr.SetLightState(false);
		mBodyViewCtr.SetActive(false);
		
		mHeadViewCtr = ViewCameraControler.CreatViewCamera();
		mHeadViewCtr.Init(true);
		mHeadViewCtr.SetTarget(mPlayerObj.GetComponent<Player>().mModel,player.mPlayerDataBlock.mSex);
		mHeadViewCtr.SetLightState(true);
		MainLeftGui_N.Instance.SetHeadTex(mHeadViewCtr.GetTex());
    }
	
	void OnTurnLeftBtn()
	{
		mPlayerObj.transform.rotation *= Quaternion.AngleAxis(36,Vector3.up);
	}
	
	void OnTurnRightBtn()
	{
		mPlayerObj.transform.rotation *= Quaternion.AngleAxis(-36,Vector3.up);
	}
	
	public override void AwakeWindow ()
	{
		if(null == PlayerFactory.mMainPlayer)
			return;
		base.AwakeWindow ();
		mBodyViewCtr.SetActive(true);
	}
	
	public override bool HideWindow ()
	{
		if(null != mBodyViewCtr)
			mBodyViewCtr.SetActive(false);
		return base.HideWindow ();
	}

	public void RemoveEquipmentByIndex(int index)
	{
		if(null != mEquipment[index].ItemObj)
			mPlayer.TakeOffEquip(mEquipment[index].ItemObj);
	}
	
	public void OnLeftMouseCliked(Grid_N grid)
	{
		ActiveWnd();
		SelectItem_N.Instance.SetItem(grid.ItemObj,grid.ItemPlace,grid.ItemIndex);
	}
	
	public void OnRightMouseCliked(Grid_N grid)
	{
		if(null == PlayerFactory.mMainPlayer || PlayerFactory.mMainPlayer.mDeath)
			return;
		ActiveWnd();
			
		if (GameConfig.IsMultiMode)
		{
			if (null != grid.ItemObj)
				PlayerFactory.mMainPlayer.ChangeItem(grid.ItemObj, grid.ItemIndex, -1, 1, -1);

			return;
		}

        if (grid.ItemObj != null)
		{
			ItemObject oldItem = grid.ItemObj;
			if(mPlayer.TakeOffEquip(grid.ItemObj))
			{
				mPlayer.GetItemPackage().AddItem(oldItem);
				GameGui_N.Instance.mItemPackGui.ResetItem();
			}
		}
	}
	
	public void OnDropItem(Grid_N grid)
	{
		ActiveWnd();
		if(SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar 
			|| SelectItem_N.Instance.ItemObj.mItemData.m_Position == 0
			|| (SelectItem_N.Instance.ItemObj.mItemData.mEquiSex != 0 && SelectItem_N.Instance.ItemObj.mItemData.mEquiSex != mPlayer.mPlayerDataBlock.mSex))
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}

		if (GameConfig.IsMultiMode)
		{
			// 放回原来位置
			if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)
			{
				PlayerFactory.mMainPlayer.ChangeItem(SelectItem_N.Instance.ItemObj, SelectItem_N.Instance.Index, grid.ItemIndex, 0, -1);
				return;
			}
		}

		switch(SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_HotKeyBar:
			SelectItem_N.Instance.SetItem(null);
			break;
		default:
			if(mPlayer.PutOnEquip(SelectItem_N.Instance.ItemObj))
			{
				SelectItem_N.Instance.RemoveOriginItem();
				grid.SetItem(SelectItem_N.Instance.ItemObj);
				SelectItem_N.Instance.SetItem(null);
			}
			break;
		}
	}
	
	//do change once between 0.5 second
	public void RebuildModel()
	{
		//mMeshRebuilding = false;
		mHeadViewCtr.transform.parent = null;
		if(null != mPlayerObj)
			Destroy(mPlayerObj);

		// zhouxun CreationMeshLoader
		CreationMeshLoader[] creation_mesh_loaders = mPlayer.GetComponentsInChildren<CreationMeshLoader>(true);
		int creation_id = 0;
		if (creation_mesh_loaders.Length > 0)
			creation_id = creation_mesh_loaders[0].CreationID;

		// Instantiate
		mPlayerObj = GameObject.Instantiate(mPlayer.mModel, new Vector3(-100,-1150,10), Quaternion.identity) as GameObject;

		// zhouxun CreationMeshLoader dest
		CreationMeshLoader[] dest_creation_mesh_loaders = mPlayerObj.GetComponentsInChildren<CreationMeshLoader>(true);
		if (dest_creation_mesh_loaders.Length > 0)
		{
			//dest_creation_mesh_loaders[0].CreationID = creation_id;
			//dest_creation_mesh_loaders[0].LoadMesh();
			//dest_creation_mesh_loaders[0].GetComponent<VCMeshMgr>().m_ColorMap = CreationMgr.GetCreation(creation_id).m_IsoData.m_Colors;
			//dest_creation_mesh_loaders[0].GetComponent<VCMeshMgr>().Init();
		}

		OutlineObject outline = mPlayerObj.GetComponentInChildren<OutlineObject>();
		if(null != outline)
			Destroy(outline);

		mPlayerObj.name = "PlayerEq";
		mPlayerObj.layer = LayerMask.NameToLayer("Show Model");
		if(null != mPlayerObj.renderer)
			mPlayerObj.renderer.enabled = true;
		mHeadViewCtr.SetTarget(mPlayerObj,mPlayer.mPlayerDataBlock.mSex);
		
		mPlayerObj.GetComponent<Animator>().enabled = true;
		
		Component[] comps = mPlayerObj.GetComponents<Component>();
		if(!mPlayerObj.activeSelf)
			mPlayerObj.SetActive(true);
	}
	
	public void UpdateShowState()
	{
		if(!mUpdateShowState)
		{
			mUpdateShowState = true;
			Invoke("DoUpadateShowState", 0.1f);
		}
	}
	
	void DoUpadateShowState()
	{
		mUpdateShowState = false;
		foreach(Grid_N grid in mEquipment)
		{
			grid.SetItem(null);
		}
		foreach(ItemObject item in mPlayer.Equipments)
		{
			for(int i = 0; i < 10; i++)
			{
				if(Convert.ToBoolean(item.mItemData.m_Position & (int)mEquipment[i].ItemMask))
					mEquipment[i].SetItem(item);
			}
		}
		MainMidGui_N.Instance.ChangeWeapon(mEquipment[4].ItemObj);
	}
	
	/// <summary>
	/// Applies the durability reduce. Type. 0 Attack 1 HpReduce
	/// </summary>
//	public void ApplyDurabilityReduce(int Type)
//	{
//		WeaponType mask = WeaponType.All;
//		if(0 == Type)
//		{
//			foreach(Grid_N itemGrid in mEquipment)
//			{
//				if(null != itemGrid.ItemObj && Convert.ToBoolean(itemGrid.ItemObj.mItemData.mWeaponType & mask))
//				{
//					switch(itemGrid.ItemObj.mItemData.mWeaponType)
//					{
//					case WeaponType.Axe:
//					case WeaponType.ShortWeapon:
//					case WeaponType.Mine:
//					case WeaponType.Spade:
//					case WeaponType.Bow:
//					case WeaponType.HandGun:
//					case WeaponType.Rifle:
//						itemGrid.ItemObj.Durability -= itemGrid.ItemObj.GetProperty(ItemProperty.DurabilityReduceWhenUse);
//						break;
//					}
//					mask &= ~itemGrid.ItemObj.mItemData.mWeaponType;
//					if(itemGrid.ItemObj.Durability == 0)
//					{
//						MessageBox_N.ShowMsgBox(MsgBoxType.Msg_OK,MsgInfoType.NoticeOnly,"The " + itemGrid.ItemObj.mItemData.m_Englishname + " is broken.");
//						if(null != itemGrid.ItemObj.EquipBuff)
//							mPlayer.m_effSkillBuffManager.RemoveBuff(itemGrid.ItemObj.EquipBuff);
//					}
//				}
//			}
//		}
//		else
//		{
//			foreach(Grid_N itemGrid in mEquipment)
//			{
//				if(null != itemGrid.ItemObj && Convert.ToBoolean(itemGrid.ItemObj.mItemData.mWeaponType & mask))
//				{
//					switch(itemGrid.ItemObj.mItemData.mWeaponType)
//					{
//					case WeaponType.Shield_Hand:
//					case WeaponType.Plastron:
//					case WeaponType.Tasse:
//					case WeaponType.Helm:
//					case WeaponType.Glove:
//					case WeaponType.Shoes:
//					case WeaponType.Bag:
//						itemGrid.ItemObj.Durability -= itemGrid.ItemObj.GetProperty(ItemProperty.DurabilityReduceWhenUse);
//						break;
//					}
//					mask &= ~itemGrid.ItemObj.mItemData.mWeaponType;
//					if(itemGrid.ItemObj.Durability == 0)
//					{
//						MessageBox_N.ShowMsgBox(MsgBoxType.Msg_OK,MsgInfoType.NoticeOnly,"The " + itemGrid.ItemObj.mItemData.m_Englishname + " is broken.");
//						if(null != itemGrid.ItemObj.EquipBuff)
//							mPlayer.m_effSkillBuffManager.RemoveBuff(itemGrid.ItemObj.EquipBuff);
//					}
//				}
//			}
//		}
//	}
}



*/
