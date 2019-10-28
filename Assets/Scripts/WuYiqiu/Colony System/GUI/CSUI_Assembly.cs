using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
public class CSUI_Assembly : MonoBehaviour
{
    public CSAssembly m_Assembly;
    public CSEntity m_Entity = null;

    private PeEntity player;
    private PlayerPackageCmpt playerPackageCmpt;

    // Upgrade
    [System.Serializable]
    public class UpgradePart
    {
        public UISlider m_Slider;
        public UILabel m_TimeLb;
        public N_ImageButton m_Button;

        public UIGrid m_Root;
        public CSUI_MaterialGrid m_MatGridPrefab;
        public UILabel m_LevelVal;
    }
    [SerializeField]
    private UpgradePart m_Upgrade;

    private List<CSUI_MaterialGrid> m_MatGrids = new List<CSUI_MaterialGrid>();
    private bool m_InitMatGrids = false;

    // Building Number
    [System.Serializable]
    public class BuildingNumPart
    {
        public UITable m_Root;
        public CSUI_BuildingNum m_BuildingNumPrefab;
    }
    [SerializeField]
    private BuildingNumPart m_BuildingNum;

    private List<CSUI_BuildingNum> m_BuildingNums = new List<CSUI_BuildingNum>();

    // Hide Shield Btn
    [SerializeField]
    private UICheckbox m_HideShieldCB;

    public void SetEntity(CSEntity enti)
    {
        if (enti == null)
        {
            Debug.LogWarning("Reference Entity is null.");
            return;
        }

        if (m_Assembly != null)
            m_Assembly.RemoveEventListener(AssemblyEventHandler);

        m_Assembly = enti as CSAssembly;

        m_Assembly.AddEventListener(AssemblyEventHandler);

        if (m_Assembly == null)
        {
            Debug.LogWarning("Reference Entity is not a Assembly Entity.");
            return;
        }
        m_Entity = enti;

        UpdateUpgradeMat();
        UpdateBuildingNum();


        m_HideShieldCB.isChecked = !m_Assembly.bShowShield;
    }

    // Update Upgrade material
    void UpdateUpgradeMat()
    {
        int[] items = m_Assembly.GetLevelUpItem();
        int[] itemCnts = m_Assembly.GetLevelUpItemCnt();

        if (!m_InitMatGrids)
            InitMatGrids();

        for (int i = 0; i < m_MatGrids.Count; i++)
        {
            if (i < items.Length)
            {
                m_MatGrids[i].ItemID = items[i];
                m_MatGrids[i].NeedCnt = itemCnts[i];
            }
            else
            {
                m_MatGrids[i].ItemID = 0;
            }
        }
    }

    void RepositionNow()
    {
        m_BuildingNum.m_Root.repositionNow = true;
    }

    void UpdateBuildingNum()
    {
        foreach (CSUI_BuildingNum b in m_BuildingNums)
            DestroyImmediate(b.gameObject);
        m_BuildingNums.Clear();

        foreach (KeyValuePair<CSConst.ObjectType, List<CSCommon>> kvp in m_Assembly.m_BelongObjectsMap)
        {
            CSUI_BuildingNum bn = Instantiate(m_BuildingNum.m_BuildingNumPrefab) as CSUI_BuildingNum;
            bn.transform.parent = m_BuildingNum.m_Root.transform;
            bn.transform.localRotation = Quaternion.identity;
            bn.transform.localPosition = Vector3.zero;
            bn.transform.localScale = Vector3.one;

            bn.m_Description = CSUtils.GetEntityName((int)(kvp.Key));
            bn.m_Count = kvp.Value.Count;
            bn.m_LimitCnt = m_Assembly.GetLimitCnt(kvp.Key);

            m_BuildingNums.Add(bn);
        }

        Invoke("RepositionNow", 0.1f);
        //m_BuildingNum.m_Root.repositionNow = true;

    }

    void UpdateUpgradeTime()
    {
        if (m_Assembly.isUpgrading)
        {

            float restTime = m_Assembly.Data.m_UpgradeTime - m_Assembly.Data.m_CurUpgradeTime;
            float percent = m_Assembly.Data.m_CurUpgradeTime / m_Assembly.Data.m_UpgradeTime;

            m_Upgrade.m_Slider.sliderValue = percent;
            m_Upgrade.m_TimeLb.text = CSUtils.GetRealTimeMS((int)restTime);
        }
        else
        {
            m_Upgrade.m_Slider.sliderValue = 0F;
            m_Upgrade.m_TimeLb.text = CSUtils.GetRealTimeMS(0);
        }
    }

    #region NGUI_CALLBACK

    void OnUpgradeBtn()
    {

        if (PeCreature.Instance.mainPlayer == null)
            return;

        ItemPackage accessor = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package._playerPak;

        //check start
        bool canUpgrade = true;
        bool bEnough = true;
        foreach (CSUI_MaterialGrid mg in m_MatGrids)
        {
            if (mg.ItemID != 0)
            {
                mg.ItemNum = playerPackageCmpt.package.GetCount(mg.ItemID);
                if (mg.ItemNum < mg.NeedCnt)
                {
                    bEnough = false;
                    new PeTipMsg(PELocalization.GetString(821000001), PeTipMsg.EMsgLevel.Warning);
                    break;
                }
            }
        }

        if (bEnough && !m_Assembly.isUpgrading && !m_Assembly.isDeleting)
            canUpgrade = true;
        else
            canUpgrade = false;

        if (m_Assembly.Level == m_Assembly.GetMaxLevel())
        {
            canUpgrade = false;
        }
        if (!canUpgrade)
        {
            return;
        }
        //check end

        if (!GameConfig.IsMultiMode)
        {
            foreach (CSUI_MaterialGrid mg in m_MatGrids)
            {
                if (mg.ItemID > 0)
                {
                    accessor.Destroy(mg.ItemID, mg.NeedCnt);
                    //package.DeleteItemWithItemID(mg.ItemID, mg.NeedCnt);
                    CSUI_MainWndCtrl.CreatePopupHint(mg.transform.position, this.transform, new Vector3(10, -2, -5), " - " + mg.NeedCnt.ToString(), false);
                }
            }

            m_Assembly.StartUpgradeCounter();

            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToUpgrade.GetString(), m_Entity.Name));
        }
        else
        {
            m_Assembly._ColonyObj._Network.ASB_LevelUp();
        }
    }

    void OnHideShield(bool active)
    {
        if (m_Assembly == null)
            return;
        if (!GameConfig.IsMultiMode)
        {
            m_Assembly.bShowShield = !active;
        }
        else
        {
            m_Assembly._ColonyObj._Network.ASB_HideShield(!active);
        }
    }

    #endregion

    #region ASSEMBLY_CALLBACK

    void AssemblyEventHandler(int event_id, CSEntity enti, object arg)
    {
        if (event_id == CSConst.eetAssembly_Upgraded)
        {
            UpdateBuildingNum();
            UpdateUpgradeMat();
        }
    }

    #endregion

    #region UNITY_INNER_FUNC

    void InitMatGrids()
    {
        for (int i = 0; i < 5; i++)
        {
            CSUI_MaterialGrid mg = Instantiate(m_Upgrade.m_MatGridPrefab) as CSUI_MaterialGrid;
            mg.transform.parent = m_Upgrade.m_Root.transform;
            mg.transform.localRotation = Quaternion.identity;
            mg.transform.localPosition = Vector3.zero;
            mg.transform.localScale = Vector3.one;

            mg.ItemID = -1;
            mg.NeedCnt = 0;

            m_MatGrids.Add(mg);
        }

        m_Upgrade.m_Root.repositionNow = true;
        m_InitMatGrids = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (m_Assembly == null)
            return;

        UpdateUpgradeTime();

        if (PeCreature.Instance.mainPlayer != null)
        {
            if (playerPackageCmpt == null || player == null || player != PeCreature.Instance.mainPlayer)
            {
                player = PeCreature.Instance.mainPlayer;
                playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
            }
            bool bEnough = true;

            foreach (CSUI_MaterialGrid mg in m_MatGrids)
            {
                if (mg.ItemID > 0)
                {
                    mg.ItemNum = playerPackageCmpt.package.GetCount(mg.ItemID);
                    if (mg.ItemNum < mg.NeedCnt)
                    {
                        bEnough = false;
                        continue;
                    }
                }
            }

            if (bEnough && !m_Assembly.isUpgrading && !m_Assembly.isDeleting && m_Assembly.Level != m_Assembly.GetMaxLevel())
                m_Upgrade.m_Button.isEnabled = true;
            else
                m_Upgrade.m_Button.isEnabled = false;

            int level = m_Assembly.Level + 1;
            if (m_Assembly.Level == m_Assembly.GetMaxLevel())
            {
                m_Upgrade.m_LevelVal.text = "LV " + level.ToString() + " (MAX)";
            }
            else
                m_Upgrade.m_LevelVal.text = "LV " + level.ToString();



        }

    }
    #endregion


    #region MultiMode
    public void UpgradeStartSuccuss(CSAssembly entity, string rolename)
    {
        if (m_Assembly == null && m_Assembly != entity)
        {
            return;
        }
        //popup material decreased
        if (PeCreature.Instance.mainPlayer.GetCmpt<EntityInfoCmpt>().characterName.givenName == rolename)
        {
            foreach (CSUI_MaterialGrid mg in m_MatGrids)
            {
                if (mg.ItemID > 0)
                {
                    CSUI_MainWndCtrl.CreatePopupHint(mg.transform.position, this.transform, new Vector3(10, -2, -5), " - " + mg.NeedCnt.ToString(), false);
                }
            }
        }
        CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToUpgrade.GetString(), m_Entity.Name));
    }
    #endregion
}
