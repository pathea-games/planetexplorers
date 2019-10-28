//#define TMPCODE_FOR_NO_GROUP
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using Mono.Data.SqliteClient;
using Random = UnityEngine.Random;

public struct AISpeciesData
{
	bool _bGrd;
    int _id;
    float _percent;
	public float Percent{ get { return _percent; } }
	public int Id{ get { return _bGrd ? (_id | Pathea.EntityProto.IdGrpMask) : _id; } }

	public static AISpeciesData[] AnalysisSpeciesString(string species, bool grp = false)
    {
        if (species == null || species == "" || species == "0") 
            return null;

        List<AISpeciesData> sp = new List<AISpeciesData>();
        string[] speciesData = species.Split(new char[] { ';' });
        foreach (string ite in speciesData)
        {
            string[] s = ite.Split(new char[] { ',' });
            if (s.Length != 2) continue;

			AISpeciesData data;
			data._bGrd = grp;
            data._id = Convert.ToInt32(s[0]);
            data._percent = Convert.ToSingle(s[1]);
            sp.Add(data);
        }
        return sp.ToArray();
    }
	public static AISpeciesData[] AnalysisSpeciesString(string species, string grpSpecies)
	{
		List<AISpeciesData> sp = new List<AISpeciesData>();

		string[] speciesData;
		speciesData = species.Split (new char[] { ';' });
		foreach (string ite in speciesData) {
			string[] s = ite.Split (new char[] { ',' });
			if (s.Length != 2)
				continue;
		
			AISpeciesData data;
			data._bGrd = false;
			data._id = Convert.ToInt32 (s [0]);
			data._percent = Convert.ToSingle (s [1]);
			sp.Add (data);
		}
		speciesData = grpSpecies.Split (new char[] { ';' });
		foreach (string ite in speciesData) {
			string[] s = ite.Split (new char[] { ',' });
			if (s.Length != 2)
				continue;
			
			AISpeciesData data;
			data._bGrd = true;
			data._id = Convert.ToInt32 (s [0]);
			data._percent = Convert.ToSingle (s [1]);
			sp.Add (data);
		}
		return sp.ToArray();
	}

    public static int GetRandomAI(AISpeciesData[] data)
    {
        if (data == null || data.Length == 0) {
			Debug.LogError("Failed to get RandomAI data");
			return -1;
		}

        int _id = -1;
        float _value = 0.0f;
        float _ranValue = Random.value;
#if TMPCODE_FOR_NO_GROUP
		while (_id == -1)		
#endif
		foreach (AISpeciesData ite in data)
        {
			_value += ite.Percent;
            if (_ranValue <= _value)
            {
                _id = ite.Id;
                break;
            }
        }
        return _id;
    }

    public static int GetRandomAI(AISpeciesData[] data, float value)
    {
        if (data == null || data.Length == 0)
            return -1;

        int _id = -1;
        float _value = 0.0f;

        foreach (AISpeciesData ite in data)
        {
			_value += ite.Percent;
            if (value <= _value)
            {
                _id = ite.Id;
                break;
            }
        }

        return _id;
    }
}

public class AISpawnDataRepository
{
    public static void LoadData()
    {
        AIResource.LoadData();
        AISpawnDataAdvMulti.LoadData();
        AISpawnDataAdvSingle.LoadData();
        AIErodeMap.LoadData();
        AISpawnDataStory.LoadData();
        AISpawnPoint.LoadData();

        AISpawnPath.LoadData();
        AISpawnWave.LoadData();
        MonsterSweepData.LoadData();
        AISpawnAutomatic.LoadData();
        AISpawnPlayerBase.LoadData();

		AISpawnTDWavesData.LoadData ();
    }
}

public class AISpawnPlayerBase
{
    int mTimerShaft;
    int mDifficulty;
    int mArea;
    int mSpawnID;

    //float mWeight;
    float mDelayTime;

    public float delayTime { get { return mDelayTime; } }
    public int spawnID { get { return mSpawnID; } }

    static List<AISpawnPlayerBase> mDataTable = new List<AISpawnPlayerBase>();

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MonsterSiege");

        while (reader.Read())
        {
            AISpawnPlayerBase pb = new AISpawnPlayerBase();

            pb.mTimerShaft = Convert.ToInt32(reader.GetString(reader.GetOrdinal("timeid_upon")));
            pb.mDifficulty = Convert.ToInt32(reader.GetString(reader.GetOrdinal("dif_id")));
            pb.mArea = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Area_type")));
            //pb.mWeight = Convert.ToSingle(reader.GetString(reader.GetOrdinal("weights")));
            pb.mSpawnID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sp_id")));
            pb.mDelayTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("start_time")));

            mDataTable.Add(pb);
        }
    }

    public static AISpawnPlayerBase GetRandomPlayerBase(int timeId, int areaId, int difficultyId)
    {
        AISpawnPlayerBase[] bases = mDataTable.FindAll(ret => ret.mTimerShaft == timeId
                                                        && ret.mArea == areaId
                                                        && ret.mDifficulty == difficultyId).ToArray();

        if (bases.Length > 0)
        {
            AISpawnPlayerBase playerBase = bases[Random.Range(0, bases.Length)];
            return playerBase;
        }

        Debug.LogError("Can't find data : [" + "time = " + timeId + "area = " + areaId + "difficulty" + difficultyId + "]");
        return new AISpawnPlayerBase();
    }
}

public class AISpawnAutomaticData
{
    public float delayTime;
    public AISpawnAutomatic data;

    public static AISpawnAutomaticData CreateAutomaticData(float delayTime, int id)
    {
        AISpawnAutomaticData data = new AISpawnAutomaticData();
        data.delayTime = delayTime;
        data.data = AISpawnAutomatic.GetAutomatic(id);

        if (data.data == null)
        {
            Debug.LogError("Can't find Automatic from ID :" + id);
        }

        return data;
    }

    public AISpawnWaveData GetWaveData(int index)
    {
        if (data == null || data.data == null)
            return null;

        return data.data.Find(ret => ret != null && ret.index == index);
    }
}

public class AISpawnWaveData
{
    public int index;
    public float delayTime;
    public AISpawnWave data;

    public static AISpawnWaveData ToWaveData(string value, int index)
    {
        AISpawnWaveData data = new AISpawnWaveData();

        string[] dataString = AiUtil.Split(value, '_');
        if (dataString.Length == 2)
        {
            data.index = index;
            data.delayTime = Convert.ToInt32(dataString[0]);
            data.data = AISpawnWave.GetWaveData(Convert.ToInt32(dataString[1]));
        }

        return data;
    }
}

public class AISpawnData
{
    public int spID;
    public int minCount;
    public int maxCount;
    public int minAngle;
    public int maxAngle;

    public bool isPath;

    public static AISpawnData ToSpawnData(string value, bool isPath)
    {
        AISpawnData data = new AISpawnData();

        data.isPath = isPath;

        string[] dataString = AiUtil.Split(value, '_');
        if (dataString.Length == 5)
        {
            data.spID = Convert.ToInt32(dataString[0]);
            data.minCount = Convert.ToInt32(dataString[1]);
            data.maxCount = Convert.ToInt32(dataString[2]);
            data.minAngle = Convert.ToInt32(dataString[3]);
            data.maxAngle = Convert.ToInt32(dataString[4]);
        }

        return data;
    }
}

public class AISpawnPath
{
    int mID;
    float mDamage;
    float mMaxHp;

    string mLandStr;
    string mWaterStr;
    string mSkyStr;
    string mCaveStr;

    int[] mLand;
    int[] mWater;
    int[] mSky;
    int[] mCave;

    public float damage { get { return mDamage; } }
    public float maxHp { get { return mMaxHp; } }

    static Dictionary<int, AISpawnPath> mDataTable = new Dictionary<int, AISpawnPath>();

    public static int GetPathID(int spid, int pointType)
    {
        if (mDataTable.ContainsKey(spid))
        {
            return mDataTable[spid].GetRandomPathID(pointType);
        }

        Debug.LogError("Can't find AISpawnPath from ID : " + spid);
        return 0;
    }

    public static AISpawnPath GetSpawnPath(int spid)
    {
        if (mDataTable.ContainsKey(spid))
        {
            return mDataTable[spid];
        }

        Debug.LogError("Can't find AISpawnPath from ID : " + spid);
        return null;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MonsterSiegeSpawnSimple");

        while (reader.Read())
        {
            AISpawnPath sp = new AISpawnPath();

            sp.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sp_id")));
            sp.mLandStr = reader.GetString(reader.GetOrdinal("land"));
            sp.mWaterStr = reader.GetString(reader.GetOrdinal("water"));
            sp.mSkyStr = reader.GetString(reader.GetOrdinal("sky"));
            sp.mCaveStr = reader.GetString(reader.GetOrdinal("hole"));
            sp.mDamage = reader.GetFloat(reader.GetOrdinal("dps"));
            sp.mMaxHp = reader.GetFloat(reader.GetOrdinal("rhp"));

            sp.InitData();

            if (mDataTable.ContainsKey(sp.mID))
            {
                Debug.LogError("AISpawnAutomatic data id is error!");
                continue;
            }

            mDataTable.Add(sp.mID, sp);
        }
    }

    void InitData()
    {
        mLand = ToData(mLandStr);
        mWater = ToData(mWaterStr);
        mSky = ToData(mSkyStr);
        mCave = ToData(mCaveStr);
    }

    int[] ToData(string value)
    {
        List<int> tmpList = new List<int>();

        string[] dataString = AiUtil.Split(value, ',');
        foreach (string ite in dataString)
        {
            tmpList.Add(Convert.ToInt32(ite));
        }

        return tmpList.ToArray();
    }

    int GetRandomPathID(int pointType)
    {
        switch (pointType)
        {
            case 1:
                return mLand[Random.Range(0, mLand.Length)];
            case 2:
                return mWater[Random.Range(0, mWater.Length)];
            case 3:
                return mCave[Random.Range(0, mCave.Length)];
            case 4:
                return mSky[Random.Range(0, mSky.Length)];
            default:
                return 0;
        }
    }
}

public class AISpawnTDWavesData
{
	public class TDMonsterData
	{
		int _type;
		public bool IsGrp{ get{ return _type == 1; } }
		public bool IsAirbornePaja{ get { return _type == 2; } }
		public bool IsAirbornePuja{ get { return _type == 3; } }
		public int ProtoId;
		public static List<TDMonsterData> GetMonsterDataLst(string strMonsterDatasDesc)
		{
			string[] descs = strMonsterDatasDesc.Split (',');
			int cnt = descs.Length;
			List<TDMonsterData> lstData = new List<TDMonsterData> (cnt);
			for (int i = 0; i < cnt; i++) {
				string[] nums = descs[i].Split('_');
				TDMonsterData md = new TDMonsterData();
				md._type = Convert.ToInt32 (nums[0]);
				md.ProtoId = Convert.ToInt32 (nums[1]);
				lstData.Add(md);
			}
			return lstData;
		}
	}
	public class TDWaveData
	{
		public int _delayTime;
        public List<int> _plotID = new List<int>();
		public List<int> _monsterTypes = new List<int>();
		public List<int> _minNums = new List<int>();
		public List<int> _maxNums = new List<int>();
		public List<int> _minDegs = new List<int>();
		public List<int> _maxDegs = new List<int>();
		public static List<TDWaveData> GetWaveDataLst(string strWaveDatasDesc)
		{
			string[] descs = strWaveDatasDesc.Split (';');
			int cnt = descs.Length;
			List<TDWaveData> lstData = new List<TDWaveData> (cnt);
			for (int i = 0; i < cnt; i++) {
				string[] nums = descs[i].Split('_',',');
				TDWaveData wd = new TDWaveData();
				wd._delayTime = Convert.ToInt32 (nums[0]);
				int idxNum = 1;
				while(nums.Length > idxNum)
				{
					wd._monsterTypes.Add(Convert.ToInt32 (nums[idxNum++]));
					wd._minNums.Add(Convert.ToInt32 (nums[idxNum++]));
					wd._maxNums.Add(Convert.ToInt32 (nums[idxNum++]));
					wd._minDegs.Add(Convert.ToInt32 (nums[idxNum++]));
					wd._maxDegs.Add(Convert.ToInt32 (nums[idxNum++]));
				}
				lstData.Add(wd);
			}
			return lstData;
		}
	}
	public class TDMonsterSpData
	{
		public int _spType;
		public int _spawnType; //Monster/Puja/Paja or for story task
		public int _areaTypeRandTer;
		public List<int> _areaTypeStoryTer;
        public int _dps;
        public int _rhp;
		public int _diflv; // just for spType==-1
		public List<TDMonsterData>[] _terMonsterDatas = new List<TDMonsterData>[4];	// land, water, hole, sky
		public static List<int> GetAreaTypes(string strAreas)
		{
			string[] descs = strAreas.Split (',');
			int cnt = descs.Length;
			List<int> lstData = new List<int> (cnt);
			for (int i = 0; i < cnt; i++) {
				int area = Convert.ToInt32 (descs[i]);					
				lstData.Add(area);
			}
			return lstData;
		}
	}
	public class TDWaveSpData
	{
		public int _dif;
		public float _weight;
		public int _spawnType;
		public int _timeToCool;
		public int _timeToStart;
		public int _timeToDelete;
        public List<TDWaveData> _waveDatas = new List<TDWaveData>();
	}
	static List<TDMonsterSpData> _lstMonsterSpData = null;	
	static List<TDWaveSpData> _lstWaveSpData = null;	
	public static void LoadData()
	{
		if (_lstWaveSpData == null) {
			_lstWaveSpData = new List<TDWaveSpData>();
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable ("MonsterBesiege");		
			while (reader.Read()) {
				TDWaveSpData wd = new TDWaveSpData ();			
				wd._dif = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("dif_coef")));
				wd._weight = Convert.ToSingle (reader.GetString (reader.GetOrdinal ("weights")));
				wd._spawnType = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("spawn_type")));
				wd._timeToCool = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("cd_time")));
				wd._timeToStart = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("start_time")));
				wd._timeToDelete = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("remaining_time")));
                wd._waveDatas = TDWaveData.GetWaveDataLst (reader.GetString (reader.GetOrdinal ("sp_type")));
				_lstWaveSpData.Add (wd);
			}
		}
		if (_lstMonsterSpData == null) {
			_lstMonsterSpData = new List<TDMonsterSpData>();
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable ("MonsterBesiegeSpawn");		
			while (reader.Read()) {
				TDMonsterSpData md = new TDMonsterSpData ();			
				md._diflv = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("dif_lv")));  // just for spType==-1
				md._spType = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("sp_type")));
				md._spawnType = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("spawn_type")));
				md._dps = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("dps")));
				md._rhp = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("rhp")));
				md._areaTypeRandTer = Convert.ToInt32 (reader.GetString (reader.GetOrdinal ("area_type_R")));
				md._areaTypeStoryTer = TDMonsterSpData.GetAreaTypes(reader.GetString (reader.GetOrdinal ("area_type_S")));
				md._terMonsterDatas[0] = TDMonsterData.GetMonsterDataLst (reader.GetString (reader.GetOrdinal ("land")));
				md._terMonsterDatas[1] = TDMonsterData.GetMonsterDataLst (reader.GetString (reader.GetOrdinal ("water")));
				md._terMonsterDatas[2] = TDMonsterData.GetMonsterDataLst (reader.GetString (reader.GetOrdinal ("hole")));
				md._terMonsterDatas[3] = TDMonsterData.GetMonsterDataLst (reader.GetString (reader.GetOrdinal ("sky")));
				_lstMonsterSpData.Add (md);
			}
		}
	}
	public static TDWaveSpData GetWaveSpData(int dif, float weight, List<int> spawnTypes)
	{
		if (dif >= 500) {
			return _lstWaveSpData.Find ((w) => dif == w._dif);
		}

		List<TDWaveSpData> spDatas = _lstWaveSpData.FindAll ((w) => dif == w._dif && spawnTypes.Contains(w._spawnType));
		if(spDatas != null && spDatas.Count > 0){
			int idx = 0;
			TDWaveSpData spData = spDatas[idx];
			do{
				spData = spDatas[idx];
				weight -= spData._weight;
				if(++idx >= spDatas.Count){
					idx = 0;
				}
			}while(weight > 0);
			return spData;
		}
        return null;
	}	
	public static TDMonsterSpData GetMonsterSpData(bool bRandTer, int spType, int diflv, int spawnType, int areaType = -1, int terType = 0)
	{
		TDMonsterSpData md = null;
		if (areaType >= 0) {
			md = (bRandTer) 
				? _lstMonsterSpData.Find ((m) => m._spType == spType && m._spawnType == spawnType && m._areaTypeRandTer == areaType) 
				: _lstMonsterSpData.Find ((m) => m._spType == spType && m._spawnType == spawnType && m._areaTypeStoryTer.Contains (areaType));
		} else {
			md = _lstMonsterSpData.Find ((m) => m._spType == spType && m._spawnType == spawnType);
		}
		if(md == null)	md = _lstMonsterSpData.Find((m)=>m._spType==-1 && m._diflv == diflv && m._spawnType==spawnType);
		if(md == null)	md = _lstMonsterSpData.Find((m)=>m._spType==-1 && m._diflv == diflv && m._spawnType==-1);		
		return md;
	}
	public static TDMonsterData GetMonsterProtoId(bool bRandTer, int spType, int diflv, int spawnType, int areaType = -1, int terType = 0, int opPlayerId = -1)
	{
		TDMonsterSpData md = GetMonsterSpData(bRandTer, spType, diflv, spawnType, areaType, terType);
		if (md != null) {
			List<TDMonsterData> lstm = md._terMonsterDatas[terType];
			int nTry = 0;
			while(nTry < 5){
				TDMonsterData mdata = lstm[Random.Range(0, lstm.Count)];
				if(opPlayerId >= 0){
					if(!mdata.IsGrp){
						Pathea.MonsterProtoDb.Item protoItem = Pathea.MonsterProtoDb.Get(mdata.ProtoId);
						if (null != protoItem){
							int playerId = (int)protoItem.dbAttr.attributeArray[(int)Pathea.AttribType.DefaultPlayerID];
							if(!PETools.PEUtil.CanDamageReputation(playerId, opPlayerId)){
								nTry++;
								continue;
							}
						}
					}
				}
				return mdata;
			}
		}
		return null;
	}
}

public class MonsterSweepData 
{
    public struct MonsterData
	{
        public int typeId;
        public int num;
	}

    public int id;
    public int cdTime;
    public int preTime;
    public int minAngle;
    public int maxAngle;
    public Vector3 spawnPos;
    public int deviation;
    public List<MonsterData> monsData;
    public List<int> plotId;

    public MonsterSweepData() 
    {
        monsData = new List<MonsterData>();
        plotId = new List<int>();
    }

    public static AISpawnTDWavesData.TDWaveSpData GetWaveSpData(List<int> tmp, Vector3 endPoint)
    {
        AISpawnTDWavesData.TDWaveSpData tdwsd = new AISpawnTDWavesData.TDWaveSpData();
        foreach (var item in tmp)
        {
            MonsterSweepData msd = monsterSweepData[item];
            if (msd == null)
                continue;
            tdwsd._dif = 0;
            List<int> mincdTime = new List<int>(Array.ConvertAll<int, int>(tmp.ToArray(), ite => monsterSweepData[ite].cdTime));
            mincdTime.Sort();
            tdwsd._timeToCool = mincdTime[0];
            tdwsd._timeToStart = 0;
            tdwsd._weight = 0;
            AISpawnTDWavesData.TDWaveData wave = new AISpawnTDWavesData.TDWaveData();
            wave._delayTime = msd.preTime;
            wave._plotID = msd.plotId;
            foreach (var item1 in msd.monsData)
            {
                if (msd.minAngle != 0 || msd.maxAngle != 0)
                {
                    wave._maxDegs.Add(msd.maxAngle);
                    wave._minDegs.Add(msd.minAngle);
                }
                else
                {
                    Vector3 v = msd.spawnPos - endPoint;
                    int angle = (int)(System.Math.Atan(v.x / v.z) / Math.PI * 180);
                    if (v.z < 0)
                        angle += 180;
                    wave._maxDegs.Add(angle + msd.deviation);
                    wave._minDegs.Add(angle - msd.deviation);
                }
                wave._maxNums.Add(item1.num);
                wave._minNums.Add(item1.num);
                wave._monsterTypes.Add(item1.typeId);
            }
            tdwsd._waveDatas.Add(wave);
        }
        return tdwsd;
    }

    public static Dictionary<int, MonsterSweepData> monsterSweepData = new Dictionary<int, MonsterSweepData>();
    public static void LoadData() 
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MonsterSweep");
        while (reader.Read())
        {
            string tmp;
            string[] tmpList,tmpList1;
            float x, y, z;
            MonsterSweepData msd = new MonsterSweepData();
            msd.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            msd.cdTime = Convert.ToInt32(reader.GetString(reader.GetOrdinal("cd_time")));
            msd.preTime = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PreTime")));
            tmpList = reader.GetString(reader.GetOrdinal("Angle")).Split('_');
            if (tmpList.Length == 2)
            {
                msd.minAngle = Convert.ToInt32(tmpList[0]);
                msd.maxAngle = Convert.ToInt32(tmpList[1]);
            }

            tmp = reader.GetString(reader.GetOrdinal("MonsPos"));
            tmpList = tmp.Split('_');
            if (tmpList.Length == 2)
            {
                msd.deviation = Convert.ToInt32(tmpList[1]);
                tmpList1 = tmpList[0].Split(',');
                if (tmpList1.Length == 3)
                {
                    x = Convert.ToSingle(tmpList1[0]);
                    y = Convert.ToSingle(tmpList1[1]);
                    z = Convert.ToSingle(tmpList1[2]);
                    msd.spawnPos = new Vector3(x, y, z);
                }
            }

            tmp = reader.GetString(reader.GetOrdinal("Monslist"));
            tmpList = tmp.Split(';');
            foreach (var item in tmpList)
            {
                tmpList1 = item.Split('_');
                if (tmpList1.Length != 2)
                    continue;
                MonsterData md;
                md.typeId = Convert.ToInt32(tmpList1[0]);
                md.num = Convert.ToInt32(tmpList1[1]);

                msd.monsData.Add(md);
            }

            tmp = reader.GetString(reader.GetOrdinal("plot"));
            tmpList = tmp.Split(',');
            foreach (var item in tmpList)
            {
                if (item == "0")
                    continue;
                msd.plotId.Add(Convert.ToInt32(item));
            }

            monsterSweepData.Add(msd.id, msd);
        }
    }
}

public class AISpawnAutomatic
{
    int mID;

    string mDataString;
    List<AISpawnWaveData> mData;

    public List<AISpawnWaveData> data { get { return mData; } }

    static Dictionary<int, AISpawnAutomatic> mDataTable = new Dictionary<int,AISpawnAutomatic>();

    public static AISpawnAutomatic GetAutomatic(int id)
    {
        return mDataTable.ContainsKey(id) ? mDataTable[id] : null;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MonsterSiege_SpawnId");

        while (reader.Read())
        {
            AISpawnAutomatic auto = new AISpawnAutomatic();

            auto.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Sp_id")));
            auto.mDataString = reader.GetString(reader.GetOrdinal("Sp_type"));
            auto.InitData();

            if (mDataTable.ContainsKey(auto.mID))
            {
                Debug.LogError("AISpawnAutomatic data id is error!");
                continue;
            }

            mDataTable.Add(auto.mID, auto);
        }
    }

    void InitData()
    {
        mData = new List<AISpawnWaveData>();

        string[] dataString = AiUtil.Split(mDataString, ',');
        foreach (string ite in dataString)
        {
            if (!string.IsNullOrEmpty(ite))
            {
                mData.Add(AISpawnWaveData.ToWaveData(ite, mData.Count));
            }
        }
    }
}

public class AISpawnWave
{
    int mID;
    bool mIsPath;
    string mDataString;
    List<AISpawnData> mData;

    public List<AISpawnData> data { get { return mData; } }

    static Dictionary<int, AISpawnWave> mDataTable = new Dictionary<int, AISpawnWave>();

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MonsterSiege_SpawnTypeId");

        while (reader.Read())
        {
            AISpawnWave wave = new AISpawnWave();

            wave.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("SpType_id")));
            wave.mIsPath = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("IsPath_Id")));
            wave.mDataString = reader.GetString(reader.GetOrdinal("Sp_type"));
            wave.InitData();

            if (mDataTable.ContainsKey(wave.mID))
            {
                Debug.LogError("AISpawnWave data id is error!");
                continue;
            }

            mDataTable.Add(wave.mID, wave);
        }
    }

    public static AISpawnWave GetWaveData(int id)
    {
        return mDataTable.ContainsKey(id) ? mDataTable[id] : null;
    }

    void InitData()
    {
        mData = new List<AISpawnData>();

        string[] dataString = AiUtil.Split(mDataString, ',');

        foreach (string ite in dataString)
        {
            if (!string.IsNullOrEmpty(ite))
            {
                mData.Add(AISpawnData.ToSpawnData(ite, mIsPath));
            }
        }
    }
}

public class AISpawnPoint
{
    public static List<SPPoint> points = new List<SPPoint>();
	public static Dictionary<int, AISpawnPoint> s_spawnPointData = new Dictionary<int, AISpawnPoint>();
	//public static Dictionary<int, AISpawnPoint> data { get { return s_spawnPointData; } }

    public int id;
    public int resId;
    public int count;
    public bool active;
    public bool fixPosition;
	public bool isGroup;
	public bool isTower;

    public float radius;
    public float refreshtime;

    private Vector3 position;

    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }
	public Vector3 euler;

    bool mActive;
    SPPoint mPoint;
    public SPPoint spPoint
    {
        get{return mPoint;}
        set
        {
            if (mPoint != null)
            {
                if(points.Contains(mPoint))
                    points.Remove(mPoint);
                GameObject.Destroy(mPoint.gameObject);

                Debug.LogError("Static SPPoint has existed!!");
            }

            mPoint = value;

            if (mPoint != null)
            {
                if (!points.Contains(mPoint))
                    points.Add(mPoint);
            }
        }
    }

	public bool isActive { get { return mActive; } }

    void OnDeath(AiObject aiObj)
    {
        mActive = false;
        if (spPoint != null)
        {
            spPoint.SetActive(false);
        }
    }

    public void OnSpawned(GameObject obj)
    {
        if (obj == null)
            return;

        AiObject aiObj = obj.GetComponent<AiObject>();
        if (aiObj != null)
        {
            aiObj.DeathHandlerEvent += OnDeath;
        }
    }

    public static void Reset()
    {
		foreach (KeyValuePair<int,AISpawnPoint> pair in s_spawnPointData)
        {
            pair.Value.ResetActive();
        }
    }

    public static void Activate(int pointID, bool isActive)
    {
		AISpawnPoint point = Find(pointID);
		if (point != null && point.spPoint != null)
        {
            point.mActive = isActive;
            point.spPoint.SetActive(isActive);
        }
    }

    public static void SpawnImmediately(int pointID)
    {
		AISpawnPoint point = Find(pointID);
		if (point != null && point.spPoint != null)
		{
            point.spPoint.SetActive(true);
			point.spPoint.InstantiateImmediately();
        }
    }

	public static AISpawnPoint Find(int pointID)
    {
		AISpawnPoint point;
		return s_spawnPointData.TryGetValue(pointID, out point) ? point : null;
    }

	public static List<int> Find(float minx, float minz, float maxx, float maxz)
	{
		List<int> ret = new List<int>();
		foreach (KeyValuePair<int,AISpawnPoint> pair in s_spawnPointData)
		{
			Vector3 pos = pair.Value.Position;
			if(pos.x > minx && pos.x <= maxx && pos.z > minz && pos.z <= maxz)
			{
				ret.Add(pair.Key);
			}
		}
		return ret;
	}

    public static List<SPPoint> GetPoints(IntVector4 node)
    {
        return points.FindAll(ret => Match(ret, node));
    }

    static bool Match(SPPoint point, IntVector4 node)
    {
		if(point == null)
			return false;

        float dx = point.position.x - node.x;
        float dz = point.position.z - node.z;

        return dx >= PETools.PEMath.Epsilon && dx <= (VoxelTerrainConstants._numVoxelsPerAxis << node.w)
            && dz >= PETools.PEMath.Epsilon && dz <= (VoxelTerrainConstants._numVoxelsPerAxis << node.w);
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("aispawn_fix");

        while (reader.Read())
        {
            AISpawnPoint sp = new AISpawnPoint();
            sp.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));

            string tmp = reader.GetString(reader.GetOrdinal("position"));
            if (tmp.Split(',').Length == 3)
                sp.position = AiUtil.ToVector3(reader.GetString(reader.GetOrdinal("position")));
            sp.resId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("pathid")));
			sp.radius = Convert.ToSingle(reader.GetString(reader.GetOrdinal("radius")));
			sp.refreshtime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("refresh")));
            sp.count = Convert.ToInt32(reader.GetString(reader.GetOrdinal("maxnum")));
            sp.active = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("produce")));
			sp.fixPosition = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("isfixpoint")));
			sp.euler = AiUtil.ToVector3(reader.GetString(reader.GetOrdinal("rotation")));
			sp.isGroup = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("isgroup")));
			sp.isTower = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("istower")));

            s_spawnPointData.Add(sp.id, sp);
        }
    }

    public static void RegisterSPPoint(SPPoint point)
    {
        if (point == null)
            return;

        if (!points.Contains(point))
            points.Add(point);
    }

    public void ResetActive()
    {
        mActive = active;
    }
}

public class AIResource
{
    public int id;
    public int aiId;
    public float height;
    public float minScale;
    public float maxScale;
    public string name;
    public string path;

    static List<AIResource> m_data = new List<AIResource>();

    public static AIResource Find(int argId)
    {
        return m_data.Find(ret => ret.id == argId);
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("path");

        while (reader.Read())
        {
            AIResource res = new AIResource();

            res.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
            res.aiId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("aiid")));
            res.name = reader.GetString(reader.GetOrdinal("name"));
            res.path = reader.GetString(reader.GetOrdinal("path"));
            res.height = Convert.ToSingle(reader.GetString(reader.GetOrdinal("height")));
            res.minScale = Convert.ToSingle(reader.GetString(reader.GetOrdinal("minscale")));
            res.maxScale = Convert.ToSingle(reader.GetString(reader.GetOrdinal("maxscale")));

            if (Find(res.id) != null)
            {
                Debug.LogError("Can't have the same id!");
                continue;
            }

            m_data.Add(res);
        }
    }

    public static bool IsGroup(int pathid)
    {
        AIResource res = Find(pathid);
        return res != null && res.path.StartsWith("Group");
    }

    public static Vector3 FixedHeightOfAIResource(int id, Vector3 position)
    {
        AIResource res = AIResource.Find(id);
        if (res == null)
            return Vector3.zero;

        Vector3 newPosition = position;
        if (FixedHeight(res, ref newPosition))
            return newPosition;
        else
            return Vector3.zero;
    }

    static bool FixedHeight(AIResource res, ref Vector3 position)
    {
        if (res == null)
            return false;

        if (res.height > -PETools.PEMath.Epsilon && res.height < PETools.PEMath.Epsilon)
            return true;

        if (res.height > PETools.PEMath.Epsilon)
        {
            position += Vector3.up * res.height;
            return true;
        }
        else
        {
            float riverHight;
            if (AiUtil.CheckPositionUnderWater(position, out riverHight))
            {
                float upHeight = riverHight;

                Vector3 cavePoint;
                if (AiUtil.CheckPositionInCave(position, out cavePoint))
                {
                    upHeight = cavePoint.y;
                }

                if (position.y < upHeight + res.height)
                {
                    position += Vector3.up * Random.Range(0.0f, upHeight + res.height - position.y);
                    return true;
                }
            }
        }

        Debug.LogWarning("Can't find right spawn point!");
        return false;
    }

	public static AssetReq Instantiate(int id, Vector3 position, Quaternion rot, AssetReq.ReqFinishDelegate onSpawned = null)
    {
        if (position == Vector3.zero)
            return null;

        AIResource res = Find(id);

        if (res != null)
        {
            AssetReq req = AssetsLoader.Instance.AddReq(res.path, position, rot, Vector3.one);
            req.ReqFinishHandler += onSpawned;
            req.ReqFinishHandler += res.OnSpawnedObject;
            return req;
        }

        return null;
    }

    public void OnSpawnedObject(GameObject obj)
    {
        if (obj == null)
            return;

        if(!GameConfig.IsMultiMode)
        {
            //AiDataObject aiDataObject = obj.GetComponent<AiDataObject>();
            //if (aiDataObject != null)
            //{
            //    aiDataObject.SetUpCollider(Random.Range(minScale, maxScale));
            //}
        }
    }
}

public class AIErodeMap
{
    public int id;
    public Vector3 pos;
    public float radius;

	static List<AIErodeMap> m_blankData = new List<AIErodeMap>();
    static List<AIErodeMap> m_data = new List<AIErodeMap>();
	static int m_nextId;
    
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("aispawn_blank");
		while (reader.Read())
        {
            AIErodeMap data = new AIErodeMap();
			data.id = m_blankData.Count;
            data.pos = AiUtil.ToVector3(reader.GetString(reader.GetOrdinal("position")));
            data.radius = Convert.ToSingle(reader.GetString(reader.GetOrdinal("radius")));
			m_blankData.Add(data);
        }
    }

	public static void ResetErodeData()
	{
		m_data.Clear();
		if (Pathea.PeGameMgr.IsStory) {
			m_data.AddRange(m_blankData);
		}
		m_nextId = m_data.Count;
	}

    public static int AddErode(Vector3 position, float radius)
    {
        AIErodeMap data = new AIErodeMap();
		data.id = m_nextId++;
        data.pos = position;
        data.radius = radius;

        m_data.Add(data);
		return data.id;
    }

    public static void UpdateErode(int id, Vector3 center, float radius = 0.0f)
    {
        AIErodeMap erode = m_data.Find(ret => ret.id == id);
        if (erode != null)
        {
            if (center != Vector3.zero)
            {
                erode.pos = center;
            }
            if (radius > PETools.PEMath.Epsilon)
            {
                erode.radius = radius;
            }
        }
    }

    public static void RemoveErode(int id)
    {
        AIErodeMap erode = m_data.Find(ret => ret.id == id);
        if (erode != null)
        {
            m_data.Remove(erode);
        }
    }

	public static AIErodeMap IsInErodeArea2D(Vector3 position)
	{
		AIErodeMap erode = m_data.Find(ret => Match2D(ret, position));		
		//if (erode != null)	Debug.Log("Spawn position " + "< " + position + " >" + " is in erode! "+" erode pos = " + erode.pos + "erode radius = " + erode.radius);		
		return erode;
	}
	public static AIErodeMap IsInScaledErodeArea2D(Vector3 position, float fScale)
	{
		AIErodeMap erode = m_data.Find(ret => MatchScaled2D(ret, position, fScale));		
		//if (erode != null)	Debug.Log("Spawn position " + "< " + position + " >" + " is in erode! "+" erode pos = " + erode.pos + "erode radius = " + erode.radius);		
		return erode;
	}
	public static AIErodeMap IsInErodeArea(Vector3 position)
    {
        AIErodeMap erode = m_data.Find(ret => Match(ret, position));
        //if (erode != null)	Debug.Log("Spawn position " + "< " + position + " >" + " is in erode! "+" erode pos = " + erode.pos + "erode radius = " + erode.radius);
        return erode;
    }

	static bool Match2D(AIErodeMap data, Vector3 position)
	{
		Vector3 delta = data.pos - position;
		delta.y = 0;
		return delta.sqrMagnitude < (data.radius*data.radius);
	}
	static bool MatchScaled2D(AIErodeMap data, Vector3 position, float fScale)
	{
		Vector3 delta = data.pos - position;
		delta.y = 0;
		return delta.sqrMagnitude < (data.radius*data.radius*fScale*fScale);
	}

	static bool Match(AIErodeMap data, Vector3 position)
    {
		Vector3 delta = data.pos - position;
		return delta.sqrMagnitude < (data.radius*data.radius);
    }
}

public class AISpawnDataStory
{
    struct EffectData
    {
        public int type1;
        public int type2;
        public int[] effectIds;
    }

	int id;
    Color mColor;

    AISpeciesData[] mapDataLand;
    AISpeciesData[] mapDataWater;
    AISpeciesData[] mapDataCave;
    AISpeciesData[] mapDataNight;
    AISpeciesData[] mapDataSky;

    int mBgSound;
    List<int> mEnvSounds;
    List<EffectData> mEffects;

	static List<AISpawnDataStory> m_data = new List<AISpawnDataStory>();

    public static void LoadData()
    {
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("aispawn_story");

        while (reader.Read())
        {
            AISpawnDataStory map = new AISpawnDataStory();
			map.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
            map.mColor.r = reader.GetByte(reader.GetOrdinal("R")) / 255.0f;
            map.mColor.g = reader.GetByte(reader.GetOrdinal("G")) / 255.0f;
            map.mColor.b = reader.GetByte(reader.GetOrdinal("B")) / 255.0f;
            map.mColor.a = reader.GetByte(reader.GetOrdinal("A")) / 255.0f;

			map.mapDataLand = AISpeciesData.AnalysisSpeciesString( reader.GetString(reader.GetOrdinal("monsterLandInfo" )), reader.GetString(reader.GetOrdinal("groupLandInfo" )));
			map.mapDataWater = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("monsterWaterInfo")), reader.GetString(reader.GetOrdinal("groupWaterInfo")));
			map.mapDataCave = AISpeciesData.AnalysisSpeciesString( reader.GetString(reader.GetOrdinal("monsterCaveInfo" )), reader.GetString(reader.GetOrdinal("groupCaveInfo" )));
			map.mapDataNight = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("monsterNightInfo")), reader.GetString(reader.GetOrdinal("groupNightInfo")));
			map.mapDataSky = AISpeciesData.AnalysisSpeciesString(  reader.GetString(reader.GetOrdinal("monsterSkyInfo"  )), reader.GetString(reader.GetOrdinal("groupSkyInfo"  )));
			map.mBgSound = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Music")));
			map.mEnvSounds = AnalysisSoundString(reader.GetString(reader.GetOrdinal("surroundMusic")));
			map.mEffects = AnalysisEffectString(reader.GetString(reader.GetOrdinal("Effect")));
            m_data.Add(map);
        }
    }
	public static AISpeciesData[] GetAiSpawnData(int mapid, int typeid)
	{
		AISpawnDataStory aispawn = m_data.Find (it => it.id == mapid);
		if (aispawn == null) {
			Debug.LogError("[AiSpawnData]Spawn not found:" + mapid);
			return null;
		}
		
		switch (typeid)
		{
		default: 
		case 0:
			Debug.LogError("[AiSpawnData]Type is error:" + typeid);
			break;
		case 1:
			if (GameConfig.IsNight)// && UnityEngine.Random.value < 0.3f)
				return aispawn.mapDataNight;
			else
				return aispawn.mapDataLand;
		case 2:
			return aispawn.mapDataWater;
		case 3:
			return aispawn.mapDataCave;
		case 4:
			return aispawn.mapDataSky;
		}		
		return null;
	}
	public static int GetBgMusicID(int mapid)
	{
		AISpawnDataStory aispawn = m_data.Find (it => it.id == mapid);
		if (aispawn == null)
			return 0;

		return aispawn.mBgSound;
	}	
	public static int GetEnvMusicID(int mapid)
	{
		AISpawnDataStory aispawn = m_data.Find (it => it.id == mapid);
		if (aispawn != null && aispawn.mEnvSounds != null && aispawn.mEnvSounds.Count > 0)
			return aispawn.mEnvSounds[UnityEngine.Random.Range(0, aispawn.mEnvSounds.Count)];

		return 0;
	}
	public static int GetEnvEffectID(int mapid, int type1, int type2)
	{
		AISpawnDataStory map = m_data.Find (it => it.id == mapid);
		if(map != null)
		{
			EffectData data = map.mEffects.Find(ret => ret.type1 == type1 && ret.type2 == type2);
			if(data.effectIds != null && data.effectIds.Length > 0)
			{
                return data.effectIds[Random.Range(0, data.effectIds.Length)];
				//int effect = data.effectIds[Random.Range(0, data.effectIds.Length)];
				//if (effect < 0)
				//	return GetEnvironmentEffect(type1, type2, GetEnvironmentColor(position));
				//else
				//	return effect; 
			}
		}
		
		return 0;
	}
	public static int GetEnvironmentEffect(int type1, int type2, Color color)
	{
		AISpawnDataStory map = m_data.Find(ret => MatchMap(ret, color));
		if (map != null)
		{
			EffectData data = map.mEffects.Find(ret => ret.type1 == type1 && ret.type2 == type2);
			if (data.effectIds != null && data.effectIds.Length > 0)
			{
				return data.effectIds[Random.Range(0, data.effectIds.Length)];
			}
		}
		
		return 0;
	}

    static List<int> AnalysisSoundString(string str)
    {
        List<int> tmp = new List<int>();

        string[] soundStr = AiUtil.Split(str, ';');
        foreach (string ite in soundStr)
        {
            tmp.Add(Convert.ToInt32(ite));
        }

        return tmp;
    }
    static List<EffectData> AnalysisEffectString(string str)
    {
        List<EffectData> temp = new List<EffectData>();

        string[] effectStr = AiUtil.Split(str, ';');
        foreach (string ite in effectStr)
        {
            string[] effDataStr = AiUtil.Split(ite, '_');
            if (effDataStr.Length == 3)
            {
                EffectData data = new EffectData();
                data.type1 = Convert.ToInt32(effDataStr[0]);
                data.type2 = Convert.ToInt32(effDataStr[1]);

                List<int> effectList = new List<int>();
                string[] effs = AiUtil.Split(effDataStr[2], ',');
                foreach (string it in effs)
                {
                    effectList.Add(Convert.ToInt32(it));
                }

                data.effectIds = effectList.ToArray();

                temp.Add(data);
            }
        }
        return temp;
    }
    public static int GetBackGroundMusic(Color color)
    {
        AISpawnDataStory map = m_data.Find(ret => MatchMap(ret, color));
        return map != null ? map.mBgSound : 0;
    }
	public static int GetRandomPathIDFromType(int type, Vector3 position)
	{
		Debug.LogError ("[Error]Specis texture not exists now.");
		return -1;
	}
    static bool MatchMap(AISpawnDataStory map, Color color)
    {
        if(map == null)
            return false;

        return Mathf.Abs(map.mColor.r - color.r) * 255.0f < 5.0f
            && Mathf.Abs(map.mColor.g - color.g) * 255.0f < 5.0f
            && Mathf.Abs(map.mColor.b - color.b) * 255.0f < 5.0f
            && Mathf.Abs(map.mColor.a - color.a) * 255.0f < 5.0f;
    }
}

public class AISpawnDataAdvMulti
{
    public int mapType;
    public int mapIndex;
    public float minScale = 1.0f;
    public float maxScale = 1.0f;

    AISpeciesData[] mapDataLand;
    AISpeciesData[] mapDataWater;
    AISpeciesData[] mapDataNight;
    AISpeciesData[] mapDataCave;
    AISpeciesData[] mapDataSky;

    AISpeciesData[] mapDataBossLand;
    AISpeciesData[] mapDataBossWater;
    AISpeciesData[] mapDataBossCave;
    //AISpeciesData[] mapDataBossNight;
    AISpeciesData[] mapDataBossSky;

    static List<AISpawnDataAdvMulti> m_data = new List<AISpawnDataAdvMulti>();

    public static int GetPathID(int mapID, int areaID, int pointTypeID)
    {
        AISpawnDataAdvMulti data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
        if (data == null)
            return 0;
        else
            return data.GetRanomAI(pointTypeID);
    }

    public static int GetPathID(int mapID, int areaID, int pointTypeID, float value)
    {
        AISpawnDataAdvMulti data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
        if (data == null)
            return 0;
        else
            return data.GetRanomAI(pointTypeID, value);
    }

    public static int GetBossPathID(int mapID, int areaID, int pointTypeID)
    {
        AISpawnDataAdvMulti data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
        if (data == null)
            return 0;
        else
            return data.GetRandomAIBoss(pointTypeID);
    }

    public static int GetBossPathID(int mapID, int areaID, int pointTypeID, float value)
    {
        AISpawnDataAdvMulti data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
        if (data == null)
            return 0;
        else
            return data.GetRanomAIBoss(pointTypeID, value);
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("aispawn_multiAVG");

        while (reader.Read())
        {
            AISpawnDataAdvMulti map = new AISpawnDataAdvMulti();

			//"area_id" is not useful to prog
            map.mapType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("area_type")));	// grassland,desert,canyon,etc
            map.mapIndex = Convert.ToInt32(reader.GetString(reader.GetOrdinal("area_Value")));	// distance

            map.GetScaleValue(reader.GetString(reader.GetOrdinal("zoomValue")));

			map.mapDataLand = AISpeciesData.AnalysisSpeciesString( reader.GetString(reader.GetOrdinal("monsterLandInfo" )), reader.GetString(reader.GetOrdinal("groupLandInfo" )));
			map.mapDataWater = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("monsterWaterInfo")), reader.GetString(reader.GetOrdinal("groupWaterInfo")));
			map.mapDataCave = AISpeciesData.AnalysisSpeciesString( reader.GetString(reader.GetOrdinal("monsterCaveInfo" )), reader.GetString(reader.GetOrdinal("groupCaveInfo" )));
			map.mapDataNight = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("monsterNightInfo")), reader.GetString(reader.GetOrdinal("groupNightInfo")));
			map.mapDataSky = AISpeciesData.AnalysisSpeciesString(  reader.GetString(reader.GetOrdinal("monsterSkyInfo"  )), reader.GetString(reader.GetOrdinal("groupSkyInfo"  )));

			map.mapDataBossLand = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossLandInfo")));
			map.mapDataBossWater = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossWaterInfo")));
			//map.mapDataBossNight = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossNightInfo")));
			map.mapDataBossCave = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossCaveInfo")));
			map.mapDataBossSky = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossSkyInfo")));
            m_data.Add(map);
        }
    }

    void GetScaleValue(string str)
    {
        string[] strDatas = str.Split(new char[] { ',' });

        if (strDatas.Length < 2)
            return;

        minScale = Convert.ToSingle(strDatas[0]);
        maxScale = Convert.ToSingle(strDatas[1]);
    }

    int GetRanomAI(int pointType)
    {
        switch (pointType)
        {
            case 1:
				if(GameConfig.IsNight)// && Random.value < 0.3f)
                    return AISpeciesData.GetRandomAI(mapDataNight);
                else
                    return AISpeciesData.GetRandomAI(mapDataLand);
            case 2: 
                return AISpeciesData.GetRandomAI(mapDataWater);
            case 3: 
                return AISpeciesData.GetRandomAI(mapDataCave);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataSky);

            default: return 0;
        }
    }

    int GetRanomAI(int pointType, float value)
    {
        switch (pointType)
        {
            case 1:
				if (GameConfig.IsNight)// && Random.value < 0.3f)
                    return AISpeciesData.GetRandomAI(mapDataNight, value);
                else
                    return AISpeciesData.GetRandomAI(mapDataLand, value);
            case 2:
                return AISpeciesData.GetRandomAI(mapDataWater, value);
            case 3:
                return AISpeciesData.GetRandomAI(mapDataCave, value);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataSky, value);

            default: return 0;
        }
    }

    int GetRandomAIBoss(int pointType)
    {
        switch (pointType)
        {
            case 1:
                return AISpeciesData.GetRandomAI(mapDataBossLand);
            case 2:
                return AISpeciesData.GetRandomAI(mapDataBossWater);
            case 3:
                return AISpeciesData.GetRandomAI(mapDataBossCave);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataBossSky);

            default: return 0;
        }
    }

    int GetRanomAIBoss(int pointType, float value)
    {
        switch (pointType)
        {
            case 1:
                return AISpeciesData.GetRandomAI(mapDataBossLand, value);
            case 2:
                return AISpeciesData.GetRandomAI(mapDataBossWater, value);
            case 3:
                return AISpeciesData.GetRandomAI(mapDataBossCave, value);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataBossSky, value);

            default: return 0;
        }
    }
}

public class AISpawnDataAdvSingle
{
    public int mapType;
    public int mapIndex;

    public float minScale = 1.0f;
    public float maxScale = 1.0f;

    AISpeciesData[] mapDataLand;
    AISpeciesData[] mapDataWater;
    AISpeciesData[] mapDataCave;
    AISpeciesData[] mapDataNight;
    AISpeciesData[] mapDataSky;

    AISpeciesData[] mapDataBossLand;
    AISpeciesData[] mapDataBossWater;
    AISpeciesData[] mapDataBossCave;
    //AISpeciesData[] mapDataBossNight;
    AISpeciesData[] mapDataBossSky;

    static List<AISpawnDataAdvSingle> m_data = new List<AISpawnDataAdvSingle>();

	public float GetScale(int mapID, int areaID)
	{
		AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		if (data == null)
			return 1.0f;
		return UnityEngine.Random.Range (data.minScale, data.maxScale);
	}

    public static int GetPathID(int mapID, int areaID, int pointTypeID){
        AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		return data == null ? 0 : data.GetRanomAI(pointTypeID);
    }
    public static int GetPathID(int mapID, int areaID, int pointTypeID, float value){
        AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		return data == null ? 0 : data.GetRanomAI(pointTypeID, value);
    }
	public static int GetPathIDScale(int mapID, int areaID, int pointTypeID, ref float fScale){
		AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		fScale = data == null ? 1.0f : UnityEngine.Random.Range (data.minScale, data.maxScale);
		return data == null ? 0 : data.GetRanomAI(pointTypeID);
	}
	public static int GetPathIDScale(int mapID, int areaID, int pointTypeID, float value, ref float fScale){
		AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		fScale = data == null ? 1.0f : UnityEngine.Random.Range (data.minScale, data.maxScale);
		return data == null ? 0 : data.GetRanomAI(pointTypeID, value);
	}

    public static int GetBossPathID(int mapID, int areaID, int pointTypeID){
        AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		return data == null ? 0 : data.GetRanomBossAI(pointTypeID);
    }
    public static int GetBossPathID(int mapID, int areaID, int pointTypeID, float value){
        AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		return data == null ? 0 : data.GetRanomBossAI(pointTypeID, value);
    }
	public static int GetBossPathIDScale(int mapID, int areaID, int pointTypeID, ref float fScale){
		AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		fScale = data == null ? 1.0f : UnityEngine.Random.Range (data.minScale, data.maxScale);
		return data == null ? 0 : data.GetRanomBossAI(pointTypeID);
	}
	public static int GetBossPathIDScale(int mapID, int areaID, int pointTypeID, float value, ref float fScale){
		AISpawnDataAdvSingle data = m_data.Find(ret => ret.mapType == mapID && ret.mapIndex == areaID);
		fScale = data == null ? 1.0f : UnityEngine.Random.Range (data.minScale, data.maxScale);
		return data == null ? 0 : data.GetRanomBossAI(pointTypeID, value);
	}

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("aispawn_singleAVG");

        while (reader.Read())
        {
            AISpawnDataAdvSingle map = new AISpawnDataAdvSingle();

			//"area_id" is not useful to prog
			map.mapType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("area_type")));	// grassland,desert,canyon,etc
            map.mapIndex = Convert.ToInt32(reader.GetString(reader.GetOrdinal("area_Value")));	// distance

            map.GetScaleValue(reader.GetString(reader.GetOrdinal("zoomValue")));

			map.mapDataLand = AISpeciesData.AnalysisSpeciesString( reader.GetString(reader.GetOrdinal("monsterLandInfo" )), reader.GetString(reader.GetOrdinal("groupLandInfo" )));
			map.mapDataWater = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("monsterWaterInfo")), reader.GetString(reader.GetOrdinal("groupWaterInfo")));
			map.mapDataCave = AISpeciesData.AnalysisSpeciesString( reader.GetString(reader.GetOrdinal("monsterCaveInfo" )), reader.GetString(reader.GetOrdinal("groupCaveInfo" )));
			map.mapDataNight = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("monsterNightInfo")), reader.GetString(reader.GetOrdinal("groupNightInfo")));
			map.mapDataSky = AISpeciesData.AnalysisSpeciesString(  reader.GetString(reader.GetOrdinal("monsterSkyInfo"  )), reader.GetString(reader.GetOrdinal("groupSkyInfo"  )));

            map.mapDataBossLand = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossLandInfo")));
			map.mapDataBossWater = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossWaterInfo")));
			map.mapDataBossCave = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossCaveInfo")));
			//map.mapDataBossNight = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossNightInfo")));
			map.mapDataBossSky = AISpeciesData.AnalysisSpeciesString(reader.GetString(reader.GetOrdinal("bossSkyInfo")));
            m_data.Add(map);
        }
    }

    void GetScaleValue(string str)
    {
        string[] strDatas = str.Split(new char[] { ',' });

        if (strDatas.Length < 2)
            return;

        minScale = Convert.ToSingle(strDatas[0]);
        maxScale = Convert.ToSingle(strDatas[1]);
    }

    int GetRanomAI(int pointType)
    {
        switch (pointType)
        {
            case 1: 
				if(GameConfig.IsNight)// && Random.value < 0.3f)
                    return AISpeciesData.GetRandomAI(mapDataNight);
                else
                    return AISpeciesData.GetRandomAI(mapDataLand);
            case 2: 
                return AISpeciesData.GetRandomAI(mapDataWater);
            case 3: 
                return AISpeciesData.GetRandomAI(mapDataCave);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataSky);

            default: return 0;
        }
    }

    int GetRanomAI(int pointType, float value)
    {
        switch (pointType)
        {
            case 1:
				if (GameConfig.IsNight)// && Random.value < 0.3f)
                    return AISpeciesData.GetRandomAI(mapDataNight, value);
                else
                    return AISpeciesData.GetRandomAI(mapDataLand, value);
            case 2:
                return AISpeciesData.GetRandomAI(mapDataWater, value);
            case 3:
                return AISpeciesData.GetRandomAI(mapDataCave, value);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataSky, value);

            default: return 0;
        }
    }

    int GetRanomBossAI(int pointType)
    {
        switch (pointType)
        {
            case 1:
                return AISpeciesData.GetRandomAI(mapDataBossLand);
            case 2:
                return AISpeciesData.GetRandomAI(mapDataBossWater);
            case 3:
                return AISpeciesData.GetRandomAI(mapDataBossCave);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataBossSky);

            default: return 0;
        }
    }

    int GetRanomBossAI(int pointType, float value)
    {
        switch (pointType)
        {
            case 1:
                return AISpeciesData.GetRandomAI(mapDataBossLand, value);
            case 2:
                return AISpeciesData.GetRandomAI(mapDataBossWater, value);
            case 3:
                return AISpeciesData.GetRandomAI(mapDataBossCave, value);
            case 4:
                return AISpeciesData.GetRandomAI(mapDataBossSky, value);

            default: return 0;
        }
    }
}