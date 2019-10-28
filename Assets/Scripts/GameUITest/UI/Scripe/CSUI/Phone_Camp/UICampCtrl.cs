using UnityEngine;
using System.Collections;
using Pathea;

public class UICampCtrl : UIBaseWidget
{

    ReputationSystem RS;
    int MainPlayerId = 0;
    //bool canUpdate = false;
    bool hasrs = false;
    bool hasid = false;

    public UIGrid m_Root;
    public GameObject m_CampPrefab;

    [System.Serializable]
    public class Left_Camp
    {
        public UILabel CampName;
        public UILabel Relationship;
        public UILabel Introduction;

        public UILabel Reputation;
        public UISlider mSlider;


        public ReputationSystem.ReputationLevel mLevel;
        public int CurrentReputation;
        public int MaxReputation;

        public RelationInfo mRelationInfo;

        public GameObject warState;
        public GameObject peaceState;
        public GameObject trade;
        public GameObject specialMission;
        public GameObject normalMission;
        public UIGrid mGrid;
    }

    [SerializeField]
    Left_Camp m_Left;

    [System.Serializable]
    public class Right_Camp
    {
        public UILabel CampName;
        public UILabel Relationship;
        public UILabel Introduction;

        public UILabel Reputation;
        public UISlider mSlider;


        public ReputationSystem.ReputationLevel mLevel;
        public int CurrentReputation;
        public int MaxReputation;

        public RelationInfo mRelationInfo;

        public GameObject warState;
        public GameObject peaceState;
        public GameObject trade;
        public GameObject specialMission;
        public GameObject normalMission;
        public UIGrid mGrid;
    }

    [SerializeField]
    Right_Camp m_Right;

    public override void Show()
    {
        base.Show();
    }

    void TryGEtMainPlayerId()
    {
        if (PeCreature.Instance.mainPlayer == null)
            return;
        MainPlayerId = (int)PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID);
        hasid = true;
    }

    void TryGetReputationsys()
    {
        if (ReputationSystem.Instance == null)
            return;
        RS = ReputationSystem.Instance;
        hasrs = true;
    }



    void Update()
    {
        if (!hasid)
            TryGEtMainPlayerId();
        if (!hasrs)
            TryGetReputationsys();

        if (hasid && hasrs)
        {
            UpdateRelationship();

            UpdateReputation();

            UpdateRelationInfo();
        }
    }


    //关系

    void UpdateRelationship()
    {
        UpdatePujaRelationship();
        UpdatePajaRelationship();

    }

    void UpdatePujaRelationship()
    {
        m_Left.mLevel = RS.GetShowLevel(MainPlayerId, (int)ReputationSystem.TargetType.Puja);
        m_Left.Relationship.text = "Relationship:" + m_Left.mLevel.ToString();

    }
    void UpdatePajaRelationship()
    {
        m_Right.mLevel = RS.GetShowLevel(MainPlayerId, (int)ReputationSystem.TargetType.Paja);
        m_Right.Relationship.text = "Relationship:" + m_Right.mLevel.ToString();

    }

    //声望
    void UpdateReputation()
    {
        UpdatePujaReputation();
        UpdatePajaReputation();
    }

    void UpdatePujaReputation()
    {
        m_Left.CurrentReputation = RS.GetShowReputationValue(MainPlayerId, (int)ReputationSystem.TargetType.Puja);
		m_Left.MaxReputation = RS.GetShowLevelThreshold(MainPlayerId, (int)ReputationSystem.TargetType.Puja);
        m_Left.Reputation.text = m_Left.CurrentReputation.ToString() + "/" + m_Left.MaxReputation.ToString();
        m_Left.mSlider.sliderValue = (float)m_Left.CurrentReputation / m_Left.MaxReputation;
    }

    void UpdatePajaReputation()
    {
		m_Right.CurrentReputation = RS.GetShowReputationValue(MainPlayerId, (int)ReputationSystem.TargetType.Paja);
		m_Right.MaxReputation = RS.GetShowLevelThreshold(MainPlayerId, (int)ReputationSystem.TargetType.Paja);
        m_Right.Reputation.text = m_Right.CurrentReputation.ToString() + "/" + m_Right.MaxReputation.ToString();
        m_Right.mSlider.sliderValue = (float)m_Right.CurrentReputation / m_Right.MaxReputation;
    }


    //hint
    void UpdateRelationInfo()
    {
        UpdatePujaRelationInfo();
        UpdatePajaRelationInfo();
    }

    void UpdatePujaRelationInfo()
    {
        m_Left.mRelationInfo = RelationInfo.GetData(m_Left.mLevel);
    }

    void UpdatePajaRelationInfo()
    {
        m_Right.mRelationInfo = RelationInfo.GetData(m_Right.mLevel);
    }

    float mTimer = 0f;
    void LateUpdate()
    {
        UpdateHint();

        mTimer += Time.deltaTime;
        if (mTimer >= 0.5f)
        {
            mTimer = 0f;
            RepositionUIGrid();
        }
    }

    void UpdateHint()
    {
        UpdatePujaHint();
        UpdatePajaHint();
    }

    void UpdatePujaHint()
    {
        if (m_Left.mRelationInfo == null)
            return;

        m_Left.warState.SetActive(m_Left.mRelationInfo.warState);
        m_Left.peaceState.SetActive(!m_Left.mRelationInfo.warState);
        m_Left.trade.SetActive(m_Left.mRelationInfo.canUseShop);
        m_Left.normalMission.SetActive(m_Left.mRelationInfo.normalMission);
        m_Left.specialMission.SetActive(m_Left.mRelationInfo.specialMission);
    }

    void UpdatePajaHint()
    {
        if (m_Right.mRelationInfo == null)
            return;

        m_Right.warState.SetActive(m_Right.mRelationInfo.warState);
        m_Right.peaceState.SetActive(!m_Right.mRelationInfo.warState);
        m_Right.trade.SetActive(m_Right.mRelationInfo.canUseShop);
        m_Right.normalMission.SetActive(m_Right.mRelationInfo.normalMission);
        m_Right.specialMission.SetActive(m_Right.mRelationInfo.specialMission);
    }


    void RepositionUIGrid()
    {
        m_Left.mGrid.Reposition();
        m_Right.mGrid.Reposition();
    }

    ////////////////////////////////////////////

    void FireToPuja()
    {
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000171), OKToPuja);
    }

    void OKToPuja()
    {
        RS.TryChangeBelligerencyState(MainPlayerId, (int)ReputationSystem.TargetType.Puja, true);
    }

    void FireToPaja()
    {
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000171), OKToPaja);
    }

    void OKToPaja()
    {
		RS.TryChangeBelligerencyState(MainPlayerId, (int)ReputationSystem.TargetType.Paja, true);
    }
}
