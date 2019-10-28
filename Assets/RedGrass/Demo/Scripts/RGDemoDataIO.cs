using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Pathea.Maths;
using RedGrass;

public class RGDemoDataIO : Chunk32DataIO
{
	public static readonly string s_orgnFilePath =  "D:/grassinst_test.dat";

	public override void Init (RedGrass.EvniAsset evni)
	{
		base.Init(evni);

		mReqs = new Queue<RedGrass.RGChunk >(100);

		// File 
		mOrgnOfsData = new int[evni.XZTileCount];
		mOrgnLenData = new int[evni.XZTileCount];


		if (File.Exists(s_orgnFilePath))
		{
			mOrgnGrassFile = new FileStream(s_orgnFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			BinaryReader _in = new BinaryReader(mOrgnGrassFile);
			
			
			// read offsets and lens
			for ( int i = 0; i < evni.XZTileCount; ++i )
			{
				mOrgnOfsData[i] = _in.ReadInt32();
				mOrgnLenData[i] = _in.ReadInt32();
			}
		}

		// Creat Thread
		if (mThread == null)
		{
			mThread = new Thread(new ThreadStart(Run));
			mThread.Name = "Red Grass IO Thread";
			mThread.Start();
		}
	}

	public override bool AddReqs (RedGrass.RGChunk chunk)
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

	public override bool AddReqs (List<RedGrass.RGChunk> chunks)
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

				BinaryReader _in = new BinaryReader(mOrgnGrassFile);
				INTVECTOR3 chunk32Pos = ChunkPosToPos32(chunk.xIndex, chunk.zIndex);
				int expands = mEvni.CHUNKSIZE / mEvni.Tile;
				
                lock(chunk)
                {
                    for (int _x = chunk32Pos.x; _x < chunk32Pos.x + expands; ++_x)
                    {
                        for (int _z = chunk32Pos.z; _z < chunk32Pos.z + expands; ++_z)
                        {
                            int index32 = Pos32ToIndex32(_x, _z);
                            _in.BaseStream.Seek(mOrgnOfsData[index32], SeekOrigin.Begin);
                            int count = mOrgnLenData[index32];
                            for (int i = 0; i < count; ++i)
                            {
                                RedGrass.RedGrassInstance rgi = new RedGrassInstance();

                                rgi.ReadFromStream(_in);

                                chunk.Write(rgi);
                            }
                        }
                    }
                }
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogError(ex.ToString());
		}
	}
	
	public override void StopProcess ()
	{
		mActive = false;

		lock (mReqs)
		{
			mReqs.Clear();
		}
	}

	public override bool Active (OnReqsFinished call_back)
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


	#region UNITY_INNER_FUNC

	void OnDestroy ()
	{
		mRunning = false;
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

	#region PRIVATE

	// File 
	private FileStream mOrgnGrassFile = null;
	private int [] mOrgnOfsData = null;
	private int [] mOrgnLenData = null;

	// Reqs & thread
	Queue<RedGrass.RGChunk> mReqs;
	Thread mThread;

	bool mRunning = true;
	bool mActive = false;
	bool mStart = false;

	OnReqsFinished onReqsFinished;

	// Thread function
	void Run ()
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
					RedGrass.RGChunk chunk = null;

					lock(mReqs)
					{
						if (mReqs.Count == 0)
							break;
						chunk = mReqs.Dequeue();
					}

					if (mOrgnGrassFile == null)
						continue;
					BinaryReader _in = new BinaryReader(mOrgnGrassFile);
					INTVECTOR3 chunk32Pos = ChunkPosToPos32(chunk.xIndex, chunk.zIndex); 
					int expands = mEvni.CHUNKSIZE / mEvni.Tile;

                    lock (chunk)
                    {
                        for (int _x = chunk32Pos.x; _x < chunk32Pos.x + expands; ++_x)
                        {
                            for (int _z = chunk32Pos.z; _z < chunk32Pos.z + expands; ++_z)
                            {
                                int index32 = Pos32ToIndex32(_x, _z);
                                _in.BaseStream.Seek(mOrgnOfsData[index32], SeekOrigin.Begin);
                                int count = mOrgnLenData[index32];
                                for (int i = 0; i < count; ++i)
                                {
                                    RedGrass.RedGrassInstance rgi = new RedGrassInstance();

                                    rgi.ReadFromStream(_in);

                                    chunk.Write(rgi);
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
			Debug.LogError("<<<< Data IO Thread error >>>> \r\n" + ex);
		}
	}

	#endregion


}
