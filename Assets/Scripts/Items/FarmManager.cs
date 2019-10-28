using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NaturalResAsset;
using System.Linq;
using Pathea;

public class PlantInfo
{
	public int		mTypeID;
	public int		mItemId;
	public float[] 	mGrowTime;
	public string[]	mModelPath;
	public string[] mDeadModePath;
	public float	mDefaultWater;
	public float	mDefaultClean;
	public float	mWaterDS;
	public float	mCleanDS;
	public float[]	mWaterLevel;
	public float[]	mCleanLevel;
	public float	mSize;
	public float	mHeight;
	public int		mItemGetNum;
	public List<ResItemGot> mItemGetPro;
	
	static Dictionary<int, PlantInfo> stbInfoDic;
	
	public static PlantInfo GetInfo(int ID)
	{
		if(stbInfoDic.ContainsKey(ID))
			return stbInfoDic[ID];
		return null;
	}
	
	public static PlantInfo GetPlantInfoByItemId(int itemId)
	{
		foreach(PlantInfo info in stbInfoDic.Values)
		{
			if(info.mItemId == itemId)
				return info;
		}
		return null;
	}

	public static Bounds GetPlantBounds(int itemId,Vector3 pos)
	{
		Bounds _mPlantBounds = new Bounds();
		PlantInfo plant = GetPlantInfoByItemId(itemId);
		if(plant == null)
			return _mPlantBounds;

		_mPlantBounds.SetMinMax( (new Vector3(-0.5f * plant.mSize,0,-0.5f * plant.mSize) + pos),
		                       (new Vector3(0.5f * plant.mSize,plant.mHeight,0.5f * plant.mSize) + pos));
		return _mPlantBounds;
	}
	
	public static void LoadData()
	{
		stbInfoDic = new Dictionary<int, PlantInfo>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("plant");
		reader.Read(); // Firstline is exp
		while (reader.Read())
		{
			PlantInfo addInfo = new PlantInfo();
			addInfo.mTypeID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			addInfo.mItemId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("seedid")));
			addInfo.mGrowTime = new float[3];
			for(int i = 0; i < 3; i++)
				addInfo.mGrowTime[i] = Convert.ToSingle(reader.GetString(reader.GetOrdinal("time" + (i + 1))));
			addInfo.mModelPath = new string[3];
			for(int i = 0; i < 3; i++)
				addInfo.mModelPath[i] = reader.GetString(reader.GetOrdinal("model" + (i + 1)));
			addInfo.mDeadModePath = new string[3];
			for(int i = 0; i < 3; i++)
				addInfo.mDeadModePath[i] = reader.GetString(reader.GetOrdinal("dmodel" + (i + 1)));
			
			addInfo.mDefaultWater = Convert.ToSingle(reader.GetString(reader.GetOrdinal("water0")));
			addInfo.mWaterDS = Convert.ToSingle(reader.GetString(reader.GetOrdinal("waterDS")));
			addInfo.mWaterLevel = new float[2];
			addInfo.mWaterLevel[0] = Convert.ToSingle(reader.GetString(reader.GetOrdinal("water1")));
			addInfo.mWaterLevel[1] = Convert.ToSingle(reader.GetString(reader.GetOrdinal("water2")));
			
			addInfo.mDefaultClean = Convert.ToSingle(reader.GetString(reader.GetOrdinal("clean0")));
			addInfo.mCleanDS = Convert.ToSingle(reader.GetString(reader.GetOrdinal("cleanDS")));
			addInfo.mCleanLevel = new float[2];
			addInfo.mCleanLevel[0] = Convert.ToSingle(reader.GetString(reader.GetOrdinal("clean1")));
			addInfo.mCleanLevel[1] = Convert.ToSingle(reader.GetString(reader.GetOrdinal("clean2")));
			addInfo.mSize = Convert.ToSingle(reader.GetString(reader.GetOrdinal("size")));
			addInfo.mHeight = Convert.ToSingle(reader.GetString(reader.GetOrdinal("height")));
			
			addInfo.mItemGetNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("maxgetnum")));
			addInfo.mItemGetPro = new List<ResItemGot>();
			string strList = reader.GetString(reader.GetOrdinal("itemget"));
			string[] spStr = strList.Split(';');
			for(int i = 0; i < spStr.Length; i++)
			{
				string[] itemstr = spStr[i].Split(',');
				ResItemGot itemget = new ResItemGot();
				itemget.m_id = Convert.ToInt32(itemstr[0]);
				itemget.m_probablity = Convert.ToSingle(itemstr[1]);
				addInfo.mItemGetPro.Add(itemget);
			}
			stbInfoDic[addInfo.mTypeID] = addInfo;
		}
	}
}

public class FarmPlantInitData
{
    public int mPlantInstanceId;
    public int mTypeID;
    public Vector3 mPos;
    public Quaternion mRot;
    public double mPutOutGameTime;
    public double mLife;
    public double mWater;
    public double mClean;
    public bool mDead;
    public int mGrowTimeIndex;

    public double mCurGrowTime;
    public byte mTerrianType;
    public float mGrowRate = 1;
	public float mExtraGrowRate = 0.0f;
	public float mNpcGrowRate = 0.0f;
	public double mLastUpdateTime;
}

public class FarmManager : MonoBehaviour 
{
	static FarmManager mInstance;
	public static FarmManager Instance{get{return mInstance;}}
    int frameCount = 0;

	const int Version = 4;  
	
	public delegate void PlantEvent(FarmPlantLogic plant);
	public event PlantEvent CreatePlantEvent;
	public event PlantEvent RemovePlantEvent;
	
	// Use this for initialization
	void Awake () 
	{
		mInstance = this;
		DigTerrainManager.onDirtyVoxel += OnDirtyVoxel;
	}

	void OnDestroy ()
	{
		DigTerrainManager.onDirtyVoxel -= OnDirtyVoxel;
	}
	
	// ItemObjID Plant map
	public Dictionary<int, FarmPlantLogic> mPlantMap = new Dictionary<int, FarmPlantLogic>();
	public Dictionary<IntVec3, int> mPlantHelpMap = new Dictionary<IntVec3, int>();

	public void Export(BinaryWriter bw)
    {
    }
    public void Import(byte[] buffer)
    {}
    public List<FarmPlantInitData> ImportPlantData(byte[] buffer)
    {
        List<FarmPlantInitData> initList = new List<FarmPlantInitData>();
        mPlantMap.Clear();
        mPlantHelpMap.Clear();
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);
        int saveVersion = _in.ReadInt32();
        int count = _in.ReadInt32();
        switch (saveVersion)
        {
            case 4:
			case 5:
                for (int i = 0; i < count; i++)
                {
                    FarmPlantInitData addPlant = new FarmPlantInitData();
                    addPlant.mPlantInstanceId = _in.ReadInt32();
                    addPlant.mTypeID = _in.ReadInt32();
                    addPlant.mPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
                    addPlant.mRot = new Quaternion(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
                    addPlant.mPutOutGameTime = _in.ReadDouble();
                    addPlant.mLife = _in.ReadDouble();
                    addPlant.mWater = _in.ReadDouble();
                    addPlant.mClean = _in.ReadDouble();
                    addPlant.mDead = _in.ReadBoolean();
                    addPlant.mGrowTimeIndex = _in.ReadInt32();
                    addPlant.mCurGrowTime = _in.ReadDouble();
                    addPlant.mTerrianType = _in.ReadByte();
                    addPlant.mExtraGrowRate = _in.ReadSingle();
                    initList.Add(addPlant);
                }
				break;
			case 6:
				for (int i = 0; i < count; i++)
				{
					FarmPlantInitData addPlant = new FarmPlantInitData();
					addPlant.mPlantInstanceId = _in.ReadInt32();
					addPlant.mTypeID = _in.ReadInt32();
					addPlant.mPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
					addPlant.mRot = new Quaternion(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
					addPlant.mPutOutGameTime = _in.ReadDouble();
					addPlant.mLife = _in.ReadDouble();
					addPlant.mWater = _in.ReadDouble();
					addPlant.mClean = _in.ReadDouble();
					addPlant.mDead = _in.ReadBoolean();
					addPlant.mGrowTimeIndex = _in.ReadInt32();
					addPlant.mCurGrowTime = _in.ReadDouble();
					addPlant.mTerrianType = _in.ReadByte();
					addPlant.mExtraGrowRate = _in.ReadSingle();
					addPlant.mLastUpdateTime = _in.ReadDouble();
					initList.Add(addPlant);
				}
				break;
		case FarmPlantLogic.Version001:
			for (int i = 0; i < count; i++)
			{
				FarmPlantInitData addPlant = new FarmPlantInitData();
				addPlant.mPlantInstanceId = _in.ReadInt32();
				addPlant.mTypeID = _in.ReadInt32();
				addPlant.mPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
				addPlant.mRot = new Quaternion(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
				addPlant.mPutOutGameTime = _in.ReadDouble();
				addPlant.mLife = _in.ReadDouble();
				addPlant.mWater = _in.ReadDouble();
				addPlant.mClean = _in.ReadDouble();
				addPlant.mDead = _in.ReadBoolean();
				addPlant.mGrowTimeIndex = _in.ReadInt32();
				addPlant.mCurGrowTime = _in.ReadDouble();
				addPlant.mTerrianType = _in.ReadByte();
				addPlant.mGrowRate = _in.ReadSingle();
				addPlant.mExtraGrowRate = _in.ReadSingle();
				addPlant.mNpcGrowRate = _in.ReadSingle();
				addPlant.mLastUpdateTime = _in.ReadDouble();
				initList.Add(addPlant);
			}
			break;
        }
        _in.Close();
        ms.Close();
        return initList;
    }
	  
	public FarmPlantLogic GetPlantByItemObjID(int itemObjID)
	{
		if(mPlantMap.ContainsKey(itemObjID))
			return mPlantMap[itemObjID];
		return null;
	}
	
    //public FarmPlantLogic CreatePlant(int itemObjID, int plantTypeID, Vector3 pos)
    //{
    //    FarmPlantLogic addPlant = new FarmPlantLogic();
    //    addPlant.mPlantInstanceId = itemObjID;
    //    addPlant._PlantType = plantTypeID;
    //    addPlant.mLife = 100f;
    //    addPlant.mPos = pos;
    //    addPlant.mPutOutGameTime = GameTime.Timer.Second;
    //    addPlant.mWater = addPlant.mPlantInfo.mDefaultWater;
    //    addPlant.mClean = addPlant.mPlantInfo.mDefaultClean;
    //    addPlant.mDead = false;
    //    addPlant.mGrowTimeIndex = 0;
    //    mPlantMap[itemObjID] = addPlant;
    //    mPlantHelpMap[new IntVec3(addPlant.mPos)] = addPlant.mPlantInstanceId;

    //    if(null != CreatePlantEvent)
    //        CreatePlantEvent(addPlant);

    //    return addPlant;
    //}
    public void AddPlant(FarmPlantLogic addPlant)
    {
        mPlantMap[addPlant.mPlantInstanceId] = addPlant;
        mPlantHelpMap[new IntVec3(addPlant.mPos)] = addPlant.mPlantInstanceId;
        if (null != CreatePlantEvent)
            CreatePlantEvent(addPlant);
    }
    public void InitPlant(FarmPlantLogic addPlant)
    {
        addPlant._PlantType = addPlant.mPlantInfo.mTypeID;
        addPlant.mLife = 100f;
        addPlant.mPutOutGameTime = GameTime.Timer.Second;
        addPlant.mWater = addPlant.mPlantInfo.mDefaultWater;
        addPlant.mClean = addPlant.mPlantInfo.mDefaultClean;
        addPlant.mDead = false;
        addPlant.mGrowTimeIndex = 0;
        IntVector3 safePos = new IntVector3(addPlant.transform.position + 0.1f * Vector3.down);
        addPlant.mTerrianType = VFVoxelTerrain.self.Voxels.SafeRead(safePos.x, safePos.y, safePos.z).Type;
        addPlant.InitGrowRate(0);
		addPlant.InitUpdateTime();
    }
	
	public void RemovePlant(int itemObjID)
	{
		if(mPlantMap.ContainsKey(itemObjID))
		{
			if(null != RemovePlantEvent)
				RemovePlantEvent(mPlantMap[itemObjID]);
			mPlantHelpMap.Remove(new IntVec3(mPlantMap[itemObjID].mPos));
			mPlantMap.Remove(itemObjID);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (!GameConfig.IsMultiMode)
        {
            //1.间隔思考时间点
            frameCount++;
            //2.获取所有表
            if (frameCount % 64 == 0)
			{
				List<CSCreator> colonyCreator = new List<CSCreator> ();
				if(CSMain.Instance!=null){
					if(PeGameMgr.IsSingle)
						colonyCreator.Add(CSMain.s_MgCreator);
					else
						colonyCreator.AddRange(CSMain.Instance.otherCreators.Values);
				}
//                foreach (FarmPlantLogic plant in mPlantMap.Values)
//                {
//					if(WeatherConfig.IsRaining){
//						plant.GetRain();
//					}
//						
//                    if (plant.mNextUpdateTime > 0 && plant.mNextUpdateTime < GameTime.Timer.Second)
//                    {
//                        plant.UpdateStatus();
//                    }
//                }

				foreach (FarmPlantLogic plant in mPlantMap.Values)
				{
                    //lz-2017.06.14 不知道为什么plant是空，防御性维护
                    if (null == plant || plant.transform == null)
                        continue;
					if(WeatherConfig.IsRaining){
						plant.GetRain();
					}
					float maxNpcRate = 0;
					if(colonyCreator.Count>0){
						foreach(CSCreator creator in colonyCreator){
							CSMgCreator mCreator = creator as CSMgCreator;
							if(mCreator==null)
								continue;
							if(creator.Assembly==null)
								continue;
							if(!creator.Assembly.InRange(plant.mPos))
								continue;
							if(creator.Assembly.Farm==null)
								continue;
							if(!creator.Assembly.Farm.IsRunning)
								continue;
							if(creator.Assembly.Farm.FarmerGrowRate<=maxNpcRate)
								continue;
							maxNpcRate = creator.Assembly.Farm.FarmerGrowRate;
						}
					}
					
					if(maxNpcRate!=plant.mNpcGrowRate)
						plant.UpdateNpcGrowRate(maxNpcRate);
					else if (plant.mNextUpdateTime > 0 && plant.mNextUpdateTime < GameTime.Timer.Second)
						plant.UpdateStatus();
	                frameCount = 0;
				}
            }
        }
	}
//	IEnumerator PlantUpdate(List<FarmPlantLogic> pList,List<CSCreator> creators){
//		int counter=0;
//
//		foreach (FarmPlantLogic plant in pList)
//		{
//			if(WeatherConfig.IsRaining){
//				plant.GetRain();
//			}
//
//			float maxNpcRate = 0;
//			if(creators.Count>0){
//				foreach(CSCreator creator in creators){
//					CSMgCreator mCreator = creator as CSMgCreator;
//					if(mCreator==null)
//						continue;
//					if(creator.Assembly==null)
//						continue;
//					if(creator.Assembly.Farm==null)
//						continue;
//					if(!creator.Assembly.Farm.IsRunning)
//						continue;
//					if(creator.Assembly.Farm.FarmerGrowRate<=maxNpcRate)
//						continue;
//					maxNpcRate = creator.Assembly.Farm.FarmerGrowRate;
//				}
//			}
//
//			if(maxNpcRate!=plant.npcGrowRate)
//				plant.UpdateNpcGrowRate(maxNpcRate);
//			else if (plant.mNextUpdateTime > 0 && plant.mNextUpdateTime < GameTime.Timer.Second)
//				plant.UpdateStatus();
//			counter++;
//			if(counter>=200){
//				yield return null;
//			}
//		}
//	}


	#region CALL_BACK

	void OnDirtyVoxel (Vector3 pos, byte terrainType)
	{
		for (int i = 0; i < 2; i++)
		{
			IntVec3 idx = new IntVec3(pos);
			if ( mPlantHelpMap.ContainsKey(idx) )
			{
				FarmPlantLogic p = mPlantMap[ mPlantHelpMap[idx]];
				if (p.mTerrianType != terrainType)
				{
					p.mTerrianType = terrainType;
					p.UpdateGrowRate(0,false);
				}
			}
		}
	}
    #endregion
}
