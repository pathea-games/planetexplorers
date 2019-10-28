// Custom game mode  Scene Entity Agent
// (c) by Wu Yiqiu

using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.PeEntityExtTrans;

namespace PeCustom
{
	public class SceneEntityAgent : PeCustomScene.SceneElement, ISceneObjAgent
	{
		const int MaxTimes = 100;
		const float OneWait = 0.05f;

        //private bool _firstActivate = true;

		#region ISceneObjAgent

		public int Id { get; set;}
        public int ScenarioId { get; set; }

        public GameObject Go{		get { return null;}}
		public Vector3 Pos{ 		get { return spawnPoint.spawnPos; }        }
		public IBoundInScene Bound{	get	{ return null;}}
		public bool NeedToActivate{	get { return true;}}
		public bool TstYOnActivate{ get { return true;}}
		public void OnActivate ()
		{
			if (entity == null && !mIsProcessing)
			{
				MonsterSpawnPoint mst = mstPoint;
				if ((mst == null || !mst.isDead) 
                    && (spawnPoint.Enable || spawnPoint.EntityID != -1))
					SceneMan.self.StartCoroutine (CreateEntity ());
			
			}
            else if (entity != null && !mIsProcessing )
            {
                if (entity.peTrans != null)
                {
                    Vector3 pos = entity.peTrans.position;
                    if (SceneAgentsContoller.CheckPos(out pos, pos, spawnPoint, spawnArea))
                    {
                        entity.ExtSetPos(pos);
                    }
                    else
                    {
                        Debug.LogWarning("The Entity id [" + entity.Id + "] position is wrong");
                    }
                }
            }
        }

        bool _firstConstruct = true;
		public void OnConstruct()
        {
            if (_firstConstruct)
            {
                _firstConstruct = false;
                Vector3 pos = spawnPoint.spawnPos;
                if (SceneAgentsContoller.CheckPos(out pos, pos, spawnPoint, spawnArea))
                {
                    spawnPoint.spawnPos = pos;
                }
            }
        }
		
		public void OnDeactivate() {}
		public void OnDestruct() {}

		#endregion

		public int protoId { get { return mPoint.Prototype; } }
		public Quaternion Rot { get { return mPoint.Rotation;} }
		public Vector3 Scale { get { return mPoint.Scale;} }

		public PeEntity entity{ get; set; }
		public EntityGrp entityGp{ get; set; }	// Group
		public MonsterSpawnPoint[]	groupPoints { get; private set; } // Group use

		public SpawnPoint spawnPoint { get { return mPoint;} }
		public MonsterSpawnPoint mstPoint { get { return mPoint as MonsterSpawnPoint; } }
		public MonsterSpawnArea spawnArea { get { return mArea;} }

		private bool mIsProcessing = false;
		private SpawnPoint mPoint;
		// Area 
		private MonsterSpawnArea mArea;

		private bool mIsSave = false;
		private EntityType mType;


		public SceneEntityAgent(MonsterSpawnPoint _point, bool is_saved = false, MonsterSpawnArea _area = null, MonsterSpawnPoint[]  _groupPoints = null)
		{
			mPoint = _point;
			mIsSave = is_saved;
			mArea = _area;
			groupPoints = _groupPoints;
			mType = EntityType.EntityType_Monster;
		}

		public SceneEntityAgent (NPCSpawnPoint _point)
		{
			mPoint = _point;
			mArea = null;
			groupPoints = null;
			mType = EntityType.EntityType_Npc;
		}

		// For SceneMan Coroutine
		IEnumerator CreateEntity()
		{
			mIsProcessing = true;

			int n = 0;
			while (n++ < MaxTimes)
			{
				switch(mType)
				{
				case EntityType.EntityType_Npc:
					scene.Notify(ESceneNoification.CreateNpc, this);
					break;
				case EntityType.EntityType_Monster:
					scene.Notify(ESceneNoification.CreateMonster, this, mIsSave);
					break;
				}

				if (entity != null)
				{
                    entity.scenarioId = ScenarioId;
					SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity> ();
							
					if (skAlive != null) 
					{
						skAlive.deathEvent += OnEntityDeath;
					}

					// Note: this entity might be group
					LodCmpt entityLodCmpt = entity.lodCmpt;
					if(entityLodCmpt != null)
					{
						entityLodCmpt.onDestruct += (e)=>{DestroyEntity();};
					}

					break;
				}

				yield return new WaitForSeconds(OneWait);
			}

			mIsProcessing = false;
		}

        public bool ForceCreateEntity ()
        {
            if (spawnPoint.isDead)
                return false;

            switch (mType)
            {
                case EntityType.EntityType_Npc:
                    scene.Notify(ESceneNoification.CreateNpc, this, false);
                    break;
                case EntityType.EntityType_Monster:
                    scene.Notify(ESceneNoification.CreateMonster, this, mIsSave, false);
                    break;
            }

            if (entity != null)
            {
                SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();

                if (skAlive != null)
                {
                    skAlive.deathEvent += OnEntityDeath;
                }

                LodCmpt entityLodCmpt = entity.lodCmpt;
                if (entityLodCmpt != null)
                {
                    entityLodCmpt.onDestruct += (e) => { DestroyEntity(); };
                }

                return true;
            }

            return false;
        }

		public void DestroyEntity()
		{
			if (entity != null) 
			{
				scene.Notify(ESceneNoification.EntityDestroy, mPoint, entity);
				entity = null;
			}
		}

		void OnEntityDeath (SkillSystem.SkEntity cur, SkillSystem.SkEntity caster)
		{
			SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity> ();
			if (cur == skAlive)
			{
				scene.Notify(ESceneNoification.MonsterDead, this);
				SceneMan.RemoveSceneObj(this);
			}
		}

		public void Respawn()
		{
			if (!mIsProcessing)
			{
				if (entity == null || (entity.GetCmpt<SkAliveEntity>() != null && entity.GetCmpt<SkAliveEntity>().isDead))
				{
					entity = null;
					SceneMan.RemoveSceneObj(this);
					SceneMan.AddSceneObj(this);
				}
			}
		}
	}
}