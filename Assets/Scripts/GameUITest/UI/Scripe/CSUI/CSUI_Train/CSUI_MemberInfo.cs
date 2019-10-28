using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using System;
using Pathea;
using Pathea.PeEntityExt;

public class CSUI_MemberInfo : MonoBehaviour
{


    #region UI_WIDGET

    //	[SerializeField] UILabel		m_NameUI;
    //	[SerializeField] UILabel		m_LifeUI;
    //	[SerializeField] UISlider		m_LifeProgressUI;
    //	[SerializeField] UILabel		m_StaminaUI;
    //	[SerializeField] UISlider		m_StaminaProgressUI;
    //	[SerializeField] UILabel		m_OxygenUI;
    //	[SerializeField] UISlider		m_OxygenProgressUI;
    //	[SerializeField] UILabel		m_ShieldUI;
    //	[SerializeField] UILabel		m_EnergyUI;
    //	[SerializeField] UILabel		m_AtkUI;
    //	[SerializeField] UILabel		m_DefUI;


    [SerializeField]
    UILabel mLbName;
    [SerializeField]
    UISprite mSprSex;
    [SerializeField]
    UILabel mLbHealth;
    [SerializeField]
    UISlider mSdHealth;
    [SerializeField]
    UILabel mLbStamina;
    [SerializeField]
    UISlider mSdStamina;
    [SerializeField]
    UILabel mLbHunger;
    [SerializeField]
    UISlider mSdHunger;
    [SerializeField]
    UILabel mLbComfort;
    [SerializeField]
    UISlider mSdComfort;
    [SerializeField]
    UILabel mLbOxygen;
    [SerializeField]
    UISlider mSdOxygen;
    [SerializeField]
    UILabel mLbShield;
    [SerializeField]
    UISlider mSdShield;
    [SerializeField]
    UILabel mLbEnergy;
    [SerializeField]
    UISlider mSdEnergy;
    [SerializeField]
    UILabel mLbAttack;
    [SerializeField]
    UILabel mLbDefense;


    [SerializeField]
    UIGrid m_ItemRoot;
    [SerializeField]
    UIGrid m_SkillRoot;

    // NPC InfoPage
    [SerializeField]
    GameObject m_InfoPage;
    [SerializeField]
    GameObject m_InventoryPage;
    //[SerializeField]
    //GameObject m_WorkPage;


    #endregion

    public CSUI_Grid m_GridPrefab;

    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            m_RefNpc = value;
            UpdateItemGrid();
            UpdateSkills();
        }
    }

    private List<CSUI_Grid> m_ItemGrids = new List<CSUI_Grid>();
    private List<CSUI_Grid> m_SkillGrids = new List<CSUI_Grid>();

    // Use this for initialization
    void Start()
    {
        // Create Item Grid
        for (int i = 0; i < 10; i++)
        {
            CSUI_Grid grid = Instantiate(m_GridPrefab) as CSUI_Grid;
            grid.transform.parent = m_ItemRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;

            grid.m_Active = true;
            grid.m_UseDefaultExchangeDel = false;
            grid.m_Index = i;
            m_ItemGrids.Add(grid);
            grid.onCheckItem = OnCheckItemGrid;
            grid.OnItemChanged = OnItemGridChanged;
            grid.m_Grid.onGridsExchangeItem = OnGridsExchangeItem;
        }

        m_ItemRoot.repositionNow = true;

        // Create skill Grid
        for (int i = 0; i < 5; i++)
        {
            CSUI_Grid grid = Instantiate(m_GridPrefab) as CSUI_Grid;
            grid.transform.parent = m_SkillRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;

            grid.m_Active = false;
            grid.m_Index = i;
            m_SkillGrids.Add(grid);
        }
        m_SkillRoot.repositionNow = true;
    }

    // Update is called once per frame
    void Update()
    {
        //		if (m_RefNpc == null)
        //		{
        //			m_NameUI.text		= "";
        //			m_LifeUI.text		= "0/0";
        //			m_StaminaUI.text	= "0/0";
        //			m_OxygenUI.text		= "0/0";
        //			m_ShieldUI.text		= "0/0";
        //			m_EnergyUI.text		= "0/0";
        //			m_AtkUI.text		= "";
        //			m_DefUI.text		= "";
        //			m_LifeProgressUI.sliderValue = 1;
        //			m_StaminaProgressUI.sliderValue = 1;
        //			m_OxygenProgressUI.sliderValue = 1;
        //			return;
        //		}
        //
        //
        //		AiNpcObject npc = m_RefNpc.m_Npc;
        //		m_NameUI.text		= npc.NpcName.ToString();
        //		m_LifeUI.text		= npc.life.ToString() + "/" + npc.maxLife.ToString();
        //		m_LifeProgressUI.sliderValue = npc.maxLife == 0? 0: npc.life / npc.maxLife;
        //		m_StaminaUI.text    = ((int)m_RefNpc.Stamina).ToString() + "/" + ((int)m_RefNpc.MaxStamina).ToString();
        //		m_StaminaProgressUI.sliderValue = npc.maxLife == 0? 0 : m_RefNpc.Stamina / m_RefNpc.MaxStamina;
        //		m_ShieldUI.text		= "0/0";
        //		m_EnergyUI.text 	= "0/0";
        //		m_AtkUI.text		= npc.damage.ToString();
        //		m_DefUI.text		= npc.defence.ToString();
        //
        //
        //		m_OxygenUI.text = npc.oxygen.ToString() + "/" + npc.maxOxygen.ToString();
        //		m_OxygenProgressUI.sliderValue = npc.maxOxygen == 0 ? 0 : npc.oxygen / npc.maxOxygen;


        if (m_RefNpc == null || m_RefNpc.NPC == null)
        {
            SetServantInfo("--", PeSex.Male, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
        // playerInfo
        else
        {
            SetServantInfo(m_RefNpc.FullName, m_RefNpc.Sex, (int)m_RefNpc.GetAttribute(AttribType.Hp), (int)m_RefNpc.GetAttribute(AttribType.HpMax), (int)m_RefNpc.Stamina, (int)m_RefNpc.MaxStamina, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (int)m_RefNpc.GetAttribute(AttribType.CutDamage), (int)m_RefNpc.GetAttribute(AttribType.Def));
        }

    }

    void UpdateItemGrid()
    {
        if (m_ItemGrids.Count == 0)
            return;

        for (int i = 0; i < 10; i++)
            m_ItemGrids[i].m_Grid.SetItem(null);

        if (m_RefNpc != null)
        {
            //--to do: wait npcpackage
            //int count = Mathf.Min(10, m_RefNpc.m_Npc.GetBagItemCount());
            //for (int i = 0; i < count; i++)
            //    m_ItemGrids[i].m_Grid.SetItem(m_RefNpc.m_Npc.GetBagItem(i));
        }

    }

    void UpdateSkills()
    {
       

        if (m_RefNpc == null || m_RefNpc.NPC == null)
            return;


        if (!m_RefNpc.IsRandomNpc)
            return;

		if(m_RefNpc.SkAlive == null)
			return;
		if(m_RefNpc.Npcabliys != null)
		{
			int index = 0;
			foreach(NpcAbility abity in m_RefNpc.Npcabliys)
			{
				if(abity != null)
				{
					m_SkillGrids[index].m_Grid.SetSkill(abity.skillId, index,m_RefNpc.SkAlive,abity.icon,abity.desc);
					index++;
				}
			}
		}
		else
		{
			for(int i=0;i<m_SkillGrids.Count;i++)
			{
				m_SkillGrids[i].m_Grid.SetSkill(0,i);
			}
		}

        //--to do: wait
        //// Skill
        //if (npcRandom.mRandomSkillList != null)
        //{
        //    List<int> skillList = npcRandom.mRandomSkillList;
        //    for (int i = 0; i < skillList.Count; i++)
        //    {
        //        if(i >= m_SkillGrids.Count)
        //            break;

        //        EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillList[i]));
        //        if (skill == null)
        //            continue;

        //        m_SkillGrids[i].m_Grid.SetSkill(skillList[i], i, skill.m_name[1]);
        //    }
        //}
    }


    void SetServantInfo(string name, PeSex sex, int health, int healthMax, int stamina, int stamina_max, int hunger, int hunger_max, int comfort, int comfort_max,
                               int oxygen, int oxygen_max, int shield, int shield_max, int energy, int energy_max, int attack, int defense)
    {
        mLbName.text = name;
        mSprSex.spriteName = sex == PeSex.Male ? "man" : "woman";

        mLbHealth.text = Convert.ToString(health) + "/" + Convert.ToString(healthMax);
        mSdHealth.sliderValue = (healthMax > 0) ? Convert.ToSingle(health) / healthMax : 0;

        mLbStamina.text = Convert.ToString(stamina) + "/" + Convert.ToString(stamina_max);
        mSdStamina.sliderValue = (stamina_max > 0) ? Convert.ToSingle(stamina) / stamina_max : 0;

        mLbHunger.text = Convert.ToString(hunger) + "/" + Convert.ToString(hunger_max);
        mSdHunger.sliderValue = (hunger_max > 0) ? Convert.ToSingle(hunger) / hunger_max : 0;

        mLbComfort.text = Convert.ToString(comfort) + "/" + Convert.ToString(comfort_max);
        mSdComfort.sliderValue = (comfort_max > 0) ? Convert.ToSingle(comfort) / comfort_max : 0;

        mLbOxygen.text = Convert.ToString(oxygen) + "/" + Convert.ToString(oxygen_max);
        mSdOxygen.sliderValue = (oxygen_max > 0) ? Convert.ToSingle(oxygen) / oxygen_max : 0;

        mLbShield.text = Convert.ToString(shield) + "/" + Convert.ToString(shield_max);
        mSdShield.sliderValue = (shield_max > 0) ? Convert.ToSingle(shield) / shield_max : 0;

        mLbEnergy.text = Convert.ToString(energy) + "/" + Convert.ToString(energy_max);
        mSdEnergy.sliderValue = (energy_max > 0) ? Convert.ToSingle(energy) / energy_max : 0;

        mLbAttack.text = Convert.ToString(attack);
        mLbDefense.text = Convert.ToString(defense);

    }

    #region CALL_BACKE

    bool OnCheckItemGrid(ItemObject item, CSUI_Grid.ECheckItemType check_type)
    {
        if (m_RefNpc == null)
            return false;

        return true;
    }

    void OnItemGridChanged(ItemObject item, ItemObject oldItem, int index)
    {
        //--to do: wait npcpackage
        //if (oldItem != null)
        //    RefNpc.m_Npc.RemoveFromBag(oldItem);

        //if (item != null)
        //    RefNpc.m_Npc.AddToBag(item);

        if (oldItem != null)
        {
            if (item == null)
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayItemFromNpc.GetString(), oldItem.protoData.GetName(), m_RefNpc.FullName));
            else if (item == oldItem)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
            else
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
        }
        else if (item != null)
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
    }



    void OnGridsExchangeItem(Grid_N grid, ItemObject item)
    {
        //--to do: wait npcpackage
        //if (grid.ItemObj != null)
        //    RefNpc.m_Npc.RemoveFromBag(grid.ItemObj);

        //if (item != null)
        //    RefNpc.m_Npc.AddToBag(item);

        grid.SetItem(item);
    }

    void PageInfoOnActive(bool active)
    {
        m_InfoPage.SetActive(active);
    }
    void PageInvetoryOnActive(bool active)
    {
        m_InventoryPage.SetActive(active);
    }
    //void PageWorkOnActive(bool active)
    //{
    //    m_WorkPage.SetActive(active);
    //}


    #endregion
}
