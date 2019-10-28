using UnityEngine;
using System.Collections.Generic;
using SkillSystem;
using PETools;

namespace Pathea
{
    public class EntityMgr: Pathea.MonoLikeSingleton<EntityMgr>
    {
        public class RMouseClickEntityEvent : PeEvent.EventArg
        {
            public Pathea.PeEntity entity;
        }

        PeEvent.Event<RMouseClickEntityEvent> mEventor = new PeEvent.Event<RMouseClickEntityEvent>();

        public PeEvent.Event<RMouseClickEntityEvent> eventor
        {
            get
            {
                return mEventor;
            }
        }

		public class NPCTalkEvent : PeEvent.EventArg
		{
			public Pathea.PeEntity entity;
		}
		
		PeEvent.Event<NPCTalkEvent> mNPCTalkEventor = new PeEvent.Event<NPCTalkEvent>();
		
		public PeEvent.Event<NPCTalkEvent> npcTalkEventor
		{
			get
			{
				return mNPCTalkEventor;
			}
		}
        
        public Dictionary<int, PeEntity> mDicEntity = new Dictionary<int, PeEntity>(100);
		public List<PeEntity> m_Entities = new List<PeEntity>();
		public List<PeEntity> m_Tmp = new List<PeEntity>();

        public bool AddAfterAssignId(PeEntity entity, int entityId)
        {
            entity.SetId(entityId);

            return Add(entity);
        }

        #region client interface

        Transform mEntityRoot;
        Transform EntityRoot
        {
            get
            {
                if (null == mEntityRoot)
                {
                    GameObject obj = new GameObject("EntityRoot");
                    mEntityRoot = obj.transform;

					GameObject obj2 = new GameObject("NpcRoot");
					obj2.AddComponent<PeNpcGroup>();
                    obj2.AddComponent<NpcHatreTargets>();
					obj2.transform.parent = mEntityRoot;
					mNpcEntityRoot = obj2.transform;

                }
                return mEntityRoot;
            }
        }

		Transform mNpcEntityRoot;
		public Transform npcEntityRoot
		{
			get
			{
				if (null == mEntityRoot)
				{
					GameObject obj = new GameObject("EntityRoot");
					mEntityRoot = obj.transform;
					
					GameObject obj2 = new GameObject("NpcRoot");
					obj2.AddComponent<PeNpcGroup>();
					obj2.AddComponent<NpcHatreTargets>();
					obj2.transform.parent = mEntityRoot;
					mNpcEntityRoot = obj2.transform;
					
				}

				return mNpcEntityRoot != null ? mNpcEntityRoot : null;
			}
		}

        bool MatchInjured(PeEntity entity, Bounds bounds)
        {
            return entity != null && entity.IntersectsExtend(bounds);
        }

        bool MatchInjured(PeEntity entity, Ray ray)
        {
            return entity != null && entity.IntersectRayExtend(ray);
        }

        bool MatchInjured(PeEntity entity, Vector3 pos)
        {
            return entity != null && entity.ContainsPointExtend(pos);
        }
		
		bool MatchProtoIDs(PeEntity entity, Vector3 pos, float radius, int[] prototIDs)
		{
			if (entity == null)
				return false;
			
			if (System.Array.IndexOf(prototIDs, entity.ProtoID) >= 0 && PETools.PEUtil.SqrMagnitudeH (entity.position, pos) <= radius * radius)
				return true;
			
			return false;
		}

        bool Match(PeEntity entity, Vector3 pos, float radius, int playerID, bool isDeath, PeEntity exclude = null)
        {
            if (entity == null || (entity.Equals(exclude)) || entity.IsDeath() != isDeath)
                return false;

            int pid = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
            if (ForceSetting.Instance.AllyPlayer(playerID, pid) 
                && PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= radius * radius)
                return true;

            return false;
        }

        bool MatchFriendly(PeEntity entity, Vector3 pos, float radius, int playerID, int prototID, bool isDeath, PeEntity exclude = null)
        {
            if (entity == null || (entity.Equals(exclude)) || entity.IsDeath() != isDeath)
                return false;

            int pid1 = playerID;
            int pid2 = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
            if (pid1 == pid2 && (pid1 != 4 || prototID == entity.ProtoID))
            {
                if(PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= radius * radius)
                    return true;
            }

            return false;
        }

        bool Match(PeEntity entity, Vector3 pos, float radius)
        {
            if(entity != null)
                return PEUtil.SqrMagnitudeH(entity.position, pos) <= (radius + entity.maxRadius) * (radius + entity.maxRadius);
            else
                return false;
        }

        bool Match(PeEntity entity, Vector3 pos, float radius, bool isDeath)
        {
            if (entity != null)
                return PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= (radius + entity.maxRadius) * (radius + entity.maxRadius) && entity.IsDeath() == isDeath;
            else
                return false;
        }

        public List<PeEntity> GetEntities(Vector3 pos, float radius)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                PeEntity entity = m_Entities[i];
                float maxRadius = radius + entity.maxRadius;
                if(entity != null && PEUtil.SqrMagnitudeH(entity.position, pos) <= maxRadius*maxRadius)
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp;
        }

        public List<PeEntity> GetEntities(Vector3 pos, float radius, bool isDeath)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                PeEntity entity = m_Entities[i];
                float maxRadius = radius + entity.maxRadius;
                if(entity != null 
                    && entity.IsDeath() == isDeath 
                    && PEUtil.SqrMagnitudeH(entity.position, pos) <= maxRadius*maxRadius)
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp;
        }

        public List<PeEntity> GetTowerEntities(Vector3 pos, float radius, bool isDeath)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                PeEntity entity = m_Entities[i];
                float maxRadius = radius + entity.maxRadius;
                if(entity != null 
                    && entity.Tower != null
                    && entity.IsDeath() == isDeath 
                    && PEUtil.SqrMagnitudeH(entity.position, pos) <= maxRadius*maxRadius)
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp;
        }

        public List<PeEntity> GetEntities(Vector3 pos, float radius, int playerID, bool isDeath, PeEntity exclude = null)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                PeEntity entity = m_Entities[i];

                if (entity == null || (entity.Equals(exclude)) || entity.IsDeath() != isDeath)
                    continue;

                int pid = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
				if ((PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= radius * radius && ForceSetting.Instance.AllyPlayer(playerID, pid))
				    || PETools.PEUtil.CanCordialReputation(playerID,pid)
				    )
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp;
        }

		public bool NearEntityModel(Vector3 pos, float radius, int playerID, bool isDeath, PeEntity exclude = null)
		{
			int n = m_Entities.Count;
			float sqrRadius = radius * radius;
			for (int i = 0; i < n; i++)
			{
				PeEntity entity = m_Entities[i];				
				if (entity == null || (entity.Equals(exclude)) || !entity.hasView || entity.IsDeath() != isDeath )
					continue;

				if(NpcRobotDb.Instance != null && entity.Id == NpcRobotDb.Instance.mID)
					continue;

				int pid = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
				if (PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= sqrRadius && ForceSetting.Instance.AllyPlayer(playerID, pid))
					return true;
			}
			return false;
		}

        public List<PeEntity> GetEntitiesFriendly(Vector3 pos, float radius, int playerID, int protoID, bool isDeath, PeEntity exclude = null)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                PeEntity entity = m_Entities[i];

                if (entity == null || (entity.Equals(exclude)) || entity.IsDeath() != isDeath)
                    continue;

                int pid1 = playerID;
                int pid2 = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
                if (pid1 == pid2 && (pid1 != 4 || protoID == entity.ProtoID))
                {
                    if(PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= radius * radius)
                        m_Tmp.Add(m_Entities[i]);
                }
            }

            return m_Tmp;
        }

		public PeEntity[] GetEntitiesByProtoIDs(Vector3 pos, float radius, int[] protoIDs)
		{
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                if (MatchProtoIDs(m_Entities[i], pos, radius, protoIDs))
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp.ToArray();
		}

        public List<PeEntity> GetEntitiesInjured(Vector3 pos)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                if(m_Entities[i] != null && m_Entities[i].ContainsPointExtend(pos))
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp;
        }

        public PeEntity[] GetEntitiesInjured(Ray ray)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                if (MatchInjured(m_Entities[i], ray))
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp.ToArray();
        }

        public List<PeEntity> GetEntitiesInjured(Bounds bounds)
        {
            m_Tmp.Clear();

            int n = m_Entities.Count;
            for (int i = 0; i < n; i++)
            {
                if(m_Entities[i] != null && m_Entities[i].IntersectsExtend(bounds))
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp;
        }

        //用以筛选固定范围类的某一种类怪
        bool MatchHatred(PeEntity entity, Vector3 pos, float radius, int monsterProtoID)
        {
			if (entity != null && entity.entityProto != null)
            {
				if (Vector3.Distance(pos, entity.position) <= radius && entity.proto == Pathea.EEntityProto.Monster && entity.entityProto.protoId == monsterProtoID)
                    return true;
            }
            return false;
        }

        public PeEntity[] GetHatredEntities(Vector3 pos, float radius, int monsterProtoID)
        {
            m_Tmp.Clear();

            for (int i = 0; i < m_Entities.Count; i++)
            {
                if (MatchHatred(m_Entities[i], pos, radius, monsterProtoID))
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp.ToArray();
        }

		bool MatchStoryAssetId(PeEntity entity, int storyAssetId)
		{
			if (entity != null)
			{
                if (entity.proto != EEntityProto.Doodad)
					return false;
				if (entity.GetComponent<SceneDoodadLodCmpt>().Index == storyAssetId)
				{
					return true;
				}
			}
			return false;
		}		
		public PeEntity[] GetDoodadEntities(int storyAssetId)
		{
            m_Tmp.Clear();

            for (int i = 0; i < m_Entities.Count; i++)
            {
                if (MatchStoryAssetId(m_Entities[i], storyAssetId))
                    m_Tmp.Add(m_Entities[i]);
            }

            return m_Tmp.ToArray();
		}

        public PeEntity[] GetDoodadEntitiesByProtoId(int prototypeID) 
        {
            List<PeEntity> entities = new List<PeEntity>(mDicEntity.Values);
            return entities.FindAll(delegate(PeEntity e)
            {
                if (e != null)
                {
                    if (e.proto != EEntityProto.Doodad)
                        return false;
                    if (!StoryDoodadMap.s_dicDoodadData.ContainsKey(e.GetComponent<SceneDoodadLodCmpt>().Index))
                        return false;
                    if (StoryDoodadMap.s_dicDoodadData[e.GetComponent<SceneDoodadLodCmpt>().Index]._protoId == prototypeID)
                        return true;
                }
                return false;
            }).ToArray();
        }

		private List<PeEntity> _entitiesWithView = new List<PeEntity>();
		private int _frmUpdateEntitiesWithView = -1;
		public List<PeEntity> GetEntitiesWithView()
		{
			if (Time.frameCount != _frmUpdateEntitiesWithView){
				_entitiesWithView.Clear ();
				for (int i = 0; i < m_Entities.Count; i++)
				{
					if (m_Entities[i].hasView)
						_entitiesWithView.Add(m_Entities[i]);
				}
				_frmUpdateEntitiesWithView = Time.frameCount;
			}
			return _entitiesWithView;
		}

        public PeEntity InitEntity(int entityId, GameObject obj)
        {
            if (null != EntityMgr.Instance.Get(entityId))
            {
                Debug.LogError("existed entity with id:" + entityId);
                return null;
            }

            PeEntity entity = obj.GetComponent<PeEntity>();
            if (entity == null)
            {
                entity = obj.AddComponent<PeEntity>();
            }
            
            AddAfterAssignId(entity, entityId);

            return entity;
        }

		public PeEntity Create(int entityId, string path, Vector3 pos, Quaternion rot, Vector3 scl,bool isnpc = false)
        {
            if (entityId == IdGenerator.Invalid)
            {
				Debug.LogError("[CreateEntity]Failed to create entity : Invalid entity id " + entityId);
                return null;
            }

            if (null != EntityMgr.Instance.Get(entityId))
            {
				Debug.LogError("[CreateEntity]Failed to create entity : Existed entity with id:" + entityId);
                return null;
            }

			PeEntity entity = PeEntity.Create(path, pos, rot, scl);
            if (null == entity)
            {
				Debug.LogError("[CreateEntity]Failed to create entity!");
                return null;
            }

            bool isPlayer = entity.GetComponent<MainPlayerCmpt>() != null;
			//NpcCmpt npccmpt = entity.GetComponent<NpcCmpt>();

			Transform root;
			if (entity.NpcCmpt == null || isPlayer)
			{
				root = EntityRoot;
			}
			else
			{
				root = npcEntityRoot;
			}

			entity.transform.parent = root;
            InitEntity(entityId, entity.gameObject);
            return entity;
        }

        public bool Destroy(int entityId)
        {
            if (entityId == IdGenerator.Invalid)
            {
                Debug.Log("<color=green>Invalid entity id:" + entityId + "</color>");
                return false;
            }

            PeEntity entity = EntityMgr.Instance.Get(entityId);

            if (null == entity)
            {
                //Debug.LogError("cant find entity with id:" + entityId);
                return false;
            }

            Remove(entityId);

            return PeEntity.Destroy(entity);
        }

        public bool Add(PeEntity entity)
        {
            if (mDicEntity.ContainsKey(entity.Id))
            {
                Debug.LogError("exist entity id: " + entity.Id);
                return false;
            }

            mDicEntity.Add(entity.Id, entity);
            m_Entities.Add(entity);
            return true;
        }

        public bool Remove(int id)
        {
            if(mDicEntity.ContainsKey(id))
            {
                m_Entities.Remove(mDicEntity[id]);
                mDicEntity.Remove(id);
                return true;
            }
            return false;
        }

        public PeEntity Get(int entityId)
        {
            if (mDicEntity.ContainsKey(entityId))
            {
                return mDicEntity[entityId];
            }

            return null;
        }

		public PeEntity Get(string entityName)
		{
			string lowerName = entityName.ToLower();
			foreach(PeEntity entity in mDicEntity.Values)
			{
				if(null != entity && entity.ToString().ToLower().Contains(lowerName))
					return entity;
			}
			return null;
		}

        public PeEntity GetByScenarioId(int scenarioId)
        {
            foreach (var iter in mDicEntity)
            {
                if (null != iter.Value && iter.Value.scenarioId == scenarioId)
                    return iter.Value;
            }

            return null;
        }

        public IEnumerable<PeEntity> All
        {
            get
            {
                return mDicEntity.Values;
            }
        }

        public void Clear()
        {
            m_Entities.Clear();
            mDicEntity.Clear();
        }

        #endregion
    }
}