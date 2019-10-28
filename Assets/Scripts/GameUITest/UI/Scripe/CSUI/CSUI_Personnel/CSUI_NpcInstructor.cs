using UnityEngine;
using System.Collections;

public class CSUI_NpcInstructor : MonoBehaviour
{

    [SerializeField]
    private UILabel m_InstructorLabel;

    [SerializeField]
    private UILabel m_TraineeLabel;


    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            m_RefNpc = value;
        }
    }


    public void Init()
    {

    }

    void OnEnable()
    {
        if (onLabelChanged != null)
            onLabelChanged();
    }

    private bool m_Active = true;
    public void Activate(bool active)
    {
        if (m_Active != active)
        {
            m_Active = active;
            _activate();
        }
        else
            m_Active = active;
    }

    private void _activate()
    {
        if (m_Active)
        {

        }
    }

    public void OnInstructorBtn()
    {
        if (onInstructorClicked != null && m_RefNpc != null)
            onInstructorClicked(m_RefNpc);
        if (onLabelChanged != null)
            onLabelChanged();
    }

    public void OnTraineeBtn()
    {
        if (onTraineeClicked != null && m_RefNpc != null)
            onTraineeClicked(m_RefNpc);
        if (onLabelChanged != null)
            onLabelChanged();
    }

    

    #region Event

    public event System.Action<CSPersonnel> onInstructorClicked;
    public event System.Action<CSPersonnel> onTraineeClicked;

    public event System.Action onLabelChanged;

    #endregion

    #region Interface

    public void SetCountLabel(int _insCurrentCnt,int _insMaxCnt,int _traCurrentCtn,int _traMaxCnt)
    {
        if (m_InstructorLabel != null)
            m_InstructorLabel.text = "[" + _insCurrentCnt.ToString() + "/" + _insMaxCnt.ToString() + "]";
        if(m_TraineeLabel!=null)
            m_TraineeLabel.text = "[" + _traCurrentCtn.ToString() + "/" + _traMaxCnt.ToString() + "]";
    }

    #endregion
}
