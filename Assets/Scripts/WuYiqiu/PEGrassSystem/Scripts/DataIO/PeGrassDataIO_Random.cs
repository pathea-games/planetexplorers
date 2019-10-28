using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RedGrass;
using Pathea.Maths;

public class PeGrassDataIO_Random : Chunk32DataIO, IRefreshDataDetect
{
	public override void Init (EvniAsset evni)
	{
		base.Init (evni);

		#if true
		mEvni.SetDensity(mEvni.Density);
		mEvni.SetLODType(ELodType.LOD_2_TYPE_2);
		
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

	#region RefreshDataDetect

	public bool CanRefresh(RGScene scene)
	{
		bool result = false;
		if (Time.frameCount % 16 == 0)
		{
			if (scene.dataIO.isActive())
				result = false;
			else
			 	result = true;
		}

		return result;
	}

	#endregion

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


	#region Private_Member

	Queue<RGChunk> mReqs;

	bool mRunning = true;
	bool mActive = false;
	bool mStart = false;

	Thread mThread;

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

				Monitor.TryEnter(VFDataRTGen.s_dicGrassInstList);

				try
				{
					while (true)
					{
						RGChunk chunk = null;
						
						lock(mReqs)
						{
							if (mReqs.Count == 0)
								break;
							chunk = mReqs.Dequeue();
						}


						INTVECTOR3 chunk32Pos = ChunkPosToPos32(chunk.xIndex, chunk.zIndex); 

						int expands = mEvni.CHUNKSIZE / mEvni.Tile;

                        lock(chunk)
                        {
                            for (int x = chunk32Pos.x; x < chunk32Pos.x + expands; ++x)
                            {
                                for (int z = chunk32Pos.z; z < chunk32Pos.z + expands; ++z)
                                {
                                    IntVector2 key = new IntVector2(x, z);
                                    if (VFDataRTGen.s_dicGrassInstList.ContainsKey(key))
                                    {
                                        foreach (VoxelGrassInstance vgi in VFDataRTGen.s_dicGrassInstList[key])
                                        {
                                            RedGrassInstance rgi = new RedGrassInstance();
                                            rgi.CopyTo(vgi);

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
				finally
				{
					Monitor.Exit(VFDataRTGen.s_dicGrassInstList);
				}

				mStart = false;
				Thread.Sleep(10);

			}
		}
		catch (System.Exception ex)
		{
            Debug.LogError(ex.ToString());
		}
	}
}
