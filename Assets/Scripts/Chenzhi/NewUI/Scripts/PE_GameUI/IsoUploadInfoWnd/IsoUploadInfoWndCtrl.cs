using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathea;

public class IsoUploadInfoWndCtrl : MonoBehaviour {

    public enum UploadState
    {
        None=0,                 //空状态
        UploadPreViewFile=1,  //正在上传预览文件到steam workshop
        UploadIsoFile,        //正在上传iso主体文件到steam workshop
        SharingIso,           //正在steam workshop中共享iso
        OthePlayerDownload,   //等待其他玩家从steam workshop中自动下载iso
        ExportComplated,      //导出成功
        ExportFailed,         //导出失败，您与steam的连接可能已经断开，请尝试重新登录steam帐号并重新导出。
        NotEnoughMaterials    //没有足够的材料，无法完成！
    }

    [SerializeField]
    UIGrid m_Grid;
    [SerializeField]
    IsoUploadItem_N m_ItemPrefab;
    [SerializeField]
    UIScrollBar m_VScrollBar;
    [SerializeField]
    private UIButton m_HideAndShowBtn;
    [SerializeField]
    private TweenPosition m_Tween;

    public static IsoUploadInfoWndCtrl Instance { get; private set; }

    private Dictionary<int, IsoUploadItem_N> m_ItemDic = new Dictionary<int, IsoUploadItem_N>();
    private bool m_ToShow;

    #region mono methods

    void Awake()
    {
        if (PeGameMgr.IsMulti)
        {
            Instance = this;
            Init();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    #endregion

    #region private methods

    private void Init()
    {
        UIEventListener.Get(m_HideAndShowBtn.gameObject).onClick = HideAndShowBtnClick;
        m_Tween.onFinished = TweenFinishEvent;
        m_ToShow = false;
        UpdateHideAndShowBtnDirection();
    }

    private void HideAndShowBtnClick(GameObject go)
    {
        if (null != m_Tween)
        {
            m_Tween.Play(!m_ToShow);
            m_HideAndShowBtn.isEnabled = false;
        }
    }

    private void TweenFinishEvent(UITweener tween)
    {
        m_ToShow = !m_ToShow;
        UpdateHideAndShowBtnDirection();
        m_HideAndShowBtn.isEnabled = true;
    }

    private void UpdateHideAndShowBtnDirection()
    {
        m_HideAndShowBtn.transform.rotation = Quaternion.Euler(m_ToShow ? Vector3.zero : new Vector3(0, 0, 180));
    }

    private IsoUploadItem_N GetNewItem()
    {
        GameObject go=GameObject.Instantiate(m_ItemPrefab.gameObject);
        go.transform.parent = m_Grid.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        return go.GetComponent<IsoUploadItem_N>();
    }

    private void Reposition()
    {
        m_Grid.Reposition();
        if (m_VScrollBar.foreground.gameObject.activeSelf)
        {
            m_VScrollBar.scrollValue = 1f;
        }
        else
        {
            m_VScrollBar.scrollValue = 0f;
        }
    }

    private void DeleteItem(int id)
    {
        if (m_ItemDic.ContainsKey(id))
        {
            IsoUploadItem_N item = m_ItemDic[id];
            m_ItemDic.Remove(id);
            Destroy(item.gameObject);
            Invoke("Reposition", 0.1f);
        }
    }

    #endregion

    #region public methods

    public void UpdateIsoState(int id, string isoName, int step)
    {
        UploadState state = (step < 0 || step > (int)(UploadState.NotEnoughMaterials)) ? UploadState.None : (UploadState)step;
        if (m_ItemDic.ContainsKey(id))
        {
            m_ItemDic[id].UpdateStep(state);
        }
        else
        {
            IsoUploadItem_N item = GetNewItem();
            item.DelEvent = DeleteItem;
            item.UpdateInfo(id,isoName, state);
            m_ItemDic.Add(id,item);
            Reposition();
        }
    }

    public void ClearAll()
    {
        if (m_ItemDic.Count > 0)
        {
            foreach (var item in m_ItemDic)
            {
                if (null != item.Value)
                {
                    Destroy(item.Value.gameObject);
                }
            }
            m_ItemDic.Clear();
        }
    }

    #endregion

}
