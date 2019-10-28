//using UnityEngine;
//using System.Collections;
//
//public class CSSimulator_Colony : CSSimulatorObject 
//{
//
//	protected override void _init (bool isNew)
//	{
//		CSEntityObject cseo = gameObject.GetComponent<CSEntityObject>();
//		if (cseo == null)
//		{
//			Debug.LogError("The simulator is not a ai Entity objects.");
//			return;
//		}
//		
//		if (isNew)
//		{
//			if (cseo.m_Entity == null)
//			{
//				Debug.LogError("The entity is missing");
//			}
//			else
//				m_Attr.m_Hp = cseo.m_Entity.m_Info.m_Durability / 0.6f;
//
//			m_Simulator.Init(m_Attr);
//		}
//
//		m_Simulator.Hp =  m_Simulator.MaxHp * cseo.m_Entity.DuraPercent;
//		m_Simulator.Position = transform.position;
//	}
//	
//	public override void Sync (int id, CSMgCreator creator)
//	{
//
//	}
//
//	#region Message_For_Tower
//
//	void OnPutGo (int id)
//	{
//		CSEntityObject cseo = gameObject.GetComponent<CSEntityObject>();
//		if (cseo == null)
//		{
//			Debug.LogError("The facililty is not a ai tower.");
//			return;
//		}
//
//		// Single player mode
//		if (CSMain.s_MgCreator == null)
//			return;
//
//		if (CSMain.s_MgCreator.Assembly == null)
//			return;
//		
//		if (!CSMain.s_MgCreator.Assembly.InRange(transform.position))
//			return;
//
//		CSSimulatorData data = null;
////		CSMain.s_MgCreator.SimulatorMgr.DataMgr.CreateData(id, out data);
//
//		if(cseo.m_Entity != null)
//		{
//			m_Attr.m_Hp = cseo.m_Entity.m_Info.m_Durability / 0.6f;
//			data.Init(m_Attr);
//			data.m_HP = m_Attr.m_Hp * cseo.m_Entity.DuraPercent;
//		}
//		else
//			data.Init(m_Attr);
//
//
//		data.m_Pos = transform.position;
//	}
//
//	void OnCreatedGo(int id)
//	{
//		// Single player mode
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
//
//		CSEntityObject cseo = gameObject.GetComponent<CSEntityObject>();
//		m_Simulator.noticeHpChanged = cseo.m_Entity.OnLifeChanged;
//	}
//
////	void OnRemoveGo(int id)
////	{
////		// Single player mode
////		if (CSMain.s_MgCreator == null)
////			return;
////		
////		
////		CSMain.s_MgCreator.SimulatorMgr.RemoveSimulator(id, true);
////	}
//	
//	#endregion
//
//	// Use this for initialization
//	new  void Start () 
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
