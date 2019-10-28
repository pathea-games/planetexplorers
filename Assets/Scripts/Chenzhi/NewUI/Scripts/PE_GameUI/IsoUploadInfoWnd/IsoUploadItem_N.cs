using UnityEngine;
using System.Collections;
using System;

public class IsoUploadItem_N : MonoBehaviour {

    [SerializeField]
    UILabel m_IsoNameLb;
    [SerializeField]
    UISlider m_UploadProgres;
    [SerializeField]
    UISprite m_ProgresForeSprite;
    [SerializeField]
    UIButton m_DelBtn;
    [SerializeField]
    BoxCollider m_ShowTipCollider;
    [SerializeField]
    Color m_NormalCol;
    [SerializeField]
    Color m_FailedCol;
    [SerializeField]
    Color m_ComplatedCol;
    float m_DeleteWaitTime=5f;

    

    private ShowToolTipItem_N m_ShowToolTipItem;
    private bool m_ToolTipIsHover;
    private IsoUploadInfoWndCtrl.UploadState m_Step;
    public int ID { get; private set; }
    public Action<int> DelEvent;

    #region mono methods

    void Awake()
    {
        Init();
    }
    #endregion

    #region private methodss
    private void Init()
    {
        //m_DelBtn.gameObject.SetActive(false);
        UIEventListener.Get(m_DelBtn.gameObject).onClick = DelBtnClick;
        m_ShowToolTipItem = m_ShowTipCollider.gameObject.AddComponent<ShowToolTipItem_N>();
        UIEventListener.Get(m_ShowTipCollider.gameObject).onHover = ShowTipColliderOnHover;
    }

    private void DelBtnClick(GameObject go)
    {
        if (null != DelEvent)
        {
            DelEvent(ID);
            if (m_ToolTipIsHover)
            {
                UITooltip.ShowText(null);
            }
        }
    }

    private void ShowTipColliderOnHover(GameObject go,bool isHover)
    {
        m_ToolTipIsHover = isHover;
    }

    private void UpdateIsoName(string isoName)
    {
        m_IsoNameLb.text = isoName.ToString();
        m_IsoNameLb.MakePixelPerfect();
    }

    private int GetTipIDByStep(IsoUploadInfoWndCtrl.UploadState step)
    {
        switch (step)
        {
            case IsoUploadInfoWndCtrl.UploadState.UploadPreViewFile:
                return 8000963;
            case IsoUploadInfoWndCtrl.UploadState.UploadIsoFile:
                return 8000964;
            case IsoUploadInfoWndCtrl.UploadState.SharingIso:
                return 8000965;
            case IsoUploadInfoWndCtrl.UploadState.OthePlayerDownload:
                return 8000966;
            case IsoUploadInfoWndCtrl.UploadState.ExportComplated:
                return 8000967;
            case IsoUploadInfoWndCtrl.UploadState.ExportFailed:
                return 8000968;
            case IsoUploadInfoWndCtrl.UploadState.NotEnoughMaterials:
                return 821000001;
            default:
                return -1;
        }
    }

    private void UpdateToolTip(IsoUploadInfoWndCtrl.UploadState step)
    {
        int tipID = GetTipIDByStep(step);
        if (-1 != tipID)
        {
            m_ShowToolTipItem.mStrID = tipID;
            if (m_ToolTipIsHover)
            {
                UITooltip.ShowText(PELocalization.GetString(tipID));
            }
        }
    }

    private void UpdateProgres(IsoUploadInfoWndCtrl.UploadState step)
    {
        if (step == IsoUploadInfoWndCtrl.UploadState.ExportFailed|| step == IsoUploadInfoWndCtrl.UploadState.NotEnoughMaterials)
        {
            m_ProgresForeSprite.color = m_FailedCol;
            m_UploadProgres.sliderValue = 1f;
        }
        else if (step == IsoUploadInfoWndCtrl.UploadState.ExportComplated)
        {
            m_ProgresForeSprite.color = m_ComplatedCol;
            m_UploadProgres.sliderValue = 1f;
        }
        else if (step == IsoUploadInfoWndCtrl.UploadState.None)
        {
            m_ProgresForeSprite.color = m_NormalCol;
            m_UploadProgres.sliderValue = 0f;
        }
        else
        {
            m_ProgresForeSprite.color = m_NormalCol;
            m_UploadProgres.sliderValue = (float)step / (float)IsoUploadInfoWndCtrl.UploadState.ExportComplated;
        }
    }

    private void UpdateDelBtnState(IsoUploadInfoWndCtrl.UploadState step)
    {
        m_DelBtn.gameObject.SetActive(step == IsoUploadInfoWndCtrl.UploadState.ExportFailed);
    }

    private IEnumerator AutoDeleteIterator()
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup-startTime<m_DeleteWaitTime)
        {
            yield return null;
        }
        DelBtnClick(gameObject);
    }

    #endregion

    #region public methods

    public void UpdateInfo(int id, string isoName, IsoUploadInfoWndCtrl.UploadState step)
    {
        ID = id;
        UpdateIsoName(isoName);
        UpdateStep(step);
    }

    public void UpdateStep(IsoUploadInfoWndCtrl.UploadState step)
    {
        m_Step = step;
        UpdateProgres(m_Step);
        UpdateToolTip(m_Step);
        //UpdateDelBtnState(m_Step);
        if (step == IsoUploadInfoWndCtrl.UploadState.ExportComplated)
        {
            StartCoroutine(AutoDeleteIterator());
        }
    }

    #endregion
}
