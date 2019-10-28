using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_NpcDoctor : MonoBehaviour
{

    [SerializeField]
    CSUI_WorkRoom WorkRoomUIPrefab;
    [SerializeField]
    UIGrid m_WorkRoomRootUI;

    private List<CSUI_WorkRoom> m_WorkRooms = new List<CSUI_WorkRoom>();

    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get
        {
            return m_RefNpc;
        }
        set
        {
            m_RefNpc = value;
            UpdateWorkRoom();

        }
    }

    void UpdateWorkRoom()
    {
        foreach (CSUI_WorkRoom wr in m_WorkRooms)
        {
            wr.m_RefNpc = m_RefNpc;
        }
    }

    public void Init()
    {

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
        for (int i = 0; i < m_WorkRooms.Count; i++)
        {
            m_WorkRooms[i].Activate(m_Active);
        }
    }


    void OnEnable()
    {

        CSCreator creator = CSUI_MainWndCtrl.Instance.Creator;
        if (creator == null)
            return;
        Dictionary<int, CSCommon> commons = creator.GetCommonEntities();

        foreach (KeyValuePair<int, CSCommon> kvp in commons)
        {
            if (kvp.Value.Assembly != null && kvp.Value.WorkerMaxCount > 0 && (kvp.Value as CSHealth) !=null)
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

    }

    void OnDisable()
    {
        foreach (CSUI_WorkRoom wr in m_WorkRooms)
            GameObject.Destroy(wr.gameObject);

        m_WorkRooms.Clear();
    }

    void Start()
    {
        _activate();
    }

}
