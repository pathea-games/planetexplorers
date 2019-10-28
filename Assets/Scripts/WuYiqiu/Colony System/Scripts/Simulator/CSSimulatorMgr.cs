//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//
//
//public class CSSimulatorMgr : MonoBehaviour 
//{
//	private Dictionary<int, CSSimulator> m_Simulators = new Dictionary<int, CSSimulator>();
//	
//	public Dictionary<int, CSSimulator> Simulators 
//	{
//		get 
//		{
//			return m_Simulators;
//		}
//	}
//
//	private CSSimulatorDataMgr m_DataMgr;
//
//	public CSSimulatorDataMgr  DataMgr 
//	{
//		get
//		{
//			return m_DataMgr;
//		}
//	}
//
//	public void Init (int creator_id)
//	{
//		if (m_DataMgr != null)
//			return;
//
//		bool isNew =  CSSimulatorDataMgr.CreateDataMgr(creator_id, out m_DataMgr);
//		if (!isNew)
//		{
//			foreach (CSSimulatorData data in m_DataMgr.Datas.Values)
//			{
//				GameObject go = new GameObject();
//				CSSimulator css = go.AddComponent<CSSimulator>();
//				css.transform.parent = transform;
//				css.m_Mgr = this;
//				css.Data = data;
//				
//				m_Simulators.Add(data.ID, css);
//			}
//		}
//	}
//	
//	public bool CreateSimulator(int ID, out CSSimulator outF)
//	{
//		if (m_Simulators.ContainsKey(ID))
//		{
//			outF = m_Simulators[ID];
//			return false;
//		}
//
//		GameObject go = new GameObject();
//		CSSimulator css = go.AddComponent<CSSimulator>();
//		css.transform.parent = transform;
//		css.m_Mgr = this;
//
//		CSSimulatorData data = null;
//		bool isNew = m_DataMgr.CreateData(ID, out data);
//		css.Data = data;
//
//		m_Simulators.Add(ID, css);
//		outF = css;
//		return isNew;
//	}
//	
//	public bool RemoveSimulator(int ID, bool bRemoveData)
//	{
//		if (m_Simulators.ContainsKey(ID))
//		{
//			GameObject.Destroy(m_Simulators[ID].gameObject);
//			m_Simulators[ID].noticeHpChanged = null;
//			m_Simulators[ID].noticeDpsChanged = null;
//
//		}
//
//		if (bRemoveData)
//			m_DataMgr.RemoveData(ID);
//
//		return m_Simulators.Remove(ID);
//	}
//	
//	public bool ContainSimulator(int ID)
//	{
//		return m_Simulators.ContainsKey(ID);
//	}
//	
//	public CSSimulator GetSimulator(int ID)
//	{
//		return m_Simulators[ID];
//	}
//	
//	public void ApplyDamage(float damage, int count = 1)
//	{
//		if (count <= 0)
//			return;
//		
//		List<CSSimulator> simulators = new List<CSSimulator>();
//		foreach (CSSimulator css in m_Simulators.Values)
//		{
//			if (css.Hp > 0)
//				simulators.Add(css);
//		}
//		
//		if (simulators.Count == 0)
//			return;
//		
//		for (int i = 0; i < count; i++)
//		{
//			int index = Random.Range(0, simulators.Count - 1);
//			simulators[index].Hp -= damage;
//		}
//
//		//CSUI_Main.Instance.PlayTween();
//	}
//	
//	public void ApplyDamage(float damage, Vector3 position, float radius, int count = 1)
//	{
//		if (count <= 0)
//			return;
//		
//		int s_count = m_Simulators.Count - 1;
//		
//		// Find simulators which meet the conditions
//		List<CSSimulator> simulators = new List<CSSimulator>();
//		foreach (CSSimulator css in m_Simulators.Values)
//		{
//			if (css.Hp > 0 && (position - css.Position).sqrMagnitude <= radius*radius)
//				simulators.Add(css);
//		}
//		
//		if (simulators.Count == 0)
//			return;
//		
//		for (int i = 0; i < count; i++)
//		{
//			int index = Random.Range(0, simulators.Count - 1);
//			simulators[i].Hp -= damage;
//		}
//
//		//CSUI_Main.Instance.PlayTween();
//	}
//
//	void Start ()
//	{
//
//	}
//
//	void Update ()
//	{
//
//	}
//
//	#region IMPORT & EXPORTi
//
//	/// <summary>
//	/// <CSVD> Simulator Manager import
//	/// </summary>
//	public bool Import( BinaryReader r, int VERSION )
//	{
//		switch(VERSION)
//		{
//		case 0x0114:
//		case 0x0115:
//		case 0x0116:
//		{
//			int count = r.ReadInt32();
//			for (int i = 0; i < count; i++)
//			{
//				GameObject go = new GameObject();
//				CSSimulator css = go.AddComponent<CSSimulator>();
//				css.transform.parent = transform;
//				css.m_Mgr = this;
////				css.ID = r.ReadInt32();
////				css.Import(r, VERSION);
//
////				m_Simulators.Add(css.ID, css);
//			}
//		} return true;
//		default: break;
//		}
//
//		return false;
//	}
//
//	public void Export(BinaryWriter w)
//	{
//		w.Write(m_Simulators.Count);
//
//		foreach (KeyValuePair<int, CSSimulator> kvp in m_Simulators)
//		{
////			w.Write(kvp.Key);
////			kvp.Value.Export(w);
//		}
//	}
//	#endregion
//}
//
//
