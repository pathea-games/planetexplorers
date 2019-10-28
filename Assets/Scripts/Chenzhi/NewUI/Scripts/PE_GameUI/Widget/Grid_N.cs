using UnityEngine;
using System;
using System.Collections;
using ItemAsset;
using Pathea;
using SkillAsset;
using System.Collections.Generic;
using System.Linq;

public enum GridMask
{
    GM_Any = -1,
    GM_Item = 1 << 31,
    GM_Skill = 1 << 30,
    GM_Resource = 1 << 29,
    GM_Armor = 1 << 28,
    GM_Mission = 1 << 27,
    GM_Equipment = (1 << 10) - 1,


    GM_HatMask = 1,//帽子
    GM_CoatMask = 1 << 1,//上装
    GM_HandMask = 1 << 2,//手套
    GM_RingMask = 1 << 3,//戒指
    GM_MainHandMask = 1 << 4,//主手
    GM_NecklaceMask = 1 << 5,//项链
    GM_TrousersMask = 1 << 6,//下装
    GM_FootMask = 1 << 7,//鞋子
    GM_BackMask = 1 << 8,//背部
    GM_OtherHandMask = 1 << 9,//副手
    GM_TwoHandMask = (1 << 4) + (1 << 9)//双手
}

public class Grid_N : MonoBehaviour
{
    public static Grid_N mActiveGrid;
    public UITexture mItemTex;
    public UISprite mItemspr;
    public UISprite mDragMark;
    public UISprite mNewMark;
    public UISprite mForbiden;
    public UILabel mNumCount;

    public UISprite mScript;
    public UISprite mScriptIco;

    public UIFilledSprite mSkillCooldown;
    public UIFilledSprite mSkillCd;
    public UISprite mDurability;
    public UISprite mDurabilitySpecial;
    public UILabel mPowerPercent;

    //used for mustn't change equipment
    public bool MustNot = false;
    public UISprite mMustNotSpr;

    ItemSample mItemGrid;

    /// <summary>
    /// Gets the ItemSample.
    /// </summary>
    public ItemSample Item { get { return mItemGrid; } }

    /// <summary>
    /// Gets the ItemObject.
    /// </summary>
    public ItemObject ItemObj { get { return mItemGrid as ItemObject; } }

    //lz-2016.10.21 因为属性中加了套装属性等，需要把通用的几个属性从基础属性提取出来，重新排版
    private static List<int> m_CommonAttributeIDs = new List<int>() { 8000806, 8000804, 8000805 };


    public static void SetActiveGrid(Grid_N activeG)
    {
        if (null != mActiveGrid)
        {
            if (null != mActiveGrid.mSkillCooldown)
                mActiveGrid.mSkillCooldown.fillAmount = 0;
            mActiveGrid = null;
        }
        if (null != activeG)
        {
            mActiveGrid = activeG;
            if (null != mActiveGrid.mSkillCooldown)
                mActiveGrid.mSkillCooldown.fillAmount = 1;
        }
    }

    //EffSkill mItemSkill;

    public bool mShowNum = true;

    ItemPlaceType mItemPlace = ItemPlaceType.IPT_Null;
    int mItemIndex = 0;

    public ItemPlaceType ItemPlace { get { return mItemPlace; } }
    public int ItemIndex { get { return mItemIndex; } set { mItemIndex = value; } }

    GridMask mGridMask = GridMask.GM_Any;	//-1:All 0:Item 1:Equipment

    public GridMask ItemMask { get { return mGridMask; } }

    int mSkillID;
    public int SkillID { get { return mSkillID; } }
    string mSkillDes;
    bool mIsSkill = false;
    SkillSystem.SkEntity mSkenity;
    public bool mSampleMode = true;

    // [CSUI] Delegate
    public delegate void GridDelegate(Grid_N grid);
    public delegate void GridsExchangeDel(Grid_N grid, ItemObject item);

    public GridDelegate onDropItem = null;
    public GridDelegate onRemoveOriginItem = null;
    public GridDelegate onLeftMouseClicked = null;
    public GridDelegate onRightMouseClicked = null;
    public GridsExchangeDel onGridsExchangeItem = null;
    public GridDelegate onDragItem = null;

    [SerializeField]
    GameObject UIGridEffectPrefab;

    void Awake()
    {
        //mItemGrid = null;
        //mItemTex.enabled = false;
        //mItemspr.enabled = false;
        //mNewMark.enabled = false;
        mNumCount.enabled = mShowNum;
        //mNumCount.text = "";
    }

    void Update()
    {

        //禁止换装图标显示
        if (mMustNotSpr != null)
        {
            mMustNotSpr.enabled = MustNot;
        }

        if (mItemGrid != null && mItemGrid.GetCount() > 1) //&& mItemGrid.protoData.maxStackNum > 1
            mNumCount.text = mItemGrid.GetCount().ToString();
        else
            mNumCount.text = "";


        if (null != mSkillCooldown
           && !mSampleMode
           && (Pathea.PeCreature.Instance != null)
           && (Pathea.PeCreature.Instance.mainPlayer != null)
           )
        {
            Pathea.UseItemCmpt useItem = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.UseItemCmpt>();
            if (useItem == null)
                return;
            if (mItemGrid == null)
                return;

            if (useItem.GetCdByItemProtoId(mItemGrid.protoId) > 0)
                mSkillCooldown.fillAmount = useItem.GetCdByItemProtoId(mItemGrid.protoId);
            else
                mSkillCooldown.fillAmount = 0;
        }

        if (mIsSkill && mSkillID > 0 && (Pathea.PeCreature.Instance != null)
           && (Pathea.PeCreature.Instance.mainPlayer != null))
        {

            if (mSkenity == null)
                return;

            Pathea.UseItemCmpt useItem = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.UseItemCmpt>();
            if (useItem == null)
                return;

            if (useItem.GetNpcSkillCd(mSkenity, mSkillID) > 0)
                mSkillCooldown.fillAmount = useItem.GetNpcSkillCd(mSkenity, mSkillID);
            else
                mSkillCooldown.fillAmount = 0;

        }

        //耐久度
        if (null != ItemObj)
        {
            if (null != mDurability)
            {
                Durability dur = ItemObj.GetCmpt<Durability>();
                if (null!=dur && null != dur.value)
                    mDurability.alpha = 1 - dur.value.current / dur.valueMax;
                else
                    mDurability.alpha = 0;
            }
            //mNewMark.enabled = ItemObj.HasProperty(ItemProperty.NewFlagTime);
        }
        else
        {
            if (null != mDurability)
                mDurability.alpha = 0;
        }

        //电池电量 
        if (null != ItemObj)
        {
            //lz-2016.06.14 唐小力说每个地方都显示电量
          if (mPowerPercent != null)
            {
                Energy _power = ItemObj.GetCmpt<Energy>();
                if (null!=_power&&null != _power.energy && ItemObj.protoId == 228)
                    mPowerPercent.text = ((int)(_power.energy.percent * 100)).ToString() + "%";
                else
                    mPowerPercent.text = "";
            }
        }
        else
        {
            if (mPowerPercent != null)
                mPowerPercent.text = "";
        }

        UpdateEffect();

        //UpdateSillcool();
    }



    #region UIGridEffect

    PeUIEffect.UIGridEffect effect;
    bool moveIn = false;
    float moveInTime = 0;

    void PlayGridEffect()
    {
        if ((mItemTex.enabled == true && mItemTex.mainTexture != null) || (mItemspr.enabled == true && mItemspr.spriteName != "Null"))
        {
            GameObject obj = GameObject.Instantiate(UIGridEffectPrefab) as GameObject;
            obj.transform.parent = this.transform.parent;
            obj.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -5);
            obj.transform.localScale = new Vector3(48, 48, 1);
            effect = obj.GetComponentInChildren<PeUIEffect.UIGridEffect>();
            if (effect != null)
                effect.e_OnEnd += EffectEnd;
        }
    }

    void EffectEnd(PeUIEffect.UIEffect _effect)
    {
        effect = null;
    }


    void OnMouseMoveOut()
    {
        moveIn = false;
    }

    void OnMouseMoveOver()
    {
        moveIn = true;
        moveInTime = 0;
        isOnce = true;
    }

    bool isOnce = false;
    void UpdateEffect()
    {
        if (moveIn)
        {
            moveInTime += Time.deltaTime;
            if (moveInTime > 0.4f && isOnce)
            {
                PlayGridEffect();
                isOnce = false;
            }
            if (moveInTime > 5.2f && !isOnce)
            {
                PlayGridEffect();
                moveInTime = 0.4f;
            }
        }
    }

    void UpdateSillcool()
    {


        if (mItemGrid == null)
        {
            return;
        }

        if ((SelectItem_N.Instance.ItemSkillmar.IsInSkillCD(mItemGrid.protoId)))
        {
            if (!SkillSystem.SkData.s_SkillTbl.ContainsKey(mItemGrid.protoData.skillId))
            {
                return;
            }
            if (mItemGrid.protoData.skillId == 0)
                return;

            float MaxCooltime = SkillSystem.SkData.s_SkillTbl[mItemGrid.protoData.skillId]._coolingTime;
            mItemGrid.ClickedTime += Time.deltaTime;
            if (mItemGrid.ClickedTime <= MaxCooltime)
            {
                SelectItem_N.Instance.ItemSkillmar.Fillcount = 1.0f - mItemGrid.ClickedTime / MaxCooltime;

            }
            else
            {
                mItemGrid.ClickedTime = 0;
                SelectItem_N.Instance.ItemSkillmar.Fillcount = 0;
                SelectItem_N.Instance.ItemSkillmar.DelateSkillCD(mItemGrid.protoId);
                mItemGrid.Click = false;
            }
        }
    }

    #endregion

    public void SetItem(int itemObjID)
    {
        SetItem(ItemMgr.Instance.Get(itemObjID));
    }


    public virtual void SetItem(ItemSample itemGrid, bool showNew = false)
    {
        mItemGrid = itemGrid;
        mSampleMode = (mItemGrid == null);

        if (mItemTex == null)
            return;

        if (mItemGrid == null)
        {
            mItemTex.enabled = false;
            mItemTex.mainTexture = null;
            mItemspr.enabled = false;
            mNewMark.enabled = false;
            mNumCount.text = "";
            //mItemSkill = null;
            mDragMark.spriteName = "Null";
            mScript.spriteName = "Null";
            if (null != mSkillCooldown)
                mSkillCooldown.fillAmount = 0;
        }
        else
		{
			mNewMark.enabled = showNew;
            if (null != ItemObj && null != ItemObj.iconTex)
			{
                mItemTex.enabled = true;
                mItemTex.mainTexture = ItemObj.iconTex;
                mItemspr.enabled = false;
            }
            else
			{
                if (mItemGrid.iconString0 == "0")
                {
                    mItemTex.enabled = true;
                    mItemTex.mainTexture = mItemGrid.iconTex; ;
                    mItemspr.enabled = false;
                }
                else
                {
                    mItemTex.enabled = false;
                    mItemTex.mainTexture = null;
                    mItemspr.enabled = true;
                    mItemspr.spriteName = mItemGrid.iconString0;
                    mItemspr.MakePixelPerfect();
                }
            }

            if (ItemObj != null && ItemObj.CanDrag())
                mDragMark.spriteName = ItemObj.iconString1;
            else
                mDragMark.spriteName = "Null";

            mDragMark.MakePixelPerfect();
            //			mNewMark.enabled = mItemGrid.mNewFlag;
			if(mItemGrid.protoData != null)
			{
				if (mItemGrid.protoData.maxStackNum == 1)
	                mNumCount.text = "";

	            if (mScript != null)
	            {
	                mScript.spriteName = mItemGrid.iconString2;
					mScript.MakePixelPerfect();
	            }
			}

            if (mSkillCooldown)
            {
                mSkillCooldown.fillAmount = SelectItem_N.Instance.ItemSkillmar.GetFillcount(mItemGrid.protoId);
            }

        }
    }

    public void SetItemPlace(ItemPlaceType itemPlace, int index)
    {
        mItemPlace = itemPlace;
        mItemIndex = index;
    }

    public void SetGridMask(GridMask mask)
    {
        mGridMask = mask;
    }

    public void SetSkill(int skillId, int index, SkillSystem.SkEntity skenity = null, string icon = "", int decsId = 0)
    {
        if (icon != "")
        {
            //Debug.Log("skillId:" + skillId);       
            //Debug.Log("技能列表：" + EffSkill.s_tblEffSkills.Count);
            //            EffSkill skill = null;
            //            if (EffSkill.s_tblEffSkills != null)
            //                skill = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillId));
            string skillDes = "";
            if (decsId > 0)
                skillDes = PELocalization.GetString(decsId);
            //int len = 0;
            //foreach (EffSkill es in EffSkill.s_tblEffSkills)
            //{
            //    //Debug.Log("技能列表中技能" + (++len) + ":" + es.m_iconImgPath);
            //    Debug.Log("技能列表中技能ID" + (++len) + ":" + es.m_id);
            //}
            //            foreach (EffSkill es in EffSkill.s_tblEffSkills)
            //            {
            //                Debug.Log(es.m_name[0]);
            //            }
            //
            //            if (skill != null && skill.m_iconImgPath != "0")
            //            {
            //                mItemspr.spriteName = skill.m_iconImgPath;
            //                mItemspr.MakePixelPerfect();
            //            }
            mItemspr.spriteName = icon;
            mItemspr.MakePixelPerfect();
            mSkillID = skillId;
            mGridMask = GridMask.GM_Skill;
            mItemIndex = index;
            mItemspr.enabled = true;
            mIsSkill = true;
            mSkenity = skenity;
            mSkillDes = skillDes;
        }
        else
        {
            mItemspr.spriteName = "Null";
            mItemspr.MakePixelPerfect();
            mItemspr.enabled = false;
            mIsSkill = false;
            mSkenity = null;
            if (null != mSkillCooldown)
                mSkillCooldown.fillAmount = 0;
        }
    }

    public bool IsForbiden()
    {
        return mForbiden.gameObject.activeSelf;
    }
    public void SetGridForbiden(bool forbiden)
    {
        GetComponent<Collider>().enabled = !forbiden;
        mForbiden.gameObject.SetActive(forbiden);
    }

    public void SetDurabilityBg(ItemObject obj)
    {
        if (mDurabilitySpecial == null)
            return;
        if (obj == null)
        {
            mDurabilitySpecial.alpha = 0;
            return;
        }

        Durability dur = obj.GetCmpt<Durability>();
        if (dur != null)
            mDurabilitySpecial.alpha = 1 - dur.value.current / dur.valueMax;
        else
            mDurabilitySpecial.alpha = 0;
    }

    public void SetClick()
    {

        if (mItemGrid == null)
            return;

        mItemGrid.Click = true;
        if (SelectItem_N.Instance == null || SelectItem_N.Instance.ItemSkillmar == null)
            return;
        SelectItem_N.Instance.ItemSkillmar.AddSkillCD(mItemGrid);
        mItemGrid.ClickedTime = 0;
    }

    void OnClick()
    {
        if (MustNot)
            return;
        if (mGridMask == GridMask.GM_Skill)
        {
            return;
        }
        if (null != ItemObj)
        {

        }

        if (Input.GetMouseButtonUp(0))//mouse Left
        {
            if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
            {
                OnDrop(null);
                return;
            }

            //if (null != mItemGrid)
            //{
            //    // [CSUI]
            //    if (onLeftMouseClicked != null)
            //        onLeftMouseClicked(this);
            //}
            //Grid_N.SetActiveGrid(this);
        }
        else if (Input.GetMouseButtonUp(1))//mouse right
        {
            if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
            {
                SelectItem_N.Instance.SetItem(null);
                return;
            }
            if (null != mItemGrid)
            {
                // [CSUI]
                if (onRightMouseClicked != null)
                {
                    onRightMouseClicked(this);

                }

            }
        }
    }


    void OnDrag(Vector2 delta)
    {
        if (MustNot)
            return;
        if (onDragItem != null)
            onDragItem(this);
    }

    void OnPress(bool press)
    {
        if (MustNot)
            return;
        if (press && Input.GetMouseButtonDown(0) && null != SelectItem_N.Instance && !SelectItem_N.Instance.HaveOpItem() && mItemGrid != null)
        {
            if (null != ItemObj)
            {
                ////ItemObj.RemoveProperty(ItemProperty.NewFlagTime);
                //if (GameConfig.IsMultiMode)
                //{
                //	PlayerFactory.mMainPlayer.RPCServer(EPacketType.PT_InGame_RemoveNewFlag, ItemObj.instanceId);
                //}
            }

            // [CSUI]
            if (onLeftMouseClicked != null)
                onLeftMouseClicked(this);
            Grid_N.SetActiveGrid(this);
        }
    }

    //int ItemId = 0;
    void i(int itemid)
    {
        //ItemId = itemid;
    }

    void OnDrop(GameObject go)
    {
        if (MustNot)
            return;
        if (Input.GetMouseButtonUp(0) && null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
        {
            //			if (SelectItem_N.Instance.ItemObj == null)
            //				return;
            //
            //			if(null != ItemObj && SelectItem_N.Instance.ItemObj.instanceId == ItemObj.instanceId)
            //			{
            //				SelectItem_N.Instance.SetItem(null);
            //				return;
            //			}

            // -[CSUI] Drop Item Call back
            if (onDropItem != null)
                onDropItem(this);
        }
    }

    /// <summary>套装属性</summary>
    string SuitAttribute(string text)
    {
        string str = text;
        if (null == str) return string.Empty;
        if (null == ItemObj) return text;

        string greenColFomat = "[B7EF54]{0}[-]";
        string grayColFomat = "[808080]{0}[-]";
        string suitNameFomat = "{0} ( {1}/{2} )";
        char newLineStr = '\n';

        string baseAttribute = ""; //基础属性
        string singleAttribute = ""; //单个属性加成
        string suitActiveInfo = ""; //套装信息
        string suitAttribute = ""; //套装属性加成
        string commonAttribute = ""; //通用属性
        List<string> curCommonAttributes = new List<string>();
        for (int i = 0; i < m_CommonAttributeIDs.Count; i++)
        {
            curCommonAttributes.Add(PELocalization.GetString(m_CommonAttributeIDs[i]));
        }

        if (curCommonAttributes.Count > 0)
        {
            string[] strArray = text.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (null != strArray && strArray.Length > 0)
            {
                string curStr = "";
                bool containsCommonAttri = false;
                for (int i = 0; i < strArray.Length; i++)
                {
                    containsCommonAttri = false;
                    curStr = strArray[i];
                    for (int j = 0; j < curCommonAttributes.Count; j++)
                    {
                        if (curStr.Contains(curCommonAttributes[j]))
                        {
                            commonAttribute += (curStr + newLineStr);
                            containsCommonAttri = true;
                            break;
                        }
                    }
                    if (!containsCommonAttri)
                    {
                        baseAttribute += (curStr + newLineStr);
                    }
                }
            }

            if (baseAttribute.Length > 1)
            {
                baseAttribute = baseAttribute.Substring(0, baseAttribute.Length - 1);
            }
            if (commonAttribute.Length > 1)
            {
                commonAttribute = commonAttribute.Substring(0, commonAttribute.Length - 1);
            }
        }
        else
        {
            baseAttribute = str;
        }

        EquipSetData data = EquipSetData.GetData(ItemObj);
        if (null != data)
        {
            singleAttribute = data.desStr;
            if (singleAttribute.Length > 1)
            {
                singleAttribute = singleAttribute.Substring(0, singleAttribute.Length - 1);
            }
            //lz-2016.11.07 颜色改到里面处理
            //singleAttribute = string.Format(greenColFomat, singleAttribute);
        }

        EquipmentCmpt equipCmpt = GetEquipmentByCurItemPlace();
        List<SuitSetData.MatchData> matchDatas = new List<SuitSetData.MatchData>();
		if (null != equipCmpt && equipCmpt.matchDatas.Count > 0)
            matchDatas = equipCmpt.matchDatas;
        else
        {
            SuitSetData suitData = SuitSetData.GetData(ItemObj.protoId);
            if (null != suitData)
            {
                SuitSetData.MatchData matchData = new SuitSetData.MatchData();
                matchData.name = suitData.suitSetName;
				matchData.itemProtoList = suitData.itemProtoList;
                matchData.itemNames = suitData.itemNames;
                matchData.tips = suitData.tips;
				matchData.activeTipsIndex = -1;
                matchDatas.Add(matchData);
            }
        }
        suitActiveInfo = "";
        if (null != matchDatas&& matchDatas.Count>0)
        {
            SuitSetData.MatchData matchData = new SuitSetData.MatchData();
            string curStr = "";
            for (int i = 0; i < matchDatas.Count; i++)
            {
                if (null != matchDatas[i].itemProtoList && matchDatas[i].itemProtoList.Contains(ItemObj.protoId))
                {
                    matchData = matchDatas[i];
                    break;
                }
            }

            if (null != matchData.itemNames && matchData.itemNames.Count > 0)
            {
                int activeCount = (null == matchData.activeIndex) ? 0 : matchData.activeIndex.Count(a => a == true);
                suitActiveInfo = string.Format(suitNameFomat, matchData.name, activeCount, matchData.itemNames.Count);
                suitActiveInfo = string.Format(greenColFomat, suitActiveInfo);
                suitActiveInfo += newLineStr;
                for (int j = 0; j < matchData.itemNames.Count; j++)
                {
                    curStr = matchData.itemNames[j];
                    if (!string.IsNullOrEmpty(curStr))
                    {
                        if (null!=matchData.activeIndex&&j < matchData.activeIndex.Count && matchData.activeIndex[j])
                        {
                            curStr = string.Format(greenColFomat, curStr);
                        }
                        else
                        {
                            curStr = string.Format(grayColFomat, curStr);
                        }
                        suitActiveInfo += (curStr+ newLineStr);
                    }
                }
            }
            
           if (null != matchData.tips && matchData.tips.Length > 0)
            {
                for (int j = 0; j < matchData.tips.Length; j++)
                {
                    if (0 != matchData.tips[j])
                    {
                        curStr = PELocalization.GetString(matchData.tips[j]);
                        if (!string.IsNullOrEmpty(curStr))
                        {
                            if (j <= matchData.activeTipsIndex)
                            {
                                curStr = string.Format(greenColFomat, curStr);
                            }
                            else
                            {
                                curStr = string.Format(grayColFomat, curStr);
                            }
                            suitAttribute += (curStr+newLineStr);
                        }
                    }
                }
            }
        }
        if (suitActiveInfo.Length > 1)
        {
            suitActiveInfo = suitActiveInfo.Substring(0, suitActiveInfo.Length - 1);
        }
        if (suitAttribute.Length > 1)
        {
            suitAttribute = suitAttribute.Substring(0, suitAttribute.Length - 1);
        }
        string newAttributeStr = "";
        if (string.IsNullOrEmpty(singleAttribute) && string.IsNullOrEmpty(suitActiveInfo) && string.IsNullOrEmpty(suitAttribute))
        {
            newAttributeStr = str;
        }
        else
        {
            if (!string.IsNullOrEmpty(baseAttribute))
            {
                newAttributeStr += (baseAttribute + newLineStr + newLineStr);
            }
            if (!string.IsNullOrEmpty(singleAttribute))
            {
                newAttributeStr += (singleAttribute + newLineStr + newLineStr);
            }
            if (!string.IsNullOrEmpty(suitActiveInfo))
            {
                newAttributeStr += (suitActiveInfo + newLineStr + newLineStr);
            }
            if (!string.IsNullOrEmpty(suitAttribute))
            {
                newAttributeStr += (suitAttribute + newLineStr + newLineStr);
            }
            if (!string.IsNullOrEmpty(commonAttribute))
            {
                newAttributeStr += commonAttribute;
            }
            for (int i = newAttributeStr.Length - 1; i >= 0; i--)
            {
                if (newAttributeStr[i] == newLineStr)
                {
                    newAttributeStr = newAttributeStr.Substring(0, i);
                }
                else
                {
                    break;
                }
            }
        }
        return newAttributeStr;
    }
    EquipmentCmpt GetEquipmentByCurItemPlace()
    {
        switch (ItemPlace)
        {
            case ItemPlaceType.IPT_Equipment:
                return (null==MainPlayer.Instance.entity) ?null:MainPlayer.Instance.entity.equipmentCmpt;
            case ItemPlaceType.IPT_ServantEqu:
                return (GameUI.Instance==null)?null:GameUI.Instance.mServantWndCtrl.GetCurServantEquipCmpt();
            case ItemPlaceType.IPT_ConolyServantEquPersonel:
                return (GameUI.Instance==null)?null:GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.NpcEquipment;
            default:
                return null;
        }
    }

    void OnTooltip(bool show)
    {
        if (!show)
        {
            ToolTipsMgr.ShowText(null);
            return;
        }

        if (mIsSkill)
        {
            ToolTipsMgr.ShowText(mSkillDes);
            return;
        }

        if (mItemGrid == null)
        {
            ToolTipsMgr.ShowText(null);
            return;
        }
        string tipStr = "";

        if (null != Item)
        {
            tipStr = Item.GetTooltip();
            tipStr=SuitAttribute(tipStr);
        }

        ToolTipsMgr.ShowText(tipStr);
    }
}


