////unknown
//using UnityEngine;
//using System.Collections;
//using ItemAsset;
//using SkillAsset;

//public class CSUI_RecycleProterty : CSUI_ItemProperty 
//{

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
//            Pathea.Replicator.Formula ms = Pathea.Replicator.Formula.Mgr.Instance.FindByProductId(item.prototypeId);

//            //MergeSkill ms  = MergeSkill.s_tblMergeSkills.Find(
//            //           delegate (MergeSkill hh)
//            //            {
//            //                return hh.m_productItemId == item.mItemID;
//            //            });
			
//            if (ms == null)
//                return false;
//        }
		
//        m_IconGrid.SetItem(item);
		
		
//        if (onItemChanged != null)
//            onItemChanged(item);
		
//        return true;
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
