////unkown
//using UnityEngine;
//using System.Collections;
//using ItemAsset;

//public class CSUI_EnhanceProperty : CSUI_ItemProperty 
//{
//    // Attack
//    [SerializeField]
//    private UILabel m_AttackValLb;
	
//    // Defense
//    [SerializeField]
//    private UILabel m_DefenseValLb;
	
//    // Durability
//    [SerializeField]
//    private UILabel m_DuraValLb;
	
//    // Times Enhanced
//    [SerializeField]
//    private UILabel m_TimesEhancedValLb;
	
//    // Costs Time
//    [SerializeField]
//    private UILabel m_CostsTimeLb;
	
//    public UILabel  CostsTimeLb		{ get { return m_CostsTimeLb; } }

//    Strengthen m_strengthenItem;
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
//        //if (item.prototypeId > CreationData.s_ObjectStartID)
//        //{
//        //    return false;
//        //}
//        //else
//        //{
//        //    if (item.prototypeData.mStrengthenRequireList.Count == 0)
//        //        return false;
//        //}
//        Strengthen strengthenItem = item.GetCmpt<Strengthen>();
//        if (strengthenItem == null)
//        {
//            return false;
//        }
//        m_IconGrid.SetItem(item);
//        m_strengthenItem = strengthenItem;

//        if (onItemChanged != null)
//            onItemChanged(item);
		
		
//        return true;
//    }
	
//    protected override void UpdateItemInfo ()
//    {
//        Strengthen item = m_strengthenItem;
//        if (item == null)
//        {
//            m_AttackValLb.text 		 = "0   ( [00BBFF] +0 [ffffff] )";
//            m_DefenseValLb.text		 = "0   ( [00BBFF] +0 [ffffff] )";
//            m_DuraValLb.text		 = "0   ( [00BBFF] +0 [ffffff] )";
//            m_TimesEhancedValLb.text = "0";
//        }
//        else
//        {
//            m_ItemNameLb.text = item.itemObj.prototypeData.m_Name;
			
//            // Item property
//            float curValue = 0.0f;
//            float nextValue = 0.0f;

//            // Attack
//            curValue = item.GetCurLevelProperty(Pathea.AttribType.Atk);
//            nextValue = item.GetNextLevelProperty(Pathea.AttribType.Atk);
//            m_AttackValLb.text = curValue.ToString() + "   ( [00BBFF] + " + (nextValue - curValue).ToString() + "[ffffff] )";

//            // Defense
//            curValue = item.GetCurLevelProperty(Pathea.AttribType.Def);
//            nextValue = item.GetNextLevelProperty(Pathea.AttribType.Def);
//            m_DefenseValLb.text = curValue.ToString() + "   ( [00BBFF] + " + nextValue.ToString() + "[ffffff] )";

//            // Durability
//            curValue = item.GetCurMaxDurability();
//            nextValue = item.GetNextMaxDurability();
//            m_DuraValLb.text = curValue.ToString() + "   ( [00BBFF] + " + Mathf.RoundToInt(nextValue - curValue) + "[ffffff] )";
            
//            // Times Ehance
//            m_TimesEhancedValLb.text = item.strengthenTime.ToString();
//        }
//    }

	
//    #region UNITY_INNER_FUNC
	
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
	
//    #endregion
//}
