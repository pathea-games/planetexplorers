using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System;
using SkillSystem;
using Pathea;
using System.IO;
//public enum RankType
//{
//	RankType_None 			= 0,
//	RankType_Miner 		= 1,
//	RankType_Timberjack = 2,
//	RankType_Architect 	= 3,
//	RankType_Warrior 		= 4,
//	RankType_Hunter 		= 5,
//	RankType_Engineer	= 6,
//	RankType_Driver 		= 7,
//	RankType_Max			= 7,
//}
public enum SkillState
{
    Lock,
    unLock,
    learnt,
}
public interface ISkillTree
{
    bool CheckEquipEnable(int type, int level);
    bool CheckDriveEnable(int type, int level);
    bool CheckMinerGetRare();
    bool CheckCutterGetRare();
    bool CheckHunterGetRare();
    bool CheckUnlockColony(int colonytype);
    bool CheckUnlockProductItemLevel(int unlocklevel);
    bool CheckUnlockProductItemType(int unlocktype);
    float CheckReduceTime(float srcTime);
    bool CheckBuildShape(int index);
    bool CheckBuildBlockLevel(int level);
    bool CheckUnlockBuildBlockBevel();
    bool CheckUnlockBuildBlockIso();
    bool CheckUnlockBuildBlockVoxel();
    bool CheckUnlockCusProduct(int unlocktype);
}
public enum SKTLearnResult
{
    SKTLearnResult_Success,
    SKTLearnResult_SkAliveEntityIsNULL,
    SKTLearnResult_DontHaveEnoughExp,
    SKTLearnResult_NeedLearntParentSkill,
    SKTLearnResult_DataError,
}
public class SkillTreeUnit
{
    public int _id;
    public int _mainType;
    public int _skillType;
    public int _skillGroup;
    public int _skillGrade;
    public int _level;
    public UInt64 _exp;
    public string _parent;
    public string _value;
    public string _sprName;
    public SkillState _state;
    public string _desc;
	public int     _descIndex;
    public int _maxLevel;
    public const bool ENABLESKTSYSTEM = false;
    public SkillTreeUnit()
    {
        _state = SkillState.Lock;
    }

    List<List<string>> cmd = new List<List<string>>();

    //	delegate bool CheckLimit(int type, int level);
    //	List<CheckLimit> _limitFuns = new List<CheckLimit>();
    //
    //	void BindFuns(string[] str)
    //	{
    //		foreach(List<string> iter in cmd)
    //		{
    //			if(iter != null)
    //			{
    //				if(iter.Count > 1)
    //				{
    //					if(iter[0] == "equiplimit")
    //					{
    //						if(!_limitFuns.Contains(CheckEquipEnable))
    //						{
    //							_limitFuns.Add( CheckEquipEnable);
    //						}
    //					}
    //					else if(iter[0] == "drivelimit")
    //					{
    //						if(!_limitFuns.Contains(CheckDriveEnable))
    //						{
    //							_limitFuns.Add( CheckDriveEnable);
    //						}
    //					}
    //				}
    //			}
    //		}		
    //	}

    public bool CheckLockItemType(int locktype)
    {//"lockitem",type all items  were unlocked default
        //
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "lockitem")
                    {
                        for (int i = 1; i < iter.Count; i++)
                        {
                            if (Convert.ToInt32(iter[i]) == locktype)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckLockColonyType(int locktype)
    {//"lockcolony",type all colonyitems  were unlocked default
        //
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "lockcolony")
                    {
                        for (int i = 1; i < iter.Count; i++)
                        {
                            if (Convert.ToInt32(iter[i]) == locktype)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    bool CheckLockProduceItemType(int locktype)
    {//"lockpitem",type all items  were unlocked default
        //
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "lockpitem")
                    {
                        for (int i = 1; i < iter.Count; i++)
                        {
                            if (Convert.ToInt32(iter[i]) == locktype)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckEquipEnable(int type, int level)
    {//"equiplimit",type,level,type1,level1....

        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "equiplimit")
                    {
                        for (int i = 1; i < iter.Count; i = i + 2)
                        {
                            if (Convert.ToInt32(iter[i]) == type && Convert.ToInt32(iter[i + 1]) >= level)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckDriveEnable(int type, int level)
    {//"drivelimit",type,level,type1,level1....
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "drivelimit")
                    {
                        for (int i = 1; i < iter.Count; i = i + 2)
                        {
                            if (Convert.ToInt32(iter[i]) == type && Convert.ToInt32(iter[i + 1]) >= level)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }


    public bool CheckMinerGetRare()
    {//"minergetrare"
        foreach (List<string> iter in cmd)
        {
            if (iter.Count > 1)
            {
                if (iter[0] == "minergetrare")
                {
                    for (int i = 1; i < iter.Count; i++)
                    {
                        if (Convert.ToInt32(iter[i]) == 1)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckCutterGetRare()
    {//"cuttergetrare"
        foreach (List<string> iter in cmd)
        {
            if (iter.Count > 1)
            {
                if (iter[0] == "cuttergetrare")
                {
                    for (int i = 1; i < iter.Count; i++)
                    {
                        if (Convert.ToInt32(iter[i]) == 1)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckHunterGetRare()
    {//"huntergetrare"
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count == 1)
                {
                    if (iter[0] == "huntergetrare")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool CheckUnlockColony(int colonytype)
    {//"unlockcolony",type
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "unlockcolony")
                    {
                        for (int i = 1; i < iter.Count; i++)
                        {
                            if (Convert.ToInt32(iter[i]) == colonytype)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckUnlockProductItemLevel(int unlocklevel)
    {//"unlockproductlevel",level
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "unlockproductlevel")
                    {
                        if (unlocklevel <= Convert.ToInt32(iter[1]))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckUnlockProductItemType(int unlocktype)
    {//"unlockproducttype",itemclassid,itemclassid,......
        if (CheckLockProduceItemType(unlocktype))
        {
            foreach (List<string> iter in cmd)
            {
                if (iter != null)
                {
                    if (iter.Count > 1)
                    {
                        if (iter[0] == "unlockproducttype")
                        {
                            for (int i = 1; i < iter.Count; i++)
                            {
                                if (Convert.ToInt32(iter[i]) == unlocktype)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        return true;
    }

    public float CheckReduceTime(float srcTime)
    {
        //"reducetime",0.01
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "reducetime")
                    {
                        srcTime = srcTime - srcTime * Convert.ToSingle(iter[1]);
                    }
                }
            }
        }
        return srcTime;
    }

    public bool CheckBuildShape(int index)
    {//"buildshape",index
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "buildshape")
                    {
                        if (index <= Convert.ToInt32(iter[1]))
                            return true;
                    }
                }
            }
        }
        return false;
    }

    public bool CheckBuildBlockLevel(int level)
    {//"blocklevel",level
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "blocklevel")
                    {
                        if (level <= Convert.ToInt32(iter[1]))
                            return true;
                    }
                }
            }
        }
        return false;
    }
    public bool CheckUnlockBuildBlockBevel()
    {//"ulblockbevel",1
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "ulblockbevel")
                    {
                        if (1 == Convert.ToInt32(iter[1]))
                            return true;
                    }
                }
            }
        }
        return false;
    }

    public bool CheckUnlockBuildBlockIso()
    {//"unlockbiso",1
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "unlockbiso")
                    {
                        if (1 == Convert.ToInt32(iter[1]))
                            return true;
                    }
                }
            }
        }
        return false;
    }
    public bool CheckUnlockBuildBlockVoxel()
    {//"unlockbvoxel",1
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "unlockbvoxel")
                    {
                        if (1 == Convert.ToInt32(iter[1]))
                            return true;
                    }
                }
            }
        }
        return false;
    }

    public bool CheckUnlockCusProduct(int unlocktype)
    {
        //"ulcusproduct",type,type,type...
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "ulcusproduct")
                    {
                        for (int i = 1; i < iter.Count; i++)
                        {
                            if (Convert.ToInt32(iter[i]) == unlocktype)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool RefreshBuff(SkAliveEntity ske)
    {
        if (ske == null)
            return false;
        foreach (List<string> iter in cmd)
        {
            if (iter != null)
            {
                if (iter.Count > 1)
                {
                    if (iter[0] == "buff")
                    {//"buff",buffid,atrtype,atrvalue,atrtype1,atrvalue1....
                        int buffid = Convert.ToInt32(iter[1]);
                        List<int> atrtype = new List<int>();
                        List<float> atrvalue = new List<float>();
                        for (int i = 2; i < iter.Count; i = i + 2)
                        {
                            atrtype.Add(Convert.ToInt32(iter[i]));
                            atrvalue.Add(Convert.ToSingle(iter[i + 1]));
                        }
                        if (atrtype.Count > 0 && atrvalue.Count > 0)
                        {
                            //remove old buff
                            SkEntity.UnmountBuff(ske, buffid);
                            //add buff
                            SkEntity.MountBuff(ske, buffid, atrtype, atrvalue);
                        }
                    }
                    else if (iter[0] == "scanradius")
                    {//"scanradius",Radius
                        int radius = Convert.ToInt32(iter[1]);
                        MSScan.Instance.radius = radius;
                    }
                    else if (iter[0] == "scanmat")
                    {//"scanmat",mat1,mat2...					
                        for (int i = 1; i < iter.Count; i++)
                        {
                            MetalScanData.AddMetalScan(Convert.ToInt32(iter[i]));
                        }
                    }
                }
            }
        }
        return true;
    }

    public bool Parse(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return false;
        }
        string[] args = line.Split(';');

        for (int i = 0; i < args.Length; i++)
        {
            if (cmd == null)
                cmd = new List<List<string>>();

            string[] args1 = args[i].Split(',');
            if (cmd.Count <= i)
                cmd.Add(new List<string>());
            cmd[i].AddRange(args1);
        }
        return true;
    }
}
//
public class SkillTreeUnitMgr : PeCmpt, ISkillTree
{
    [HideInInspector]
    public NetworkInterface _net;
    List<SkillTreeUnit> _learntSkills = new List<SkillTreeUnit>();
    SkAliveEntity _ske = null;
    public static int _defaultSkillType = 0;
    public override void Start()
    {
        base.Start();
        _ske = gameObject.GetComponent<SkAliveEntity>();
        InitDefaultSkill();
        RefreshAllBuffs();
    }

    #region basefunc
    public void InitDefaultSkill()
    {
        if (RandomMapConfig.useSkillTree)
        {
            if (_learntSkills.Find((iter) => iter._skillType == _defaultSkillType) == null)
                SKTLearn(_defaultSkillType);
        }
    }



    public void SetNet(NetworkInterface net)
    {
        _net = net;
    }

    public SkillTreeUnit FindSkillUnit(int SkillType)
    {
        foreach (SkillTreeUnit itr in _learntSkills)
        {
            if (itr._skillType == SkillType)
                return itr;
        }
        return null;
    }

    public void RemoveSkillUnit(SkillTreeUnit unit)
    {
        _learntSkills.Remove(unit);
    }

    public void AddSkillUnit(SkillTreeUnit unit)
    {
        if (_learntSkills.Contains(unit))
            return;
        unit.RefreshBuff(_ske);
        _learntSkills.Add(unit);
    }

    public SkillTreeUnit FindSkillUnitByID(int skillid)
    {
        foreach (SkillTreeUnit itr in _learntSkills)
        {
            if (itr._id == skillid)
                return itr;
        }
        return null;
    }

    public List<SkillTreeUnit> GetSkillsByMainType(int mainType)
    {
        if (_learntSkills == null)
            return null;
        List<SkillTreeUnit> skills = new List<SkillTreeUnit>();
        foreach (var iter in _learntSkills)
        {
            if (iter._mainType == mainType)
            {
                skills.Add(iter);
            }
        }
        return skills;
    }

    public void ChangeSkillState(SkillTreeUnit skill)
    {
        skill._state = SkillState.Lock;
        if (_learntSkills.Contains(skill))
        {
            skill._state = SkillState.learnt;
            return;
        }
        string[] args = skill._parent.Split(';');
        for (int i = 0; i < args.Length; i++)
        {
            SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(System.Int32.Parse(args[i]));
            if (skillUnit == null)
            {
                if (args[i] == "0")
                {
                    skill._state = SkillState.unLock;
                }
                else
                {
                    skill._state = SkillState.Lock;
                    Debug.LogError("parent skill is not exsit");
                }
                return;
            }
            else
            {
                SkillTreeUnit skilllearnt = FindSkillUnit(skillUnit._skillType);
                if (skilllearnt != null)
                {
                    if (skilllearnt._level >= skillUnit._level)
                    {
                        if (i == args.Length - 1)
                            skill._state = SkillState.unLock;
                    }
                    else
                    {
                        skill._state = SkillState.Lock;
                        return;
                    }
                }
                else
                {
                    skill._state = SkillState.Lock;
                    return;
                }
            }
        }
    }



    #endregion

    bool CheckCommon(int level = 1)
    {
        //lz-2016.09.01 先判断类型比较保险
        if (!Pathea.PeGameMgr.IsAdventure)
            return true;

        if (!RandomMapConfig.useSkillTree)
            return true;
       
        if (level == 0)
            return true;
        return false;
    }
    #region functional
    public bool CheckEquipEnable(int type, int level)
    {
        if (CheckCommon(level))
            return true;
        bool isLock = false;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null && iter.CheckLockItemType(type))
            {
                isLock = true;
            }
        }

        if (isLock)
        {
            foreach (SkillTreeUnit iter in _learntSkills)
            {
                if (iter != null && iter.CheckEquipEnable(type, level))
                    return true;
            }

            //lz-2016.10.31 错误 #5336 提示修改为Insufficient skill to use this item
            new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
            return false;
        }
        else
            return true;

    }

    public bool CheckDriveEnable(int type, int level)
    {
        if (CheckCommon(level))
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null && iter.CheckDriveEnable(type, level))
                return true;
        }
        new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
        return false;
    }

    public bool CheckMinerGetRare()
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckMinerGetRare())
                    return true;
            }
        }

        return false;
    }

    public bool CheckCutterGetRare()
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckCutterGetRare())
                    return true;
            }
        }

        return false;
    }

    public bool CheckHunterGetRare()
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckHunterGetRare())
                    return true;
            }
        }

        return false;
    }


    public bool CheckUnlockColony(int itemclasstype)
    {
        if (CheckCommon())
            return true;
        bool isLock = false;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null && iter.CheckLockColonyType(itemclasstype))
            {
                isLock = true;
            }
        }

        if (isLock)
        {
            foreach (SkillTreeUnit iter in _learntSkills)
            {
                if (iter != null)
                {
                    if (iter.CheckUnlockColony(itemclasstype))
                        return true;
                }
            }
            new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
            return false;
        }
        else
            return true;
    }

    public bool CheckUnlockProductItemLevel(int unlocklevel)
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckUnlockProductItemLevel(unlocklevel))
                    return true;
            }
        }

        return false;
    }

    public bool CheckUnlockProductItemType(int unlocktype)
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckUnlockProductItemType(unlocktype))
                    return true;
            }
        }

        return false;
    }

    public float CheckReduceTime(float srcTime)
    {
        if (CheckCommon())
            return srcTime;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                iter.CheckReduceTime(srcTime);
            }
        }
        return srcTime;
    }

    public bool CheckBuildShape(int index)
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckBuildShape(index))
                    return true;
            }
        }

        return false;
    }

    public bool CheckBuildBlockLevel(int level)
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckBuildBlockLevel(level))
                    return true;
            }
        }

        return false;
    }

    public bool CheckUnlockBuildBlockBevel()
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckUnlockBuildBlockBevel())
                    return true;
            }
        }

        return false;
    }

    public bool CheckUnlockBuildBlockIso()
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckUnlockBuildBlockIso())
                    return true;
            }
        }

        return false;
    }

    public bool CheckUnlockBuildBlockVoxel()
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckUnlockBuildBlockVoxel())
                    return true;
            }
        }

        return false;
    }

    public bool CheckUnlockCusProduct(int unlocktype)
    {
        if (CheckCommon())
            return true;
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                if (iter.CheckUnlockCusProduct(unlocktype))
                    return true;
            }
        }

        return false;
    }

    #endregion


    #region op
    public void RefreshAllBuffs()
    {
        foreach (SkillTreeUnit iter in _learntSkills)
        {
            if (iter != null)
            {
                iter.RefreshBuff(_ske);
            }
        }
    }
    #endregion

    #region skill level up
    bool DecExp(UInt64 needExp)
    {
        if (_ske == null)
        {
            return false;
        }
        if (_ske.GetAttribute((int)AttribType.Exp) < needExp)
            return false;
        _ske.SetAttribute((int)AttribType.Exp, _ske.GetAttribute((int)AttribType.Exp) - needExp);
        return true;
    }

    SKTLearnResult CheckLevelUpCondition(SkillTreeUnit skill)
    {
        if (_ske == null)
        {
            return SKTLearnResult.SKTLearnResult_SkAliveEntityIsNULL;
        }
        if (skill != null)
        {
            if (_ske.GetAttribute((int)AttribType.Exp) < skill._exp)
                return SKTLearnResult.SKTLearnResult_DontHaveEnoughExp;
            if (skill._parent != "0")
            {
                string[] args = skill._parent.Split(';');
                for (int i = 0; i < args.Length; i++)
                {
                    SkillTreeUnit skillunit = SkillTreeInfo.GetSkillUnit(System.Int32.Parse(args[i]));
                    if (skillunit == null)
                    {
                        return SKTLearnResult.SKTLearnResult_DataError;
                    }
                    else
                    {
                        SkillTreeUnit skilllearnt = FindSkillUnit(skillunit._skillType);
                        if (skilllearnt != null)
                        {
                            if (skilllearnt._level >= skillunit._level)
                            {
                                if (i == args.Length - 1)
                                    return SKTLearnResult.SKTLearnResult_Success;
                            }
                            else
                                return SKTLearnResult.SKTLearnResult_NeedLearntParentSkill;
                        }
                    }
                }
                return SKTLearnResult.SKTLearnResult_DataError;
            }
            return SKTLearnResult.SKTLearnResult_Success;
        }
        return SKTLearnResult.SKTLearnResult_DataError;
    }

    public SKTLearnResult SKTLearn(int skillType)
    {

        SkillTreeUnit skill = FindSkillUnit(skillType);
        SkillTreeUnit nextSkill;
        if (skill != null)
        {
            int nextLevel = skill._level + 1;
            nextSkill = SkillTreeInfo.GetSkillUnit(skillType, nextLevel);
            if (nextSkill != null)
            {
                SKTLearnResult result = CheckLevelUpCondition(nextSkill);
                if (result != SKTLearnResult.SKTLearnResult_Success)
                    return result;
                if (!DecExp(nextSkill._exp))
                    return SKTLearnResult.SKTLearnResult_DontHaveEnoughExp;
                RemoveSkillUnit(skill);
                AddSkillUnit(nextSkill);
            }
            else
                return SKTLearnResult.SKTLearnResult_DataError;
        }
        else
        {
            nextSkill = SkillTreeInfo.GetMinLevelSkillByType(skillType);
            if (nextSkill != null)
            {
                SKTLearnResult result = CheckLevelUpCondition(nextSkill);
                if (result != SKTLearnResult.SKTLearnResult_Success)
                    return result;
                if (!DecExp(nextSkill._exp))
                    return SKTLearnResult.SKTLearnResult_DontHaveEnoughExp; ;
                AddSkillUnit(nextSkill);
            }
            else
                return SKTLearnResult.SKTLearnResult_DataError;
        }
        SkillTreeInfo.RefreshUI(nextSkill._mainType);
        if (GameConfig.IsMultiMode && _net != null)
            _net.RPCServer(EPacketType.PT_InGame_SKTLevelUp, skillType, PlayerNetwork.mainPlayerId);
        return SKTLearnResult.SKTLearnResult_Success;
    }
    public ulong GetNextExpBySkillType(int skillType)
    {
        SkillTreeUnit skill = FindSkillUnit(skillType);
        SkillTreeUnit nextSkill;
        if (skill != null)
        {
            int nextLevel = skill._level + 1;
            nextSkill = SkillTreeInfo.GetSkillUnit(skillType, nextLevel);
            if (nextSkill != null)
            {
                return nextSkill._exp;
            }
            else
                return 0;
        }
        else
        {
            nextSkill = SkillTreeInfo.GetMinLevelSkillByType(skillType);
            if (nextSkill != null)
            {
                return nextSkill._exp;
            }
            else
                return 0;
        }
    }
    #endregion


    #region cmpt func
    public override void Serialize(BinaryWriter _out)
    {
        if (_net != null)
            return;
        _out.Write(_learntSkills.Count);
        foreach (SkillTreeUnit itr in _learntSkills)
        {
            _out.Write(itr._id);
        }
    }

    public override void Deserialize(BinaryReader _in)
    {
        if (_net != null)
            return;
        int count = _in.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            int id = _in.ReadInt32();
            SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(id);
            if (skillUnit != null)
                AddSkillUnit(skillUnit);
        }
    }
    #endregion
}


public class SkillMainType
{
    public int _mainType;
    public List<string> _icon = new List<string>();
    public string _desc;
    public int _skillGroup;
}



public class SkillTreeInfo
{

    static Dictionary<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> _skillTreeInfo;// = new Dictionary<int,Dictionary<int,Dictionary<int,SkillTreeUnit>>>();
    static Dictionary<int, List<SkillMainType>> _skillMainTypeInfo;
    public delegate void RefreshUIEvent(int mainType);
    static RefreshUIEvent CallBackRefresh;
    public static Dictionary<int, List<SkillMainType>> SkillMainTypeInfo
    {
        get
        {
            return _skillMainTypeInfo;
        }
    }
    #region SkillTree
    public static void LoadData()
    {
        _skillTreeInfo = new Dictionary<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>>();
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("skilltree");
//        int nFieldCount = reader.FieldCount;
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
            int maintype = Convert.ToInt32(reader.GetString(reader.GetOrdinal("maintype")));
            int skilltype = Convert.ToInt32(reader.GetString(reader.GetOrdinal("skilltype")));
            int skillgroup = Convert.ToInt32(reader.GetString(reader.GetOrdinal("skillgroup")));
            int skillgrade = Convert.ToInt32(reader.GetString(reader.GetOrdinal("skillgrade")));
            int level = Convert.ToInt32(reader.GetString(reader.GetOrdinal("level")));
            UInt64 exp = Convert.ToUInt64(reader.GetString(reader.GetOrdinal("exp")));
            string parent = reader.GetString(reader.GetOrdinal("parent"));
            string value = reader.GetString(reader.GetOrdinal("value"));
            string sprName = reader.GetString(reader.GetOrdinal("sprname"));
            int desc = Convert.ToInt32(reader.GetString(reader.GetOrdinal("desc")));
            if (!_skillTreeInfo.ContainsKey(maintype))
                _skillTreeInfo[maintype] = new Dictionary<int, Dictionary<int, SkillTreeUnit>>();
            if (!_skillTreeInfo[maintype].ContainsKey(skilltype))
                _skillTreeInfo[maintype][skilltype] = new Dictionary<int, SkillTreeUnit>();
            SkillTreeUnit info = new SkillTreeUnit();
            info._id = id;
            info._mainType = maintype;
            info._skillType = skilltype;
            info._skillGroup = skillgroup;
            info._skillGrade = skillgrade;
            info._level = level;
            info._exp = exp;
            info._parent = parent;
            info._value = value;
            info._sprName = sprName;
			info._descIndex = desc;
            info._desc = PELocalization.GetString(desc);
            info.Parse(value);
            _skillTreeInfo[maintype][skilltype][level] = info;
        }
        foreach (var iter in _skillTreeInfo)
        {
            foreach (var iter1 in iter.Value)
            {
                foreach (var iter2 in iter1.Value)
                {
                    SkillTreeUnit unit = GetMaxLevelSkillByType(iter2.Value._skillType);
                    if (unit != null)
                        iter2.Value._maxLevel = unit._level;
                }
            }
        }
        LoadMainTypeData();
    }



    public static UInt64 GetEpx(int maintype, int skilltype, int level)
    {
        if (_skillTreeInfo.ContainsKey(maintype))
        {
            if (_skillTreeInfo[maintype].ContainsKey(skilltype))
            {
                if (_skillTreeInfo[maintype][skilltype].ContainsKey(level))
                    return _skillTreeInfo[maintype][skilltype][level]._exp;
            }
        }
        return 0;
    }
    public static string GetParentSkill(int maintype, int skilltype, int level)
    {
        if (_skillTreeInfo.ContainsKey(maintype))
        {
            if (_skillTreeInfo[maintype].ContainsKey(skilltype))
            {
                if (_skillTreeInfo[maintype][skilltype].ContainsKey(level))
                    return _skillTreeInfo[maintype][skilltype][level]._parent;
            }
        }
        return "";
    }
    public static string GetValue(int maintype, int skilltype, int level)
    {
        if (_skillTreeInfo.ContainsKey(maintype))
        {
            if (_skillTreeInfo[maintype].ContainsKey(skilltype))
            {
                if (_skillTreeInfo[maintype][skilltype].ContainsKey(level))
                    return _skillTreeInfo[maintype][skilltype][level]._value;
            }
        }
        return null;
    }

    public static SkillTreeUnit GetSkillUnit(int id)
    {
        foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> kvp1 in _skillTreeInfo)
        {
            foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> kvp2 in kvp1.Value)
            {
                foreach (KeyValuePair<int, SkillTreeUnit> item in kvp2.Value)
                {
                    if (item.Value._id == id)
                        return item.Value;
                }
            }
        }
        return null;
    }

    public static SkillTreeUnit GetMinLevelSkillByType(int type)
    {
        SkillTreeUnit temp = null;
        foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> kvp1 in _skillTreeInfo)
        {
            foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> kvp2 in kvp1.Value)
            {
                foreach (KeyValuePair<int, SkillTreeUnit> item in kvp2.Value)
                {
                    if (item.Value._skillType == type)
                    {
                        if (temp == null)
                        {
                            temp = item.Value;
                        }
                        else
                        {
                            if (temp._level > item.Value._level)
                                temp = item.Value;
                        }
                    }
                }
            }
        }
        return temp;
    }

    public static SkillTreeUnit GetMaxLevelSkillByType(int type)
    {
        SkillTreeUnit temp = null;
        foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> kvp1 in _skillTreeInfo)
        {
            foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> kvp2 in kvp1.Value)
            {
                foreach (KeyValuePair<int, SkillTreeUnit> item in kvp2.Value)
                {
                    if (item.Value._skillType == type)
                    {
                        if (temp == null)
                        {
                            temp = item.Value;
                        }
                        else
                        {
                            if (temp._level < item.Value._level)
                                temp = item.Value;
                        }
                    }
                }
            }
        }
        return temp;
    }

    public static List<SkillTreeUnit> GetMinLevelByMainType(int mainType)
    {
        List<SkillTreeUnit> skills = new List<SkillTreeUnit>();
        if (_skillTreeInfo.ContainsKey(mainType))
        {
            foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> kvp1 in _skillTreeInfo[mainType])
            {
                SkillTreeUnit unit = null;
                foreach (KeyValuePair<int, SkillTreeUnit> item in kvp1.Value)
                {
                    if (unit == null) unit = item.Value;
                    if (unit != null && item.Value != null)
                    {
                        if (unit._level > item.Value._level)
                            unit = item.Value;
                    }
                }
                if (unit != null)
                {
                    skills.Add(unit);
                }
            }
        }
        if (skills != null)
        {
            skills.Sort((left, right) =>
                   {
                       if (left._skillGrade > right._skillGrade)
                           return 1;
                       else if (left._skillGrade == right._skillGrade)
                           return 0;
                       else
                           return -1;
                   });
        }
        return skills;
    }

    public static SkillTreeUnit GetSkillUnit(int type, int level)
    {
        foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> kvp in _skillTreeInfo)
        {
            foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> kvp1 in kvp.Value)
            {
                foreach (KeyValuePair<int, SkillTreeUnit> item in kvp1.Value)
                {
                    if (item.Value._skillType == type && item.Value._level == level)
                        return item.Value;
                }
            }
        }
        return null;
    }
    #endregion
    #region SkillMainType
    static void LoadMainTypeData()
    {
        _skillMainTypeInfo = new Dictionary<int, List<SkillMainType>>();
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("skilltreemt");
//        int nFieldCount = reader.FieldCount;
        while (reader.Read())
        {
            SkillMainType info = new SkillMainType();
            info._mainType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("typeid")));
            string temp = reader.GetString(reader.GetOrdinal("icon"));
            string[] icons = temp.Split(',');
            info._icon.AddRange(icons);
            info._desc = PELocalization.GetString(Convert.ToInt32(reader.GetString(reader.GetOrdinal("desc"))));
            info._skillGroup = Convert.ToInt32(reader.GetString(reader.GetOrdinal("skillgroup")));
            if (!_skillMainTypeInfo.ContainsKey(info._skillGroup))
                _skillMainTypeInfo[info._skillGroup] = new List<SkillMainType>();
            _skillMainTypeInfo[info._skillGroup].Add(info);
        }
    }

    public static void RefreshUI(int mainType)
    {
        if (CallBackRefresh != null)
            CallBackRefresh(mainType);
    }
    #endregion

    static void FindChindren(List<string> parentSkills, List<SkillTreeUnit> allSkills, Dictionary<string, List<SkillTreeUnit>> outSkills)
    {
        if (outSkills == null)
            outSkills = new Dictionary<string, List<SkillTreeUnit>>();
        Dictionary<string, List<SkillTreeUnit>> temp = new Dictionary<string, List<SkillTreeUnit>>();
        if (parentSkills == null || parentSkills.Count == 0)
        {
            for (int i = 0; i < allSkills.Count; i++)
            {
                if (allSkills[i]._parent == "0")
                {
                    if (!temp.ContainsKey("0"))
                        temp["0"] = new List<SkillTreeUnit>();
                    temp["0"].Add(allSkills[i]);
                    allSkills.RemoveAt(i);
                    i--;
                }
            }
        }
        else
        {
            for (int n = 0; n < allSkills.Count; n++)
            {
                string[] parents = allSkills[n]._parent.Split(';');
                if (parents.Length > 0)
                {
                    bool bFound = false;
                    for (int i = 0; i < parents.Length; i++)
                    {
                        bool bFound1 = false;
                        for (int j = 0; j < parentSkills.Count; j++)
                        {
                            if (parentSkills[j] == parents[i])
                            {
                                bFound1 = true;
                                break;
                            }
                        }
                        if (bFound1 == false)
                        {
                            break;
                        }
                        if (i == parents.Length - 1)
                            bFound = true;
                    }
                    if (bFound)
                    {
                        if (!temp.ContainsKey(allSkills[n]._parent))
                            temp[allSkills[n]._parent] = new List<SkillTreeUnit>();
                        temp[allSkills[n]._parent].Add(allSkills[n]);
                        allSkills.RemoveAt(n);
                        n--;
                    }
                }
            }
        }
        if (temp.Count > 0)
        {
            if (allSkills.Count == 0)
                return;
            foreach (var iter in temp)
            {
                if (!outSkills.ContainsKey(iter.Key))
                    outSkills[iter.Key] = new List<SkillTreeUnit>();
                outSkills[iter.Key].AddRange(iter.Value);
                List<string> parents = new List<string>();
                parents.AddRange(iter.Key.Split(';'));
                FindChindren(parents, allSkills, outSkills);
            }
        }
    }
    #region interface



    public static void SetUICallBack(RefreshUIEvent callBack)
    {
        CallBackRefresh = callBack;
    }

    public static Dictionary<int, List<SkillTreeUnit>> GetUIShowList(int mainType, SkillTreeUnitMgr mgr)
    {
        //[grade][unit]
        List<SkillTreeUnit> minLevelSkills = SkillTreeInfo.GetMinLevelByMainType(mainType);
        List<SkillTreeUnit> learntSkills = mgr.GetSkillsByMainType(mainType);
        //[grade][unit]
        Dictionary<int, List<SkillTreeUnit>> outSkills = new Dictionary<int, List<SkillTreeUnit>>();
        for (int j = 0; j < minLevelSkills.Count; j++)
        {
            for (int i = 0; i < learntSkills.Count; i++)
            {
                if (learntSkills[i]._skillType == minLevelSkills[j]._skillType)
                {
                    minLevelSkills[j] = learntSkills[i];
                    break;
                }
            }
            mgr.ChangeSkillState(minLevelSkills[j]);
            if (!outSkills.ContainsKey(minLevelSkills[j]._skillGrade))
                outSkills[minLevelSkills[j]._skillGrade] = new List<SkillTreeUnit>();
            outSkills[minLevelSkills[j]._skillGrade].Add(minLevelSkills[j]);
        }
		foreach(var iter in outSkills)
		{
			if(iter.Value != null && iter.Value.Count > 1)
			{
				iter.Value.Sort((left, right) =>
				                {
						if (left._descIndex > right._descIndex)
							return 1;
						else if (left._descIndex ==right._descIndex)
							return 0;
						else
							return -1;
				});
			}
		}
		return outSkills;
	}
	
	#endregion
	
}

