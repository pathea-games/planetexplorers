using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using SkillSystem;


public class SpecialHatred
{
    const int allMonsters = 8888;
    const int fixMonsBase = 8000;
    const int allNpcs = 30000;
    const int mainPlayer = 20000;
    const int npcBase = 9000;

    enum hatredType 
    {
        recovery = 0,
        ignore = 1,
        highHatred = 2
    }

    static Dictionary<int, Dictionary<int, int>> dstEntity_hatredData = new Dictionary<int, Dictionary<int, int>>();
    static List<int> entityHarmRecord = new List<int>();

    public static void ClearRecord()
    {
        dstEntity_hatredData.Clear();
        entityHarmRecord.Clear();
    }

    public static void MonsterHatredAdd(List<int> tmp)
    {
        if (tmp.Count != 5) 
        {
            Debug.LogError("MonsterHatredAdd's format is wrong!");
            return;
        }
        List<PeEntity> currentDst;                                  //设置仇恨时，符合筛选而又已经存在的怪
        int dst, src;                                               //dst是被打怪的编号，src是发起殴打的怪的编号
        if (tmp[1] == 1) 
        {
            currentDst = new List<PeEntity>(EntityMgr.Instance.All);
            currentDst = currentDst.FindAll(delegate(PeEntity e)
            {
                if (e == null)
                    return false;
                if (e.proto == EEntityProto.Monster && e.entityProto.protoId == tmp[2])
                    return true;
                return false;
            });
            dst = tmp[2];
        }
        else
        {
            currentDst = new List<PeEntity>();
            PeEntity fixEntity = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(tmp[2]);
            if(fixEntity != null)
                currentDst.Add(fixEntity);
            dst = tmp[2] + fixMonsBase;
        }

        List<PeEntity> currentSrc;

        if (tmp[3] == 1)                //种类怪
        {
            src = tmp[4];
            currentSrc = new List<PeEntity>(EntityMgr.Instance.All);
            currentSrc = currentSrc.FindAll(delegate(PeEntity n)
            {
                if (n == null)
                    return false;
                if (n.proto == EEntityProto.Monster && n.entityProto.protoId == src)
                    return true;
                return false;
            });
        }
        else if (tmp[3] == 2)           //定点怪
        {
            src = tmp[4] + fixMonsBase;
            currentSrc = new List<PeEntity>();
            currentSrc.Add(SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(tmp[4]));
        }
        else if (tmp[3] == 3)           //所有怪
        {
            src = allMonsters;
            currentSrc = new List<PeEntity>(EntityMgr.Instance.All);
            currentSrc = currentSrc.FindAll(delegate(PeEntity n)
            {
                if (n == null)
                    return false;
                if (n.proto == EEntityProto.Monster)
                    return true;
                return false;
            });
        }
        else                            //30000.npcid
        {
            src = tmp[4];
            if (tmp[4] == allNpcs)
            {
                currentSrc = new List<PeEntity>(EntityMgr.Instance.All);
                currentSrc = currentSrc.FindAll(delegate(PeEntity n)
                {
                    if (n == null)
                        return false;
                    if (n.proto == EEntityProto.Npc || n.proto == EEntityProto.RandomNpc)
                        return true;
                    return false;
                });
            }
            else
            {
                currentSrc = new List<PeEntity>();
                if (tmp[4] == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
                    currentSrc.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
                else if (tmp[4] == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
                    currentSrc.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
                else if (tmp[4] == 25010)
                {
                    if (ServantLeaderCmpt.Instance.GetServant(0) != null)
                        currentSrc.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
                    if (ServantLeaderCmpt.Instance.GetServant(1) != null)
                        currentSrc.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
                }
                else
                    currentSrc.Add(EntityMgr.Instance.Get(tmp[4]));
            }
        }

        if (tmp[0] == 0) 
        {
            foreach (var item in currentDst)
            {
                SkEntity sk = item.aliveEntity;

                if (sk == null)
                    continue;
                if (sk.SpecialHatredData.ContainsKey(src))
                    sk.SpecialHatredData.Remove(src);
            }
            if (dstEntity_hatredData.ContainsKey(dst)) 
            {
                if(dstEntity_hatredData[dst].ContainsKey(src))
                    dstEntity_hatredData[dst].Remove(src);
                if (dstEntity_hatredData[dst].Count == 0)
                    dstEntity_hatredData.Remove(dst);
            }
            if(dstEntity_hatredData.Count == 0)
                MonsterEntityCreator.commonCreateEvent -= SpecialHatred.SetSpecialHatred;
        }
        else if (tmp[0] == 3)
        {
            foreach (var item in currentSrc)
            {
                if (item == null)
                    continue;

                SkEntity sk = item.aliveEntity;
                if (sk == null)
                    continue;
                if (sk.SpecialHatredData.ContainsKey(dst))
                    sk.SpecialHatredData[dst] = tmp[0];
                else
                    sk.SpecialHatredData.Add(dst, tmp[0]);
            }
            if (src > npcBase)
                return;

            if (dstEntity_hatredData.ContainsKey(src))
            {
                if (!dstEntity_hatredData[src].ContainsKey(dst))
                    dstEntity_hatredData[src].Add(dst, tmp[0]);
                else
                    dstEntity_hatredData[src][dst] = tmp[0];
            }
            else
            {
                Dictionary<int, int> dstData = new Dictionary<int, int>();   //被打怪身上存放的数据（哪些可以打我哪些不可以打我）
                dstData.Add(dst, tmp[0]);
                if (dstEntity_hatredData.Count == 0)
                    MonsterEntityCreator.commonCreateEvent += SpecialHatred.SetSpecialHatred;
                dstEntity_hatredData.Add(src, dstData);
            }
        }
        else
        {
            foreach (var item in currentDst)
            {
                if (item == null)
                    continue;

                SkEntity sk = item.aliveEntity;
                if (sk == null)
                    continue;
                if (sk.SpecialHatredData.ContainsKey(src))
                    sk.SpecialHatredData[src] = tmp[0];
                else
                    sk.SpecialHatredData.Add(src, tmp[0]);
            }
            if (dstEntity_hatredData.ContainsKey(dst))
            {
                if (!dstEntity_hatredData[dst].ContainsKey(src))
                    dstEntity_hatredData[dst].Add(src, tmp[0]);
                else
                    dstEntity_hatredData[dst][src] = tmp[0];
            }
            else
            {
                Dictionary<int, int> dstData = new Dictionary<int, int>();   //被打怪身上存放的数据（哪些可以打我哪些不可以打我）
                dstData.Add(src, tmp[0]);
                if (dstEntity_hatredData.Count == 0)
                    MonsterEntityCreator.commonCreateEvent += SpecialHatred.SetSpecialHatred;
                dstEntity_hatredData.Add(dst, dstData);
            }
        }
    }

    public static void NpcHatredAdd(List<int> tmp) 
    {
        if (tmp.Count != 4)
        {
            return;
        }
        int src;
        List<PeEntity> currentDst = new List<PeEntity>();
        if (tmp[1] == mainPlayer)
        {
            currentDst.Add(PeCreature.Instance.mainPlayer);
        }
        else if (tmp[1] == allNpcs)
        {
            currentDst = new List<PeEntity>(EntityMgr.Instance.All);
            currentDst = currentDst.FindAll(delegate(PeEntity e)
            {
                if (e == null)
                    return false;
                if (e.proto == EEntityProto.Npc || e.proto == EEntityProto.RandomNpc)
                    return true;
                return false;
            });
            currentDst.Add(PeCreature.Instance.mainPlayer);
        }
        else
        {
            if (tmp[1] == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
                currentDst.Add(ServantLeaderCmpt.Instance.GetServant(0).GetComponent<PeEntity>());
            else if (tmp[1] == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
                currentDst.Add(ServantLeaderCmpt.Instance.GetServant(1).GetComponent<PeEntity>());
            else if (tmp[1] == 25010)
            {
                if (ServantLeaderCmpt.Instance.GetServant(0) != null)
                    currentDst.Add(ServantLeaderCmpt.Instance.GetServant(0).GetComponent<PeEntity>());
                if (ServantLeaderCmpt.Instance.GetServant(1) != null)
                    currentDst.Add(ServantLeaderCmpt.Instance.GetServant(1).GetComponent<PeEntity>());
            }
            else
                currentDst.Add(EntityMgr.Instance.Get(tmp[1]));
        }

        List<PeEntity> currentSrc;

        if (tmp[2] == 1)                //种类怪
        {
            src = tmp[3];
            currentSrc = new List<PeEntity>(EntityMgr.Instance.All);
            currentSrc = currentSrc.FindAll(delegate(PeEntity n)
            {
                if (n == null)
                    return false;
                if (n.proto == EEntityProto.Monster && n.entityProto.protoId == src)
                    return true;
                return false;
            });
        }
        else if (tmp[2] == 2)           //定点怪
        {
            src = tmp[3] + fixMonsBase;
            currentSrc = new List<PeEntity>();
            currentSrc.Add(SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(tmp[3]));
        }
        else if (tmp[2] == 3)           //所有怪
        {
            src = allMonsters;
            currentSrc = new List<PeEntity>(EntityMgr.Instance.All);
            currentSrc = currentSrc.FindAll(delegate(PeEntity n)
            {
                if (n == null)
                    return false;
                if (n.proto == EEntityProto.Monster)
                    return true;
                return false;
            });
        }
        else                            //30000.npcid
        {
            src = tmp[3];
            if (tmp[3] == allNpcs)
            {
                currentSrc = new List<PeEntity>(EntityMgr.Instance.All);
                currentSrc = currentSrc.FindAll(delegate(PeEntity n)
                {
                    if (n == null)
                        return false;
                    if (n.proto == EEntityProto.Npc || n.proto == EEntityProto.RandomNpc)
                        return true;
                    return false;
                });
            }
            else
            {
                currentSrc = new List<PeEntity>();
                currentSrc.Add(EntityMgr.Instance.Get(tmp[3]));
            }
        }

        if (tmp[0] == 0)
        {
            //lw 11.24 :取消特殊仇恨时清除一下自己的仇恨列表
            foreach (var item in currentSrc)
            {
                if (item == null)
                    continue;

                if (item.target != null)
                    item.target.ClearEnemy();
            }

            foreach (var item in currentDst)
            {
                if (item == null)
                    continue;
                SkEntity sk = item.aliveEntity;
                if (sk == null)
                    continue;
                if (sk.SpecialHatredData.ContainsKey(src))
                    sk.SpecialHatredData.Remove(src);
            }
        }
        else if (tmp[0] == 3)
        {
            foreach (var item in currentSrc)
            {
                if (item == null)
                    continue;

                SkEntity sk = item.aliveEntity;
                if (sk == null)
                    continue;
                if (sk.SpecialHatredData.ContainsKey(tmp[1]))
                    sk.SpecialHatredData[tmp[1]] = tmp[0];
                else
                    sk.SpecialHatredData.Add(tmp[1], tmp[0]);
            }
            if (src > npcBase)
                return;

            if (dstEntity_hatredData.ContainsKey(src))
            {
                if (!dstEntity_hatredData[src].ContainsKey(tmp[1]))
                    dstEntity_hatredData[src].Add(tmp[1], tmp[0]);
                else
                    dstEntity_hatredData[src][tmp[1]] = tmp[0];
            }
            else
            {
                Dictionary<int, int> dstData = new Dictionary<int, int>();   //被打怪身上存放的数据（哪些可以打我哪些不可以打我）
                dstData.Add(tmp[1], tmp[0]);
                if (dstEntity_hatredData.Count == 0)
                    MonsterEntityCreator.commonCreateEvent += SpecialHatred.SetSpecialHatred;
                dstEntity_hatredData.Add(src, dstData);
            }
        }
        else
        {
            foreach (var item in currentDst)
            {
                if (item == null)
                    continue;

                SkEntity sk = item.aliveEntity;
                if (sk == null)
                    continue;
                if (sk.SpecialHatredData.ContainsKey(src))
                    sk.SpecialHatredData[src] = tmp[0];
                else
                    sk.SpecialHatredData.Add(src, tmp[0]);
            }

            if (tmp[0] == 1)
            {
                //lW11.24: 取消特殊仇恨时清除一下自己的仇恨列表
                foreach (var item in currentSrc)
                {
                    if (item == null)
                        continue;

                    if (item.target != null)
                        item.target.ClearEnemy();
                }
            }
        }
    }

    public static void HarmAdd(List<int> tmp) 
    {
        if (tmp.Count != 3)
        {
            return;
        }
        List<PeEntity> currentDst;
        int dst = 0;
        switch (tmp[1]) 
        {
            case 1:
                currentDst = new List<PeEntity>(EntityMgr.Instance.All);
                currentDst = currentDst.FindAll(delegate(PeEntity e)
                {
                    if (e == null)
                        return false;
                    if (e.proto == EEntityProto.Monster && e.entityProto.protoId == tmp[2])
                        return true;
                    return false;
                });
                dst = tmp[2];
                break;
            case 2:
                currentDst = new List<PeEntity>();
                PeEntity fixEntity = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(tmp[2]);
                if(fixEntity != null)
                    currentDst.Add(SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(tmp[2]));
                dst = tmp[2] + fixMonsBase;
                break;
            case 3:
                if (tmp[2] == mainPlayer)
                {
                    currentDst = new List<PeEntity>();
                    currentDst.Add(PeCreature.Instance.mainPlayer);
                }
                else if (tmp[2] == allNpcs)
                {
                    currentDst = new List<PeEntity>(EntityMgr.Instance.All);
                    currentDst = currentDst.FindAll(delegate(PeEntity e)
                    {
                        if (e == null)
                            return false;
                        if (e.proto == EEntityProto.Npc || e.proto == EEntityProto.RandomNpc)
                            return true;
                        return false;
                    });
                    currentDst.Add(PeCreature.Instance.mainPlayer);
                }
                else 
                {
                    currentDst = new List<PeEntity>();
                    currentDst.Add(EntityMgr.Instance.Get(tmp[2]));
                }
                break;
            default:
                currentDst = new List<PeEntity>();
                break;
        }
        if (tmp[0] == 1)
        {
            foreach (PeEntity item in currentDst)
            {
                SkEntity skEntity = item.aliveEntity;
                if(skEntity != null)
                    SkEntity.MountBuff(skEntity, 30200102, new List<int>(), new List<float>());
            }
            if (!entityHarmRecord.Contains(dst) && dst != 0) 
            {
                if (entityHarmRecord.Count == 0)
                    MonsterEntityCreator.commonCreateEvent += SetHarm;
                entityHarmRecord.Add(dst);
            }
        }
        else if (tmp[0] == 0)
        {
            foreach (PeEntity item in currentDst)
            {
                SkEntity skEntity = item.aliveEntity;
                if (skEntity != null)
                    skEntity.CancelBuffById(30200102);
            }
            if (entityHarmRecord.Contains(dst) && dst != 0) 
            {
                entityHarmRecord.Remove(dst);
                if (entityHarmRecord.Count == 0)
                    MonsterEntityCreator.commonCreateEvent -= SetHarm;
            }
        }
    }

    public static void IsHaveEnnemy(PeEntity me, ref List<PeEntity> result)                  //主动攻击目标 冉哥用
    {
        if (me == null || me.aliveEntity == null)
            return;
        if (me.aliveEntity.SpecialHatredData.Count == 0)
            return;

        foreach (var item in me.aliveEntity.SpecialHatredData)
        {
            if (item.Value != 3)
                continue;
            if (item.Key == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
                result.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
            else if (item.Key == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
                result.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
            else if (item.Key == 25010)
            {
                if (ServantLeaderCmpt.Instance.GetServant(0) != null)
                    result.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
                if (ServantLeaderCmpt.Instance.GetServant(1) != null)
                    result.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
            }
            else if (item.Key == mainPlayer)
                result.Add(PeCreature.Instance.mainPlayer);
            else
            {
                if (item.Key / 9000 == 1) 
                {
                    PeEntity tmp = EntityMgr.Instance.Get(item.Key);
                    if (tmp != null)
                        result.Add(tmp);
                }
                else if (item.Key / fixMonsBase == 1)
                {
                    PeEntity tmp = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.Key % fixMonsBase);
                    if (tmp != null)
                        result.Add(tmp);
                }
            }
        }
    }

    public static bool IsHaveSpecialHatred(PeEntity me,PeEntity ennemy,out int ignoreOrHighHatred)        //不会成为目标:1  优先成为目标:2   正常处理:0  冉哥用
    {
        ignoreOrHighHatred = 0;
        if (ennemy == null || ennemy.aliveEntity == null)
        {
            return false;
        }

        if (ennemy.aliveEntity.SpecialHatredData.Count == 0)
            return false;

        List<int> targets = new List<int>(ennemy.aliveEntity.SpecialHatredData.Keys);
        if (me.entityProto.proto == EEntityProto.Monster) 
        {
            if (targets.Contains(me.entityProto.protoId)) 
            {
                ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[me.entityProto.protoId];
                return true;
            }
            else if (targets.Contains(allMonsters))
            {
                ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[allMonsters];
                return true;
            }

            foreach (var item in targets)
            {
                if (item / 1000 != 8 || item == allMonsters)
                    continue;
                if (ennemy.aliveEntity.SpecialHatredData[item] == 3)
                    continue;
                PeEntity mon = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item % fixMonsBase);
                if (mon != null)
                {
                    if (mon is EntityGrp)
                    {
                        EntityGrp eg = mon as EntityGrp;
                        foreach (var item1 in eg.memberAgents)
                        {
                            if (!(item1 is SceneEntityPosAgent))
                                continue;
                            if ((item1 as SceneEntityPosAgent).entity == me)
                            {
                                ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[item];
                                return true;
                            }
                        }
                    }
                    else if (mon == me)
                    {
                        ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[item];
                        return true;
                    }
                }
            }
        }
        else if (me.entityProto.proto == EEntityProto.Npc || me.entityProto.proto == EEntityProto.RandomNpc)
        {
            if (targets.Contains(me.Id))
            {
                ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[me.Id];
                return true;
            }
            else if (targets.Contains(allNpcs))
            {
                ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[allNpcs];
                return true;
            }

            for (int i = 0; i < ServantLeaderCmpt.mMaxFollower; i++)
            {
                NpcCmpt nc = ServantLeaderCmpt.Instance.GetServant(i);
                if (nc != null)
                {
                    PeEntity npcEntity = nc.Entity;
                    if (npcEntity != null && npcEntity == me)
                    {
                        if (targets.Contains(25001 + i)) 
                        {
                            ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[25001 + i];
                            return true;
                        }
                        else if(targets.Contains(25010))
                        {
                            ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[25010];
                            return true;
                        }
                        //targets.Add(25001 + i);
                        //if (!targets.Contains(25010))
                        //    targets.Add(25010);
                    }
                }
            }
        }
        ignoreOrHighHatred = 0;
        return false;
    }

    static void SetHarm(PeEntity entity)                                    //王哥的怪物生成事件触发
    {
        SkEntity skEntity = entity.aliveEntity;
        if (skEntity == null)
            return;
        int protoID = entity.entityProto.protoId;
        int fixSpawnID = 0;
        foreach (int item in AISpawnPoint.s_spawnPointData.Keys)
        {
            if (SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item) == entity)
            {
                fixSpawnID = item;
                break;
            }
        }
        if (entityHarmRecord.Contains(protoID)) 
            SkEntity.MountBuff(skEntity, 30200102, new List<int>(), new List<float>());
        if (fixSpawnID == 0)
            return;
        if (entityHarmRecord.Contains(fixMonsBase + fixSpawnID))
            SkEntity.MountBuff(skEntity, 30200102, new List<int>(), new List<float>());
    }

    static void SetSpecialHatred(PeEntity entity)                           //王哥的怪物生成事件触发
    {
        if (dstEntity_hatredData.Count == 0)
            return;
        SkEntity me = entity.aliveEntity;
        if (me == null) 
        {
            return;
        }
        int protoID = entity.entityProto.protoId;
        int fixSpawnID = 0;
        foreach (int item in AISpawnPoint.s_spawnPointData.Keys)
        {
            if (SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item) == entity)
            {
                fixSpawnID = item;
            }
        }
        me.SpecialHatredData = new Dictionary<int, int>();
        if (dstEntity_hatredData.ContainsKey(protoID))
        {
            foreach (var item in dstEntity_hatredData[protoID])
            {
                me.SpecialHatredData.Add(item.Key, item.Value);
            }
        }
        if (fixSpawnID == 0)
            return;
        if (dstEntity_hatredData.ContainsKey(fixMonsBase + fixSpawnID))
        {
            foreach (var item in dstEntity_hatredData[fixMonsBase + fixSpawnID])
            {
                me.SpecialHatredData.Add(item.Key, item.Value);
            }
        }
    }
}
