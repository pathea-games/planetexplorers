////unknown
//using UnityEngine;
//using System.Collections;
//using ItemAsset;

//public class CSUI_RepairProperty : CSUI_ItemProperty 
//{
//    // Durability
//    [SerializeField]
//    private UILabel m_DuraValLb;
	
//    // Costs Time
//    [SerializeField]
//    private UILabel m_CostsTimeLb;
	
//    public UILabel  CostsTimeLb		{ get { return m_CostsTimeLb; } }
	
	
//    public override bool SetItem (ItemObject item)
//    {
//        if (m_IconGrid == null)
//            InitIconGrid();
		
//        if (item == null)
//        {	
//            m_IconGrid.SetItem(null);
			
//            if (onItemChanged != null)
//                onItemChanged(item);
			
//            return true;
//        }
		
//        // Creation Item
//        if (item.prototypeId > CreationData.s_ObjectStartID)
//        {
//            return false;
//        }
//        else
//        {
//            if (item.prototypeData.mRepairRequireList.Count == 0)
//                return false;
//        }
		
//        m_IconGrid.SetItem(item);
		
//        if (onItemChanged != null)
//            onItemChanged(item);
		
		
//        return true;
//    }
	
//    protected override void UpdateItemInfo ()
//    {

//        LifeLimit item = m_IconGrid.ItemObj.GetCmpt<LifeLimit>();
//        if (item == null)
//        {
//            m_ItemNameLb.text = "";
//            m_DuraValLb.text	= "0   ( [00BBFF] +0 [ffffff] )";
//        }
//        else
//        {
//            m_ItemNameLb.text = item.itemObj.prototypeData.m_Name;
//            //m_DuraValLb.text = item.GetInfo();//todocolony
//        }
//    }

//    // Use this for initialization
//    new void Start () 
//    {
//        base.Start();
//    }
	
//    // Update is called once per frame
//    new void Update () 
//    {
//        base.Update();
		
//    }
//}
