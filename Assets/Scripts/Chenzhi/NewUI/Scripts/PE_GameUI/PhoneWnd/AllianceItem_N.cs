using UnityEngine;
using System.Collections;
using System;
using Pathea;
using PeMap;

public class AllianceItem_N : MonoBehaviour
{
    [SerializeField]
    private UISprite m_IconSpr;
    [SerializeField]
    private UILabel m_AllianceNameLbl;
    [SerializeField]
    private UISprite m_SurplusSpr;
    [SerializeField]
    private UILabel m_SurplusCountLbl;
    [SerializeField]
    private UISprite m_DestorySpr;
    [SerializeField]
    private UILabel m_DestoryCountLbl;
    [SerializeField]
    private UISlider m_ReputationSlider;
    [SerializeField]
    private UILabel m_ReputationNumLbl;
    [SerializeField]
    private UILabel m_ReputationLvLbl;
    [SerializeField]
    private UIButton m_FightBtn;

    private int m_AllianceID;
    private int m_PlayerID;
    private int m_MainPlayerID;

    private UIPanel m_CurPanel;

    #region mono methods

    void Start()
    {
        UIEventListener.Get(m_FightBtn.gameObject).onClick += (go) =>
        {
            ReputationSystem.Instance.TryChangeBelligerencyState(m_MainPlayerID, m_PlayerID, true);
        };
    }

    #endregion

    #region private methods

    private void UpdateAllyIcon()
    {
        string iconName="Null";
        AllyType type = VATownGenerator.Instance.GetAllyType(m_AllianceID);
        int index = VATownGenerator.Instance.GetAllyNum(m_AllianceID);
        if (index >= 0)
        {
            switch (type)
            {
                case AllyType.Npc:
                case AllyType.Player:
                    if (index < AllyIcon.HummanIcon.Length)
                    {
                        iconName = AllyIcon.HummanIcon[index];
                    }
                    break;
                case AllyType.Puja:
                    if (index < AllyIcon.PujaIcon.Length)
                    {
                        iconName = AllyIcon.PujaIcon[index];
                    }
                    break;
                case AllyType.Paja:
                    if (index < AllyIcon.PajaIcon.Length)
                    {
                        iconName = AllyIcon.PajaIcon[index];
                    }
                    break;
            }
        }
        m_IconSpr.spriteName = iconName;
        m_IconSpr.MakePixelPerfect();
    }

    private void UpdateSurplusBulidIcon()
    {
        AllyType type = VATownGenerator.Instance.GetAllyType(m_AllianceID);
        int iconID = -1;
        switch (type)
        {
            case AllyType.Npc:
            case AllyType.Player:
                iconID = MapIcon.AdventureCamp;
                break;
            case AllyType.Puja:
                iconID = MapIcon.PujaBase;
                break;
            case AllyType.Paja:
                iconID = MapIcon.PajaBase;
                break;
        }
        if (iconID != -1)
        {
            PeMap.MapIcon mapIcon = PeMap.MapIcon.Mgr.Instance.iconList.Find(itr => (itr.id == iconID));
            m_SurplusSpr.spriteName = mapIcon != null ? mapIcon.iconName : "Null";
            m_SurplusSpr.MakePixelPerfect();
        }
    }

    private void UpdateDestoryBulidIcon()
    {
        AllyType type = VATownGenerator.Instance.GetAllyType(m_AllianceID);
        int iconID = -1;
        switch (type)
        {
            case AllyType.Npc:
            case AllyType.Player:
                iconID = MapIcon.HumanBrokenBase;
                break;
            case AllyType.Puja:
                iconID = MapIcon.PujaBrokenBase;
                break;
            case AllyType.Paja:
                iconID = MapIcon.PajaBrokenBase;
                break;
        }
        if (iconID != -1)
        {
            PeMap.MapIcon mapIcon = PeMap.MapIcon.Mgr.Instance.iconList.Find(itr => (itr.id == iconID));
            m_DestorySpr.spriteName = mapIcon != null ? mapIcon.iconName : "Null";
            m_DestorySpr.MakePixelPerfect();
        }
    }

    private void UpdateIconCol()
    {
        int colIndex = VATownGenerator.Instance.GetAllyColor(m_AllianceID);
        if (colIndex < 0 || colIndex >= AllyColor.AllianceCols.Length)
            return;
        Color32 color32 = AllyColor.AllianceCols[colIndex];
        Color col = new Color(color32.r / 255.0f, color32.g / 255.0f, color32.b / 255.0f, color32.a / 255.0f);
        m_IconSpr.color = col;
        m_SurplusSpr.color = col;
        m_DestorySpr.color = col;
    }

    private void UpdateName()
    {
        //lz-2016.10.12 通过接口取名字的翻译ID
        int nameID = VATownGenerator.Instance.GetAllyName(m_AllianceID);
        m_AllianceNameLbl.text = PELocalization.GetString(nameID);
        m_AllianceNameLbl.MakePixelPerfect();
    }

    private void UpdateReputationProgress(float progress)
    {
        m_ReputationSlider.sliderValue = Mathf.Clamp01(progress);
    }

    private void UpdateReputationNum(float number)
    {
        m_ReputationNumLbl.text = number.ToString();
        m_ReputationNumLbl.MakePixelPerfect();
    }

    private void UpdateReputationLv(ReputationSystem.ReputationLevel level)
    {
        int strID=-1;
        switch (level)
        {
            case ReputationSystem.ReputationLevel.Fear:
                strID = 8000704;
                break;
            case ReputationSystem.ReputationLevel.Hatred:
                strID = 8000705;
                break;
            case ReputationSystem.ReputationLevel.Animosity:
                strID = 8000706;
                break;
            case ReputationSystem.ReputationLevel.Cold:
                strID = 8000707;
                break;
            case ReputationSystem.ReputationLevel.Neutral:
                strID = 8000708;
                break;
            case ReputationSystem.ReputationLevel.Cordial:
                strID = 8000709;
                break;
            case ReputationSystem.ReputationLevel.Amity:
                strID = 8000710;
                break;
            case ReputationSystem.ReputationLevel.Respectful:
                strID = 8000711;
                break;
            case ReputationSystem.ReputationLevel.Reverence:
                strID = 8000712;
                break;
            case ReputationSystem.ReputationLevel.MAX:
                return;
        }
        if (strID != -1)
        {
            m_ReputationLvLbl.text = PELocalization.GetString(strID);
            m_ReputationLvLbl.MakePixelPerfect();
        }
    }

    private void UpdateFightBtnState()
    {
        m_FightBtn.isEnabled = !ReputationSystem.Instance.GetBelligerency(m_MainPlayerID,m_PlayerID);
    }

    private void GetPanelCmpt()
    {
        UIDraggablePanel draggablePanel = NGUITools.FindInParents<UIDraggablePanel>(this.gameObject);
        if (null != draggablePanel)
        {
            this.m_CurPanel = draggablePanel.GetComponent<UIPanel>();
            draggablePanel.onDragFinished += OnDragFinishEvent;
        }
    }

    private void OnDragFinishEvent()
    {
        if (null != this.m_CurPanel)
        {
            bool isVisible= m_CurPanel.IsVisible(this.transform.position);
            BoxCollider[] collider = transform.GetComponentsInChildren<BoxCollider>(true);
            for (int i = 0; i < collider.Length; i++)
            {
                collider[i].enabled = isVisible;
            }
        }
    }

    #endregion

    #region public methods

    public void UpdateInfo(int allyId,int playerID,int mainPlayerID)
    {
        m_AllianceID = allyId;
        m_PlayerID = playerID;
        m_MainPlayerID = mainPlayerID;
        UpdateName();
        UpdateBuildCount();
        UpdateReputation();
        UpdateIconCol();
        UpdateIconCol();
        UpdateAllyIcon();
        UpdateSurplusBulidIcon();
        UpdateDestoryBulidIcon();
        GetPanelCmpt();
    }

    public void UpdateBuildCount()
    {
        int allCount = VATownGenerator.Instance.GetAllyTownCount(m_AllianceID);
        int destroyCount= VATownGenerator.Instance.GetAllyTownDestroyedCount(m_AllianceID);
        m_SurplusCountLbl.text = (allCount- destroyCount).ToString();
        m_SurplusCountLbl.MakePixelPerfect();
        m_DestoryCountLbl.text = destroyCount.ToString();
        m_DestoryCountLbl.MakePixelPerfect();
    }

    public void UpdateReputation()
    {
        int curShowReputation=ReputationSystem.Instance.GetShowReputationValue(m_MainPlayerID, m_PlayerID);
        int maxReputation = ReputationSystem.Instance.GetShowLevelThreshold(m_MainPlayerID, m_PlayerID);
        ReputationSystem.ReputationLevel reputationLevel = ReputationSystem.Instance.GetShowLevel(m_MainPlayerID, m_PlayerID);
        UpdateReputationProgress(((float)curShowReputation) / maxReputation);
        UpdateReputationNum(curShowReputation);
        UpdateReputationLv(reputationLevel);
        UpdateFightBtnState();
    }

    public void Reset()
    {
        m_AllianceID = -1;
        m_PlayerID = -1;
        m_IconSpr.spriteName = "Null";
        m_SurplusSpr.spriteName = "Null";
        m_DestorySpr.spriteName = "Null";
        m_AllianceNameLbl.text = "";
        m_SurplusCountLbl.text = "";
        m_DestoryCountLbl.text = "";
        m_ReputationSlider.sliderValue = 0;
        m_ReputationNumLbl.text = "";
        m_ReputationLvLbl.text = "";
    }

    #endregion
}
