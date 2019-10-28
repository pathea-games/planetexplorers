using UnityEngine;
using System.Collections.Generic;

public class UIMissionTrackWnd : UIBaseWnd
{
    [SerializeField] UITable viewTable;
    [SerializeField] UIMissionGoalNode viewNodePrefab;
    public GameObject childNodePrefab;

    List<UIMissionGoalNode> m_ViewNodes = new List<UIMissionGoalNode>(10);

    public System.Action<UIMissionGoalNode> onSetViewNodeContent;

    public System.Action<UIMissionGoalNode> onDestroyViewNode;

    public bool repositionNow { get { return viewTable.repositionNow; } set { viewTable.repositionNow = true; } }
    public List<UIMissionGoalNode> viewNodes { get { return m_ViewNodes; } }

    public UIMissionGoalNode GetNode(int index) {  return m_ViewNodes[index]; } 

    public void UpdateViewNode (int count)
    {
        UIUtility.UpdateListItems<UIMissionGoalNode>(m_ViewNodes, viewNodePrefab,
           viewTable.transform, count, OnSetViewNodeContent, OnDestroyViewNode);

        viewTable.repositionNow = true;
    }

    public void AddViewNode ()
    {
        UIMissionGoalNode node = UIUtility.CreateItem<UIMissionGoalNode>(viewNodePrefab, viewTable.transform);
        m_ViewNodes.Add(node);
        OnSetViewNodeContent(m_ViewNodes.Count - 1, node);

        viewTable.repositionNow = true;
        return;
    }

    public void RemoveViewNode (int index)
    {
        if (index < 0 && index >= m_ViewNodes.Count)
            return;

        UIMissionGoalNode node = m_ViewNodes[index];
        OnDestroyViewNode (node);
        Destroy(node.gameObject);
        node.transform.parent = null;
        m_ViewNodes.RemoveAt(index);

        return;
    }

    void OnSetViewNodeContent (int index, UIMissionGoalNode node)
    {
        node.index = index;

        if (onSetViewNodeContent != null)
        {
            onSetViewNodeContent(node);
        }
    }

    void OnDestroyViewNode (UIMissionGoalNode node)
    {
        if (onDestroyViewNode != null)
        {
            onDestroyViewNode(node);
        }
    }

    #region UIBASEWND
    public override void Show()
    {
        base.Show();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void OnClose()
    {
        base.OnClose();
    }
    #endregion
}
