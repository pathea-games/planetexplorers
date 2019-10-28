using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RedGrass;
using Pathea.Maths;

/// <summary>
/// PE grass data IO for stroy mode
/// </summary>
public class PeGrassDataIO_Story : Chunk32DataIO 
{
	static string sOriginalSubTerrainDir = null;
	public static string originalSubTerrainDir
	{
		get
		{
			return sOriginalSubTerrainDir;
		}
		
		set
		{
			sOriginalSubTerrainDir = value + "/";
		}
    }
    
    public override void Init (EvniAsset evni)
    {
        base.Init(evni);

		#if true
		mEvni.SetDensity(mEvni.Density);
        if ((int)SystemSettingData.Instance.GrassLod > (int) ELodType.LOD_3_TYPE_1)
            SystemSettingData.Instance.GrassLod = ELodType.LOD_3_TYPE_1;

        mEvni.SetLODType((ELodType)((int)SystemSettingData.Instance.GrassLod));

		mEvni.LODDensities = new float[mEvni.MaxLOD + 1];
		for (int i = 0; i < mEvni.MaxLOD + 1; i ++)
		{
			if (i == 0)
				mEvni.LODDensities[i] = 1;
			else
			{
				mEvni.LODDensities[i] = 0.4f / (1 << (i-1));
			}
		}
		#endif

		mReqs = new Queue<RGChunk>(100);

//		originalSubTerrainDir = GameConfig.PEDataPath + "VoxelData/SubTerrains";
		// File
		mOrgnFilePath = new string[9]
		{
			originalSubTerrainDir + "subTerG_x0_y0.dat",
			originalSubTerrainDir + "subTerG_x1_y0.dat",
			originalSubTerrainDir + "subTerG_x2_y0.dat",
			originalSubTerrainDir + "subTerG_x0_y1.dat",
			originalSubTerrainDir + "subTerG_x1_y1.dat",
			originalSubTerrainDir + "subTerG_x2_y1.dat",
			originalSubTerrainDir + "subTerG_x0_y2.dat",
			originalSubTerrainDir + "subTerG_x1_y2.dat",
			originalSubTerrainDir + "subTerG_x2_y2.dat"
        };

		// File stream
		mOrgnGrassFile = new FileStream[evni.FileXZcount];
		for (int i = 0; i < evni.FileXZcount; ++i)
		{
			if ( File.Exists(mOrgnFilePath[i]))
				mOrgnGrassFile[i] = new FileStream(mOrgnFilePath[i], FileMode.Open, FileAccess.Read, FileShare.Read);
			else
				Debug.LogWarning("The path file: " + mOrgnFilePath[i] + "is missing!");
        }

		mOrgnOfsData = new int[mEvni.FileXZcount, mEvni.XZTileCount];

		for (int i = 0; i < mEvni.FileXZcount; ++i)
		{
			if (mOrgnGrassFile[i] == null)
				continue;
			
			BinaryReader _in = new BinaryReader(mOrgnGrassFile[i]);
			for (int j = 0; j < mEvni.XZTileCount; ++j)
			{
				mOrgnOfsData[i,j]  = _in.ReadInt32();
            }
        }



		// Create Thread
		if (mThread == null)
		{
			mThread = new Thread(new ThreadStart(Run));
			mThread.Name = " Story mode Grass IO Thread";
            mThread.Start();
        }
    }
    

    public override bool AddReqs (RGChunk chunk)
	{

		if (mActive)
		{
			Debug.LogError("The data IO is already acitve");
			return false;
			
		}
		
		lock (mReqs)
        {
            mReqs.Enqueue(chunk);
        }
        
        return true;
	}

	public override bool AddReqs (System.Collections.Generic.List<RGChunk> chunks)
	{
		if (mActive)
		{
			Debug.LogError("The data IO is already acitve");
			return false;
			
		}
		
		lock (mReqs)
		{
			foreach (RGChunk chunk in chunks)
                mReqs.Enqueue(chunk);
        }
        
        return true;
	}

	public override void ClearReqs ()
	{

	}

	public override void StopProcess ()
	{
		mActive = false;
		
		lock (mReqs)
		{
            mReqs.Clear();
        }
	}

	public override void ProcessReqsImmediatly ()
	{
		try
		{
			while (mReqs.Count != 0)
			{
				RedGrass.RGChunk chunk = null;
				
				lock(mReqs)
				{
					chunk = mReqs.Dequeue();
				}
				
				INTVECTOR3 chunk32Pos = ChunkPosToPos32(chunk.xIndex, chunk.zIndex);

				// Find the file index
				int file_index = FindOrginFileIndex(chunk32Pos);

				if (file_index != -1 && mOrgnGrassFile[file_index] != null)
				{
					BinaryReader _in = new BinaryReader(mOrgnGrassFile[file_index]);

					int expands = mEvni.CHUNKSIZE / mEvni.Tile;

                    lock(chunk)
                    {
                        for (int x = chunk32Pos.x; x < chunk32Pos.x + expands; ++x)
                        {
                            for (int z = chunk32Pos.z; z < chunk32Pos.z + expands; ++z)
                            {
                                int _x = x % mEvni.XTileCount;
                                int _z = z % mEvni.ZTileCount;

                                int index32 = Pos32ToIndex32(_x, _z);

                                if (mOrgnOfsData[file_index, index32] > 0)
                                {
                                    _in.BaseStream.Seek(mOrgnOfsData[file_index, index32], SeekOrigin.Begin);
                                    int count = _in.ReadInt32();

                                    for (int i = 0; i < count; ++i)
                                    {
                                        RedGrassInstance rgi = new RedGrassInstance();
                                        rgi.ReadFromStream(_in);

                                        Vector3 pos = rgi.Position;
                                        if (!GrassDataSL.m_mapDelPos.ContainsKey(new INTVECTOR3((int)pos.x, (int)pos.y, (int)pos.z)))
                                        {
                                            chunk.Write(rgi);
                                        }
                                    }
                                }
                            }
                        }
                    }
				}
	        }
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public override bool Active(OnReqsFinished call_back)
	{
		if (IsEmpty())
		{
			mActive = false;
			return false;
		}
		
		mActive = true;
		onReqsFinished = call_back;
		mStart = true;
		return true;
	}

	public override void Deactive ()
	{
		mActive = false;
	}
	
	public override bool isActive ()
	{
		return mActive;
	}

	bool IsEmpty ()
	{
		bool empty = false;
		lock (mReqs)
		{
			empty = (mReqs.Count == 0);
		}
		
		return empty;
	}


	public int FindOrginFileIndex (Vector3 chunk32Pos)
	{
		int file_index = -1;
		for (int i = 0; i < mEvni.FileXCount; ++i)
		{
			int zStart = (mEvni.ZStart >> mEvni.Tile) + (mEvni.ZTileCount * i);
			int zEnd = zStart + mEvni.ZTileCount;
			for (int j = 0; j <  mEvni.FlieZCount; ++j)
			{
				int xStart = (mEvni.XStart >> mEvni.Tile) + (mEvni.XTileCount * j);
				int xEnd = xStart + mEvni.XTileCount;
				
				if ( xStart <= chunk32Pos.x && xEnd > chunk32Pos.x
				    && zStart <= chunk32Pos.z && zEnd > chunk32Pos.z)
                {
					file_index = i * mEvni.FlieZCount + j;
                    break;
                }
            }
        }
        return file_index;
    }

	#region UNITY_INNER_FUNC
	
    object mDestoryLockObj = new object(); 
	void OnDestroy ()
	{
		mRunning = false;

        lock(mDestoryLockObj)
        {
            foreach (FileStream fs in mOrgnGrassFile)
            {
                if (fs != null)
                    fs.Close();
            }
        }
	}
	
	void Update ()
	{
		if (mActive && !mStart)
		{
			mActive = false;
			if (onReqsFinished != null)
				onReqsFinished();
		}
	}
	
	#endregion

	#region Private_Member

	// File 
	private FileStream[] mOrgnGrassFile = null;
	private int [,] mOrgnOfsData = null;

	public static string[] mOrgnFilePath = null;

	Queue<RGChunk> mReqs;
	Thread mThread;

	bool mRunning = true;
	bool mActive = false;
	bool mStart = false;

	OnReqsFinished onReqsFinished;

	#endregion


	void Run()
	{
		try
		{
			while (mRunning)
			{
				if (!mActive)
				{
					Thread.Sleep(10);
					continue;
				}

				while (true)
				{
					RGChunk chunk = null;

					lock(mReqs)
					{
						if (mReqs.Count == 0)
							break;
						chunk = mReqs.Dequeue();
					}

                    lock(mDestoryLockObj)
                    {

                        INTVECTOR3 chunk32Pos = ChunkPosToPos32(chunk.xIndex, chunk.zIndex);

                        // Find the file index
                        int file_index = FindOrginFileIndex(chunk32Pos);

                        if (file_index != -1 && mOrgnGrassFile[file_index] != null)
                        {
                            BinaryReader _in = new BinaryReader(mOrgnGrassFile[file_index]);

                            int expands = mEvni.CHUNKSIZE / mEvni.Tile;

                            lock(chunk)
                            {
                                for (int x = chunk32Pos.x; x < chunk32Pos.x + expands; ++x)
                                {
                                    for (int z = chunk32Pos.z; z < chunk32Pos.z + expands; ++z)
                                    {
                                        int _x = x % mEvni.XTileCount;
                                        int _z = z % mEvni.ZTileCount;

                                        int index32 = Pos32ToIndex32(_x, _z);

                                        if (mOrgnOfsData[file_index, index32] > 0)
                                        {
                                            _in.BaseStream.Seek(mOrgnOfsData[file_index, index32], SeekOrigin.Begin);
                                            int count = _in.ReadInt32();

                                            for (int i = 0; i < count; ++i)
                                            {
                                                RedGrassInstance rgi = new RedGrassInstance();
                                                rgi.ReadFromStream(_in);

                                                Vector3 pos = rgi.Position;
                                                if (!GrassDataSL.m_mapDelPos.ContainsKey(new INTVECTOR3((int)pos.x, (int)pos.y, (int)pos.z)))
                                                {
                                                    chunk.Write(rgi);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
				}

				mStart = false;

				Thread.Sleep(10);
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("<<<< Data IO Thread error >>>> \r\n" + ex);
		}
	}
}
