using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml;

public class KickstarterCtrl : UIBaseWnd
{
    private struct TeamInfo
    {
        public string TitleStr { get; private set; }
        public List<string> PeoplesStr { get; private set; }
        public TeamInfo(int titleID, string peoplesStr)
        {
            TitleStr = PELocalization.GetString(titleID);
            PeoplesStr = new List<string>();
            string[] peoplesArray = peoplesStr.Split(new char[] {'\n'},StringSplitOptions.RemoveEmptyEntries).ToArray();
            string temp = "";
            for (int i = 0; i < peoplesArray.Length; i++)
            {
                temp = peoplesArray[i];
                temp = temp.Trim();
                if (!string.IsNullOrEmpty(temp))
                {
                    PeoplesStr.Add(temp);
                }
            }
        }
    }

    [SerializeField]
    private UILabel m_TitleLabel;
    [SerializeField]
    private UIPanel m_ClipPanel;
    [SerializeField]
    private BoxCollider m_DragCollider;
    [SerializeField]
    private UIScrollBar m_VScrollBars;
    [SerializeField]
    private Transform m_ItemParent;
    [SerializeField]
    private ManyPeopleItem m_KickstarterItemPrefab;
    [SerializeField]
    private int m_ColumnCount;
    [SerializeField]
    private int m_PaddingY;
    [SerializeField]
    private Transform m_TopPos;
    [SerializeField]
    private Transform m_BottomPos;
    [SerializeField]
    private float m_StartWaitTime=2;
    [SerializeField]
    private float m_MoveSpeed=2;


    private List<TeamInfo> m_TeamsInfo;
    private int m_CurShowIndex;
    private bool m_FirstShow;
    private bool m_IsPress;
    private bool m_IsScroll;
    private float m_ContentHeight;


    #region override methods

    protected override void InitWindow()
    {
        base.InitWindow();
        Init();
    }

    public override void Show()
    {
        base.Show();
        if (null != m_TeamsInfo & m_TeamsInfo.Count > 0)
        {
            if (!m_FirstShow)
                ChangeCurShowIndex();
            else
                m_CurShowIndex = 0;
            ShowCurContent();
            StartCoroutine("AutoScrollIterator");
        }
        m_FirstShow = false;
    }

    protected override void OnHide()
    {
        StopCoroutine("AutoScrollIterator");
        DestoryItems();
        base.OnHide();
    }
    #endregion

    #region mono methods

    void Update()
    {
        if (null != UICamera.hoveredObject)
        {
            if (UICamera.hoveredObject == m_DragCollider.gameObject)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                m_IsScroll = (scroll!=0);
            }
        }
    }
    #endregion

    #region private methods

    private void DestoryItems()
    {
        int count = m_ItemParent.childCount;
        for (int i = 0; i < count; i++)
        {
            Destroy(m_ItemParent.GetChild(i).gameObject);
        }
    }

    private void Init()
    {
        m_TeamsInfo = new List<TeamInfo>();
        m_CurShowIndex = 0;
        m_FirstShow = true;
        m_IsPress = false;
        m_IsScroll = false;
        FillTeamsInfo();

        UIEventListener.Get(m_DragCollider.gameObject).onPress = (go, isPress) =>
        {
            m_IsPress = isPress;
        };
        
        UIEventListener.Get(m_VScrollBars.foreground.gameObject).onPress = (go, isPress) =>
        {
            m_IsPress = isPress;
        };
    }

    private void ChangeCurShowIndex()
    {
        if (null != m_TeamsInfo && m_TeamsInfo.Count > 0)
        {
            m_CurShowIndex++;
            if (m_CurShowIndex < 0 || m_CurShowIndex >= m_TeamsInfo.Count)
            {
                m_CurShowIndex = 0;
            }
        }
    }

    private void ShowCurContent()
    {
        if (null != m_TeamsInfo && m_TeamsInfo.Count > 0)
        {
            if (m_CurShowIndex >= 0 && m_CurShowIndex < m_TeamsInfo.Count)
            {
                TeamInfo curTeamInfo= m_TeamsInfo[m_CurShowIndex];
                m_TitleLabel.text = curTeamInfo.TitleStr;
                m_TitleLabel.MakePixelPerfect();
                List<string> peoplesName = curTeamInfo.PeoplesStr;
                if (null != peoplesName && peoplesName.Count > 0)
                {
                    for (int i = 0; i < peoplesName.Count; i+= m_ColumnCount)
                    {
                        ManyPeopleItem item= GetNewKickstarterItem();
                        item.UpdateNames(peoplesName.GetRange(i, (i+m_ColumnCount>= peoplesName.Count)? (peoplesName.Count-i):m_ColumnCount));
                    }
                    RepositionVertical();
                }
            }
        }
    }

    private void FillTeamsInfo()
    {
        TextAsset textAsset = Resources.Load("Credits/KickstarterInGame", typeof(TextAsset)) as TextAsset;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(textAsset.text);

        //Teams
        XmlNodeList teamNodeList = xmlDoc.SelectNodes("Root/Team");

        if (null != teamNodeList&& teamNodeList.Count>0)
        {
            for (int i = 0; i < teamNodeList.Count; i++)
            {
                //Team
                XmlNode teamNode=teamNodeList[i];
                //Title
                XmlNode titleNpde=teamNode.SelectSingleNode("Title");
                //Peoples
                XmlNode peoplesNode = teamNode.SelectSingleNode("Peoples");
                m_TeamsInfo.Add(new TeamInfo(null == titleNpde ? 0 : int.Parse(titleNpde.InnerText), null == peoplesNode ? "" : peoplesNode.InnerText));
            }
        }
    }

    ManyPeopleItem GetNewKickstarterItem()
    {
        GameObject go = GameObject.Instantiate(m_KickstarterItemPrefab.gameObject) as GameObject;
        go.transform.parent = this.m_ItemParent.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go.GetComponent<ManyPeopleItem>();
    }

    void RepositionVertical()
    {
        int count = m_ItemParent.childCount;
        Bounds tempBounds;
        Vector3 preEndPos = Vector3.zero;
        Transform curTrans;
        Vector3 padingY = new Vector3(0, m_PaddingY, 0);
        //ManyPeopleItem tempManyPeopleItem = null;
        for (int i = 0; i < count; i++)
        {
            curTrans = m_ItemParent.GetChild(i);
            //tempManyPeopleItem = curTrans.GetComponent<ManyPeopleItem>();
            curTrans.localPosition = preEndPos - padingY;
            tempBounds = NGUIMath.CalculateRelativeWidgetBounds(curTrans);
            preEndPos.y = curTrans.localPosition.y - tempBounds.size.y;
        }
        m_ContentHeight =Math.Abs(preEndPos.y);
        ResetClipPanel();
        m_ItemParent.transform.localPosition = m_TopPos.localPosition;
    }

    void ResetClipPanel()
    {
        m_VScrollBars.scrollValue = 0;
        Vector3 panelPos = m_ClipPanel.transform.localPosition;
        panelPos.y = 0;
        m_ClipPanel.transform.localPosition = panelPos;
        m_ClipPanel.clipRange = new Vector4(0, 0, 800, 430);
        m_ClipPanel.clipSoftness = new Vector2(10, 20);
    }

    IEnumerator AutoScrollIterator()
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < m_StartWaitTime)
        {
            yield return null;
        }
        Vector3 Offset = new Vector3(0, m_MoveSpeed, 0);
        while (true)
        {
            if (!m_IsScroll && !m_IsPress)
            {
                m_ItemParent.transform.localPosition += Offset;
                if ((m_ClipPanel.transform.localPosition.y+m_ItemParent.transform.localPosition.y - m_ContentHeight)> m_TopPos.localPosition.y)
                {
                    ResetClipPanel();
                    Vector3 curPos = m_ItemParent.transform.localPosition;
                    curPos.y = m_BottomPos.localPosition.y;
                    m_ItemParent.transform.localPosition = curPos;
                }
            }
            yield return null;
        }
    }
    #endregion
}
