using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;

public class CSUI_NPCEquip : MonoBehaviour
{
    #region UI_WIDGET

    [SerializeField]
    UIGrid m_EquipRoot;
    [SerializeField]
    UITexture m_NpcTex;
    [SerializeField]
    UILabel m_BuffStrsLb;
    #endregion

    [SerializeField]
    CSUI_Grid m_GridPrefab;

    private PeEntity m_RefNpc;
    EquipmentCmpt equipmentCmpt = null;
    public EquipmentCmpt NpcEquipment
    {
        get { return equipmentCmpt; }
        set
        {
            if (equipmentCmpt != null)
            {
                equipmentCmpt.onSuitSetChange -= UpdateSuitBuffTips;
            }
            equipmentCmpt = value;
            if (equipmentCmpt != null)
            {
                equipmentCmpt.onSuitSetChange += UpdateSuitBuffTips;
                UpdateSuitBuffTips(equipmentCmpt.matchDatas);
            }
            else
            {
                UpdateSuitBuffTips(null);
            }
        }
    }
    //private EntityInfoCmpt m_NpcInfo;
    private CommonCmpt m_NpcCommonInfo;
    private NpcCmpt m_NpcCmpt;
    private BiologyViewCmpt m_ViewCmpt;
    public PeEntity RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            m_RefNpc = value;
            if (m_RefNpc != null)
            {
                NpcEquipment = m_RefNpc.equipmentCmpt;
                NpcEquipment.changeEventor.Unsubscribe(EquipmentChangeEvent);
                NpcEquipment.changeEventor.Subscribe(EquipmentChangeEvent);
                //m_NpcInfo = m_RefNpc.enityInfoCmpt;
                m_NpcCommonInfo = m_RefNpc.commonCmpt;
                m_NpcCmpt = m_RefNpc.NpcCmpt;
                m_ViewCmpt = m_RefNpc.biologyViewCmpt;
            }
            else
            {
                NpcEquipment = null;
                //m_NpcInfo = null;
                m_NpcCommonInfo = null;
                m_NpcCmpt = null;
                m_ViewCmpt = null;
                m_NpcTex.enabled = false;
            }

            UpdateEquipAndTex();
            UpdateMustNotTag();
        }
    }

    void UpdateMustNotTag()
    {
        if (m_RefNpc == null || m_RefNpc.IsRandomNpc())
        {
            for (int i = 0; i < m_EquipGrids.Count; i++)
            {
                m_EquipGrids[i].m_Grid.MustNot = false;
            }
        }
        else
        {
            for (int i = 0; i < m_EquipGrids.Count; i++)
            {
                m_EquipGrids[i].m_Grid.MustNot = true;
            }
        }

    }

    #region VIEW_CONTROLLER
    [SerializeField]
    PeViewController _viewController;
    GameObject _viewModel;
    bool mInit = false;

    void InitBodyCamera()
    {
        //mTakeEqPhotho = new TakeEquipmentPhoto();
        //mTakeEqPhotho.Init(new Vector3(-300, -1000, -2000));

        _viewController = PeViewStudio.CreateViewController(ViewControllerParam.DefaultCharacter);
		_viewController.SetLocalPos(PeViewStudio.s_ViewPos);
		_viewController.name = "ViewController_NPCEquip";
		//_viewController.gameObject.SetActive(false); // this is invoked in Start, _viewController should be active. 
		UpdateBodyCamera();
    }

    void UpdateBodyCamera()
    {
        if (m_RefNpc == null || NpcEquipment == null || m_NpcCmpt == null)
        {
            m_NpcTex.enabled = false;
            return;

        }

        if (_viewModel != null)
            Destroy(_viewModel);
        _viewModel = PeViewStudio.CloneCharacterViewModel(m_ViewCmpt);
        if (_viewModel != null)
        {
            _viewController.SetTarget(_viewModel.transform);

        }

        m_NpcTex.mainTexture = _viewController.RenderTex;
        m_NpcTex.GetComponent<WhiteCat.UIViewController>().Init(_viewController);

        m_NpcTex.enabled = true;

    }

    #endregion

    // Status Event
    public enum EEventType
    {
        CantWork,
        PutItemInto,
        TakeAwayItem,
        ResortItem,
        SplitItem,
        DeleteItem
    }
    public delegate void OpStatusDel(EEventType type, object obj1, object obj2);
    //public event OpStatusDel OpStatusEvent;

    private List<CSUI_Grid> m_EquipGrids = new List<CSUI_Grid>();

    string[] mSpName = new string[10];
    // Use this for initialization
    void Start()
    {
        InitBodyCamera();
        InitEquipGrids();
        mInit = true;
    }

	void OnEnable()
	{
		if(null != _viewController)
			_viewController.gameObject.SetActive(true);
	}

	void OnDisable()
	{		
		if(null != _viewController)
			_viewController.gameObject.SetActive(false);
	}

    void InitEquipGrids()
    {
        // InitEquipments
        mSpName[0] = "Icons_helmet";//头盔
        mSpName[1] = "Icons_coat";//衣服
        mSpName[2] = "Icons_gloves";//手套
        mSpName[3] = "Icons_battery";//电池
        mSpName[4] = "Icons_arms";//武器
        mSpName[5] = "Icons_back";//背包
        mSpName[6] = "Icons_pants";//裤子
        mSpName[7] = "Icons_shoes";//鞋
        mSpName[8] = "Icons_glass";//眼镜
        mSpName[9] = "Icons_dun";//盾牌

        // Create NPC Item Grid
        for (int i = 0; i < 10; i++)
        {
            CSUI_Grid grid = Instantiate(m_GridPrefab) as CSUI_Grid;
            grid.transform.parent = m_EquipRoot.transform;
            if (i < 5)
                grid.transform.localPosition = new Vector3(-112, 176 - i % 5 * 58, -2);
            else
                grid.transform.localPosition = new Vector3(112, 176 - i % 5 * 58, -2);
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            grid.m_Grid.SetItemPlace(ItemPlaceType.IPT_ConolyServantEquPersonel, i);
            grid.m_Grid.SetGridMask((GridMask)(1 << i));
            grid.m_Grid.mScriptIco.spriteName = mSpName[i];
            grid.m_Grid.mScriptIco.MakePixelPerfect();
            grid.m_Active = false;
            //grid.m_Index = i;
            m_EquipGrids.Add(grid);

            grid.m_Grid.onLeftMouseClicked = OnEquipLeftMouseClicked;
            grid.m_Grid.onDropItem = OnEquipDropItem;
            grid.m_Grid.onRightMouseClicked = OnEquipRightMouseClicked;
        }
        m_EquipRoot.repositionNow = true;
    }

    public bool IsShow { get { return gameObject.activeInHierarchy; } }


    public bool EquipItem(ItemObject itemObj)
    {
        if (NpcEquipment != null)
        {
            EquipmentCmpt.Receiver receiver = PeCreature.Instance.mainPlayer == null ? null : PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>();
            if (NpcEquipment.PutOnEquipment(itemObj, true, receiver))
            {
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
                return true;
            }
        }
        return false;
    }

    /// <summary>更新套装Buff提示</summary>
    void UpdateSuitBuffTips(List<SuitSetData.MatchData> datas)
    {
        string tips = "";
        if (null != datas && datas.Count > 0)
        {
            SuitSetData.MatchData data = new SuitSetData.MatchData();
            for (int i = 0; i < datas.Count; i++)
            {
                data = datas[i];
                if (null != data.tips && data.tips.Length > 0)
                {
                    for (int j = 0; j < data.tips.Length; j++)
                    {
                        if (0 != data.tips[j] && data.activeTipsIndex >= j)
                        {
                            tips += PELocalization.GetString(data.tips[j]) + "\n";
                        }
                    }
                }
            }
        }
        if (tips.Length > 0)
        {
            tips = tips.Substring(0, tips.Length - 1);
        }
        m_BuffStrsLb.text = tips;
        m_BuffStrsLb.MakePixelPerfect();
    }

    #region CALL_BACKE

    //lz-2016.09.07 装备相关的操作，为了保证ISO装备正常，并且不闪烁
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
        if (mInit && IsShow)
        {
            UpdateEquipGrid();
            RefreshEquipment();
        }
    }

    //lz-2016.07.15 注册装备改变事件
    void EquipmentChangeEvent(object sender, EquipmentCmpt.EventArg arg)
    {
        UpdateEquipAndTex();
    }


    public void UpdateEquipGrid()
    {
        for (int i = 0; i < m_EquipGrids.Count; i++)
        {
            m_EquipGrids[i].m_Grid.SetItem(null);
        }

        if (m_RefNpc == null)
            return;

        if (NpcEquipment != null)
        {
            foreach (ItemObject item in NpcEquipment._ItemList)
            {
                for (int i = 0; i < m_EquipGrids.Count; i++)
                {
                    if (0 != (item.protoData.equipPos & (int)m_EquipGrids[i].m_Grid.ItemMask))
                        m_EquipGrids[i].m_Grid.SetItem(item);
                }
            }
        }
    }

    void RefreshEquipment()
    {
        if (mInit && IsShow && null!=m_ViewCmpt)
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
        if (null != m_ViewCmpt)
        {
            _meshControllers = m_ViewCmpt.GetComponentsInChildren<WhiteCat.CreationController>();

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
            _newViewModel = PeViewStudio.CloneCharacterViewModel(m_ViewCmpt);
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
            m_NpcTex.GetComponent<WhiteCat.UIViewController>().Init(_viewController);
            m_NpcTex.mainTexture = _viewController.RenderTex;
            m_NpcTex.enabled = true;
            _newViewModel = null;
        }
        waitingToCloneModel = false;
    }

    #endregion

    void OnEquipDropItem(Grid_N grid)
    {
        if (m_RefNpc == null)
            return;

        //EquipedNpc equip_npc = m_RefNpc as EquipedNpc;
        if (NpcEquipment == null)
            return;

        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar
            || !UIToolFuncs.CanEquip(SelectItem_N.Instance.ItemObj, m_NpcCommonInfo.sex)
            || ((int)grid.ItemMask & SelectItem_N.Instance.ItemObj.protoData.equipPos) == 0)
        {
            return;
        }


        if (GameConfig.IsMultiMode)
        {
            if (NpcEquipment.NetTryPutOnEquipment(SelectItem_N.Instance.ItemObj))
            {
                PlayerNetwork.mainPlayer.RequestNpcPutOnEquip(m_RefNpc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                SelectItem_N.Instance.SetItem(null);
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
            }
        }
        else
        {
            //EquipmentCmpt.Receiver receiver = PeCreature.Instance.mainPlayer == null ? null : PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>();
            if (NpcEquipment.PutOnEquipment(SelectItem_N.Instance.ItemObj, true))
            {
                SelectItem_N.Instance.RemoveOriginItem();
                SelectItem_N.Instance.SetItem(null);
				UpdateEquipAndTex();
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
            }
        }
    }

    public bool EquipRemoveOriginItem(int index)
    {
        if (null != m_EquipGrids && index >= 0 && index < m_EquipGrids.Count && null != m_EquipGrids[index].m_Grid.ItemObj)
        {
            if (GameConfig.IsMultiMode)
            {
                if (equipmentCmpt.TryTakeOffEquipment(m_EquipGrids[index].m_Grid.ItemObj))
                    return true;
            }
            else
            {
                if (equipmentCmpt.TakeOffEquipment(m_EquipGrids[index].m_Grid.ItemObj, false))
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

    void OnEquipLeftMouseClicked(Grid_N grid)
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

    void OnEquipRightMouseClicked(Grid_N grid)
    {
        if (m_RefNpc == null)
            return;

        if (null == grid.ItemObj)
            return;

        if (GameConfig.IsMultiMode)
        {
            if (NpcEquipment.TryTakeOffEquipment(grid.ItemObj))
            {
                PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(m_RefNpc.Id, grid.ItemObj.instanceId, -1);
                //lz-2016.08.31 脱下装备成功播放音效
                GameUI.Instance.PlayTakeOffEquipAudio();
            }
        }
        else
        {
            PlayerPackageCmpt playerPackageCmpt = PeCreature.Instance.mainPlayer == null ? null : Pathea.PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            //lz-2016.07.19 这里应该用添加和移除同步检测和操作的接口
            if (NpcEquipment.TakeOffEquipment(grid.ItemObj, true,playerPackageCmpt))
            {
                GameUI.Instance.mItemPackageCtrl.Show();
                if (GameUI.Instance.mItemPackageCtrl != null)
                    GameUI.Instance.mItemPackageCtrl.ResetItem();
                //lz-2016.08.31 脱下装备成功播放音效
                GameUI.Instance.PlayTakeOffEquipAudio();
                
            }
            else
            {
                
                //lz-2016.07.19 玩家包裹可以添加，说明在取下装备的时候失败了
                if (null == playerPackageCmpt || playerPackageCmpt.package.CanAdd(grid.ItemObj))
                {
                    //lz-2016.07.19 提示NPC正在使用此装备
                    CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000594));
                }
                else
                {
                    //lz-2016.07.19  提示玩家背包已经满了
                    CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000050).Replace("\\n", " "));
                }
            }
        }
    }
    #endregion
}
