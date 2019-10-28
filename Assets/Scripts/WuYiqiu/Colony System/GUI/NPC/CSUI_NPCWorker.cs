using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_NPCWorker : MonoBehaviour
{
    #region UI_WIDGET

    [SerializeField]
    UIGrid m_WorkRoomRootUI;

    [SerializeField]
    UIPopupList m_ModeUI;
    [SerializeField]
    UISprite m_NormalModeUI;
    [SerializeField]
    UISprite m_WorkWhenNeedUI;
    [SerializeField]
    UISprite m_WorkaholicUI;

    #endregion

    [SerializeField]
    CSUI_WorkRoom WorkRoomUIPrefab;

    private List<CSUI_WorkRoom> m_WorkRooms = new List<CSUI_WorkRoom>();

    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            m_RefNpc = value;
            UpdateWorkRoom();
            UpdateModeUI();
        }
    }

    public delegate void SelectItemDel(string item);
    public SelectItemDel onSelectChange;

    #region ACTIVE_PART

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
        if (!m_Active)
        {
            m_ModeUI.items.Clear();
            if (m_RefNpc != null)
                m_ModeUI.items.Add(CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode));
            else
                m_ModeUI.items.Add("None");

        }
        else
            UpdateModeUI();

        for (int i = 0; i < m_WorkRooms.Count; i++)
        {
            m_WorkRooms[i].Activate(m_Active);
        }
    }

    #endregion

    public void Init()
    {
        CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);
    }

    void OnEnable()
    {

        CSCreator creator = CSUI_MainWndCtrl.Instance.Creator;
        if (creator == null)
            return;
        Dictionary<int, CSCommon> commons = creator.GetCommonEntities();

        foreach (KeyValuePair<int, CSCommon> kvp in commons)
        {
            if (kvp.Value.Assembly != null && kvp.Value.WorkerMaxCount > 0 && ( kvp.Value as CSWorkerMachine)!=null)
            {
                CSUI_WorkRoom wr = Instantiate(WorkRoomUIPrefab) as CSUI_WorkRoom;
                wr.transform.parent = m_WorkRoomRootUI.transform;
                wr.transform.localPosition = Vector3.zero;
                wr.transform.localRotation = Quaternion.identity;
                wr.transform.localScale = Vector3.one;

                wr.m_RefCommon = kvp.Value;
                wr.m_RefNpc = RefNpc;

                m_WorkRooms.Add(wr);
            }
        }

        m_WorkRoomRootUI.repositionNow = true;

        //		CSPersonnel.RegisterStateChangedListener(OnNpcStateChangedListener);

    }

    void OnDisable()
    {
        foreach (CSUI_WorkRoom wr in m_WorkRooms)
            GameObject.Destroy(wr.gameObject);

        m_WorkRooms.Clear();

        //		CSPersonnel.UnRegisterStateChangedListener(OnNpcStateChangedListener);

    }

    // Use this for initialization
    void Start()
    {
        _activate();
    }

    void Awake()
    {

    }

    void OnDestroy()
    {
        CSPersonnel.UnregisterOccupaChangedListener(OnOccupationChange);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateWorkRoom()
    {
        foreach (CSUI_WorkRoom wr in m_WorkRooms)
        {
            wr.m_RefNpc = m_RefNpc;
        }

    }

    void UpdateModeUI()
    {
        if (!m_Active)
        {
            _activate();
            return;
        }

        m_ModeUI.items.Clear();


        if (m_RefNpc != null)
        {
            if (m_RefNpc.m_Occupation == CSConst.potWorker)
            {
                m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtNormalWork));
                m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtWorkWhenNeed));
                m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtWorkaholic));
            }
            ShowStatusTips = false;
            m_ModeUI.selection = CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode);
            ShowStatusTips = true;
        }
        else
        {
            m_ModeUI.items.Add("None");
        }
    }

    #region CALL_BACK

    bool ShowStatusTips = true;
    void OnSelectionChange(string item)
    {
        if (item == CSUtils.GetWorkModeName(CSConst.pwtNormalWork))
        {
            m_NormalModeUI.enabled = true;
            m_WorkWhenNeedUI.enabled = false;
            m_WorkaholicUI.enabled = false;

            if (m_RefNpc != null)
                m_RefNpc.m_WorkMode = CSConst.pwtNormalWork;

            if (ShowStatusTips)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mWorkerForNormal.GetString(), 6f);
        }
        else if (item == CSUtils.GetWorkModeName(CSConst.pwtWorkWhenNeed))
        {
            m_NormalModeUI.enabled = false;
            m_WorkWhenNeedUI.enabled = true;
            m_WorkaholicUI.enabled = false;

            if (m_RefNpc != null)
                m_RefNpc.m_WorkMode = CSConst.pwtWorkWhenNeed;

            if (ShowStatusTips)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mWorkerForWorkWhenNeed.GetString(), 6f);
        }
        else if (item == CSUtils.GetWorkModeName(CSConst.pwtWorkaholic))
        {
            m_NormalModeUI.enabled = false;
            m_WorkWhenNeedUI.enabled = false;
            m_WorkaholicUI.enabled = true;

            if (m_RefNpc != null)
                m_RefNpc.m_WorkMode = CSConst.pwtWorkaholic;

            if (ShowStatusTips)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mWorkerForWorkaholic.GetString(), 6f);
        }

        if (onSelectChange != null)
            onSelectChange(item);
    }

    void OnOccupationChange(CSPersonnel person, int prvState)
    {
        if (person != m_RefNpc)
            return;

        UpdateModeUI();
    }

    void OnPopupListClick()
    {
        if (!m_Active)
            CSUI_StatusBar.ShowText(UIMsgBoxInfo.mCantHandlePersonnel.GetString(), Color.red, 5.5f);
    }

    #endregion
}
