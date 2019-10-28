//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public partial class CSPersonnel : PersonnelBase  
//{
//	// Brain memory
//	public class CBrainMemory
//	{
//		public CSPersonnel m_Person = null;
//
//		#region OBSTACLE_ENEMYS
//
//		// Enemy of Obstacle 's  memories
//		public List<AiObject>  m_ObstacleEnemys;
//
//		class MemoryDuration
//		{
//			public int m_Duration;
//			public float m_CurTime = 0;
//
//			public MemoryDuration()
//			{
//				m_Duration = 2;
//			}
//
//			public void UpgradeDuration()
//			{
//				m_Duration = m_Duration << 1;
//			}
//		}
//		Dictionary<AiObject, MemoryDuration> m_DurationsMap;
//
//		void _removeObstacleEnemy(int index)
//		{
//			if (index >= 0 && index < m_ObstacleEnemys.Count)
//			{
//				if (OnRemoveObstacleHandler != null)
//					OnRemoveObstacleHandler(m_ObstacleEnemys[index]);
//				m_ObstacleEnemys.RemoveAt(index);
//				
//			}
//		}
//
//		public void ClearObstacleEnemy()
//		{
//			for (int i = 0; i < m_ObstacleEnemys.Count; i++)
//			{
//				if (OnRemoveObstacleHandler != null)
//					OnRemoveObstacleHandler(m_ObstacleEnemys[i]);
//			}
//			
//			m_ObstacleEnemys.Clear();
//			
//			foreach (AiObject ai in m_DurationsMap.Keys)
//			{
//				ai.DestroyHandlerEvent -= OnDestroyHandlerEvent;
//				ai.DeathHandlerEvent  -= OnDestroyHandlerEvent;
//			}
//			m_DurationsMap.Clear();
//		}
//
//		#endregion
//
//		#region WORK_ROOM
//		// Work room
//		private List<CSCommon> m_WorkRoomHistory;
//		private CSCommon m_WorkRoom;
//		public CSCommon WorkRoom  // Current work Room
//		{
//			get {
//				return m_WorkRoom;
//			}
//		}
//
//		public void ClearWorkRoomAndRemember()
//		{
//			if (m_WorkRoom != null)
//			{
//				m_WorkRoom.RemoveWorker(m_Person);
//
//				_addWorkRoomToHistory(m_WorkRoom);
//				if (m_WorkRoomHistory.Count > 5)
//					_removeWorkRoomInHistory(0);
//
//				m_WorkRoom = null;
//			}
//		}
//
//		public void ClearWorkRoom ()
//		{
//			if (m_WorkRoom != null)
//			{
//				m_WorkRoom.RemoveWorker(m_Person);
//				m_WorkRoom = null;
//			}
//		}
//
//		public CSCommon PeekWorkRoomHistory()
//		{
//			int len = m_WorkRoomHistory.Count;
//			if (len != 0)
//				return m_WorkRoomHistory[len - 1];
//
//			return null;
//		}
//
//		public CSCommon PopWorkRoomHistory()
//		{
//			CSCommon r = null;
//			int len = m_WorkRoomHistory.Count;
//			if (len != 0)
//			{
//				r = m_WorkRoomHistory[len - 1];
//				_removeWorkRoomInHistory(r);
//			}
//
//			return r;
//		}
//
//		public void ClearWorkRoomHistory()
//		{
//			for (int i = 0; i < m_WorkRoomHistory.Count; i++)
//				m_WorkRoomHistory[i].RemoveEventListener(OnWorkRoomListener);
//			m_WorkRoomHistory.Clear();
//		}
//
//		private void OnWorkRoomListener(int event_id, CSEntity cse, object arg)
//		{
//			CSCommon csc = cse as CSCommon;
//
//			if (csc == null)
//				return;
//
//			if (event_id == CSConst.eetDestroy)
//			{
//				_removeWorkRoomInHistory(csc); 
//			}
//		}
//
//		private void _addWorkRoomToHistory(CSCommon room)
//		{
//			room.AddEventListener(OnWorkRoomListener);
//			m_WorkRoomHistory.Add(m_WorkRoom);
//		}
//
//		private void _removeWorkRoomInHistory(CSCommon room)
//		{
//			room.RemoveEventListener(OnWorkRoomListener);
//			m_WorkRoomHistory.Remove(room);
//		}
//
//		private void _removeWorkRoomInHistory(int index)
//		{
//#if UNITY_EDITOR
//			if (index < 0 || index >= m_WorkRoomHistory.Count)
//			{
//				Debug.LogError("Remove index out of range");
//				Debug.DebugBreak();
//			}
//#endif
//
//			m_WorkRoomHistory[index].RemoveEventListener(OnWorkRoomListener);
//			m_WorkRoomHistory.RemoveAt(index);
//		}
//
//		#endregion
//
//		public CBrainMemory(CSPersonnel person)
//		{
//			m_ObstacleEnemys = new List<AiObject>();
//			m_DurationsMap = new Dictionary<AiObject, MemoryDuration>();
//			m_WorkRoomHistory = new List<CSCommon>();
//			m_Person = person;
//		}
//
//		public void Add(object memory)
//		{
//			// Enemy of Obstacle
//			if (memory as AiObject != null)
//			{
//				AiObject ai = memory as AiObject;
//				if (!m_DurationsMap.ContainsKey(ai))
//				{
//					ai.DestroyHandlerEvent += OnDestroyHandlerEvent;
//					ai.DeathHandlerEvent  += OnDestroyHandlerEvent;
//					m_DurationsMap.Add(ai, new MemoryDuration());
//				}
//			}
//			// Work Room
//			else if (memory as CSCommon != null)
//			{
//				CSCommon csc = memory as CSCommon;
//				if (m_WorkRoom != null)
//				{
//					m_WorkRoom.RemoveWorker(m_Person);
//
//					_addWorkRoomToHistory(m_WorkRoom);
//					if (m_WorkRoomHistory.Count > 5)
//						_removeWorkRoomInHistory(0);
//				}
//
//				m_WorkRoom = csc;
//				csc.AddWorker(m_Person);
//			}
//			else
//				Debug.LogWarning("add this memory type is invalid.");
//		}
//
//		public void Remove(object memory)
//		{
//			// Enemy of Obstacle
//			if (memory as AiObject != null)
//			{
//				AiObject ai = memory as AiObject;
//				if (m_ObstacleEnemys.Remove(ai))
//				{
//					if (OnRemoveObstacleHandler != null)
//						OnRemoveObstacleHandler(ai);
//				}
//			}
//			else 
//				Debug.LogWarning("remove this memory type is invalid.");
//		}
//
//		public void Clear()
//		{
//			ClearObstacleEnemy();
//			ClearWorkRoomHistory();
//		}
//
//		public void UpdateMemory(float delta_time, int frame_count)
//		{
//			// Enemy obstacle
//			for (int i = 0; i < m_ObstacleEnemys.Count; )
//			{
//				MemoryDuration md = m_DurationsMap[m_ObstacleEnemys[i]];
//				md.m_CurTime += delta_time;
//				if (md.m_CurTime >= md.m_Duration)
//				{
//					md.m_CurTime = 0;
//					md.UpgradeDuration();
//					_removeObstacleEnemy(i);
//				}
//				else
//					++i;
//			}
//		}
//
//		public delegate void AIObjectDel (AiObject ai);
//		public event AIObjectDel OnRemoveObstacleHandler;
//
//
//		#region AIObject_CALLBACK
//
//		public void OnDestroyHandlerEvent (AiObject ai)
//		{
//
//			if (m_DurationsMap.Remove(ai))
//				m_ObstacleEnemys.Remove(ai);
//		}
//
//		#endregion
//
//	}
//
//	void OnRemoveObstacle_BrainMemory (AiObject ai)
//	{
//
//	}
//
//	private void SetEnemyVaild (AiObject ai, bool valid)
//	{
//        //--to do: wait
//		//m_Npc.aiTarget.SetValid(ai.gameObject, valid);
//	}
//
//	private CBrainMemory m_BrainMemory;
//
//	// Obstacle Detection
//	const int c_MaxStagnantTime = 2;
//
//	// obstacle Detection for movement
//	private bool m_bObstacleDetect = false;    
//
//	public bool ObstacleDetect  
//	{
//		get	{ return m_bObstacleDetect;}
//		set
//		{
////			if (value && m_bObstacleDetect != value)
////				m_CurTime = 0;
//			m_bObstacleDetect = value;
//		}
//	}
//
//
////	private float m_CurTime = 0f;
//	private Vector2 prePos;
//
//
//	void InitMisc ()
//	{
////		m_BrainMemory.OnRemoveObstacleHandler += OnRemoveObstacle_BrainMemory;
//		m_BrainMemory = new CBrainMemory(this);
////		m_BrainMemory.OnBrainRemoveSpaceSelf += OnRemoveSpace_BrainMemory;
//	}
//
//
//
//	// Update is called once per frame
//	void MiscUpdate () 
//	{
//		// Update Memory 
//		m_BrainMemory.UpdateMemory(Time.deltaTime, Time.frameCount);
//
//        //--to do: wait dead isActive IsMoving
//        ////  Obstacle Detection
//        //if (m_bObstacleDetect)
//        //{
//        //    if (m_Npc.dead || !m_Npc.isActive)
//        //        return;
//
//        //    if (m_Npc.IsMoving)
//        //    {
//        //        return;
//        //    }
//
//        //    if (((int)m_CurTime) == 0)
//        //        prePos = new Vector2 (m_Pos.x, m_Pos.z);
//
//        //    // Current position every tick
//        //    Vector2 curPos = new Vector2(m_Pos.x, m_Pos.z);
//
//        //    // Check if this man is Stagnant? 
//        //    if (Mathf.FloorToInt(m_CurTime) == c_MaxStagnantTime)
//        //    {
//        //        if ((curPos - prePos).sqrMagnitude < 2.0f)
//        //        {
//        //            Vector3 newPos = m_Pos + Vector3.Normalize( m_Npc.forward) * 0.5f + Vector3.up * 100f;
//        //            RaycastHit rch;
//        //            float old_y = m_Pos.y;
//        //            if ( Physics.Raycast(newPos, Vector3.down, out rch, 100, 1 << Pathea.Layer.VFVoxelTerrain 
//        //                                   | 1 << Pathea.Layer.Unwalkable | 1 << Pathea.Layer.Building) )
//        //            {
//        //                m_Pos = rch.point;
//
//        //            }
//
//        //            // mark this target enemy to Obstacle, if meet the conditions
//        //            if (TargetEnemy != null && !TargetEnemy.isSkyMonster)
//        //            {
//        //                if ( Mathf.Abs(m_Pos.y - old_y) < 1.0f && (TargetEnemy.Position - m_Pos).sqrMagnitude > 10f)
//        //                {
//        //                    m_BrainMemory.Add(TargetEnemy.m_Enemy);
//        //                }
//        //            }
//        //        }
//
//        //        m_CurTime = 0;
//        //    }
//
//        //    m_CurTime += Time.deltaTime; 
//        //}
//	}
//}
