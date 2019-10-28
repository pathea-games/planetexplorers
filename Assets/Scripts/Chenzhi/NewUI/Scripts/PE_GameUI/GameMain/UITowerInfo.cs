using UnityEngine;
using System.Collections;


public class UITowerInfo : UIStaticWnd 
{
	static UITowerInfo 							mInstance;
	public static UITowerInfo 					Instance{ get{ return mInstance; } }

	[SerializeField] UILabel mLbPrepTime;
	[SerializeField] UILabel mLbMonsterNum;
    [SerializeField] UILabel mLbRemainTime;
    [SerializeField] UILabel mLbWavesRemaining;
    [SerializeField] Color32 LbPrepTimeColor;
    [SerializeField] Color32 LbMonsterNumColor;
    [SerializeField] Color32 LbRemainTimeColor;
    [SerializeField] Color32 LbWavesRemainingColor;
    [SerializeField] string m_TitleSpece;

    string m_PrepTimeTitleStr;
    string m_MonsterNumTitleStr;
    string m_RemainTimeStr;
    string m_WavesRemainingTitleStr;
    string m_PrepTimeColorStr;
    string m_MonsterNumColorStr;
    string m_RemainTimeColorStr;
    string m_WavesRemainingColorStr;
    string m_ColorEnd;
    int m_PrepTimeTitleID;

    UTimer m_UTimer;
    TowerInfoUIData m_info = null;

    void Awake()
    {
        m_PrepTimeColorStr = ConvertColor32ToHexStr(LbPrepTimeColor);
        m_MonsterNumColorStr = ConvertColor32ToHexStr(LbMonsterNumColor);
        m_RemainTimeColorStr = ConvertColor32ToHexStr(LbRemainTimeColor);
        m_WavesRemainingColorStr = ConvertColor32ToHexStr(LbWavesRemainingColor);

        m_ColorEnd = "[-]";
        m_PrepTimeTitleID = 8000602;
        m_PrepTimeTitleStr = PELocalization.GetString(m_PrepTimeTitleID) + m_TitleSpece + m_PrepTimeColorStr;
        m_MonsterNumTitleStr = PELocalization.GetString(8000601) + m_TitleSpece + m_MonsterNumColorStr;
        m_RemainTimeStr = PELocalization.GetString(8000607) + m_TitleSpece + m_RemainTimeColorStr;
        m_WavesRemainingTitleStr = PELocalization.GetString(8000608) + m_TitleSpece + m_WavesRemainingColorStr; 
        
        
    }

    string ConvertColor32ToHexStr(Color32 color)
    {
        string str="[";
        string tempStr="";

        tempStr= System.Convert.ToString(color.r, 16);
        if(tempStr.Length==1)
            tempStr="0"+tempStr;
        str += tempStr;

        tempStr = System.Convert.ToString(color.g, 16);
        if (tempStr.Length == 1)
            tempStr = "0" + tempStr;
        str += tempStr;

        tempStr = System.Convert.ToString(color.b, 16);
        if (tempStr.Length == 1)
            tempStr = "0" + tempStr;
        str += tempStr;

        str += "]";
        return str;
    }

    public void SetInfo(TowerInfoUIData info)
    {
        m_info = info;
    }

	public override void OnCreate ()
	{
		base.OnCreate ();
		mInstance = this;
        m_UTimer = new UTimer();
	}

    public void SetPrepTime( float preTime)
	{
        if (preTime >= 0)
        {
            m_UTimer.Second = preTime;
            mLbPrepTime.text = m_PrepTimeTitleStr + m_UTimer.FormatString("mm:ss") + m_ColorEnd;
        }
        else
        {
            mLbPrepTime.text = m_PrepTimeTitleStr + "--:--" + m_ColorEnd;
        }
	}

    public void SetWavesRemaining(int curWavesRemaining, int totalWaves)
    {
        int prepTimeTitleID = (curWavesRemaining == totalWaves) ? 8000602 : 8000606;
        if (m_PrepTimeTitleID != prepTimeTitleID)
        {
            m_PrepTimeTitleID=prepTimeTitleID;
            m_PrepTimeTitleStr = PELocalization.GetString(m_PrepTimeTitleID) + m_TitleSpece + m_PrepTimeColorStr;
        }
        if (curWavesRemaining>=0)
        {
            this.mLbWavesRemaining.text = m_WavesRemainingTitleStr + curWavesRemaining.ToString() + m_ColorEnd;
        }
    }

	void SetMonsterNum( int curCount, int maxCount)
	{
        if (maxCount > 0)
        {
            mLbMonsterNum.text = m_MonsterNumTitleStr + curCount.ToString() + "/" + maxCount.ToString() + m_ColorEnd;
        }
        else
        {
            //lz-2016.08.08 如果杀怪总数量为0，不显示分母
            mLbMonsterNum.text = m_MonsterNumTitleStr + curCount.ToString()+m_ColorEnd;
        }
	}

    void SetRemainTime(float remainTime)
    {
        if (remainTime > 0)
        {
            m_UTimer.Second = remainTime;
            mLbRemainTime.text = m_RemainTimeStr + m_UTimer.FormatString("mm:ss") + m_ColorEnd;
        }
        else
        {
            mLbRemainTime.text = m_RemainTimeStr + "--:--" + m_ColorEnd;
        }
    }


	public event OnGuiBtnClicked e_BtnReady = null;
	void BtnReady_OnClick()
	{
        if (!EntityMonsterBeacon.IsController())
            return;
		if (e_BtnReady != null)
			e_BtnReady();
	}

    void Update()
    {
        if (m_info == null)
            return;
        if(Pathea.PeGameMgr.IsSingle)
            SetMonsterNum(m_info.curCount(), m_info.MaxCount);
        else
            SetMonsterNum(m_info.CurCount, m_info.MaxCount);
        SetRemainTime(m_info.RemainTime);
        SetPrepTime(m_info.PreTime);
        SetWavesRemaining(m_info.CurWavesRemaining, m_info.TotalWaves);
    }
}
