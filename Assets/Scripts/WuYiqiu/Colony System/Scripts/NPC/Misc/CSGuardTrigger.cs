//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//
//public class EnemyInst
//{
//	public AiObject m_Enemy;
//
//	public bool isSkyMonster 	{	get { return false; } }
//	public bool isLandMonster {	get { return false; } }
//	public bool isWaterMonster { get { return false; } }
//
//	public Vector3 Position { get { return m_Enemy.position; } }
//
//	private HashSet<object> m_Reserves;
//	public int ReservesCount { get { return m_Reserves.Count; } }
//	public HashSet<object> Resrves { get { return m_Reserves; } }
//
//	public const int c_MaxReservesCnt = 3;
//
//	public void AddReserves(object obj)
//	{
//		if (!m_Reserves.Contains(obj))
//		{
//			int old_cnt = m_Reserves.Count;
//			m_Reserves.Add(obj);
//			if (ReservesChangedHandler != null)
//				ReservesChangedHandler(this, old_cnt);
//		}
//	}
//
//	public void RemoveReserves(object obj)
//	{
//		if (m_Reserves.Contains(obj))
//		{
//			int old_cnt = m_Reserves.Count;
//			m_Reserves.Remove(obj);
//
//			if (ReservesChangedHandler != null)
//				ReservesChangedHandler(this, old_cnt);
//		}
//	}
//
//	public delegate void ReservesChangedDel (EnemyInst ei, int old_reserves_cnt);
//
//	public event ReservesChangedDel ReservesChangedHandler;
//	
//	public EnemyInst(AiObject enemy)
//	{
//		m_Enemy = enemy;
//		m_Reserves = new HashSet<object>() ;
//	}
//}
//
//[RequireComponent (typeof (SphereCollider))]
//public class CSGuardTrigger : MonoBehaviour 
//{
//	public CSGuardTrigger m_Parent;
//
//	private SphereCollider m_Collider;
//	private Transform 	   m_Trans;
//
//	public delegate void EnemyHandleDel (EnemyInst ei);
//	public event EnemyHandleDel RemoveEnemyHandler;
//
//	public bool IsEmpty;
//
//	public GameObject  m_Model;
//
//	public float Radius 
//	{
//		get { return m_Collider.radius; }
//		set { m_Collider.radius = value; }
//	}
//
//	public Vector3 Pos 
//	{
//		get { return m_Trans.position; }
//		set { m_Trans.position = value; }
//	}
//
//	public List<EnemyInst> Enemys
//	{
//		get 
//		{ 
//			List<EnemyInst> monsters = new List<EnemyInst>();
//			monsters.AddRange(SkyEnemys);
//			monsters.AddRange(LandEnemys);
//			monsters.AddRange(WaterEnemys);
//			return monsters;
//		}
//		
//	}
//
//	// Event
//	// -----------------------------------------------
//	public delegate void TriggerEvent(Collider other);
//	public TriggerEvent onEnterTrigger;
//	public TriggerEvent onTickTrigger;
//	public TriggerEvent onExitTrigger;
//	// -----------------------------------------------
//
//
//
//	// All Enemys contain its children
//	Dictionary<AiObject, EnemyInst> m_EnemysDic = new Dictionary<AiObject, EnemyInst>(); 
//	
//	SortedList<int, List<EnemyInst>> m_SkyEnemys = new SortedList<int, List<EnemyInst>>();
//	public List<EnemyInst> SkyEnemys  
//	{
//		get {
//			List<EnemyInst> sky_enemy = new List<EnemyInst>();
//			foreach (List<EnemyInst> eis in m_SkyEnemys.Values)
//				sky_enemy.AddRange(eis);
//			return sky_enemy;
//		} 
//	}	
//
//	SortedList<int, List<EnemyInst>> m_LandEnemys = new SortedList<int, List<EnemyInst>>();
//	public List<EnemyInst> LandEnemys  
//	{ 
//		get 
//		{ 
//			List<EnemyInst> land_enemy = new List<EnemyInst>();
//			foreach (List<EnemyInst> eis in m_LandEnemys.Values)
//				land_enemy.AddRange(eis);
//			return land_enemy;
//		} 
//	}
//
//	SortedList<int, List<EnemyInst>> m_WaterEnemys = new SortedList<int, List<EnemyInst>>();
//	public List<EnemyInst> WaterEnemys  
//	{ 
//		get 
//		{ 
//			List<EnemyInst> water_enemy = new List<EnemyInst>();
//			foreach (List<EnemyInst> eis in m_WaterEnemys.Values)
//				water_enemy.AddRange(eis);
//			return water_enemy;
//		} 
//	}
//
//
//	private EnemyInst FindEnemy (AiObject ai_enemy)
//	{
//		if (m_EnemysDic.ContainsKey(ai_enemy))
//		{
//			return m_EnemysDic[ai_enemy];
//		}
//
//		if (m_Parent != null)
//			return m_Parent.FindEnemy(ai_enemy);
//		else
//			return null;
//	}
//
//	private EnemyInst AddEnemy(AiObject ai_enemy, EnemyInst ei = null)
//	{
//		if (m_EnemysDic.ContainsKey(ai_enemy))
//		{
//			ei = m_EnemysDic[ai_enemy];
//		}
//		else if (ei == null)
//			ei = new EnemyInst(ai_enemy);
//
//		m_EnemysDic.Add(ai_enemy, ei);
//		ei.ReservesChangedHandler += OnReservesChangedHandler;
//		ai_enemy.DeathHandlerEvent += OnMonsterDead;
//		ai_enemy.DestroyHandlerEvent += OnMonsterDead;
//
//		if (m_Parent != null)
//			return m_Parent.AddEnemy(ai_enemy, ei);
//		else
//			return ei;
//
//	}
//	
//
//	void OnTriggerEnter (Collider other)
//	{
//		AiObject ai_Enemy = other.gameObject.GetComponent<AiObject>();
//		
//		if (ai_Enemy != null)
//		{
//			EnemyInst ei = FindEnemy(ai_Enemy);
//
//			if (ei == null)
//				ei = AddEnemy (ai_Enemy, ei);
//
//            //if (ai_Enemy as AiSkyMonster != null)
//            //{
//            //    if (!m_SkyEnemys.ContainsKey(ei.ReservesCount))
//            //        m_SkyEnemys.Add(ei.ReservesCount, new List<EnemyInst>());
//
//            //    m_SkyEnemys[ei.ReservesCount].Add(ei);
//
//            //}
//            //else if (ai_Enemy as AiLandMonster != null || ai_Enemy as AiNative)
//            //{
//            //    if (!m_LandEnemys.ContainsKey(ei.ReservesCount))
//            //        m_LandEnemys.Add(ei.ReservesCount, new List<EnemyInst>());
//
//            //    m_LandEnemys[ei.ReservesCount].Add(ei);
//            //}
//            //else if (ai_Enemy as AiWaterMonster != null)
//            //{
//            //    if (!m_WaterEnemys.ContainsKey(ei.ReservesCount))
//            //        m_WaterEnemys.Add(ei.ReservesCount, new List<EnemyInst>());
//
//            //    m_WaterEnemys[ei.ReservesCount].Add(ei);
//            //}
//		}
//
//		if (onEnterTrigger != null)
//			onEnterTrigger(other);
//		
//	}
//
//	void OnTriggerStay(Collider other)
//	{
//		if (onTickTrigger != null)
//			onTickTrigger(other);
//	}
//
//	void OnTriggerExit (Collider other)
//	{
//		AiObject enemy = other.gameObject.GetComponent<AiObject>();
//		
//		_removeMonster(enemy);
//
//		if (onExitTrigger != null)
//			onExitTrigger(other);
//		
//	}
//
//	void OnMonsterDead (AiObject aiObject)
//	{
//		AiObject enemy = aiObject as AiObject;
//		
//		_removeMonster(enemy);
//	}
//
//	void _removeMonster (AiObject enemy)
//	{
//		if (enemy != null)
//		{
//			if (m_EnemysDic.ContainsKey(enemy))
//			{
//				if (RemoveEnemyHandler != null)
//					RemoveEnemyHandler(m_EnemysDic[enemy]);
//
//				m_EnemysDic[enemy].ReservesChangedHandler -= OnReservesChangedHandler;
//				m_EnemysDic.Remove(enemy);
//
//				enemy.DeathHandlerEvent -= OnMonsterDead;
//				enemy.DestroyHandlerEvent -= OnMonsterDead;
//
//			}
//
//            //if (enemy as AiSkyMonster != null)
//            //{
//            //    _removeMonster(m_SkyEnemys, enemy);
//            //}
//            //else if (enemy as AiLandMonster != null || enemy as AiNative != null)
//            //{
//            //    _removeMonster(m_LandEnemys, enemy);
//            //}
//            //else if (enemy as AiWaterMonster != null)
//            //{
//            //    _removeMonster(m_WaterEnemys, enemy);
//            //}
//
//
//		}
//	}
//
//	void _removeMonster(SortedList<int, List<EnemyInst>> collections, AiObject enemy)
//	{
//		if (collections == null)
//			return;
//
//		List<int> remove_list = new List<int>();
//		foreach (KeyValuePair<int, List<EnemyInst>> kvp in collections )
//		{
//			bool remove = false;
//			int index = kvp.Value.FindIndex(item0 => item0.m_Enemy == enemy);
//			if (index != -1)
//			{
//				List<CSPersonnel> person_list = new List<CSPersonnel>();
//				foreach (object obj in kvp.Value[index].Resrves)
//				{
//					CSPersonnel csp = obj as CSPersonnel;
//					if (csp != null)
//						person_list.Add(csp);
//				}
//				
//				foreach (CSPersonnel csp in person_list)
//				{
//					csp.SetTargetMonster(null);
//                    //--to do: wait aitarget
//                    //csp.m_Npc.aiTarget.ClearHatred(enemy.gameObject);
//				}
//				kvp.Value.RemoveAt(index);
//				remove = true;
//			}
//			if (kvp.Value.Count == 0)
//				remove_list.Add(kvp.Key);
//
//			if (remove)
//				break;
//		}
//
//		for (int i = 0; i < remove_list.Count; i++)
//		{
//			collections.Remove(remove_list[i]);
//		}
//	}
//	
//
//	void OnReservesChangedHandler(EnemyInst ei, int old_reserves_cnt)
//	{
//        //if (ei.m_Enemy as AiSkyMonster)
//        //{
//        //    if (m_SkyEnemys.ContainsKey(old_reserves_cnt))
//        //        m_SkyEnemys[old_reserves_cnt].Remove(ei);
//			
//			
//        //    if (!m_SkyEnemys.ContainsKey(ei.ReservesCount))
//        //        m_SkyEnemys.Add(ei.ReservesCount, new List<EnemyInst>());
//			
//        //    m_SkyEnemys[ei.ReservesCount].Add(ei);
//        //}
//        //else if (ei.m_Enemy as AiLandMonster != null || ei.m_Enemy as AiNative )
//        //{
//        //    if (m_LandEnemys.ContainsKey(old_reserves_cnt))
//        //        m_LandEnemys[old_reserves_cnt].Remove(ei);
//			
//        //    if (!m_LandEnemys.ContainsKey(ei.ReservesCount))
//        //        m_LandEnemys.Add(ei.ReservesCount, new List<EnemyInst>());
//			
//        //    m_LandEnemys[ei.ReservesCount].Add(ei);
//        //}
//        //else if (ei.m_Enemy as AiWaterMonster != null)
//        //{
//        //    if (m_WaterEnemys.ContainsKey(old_reserves_cnt))
//        //        m_WaterEnemys[old_reserves_cnt].Remove(ei);
//			
//        //    if (!m_WaterEnemys.ContainsKey(ei.ReservesCount))
//        //        m_WaterEnemys.Add(ei.ReservesCount, new List<EnemyInst>());
//			
//        //    m_WaterEnemys[ei.ReservesCount].Add(ei);
//        //}
//	}
//
//
//
//	void Awake ()
//	{
//		m_Collider = GetComponent<Collider>() as SphereCollider;
//		m_Collider.isTrigger = true;
//
//		m_Trans = transform;
//	}
//	// Use this for initialization
//	void Start () 
//	{
//
//	}
//	
//	// Update is called once per frame
//	void Update () 
//	{
//		IsEmpty = m_LandEnemys.Count == 0 && m_SkyEnemys.Count == 0 && m_WaterEnemys.Count == 0;
//
//
//		#if UNITY_EDITOR
//		if (Time.frameCount % 32 == 0)
//		{
//			foreach (List<EnemyInst> list in m_SkyEnemys.Values)
//			{
//				foreach (EnemyInst ei in list)
//				{
//					if (ei.m_Enemy == null)
//					{
//						Debug.LogError("The enemy core in guard trigger is missing, meet error somewhere?");
//						Debug.DebugBreak();
//					}
//				}
//			}
//
//			foreach (List<EnemyInst> list in m_LandEnemys.Values)
//			{
//				foreach (EnemyInst ei in list)
//				{
//					if (ei.m_Enemy == null)
//					{
//						Debug.LogError("The enemy core in guard trigger is missing, meet error somewhere?");
//						Debug.DebugBreak();
//					}
//				}
//			}
//
//			foreach (List<EnemyInst> list in m_SkyEnemys.Values)
//			{
//				foreach (EnemyInst ei in list)
//				{
//					if (ei.m_Enemy == null)
//					{
//						Debug.LogError("The enemy core in guard trigger is missing, meet error somewhere?");
//						Debug.DebugBreak();
//					}
//				}
//			}
//		}
//		#endif
//
//
//	}
//}