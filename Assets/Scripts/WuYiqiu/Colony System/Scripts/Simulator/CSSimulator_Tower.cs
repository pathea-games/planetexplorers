//using UnityEngine;
//using System.Collections;
//
//public class CSSimulator_Tower : CSSimulatorObject 
//{
//
//	protected override void _init (bool isNew)
//	{
//        //AiTower tower = gameObject.GetComponent<AiTower>();
//        //if (tower == null)
//        //{
//        //    Debug.LogError("The facililty is not a ai tower.");
//        //    return;
//        //}
//
//        //if (isNew)
//        //{
//        //    m_Attr.m_Hp = tower.dataBlock.maxHpSimulate;
//        //    m_Attr.m_AOE.m_Radius = tower.attackRadius;
//        //    m_Attr.m_Dps   = tower.dataBlock.damageSimulate;
//        //    m_Simulator.Init(m_Attr); 
//        //}
//
//        //m_Simulator.Hp =  m_Simulator.MaxHp * tower.lifePercent;
//        //m_Simulator.Position = transform.position;
//	}
//
// 	public override void Sync (int id, CSMgCreator creator)
//	{
//        //AiTower tower = gameObject.GetComponent<AiTower>();
//        //if (tower == null)
//        //{
//        //    Debug.LogError("The facililty is not a ai tower.");
//        //    return;
//        //}
//
//        //if (creator.SimulatorMgr.Simulators.ContainsKey(id))
//        //{
//        //    CSSimulator csf = CSMain.s_MgCreator.SimulatorMgr.Simulators[id];
//			
//        //    tower.lifePercent = csf.Hp / csf.MaxHp;
//        //}
//	}
//
//	#region Message_For_Tower
//
//	void OnPutGo (int id)
//	{
//        //AiTower tower = gameObject.GetComponent<AiTower>();
//        //if (tower == null)
//        //{
//        //    Debug.LogError("The facililty is not a ai tower.");
//        //    return;
//        //}
//
//        //// Single player mode
//        //if (CSMain.s_MgCreator == null)
//        //    return;
//
//        //if (CSMain.s_MgCreator.Assembly == null)
//        //    return;
//		
//        //if (!CSMain.s_MgCreator.Assembly.InRange(transform.position))
//        //    return;
//
//        //CSSimulatorData data = null;
//        //CSMain.s_MgCreator.SimulatorMgr.DataMgr.CreateData(id, out data);
//
//        //m_Attr.m_Hp = tower.dataBlock.maxHpSimulate;
//        //m_Attr.m_AOE.m_Radius = tower.attackRadius;
//        //m_Attr.m_Dps   = tower.dataBlock.damageSimulate;
//
//        //data.Init(m_Attr);
//
//        //data.m_HP = m_Attr.m_Hp * tower.lifePercent;
//        //data.m_Pos = transform.position;
//
//	}
//
//	void OnCreatedGo(int id)
//	{
//		// Single player mode
//		if (CSMain.s_MgCreator == null)
//			return;
//		
//		if (CSMain.s_MgCreator.Assembly == null)
//		{
//			CSMain.s_MgCreator.SimulatorMgr.RemoveSimulator(id, false);
//			return;
//		}
//		
//		Sync(id , CSMain.s_MgCreator);
//
//		CSMain.s_MgCreator.SimulatorMgr.RemoveSimulator(id, false);
//	}
//
//	void OnDestoryGo(int id)
//	{
//		// Single player mode
//		if (CSMain.s_MgCreator == null)
//			return;
//		
//		if (CSMain.s_MgCreator.Assembly == null)
//		{
//			CSMain.s_MgCreator.SimulatorMgr.RemoveSimulator(id, false);
//			return;
//		}
//		
//		if (!CSMain.s_MgCreator.Assembly.InRange(transform.position))
//		{
//			CSMain.s_MgCreator.SimulatorMgr.RemoveSimulator(id, false);
//			return;
//		}
//		
//		Init(id, CSMain.s_MgCreator);
//	}
//
//	void OnRemoveGo(int id)
//	{
//		// Single player mode
//		if (CSMain.s_MgCreator == null)
//			return;
//
//
//		CSMain.s_MgCreator.SimulatorMgr.RemoveSimulator(id, true);
//	}
//
//	void OnDestroy()
//	{
//	}
//
//	#endregion
//
//	// Use this for initialization
//	new void Start () 
//	{
//		base.Start();
//	}
//	
//	// Update is called once per frame
//	new void Update ()
//	{
//		base.Update();
//	}
//}
