using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SkillAsset;


public class UIGraphNode  
{

    public UIGraphNode(int lever_v, UIGraphNode partent)
	{
		mLever_v = lever_v;
		mPartent = partent;
		
		mIndexForPartent = -1;
		mObject = null;
		mCtrl = null;
		ms = null;
		needCount = 0;
		getCount = 0;
		bagCount = 0;
	}

	public int mLever_v;
    public UIGraphNode mPartent;
	public int mChildCount;

	public int needCount;
	public int getCount;
	public int bagCount;

	//public int CreateCount

	//public MergeSkill ms;
    public Pathea.Replicator.Formula ms;

	public GameObject mObject;
	public int mIndexForPartent;
	public int mIndex;
	public UIGraphItemCtrl mCtrl;
	public UITreeGrid mTreeGrid;
	public UIComWndToolTipCtrl mTipCtrl;

	public int GetItemID()
	{
		if(mTipCtrl != null)
			return mTipCtrl.GetItemID();
		return 0;
	}
	
}


public class UIGraphControl : MonoBehaviour
{
	
	public GameObject mContent = null;
	public float mWndWidth = 450;
	public float mWndHeight = 200;
	public float mGraphPos_x = 30;
	public float mGraphPos_y = -30;

	public float mGrphLineWidth = 36;


	public GameObject mPrefabGraphLine_H = null;
	public GameObject mPrefabGraphLine_V = null;
	public GameObject mPrefabGraphItem = null;





	public UIScrollBar mScrollBar_V;

	public int mSelectedIndex = -1;

    public List<UIGraphNode> mGraphItemList = new List<UIGraphNode>();

	private List<GameObject> GraphLineList = new List<GameObject>();



    public UIGraphNode rootNode = null;

	//private float mGraphItemWidth = 0;
	private float mGraphItemHeight = 0;

	//private float mGraphWidth = 0;
	//private float mGraphHeight = 0;

	//private int mMaxLever_h = 0;
	//private int mMaxLever_v = 0;

	private float nodeIndex0Pos;


	public void DrawGraph()
	{
		//mMaxLever_h = 0;
		//mMaxLever_v = 0;

		UpdateGraph();


		mGraphPos_x = mWndWidth/2 - rootNode.mTreeGrid.m_Content.transform.localPosition.x + 16;
		mContent.transform.localPosition = new Vector3(mGraphPos_x,10,0);

		mContent.SetActive(true);
		mSelectedIndex = 0;
		rootNode.mCtrl.SetSelected(true);

		Invoke("SetScrolbarVale",0.1f);
	}


	public void UpdateGraphCount()
	{
		for(int i=0;i<mGraphItemList.Count;i++)
		{
			mGraphItemList[i].mCtrl.SetCount(mGraphItemList[i].needCount,mGraphItemList[i].bagCount,mGraphItemList[i].getCount);
		}
	}

	void SetScrolbarVale()
	{
		mScrollBar_V.scrollValue = 0;
	}
	

	private void UpdateGraph()
	{
		if(rootNode != null)
		{
			if(rootNode.mTreeGrid != null)
			{
				rootNode.mTreeGrid.Reposition();
				SetGraphCount(rootNode.ms.m_productItemCount);
			}
		}
	}


	public void SetGraphCount( int rootNodecount)
	{
        Pathea.Replicator r = GetReplicator();
        if (null == r)
        {
            return;
        }

		int k = rootNodecount / rootNode.ms.m_productItemCount;

		rootNode.getCount = rootNodecount;

		int index = 0;
		for(int i=0;i<mGraphItemList.Count;i++)
		{
			if(mGraphItemList[i].mPartent == rootNode)
			{
				mGraphItemList[i].needCount = rootNode.ms.materials[index].itemCount * k;
				index++;
			}
			mGraphItemList[i].bagCount = r.GetItemCount(mGraphItemList[i].GetItemID());
		}
		UpdateGraphCount();

        //lz-2018.01.05 如果在道具追踪列表，数量改变的时候更新追踪数量
        if (GameUI.Instance && GameUI.Instance.mItemsTrackWnd.ContainsScript(rootNode.ms.id))
        {
            GameUI.Instance.mItemsTrackWnd.UpdateOrAddScript(rootNode.ms,k);
        }
	}

    public static Pathea.Replicator GetReplicator()
    {
        Pathea.PeEntity e = Pathea.PeCreature.Instance.mainPlayer;
        if (null == e)
        {
            return null;
        }

        Pathea.ReplicatorCmpt c = e.GetCmpt<Pathea.ReplicatorCmpt>();
        if (null == c)
        {
            return null;
        }

        return c.replicator;
    }

    public int GetMinCount()
    {
        //lz-2016.10.16 错误 #4460  空对象
        if (null == rootNode || null == rootNode.ms)
            return 1;

        Pathea.Replicator replicator = GetReplicator();
        if (null == replicator)
        {
            return rootNode.ms.m_productItemCount;
        }

        int count = replicator.MinProductCount(rootNode.ms.id);

        return (0 == count ? 1 : count) * rootNode.ms.m_productItemCount;
    }

	public int GetMaxCount()
	{
        //lz-2016.10.13 错误 #3912 空对象
        if (null == rootNode || null==rootNode.ms)
            return 1;

        Pathea.Replicator replicator = GetReplicator();
        if (null == replicator)
        {
            return rootNode.ms.m_productItemCount;
        }

        //lz-2016.08.10 MaxProductCount材料最多可以支持造多少组Product，这里还要乘上这组Product的数量
        int count = replicator.MaxProductCount(rootNode.ms.id) * rootNode.ms.m_productItemCount;

        int stackNum = ItemAsset.ItemProto.GetStackMax(rootNode.GetItemID());

        //lz-2016.08.03 蒲及说如果合成的物品堆叠数为1限制最大数量为一个常数个
        if (stackNum == 1)
        {
            count = Mathf.Min(count,ColonyConst.FACTORY_COMPOUND_GRID_COUNT);
        }
        else
        {
            //lz-2016.08.03 堆叠数大于1的限制最多制造数为 一个常数个*堆叠数
			count = Mathf.Min(count, ColonyConst.FACTORY_COMPOUND_GRID_COUNT*stackNum);
        }

        return (0 == count ? 1 : count);
	}


	public bool isCanCreate( )
	{
		if(mGraphItemList.Count == 0)
			return false;

        Pathea.Replicator r = GetReplicator();
        if(null == r)
        {
            return false;
        }

		Pathea.Replicator.Formula formula = Pathea.Replicator.Formula.Mgr.Instance.Find(rootNode.ms.id);
		if(null == formula)
		{
			return false;
		}

		ItemAsset.ItemProto proto = ItemAsset.ItemProto.GetItemData(formula.productItemId);
		if(proto != null)
		{
			if(!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockProductItemLevel(proto.level))
			{
				return false;
			}
			if(!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockProductItemType(proto.itemClassId))
			{
				return false;
			}
		}
		else
			return false;

		if (r.MaxProductCount(rootNode.ms.id) < (rootNode.getCount / rootNode.ms.m_productItemCount))
        {
            return false;
        }
        return true;
	}





    private GameObject CreateGraphItem(UIGraphNode parent, int lever_v)
	{
		if(mContent == null || mPrefabGraphItem == null)
			return null;

		GameObject o = GameObject.Instantiate(mPrefabGraphItem) as GameObject;
		if(parent == null)
		{
			o.transform.parent = mContent.transform;
			o.transform.localPosition = new  Vector3(0,0,0);
		}
		else
		{
			o.transform.parent = parent.mCtrl.child.transform;
			o.transform.localPosition = new  Vector3(0,mGraphItemHeight,0);
		}

		o.transform.localScale = new Vector3(1,1,1);


		if ( parent != null )
		{
			UITreeGrid treegrid = parent.mObject.GetComponent<UITreeGrid>();
			UITreeGrid thistreegrid = o.GetComponent<UITreeGrid>();
			if ( treegrid != null && thistreegrid != null )
			{
				treegrid.m_Children.Add(thistreegrid);
			}
		}

		return o;
	}


	public void ClearGraph()
	{
		for(int i=0;i<mGraphItemList.Count;i++)
		{
			GameObject.Destroy(mGraphItemList[i].mObject);
		}
		mGraphItemList.Clear();
		for(int i=0;i<GraphLineList.Count;i++)
		{
			GameObject.Destroy(GraphLineList[i]);
		}
		GraphLineList.Clear();
		mContent.SetActive(false);
	}


    public UIGraphNode AddGraphItem(int lever_v, UIGraphNode partent, Pathea.Replicator.Formula ms, Texture contentTexture)
	{
        UIGraphNode node = new UIGraphNode(lever_v, partent);

		GameObject o = CreateGraphItem(partent,lever_v);
		int tempIndex = mGraphItemList.Count;
		node.mObject = o;
		node.mIndex = tempIndex;
		node.mCtrl = o.GetComponent<UIGraphItemCtrl>();
		node.mTreeGrid = o.GetComponent<UITreeGrid>();
		node.ms = ms;



		if(partent == null)
		{
			rootNode = node;
		}
		
		if(node.mCtrl != null)
		{
			node.mCtrl.SetCotent(contentTexture);
			node.mCtrl.SetIndex(tempIndex);
		}

		mGraphItemList.Add(node);
		return node;
	}


    public UIGraphNode AddGraphItem(int lever_v, UIGraphNode partent, Pathea.Replicator.Formula ms, string[] strSprites, string strAtlas)
	{
        UIGraphNode node = new UIGraphNode(lever_v, partent);

		GameObject o = CreateGraphItem(partent,lever_v);
		int tempIndex = mGraphItemList.Count;
		node.mObject = o;
		node.mIndex = tempIndex;
		node.mCtrl = o.GetComponent<UIGraphItemCtrl>();
		node.mTreeGrid = o.GetComponent<UITreeGrid>();
		node.mTipCtrl = node.mCtrl.mTipCtrl;
		node.ms = ms;

		if(partent == null)
		{
			rootNode = node;
		}

		if(node.mCtrl != null)
		{
            node.mCtrl.SetCotent(strSprites, strAtlas);
			node.mCtrl.SetIndex(tempIndex);
		}
		mGraphItemList.Add(node);
		return node;
	}


	void Awake()
	{

	}

	// Use this for initialization
	void Start () 
	{
//		GraphNode nd_00 = AddGraphItem(0,0,null ,5,15,"A","Icon");
//
//		GraphNode nd_10 = AddGraphItem(1,0,nd_00,5,15,"A","Icon");
//		GraphNode nd_11 = AddGraphItem(1,1,nd_00,5,15,"A","Icon");
//		GraphNode nd_12 = AddGraphItem(1,2,nd_00,5,15,"A","Icon");
//		
//		GraphNode nd_20 = AddGraphItem(2,0,nd_10,5,15,"A","Icon");
//		GraphNode nd_21 = AddGraphItem(2,1,nd_10,5,15,"A","Icon");
//		GraphNode nd_22 = AddGraphItem(2,2,nd_11,5,15,"A","Icon");
//		GraphNode nd_23 = AddGraphItem(2,3,nd_11,5,15,"A","Icon");
//		GraphNode nd_24 = AddGraphItem(2,4,nd_12,5,15,"A","Icon");
//		GraphNode nd_25 = AddGraphItem(2,5,nd_12,5,15,"A","Icon");
//		GraphNode nd_26 = AddGraphItem(2,6,nd_12,5,15,"A","Icon");
//		GraphNode nd_27 = AddGraphItem(2,7,nd_12,5,15,"A","Icon");
//		GraphNode nd_28 = AddGraphItem(2,8,nd_12,5,15,"A","Icon");
//		GraphNode nd_29 = AddGraphItem(2,9,nd_12,5,15,"A","Icon");
//		GraphNode nd_30 = AddGraphItem(3,0,nd_21,5,15,"A","Icon");




//		DrawGraph();
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	private int tempCount = 0;
	void FixedUpdate()
	{
		tempCount++;
		if(tempCount>=50)
		{
			UpdateGraphNodeCount();
			tempCount = 0;
		}
		
	}


	void UpdateGraphNodeCount()
	{
        Pathea.Replicator r = GetReplicator();
        if (null == r)
        {
            return;
        }

		for(int i=0;i<mGraphItemList.Count;i++)
		{
			int bagCount = r.GetItemCount(mGraphItemList[i].GetItemID());
			mGraphItemList[i].bagCount = bagCount;
			mGraphItemList[i].mCtrl.SetCount(mGraphItemList[i].needCount,mGraphItemList[i].bagCount,mGraphItemList[i].getCount);
		}
	}



}
