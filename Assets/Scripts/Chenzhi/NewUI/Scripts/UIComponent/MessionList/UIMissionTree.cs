using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMissionTree : UIComponent
{
    public GameObject mNodePrefab;
    [HideInInspector]
    public List<UIMissionNode> mNodes = new List<UIMissionNode>();
    [HideInInspector]
    public UIMissionNode mSelectedNode = null;
    public UITable mContentTable;
    public int UITreeHight = 30;
    [SerializeField]
    UIScrollBar mScroBar = null;

    public delegate void BaseMsgEvent(object sender);
    public event BaseMsgEvent e_ChangeSelectedNode = null;

    // Use this for initialization
    void Start()
    {
        // test code
        //		for (int i=0;i<5;i++)
        //		{
        //			UIMissionNode nodetest = AddMissionNode(null,"node_1" + i.ToString());
        //			//if (i == 1 || i==0)
        //			{
        //				for (int j=0;j<4;j++)
        //				{
        //					AddMissionNode(nodetest,"child",false,false,true);
        //				}
        //			}
        //		}
    }

    #region public methods
    public void Clear()
    {
        for (int i = mNodes.Count - 1; i >= 0; i--)
        {
            DeleteMissionNode(mNodes[i]);
        }
        mNodes.Clear();
    }

    public void DeleteMissionNode(UIMissionNode node)
    {
        if (node == null)
            return;
        for (int i = mNodes.Count - 1; i >= 0; i--)
        {
            if (i < mNodes.Count)
            {
                if (mNodes[i].mParent == node)
                    mNodes.Remove(mNodes[i]);
            }
        }
        mNodes.Remove(node);
        GameObject.Destroy(node.gameObject);
        node.gameObject.transform.parent = null;
        node = null;
        RepositionContent();
    }

    public void RepositionContent()
    {
        mContentTable.repositionNow = true;
        mScroBar.scrollValue = 0;
    }

    public List<UIMissionNode> GetChildNode(UIMissionNode parentNode)
    {
        List<UIMissionNode> childs = new List<UIMissionNode>();
        for (int i = 0; i < mNodes.Count; i++)
        {
            if (parentNode == mNodes[i].mParent)
                childs.Add(mNodes[i]);
        }
        return childs;
    }

    public UIMissionNode AddMissionNode(UIMissionNode parentNode, string text, bool enableCkTag = true, bool enableBtnDel = true, bool canSelected = true)
    {
        GameObject o = GameObject.Instantiate(mNodePrefab) as GameObject;
        UIMissionNode node = o.GetComponent<UIMissionNode>();

        if (parentNode == null)
        {
            node.transform.parent = mContentTable.gameObject.transform;
            node.mTablePartent = mContentTable;
        }
        else
        {
            node.transform.parent = parentNode.mTable.gameObject.transform;
            node.mTablePartent = parentNode.mTable;
        }

        node.gameObject.transform.localScale = Vector3.one;
        node.mLbTitle.text = text;
        node.enableCkTag = enableCkTag;
        node.enableBtnDelete = enableBtnDel;
        node.e_OnClick += OnChangeSelectedNode;
        node.gameObject.SetActive(true);
        node.mCanSelected = canSelected;
        node.mParent = parentNode;
        node.transform.localPosition = new Vector3(0, -(FindChildNodeCount(parentNode) * UITreeHight), 0);

        if (parentNode)
            parentNode.mChilds.Add(node);
        mNodes.Add(node);

        return node;
    }



    #endregion

    #region private methods
    int FindChildNodeCount(UIMissionNode parentNode)
    {
        int count = 0;
        for (int i = 0; i < mNodes.Count; i++)
        {
            if (mNodes[i].mParent == parentNode)
                count++;
        }
        return count;
    }

    void ResetSelectBg(UIMissionNode node)
    {
        if (node.mParent == null)
            return;
        UIMissionNode parent = node.mParent;
        int deepth = 0;
        while (parent != null)
        {
            deepth++;
            parent = parent.mParent;

        }
        Vector3 scale = node.mSpSelected.transform.localScale;
        node.mSpSelected.transform.localScale = new Vector3(scale.x - 30, scale.y, scale.z);
    }

    void OnChangeSelectedNode(object sender)
    {
        UIMissionNode node = sender as UIMissionNode;
        if (node == null || node == mSelectedNode)
            return;

        if (mSelectedNode != null)
            mSelectedNode.Selected = false;
        node.Selected = true;
        mSelectedNode = node;

        if (e_ChangeSelectedNode != null)
            e_ChangeSelectedNode(mSelectedNode);

    }
    #endregion
}
