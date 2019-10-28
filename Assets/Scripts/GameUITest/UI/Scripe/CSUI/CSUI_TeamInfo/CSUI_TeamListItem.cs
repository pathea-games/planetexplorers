using UnityEngine;
using System.Collections;

public class CSUI_TeamListItem : MonoBehaviour
{

    public UILabel m_team;
    public UILabel m_name;
    public UILabel m_killAndDeath;
    // public UILabel m_death;
    public UILabel m_score;
    public UISprite m_captainSpr;
    public UISprite m_AgreeSpr;
    public UISprite m_DisAgreeSpr;

    public BoxCollider mBoxCollider;

    public UISlicedSprite mCkSelectedBg;

    bool IsSelected = false;
    [HideInInspector]
    public int mIndex;
    [HideInInspector]
    public MyItemType mType;

    [HideInInspector]
    public PlayerNetwork mPnet;


    public delegate void OnCheckItem(int _index, MyItemType _type);
    public event OnCheckItem ItemChecked = null;

    public delegate void OnCheckItemPlayer(PlayerNetwork pnet);
    public event OnCheckItemPlayer ItemCheckedPlayer = null;


    public delegate void OnAgreementBtn(bool _isAgree, PlayerNetwork _mPnet);
    public event OnAgreementBtn OnAgreementBtnEvent;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetActive(bool isActive)
    {
        mBoxCollider.enabled = isActive;
    }

    public void SetInfo(int _team, int _kill, int _death, float _score)//Integration
    {
        SetActive(true);
        if (m_team != null)
            m_team.text = "Team" + _team.ToString();
        if (m_killAndDeath != null)
            m_killAndDeath.text = _kill.ToString() + "/" + _death.ToString();

        if (m_score != null)
            m_score.text = _score.ToString();
    }
    public void ClearInfo()
    {
        SetActive(false);
        if (m_team != null)
            m_team.text = "";
        if (m_name != null)
            m_name.text = "";
        if (m_killAndDeath != null)
            m_killAndDeath.text = "";
        if (m_score != null)
            m_score.text = "";
        if (m_captainSpr != null)
            m_captainSpr.enabled = false;
        if (m_AgreeSpr != null)
            m_AgreeSpr.gameObject.SetActive(false);
        if (m_DisAgreeSpr != null)
            m_DisAgreeSpr.gameObject.SetActive(false);
        mPnet = null;
        
    }

    public void SetInfo(int _team, string _name, int _kill, int _death, float _score, PlayerNetwork _pnet)//Players
    {
        SetActive(true);
        if (m_team != null)
            m_team.text = "Team" + _team.ToString();
        if (m_name != null)
            m_name.text = _name;
        if (m_killAndDeath != null)
            m_killAndDeath.text = _kill.ToString() + "/" + _death.ToString();
        if (m_score != null)
            m_score.text = _score.ToString();
        //if (m_captainSpr != null)
        //    m_captainSpr.enabled = _iscaptain;

        if (_pnet != null)
            mPnet = _pnet;
    }

    public void SetInfo(string _name, int _kill, int _death, float _score, PlayerNetwork _pnet)//Troops
    {
		if (-1 == PlayerNetwork.mainPlayer.TeamId || null == _pnet)
			return;

        SetActive(true);

        if (m_name != null)
            m_name.text = _name;

        if (m_killAndDeath != null)
            m_killAndDeath.text = _kill.ToString() + "/" + _death.ToString();

        if (m_score != null)
            m_score.text = _score.ToString();

		//if (m_captainSpr != null)
		//    m_captainSpr.enabled = _iscaptain;
		//if (m_AgreeSpr != null)
		//    m_AgreeSpr.enabled = _request;
		//if (m_DisAgreeSpr != null)
		//    m_DisAgreeSpr.enabled = _request;

		TeamData td = GroupNetwork.GetTeamInfo(PlayerNetwork.mainPlayer.TeamId);

		mPnet = _pnet;

		//队长的判断
		if (_pnet.Id == td.LeaderId)
		{
			if (m_captainSpr != null)
				m_captainSpr.enabled = true;
		}
		else if (_pnet.Id != td.LeaderId)
		{
			if (m_captainSpr != null)
				m_captainSpr.enabled = false;
		}

		//是不是申请人的判断
		if (PlayerNetwork.mainPlayer.Id == td.LeaderId && GroupNetwork.IsJoinRequest(_pnet))
		{
			if (m_AgreeSpr != null)
				m_AgreeSpr.gameObject.SetActive(true);
			if (m_DisAgreeSpr != null)
				m_DisAgreeSpr.gameObject.SetActive(true);
		}
		else
		{
			if (m_AgreeSpr != null)
				m_AgreeSpr.gameObject.SetActive(false);
			if (m_DisAgreeSpr != null)
				m_DisAgreeSpr.gameObject.SetActive(false);
		}
	}

    public void OnActivate(bool active)
    {
        //if(active)

    }
    public void SetSelected(bool _isSelected)
    {
        IsSelected = _isSelected;
        if (mCkSelectedBg.enabled != _isSelected)
            mCkSelectedBg.enabled = _isSelected;
    }

    private void OnChecked()
    {
        if (Input.GetMouseButtonUp(0) && IsSelected == false)
        {
            if (ItemChecked != null)
                ItemChecked(mIndex, mType);
            if (ItemCheckedPlayer != null)
                ItemCheckedPlayer(mPnet);
        }
    }


    private void OnAgreeBtn()
    {
        if (OnAgreementBtnEvent != null && mPnet != null)
            OnAgreementBtnEvent(true, mPnet);

		ClearInfo();
    }
    private void OnDisAgreeBtn()
    {
        if (OnAgreementBtnEvent != null && mPnet != null)
            OnAgreementBtnEvent(false, mPnet);

		ClearInfo();
	}

}
