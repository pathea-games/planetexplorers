//using UnityEngine;
//using System.Collections;
//
//public abstract class CSSimulatorObject : MonoBehaviour 
//{
//	public CSSimulatorAttr m_Attr;
//
//	protected CSSimulator m_Simulator;
//	public CSSimulator	Simulator 	 { get {return m_Simulator;} }
//
//	public bool Init(int id, CSMgCreator creator)
//	{
//		bool isNew = true;//creator.SimulatorMgr.CreateSimulator(id, out m_Simulator);
//
//		_init(isNew);
//		return isNew;
//	}
//
//	public abstract void Sync(int id, CSMgCreator creator);
//
//	protected abstract void _init(bool isNew); 
//
//	protected Transform m_Trans;
//	// Use this for initialization
//	protected void Start () 
//	{
//		m_Trans = transform;
//	}
//	
//	// Update is called once per frame
//	protected void Update () 
//	{
////		m_Attr.m_Position = m_Trans.position;
////		if (Simulator != null)
////		{
////			Simulator.Position = m_Trans.position;
////		}
//	}
//}
