using UnityEngine;
using System.Collections.Generic;
using ScenarioRTL;
using Pathea;

namespace PeCustom
{
	public static class PeScenarioUtility
	{
		/// <summary>
		/// 检查某个动作是否应该在当前客户端上发生。
		/// </summary>
		/// <param name="curr_player_id">当前玩家ID。</param>
		/// <param name="sender_id">动作发起者ID，单人游戏为当前玩家ID。</param>
		/// <param name="player">OBJECT 结构，来自statement的自定义参数。</param>
		public static bool OwnerCheck (int curr_player_id, int sender_id, OBJECT player)
		{
			if (player.type != OBJECT.OBJECTTYPE.Player)
				return false;

			// 判断 player id
			if (player.isAnyPlayer)
				return true;
			if (player.isPlayerId)
				return player.Id == curr_player_id;
			if (player.isCurrentPlayer)
				return curr_player_id == sender_id;
			if (player.isAnyOtherPlayer)
				return curr_player_id != sender_id;
			
			// 根据 curr_player_id 在 ForceSettings 里面获取 curr_player_force
			int curr_player_force = CustomGameData.Mgr.Instance.curGameData.curPlayer.Force;

			if (player.isForceId)
				return curr_player_force == player.Group;
			if (player.isAnyOtherForce)
				return curr_player_force != player.Group;
			
			// TODO: 还差对 Ally force 和 Enemy force 进行判断

			return false;
		}

		public static bool PlayerCheck (int curr_player_id, int query_player, OBJECT player)
		{
			if (player.type != OBJECT.OBJECTTYPE.Player)
				return false;

			// 判断 player id
			if (player.isAnyPlayer)
				return true;
			if (player.isPlayerId)
				return player.Id == query_player;
			if (player.isCurrentPlayer)
				return curr_player_id == query_player;
			if (player.isAnyOtherPlayer)
				return curr_player_id != query_player;

			// 根据 player_id 在 ForceSettings 里面获取 player_force
			int curr_player_force = CustomGameData.Mgr.Instance.curGameData.curPlayer.Force;
			int query_player_force = 0;

			PlayerDesc pd = CustomGameData.Mgr.Instance.curGameData.mPlayerDescs.Find(iter => iter.ID == query_player);
			if (pd != null)
				query_player_force = pd.Force;

			if (player.isForceId)
				return query_player_force == player.Group;
			if (player.isAnyOtherForce)
				return curr_player_force != query_player_force;

			// 对 Ally force 和 Enemy force 进行判断
			ForceDesc fd = CustomGameData.Mgr.Instance.curGameData.mForceDescs.Find(iter => iter.ID == curr_player_force);
			if (fd != null)
			{
				if (player.isAllyForce)
					return fd.Allies.Contains(query_player_force);
				else if (player.isEnemyForce)
					return !fd.Allies.Contains(query_player_force);
			}

			return false;
		}

		/// <summary>
		/// 根据 OBJECT 找到相应的 GameObject
		/// </summary>
		/// <returns>The GameObject.</returns>
		/// <param name="obj">OBJECT 结构，来自statement的自定义参数.</param>
		public static GameObject GetGameObject (OBJECT obj)
		{
            if (obj.isCurrentPlayer)
                return PeCreature.Instance.mainPlayer.gameObject;

            if (!obj.isSpecificEntity)
                return null;

            if (obj.isPlayerId)
            {
                if (CustomGameData.Mgr.Instance != null
                    && CustomGameData.Mgr.Instance.curGameData != null)
                {
                    if (CustomGameData.Mgr.Instance.curGameData.curPlayer.ID == obj.Id)
                    {
                        if (PeCreature.Instance.mainPlayer != null)
                            return PeCreature.Instance.mainPlayer.gameObject;
                    }
                      
                }
            }
            else if (obj.isNpoId)
            {
                if (CustomGameData.Mgr.Instance != null
                    && CustomGameData.Mgr.Instance.curGameData != null)
                {
                    if (CustomGameData.Mgr.Instance.curGameData.WorldIndex == obj.Group)
                    {
                        if (PeGameMgr.IsSingle)
                        {
                            SpawnDataSource sds = PeCustomScene.Self.spawnData;
                            // Monster ?
                            if (sds.ContainMonster(obj.Id))
                            {
                                MonsterSpawnPoint msp = sds.GetMonster(obj.Id);
                                if (msp.agent != null)
                                {
                                    if (msp.agent.entity != null)
                                        return msp.agent.entity.gameObject;
                                    else
                                    {
                                        if (msp.agent.ForceCreateEntity())
                                            return msp.agent.entity.gameObject;
                                        else
                                            Debug.Log("Create Entity Faild");
                                    }
                                }
                            }
                            // Npc ?
                            else if (sds.ContainNpc(obj.Id))
                            {
                                NPCSpawnPoint nsp = sds.GetNpc(obj.Id);
                                if (nsp.agent != null)
                                {
                                    if (nsp.agent.entity != null)
                                        return nsp.agent.entity.gameObject;
                                    else
                                    {
                                        if (nsp.agent.ForceCreateEntity())
                                            return nsp.agent.entity.gameObject;
                                        else
                                            Debug.Log("Create Entity Faild");
                                    }
                                }
                            }
                            // Doodad ?
                            else if (sds.ContainDoodad(obj.Id))
                            {
                                DoodadSpawnPoint dsp = sds.GetDoodad(obj.Id);
                                if (dsp.agent != null)
                                {
                                    if (dsp.agent.entity != null)
                                        return dsp.agent.entity.gameObject;
                                    else
                                    {
                                        if (dsp.agent.ForceCreateEntity())
                                            return dsp.agent.entity.gameObject;
                                        else
                                            Debug.Log("Create Entity faild");
                                    }
                                }
                            }
                            // Item ?
                            else if (sds.ContainItem(obj.Id))
                            {
                                //ItemAsset.Drag drag = null;
                                ItemSpwanPoint isp = sds.GetItem(obj.Id);
                                List<ISceneObjAgent> agents = SceneMan.GetSceneObjs<DragArticleAgent>();
                                DragArticleAgent tar_agent = null;
                                for (int i = 0; i < agents.Count; i++)
                                {
                                    DragArticleAgent _tar = agents[i] as DragArticleAgent;
                                    if (_tar != null && _tar.itemDrag.itemObj.instanceId == isp.ItemObjId)
                                    {
                                        tar_agent = _tar;
                                        break;
                                    }
                                }

                                if (tar_agent != null)
                                {
                                    if (tar_agent.itemLogic != null || tar_agent.itemScript != null)
                                    {
                                        if (tar_agent.itemLogic != null)
                                            return tar_agent.itemLogic.gameObject;
                                        else
                                            return tar_agent.itemScript.gameObject;
                                    }
                                    else
                                    {
                                        //TODO: 当前item并没有创建gameobj，强制创建？
                                        //tar_agent.TryForceCreateGO();
                                        //if (tar_agent.itemScript != null)
                                        //    return tar_agent.itemScript.gameObject;

                                    }
                                }

                            }
                        }
                        else
                        {
                            PeEntity entity = EntityMgr.Instance.GetByScenarioId(obj.Id);
                            if (null != entity)
                                return entity.gameObject;
                        }
                    }
                }
            }
            

			return null;
		}

		/// <summary>
		/// 根据 OBJECT 找到相应的 PeEntity
		/// </summary>
		/// <returns>The PeEntity.</returns>
		/// <param name="obj">OBJECT 结构，来自statement的自定义参数.</param>
		public static PeEntity GetEntity (OBJECT obj)
		{
			if (obj.isCurrentPlayer) // 优化
				return PeCreature.Instance.mainPlayer;
			
            GameObject go = GetGameObject(obj);
            if (go != null)
            {
                return go.GetComponent<PeEntity>();
            }
			return null;
		}

		/// <summary>
		/// 根据 OBJECT 找到能代表其位置的 Transform
		/// </summary>
		/// <returns>The Transform.</returns>
		/// <param name="obj">OBJECT 结构，来自statement的自定义参数.</param>
		public static Transform GetObjectTransform (OBJECT obj)
		{
			// TODO: 根据 OBJECT 找到相应的 Transform
			// 注意有些东西上面没挂PeEntity, fuck 
            // 逗是，坑爹
            GameObject go = GetGameObject(obj);
            if (go != null)
            {
                PeEntity entity = go.GetComponent<PeEntity>();
                if (entity != null)
                    return entity.GetCmpt<PeTrans>().trans;
                else
                    return go.transform;
            }

			return null;
		}

        /// <summary>
        /// 判断 obj 的意义是否涵盖具体对象 entity
        /// </summary>
        /// <param name="obj">OBJECT 结构，来自statement的自定义参数.</param>
        /// <param name="entity">具体对象entity</param>
        /// <returns></returns>
        public static bool IsObjectContainEntity(OBJECT obj, Pathea.PeEntity target)
        {
            return IsObjectContainEntity(obj, target.scenarioId);
        }

        public static bool IsObjectContainEntity(OBJECT obj, int scenarioId)	// negative = player
        {
            if (obj.type == OBJECT.OBJECTTYPE.AnyObject)
                return true;
            
			if (scenarioId > 0)
			{
                if (obj.isSpecificEntity)
	            {
	                if (CustomGameData.Mgr.Instance != null)
	                    return (obj.Id == scenarioId) && (CustomGameData.Mgr.Instance.curGameData.WorldIndex == obj.Group);
	            }
	            else if (obj.type == OBJECT.OBJECTTYPE.MonsterProto)
	            {
	                SpawnPoint sp = PeCustomScene.Self.spawnData.GetMonster(scenarioId);
                    if (sp != null)
                    {
                        if (obj.isSpecificPrototype)
                        {
                            if (obj.Id == sp.Prototype)
                                return true;
                        }
                        else if (obj.isAnyPrototypeInCategory)
                        {
                            Pathea.MonsterProtoDb.Item mpd = Pathea.MonsterProtoDb.Get(sp.Prototype);
                            if (mpd != null && System.Array.FindIndex<int>(mpd.monsterAreaId, ite => ite == obj.Group) != -1)
                                return true;
                        }
                    }

                    MonsterSpawnArea spa = PeCustomScene.Self.spawnData.GetMonsterArea(scenarioId);
                    if (spa != null)
                    {
                        if (obj.isSpecificPrototype)
                        {
                            if (obj.Id == spa.Prototype)
                                return true;
                        }
                        else if (obj.isAnyPrototypeInCategory)
                        {
                            Pathea.MonsterProtoDb.Item mpd = Pathea.MonsterProtoDb.Get(spa.Prototype);
                            if (mpd != null && System.Array.FindIndex<int>(mpd.monsterAreaId, ite => ite == obj.Group) != -1)
                                return true;
                        }
                    }
                }
	            else if (obj.type == OBJECT.OBJECTTYPE.ItemProto)
	            {
	                SpawnPoint sp = PeCustomScene.Self.spawnData.GetItem(scenarioId);
	                if (sp != null)
	                {
	                    if (obj.isSpecificPrototype)
	                    {
	                        if (obj.Id == sp.Prototype)
	                            return true;
	                    }
	                    else if (obj.isAnyPrototypeInCategory)
	                    {
	                        ItemAsset.ItemProto item = ItemAsset.ItemProto.GetItemData(sp.Prototype);
	                        return item.editorTypeId == obj.Group;
	                    }

	                }
	            }
	            else if (obj.isAnyNpo || obj.isAnyNpoInSpecificWorld)
	            {
                    SpawnPoint sp = PeCustomScene.Self.spawnData.GetSpawnPoint(scenarioId);
                    if (sp == null)
                        return false;

                    return true;
	            }
			}
			else
			{
				int current_player_id = PeCustomScene.Self.scenario.playerId;
				return PlayerCheck(current_player_id, -scenarioId, obj);
			}
            return false;
        }

        public static void SetNpoReqDialogue(PeEntity npc, string RqAction = "", object npoidOrVecter3 = null)
        {
            NpcCmpt npccmpt = npc.NpcCmpt;
            if (npccmpt != null)
            {
                if (npoidOrVecter3 != null)
                {
                    if (npoidOrVecter3 is int)
                    {
                        if ((int)npoidOrVecter3 == 0 || EntityMgr.Instance.Get((int)npoidOrVecter3) == null)
                            npccmpt.Req_Dialogue(RqAction, null);
                        else
                        {
                            PeTrans trans = EntityMgr.Instance.Get((int)npoidOrVecter3).peTrans;
                            if (trans != null)
                                npccmpt.Req_Dialogue(RqAction, trans);
                        }
                    }
                    else if (npoidOrVecter3 is Vector3)
                    {
                        npccmpt.Req_Dialogue(RqAction, (Vector3)npoidOrVecter3);
                    }
                }
                else
                    npccmpt.Req_Dialogue(RqAction);
            }
            else
            {
                // TODO: 设置 Monster 对话 状态
            }
        }

        public static void RemoveNpoReq(PeEntity npc, EReqType type)
        {
            NpcCmpt npccmpt = npc.NpcCmpt;
            if (npccmpt != null)
            {
                npccmpt.Req_Remove(type);
            }
            else
            {
                // TODO: 移除 Monster 当前状态
            }
        }


        /// <summary>
        /// 在指定位置创建Object
        /// </summary>
        /// <param name="proto"> Object 的proto类型 </param>
        /// <param name="pos">创建位置 </param>
        /// <returns></returns>
        public static bool CreateObject(OBJECT proto, Vector3 pos)
        {
            if (!proto.isSpecificPrototype)
                return false;

            if (proto.type == OBJECT.OBJECTTYPE.MonsterProto)
            {
                int id = PeCustomScene.Self.spawnData.GenerateId();
                MonsterSpawnPoint msp = new MonsterSpawnPoint();
                msp.ID = id;
                msp.spawnPos = pos;
                msp.Rotation = Quaternion.identity;
                msp.Scale = Vector3.one;
                msp.entityPos = pos;
                msp.Prototype = proto.Id;
                msp.IsTarget = true;
                msp.Visible = true;
                msp.isDead = false;
                msp.MaxRespawnCount = 0;
                msp.RespawnTime = 0;
                msp.bound = new Bounds();

                PeCustomScene.Self.CreateAgent(msp);

            }
            else if (proto.type == OBJECT.OBJECTTYPE.ItemProto)
            {
                int id = PeCustomScene.Self.spawnData.GenerateId();
                ItemSpwanPoint isp = new ItemSpwanPoint();
                isp.ID = id;
                isp.spawnPos = pos;
                isp.Rotation = Quaternion.identity;
                isp.Scale = Vector3.one;
                isp.entityPos = pos;
                isp.Prototype = proto.Id;
                isp.IsTarget = false;
                isp.Visible = true;
                isp.isDead = false;
                isp.CanPickup = true;

                PeCustomScene.Self.CreateAgent(isp);

            }
            

            return true;
            
        }

        /// <summary>
        /// 在一定范围创建指定数量的Object
        /// </summary>
        /// <param name="amout">创建Object数量</param>
        /// <param name="proto">Object 类型</param>
        /// <param name="range">创建范围</param>
        /// <returns></returns>
        public static bool CreateObjects(int amout, OBJECT proto, RANGE range)
        {
            if (range.type == RANGE.RANGETYPE.Anywhere || range.type == RANGE.RANGETYPE.Circle)
                return false;

            if (!proto.isSpecificPrototype)
                return false;

            int min_sqr_dis = 5;
            List<Vector3> spawn_pos = new List<Vector3>(10);
            if (range.type == RANGE.RANGETYPE.Sphere)
            {
                float raduis = range.radius;
                for (int i = 0; i < amout; i++)
                {
                    int n = 0;
                    int index = -1;
                    Vector3 pos = Vector3.zero;
                    do
                    {
                        pos = range.center + (Random.insideUnitSphere * raduis * 2 
                            - new Vector3(raduis, raduis, raduis)); 

                        index = spawn_pos.FindIndex(item0 => (pos - item0).sqrMagnitude < min_sqr_dis);
                        n++;
                    }
                    while (index != -1 && n < 30);

                    spawn_pos.Add(pos);

                    // Create
                    CreateObject(proto, pos);

                   if (n >= 30)
                        break;
                }
            }
            else if (range.type == RANGE.RANGETYPE.Box)
            {
                Vector3 extent = new Vector3(range.extend.x, range.extend.y, range.extend.z);
                for (int i = 0; i < amout; i++)
                {
                    int n = 0;
                    int index = -1;
                    Vector3 pos = Vector3.zero;
                    do
                    {
                        pos = range.center + (new Vector3(Random.value * extent.x , 
                            Random.value * extent.y, Random.value * extent.z) * 2 - extent);

                        index = spawn_pos.FindIndex(item0 => (pos - item0).sqrMagnitude < min_sqr_dis);
                        n++;
                    }
                    while (index != -1 && n < 30);

                    spawn_pos.Add(pos);

                    // Create
                    CreateObject(proto, pos);

                    if (n >= 30)
                        break;
                }
            }
           
            return true;
        }

        /// <summary>
        /// 删除一个特定的OBJECT
        /// </summary>
        /// <param name="obj">删除的特定Object</param>
        /// <returns></returns>
        public static bool RemoveObject (OBJECT obj)
        {
            if (!obj.isSpecificEntity)
                return false;

            if (obj.isNpoId)
            {
                SpawnDataSource ds = PeCustomScene.Self.spawnData;
                if (ds.ContainMonster(obj.Id))
                {
                    SpawnPoint sp = ds.GetMonster(obj.Id);
                    PeCustomScene.Self.RemoveSpawnPoint(sp);
                }
                else if (ds.ContainNpc(obj.Id))
                {
                    SpawnPoint sp = ds.GetNpc(obj.Id);
                    PeCustomScene.Self.RemoveSpawnPoint(sp);
                }
                else if(ds.ContainDoodad(obj.Id))
                {
                    SpawnPoint sp = ds.GetDoodad(obj.Id);
                    PeCustomScene.Self.RemoveSpawnPoint(sp);
                }
                else if (ds.ContainItem(obj.Id))
                {
                    ItemSpwanPoint sp = ds.GetItem(obj.Id);
                    

                    List<ISceneObjAgent> agents = SceneMan.GetSceneObjs<DragItemAgent>();
                    for (int i = 0; i < agents.Count; i++)
                    {
                        DragItemAgent drag_agent = agents[i] as DragItemAgent;
                        if (drag_agent.itemDrag.itemObj.protoId == sp.Prototype
                            && drag_agent.itemDrag.itemObj.instanceId == sp.ItemObjId)
                        {
                            DragItemAgent.Destory(drag_agent);
                        }
                    }
                }
                    
            }

            return false;
        }

        private static List<SpawnPoint> _tempSPList = new List<SpawnPoint>(10);
       /// <summary>
       /// 删除指定范围内Object
       /// </summary>
       /// <param name="proto">Object类型</param>
       /// <param name="range">删除范围</param>
       /// <returns></returns>
        public static bool RemoveObjects (OBJECT proto, RANGE range)
        {
            if (!proto.isPrototype)
                return false;

            if (proto.isAnyPrototype)
            {
                SpawnDataSource data = PeCustomScene.Self.spawnData;

                if (proto.type == OBJECT.OBJECTTYPE.MonsterProto)
                {
                    // Monster point
                    _tempSPList.Clear();
                    foreach (var kvp in data.monsters)
                    {
                        if (range.Contains(kvp.Value.entityPos))
                        {
                            _tempSPList.Add(kvp.Value);
                        }
                    }

                    for (int i = 0; i < _tempSPList.Count; i++)
                    {
                        PeCustomScene.Self.RemoveSpawnPoint(_tempSPList[i]);
                    }
                    _tempSPList.Clear();

                    // Monster area
                    foreach (var kvp in data.areas)
                    {
                        for (int i = 0; i < kvp.Value.Spawns.Count; i++)
                        {
                            for (int j = 0; j < kvp.Value.Spawns[i].spawnPoints.Count; j++)
                            {
                                MonsterSpawnPoint msp = kvp.Value.Spawns[i].spawnPoints[j];
                                if (range.Contains(msp.entityPos))
                                {
                                    if (msp.EntityID != -1)
                                    {
                                        CreatureMgr.Instance.Destory(msp.EntityID);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (proto.type == OBJECT.OBJECTTYPE.ItemProto)
                {
                    List<ISceneObjAgent> agents = SceneMan.GetSceneObjs<DragItemAgent>();
                    for (int i = 0; i < agents.Count; i++)
                    {
                        DragItemAgent drag_agent = agents[i] as DragItemAgent;
                        if (range.Contains(drag_agent.position))
                        {
                            DragItemAgent.Destory(drag_agent);
                        }
                    }
                }

            }
            else
            {
                SpawnDataSource data = PeCustomScene.Self.spawnData;
                if (proto.type == OBJECT.OBJECTTYPE.MonsterProto)
                {
                    // Monster point
                    _tempSPList.Clear();
                    foreach (var kvp in data.monsters)
                    {
                        if (kvp.Value.Prototype == proto.Id && range.Contains(kvp.Value.entityPos))
                        {
                            _tempSPList.Add(kvp.Value);
                        }
                    }

                    for (int i = 0; i < _tempSPList.Count; i++)
                    {
                        PeCustomScene.Self.RemoveSpawnPoint(_tempSPList[i]);
                    }
                    _tempSPList.Clear();

                    // Monster area
                    foreach (var kvp in data.areas)
                    {
                        for (int i = 0; i < kvp.Value.Spawns.Count; i++)
                        {
                            for (int j = 0; j < kvp.Value.Spawns[i].spawnPoints.Count; j++)
                            {
                                MonsterSpawnPoint msp = kvp.Value.Spawns[i].spawnPoints[j];
                                if (kvp.Value.Prototype == proto.Id && range.Contains(msp.entityPos))
                                {
                                    if (msp.EntityID != -1)
                                    {
                                        CreatureMgr.Instance.Destory(msp.EntityID);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (proto.type == OBJECT.OBJECTTYPE.ItemProto)
                {
                    List<ISceneObjAgent> agents = SceneMan.GetSceneObjs<DragArticleAgent>();
                    for (int i = 0; i < agents.Count; i++)
                    {
                        DragArticleAgent drag_agent = agents[i] as DragArticleAgent;
                        if (drag_agent.itemDrag.itemObj.protoId == proto.Id && range.Contains(drag_agent.position))
                        {
                            DragItemAgent.Destory(drag_agent);
                        }
                    }
                }
            }

            return true;
        }

        public static bool EnableSpawnPoint (OBJECT obj, bool enable)
        {
            if (obj.isPlayerId)
                return false;

            if (obj.isAnyNpo ||
                (CustomGameData.Mgr.Instance != null && CustomGameData.Mgr.Instance.curGameData != null
                && obj.isAnyNpoInSpecificWorld && obj.Group == CustomGameData.Mgr.Instance.curGameData.WorldIndex))
            {
                foreach (var kvp in PeCustomScene.Self.spawnData.monsters)
                {
                    PeCustomScene.Self.EnableSpawnPoint(kvp.Value, enable);
                }

                foreach (var kvp in PeCustomScene.Self.spawnData.areas)
                {
                    PeCustomScene.Self.EnableSpawnPoint(kvp.Value, enable);
                }

                foreach (var kvp in PeCustomScene.Self.spawnData.npcs)
                {
                    PeCustomScene.Self.EnableSpawnPoint(kvp.Value, enable);
                }

            }
            else if (obj.isSpecificEntity)
            {
                if (obj.Group == CustomGameData.Mgr.Instance.curGameData.WorldIndex)
                {
                    SpawnDataSource ds = PeCustomScene.Self.spawnData;
                    SpawnPoint sp = ds.GetSpawnPoint(obj.Id);
                    if (sp != null)
                    {
                        PeCustomScene.Self.EnableSpawnPoint(sp, enable);
                    }
                   
                }
            }
            return true;
        }
    }
}
