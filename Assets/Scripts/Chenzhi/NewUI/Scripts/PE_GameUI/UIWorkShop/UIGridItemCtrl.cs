using UnityEngine;
using System.Collections;
using ItemAsset;
using SkillAsset;
using System;



public enum ListItemType
{
    mItem = 0,
    mIso = 1,
    mIsoDectory = 2
};


public class UIGridItemCtrl : MonoBehaviour
{
    public delegate void ChickFunc(int index);
    public event ChickFunc mItemClick = null;




    public UISprite[] mContentSprites;

    public UITexture mContentTexture;

    public UILabel mTextCount;

    public BoxCollider mBoxCollider;

    public UIAtlas mAtlasButton;
    public UIAtlas mAtlasIcon;

    public int mIndex = -1;
    public int mItemId = 0;



    private ListItemType mType;

    private ItemSample mItemSample = null;


    public void SetToolTipInfo(ListItemType type, int id)
    {
        mType = type;
        mItemId = id;
    }


    public void SetTextCount(int count)
    {
        if (mTextCount == null)
            return;
        mTextCount.text = count.ToString();
    }


    public void SetCotent(string[] ico)
    {

        if (mContentSprites == null || mContentSprites.Length <= 0)
            return;

        for (int i = 0; i < ico.Length; i++)
        {
            if (i < mContentSprites.Length)
            {
                if (ico[i] == "0")
                {
                    mContentSprites[i].gameObject.SetActive(false);
                }
                else
                {
                    mContentSprites[i].spriteName = ico[i];
                    mContentSprites[i].gameObject.SetActive(true);
                }
            }
        }
        if (mContentTexture != null)
            mContentTexture.gameObject.SetActive(false);
    }

    public void SetCotent(Texture _contentTexture)
    {
        if (mContentTexture == null)
            return;

        mContentTexture.mainTexture = _contentTexture;
        mContentTexture.gameObject.SetActive(true);

        if (mContentSprites == null || mContentSprites.Length <= 0)
            return;
        for (int i = 0; i < mContentSprites.Length; i++)
        {
            if (mContentSprites[i] != null)
            {
                mContentSprites[i].gameObject.SetActive(false);
            }
        }
    }


    void OnClickItem()
    {
        if (UICamera.currentTouchID == -1)
        {
            if (mItemClick != null)
                mItemClick(mIndex);
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    ItemObject _itemObj = null;

    void OnTooltip(bool show)
    {

        if (ListItemType.mItem == mType)
        {
            if (show == true && mItemSample == null && mItemId != 0)
                mItemSample = new ItemSample(mItemId);
            else if (show == false)
                mItemSample = null;

            if (mItemSample != null)
            {
                //string  tipStr = PELocalization.GetString(mItemSample.protoData.descriptionStringId);

                //EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1=>EffSkill.MatchId(iterSkill1,mItemSample.mItemData.m_SkillID));
                //if(skill != null && skill.m_skillIdsGot != null)
                //{
                //    bool learnAllSkill = true;
                //    foreach(int id in skill.m_skillIdsGot)
                //    {
                //        if(!PlayerFactory.mMainPlayer.m_skillBook.m_mergeSkillIDs.Contains(id))
                //        {
                //            learnAllSkill = false;
                //            break;
                //        }
                //    }
                //    if(learnAllSkill)
                //        tipStr += PELocalization.GetString(4000001);
                //}

                //foreach(ItemProperty pro in Enum.GetValues(typeof(ItemProperty)))
                //{
                //    string replaceName = "$" + (int)pro + "$";
                //    switch(pro)
                //    {
                //    case ItemProperty.DamageReduceByShortAttack:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.DamageReduceByShortAttack))
                //            tipStr = tipStr.Replace(replaceName,((int)(mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.DamageReduceByShortAttack] * 100)).ToString() + "%");
                //        break;
                //    case ItemProperty.DamageReduceByProjectile:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.DamageReduceByProjectile))
                //            tipStr = tipStr.Replace(replaceName,((int)(mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.DamageReduceByProjectile] * 100)).ToString() + "%");
                //        break;
                //    case ItemProperty.ShieldEnergyCostFacter:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.ShieldEnergyCostFacter))
                //            tipStr = tipStr.Replace(replaceName,((int)(mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.ShieldEnergyCostFacter] * 100)).ToString() + "%");
                //        break;
                //    case ItemProperty.BatteryPower:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.BatteryPowerMax))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.BatteryPowerMax]).ToString());
                //        break;
                //    case ItemProperty.Durability:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.DurabilityMax))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.DurabilityMax]).ToString());
                //        break;
                //    case ItemProperty.ShieldEnergy:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.ShieldMax))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.ShieldMax]).ToString());
                //        break;
                //    default:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(pro))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[pro]).ToString());
                //        else
                //            tipStr = tipStr.Replace(replaceName, "0");
                //        break;
                //    }
                //}
                _itemObj = ItemMgr.Instance.CreateItem(mItemId);
                string tipStr = _itemObj.GetTooltip();
                ToolTipsMgr.ShowText(tipStr);

            }
            else
            {
                ItemMgr.Instance.DestroyItem(_itemObj);
                ToolTipsMgr.ShowText(null);
            }
        }
    }

}
