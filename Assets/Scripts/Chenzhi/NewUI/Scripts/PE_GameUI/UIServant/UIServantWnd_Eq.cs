using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Pathea;
using ItemAsset;

public partial class UIServantWnd : UIBaseWnd
{
    #region Equipment func

    public bool RemoveEqByIndex(int index)
    {
        if (null != mEquipmentList && index >= 0 && index < mEquipmentList.Count && null != mEquipmentList[index].ItemObj)
        {
            if (GameConfig.IsMultiMode)
            {
                if (equipmentCmpt.TryTakeOffEquipment(mEquipmentList[index].ItemObj))
                    return true;
            }
            else
            {
                if (equipmentCmpt.TakeOffEquipment(mEquipmentList[index].ItemObj, false))
                {
                    //lz-2016.08.30 脱下装备成功播放音效
                    GameUI.Instance.PlayTakeOffEquipAudio();
                    return true;
                }
            }
        }

        //lz-2016.10.09 提示正在使用此装备,无法操作
        PeTipMsg.Register(PELocalization.GetString(8000594), PeTipMsg.EMsgLevel.Error);
        return false;
    }

	public bool RemoveEqByObj(ItemObject itemObj, bool addToReceiver,EquipmentCmpt.Receiver receiver)
	{
        if (equipmentCmpt.TakeOffEquipment(itemObj, addToReceiver, receiver))
        {
            //lz-2016.08.31 脱下装备成功播放音效
            GameUI.Instance.PlayTakeOffEquipAudio();
            return true;
        }
        return false;
	}

    public bool EquipItem(ItemObject itemObj)
    {
        if (equipmentCmpt != null)
        {
            EquipmentCmpt.Receiver receiver = PeCreature.Instance.mainPlayer == null ? null : PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>();
            if (equipmentCmpt.PutOnEquipment(itemObj, true, receiver))
            {
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
                return true;
            }
        }

        return false;
    }

	public bool EquipItem(ItemObject itemObj,EquipmentCmpt.Receiver receiver)
	{
		if (equipmentCmpt != null)
		{
            if (equipmentCmpt.PutOnEquipment(itemObj, true, receiver))
            {
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
                return true;
            }
		}
		
		return false;
	}


    void RefreshEqList()
    {
        ClearEqList();
        //lz-2016.11.23 错误 #6922 Crush bug
        if (null != equipmentCmpt && null != equipmentCmpt._ItemList && equipmentCmpt._ItemList.Count > 0)
        {
            foreach (ItemObject item in equipmentCmpt._ItemList)
            {
                ItemAsset.Equip equip = item.GetCmpt<ItemAsset.Equip>();

                if (null != equip && null != mEquipmentList && mEquipmentList.Count >= 10)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (Convert.ToBoolean(equip.equipPos & (int)mEquipmentList[i].ItemMask))
                            mEquipmentList[i].SetItem(item);
                    }
                }
            }
        }
    }

    void ClearEqList()
    {
        if (null != mEquipmentList&& mEquipmentList.Count>=10)
        {
            for (int i = 0; i < 10; i++)
            {
                mEquipmentList[i].SetItem(null);
            }
        }
    }

    #endregion

    //lz-2016.09.07 装备相关的操作，为了保证ISO装备显示正常，并且不闪烁
    #region Equipment funcs
    [SerializeField]
    private GameObject waitingImage;
    WhiteCat.CreationController[] _meshControllers;
    GameObject _newViewModel;

    bool waitingToCloneModel
    {
        get { return waitingImage.activeSelf; }
        set { waitingImage.SetActive(value); }
    }


    public void UpdateEquipAndTex()
    {
        if (mInit && isShow)
        {
            RefreshEqList();
            RefreshEquipment();
        }
    }

    //lz-2016.07.15 注册装备改变事件
    void EquipmentChangeEvent(object sender, EquipmentCmpt.EventArg arg)
    {
        UpdateEquipAndTex();
    }

    void RefreshEquipment()
    {
        if (mInit && isShow && null != viewCmpt)
        {
            StopCoroutine("UpdateModelIterator");
            StopRefreshEquipment();
            StartCoroutine("UpdateModelIterator");
        }
    }

    void StopRefreshEquipment()
    {
        waitingToCloneModel = false;
        if (_newViewModel)
        {
            Destroy(_newViewModel);
            _newViewModel = null;
        }
        _meshControllers = null;
    }

    IEnumerator UpdateModelIterator()
    {
        waitingToCloneModel = true;
        if (null != viewCmpt)
        {
            _meshControllers = viewCmpt.GetComponentsInChildren<WhiteCat.CreationController>();

            if (null != _meshControllers)
            {
                for (int i = 0; i < _meshControllers.Length; i++)
                {
                    if (_meshControllers[i] != null && !_meshControllers[i].isBuildFinished)
                    {
                        //lz-2016.09.07 如果有ISO，并且每创建完成，就等待创建完成
                        yield return null;
                        i--;
                    }
                }
            }
            _meshControllers = null;
            //lz-2016.09.14 克隆前需要等一帧，克隆后不需要等了
            yield return null;
            //lz-2016.09.07 克隆新模型，刷新SkinnedMeshRenderer
            _newViewModel = PeViewStudio.CloneCharacterViewModel(viewCmpt);
            if (_newViewModel)
            {
                _newViewModel.transform.position = new Vector3(0, -1000, 0);
                var renderer = _newViewModel.GetComponent<SkinnedMeshRenderer>();
                renderer.updateWhenOffscreen = true;
            }
        }
        if (_newViewModel)
        {
            if (_viewModel != null) Destroy(_viewModel);
            _viewModel = _newViewModel;
            _viewController.SetTarget(_viewModel.transform);
            mEqTex.GetComponent<WhiteCat.UIViewController>().Init(_viewController);
            mEqTex.mainTexture = _viewController.RenderTex;
            mEqTex.enabled = true;
            _newViewModel = null;
        }
        waitingToCloneModel = false;
    }

    #endregion

    #region ui event
    public void OnLeftMouseCliked_Equip(Grid_N grid)
    {

        if (m_NpcCmpt == null || null == equipmentCmpt)
            return;

        if (null == grid || null == grid.ItemObj) return;

        if (equipmentCmpt.TryTakeOffEquipment(grid.ItemObj))
        {
            SelectItem_N.Instance.SetItemGrid(grid);
        }
        else
        {
            //lz-2016.10.09 提示正在使用此装备,无法操作
            PeTipMsg.Register(PELocalization.GetString(8000594), PeTipMsg.EMsgLevel.Error);
        }
    }

    public void OnRightMouseCliked_Equip(Grid_N grid)
    {
        if (null == equipmentCmpt)
            return;

        if (grid.ItemObj == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            //lz-2016.10.31 错误 #5316 先尝试脱装备，可以脱的时候再脱，避免复制装备
            if (equipmentCmpt.TryTakeOffEquipment(grid.ItemObj))
            {
                PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(servant.Id, grid.ItemObj.instanceId, -1);
                //lz-2016.08.31 脱下装备成功播放音效
                GameUI.Instance.PlayTakeOffEquipAudio();
            }
        }
        else
        {
            PlayerPackageCmpt playerPackageCmpt = PeCreature.Instance.mainPlayer == null ? null : Pathea.PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            if (equipmentCmpt.TakeOffEquipment(grid.ItemObj, true, playerPackageCmpt))
            {
                GameUI.Instance.mItemPackageCtrl.ResetItem();
                //lz-2016.08.30 脱下装备成功播放音效
                GameUI.Instance.PlayTakeOffEquipAudio();
            }
            else
            {
                //lz-2016.07.19 玩家包裹可以添加，说明在取下装备的时候失败了
                if (null == playerPackageCmpt || playerPackageCmpt.package.CanAdd(grid.ItemObj))
                {
                    //lz-2016.07.19 提示NPC正在使用此装备
                    MessageBox_N.ShowOkBox(PELocalization.GetString(8000594));
                }
                else
                {
                    //lz-2016.07.19  提示玩家背包已经满了
                    CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000050));
                }
            }
        }
    }

    public void OnDropItem_Equip(Grid_N grid)
    {
        if (null == equipmentCmpt)
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar
            ||!UIToolFuncs.CanEquip(SelectItem_N.Instance.ItemObj, commonCmpt.sex) 
            || ((int)grid.ItemMask & SelectItem_N.Instance.ItemObj.protoData.equipPos) == 0)
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        if (PeGameMgr.IsMulti)
        {
            if (equipmentCmpt.NetTryPutOnEquipment(SelectItem_N.Instance.ItemObj))
            {
                PlayerNetwork.mainPlayer.RequestNpcPutOnEquip(servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                SelectItem_N.Instance.SetItem(null);
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
            }
        }
        else
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ServantInteraction:
                case ItemPlaceType.IPT_ServantInteraction2:
                case ItemPlaceType.IPT_Bag:
                    if (equipmentCmpt.PutOnEquipment(SelectItem_N.Instance.ItemObj, true))
                    {
                        SelectItem_N.Instance.RemoveOriginItem();
                        grid.SetItem(SelectItem_N.Instance.ItemObj);
                        SelectItem_N.Instance.SetItem(null);
                        //lz-2016.08.31 装备成功播放音效
                        GameUI.Instance.PlayPutOnEquipAudio();
                    }
                    break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }

    #endregion

}
