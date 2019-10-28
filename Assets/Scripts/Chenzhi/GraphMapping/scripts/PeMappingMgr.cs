using UnityEngine;
using System.Collections;
using System;
using GraphMapping;
using System.IO;


public class PeMappingMgr : Pathea.MonoLikeSingleton<PeMappingMgr>
{
    public static bool inited = false;

	private Vector2 mWorldSize;

	// if you add graph_map you need modify the code below-----------------------------------------------
	public PeBiomeMapping mBiomeMap;
	public PeHeightMapping mHeightMap;
	public PeAiSpawnMapping mAiSpawnMap;

	// interface
	private EBiome mBiome;
	private float mHeight;
	private int mAiSpawnID;

	public EBiome Biome{get{return mBiome;}}
	public float Height{get{return mHeight;}}
	public int AiSpawnID{get{return mAiSpawnID;}}

    Pathea.PeTrans mTrans = null;
    Vector2 GetPlayerPos()
    {
        if (mTrans == null)
        {
            Pathea.PeEntity entity = Pathea.PeCreature.Instance.mainPlayer;
            if (null != entity)
            {
                mTrans = entity.GetCmpt<Pathea.PeTrans>();
            }
        }

        if (null != mTrans)
        {
			return new Vector2(mTrans.position.x,mTrans.position.z);
        }
        return Vector2.zero;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        inited = false;
    }

	public override void Update()
    {
		if (!inited)
		{
			if(Pathea.PeGameMgr.IsStory){
				Debug.LogError ("PeMapping has be used but it is not init!");
			}
			return;
		}
        base.Update();
        UpdateMappings(GetPlayerPos());
    }

	public void UpdateMappings(Vector2 playerPos)
	{
		mBiome = mBiomeMap.GetBiom(playerPos,mWorldSize);
		mHeight = mHeightMap.GetHeight(playerPos,mWorldSize);
		mAiSpawnID = mAiSpawnMap.GetAiSpawnMapId(playerPos,mWorldSize);
	}


    public float GetTerrainHeight(Vector3 pos)
    {
        if (Pathea.PeGameMgr.IsStory)
        {
			if(null == mHeightMap)
				return 100f;
            return mHeightMap.GetHeight(new Vector2(pos.x, pos.z), mWorldSize);
        }
        if (Pathea.PeGameMgr.IsCustom)
        {
            return 100f;
        }
        return VFDataRTGen.GetBaseTerHeight(new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)));
    }


    public int GetAiSpawnMapId(Vector2 targetPos)
	{
        if (mAiSpawnMap == null) {
			Debug.LogError("Failed to read AiSpawnMap");
			return -1;
		}
		return mAiSpawnMap.GetAiSpawnMapId(targetPos,mWorldSize);
	}


	public void Init(Vector2 worldSzie)
	{
		mBiomeMap = new PeBiomeMapping();
		mHeightMap = new PeHeightMapping();
		mAiSpawnMap = new PeAiSpawnMapping();
		mWorldSize = worldSzie;
		LoadFile();

        inited = true;
	}

	void SaveData(BinaryWriter bw)
	{
		// biome
		byte[] buf = mBiomeMap.Serialize();
		bw.Write(buf.Length);
		bw.Write(buf);
		// height
		buf = mHeightMap.Serialize();
		bw.Write(buf.Length);
		bw.Write(buf);
		// aisPawn
		buf = mAiSpawnMap.Serialize();
		bw.Write(buf.Length);
		bw.Write(buf);
	}
	
	void ReadData(BinaryReader br)
	{
		// biome
		int len = br.ReadInt32();
		mBiomeMap.Deserialize(br.ReadBytes(len));
		// height
		len = br.ReadInt32();
		mHeightMap.Deserialize(br.ReadBytes(len));
		// aisPawn
		len = br.ReadInt32();
		mAiSpawnMap.Deserialize(br.ReadBytes(len));
	}

	
	// End modify-------------------------------------------------------------------------------------

	bool LoadFile()
	{
		try
		{
			TextAsset textFile = Resources.Load(filePath) as TextAsset;
			if (textFile == null)
			{
				Debug.LogError("Load '" + filePath +"' failed!");
				return false;
			}
			MemoryStream ms = new MemoryStream(textFile.bytes);
			BinaryReader br = new BinaryReader(ms);
			
			ReadData(br);
			
			br.Close();
			ms.Close();
			return true;
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e);
			return false;	
		}
	}

	public bool SaveFile(string DirPath)
	{
		try
		{
			CheckDir(DirPath);
			using (FileStream fs = new FileStream(DirPath + filePath + ".bytes", FileMode.Create, FileAccess.Write))
			{
				BinaryWriter bw = new BinaryWriter(fs);
				SaveData(bw);

				bw.Close();
				fs.Close();
			}
			return true;
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e);
			return false;	
		}
	}

	string filePath{get{return "PeMappingData";}}
	
	void CheckDir(string DirPath)
	{
		if (!Directory.Exists(DirPath))
		{
			Directory.CreateDirectory(DirPath);
		}
	}
}
