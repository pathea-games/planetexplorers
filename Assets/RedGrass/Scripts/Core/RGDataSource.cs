using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea.Maths;

namespace RedGrass
{
	public class RGChunks 
	{
		Dictionary<int, RGChunk> _dicData = new Dictionary<int, RGChunk> (4096);
		public RGChunk this[int x, int z]{
			get{
				RGChunk ret = null;
				int key = Utils.PosToIndex(x,z);
				_dicData.TryGetValue(key, out ret);
				return ret;
			}
			set{
				int key = Utils.PosToIndex(x,z);
				_dicData[key] = value;
			}
		}
		public void Clear(){
			foreach (KeyValuePair<int, RGChunk> p in _dicData) {
				p.Value.Free();
				RGPoolSig.RecycleChunk(p.Value);
			}
			_dicData.Clear();
		}
		public bool Contains(int x, int z){
			int key = Utils.PosToIndex(x,z);
			return _dicData.ContainsKey (key);
		}
		public bool Remove(int x, int z){
			int key = Utils.PosToIndex(x,z);
			return _dicData.Remove (key);
		}
	}

	public class RGDataSource
	{
		RGChunks  mChunks;

		public RGChunks Chunks { get { return mChunks; }}
		public RGChunk Node (int x, int z) 
		{ 
			return mChunks[x,z];
		}

		EvniAsset mEvni = null;

		public int SIZE  { get { return (int)mEvni.CHUNKSIZE;} }
		public int SHIFT { get { return mEvni.SHIFT; } }
		public int MASK  { get { return mEvni.MASK; } }

		ChunkDataIO mIO; 
		public ChunkDataIO dataIO { get {return mIO;} }

		public INTVECTOR2 mCenter = new INTVECTOR2(-100, -100);

		public bool IsProcessReqs  { get; private set; }

		public void Init (EvniAsset evni, ChunkDataIO io)
		{
			mChunks = new RGChunks();
			mEvni = evni;

			mIO = io;
			mIO.Init(evni);
		}

		public void Free()
		{
			Clear();
			mChunks = null;
		}

		public void Clear()
		{
			mChunks.Clear();
		}

		public RedGrassInstance Read (int x, int y, int z)
		{
			int cx = x >> SHIFT;
			int cz = z >> SHIFT;

			RGChunk chunk = mChunks [cx, cz];
			if (chunk != null) {
				return chunk.Read(x, y, z);
			}

			return new RedGrassInstance();
		}

		public List<RedGrassInstance> Read (int x, int z, int ymin, int ymax)
		{
			int cx = x >> SHIFT;
			int cz = z >> SHIFT;

			RGChunk chunk = mChunks [cx, cz];
			if (chunk != null) {
				return chunk.Read(x, z, ymin, ymax);
			}
			return new List<RedGrassInstance>();
		}

		public bool Write (RedGrassInstance rgi)
		{
			Vector3 pos = rgi.Position;

			int cx = (int)pos.x >> SHIFT;
			int cz = (int)pos.z >> SHIFT;

			RGChunk chunk = mChunks [cx, cz];
			if (chunk != null) {
				if (rgi.Density < 0.001f)
					chunk.Remove((int)pos.x, (int)pos.y, (int)pos.z);
				else
					chunk.Write(rgi);
			} else {
				if (rgi.Density > 0.001f) {
					chunk = RGPoolSig.GetChunk();
					chunk.Init(cx, cz, mEvni);
					mChunks[cx, cz] = chunk;
					chunk.Write(rgi);
				}
			}

			return true;
		}

		public bool Remove(int x, int y, int z)
		{
			int cx = x >> SHIFT;
			int cz = z >> SHIFT; 

			RGChunk chunk = mChunks [cx, cz];
			if (chunk != null) {
				return chunk.Remove(x, y, z);
			}
			return false;
		}

		public void RefreshImmediately(int cx, int cz)
		{
			mIO.StopProcess();

			Clear();

			mReqsOutput.Clear();

			int expand = mEvni.DataExpandNum;
			for (int i = cx - expand; i <= cx + expand; i++)
			{
				for (int j =  cz - expand; j <= cz + expand; j++)
				{
					RGChunk chunk = RGPoolSig.GetChunk();
					chunk.Init(i, j, mEvni);
					mReqsOutput.reqsChunk.Add (chunk);
				}
			}

			mIO.AddReqs(mReqsOutput.reqsChunk);
			mIO.ProcessReqsImmediatly();

			foreach (RGChunk chunk in mReqsOutput.reqsChunk)
			{
				if (chunk.isEmpty)
					continue;
				mChunks[chunk.xIndex, chunk.zIndex] = chunk;
			}

			mCenter = new INTVECTOR2(cx, cz);
		}


		#region UPDATE_REQS
		public class ReqsOutput
		{
			public List<RGChunk> reqsChunk;
			public List<RGChunk> discardChunk;

			public ReqsOutput()
			{
				reqsChunk = new List<RGChunk>();
				discardChunk = new List<RGChunk>();
			}

			public void Clear ()
			{
				reqsChunk.Clear ();
				discardChunk.Clear ();
			}

			public bool IsEmpty()
			{
				return (reqsChunk.Count == 0) && (discardChunk.Count == 0);
			}

		}

		private ReqsOutput mReqsOutput = new ReqsOutput();
		public  ReqsOutput reqsOutput { get { return mReqsOutput; } }

		public void ReqsUpdate (int cx, int cz)
		{
			Profiler.BeginSample ("Req");
			mReqsOutput.Clear();

			IsProcessReqs = true;


			// Find the loading chunk
			int expand = mEvni.DataExpandNum;
			for (int i = cx - expand; i <= cx + expand; i++)
			{
				for (int j =  cz - expand; j <= cz + expand; j++)
				{
					if (!mChunks.Contains(i, j))
					{
						RGChunk chunk = RGPoolSig.GetChunk();
						chunk.Init(i, j, mEvni);
						mReqsOutput.reqsChunk.Add (chunk);
					}
				}
			}

			if (mReqsOutput.reqsChunk.Count != 0)
			{
				mIO.AddReqs(mReqsOutput.reqsChunk);
			}

			// Descarding part
			int max_new_x = cx + expand;
			int min_new_x = cx - expand;
			int max_new_z = cz + expand;
			int min_new_z = cz - expand;

			for (int i = mCenter.x - expand; i <= mCenter.x + expand; i++)
			{
				for (int j = mCenter.y - expand; j <= mCenter.y + expand; j++)
				{
					if (i < min_new_x || i > max_new_x ||
					    j < min_new_z || j > max_new_z )
					{
						RGChunk chunk = mChunks[i, j];
						if (chunk != null)
							mReqsOutput.discardChunk .Add(chunk);	
					}
				}
			}


			if (mReqsOutput.reqsChunk.Count != 0 || mReqsOutput.discardChunk.Count != 0)
			{
				// Activate the DataIO
				mIO.Active( delegate() {
					IsProcessReqs = false;
				});
			}
			Profiler.EndSample ();
		}

		public void SumbmitReqs (INTVECTOR2 center)
		{
			Profiler.BeginSample ("sr0");
			for (int i = 0; i < mReqsOutput.discardChunk.Count; ++i)
			{
				mReqsOutput.discardChunk[i].Free();
				if (!mChunks.Remove(mReqsOutput.discardChunk[i].xIndex, mReqsOutput.discardChunk[i].zIndex))
				{
//					Debug.LogError ("This key is not exist.");
				}
				else
				{
					RGPoolSig.RecycleChunk(mReqsOutput.discardChunk[i]);
				}
					
			}
			Profiler.EndSample ();

			Profiler.BeginSample ("sr1");
			for (int i = 0; i < mReqsOutput.reqsChunk.Count; i++)
			{
				if ( mReqsOutput.reqsChunk[i].isEmpty)
				{
					RGPoolSig.RecycleChunk( mReqsOutput.reqsChunk[i]);
					continue;
				}
//				mChunks.Add(key, mReqsOutput.reqsChunk[i]);
				mChunks[mReqsOutput.reqsChunk[i].xIndex, mReqsOutput.reqsChunk[i].zIndex] = mReqsOutput.reqsChunk[i];

			}
			Profiler.EndSample ();

			mCenter = center;
			mReqsOutput.Clear();
		}
		
		#endregion

	}

}
