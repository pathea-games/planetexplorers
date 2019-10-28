using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using System;
using Pathea;
using SkillSystem;
using System.Linq;


public struct AttributeInfo
{
    public AttributeInfo(AttribType curType, AttribType maxType)
    {
        m_CurType = curType;
        m_MaxType = maxType;
        m_Player = null;
    }

    public void SetEntity(PeEntity player)
    {
        m_Player = player;
    }

    private AttribType m_CurType;
    private AttribType m_MaxType;
    private PeEntity m_Player;
    private const string BuffGreenColFormat = "[B7EF54]+{0}[-]";
    public const string RedColFomat = "[FF1D05]{0}[-]";
    public const string GreenColFormat = "[B7EF54]{0}[-]";
    public const string YellowColFomat = "[FFE000]{0}[-]";


    public int CurValue { get { return m_Player == null ? 0 : Convert.ToInt32(m_Player.GetAttribute(m_CurType)); } }
    public int MaxValue { get { return m_Player == null ? 0 : Convert.ToInt32(m_Player.GetAttribute(m_MaxType)); } }
    //lz-2016.10.19 buff增加的值最大值减去基础值
    public int BuffValue { get { return m_Player == null ? 0 : Convert.ToInt32(MaxValue - Convert.ToInt32(m_Player.GetAttribute(m_MaxType, false))); } }
    public string GetCur_MaxStr()
    {
        string str = "";
        int buffValue = BuffValue;
        if (buffValue > 0)
        {
            str = string.Format(YellowColFomat, CurValue + "/")+string.Format(GreenColFormat, MaxValue);
        }
        else if (buffValue < 0)
        {
            str = string.Format(YellowColFomat, CurValue + "/") + string.Format(RedColFomat, MaxValue);
        }
        else
        {
            str = string.Format(YellowColFomat, CurValue + "/" + MaxValue);
        }
        return str;
    }

    public string GetBuffStr()
    {
        string str = "";
        int buffValue = BuffValue;
        if (buffValue > 0)
        {
            str = string.Format(BuffGreenColFormat, buffValue);
        }
        else if (buffValue < 0)
        {
            str = string.Format(RedColFomat, buffValue);
        }
        return str;
    }
}
public class UIPlayerInfoCtrl : UIBaseWnd
{
    public GameObject GridContent;
    public Grid_N mGridPrefab;
    public UITexture mEqTex;
    public GameObject mPlayerStagePrefab;
    public Vector3 mPlayerStagePos;
    public Vector3 mPlayerStageScale;

    public UILabel mLbName;
    public UISprite mSprSexFemale;
    public UISprite mSprSexMale;

    public UILabel mLbHealth;
    public UISlider mSdHealth;
    public UILabel mLbHealthBuff;
    public UILabel mLbStamina;
    public UISlider mSdStamina;
    public UILabel mLbStaminaBuff;
    public UILabel mLbHunger;
    public UISlider mSdHunger;
    public UILabel mLbHungerBuff;
    public UILabel mLbComfort;
    public UISlider mSdComfort;
    public UILabel mLbComfortBuff;
    public UILabel mLbOxygen;
    public UISlider mSdOxygen;
    public UILabel mLbOxygenBuff;
    public UILabel mLbShield;
    public UISlider mSdShield;
    public UILabel mLbShieldBuff;
    public UILabel mLbEnergy;
    public UISlider mSdEnergy;
    public UILabel mLbEnergyBuff;
    public UILabel mLbAttack;
    public UILabel mLbAttackBuff;
    public UILabel mLbDefense;
    public UILabel mLbDefenseBuff;
    public List<BoxCollider> mSuiteBtns;

    public GameObject mEquipmentPage;
    public GameObject armorPage;
    public GameObject waitingImage;

    public WhiteCat.UIArmorSuitEvent[] armorSuitButtons;
    public WhiteCat.Bone2DObjects bone2DObjects;
    public UIDragObject dragObject;


    [SerializeField]
    PeUIEffect.UISpriteScaleEffect effect;

    //lz-2016.10.19 属性封装
    AttributeInfo AttrHpInfo = new AttributeInfo(AttribType.Hp, AttribType.HpMax);
    AttributeInfo AttrStaminaInfo= new AttributeInfo(AttribType.Stamina, AttribType.StaminaMax);
    AttributeInfo AttrHungerInfo = new AttributeInfo(AttribType.Hunger, AttribType.HungerMax);
    AttributeInfo AttrComfortInfo = new AttributeInfo(AttribType.Comfort, AttribType.ComfortMax);
    AttributeInfo AttrOxygenInfo= new AttributeInfo(AttribType.Oxygen, AttribType.OxygenMax);
    AttributeInfo AttrShieldInfo= new AttributeInfo(AttribType.Shield, AttribType.ShieldMax);
    AttributeInfo AttrEnergyInfo= new AttributeInfo(AttribType.Energy, AttribType.EnergyMax);
    AttributeInfo AttrAtkInfo = new AttributeInfo(AttribType.Atk, AttribType.Atk); 
    AttributeInfo AttrDefInfo = new AttributeInfo(AttribType.Def, AttribType.Def);

    [SerializeField]
    UILabel m_BuffStrsLb;
    //private string m_BuffStrFormat="+{0}";


    PeEntity m_Player=null;
    PeEntity player { get { return m_Player; }
        set {
            m_Player = value;
            AttrHpInfo.SetEntity(value);
            AttrStaminaInfo.SetEntity(value);
            AttrHungerInfo.SetEntity(value);
            AttrComfortInfo.SetEntity(value);
            AttrOxygenInfo.SetEntity(value);
            AttrShieldInfo.SetEntity(value);
            AttrEnergyInfo.SetEntity(value);
            AttrAtkInfo.SetEntity(value);
            AttrDefInfo.SetEntity(value);
        } }



	BiologyViewCmpt viewCmpt = null;
    CommonCmpt commonCmpt = null;
    EquipmentCmpt m_EquipmentCmpt=null;
    EquipmentCmpt equipmentCmpt {   
        get { return m_EquipmentCmpt; }
        set {
                if (m_EquipmentCmpt != null)
                {
                    m_EquipmentCmpt.onSuitSetChange -= UpdateSuitBuffTips;
                }
                m_EquipmentCmpt = value;
                if (m_EquipmentCmpt != null)
                {
                    m_EquipmentCmpt.onSuitSetChange += UpdateSuitBuffTips;
                    UpdateSuitBuffTips(m_EquipmentCmpt.matchDatas);
                }
                else
                {
                    UpdateSuitBuffTips(null);
                }
            }
    }
    //PlayerPackageCmpt packageCmpt = null;
    EntityInfoCmpt entityInfoCmpt = null;
    WhiteCat.PlayerArmorCmpt playerArmorCmpt;

    PeViewController _viewController;
    GameObject _viewModel;

    List<Grid_N> mEquipment = new List<Grid_N>();
    string[] mSpName;

    //	protected override void InitWindow ()
    //	{
    //
    //		_viewController = PeViewStudio.CreateViewController(ViewControllerParam.DefaultPerps);
    //		_viewController.SetLocalPos(PeViewStudio.s_ViewPos);
    //		base.InitWindow ();
    //	}


    WhiteCat.CreationController[] _meshControllers;
    GameObject _newViewModel;
    int _delay;		// 执行 clone 和 应用新模型都需要延迟

    bool waitingToCloneModel
    {
        get { return waitingImage.activeSelf; }
        set { waitingImage.SetActive(value); }
    }


    public override void OnCreate()
    {
        base.OnCreate();
        _viewController = PeViewStudio.CreateViewController(ViewControllerParam.DefaultCharacter);
		_viewController.SetLocalPos(PeViewStudio.s_ViewPos);
		_viewController.name = "ViewController_Player";
		_viewController.gameObject.SetActive(false);
        InitGrid();

        if (MainPlayerCmpt.gMainPlayer != null)
            MainPlayerCmpt.gMainPlayer.onDurabilityDeficiency += DurabilityDeficiencyTip;
    }

    void Start()
    {
        //lz-2016.06.30 套装按钮注册悬浮提示
        if (mSuiteBtns != null && mSuiteBtns.Count > 0)
        {
            for (int i = 0; i < mSuiteBtns.Count; i++)
            {
                UIEventListener.Get(mSuiteBtns[i].gameObject).onHover += (go, hover) => {
                    if (hover)
                        UITooltip.ShowText(PELocalization.GetString(8000542)+(mSuiteBtns.FindIndex(a=>a.gameObject==go)+1).ToString());
                    else
                        UITooltip.ShowText("");
                };
            }
        }
    }

    void DurabilityDeficiencyTip()
    {
        //lz-2016.10.31 This item is broken!
        new PeTipMsg(PELocalization.GetString(8000851), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
    }

    void Update()
    {
        UpdatePalyerInfo();
        UpdateModel();
        UpdateArmor();
    }


    void InitGrid()
    {
        mSpName = new string[10];
        mSpName[0] = "Icons_helmet";
        mSpName[1] = "Icons_coat";
        mSpName[2] = "Icons_gloves";
        mSpName[3] = "Icons_battery";
        mSpName[4] = "Icons_arms";
        mSpName[5] = "Icons_back";
        mSpName[6] = "Icons_pants";
        mSpName[7] = "Icons_shoes";
        mSpName[8] = "Icons_glass";
        mSpName[9] = "Icons_dun";



        for (int i = 0; i < 10; i++)
        {
            mEquipment.Add(Instantiate(mGridPrefab) as Grid_N);
            mEquipment[i].gameObject.name = "HotKey" + i;
            mEquipment[i].transform.parent = GridContent.transform;
            mEquipment[i].transform.localPosition = new Vector3(i / 5 * 235, -i % 5 * 58, -2);
            mEquipment[i].transform.localRotation = Quaternion.identity;
            mEquipment[i].transform.localScale = Vector3.one;
            mEquipment[i].SetItemPlace(ItemPlaceType.IPT_Equipment, i);
            mEquipment[i].SetGridMask((GridMask)(1 << i));
            mEquipment[i].mScriptIco.spriteName = mSpName[i];
            mEquipment[i].onDropItem += OnDropItemToEquipment;
            mEquipment[i].onLeftMouseClicked += OnLeftMouseCliked;
            mEquipment[i].onRightMouseClicked += OnRightMouseCliked;
        }

    }


    void UpdatePalyerInfo()
    {
        if (null == entityInfoCmpt)
        {
            mLbName.text = "--";
            mLbName.MakePixelPerfect();
        }
        else
        {
            mLbName.text = entityInfoCmpt.characterName.ToString();
            mLbName.MakePixelPerfect();
        }

        if (null == commonCmpt)
        {
            mSprSexFemale.spriteName = "male_icon";
            mSprSexFemale.MakePixelPerfect();
        }
        else
        {
            if (commonCmpt.sex == PeSex.Female)
            {
                mSprSexFemale.spriteName = "female_icon";
                mSprSexFemale.enabled = true;
                mSprSexMale.enabled = false;
            }
            else
            {
                mSprSexMale.spriteName = "male_icon";
                mSprSexFemale.enabled = false;
                mSprSexMale.enabled = true;
            }
        }
       

        float cur, max;

        cur = AttrHpInfo.CurValue;
        max = AttrHpInfo.MaxValue;
        mLbHealth.text = AttrHpInfo.GetCur_MaxStr();
        mSdHealth.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbHealthBuff.text = AttrHpInfo.GetBuffStr();


        cur = AttrStaminaInfo.CurValue;
        max = AttrStaminaInfo.MaxValue;
        mLbStamina.text = AttrStaminaInfo.GetCur_MaxStr();
        mSdStamina.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbStaminaBuff.text = AttrStaminaInfo.GetBuffStr();

        cur = AttrHungerInfo.CurValue;
        max = AttrHungerInfo.MaxValue;
        mLbHunger.text = AttrHungerInfo.GetCur_MaxStr();
        mSdHunger.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbHungerBuff.text = AttrHungerInfo.GetBuffStr();


        cur = AttrComfortInfo.CurValue;
        max = AttrComfortInfo.MaxValue;
        mLbComfort.text = AttrComfortInfo.GetCur_MaxStr();
        mSdComfort.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbComfortBuff.text = AttrComfortInfo.GetBuffStr();

        cur = AttrOxygenInfo.CurValue;
        max = AttrOxygenInfo.MaxValue;
        mLbOxygen.text = AttrOxygenInfo.GetCur_MaxStr();
        mSdOxygen.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbOxygenBuff.text = AttrOxygenInfo.GetBuffStr();

        cur = AttrShieldInfo.CurValue;
        max = AttrShieldInfo.MaxValue;
        mLbShield.text = AttrShieldInfo.GetCur_MaxStr();
        mSdShield.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbShieldBuff.text = AttrShieldInfo.GetBuffStr();

        cur = AttrEnergyInfo.CurValue;
        max = AttrEnergyInfo.MaxValue;
        mLbEnergy.text = AttrEnergyInfo.GetCur_MaxStr();
        mSdEnergy.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbEnergyBuff.text = AttrEnergyInfo.GetBuffStr();

        cur = AttrAtkInfo.CurValue;
        mLbAttack.text = string.Format((AttrAtkInfo.BuffValue > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.YellowColFomat), cur);
        //mLbAttackBuff.text = AttrAtkInfo.GetBuffStr();

        cur = AttrDefInfo.CurValue;
        mLbDefense.text = string.Format((AttrDefInfo.BuffValue > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.YellowColFomat), cur);
        //mLbDefenseBuff.text = AttrDefInfo.GetBuffStr();
    }

    /// <summary>更新套装Buff提示</summary>
    void UpdateSuitBuffTips(List<SuitSetData.MatchData> datas)
    {
        string tips = "";
        if (null != datas && datas.Count > 0)
        {
            SuitSetData.MatchData data = new SuitSetData.MatchData();
            for (int i = 0; i < datas.Count; ++i)
            {
                data=datas[i];
                if (null != data.tips && data.tips.Length > 0)
                {
                    for (int j = 0; j < data.tips.Length; ++j)
                    {
                        if (0 != data.tips[j]&& data.activeTipsIndex >= j)
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


    void GetEntityCmpt()
    {
        if (player != GameUI.Instance.mMainPlayer)
        {
            player = GameUI.Instance.mMainPlayer;
            viewCmpt = player.biologyViewCmpt;
            commonCmpt = player.commonCmpt;
            equipmentCmpt = player.equipmentCmpt;
            //packageCmpt = player.GetCmpt<PlayerPackageCmpt>();
            entityInfoCmpt = player.enityInfoCmpt;
            playerArmorCmpt = player.GetCmpt<WhiteCat.PlayerArmorCmpt>();
        }
    }


    #region Equipment funcs

    void EquipmentChange(object sender, EquipmentCmpt.EventArg arg)
    {
        //  GetEntityCmpt();
        if (mInit && isShow)
        {
            RefreshEquipmentList();
            RefreshEquipment();
        }
    }


    void RefreshEquipmentList()
    {
        for (int i = 0; i < 10; i++)
        {
            mEquipment[i].SetItem(null);
        }

        foreach (ItemObject item in equipmentCmpt._ItemList)
        {
            ItemAsset.Equip equip = item.GetCmpt<ItemAsset.Equip>();

            for (int i = 0; i < 10; i++)
            {
                if (Convert.ToBoolean(equip.equipPos & (int)mEquipment[i].ItemMask))
                    mEquipment[i].SetItem(item);
            }
        }
    }


    void RefreshEquipment()
    {
        if (mInit && isShow && viewCmpt)
        {
            if (viewCmpt != null)
            {
                StopRefreshEquipment();
                waitingToCloneModel = true;
                _meshControllers = viewCmpt.GetComponentsInChildren<WhiteCat.CreationController>();
            }
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
        _delay = 1;
        _meshControllers = null;
    }

    #endregion


    #region Armor Funcs

    ItemObject _selectedArmorItem;
    WhiteCat.ArmorType _selectedArmorType;


    void OnItemSelected(ItemObject item)
    {
        if (item != null && armorPage.activeInHierarchy)
        {
            _selectedArmorType = WhiteCat.CreationHelper.GetArmorType(item.instanceId);

            bone2DObjects.SetActiveGroup(_selectedArmorType);

            if (_selectedArmorType != WhiteCat.ArmorType.None)
            {
                _selectedArmorItem = item;
            }
            else _selectedArmorItem = null;
        }
    }


    void UpdateArmor()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (_selectedArmorItem != null && _selectedArmorType != WhiteCat.ArmorType.None)
            {
                int boneGroup, boneIndex;
                if (bone2DObjects.GetHoverBone(out boneGroup, out boneIndex))
                {
                    if (PeGameMgr.IsMulti)
                    {
                        if (!playerArmorCmpt.hasRequest)
                        {
                            playerArmorCmpt.C2S_EquipArmorPartFromPackage(
                                _selectedArmorItem.instanceId,
                                (int)_selectedArmorType,
                                boneGroup,
                                boneIndex,
                                OnArmorPartEquiped);
                        }
                    }
                    else
                    {
                        OnArmorPartEquiped(
                            playerArmorCmpt.EquipArmorPartFromPackage(
                                _selectedArmorItem,
                                _selectedArmorType,
                                boneGroup,
                                boneIndex));
                    }
                }

                _selectedArmorItem = null;
                _selectedArmorType = WhiteCat.ArmorType.None;
                bone2DObjects.HideAll();
            }
        }
    }

    #endregion


    /// <summary>
    /// 在 update 中调用.
    /// 检查到需要更新时, 先判断角色装备是否创建完整, 在创建完成后复制模型, 然后延迟一帧应用模型
    /// </summary>
    void UpdateModel()
    {
        if (waitingToCloneModel)
        {
            if (!_newViewModel)
            {
                if (_meshControllers != null)
                {
                    for (int i = 0; i < _meshControllers.Length; i++)
                    {
                        if (_meshControllers[i] != null && !_meshControllers[i].isBuildFinished)
                        {
                            return;
                        }
                    }
                    _meshControllers = null;
                }

                if (_delay == 0)
                {
                    _newViewModel = PeViewStudio.CloneCharacterViewModel(viewCmpt);
                    //lz-2016.10.11 有可能为空【错误 #3973】
                    if (null != _newViewModel)
                    {
                        _newViewModel.transform.position = new Vector3(0, -1000, 0);
                        var renderer = _newViewModel.GetComponent<SkinnedMeshRenderer>();
                        renderer.updateWhenOffscreen = true;
                        _delay = 0;
                    }
                }
                else
                {
                    _delay--;
                    return;
                }
            }

            if (_newViewModel)
            {
                if (_delay == 0)
                {
                    if (_viewModel != null) Destroy(_viewModel);
                    _viewModel = _newViewModel;
                    _viewController.SetTarget(_viewModel.transform);

                    mEqTex.GetComponent<WhiteCat.UIViewController>().Init(_viewController);
                    bone2DObjects.Init(_viewController, _viewModel, playerArmorCmpt);

                    mEqTex.mainTexture = _viewController.RenderTex;

                    _newViewModel = null;
                    StopRefreshEquipment();
                }
                else
                {
                    _delay--;
                    return;
                }
            }
        }
    }


    public override void Show()
    {
        if (null == GameUI.Instance.mMainPlayer) return;
        base.Show();
        effect.Play();

        GetEntityCmpt();
        if (equipmentCmpt) equipmentCmpt.changeEventor.Subscribe(EquipmentChange);
        if (playerArmorCmpt) playerArmorCmpt.onAddOrRemove += RefreshEquipment;
        GameUI.Instance.mItemPackageCtrl.onItemSelected += OnItemSelected;

        RefreshEquipmentList();
        RefreshEquipment();
        _viewController.gameObject.SetActive(true);
    }


    protected override void OnHide()
    {
        _viewController.gameObject.SetActive(false);
        if (_viewModel != null) Destroy(_viewModel);

        if (equipmentCmpt) equipmentCmpt.changeEventor.Unsubscribe(EquipmentChange);
        if (playerArmorCmpt) playerArmorCmpt.onAddOrRemove -= RefreshEquipment;
        GameUI.Instance.mItemPackageCtrl.onItemSelected -= OnItemSelected;

        StopRefreshEquipment();
        base.OnHide();
    }


    public bool RemoveEquipmentByIndex(int index)
    {
        if (null!=mEquipment&&index>=0&&index< mEquipment.Count&& null != mEquipment[index].ItemObj)
        {
            if (GameConfig.IsMultiMode)
            {
                if (equipmentCmpt.TryTakeOffEquipment(mEquipment[index].ItemObj))
                    return true;
            }
            else
            {
                if (equipmentCmpt.TakeOffEquipment(mEquipment[index].ItemObj, false))
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


    public void OnLeftMouseCliked(Grid_N grid)
    {
        if (m_Player == null || null == equipmentCmpt)
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


    public void OnRightMouseCliked(Grid_N grid)
    {
        if (null == player || GameUI.Instance.bMainPlayerIsDead)
            return;

        if (GameConfig.IsMultiMode)
        {
            if (null != grid.ItemObj && equipmentCmpt.TryTakeOffEquipment(grid.ItemObj))
                PlayerNetwork.mainPlayer.RequestTakeOffEquipment(grid.ItemObj);
        }
        else
        {
            if (grid.ItemObj != null)
            {
                //ItemObject oldItem = grid.ItemObj;
                if (equipmentCmpt.TakeOffEquipment(grid.ItemObj))
                {
                    if (GameUI.Instance.mItemPackageCtrl != null)
                        GameUI.Instance.mItemPackageCtrl.ResetItem();
                    //lz-2016.08.30 脱下装备成功播放音效
                    GameUI.Instance.PlayTakeOffEquipAudio();
                }
            }
        }
    }


    public void OnDropItemToEquipment(Grid_N grid)
    {
        if (SelectItem_N.Instance.Place != ItemPlaceType.IPT_Bag
           || !UIToolFuncs.CanEquip(SelectItem_N.Instance.ItemObj, commonCmpt.sex)
           || ((int)grid.ItemMask & SelectItem_N.Instance.ItemObj.protoData.equipPos) == 0)
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        if (GameConfig.IsMultiMode)
        {
            if (equipmentCmpt.NetTryPutOnEquipment(SelectItem_N.Instance.ItemObj))
            {
                PlayerNetwork.mainPlayer.RequestPutOnEquipment(SelectItem_N.Instance.ItemObj, SelectItem_N.Instance.Index);
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
            }
            SelectItem_N.Instance.SetItem(null);
        }
        else
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_Bag:
                    if (equipmentCmpt.PutOnEquipment(SelectItem_N.Instance.ItemObj))
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

    private void OnEquipmentBtn(bool active)
    {
        if (active)
            mEquipmentPage.SetActive(true);
        else
            mEquipmentPage.SetActive(false);
    }


    private void OnArmorBtn(bool active)
    {
        armorPage.SetActive(active);
        if (active) SetArmorSuit(playerArmorCmpt.currentSuitIndex);
    }


    public void SetArmorSuit(int selectedIndex)
    {
        if (PeGameMgr.IsMulti)
        {
            if (playerArmorCmpt.hasRequest) return;
            playerArmorCmpt.C2S_SwitchArmorSuit(selectedIndex, OnArmorSuitChanged);
        }
        else
        {
            OnArmorSuitChanged(playerArmorCmpt.SwitchArmorSuit(selectedIndex));
        }

        bone2DObjects.HideAll();
    }


    public void OnArmorSuitChanged(bool success)
    {
        if (success)
        {
            for (int i = 0; i < armorSuitButtons.Length; i++)
            {
                armorSuitButtons[i].SetSelected(playerArmorCmpt.currentSuitIndex == i);
            }
        }
        else
        {
            MessageBox_N.ShowOkBox("/\\__/\\");
        }
    }


    public void OnArmorPartEquiped(bool success)
    {
        if (!success)
        {
            MessageBox_N.ShowOkBox("/\\__/\\");
        }
    }
}