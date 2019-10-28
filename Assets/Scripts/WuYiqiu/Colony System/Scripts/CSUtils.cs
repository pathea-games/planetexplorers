using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;

public static class CSUtils
{
    /// <summary>
    /// Gets the name  of the Occupation.
    /// </summary>
    /// <returns>The occupa name.</returns>
    /// <param name="occupation">Occupation type in CSConst.pot~</param>
    public static string GetOccupaName(int occupation)
    {
        string name = "";

        switch (occupation)
        {
            case CSConst.potDweller:
                //name = "Dweller";
                name = PELocalization.GetString(82211001);
                break;
            case CSConst.potWorker:
                //			name = "Worker";
                name = PELocalization.GetString(82211002);
                break;
            case CSConst.potSoldier:
                //			name = "Soldier";
                name = PELocalization.GetString(82211003);
                break;
            case CSConst.potFarmer:
                //			name = "Farmer";
                name = PELocalization.GetString(82211004);
                break;
            case CSConst.potFollower:
                //			name = "Follower";
                name = PELocalization.GetString(82211005);
                break;
            case CSConst.potProcessor:
                name = PELocalization.GetString(82230015);
                break;
            case CSConst.potDoctor:
                name = PELocalization.GetString(82211006);
                //name = "Doctor";
                break;
            case CSConst.potTrainer:
                name = PELocalization.GetString(82211007);
                //name = "Instructor";
                break;
            default:
                break;
        }

        return name;

    }

    /// <summary>
    /// Gets the name of the work mode.
    /// </summary>
    /// <returns>The work mode name.</returns>
    /// <param name="mode">Work mode type in CSConst.pwt~</param>
    public static string GetWorkModeName(int mode)
    {
        string name = "";

        switch (mode)
        {
            case CSConst.pwtNormalWork:
                //			name = "Normal";
                name = PELocalization.GetString(82212001);
                break;
            case CSConst.pwtWorkWhenNeed:
                //			name = "WorkWhenNeed";
                name = PELocalization.GetString(82212002);
                break;
            case CSConst.pwtWorkaholic:
                //			name = "Workaholic";
                name = PELocalization.GetString(82212003);
                break;
            case CSConst.pwtFarmForMag:
                //			name = "Manage";
                name = PELocalization.GetString(82212101);
                break;
            case CSConst.pwtFarmForPlant:
                //			name = "Plant";
                name = PELocalization.GetString(82212102);
                break;
            case CSConst.pwtFarmForHarvest:
                //			name = "Harvest";
                name = PELocalization.GetString(82212103);
                break;
            case CSConst.pwtPatrol:
                //			name = "Patrol";
                name = PELocalization.GetString(82212201);
                break;
            case CSConst.pwtGuard:
                //			name = "Guard";
                name = PELocalization.GetString(82212202);
                break;
            default:
                break;
        }

        return name;
    }

    /// <summary>
    ///  <CETC> Gets the name of different type Entity y.
    /// </summary>
    /// <returns>The entity name.</returns>
    /// <param name="type">Entity type in CSConst.et~ and CSConst.dt~</param>
    public static string GetEntityName(int type)
    {
        string objName = "";

        int _type = type;
        switch (_type)
        {
            case CSConst.etAssembly:
                objName = PELocalization.GetString(ColonyNameID.ASSEMBLY);
                break;
            case CSConst.etStorage:
                objName = PELocalization.GetString(ColonyNameID.STORAGE);
                break;
            case CSConst.etEngineer:
                objName = PELocalization.GetString(ColonyNameID.ENGINEER);
                break;
            case CSConst.etEnhance:
                objName = PELocalization.GetString(ColonyNameID.ENHANCE);
                break;
            case CSConst.etRepair:
                objName = PELocalization.GetString(ColonyNameID.REPAIR);
                break;
            case CSConst.etRecyle:
                objName = PELocalization.GetString(ColonyNameID.RECYCLE);
                break;
            case CSConst.etDwelling:
                objName = PELocalization.GetString(ColonyNameID.DWELLING);
                break;
            case CSConst.etppCoal:
                objName = PELocalization.GetString(ColonyNameID.PPCOAL);
                break;
            case CSConst.etFarm:
                objName = PELocalization.GetString(ColonyNameID.FARM);
                break;
            case CSConst.etFactory:
                objName = PELocalization.GetString(ColonyNameID.FACTORY);
                break;
            case CSConst.etProcessing:
                objName = PELocalization.GetString(ColonyNameID.PROCESSING_FACILITY);
                break;
            case CSConst.dtTrade:
                objName = PELocalization.GetString(ColonyNameID.TRADE_POST);
                break;
            case CSConst.dtTrain:
                objName = PELocalization.GetString(ColonyNameID.TRAINING_CENTER);
                break;
            case CSConst.dtCheck:
                objName = PELocalization.GetString(ColonyNameID.MEDICAL_DETECTOR);
                break;
            case CSConst.dtTreat:
                objName = PELocalization.GetString(ColonyNameID.MEDICAL_LAB);
                break;
            case CSConst.dtTent:
                objName = PELocalization.GetString(ColonyNameID.QUARANTINE_TENT);
				break;
			case CSConst.dtppFusion:
				objName = PELocalization.GetString(ColonyNameID.PPFUSION);
				break;
            default:
                break;
        }

        return objName;
    }

    public static string GetEntityEnlishName(int type)
    {
        string objName = "";

        int _type = type;
        if ((type & CSConst.etPowerPlant) != 0)
            _type = CSConst.etPowerPlant;

        switch (_type)
        {
            case CSConst.etAssembly:
                objName = "Assembly Core";
                break;
            case CSConst.etStorage:
                objName = "Storage";
                break;
            case CSConst.etEngineer:
                objName = "Machinery";
                break;
            case CSConst.etEnhance:
                objName = "Enhance machine";
                break;
            case CSConst.etRepair:
                objName = "Repair machine";
                break;
            case CSConst.etRecyle:
                objName = "Recycle machine";
                break;
            case CSConst.etDwelling:
                objName = "Dwellings";
                break;
            case CSConst.etPowerPlant:
                objName = "Power Plant";
                break;
            case CSConst.etFarm:
                objName = "Farm";
                break;
            default:
                break;
        }
        return objName;
    }

	public static int GetEntityNameID(int type){
		int id = ColonyNameID.ASSEMBLY;
		switch (type)
		{
		case CSConst.etAssembly:
			id = ColonyNameID.ASSEMBLY;
			break;
		case CSConst.etStorage:
			id = ColonyNameID.STORAGE;
			break;
		case CSConst.etEngineer:
			id = ColonyNameID.ENGINEER;
			break;
		case CSConst.etEnhance:
			id = ColonyNameID.ENHANCE;
			break;
		case CSConst.etRepair:
			id =ColonyNameID.REPAIR;
			break;
		case CSConst.etRecyle:
			id = ColonyNameID.RECYCLE;
			break;
		case CSConst.etDwelling:
			id = ColonyNameID.DWELLING;
			break;
		case CSConst.etppCoal:
			id = ColonyNameID.PPCOAL;
			break;
		case CSConst.etFarm:
			id = ColonyNameID.FARM;
			break;
		case CSConst.etFactory:
			id = ColonyNameID.FACTORY;
			break;
		case CSConst.etProcessing:
			id = ColonyNameID.PROCESSING_FACILITY;
			break;
		case CSConst.etTrade:
			id = ColonyNameID.TRADE_POST;
			break;
		case CSConst.etTrain:
			id = ColonyNameID.TRAINING_CENTER;
			break;
		case CSConst.etCheck:
			id = ColonyNameID.MEDICAL_DETECTOR;
			break;
		case CSConst.etTreat:
			id = ColonyNameID.MEDICAL_LAB;
			break;
		case CSConst.etTent:
			id = ColonyNameID.QUARANTINE_TENT;
			break;
		case CSConst.etppFusion:
			id = ColonyNameID.PPFUSION;
			break;
		default:
			break;
		}
		
		return id;
	}
    /// <summary>
    /// Gets the real time string minute and second.
    /// </summary>
    /// <returns>The real time string M.</returns>
    /// <param name="time">Time in seconds</param>
    /// <param name="linkStr">Link the minutes between seconds.</param>
    public static string GetRealTimeMS(int time, string linkStr = " : ")
    {
        if (time <= 0)
        {
            return "00" + linkStr + "00";
        }

        string timeStr = "";

        int minutes = time / 60;
        int secound = time % 60;

        if (minutes < 10)
            timeStr += "0" + minutes.ToString();
        else
            timeStr += minutes.ToString();

        timeStr += linkStr;

        if (secound < 10)
            timeStr += "0" + secound.ToString();
        else
            timeStr += secound.ToString();

        return timeStr;
    }

    /// <summary>
    /// Splits the decimals from float.
    /// </summary>
    public static float SplitDecimals(float f)
    {
        return f > 0 ? f - Mathf.Floor(f) : f - Mathf.Ceil(f);
    }

    /// <summary>
    /// Resets the loacal transform to "Zero".
    /// </summary>
    /// <param name="trans">Trans.</param>
    public static void ResetLoacalTransform(Transform trans)
    {
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = Vector3.one;
    }

    /// <summary>
    /// Finds the position which Affixed the Voxel Terrian.
    /// </summary>
    /// <returns>The affixed position.</returns>
    /// <param name="source_pos">The source postion.</param>
    /// <param name="bounds">source position, only segment</param>
    /// <param name="downward_dis">search distance for Downward.</param>
    public static Vector3 FindAffixedPos(Vector3 source_pos, Vector3[] bounds, float downward_dis)
    {
        RaycastHit rch;
        Vector3 _pos = source_pos + new Vector3(0, 3, 0);
        if (Physics.Raycast(_pos, Vector3.down, out rch, 4, 1 << Pathea.Layer.VFVoxelTerrain))
        {
            source_pos = rch.point;
        }

        Vector3 rPos = source_pos;


        Vector3 blockPos = _FindAffixedPosForBlock(rPos, bounds, downward_dis);
        Vector3 voxelPos = _FindAffixedPosForVoxel(rPos, bounds, downward_dis);

        rPos = blockPos.y > voxelPos.y ? blockPos : voxelPos;

        return rPos;
    }


    private static Vector3 _FindAffixedPosForBlock(Vector3 source_pos, Vector3[] bounds, float downward_dis)
    {
        Vector3 rPos = source_pos;

        if (Block45Man.self != null)
        {
            int blockCnt = 0;
            foreach (Vector3 vec in bounds)
            {
                Vector3 pos = (source_pos + vec) * Block45Constants._scaleInverted;
                B45Block block = Block45Man.self.DataSource.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
                if (block.blockType != 0 || block.materialType != 0)
                    blockCnt++;
            }

            // search up 
            if (blockCnt > 0)
            {
                int lg = 2;
                Vector3 orign = source_pos + bounds[bounds.Length - 1];

                while (true)
                {
                    Vector3 pos = orign;
                    int _bCnt = 0;
                    for (int i = 0; i < lg; i++)
                    {
                        Vector3 blockPos = pos * Block45Constants._scaleInverted;
                        B45Block block = Block45Man.self.DataSource.SafeRead((int)blockPos.x, (int)blockPos.y, (int)blockPos.z);
                        if (block.blockType != 0 || block.materialType != 0)
                            _bCnt++;

                        pos += new Vector3(0, Block45Constants._scale, 0);
                    }

                    if (_bCnt == 0)
                    {
                        rPos = orign;
                        break;
                    }

                    orign += new Vector3(0, lg * Block45Constants._scale, 0);
                }
            }
            // search down
            else
            {
                Vector3 orign = source_pos + bounds[0];

                int count = Mathf.CeilToInt(downward_dis) * Block45Constants._scaleInverted + 1;
                for (int i = count; i > 0; i--)
                {
                    Vector3 blockPos = orign * Block45Constants._scaleInverted;
                    B45Block block = Block45Man.self.DataSource.SafeRead((int)blockPos.x, (int)blockPos.y, (int)blockPos.z);
                    if (block.blockType != 0 || block.materialType != 0)
                    {
                        orign += new Vector3(0, Block45Constants._scale, 0);
                        break;
                    }
                    else
                        orign -= new Vector3(0, Block45Constants._scale, 0);
                }

                rPos = orign;
            }
        }

        return rPos;
    }

    private static Vector3 _FindAffixedPosForVoxel(Vector3 source_pos, Vector3[] bounds, float downward_dis)
    {
        Vector3 rPos = source_pos;

        if (VFVoxelTerrain.self != null)
        {
            int voxelCnt = 0;
            int index = 0;
            for (int i = 0; i < bounds.Length; i++)
            {
                Vector3 pos = source_pos + bounds[i];
                VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
                if (voxel.Volume >= 128)
                {
                    index = Mathf.Max(i, index);
                    voxelCnt++;
                }
            }

            // search up 
            if (voxelCnt > 0)
            {
                Vector3 orgin = source_pos + bounds[index] + Vector3.up;

                while (true)
                {
                    int cnt = 0;
                    int idx = 0;
                    Vector3 pos;
                    for (int i = 0; i < bounds.Length; i++)
                    {
                        pos = orgin + bounds[i];
                        VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
                        if (voxel.Volume >= 128)
                        {
                            idx = Mathf.Max(i, idx);
                            cnt++;
                        }
                    }

                    if (cnt > 0)
                        orgin = orgin + bounds[idx] + Vector3.up;
                    else
                    {
                        //orgin = orgin;
                        break;
                    }
                }

                rPos = orgin;
            }
            // Search down
            else
            {
                Vector3 orign = source_pos + bounds[0] + Vector3.down;

                int count = Mathf.CeilToInt(downward_dis) + 1;
                for (int i = count; i > 0; i--)
                {
                    Vector3 pos = orign;
                    VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
                    if (voxel.Volume > 128)
                        break;
                    else
                        orign -= Vector3.up;
                }

                rPos = orign + Vector3.up;
            }

        }

        return rPos;
    }


    /// <summary>
    /// Spheres the contains and intersect bound.
    /// </summary>
    /// <param name="sphere_center">Sphere_center.</param>
    /// <param name="sphere_radius">Sphere_radius.</param>
    /// <param name="bound">Bound.</param>
    public static bool SphereContainsAndIntersectBound(Vector3 sphere_center, float sphere_radius, Bounds bound)
    {
        Vector3[] vertex = new Vector3[8];
        vertex[0] = bound.min;
        vertex[1] = new Vector3(bound.min.x, bound.min.y, bound.max.z);
        vertex[2] = new Vector3(bound.max.x, bound.min.y, bound.max.z);
        vertex[3] = new Vector3(bound.max.x, bound.min.y, bound.min.z);
        vertex[4] = new Vector3(bound.min.x, bound.max.y, bound.min.z);
        vertex[5] = new Vector3(bound.min.x, bound.max.y, bound.max.z);
        vertex[6] = bound.max;
        vertex[7] = new Vector3(bound.max.x, bound.max.y, bound.min.z);

        float sqr_radius = sphere_radius * sphere_radius;
        bool contain = false;
        for (int i = 0; i < 8; i++)
        {
            float sqr_dis = Vector3.SqrMagnitude(sphere_center - vertex[i]);
            if (sqr_dis < sqr_radius)
            {
                contain = true;
                break;
            }
        }

        return contain;
    }

    /// <summary>
    /// Gets the no format string.
    /// </summary>
    /// <returns>The no format string.</returns>
    /// <param name="source">Source string contain format</param>
    /// <param name="replace_str1">Replace_str1.</param>
    /// <param name="format">Format.</param>
    public static string GetNoFormatString(string source, string replace_str1)
    {
        return source.Replace("$A$", replace_str1);
    }

    public static string GetNoFormatString(string source, string replace_str1, string replace_str2)
    {
        string rStr = "";
        rStr = source.Replace("$A$", replace_str1);
        rStr = rStr.Replace("$B$", replace_str2);
        return rStr;
    }

    public static string GetNoFormatString(string source, object[] replace_objs, string link_str = ", ")
    {
        string replace_str = "";
        for (int i = 0; i < replace_objs.Length; i++)
        {
            replace_str += replace_objs[i].ToString() + link_str;
        }

        return source.Replace("$A$", replace_str);
    }

    /// <summary>
    /// string "str" match the src
    /// </summary>ram>
    public static bool MatchString(string src, string match)
    {
        if (match.Trim().Length == 0)
            return true;


        Regex r = new Regex(match); // 定义一个Regex对象实例
        Match m = r.Match(src); // 在字符串中匹配

        if (m.Success)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Determines if the "str" is number.
    /// </summary>
    public static bool IsNumber(string str)
    {
        Regex objNotNumberPattern = new Regex("[^0-9.-]");
        Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
        Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
        String strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
        String strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
        Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");
        return !objNotNumberPattern.IsMatch(str) &&
            !objTwoDotPattern.IsMatch(str) &&
                !objTwoMinusPattern.IsMatch(str) &&
                objNumberPattern.IsMatch(str);
    }

    //public static PersonnelSpace GetWorkRoom(CSEntity mEntity)
    //{
    //    CSCommon WorkRoom = mEntity as CSCommon;
    //    if (WorkRoom == null || WorkRoom.m_Type == CSConst.etFarm)
    //        return null;
    //    Vector3 pos = WorkRoom.Position;

    //    // Find valid space
    //    PersonnelSpace space = null;
    //    if (WorkRoom.WorkSpaces.Length != 0)
    //    {
    //        for (int i = 0; i < WorkRoom.WorkSpaces.Length; i++)
    //        {
    //            if (WorkRoom.WorkSpaces[i] == null || WorkRoom.WorkSpaces[i].m_Person != null || !WorkRoom.WorkSpaces[i].m_CanUse)
    //                continue;
    //            else
    //            {
    //                space = WorkRoom.WorkSpaces[i];
    //                break;
    //            }
    //        }
    //    }
    //    return space;
    //}

    public static Vector3 GetColonyPos(CSCreator creator)
    {
        CSAssembly m_Assembly = creator.Assembly;
        Vector2 randomFactor = UnityEngine.Random.insideUnitCircle;
        Vector3 pos = new Vector3(m_Assembly.Position.x + 5, 1000, m_Assembly.Position.z)
            + new Vector3(randomFactor.x, 0, randomFactor.y) * (m_Assembly.Radius - 10);

        // Find the Postion Y
        RaycastHit hit;
        if (Physics.Raycast(pos, Vector3.down, out hit, 1000, 1 << Pathea.Layer.VFVoxelTerrain))
        {
            pos.y = hit.point.y + 1;
        }
        else
            pos.y = m_Assembly.Position.y + 5;

        return pos;
    }

    #region storage
    public static CSStorage GetTargetStorage(List<int> protoTypeIdList, PeEntity npc = null)
    {
        if (npc == null)
            return null;
        CSMgCreator creator;
        if (PeGameMgr.IsSingle || PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
        {

            creator = (CSMgCreator)CSMain.GetCreator(CSConst.ciDefMgCamp);
        }
        else
        {
            int teamId = NetworkInterface.Get(npc.Id).TeamId;
            creator = MultiColonyManager.GetCreator(teamId, false);
        }
        if (creator == null || creator.Assembly == null)
        {
            return null;
        }
        List<CSCommon> CSStorageList = creator.Assembly.Storages;
        if (CSStorageList == null)
            return null;
        CSStorage resultStorage = null;
        foreach (CSCommon entity in CSStorageList)
        {
            CSStorage storage = entity as CSStorage;
            if (storage == null || !storage.IsRunning)
            {
                continue;
            }
            foreach (int protoTypeId in protoTypeIdList)
            {
                if (null != storage.m_Package.FindItemByProtoId(protoTypeId))
                {
                    resultStorage = storage;
                    break;
                }
            }
            if (resultStorage != null)
            {
                break;
            }
        }
        return resultStorage;
    }
    public static ItemAsset.ItemObject GetItemNumInStorage(CSStorage storage, int protoTypeId)
    {
        return storage.m_Package.FindItemByProtoId(protoTypeId);

    }

    public static bool DeleteItemInStorage(CSStorage storage, int protoTypeId, int num = 1)
    {
        return ItemAsset.PackageHelper.PackageAccessor.Destroy(storage.m_Package, protoTypeId, num);
    }


	public static List<ItemAsset.ItemObject> GetItemListInStorage(int protoId,CSAssembly assembly){
		List<ItemObject> allItem = new List<ItemObject> ();
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return allItem;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			allItem.AddRange(css.m_Package.GetAllItemByProtoId(protoId));
		}
		return allItem;
	}

	public static int GetItemCounFromFactoryAndAllStorage(int protoId,CSAssembly assembly){
		if(assembly==null)
			return 0;

		if(assembly.Factory!=null){
			return assembly.Factory.GetCompoundEndItemCount(protoId)+
				GetItemCountFromAllStorage(protoId,assembly);
		}else
			return 
				GetItemCountFromAllStorage(protoId,assembly);
	}

	public static int GetItemCountFromAllStorage(int protoId,CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return 0;
		int count =0;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			count += css.GetItemCount(protoId);
		}
		return count;
	}

	public static int GetMaterialListCount(List<ItemIdCount> materialList,CSAssembly assembly){
		ItemIdCount mo = materialList[0];

		int minCount  = GetItemCounFromFactoryAndAllStorage(mo.protoId,assembly)/mo.count;
		for(int i=1;i<materialList.Count;i++){
			int tempCount = GetItemCounFromFactoryAndAllStorage(materialList[i].protoId,assembly)/materialList[i].count;
			if(tempCount <minCount)
				minCount= tempCount;
		}
		return minCount;
	}


	public static bool CountDownItemFromAllStorage(int protoId,int count,CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return false;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			int sItemCount = css.GetItemCount(protoId);
			if(sItemCount>=count){
				css.CountDownItem(protoId,count);
				return true;
			}else{
				css.CountDownItem (protoId,sItemCount);
				count-=sItemCount;
			}
		}
		return false;
	}

	public static bool CountDownItemFromFactoryAndAllStorage(int protoId,int count,CSAssembly assembly){
		if(assembly==null)
			return false;
		if(assembly.Factory==null&&assembly.Storages==null)
			return false;
		if(assembly.Factory!=null){
			int fCount = assembly.Factory.GetCompoundEndItemCount(protoId);
			if(fCount>0)
			{
				if(fCount>=count){
					assembly.Factory.CountDownItem(protoId,count);
					return true;
				}else{
					assembly.Factory.CountDownItem(protoId,fCount);
					count-=fCount;
				}
			}
		}

		if(assembly.Storages!=null){
			foreach(CSCommon csc in assembly.Storages){
				CSStorage css = csc as CSStorage;
				int sItemCount = css.GetItemCount(protoId);
				if(sItemCount>=count){
					css.CountDownItem(protoId,count);
					return true;
				}else{
					css.CountDownItem (protoId,sItemCount);
					count-=sItemCount;
				}
			}
		}
		return false;
	}

	public static bool CanAddToStorage(int protoId,int count, CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return false;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			if(css.CanAdd(protoId,count))
				return true;
			else
				continue;
		}
		return false;
	}
	public static bool CanAddListToStorage(List<ItemIdCount> iicList, CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return false;

		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			if(css.CanAdd(iicList))
				return true;
			else
				continue;
		}
		return false;
	}

	public static bool AddToStorage(int protoId,int count,CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return false;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			if(css.CanAdd(protoId,count)){
				css.Add(protoId,count);
				return true;
			}
			else
				continue;
		}
		return false;
	}

	public static bool AddItemObjToStorage(int instanceId,CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return false;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			if(css.AddItemObj(instanceId))
				return true;
			else
				continue;
		}
		return false;
	}

	public static bool RemoveItemObjFromStorage(int instanceId,CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return false;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			if(css.RemoveItemObj(instanceId))
				return true;
			else
				continue;
		}
		return false;
	}
	public static bool AddItemListToStorage(List<ItemIdCount> iicList, CSAssembly assembly){
		if(assembly==null||assembly.Storages==null||assembly.Storages.Count==0)
			return false;
		foreach(CSCommon csc in assembly.Storages){
			CSStorage css = csc as CSStorage;
			if(css.CanAdd(iicList)){
				css.Add(iicList);
				return true;
			}
			else
				continue;
		}
		return false;
	}
    #endregion

    #region farm
    public static bool CheckFarmAvailable(PeEntity npc = null)
    {
        if (npc == null)
            return false;
        if (PeGameMgr.IsSingle)
        {
			if(CSMain.s_MgCreator==null)
				return false;

            if (CSMain.s_MgCreator.Assembly == null)
            {
                return false;
            }
            if (CSMain.s_MgCreator.Assembly.Farm == null)
            {
                return false;
            }
			if (!CSMain.s_MgCreator.Assembly.Farm.IsRunning)
			{
				return false;
			}
        }
        else
        {
			NetworkInterface net = NetworkInterface.Get(npc.Id);
			if(net == null)
				return false;

			int teamId = net.TeamId;
            CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
            if (creator == null)
            {
                return false;
            }
            if (creator.Assembly == null)
            {
                return false;
            }
            if (creator.Assembly.Farm == null)
            {
                return false;
            }
			if (!creator.Assembly.Farm.IsRunning)
			{
				return false;
			}
        }

        return true;
    }
	public static bool CheckPlantExist(FarmWorkInfo fwi){
		if(fwi==null)
			return false;
		if(fwi.m_Plant==null||fwi.m_Plant.gameObject==null)
			return false;
		return true;
	}
    public static bool FarmPlantReady(PeEntity npc = null)
    {
        if (npc == null)
            return false;
        if (!CheckFarmAvailable(npc))
        {
            return false;
        }
        CSMgCreator creator;
        if (PeGameMgr.IsSingle)
        {
            creator = CSMain.s_MgCreator;
        }
        else
        {
            int teamId = NetworkInterface.Get(npc.Id).TeamId;
            creator = MultiColonyManager.GetCreator(teamId, false);
        }
        return creator.Assembly.Farm.HasPlantSeed();
    }
    public static bool FarmWaterReady(PeEntity npc = null)
    {
        if (npc == null)
            return false;
        if (!CheckFarmAvailable(npc))
        {
            return false;
        }
        CSMgCreator creator;
        if (PeGameMgr.IsSingle)
        {
            creator = CSMain.s_MgCreator;
        }
        else
        {
            int teamId = NetworkInterface.Get(npc.Id).TeamId;
            creator = MultiColonyManager.GetCreator(teamId, false);
        }
        return creator.Assembly.Farm.GetPlantTool(CSFarm.TOOL_INDEX_WATER) != null;
    }
    public static bool FarmCleanReady(PeEntity npc = null)
    {
        if (npc == null)
            return false;
        if (!CheckFarmAvailable(npc))
        {
            return false;
        }
        CSMgCreator creator;
        if (PeGameMgr.IsSingle)
        {
            creator = CSMain.s_MgCreator;
        }
        else
        {
            int teamId = NetworkInterface.Get(npc.Id).TeamId;
            creator = MultiColonyManager.GetCreator(teamId, false);
        }
        return creator.Assembly.Farm.GetPlantTool(CSFarm.TOOL_INDEX_INSECTICIDE) != null;
    }

	public static bool CheckFarmPlantAround(Vector3 pos,PeEntity npc = null)
	{
		if (npc == null)
			return false;

		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator creator;
		if (PeGameMgr.IsSingle)
		{
			creator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			creator = MultiColonyManager.GetCreator(teamId, false);
		}

		if(creator.Assembly == null || creator.Assembly.Farm == null)
			return false;

		int itemid = creator.Assembly.Farm.GetPlantSeedId();
		if(itemid <0)
			return false;

//		PlantInfo mPlantInfo = PlantInfo.GetPlantInfoByItemId(itemid);
//		if(mPlantInfo == null)
//			return false;

		return  creator.Assembly.Farm.checkRroundCanPlant(itemid,pos);
	}
    
	public static bool FarmWaterEnough(PeEntity npc = null,FarmPlantLogic plant=null)
	{
		if (npc == null)
			return false;
		if(plant ==null)
			return false;
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator creator;
		if (PeGameMgr.IsSingle)
		{
			creator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			creator = MultiColonyManager.GetCreator(teamId, false);
		}

		ItemAsset.ItemObject waterItem = creator.Assembly.Farm.GetPlantTool(CSFarm.TOOL_INDEX_WATER);
		if(waterItem == null)
			return false;

		int needNum = plant.GetWaterItemCount();
		if (needNum <= 0)
			return false;
		if (waterItem.GetCount() < needNum)
			return false;
		return true;
	}
	public static bool FarmCleanEnough(PeEntity npc = null,FarmPlantLogic plant=null)
	{
		if (npc == null)
			return false;
		if(plant ==null)
			return false;
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator creator;
		if (PeGameMgr.IsSingle)
		{
			creator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			creator = MultiColonyManager.GetCreator(teamId, false);
		}
		
		ItemAsset.ItemObject cleanItem = creator.Assembly.Farm.GetPlantTool(CSFarm.TOOL_INDEX_INSECTICIDE);
		if(cleanItem == null)
			return false;

		int needNum = plant.GetCleaningItemCount();
		if (needNum <= 0)
			return false;
		if (cleanItem.GetCount() < needNum)
			return false;
		return true;
	}

	public static FarmWorkInfo FindPlantPos(PeEntity npc = null)
    {
        if (npc == null)
            return null;
        if (!CheckFarmAvailable(npc))
        {
            return null;
        }
        CSMgCreator creator;
        if (PeGameMgr.IsSingle)
        {
            creator = CSMain.s_MgCreator;
        }
        else
        {
            int teamId = NetworkInterface.Get(npc.Id).TeamId;
            creator = MultiColonyManager.GetCreator(teamId, false);
        }

        if (creator.m_Clod == null)
        {
            return null;
        }
        ClodChunk cc = creator.m_Clod.FindCleanChunk(creator.Assembly.Position, creator.Assembly.Radius);
        if (cc == null)
        {
            return null;
        }
        Vector3 pos;
        bool posGot = cc.FindCleanClod(out pos);
        if (posGot)
        {
            FarmWorkInfo farmWorkInfo = new FarmWorkInfo(cc, pos);
            creator.m_Clod.DirtyTheChunk(cc.m_ChunkIndex, true);
            return farmWorkInfo;
        }

        return null;
    }
	
	public static FarmWorkInfo FindPlantPos(FarmWorkInfo farmWorkInfo, PeEntity npc = null)
    {
		if (npc == null ){
			return null;
		}
        if (!CheckFarmAvailable(npc))
		{ 
			return null;
        }
        //CSMgCreator creator = CSMain.s_MgCreator;
        //ClodChunk cc = creator.m_Clod.FindCleanChunk(creator.Assembly.Position, creator.Assembly.Radius);
        Vector3 pos;
        bool posGot = farmWorkInfo.m_ClodChunk.FindCleanClod(out pos);
        if (posGot)
        {
            FarmWorkInfo farmWorkInfo2 = new FarmWorkInfo(farmWorkInfo.m_ClodChunk, pos);
            return farmWorkInfo2;
        }
        return null;
    }

	public static FarmWorkInfo FindPlantPosNewChunk(PeEntity npc = null)
	{
		if (npc == null)
			return null;

		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSMgCreator creator;
		if (PeGameMgr.IsSingle)
		{
			creator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			creator = MultiColonyManager.GetCreator(teamId, false);
		}
		
		if (creator.m_Clod == null)
		{
			return null;
		}
		
		if(creator.Assembly.Farm == null)
			return null;

		int itemid = creator.Assembly.Farm.GetPlantSeedId();
		if(itemid <0)
			return null;
		
//		PlantInfo mPlantInfo = PlantInfo.GetPlantInfoByItemId(itemid);
//		if(mPlantInfo == null)
//			return null;
		
		Vector3 pos;
		ClodChunk cc = creator.m_Clod.FindHasIdleClodsChunk(creator.Assembly.Position, creator.Assembly.Radius,creator.Assembly.Farm,itemid,out pos);
		if (cc != null)
		{
			FarmWorkInfo farmWorkInfo = new FarmWorkInfo(cc, pos);
			creator.m_Clod.DirtyTheChunk(cc.m_ChunkIndex, true);
			return farmWorkInfo;
		}
		
		return null;
	}
	
	public static FarmWorkInfo FindPlantPosSameChunk(FarmWorkInfo farmWorkInfo,PeEntity npc = null)
	{
		if (npc == null || farmWorkInfo == null)
			return null;
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSMgCreator creator;
		if (PeGameMgr.IsSingle)
		{
			creator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			creator = MultiColonyManager.GetCreator(teamId, false);
		}
		
		if(creator.Assembly == null || creator.Assembly.Farm == null)
			return null ;
		
		int itemid = creator.Assembly.Farm.GetPlantSeedId();
		if(itemid <0)
			return null;
		
//		PlantInfo mPlantInfo = PlantInfo.GetPlantInfoByItemId(itemid);
//		if(mPlantInfo == null)
//			return null;
		
		Vector3 pos;
		bool posGot = farmWorkInfo.m_ClodChunk.FindBetterClod(creator.Assembly.Position,creator.Assembly.Radius,creator.Assembly.Farm,itemid,out pos);
		if (posGot)
		{
			FarmWorkInfo farmWorkInfo2 = new FarmWorkInfo(farmWorkInfo.m_ClodChunk, pos);
			return farmWorkInfo2;
		}
		return null;
	}

    public static bool TryPlant(FarmWorkInfo farmworkInfo, PeEntity npc = null)
    {
        if (npc == null){
            return false;
		}
        if (FarmPlantReady(npc))
        {
            CSMgCreator creator;
            if (PeGameMgr.IsSingle)
            {
                creator = CSMain.s_MgCreator;

                FarmPlantLogic fpl = creator.Assembly.Farm.PlantTo(farmworkInfo.m_Pos);
                if (fpl != null)
                {
                    //farmworkInfo.m_ClodChunk.DirtyTheClod(farmworkInfo.m_Pos, true);
					farmworkInfo.m_ClodChunk.DirtyTheClodByPlantBounds(fpl.protoTypeId,farmworkInfo.m_Pos,true);
                }
            }
            else
            {
                AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
                if (npcNet == null)
				{
					return false;
                }
                int teamId = npcNet.TeamId;
                creator = MultiColonyManager.GetCreator(teamId, false);
                int farmId = creator.Assembly.Farm.ID;
                IntVector3 safePos = new IntVector3(farmworkInfo.m_Pos + 0.1f * Vector3.down);
                byte terrainType = VFVoxelTerrain.self.Voxels.SafeRead(safePos.x, safePos.y, safePos.z).Type;
                npcNet.PlantPutOut(farmworkInfo.m_Pos, farmId, terrainType);
                farmworkInfo.m_ClodChunk.DirtyTheClod(farmworkInfo.m_Pos, true);
				//farmworkInfo.m_ClodChunk.DirtyTheClodByPlantBounds(fpl.protoTypeId,farmworkInfo.m_Pos,true);
            }
            return true;
		}
		return false;
    }

	public static void ReturnCleanChunk(FarmWorkInfo farmWorkInfo, PeEntity npc = null)
	{
		if (npc == null)
			return;
		CSMgCreator creator;
		if (PeGameMgr.IsSingle)
		{
			creator = CSMain.s_MgCreator;
		}
		else
		{
			if(NetworkInterface.Get(npc.Id) == null)
				return ;

			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			creator = MultiColonyManager.GetCreator(teamId, false);
		}
		
		if (creator==null||creator.m_Clod == null)
		{
			return;
		}
		creator.m_Clod.AddClod(farmWorkInfo.m_Pos);
	}
	
	public static FarmWorkInfo FindPlantToWater(PeEntity npc = null)
	{
		if (npc == null)
            return null;
        if (!CheckFarmAvailable(npc))
        {
            return null;
        }

        CSFarm farm;
        if (PeGameMgr.IsSingle)
        {
            farm = CSMain.s_MgCreator.Assembly.Farm;
        }
        else
        {
            AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
            if (npcNet == null)
            {
                return null;
            }
            int teamId = npcNet.TeamId;
            CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
            farm = creator.Assembly.Farm;
        }
        ItemAsset.ItemObject waterItem = farm.GetPlantTool(CSFarm.TOOL_INDEX_WATER);
        FarmPlantLogic plant = waterItem == null ? null : farm.AssignOutWateringPlant();
        if (plant != null)
        {
			int needNum = plant.GetWaterItemCount();
			if (needNum <= 0)
				return null;
			if (waterItem.GetCount() < needNum)
			{
				farm.RestoreWateringPlant(plant);
				return null;
			}
            FarmWorkInfo fwi = new FarmWorkInfo(plant);
            return fwi;
        }

        return null;
    }

    public static bool TryWater(FarmWorkInfo fwi, PeEntity npc = null)
    {
        if (npc == null)
            return false;
		if(!CheckPlantExist(fwi)){
			return false;
		}
        if (!CheckFarmAvailable(npc))
        {
            return false;
        }

        if (!FarmWaterReady(npc))
        {
            return false;
        }
        if (PeGameMgr.IsSingle)
        {
            CSFarm farm = CSMain.s_MgCreator.Assembly.Farm;
            fwi.m_Plant.UpdateStatus();
            int needNum = fwi.m_Plant.GetWaterItemCount();
            if (needNum <= 0)
                return false;
            ItemAsset.ItemObject water = farm.GetPlantTool(CSFarm.TOOL_INDEX_WATER);
            int haveNum = water.GetCount();
            if (haveNum >= needNum)
            {
                fwi.m_Plant.Watering(needNum);
                fwi.m_Plant.UpdateStatus();
                water.DecreaseStackCount(needNum);

                if (water.GetCount() <= 0)
                {
                    ItemAsset.ItemMgr.Instance.DestroyItem(water.instanceId);
                    farm.SetPlantTool(CSFarm.TOOL_INDEX_WATER, null);
                }
                return true;
            }else
				return false;
//            farm.RestoreWateringPlant(fwi.m_Plant);
        }
        else
        {
            AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
            if (npcNet == null)
            {
                return false;
            }
            int teamId = npcNet.TeamId;
            CSFarm farm = MultiColonyManager.GetCreator(teamId, false).Assembly.Farm;
            int farmId = farm.ID;
            int objId = fwi.m_Plant.id;

			npcNet.PlantWater(objId, farmId);
        }

        return true;
    }
	public static void ReturnWaterPlant(PeEntity npc,FarmWorkInfo fwi){
		if(npc==null)
			return;
		if(!CheckPlantExist(fwi)){
			return;
		}
		if (!CheckFarmAvailable(npc))
		{
			return;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (npcNet == null)
			{
				return;
			}
			int teamId = npcNet.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
			farm = creator.Assembly.Farm;
		}
		if(farm!=null)
			farm.RestoreWateringPlant(fwi.m_Plant);
	}
    public static FarmWorkInfo FindPlantToClean(PeEntity npc = null)
    {
        if (npc == null)
            return null;
        if (!CheckFarmAvailable(npc))
        {
            return null;
        }
        CSFarm farm;
        if (PeGameMgr.IsSingle)
        {
            farm = CSMain.s_MgCreator.Assembly.Farm;
        }
        else
        {
            AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
            if (npcNet == null)
            {
                return null;
            }
            int teamId = npcNet.TeamId;
            CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
            farm = creator.Assembly.Farm;
        }
        ItemAsset.ItemObject cleanItem = farm.GetPlantTool(CSFarm.TOOL_INDEX_INSECTICIDE);
        FarmPlantLogic plant = cleanItem == null ? null : farm.AssignOutCleaningPlant();
        if (plant != null)
		{
			int needNum = plant.GetCleaningItemCount();
			if (needNum <= 0)
				return null;
			if (cleanItem.GetCount() < needNum)
			{
				farm.RestoreCleaningPlant(plant);
				return null;
			}
            FarmWorkInfo fwi = new FarmWorkInfo(plant);
            return fwi;
        }
        return null;
    }

    public static bool TryClean(FarmWorkInfo fwi, PeEntity npc = null)
    {
        if (npc == null)
			return false;
		if(!CheckPlantExist(fwi)){
			return false;
		}
        if (!CheckFarmAvailable(npc))
        {
            return false;
        }

        if (!FarmCleanReady(npc))
        {
            return false;
        }

        if (PeGameMgr.IsSingle)
        {
            CSFarm farm = CSMain.s_MgCreator.Assembly.Farm;
            fwi.m_Plant.UpdateStatus();
            int needNum = fwi.m_Plant.GetCleaningItemCount();
            if (needNum <= 0)
                return false;
            ItemAsset.ItemObject insecticide = farm.GetPlantTool(CSFarm.TOOL_INDEX_INSECTICIDE);
            int haveNum = insecticide.GetCount();
            if (haveNum >= needNum)
            {
                fwi.m_Plant.Cleaning(needNum);
                fwi.m_Plant.UpdateStatus();
                insecticide.DecreaseStackCount(needNum);

                if (insecticide.GetCount() <= 0)
                {
                    ItemAsset.ItemMgr.Instance.DestroyItem(insecticide.instanceId);
                    farm.SetPlantTool(CSFarm.TOOL_INDEX_INSECTICIDE, null);
                }
                return true;
            }else
				return false;

//            farm.RestoreCleaningPlant(fwi.m_Plant);
        }
        else
        {
            AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
            if (npcNet == null)
            {
                return false;
            }
            int teamId = npcNet.TeamId;
            CSFarm farm = MultiColonyManager.GetCreator(teamId, false).Assembly.Farm;
            int farmId = farm.ID;
            int objId = fwi.m_Plant.id;

            npcNet.PlantClean(objId, farmId);
        }

        return true;
    }
	public static void ReturnCleanPlant(PeEntity npc,FarmWorkInfo fwi){
		if(npc==null)
			return;
		if(!CheckPlantExist(fwi)){
			return;
		}
		if (!CheckFarmAvailable(npc))
		{
			return;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (npcNet == null)
			{
				return;
			}
			int teamId = npcNet.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
			farm = creator.Assembly.Farm;
		}
		if(farm!=null)
			farm.RestoreCleaningPlant(fwi.m_Plant);
	}
    public static FarmWorkInfo FindPlantGet(PeEntity npc = null)
    {
        if (npc == null)
            return null;
        if (!CheckFarmAvailable(npc))
        {
            return null;
        }

        CSFarm farm;
        if (PeGameMgr.IsSingle)
        {
            farm = CSMain.s_MgCreator.Assembly.Farm;
        }
        else
        {
            AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
            if (npcNet == null)
            {
                return null;
            }
            int teamId = npcNet.TeamId;
            CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
            farm = creator.Assembly.Farm;
        }
        CSStorage storage = null;

        if (CSMain.s_MgCreator.Assembly.Storages == null)
            return null;

        foreach (CSStorage css in CSMain.s_MgCreator.Assembly.Storages)
        {
            storage = css;
            break;
        }
        if (storage != null)
        {
            FarmPlantLogic plant = farm.AssignOutRipePlant();
            if (plant != null)
            {
                int itemGetNum = plant.GetHarvestItemNumMax();
                Dictionary<int, int> retItems = plant.GetHarvestItemIdsMax(itemGetNum);
                foreach (CSStorage css in CSMain.s_MgCreator.Assembly.Storages)
                {
                    if(CanAddToPackage(css.m_Package, retItems))
					{
	                    FarmWorkInfo fwi = new FarmWorkInfo(plant);
	                    return fwi;
					}
                }
                farm.RestoreRipePlant(plant);
            }
        }
        return null;
    }

    public static bool TryHarvest(FarmWorkInfo fwi, PeEntity npc = null)
    {
        if (npc == null)
			return false;
		if(!CheckPlantExist(fwi)){
			return false;
		}
        if (!CheckFarmAvailable(npc))
        {
            return false;
        }
        FarmPlantLogic mPlant = fwi.m_Plant;

        CSFarm farm;

        if (PeGameMgr.IsSingle)
        {
            farm = CSMain.s_MgCreator.Assembly.Farm;
            float harvestAbility = 1 + npc.GetCmpt<NpcCmpt>().Npcskillcmpt.GetTalentPercent(AblityType.Harvest);
            int itemGetNum = mPlant.GetHarvestItemNum(harvestAbility);
            Dictionary<int, int> retItems = mPlant.GetHarvestItemIds(itemGetNum);

            CSStorage storage = FindStorageForHarvest(retItems, npc);
            if (storage == null)
            {
//                farm.RestoreRipePlant(mPlant);
                return false;
            }
            AddToPackage(storage.m_Package, retItems);
            FarmManager.Instance.RemovePlant(mPlant.mPlantInstanceId);
            DragArticleAgent.Destory(mPlant.id);
            CSMain.s_MgCreator.m_Clod.AddClod(fwi.m_Pos, false);
        }
        else
        {
            AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
            if (npcNet == null)
            {
                return false;
            }
            int teamId = npcNet.TeamId;
            CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
			farm = creator.Assembly.Farm; 
			float harvestAbility = 1 + npc.GetCmpt<NpcCmpt>().Npcskillcmpt.GetTalentPercent(AblityType.Harvest);
			int itemGetNum = mPlant.GetHarvestItemNum(harvestAbility);
            Dictionary<int, int> retItems = mPlant.GetHarvestItemIds(itemGetNum);
            CSStorage storage = FindStorageForHarvest(retItems, npc);
            if (storage == null)
            {
//                farm.RestoreRipePlant(mPlant);
                return false;
            }
            npcNet.PlantGetBack(mPlant.id, farm.ID);
        }

        return true;
    }
	public static void ReturnHarvestPlant(PeEntity npc,FarmWorkInfo fwi){
		if(npc==null)
			return;
		if(!CheckPlantExist(fwi)){
			return;
		}
		if (!CheckFarmAvailable(npc))
		{
			return;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (npcNet == null)
			{
				return;
			}
			int teamId = npcNet.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
			farm = creator.Assembly.Farm;
		}
		if(farm!=null)
			farm.RestoreRipePlant(fwi.m_Plant);
	}
    public static CSStorage FindStorageForHarvest(Dictionary<int, int> retItems, PeEntity npc)
    {
        if (npc == null)
            return null;
        if (!CheckFarmAvailable(npc))
        {
            return null;
        }
        CSFarm farm;
        if (PeGameMgr.IsSingle)
        {
            farm = CSMain.s_MgCreator.Assembly.Farm;
        }
        else
        {
            AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
            if (npcNet == null)
            {
                return null;
            }
            int teamId = npcNet.TeamId;
            CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
            farm = creator.Assembly.Farm;
        }
        foreach (CSStorage css in farm.Assembly.Storages)
        {
            if (CanAddToPackage(css.m_Package, retItems))
            {
                return css;
            }
        }
        return null;
    }

	//remove
	public static FarmWorkInfo FindPlantRemove(PeEntity npc = null)
	{
		if (npc == null)
			return null;
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (npcNet == null)
			{
				return null;
			}
			int teamId = npcNet.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, false);
			farm = creator.Assembly.Farm;
		}

		FarmPlantLogic plant = farm.AssignOutDeadPlant();
		if (plant != null)
		{
			FarmWorkInfo fwi = new FarmWorkInfo(plant);
			return fwi;
		}
		return null;
	}
	public static bool TryRemove(FarmWorkInfo fwi, PeEntity npc = null){
		if(!CheckPlantExist(fwi)){
			return false;
		}
		FarmPlantLogic mPlant = fwi.m_Plant;

		if (PeGameMgr.IsSingle)
		{
			FarmManager.Instance.RemovePlant(mPlant.mPlantInstanceId);
			DragArticleAgent.Destory(mPlant.id);
			CSMain.s_MgCreator.m_Clod.AddClod(fwi.m_Pos, false);
		}
		else
		{
			if(npc==null)
				return false;
			AiAdNpcNetwork npcNet = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (npcNet == null)
			{
				return false;
			}
			//int teamId = npcNet.TeamId;
			npcNet.PlantClear(mPlant.id);
		}
		
		return true;
	}
    #endregion

    #region tool
    public static bool CanAddToPackage(ItemPackage package, Dictionary<int, int> retItems)
    {
        List<MaterialItem> mItems = new List<MaterialItem>();
        foreach (int protoTypeId in retItems.Keys)
        {
            MaterialItem mi = new MaterialItem();
            mi.protoId = protoTypeId;
            mi.count = retItems[protoTypeId];
            mItems.Add(mi);
        }
        return package.CanAdd(mItems);
    }

    public static void AddToPackage(ItemPackage package, Dictionary<int, int> retItems)
    {
        List<MaterialItem> mItems = new List<MaterialItem>();
        foreach (int protoTypeId in retItems.Keys)
        {
            MaterialItem mi = new MaterialItem();
            mi.protoId = protoTypeId;
            mi.count = retItems[protoTypeId];
            mItems.Add(mi);
        }
        package.Add(mItems);
    }

	public static List<MaterialItem> ItemIdCountToMaterialItem(List<ItemIdCount> itemList){
		List<MaterialItem> mItems = new List<MaterialItem>();
		foreach (ItemIdCount idCount in itemList)
		{
			MaterialItem mi = new MaterialItem();
			mi.protoId = idCount.protoId;
			mi.count = idCount.count;
			mItems.Add(mi);
		}
		return mItems;
	}


	public static void AddItemIdCount(List<ItemIdCount> list,int protoId,int count){
		ItemIdCount findItem = list.Find(it=>it.protoId==protoId);
		if(findItem==null)
			list.Add (new ItemIdCount(protoId,count));
		else
			findItem.count+=count;
	}
	public static void RemoveItemIdCount(List<ItemIdCount> list, int protoId,int count){
		ItemIdCount removeItem = list.Find(it=>it.protoId==protoId);
		if(removeItem!=null)
		{
			if(removeItem.count<=count)
				list.Remove(removeItem);
			else
				removeItem.count-=count;
		}
	}

	public static int[] ItemIdCountListToIntArray(List<ItemIdCount> list){
		if(list==null)
			return null;
		int[] items = new int[list.Count * 2];
		int index = 0;
		foreach (ItemIdCount item in list)
		{
			items[index++] = item.protoId;
			items[index++] = item.count;
		}
		return items;
	}
    #endregion

	#region information
	public static void ShowTips(int tipsCode,int replaceStrId=-1){
		string str="?";
		string replaceStr = PELocalization.GetString(replaceStrId);
		str= CSUtils.GetNoFormatString(PELocalization.GetString(tipsCode),replaceStr);
		new PeTipMsg(str,PeTipMsg.EMsgLevel.Norm,PeTipMsg.EMsgType.Colony);
		string[] infoInColony=Regex.Split(str,"\\[-\\] ",RegexOptions.IgnoreCase);
		if(infoInColony.Length<2)
		{
			CSUI_MainWndCtrl.ShowStatusBar(infoInColony[0],10);
			return;
		}
		CSUI_MainWndCtrl.ShowStatusBar(infoInColony[1],10);
	}
	public static void ShowTips(int tipsCode,string replaceStr){
		string str="?";
		str= CSUtils.GetNoFormatString(PELocalization.GetString(tipsCode),replaceStr);
		new PeTipMsg(str,PeTipMsg.EMsgLevel.Norm,PeTipMsg.EMsgType.Colony);
		string[] infoInColony=Regex.Split(str,"\\[-\\] ",RegexOptions.IgnoreCase);
		if(infoInColony.Length<2)
		{
			CSUI_MainWndCtrl.ShowStatusBar(infoInColony[0],10);
			return;
		}
		CSUI_MainWndCtrl.ShowStatusBar(infoInColony[1],10);
	}

	public static void ShowTips(string str){
		CSUI_MainWndCtrl.ShowStatusBar(str);
	}

	public static void ShowCannotWorkReason(ENpcUnableWorkType state,string fullname){
		switch(state){
		case ENpcUnableWorkType.HasRequest:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_HasAnyQuest),fullname));
			break;
		case ENpcUnableWorkType.IsNeedMedicine:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_IsNeedMedicine),fullname));
			break;
		case ENpcUnableWorkType.IsHunger:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_IsHunger),fullname));
			break;
		case ENpcUnableWorkType.IsHpLow:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_HpLow),fullname));
			break;
		case ENpcUnableWorkType.IsUncomfortable:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_IsUncomfortable),fullname));
			break;
		case ENpcUnableWorkType.IsSleeepTime:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_IsInSleepTime),fullname));
			break;
		case ENpcUnableWorkType.IsDinnerTime:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_IsInDinnerTime),fullname));
			break;
		default:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_WORK_None),fullname));
			break;
		}
	}
	#endregion
}
