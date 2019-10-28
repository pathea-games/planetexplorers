using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcTalkHistoryWnd : UIBaseWnd
{
    [SerializeField]
    GameObject m_MsgPrefab;
    [SerializeField]
    UITable m_Table;
    [SerializeField]
    GoPool m_ItemGoPool;
    [SerializeField]
    UIScrollBar m_ScrollBar;
    [SerializeField]
    string m_MsgFormat = "[45D0FF]{0} : [-]\n[ffffff]{1}[-]";

    List<GameObject> m_MsgGoList = new List<GameObject>();

    #region override methods

    protected override void InitWindow()
    {
        if (null != NPCTalkHistroy.Instance)
        {
            FirstShowAllData();
            NPCTalkHistroy.Instance.onAddHistroy += AddHistroy;
            NPCTalkHistroy.Instance.onRemoveHistroy += RemoveHistroy;
        }
        base.InitWindow();
    }

    public override void Show()
    {
        base.Show();
        Reposition();
    }

    #endregion

    #region private methods 
    private void FirstShowAllData()
    {
        if (!base.mInit)
        {
            if (null != NPCTalkHistroy.Instance && NPCTalkHistroy.Instance.histroies.Count > 0)
            {
                for (int i = 0; i < NPCTalkHistroy.Instance.histroies.Count; i++)
                {
                    AddHistroy(NPCTalkHistroy.Instance.histroies[i]);
                }
                Reposition();
            }
        }
    }

    private void AddHistroy(NPCTalkHistroy.Histroy histroyMsg)
    {
        GameObject go = CreateGo();
        UILabel uiLabel = go.GetComponent<UILabel>();
        if (null != uiLabel)
        {
            if (null != histroyMsg.npcName&& null!=histroyMsg.countent)
            {
                string content = histroyMsg.countent.Replace("\n", string.Empty);
                uiLabel.text = string.Format(m_MsgFormat, histroyMsg.npcName, content);
                uiLabel.MakePixelPerfect();
            }
        }
        go.SetActive(true);
        m_MsgGoList.Add(go);
        Reposition();
    }

    void RemoveHistroy()
    {
        if (m_MsgGoList.Count > 0)
        {
            DestroyGo(m_MsgGoList[0]);
            m_MsgGoList.RemoveAt(0);
            Reposition();
        }
    }

    private void Reposition()
    {
        m_Table.Reposition();
        Invoke("RepostionScroll", 0.2f);
    }

    private void RepostionScroll()
    {
        if (m_ScrollBar.foreground.gameObject.activeSelf)
        {
            m_ScrollBar.scrollValue = 1;
        }
        else
        {
            m_ScrollBar.scrollValue = 0;
        }
    }

    GameObject CreateGo()
    {
        if (m_ItemGoPool != null)
        {
            return m_ItemGoPool.GetGo(m_Table.transform, false);
        }

        GameObject go = Instantiate(m_MsgPrefab) as GameObject;
        go.transform.parent = m_Table.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    void DestroyGo(GameObject go)
    {
        if (go)
        {
            if (m_ItemGoPool != null)
            {
                m_ItemGoPool.GiveBackGo(go, true);
                return;
            }
            GameObject.Destroy(go);
        }
    }

    void OnClearBtn()
    {
        for (int i = 0; i < m_MsgGoList.Count; i++)
        {
            DestroyGo(m_MsgGoList[i]);
        }
        m_MsgGoList.Clear();
        m_ScrollBar.scrollValue = 0f;

        if (null != NPCTalkHistroy.Instance)
        {
            NPCTalkHistroy.Instance.Clear();
        }
    }
    #endregion

}
