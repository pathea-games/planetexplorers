using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using Pathea;
using ItemAsset.PackageHelper;

public class CSUI_QueryMat : MonoBehaviour 
{
	public enum FuncType
	{
		Delete,
		Repair
	}
	public FuncType funcType;
	
	[SerializeField]
	private UIButton m_OkBtn;
	[SerializeField]
	private UIGrid 	m_MatGridRoot;
	[SerializeField]
	private UILabel m_TipLabel;
	
	public string Tip		{ get { return m_TipLabel.text; } set { m_TipLabel.text = value; } }
	
	[SerializeField]
    private CSUI_MaterialGrid m_MaterialItemPrefab;

    private List<CSUI_MaterialGrid> m_MatItemList;

    private PlayerPackageCmpt m_PlayerPackageCmpt;

    public CSEntity	  m_Entity;
	
	//Debug
	[SerializeField]
	private int[] m_DefItemCostsId;
	[SerializeField]
	private int[] m_DefItemCostsCnt;
	
	#region NGUI_CALLBACK
	
	void OnOKBtn()
	{
		if (m_Entity == null)	return;

        if (!GameConfig.IsMultiMode)
        {
            if (funcType == FuncType.Repair)
            {
                foreach (CSUI_MaterialGrid item in m_MatItemList)
                {
                    m_PlayerPackageCmpt.package.Destroy(item.ItemID, item.NeedCnt);
                }

                m_Entity.StartRepairCounter();

                //			CSUI_MainWndCtrl.ShowStatusBar("Start to repair the " + m_Entity.Name + ".");
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Entity.Name));
            }
            else
            {
				m_Entity.ClearDeleteGetsItem();
                foreach (CSUI_MaterialGrid item in m_MatItemList)
                {
                    m_Entity.AddDeleteGetsItem(item.ItemID, item.NeedCnt);
                }
                m_Entity.StartDeleteCounter();

                //			CSUI_MainWndCtrl.ShowStatusBar("Start to delete the " + m_Entity.Name + ".");
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToDelete.GetString(), m_Entity.Name));
            }
        }
        else
        {
            //multimode
            if (funcType == FuncType.Repair)
            {
                m_Entity._ColonyObj.Repair();
            }
            else
            {
                m_Entity._ColonyObj.RecycleItems();
            }
        }
		
		Destroy(gameObject);
	}
	
	void OnCancelBtn ()
	{
		Destroy(gameObject);
	}
	#endregion
	
	void CreateGrid (int itemId,int hasCount,int needCount)
	{
        CSUI_MaterialGrid matItem = Instantiate(this.m_MaterialItemPrefab) as CSUI_MaterialGrid;
		matItem.transform.parent		 = m_MatGridRoot.transform;
		matItem.transform.localPosition = Vector3.zero;
		matItem.transform.localRotation = Quaternion.identity;
		matItem.transform.localScale	 = Vector3.one;
        matItem.bUseColors = funcType == FuncType.Repair;
        matItem.ItemID = itemId;
        matItem.ItemNum = hasCount;
        matItem.NeedCnt = needCount;
        m_MatItemList.Add(matItem);
	}
	
	#region UNITY_INNER_FUNC

	// Use this for initialization
	void Start () 
	{
        m_MatItemList = new List<CSUI_MaterialGrid>();

        if (m_Entity == null) return;

        if (null != PeCreature.Instance && null != PeCreature.Instance.mainPlayer)
        {
            m_PlayerPackageCmpt = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
        }

        Tip = PELocalization.GetString(funcType == FuncType.Delete ? 82201081 : 82201082);
		
        //MergeSkill ms  = MergeSkill.s_tblMergeSkills.Find(
        //               delegate (MergeSkill hh)
        //                {
        //                    return hh.m_productItemId == m_Entity.ItemID;
        //                });
        
        Pathea.Replicator.Formula ms = Pathea.Replicator.Formula.Mgr.Instance.FindByProductId(m_Entity.ItemID);
		List<MaterialItem> miList = ItemProto.GetRepairMaterialList(m_Entity.ItemID);
		float percent = 0.0f;
		if (funcType == FuncType.Repair)
			percent = (m_Entity.m_Info.m_Durability - m_Entity.BaseData.m_Durability) / m_Entity.m_Info.m_Durability;
		else if (funcType == FuncType.Delete)
			percent =  m_Entity.BaseData.m_Durability / m_Entity.m_Info.m_Durability;
			
		if (ms != null)
		{
			if(funcType == FuncType.Repair&&miList!=null&&miList.Count>0)
			{
				foreach (MaterialItem mi in miList)
				{				
					int count = Mathf.CeilToInt( mi.count * percent);
					int playerCnt = m_PlayerPackageCmpt.package.GetCount(mi.protoId);
					CreateGrid(mi.protoId, playerCnt, count);
					if (playerCnt < count && funcType == FuncType.Repair)
						m_OkBtn.isEnabled = false;
				}
			}else{
				if(miList==null||miList.Count==0)
					Debug.LogError("no ItemProto.repairMaterialList:"+m_Entity.ItemID);
				List<Pathea.Replicator.Formula.Material> m_MSMIlist = ms.materials;
				
				foreach (Pathea.Replicator.Formula.Material msmi in m_MSMIlist)
				{				
					int count = Mathf.CeilToInt( msmi.itemCount * percent);
					int playerCnt = m_PlayerPackageCmpt.package.GetCount(msmi.itemId);
					CreateGrid(msmi.itemId, playerCnt, count);
					if (playerCnt < count && funcType == FuncType.Repair)
						m_OkBtn.isEnabled = false;
				}
				
				m_MatGridRoot.repositionNow = true;
			}

		}
		else
		{	
			for (int i = 0; i < m_DefItemCostsId.Length; i++)
			{
				int count = Mathf.CeilToInt(m_DefItemCostsCnt[i] * percent);
                int playerCnt = m_PlayerPackageCmpt.package.GetCount(m_DefItemCostsId[i]);
                CreateGrid(m_DefItemCostsId[i], playerCnt,count);
				if (playerCnt < count && funcType == FuncType.Repair)
					m_OkBtn.isEnabled = false;
			}
				
			m_MatGridRoot.repositionNow = true;
		}

	}
	
	#endregion
}
