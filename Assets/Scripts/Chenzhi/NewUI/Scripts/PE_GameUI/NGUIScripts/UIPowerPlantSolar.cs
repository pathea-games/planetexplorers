using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;

public class UIPowerPlantSolar : UIBaseWnd
{
    public UIGrid mChargingItemTable;

    public GameObject mChargingItemPrefab;

    CSUI_ChargingGrid[] mChargingItemUi;

    CSPowerPlantObject mPowerPlantSolor;

    Vector3 mMachinePos = Vector3.zero;

    public MapObjNetwork _net = null;

    protected override void InitWindow()
    {
        base.InitWindow();

        mChargingItemUi = new CSUI_ChargingGrid[12];

        for (int i = 0; i < mChargingItemUi.Length; i++)
        {
            GameObject obj = Object.Instantiate(mChargingItemPrefab) as GameObject;
            obj.transform.parent = mChargingItemTable.transform;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            mChargingItemUi[i] = obj.GetComponent<CSUI_ChargingGrid>();
            mChargingItemUi[i].m_bCanChargeLargedItem = false;
            mChargingItemUi[i].onItemChanded = OnItemChanged;
            mChargingItemUi[i].OnDropItemMulti = OnDropItemMulti;
            mChargingItemUi[i].OnRightMouseClickedMulti = OnRightMouseClickedMulti;
            mChargingItemUi[i].m_Index = i;
        }

        mChargingItemTable.repositionNow = true;
    }

    void OnItemChanged(int index, ItemObject item)
    {
        if (mPowerPlantSolor == null)
        {
            return;
        }

        mPowerPlantSolor.m_PowerPlant.SetChargingItem(index, item);
    }

    public void OpenWnd(CSPowerPlantObject powerPlantSolor)
    {

        //lz-2016.11.09 Cursh bug 错误 #5603
        if (null == powerPlantSolor|| null== powerPlantSolor.m_Entity)
        {
            Debug.LogError("powerPlantSolor is null.");
            return;
        }

        mPowerPlantSolor = powerPlantSolor;

        PeEntity entity = powerPlantSolor.GetComponentInParent<PeEntity>();
        if (null!=entity)
            mMachinePos = entity.transform.position;

        Show();
        //多人模式
        if (Pathea.PeGameMgr.IsMulti)
        {
			_net = MapObjNetwork.GetNet(mPowerPlantSolor.m_Entity.ID) ;
			if(_net != null)
            	_net.RequestItemList();
			else
			{
				Debug.LogError("can't find net id = " + mPowerPlantSolor.m_Entity.ID);
			}
            return;
        }

        if (null != mPowerPlantSolor.m_PowerPlant && mPowerPlantSolor.m_PowerPlant.GetChargingItemsCnt() > 0)
        {
            for (int i = 0; i < mPowerPlantSolor.m_PowerPlant.GetChargingItemsCnt(); i++)
            {
                ItemObject itemObject = mPowerPlantSolor.m_PowerPlant.GetChargingItem(i);
                if (i >= mChargingItemUi.Length)
                {
                    Debug.LogError("too many charing item to show on solar ui");
                    break;
                }

                mChargingItemUi[i].SetItem(itemObject);
            }
        }
    }

    /// <summary>
    /// 打开界面或放物品进来的回调
    /// </summary>
    /// <param name="_idarray"></param>
    public void OnMultiOpenDropCallBack(MapObjNetwork net, int[] _idarray)
    {
        if (_net != null && _net == net)
        {
            if (mChargingItemUi == null)
                return;
            for (int i = 0; i < _idarray.Length; i++)
            {
                if (_idarray[i] != -1)
                {
                    ItemObject obj = ItemMgr.Instance.Get(_idarray[i]);
                    mChargingItemUi[i].SetItem(obj);
                }
                else
                {
                    mChargingItemUi[i].SetItem(null);
                }
            }

            SelectItem_N.Instance.SetItem(null);
            GameUI.Instance.mItemPackageCtrl.ResetItem();
        }
    }

    /// <summary>
    /// 取出物品回调
    /// </summary>
    /// <param name="_index"></param>
    /// <param name="_id"></param>
    public void OnMultiRemoveCallBack(MapObjNetwork net, int _index, int _id)
    {
        if (_net != null && _net == net)
        {
            if (mChargingItemUi == null)
                return;
//            ItemObject obj = ItemMgr.Instance.Get(_id);
            mChargingItemUi[_index].SetItem(null);
            GameUI.Instance.mItemPackageCtrl.ResetItem();
        }
    }

    void OnDropItemMulti(int index, Grid_N grid)
    {
        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
            return;
        if (SelectItem_N.Instance.ItemObj.instanceId >= 100000000)
            return;
        _net.InsertItemList(SelectItem_N.Instance.ItemObj.instanceId, index);
    }

    void OnRightMouseClickedMulti(int index, Grid_N grid)
    {
        _net.GetItem(grid.ItemObj.instanceId);
    }

    protected override void OnClose()
    {
        _net = null;
        mMachinePos = Vector3.zero;
        Hide();
    }

    void Update()
    {
        if (null != GameUI.Instance.mMainPlayer && mMachinePos != Vector3.zero)
        {
            if (Vector3.Distance(GameUI.Instance.mMainPlayer.position, mMachinePos) > 8f)
                OnClose();
        }
    }
}
