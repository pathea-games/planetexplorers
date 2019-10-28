//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//
//
//public class CSPolarShield : MonoBehaviour 
//{
//
////	List<EnemyInst> m_Monsters = new List<EnemyInst>();
//
//	public List<EnemyInst> Enemys
//	{
//		get 
//		{ 
//			List<EnemyInst> monsters = new List<EnemyInst>();
//			monsters.AddRange(m_SkyEnemys);
//			monsters.AddRange(m_LandEnemys);
//			monsters.AddRange(m_WaterEnemys);
//			return monsters;
//		}
//
//	}
//
//	List<EnemyInst> m_SkyEnemys = new List<EnemyInst>();
//	public List<EnemyInst> SkyEnemys  { get { return m_SkyEnemys;} }	
//
//	List<EnemyInst> m_LandEnemys = new List<EnemyInst>();
//	public List<EnemyInst> LandEnemys  { get { return m_LandEnemys;} }
//
//	List<EnemyInst> m_WaterEnemys = new List<EnemyInst>();
//	public List<EnemyInst> WaterEnemys  { get { return m_WaterEnemys;} }
//
//	void OnTriggerEnter (Collider other)
//	{
//		AiObject ai_Enemy = other.gameObject.GetComponent<AiObject>();
//
//		if (ai_Enemy != null)
//		{
//            //if (ai_Enemy as AiSkyMonster != null)
//            //{
//            //    m_SkyEnemys.Add(new EnemyInst(ai_Enemy));
//            //    ai_Enemy.DeathHandlerEvent += OnMonsterDead;
//            //}
//            //else if (ai_Enemy as AiLandMonster != null || ai_Enemy as AiNative)
//            //{
//            //    m_LandEnemys.Add(new EnemyInst(ai_Enemy));
//            //    ai_Enemy.DeathHandlerEvent += OnMonsterDead;
//            //}
//            //else if (ai_Enemy as AiWaterMonster != null)
//            //{
//            //    m_WaterEnemys.Add(new EnemyInst(ai_Enemy));
//            //    ai_Enemy.DeathHandlerEvent += OnMonsterDead;
//            //}
//		}
//
//	}
//
//	void OnTriggerStay (Collider other) 
//	{
//
//	}
//
//	void OnTriggerExit (Collider other)
//	{
//		AiObject enemy = other.gameObject.GetComponent<AiObject>();
//
//		_removeMonster(enemy);
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
//	}
//
//	void OnSpawnMonster (GameObject go)
//	{
//	}
//
//	void OnDestroy ()
//	{
//	}
//
//	// Use this for initialization
//	void Start () 
//	{
//	}
//	
//	// Update is called once per frame
//	void Update ()
//	{
//		
//	}
//}
