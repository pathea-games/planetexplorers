using UnityEngine;
using System.Collections;

public class ParticipantItem : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_LeftLabel;
    [SerializeField]
    private UILabel m_CenterLabel;
    [SerializeField]
    private UILabel m_RightLabel;

    #region mono methods

    void Awake()
    {
        this.ResetItem();
    }

    #endregion

    #region public methods

    public void UpdateLeft(string info)
    {
        this.ResetItem();
        this.m_LeftLabel.text = info;
        Destroy(this.m_CenterLabel.gameObject);
        Destroy(this.m_RightLabel.gameObject);
    }

    public void UpdateCenter(string info)
    {
        this.ResetItem();
        this.m_CenterLabel.text = info;
        Destroy(this.m_LeftLabel.gameObject);
        Destroy(this.m_RightLabel.gameObject);
    }

    public void UpdateLeftAndRight(string leftInfo, string rightInfo)
    {
        this.ResetItem();
        this.m_LeftLabel.text = leftInfo;
        this.m_RightLabel.text = rightInfo;
        Destroy(this.m_CenterLabel.gameObject);
    }

    #endregion

    #region private methods

    void ResetItem()
    {
        this.m_LeftLabel.text = "";
        this.m_CenterLabel.text = "";
        this.m_RightLabel.text = "";
    }
    #endregion
}
