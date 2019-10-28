using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomItem;
using System.IO;
using Mono.Data.SqliteClient;
using UnityEngine;

public class RandomItemDataMgr
{
    const string testPath = "Prefab/Item/Scene/randomitem_test";

    public static int GetBoxAmount(int boxId)
    {
        RandomItemBoxInfo bibi = RandomItemBoxInfo.GetBoxInfoById(boxId);
        if (bibi != null)
            return bibi.boxAmount;
        return 0;
    }

    public static List<RandomItemBoxInfo> GetBoxIdByCondition(List<int> conditionList, int height)
    {
        List<RandomItemBoxInfo> ribi = RandomItemBoxInfo.RandomBoxMatchCondition(conditionList, height);
        return ribi;
    }

    public static List<ItemIdCount> GenItemDicByBoxId(int boxId, out string path,System.Random rand = null)
    {
        //test return 1,1
        List<ItemIdCount> items = new List<ItemIdCount>();
        items.Add(new ItemIdCount(1, 1));
        
        //--to do
        //1.getbox
        path = testPath;
        RandomItemBoxInfo ribi = RandomItemBoxInfo.GetBoxInfoById(boxId);
        if (ribi == null)
            return null;
        path = ribi.boxModelPath;

        //2.getrule
        RandomItemRulesInfo riri = RandomItemRulesInfo.GetRuleInfoById(ribi.rulesId);
        if (riri == null)
            return null;
          
        //3.random items
		if(rand==null)
			rand = new System.Random((int)System.DateTime.UtcNow.Ticks);
		int itemAmount = rand.Next(ribi.boxItemAmountMin, ribi.boxItemAmountMax + 1);
        items = riri.RandomItemDict(itemAmount,rand);

        return items;
    }


    public static void LoadData()
    {
        RandomItemBoxInfo.LoadData();
        RandomItemRulesInfo.LoadData();
        RandomItemTypeInfo.LoadData();
    }

}

namespace RandomItem{
    public class RandomItemBoxInfo
    {
        public int boxNo;
        public int boxId;
        public string boxName;
        public int boxAmount;
        public float boxDepth;
        public int boxRange;
        public List<int> boxMapType;
        public int boxItemAmountMin;
        public int boxItemAmountMax;
        public int rulesId;
        public string boxModelPath;
 

        public static Dictionary<int, RandomItemBoxInfo> mDataDic = new Dictionary<int, RandomItemBoxInfo>();
        
        public bool MatchCondition(List<int> conditions)
        {
            foreach(int i in boxMapType){
                if(!conditions.Contains(i)){
                    return false;
                }
            }
            return true;
        }

        public static RandomItemBoxInfo GetBoxInfoById(int id)
        {
            if (mDataDic.ContainsKey(id))
                return mDataDic[id];
            return null;
        }

        public static List<RandomItemBoxInfo> RandomBoxMatchCondition(List<int> conditions, int height)
        {
            List<RandomItemBoxInfo> boxIdList = new List<RandomItemBoxInfo>();
            foreach (RandomItemBoxInfo ribi in mDataDic.Values)
            {
                if (ribi.MatchCondition(conditions) && ribi.boxDepth <= height)
                {
                    boxIdList.Add(ribi);
                }
            }
            return boxIdList;
        }
        
        public static void LoadData()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("randombox");
            while (reader.Read())
            {
                RandomItemBoxInfo boxInfo = new RandomItemBoxInfo();

                boxInfo.boxNo = Convert.ToInt32(reader.GetString(reader.GetOrdinal("boxno")));
                boxInfo.boxId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("boxid")));
                boxInfo.boxName = reader.GetString(reader.GetOrdinal("boxname"));
                boxInfo.boxAmount = Convert.ToInt32(reader.GetString(reader.GetOrdinal("boxamount")));
                boxInfo.boxDepth = Convert.ToSingle(reader.GetString(reader.GetOrdinal("boxdepth")));
                boxInfo.boxRange = Convert.ToInt32(reader.GetString(reader.GetOrdinal("boxrange")));
                string[] boxMapTypeStr = reader.GetString(reader.GetOrdinal("boxmaptype")).Split(',');
                boxInfo.boxMapType= new List<int> ();
                foreach (string str in boxMapTypeStr)
                {
                    boxInfo.boxMapType.Add(Convert.ToInt32(str));
                }
                string[] itemAmountStr = reader.GetString(reader.GetOrdinal("boxitemamount")).Split(',');
                boxInfo.boxItemAmountMin = Convert.ToInt32(itemAmountStr[0]);
                boxInfo.boxItemAmountMax = boxInfo.boxItemAmountMin;
                if(itemAmountStr.Count()>1)
                    boxInfo.boxItemAmountMax = Convert.ToInt32(itemAmountStr[1]);
                boxInfo.rulesId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("rulesid")));
                boxInfo.boxModelPath = reader.GetString(reader.GetOrdinal("boxmodelpath"));
                
                mDataDic.Add(boxInfo.boxId,boxInfo);
            }
        }
		public static void CachePrefab()
		{
			foreach (KeyValuePair<int, RandomItemBoxInfo> pair in mDataDic) {
				AssetsLoader.Instance.LoadPrefabImm (pair.Value.boxModelPath, true);
			}
		}
    }

    public class RandomItemRulesInfo
    {
        public int rulesNo;
        public int rulesId;
        public int equipmentLevel_1;
        public int equipmentWeightLevel_1;
        public int equipmentLevel_2;
        public int equipmentWeightLevel_2;
        public int equipmentLevel_3;
        public int equipmentWeightLevel_3;
        public int toolLevel_1;
        public int toolWeightLevel_1;
        public int toolLevel_2;
        public int toolWeightLevel_2;
        public int toolLevel_3;
        public int toolWeightLevel_3;
        public int scriptLevel_1;
        public int scriptWeightLevel_1;
        public int scriptLevel_2;
        public int scriptWeightLevel_2;
        public int scriptLevel_3;
        public int scriptWeightLevel_3;
        public int consumables;
        public int consumablesWeight;

        public int weightPool = 0;

        public static Dictionary<int, RandomItemRulesInfo> mDataDic = new Dictionary<int, RandomItemRulesInfo>();

        public List<ItemIdCount> RandomItemDict(int count,System.Random rand=null)
        {
            List<ItemIdCount> items = new List<ItemIdCount>();
            if(rand==null)
				rand = new System.Random((int)System.DateTime.UtcNow.Ticks);
			
//			if(count ==0)
//				Debug.LogError(string.Format("RandomItemDict count 0!"));
            for (int i = 0; i < count; i++)
            {
                int type;
				int level = RandomTypeLevel(out type,rand);
                List<RandomItemTypeInfo> riti = RandomItemTypeInfo.RandomItemTypeInfoByLevel(level,type);
                if (riti != null && riti.Count > 0)
                {
                    RandomItemTypeInfo ritiPicked = new RandomItemTypeInfo();
                    ritiPicked = riti[rand.Next(riti.Count)];
                    int itemProtoId = ritiPicked.prototypeItem_id;
                    double countDouble = ritiPicked.itemAmount * (1 + ritiPicked.itemFloating * rand.NextDouble());
                    int itemCount = Mathf.RoundToInt((float)countDouble);
                    if (itemCount > 0)
                    {
                        ItemIdCount mi = new ItemIdCount(itemProtoId, itemCount);
                        items.Add(mi);
//					}else{
//						
//						Debug.LogError(string.Format("RandomItemDict itemCount 0:{0},{1}",ritiPicked.prototypeItem_id,ritiPicked.itemAmount));
					}
                }
            }
            return items;
        }

        public int CountWeightPool()
        {
            return equipmentWeightLevel_1 + equipmentWeightLevel_2 + equipmentWeightLevel_3
                + toolWeightLevel_1 + toolWeightLevel_2 + toolWeightLevel_3
                + scriptWeightLevel_1 + scriptWeightLevel_2 + scriptWeightLevel_3
                + consumablesWeight;
        }

        private int RandomTypeLevel(out int type,System.Random rand=null)
        {
			if(rand==null)
				rand = new System.Random((int)System.DateTime.UtcNow.Ticks);
            int weightNum = rand.Next(weightPool);
            if (weightNum - equipmentWeightLevel_1 < 0)
            {
                type = RandomItemType.EQUIPMENT;
                return equipmentLevel_1;
            }
            weightNum -= equipmentWeightLevel_1;

            if (weightNum - equipmentWeightLevel_2 < 0)
            {
                type = RandomItemType.EQUIPMENT;
                return equipmentLevel_2;
            }
            weightNum -= equipmentWeightLevel_2;

            if (weightNum - equipmentWeightLevel_3 < 0)
            {
                type = RandomItemType.EQUIPMENT;
                return equipmentLevel_3;
            }
            weightNum -= equipmentWeightLevel_3;

            if (weightNum - toolWeightLevel_1 < 0)
            {
                type = RandomItemType.TOOL;
                return toolLevel_1;
            }
            weightNum -= toolWeightLevel_1;

            if (weightNum - toolWeightLevel_2 < 0)
            {
                type = RandomItemType.TOOL;
                return toolLevel_2;
            }
            weightNum -= toolWeightLevel_2;

            if (weightNum - toolWeightLevel_3 < 0)
            {
                type = RandomItemType.TOOL;
                return toolLevel_3;
            }
            weightNum -= toolWeightLevel_3;

            if (weightNum - scriptWeightLevel_1 < 0)
            {
                type = RandomItemType.SCRIPT;
                return scriptLevel_1;
            }
            weightNum -= scriptWeightLevel_1;

            if (weightNum - scriptWeightLevel_2 < 0)
            {
                type = RandomItemType.SCRIPT;
                return scriptLevel_2;
            }
            weightNum -= scriptWeightLevel_2;

            if (weightNum - scriptWeightLevel_3 < 0)
            {
                type = RandomItemType.SCRIPT;
                return scriptLevel_3;
            }
            weightNum -= scriptWeightLevel_3;

            if (weightNum - consumablesWeight < 0)
            {
                type = RandomItemType.CONSUMABLE;
                return consumables;
            }
            weightNum -= consumablesWeight; 

            type = -1;
            return -1;
        }

        public static RandomItemRulesInfo GetRuleInfoById(int id)
        {
            if (mDataDic.ContainsKey(id))
                return mDataDic[id];
            return null;
        }

        public static void LoadData(){

            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("randomItemrules");
            while (reader.Read())
            {
                RandomItemRulesInfo rulesInfo = new RandomItemRulesInfo();

                rulesInfo.rulesNo = Convert.ToInt32(reader.GetString(reader.GetOrdinal("rulesno")));
                rulesInfo.rulesId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("rulesid")));
                rulesInfo.equipmentLevel_1 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("equipmentLevel_1")));
                rulesInfo.equipmentWeightLevel_1 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("equipmentWeightLevel_1")));
                rulesInfo.equipmentLevel_2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("equipmentLevel_2")));
                rulesInfo.equipmentWeightLevel_2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("equipmentWeightLevel_2")));
                rulesInfo.equipmentLevel_3 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("equipmentLevel_3")));
                rulesInfo.equipmentWeightLevel_3 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("equipmentWeightLevel_3")));
                rulesInfo.toolLevel_1 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("toolLevel_1")));
                rulesInfo.toolWeightLevel_1 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("toolWeightLevel_1")));
                rulesInfo.toolLevel_2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("toolLevel_2")));
                rulesInfo.toolWeightLevel_2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("toolWeightLevel_2")));
                rulesInfo.toolLevel_3 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("toolLevel_3")));
                rulesInfo.toolWeightLevel_3 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("toolWeightLevel_3")));
                rulesInfo.scriptLevel_1 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptLevel_1")));
                rulesInfo.scriptWeightLevel_1 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptWeightLevel_1")));
                rulesInfo.scriptLevel_2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptLevel_2")));
                rulesInfo.scriptWeightLevel_2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptWeightLevel_2")));
                rulesInfo.scriptLevel_3 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptLevel_3")));
                rulesInfo.scriptWeightLevel_3 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptWeightLevel_3")));

                rulesInfo.consumables = Convert.ToInt32(reader.GetString(reader.GetOrdinal("consumables")));
                rulesInfo.consumablesWeight = Convert.ToInt32(reader.GetString(reader.GetOrdinal("consumablesWeight")));

                rulesInfo.weightPool = rulesInfo.CountWeightPool();

                mDataDic.Add(rulesInfo.rulesId, rulesInfo);
            }
        }
    }

    public class RandomItemTypeInfo
    {
        public int itemNo;
        public int prototypeItem_id;
        public int itemAmount;
        public int itemWeight;
        public float itemFloating;
        public int itemLevel;
        public int itemType;

        public static Dictionary<int, Dictionary<int, List<RandomItemTypeInfo>>> mDataDic = new Dictionary<int, Dictionary<int, List<RandomItemTypeInfo>>>();

        public static List<RandomItemTypeInfo> RandomItemTypeInfoByLevel(int level,int type)
        {
            if (mDataDic.ContainsKey(type)&&mDataDic[type].ContainsKey(level))
            {
                return mDataDic[type][level];
            }
//			Debug.LogError(string.Format("randomitemtype not exist: {0},{1} ",type,level));
            return null;
        }


        public static void LoadData(){
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("randomitemtype");
            while (reader.Read())
            {
                RandomItemTypeInfo typeInfo = new RandomItemTypeInfo();

                typeInfo.itemNo = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemno")));
                typeInfo.prototypeItem_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("prototypeItem_id")));
                typeInfo.itemAmount = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemAmount")));
                typeInfo.itemWeight = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemWeight")));
                typeInfo.itemFloating = Convert.ToSingle(reader.GetString(reader.GetOrdinal("itemFloating")));
                typeInfo.itemLevel = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemtypeLevel")));
                typeInfo.itemType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemtype")));
                if (ItemAsset.ItemProto.Mgr.Instance.Get(typeInfo.prototypeItem_id)==null)
                {
                    Debug.LogError("database Error:table-randomitemtype, itemno-" + typeInfo.itemNo + ", prototypeItem_id-" + typeInfo.prototypeItem_id);
                    continue;
                }

                if (!mDataDic.ContainsKey(typeInfo.itemType))
                    mDataDic[typeInfo.itemType] = new Dictionary<int, List<RandomItemTypeInfo>>();
                if(!mDataDic[typeInfo.itemType].ContainsKey(typeInfo.itemLevel))
                    mDataDic[typeInfo.itemType][typeInfo.itemLevel]=new List<RandomItemTypeInfo> ();
                mDataDic[typeInfo.itemType][typeInfo.itemLevel].Add(typeInfo);
            }
        }
    }
}